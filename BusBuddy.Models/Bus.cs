using System;

namespace BusBuddy.Models
{
    /// <summary>
    /// Bus model that represents a bus in the BusBuddy system
    /// Maps directly to the Buses table in the database
    /// </summary>
    public class Bus
    {
        public int BusId { get; set; } // Primary key - matches SQL BusId
        public string BusNumber { get; set; } = string.Empty; // Matches SQL BusNumber
        public int? Year { get; set; } // Matches SQL Year
        public string? Make { get; set; } // Matches SQL Make
        public string? Model { get; set; } // Matches SQL Model
        public int Capacity { get; set; } // Matches SQL Capacity
        // For backward compatibility with tests and UI
        public int SeatingCapacity
        {
            get => Capacity;
            set => Capacity = value;
        }
        public string? VIN { get; set; } // Matches SQL VIN
        public string? LicenseNumber { get; set; } // Matches SQL LicenseNumber
        public DateTime? LastInspectionDate { get; set; } // Matches SQL LastInspectionDate
        public string? Status { get; set; } // UI expects Status

        // Computed properties for UI compatibility and display
        public string DisplayName => $"{BusNumber} ({Year} {Make} {Model})".Trim();
        public string FullDescription => $"Bus {BusNumber} - {Year} {Make} {Model} (Seats: {Capacity})";
        public override string ToString()
        {
            return BusNumber;
        }
    }
}

