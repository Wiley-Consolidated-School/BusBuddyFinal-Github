using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.TimeCard.Services
{
    /// <summary>
    /// Service for validating time card entries
    /// </summary>
    public class TimeEntryValidationService
    {
        private readonly ITimeCardRepository _timeCardRepository;
        private readonly IDriverRepository _driverRepository;

        public TimeEntryValidationService(ITimeCardRepository timeCardRepository, IDriverRepository driverRepository)
        {
            _timeCardRepository = timeCardRepository ?? throw new ArgumentNullException(nameof(timeCardRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
        }

        /// <summary>
        /// Validate a time card entry and return warnings
        /// </summary>
        public async Task<List<TimeEntryWarning>> ValidateTimeCardAsync(Models.TimeCard timeCard, bool isNew)
        {
            var warnings = new List<TimeEntryWarning>();

            // Basic validation
            if (!timeCard.Date.HasValue)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.MissingDate,
                    Message = "Date is required.",
                    Severity = WarningSeverity.High
                });
                return warnings; // Can't continue without a date
            }

            // Check if driver exists
            if (timeCard.DriverId > 0)
            {
                var driver = _driverRepository.GetDriverById(timeCard.DriverId);
                if (driver == null)
                {
                    warnings.Add(new TimeEntryWarning
                    {
                        Type = WarningType.InvalidDriver,
                        Message = "Selected driver does not exist.",
                        Severity = WarningSeverity.High
                    });
                }
            }

            // Check for duplicate entries
            if (isNew)
            {
                var existingEntries = await _timeCardRepository.GetByDateRangeAsync(
                    timeCard.Date.Value.Date,
                    timeCard.Date.Value.Date);

                var duplicateEntry = existingEntries.FirstOrDefault(tc =>
                    tc.DriverId == timeCard.DriverId && tc.Date?.Date == timeCard.Date?.Date);

                if (duplicateEntry != null)
                {
                    warnings.Add(new TimeEntryWarning
                    {
                        Type = WarningType.DuplicateEntry,
                        Message = $"A time card entry already exists for this driver on {timeCard.Date:yyyy-MM-dd}.",
                        Severity = WarningSeverity.High
                    });
                }
            }

            // Validate time sequences
            if (!timeCard.IsValid(out string validationError))
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.InvalidTimeSequence,
                    Message = validationError,
                    Severity = WarningSeverity.High
                });
            }

            // Check for missing required times
            if (timeCard.ClockIn == null && !timeCard.IsPaidTimeOff)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.MissingClockIn,
                    Message = "Clock in time is required for work days.",
                    Severity = WarningSeverity.Medium
                });
            }

            if (timeCard.ClockOut == null && !timeCard.IsPaidTimeOff && timeCard.ClockIn != null)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.MissingClockOut,
                    Message = "Clock out time is required when clock in time is recorded.",
                    Severity = WarningSeverity.Medium
                });
            }

            // Check for excessive hours
            timeCard.CalculateTotalHours();
            if (timeCard.TotalTime > 16)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.ExcessiveHours,
                    Message = $"Total hours ({timeCard.TotalTime:F2}) exceeds 16 hours. Please verify.",
                    Severity = WarningSeverity.Medium
                });
            }

            // Check for overtime
            if (timeCard.Overtime > 0)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.OvertimeDetected,
                    Message = $"Overtime detected: {timeCard.Overtime:F2} hours.",
                    Severity = WarningSeverity.Low
                });
            }

            // Check weekly hours
            await ValidateWeeklyHours(timeCard, warnings);

            return warnings;
        }

        /// <summary>
        /// Validate weekly hours and add warnings if necessary
        /// </summary>
        private async Task ValidateWeeklyHours(Models.TimeCard timeCard, List<TimeEntryWarning> warnings)
        {
            if (!timeCard.Date.HasValue) return;

            // Get start of week (Monday)
            var monday = timeCard.Date.Value.AddDays(-(int)timeCard.Date.Value.DayOfWeek + 1);

            var weeklyHours = await _timeCardRepository.GetWeeklyTotalHoursAsync(timeCard.DriverId, monday);

            // Add current entry's hours if it's new
            weeklyHours += timeCard.TotalTime;

            if (weeklyHours > 60)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.ExcessiveWeeklyHours,
                    Message = $"Weekly hours ({weeklyHours:F2}) exceed 60 hours. Please verify.",
                    Severity = WarningSeverity.Medium
                });
            }
            else if (weeklyHours > 50)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.HighWeeklyHours,
                    Message = $"Weekly hours ({weeklyHours:F2}) are approaching overtime limits.",
                    Severity = WarningSeverity.Low
                });
            }
        }

        /// <summary>
        /// Auto-fix common issues in time card entries
        /// </summary>
        public bool TryAutoFixIssues(Models.TimeCard timeCard, List<TimeEntryWarning> warnings)
        {
            bool anyFixed = false;

            foreach (var warning in warnings.Where(w => w.Severity != WarningSeverity.High))
            {
                switch (warning.Type)
                {
                    case WarningType.MissingClockOut:
                        // If clock in is present but clock out is missing, and it's not today,
                        // assume a standard 8-hour day
                        if (timeCard.ClockIn.HasValue && timeCard.Date?.Date < DateTime.Today)
                        {
                            timeCard.ClockOut = timeCard.ClockIn.Value.Add(TimeSpan.FromHours(8));
                            anyFixed = true;
                        }
                        break;

                    case WarningType.MissingLunchOut:
                        // If working more than 6 hours, add a 30-minute lunch break
                        if (timeCard.ClockIn.HasValue && timeCard.ClockOut.HasValue)
                        {
                            var workHours = (timeCard.ClockOut.Value - timeCard.ClockIn.Value).TotalHours;
                            if (workHours > 6)
                            {
                                var midDay = timeCard.ClockIn.Value.Add(TimeSpan.FromHours(workHours / 2));
                                timeCard.LunchOut = midDay;
                                timeCard.LunchIn = midDay.Add(TimeSpan.FromMinutes(30));
                                anyFixed = true;
                            }
                        }
                        break;
                }
            }

            if (anyFixed)
            {
                timeCard.CalculateTotalHours();
            }

            return anyFixed;
        }

        /// <summary>
        /// Enhanced validation with intelligent conflict resolution suggestions
        /// </summary>
        public async Task<TimeEntryValidationResult> ValidateTimeCardWithResolutionAsync(Models.TimeCard timeCard, bool isNew)
        {
            var warnings = await ValidateTimeCardAsync(timeCard, isNew);
            var result = new TimeEntryValidationResult
            {
                Warnings = warnings,
                HasWarnings = warnings.Any(),
                HasCriticalIssues = warnings.Any(w => w.Severity == WarningSeverity.High)
            };

            // Generate intelligent resolution suggestions
            result.ResolutionSuggestions = GenerateResolutionSuggestions(timeCard, warnings);

            // Auto-fix recommendations
            result.AutoFixableIssues = GetAutoFixableIssues(warnings);

            return result;
        }

        /// <summary>
        /// Generate intelligent resolution suggestions for time entry issues
        /// </summary>
        private List<ResolutionSuggestion> GenerateResolutionSuggestions(Models.TimeCard timeCard, List<TimeEntryWarning> warnings)
        {
            var suggestions = new List<ResolutionSuggestion>();

            foreach (var warning in warnings)
            {
                switch (warning.Type)
                {
                    case WarningType.MissingClockIn:
                        suggestions.Add(new ResolutionSuggestion
                        {
                            WarningType = warning.Type,
                            SuggestionText = "Set Clock In to 7:00 AM (common start time)",
                            AutoFixAction = () => timeCard.ClockIn = new TimeSpan(7, 0, 0),
                            Confidence = 0.7f
                        });
                        break;

                    case WarningType.MissingClockOut:
                        if (timeCard.ClockIn.HasValue)
                        {
                            var suggestedClockOut = timeCard.ClockIn.Value.Add(TimeSpan.FromHours(8));
                            suggestions.Add(new ResolutionSuggestion
                            {
                                WarningType = warning.Type,
                                SuggestionText = $"Set Clock Out to {suggestedClockOut:hh\\:mm} (8-hour day)",
                                AutoFixAction = () => timeCard.ClockOut = suggestedClockOut,
                                Confidence = 0.8f
                            });
                        }
                        break;

                    case WarningType.MissingLunchOut:
                        if (timeCard.ClockIn.HasValue)
                        {
                            var suggestedLunchOut = timeCard.ClockIn.Value.Add(TimeSpan.FromHours(4));
                            suggestions.Add(new ResolutionSuggestion
                            {
                                WarningType = warning.Type,
                                SuggestionText = $"Set Lunch Out to {suggestedLunchOut:hh\\:mm} (4 hours after start)",
                                AutoFixAction = () => timeCard.LunchOut = suggestedLunchOut,
                                Confidence = 0.6f
                            });
                        }
                        break;

                    case WarningType.MissingLunchIn:
                        if (timeCard.LunchOut.HasValue)
                        {
                            var suggestedLunchIn = timeCard.LunchOut.Value.Add(TimeSpan.FromMinutes(30));
                            suggestions.Add(new ResolutionSuggestion
                            {
                                WarningType = warning.Type,
                                SuggestionText = $"Set Lunch In to {suggestedLunchIn:hh\\:mm} (30-minute lunch)",
                                AutoFixAction = () => timeCard.LunchIn = suggestedLunchIn,
                                Confidence = 0.9f
                            });
                        }
                        break;

                    case WarningType.InvalidTimeSequence:
                        suggestions.Add(new ResolutionSuggestion
                        {
                            WarningType = warning.Type,
                            SuggestionText = "Review time sequence - times should be in chronological order",
                            AutoFixAction = () => FixTimeSequence(timeCard),
                            Confidence = 0.5f
                        });
                        break;

                    case WarningType.ExcessiveHours:
                        suggestions.Add(new ResolutionSuggestion
                        {
                            WarningType = warning.Type,
                            SuggestionText = "Consider splitting long shifts or adding breaks",
                            AutoFixAction = () => OptimizeWorkHours(timeCard),
                            Confidence = 0.4f
                        });
                        break;
                }
            }

            return suggestions.OrderByDescending(s => s.Confidence).ToList();
        }

        /// <summary>
        /// Get auto-fixable issues with high confidence
        /// </summary>
        private List<ResolutionSuggestion> GetAutoFixableIssues(List<TimeEntryWarning> warnings)
        {
            var suggestions = new List<ResolutionSuggestion>();

            // Only include high-confidence fixes
            foreach (var warning in warnings)
            {
                if (warning.Type == WarningType.MissingLunchIn && warning.Severity != WarningSeverity.High)
                {
                    suggestions.Add(new ResolutionSuggestion
                    {
                        WarningType = warning.Type,
                        SuggestionText = "Auto-set 30-minute lunch break",
                        Confidence = 0.9f
                    });
                }
            }

            return suggestions;
        }

        /// <summary>
        /// Fix time sequence issues by reordering times logically
        /// </summary>
        private void FixTimeSequence(Models.TimeCard timeCard)
        {
            var times = new List<(string Name, TimeSpan? Time)>
            {
                ("ClockIn", timeCard.ClockIn),
                ("LunchOut", timeCard.LunchOut),
                ("LunchIn", timeCard.LunchIn),
                ("ClockOut", timeCard.ClockOut)
            };

            // Remove null times and sort
            var validTimes = times.Where(t => t.Time.HasValue)
                                 .OrderBy(t => t.Time!.Value)
                                 .ToList();

            // Reassign in logical order
            if (validTimes.Count >= 2)
            {
                timeCard.ClockIn = validTimes[0].Time;
                timeCard.ClockOut = validTimes.Last().Time;

                if (validTimes.Count >= 4)
                {
                    timeCard.LunchOut = validTimes[1].Time;
                    timeCard.LunchIn = validTimes[2].Time;
                }
            }
        }

        /// <summary>
        /// Optimize work hours to prevent excessive hours
        /// </summary>
        private void OptimizeWorkHours(Models.TimeCard timeCard)
        {
            if (timeCard.ClockIn.HasValue && timeCard.ClockOut.HasValue)
            {
                var totalHours = (timeCard.ClockOut.Value - timeCard.ClockIn.Value).TotalHours;

                // If over 12 hours, suggest 8-hour day
                if (totalHours > 12)
                {
                    timeCard.ClockOut = timeCard.ClockIn.Value.Add(TimeSpan.FromHours(8));
                }
            }
        }

        /// <summary>
        /// Validate driver schedule conflicts across multiple routes/activities
        /// </summary>
        public async Task<List<ScheduleConflict>> CheckDriverScheduleConflictsAsync(int driverId, DateTime date)
        {
            var conflicts = new List<ScheduleConflict>();

            // Get all time entries for the driver on this date
            var allTimeCards = await _timeCardRepository.GetByDriverIdAsync(driverId);
            var timeCards = allTimeCards.Where(tc => tc.Date.HasValue && tc.Date.Value.Date == date.Date);

            foreach (var timeCard in timeCards)
            {
                // Check for overlapping time periods
                var overlaps = timeCards.Where(tc => tc.TimeCardId != timeCard.TimeCardId)
                                       .Where(tc => HasTimeOverlap(timeCard, tc));

                foreach (var overlap in overlaps)
                {
                    conflicts.Add(new ScheduleConflict
                    {
                        DriverId = driverId,
                        Date = date,
                        ConflictType = ConflictType.OverlappingTimeCards,
                        Description = $"Time card {timeCard.TimeCardId} overlaps with {overlap.TimeCardId}",
                        Severity = WarningSeverity.High
                    });
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Check if two time cards have overlapping time periods
        /// </summary>
        private bool HasTimeOverlap(Models.TimeCard card1, Models.TimeCard card2)
        {
            if (!card1.ClockIn.HasValue || !card1.ClockOut.HasValue ||
                !card2.ClockIn.HasValue || !card2.ClockOut.HasValue)
                return false;

            return card1.ClockIn < card2.ClockOut && card2.ClockIn < card1.ClockOut;
        }
    }
}
