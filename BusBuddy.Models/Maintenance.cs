using System;
using System.Globalization;
using BusBuddy.Models;

namespace BusBuddy.Models
{
    public class Maintenance
    {
        public int MaintenanceID { get; set; }
        public string? Date { get; set; }
        public int? BusId { get; set; }

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

        public decimal? OdometerReading { get; set; }
        public string? MaintenanceCompleted { get; set; }  // Tires, Windshield, Alignment, Mechanical, Car Wash, Cleaning, Accessory Install
        public string? Vendor { get; set; }
        public decimal? RepairCost { get; set; }
        public string? Notes { get; set; }

        // Additional properties for form compatibility
        public string? BusNumber { get; set; }
        public decimal? Odometer => OdometerReading;
        public string? Category => MaintenanceCompleted;
        public decimal? Cost => RepairCost;
        public string? Description => Notes;

        // Navigation property
        public Bus? Vehicle { get; set; }
    }
}

