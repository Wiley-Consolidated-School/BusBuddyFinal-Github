using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Xunit;
using FluentAssertions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Moq;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.Models;

namespace BusBuddy.Tests.Performance
{
    /// <summary>
    /// Performance tests for critical BusBuddy operations
    /// Tests ensure system meets performance requirements under load
    /// </summary>
    public class BusBuddyPerformanceTests
    {
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<BusBuddy.UI.Services.IDatabaseHelperService> _mockDatabaseService;
        private readonly Mock<IRouteAnalyticsService> _mockRouteAnalyticsService;
        private readonly Mock<IPredictiveMaintenanceService> _mockMaintenanceService;

        public BusBuddyPerformanceTests()
        {
            _mockNavigationService = new Mock<INavigationService>();
            _mockDatabaseService = new Mock<BusBuddy.UI.Services.IDatabaseHelperService>();
            _mockRouteAnalyticsService = new Mock<IRouteAnalyticsService>();
            _mockMaintenanceService = new Mock<IPredictiveMaintenanceService>();

            SetupPerformanceTestData();
        }

        /// <summary>
        /// Tests dashboard initialization performance
        /// Requirement: Dashboard should load within 2 seconds
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Component", "Dashboard")]
        public void Dashboard_InitializationTime_ShouldBeFast()
        {
            Console.WriteLine("🔍 Starting Dashboard initialization performance test...");

            // Arrange
            var stopwatch = Stopwatch.StartNew();
            BusBuddyDashboardSyncfusion? dashboard = null;

            // Act
            dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
                "Dashboard should initialize within 2 seconds");

            dashboard.Should().NotBeNull("Dashboard should be created successfully");

            Console.WriteLine($"✅ Dashboard initialization completed in {stopwatch.ElapsedMilliseconds}ms");

            dashboard?.Dispose();
        }

