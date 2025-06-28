using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.Business
{    /// <summary>
    /// Service for analyzing route efficiency and providing optimization insights
    /// Supports the roadmap goal of analytics for future web dashboard
    /// </summary>
    public class RouteAnalyticsService : IRouteAnalyticsService
    {
        private readonly IRouteRepository _routeRepository;
        private readonly BusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IFuelRepository _fuelRepository;
        private readonly IActivityRepository _activityRepository;

        public RouteAnalyticsService(
            IRouteRepository? routeRepository = null,
            BusRepository? busRepository = null,
            IDriverRepository? driverRepository = null,
            IFuelRepository? fuelRepository = null,
            IActivityRepository? activityRepository = null)
        {
            _routeRepository = routeRepository ?? new RouteRepository();
            _busRepository = busRepository ?? new BusRepository();
            _driverRepository = driverRepository ?? new DriverRepository();
            _fuelRepository = fuelRepository ?? new FuelRepository();
            _activityRepository = activityRepository ?? new ActivityRepository();
        }

        /// <summary>
        /// Calculate comprehensive efficiency metrics for a route
        /// </summary>
        public RouteEfficiencyMetrics? CalculateRouteEfficiency(Route? route)
        {
            try
            {
                // Return null for null routes
                if (route == null)
                    return null;

                // Return null for routes with no meaningful data
                if (route.RouteId <= 0 &&
                    string.IsNullOrWhiteSpace(route.RouteName) &&
                    !route.AMBusId.HasValue &&
                    !route.PMBusId.HasValue &&
                    !route.AMDriverId.HasValue &&
                    !route.PMDriverId.HasValue &&
                (!route.AMBeginMiles.HasValue || !route.AMEndMiles.HasValue) &&
                (!route.PMBeginMiles.HasValue || !route.PMEndMiles.HasValue))
            {
                return null;
            }

            var metrics = new RouteEfficiencyMetrics
            {
                RouteId = route.RouteId,
                RouteName = route.RouteName ?? "Unknown",
                Date = route.DateAsDateTime,

                // AM Period Metrics
                AMTotalMiles = CalculatePeriodMiles(route.AMBeginMiles, route.AMEndMiles),
                AMRiders = route.AMRiders ?? 0,
                AMBusId = route.AMBusId,
                AMDriverId = route.AMDriverId,

                // PM Period Metrics
                PMTotalMiles = CalculatePeriodMiles(route.PMBeginMiles, route.PMEndMiles),
                PMRiders = route.PMRiders ?? 0,
                PMBusId = route.PMBusId,
                PMDriverId = route.PMDriverId
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
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating route efficiency for route {route?.RouteId}: {ex.Message}");
                throw new ApplicationException($"Failed to calculate route efficiency: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get efficiency metrics for routes in a date range
        /// </summary>
        public async Task<List<RouteEfficiencyMetrics>> GetRouteEfficiencyMetricsAsync(DateTime startDate, DateTime endDate)
        {
            // Add timeout protection
            using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2)))
            {
                return await Task.Run(() =>
                {
                    var metrics = new List<RouteEfficiencyMetrics>();

                    // Safety checks
                    if (startDate > endDate)
                    {
                        throw new ArgumentException("Start date cannot be after end date");
                    }

                    var daysDifference = (endDate.Date - startDate.Date).Days;
                    if (daysDifference > 365)
                    {
                        throw new ArgumentException("Date range cannot exceed 365 days");
                    }

                    var currentDate = startDate.Date;
                    var maxIterations = Math.Min(daysDifference + 1, 365); // Hard limit
                    var iterations = 0;

                    while (currentDate <= endDate.Date && iterations < maxIterations && !cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var routes = _routeRepository.GetRoutesByDate(currentDate);
                            foreach (var route in routes)
                            {
                                if (cts.Token.IsCancellationRequested) break;

                                var routeMetrics = CalculateRouteEfficiency(route);
                                if (routeMetrics != null)
                                {
                                    metrics.Add(routeMetrics);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log but continue with next day
                            Console.WriteLine($"⚠️ Error loading routes for {currentDate:yyyy-MM-dd}: {ex.Message}");
                        }

                        currentDate = currentDate.AddDays(1);
                        iterations++;

                        // Additional safety check
                        if (iterations >= maxIterations)
                        {
                            Console.WriteLine($"⚠️ Route efficiency loop reached max iterations ({maxIterations})");
                            break;
                        }
                    }

                    if (cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("⚠️ Route efficiency calculation timed out");
                    }

                    return metrics.OrderBy(m => m.Date).ThenBy(m => m.RouteName).ToList();
                }, cts.Token);
            }
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
        public async Task<DriverPerformanceMetrics> CalculateDriverPerformanceAsync(int DriverId, DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var driver = _driverRepository.GetDriverById(DriverId);
                if (driver == null)
                    throw new ArgumentException($"Driver with ID {DriverId} not found");

                var metrics = new DriverPerformanceMetrics
                {
                    DriverId = DriverId,
                    Name = driver.Name,
                    PeriodStart = startDate,
                    PeriodEnd = endDate
                };

                // Get all routes for this driver in the period
                var allRoutes = new List<Route>();
                var currentDate = startDate.Date;
                var maxIterations = (endDate.Date - startDate.Date).Days + 1;
                var iterations = 0;

                while (currentDate <= endDate.Date && iterations < maxIterations)
                {
                    var dayRoutes = _routeRepository.GetRoutesByDate(currentDate)
                        .Where(r => r.AMDriverId == DriverId || r.PMDriverId == DriverId);
                    allRoutes.AddRange(dayRoutes);
                    currentDate = currentDate.AddDays(1);
                    iterations++;
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
                var maxIterations = (endDate.Date - startDate.Date).Days + 1;
                var iterations = 0;

                while (currentDate <= endDate.Date && iterations < maxIterations)
                {
                    allRoutes.AddRange(_routeRepository.GetRoutesByDate(currentDate));
                    currentDate = currentDate.AddDays(1);
                    iterations++;
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
                var activeVehicles = allRoutes.SelectMany(r => new[] { r.AMBusId, r.PMBusId })
                    .Where(id => id.HasValue).Distinct().Count();
                var totalVehicles = _busRepository.GetAllBuses().Count();
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

        /// <summary>
        /// Calculate simplified cost per student metrics for routes and activities
        /// </summary>
        public async Task<CostPerStudentMetrics> CalculateCostPerStudentMetricsAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var metrics = new CostPerStudentMetrics
                {
                    StartDate = startDate,
                    EndDate = endDate
                };

                // Get all routes in the date range
                var routes = new List<Route>();
                var currentDate = startDate.Date;
                var maxIterations = (endDate.Date - startDate.Date).Days + 1;
                var iterations = 0;

                while (currentDate <= endDate.Date && iterations < maxIterations)
                {
                    routes.AddRange(_routeRepository.GetRoutesByDate(currentDate));
                    currentDate = currentDate.AddDays(1);
                    iterations++;
                }

                // Get all activities in the date range
                var activities = _activityRepository.GetAllActivities()
                    .Where(a => a.DateAsDateTime.HasValue &&
                               a.DateAsDateTime.Value.Date >= startDate.Date &&
                               a.DateAsDateTime.Value.Date <= endDate.Date)
                    .ToList();

                // Calculate route costs and student-days
                metrics.TotalRouteStudentDays = routes.Sum(r => (r.AMRiders ?? 0) + (r.PMRiders ?? 0));

                foreach (var route in routes)
                {
                    // Calculate miles for each route
                    var amMiles = CalculatePeriodMiles(route.AMBeginMiles, route.AMEndMiles);
                    var pmMiles = CalculatePeriodMiles(route.PMBeginMiles, route.PMEndMiles);
                    var totalMiles = amMiles + pmMiles;

                    // Basic cost calculation: fuel + maintenance estimate + driver time
                    var fuelCost = CalculateEstimatedFuelCost(totalMiles);
                    var maintenanceCost = (decimal)totalMiles * 0.20m; // $0.20 per mile maintenance estimate

                    // Driver cost - assume 2 hours per route (AM + PM) at $16.50/hour
                    var driverCost = 2m * 16.50m;

                    metrics.TotalRouteCosts += fuelCost + maintenanceCost + driverCost;
                }

                // Calculate activity costs
                var sportsActivities = activities.Where(a =>
                    a.ActivityType?.Contains("Sports", StringComparison.OrdinalIgnoreCase) == true).ToList();
                var fieldTripActivities = activities.Where(a =>
                    a.ActivityType?.Contains("Field", StringComparison.OrdinalIgnoreCase) == true ||
                    a.ActivityType?.Contains("Trip", StringComparison.OrdinalIgnoreCase) == true).ToList();

                // Estimate activity costs (simplified - could be enhanced with actual mileage data)
                foreach (var activity in sportsActivities)
                {
                    // Estimate 50 miles average for sports trips
                    var estimatedMiles = 50.0;
                    var fuelCost = CalculateEstimatedFuelCost(estimatedMiles);
                    var maintenanceCost = (decimal)estimatedMiles * 0.20m;
                    var driverCost = 50m; // Flat rate for activity trips (teacher/coach stipend)

                    metrics.TotalSportsCosts += fuelCost + maintenanceCost + driverCost;
                }

                foreach (var activity in fieldTripActivities)
                {
                    // Estimate 75 miles average for field trips
                    var estimatedMiles = 75.0;
                    var fuelCost = CalculateEstimatedFuelCost(estimatedMiles);
                    var maintenanceCost = (decimal)estimatedMiles * 0.20m;
                    var driverCost = 50m; // Flat rate for activity trips

                    metrics.TotalFieldTripCosts += fuelCost + maintenanceCost + driverCost;
                }

                // Count students for activities (estimate if not available)
                metrics.TotalSportsStudents = sportsActivities.Count * 20; // Estimate 20 students per sports trip
                metrics.TotalFieldTripStudents = fieldTripActivities.Count * 25; // Estimate 25 students per field trip

                // Calculate cost per student metrics
                metrics.RouteCostPerStudentPerDay = metrics.TotalRouteStudentDays > 0
                    ? metrics.TotalRouteCosts / metrics.TotalRouteStudentDays
                    : 0;

                metrics.SportsCostPerStudent = metrics.TotalSportsStudents > 0
                    ? metrics.TotalSportsCosts / metrics.TotalSportsStudents
                    : 0;

                metrics.FieldTripCostPerStudent = metrics.TotalFieldTripStudents > 0
                    ? metrics.TotalFieldTripCosts / metrics.TotalFieldTripStudents
                    : 0;

                return metrics;
            });
        }

        /// <summary>
        /// Gets the efficiency score for a route by its ID
        /// </summary>
        public double GetRouteEfficiency(int RouteId)
        {
            var route = _routeRepository.GetRouteById(RouteId);
            var metrics = CalculateRouteEfficiency(route);
            return metrics?.EfficiencyScore ?? 0;
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
                if (route.AMBusId.HasValue)
                    vehicleUsage[route.AMBusId.Value] = vehicleUsage.GetValueOrDefault(route.AMBusId.Value) + 1;
                if (route.PMBusId.HasValue && route.PMBusId != route.AMBusId)
                    vehicleUsage[route.PMBusId.Value] = vehicleUsage.GetValueOrDefault(route.PMBusId.Value) + 1;
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

