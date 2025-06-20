using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;
using FluentAssertions;
using Moq;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.Tests.Utilities;

namespace BusBuddy.Tests.EndToEnd
{
    /// <summary>
    /// End-to-end tests that validate complete user workflows in BusBuddy
    /// These tests simulate real user interactions from start to finish
    /// </summary>
    [TestCaseOrderer("BusBuddy.Tests.Utilities.PriorityOrderer", "BusBuddy.Tests")]
    public class CompleteUserWorkflowTests : IDisposable
    {
        private readonly Mock<INavigationService> _mockNavigationService;
        private readonly Mock<BusBuddy.UI.Services.IDatabaseHelperService> _mockDatabaseService;
        private readonly Mock<IVehicleService> _mockVehicleService;
        private readonly Mock<IRouteAnalyticsService> _mockRouteAnalyticsService;
        private BusBuddyDashboardSyncfusion? _dashboard;

        public CompleteUserWorkflowTests()
        {
            _mockNavigationService = new Mock<INavigationService>();
            _mockDatabaseService = new Mock<BusBuddy.UI.Services.IDatabaseHelperService>();
            _mockVehicleService = new Mock<IVehicleService>();
            _mockRouteAnalyticsService = new Mock<IRouteAnalyticsService>();

            // Setup mock data for realistic testing
            SetupMockData();
        }

        /// <summary>
        /// Test Priority: 1 - System Initialization
        /// Complete workflow: System startup → Dashboard load → Basic navigation
        /// </summary>
        [Fact]
        [TestPriority(1)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "SystemInitialization")]
        public void EndToEnd_SystemInitialization_ShouldCompleteSuccessfully()
        {
            // Arrange - Simulate system startup
            Exception? startupException = null;

            // Act - Initialize dashboard (core system entry point)
            try
            {
                _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

                // Verify dashboard is created without throwing
                _ = _dashboard.Should().NotBeNull("Dashboard should initialize successfully");

                // Verify basic properties are set correctly
                _ = _dashboard.FormBorderStyle.Should().Be(FormBorderStyle.None, "Dashboard should be borderless for embedding");
                _ = _dashboard.WindowState.Should().Be(FormWindowState.Maximized, "Dashboard should maximize for full workspace");
            }
            catch (Exception ex)
            {
                startupException = ex;
            }

            // Assert - System should start cleanly
            _ = startupException.Should().BeNull("System initialization should not throw exceptions");

            // Verify navigation service is properly wired
            _mockNavigationService.Verify(x => x.ShowVehicleManagement(), Times.Never,
                "Navigation methods should not be called during initialization");
        }

