using System;
using System.Collections.Generic;
using Xunit;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;

// Disable nullable reference types for testing null arguments
#nullable disable

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Tests for ValidationService ensuring proper business rule enforcement and data integrity.
    /// Tests cover vehicle availability, driver availability, fuel records, and maintenance validation.
    /// </summary>
    public class ValidationServiceTest : IDisposable
    {
        private readonly ValidationService _validationService;
        private readonly VehicleRepository _vehicleRepository;
        private readonly DriverRepository _driverRepository;
        private readonly List<int> _testVehicleIds;
        private readonly List<int> _testDriverIds;

        public ValidationServiceTest()
        {
            _vehicleRepository = new VehicleRepository();
            _driverRepository = new DriverRepository();
            _validationService = new ValidationService(
                _vehicleRepository,
                _driverRepository,
                new RouteRepository(),
                new MaintenanceRepository(),
                new FuelRepository());

            _testVehicleIds = new List<int>();
            _testDriverIds = new List<int>();
        }

        public void Dispose()
        {
            // Cleanup test data
            foreach (var id in _testVehicleIds)
            {
                try { _vehicleRepository.DeleteVehicle(id); } catch { }
            }
            foreach (var id in _testDriverIds)
            {
                try { _driverRepository.DeleteDriver(id); } catch { }
            }
        }

        #region Vehicle Availability Tests

        [Fact]
        public void ValidateVehicleAvailability_WithValidVehicle_ShouldReturnSuccess()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            testVehicle.Status = "Active";
            var vehicleId = _vehicleRepository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            // Act
            var result = _validationService.ValidateVehicleAvailability(vehicleId, DateTime.Today, "test operation");

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateVehicleAvailability_WithNonExistentVehicle_ShouldReturnFailure()
        {
            // Act
            var result = _validationService.ValidateVehicleAvailability(99999, DateTime.Today, "test operation");

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("does not exist", result.GetErrorMessage());
        }

        [Fact]
        public void ValidateVehicleAvailability_WithOutOfServiceVehicle_ShouldReturnFailure()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            testVehicle.Status = "Out of Service";
            var vehicleId = _vehicleRepository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            // Act
            var result = _validationService.ValidateVehicleAvailability(vehicleId, DateTime.Today, "test operation");

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("out of service", result.GetErrorMessage().ToLower());
        }

        #endregion

        #region Driver Availability Tests

        [Fact]
        public void ValidateDriverAvailability_WithActiveDriver_ShouldReturnSuccess()
        {
            // Arrange
            var testDriver = CreateTestDriver();
            testDriver.Status = "Active";
            testDriver.CDLExpirationDate = DateTime.Today.AddYears(1);
            var driverId = _driverRepository.AddDriver(testDriver);
            _testDriverIds.Add(driverId);

            // Act
            var result = _validationService.ValidateDriverAvailability(driverId, DateTime.Today, "test operation");

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateDriverAvailability_WithInactiveDriver_ShouldReturnFailure()
        {
            // Arrange
            var testDriver = CreateTestDriver();
            testDriver.Status = "Inactive";
            var driverId = _driverRepository.AddDriver(testDriver);
            _testDriverIds.Add(driverId);

            // Act
            var result = _validationService.ValidateDriverAvailability(driverId, DateTime.Today, "test operation");

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("inactive", result.GetErrorMessage().ToLower());
        }

        [Fact]
        public void ValidateDriverAvailability_WithExpiredLicense_ShouldReturnFailure()
        {
            // Arrange
            var testDriver = CreateTestDriver();
            testDriver.Status = "Active";
            testDriver.CDLExpirationDate = DateTime.Today.AddDays(-1); // Expired yesterday
            var driverId = _driverRepository.AddDriver(testDriver);
            _testDriverIds.Add(driverId);

            // Act
            var result = _validationService.ValidateDriverAvailability(driverId, DateTime.Today, "test operation");

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("expired", result.GetErrorMessage().ToLower());
        }

        #endregion

        #region Fuel Record Validation Tests

        [Fact]
        public void ValidateFuelRecord_WithValidRecord_ShouldReturnSuccess()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            testVehicle.Status = "Active";
            var vehicleId = _vehicleRepository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            var fuelRecord = new Fuel
            {
                FuelDate = DateTime.Today.ToString("yyyy-MM-dd"),
                FuelLocation = "Test Station",
                VehicleFueledID = vehicleId,
                FuelAmount = 25.5m,
                FuelCost = 87.98m,
                VehicleOdometerReading = 125000
            };

            // Act
            var result = _validationService.ValidateFuelRecord(fuelRecord);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateFuelRecord_WithNullRecord_ShouldReturnFailure()
        {
            // Act
            var result = _validationService.ValidateFuelRecord(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot be null", result.GetErrorMessage());
        }

        [Fact]
        public void ValidateFuelRecord_WithNegativeGallons_ShouldReturnFailure()
        {
            // Arrange
            var fuelRecord = new Fuel
            {
                FuelDate = DateTime.Today.ToString("yyyy-MM-dd"),
                FuelLocation = "Test Station",
                VehicleFueledID = 1,
                FuelAmount = -5.0m, // Negative amount
                FuelCost = 87.98m
            };

            // Act
            var result = _validationService.ValidateFuelRecord(fuelRecord);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("negative", result.GetErrorMessage().ToLower());
        }

        #endregion

        #region Maintenance Record Validation Tests

        [Fact]
        public void ValidateMaintenanceRecord_WithValidRecord_ShouldReturnSuccess()
        {
            // Arrange
            var testVehicle = CreateTestVehicle();
            testVehicle.Status = "Active";
            var vehicleId = _vehicleRepository.AddVehicle(testVehicle);
            _testVehicleIds.Add(vehicleId);

            var maintenanceRecord = new Maintenance
            {
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                VehicleID = vehicleId,
                MaintenanceCompleted = "Oil Change",
                RepairCost = 75.00m,
                OdometerReading = 125000
            };

            // Act
            var result = _validationService.ValidateMaintenanceRecord(maintenanceRecord);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void ValidateMaintenanceRecord_WithNullRecord_ShouldReturnFailure()
        {
            // Act
            var result = _validationService.ValidateMaintenanceRecord(null);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("cannot be null", result.GetErrorMessage());
        }

        [Fact]
        public void ValidateMaintenanceRecord_WithNegativeCost_ShouldReturnFailure()
        {
            // Arrange
            var maintenanceRecord = new Maintenance
            {
                Date = DateTime.Today.ToString("yyyy-MM-dd"),
                VehicleID = 1,
                MaintenanceCompleted = "Oil Change",
                RepairCost = -50.00m, // Negative cost
                OdometerReading = 125000
            };

            // Act
            var result = _validationService.ValidateMaintenanceRecord(maintenanceRecord);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("negative", result.GetErrorMessage().ToLower());
        }

        #endregion

        #region Vehicle Number Validation Tests

        [Theory]
        [InlineData("BUS001", true)]
        [InlineData("V123", true)]
        [InlineData("AB", false)] // Too short
        [InlineData("", false)] // Empty
        [InlineData(null, false)] // Null
        public void IsValidVehicleNumber_WithVariousInputs_ShouldReturnExpectedResult(string vehicleNumber, bool expected)
        {
            // Act
            var result = _validationService.IsValidVehicleNumber(vehicleNumber);

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Helper Methods

        private Vehicle CreateTestVehicle()
        {
            var timestamp = DateTime.Now.Ticks;
            return new Vehicle
            {
                VehicleNumber = $"VTEST{timestamp}",
                Make = "Test Make",
                Model = "Test Model",
                Year = 2023,
                SeatingCapacity = 72,
                FuelType = "Diesel",
                Status = "Active",
                VINNumber = $"TESTVIN{timestamp}",
                LicenseNumber = $"TESTLIC{timestamp}",
                DateLastInspection = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd")
            };
        }

        private Driver CreateTestDriver()
        {
            var timestamp = DateTime.Now.Ticks;
            return new Driver
            {
                FirstName = "Test",
                LastName = $"Driver{timestamp}",
                DriverName = $"Test Driver{timestamp}",
                DriversLicenseType = "CDL",
                Status = "Active",
                DriverPhone = "555-0123",
                DriverEmail = $"test{timestamp}@example.com",
                CDLExpirationDate = DateTime.Today.AddYears(2),
                TrainingComplete = 1
            };
        }

        #endregion
    }
}
