using System;

namespace BusBuddy.Models
{
    public class Fuel
    {
        public int FuelID { get; set; }
        public DateTime? FuelDate { get; set; }
        public string? FuelLocation { get; set; }  // Key Pumps, Gas Station
        public int? VehicleFueledID { get; set; }
        public decimal? VehicleOdometerReading { get; set; }
        public string? FuelType { get; set; }  // Gasoline, Diesel
        public decimal? FuelAmount { get; set; }
        public decimal? FuelCost { get; set; }
        public string? Notes { get; set; }

        // Navigation property
        public Vehicle? VehicleFueled { get; set; }
    }
}
