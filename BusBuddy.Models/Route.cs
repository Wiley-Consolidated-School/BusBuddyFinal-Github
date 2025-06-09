using System;

namespace BusBuddy.Models
{
    public class Route
    {
        public int RouteID { get; set; }
        public DateTime Date { get; set; }
        public string? RouteName { get; set; }  // Truck Plaza, East Route, West Route, SPED
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
    }
}
