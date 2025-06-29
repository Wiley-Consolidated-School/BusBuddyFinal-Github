using System;
using System.Globalization;
using BusBuddy.Models;

namespace BusBuddy.Models
{
    public class ActivitySchedule
    {
        public int ScheduleID { get; set; }
        public DateTime Date { get; set; }
        public string? ActivityName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? VehicleID { get; set; }
        public int? DriverID { get; set; }
        public string? Notes { get; set; }
        public int? CalendarID { get; set; } // Foreign key to SchoolCalendar

        // Legacy properties for backward compatibility
        public string? TripType
        {
            get => ActivityName;
            set => ActivityName = value;
        }
        public DateTime? DateAsDateTime
        {
            get => Date;
            set => Date = value ?? DateTime.Today;
        }
        public int? ScheduledVehicleID
        {
            get => VehicleID;
            set => VehicleID = value;
        }
        public string? ScheduledDestination { get; set; } // Additional field for destination
        public TimeSpan? ScheduledLeaveTime
        {
            get => StartTime;
            set => StartTime = value;
        }
        public TimeSpan? ScheduledEventTime { get; set; } // Additional field for event time
        public TimeSpan? ScheduledReturnTime
        {
            get => EndTime;
            set => EndTime = value;
        }
        public int? ScheduledRiders { get; set; } // Additional field for rider count
        public int? ScheduledDriverID
        {
            get => DriverID;
            set => DriverID = value;
        }

        // Navigation properties
        public Bus? ScheduledVehicle { get; set; }
        public Driver? ScheduledDriver { get; set; }
        public SchoolCalendar? Calendar { get; set; } // Navigation to calendar
    }
}

