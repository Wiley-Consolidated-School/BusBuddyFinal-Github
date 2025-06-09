using Xunit;
using BusBuddy.Models;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Simple test to verify project compilation and basic functionality
    /// </summary>
    public class SimpleCompilationTest
    {
        [Fact]
        public void Vehicle_Creation_ShouldWork()
        {
            // Arrange & Act
            var vehicle = new Vehicle
            {
                Id = 1,
                VehicleNumber = "TEST001",
                Make = "TestMake",
                Model = "TestModel",
                Year = 2023,
                Status = "Active"
            };

            // Assert
            Assert.Equal(1, vehicle.Id);
            Assert.Equal("TEST001", vehicle.VehicleNumber);
            Assert.Equal("TestMake", vehicle.Make);
        }

        [Fact]
        public void Driver_Creation_ShouldWork()
        {
            // Arrange & Act
            var driver = new Driver
            {
                DriverID = 1,
                DriverName = "Test Driver",
                DriverPhone = "555-1234",
                TrainingComplete = 1  // Using int as per model definition
            };

            // Assert
            Assert.Equal(1, driver.DriverID);
            Assert.Equal("Test Driver", driver.DriverName);
            Assert.True(driver.IsTrainingComplete); // Using the helper property
        }
    }
}
