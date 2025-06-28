using System;
using System.Globalization;

namespace BusBuddy.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        public DateTime RouteDate { get; set; } // Primary date property that matches the database column
        public string? RouteName { get; set; }  // Truck Plaza, East Route, West Route, SPED

        // Date property for backward compatibility - maps to RouteDate
        public string Date
        {
            get => RouteDate.ToString("yyyy-MM-dd");
            set => RouteDate = DateTime.TryParse(value, out var result) ? result : DateTime.Today;
        }

        // Helper property to get/set Date as DateTime (same as RouteDate)
        public DateTime DateAsDateTime
        {
            get => RouteDate;
            set => RouteDate = EnsureSqlServerCompatible(value);
        }

        // Helper method to ensure DateTime values are within SQL Server's valid range
        private static DateTime EnsureSqlServerCompatible(DateTime dateTime)
        {
            // SQL Server DateTime range: 1753-01-01 00:00:00 to 9999-12-31 23:59:59.997
            var minSqlDate = new DateTime(1753, 1, 1);
            var maxSqlDate = new DateTime(9999, 12, 31, 23, 59, 59, 997);

            if (dateTime < minSqlDate)
                return minSqlDate;
            if (dateTime > maxSqlDate)
                return maxSqlDate;
            return dateTime;
        }

        // AM Route Data
        public int? AMBusId { get; set; }
        public int? AMBeginMiles { get; set; }
        public int? AMEndMiles { get; set; }
        public int? AMRiders { get; set; }
        public int? AMDriverId { get; set; }

        // PM Route Data
        public int? PMBusId { get; set; }
        public int? PMBeginMiles { get; set; }
        public int? PMEndMiles { get; set; }
        public int? PMRiders { get; set; }
        public int? PMDriverId { get; set; }

        // Additional Properties
        public string? Notes { get; set; }
        public string? RouteType { get; set; } = "CDL"; // CDL, SmallBus, SPED

        // Navigation properties
        public Bus? AMBus { get; set; }
        public Driver? AMDriver { get; set; }
        public Bus? PMBus { get; set; }
        public Driver? PMDriver { get; set; }

        // Computed properties for form compatibility
        public string? AMBusNumber => AMBus?.BusNumber;
        public string? AMDriverName => AMDriver?.Name;
        public string? PMBusNumber => PMBus?.BusNumber;
        public string? PMDriverName => PMDriver?.Name;

        // Computed mileage properties for backward compatibility
        public int? AMMiles => AMEndMiles.HasValue && AMBeginMiles.HasValue ? AMEndMiles - AMBeginMiles : null;
        public int? PMMiles => PMEndMiles.HasValue && PMBeginMiles.HasValue ? PMEndMiles - PMBeginMiles : null;
    }
}

