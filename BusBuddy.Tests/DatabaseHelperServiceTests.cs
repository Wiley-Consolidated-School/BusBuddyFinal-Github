using Xunit;
using Moq;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Comprehensive test class for DatabaseHelperService
    /// Tests all data aggregation and helper functionality
    /// </summary>
    public class DatabaseHelperServiceTests
    {
        private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
        private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

        #region Constructor Tests

        [Fact]
        public void DatabaseHelperService_Constructor_ShouldInitializeSuccessfully()
        {
            // Act & Assert
            var service = new DatabaseHelperService();
            Assert.NotNull(service);
        }

        #endregion

        #region GetRouteWithDetails Tests

        [Fact]
        public void GetRouteWithDetails_WithValidRouteId_ShouldReturnRouteWithAllDetails()
        {
            // Arrange
            var service = new DatabaseHelperService();

            // First create test data
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var routeRepo = new RouteRepository();

            // Create test vehicle and driver
            var testVehicle = new Vehicle
            {
                VehicleNumber = "TEST001",
                Status = "Active",
                Capacity = 50,
                Make = "Test Make",
                Model = "Test Model",
                Year = 2020
            };
            vehicleRepo.AddVehicle(testVehicle);
            var vehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = vehicles.FirstOrDefault(v => v.VehicleNumber == "TEST001");

            var testDriver = new Driver
            {
                FirstName = "Test",
                LastName = "Driver",
                DriversLicenseType = "CDL",
                DriverPhone = "555-0123",
                DriverEmail = "test@example.com"
            };
            driverRepo.AddDriver(testDriver);
            var drivers = driverRepo.GetAllDrivers();
            var createdDriver = drivers.FirstOrDefault(d => d.FirstName == "Test");

            var testRoute = new Route
            {
                RouteName = "Test Route",
                DateAsDateTime = DateTime.Today,
                AMVehicleID = createdVehicle?.VehicleID,
                AMDriverID = createdDriver?.DriverID,
                PMVehicleID = createdVehicle?.VehicleID,
                PMDriverID = createdDriver?.DriverID,
                AMBeginMiles = 100,
                AMEndMiles = 150,
                PMBeginMiles = 150,
                PMEndMiles = 200
            };
            routeRepo.AddRoute(testRoute);
            var routes = routeRepo.GetAllRoutes();
            var createdRoute = routes.FirstOrDefault(r => r.RouteName == "Test Route");
            Assert.NotNull(createdRoute); // Ensure route was created

            // Act
            var result = service.GetRouteWithDetails(createdRoute.RouteID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Route", result.RouteName);
            Assert.NotNull(result.AMVehicle);
            Assert.NotNull(result.AMDriver);
            Assert.NotNull(result.PMVehicle);
            Assert.NotNull(result.PMDriver);
            Assert.Equal("TEST001", result.AMVehicle.VehicleNumber);
            Assert.Equal("Test Driver", result.AMDriver.Name);

            // Cleanup
            Assert.NotNull(createdVehicle); // Ensure vehicle was created
            Assert.NotNull(createdDriver); // Ensure driver was created
            routeRepo.DeleteRoute(createdRoute.RouteID);
            vehicleRepo.DeleteVehicle(createdVehicle.VehicleID);
            driverRepo.DeleteDriver(createdDriver.DriverID);
        }

        [Fact]
        public void GetRouteWithDetails_WithNonExistentRouteId_ShouldReturnNull()
        {
            // Arrange
            var service = new DatabaseHelperService();

            // Act
            var result = service.GetRouteWithDetails(999999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetRouteWithDetails_WithRouteHavingNoVehicles_ShouldReturnRouteWithNullVehicles()
        {
            // Arrange
            var service = new DatabaseHelperService();
            var routeRepo = new RouteRepository();

            var testRoute = new Route
            {
                RouteName = "No Vehicle Route",
                DateAsDateTime = DateTime.Today,
                AMVehicleID = null,
                AMDriverID = null,
                PMVehicleID = null,
                PMDriverID = null,
                AMBeginMiles = 0,
                AMEndMiles = 0,
                PMBeginMiles = 0,
                PMEndMiles = 0
            };
            routeRepo.AddRoute(testRoute);
            var routes = routeRepo.GetAllRoutes();
            var createdRoute = routes.FirstOrDefault(r => r.RouteName == "No Vehicle Route");
            Assert.NotNull(createdRoute); // Ensure route was created

            // Act
            var result = service.GetRouteWithDetails(createdRoute.RouteID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("No Vehicle Route", result.RouteName);
            Assert.Null(result.AMVehicle);
            Assert.Null(result.AMDriver);
            Assert.Null(result.PMVehicle);
            Assert.Null(result.PMDriver);

            // Cleanup
            routeRepo.DeleteRoute(createdRoute.RouteID);
        }

        #endregion

        #region GetVehicleDetails Tests

        [Fact]
        public void GetVehicleDetails_WithValidVehicleId_ShouldReturnVehicleDetails()
        {
            // Arrange
            var service = new DatabaseHelperService();
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);

            // Create test vehicle
            var testVehicle = new Vehicle
            {
                VehicleNumber = "MAINT001",
                Status = "Active",
                Capacity = 50,
                Make = "Test Make",
                Model = "Test Model",
                Year = 2020
            };
            vehicleRepo.AddVehicle(testVehicle);
            var vehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = vehicles.FirstOrDefault(v => v.VehicleNumber == "MAINT001");
            Assert.NotNull(createdVehicle); // Ensure vehicle was created

            // Create test maintenance records
            var maintenance1 = new Maintenance
            {
                VehicleID = createdVehicle.VehicleID,
                DateAsDateTime = DateTime.Today.AddDays(-10),
                Notes = "Oil Change",
                RepairCost = 50.00m,
                MaintenanceCompleted = "Routine"
            };
            var maintenance2 = new Maintenance
            {
                VehicleID = createdVehicle.VehicleID,
                DateAsDateTime = DateTime.Today.AddDays(-5),
                Notes = "Brake Repair",
                RepairCost = 200.00m,
                MaintenanceCompleted = "Repair"
            };
            maintenanceRepo.AddMaintenance(maintenance1);
            maintenanceRepo.AddMaintenance(maintenance2);

            // Act
            var result = service.GetVehicleDetails(createdVehicle.VehicleID);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.MaintenanceRecords);
            Assert.True(result.MaintenanceRecords.Count >= 2);
            Assert.Contains(result.MaintenanceRecords, m => m.Description == "Oil Change");
            Assert.Contains(result.MaintenanceRecords, m => m.Description == "Brake Repair");

            // Cleanup
            var allMaintenance = maintenanceRepo.GetMaintenanceByVehicle(createdVehicle.VehicleID);
            foreach (var maint in allMaintenance)
            {
                maintenanceRepo.DeleteMaintenance(maint.MaintenanceID);
            }
            vehicleRepo.DeleteVehicle(createdVehicle.VehicleID);
        }

        [Fact]
        public void GetVehicleDetails_WithNonExistentVehicleId_ShouldReturnNull()
        {
            // Arrange
            var service = new DatabaseHelperService();

            // Act
            var result = service.GetVehicleDetails(999999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetDriverDetails Tests

        [Fact]
        public void GetDriverDetails_WithValidDriverId_ShouldReturnDriverDetails()
        {
            // Arrange
            var service = new DatabaseHelperService();
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var routeRepo = new RouteRepository();

            // Create test driver
            var testDriver = new Driver
            {
                FirstName = "Assignment",
                LastName = "Test",
                DriversLicenseType = "CDL",
                DriverPhone = "555-0123",
                DriverEmail = "assign@test.com"
            };
            driverRepo.AddDriver(testDriver);
            var drivers = driverRepo.GetAllDrivers();
            var createdDriver = drivers.FirstOrDefault(d => d.FirstName == "Assignment");
            Assert.NotNull(createdDriver); // Ensure driver was created

            // Create test routes with driver assignments
            var route1 = new Route
            {
                RouteName = "Assignment Route 1",
                DateAsDateTime = DateTime.Today,
                AMDriverID = createdDriver.DriverID,
                AMBeginMiles = 100,
                AMEndMiles = 120
            };
            var route2 = new Route
            {
                RouteName = "Assignment Route 2",
                DateAsDateTime = DateTime.Today,
                PMDriverID = createdDriver.DriverID,
                PMBeginMiles = 150,
                PMEndMiles = 170
            };
            routeRepo.AddRoute(route1);
            routeRepo.AddRoute(route2);

            // Act
            var result = service.GetDriverDetails(createdDriver.DriverID);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Driver);
            Assert.Equal("Assignment", result.Driver.FirstName);

            // Cleanup
            var routes = routeRepo.GetAllRoutes();
            var createdRoutes = routes.Where(r => r.RouteName != null && r.RouteName.Contains("Assignment Route")).ToList();
            foreach (var route in createdRoutes)
            {
                routeRepo.DeleteRoute(route.RouteID);
            }
            driverRepo.DeleteDriver(createdDriver.DriverID);
        }

        [Fact]
        public void GetDriverDetails_WithNonExistentDriverId_ShouldReturnNull()
        {
            // Arrange
            var service = new DatabaseHelperService();

            // Act
            var result = service.GetDriverDetails(999999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetDailyRoutesSummary Tests

        [Fact]
        public void GetDailyRoutesSummary_WithValidDate_ShouldReturnSummary()
        {
            // Arrange
            var service = new DatabaseHelperService();
            var routeRepo = new RouteRepository();

            var testDate = DateTime.Today;
            var route1 = new Route
            {
                RouteName = "Summary Route 1",
                DateAsDateTime = testDate,
                AMBeginMiles = 100,
                AMEndMiles = 120,
                PMBeginMiles = 120,
                PMEndMiles = 140,
                AMRiders = 25,
                PMRiders = 30
            };
            var route2 = new Route
            {
                RouteName = "Summary Route 2",
                DateAsDateTime = testDate,
                AMBeginMiles = 200,
                AMEndMiles = 230,
                PMBeginMiles = 230,
                PMEndMiles = 250,
                AMRiders = 35,
                PMRiders = 28
            };
            routeRepo.AddRoute(route1);
            routeRepo.AddRoute(route2);

            // Act
            var result = service.GetRoutesWithDetailsByDate(testDate);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count >= 2); // Should have at least the 2 routes we created
            var createdRoutes = result.Where(r => r.RouteName?.Contains("Summary Route") == true).ToList();
            Assert.True(createdRoutes.Count >= 2);

            // Cleanup
            var allRoutes = routeRepo.GetAllRoutes();
            var routesToDelete = allRoutes.Where(r => r.RouteName?.Contains("Summary Route") == true).ToList();
            foreach (var route in routesToDelete)
            {
                routeRepo.DeleteRoute(route.RouteID);
            }
        }

        [Fact]
        public void GetRoutesWithDetailsByDate_WithNoRoutesForDate_ShouldReturnEmptyList()
        {
            // Arrange
            var service = new DatabaseHelperService();
            var futureDate = DateTime.Today.AddYears(1);

            // Act
            var result = service.GetRoutesWithDetailsByDate(futureDate);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void DatabaseHelperService_ShouldHandleNullParametersGracefully()
        {
            // Arrange
            var service = new DatabaseHelperService();

            // Act & Assert - Should not throw exceptions
            var result1 = service.GetVehicleDetails(0);
            // result1 can be null for invalid ID

            var result2 = service.GetDriverDetails(0);
            // result2 can be null for invalid ID

            var result3 = service.GetRoutesWithDetailsByDate(DateTime.MinValue);
            Assert.NotNull(result3); // Should return empty list, not null
        }

        #endregion
    }
}
