using System;
using System.Globalization;
using BusBuddy.Models;

namespace BusBuddy.Models
{
    public class Fuel
    {
        public int FuelID { get; set; }
        public string? FuelDate { get; set; }
        public string FuelLocation { get; set; }

        // Helper property to get/set FuelDate as DateTime
        public DateTime? FuelDateAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(FuelDate)) return null;
                if (DateTime.TryParseExact(FuelDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(FuelDate, out var fallbackResult))
                    return fallbackResult;
                return null;
            }
            set => FuelDate = value?.ToString("yyyy-MM-dd");
        }

        public int? VehicleFueledID { get; set; }
        public decimal? VehicleOdometerReading { get; set; }
        public string? FuelType { get; set; }  // Gasoline, Diesel
        public decimal? FuelAmount { get; set; }
        public decimal? FuelCost { get; set; }
        public string? Notes { get; set; }

        // Navigation property
        public Bus? VehicleFueled { get; set; }
    }
}

