using System;

namespace BusBuddy.Models
{
    public class ActivitySchedule
    {
        public int ScheduleID { get; set; }
        public DateTime? Date { get; set; }
        public string? TripType { get; set; }  // Sports Trip, Activity Trip
        public int? ScheduledVehicleID { get; set; }
        public string? ScheduledDestination { get; set; }
        public TimeSpan? ScheduledLeaveTime { get; set; }
        public TimeSpan? ScheduledEventTime { get; set; }
        public TimeSpan? ScheduledReturnTime { get; set; }
        public int? ScheduledRiders { get; set; }
        public int? ScheduledDriverID { get; set; }
        public string? Notes { get; set; }
        
        // Navigation properties
        public Vehicle? ScheduledVehicle { get; set; }
        public Driver? ScheduledDriver { get; set; }
    }
}
