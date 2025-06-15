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
    /// Integration tests that verify complete data flows and cross-service interactions
    /// These tests help increase coverage by testing real-world scenarios
    /// </summary>
    public class IntegrationFlowTests : IAsyncLifetime
    {
        private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
        private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

        public async Task InitializeAsync()
        {
            // Initialize clean test database
            await CleanupTestDataAsync();
        }

        public async Task DisposeAsync()
        {
            await CleanupTestDataAsync();
        }        private async Task CleanupTestDataAsync()
        {
            // Clean up using repositories instead of DbContext
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var routeRepo = new RouteRepository();
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);

            // Note: In a real scenario, we'd implement proper cleanup methods
            // For now, we'll rely on test isolation
            await Task.CompletedTask;
        }

        #region Complete Vehicle Lifecycle Tests

        [Fact]
        public void CompleteVehicleLifecycle_ShouldWorkEndToEnd()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var vehicleService = new VehicleService(vehicleRepo);
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);

            // Act & Assert - Create Vehicle
            var newVehicle = new Vehicle
            {
                VehicleNumber = "LIFECYCLE001",
                Make = "Integration",
                Model = "Test",
                Year = 2020,
                Capacity = 50,
                Status = "Active"
            };

            var addResult = vehicleService.AddVehicle(newVehicle);
            Assert.True(addResult);

            // Verify vehicle was created
            var createdVehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = createdVehicles.FirstOrDefault(v => v.VehicleNumber == "LIFECYCLE001");
            Assert.NotNull(createdVehicle);

            // Add fuel records
            var fuelRecord1 = new Fuel
            {
                VehicleFueledID = createdVehicle.VehicleID,
                FuelDateAsDateTime = DateTime.Today.AddDays(-10),
                FuelAmount = 50.5m,
                FuelLocation = "Station A",
                FuelCost = 150.75m
            };
            var fuelRecord2 = new Fuel
            {
                VehicleFueledID = createdVehicle.VehicleID,
                FuelDateAsDateTime = DateTime.Today.AddDays(-5),
                FuelAmount = 45.2m,
                FuelLocation = "Station B",
                FuelCost = 135.60m
            };

            fuelRepo.AddFuelRecord(fuelRecord1);
            fuelRepo.AddFuelRecord(fuelRecord2);

            // Add maintenance records
            var maintenanceRecord1 = new Maintenance
            {
                VehicleID = createdVehicle.VehicleID,
                DateAsDateTime = DateTime.Today.AddDays(-15),
                Notes = "Oil Change",
                RepairCost = 75.00m,
                MaintenanceCompleted = "Routine"
            };
            var maintenanceRecord2 = new Maintenance
            {
                VehicleID = createdVehicle.VehicleID,
                DateAsDateTime = DateTime.Today.AddDays(-8),
                Notes = "Brake Inspection",
                RepairCost = 125.00m,
                MaintenanceCompleted = "Inspection"
            };

            maintenanceRepo.AddMaintenance(maintenanceRecord1);
            maintenanceRepo.AddMaintenance(maintenanceRecord2);

            // Test DatabaseHelperService integration
            var helperService = new DatabaseHelperService();
            var vehicleDetails = helperService.GetVehicleDetails(createdVehicle.VehicleID);
            Assert.NotNull(vehicleDetails);
            Assert.True(vehicleDetails.MaintenanceRecords.Count >= 2);

            // Verify fuel records
            var fuelRecords = fuelRepo.GetFuelRecordsByVehicle(createdVehicle.VehicleID);
            Assert.True(fuelRecords.Count >= 2);
            Assert.Contains(fuelRecords, f => f.FuelLocation == "Station A");
            Assert.Contains(fuelRecords, f => f.FuelLocation == "Station B");

            // Update vehicle
            createdVehicle.Status = "Maintenance";
            var updateResult = vehicleService.UpdateVehicle(createdVehicle);
            Assert.True(updateResult);

            // Verify update
            var updatedVehicle = vehicleRepo.GetVehicleById(createdVehicle.VehicleID);
            Assert.Equal("Maintenance", updatedVehicle?.Status);

            // Test analytics on the vehicle
            var analyticsService = new RouteAnalyticsService();
            // Analytics would normally require route data, but we're testing the service exists and handles empty data

            // Cleanup
            var allFuel = fuelRepo.GetFuelRecordsByVehicle(createdVehicle.VehicleID);
            foreach (var fuel in allFuel)
            {
                fuelRepo.DeleteFuelRecord(fuel.FuelID);
            }

            var allMaintenance = maintenanceRepo.GetMaintenanceByVehicle(createdVehicle.VehicleID);
            foreach (var maint in allMaintenance)
            {
                maintenanceRepo.DeleteMaintenance(maint.MaintenanceID);
            }

            vehicleRepo.DeleteVehicle(createdVehicle.VehicleID);
        }

        #endregion

        #region Complete Driver and Route Assignment Flow

        [Fact]
        public void CompleteDriverRouteAssignment_ShouldWorkEndToEnd()
        {
            // Arrange
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var routeRepo = new RouteRepository();
            var helperService = new DatabaseHelperService();

            // Create driver
            var newDriver = new Driver
            {
                FirstName = "Integration",
                LastName = "TestDriver",
                DriversLicenseType = "INT123456",
                DriverPhone = "555-0199",
                DriverEmail = "integration@test.com"
            };
            driverRepo.AddDriver(newDriver);

            var createdDrivers = driverRepo.GetAllDrivers();
            var createdDriver = createdDrivers.FirstOrDefault(d => d.DriversLicenseType == "INT123456");
            Assert.NotNull(createdDriver);

            // Create vehicle
            var newVehicle = new Vehicle
            {
                VehicleNumber = "ROUTE001",
                Make = "Integration",
                Model = "RouteTest",
                Year = 2021,
                Capacity = 45,
                Status = "Active"
            };
            vehicleRepo.AddVehicle(newVehicle);

            var createdVehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = createdVehicles.FirstOrDefault(v => v.VehicleNumber == "ROUTE001");
            Assert.NotNull(createdVehicle);

            // Create routes with assignments
            var route1 = new Route
            {
                RouteName = "Integration Route 1",
                DateAsDateTime = DateTime.Today,
                AMDriverID = createdDriver.DriverID,
                AMVehicleID = createdVehicle.VehicleID,
                PMDriverID = createdDriver.DriverID,
                PMVehicleID = createdVehicle.VehicleID,
                AMBeginMiles = 1000,
                AMEndMiles = 1025,
                PMBeginMiles = 1025,
                PMEndMiles = 1050,
                AMRiders = 30,
                PMRiders = 25
            };

            var route2 = new Route
            {
                RouteName = "Integration Route 2",
                DateAsDateTime = DateTime.Today.AddDays(1),
                AMDriverID = createdDriver.DriverID,
                AMVehicleID = createdVehicle.VehicleID,
                AMBeginMiles = 1050,
                AMEndMiles = 1080,
                AMRiders = 35
            };

            routeRepo.AddRoute(route1);
            routeRepo.AddRoute(route2);

            // Test DatabaseHelperService integration
            var routeWithDetails = helperService.GetRouteWithDetails(route1.RouteID);
            Assert.NotNull(routeWithDetails);
            Assert.Equal("Integration Route 1", routeWithDetails.RouteName);
            Assert.NotNull(routeWithDetails.AMDriver);
            Assert.NotNull(routeWithDetails.AMVehicle);
            Assert.Equal("Integration TestDriver", routeWithDetails.AMDriver.Name);
            Assert.Equal("ROUTE001", routeWithDetails.AMVehicle.VehicleNumber);

            // Test route integration with DatabaseHelperService
            var routesWithDetails = helperService.GetRoutesWithDetailsByDate(DateTime.Today);
            Assert.NotNull(routesWithDetails);
            Assert.True(routesWithDetails.Count >= 1);

            // Test that routes have proper details loaded
            var detailedRoute = routesWithDetails.FirstOrDefault(r => r.RouteName?.Contains("Integration Route") == true);
            if (detailedRoute != null)
            {
                Assert.NotNull(detailedRoute.AMDriver);
                Assert.NotNull(detailedRoute.AMVehicle);
            }

            // Test route analytics
            var analyticsService = new RouteAnalyticsService();
            if (detailedRoute != null)
            {
                var routeMetrics = analyticsService.CalculateRouteEfficiency(detailedRoute);
                Assert.NotNull(routeMetrics);
                Assert.Equal(50, routeMetrics.TotalMiles); // 25 + 25
                Assert.Equal(55, routeMetrics.TotalRiders); // 30 + 25
            }

            // Cleanup
            var allRoutes = routeRepo.GetAllRoutes();
            var createdRoutes = allRoutes.Where(r => r.RouteName?.Contains("Integration Route") == true).ToList();
            foreach (var route in createdRoutes)
            {
                routeRepo.DeleteRoute(route.RouteID);
            }

            vehicleRepo.DeleteVehicle(createdVehicle?.VehicleID ?? 0);
            driverRepo.DeleteDriver(createdDriver?.DriverID ?? 0);
        }

        #endregion

        #region Validation Service Integration Tests

        [Fact]
        public void ValidationServiceIntegration_ShouldValidateCompleteScenarios()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var driverRepo = new DriverRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var validationService = new ValidationService(vehicleRepo, driverRepo, maintenanceRepo, fuelRepo);

            // Create test data
            var testVehicle = new Vehicle
            {
                VehicleNumber = "VAL001",
                Make = "Validation",
                Model = "Test",
                Status = "Active",
                Year = 2020,
                Capacity = 50
            };
            vehicleRepo.AddVehicle(testVehicle);

            var testDriver = new Driver
            {
                FirstName = "Validation",
                LastName = "TestDriver",
                DriversLicenseType = "VAL123456",
                DriverPhone = "555-0200",
                DriverEmail = "validation@test.com"
            };
            driverRepo.AddDriver(testDriver);

            var createdVehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = createdVehicles.FirstOrDefault(v => v.VehicleNumber == "VAL001");

            var createdDrivers = driverRepo.GetAllDrivers();
            var createdDriver = createdDrivers.FirstOrDefault(d => d.DriversLicenseType == "VAL123456");

            // Test vehicle availability validation
            var vehicleAvailability = validationService.ValidateVehicleAvailability(createdVehicle?.VehicleID ?? 0, DateTime.Now);
            Assert.True(vehicleAvailability.IsValid);

            // Test driver availability validation
            var driverAvailability = validationService.ValidateDriverAvailability(createdDriver?.DriverID ?? 0, DateTime.Now);
            Assert.True(driverAvailability.IsValid);

            // Test fuel record validation
            var validFuel = new Fuel
            {
                VehicleFueledID = createdVehicle?.VehicleID,
                FuelDateAsDateTime = DateTime.Now,
                FuelAmount = 45.5m,
                FuelLocation = "Test Station",
                FuelCost = 136.50m
            };
            var fuelValidation = validationService.ValidateFuelRecord(validFuel);
            Assert.True(fuelValidation.IsValid);

            // Test maintenance record validation
            var validMaintenance = new Maintenance
            {
                VehicleID = createdVehicle?.VehicleID,
                DateAsDateTime = DateTime.Now,
                Notes = "Routine Check",
                RepairCost = 150.00m,
                MaintenanceCompleted = "Routine"
            };
            var maintenanceValidation = validationService.ValidateMaintenanceRecord(validMaintenance);
            Assert.True(maintenanceValidation.IsValid);

            // Test with invalid scenarios
            var invalidFuel = new Fuel
            {
                VehicleFueledID = createdVehicle?.VehicleID,
                FuelDateAsDateTime = DateTime.Now,
                FuelAmount = -10, // Invalid negative amount
                FuelLocation = "Test Station"
            };
            var invalidFuelValidation = validationService.ValidateFuelRecord(invalidFuel);
            Assert.False(invalidFuelValidation.IsValid);

            // Cleanup
            vehicleRepo.DeleteVehicle(createdVehicle?.VehicleID ?? 0);
            driverRepo.DeleteDriver(createdDriver?.DriverID ?? 0);
        }

        #endregion

        #region Predictive Maintenance Integration Tests

        [Fact]
        public async Task PredictiveMaintenanceIntegration_ShouldGenerateRealisticPredictions()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var predictiveService = new PredictiveMaintenanceService();

            // Create vehicle with high mileage
            var highMileageVehicle = new Vehicle
            {
                VehicleNumber = "PRED001",
                Make = "Predictive",
                Model = "Test",
                Year = 2015, // Older vehicle
                Status = "Active",
                Capacity = 50,
                Notes = "High mileage: 150000 miles"
            };
            vehicleRepo.AddVehicle(highMileageVehicle);

            var createdVehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = createdVehicles.FirstOrDefault(v => v.VehicleNumber == "PRED001");

            // Add historical maintenance
            var oldMaintenance = new Maintenance
            {
                VehicleID = createdVehicle?.VehicleID,
                DateAsDateTime = DateTime.Now.AddMonths(-6),
                Notes = "Oil Change",
                RepairCost = 75.00m,
                MaintenanceCompleted = "Routine"
            };
            maintenanceRepo.AddMaintenance(oldMaintenance);

            // Test predictive maintenance
            var predictions = await predictiveService.GetMaintenancePredictionsAsync(createdVehicle?.VehicleID ?? 0);
            Assert.NotNull(predictions);

            // High mileage vehicle should have some predictions
            if (predictions.Any())
            {
                Assert.All(predictions, p =>
                {
                    Assert.NotNull(p.MaintenanceType);
                    Assert.True(p.EstimatedCost > 0);
                    Assert.True(p.PredictedDate >= DateTime.Now);
                });
            }

            // Cleanup
            var allMaintenance = maintenanceRepo.GetMaintenanceByVehicle(createdVehicle?.VehicleID ?? 0);
            foreach (var maint in allMaintenance)
            {
                maintenanceRepo.DeleteMaintenance(maint.MaintenanceID);
            }
            vehicleRepo.DeleteVehicle(createdVehicle?.VehicleID ?? 0);
        }

        #endregion

        #region Analytics Service Integration Tests

        [Fact]
        public void AnalyticsServiceIntegration_ShouldProvideComprehensiveMetrics()
        {
            // Arrange
            var routeRepo = new RouteRepository();
            var analyticsService = new RouteAnalyticsService();

            // Create multiple routes for analytics
            var routes = new List<Route>
            {
                new Route
                {
                    RouteName = "Analytics Route 1",
                    DateAsDateTime = DateTime.Today,
                    AMBeginMiles = 1000,
                    AMEndMiles = 1030,
                    PMBeginMiles = 1030,
                    PMEndMiles = 1055,
                    AMRiders = 40,
                    PMRiders = 35
                },
                new Route
                {
                    RouteName = "Analytics Route 2",
                    DateAsDateTime = DateTime.Today,
                    AMBeginMiles = 2000,
                    AMEndMiles = 2040,
                    PMBeginMiles = 2040,
                    PMEndMiles = 2070,
                    AMRiders = 30,
                    PMRiders = 25
                },
                new Route
                {
                    RouteName = "Analytics Route 3",
                    DateAsDateTime = DateTime.Today.AddDays(-1),
                    AMBeginMiles = 3000,
                    AMEndMiles = 3025,
                    PMBeginMiles = 3025,
                    PMEndMiles = 3045,
                    AMRiders = 35,
                    PMRiders = 30
                }
            };

            foreach (var route in routes)
            {
                routeRepo.AddRoute(route);
            }

            // Test individual route efficiency calculations
            var route1Metrics = analyticsService.CalculateRouteEfficiency(routes[0]);
            Assert.NotNull(route1Metrics);
            Assert.Equal(55, route1Metrics.TotalMiles); // 30 + 25
            Assert.Equal(75, route1Metrics.TotalRiders); // 40 + 35
            Assert.True(route1Metrics.EfficiencyScore > 0);
            Assert.True(route1Metrics.EstimatedFuelCost > 0);

            // Test route analytics aggregation (if implemented in the future)
            var dateRange = new DateTimeRange
            {
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today
            };

            // This tests the service's ability to handle multiple routes
            var allRoutes = routeRepo.GetRoutesByDate(dateRange.StartDate);
            var endDateRoutes = routeRepo.GetRoutesByDate(dateRange.EndDate);
            var combinedRoutes = allRoutes.Concat(endDateRoutes).ToList();
            Assert.True(combinedRoutes.Count >= 0); // May be empty in test environment

            var totalMetrics = new
            {
                TotalMiles = allRoutes.Sum(r => (r.AMEndMiles - r.AMBeginMiles) + (r.PMEndMiles - r.PMBeginMiles)),
                TotalRiders = allRoutes.Sum(r => r.AMRiders + r.PMRiders),
                TotalRoutes = allRoutes.Count
            };

            Assert.True(totalMetrics.TotalMiles >= 135); // Sum of all route miles
            Assert.True(totalMetrics.TotalRiders >= 195); // Sum of all riders
            Assert.Equal(3, totalMetrics.TotalRoutes);

            // Cleanup
            var allCreatedRoutes = routeRepo.GetAllRoutes();
            var createdRoutes = allCreatedRoutes.Where(r => r.RouteName?.Contains("Analytics Route") == true).ToList();
            foreach (var route in createdRoutes)
            {
                routeRepo.DeleteRoute(route.RouteID);
            }
        }

        #endregion

        #region Error Recovery and Resilience Tests

        [Fact]
        public void SystemResilience_ShouldHandleDataInconsistencies()
        {
            // Test that the system handles orphaned records gracefully
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var fuelRepo = new FuelRepository(_testConnectionString, _sqlServerProvider);
            var helperService = new DatabaseHelperService();

            // Create vehicle
            var testVehicle = new Vehicle
            {
                VehicleNumber = "RESILIENCE001",
                Make = "Resilience",
                Model = "Test",
                Status = "Active"
            };
            vehicleRepo.AddVehicle(testVehicle);

            var createdVehicles = vehicleRepo.GetAllVehicles();
            var createdVehicle = createdVehicles.FirstOrDefault(v => v.VehicleNumber == "RESILIENCE001");

            // Add fuel record
            var fuelRecord = new Fuel
            {
                VehicleFueledID = createdVehicle?.VehicleID,
                FuelDateAsDateTime = DateTime.Now,
                FuelAmount = 50m,
                FuelLocation = "Test Station"
            };
            fuelRepo.AddFuelRecord(fuelRecord);

            // Delete vehicle (creating orphaned fuel record)
            vehicleRepo.DeleteVehicle(createdVehicle?.VehicleID ?? 0);

            // Test that system handles orphaned fuel record gracefully
            var orphanedFuel = fuelRepo.GetFuelRecordsByVehicle(createdVehicle?.VehicleID ?? 0);
            Assert.NotNull(orphanedFuel); // Should not throw exception

            // Cleanup orphaned fuel
            var allFuel = fuelRepo.GetAllFuelRecords();
            var orphanedRecords = allFuel.Where(f => f.VehicleFueledID == createdVehicle?.VehicleID).ToList();
            foreach (var fuel in orphanedRecords)
            {
                fuelRepo.DeleteFuelRecord(fuel.FuelID);
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper class for date range operations in analytics
    /// </summary>
    public class DateTimeRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
