using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{
    /// <summary>
    /// Service for analyzing route efficiency and providing optimization insights
    /// Supports the roadmap goal of analytics for future web dashboard
    /// </summary>
    public class RouteAnalyticsService : IRouteAnalyticsService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IFuelRepository _fuelRepository;

        public RouteAnalyticsService(
            IRouteRepository? routeRepository = null,
            IVehicleRepository? vehicleRepository = null,
            IDriverRepository? driverRepository = null,
            IFuelRepository? fuelRepository = null)
        {
            _routeRepository = routeRepository ?? new RouteRepository();
            _vehicleRepository = vehicleRepository ?? new VehicleRepository();
            _driverRepository = driverRepository ?? new DriverRepository();
            _fuelRepository = fuelRepository ?? new FuelRepository();
        }

        /// <summary>
        /// Calculate comprehensive efficiency metrics for a route
        /// </summary>
        public RouteEfficiencyMetrics? CalculateRouteEfficiency(Route? route)
        {
            // Return null for null routes
            if (route == null)
                return null;

            // Return null for routes with no meaningful data
            if (route.RouteID <= 0 &&
                string.IsNullOrWhiteSpace(route.RouteName) &&
                !route.AMVehicleID.HasValue &&
                !route.PMVehicleID.HasValue &&
                !route.AMDriverID.HasValue &&
                !route.PMDriverID.HasValue &&
                (!route.AMBeginMiles.HasValue || !route.AMEndMiles.HasValue) &&
                (!route.PMBeginMiles.HasValue || !route.PMEndMiles.HasValue))
            {
                return null;
            }

            var metrics = new RouteEfficiencyMetrics
            {
                RouteId = route.RouteID,
                RouteName = route.RouteName ?? "Unknown",
                Date = route.DateAsDateTime,

                // AM Period Metrics
                AMTotalMiles = CalculatePeriodMiles(route.AMBeginMiles, route.AMEndMiles),
                AMRiders = route.AMRiders ?? 0,
                AMVehicleId = route.AMVehicleID,
                AMDriverId = route.AMDriverID,

                // PM Period Metrics
                PMTotalMiles = CalculatePeriodMiles(route.PMBeginMiles, route.PMEndMiles),
                PMRiders = route.PMRiders ?? 0,
                PMVehicleId = route.PMVehicleID,
                PMDriverId = route.PMDriverID
            };

            // Calculate derived metrics
            metrics.TotalMiles = metrics.AMTotalMiles + metrics.PMTotalMiles;
            metrics.TotalRiders = metrics.AMRiders + metrics.PMRiders;

            // Efficiency calculations
            if (metrics.TotalRiders > 0)
            {
                metrics.MilesPerRider = Math.Round(metrics.TotalMiles / metrics.TotalRiders, 2);
                metrics.EfficiencyScore = CalculateEfficiencyScore(metrics);
            }

            // Cost estimates (basic calculation - could be enhanced with fuel data)
            metrics.EstimatedFuelCost = CalculateEstimatedFuelCost(metrics.TotalMiles);

            return metrics;
        }

        /// <summary>
        /// Get efficiency metrics for routes in a date range
        /// </summary>
        public async Task<List<RouteEfficiencyMetrics>> GetRouteEfficiencyMetricsAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var metrics = new List<RouteEfficiencyMetrics>();
                var currentDate = startDate.Date;

                while (currentDate <= endDate.Date)
                {
                    var routes = _routeRepository.GetRoutesByDate(currentDate);
                    foreach (var route in routes)
                    {
                        var routeMetrics = CalculateRouteEfficiency(route);
                        if (routeMetrics != null)
                        {
                            metrics.Add(routeMetrics);
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }

                return metrics.OrderBy(m => m.Date).ThenBy(m => m.RouteName).ToList();
            });
        }

        /// <summary>
        /// Analyze potential route optimizations for a specific date
        /// </summary>
        public async Task<List<RouteOptimizationSuggestion>> AnalyzeRouteOptimizationsAsync(DateTime date)
        {
            return await Task.Run(() =>
            {
                var suggestions = new List<RouteOptimizationSuggestion>();
                var routes = _routeRepository.GetRoutesByDate(date);
                var metrics = routes.Select(CalculateRouteEfficiency).Where(m => m != null).Cast<RouteEfficiencyMetrics>().ToList();

                // Find routes with low efficiency
                var inefficientRoutes = metrics.Where(m => m.EfficiencyScore < 60).ToList();
                foreach (var route in inefficientRoutes)
                {
                    suggestions.Add(new RouteOptimizationSuggestion
                    {
                        RouteId = route.RouteId,
                        RouteName = route.RouteName,
                        SuggestionType = OptimizationType.EfficiencyImprovement,
                        Description = $"Route efficiency score is {route.EfficiencyScore:F1}%. Consider consolidating stops or adjusting route path.",
                        PotentialSavings = CalculatePotentialSavings(route),
                        Priority = route.EfficiencyScore < 40 ? Priority.High : Priority.Medium
                    });
                }

                // Find routes with high miles per rider
                var highMileageRoutes = metrics.Where(m => m.MilesPerRider > 5.0).ToList();
                foreach (var route in highMileageRoutes)
                {
                    suggestions.Add(new RouteOptimizationSuggestion
                    {
                        RouteId = route.RouteId,
                        RouteName = route.RouteName,
                        SuggestionType = OptimizationType.MileageReduction,
                        Description = $"High miles per rider ({route.MilesPerRider:F1}). Consider route consolidation.",
                        PotentialSavings = (decimal)route.MilesPerRider * 2.5m, // Estimated savings in dollars
                        Priority = Priority.Medium
                    });
                }

                // Check for vehicle utilization issues
                var vehicleUtilization = AnalyzeVehicleUtilization(date);
                foreach (var suggestion in vehicleUtilization)
                {
                    suggestions.Add(suggestion);
                }

                return suggestions.OrderByDescending(s => s.Priority).ThenByDescending(s => s.PotentialSavings).ToList();
            });
        }

        /// <summary>
        /// Calculate comprehensive driver performance metrics
        /// </summary>
        public async Task<DriverPerformanceMetrics> CalculateDriverPerformanceAsync(int driverId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var driver = _driverRepository.GetDriverById(driverId);
                if (driver == null)
                    throw new ArgumentException($"Driver with ID {driverId} not found");

                var metrics = new DriverPerformanceMetrics
                {
                    DriverId = driverId,
                    DriverName = driver.Name,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };

                // Get all routes for this driver in the period
                var allRoutes = new List<Route>();
                var currentDate = startDate.Date;
                while (currentDate <= endDate.Date)
                {
                    var dayRoutes = _routeRepository.GetRoutesByDate(currentDate)
                        .Where(r => r.AMDriverID == driverId || r.PMDriverID == driverId);
                    allRoutes.AddRange(dayRoutes);
                    currentDate = currentDate.AddDays(1);
                }

                // Calculate metrics
                metrics.TotalRoutes = allRoutes.Count;
                metrics.TotalMiles = allRoutes.Sum(r =>
                    CalculatePeriodMiles(r.AMBeginMiles, r.AMEndMiles) +
                    CalculatePeriodMiles(r.PMBeginMiles, r.PMEndMiles));

                metrics.TotalRiders = allRoutes.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));

                if (metrics.TotalRoutes > 0)
                {
                    metrics.AverageMilesPerRoute = Math.Round(metrics.TotalMiles / metrics.TotalRoutes, 2);
                    metrics.AverageRidersPerRoute = Math.Round((double)metrics.TotalRiders / metrics.TotalRoutes, 1);
                }

                // Calculate efficiency score
                var routeMetrics = allRoutes.Select(CalculateRouteEfficiency).Where(rm => rm != null).Cast<RouteEfficiencyMetrics>();
                if (routeMetrics.Any())
                {
                    metrics.OverallEfficiencyScore = Math.Round(routeMetrics.Average(rm => rm.EfficiencyScore), 1);
                }

                // Performance rating
                metrics.PerformanceRating = CalculateDriverPerformanceRating(metrics);

                return metrics;
            });
        }

        /// <summary>
        /// Get comprehensive fleet analytics summary
        /// </summary>
        public async Task<FleetAnalyticsSummary> GetFleetAnalyticsSummaryAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var summary = new FleetAnalyticsSummary
                {
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };

                // Get all routes in period
                var allRoutes = new List<Route>();
                var currentDate = startDate.Date;
                while (currentDate <= endDate.Date)
                {
                    allRoutes.AddRange(_routeRepository.GetRoutesByDate(currentDate));
                    currentDate = currentDate.AddDays(1);
                }

                // Basic metrics
                summary.TotalRoutes = allRoutes.Count;
                summary.TotalMiles = allRoutes.Sum(r =>
                    CalculatePeriodMiles(r.AMBeginMiles, r.AMEndMiles) +
                    CalculatePeriodMiles(r.PMBeginMiles, r.PMEndMiles));
                summary.TotalRiders = allRoutes.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));

                // Efficiency metrics
                var routeMetrics = allRoutes.Select(CalculateRouteEfficiency).Where(rm => rm != null).Cast<RouteEfficiencyMetrics>().ToList();
                if (routeMetrics.Any())
                {
                    summary.AverageEfficiencyScore = Math.Round(routeMetrics.Average(rm => rm.EfficiencyScore), 1);
                    summary.AverageMilesPerRider = Math.Round(routeMetrics.Average(rm => rm.MilesPerRider), 2);
                }

                // Vehicle utilization
                var activeVehicles = allRoutes.SelectMany(r => new[] { r.AMVehicleID, r.PMVehicleID })
                    .Where(id => id.HasValue).Distinct().Count();
                var totalVehicles = _vehicleRepository.GetAllVehicles().Count;
                summary.VehicleUtilizationRate = totalVehicles > 0 ?
                    Math.Round((double)activeVehicles / totalVehicles * 100, 1) : 0;

                // Cost estimates
                summary.EstimatedFuelCosts = CalculateEstimatedFuelCost(summary.TotalMiles);

                // Top performing routes
                summary.TopPerformingRoutes = routeMetrics
                    .OrderByDescending(rm => rm.EfficiencyScore)
                    .Take(5)
                    .Select(rm => $"{rm.RouteName} ({rm.EfficiencyScore:F1}%)")
                    .ToList();

                return summary;
            });
        }

        #region Private Helper Methods

        private double CalculatePeriodMiles(decimal? beginMiles, decimal? endMiles)
        {
            if (!beginMiles.HasValue || !endMiles.HasValue || endMiles <= beginMiles)
                return 0;

            return (double)(endMiles.Value - beginMiles.Value);
        }

        private double CalculateEfficiencyScore(RouteEfficiencyMetrics metrics)
        {
            // Efficiency score based on riders per mile and other factors
            // Higher score = more efficient (more riders, fewer miles)

            if (metrics.TotalMiles <= 0) return 0;

            var baseScore = 50.0; // Base efficiency score

            // Bonus for rider density (riders per mile)
            var riderDensity = metrics.TotalRiders / Math.Max(metrics.TotalMiles, 1);
            var riderBonus = Math.Min(riderDensity * 20, 30); // Max 30 point bonus

            // Penalty for very long routes with few riders
            var lengthPenalty = metrics.TotalMiles > 50 && metrics.TotalRiders < 10 ? 20 : 0;

            var score = baseScore + riderBonus - lengthPenalty;
            return Math.Max(0, Math.Min(100, score)); // Clamp between 0-100
        }

        private decimal CalculateEstimatedFuelCost(double totalMiles)
        {
            // Basic fuel cost estimation
            // Assumes ~6 MPG for school bus and $3.50/gallon
            const double mpg = 6.0;
            const decimal costPerGallon = 3.50m;

            if (totalMiles <= 0) return 0;

            var gallonsUsed = totalMiles / mpg;
            return Math.Round((decimal)gallonsUsed * costPerGallon, 2);
        }

        private decimal CalculatePotentialSavings(RouteEfficiencyMetrics route)
        {
            // Estimate potential savings from route optimization
            var inefficiencyFactor = (100 - route.EfficiencyScore) / 100;
            var potentialMilesSaved = route.TotalMiles * inefficiencyFactor * 0.3; // 30% of inefficient miles
            return CalculateEstimatedFuelCost(potentialMilesSaved);
        }

        private List<RouteOptimizationSuggestion> AnalyzeVehicleUtilization(DateTime date)
        {
            var suggestions = new List<RouteOptimizationSuggestion>();
            var routes = _routeRepository.GetRoutesByDate(date);

            // Find vehicles used only once
            var vehicleUsage = new Dictionary<int, int>();
            foreach (var route in routes)
            {
                if (route.AMVehicleID.HasValue)
                    vehicleUsage[route.AMVehicleID.Value] = vehicleUsage.GetValueOrDefault(route.AMVehicleID.Value) + 1;
                if (route.PMVehicleID.HasValue && route.PMVehicleID != route.AMVehicleID)
                    vehicleUsage[route.PMVehicleID.Value] = vehicleUsage.GetValueOrDefault(route.PMVehicleID.Value) + 1;
            }

            var underutilizedVehicles = vehicleUsage.Where(kv => kv.Value == 1).ToList();
            if (underutilizedVehicles.Any())
            {
                suggestions.Add(new RouteOptimizationSuggestion
                {
                    RouteId = 0,
                    RouteName = "Fleet Utilization",
                    SuggestionType = OptimizationType.VehicleUtilization,
                    Description = $"{underutilizedVehicles.Count} vehicles used only once. Consider route consolidation.",
                    PotentialSavings = underutilizedVehicles.Count * 25m, // Estimated savings per unused vehicle
                    Priority = Priority.Medium
                });
            }

            return suggestions;
        }

        private PerformanceRating CalculateDriverPerformanceRating(DriverPerformanceMetrics metrics)
        {
            if (metrics.OverallEfficiencyScore >= 80) return PerformanceRating.Excellent;
            if (metrics.OverallEfficiencyScore >= 70) return PerformanceRating.Good;
            if (metrics.OverallEfficiencyScore >= 60) return PerformanceRating.Average;
            if (metrics.OverallEfficiencyScore >= 50) return PerformanceRating.BelowAverage;
            return PerformanceRating.NeedsImprovement;
        }

        #endregion
    }
}
