using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int VehicleID { get { return Id; } set { Id = value; } } // For backward compatibility
        public string? VehicleNumber { get; set; }

        // BusNumber is mapped from database but we don't want it to override VehicleNumber
        // Use a private field to store the database BusNumber value separately
        private string? _busNumber;
        public string? BusNumber
        {
            get { return _busNumber ?? VehicleNumber; }
            set { _busNumber = value; } // Store separately, don't override VehicleNumber
        }

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
