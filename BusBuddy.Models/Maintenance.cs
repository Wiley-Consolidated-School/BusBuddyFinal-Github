using System;

namespace BusBuddy.Models
{
    public class Maintenance
    {
        public int MaintenanceID { get; set; }
        public DateTime? Date { get; set; }
        public int? VehicleID { get; set; }
        public decimal? OdometerReading { get; set; }
        public string? MaintenanceCompleted { get; set; }  // Tires, Windshield, Alignment, Mechanical, Car Wash, Cleaning, Accessory Install
        public string? Vendor { get; set; }
        public decimal? RepairCost { get; set; }
        public string? Notes { get; set; }
        
        // Navigation property
        public Vehicle? Vehicle { get; set; }
    }
}
