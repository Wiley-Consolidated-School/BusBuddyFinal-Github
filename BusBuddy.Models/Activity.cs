using System;
using System.Globalization;

namespace BusBuddy.Models
{
    public class Activity
    {
        public int ActivityID { get; set; }
        public string? Date { get; set; }
        public string? ActivityType { get; set; }  // Sports Trip, Activity Trip
        public string? Destination { get; set; }

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

        // Store times as strings to match database schema
        public string? LeaveTime { get; set; }
        public string? EventTime { get; set; }
        public string? ReturnTime { get; set; }

        // Helper properties to get/set as TimeSpan for application logic
        public TimeSpan? LeaveTimeSpan
        {
            get => string.IsNullOrEmpty(LeaveTime) ? null : TimeSpan.Parse(LeaveTime);
            set => LeaveTime = value?.ToString(@"hh\:mm");
        }

        public TimeSpan? EventTimeSpan
        {
            get => string.IsNullOrEmpty(EventTime) ? null : TimeSpan.Parse(EventTime);
            set => EventTime = value?.ToString(@"hh\:mm");
        }

        public TimeSpan? ReturnTimeSpan
        {
            get => string.IsNullOrEmpty(ReturnTime) ? null : TimeSpan.Parse(ReturnTime);
            set => ReturnTime = value?.ToString(@"hh\:mm");
        }

        public string? RequestedBy { get; set; }
        public int? AssignedVehicleID { get; set; }
        public int? AssignedBusID { get; set; }
        public int? DriverId { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Bus? AssignedVehicle { get; set; }
        public Driver? Driver { get; set; }
    }
}

