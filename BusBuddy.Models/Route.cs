using System;
using System.Globalization;

namespace BusBuddy.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public string Date { get; set; } = string.Empty;
        public string? RouteName { get; set; }  // Truck Plaza, East Route, West Route, SPED

        // Helper property to get/set Date as DateTime
        public DateTime DateAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(Date)) return DateTime.Today;
                if (DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(Date, out var fallbackResult))
                    return fallbackResult;
                return DateTime.Today;
            }
            set => Date = value.ToString("yyyy-MM-dd");
        }
        public int? AMVehicleID { get; set; }
        public decimal? AMBeginMiles { get; set; }
        public decimal? AMEndMiles { get; set; }
        public int? AMRiders { get; set; }
        public int? AMDriverID { get; set; }
        public int? PMVehicleID { get; set; }
        public decimal? PMBeginMiles { get; set; }
        public decimal? PMEndMiles { get; set; }
        public int? PMRiders { get; set; }
        public int? PMDriverID { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public Vehicle? AMVehicle { get; set; }
        public Driver? AMDriver { get; set; }
        public Vehicle? PMVehicle { get; set; }
        public Driver? PMDriver { get; set; }

        // Computed properties for form compatibility
        public string? AMVehicleNumber => AMVehicle?.VehicleNumber;
        public string? AMDriverName => AMDriver?.Name;
        public string? PMVehicleNumber => PMVehicle?.VehicleNumber;
        public string? PMDriverName => PMDriver?.Name;
    }
}
