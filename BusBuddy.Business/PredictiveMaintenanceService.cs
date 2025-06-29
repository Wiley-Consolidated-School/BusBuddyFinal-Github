using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Data;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Service for predictive maintenance scheduling and bus health analysis
    /// Provides intelligent maintenance recommendations based on usage patterns and history
    /// </summary>
    public class PredictiveMaintenanceService : IPredictiveMaintenanceService
    {
        private readonly BusRepository _busRepository;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IFuelRepository _fuelRepository;
        private readonly IRouteRepository _routeRepository;

        public PredictiveMaintenanceService(
            BusRepository? busRepository = null,
            IMaintenanceRepository? maintenanceRepository = null,
            IFuelRepository? fuelRepository = null,
            IRouteRepository? routeRepository = null)
        {
            _busRepository = busRepository ?? new BusRepository();
            _maintenanceRepository = maintenanceRepository ?? new MaintenanceRepository();
            _fuelRepository = fuelRepository ?? new FuelRepository();
            _routeRepository = routeRepository ?? new RouteRepository();
        }

        /// <summary>
        /// Generate maintenance predictions based on bus usage and history
        /// </summary>
        public async Task<List<MaintenancePrediction>> GetMaintenancePredictionsAsync(int busId)
        {
            if (busId <= 0)
                throw new ArgumentException($"Invalid bus ID: {busId}");
            try
            {
                return await Task.Run(() =>
                {
                    var bus = _busRepository.GetBusById(busId);
                    if (bus == null)
                        throw new ArgumentException($"Bus with ID {busId} not found");
                    var predictions = new List<MaintenancePrediction>();
                    var maintenanceHistory = _maintenanceRepository.GetMaintenanceByBus(busId);
                    var currentMileage = GetCurrentMileage(busId);
                    // Oil Change Prediction
                    var lastOilChange = GetLastMaintenanceOfType(maintenanceHistory, "Oil Change");
                    var oilChangePrediction = PredictOilChange(busId, lastOilChange, currentMileage);
                    if (oilChangePrediction != null)
                        predictions.Add(oilChangePrediction);
                    // Brake Inspection Prediction
                    var lastBrakeService = GetLastMaintenanceOfType(maintenanceHistory, "Brake");
                    var brakePrediction = PredictBrakeService(busId, lastBrakeService, currentMileage);
                    if (brakePrediction != null)
                        predictions.Add(brakePrediction);
                    // Tire Rotation/Replacement
                    var lastTireService = GetLastMaintenanceOfType(maintenanceHistory, "Tire");
                    var tirePrediction = PredictTireService(busId, lastTireService, currentMileage);
                    if (tirePrediction != null)
                        predictions.Add(tirePrediction);
                    // Annual Inspection
                    var inspectionPrediction = PredictAnnualInspection(bus);
                    if (inspectionPrediction != null)
                        predictions.Add(inspectionPrediction);
                    // Engine Service based on age and mileage
                    var enginePrediction = PredictEngineService(bus, maintenanceHistory, currentMileage);
                    if (enginePrediction != null)
                        predictions.Add(enginePrediction);
                    return predictions.OrderBy(p => p.PredictedDate).ToList();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting maintenance predictions for bus {busId}: {ex.Message}");
                throw new ApplicationException($"Failed to get maintenance predictions: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Generate fleet-wide maintenance schedule recommendations
        /// </summary>
        public async Task<List<MaintenanceRecommendation>> GetFleetMaintenanceScheduleAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var recommendations = new List<MaintenanceRecommendation>();
                var buses = _busRepository.GetAllBuses();
                foreach (var bus in buses)
                {
                    var predictions = GetMaintenancePredictionsAsync(bus.BusId).Result;
                    var periodicPredictions = predictions
                        .Where(p => p.PredictedDate >= startDate && p.PredictedDate <= endDate);
                    foreach (var prediction in periodicPredictions)
                    {
                        recommendations.Add(new MaintenanceRecommendation
                        {
                            BusId = bus.BusId,
                            BusNumber = bus.BusNumber ?? "Unknown",
                            MaintenanceType = prediction.MaintenanceType,
                            RecommendedDate = prediction.PredictedDate,
                            Priority = prediction.Priority,
                            EstimatedCost = prediction.EstimatedCost,
                            Reason = prediction.Reason,
                            CurrentMileage = GetCurrentMileage(bus.BusId)
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
        /// Calculate comprehensive bus health score
        /// </summary>
        public async Task<BusHealthScore> CalculateBusHealthScoreAsync(int busId)
        {
            return await Task.Run(() =>
            {
                var bus = _busRepository.GetBusById(busId);
                if (bus == null)
                    throw new ArgumentException($"Bus with ID {busId} not found");
                var healthScore = new BusHealthScore
                {
                    BusId = busId,
                    BusNumber = bus.BusNumber ?? "Unknown",
                    CalculatedDate = DateTime.Now
                };
                var maintenanceHistory = _maintenanceRepository.GetMaintenanceByBus(busId);
                var currentMileage = GetCurrentMileage(busId);
                // Calculate component scores
                healthScore.MaintenanceComplianceScore = CalculateMaintenanceComplianceScore(maintenanceHistory);
                healthScore.AgeScore = CalculateAgeScore(bus.Year ?? DateTime.Now.Year);
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
                    >= 85 => BusHealthStatus.Excellent,
                    >= 70 => BusHealthStatus.Good,
                    >= 55 => BusHealthStatus.Fair,
                    >= 40 => BusHealthStatus.Poor,
                    _ => BusHealthStatus.Critical
                };
                // Generate recommendations
                healthScore.Recommendations = GenerateHealthRecommendations(healthScore, maintenanceHistory);
                return healthScore;
            });
        }

        /// <summary>
        /// Analyze maintenance cost trends and provide insights
        /// </summary>
        public async Task<MaintenanceCostAnalysis> AnalyzeMaintenanceCostsAsync(int busId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var bus = _busRepository.GetBusById(busId);
                if (bus == null)
                    throw new ArgumentException($"Bus with ID {busId} not found");
                var maintenanceRecords = _maintenanceRepository.GetMaintenanceByBus(busId)
                    .Where(m => m.DateAsDateTime >= startDate && m.DateAsDateTime <= endDate)
                    .ToList();
                var analysis = new MaintenanceCostAnalysis
                {
                    BusId = busId,
                    BusNumber = bus.BusNumber ?? "Unknown",
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
                var buses = _busRepository.GetAllBuses();
                foreach (var bus in buses)
                {
                    List<MaintenancePrediction> predictions;
                    try
                    {
                        predictions = GetMaintenancePredictionsAsync(bus.BusId).Result;
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
                            BusId = bus.BusId,
                            BusNumber = bus.BusNumber ?? "Unknown",
                            AlertType = prediction.Priority == MaintenancePriority.Critical ?
                                AlertType.Critical : AlertType.Urgent,
                            Message = $"{prediction.MaintenanceType} due for {bus.BusNumber}",
                            DueDate = prediction.PredictedDate,
                            EstimatedCost = prediction.EstimatedCost
                        });
                    }
                    // Check for overdue inspections
                    if (bus.LastInspectionDate.HasValue)
                    {
                        var daysSinceInspection = (DateTime.Now - bus.LastInspectionDate.Value).Days;
                        if (daysSinceInspection > 365)
                        {
                            alerts.Add(new MaintenanceAlert
                            {
                                BusId = bus.BusId,
                                BusNumber = bus.BusNumber ?? "Unknown",
                                AlertType = AlertType.Critical,
                                Message = $"Annual inspection overdue by {daysSinceInspection - 365} days",
                                DueDate = bus.LastInspectionDate.Value.AddDays(365),
                                EstimatedCost = 150m
                            });
                        }
                    }
                }
                return alerts.OrderBy(a => a.AlertType).ThenBy(a => a.DueDate).ToList();
            });
        }

        #region Private Helper Methods

        private decimal GetCurrentMileage(int busId)
        {
            // Get latest mileage from fuel records or routes
            var latestFuel = _fuelRepository.GetFuelRecordsByBus(busId)
                .OrderByDescending(f => f.FuelDateAsDateTime)
                .FirstOrDefault();
            if (latestFuel?.VehicleOdometerReading.HasValue == true)
                return latestFuel.VehicleOdometerReading.Value;
            // Fallback to route mileage if available
            var routes = _routeRepository.GetAllRoutes()
                .Where(r => r.AMBusId == busId || r.PMBusId == busId)
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

        private MaintenancePrediction? PredictOilChange(int busId, Maintenance? lastOilChange, decimal currentMileage)
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
                    BusId = busId,
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

        private MaintenancePrediction? PredictBrakeService(int busId, Maintenance? lastBrakeService, decimal currentMileage)
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
                    BusId = busId,
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

        private MaintenancePrediction? PredictTireService(int busId, Maintenance? lastTireService, decimal currentMileage)
        {
            const int tireRotationIntervalMiles = 8000;
            var milesSinceLastService = lastTireService?.OdometerReading != null ?
                currentMileage - (decimal)lastTireService.OdometerReading : currentMileage;
            if (milesSinceLastService >= tireRotationIntervalMiles)
            {
                return new MaintenancePrediction
                {
                    BusId = busId,
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

        private MaintenancePrediction? PredictAnnualInspection(Bus bus)
        {
            if (bus.LastInspectionDate.HasValue)
            {
                var daysSinceInspection = (DateTime.Now - bus.LastInspectionDate.Value).Days;
                var nextInspectionDue = bus.LastInspectionDate.Value.AddDays(365);
                if (daysSinceInspection >= 330) // 11 months
                {
                    var priority = daysSinceInspection >= 365 ?
                        MaintenancePriority.Critical : MaintenancePriority.High;
                    return new MaintenancePrediction
                    {
                        BusId = bus.BusId,
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

        private MaintenancePrediction? PredictEngineService(Bus bus, List<Maintenance> history, decimal currentMileage)
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
                    BusId = bus.BusId,
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

        private List<string> GenerateHealthRecommendations(BusHealthScore healthScore, List<Maintenance> history)
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

