using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace BusBuddy.Models
{    public class Vehicle
    {
        public int BusId { get; set; } // Primary key as it exists in database
        public string? BusNumber { get; set; } // This matches the actual database column

        // Properties for UI compatibility
        public int Id { get; set; } // UI expects Id property with setter for tests

        public string? Make { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public int Capacity { get; set; }
        public int SeatingCapacity { get { return Capacity; } set { Capacity = value; } } // For database compatibility
        public string? FuelType { get; set; }
        public string? Status { get; set; }

        // Adding missing properties that are used in VehicleForm
        public string? VINNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LastInspectionDate { get; set; }
        public string? Notes { get; set; }

        // Helper property to get/set LastInspectionDate as DateTime
        public DateTime? DateLastInspectionAsDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(LastInspectionDate)) return null;
                if (DateTime.TryParseExact(LastInspectionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    return result;
                if (DateTime.TryParse(LastInspectionDate, out var fallbackResult))
                    return fallbackResult;
                return null;
            }
            set => LastInspectionDate = value?.ToString("yyyy-MM-dd");
        }        // Additional properties for compatibility
        public string? VIN => VINNumber;

        // Add concurrency control
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

