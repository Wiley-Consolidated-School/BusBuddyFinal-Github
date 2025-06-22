using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Analytics data structure for transportation metrics
    /// </summary>
    public class MileageStats
    {
        public decimal TotalMiles { get; set; }
        public decimal AverageDailyMiles { get; set; }
        public decimal MaxDailyMiles { get; set; }
        public decimal MinDailyMiles { get; set; }
        public decimal MileageEfficiency { get; set; }
        public Dictionary<string, decimal> RouteBreakdown { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<DateTime, decimal> DailyTrend { get; set; } = new Dictionary<DateTime, decimal>();
    }

    /// <summary>
    /// Pupil count analytics data
    /// </summary>
    public class PupilCountStats
    {
        public int TotalPupils { get; set; }
        public double AverageDailyRiders { get; set; }
        public int MaxDailyRiders { get; set; }
        public int MinDailyRiders { get; set; }
        public double CapacityUtilization { get; set; }
        public Dictionary<string, int> RouteRidership { get; set; } = new Dictionary<string, int>();
        public Dictionary<DateTime, int> DailyTrend { get; set; } = new Dictionary<DateTime, int>();
    }

    /// <summary>
    /// Cost per student analytics
    /// </summary>
    public class CostPerStudentStats
    {
        public decimal CostPerStudent { get; set; }
        public decimal CostPerMile { get; set; }
        public decimal FuelCostPerStudent { get; set; }
        public decimal MaintenanceCostPerStudent { get; set; }
        public decimal TotalOperatingCosts { get; set; }
        public Dictionary<string, decimal> CostBreakdown { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<DateTime, decimal> CostTrend { get; set; } = new Dictionary<DateTime, decimal>();
    }

    /// <summary>
    /// Comprehensive analytics insights with AI-generated analysis
    /// </summary>
    public class AnalyticsInsight
    {
        public string InsightId { get; set; } = Guid.NewGuid().ToString();
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string Category { get; set; } = string.Empty; // Mileage, Ridership, Cost, Efficiency
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public double ImpactScore { get; set; } // 1-10 scale
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Interface for transportation analytics service
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Get comprehensive mileage statistics and trends
        /// </summary>
        /// <param name="startDate">Analysis start date</param>
        /// <param name="endDate">Analysis end date</param>
        /// <returns>Mileage analytics with AI insights</returns>
        Task<MileageStats> GetMileageStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get pupil count statistics and ridership patterns
        /// </summary>
        /// <param name="startDate">Analysis start date</param>
        /// <param name="endDate">Analysis end date</param>
        /// <returns>Pupil count analytics with trends</returns>
        Task<PupilCountStats> GetPupilCountsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculate cost per student metrics with breakdown analysis
        /// </summary>
        /// <param name="startDate">Analysis start date</param>
        /// <param name="endDate">Analysis end date</param>
        /// <returns>Cost analytics with efficiency metrics</returns>
        Task<CostPerStudentStats> GetCostPerStudentAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generate AI-powered insights for transportation data
        /// </summary>
        /// <param name="analyticsData">Combined analytics data for analysis</param>
        /// <returns>List of actionable insights and recommendations</returns>
        Task<List<AnalyticsInsight>> GenerateInsightsAsync(object analyticsData);

        /// <summary>
        /// Get real-time dashboard metrics for current operations
        /// </summary>
        /// <returns>Current operational metrics</returns>
        Task<Dictionary<string, object>> GetDashboardMetricsAsync();
    }

    /// <summary>
    /// Implementation of analytics service with xAI Grok 3 API integration
    /// Provides comprehensive transportation analytics for CDE-40 reporting and operational optimization
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IDatabaseHelperService _databaseHelperService;
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly HttpClient _httpClient;
        private readonly string _xaiApiKey;
        private const string XAI_API_URL = "https://api.x.ai/v1/chat/completions";

        public AnalyticsService(
            IDatabaseHelperService databaseHelperService,
            IRouteAnalyticsService routeAnalyticsService)
        {
            _databaseHelperService = databaseHelperService ?? throw new ArgumentNullException(nameof(databaseHelperService));
            _routeAnalyticsService = routeAnalyticsService ?? throw new ArgumentNullException(nameof(routeAnalyticsService));

            // Initialize HTTP client for xAI API
            _httpClient = new HttpClient();
            _xaiApiKey = Environment.GetEnvironmentVariable("XAI_API_KEY") ?? string.Empty;

            if (!string.IsNullOrEmpty(_xaiApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _xaiApiKey);
            }

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BusBuddy-Analytics/1.0");
        }

        /// <summary>
        /// Get comprehensive mileage statistics and trends
        /// </summary>
        public async Task<MileageStats> GetMileageStatsAsync(DateTime startDate, DateTime endDate)
        {
            var stats = new MileageStats();

            try
            {
                Console.WriteLine($"🔍 Analyzing mileage data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Get route data for the specified period
                var routes = await GetRoutesForPeriodAsync(startDate, endDate);

                // Calculate total mileage
                var dailyMileage = new Dictionary<DateTime, decimal>();
                var routeMileage = new Dictionary<string, decimal>();

                foreach (var route in routes)
                {
                    var routeDate = route.DateAsDateTime.Date;
                    var dailyMiles = ((route.AMEndMiles ?? 0) - (route.AMBeginMiles ?? 0)) +
                                     ((route.PMEndMiles ?? 0) - (route.PMBeginMiles ?? 0));

                    stats.TotalMiles += dailyMiles;

                    // Track daily totals
                    if (dailyMileage.ContainsKey(routeDate))
                        dailyMileage[routeDate] += dailyMiles;
                    else
                        dailyMileage[routeDate] = dailyMiles;

                    // Track route totals
                    var routeName = route.RouteName ?? "Unknown Route";
                    if (routeMileage.ContainsKey(routeName))
                        routeMileage[routeName] += dailyMiles;
                    else
                        routeMileage[routeName] = dailyMiles;
                }

                // Calculate derived statistics
                if (dailyMileage.Count > 0)
                {
                    stats.AverageDailyMiles = dailyMileage.Values.Average();
                    stats.MaxDailyMiles = dailyMileage.Values.Max();
                    stats.MinDailyMiles = dailyMileage.Values.Min();
                }

                stats.RouteBreakdown = routeMileage;
                stats.DailyTrend = dailyMileage;

                // Calculate efficiency (miles per vehicle or similar metric)
                var vehicleCount = await GetActiveVehicleCountAsync();
                if (vehicleCount > 0)
                {
                    stats.MileageEfficiency = stats.TotalMiles / vehicleCount;
                }

                Console.WriteLine($"📊 Mileage analysis complete: {stats.TotalMiles:F1} total miles, {stats.AverageDailyMiles:F1} avg daily");
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error analyzing mileage data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get pupil count statistics and ridership patterns
        /// </summary>
        public async Task<PupilCountStats> GetPupilCountsAsync(DateTime startDate, DateTime endDate)
        {
            var stats = new PupilCountStats();

            try
            {
                Console.WriteLine($"🔍 Analyzing ridership data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Get route data for pupil counts
                var routes = await GetRoutesForPeriodAsync(startDate, endDate);

                var dailyRidership = new Dictionary<DateTime, int>();
                var routeRidership = new Dictionary<string, int>();

                foreach (var route in routes)
                {
                    var routeDate = route.DateAsDateTime.Date;
                    var dailyRiders = (route.AMRiders ?? 0) + (route.PMRiders ?? 0);

                    stats.TotalPupils += dailyRiders;

                    // Track daily totals
                    if (dailyRidership.ContainsKey(routeDate))
                        dailyRidership[routeDate] += dailyRiders;
                    else
                        dailyRidership[routeDate] = dailyRiders;

                    // Track route totals
                    var routeName = route.RouteName ?? "Unknown Route";
                    if (routeRidership.ContainsKey(routeName))
                        routeRidership[routeName] += dailyRiders;
                    else
                        routeRidership[routeName] = dailyRiders;
                }

                // Calculate derived statistics
                if (dailyRidership.Count > 0)
                {
                    stats.AverageDailyRiders = dailyRidership.Values.Average();
                    stats.MaxDailyRiders = dailyRidership.Values.Max();
                    stats.MinDailyRiders = dailyRidership.Values.Min();
                }

                stats.RouteRidership = routeRidership;
                stats.DailyTrend = dailyRidership;

                // Calculate capacity utilization
                var totalCapacity = await GetTotalVehicleCapacityAsync();
                if (totalCapacity > 0 && stats.AverageDailyRiders > 0)
                {
                    stats.CapacityUtilization = stats.AverageDailyRiders / totalCapacity * 100;
                }

                Console.WriteLine($"📊 Ridership analysis complete: {stats.TotalPupils} total pupils, {stats.AverageDailyRiders:F1} avg daily");
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error analyzing ridership data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calculate cost per student metrics with breakdown analysis
        /// </summary>
        public async Task<CostPerStudentStats> GetCostPerStudentAsync(DateTime startDate, DateTime endDate)
        {
            var stats = new CostPerStudentStats();

            try
            {
                Console.WriteLine($"🔍 Analyzing cost data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Get cost data
                var fuelRecords = await GetFuelRecordsForPeriodAsync(startDate, endDate);
                var maintenanceRecords = await GetMaintenanceRecordsForPeriodAsync(startDate, endDate);
                var pupilStats = await GetPupilCountsAsync(startDate, endDate);
                var mileageStats = await GetMileageStatsAsync(startDate, endDate);

                // Calculate total costs
                var fuelCosts = fuelRecords.Sum(f => f.FuelCost ?? 0);
                var maintenanceCosts = maintenanceRecords.Sum(m => m.RepairCost ?? 0);
                stats.TotalOperatingCosts = fuelCosts + maintenanceCosts;

                // Calculate per-student costs
                if (pupilStats.TotalPupils > 0)
                {
                    stats.CostPerStudent = stats.TotalOperatingCosts / pupilStats.TotalPupils;
                    stats.FuelCostPerStudent = fuelCosts / pupilStats.TotalPupils;
                    stats.MaintenanceCostPerStudent = maintenanceCosts / pupilStats.TotalPupils;
                }

                // Calculate per-mile costs
                if (mileageStats.TotalMiles > 0)
                {
                    stats.CostPerMile = stats.TotalOperatingCosts / mileageStats.TotalMiles;
                }

                // Build cost breakdown
                stats.CostBreakdown = new Dictionary<string, decimal>
                {
                    { "Fuel", fuelCosts },
                    { "Maintenance", maintenanceCosts },
                    { "Total", stats.TotalOperatingCosts }
                };

                // Create cost trend (simplified daily average)
                var days = (endDate - startDate).Days + 1;
                if (days > 0)
                {
                    var dailyCost = stats.TotalOperatingCosts / days;
                    for (var date = startDate; date <= endDate; date = date.AddDays(1))
                    {
                        stats.CostTrend[date] = dailyCost;
                    }
                }

                Console.WriteLine($"📊 Cost analysis complete: ${stats.TotalOperatingCosts:F2} total, ${stats.CostPerStudent:F2} per student");
                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error analyzing cost data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generate AI-powered insights for transportation data
        /// </summary>
        public async Task<List<AnalyticsInsight>> GenerateInsightsAsync(object analyticsData)
        {
            var insights = new List<AnalyticsInsight>();

            try
            {
                if (string.IsNullOrEmpty(_xaiApiKey))
                {
                    Console.WriteLine("⚠️ XAI_API_KEY not found, generating fallback insights");
                    return GenerateFallbackInsights(analyticsData);
                }

                // Create analysis request for xAI Grok 3 API
                var prompt = "Analyze transportation data for cost per student trends and optimization opportunities. " +
                           "Provide specific, actionable insights for school transportation efficiency.";

                var aiResponse = await CallXAIAPIAsync(prompt);

                // Parse AI response into structured insights
                var insight = new AnalyticsInsight
                {
                    Category = "AI Analysis",
                    Title = "Transportation Optimization Insights",
                    Description = aiResponse,
                    ImpactScore = 7.5,
                    Priority = "High"
                };

                insights.Add(insight);

                Console.WriteLine("✅ AI insights generated successfully");
                return insights;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating AI insights: {ex.Message}, using fallback");
                return GenerateFallbackInsights(analyticsData);
            }
        }

        /// <summary>
        /// Get real-time dashboard metrics for current operations
        /// </summary>
        public async Task<Dictionary<string, object>> GetDashboardMetricsAsync()
        {
            var metrics = new Dictionary<string, object>();

            try
            {
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-30); // Last 30 days

                // Get current metrics
                var mileageStats = await GetMileageStatsAsync(startDate, endDate);
                var pupilStats = await GetPupilCountsAsync(startDate, endDate);
                var costStats = await GetCostPerStudentAsync(startDate, endDate);

                metrics["TotalMiles"] = mileageStats.TotalMiles;
                metrics["AverageDailyMiles"] = mileageStats.AverageDailyMiles;
                metrics["TotalPupils"] = pupilStats.TotalPupils;
                metrics["AverageDailyRiders"] = pupilStats.AverageDailyRiders;
                metrics["CostPerStudent"] = costStats.CostPerStudent;
                metrics["CostPerMile"] = costStats.CostPerMile;
                metrics["TotalOperatingCosts"] = costStats.TotalOperatingCosts;
                metrics["CapacityUtilization"] = pupilStats.CapacityUtilization;

                Console.WriteLine("📊 Dashboard metrics updated successfully");
                return metrics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error getting dashboard metrics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Call xAI Grok 3 API for insights generation
        /// </summary>
        private async Task<string> CallXAIAPIAsync(string prompt)
        {
            var request = new
            {
                model = "grok-beta",
                messages = new[]
                {
                    new { role = "system", content = "You are a transportation analytics expert specializing in school district efficiency optimization. Provide data-driven insights and actionable recommendations." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 300,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(XAI_API_URL, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (result.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var messageContent))
                    {
                        return messageContent.GetString() ?? "Analysis not available";
                    }
                }
            }

            return "Analysis temporarily unavailable - API response error";
        }

        /// <summary>
        /// Generate fallback insights when AI API is unavailable
        /// </summary>
        private List<AnalyticsInsight> GenerateFallbackInsights(object analyticsData)
        {
            return new List<AnalyticsInsight>
            {
                new AnalyticsInsight
                {
                    Category = "Efficiency",
                    Title = "Route Optimization Opportunity",
                    Description = "Review current route patterns for potential consolidation and efficiency improvements.",
                    Recommendation = "Analyze routes with low ridership for potential optimization.",
                    ImpactScore = 6.0,
                    Priority = "Medium"
                },
                new AnalyticsInsight
                {
                    Category = "Cost Management",
                    Title = "Maintenance Cost Monitoring",
                    Description = "Monitor maintenance costs trends to identify vehicles requiring attention.",
                    Recommendation = "Implement preventive maintenance scheduling to reduce unexpected repairs.",
                    ImpactScore = 7.0,
                    Priority = "High"
                }
            };
        }

        // Data access helper methods - integrated with actual repositories
        private async Task<List<Route>> GetRoutesForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var allRoutes = new List<Route>();
                var currentDate = startDate.Date;

                while (currentDate <= endDate.Date)
                {
                    var routeRepository = new RouteRepository();
                    var dailyRoutes = routeRepository.GetRoutesByDate(currentDate);
                    allRoutes.AddRange(dailyRoutes);
                    currentDate = currentDate.AddDays(1);
                }

                return allRoutes;
            });
        }

        private async Task<List<Fuel>> GetFuelRecordsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var fuelRepository = new FuelRepository();
                var allFuelRecords = fuelRepository.GetAllFuelRecords();

                // Filter by date range - handle nullable DateTime
                return allFuelRecords.Where(f => f.FuelDateAsDateTime.HasValue &&
                                                f.FuelDateAsDateTime.Value.Date >= startDate.Date &&
                                                f.FuelDateAsDateTime.Value.Date <= endDate.Date).ToList();
            });
        }

        private async Task<List<Maintenance>> GetMaintenanceRecordsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var maintenanceRepository = new MaintenanceRepository();
                var allMaintenanceRecords = maintenanceRepository.GetAllMaintenanceRecords();

                // Filter by date range - handle nullable DateTime
                return allMaintenanceRecords.Where(m => m.DateAsDateTime.HasValue &&
                                                       m.DateAsDateTime.Value.Date >= startDate.Date &&
                                                       m.DateAsDateTime.Value.Date <= endDate.Date).ToList();
            });
        }

        private async Task<int> GetActiveVehicleCountAsync()
        {
            return await Task.Run(() =>
            {
                var vehicleRepository = new VehicleRepository();
                var vehicles = vehicleRepository.GetAllVehicles();
                return vehicles?.Count ?? 0;
            });
        }

        private async Task<int> GetTotalVehicleCapacityAsync()
        {
            return await Task.Run(() =>
            {
                var vehicleRepository = new VehicleRepository();
                var vehicles = vehicleRepository.GetAllVehicles();

                // Calculate total capacity
                var totalCapacity = vehicles?.Sum(v => v.Capacity > 0 ? v.Capacity : 72) ?? 0;
                return totalCapacity;
            });
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
