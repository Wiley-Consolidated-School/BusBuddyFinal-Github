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
    }

    /// <summary>
    /// Represents a time entry validation warning
    /// </summary>
    public class TimeEntryWarning
    {
        public WarningType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public WarningSeverity Severity { get; set; }
    }

    /// <summary>
    /// Types of time entry warnings
    /// </summary>
    public enum WarningType
    {
        MissingDate,
        MissingClockIn,
        MissingClockOut,
        MissingLunchOut,
        MissingLunchIn,
        InvalidTimeSequence,
        DuplicateEntry,
        ExcessiveHours,
        ExcessiveWeeklyHours,
        HighWeeklyHours,
        OvertimeDetected,
        InvalidDriver
    }

    /// <summary>
    /// Severity levels for warnings
    /// </summary>
    public enum WarningSeverity
    {
        Low,
        Medium,
        High
    }
}
