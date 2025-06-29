using System;

namespace BusBuddy.Models
{
    public class Driver
    {
        public int DriverId { get; set; } // Primary key - matches SQL DriverID
        public string DriverName { get; set; } = string.Empty; // Matches SQL DriverName
        public string FirstName { get; set; } = string.Empty; // Matches SQL FirstName
        public string LastName { get; set; } = string.Empty; // Matches SQL LastName
        public string Name
        {
            get => $"{FirstName} {LastName}";
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    FirstName = parts.Length > 0 ? parts[0] : string.Empty;
                    LastName = parts.Length > 1 ? parts[1] : string.Empty;
                }
                else
                {
                    FirstName = string.Empty;
                    LastName = string.Empty;
                }
            }
        } // Computed property for full name
        public DateTime? CDLExpirationDate { get; set; } // Matches SQL CDLExpirationDate
        public string? Status { get; set; } // Matches SQL Status
        public string? Notes { get; set; } // Matches SQL Notes
        public bool IsTrainingComplete { get; set; } // Matches SQL IsTrainingComplete
        public string? DriverPhone { get; set; } // Matches SQL DriverPhone
        public string? DriverEmail { get; set; } // Matches SQL DriverEmail
        public string? Address { get; set; } // Matches SQL Address
        public string? City { get; set; } // Matches SQL City
        public string? State { get; set; } // Matches SQL State
        public string? Zip { get; set; } // Matches SQL Zip
        public string? DriversLicenseType { get; set; } // Matches SQL DriversLicenseType
        public bool TrainingComplete { get; set; } // Matches SQL TrainingComplete

        // Computed property for UI compatibility
        public string DisplayName => $"{DriverName} ({DriversLicenseType})";
    }
}

