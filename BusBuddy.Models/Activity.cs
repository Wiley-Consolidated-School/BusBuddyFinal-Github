using System;

namespace BusBuddy.Models
{
    public class Activity
    {
        public int ActivityID { get; set; }
        public DateTime? Date { get; set; }
        public string? ActivityType { get; set; }  // Sports Trip, Activity Trip
        public string? Destination { get; set; }
        public TimeSpan? LeaveTime { get; set; }
        public TimeSpan? EventTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string? RequestedBy { get; set; }
        public int? AssignedVehicleID { get; set; }
        public int? DriverID { get; set; }
        public string? Notes { get; set; }
        
        // Navigation properties
        public Vehicle? AssignedVehicle { get; set; }
        public Driver? Driver { get; set; }
    }
}
