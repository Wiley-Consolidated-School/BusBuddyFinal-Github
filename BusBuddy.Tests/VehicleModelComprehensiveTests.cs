using Xunit;
using BusBuddy.Models;
using System;

namespace BusBuddy.Tests
{
    public class VehicleModelComprehensiveTests
    {
        [Fact]
        public void Vehicle_Constructor_InitializesWithDefaultValues()
        {
            // Act
            var vehicle = new Vehicle();

            // Assert
            Assert.Equal(0, vehicle.Id);
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Equal(0, vehicle.Year);
            Assert.Equal(0, vehicle.Capacity);
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
        }

        [Fact]
        public void Vehicle_AllProperties_SetAndGetCorrectly()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Id = 123;
            vehicle.VehicleNumber = "BUS001";
            vehicle.Make = "Blue Bird";
            vehicle.Model = "Vision";
            vehicle.Year = 2020;
            vehicle.Capacity = 72;
            vehicle.FuelType = "Diesel";
            vehicle.Status = "Active";
            vehicle.VINNumber = "1234567890ABCDEF";
            vehicle.LicenseNumber = "ABC123";

            // Assert
            Assert.Equal(123, vehicle.Id);
            Assert.Equal("BUS001", vehicle.VehicleNumber);
            Assert.Equal("Blue Bird", vehicle.Make);
            Assert.Equal("Vision", vehicle.Model);
            Assert.Equal(2020, vehicle.Year);
            Assert.Equal(72, vehicle.Capacity);
            Assert.Equal("Diesel", vehicle.FuelType);
            Assert.Equal("Active", vehicle.Status);
            Assert.Equal("1234567890ABCDEF", vehicle.VINNumber);
            Assert.Equal("ABC123", vehicle.LicenseNumber);
        }

        [Fact]
        public void VehicleID_BackwardCompatibility_WorksCorrectly()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.VehicleID = 456;

            // Assert
            Assert.Equal(456, vehicle.Id);
            Assert.Equal(456, vehicle.VehicleID);
        }

        [Fact]
        public void SeatingCapacity_BackwardCompatibility_WorksCorrectly()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.SeatingCapacity = 60;

            // Assert
            Assert.Equal(60, vehicle.Capacity);
            Assert.Equal(60, vehicle.SeatingCapacity);
        }

        [Fact]
        public void BusNumber_WhenVehicleNumberExists_ReturnsBusNumberIfSet()
        {
            // Arrange
            var vehicle = new Vehicle();
            vehicle.VehicleNumber = "VEH001";
            vehicle.BusNumber = "BUS001";

            // Act & Assert
            Assert.Equal("BUS001", vehicle.BusNumber);
        }

        [Fact]
        public void BusNumber_WhenVehicleNumberExists_ReturnsFallback()
        {
            // Arrange
            var vehicle = new Vehicle();
            vehicle.VehicleNumber = "VEH001";

            // Act & Assert
            Assert.Equal("VEH001", vehicle.BusNumber);
        }

        [Fact]
        public void BusNumber_WhenNeitherSet_ReturnsNull()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act & Assert
            Assert.Null(vehicle.BusNumber);
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("Inactive")]
        [InlineData("Maintenance")]
        [InlineData("Retired")]
        public void Status_ValidStatuses_SetCorrectly(string status)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Status = status;

            // Assert
            Assert.Equal(status, vehicle.Status);
        }

        [Theory]
        [InlineData("Diesel")]
        [InlineData("Gasoline")]
        [InlineData("Electric")]
        [InlineData("Hybrid")]
        [InlineData("CNG")]
        public void FuelType_ValidTypes_SetCorrectly(string fuelType)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.FuelType = fuelType;

            // Assert
            Assert.Equal(fuelType, vehicle.FuelType);
        }

        [Fact]
        public void Vehicle_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.VehicleNumber = null;
            vehicle.Make = null;
            vehicle.Model = null;
            vehicle.FuelType = null;
            vehicle.Status = null;
            vehicle.VINNumber = null;
            vehicle.LicenseNumber = null;

            // Assert - Should not throw exceptions
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
            Assert.Null(vehicle.VINNumber);
            Assert.Null(vehicle.LicenseNumber);
        }

        [Fact]
        public void Vehicle_YearValidation_AcceptsReasonableValues()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act & Assert
            vehicle.Year = 1990;
            Assert.Equal(1990, vehicle.Year);

            vehicle.Year = 2025;
            Assert.Equal(2025, vehicle.Year);

            vehicle.Year = 2050;
            Assert.Equal(2050, vehicle.Year);
        }

        [Fact]
        public void Vehicle_CapacityValidation_AcceptsValidValues()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act & Assert
            vehicle.Capacity = 0;
            Assert.Equal(0, vehicle.Capacity);

            vehicle.Capacity = 30;
            Assert.Equal(30, vehicle.Capacity);

            vehicle.Capacity = 90;
            Assert.Equal(90, vehicle.Capacity);
        }
    }
}
