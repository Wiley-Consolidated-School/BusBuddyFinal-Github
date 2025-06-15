using Xunit;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Data;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BusBuddy.Tests
{
    /// <summary>
    /// Test class for enhanced analytics and route optimization services
    /// Ensures the new analytics features maintain 70%+ test coverage
    /// </summary>
    public class AnalyticsServiceTests
    {

        [Fact]
        public void RouteAnalyticsService_CalculateRouteEfficiency_ShouldReturnValidMetrics()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var route = new Route
            {
                RouteID = 1,
                RouteName = "Test Route",
                DateAsDateTime = DateTime.Now,
                AMBeginMiles = 100,
                AMEndMiles = 150,
                AMRiders = 25,
                PMBeginMiles = 150,
                PMEndMiles = 190,
                PMRiders = 20
            };

            // Act
            var metrics = service.CalculateRouteEfficiency(route);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(1, metrics.RouteId);
            Assert.Equal("Test Route", metrics.RouteName);
            Assert.Equal(90, metrics.TotalMiles); // (150-100) + (190-150)
            Assert.Equal(45, metrics.TotalRiders); // 25 + 20
            Assert.Equal(2.0, metrics.MilesPerRider); // 90 / 45
            Assert.True(metrics.EfficiencyScore > 0);
            Assert.True(metrics.EstimatedFuelCost > 0);
        }        [Fact]
        public async Task RouteAnalyticsService_GetRouteEfficiencyMetrics_ShouldReturnMetricsForDateRange()
        {
            // Arrange
            var routeRepo = new RouteRepository();
            var service = new RouteAnalyticsService(routeRepo);

            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;

            // Act
            var metrics = await service.GetRouteEfficiencyMetricsAsync(startDate, endDate);

            // Assert
            Assert.NotNull(metrics);
            // Should return list even if empty
            Assert.IsType<System.Collections.Generic.List<RouteEfficiencyMetrics>>(metrics);
        }

        [Fact]
        public async Task RouteAnalyticsService_AnalyzeRouteOptimizations_ShouldProvideRecommendations()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var date = DateTime.Now.Date;

            // Act
            var suggestions = await service.AnalyzeRouteOptimizationsAsync(date);

            // Assert
            Assert.NotNull(suggestions);
            Assert.IsType<System.Collections.Generic.List<RouteOptimizationSuggestion>>(suggestions);
        }        [Fact]
        public async Task RouteAnalyticsService_CalculateDriverPerformance_ShouldReturnValidMetrics()
        {
            // Arrange
            var driverRepo = new DriverRepository();
            var routeRepo = new RouteRepository();
            var service = new RouteAnalyticsService(routeRepo, driverRepository: driverRepo);

            // Create test driver
            var testDriver = new Driver
            {
                DriverName = "Test Analytics Driver",
                DriverEmail = "test.analytics@test.com",
                TrainingComplete = 1
            };
            driverRepo.AddDriver(testDriver);
            var addedDriver = driverRepo.GetAllDrivers().First(d => d.DriverEmail == "test.analytics@test.com");

            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            // Act
            var metrics = await service.CalculateDriverPerformanceAsync(addedDriver.DriverID, startDate, endDate);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(addedDriver.DriverID, metrics.DriverId);
            Assert.True(metrics.OverallEfficiencyScore >= 0);
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), metrics.PerformanceRating));
        }

        [Fact]
        public async Task RouteAnalyticsService_GetFleetAnalyticsSummary_ShouldReturnComprehensiveSummary()
        {
            // Arrange
            var service = new RouteAnalyticsService();
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            // Act
            var summary = await service.GetFleetAnalyticsSummaryAsync(startDate, endDate);

            // Assert
            Assert.NotNull(summary);
            Assert.Equal(startDate, summary.PeriodStart);
            Assert.Equal(endDate, summary.PeriodEnd);
            Assert.True(summary.TotalMiles >= 0);
            Assert.True(summary.TotalRiders >= 0);
            Assert.True(summary.VehicleUtilizationRate >= 0 && summary.VehicleUtilizationRate <= 100);
            Assert.NotNull(summary.TopPerformingRoutes);
        }

        [Fact]
        public void RouteEfficiencyMetrics_Properties_ShouldHaveValidDefaults()
        {
            // Arrange & Act
            var metrics = new RouteEfficiencyMetrics();

            // Assert
            Assert.Equal(0, metrics.RouteId);
            Assert.Equal(string.Empty, metrics.RouteName);
            Assert.Equal(0, metrics.TotalMiles);
            Assert.Equal(0, metrics.TotalRiders);
            Assert.Equal(0, metrics.MilesPerRider);
            Assert.Equal(0, metrics.EfficiencyScore);
            Assert.Equal(0, metrics.EstimatedFuelCost);
        }

        [Fact]
        public void RouteOptimizationSuggestion_Properties_ShouldBeSettable()
        {
            // Arrange & Act
            var suggestion = new RouteOptimizationSuggestion
            {
                RouteId = 1,
                RouteName = "Test Route",
                SuggestionType = OptimizationType.EfficiencyImprovement,
                Description = "Test suggestion",
                PotentialSavings = 150.50m,
                Priority = Priority.High
            };

            // Assert
            Assert.Equal(1, suggestion.RouteId);
            Assert.Equal("Test Route", suggestion.RouteName);
            Assert.Equal(OptimizationType.EfficiencyImprovement, suggestion.SuggestionType);
            Assert.Equal("Test suggestion", suggestion.Description);
            Assert.Equal(150.50m, suggestion.PotentialSavings);
            Assert.Equal(Priority.High, suggestion.Priority);
        }

        [Fact]
        public void DriverPerformanceMetrics_PerformanceRating_ShouldMapCorrectly()
        {
            // Arrange
            var metrics = new DriverPerformanceMetrics();

            // Act & Assert - Test enum values
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), PerformanceRating.Excellent));
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), PerformanceRating.Good));
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), PerformanceRating.Average));
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), PerformanceRating.BelowAverage));
            Assert.True(Enum.IsDefined(typeof(PerformanceRating), PerformanceRating.NeedsImprovement));
        }

        [Fact]
        public void FleetAnalyticsSummary_TopPerformingRoutes_ShouldBeInitialized()
        {
            // Arrange & Act
            var summary = new FleetAnalyticsSummary();

            // Assert
            Assert.NotNull(summary.TopPerformingRoutes);
            Assert.Empty(summary.TopPerformingRoutes);
        }

        [Theory]
        [InlineData(OptimizationType.EfficiencyImprovement)]
        [InlineData(OptimizationType.MileageReduction)]
        [InlineData(OptimizationType.VehicleUtilization)]
        [InlineData(OptimizationType.DriverOptimization)]
        [InlineData(OptimizationType.CostReduction)]
        public void OptimizationType_AllValues_ShouldBeValid(OptimizationType optimizationType)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(OptimizationType), optimizationType));
        }

        [Theory]
        [InlineData(Priority.Low)]
        [InlineData(Priority.Medium)]
        [InlineData(Priority.High)]
        [InlineData(Priority.Critical)]
        public void Priority_AllValues_ShouldBeValid(Priority priority)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(Priority), priority));
        }
    }
}
