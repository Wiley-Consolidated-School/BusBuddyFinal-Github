using System;

namespace BusBuddy.Models
{
    public class Driver
    {
        public int DriverID { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? DriversLicenseType { get; set; }  // CDL or Passenger
        public int TrainingComplete { get; set; }
        public string? Notes { get; set; }

        // Additional properties needed by forms and validation
        public string? Status { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? CDLExpirationDate { get; set; }

        // Computed property for full name
        public string Name => $"{FirstName} {LastName}".Trim();

        // Helper property to convert between int and bool
        public bool IsTrainingComplete
        {
            get { return TrainingComplete > 0; }
            set { TrainingComplete = value ? 1 : 0; }
        }
    }
}
