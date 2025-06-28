using System;
using System.Globalization;

namespace BusBuddy.Models
{
    public class ActivitySchedule
    {
        public int ScheduleID { get; set; }
        public string? Date { get; set; }
        public string? TripType { get; set; }  // Sports Trip, Activity Trip

        // Helper property to get/set Date as DateTime
        public DateTime? DateAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(Date)) return null;
                if (DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(Date, out var fallbackResult))
                    return fallbackResult;
                return null;
            }
            set => Date = value?.ToString("yyyy-MM-dd");
        }
        public int? ScheduledVehicleID { get; set; }
        public string? ScheduledDestination { get; set; }
        public TimeSpan? ScheduledLeaveTime { get; set; }
        public TimeSpan? ScheduledEventTime { get; set; }
        public TimeSpan? ScheduledReturnTime { get; set; }
        public int? ScheduledRiders { get; set; }
        public int? ScheduledDriverID { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Bus? ScheduledVehicle { get; set; }
        public Driver? ScheduledDriver { get; set; }
    }
}