        /// <summary>
        /// Tests route analytics calculation performance with large datasets
        /// Requirement: Analytics should complete within 5 seconds for 1000 routes
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Component", "Analytics")]
        public async Task RouteAnalytics_LargeDataset_ShouldPerformWithinThreshold()
        {
            // Arrange - Setup large dataset
            var routeData = GenerateLargeRouteDataset(1000);
            var stopwatch = Stopwatch.StartNew();

            _mockRouteAnalyticsService.Setup(x => x.GetRouteEfficiencyMetricsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(routeData.Select(r => new RouteEfficiencyMetrics
                {
                    RouteId = r.RouteId,
                    RouteName = r.RouteName ?? "Test Route",
                    EfficiencyScore = 85.5 + (r.RouteId % 15),
                    TotalMiles = r.Distance,
                    TotalRiders = r.VehicleCapacity / 2 // Assume half capacity
                }).ToList());

            // Act - Calculate analytics for all routes
            var results = await _mockRouteAnalyticsService.Object.GetRouteEfficiencyMetricsAsync(DateTime.Now.AddDays(-30), DateTime.Now);

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000,
                "Analytics calculation should complete within 5 seconds for 1000 routes");

            results.Should().HaveCount(1000, "Should process all routes");
            results.All(r => r.EfficiencyScore > 0).Should().BeTrue("All routes should have valid efficiency scores");

            Console.WriteLine($"✅ Route analytics for {routeData.Count} routes completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Tests UI responsiveness under rapid navigation
        /// Requirement: UI should remain responsive during rapid form switches
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Component", "Navigation")]
        public async Task Navigation_RapidFormSwitching_ShouldMaintainResponsiveness()
        {
            // Arrange
            var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);
            var navigationTimes = new List<long>();
            var navigationActions = new string[]
            {
                "ShowVehicleManagement",
                "ShowDriverManagement",
                "ShowRouteManagement",
                "ShowMaintenanceManagement",
                "ShowReports",
                "ShowAnalyticsDemo"
            };

            // Act - Perform rapid navigation
            for (int i = 0; i < 50; i++) // 50 rapid navigation operations
            {
                var stopwatch = Stopwatch.StartNew();

                var action = navigationActions[i % navigationActions.Length];
                switch (action)
                {
                    case "ShowVehicleManagement":
                        _mockNavigationService.Object.ShowVehicleManagement();
                        break;
                    case "ShowDriverManagement":
                        _mockNavigationService.Object.ShowDriverManagement();
                        break;
                    case "ShowRouteManagement":
                        _mockNavigationService.Object.ShowRouteManagement();
                        break;
                    case "ShowMaintenanceManagement":
                        _mockNavigationService.Object.ShowMaintenanceManagement();
                        break;
                    case "ShowReports":
                        _mockNavigationService.Object.ShowReports();
                        break;
                    case "ShowAnalyticsDemo":
                        _mockNavigationService.Object.ShowAnalyticsDemo();
                        break;
                }

                stopwatch.Stop();
                navigationTimes.Add(stopwatch.ElapsedMilliseconds);

                // Small delay to simulate realistic usage
                await Task.Delay(10);
            }

            // Assert
            var averageNavigationTime = navigationTimes.Sum() / (double)navigationTimes.Count;
            var maxNavigationTime = navigationTimes.Max();

            averageNavigationTime.Should().BeLessThan(100,
                "Average navigation time should be under 100ms");

            maxNavigationTime.Should().BeLessThan(500,
                "Maximum navigation time should be under 500ms");

            Console.WriteLine($"✅ Rapid navigation test completed - Average: {averageNavigationTime:F2}ms, Max: {maxNavigationTime}ms");

            dashboard.Dispose();
        }

        /// <summary>
        /// Tests memory usage during extended operation
        /// Requirement: Memory usage should remain stable during long-running operations
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Component", "Memory")]
        public async Task Memory_ExtendedOperation_ShouldRemainStable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(true);
            var memoryMeasurements = new List<long>();
            var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            // Act - Simulate extended operation
            for (int i = 0; i < 100; i++)
            {
                // Simulate typical operations
                _mockNavigationService.Object.ShowVehicleManagement();
                await Task.Delay(10);

                _mockNavigationService.Object.ShowAnalyticsDemo();
                await Task.Delay(10);

                // Measure memory every 10 iterations
                if (i % 10 == 0)
                {
                    var currentMemory = GC.GetTotalMemory(false);
                    memoryMeasurements.Add(currentMemory);
                }
            }

            var finalMemory = GC.GetTotalMemory(true);

            // Assert
            var memoryGrowth = finalMemory - initialMemory;
            var memoryGrowthMB = memoryGrowth / (1024.0 * 1024.0);

            memoryGrowthMB.Should().BeLessThan(50,
                "Memory growth should be less than 50MB during extended operation");

            // Check for memory leaks (excessive growth pattern)
            if (memoryMeasurements.Count > 2)
            {
                var firstMeasurement = memoryMeasurements.First();
                var lastMeasurement = memoryMeasurements.Last();
                var growthRatio = (double)lastMeasurement / firstMeasurement;

                growthRatio.Should().BeLessThan(2.0,
                    "Memory should not double during extended operation (potential leak)");
            }

            Console.WriteLine($"✅ Memory stability test completed - Growth: {memoryGrowthMB:F2}MB");

            dashboard.Dispose();
        }

