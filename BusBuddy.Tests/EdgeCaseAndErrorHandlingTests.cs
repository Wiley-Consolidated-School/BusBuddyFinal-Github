using Xunit;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Additional comprehensive tests for edge cases and error scenarios
    /// Focuses on improving test coverage for boundary conditions
    /// </summary>
    public class EdgeCaseAndErrorHandlingTests
    {
        private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
        private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

        #region Repository Edge Cases

        [Fact]
        public void VehicleRepository_GetVehicleById_WithNegativeId_ShouldReturnNull()
        {
            // Arrange
            var repository = new VehicleRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetVehicleById(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void VehicleRepository_GetVehicleById_WithZeroId_ShouldReturnNull()
        {
            // Arrange
            var repository = new VehicleRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetVehicleById(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void VehicleRepository_UpdateVehicle_WithNonExistentVehicle_ShouldReturnFalse()
        {
            // Arrange
            var repository = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var nonExistentVehicle = new Vehicle
            {
                VehicleID = 999999,
                VehicleNumber = "NONEXIST",
                Make = "Test",
                Model = "Test"
            };

            // Act
            var result = repository.UpdateVehicle(nonExistentVehicle);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DriverRepository_GetDriverById_WithNegativeId_ShouldReturnNull()
        {
            // Arrange
            var repository = new DriverRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetDriverById(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DriverRepository_UpdateDriver_WithNonExistentDriver_ShouldReturnFalse()
        {
            // Arrange
            var repository = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var nonExistentDriver = new Driver
            {
                DriverID = 999999,
                FirstName = "NonExistent",
                LastName = "Driver",
                DriversLicenseType = "CDL"
            };

            // Act
            var result = repository.UpdateDriver(nonExistentDriver);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RouteRepository_GetRouteById_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var repository = new RouteRepository();

            // Act
            var result = repository.GetRouteById(-999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FuelRepository_GetFuelRecordsByVehicleId_WithNonExistentVehicle_ShouldReturnEmptyList()
        {
            // Arrange
            var repository = new FuelRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetFuelRecordsByVehicle(999999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MaintenanceRepository_GetMaintenanceByVehicleId_WithNonExistentVehicle_ShouldReturnEmptyList()
        {
            // Arrange
            var repository = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetMaintenanceByVehicle(999999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Business Logic Edge Cases

        [Fact]
        public void ValidationService_ValidateVehicleAvailability_WithNegativeVehicleId_ShouldReturnInvalid()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var validationService = new ValidationService(vehicleRepo, driverRepo, maintenanceRepo, fuelRepo);

            // Act
            var result = validationService.ValidateVehicleAvailability(-1, DateTime.Now);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidationService_ValidateDriverAvailability_WithNegativeDriverId_ShouldReturnInvalid()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var validationService = new ValidationService(vehicleRepo, driverRepo, maintenanceRepo, fuelRepo);

            // Act
            var result = validationService.ValidateDriverAvailability(-1, DateTime.Now);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidationService_ValidateFuelRecord_WithNullFuelRecord_ShouldReturnInvalid()
        {
            // Arrange
            var validationService = new ValidationService();
            var invalidFuelRecord = new Fuel(); // Empty fuel record should be invalid

            // Act
            var result = validationService.ValidateFuelRecord(invalidFuelRecord);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public void ValidationService_ValidateMaintenanceRecord_WithNullMaintenanceRecord_ShouldReturnInvalid()
        {
            // Arrange
            var validationService = new ValidationService();
            var invalidMaintenanceRecord = new Maintenance(); // Empty maintenance record should be invalid

            // Act
            var result = validationService.ValidateMaintenanceRecord(invalidMaintenanceRecord);

            // Assert
            Assert.False(result.IsValid);
        }

        #endregion

        #region Model Edge Cases

        [Fact]
        public void Vehicle_Properties_ShouldHandleNullAndEmptyValues()
        {
            // Arrange & Act
            var vehicle = new Vehicle
            {
                VehicleNumber = null,
                Make = "",
                Model = null,
                Status = ""
            };

            // Assert
            Assert.Null(vehicle.VehicleNumber);
            Assert.Equal("", vehicle.Make);
            Assert.Null(vehicle.Model);
            Assert.Equal("", vehicle.Status);
        }

        [Fact]
        public void Driver_Name_Property_WithNullFirstAndLastName_ShouldReturnEmptyString()
        {
            // Arrange
            var driver = new Driver
            {
                FirstName = null,
                LastName = null
            };

            // Act
            var name = driver.Name;

            // Assert
            Assert.Equal("", name.Trim());
        }

        [Fact]
        public void Route_DateAsDateTime_Property_ShouldHandleMinAndMaxValues()
        {
            // Arrange & Act
            var route1 = new Route { DateAsDateTime = DateTime.MinValue };
            var route2 = new Route { DateAsDateTime = DateTime.MaxValue };

            // Assert
            Assert.Equal(DateTime.MinValue, route1.DateAsDateTime);
            Assert.Equal(DateTime.MaxValue, route2.DateAsDateTime);
        }

        [Fact]
        public void Fuel_FuelAmount_Property_ShouldHandleZeroValue()
        {
            // Arrange & Act
            var fuel = new Fuel { FuelAmount = 0 };

            // Assert
            Assert.Equal(0, fuel.FuelAmount);
        }

        [Fact]
        public void Maintenance_RepairCost_Property_ShouldHandleMaxDecimalValue()
        {
            // Arrange & Act
            var maintenance = new Maintenance { RepairCost = decimal.MaxValue };

            // Assert
            Assert.Equal(decimal.MaxValue, maintenance.RepairCost);
        }

        #endregion

        #region Analytics Service Edge Cases

        [Fact]
        public void RouteAnalyticsService_CalculateRouteEfficiency_WithZeroMiles_ShouldHandleGracefully()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var route = new Route
            {
                RouteID = 1,
                RouteName = "Zero Miles Route",
                DateAsDateTime = DateTime.Now,
                AMBeginMiles = 100,
                AMEndMiles = 100,  // No miles driven
                AMRiders = 25,
                PMBeginMiles = 100,
                PMEndMiles = 100,  // No miles driven
                PMRiders = 20
            };

            // Act
            var metrics = service.CalculateRouteEfficiency(route);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(0, metrics.TotalMiles);
            Assert.Equal(45, metrics.TotalRiders);
            Assert.Equal(0, metrics.MilesPerRider); // Should handle division by zero
        }

        [Fact]
        public void RouteAnalyticsService_CalculateRouteEfficiency_WithZeroRiders_ShouldHandleGracefully()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var route = new Route
            {
                RouteID = 1,
                RouteName = "Zero Riders Route",
                DateAsDateTime = DateTime.Now,
                AMBeginMiles = 100,
                AMEndMiles = 150,
                AMRiders = 0,  // No riders
                PMBeginMiles = 150,
                PMEndMiles = 200,
                PMRiders = 0   // No riders
            };

            // Act
            var metrics = service.CalculateRouteEfficiency(route);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(100, metrics.TotalMiles);
            Assert.Equal(0, metrics.TotalRiders);
            // Should handle infinite miles per rider appropriately
            Assert.True(metrics.MilesPerRider >= 0);
        }

        [Fact]
        public void RouteAnalyticsService_CalculateRouteEfficiency_WithNullRoute_ShouldReturnNull()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var invalidRoute = new Route(); // Empty route should return null metrics

            // Act
            var metrics = service.CalculateRouteEfficiency(invalidRoute);

            // Assert
            Assert.Null(metrics);
        }

        #endregion

        #region Predictive Maintenance Edge Cases

        [Fact]
        public async Task PredictiveMaintenanceService_GetMaintenancePredictionsAsync_WithInvalidVehicleId_ShouldThrowException()
        {
            // Arrange
            var service = new PredictiveMaintenanceService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.GetMaintenancePredictionsAsync(-1);
            });
        }

        [Fact]
        public async Task PredictiveMaintenanceService_GetMaintenancePredictionsAsync_WithVehicleHavingNoMileage_ShouldReturnBasicPredictions()
        {
            // Arrange
            var service = new PredictiveMaintenanceService();
            var vehicleId = 1; // Assuming vehicle with ID 1 exists in test data

            // Act
            var predictions = await service.GetMaintenancePredictionsAsync(vehicleId);

            // Assert
            Assert.NotNull(predictions);
        }

        #endregion

        #region Boundary Value Tests

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        public void VehicleRepository_GetVehicleById_WithBoundaryValues_ShouldHandleGracefully(int vehicleId)
        {
            // Arrange
            var repository = new VehicleRepository(_testConnectionString, _sqlServerProvider);

            // Act
            var result = repository.GetVehicleById(vehicleId);

            // Assert
            // Should not throw exceptions and return null for invalid IDs
            if (vehicleId <= 0)
            {
                Assert.Null(result);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("AB")]
        [InlineData("123456789012345678901234567890")] // Very long string
        public void VehicleService_IsValidVehicleNumber_WithBoundaryValues_ShouldValidateCorrectly(string vehicleNumber)
        {
            // Arrange
            var mockRepo = new Moq.Mock<IVehicleRepository>();
            var service = new VehicleService(mockRepo.Object);

            // Act
            var result = service.IsValidVehicleNumber(vehicleNumber);

            // Assert
            if (string.IsNullOrEmpty(vehicleNumber) || vehicleNumber.Length < 3)
            {
                Assert.False(result);
            }
            else
            {
                Assert.True(result);
            }
        }

        [Theory]
        [InlineData(1900)] // Very old year
        [InlineData(2100)] // Future year
        [InlineData(0)]    // Invalid year
        [InlineData(-1)]   // Negative year
        public void VehicleService_CalculateVehicleAge_WithBoundaryYears_ShouldCalculateCorrectly(int year)
        {
            // Arrange
            var mockRepo = new Moq.Mock<IVehicleRepository>();
            var service = new VehicleService(mockRepo.Object);
            var expectedAge = DateTime.Now.Year - year;

            // Act
            var age = service.CalculateVehicleAge(year);

            // Assert
            Assert.Equal(expectedAge, age);
        }

        #endregion

        #region Date Handling Edge Cases

        [Fact]
        public void RouteRepository_GetRoutesByDate_WithInvalidDate_ShouldHandleGracefully()
        {
            // Arrange
            var repository = new RouteRepository();
            var invalidDate = DateTime.MaxValue;

            // Act
            var result = repository.GetRoutesByDate(invalidDate);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result); // Should return empty list for invalid date
        }

        [Fact]
        public void FuelRepository_GetFuelRecordsByDate_WithSpecificDate_ShouldReturnRecordsForThatDay()
        {
            // Arrange
            var repository = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var targetDate = DateTime.Today;

            // Act
            var result = repository.GetFuelRecordsByDate(targetDate);

            // Assert
            Assert.NotNull(result);
            // Should return records for that specific day
        }

        #endregion
    }
}
