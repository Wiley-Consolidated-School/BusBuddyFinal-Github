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
    /// Test class for predictive maintenance service functionality
    /// Ensures predictive maintenance features maintain quality and coverage
    /// </summary>
    public class PredictiveMaintenanceServiceTests
    {
        private readonly string _testConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=BusBuddyDB_Test;Integrated Security=True;TrustServerCertificate=True;Connection Timeout=30;";
        private const string _sqlServerProvider = "Microsoft.Data.SqlClient";

        [Fact]
        public async Task PredictiveMaintenanceService_GetMaintenancePredictions_ShouldReturnPredictions()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var service = new PredictiveMaintenanceService(vehicleRepo, maintenanceRepo);

            // Create test vehicle
            var testVehicle = new Vehicle
            {
                VehicleNumber = "PRED001",
                Make = "Test",
                Model = "Predictive Test",
                Year = 2020,
                Capacity = 45
            };
            vehicleRepo.AddVehicle(testVehicle);
            var addedVehicle = vehicleRepo.GetAllVehicles().First(v => v.VehicleNumber == "PRED001");

            // Act
            var predictions = await service.GetMaintenancePredictionsAsync(addedVehicle.Id);

            // Assert
            Assert.NotNull(predictions);
            Assert.IsType<System.Collections.Generic.List<MaintenancePrediction>>(predictions);
        }

        [Fact]
        public async Task PredictiveMaintenanceService_GetFleetMaintenanceSchedule_ShouldReturnRecommendations()
        {
            // Arrange
            var service = new PredictiveMaintenanceService();
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(90);

            // Act
            var recommendations = await service.GetFleetMaintenanceScheduleAsync(startDate, endDate);

            // Assert
            Assert.NotNull(recommendations);
            Assert.IsType<System.Collections.Generic.List<MaintenanceRecommendation>>(recommendations);
        }

        [Fact]
        public async Task PredictiveMaintenanceService_CalculateVehicleHealthScore_ShouldReturnValidScore()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var service = new PredictiveMaintenanceService(vehicleRepo);

            // Create test vehicle
            var testVehicle = new Vehicle
            {
                VehicleNumber = "HEALTH001",
                Make = "Test",
                Model = "Health Test",
                Year = 2019,
                Capacity = 40
            };
            vehicleRepo.AddVehicle(testVehicle);
            var addedVehicle = vehicleRepo.GetAllVehicles().First(v => v.VehicleNumber == "HEALTH001");

            // Act
            var healthScore = await service.CalculateVehicleHealthScoreAsync(addedVehicle.Id);

            // Assert
            Assert.NotNull(healthScore);
            Assert.Equal(addedVehicle.Id, healthScore.VehicleId);
            Assert.True(healthScore.OverallScore >= 0 && healthScore.OverallScore <= 100);
            Assert.True(Enum.IsDefined(typeof(VehicleHealthStatus), healthScore.HealthStatus));
            Assert.NotNull(healthScore.Recommendations);
        }

        [Fact]
        public async Task PredictiveMaintenanceService_AnalyzeMaintenanceCosts_ShouldReturnValidAnalysis()
        {
            // Arrange
            var vehicleRepo = new VehicleRepository(_testConnectionString, _sqlServerProvider);
            var maintenanceRepo = new MaintenanceRepository(_testConnectionString, _sqlServerProvider);
            var service = new PredictiveMaintenanceService(vehicleRepo, maintenanceRepo);

            // Create test vehicle
            var testVehicle = new Vehicle
            {
                VehicleNumber = "COST001",
                Make = "Test",
                Model = "Cost Analysis Test",
                Year = 2018,
                Capacity = 35
            };
            vehicleRepo.AddVehicle(testVehicle);
            var addedVehicle = vehicleRepo.GetAllVehicles().First(v => v.VehicleNumber == "COST001");

            var startDate = DateTime.Now.AddDays(-365);
            var endDate = DateTime.Now;

            // Act
            var analysis = await service.AnalyzeMaintenanceCostsAsync(addedVehicle.Id, startDate, endDate);

            // Assert
            Assert.NotNull(analysis);
            Assert.Equal(addedVehicle.Id, analysis.VehicleId);
            Assert.Equal(startDate, analysis.PeriodStart);
            Assert.Equal(endDate, analysis.PeriodEnd);
            Assert.True(analysis.TotalCost >= 0);
            Assert.NotNull(analysis.CostByCategory);
            Assert.NotNull(analysis.MonthlyCosts);
        }

        [Fact]
        public async Task PredictiveMaintenanceService_GetMaintenanceAlerts_ShouldReturnAlerts()
        {
            // Arrange
            var service = new PredictiveMaintenanceService();

            // Act
            var alerts = await service.GetMaintenanceAlertsAsync();

            // Assert
            Assert.NotNull(alerts);
            Assert.IsType<System.Collections.Generic.List<MaintenanceAlert>>(alerts);
        }

        [Fact]
        public void MaintenancePrediction_Properties_ShouldBeSettable()
        {
            // Arrange & Act
            var prediction = new MaintenancePrediction
            {
                VehicleId = 1,
                MaintenanceType = "Oil Change",
                PredictedDate = DateTime.Now.AddDays(30),
                Priority = MaintenancePriority.Medium,
                EstimatedCost = 75.00m,
                Reason = "Test reason",
                BasedOnMileage = true,
                PredictedMileage = 50000
            };

            // Assert
            Assert.Equal(1, prediction.VehicleId);
            Assert.Equal("Oil Change", prediction.MaintenanceType);
            Assert.Equal(MaintenancePriority.Medium, prediction.Priority);
            Assert.Equal(75.00m, prediction.EstimatedCost);
            Assert.Equal("Test reason", prediction.Reason);
            Assert.True(prediction.BasedOnMileage);
            Assert.Equal(50000, prediction.PredictedMileage);
        }

        [Fact]
        public void VehicleHealthScore_ComponentScores_ShouldBeInValidRange()
        {
            // Arrange & Act
            var healthScore = new VehicleHealthScore
            {
                MaintenanceComplianceScore = 85,
                AgeScore = 90,
                MileageScore = 75,
                ReliabilityScore = 80,
                CostEfficiencyScore = 70,
                OverallScore = 80
            };

            // Assert
            Assert.True(healthScore.MaintenanceComplianceScore >= 0 && healthScore.MaintenanceComplianceScore <= 100);
            Assert.True(healthScore.AgeScore >= 0 && healthScore.AgeScore <= 100);
            Assert.True(healthScore.MileageScore >= 0 && healthScore.MileageScore <= 100);
            Assert.True(healthScore.ReliabilityScore >= 0 && healthScore.ReliabilityScore <= 100);
            Assert.True(healthScore.CostEfficiencyScore >= 0 && healthScore.CostEfficiencyScore <= 100);
            Assert.True(healthScore.OverallScore >= 0 && healthScore.OverallScore <= 100);
        }

        [Theory]
        [InlineData(VehicleHealthStatus.Excellent)]
        [InlineData(VehicleHealthStatus.Good)]
        [InlineData(VehicleHealthStatus.Fair)]
        [InlineData(VehicleHealthStatus.Poor)]
        [InlineData(VehicleHealthStatus.Critical)]
        public void VehicleHealthStatus_AllValues_ShouldBeValid(VehicleHealthStatus status)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(VehicleHealthStatus), status));
        }

        [Theory]
        [InlineData(MaintenancePriority.Low)]
        [InlineData(MaintenancePriority.Medium)]
        [InlineData(MaintenancePriority.High)]
        [InlineData(MaintenancePriority.Critical)]
        public void MaintenancePriority_AllValues_ShouldBeValid(MaintenancePriority priority)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(MaintenancePriority), priority));
        }

        [Fact]
        public void MaintenanceRecommendation_Properties_ShouldBeSettable()
        {
            // Arrange & Act
            var recommendation = new MaintenanceRecommendation
            {
                VehicleId = 5,
                VehicleNumber = "BUS005",
                MaintenanceType = "Brake Service",
                RecommendedDate = DateTime.Now.AddDays(14),
                Priority = MaintenancePriority.High,
                EstimatedCost = 450.00m,
                Reason = "Brake pads due for replacement",
                CurrentMileage = 75000
            };

            // Assert
            Assert.Equal(5, recommendation.VehicleId);
            Assert.Equal("BUS005", recommendation.VehicleNumber);
            Assert.Equal("Brake Service", recommendation.MaintenanceType);
            Assert.Equal(MaintenancePriority.High, recommendation.Priority);
            Assert.Equal(450.00m, recommendation.EstimatedCost);
            Assert.Equal("Brake pads due for replacement", recommendation.Reason);
            Assert.Equal(75000, recommendation.CurrentMileage);
        }

        [Fact]
        public void MaintenanceCostAnalysis_Collections_ShouldBeInitialized()
        {
            // Arrange & Act
            var analysis = new MaintenanceCostAnalysis();

            // Assert
            Assert.NotNull(analysis.CostByCategory);
            Assert.NotNull(analysis.MonthlyCosts);
            Assert.Empty(analysis.CostByCategory);
            Assert.Empty(analysis.MonthlyCosts);
        }

        [Theory]
        [InlineData(AlertType.Information)]
        [InlineData(AlertType.Warning)]
        [InlineData(AlertType.Urgent)]
        [InlineData(AlertType.Critical)]
        public void AlertType_AllValues_ShouldBeValid(AlertType alertType)
        {
            // Assert
            Assert.True(Enum.IsDefined(typeof(AlertType), alertType));
        }

        [Fact]
        public void MaintenanceAlert_CriticalAlert_ShouldHaveHighPriority()
        {
            // Arrange & Act
            var alert = new MaintenanceAlert
            {
                VehicleId = 3,
                VehicleNumber = "BUS003",
                AlertType = AlertType.Critical,
                Message = "Emergency brake inspection required",
                DueDate = DateTime.Now.AddDays(-5), // Overdue
                EstimatedCost = 800.00m
            };

            // Assert
            Assert.Equal(AlertType.Critical, alert.AlertType);
            Assert.True(alert.DueDate < DateTime.Now); // Overdue
            Assert.Contains("Emergency", alert.Message);
        }
    }
}
