using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Models;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Performance tests validating system performance under various conditions.
    /// Tests cover dashboard loading, analytics calculations, and memory usage.
    /// </summary>
    public class PerformanceTests : SystemTestBase
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Dashboard Performance Tests

        [Fact]
        public async Task Dashboard_LoadTime_ShouldBeFast()
        {
            _output.WriteLine("Testing dashboard load time performance...");

            // Create test data for realistic load testing
            CreateTestDataSet(25); // Create 25 vehicles for testing

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();
            _output.WriteLine($"Starting memory: {PerformanceTestHelpers.BytesToMB(beforeMemory):F2}MB");

            // Measure dashboard-like operations
            await PerformanceTestHelpers.AssertPerformanceAsync(async () =>
            {
                // Simulate dashboard loading operations
                var vehicles = VehicleRepository.GetAllVehicles();
                var drivers = DriverRepository.GetAllDrivers();

                // Simulate analytics loading
                var startDate = DateTime.Today.AddDays(-30);
                var endDate = DateTime.Today;
                var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);

                _output.WriteLine($"Loaded {vehicles?.Count ?? 0} vehicles, {drivers?.Count ?? 0} drivers");

            }, TimeSpan.FromSeconds(5), "Dashboard loading");

            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = afterMemory - beforeMemory;

            // Should use less than 50MB for this operation
            Assert.True(PerformanceTestHelpers.BytesToMB(memoryIncrease) < 50,
                $"Memory usage too high: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            var report = PerformanceTestHelpers.CreatePerformanceReport(
                "Dashboard Loading",
                TimeSpan.FromMilliseconds(100), // Placeholder
                memoryIncrease);

            _output.WriteLine(report);
            _output.WriteLine("✅ Dashboard load time test PASSED");
        }

        [Fact]
        public async Task Analytics_Calculation_Performance()
        {
            _output.WriteLine("Testing analytics calculation performance...");

            // Create larger dataset for performance testing
            CreateTestDataSet(50);
            CreateTestRoutes(100); // Create 100 routes

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();

            // Test route efficiency calculations
            await PerformanceTestHelpers.AssertPerformanceAsync(async () =>
            {
                var startDate = DateTime.Today.AddDays(-90); // 3 months of data
                var endDate = DateTime.Today;

                var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
                var fleetSummary = await AnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);
                var costMetrics = await AnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);

                _output.WriteLine("✅ Completed analytics calculations");

            }, TimeSpan.FromSeconds(10), "Analytics calculations for 3 months");

            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = afterMemory - beforeMemory;

            _output.WriteLine($"Memory used for analytics: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            // Analytics should complete within reasonable time and memory
            Assert.True(PerformanceTestHelpers.BytesToMB(memoryIncrease) < 100,
                "Analytics calculations using too much memory");

            _output.WriteLine("✅ Analytics calculation performance test PASSED");
        }

        #endregion

        #region Data Repository Performance Tests

        [Fact]
        public void Repository_BulkOperations_Performance()
        {
            _output.WriteLine("Testing repository bulk operations performance...");

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();

            // Test bulk vehicle operations
            PerformanceTestHelpers.AssertPerformance(() =>
            {
                // Create multiple vehicles quickly
                for (int i = 0; i < 20; i++)
                {
                    var vehicle = CreateTestVehicle($"_Bulk_{i}");
                    var vehicleId = VehicleRepository.AddVehicle(vehicle);
                    TestVehicleIds.Add(vehicleId);
                }

                // Retrieve all vehicles
                var allVehicles = VehicleRepository.GetAllVehicles();
                _output.WriteLine($"Created and retrieved {TestVehicleIds.Count} vehicles");

            }, TimeSpan.FromSeconds(5), "Bulk repository operations");

            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = afterMemory - beforeMemory;

            _output.WriteLine($"Memory used for bulk operations: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");
            _output.WriteLine("✅ Repository bulk operations performance test PASSED");
        }

        [Fact]
        public void ValidationService_PerformanceUnderLoad()
        {
            _output.WriteLine("Testing validation service performance under load...");

            // Create test data
            var vehicle = CreateTestVehicle("_ValidationPerf");
            var driver = CreateTestDriver("_ValidationPerf");

            var vehicleId = VehicleRepository.AddVehicle(vehicle);
            var driverId = DriverRepository.AddDriver(driver);

            TestVehicleIds.Add(vehicleId);
            TestDriverIds.Add(driverId);

            // Test validation performance
            PerformanceTestHelpers.AssertPerformance(() =>
            {
                // Perform many validation operations
                for (int i = 0; i < 100; i++)
                {
                    ValidationService.ValidateVehicleAvailability(vehicleId, DateTime.Today.AddDays(i % 30));
                    ValidationService.ValidateDriverAvailability(driverId, DateTime.Today.AddDays(i % 30));

                    if (i % 10 == 0)
                    {
                        var testFuel = new Fuel
                        {
                            VehicleFueledID = vehicleId,
                            FuelDate = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd"),
                            FuelLocation = "Test Station",
                            FuelAmount = 50.0m,
                            FuelCost = 150.0m
                        };
                        ValidationService.ValidateFuelRecord(testFuel);
                    }
                }

                _output.WriteLine("Completed 100 validation operations");

            }, TimeSpan.FromSeconds(3), "100 validation operations");

            _output.WriteLine("✅ Validation service performance test PASSED");
        }

        #endregion

        #region Memory Usage Tests

        [Fact]
        public void Memory_Usage_ShouldStayWithinLimits()
        {
            _output.WriteLine("Testing memory usage limits...");

            var initialMemory = PerformanceTestHelpers.GetMemoryUsage();
            _output.WriteLine($"Initial memory: {PerformanceTestHelpers.BytesToMB(initialMemory):F2}MB");

            // Test memory usage with moderate data operations
            PerformanceTestHelpers.AssertMemoryUsage(() =>
            {
                // Create temporary data
                var tempVehicleIds = new System.Collections.Generic.List<int>();

                for (int i = 0; i < 30; i++)
                {
                    var vehicle = CreateTestVehicle($"_MemTest_{i}");
                    var vehicleId = VehicleRepository.AddVehicle(vehicle);
                    tempVehicleIds.Add(vehicleId);
                }

                // Retrieve and process data
                var vehicles = VehicleRepository.GetAllVehicles();

                // Cleanup temporary data
                foreach (var id in tempVehicleIds)
                {
                    try { VehicleRepository.DeleteVehicle(id); } catch { }
                }

                _output.WriteLine($"Processed {tempVehicleIds.Count} temporary vehicles");

            }, 30, "Moderate data operations"); // 30MB limit

            var finalMemory = PerformanceTestHelpers.GetMemoryUsage();
            _output.WriteLine($"Final memory: {PerformanceTestHelpers.BytesToMB(finalMemory):F2}MB");

            _output.WriteLine("✅ Memory usage test PASSED");
        }

        #endregion

        #region Helper Methods

        private void CreateTestDataSet(int vehicleCount)
        {
            _output.WriteLine($"Creating test dataset with {vehicleCount} vehicles...");

            for (int i = 0; i < vehicleCount; i++)
            {
                var vehicle = CreateTestVehicle($"_PerfTest_{i}");
                var driver = CreateTestDriver($"_PerfTest_{i}");

                var vehicleId = VehicleRepository.AddVehicle(vehicle);
                var driverId = DriverRepository.AddDriver(driver);

                TestVehicleIds.Add(vehicleId);
                TestDriverIds.Add(driverId);

                if (i % 10 == 0)
                {
                    _output.WriteLine($"Created {i + 1}/{vehicleCount} test records...");
                }
            }

            _output.WriteLine($"✅ Test dataset created: {vehicleCount} vehicles and drivers");
        }

        private void CreateTestRoutes(int routeCount)
        {
            _output.WriteLine($"Creating {routeCount} test routes...");

            // Use existing test vehicles and drivers
            var vehicleIndex = 0;
            var driverIndex = 0;

            for (int i = 0; i < routeCount && vehicleIndex < TestVehicleIds.Count && driverIndex < TestDriverIds.Count; i++)
            {
                var route = CreateTestRoute(TestVehicleIds[vehicleIndex], TestDriverIds[driverIndex], $"_PerfRoute_{i}");
                route.Date = DateTime.Today.AddDays(-(i % 90)).ToString("yyyy-MM-dd"); // Spread over 90 days

                var routeId = RouteRepository.AddRoute(route);
                TestRouteIds.Add(routeId);

                // Cycle through available vehicles and drivers
                vehicleIndex = (vehicleIndex + 1) % TestVehicleIds.Count;
                driverIndex = (driverIndex + 1) % TestDriverIds.Count;
            }

            _output.WriteLine($"✅ Created {TestRouteIds.Count} test routes");
        }

        #endregion

        #region Advanced Performance Tests

        [Fact]
        public async Task LargeDataset_Dashboard_ShouldLoadEfficiently()
        {
            _output.WriteLine("Testing dashboard performance with large dataset...");

            // Create large realistic dataset
            var vehicles = TestDataFactory.CreateVehicleFleet(200, "PERF");
            var drivers = TestDataFactory.CreateDriverCohort(150, "PERF");

            var vehicleIds = new List<int>();
            var driverIds = new List<int>();

            // Add vehicles in batches
            foreach (var vehicle in vehicles.Take(200))
            {
                var vehicleId = VehicleRepository.AddVehicle(vehicle);
                vehicleIds.Add(vehicleId);
                TestVehicleIds.Add(vehicleId);
            }

            // Add drivers in batches
            foreach (var driver in drivers.Take(150))
            {
                var driverId = DriverRepository.AddDriver(driver);
                driverIds.Add(driverId);
                TestDriverIds.Add(driverId);
            }

            // Create historical routes
            var routeHistory = TestDataFactory.CreateRouteHistory(
                vehicleIds.Take(50).ToList(),
                driverIds.Take(50).ToList(),
                DateTime.Today.AddDays(-30),
                DateTime.Today.AddDays(-1));

            foreach (var route in routeHistory)
            {
                var routeId = RouteRepository.AddRoute(route);
                TestRouteIds.Add(routeId);
            }

            _output.WriteLine($"Created large dataset: {vehicleIds.Count} vehicles, {driverIds.Count} drivers, {TestRouteIds.Count} routes");

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();

            // Test dashboard-like operations with large dataset
            await PerformanceTestHelpers.AssertPerformanceAsync(async () =>
            {
                // Simulate dashboard loading with large dataset
                var allVehicles = VehicleRepository.GetAllVehicles();
                var allDrivers = DriverRepository.GetAllDrivers();
                var recentRoutes = RouteRepository.GetAllRoutes();

                // Simulate analytics calculations
                var startDate = DateTime.Today.AddDays(-30);
                var endDate = DateTime.Today;
                var metrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
                var fleetSummary = await AnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);

                _output.WriteLine($"Dashboard loaded: {allVehicles?.Count ?? 0} vehicles, {allDrivers?.Count ?? 0} drivers, {recentRoutes?.Count ?? 0} routes");

            }, TimeSpan.FromSeconds(10), "Large dataset dashboard loading");

            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = afterMemory - beforeMemory;

            _output.WriteLine($"Memory used for large dataset: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            // Should handle large dataset efficiently
            Assert.True(PerformanceTestHelpers.BytesToMB(memoryIncrease) < 150,
                $"Large dataset dashboard should use less than 150MB: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            _output.WriteLine("✅ Large dataset dashboard performance test PASSED");
        }

        [Fact]
        public async Task ComplexAnalytics_Performance_ShouldScaleWell()
        {
            _output.WriteLine("Testing complex analytics performance scaling...");

            // Create comprehensive test data for analytics
            CreateAnalyticsTestDataSet(100, 60); // 100 vehicles, 60 days

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();

            // Test multiple complex analytics operations
            await PerformanceTestHelpers.AssertPerformanceAsync(async () =>
            {
                var startDate = DateTime.Today.AddDays(-60);
                var endDate = DateTime.Today;

                // Parallel analytics calculations
                var task1 = AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
                var task2 = AnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);
                var task3 = AnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);

                await Task.WhenAll(task1, task2, task3);

                _output.WriteLine("✅ Completed parallel analytics calculations");

            }, TimeSpan.FromSeconds(20), "Complex parallel analytics for 60 days");

            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = afterMemory - beforeMemory;

            _output.WriteLine($"Memory used for complex analytics: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            Assert.True(PerformanceTestHelpers.BytesToMB(memoryIncrease) < 200,
                "Complex analytics should use less than 200MB");

            _output.WriteLine("✅ Complex analytics performance scaling test PASSED");
        }

        #endregion

        #region Enhanced Helper Methods

        private void CreateAnalyticsTestDataSet(int vehicleCount, int daysOfData)
        {
            _output.WriteLine($"Creating analytics dataset: {vehicleCount} vehicles, {daysOfData} days...");

            // Create vehicles with history
            var vehicles = TestDataFactory.CreateVehicleFleet(vehicleCount, "ANALYTICS");
            var drivers = TestDataFactory.CreateDriverCohort(vehicleCount, "ANALYTICS");

            var vehicleIds = new List<int>();
            var driverIds = new List<int>();

            // Batch create vehicles and drivers
            for (int i = 0; i < Math.Min(vehicles.Count, drivers.Count); i++)
            {
                var vehicleId = VehicleRepository.AddVehicle(vehicles[i]);
                var driverId = DriverRepository.AddDriver(drivers[i]);

                vehicleIds.Add(vehicleId);
                driverIds.Add(driverId);

                TestVehicleIds.Add(vehicleId);
                TestDriverIds.Add(driverId);
            }

            // Create historical routes for analytics
            var routes = TestDataFactory.CreateRouteHistory(
                vehicleIds, driverIds,
                DateTime.Today.AddDays(-daysOfData),
                DateTime.Today.AddDays(-1));

            foreach (var route in routes)
            {
                var routeId = RouteRepository.AddRoute(route);
                TestRouteIds.Add(routeId);
            }

            // Create fuel history for cost analytics
            foreach (var vehicleId in vehicleIds.Take(20)) // Limit fuel records for performance
            {
                var fuelRecords = TestDataFactory.CreateFuelHistory(vehicleId, daysOfData / 30);
                foreach (var fuel in fuelRecords)
                {
                    FuelRepository.AddFuelRecord(fuel);
                }
            }

            _output.WriteLine($"✅ Created analytics dataset: {vehicleIds.Count} vehicles, {TestRouteIds.Count} routes");
        }

        #endregion
    }
}