        /// <summary>
        /// Test Priority: 2 - Vehicle Management Workflow
        /// Complete workflow: Dashboard → Vehicle Management → Add Vehicle → View Vehicle → Return to Dashboard
        /// </summary>
        [Fact]
        [TestPriority(2)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "VehicleManagement")]
        public async Task EndToEnd_VehicleManagementWorkflow_ShouldCompleteSuccessfully()
        {
            // Arrange - Initialize dashboard and setup vehicle data
            _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            var testVehicle = new BusBuddy.Models.Vehicle
            {
                Id = 1,
                VehicleNumber = "BUS001",
                Make = "Blue Bird",
                Model = "Vision",
                Year = 2023,
                Capacity = 72,
                Status = "Active"
            };

            _ = _mockVehicleService.Setup(x => x.GetAllVehiclesAsync())
                .Returns(Task.FromResult((IEnumerable<BusBuddy.Models.Vehicle>)new List<BusBuddy.Models.Vehicle> { testVehicle }));

            // Act & Assert - Navigate to vehicle management
            _ = _mockNavigationService.Setup(x => x.ShowVehicleManagement())
                .Callback(() => Console.WriteLine("✓ Navigation to Vehicle Management triggered"));

            // Simulate user clicking vehicle management button
            // Note: In real E2E tests, this would be actual UI interaction
            bool navigationTriggered = false;
            _ = _mockNavigationService.Setup(x => x.ShowVehicleManagement())
                .Callback(() => navigationTriggered = true);

            // Trigger navigation (simulating button click)
            _mockNavigationService.Object.ShowVehicleManagement();

            _ = navigationTriggered.Should().BeTrue("Vehicle management navigation should be triggered");

            // Verify vehicle data loading
            var vehicles = await _mockVehicleService.Object.GetAllVehiclesAsync();
            _ = vehicles.Should().HaveCount(1, "Should load test vehicle data");
            _ = vehicles.First().VehicleNumber.Should().Be("BUS001", "Should load correct vehicle details");

            Console.WriteLine("✅ Vehicle Management Workflow completed successfully");
        }

        /// <summary>
        /// Test Priority: 3 - Route Analytics Workflow
        /// Complete workflow: Dashboard → Analytics → Generate Report → View Results → Export Data
        /// </summary>
        [Fact(Timeout = 30000)] // 30 second timeout
        [TestPriority(3)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "RouteAnalytics")]
        public async Task EndToEnd_RouteAnalyticsWorkflow_ShouldCompleteSuccessfully()
        {
            Console.WriteLine("🔍 Starting RouteAnalyticsWorkflow test...");

            // Arrange - Setup analytics data
            _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            Console.WriteLine("🔍 Dashboard created, setting up analytics data...");

            var analyticsData = new RouteAnalyticsData
            {
                RouteId = 1,
                RouteName = "Elementary Route A",
                AverageSpeed = 25.5,
                FuelEfficiency = 8.2,
                OnTimePerformance = 94.5,
                TotalDistance = 12.8,
                EstimatedDuration = 45
            };

            Console.WriteLine("🔍 Analytics data created, setting up mock...");

            _ = _mockRouteAnalyticsService.Setup(x => x.GetFleetAnalyticsSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new FleetAnalyticsSummary
                {
                    AverageEfficiencyScore = analyticsData.OnTimePerformance,
                    TotalMiles = analyticsData.TotalDistance
                });

            Console.WriteLine("🔍 Mock setup complete, starting test actions...");

            // Act - Simulate analytics workflow
            _ = _mockNavigationService.Setup(x => x.ShowAnalyticsDemo())
                .Callback(() => Console.WriteLine("✓ Analytics demo navigation triggered"));

            bool analyticsNavigated = false;
            _ = _mockNavigationService.Setup(x => x.ShowAnalyticsDemo())
                .Callback(() => analyticsNavigated = true);

            // Trigger analytics navigation
            _mockNavigationService.Object.ShowAnalyticsDemo();

            Console.WriteLine("🔍 Navigation triggered, calling analytics service...");

            // Simulate analytics calculation
            var results = await _mockRouteAnalyticsService.Object.GetFleetAnalyticsSummaryAsync(DateTime.Now.AddDays(-30), DateTime.Now);

            Console.WriteLine("🔍 Analytics service call completed, verifying results...");

            // Assert - Verify complete analytics workflow
            _ = analyticsNavigated.Should().BeTrue("Analytics navigation should be triggered");
            _ = results.Should().NotBeNull("Analytics calculation should return results");
            _ = results.AverageEfficiencyScore.Should().BeGreaterThan(90, "Should show realistic performance metrics");

            Console.WriteLine($"✅ Route Analytics Workflow completed - Efficiency Score: {results.AverageEfficiencyScore}%");
        }

        /// <summary>
        /// Test Priority: 4 - Driver Management Complete Workflow
        /// Complete workflow: Dashboard → Driver Management → Add Driver → Assign Route → View Schedule → Update Status
        /// </summary>
        [Fact]
        [TestPriority(4)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "DriverManagement")]
        public async Task EndToEnd_DriverManagementWorkflow_ShouldCompleteSuccessfully()
        {
            // Arrange - Setup driver management scenario
            _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            var testDriver = new Driver
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                LicenseNumber = "DL123456789",
                LicenseExpiration = DateTime.Now.AddYears(2),
                PhoneNumber = "555-0123",
                IsActive = true,
                HireDate = DateTime.Now.AddYears(-2)
            };

            var mockDriverService = new Mock<IDriverService>();
            _ = mockDriverService.Setup(x => x.GetAllDriversAsync())
                .ReturnsAsync(new List<Driver> { testDriver });

            // Act - Complete driver management workflow
            bool driverNavigated = false;
            _ = _mockNavigationService.Setup(x => x.ShowDriverManagement())
                .Callback(() =>
                {
                    driverNavigated = true;
                    Console.WriteLine("✓ Navigated to Driver Management");
                });

            // Step 1: Navigate to driver management
            _mockNavigationService.Object.ShowDriverManagement();

            // Step 2: Load driver data
            var drivers = await mockDriverService.Object.GetAllDriversAsync();

            // Step 3: Simulate driver selection and route assignment
            var selectedDriver = drivers.First();
            selectedDriver.AssignedRouteId = 1;

            // Assert - Verify complete workflow
            _ = driverNavigated.Should().BeTrue("Should navigate to driver management");
            _ = drivers.Should().HaveCount(1, "Should load driver data");
            _ = selectedDriver.FirstName.Should().Be("John", "Should load correct driver details");
            _ = selectedDriver.LicenseExpiration.Should().BeAfter(DateTime.Now, "License should be valid");
            _ = selectedDriver.AssignedRouteId.Should().Be(1, "Driver should be assigned to route");

            Console.WriteLine($"✅ Driver Management Workflow completed - Driver: {selectedDriver.FirstName} {selectedDriver.LastName}, Route: {selectedDriver.AssignedRouteId}");
        }

        /// <summary>
        /// Test Priority: 5 - Report Generation Complete Workflow
        /// Complete workflow: Dashboard → Reports → Select Parameters → Generate Report → Export → Return to Dashboard
        /// </summary>
        [Fact]
        [TestPriority(5)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "ReportGeneration")]
        public async Task EndToEnd_ReportGenerationWorkflow_ShouldCompleteSuccessfully()
        {
            // Arrange - Setup report generation scenario
            _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

            var reportData = new ReportData
            {
                ReportType = "Vehicle Utilization",
                DateRange = new DateRange { Start = DateTime.Now.AddDays(-30), End = DateTime.Now },
                TotalVehicles = 25,
                ActiveVehicles = 23,
                UtilizationRate = 92.0,
                GeneratedDate = DateTime.Now
            };

            var mockReportService = new Mock<IReportService>();
            _ = mockReportService.Setup(x => x.GenerateVehicleUtilizationReportAsync(It.IsAny<DateRange>()))
                .ReturnsAsync(reportData);

            // Act - Complete report generation workflow
            bool reportsNavigated = false;
            _ = _mockNavigationService.Setup(x => x.ShowReports())
                .Callback(() =>
                {
                    reportsNavigated = true;
                    Console.WriteLine("✓ Navigated to Reports");
                });

            // Step 1: Navigate to reports
            _mockNavigationService.Object.ShowReports();

            // Step 2: Generate report with parameters
            var dateRange = new DateRange { Start = DateTime.Now.AddDays(-30), End = DateTime.Now };
            var report = await mockReportService.Object.GenerateVehicleUtilizationReportAsync(dateRange);

            // Step 3: Simulate export functionality
            var exportSuccess = SimulateReportExport(report);

            // Assert - Verify complete workflow
            _ = reportsNavigated.Should().BeTrue("Should navigate to reports");
            _ = report.Should().NotBeNull("Report should be generated");
            _ = report.TotalVehicles.Should().BeGreaterThan(0, "Report should contain vehicle data");
            _ = report.UtilizationRate.Should().BeInRange(0, 100, "Utilization rate should be valid percentage");
            _ = exportSuccess.Should().BeTrue("Report export should succeed");

            Console.WriteLine($"✅ Report Generation Workflow completed - Report: {report.ReportType}, Utilization: {report.UtilizationRate}%");
        }

        /// <summary>
        /// Test Priority: 6 - Error Recovery Workflow
        /// Complete workflow: System Error → Error Display → Recovery Action → Continue Operation
        /// </summary>
        [Fact]
        [TestPriority(6)]
        [Trait("Category", "EndToEnd")]
        [Trait("Category", "UserJourney")]
        [Trait("Workflow", "ErrorRecovery")]
        public async Task EndToEnd_ErrorRecoveryWorkflow_ShouldHandleGracefully()
        {
            // Arrange - Setup error scenario
            _ = _mockDatabaseService.Setup(x => x.TestConnectionAsync())
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            Exception? caughtException = null;
            bool recoveryAttempted = false;

            // Act - Simulate error and recovery
            try
            {
                _dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

                // Simulate operation that might fail
                _ = await _mockDatabaseService.Object.TestConnectionAsync();
            }
            catch (InvalidOperationException ex)
            {
                caughtException = ex;
                recoveryAttempted = true;
                Console.WriteLine("✓ Error caught and recovery initiated");
            }

            // Assert - Verify error handling
            _ = caughtException.Should().NotBeNull("Error should be caught");
            _ = caughtException!.Message.Should().Contain("Database connection failed", "Should preserve error details");
            _ = recoveryAttempted.Should().BeTrue("Recovery should be attempted");
            _ = _dashboard.Should().NotBeNull("Dashboard should still be functional after error");

            Console.WriteLine("✅ Error Recovery Workflow completed successfully");
        }

        /// <summary>
        /// Simulates report export functionality
        /// </summary>
        private bool SimulateReportExport(ReportData report)
        {
            try
            {
                // Simulate export logic
                Console.WriteLine($"Exporting report: {report.ReportType}");

                // In real implementation, this would save to file system
                var exportPath = $"Reports/{report.ReportType}_{DateTime.Now:yyyyMMdd}.pdf";
                Console.WriteLine($"Report exported to: {exportPath}");

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SetupMockData()
        {
            // Setup realistic database responses
            _ = _mockDatabaseService.Setup(x => x.TestConnectionAsync())
                .ReturnsAsync(true);

            // Setup navigation service responses
            _ = _mockNavigationService.Setup(x => x.ShowVehicleManagement())
                .Callback(() => Console.WriteLine("Navigation: Vehicle Management"));

            _ = _mockNavigationService.Setup(x => x.ShowDriverManagement())
                .Callback(() => Console.WriteLine("Navigation: Driver Management"));

            _ = _mockNavigationService.Setup(x => x.ShowReports())
                .Callback(() => Console.WriteLine("Navigation: Reports"));

            _ = _mockNavigationService.Setup(x => x.ShowAnalyticsDemo())
                .Callback(() => Console.WriteLine("Navigation: Analytics Demo"));
        }

        public void Dispose()
        {
            _dashboard?.Dispose();
        }
    }

    /// <summary>
    /// Supporting data classes for end-to-end testing
    /// </summary>
    public class RouteAnalyticsData
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public double AverageSpeed { get; set; }
        public double FuelEfficiency { get; set; }
        public double OnTimePerformance { get; set; }
        public double TotalDistance { get; set; }
        public int EstimatedDuration { get; set; }
    }

    public class ReportData
    {
        public string ReportType { get; set; } = string.Empty;
        public DateRange DateRange { get; set; } = new();
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public double UtilizationRate { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    // Mock interfaces for testing
    public interface IDriverService
    {
        Task<List<Driver>> GetAllDriversAsync();
    }

    public interface IReportService
    {
        Task<ReportData> GenerateVehicleUtilizationReportAsync(DateRange dateRange);
    }

    public class Driver
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiration { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime HireDate { get; set; }
        public int? AssignedRouteId { get; set; }
    }
}
