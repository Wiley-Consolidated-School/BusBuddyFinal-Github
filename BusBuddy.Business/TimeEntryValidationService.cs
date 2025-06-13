using System;
using System.Collections.Generic;
using System.Linq;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    public class TimeEntryValidationService
    {
        private readonly ITimeCardRepository _timeCardRepository;

        public TimeEntryValidationService(ITimeCardRepository timeCardRepository)
        {
            _timeCardRepository = timeCardRepository;
        }

        /// <summary>
        /// Validates a time card entry and returns warnings about potential clock-in/out mistakes
        /// </summary>
        public List<TimeEntryWarning> ValidateTimeCard(TimeCard timeCard, bool isNewEntry = true)
        {
            var warnings = new List<TimeEntryWarning>();

            // Get previous and next day entries for context
            var previousDay = GetPreviousWorkDay(timeCard.Date ?? DateTime.Today);
            var nextDay = GetNextWorkDay(timeCard.Date ?? DateTime.Today);

            var previousEntry = GetTimeCardForDate(previousDay);
            var nextEntry = GetTimeCardForDate(nextDay);

            // Check for missing clock out on previous day
            if (previousEntry != null && HasMissingClockOut(previousEntry))
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.MissingPreviousClockOut,
                    Message = $"Previous day ({previousDay:MM/dd}) appears to be missing a clock out time.",
                    Severity = WarningSeverity.High,
                    SuggestedAction = "Did you forget to clock out yesterday?",
                    RelatedDate = previousDay
                });
            }

            // Check for missing clock in on current day
            if (timeCard.AMClockIn == null && (timeCard.PMClockOut != null || timeCard.LunchClockOut != null))
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.MissingClockIn,
                    Message = "You have time entries but no clock in time.",
                    Severity = WarningSeverity.High,
                    SuggestedAction = "Did you forget to enter your clock in time?",
                    RelatedDate = timeCard.Date
                });
            }

            // Check for unusual time gaps
            var timeGapWarnings = CheckForUnusualTimeGaps(timeCard);
            warnings.AddRange(timeGapWarnings);

            // Check for overlapping times between days
            var overlapWarnings = CheckForTimeOverlaps(timeCard, previousEntry, nextEntry);
            warnings.AddRange(overlapWarnings);

            // Check for incomplete route times
            if (timeCard.DayType == "Route Day")
            {
                var routeWarnings = ValidateRouteEntries(timeCard);
                warnings.AddRange(routeWarnings);
            }

            // Check for extremely long work days (over 12 hours)
            var longDayWarning = CheckForLongWorkDay(timeCard);
            if (longDayWarning != null)
                warnings.Add(longDayWarning);

            return warnings;
        }

        private bool HasMissingClockOut(TimeCard timeCard)
        {
            // Check if there's a clock in but no clock out
            if (timeCard.AMClockIn != null && timeCard.PMClockOut == null)
                return true;

            // Check if there's route out but no route in
            if (timeCard.RouteAMClockOut != null && timeCard.RouteAMClockIn == null)
                return true;

            if (timeCard.RoutePMClockOut != null && timeCard.RoutePMClockIn == null)
                return true;

            return false;
        }

        private List<TimeEntryWarning> CheckForUnusualTimeGaps(TimeCard timeCard)
        {
            var warnings = new List<TimeEntryWarning>();

            // Check for extremely short lunch breaks (less than 15 minutes)
            if (timeCard.LunchClockOut != null && timeCard.LunchClockIn != null)
            {
                var lunchDuration = timeCard.LunchClockIn.Value - timeCard.LunchClockOut.Value;
                if (lunchDuration.TotalMinutes < 15)
                {
                    warnings.Add(new TimeEntryWarning
                    {
                        Type = WarningType.ShortLunchBreak,
                        Message = $"Lunch break is only {lunchDuration.TotalMinutes:F0} minutes.",
                        Severity = WarningSeverity.Medium,
                        SuggestedAction = "Is this correct? Most lunch breaks are at least 30 minutes.",
                        RelatedDate = timeCard.Date
                    });
                }
            }

            // Check for extremely long lunch breaks (over 2 hours)
            if (timeCard.LunchClockOut != null && timeCard.LunchClockIn != null)
            {
                var lunchDuration = timeCard.LunchClockIn.Value - timeCard.LunchClockOut.Value;
                if (lunchDuration.TotalHours > 2)
                {
                    warnings.Add(new TimeEntryWarning
                    {
                        Type = WarningType.LongLunchBreak,
                        Message = $"Lunch break is {lunchDuration.TotalHours:F1} hours.",
                        Severity = WarningSeverity.Medium,
                        SuggestedAction = "Is this correct? This seems like a very long lunch break.",
                        RelatedDate = timeCard.Date
                    });
                }
            }

            return warnings;
        }

        private List<TimeEntryWarning> CheckForTimeOverlaps(TimeCard currentCard, TimeCard previousCard, TimeCard nextCard)
        {
            var warnings = new List<TimeEntryWarning>();

            // Check if current day's clock in is very early (before 5 AM)
            if (currentCard.AMClockIn != null && currentCard.AMClockIn.Value.Hours < 5)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.VeryEarlyClockIn,
                    Message = $"Clock in time is {currentCard.AMClockIn.Value:hh\\:mm} - very early.",
                    Severity = WarningSeverity.Medium,
                    SuggestedAction = "Is this time correct?",
                    RelatedDate = currentCard.Date
                });
            }

            // Check if current day's clock out is very late (after 10 PM)
            if (currentCard.PMClockOut != null && currentCard.PMClockOut.Value.Hours > 22)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.VeryLateClockOut,
                    Message = $"Clock out time is {currentCard.PMClockOut.Value:hh\\:mm} - very late.",
                    Severity = WarningSeverity.Medium,
                    SuggestedAction = "Is this time correct?",
                    RelatedDate = currentCard.Date
                });
            }

            return warnings;
        }

        private List<TimeEntryWarning> ValidateRouteEntries(TimeCard timeCard)
        {
            var warnings = new List<TimeEntryWarning>();

            // Check AM route times
            if (timeCard.RouteAMClockOut != null && timeCard.RouteAMClockIn == null)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.IncompleteRouteEntry,
                    Message = "AM route has clock out but no clock in time.",
                    Severity = WarningSeverity.High,
                    SuggestedAction = "Did you forget to enter the AM route clock in time?",
                    RelatedDate = timeCard.Date
                });
            }

            // Check PM route times
            if (timeCard.RoutePMClockOut != null && timeCard.RoutePMClockIn == null)
            {
                warnings.Add(new TimeEntryWarning
                {
                    Type = WarningType.IncompleteRouteEntry,
                    Message = "PM route has clock out but no clock in time.",
                    Severity = WarningSeverity.High,
                    SuggestedAction = "Did you forget to enter the PM route clock in time?",
                    RelatedDate = timeCard.Date
                });
            }

            return warnings;
        }

        private TimeEntryWarning CheckForLongWorkDay(TimeCard timeCard)
        {
            if (timeCard.AMClockIn == null || timeCard.PMClockOut == null)
                return null;

            var totalWorkTime = timeCard.PMClockOut.Value - timeCard.AMClockIn.Value;

            // Subtract lunch break if provided
            if (timeCard.LunchClockOut != null && timeCard.LunchClockIn != null)
            {
                var lunchBreak = timeCard.LunchClockIn.Value - timeCard.LunchClockOut.Value;
                totalWorkTime = totalWorkTime - lunchBreak;
            }

            if (totalWorkTime.TotalHours > 12)
            {
                return new TimeEntryWarning
                {
                    Type = WarningType.ExcessiveWorkHours,
                    Message = $"Total work time is {totalWorkTime.TotalHours:F1} hours.",
                    Severity = WarningSeverity.High,
                    SuggestedAction = "This is an unusually long work day. Please verify all times are correct.",
                    RelatedDate = timeCard.Date
                };
            }

            return null;
        }

        private DateTime GetPreviousWorkDay(DateTime date)
        {
            var previousDay = date.AddDays(-1);

            // Skip weekends (assuming Saturday/Sunday are non-work days)
            while (previousDay.DayOfWeek == DayOfWeek.Saturday || previousDay.DayOfWeek == DayOfWeek.Sunday)
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        private DateTime GetNextWorkDay(DateTime date)
        {
            var nextDay = date.AddDays(1);

            // Skip weekends
            while (nextDay.DayOfWeek == DayOfWeek.Saturday || nextDay.DayOfWeek == DayOfWeek.Sunday)
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        private TimeCard GetTimeCardForDate(DateTime date)
        {
            try
            {
                var timeCards = _timeCardRepository.GetTimeCardsByDate(date);
                return timeCards.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }

    // Supporting classes for warnings
    public class TimeEntryWarning
    {
        public WarningType Type { get; set; }
        public string Message { get; set; }
        public WarningSeverity Severity { get; set; }
        public string SuggestedAction { get; set; }
        public DateTime? RelatedDate { get; set; }
    }

    public enum WarningType
    {
        MissingPreviousClockOut,
        MissingClockIn,
        ShortLunchBreak,
        LongLunchBreak,
        VeryEarlyClockIn,
        VeryLateClockOut,
        IncompleteRouteEntry,
        ExcessiveWorkHours
    }

    public enum WarningSeverity
    {
        Low,
        Medium,
        High
    }
}
