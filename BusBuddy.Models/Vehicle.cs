using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int VehicleID { get { return Id; } set { Id = value; } } // For backward compatibility
        public string? VehicleNumber { get; set; }
        public string? BusNumber { get { return VehicleNumber; } set { VehicleNumber = value; } } // For backward compatibility
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public int Capacity { get; set; }
        public int SeatingCapacity { get { return Capacity; } set { Capacity = value; } } // For backward compatibility
        public string? FuelType { get; set; }
        public string? Status { get; set; }
        
        // Adding missing properties that are used in VehicleForm
        public string? VINNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? DateLastInspection { get; set; }
        public string? Notes { get; set; }
    }
}