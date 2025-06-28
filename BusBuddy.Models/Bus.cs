using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BusBuddy.Models
{
    /// <summary>
    /// Bus model that represents a vehicle in bus-specific terminology
    /// Maps to the underlying Vehicle data structure but provides bus-specific properties
    /// </summary>
    public class Bus
    {
        public int Id { get; set; } // Primary identifier
        public string? BusNumber { get; set; } // Bus-specific identifier
        public string? VehicleNumber { get; set; } // Vehicle identifier from database
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public int Capacity { get; set; }
        public int SeatingCapacity { get; set; }
        public string? FuelType { get; set; }
        public string? Status { get; set; }
        public string? VINNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public DateTime? DateLastInspection { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// Gets a display-friendly identifier for the bus
        /// </summary>
        public string DisplayName => BusNumber ?? VehicleNumber ?? $"Bus {Id}";

        /// <summary>
        /// Gets a comprehensive description of the bus
        /// </summary>
        public string Description => $"{Year} {Make} {Model} ({DisplayName})";

        /// <summary>
        /// Checks if the bus is active and available for service
        /// </summary>
        public bool IsActive => Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Checks if the bus is due for inspection (over 1 year since last inspection)
        /// </summary>
        public bool IsDueForInspection
        {
            get
            {
                if (!DateLastInspection.HasValue)
                    return true;

                return (DateTime.Now - DateLastInspection.Value).TotalDays > 365;
            }
        }

        /// <summary>
        /// Gets the age of the bus in years
        /// </summary>
        public int Age => DateTime.Now.Year - Year;

        /// <summary>
        /// Gets a status indicator for display purposes
        /// </summary>
        public string StatusIndicator
        {
            get
            {
                return Status?.ToLower() switch
                {
                    "active" => "🟢 Active",
                    "maintenance" => "🟡 Maintenance",
                    "out of service" => "🔴 Out of Service",
                    _ => "⚪ Unknown"
                };
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Bus other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
