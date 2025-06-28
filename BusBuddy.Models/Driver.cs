using System;

namespace BusBuddy.Models
{
    public class Driver
    {
        public int DriverId { get; set; } // Primary key - matches SQL DriverId
        public string Name { get; set; } = string.Empty; // Matches SQL Name
        public string? FirstName { get; set; } // Matches SQL FirstName
        public string? LastName { get; set; } // Matches SQL LastName
        public string? DriverPhone { get; set; } // Matches SQL DriverPhone
        public string? DriverEmail { get; set; } // Matches SQL DriverEmail
        public string? Address { get; set; } // Matches SQL Address
        public string? City { get; set; } // Matches SQL City
        public string? State { get; set; } // Matches SQL State
        public string? Zip { get; set; } // Matches SQL Zip
        public string? DriversLicenseType { get; set; } // Matches SQL DriversLicenseType
        public DateTime? CDLExpirationDate { get; set; } // Matches SQL CDLExpirationDate
        public string? Status { get; set; } // Matches SQL Status
        public string? Notes { get; set; } // Matches SQL Notes
        public bool IsTrainingComplete { get; set; } // Matches SQL IsTrainingComplete

        // Computed properties for UI compatibility
        public string FullName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)
            ? Name
            : $"{FirstName} {LastName}".Trim();
        public string DisplayName => $"{Name} ({DriversLicenseType})";
    }
}

