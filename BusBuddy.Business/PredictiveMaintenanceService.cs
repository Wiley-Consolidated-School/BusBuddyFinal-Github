using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    /// <summary>
    /// Service for predictive maintenance scheduling and vehicle health analysis
    /// Provides intelligent maintenance recommendations based on usage patterns and history
    /// </summary>
    public class PredictiveMaintenanceService : IPredictiveMaintenanceService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IFuelRepository _fuelRepository;
        private readonly IRouteRepository _routeRepository;

        public PredictiveMaintenanceService(
            IVehicleRepository? vehicleRepository = null,
            IMaintenanceRepository? maintenanceRepository = null,
            IFuelRepository? fuelRepository = null,
            IRouteRepository? routeRepository = null)
        {
            _vehicleRepository = vehicleRepository ?? new VehicleRepository();
            _maintenanceRepository = maintenanceRepository ?? new MaintenanceRepository();
            _fuelRepository = fuelRepository ?? new FuelRepository();
            _routeRepository = routeRepository ?? new RouteRepository();
        }

        /// <summary>
        /// Generate maintenance predictions based on vehicle usage and history
        /// </summary>
        public async Task<List<MaintenancePrediction>> GetMaintenancePredictionsAsync(int vehicleId)
        {
            if (vehicleId <= 0)
                throw new ArgumentException($"Invalid vehicle ID: {vehicleId}");
            return await Task.Run(() =>
            {
                var vehicle = _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with ID {vehicleId} not found");

                var predictions = new List<MaintenancePrediction>();
                var maintenanceHistory = _maintenanceRepository.GetMaintenanceByVehicle(vehicleId);
                var currentMileage = GetCurrentMileage(vehicleId);

                // Oil Change Prediction
                var lastOilChange = GetLastMaintenanceOfType(maintenanceHistory, "Oil Change");
                var oilChangePrediction = PredictOilChange(vehicleId, lastOilChange, currentMileage);
                if (oilChangePrediction != null)
                    predictions.Add(oilChangePrediction);

                // Brake Inspection Prediction
                var lastBrakeService = GetLastMaintenanceOfType(maintenanceHistory, "Brake");
                var brakePrediction = PredictBrakeService(vehicleId, lastBrakeService, currentMileage);
                if (brakePrediction != null)
                    predictions.Add(brakePrediction);

                // Tire Rotation/Replacement
                var lastTireService = GetLastMaintenanceOfType(maintenanceHistory, "Tire");
                var tirePrediction = PredictTireService(vehicleId, lastTireService, currentMileage);
                if (tirePrediction != null)
                    predictions.Add(tirePrediction);

                // Annual Inspection
                var inspectionPrediction = PredictAnnualInspection(vehicle);
                if (inspectionPrediction != null)
                    predictions.Add(inspectionPrediction);

                // Engine Service based on age and mileage
                var enginePrediction = PredictEngineService(vehicle, maintenanceHistory, currentMileage);
                if (enginePrediction != null)
                    predictions.Add(enginePrediction);

                return predictions.OrderBy(p => p.PredictedDate).ToList();
            });
        }

        /// <summary>
        /// Generate fleet-wide maintenance schedule recommendations
        /// </summary>
        public async Task<List<MaintenanceRecommendation>> GetFleetMaintenanceScheduleAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var recommendations = new List<MaintenanceRecommendation>();
                var vehicles = _vehicleRepository.GetAllVehicles();

                foreach (var vehicle in vehicles)
                {
                    var predictions = GetMaintenancePredictionsAsync(vehicle.Id).Result;
                    var periodicPredictions = predictions
                        .Where(p => p.PredictedDate >= startDate && p.PredictedDate <= endDate);

                    foreach (var prediction in periodicPredictions)
                    {
                        recommendations.Add(new MaintenanceRecommendation
                        {
                            VehicleId = vehicle.Id,
                            VehicleNumber = vehicle.VehicleNumber ?? "Unknown",
                            MaintenanceType = prediction.MaintenanceType,
                            RecommendedDate = prediction.PredictedDate,
                            Priority = prediction.Priority,
                            EstimatedCost = prediction.EstimatedCost,
                            Reason = prediction.Reason,
                            CurrentMileage = GetCurrentMileage(vehicle.Id)
                        });
                    }
                }

                // Group by week for better scheduling
                return recommendations
                    .OrderBy(r => r.Priority)
                    .ThenBy(r => r.RecommendedDate)
                    .ToList();
            });
        }

        /// <summary>
        /// Calculate comprehensive vehicle health score
        /// </summary>
        public async Task<VehicleHealthScore> CalculateVehicleHealthScoreAsync(int vehicleId)
        {
            return await Task.Run(() =>
            {
                var vehicle = _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with ID {vehicleId} not found");

                var healthScore = new VehicleHealthScore
                {
                    VehicleId = vehicleId,
                    VehicleNumber = vehicle.VehicleNumber ?? "Unknown",
                    CalculatedDate = DateTime.Now
                };

                var maintenanceHistory = _maintenanceRepository.GetMaintenanceByVehicle(vehicleId);
                var currentMileage = GetCurrentMileage(vehicleId);

                // Calculate component scores
                healthScore.MaintenanceComplianceScore = CalculateMaintenanceComplianceScore(maintenanceHistory);
                healthScore.AgeScore = CalculateAgeScore(vehicle.Year);
                healthScore.MileageScore = CalculateMileageScore(currentMileage);
                healthScore.ReliabilityScore = CalculateReliabilityScore(maintenanceHistory);
                healthScore.CostEfficiencyScore = CalculateCostEfficiencyScore(maintenanceHistory);

                // Overall score (weighted average)
                healthScore.OverallScore = (int)Math.Round(
                    (healthScore.MaintenanceComplianceScore * 0.25) +
                    (healthScore.AgeScore * 0.15) +
                    (healthScore.MileageScore * 0.20) +
                    (healthScore.ReliabilityScore * 0.30) +
                    (healthScore.CostEfficiencyScore * 0.10)
                );

                // Health status based on score
                healthScore.HealthStatus = healthScore.OverallScore switch
                {
                    >= 85 => VehicleHealthStatus.Excellent,
                    >= 70 => VehicleHealthStatus.Good,
                    >= 55 => VehicleHealthStatus.Fair,
                    >= 40 => VehicleHealthStatus.Poor,
                    _ => VehicleHealthStatus.Critical
                };

                // Generate recommendations
                healthScore.Recommendations = GenerateHealthRecommendations(healthScore, maintenanceHistory);

                return healthScore;
            });
        }

        /// <summary>
        /// Analyze maintenance cost trends and provide insights
        /// </summary>
        public async Task<MaintenanceCostAnalysis> AnalyzeMaintenanceCostsAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var vehicle = _vehicleRepository.GetVehicleById(vehicleId);
                if (vehicle == null)
                    throw new ArgumentException($"Vehicle with ID {vehicleId} not found");

                var maintenanceRecords = _maintenanceRepository.GetMaintenanceByVehicle(vehicleId)
                    .Where(m => m.DateAsDateTime >= startDate && m.DateAsDateTime <= endDate)
                    .ToList();

                var analysis = new MaintenanceCostAnalysis
                {
                    VehicleId = vehicleId,
                    VehicleNumber = vehicle.VehicleNumber ?? "Unknown",
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };

                // Basic metrics
                analysis.TotalCost = maintenanceRecords.Sum(m => m.RepairCost ?? 0);
                analysis.AverageCostPerService = maintenanceRecords.Any() ?
                    analysis.TotalCost / maintenanceRecords.Count : 0;
                analysis.ServiceCount = maintenanceRecords.Count;

                // Cost by category
                analysis.CostByCategory = maintenanceRecords
                    .GroupBy(m => m.MaintenanceCompleted ?? "Other")
                    .ToDictionary(g => g.Key, g => g.Sum(m => m.RepairCost ?? 0));

                // Monthly trends
                analysis.MonthlyCosts = maintenanceRecords
                    .GroupBy(m => new { Year = m.DateAsDateTime?.Year ?? 0, Month = m.DateAsDateTime?.Month ?? 0 })
                    .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:00}", g => g.Sum(m => m.RepairCost ?? 0));

                // Cost predictions
                analysis.ProjectedAnnualCost = CalculateProjectedAnnualCost(maintenanceRecords);
                analysis.CostTrend = CalculateCostTrend(maintenanceRecords);

                return analysis;
            });
        }

        /// <summary>
        /// Get high-priority maintenance alerts
        /// </summary>
        public async Task<List<MaintenanceAlert>> GetMaintenanceAlertsAsync()
        {
            return await Task.Run(() =>
            {
                var alerts = new List<MaintenanceAlert>();
                var vehicles = _vehicleRepository.GetAllVehicles();

                foreach (var vehicle in vehicles)
                {
                    List<MaintenancePrediction> predictions;
                    try
                    {
                        predictions = GetMaintenancePredictionsAsync(vehicle.Id).Result;
                    }
                    catch
                    {
                        continue; // Skip vehicles that cause errors
                    }
                    var urgentPredictions = predictions
                        .Where(p => p.Priority == MaintenancePriority.Critical ||
                                   (p.Priority == MaintenancePriority.High && p.PredictedDate <= DateTime.Now.AddDays(7)));

                    foreach (var prediction in urgentPredictions)
                    {
                        alerts.Add(new MaintenanceAlert
                        {
                            VehicleId = vehicle.Id,
                            VehicleNumber = vehicle.VehicleNumber ?? "Unknown",
                            AlertType = prediction.Priority == MaintenancePriority.Critical ?
                                AlertType.Critical : AlertType.Urgent,
                            Message = $"{prediction.MaintenanceType} due for {vehicle.VehicleNumber}",
                            DueDate = prediction.PredictedDate,
                            EstimatedCost = prediction.EstimatedCost
                        });
                    }

                    // Check for overdue inspections
                    if (vehicle.DateLastInspectionAsDateTime.HasValue)
                    {
                        var daysSinceInspection = (DateTime.Now - vehicle.DateLastInspectionAsDateTime.Value).Days;
                        if (daysSinceInspection > 365)
                        {
                            alerts.Add(new MaintenanceAlert
                            {
                                VehicleId = vehicle.Id,
                                VehicleNumber = vehicle.VehicleNumber ?? "Unknown",
                                AlertType = AlertType.Critical,
                                Message = $"Annual inspection overdue by {daysSinceInspection - 365} days",
                                DueDate = vehicle.DateLastInspectionAsDateTime.Value.AddDays(365),
                                EstimatedCost = 150m
                            });
                        }
                    }
                }

                return alerts.OrderBy(a => a.AlertType).ThenBy(a => a.DueDate).ToList();
            });
        }

        #region Private Helper Methods

        private decimal GetCurrentMileage(int vehicleId)
        {
            // Get latest mileage from fuel records or routes
            var latestFuel = _fuelRepository.GetFuelRecordsByVehicle(vehicleId)
                .OrderByDescending(f => f.FuelDateAsDateTime)
                .FirstOrDefault();

            if (latestFuel?.VehicleOdometerReading.HasValue == true)
                return latestFuel.VehicleOdometerReading.Value;

            // Fallback to route mileage if available
            var routes = _routeRepository.GetAllRoutes()
                .Where(r => r.AMVehicleID == vehicleId || r.PMVehicleID == vehicleId)
                .OrderByDescending(r => r.DateAsDateTime);

            var latestRoute = routes.FirstOrDefault();
            if (latestRoute != null)
            {
                var maxMileage = Math.Max(latestRoute.AMEndMiles ?? 0, latestRoute.PMEndMiles ?? 0);
                return maxMileage;
            }

            return 0;
        }

        private Maintenance? GetLastMaintenanceOfType(List<Maintenance> history, string type)
        {
            return history
                .Where(m => m.MaintenanceCompleted?.Contains(type, StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(m => m.DateAsDateTime)
                .FirstOrDefault();
        }

        private MaintenancePrediction? PredictOilChange(int vehicleId, Maintenance? lastOilChange, decimal currentMileage)
        {
            const int oilChangeIntervalMiles = 5000;
            const int oilChangeIntervalDays = 180; // 6 months

            var milesSinceLastOilChange = lastOilChange?.OdometerReading != null ?
                currentMileage - (decimal)lastOilChange.OdometerReading : currentMileage;

            var daysSinceLastOilChange = lastOilChange?.DateAsDateTime != null ?
                (DateTime.Now - lastOilChange.DateAsDateTime.Value).Days : 365;

            var mileageBasedDue = milesSinceLastOilChange >= oilChangeIntervalMiles;
            var timeBasedDue = daysSinceLastOilChange >= oilChangeIntervalDays;

            if (mileageBasedDue || timeBasedDue)
            {
                var priority = (milesSinceLastOilChange > oilChangeIntervalMiles + 1000 ||
                               daysSinceLastOilChange > oilChangeIntervalDays + 30) ?
                    MaintenancePriority.High : MaintenancePriority.Medium;

                return new MaintenancePrediction
                {
                    VehicleId = vehicleId,
                    MaintenanceType = "Oil Change",
                    PredictedDate = DateTime.Now.AddDays(priority == MaintenancePriority.High ? 7 : 14),
                    Priority = priority,
                    EstimatedCost = 75m,
                    Reason = $"Due - {milesSinceLastOilChange:N0} miles since last service",
                    BasedOnMileage = true,
                    PredictedMileage = (int)currentMileage
                };
            }

            return null;
        }

        private MaintenancePrediction? PredictBrakeService(int vehicleId, Maintenance? lastBrakeService, decimal currentMileage)
        {
            const int brakeServiceIntervalMiles = 25000;

            var milesSinceLastService = lastBrakeService?.OdometerReading != null ?
                currentMileage - (decimal)lastBrakeService.OdometerReading : currentMileage;

            if (milesSinceLastService >= (decimal)(brakeServiceIntervalMiles * 0.8)) // 80% threshold
            {
                var priority = milesSinceLastService >= (decimal)brakeServiceIntervalMiles ?
                    MaintenancePriority.High : MaintenancePriority.Medium;

                return new MaintenancePrediction
                {
                    VehicleId = vehicleId,
                    MaintenanceType = "Brake Inspection",
                    PredictedDate = DateTime.Now.AddDays(priority == MaintenancePriority.High ? 14 : 30),
                    Priority = priority,
                    EstimatedCost = 200m,
                    Reason = $"Brake service due - {milesSinceLastService:N0} miles since last service",
                    BasedOnMileage = true,
                    PredictedMileage = (int)currentMileage
                };
            }

            return null;
        }

        private MaintenancePrediction? PredictTireService(int vehicleId, Maintenance? lastTireService, decimal currentMileage)
        {
            const int tireRotationIntervalMiles = 8000;

            var milesSinceLastService = lastTireService?.OdometerReading != null ?
                currentMileage - (decimal)lastTireService.OdometerReading : currentMileage;

            if (milesSinceLastService >= tireRotationIntervalMiles)
            {
                return new MaintenancePrediction
                {
                    VehicleId = vehicleId,
                    MaintenanceType = "Tire Rotation/Inspection",
                    PredictedDate = DateTime.Now.AddDays(21),
                    Priority = MaintenancePriority.Medium,
                    EstimatedCost = 100m,
                    Reason = $"Tire rotation due - {milesSinceLastService:N0} miles since last service",
                    BasedOnMileage = true,
                    PredictedMileage = (int)currentMileage
                };
            }

            return null;
        }

        private MaintenancePrediction? PredictAnnualInspection(Vehicle vehicle)
        {
            if (vehicle.DateLastInspectionAsDateTime.HasValue)
            {
                var daysSinceInspection = (DateTime.Now - vehicle.DateLastInspectionAsDateTime.Value).Days;
                var nextInspectionDue = vehicle.DateLastInspectionAsDateTime.Value.AddDays(365);

                if (daysSinceInspection >= 330) // 11 months
                {
                    var priority = daysSinceInspection >= 365 ?
                        MaintenancePriority.Critical : MaintenancePriority.High;

                    return new MaintenancePrediction
                    {
                        VehicleId = vehicle.Id,
                        MaintenanceType = "Annual Inspection",
                        PredictedDate = nextInspectionDue,
                        Priority = priority,
                        EstimatedCost = 150m,
                        Reason = $"Annual inspection due",
                        BasedOnMileage = false
                    };
                }
            }

            return null;
        }

        private MaintenancePrediction? PredictEngineService(Vehicle vehicle, List<Maintenance> history, decimal currentMileage)
        {
            var majorEngineService = history
                .Where(m => m.MaintenanceCompleted?.Contains("Engine", StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(m => m.DateAsDateTime)
                .FirstOrDefault();

            var milesSinceLastService = majorEngineService?.OdometerReading != null ?
                currentMileage - (decimal)majorEngineService.OdometerReading : currentMileage;

            const int engineServiceInterval = 100000; // Major service every 100k miles

            if (milesSinceLastService >= (decimal)(engineServiceInterval * 0.9)) // 90% threshold
            {
                var priority = milesSinceLastService >= (decimal)engineServiceInterval ?
                    MaintenancePriority.High : MaintenancePriority.Low;

                return new MaintenancePrediction
                {
                    VehicleId = vehicle.Id,
                    MaintenanceType = "Major Engine Service",
                    PredictedDate = DateTime.Now.AddDays(priority == MaintenancePriority.High ? 30 : 90),
                    Priority = priority,
                    EstimatedCost = 1500m,
                    Reason = $"Major engine service approaching - {milesSinceLastService:N0} miles since last service",
                    BasedOnMileage = true,
                    PredictedMileage = (int)currentMileage
                };
            }

            return null;
        }

        private int CalculateMaintenanceComplianceScore(List<Maintenance> history)
        {
            // Score based on maintenance frequency and regularity
            if (!history.Any()) return 50; // Neutral score for no history

            var recentMaintenanceCount = history.Count(m =>
                m.DateAsDateTime >= DateTime.Now.AddMonths(-12));

            // Good maintenance should have at least 3-4 services per year
            var score = Math.Min(100, (recentMaintenanceCount * 25));
            return Math.Max(10, score); // Minimum 10 points
        }

        private int CalculateAgeScore(int vehicleYear)
        {
            var age = DateTime.Now.Year - vehicleYear;

            return age switch
            {
                <= 2 => 100,
                <= 5 => 90,
                <= 10 => 75,
                <= 15 => 60,
                <= 20 => 40,
                _ => 20
            };
        }

        private int CalculateMileageScore(decimal currentMileage)
        {
            return (int)currentMileage switch
            {
                <= 50000 => 100,
                <= 100000 => 85,
                <= 150000 => 70,
                <= 200000 => 55,
                <= 300000 => 40,
                _ => 25
            };
        }

        private int CalculateReliabilityScore(List<Maintenance> history)
        {
            // Score based on frequency of breakdowns vs. preventive maintenance
            var recentMaintenance = history.Where(m =>
                m.DateAsDateTime >= DateTime.Now.AddMonths(-12));

            var breakdownKeywords = new[] { "breakdown", "emergency", "tow", "failure" };
            var breakdowns = recentMaintenance.Count(m =>
                breakdownKeywords.Any(keyword =>
                    m.Notes?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true));

            var totalServices = recentMaintenance.Count();

            if (totalServices == 0) return 70; // Neutral score

            var reliabilityRatio = 1.0 - ((double)breakdowns / totalServices);
            return (int)(reliabilityRatio * 100);
        }

        private int CalculateCostEfficiencyScore(List<Maintenance> history)
        {
            // Score based on cost reasonableness
            var recentMaintenance = history.Where(m =>
                m.DateAsDateTime >= DateTime.Now.AddMonths(-12) &&
                m.RepairCost.HasValue);

            if (!recentMaintenance.Any()) return 70; // Neutral score

            var avgCost = recentMaintenance.Average(m => m.RepairCost!.Value);

            // Reasonable annual maintenance cost ranges
            return avgCost switch
            {
                <= 200 => 100,  // Very reasonable
                <= 500 => 85,   // Reasonable
                <= 1000 => 70,  // Moderate
                <= 2000 => 55,  // Higher
                _ => 30          // Expensive
            };
        }

        private List<string> GenerateHealthRecommendations(VehicleHealthScore healthScore, List<Maintenance> history)
        {
            var recommendations = new List<string>();

            if (healthScore.MaintenanceComplianceScore < 60)
                recommendations.Add("Increase preventive maintenance frequency");

            if (healthScore.ReliabilityScore < 70)
                recommendations.Add("Focus on addressing recurring issues");

            if (healthScore.CostEfficiencyScore < 60)
                recommendations.Add("Review maintenance costs - consider different service providers");

            if (healthScore.OverallScore < 50)
                recommendations.Add("Consider vehicle replacement evaluation");

            return recommendations;
        }

        private decimal CalculateProjectedAnnualCost(List<Maintenance> maintenanceRecords)
        {
            if (!maintenanceRecords.Any()) return 0;

            var daysInPeriod = (maintenanceRecords.Max(m => m.DateAsDateTime) -
                               maintenanceRecords.Min(m => m.DateAsDateTime))?.Days ?? 365;

            var totalCost = maintenanceRecords.Sum(m => m.RepairCost ?? 0);

            return totalCost / daysInPeriod * 365;
        }

        private string CalculateCostTrend(List<Maintenance> maintenanceRecords)
        {
            if (maintenanceRecords.Count < 2) return "Insufficient data";

            var orderedRecords = maintenanceRecords.OrderBy(m => m.DateAsDateTime).ToList();
            var firstHalfCost = orderedRecords.Take(orderedRecords.Count / 2).Sum(m => m.RepairCost ?? 0);
            var secondHalfCost = orderedRecords.Skip(orderedRecords.Count / 2).Sum(m => m.RepairCost ?? 0);

            var trend = secondHalfCost - firstHalfCost;

            return trend switch
            {
                > 500 => "Increasing",
                < -500 => "Decreasing",
                _ => "Stable"
            };
        }

        #endregion
    }
}
