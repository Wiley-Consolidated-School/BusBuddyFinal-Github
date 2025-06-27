using System;
using Xunit;
using BusBuddy.Models;

// Disable nullable reference types for testing null arguments
#nullable disable

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Tests for Vehicle model ensuring proper property handling and data integrity.
    /// Tests cover property validation, relationships, and data format consistency.
    /// </summary>
    public class VehicleModelTest
    {
        #region Property Tests

        [Fact]
        public void Vehicle_WhenCreated_ShouldHaveDefaultValues()
        {
            // Act
            var vehicle = new Vehicle();

            // Assert
            Assert.Equal(0, vehicle.Id);
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Equal(0, vehicle.Year);
            Assert.Equal(0, vehicle.SeatingCapacity);
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
        }

        [Fact]
        public void Vehicle_PropertiesCanBeSetAndRetrieved()
        {
            // Arrange
            var vehicle = new Vehicle();
            var testData = CreateValidVehicleData();

            // Act
            vehicle.Id = testData.Id;
            vehicle.VehicleNumber = testData.VehicleNumber;
            vehicle.Make = testData.Make;
            vehicle.Model = testData.Model;
            vehicle.Year = testData.Year;
            vehicle.SeatingCapacity = testData.SeatingCapacity;
            vehicle.FuelType = testData.FuelType;
            vehicle.Status = testData.Status;
            vehicle.VINNumber = testData.VINNumber;
            vehicle.LicenseNumber = testData.LicenseNumber;
            vehicle.DateLastInspection = testData.DateLastInspection;

            // Assert
            Assert.Equal(testData.Id, vehicle.Id);
            Assert.Equal(testData.VehicleNumber, vehicle.VehicleNumber);
            Assert.Equal(testData.Make, vehicle.Make);
            Assert.Equal(testData.Model, vehicle.Model);
            Assert.Equal(testData.Year, vehicle.Year);
            Assert.Equal(testData.SeatingCapacity, vehicle.SeatingCapacity);
            Assert.Equal(testData.FuelType, vehicle.FuelType);
            Assert.Equal(testData.Status, vehicle.Status);
            Assert.Equal(testData.VINNumber, vehicle.VINNumber);
            Assert.Equal(testData.LicenseNumber, vehicle.LicenseNumber);
            Assert.Equal(testData.DateLastInspection, vehicle.DateLastInspection);
        }

        #endregion

        #region Date Handling Tests

        [Fact]
        public void Vehicle_DateLastInspection_AcceptsValidDateString()
        {
            // Arrange
            var vehicle = new Vehicle();
            var validDate = "2023-12-15";

            // Act
            vehicle.DateLastInspection = validDate;

            // Assert
            Assert.Equal(validDate, vehicle.DateLastInspection);
        }

        [Fact]
        public void Vehicle_DateLastInspection_AcceptsNullValue()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.DateLastInspection = null;

            // Assert
            Assert.Null(vehicle.DateLastInspection);
        }

        [Fact]
        public void Vehicle_DateLastInspection_AcceptsEmptyString()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.DateLastInspection = string.Empty;

            // Assert
            Assert.Equal(string.Empty, vehicle.DateLastInspection);
        }

        #endregion

        #region Numeric Property Tests

        [Theory]
        [InlineData(1990)]
        [InlineData(2023)]
        [InlineData(2025)]
        public void Vehicle_Year_AcceptsValidYears(int year)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Year = year;

            // Assert
            Assert.Equal(year, vehicle.Year);
        }

        [Theory]
        [InlineData(12)]
        [InlineData(72)]
        [InlineData(84)]
        public void Vehicle_SeatingCapacity_AcceptsValidCapacities(int capacity)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.SeatingCapacity = capacity;

            // Assert
            Assert.Equal(capacity, vehicle.SeatingCapacity);
        }

        #endregion

        #region String Property Tests

        [Theory]
        [InlineData("BUS001")]
        [InlineData("V-123")]
        [InlineData("SCHOOL_BUS_45")]
        public void Vehicle_VehicleNumber_AcceptsValidNumbers(string vehicleNumber)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.VehicleNumber = vehicleNumber;

            // Assert
            Assert.Equal(vehicleNumber, vehicle.VehicleNumber);
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("Out of Service")]
        [InlineData("Maintenance")]
        public void Vehicle_Status_AcceptsValidStatuses(string status)
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
        [InlineData("Propane")]
        [InlineData("Electric")]
        public void Vehicle_FuelType_AcceptsValidFuelTypes(string fuelType)
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.FuelType = fuelType;

            // Assert
            Assert.Equal(fuelType, vehicle.FuelType);
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public void Vehicle_CanHandleNullStringProperties()
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
            vehicle.DateLastInspection = null;

            // Assert - Should not throw exceptions
            Assert.Null(vehicle.VehicleNumber);
            Assert.Null(vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Null(vehicle.FuelType);
            Assert.Null(vehicle.Status);
            Assert.Null(vehicle.VINNumber);
            Assert.Null(vehicle.LicenseNumber);
            Assert.Null(vehicle.DateLastInspection);
        }

        [Fact]
        public void Vehicle_CanHandleNullNumericProperties()
        {
            // Arrange
            var vehicle = new Vehicle();

            // Act
            vehicle.Year = 0;
            vehicle.SeatingCapacity = 0;

            // Assert
            Assert.Equal(0, vehicle.Year);
            Assert.Equal(0, vehicle.SeatingCapacity);
        }

        #endregion

        #region Helper Methods

        private Vehicle CreateValidVehicleData()
        {
            return new Vehicle
            {
                Id = 1,
                VehicleNumber = "BUS001",
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2023,
                SeatingCapacity = 72,
                FuelType = "Diesel",
                Status = "Active",
                VINNumber = "1BAANBBX8KF123456",
                LicenseNumber = "ABC123",
                DateLastInspection = "2023-12-15"
            };
        }

        #endregion
    }
}
