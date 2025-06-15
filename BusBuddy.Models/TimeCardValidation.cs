using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
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

    /// <summary>
    /// Enhanced time entry validation result with resolution suggestions
    /// </summary>
    public class TimeEntryValidationResult
    {
        public List<TimeEntryWarning> Warnings { get; set; } = new List<TimeEntryWarning>();
        public bool HasWarnings { get; set; }
        public bool HasCriticalIssues { get; set; }
        public List<ResolutionSuggestion> ResolutionSuggestions { get; set; } = new List<ResolutionSuggestion>();
        public List<ResolutionSuggestion> AutoFixableIssues { get; set; } = new List<ResolutionSuggestion>();
    }

    /// <summary>
    /// Represents a suggestion for resolving a time entry issue
    /// </summary>
    public class ResolutionSuggestion
    {
        public WarningType WarningType { get; set; }
        public string SuggestionText { get; set; } = string.Empty;
        public Action? AutoFixAction { get; set; }
        public float Confidence { get; set; } // 0.0 to 1.0
    }

    /// <summary>
    /// Represents a schedule conflict for a driver
    /// </summary>
    public class ScheduleConflict
    {
        public int DriverId { get; set; }
        public DateTime Date { get; set; }
        public ConflictType ConflictType { get; set; }
        public string Description { get; set; } = string.Empty;
        public WarningSeverity Severity { get; set; }
    }

    /// <summary>
    /// Types of schedule conflicts
    /// </summary>
    public enum ConflictType
    {
        OverlappingTimeCards,
        OverlappingRoutes,
        OverlappingActivities,
        ExcessiveHours,
        InsufficientRestTime
    }
}
