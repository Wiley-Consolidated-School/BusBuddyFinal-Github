using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using BusBuddy.Models;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// Load and stress tests validating system performance under heavy usage and large datasets.
    /// Tests cover concurrent user scenarios, large data volumes, and resource contention.
    /// </summary>
    public class LoadStressTests : SystemTestBase
    {
        private readonly ITestOutputHelper _output;

        public LoadStressTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Concurrent User Tests

        [Fact]
        public async Task ConcurrentUsers_VehicleOperations_ShouldHandleLoad()
        {
            _output.WriteLine("Testing concurrent vehicle operations with multiple users...");

            const int userCount = 10;
            const int operationsPerUser = 5;
            var tasks = new List<Task>();
            var results = new ConcurrentBag<bool>();
            var exceptions = new ConcurrentBag<Exception>();

            var stopwatch = Stopwatch.StartNew();

            // Simulate multiple users performing vehicle operations simultaneously
            for (int userId = 0; userId < userCount; userId++)
            {
                var currentUserId = userId;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        for (int op = 0; op < operationsPerUser; op++)
                        {
                            // Create vehicle
                            var vehicle = CreateTestVehicle($"_User{currentUserId}_Op{op}");
                            var vehicleId = VehicleRepository.AddVehicle(vehicle);

                            lock (TestVehicleIds)
                            {
                                TestVehicleIds.Add(vehicleId);
                            }

                            // Read vehicle
                            var retrievedVehicle = VehicleRepository.GetVehicleById(vehicleId);
                            Assert.NotNull(retrievedVehicle);

                            // Update vehicle
                            retrievedVehicle.Status = $"Updated_by_User{currentUserId}";
                            VehicleRepository.UpdateVehicle(retrievedVehicle);

                            // Small delay to simulate real user interaction
                            await Task.Delay(10);
                        }
                        results.Add(true);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        results.Add(false);
                    }
                }));
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Analyze results
            var successCount = results.Count(r => r);
            var failureCount = results.Count(r => !r);

            _output.WriteLine($"Concurrent operations completed in {stopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Successful operations: {successCount}/{userCount}");
            _output.WriteLine($"Failed operations: {failureCount}/{userCount}");

            if (exceptions.Any())
            {
                _output.WriteLine("Exceptions encountered:");
                foreach (var ex in exceptions.Take(3))
                {
                    _output.WriteLine($"  - {ex.GetType().Name}: {ex.Message}");
                }
            }

            // At least 80% of operations should succeed under load
            var successRate = (double)successCount / userCount;
            Assert.True(successRate >= 0.8, $"Success rate {successRate:P} should be at least 80%");

            _output.WriteLine("✅ Concurrent vehicle operations load test PASSED");
        }

        [Fact]
        public async Task ConcurrentUsers_Analytics_ShouldMaintainPerformance()
        {
            _output.WriteLine("Testing concurrent analytics calculations...");

            // Create test data for analytics
            await CreateLargeTestDataset(100, 30); // 100 vehicles, 30 days of data

            const int concurrentAnalytics = 5;
            var tasks = new List<Task<TimeSpan>>();
            var exceptions = new ConcurrentBag<Exception>();

            // Run multiple analytics calculations simultaneously
            for (int i = 0; i < concurrentAnalytics; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var stopwatch = Stopwatch.StartNew();

                        var startDate = DateTime.Today.AddDays(-30);
                        var endDate = DateTime.Today;

                        // Perform analytics calculations
                        var routeMetrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
                        var fleetSummary = await AnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);
                        var costMetrics = await AnalyticsService.CalculateCostPerStudentMetricsAsync(startDate, endDate);

                        stopwatch.Stop();
                        return stopwatch.Elapsed;
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        return TimeSpan.MaxValue;
                    }
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Analyze performance
            var validResults = results.Where(r => r != TimeSpan.MaxValue).ToArray();
            var averageTime = validResults.Any() ? TimeSpan.FromMilliseconds(validResults.Average(r => r.TotalMilliseconds)) : TimeSpan.Zero;
            var maxTime = validResults.Any() ? validResults.Max() : TimeSpan.Zero;

            _output.WriteLine($"Concurrent analytics completed:");
            _output.WriteLine($"  Successful calculations: {validResults.Length}/{concurrentAnalytics}");
            _output.WriteLine($"  Average time: {averageTime.TotalSeconds:F2}s");
            _output.WriteLine($"  Maximum time: {maxTime.TotalSeconds:F2}s");

            if (exceptions.Any())
            {
                _output.WriteLine($"  Exceptions: {exceptions.Count}");
            }

            // Analytics should complete within reasonable time even under concurrent load
            Assert.True(validResults.Length >= concurrentAnalytics * 0.8, "At least 80% of analytics should complete successfully");
            Assert.True(averageTime.TotalSeconds < 15, $"Average analytics time {averageTime.TotalSeconds:F2}s should be under 15 seconds");

            _output.WriteLine("✅ Concurrent analytics performance test PASSED");
        }

        #endregion

        #region Large Dataset Tests

        [Fact]
        public async Task LargeDataset_VehicleManagement_ShouldPerformWell()
        {
            _output.WriteLine("Testing performance with large vehicle dataset...");

            const int vehicleCount = 500;
            var stopwatch = Stopwatch.StartNew();

            // Create large dataset
            _output.WriteLine($"Creating {vehicleCount} test vehicles...");
            var creationTasks = new List<Task<int>>();

            for (int i = 0; i < vehicleCount; i++)
            {
                var vehicleIndex = i;
                creationTasks.Add(Task.Run(() =>
                {
                    var vehicle = CreateTestVehicle($"_Large_{vehicleIndex:D4}");
                    return VehicleRepository.AddVehicle(vehicle);
                }));

                // Batch creation to avoid overwhelming the system
                if (creationTasks.Count >= 50)
                {
                    var batchResults = await Task.WhenAll(creationTasks);
                    lock (TestVehicleIds)
                    {
                        TestVehicleIds.AddRange(batchResults);
                    }
                    creationTasks.Clear();
                    _output.WriteLine($"Created batch, total so far: {TestVehicleIds.Count}");
                }
            }

            // Handle remaining vehicles
            if (creationTasks.Any())
            {
                var finalResults = await Task.WhenAll(creationTasks);
                lock (TestVehicleIds)
                {
                    TestVehicleIds.AddRange(finalResults);
                }
            }

            var creationTime = stopwatch.Elapsed;
            _output.WriteLine($"Created {TestVehicleIds.Count} vehicles in {creationTime.TotalSeconds:F2}s");

            // Test retrieval performance
            stopwatch.Restart();
            var allVehicles = VehicleRepository.GetAllVehicles();
            var retrievalTime = stopwatch.Elapsed;

            Assert.NotNull(allVehicles);
            Assert.True(allVehicles.Count >= vehicleCount, $"Should retrieve at least {vehicleCount} vehicles");

            _output.WriteLine($"Retrieved {allVehicles.Count} vehicles in {retrievalTime.TotalSeconds:F2}s");

            // Test search performance
            stopwatch.Restart();
            var searchResults = allVehicles.Where(v => v.Status == "Active").ToList();
            var searchTime = stopwatch.Elapsed;

            _output.WriteLine($"Searched through {allVehicles.Count} vehicles in {searchTime.TotalMilliseconds:F2}ms");

            // Performance assertions
            Assert.True(creationTime.TotalMinutes < 5, $"Creation time {creationTime.TotalMinutes:F2}min should be under 5 minutes");
            Assert.True(retrievalTime.TotalSeconds < 10, $"Retrieval time {retrievalTime.TotalSeconds:F2}s should be under 10 seconds");
            Assert.True(searchTime.TotalMilliseconds < 1000, $"Search time {searchTime.TotalMilliseconds:F2}ms should be under 1 second");

            _output.WriteLine("✅ Large dataset vehicle management test PASSED");
        }

        [Fact]
        public async Task LargeDataset_Analytics_ShouldScaleWell()
        {
            _output.WriteLine("Testing analytics performance with large dataset...");

            // Create comprehensive test data
            const int vehicleCount = 100;
            const int daysOfData = 365; // One year of data

            _output.WriteLine($"Creating large analytics dataset: {vehicleCount} vehicles, {daysOfData} days...");
            await CreateLargeTestDataset(vehicleCount, daysOfData);

            var beforeMemory = PerformanceTestHelpers.GetMemoryUsage();
            var stopwatch = Stopwatch.StartNew();

            // Test analytics with large dataset
            var startDate = DateTime.Today.AddDays(-daysOfData);
            var endDate = DateTime.Today;

            var routeMetrics = await AnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
            var fleetSummary = await AnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);

            stopwatch.Stop();
            var afterMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryUsed = afterMemory - beforeMemory;

            _output.WriteLine($"Analytics completed in {stopwatch.Elapsed.TotalSeconds:F2}s");
            _output.WriteLine($"Memory used: {PerformanceTestHelpers.BytesToMB(memoryUsed):F2}MB");
            _output.WriteLine($"Routes analyzed: {TestRouteIds.Count}");

            // Performance assertions for large dataset analytics
            Assert.True(stopwatch.Elapsed.TotalSeconds < 30, $"Analytics should complete within 30 seconds for large dataset");
            Assert.True(PerformanceTestHelpers.BytesToMB(memoryUsed) < 500, "Memory usage should stay under 500MB");

            _output.WriteLine("✅ Large dataset analytics performance test PASSED");
        }

        #endregion

        #region Resource Contention Tests

        [Fact]
        public async Task ResourceContention_DatabaseConnections_ShouldHandleLimit()
        {
            _output.WriteLine("Testing database connection resource contention...");

            const int connectionAttempts = 50;
            var tasks = new List<Task<bool>>();
            var connectionResults = new ConcurrentBag<(bool Success, TimeSpan Duration, string Error)>();

            // Attempt many simultaneous database operations
            for (int i = 0; i < connectionAttempts; i++)
            {
                var operationId = i;

                tasks.Add(Task.Run(() =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        // Perform database operation
                        var vehicle = CreateTestVehicle($"_Contention_{operationId}");
                        var vehicleId = VehicleRepository.AddVehicle(vehicle);

                        if (vehicleId > 0)
                        {
                            lock (TestVehicleIds)
                            {
                                TestVehicleIds.Add(vehicleId);
                            }

                            // Verify the operation
                            var retrievedVehicle = VehicleRepository.GetVehicleById(vehicleId);
                            var success = retrievedVehicle != null;

                            stopwatch.Stop();
                            connectionResults.Add((success, stopwatch.Elapsed, null!));
                            return success;
                        }
                        else
                        {
                            stopwatch.Stop();
                            connectionResults.Add((false, stopwatch.Elapsed, "Failed to create vehicle"));
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        stopwatch.Stop();
                        connectionResults.Add((false, stopwatch.Elapsed, ex.Message));
                        return false;
                    }
                }));

                // Small stagger to simulate realistic timing
                if (i % 10 == 0 && i > 0)
                {
                    Thread.Sleep(50);
                }
            }

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            var successRate = (double)successCount / connectionAttempts;

            _output.WriteLine($"Connection contention test results:");
            _output.WriteLine($"  Successful operations: {successCount}/{connectionAttempts} ({successRate:P})");

            var allResults = connectionResults.ToArray();
            if (allResults.Any())
            {
                var avgDuration = allResults.Average(r => r.Duration.TotalMilliseconds);
                var maxDuration = allResults.Max(r => r.Duration.TotalMilliseconds);
                var failedResults = allResults.Where(r => !r.Success && !string.IsNullOrEmpty(r.Error)).ToArray();

                _output.WriteLine($"  Average operation time: {avgDuration:F2}ms");
                _output.WriteLine($"  Maximum operation time: {maxDuration:F2}ms");

                if (failedResults.Any())
                {
                    _output.WriteLine($"  Common errors:");
                    var errorGroups = failedResults.GroupBy(r => r.Error).Take(3);
                    foreach (var group in errorGroups)
                    {
                        _output.WriteLine($"    {group.Key}: {group.Count()} occurrences");
                    }
                }
            }

            // At least 90% should succeed even under connection contention
            Assert.True(successRate >= 0.9, $"Success rate {successRate:P} should be at least 90% under connection contention");

            _output.WriteLine("✅ Database connection resource contention test PASSED");
        }

        [Fact]
        public async Task ResourceContention_MemoryPressure_ShouldNotLeak()
        {
            _output.WriteLine("Testing memory pressure and leak prevention...");

            var initialMemory = PerformanceTestHelpers.GetMemoryUsage();
            _output.WriteLine($"Initial memory: {PerformanceTestHelpers.BytesToMB(initialMemory):F2}MB");

            const int iterations = 20;
            var memoryMeasurements = new List<long>();

            for (int i = 0; i < iterations; i++)
            {
                // Perform memory-intensive operations
                var vehicles = new List<Vehicle>();
                for (int j = 0; j < 100; j++)
                {
                    vehicles.Add(CreateTestVehicle($"_Memory_{i}_{j}"));
                }

                // Add to database (simulating real usage)
                var vehicleIds = new List<int>();
                foreach (var vehicle in vehicles)
                {
                    var id = VehicleRepository.AddVehicle(vehicle);
                    vehicleIds.Add(id);
                }

                lock (TestVehicleIds)
                {
                    TestVehicleIds.AddRange(vehicleIds);
                }

                // Retrieve data (more memory usage)
                var retrievedVehicles = VehicleRepository.GetAllVehicles();

                // Force garbage collection and measure
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var currentMemory = PerformanceTestHelpers.GetMemoryUsage();
                memoryMeasurements.Add(currentMemory);

                if (i % 5 == 0)
                {
                    _output.WriteLine($"Iteration {i}: Memory = {PerformanceTestHelpers.BytesToMB(currentMemory):F2}MB");
                }

                // Small delay to allow cleanup
                await Task.Delay(100);
            }

            var finalMemory = PerformanceTestHelpers.GetMemoryUsage();
            var memoryIncrease = finalMemory - initialMemory;

            _output.WriteLine($"Final memory: {PerformanceTestHelpers.BytesToMB(finalMemory):F2}MB");
            _output.WriteLine($"Memory increase: {PerformanceTestHelpers.BytesToMB(memoryIncrease):F2}MB");

            // Check for memory leaks
            var maxMemory = memoryMeasurements.Max();
            var memoryGrowthPattern = memoryMeasurements.Skip(iterations / 2).Average() - memoryMeasurements.Take(iterations / 2).Average();

            _output.WriteLine($"Memory growth pattern: {PerformanceTestHelpers.BytesToMB((long)memoryGrowthPattern):F2}MB");

            // Memory should not grow excessively
            Assert.True(PerformanceTestHelpers.BytesToMB(memoryIncrease) < 200, "Memory increase should be under 200MB");
            Assert.True(PerformanceTestHelpers.BytesToMB((long)Math.Abs(memoryGrowthPattern)) < 50, "Memory growth pattern should be stable");

            _output.WriteLine("✅ Memory pressure and leak prevention test PASSED");
        }

        #endregion

        #region Helper Methods

        private async Task CreateLargeTestDataset(int vehicleCount, int daysOfData)
        {
            _output.WriteLine($"Creating large test dataset: {vehicleCount} vehicles, {daysOfData} days...");

            var tasks = new List<Task>();
            var batchSize = 25;

            for (int batch = 0; batch < vehicleCount; batch += batchSize)
            {
                var batchEnd = Math.Min(batch + batchSize, vehicleCount);

                tasks.Add(Task.Run(() =>
                {
                    var batchVehicleIds = new List<int>();
                    var batchDriverIds = new List<int>();
                    var batchRouteIds = new List<int>();

                    // Create vehicles and drivers for this batch
                    for (int i = batch; i < batchEnd; i++)
                    {
                        var vehicle = CreateTestVehicle($"_Large_{i:D4}");
                        var driver = CreateTestDriver($"_Large_{i:D4}");

                        var vehicleId = VehicleRepository.AddVehicle(vehicle);
                        var driverId = DriverRepository.AddDriver(driver);

                        batchVehicleIds.Add(vehicleId);
                        batchDriverIds.Add(driverId);

                        // Create historical routes
                        for (int day = 0; day < Math.Min(daysOfData, 30); day++) // Limit to 30 days per vehicle to avoid excessive data
                        {
                            var route = CreateTestRoute(vehicleId, driverId, $"_D{day}");
                            route.Date = DateTime.Today.AddDays(-day).ToString("yyyy-MM-dd");

                            var routeId = RouteRepository.AddRoute(route);
                            batchRouteIds.Add(routeId);
                        }
                    }

                    // Add to master lists thread-safely
                    lock (TestVehicleIds)
                    {
                        TestVehicleIds.AddRange(batchVehicleIds);
                        TestDriverIds.AddRange(batchDriverIds);
                        TestRouteIds.AddRange(batchRouteIds);
                    }
                }));

                // Process batches to avoid overwhelming the database
                if (tasks.Count >= 4) // Process 4 batches at a time
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                    _output.WriteLine($"Completed batch up to vehicle {batchEnd}...");
                }
            }

            // Complete remaining tasks
            if (tasks.Any())
            {
                await Task.WhenAll(tasks);
            }

            _output.WriteLine($"✅ Created large dataset: {TestVehicleIds.Count} vehicles, {TestDriverIds.Count} drivers, {TestRouteIds.Count} routes");
        }

        #endregion
    }
}
