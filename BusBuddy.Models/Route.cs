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
                // Try ISO format first (preserves time)
                if (DateTime.TryParseExact(Date, "yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var isoResult))
                    return EnsureSqlServerCompatible(isoResult);
                if (DateTime.TryParseExact(Date, "yyyy-MM-ddTHH:mm:ss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.None, out var isoResult2))
                    return EnsureSqlServerCompatible(isoResult2);
                // Try date-only format
                if (DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return EnsureSqlServerCompatible(result);
                if (DateTime.TryParse(Date, out var fallbackResult))
                    return EnsureSqlServerCompatible(fallbackResult);
                return DateTime.Today;
            }
            set => Date = EnsureSqlServerCompatible(value).ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
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
        public Driver? PMDriver { get; set; }        // Computed properties for form compatibility
        public string? AMVehicleNumber => AMVehicle?.VehicleNumber;
        public string? AMDriverName => AMDriver?.Name;
        public string? PMVehicleNumber => PMVehicle?.VehicleNumber;
        public string? PMDriverName => PMDriver?.Name;

        // Computed mileage properties for backward compatibility
        public decimal? AMMiles => AMEndMiles.HasValue && AMBeginMiles.HasValue ? AMEndMiles - AMBeginMiles : null;
        public decimal? PMMiles => PMEndMiles.HasValue && PMBeginMiles.HasValue ? PMEndMiles - PMBeginMiles : null;
    }
}
