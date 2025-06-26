using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Pay scheme configuration for driver pay calculations
    /// JSON-based configuration that can be easily updated
    /// </summary>
    public class PayScheme
    {
        public decimal CDLTripRate { get; set; } = 33.00m;
        public decimal SmallBusTripRate { get; set; } = 15.00m;
        public decimal SPEDDayRate { get; set; } = 66.00m;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Implementation of analytics service with xAI Grok 3 API integration
    /// Provides comprehensive transportation analytics for CDE-40 reporting and operational optimization
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly BusBuddy.Business.IDatabaseHelperService _databaseHelperService;
        private readonly BusBuddy.Business.IRouteAnalyticsService _routeAnalyticsService;
        private readonly HttpClient _httpClient;
        private readonly string _xaiApiKey;
        private readonly string _paySchemeConfigPath;
        private const string XAI_API_URL = "https://api.x.ai/v1/chat/completions";

        public AnalyticsService(
            BusBuddy.Business.IDatabaseHelperService databaseHelperService,
            BusBuddy.Business.IRouteAnalyticsService routeAnalyticsService)
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

            // Set up pay scheme config path
            _paySchemeConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payscheme.json");
        }

        /// <summary>
        /// Gets total mileage statistics for a given period
        /// </summary>
        public async Task<decimal> GetMileageStatsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Analyzing mileage data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var routes = await GetRoutesForPeriodAsync(startDate, endDate);
                var activities = await GetActivitySchedulesForPeriodAsync(startDate, endDate);

                decimal totalMiles = 0;

                // Calculate route mileage
                foreach (var route in routes)
                {
                    var amMiles = (route.AMEndMiles ?? 0) - (route.AMBeginMiles ?? 0);
                    var pmMiles = (route.PMEndMiles ?? 0) - (route.PMBeginMiles ?? 0);
                    totalMiles += amMiles + pmMiles;
                }

                // Add activity schedule mileage (estimate based on destination)
                foreach (var activity in activities)
                {
                    // Estimate trip distance based on destination - placeholder logic
                    totalMiles += EstimateTripDistance(activity.ScheduledDestination ?? "");
                }

                Console.WriteLine($"📊 Total mileage calculated: {totalMiles:F1} miles");
                return totalMiles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating mileage: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets total pupil counts for a given period
        /// </summary>
        public async Task<int> GetPupilCountsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Analyzing pupil count data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var routes = await GetRoutesForPeriodAsync(startDate, endDate);
                var activities = await GetActivitySchedulesForPeriodAsync(startDate, endDate);

                int totalPupils = 0;

                // Calculate route ridership
                foreach (var route in routes)
                {
                    totalPupils += (route.AMRiders ?? 0) + (route.PMRiders ?? 0);
                }

                // Add activity schedule ridership
                foreach (var activity in activities)
                {
                    totalPupils += activity.ScheduledRiders ?? 0;
                }

                Console.WriteLine($"📊 Total pupil count: {totalPupils}");
                return totalPupils;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating pupil counts: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Calculates cost per student for a given period
        /// </summary>
        public async Task<decimal> GetCostPerStudentAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Analyzing cost per student from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var totalPupils = await GetPupilCountsAsync(startDate, endDate);
                var fuelRecords = await GetFuelRecordsForPeriodAsync(startDate, endDate);
                var maintenanceRecords = await GetMaintenanceRecordsForPeriodAsync(startDate, endDate);

                var totalCosts = fuelRecords.Sum(f => f.FuelCost ?? 0) +
                                maintenanceRecords.Sum(m => m.RepairCost ?? 0);

                var costPerStudent = totalPupils > 0 ? totalCosts / totalPupils : 0;

                Console.WriteLine($"📊 Cost per student: ${costPerStudent:F2} (Total: ${totalCosts:F2}, Pupils: {totalPupils})");
                return costPerStudent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating cost per student: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates comprehensive driver pay report
        /// </summary>
        public async Task<List<DriverPayReport>> GenerateDriverPayReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Generating driver pay report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var payScheme = await GetPaySchemeAsync();
                var routes = await GetRoutesForPeriodAsync(startDate, endDate);
                var drivers = await GetAllDriversAsync();
                var reports = new List<DriverPayReport>();

                // Group routes by driver
                var driverRoutes = new Dictionary<int, List<Route>>();

                foreach (var route in routes)
                {
                    // Add AM driver routes
                    if (route.AMDriverID.HasValue)
                    {
                        if (!driverRoutes.ContainsKey(route.AMDriverID.Value))
                            driverRoutes[route.AMDriverID.Value] = new List<Route>();
                        driverRoutes[route.AMDriverID.Value].Add(route);
                    }

                    // Add PM driver routes
                    if (route.PMDriverID.HasValue)
                    {
                        if (!driverRoutes.ContainsKey(route.PMDriverID.Value))
                            driverRoutes[route.PMDriverID.Value] = new List<Route>();
                        driverRoutes[route.PMDriverID.Value].Add(route);
                    }
                }

                // Calculate pay for each driver
                foreach (var driverGroup in driverRoutes)
                {
                    var driver = drivers.FirstOrDefault(d => d.DriverID == driverGroup.Key);
                    if (driver == null) continue;

                    var report = new DriverPayReport
                    {
                        DriverID = driver.DriverID,
                        DriverName = driver.Name,
                        LicenseType = driver.DriversLicenseType ?? "Unknown",
                        PayPeriodStart = startDate,
                        PayPeriodEnd = endDate,
                        TotalTrips = CountTrips(driverGroup.Value),
                        SPEDDays = 0,
                        PayAmount = 0m,
                        Notes = new List<string>()
                    };

                    // Determine if this is a SPED driver
                    bool isSPEDDriver = (driver.DriversLicenseType?.Contains("SPED") == true);

                    // For SPED drivers, count days worked
                    if (isSPEDDriver)
                    {
                        report.SPEDDays = CountWorkDays(driverGroup.Value);
                        report.PayAmount = report.SPEDDays * payScheme.SPEDDayRate;
                        report.Notes.Add($"SPED services provided on {report.SPEDDays} days");
                    }
                    else if (driver.DriversLicenseType?.Contains("CDL") == true)
                    {
                        // CDL drivers paid per trip
                        report.PayAmount = report.TotalTrips * payScheme.CDLTripRate;
                        report.Notes.Add($"CDL rate: ${payScheme.CDLTripRate} per trip");
                    }
                    else
                    {
                        // Small Bus drivers paid per trip
                        report.PayAmount = report.TotalTrips * payScheme.SmallBusTripRate;
                        report.Notes.Add($"Small Bus rate: ${payScheme.SmallBusTripRate} per trip");
                    }

                    report.Notes.Add($"Pay period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                    reports.Add(report);
                }

                Console.WriteLine($"📊 Driver pay report generated: {reports.Count} drivers");
                return reports;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating driver pay report: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets transportation efficiency metrics with AI insights
        /// </summary>
        public async Task<EfficiencyMetrics> GetEfficiencyMetricsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Analyzing efficiency metrics from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var totalMiles = await GetMileageStatsAsync(startDate, endDate);
                var totalPupils = await GetPupilCountsAsync(startDate, endDate);
                var costPerStudent = await GetCostPerStudentAsync(startDate, endDate);
                var vehicleCapacity = await GetTotalVehicleCapacityAsync();

                var metrics = new EfficiencyMetrics();

                // Calculate key metrics
                if (totalMiles > 0)
                {
                    var fuelCosts = (await GetFuelRecordsForPeriodAsync(startDate, endDate)).Sum(f => f.FuelCost ?? 0);
                    var maintenanceCosts = (await GetMaintenanceRecordsForPeriodAsync(startDate, endDate)).Sum(m => m.RepairCost ?? 0);
                    metrics.CostPerMile = (fuelCosts + maintenanceCosts) / totalMiles;
                }

                if (vehicleCapacity > 0 && totalPupils > 0)
                {
                    metrics.UtilizationRate = (decimal)totalPupils / vehicleCapacity * 100;
                }

                metrics.KeyMetrics = new Dictionary<string, decimal>
                {
                    ["TotalMiles"] = totalMiles,
                    ["TotalPupils"] = totalPupils,
                    ["CostPerStudent"] = costPerStudent,
                    ["CostPerMile"] = metrics.CostPerMile,
                    ["UtilizationRate"] = metrics.UtilizationRate
                };

                // Generate AI-powered recommendations
                metrics.AIInsights = await GenerateEfficiencyInsightsAsync(metrics.KeyMetrics);
                metrics.Recommendations = ExtractRecommendations(metrics.AIInsights);

                Console.WriteLine($"📊 Efficiency metrics calculated: {metrics.CostPerMile:C}/mile, {metrics.UtilizationRate:F1}% utilization");
                return metrics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating efficiency metrics: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets comprehensive management dashboard data
        /// Includes metrics for fleet utilization, costs, driver performance
        /// </summary>
        public async Task<Dictionary<string, object>> GetManagementDashboardDataAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                Console.WriteLine($"🔍 Generating management dashboard data from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                var result = new Dictionary<string, object>();

                // Get basic metrics
                result["TotalMileage"] = await GetMileageStatsAsync(startDate, endDate);
                result["TotalPupils"] = await GetPupilCountsAsync(startDate, endDate);
                result["CostPerStudent"] = await GetCostPerStudentAsync(startDate, endDate);

                // Get vehicle fleet data
                var vehicles = await GetAllVehiclesAsync();
                var routes = await GetRoutesForPeriodAsync(startDate, endDate);
                var maintenance = await GetMaintenanceRecordsForPeriodAsync(startDate, endDate);
                var fuel = await GetFuelRecordsForPeriodAsync(startDate, endDate);

                // Calculate vehicle utilization
                var vehicleUtilization = new Dictionary<string, decimal>();
                foreach (var vehicle in vehicles)
                {
                    var vehicleRoutes = routes.Count(r => r.AMVehicleID == vehicle.Id || r.PMVehicleID == vehicle.Id);
                    var utilizationPercentage = vehicleRoutes > 0
                        ? (decimal)vehicleRoutes / (decimal)routes.Count * 100
                        : 0;
                    vehicleUtilization[vehicle.VehicleNumber ?? $"Vehicle {vehicle.Id}"] = Math.Round(utilizationPercentage, 1);
                }
                result["VehicleUtilization"] = vehicleUtilization;

                // Calculate maintenance costs by vehicle
                var maintenanceCosts = maintenance
                    .GroupBy(m => m.VehicleID)
                    .ToDictionary(
                        g => vehicles.FirstOrDefault(v => v.Id == g.Key)?.VehicleNumber ?? $"Vehicle {g.Key}",
                        g => g.Sum(m => m.RepairCost ?? 0)
                    );
                result["MaintenanceCosts"] = maintenanceCosts;

                // Calculate fuel costs by vehicle
                var fuelCosts = fuel
                    .GroupBy(f => f.VehicleFueledID)
                    .ToDictionary(
                        g => vehicles.FirstOrDefault(v => v.Id == g.Key)?.VehicleNumber ?? $"Vehicle {g.Key}",
                        g => g.Sum(f => f.FuelCost ?? 0)
                    );
                result["FuelCosts"] = fuelCosts;

                // Calculate total costs
                result["TotalMaintenanceCost"] = maintenance.Sum(m => m.RepairCost ?? 0);
                result["TotalFuelCost"] = fuel.Sum(f => f.FuelCost ?? 0);
                result["TotalOperatingCost"] = (decimal)result["TotalMaintenanceCost"] + (decimal)result["TotalFuelCost"];

                // Calculate cost per mile
                if ((decimal)result["TotalMileage"] > 0)
                {
                    result["CostPerMile"] = (decimal)result["TotalOperatingCost"] / (decimal)result["TotalMileage"];
                }
                else
                {
                    result["CostPerMile"] = 0;
                }

                // Generate AI insights
                var insights = await GenerateManagementInsightsAsync(result);
                result["AIInsights"] = insights;

                Console.WriteLine($"📊 Management dashboard data generated with {result.Count} metrics");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating management dashboard data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the current pay scheme configuration
        /// </summary>
        public async Task<PayScheme> GetPaySchemeAsync()
        {
            try
            {
                Console.WriteLine("🔍 Loading pay scheme configuration");
                return await LoadPaySchemeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading pay scheme: {ex.Message}");
                throw;
            }
        }        /// <summary>
        /// Saves the updated pay scheme configuration
        /// </summary>
        public async Task SavePaySchemeAsync(PayScheme payScheme)
        {
            try
            {
                Console.WriteLine("💾 Saving pay scheme configuration");

                // Update last updated date
                payScheme.LastUpdated = DateTime.Now;

                // Serialize to JSON
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(payScheme, options);

                // Save to file
                await File.WriteAllTextAsync(_paySchemeConfigPath, json);

                Console.WriteLine($"✅ Pay scheme saved: CDL=${payScheme.CDLTripRate}, SmallBus=${payScheme.SmallBusTripRate}, SPED=${payScheme.SPEDDayRate}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error saving pay scheme: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generates management insights using xAI API
        /// </summary>
        private async Task<string> GenerateManagementInsightsAsync(Dictionary<string, object> dashboardData)
        {
            try
            {
                if (string.IsNullOrEmpty(_xaiApiKey))
                {
                    return "AI insights not available (API key not configured)";
                }

                var prompt = BuildManagementInsightsPrompt(dashboardData);
                var response = await CallXaiApiAsync(prompt);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating management insights: {ex.Message}");
                return "AI insights not available due to an error";
            }
        }

        /// <summary>
        /// Builds a prompt for management insights
        /// </summary>
        private string BuildManagementInsightsPrompt(Dictionary<string, object> dashboardData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Analyze the following school transportation data and provide 3-5 key insights for management:");
            sb.AppendLine();

            foreach (var item in dashboardData)
            {
                if (item.Value is Dictionary<string, decimal> dictDecimal)
                {
                    sb.AppendLine($"{item.Key}:");
                    foreach (var subItem in dictDecimal)
                    {
                        sb.AppendLine($"  {subItem.Key}: {subItem.Value}");
                    }
                }
                else if (item.Value is Dictionary<string, object> dictObject)
                {
                    sb.AppendLine($"{item.Key}:");
                    foreach (var subItem in dictObject)
                    {
                        sb.AppendLine($"  {subItem.Key}: {subItem.Value}");
                    }
                }
                else
                {
                    sb.AppendLine($"{item.Key}: {item.Value}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Focus on cost efficiency, vehicle utilization, and opportunities for improvement.");

            return sb.ToString();
        }

        #region Private Helper Methods

        private async Task<List<Route>> GetRoutesForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            // Implementation depends on your data access pattern
            // This is a placeholder - you'll need to implement based on your repository pattern
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Routes
                .Where(r => r.DateAsDateTime >= startDate && r.DateAsDateTime <= endDate)
                .ToList());
        }

        private async Task<List<ActivitySchedule>> GetActivitySchedulesForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.ActivitySchedules
                .Where(a => a.DateAsDateTime >= startDate && a.DateAsDateTime <= endDate)
                .ToList());
        }

        private async Task<List<Fuel>> GetFuelRecordsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Fuels
                .Where(f => f.FuelDateAsDateTime >= startDate && f.FuelDateAsDateTime <= endDate)
                .ToList());
        }

        private async Task<List<Maintenance>> GetMaintenanceRecordsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Maintenances
                .Where(m => m.DateAsDateTime >= startDate && m.DateAsDateTime <= endDate)
                .ToList());
        }

        private async Task<List<Driver>> GetAllDriversAsync()
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Drivers.ToList());
        }

        private async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Vehicles.ToList());
        }

        private async Task<decimal> GetTotalVehicleCapacityAsync()
        {
            var context = new BusBuddyContext();
            return await Task.FromResult(context.Vehicles.Sum(v => v.Capacity));
        }

        private async Task<PayScheme> LoadPaySchemeAsync()
        {
            try
            {
                if (File.Exists(_paySchemeConfigPath))
                {
                    var json = await File.ReadAllTextAsync(_paySchemeConfigPath);
                    return JsonSerializer.Deserialize<PayScheme>(json) ?? new PayScheme();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading pay scheme config: {ex.Message}");
            }

            // Return default pay scheme
            var defaultScheme = new PayScheme();
            await SavePaySchemeAsync(defaultScheme);
            return defaultScheme;
        }

        private async Task<string> CallXaiApiAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_xaiApiKey))
            {
                return "AI insights not available (API key not configured)";
            }

            try
            {
                var content = new
                {
                    model = "grok-3",
                    messages = new[]
                    {
                        new { role = "system", content = "You are an AI assistant for school transportation analytics." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.5,
                    max_tokens = 500
                };

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(content),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(XAI_API_URL, requestContent);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var choices = jsonDoc.RootElement.GetProperty("choices");
                var message = choices[0].GetProperty("message");
                var insight = message.GetProperty("content").GetString();

                return insight ?? "No insights available";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error calling xAI API: {ex.Message}");
                return "AI insights not available due to an error";
            }
        }

        private string DetermineDriverType(Driver driver, List<Route> routes)
        {
            // Check if this is SPED based on route name patterns
            var isSPED = routes.Any(r => r.RouteName?.ToUpper().Contains("SPED") == true);
            if (isSPED) return "SPED";

            // Check license type
            if (driver.DriversLicenseType?.ToUpper().Contains("CDL") == true)
                return "CDL";

            // Default to Small Bus for Passenger license or unknown
            return "Small Bus";
        }

        private int CountWorkDays(List<Route> routes)
        {
            // Count unique dates for SPED drivers (paid per day)
            return routes.Select(r => r.DateAsDateTime.Date).Distinct().Count();
        }

        private int CountTrips(List<Route> routes)
        {
            // Count AM and PM trips separately
            int trips = 0;
            foreach (var route in routes)
            {
                if (route.AMDriverID.HasValue) trips++;
                if (route.PMDriverID.HasValue) trips++;
            }
            return trips;
        }

        private async Task<string> GeneratePayReportInsightsAsync(List<DriverPayReport> reports)
        {
            if (string.IsNullOrEmpty(_xaiApiKey))
            {
                return "XAI API key not configured. Unable to generate AI insights.";
            }

            try
            {
                var totalPay = reports.Sum(r => r.TotalPay);
                var driverCount = reports.Count;
                var avgPay = driverCount > 0 ? totalPay / driverCount : 0;

                var prompt = $@"Analyze this driver pay report data and provide insights:
- Total drivers: {driverCount}
- Total pay: ${totalPay:F2}
- Average pay per driver: ${avgPay:F2}
- Driver types: {reports.GroupBy(r => r.DriverType).Select(g => $"{g.Key}: {g.Count()}").Aggregate((a, b) => a + ", " + b)}

Provide specific recommendations for pay equity, budget planning, and driver retention.";

                return await CallXAIAPIAsync(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating pay report insights: {ex.Message}");
                return "AI insights unavailable due to API error.";
            }
        }

        private async Task<string> GenerateEfficiencyInsightsAsync(Dictionary<string, decimal> keyMetrics)
        {
            if (string.IsNullOrEmpty(_xaiApiKey))
            {
                return "XAI API key not configured. Unable to generate AI insights.";
            }

            try
            {
                var metricsText = string.Join(", ", keyMetrics.Select(kv => $"{kv.Key}: {kv.Value:F2}"));

                var prompt = $@"Analyze these transportation efficiency metrics and provide optimization recommendations:
{metricsText}

Focus on CDE-40 reporting value and cost-per-student optimization for school transportation.";

                return await CallXAIAPIAsync(prompt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating efficiency insights: {ex.Message}");
                return "AI insights unavailable due to API error.";
            }
        }

        private async Task<string> CallXAIAPIAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = "grok-beta",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a transportation analytics expert specializing in school bus operations and CDE-40 reporting. Provide specific, actionable insights." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 500,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(XAI_API_URL, content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

                return apiResponse.GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "No response from AI API";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ XAI API call failed: {ex.Message}");
                return "AI analysis unavailable.";
            }
        }

        private List<string> ExtractRecommendations(string aiInsights)
        {
            var recommendations = new List<string>();

            // Simple extraction - look for bullet points or numbered items
            var lines = aiInsights.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("•") || trimmedLine.StartsWith("-") ||
                    trimmedLine.StartsWith("*") || char.IsDigit(trimmedLine.FirstOrDefault()))
                {
                    recommendations.Add(trimmedLine);
                }
            }

            // If no structured recommendations found, add the full insight
            if (recommendations.Count == 0)
            {
                recommendations.Add(aiInsights);
            }

            return recommendations;
        }

        private decimal EstimateTripDistance(string destination)
        {
            // Simple estimation based on destination - replace with actual logic if needed
            if (string.IsNullOrEmpty(destination)) return 5.0m;

            // Basic estimation: local trips = 5 miles, out-of-town = 15 miles
            var lowerDest = destination.ToLower();
            if (lowerDest.Contains("local") || lowerDest.Contains("school")) return 5.0m;
            if (lowerDest.Contains("town") || lowerDest.Contains("city")) return 15.0m;

            return 10.0m; // Default estimate
        }

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