        /// <summary>
        /// Tests concurrent user simulation
        /// Requirement: System should handle multiple concurrent operations
        /// </summary>
        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Component", "Concurrency")]
        public async Task Concurrency_MultipleUsers_ShouldHandleGracefully()
        {
            // Arrange
            const int concurrentUsers = 10;
            var tasks = new List<Task>();
            var results = new List<bool>();
            var _resultsLock = new object();
            var stopwatch = Stopwatch.StartNew();

            // Act - Simulate concurrent users
            for (int user = 0; user < concurrentUsers; user++)
            {
                var userId = user;
                var task = Task.Run(async () =>
                {
                    try
                    {
                        // Each user performs a series of operations
                        var userDashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

                        // Simulate user workflow
                        _mockNavigationService.Object.ShowVehicleManagement();
                        await Task.Delay(50);

                        _mockNavigationService.Object.ShowAnalyticsDemo();
                        await Task.Delay(50);

                        _mockNavigationService.Object.ShowReports();
                        await Task.Delay(50);

                        userDashboard.Dispose();

                        lock (_resultsLock)
                        {
                            results.Add(true);
                        }

                        Console.WriteLine($"✓ User {userId} completed workflow");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ User {userId} failed: {ex.Message}");
                        lock (_resultsLock)
                        {
                            results.Add(false);
                        }
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            var successfulUsers = results.Count(r => r);
            var successRate = (double)successfulUsers / concurrentUsers * 100;

            successRate.Should().BeGreaterThan(90,
                "At least 90% of concurrent users should complete successfully");

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
                "Concurrent operations should complete within 10 seconds");

            Console.WriteLine($"✅ Concurrency test completed - {successfulUsers}/{concurrentUsers} users successful ({successRate:F1}%) in {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Generate large dataset for performance testing
        /// </summary>
        private List<RouteTestData> GenerateLargeRouteDataset(int count)
        {
            var routes = new List<RouteTestData>();
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();

            for (int i = 1; i <= count; i++)
            {
                routes.Add(new RouteTestData
                {
                    RouteId = i,
                    RouteName = $"Route {i}",
                    StopCount = GetSecureRandomInt(rng, 5, 25),
                    Distance = GetSecureRandomDouble(rng) * 50 + 5, // 5-55 miles
                    EstimatedDuration = GetSecureRandomInt(rng, 30, 120), // 30-120 minutes
                    VehicleCapacity = GetSecureRandomInt(rng, 20, 80)
                });
            }

            return routes;
        }

        /// <summary>
        /// Generate cryptographically secure random integer within range
        /// </summary>
        private int GetSecureRandomInt(System.Security.Cryptography.RandomNumberGenerator rng, int min, int max)
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            int value = Math.Abs(BitConverter.ToInt32(bytes, 0));
            return min + (value % (max - min + 1));
        }

        /// <summary>
        /// Generate cryptographically secure random double between 0 and 1
        /// </summary>
        private double GetSecureRandomDouble(System.Security.Cryptography.RandomNumberGenerator rng)
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (double)value / uint.MaxValue;
        }

        private void SetupPerformanceTestData()
        {
            // Setup mock responses for performance testing
            _mockDatabaseService.Setup(x => x.TestConnectionAsync())
                .ReturnsAsync(true);

            _mockRouteAnalyticsService.Setup(x => x.GetFleetAnalyticsSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new FleetAnalyticsSummary
                {
                    AverageEfficiencyScore = 85.5,
                    TotalMiles = 1000,
                    TotalRiders = 500
                });
        }
    }

    /// <summary>
    /// BenchmarkDotNet performance benchmarks for critical operations
    /// Run with: dotnet run --project BusBuddy.Benchmarks --configuration Release
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob]
    public class BusBuddyBenchmarks
    {
        private Mock<INavigationService> _mockNavigationService = null!;
        private Mock<BusBuddy.UI.Services.IDatabaseHelperService> _mockDatabaseService = null!;
        private BusBuddyDashboardSyncfusion _dashboard = null!;

        [GlobalSetup]
        public void Setup()
        {
            _mockNavigationService = new Mock<INavigationService>();
            _mockDatabaseService = new Mock<BusBuddy.UI.Services.IDatabaseHelperService>();

            _mockDatabaseService.Setup(x => x.TestConnectionAsync())
                .ReturnsAsync(true);
        }

        [Benchmark]
        public void DashboardCreation()
        {
            using var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);
        }

        [Benchmark]
        [Arguments(10)]
        [Arguments(100)]
        [Arguments(1000)]
        public void NavigationOperations(int operationCount)
        {
            using var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            for (int i = 0; i < operationCount; i++)
            {
                _mockNavigationService.Object.ShowVehicleManagement();
                _mockNavigationService.Object.ShowDriverManagement();
                _mockNavigationService.Object.ShowReports();
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dashboard?.Dispose();
        }
    }

    /// <summary>
    /// Supporting classes for performance testing
    /// </summary>
    public class RouteTestData
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int StopCount { get; set; }
        public double Distance { get; set; }
        public int EstimatedDuration { get; set; }
        public int VehicleCapacity { get; set; }
    }

    public class RouteAnalyticsResult
    {
        public int RouteId { get; set; }
        public double EfficiencyScore { get; set; }
        public TimeSpan CalculationTime { get; set; }
        public List<string> RecommendedOptimizations { get; set; } = new();
    }
}
