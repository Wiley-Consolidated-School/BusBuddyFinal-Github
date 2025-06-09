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

        // Helper property to convert between int and bool
        public bool IsTrainingComplete
        {
            get { return TrainingComplete > 0; }
            set { TrainingComplete = value ? 1 : 0; }
        }
    }
}
