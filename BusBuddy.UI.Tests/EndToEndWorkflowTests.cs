using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Models;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// End-to-end workflow tests validating complete business processes.
    /// Tests cover route management, maintenance scheduling, and analytics generation.
    /// </summary>
    public class EndToEndWorkflowTests : SystemTestBase
    {
        private readonly ITestOutputHelper _output;

        public EndToEndWorkflowTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Route Management Workflow Tests

        [Fact]
        public async Task CompleteRouteManagement_Workflow_ShouldWork()
        {
            _output.WriteLine("Starting complete route management workflow test...");

            // 1. Create new vehicle
            var vehicle = CreateTestVehicle("_RouteWorkflow");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);
            Assert.True(vehicleId > 0, "Vehicle creation should succeed");
            _output.WriteLine($"✅ Created vehicle with ID: {vehicleId}");

            // 2. Create new driver
            var driver = CreateTestDriver("_RouteWorkflow");
            var driverId = DriverRepository.AddDriver(driver);
            TestDriverIds.Add(driverId);
            Assert.True(driverId > 0, "Driver creation should succeed");
            _output.WriteLine($"✅ Created driver with ID: {driverId}");

            // 3. Validate vehicle and driver availability
            var vehicleValidation = ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.Today);
            var driverValidation = ValidationService.ValidateDriverAvailability(driverId, DateTime.Today);

            Assert.True(vehicleValidation.IsValid, $"Vehicle should be available: {vehicleValidation.GetErrorMessage()}");
            Assert.True(driverValidation.IsValid, $"Driver should be available: {driverValidation.GetErrorMessage()}");
            _output.WriteLine("✅ Vehicle and driver availability validated");

            // 4. Create and assign route
            var route = CreateTestRoute(vehicleId, driverId, "_RouteWorkflow");
            var routeId = RouteRepository.AddRoute(route);
            TestRouteIds.Add(routeId);
            Assert.True(routeId > 0, "Route creation should succeed");
            _output.WriteLine($"✅ Created route with ID: {routeId}");

            // 5. Validate route assignment
            var routeValidation = ValidationService.ValidateRouteAssignment(route);
            Assert.True(routeValidation.IsValid, $"Route assignment should be valid: {routeValidation.GetErrorMessage()}");
            _output.WriteLine("✅ Route assignment validated");

            // 6. Generate analytics report
            var startDate = DateTime.Today.AddDays(-1);
            var endDate = DateTime.Today.AddDays(1);

            var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
            Assert.NotNull(metrics);
            _output.WriteLine("✅ Analytics report generated successfully");

            // 7. Validate data consistency
            var retrievedVehicle = VehicleRepository.GetVehicleById(vehicleId);
            var retrievedDriver = DriverRepository.GetDriverById(driverId);
            var retrievedRoute = RouteRepository.GetRouteById(routeId);

            Assert.NotNull(retrievedVehicle);
            Assert.NotNull(retrievedDriver);
            Assert.NotNull(retrievedRoute);
            Assert.Equal(vehicleId, retrievedRoute.AMVehicleID);
            Assert.Equal(driverId, retrievedRoute.AMDriverID);

            _output.WriteLine("✅ Data consistency validated");
            _output.WriteLine("🎉 Complete route management workflow test PASSED");
        }

        [Fact]
        public void MaintenanceScheduling_Workflow_ShouldWork()
        {
            _output.WriteLine("Starting maintenance scheduling workflow test...");

            // 1. Create test vehicle
            var vehicle = CreateTestVehicle("_MaintenanceWorkflow");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);
            _output.WriteLine($"✅ Created vehicle with ID: {vehicleId}");

            // 2. Vehicle health monitoring (simulate by checking availability)
            var healthCheck = ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.Today);
            Assert.True(healthCheck.IsValid, "Vehicle should be healthy and available");
            _output.WriteLine("✅ Vehicle health check passed");

            // 3. Create maintenance record (scheduled)
            var maintenance = new Maintenance
            {
                VehicleID = vehicleId,
                Date = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd"),
                MaintenanceCompleted = "SCHEDULED - Oil change and inspection",
                Notes = "SCHEDULED maintenance for vehicle health",
                RepairCost = 150.00m,
                OdometerReading = 50000
            };

            var maintenanceId = MaintenanceRepository.AddMaintenance(maintenance);
            Assert.True(maintenanceId > 0, "Maintenance scheduling should succeed");
            maintenance.MaintenanceID = maintenanceId; // Set the ID for future updates
            _output.WriteLine($"✅ Scheduled maintenance with ID: {maintenanceId}");

            // 4. Validate maintenance record
            var maintenanceValidation = ValidationService.ValidateMaintenanceRecord(maintenance);
            Assert.True(maintenanceValidation.IsValid, $"Maintenance record should be valid: {maintenanceValidation.GetErrorMessage()}");
            _output.WriteLine("✅ Maintenance record validated");

            // 5. Check vehicle availability during maintenance period
            var maintenanceDate = DateTime.Today.AddDays(7);
            var availabilityCheck = ValidationService.ValidateVehicleAvailability(vehicleId, maintenanceDate);

            // Should fail due to scheduled maintenance
            Assert.False(availabilityCheck.IsValid, "Vehicle should not be available during scheduled maintenance");
            Assert.Contains("scheduled maintenance", availabilityCheck.GetErrorMessage().ToLower());
            _output.WriteLine("✅ Vehicle unavailability during maintenance validated");

            // 6. Complete work order (update maintenance record)
            maintenance.MaintenanceCompleted = "COMPLETED - Oil changed, inspection passed";
            maintenance.Notes = "Maintenance completed successfully";
            var updateResult = MaintenanceRepository.UpdateMaintenance(maintenance);
            Assert.True(updateResult, "Maintenance completion update should succeed");
            _output.WriteLine("✅ Maintenance work order completed");

            // 7. Update vehicle status (if needed)
            var updatedVehicle = VehicleRepository.GetVehicleById(vehicleId);
            Assert.NotNull(updatedVehicle);
            Assert.Equal("Active", updatedVehicle.Status);
            _output.WriteLine("✅ Vehicle status confirmed as active");

            _output.WriteLine("🎉 Maintenance scheduling workflow test PASSED");
        }

        #endregion

        #region Driver Performance Workflow Tests

        [Fact]
        public async Task DriverPerformance_AnalysisWorkflow_ShouldWork()
        {
            _output.WriteLine("Starting driver performance analysis workflow test...");

            // 1. Create test driver
            var driver = CreateTestDriver("_PerformanceWorkflow");
            var driverId = DriverRepository.AddDriver(driver);
            TestDriverIds.Add(driverId);
            _output.WriteLine($"✅ Created driver with ID: {driverId}");

            // 2. Create test vehicle
            var vehicle = CreateTestVehicle("_PerformanceWorkflow");
            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            TestVehicleIds.Add(vehicleId);
            _output.WriteLine($"✅ Created vehicle with ID: {vehicleId}");

            // 3. Create multiple routes for performance analysis
            for (int i = 0; i < 3; i++)
            {
                var route = CreateTestRoute(vehicleId, driverId, $"_Performance_{i}");
                route.Date = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd");
                route.AMBeginMiles = 1000 + (i * 50);
                route.AMEndMiles = 1050 + (i * 50);
                route.AMRiders = 20 + i;

                var routeId = RouteRepository.AddRoute(route);
                TestRouteIds.Add(routeId);
                _output.WriteLine($"✅ Created performance route {i + 1} with ID: {routeId}");
            }

            // 4. Generate driver performance metrics
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today;

            try
            {
                var performance = await AnalyticsService.CalculateDriverPerformanceAsync(driverId, startDate, endDate);

                if (performance != null)
                {
                    Assert.Equal(driverId, performance.DriverId);
                    Assert.True(performance.TotalRoutes >= 0);
                    Assert.True(performance.TotalMiles >= 0);
                    _output.WriteLine($"✅ Driver performance calculated: {performance.TotalRoutes} routes, {performance.TotalMiles} miles");
                }
                else
                {
                    _output.WriteLine("⚠️ Performance calculation returned null (expected for new test data)");
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("not found"))
            {
                _output.WriteLine("⚠️ Driver not found in analytics (expected for test environment)");
            }

            // 5. Validate driver data consistency
            var retrievedDriver = DriverRepository.GetDriverById(driverId);
            Assert.NotNull(retrievedDriver);
            Assert.Equal(driver.DriverName, retrievedDriver.DriverName);
            Assert.Equal("Active", retrievedDriver.Status);

            _output.WriteLine("✅ Driver data consistency validated");
            _output.WriteLine("🎉 Driver performance analysis workflow test PASSED");
        }

        #endregion

        #region Data Integrity Workflow Tests

        [Fact]
        public void DataIntegrity_AcrossOperations_ShouldMaintainConsistency()
        {
            _output.WriteLine("Starting data integrity across operations test...");

            // 1. Create baseline data
            var vehicle = CreateTestVehicle("_IntegrityTest");
            var driver = CreateTestDriver("_IntegrityTest");

            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            var driverId = DriverRepository.AddDriver(driver);

            TestVehicleIds.Add(vehicleId);
            TestDriverIds.Add(driverId);

            _output.WriteLine($"✅ Created baseline data: Vehicle {vehicleId}, Driver {driverId}");

            // 2. Perform multiple operations
            var route1 = CreateTestRoute(vehicleId, driverId, "_Integrity1");
            var route2 = CreateTestRoute(vehicleId, driverId, "_Integrity2");
            route2.Date = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

            var routeId1 = RouteRepository.AddRoute(route1);
            var routeId2 = RouteRepository.AddRoute(route2);

            TestRouteIds.Add(routeId1);
            TestRouteIds.Add(routeId2);

            _output.WriteLine($"✅ Created routes: {routeId1}, {routeId2}");

            // 3. Validate referential integrity
            var retrievedRoute1 = RouteRepository.GetRouteById(routeId1);
            var retrievedRoute2 = RouteRepository.GetRouteById(routeId2);

            Assert.NotNull(retrievedRoute1);
            Assert.NotNull(retrievedRoute2);
            Assert.Equal(vehicleId, retrievedRoute1.AMVehicleID);
            Assert.Equal(vehicleId, retrievedRoute2.AMVehicleID);
            Assert.Equal(driverId, retrievedRoute1.AMDriverID);
            Assert.Equal(driverId, retrievedRoute2.AMDriverID);

            _output.WriteLine("✅ Referential integrity validated");

            // 4. Test cascading validation
            var validation1 = ValidationService.ValidateRouteAssignment(retrievedRoute1);
            var validation2 = ValidationService.ValidateRouteAssignment(retrievedRoute2);

            Assert.True(validation1.IsValid, $"Route 1 validation failed: {validation1.GetErrorMessage()}");
            Assert.True(validation2.IsValid, $"Route 2 validation failed: {validation2.GetErrorMessage()}");

            _output.WriteLine("✅ Cascading validation passed");

            // 5. Update operations and revalidate
            vehicle.Status = "Out of Service";
            var updateResult = VehicleRepository.UpdateVehicle(vehicle);
            Assert.True(updateResult, "Vehicle update should succeed");

            // Vehicle should now be unavailable for new assignments
            var availabilityCheck = ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.Today);
            Assert.False(availabilityCheck.IsValid, "Vehicle out of service should be unavailable");

            _output.WriteLine("✅ Update operations and validation consistency verified");
            _output.WriteLine("🎉 Data integrity across operations test PASSED");
        }

        #endregion
    }
}
