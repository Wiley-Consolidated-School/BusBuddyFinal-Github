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

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Data structure for CDE-40 report generation
    /// </summary>
    public class CDE40ReportData
    {
        public decimal TotalMiles { get; set; }
        public int TotalPupilCount { get; set; }
        public decimal TotalOperatingCosts { get; set; }
        public decimal FuelCosts { get; set; }
        public decimal MaintenanceCosts { get; set; }
        public int VehicleCount { get; set; }
        public int DriverCount { get; set; }
        public int SchoolDays { get; set; }
        public decimal CostPerStudent { get; set; }
        public decimal CostPerMile { get; set; }
        public List<Route> Routes { get; set; } = new List<Route>();
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public List<Fuel> FuelRecords { get; set; } = new List<Fuel>();
        public List<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
    }

    /// <summary>
    /// Generated CDE-40 report with AI insights
    /// </summary>
    public class CDE40Report
    {
        public string ReportId { get; set; } = Guid.NewGuid().ToString();
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public CDE40ReportData Data { get; set; } = new CDE40ReportData();
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string MileageAnalysis { get; set; } = string.Empty;
        public string PupilCountAnalysis { get; set; } = string.Empty;
        public string CostAnalysis { get; set; } = string.Empty;
        public string FinancialContributions { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public bool IsValid { get; set; } = true;
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Interface for CDE-40 report generation service
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generate a comprehensive CDE-40 report with AI-powered insights
        /// </summary>
        /// <param name="schoolYear">School year for the report (e.g., "2024-25")</param>
        /// <returns>Generated CDE-40 report with analysis and recommendations</returns>
        Task<CDE40Report> GenerateCDE40ReportAsync(string schoolYear = "2024-25");

        /// <summary>
        /// Validate CDE-40 report data for completeness and accuracy
        /// </summary>
        /// <param name="reportData">Report data to validate</param>
        /// <returns>List of validation errors, empty if valid</returns>
        List<string> ValidateCDE40Data(CDE40ReportData reportData);

        /// <summary>
        /// Export CDE-40 report to various formats
        /// </summary>
        /// <param name="report">Report to export</param>
        /// <param name="format">Export format (PDF, Excel, CSV)</param>
        /// <param name="filePath">Output file path</param>
        /// <returns>True if export was successful</returns>
        Task<bool> ExportReportAsync(CDE40Report report, string format, string filePath);
    }

    /// <summary>
    /// Implementation of report service with xAI Grok 3 API integration
    /// Based on CDE40_Requirements.md specifications
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IDatabaseHelperService _databaseHelperService;
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly HttpClient _httpClient;
        private readonly string _xaiApiKey;
        private const string XAI_API_URL = "https://api.x.ai/v1/chat/completions";

        public ReportService(
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

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BusBuddy-CDE40-Reporter/1.0");
        }

        /// <summary>
        /// Generate a comprehensive CDE-40 report with AI-powered insights
        /// </summary>
        /// <param name="schoolYear">School year for the report (e.g., "2024-25")</param>
        /// <returns>Generated CDE-40 report with analysis and recommendations</returns>
        public async Task<CDE40Report> GenerateCDE40ReportAsync(string schoolYear = "2024-25")
        {
            var report = new CDE40Report();

            try
            {
                Console.WriteLine($"🔍 Starting CDE-40 report generation for school year {schoolYear}");

                // Step 1: Collect data from all sources
                report.Data = await CollectReportDataAsync(schoolYear);

                // Step 2: Validate data completeness
                report.ValidationErrors = ValidateCDE40Data(report.Data);
                report.IsValid = report.ValidationErrors.Count == 0;

                if (!report.IsValid)
                {
                    Console.WriteLine($"⚠️ Report validation found {report.ValidationErrors.Count} issues");
                    return report;
                }

                // Step 3: Generate AI-powered insights using xAI Grok 3 API
                await GenerateAIInsightsAsync(report);

                Console.WriteLine("✅ CDE-40 report generation completed successfully");
                return report;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating CDE-40 report: {ex.Message}");
                report.IsValid = false;
                report.ValidationErrors.Add($"Report generation failed: {ex.Message}");
                return report;
            }
        }

        /// <summary>
        /// Collect all data required for CDE-40 reporting
        /// </summary>
        private async Task<CDE40ReportData> CollectReportDataAsync(string schoolYear)
        {
            var reportData = new CDE40ReportData();

            try
            {
                // Collect route data for mileage and pupil counts
                var routes = await GetRoutesForSchoolYearAsync(schoolYear);
                reportData.Routes = routes;

                // Calculate total mileage from routes
                foreach (var route in routes)
                {
                    decimal routeMiles = ((route.AMEndMiles ?? 0) - (route.AMBeginMiles ?? 0)) +
                                         ((route.PMEndMiles ?? 0) - (route.PMBeginMiles ?? 0));
                    reportData.TotalMiles += routeMiles;
                    reportData.TotalPupilCount += (route.AMRiders ?? 0) + (route.PMRiders ?? 0);
                }

                // Collect vehicle data
                reportData.Vehicles = await GetVehiclesAsync();
                reportData.VehicleCount = reportData.Vehicles.Count;
                  // Collect fuel cost data
                reportData.FuelRecords = await GetFuelRecordsForSchoolYearAsync(schoolYear);
                reportData.FuelCosts = reportData.FuelRecords.Sum(f => f.FuelCost ?? 0);

                // Collect maintenance cost data
                reportData.MaintenanceRecords = await GetMaintenanceRecordsForSchoolYearAsync(schoolYear);
                reportData.MaintenanceCosts = reportData.MaintenanceRecords.Sum(m => m.RepairCost ?? 0);

                // Calculate derived metrics
                reportData.TotalOperatingCosts = reportData.FuelCosts + reportData.MaintenanceCosts;
                reportData.SchoolDays = await GetSchoolDaysCountAsync(schoolYear);

                if (reportData.TotalPupilCount > 0)
                {
                    reportData.CostPerStudent = reportData.TotalOperatingCosts / reportData.TotalPupilCount;
                }

                if (reportData.TotalMiles > 0)
                {
                    reportData.CostPerMile = reportData.TotalOperatingCosts / reportData.TotalMiles;
                }

                Console.WriteLine($"📊 Data collected: {reportData.TotalMiles:F1} miles, {reportData.TotalPupilCount} pupils, ${reportData.TotalOperatingCosts:F2} costs");
                return reportData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error collecting report data: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Generate AI-powered insights using xAI Grok 3 API
        /// </summary>
        private async Task GenerateAIInsightsAsync(CDE40Report report)
        {
            if (string.IsNullOrEmpty(_xaiApiKey))
            {
                Console.WriteLine("⚠️ XAI_API_KEY not found, using fallback analysis");
                GenerateFallbackInsights(report);
                return;
            }

            try
            {
                // Executive Summary
                report.ExecutiveSummary = await CallXAIAPIAsync(
                    $"Analyze CDE-40 transportation data for Colorado Department of Education: " +
                    $"Total miles: {report.Data.TotalMiles:F1}, " +
                    $"Pupils transported: {report.Data.TotalPupilCount}, " +
                    $"Operating costs: ${report.Data.TotalOperatingCosts:F2}, " +
                    $"Cost per student: ${report.Data.CostPerStudent:F2}, " +
                    $"Vehicles: {report.Data.VehicleCount}. " +
                    $"Generate executive summary highlighting transportation value and efficiency for state funding justification."
                );

                // Mileage Analysis
                report.MileageAnalysis = await CallXAIAPIAsync(
                    $"Analyze daily route efficiency: total miles {report.Data.TotalMiles:F1}, " +
                    $"cost per mile ${report.Data.CostPerMile:F2}, " +
                    $"routes served {report.Data.Routes.Count}. " +
                    $"Provide insights on route optimization and mileage efficiency trends."
                );

                // Pupil Count Analysis
                report.PupilCountAnalysis = await CallXAIAPIAsync(
                    $"Summarize pupil transportation impact: {report.Data.TotalPupilCount} students transported, " +
                    $"cost per student ${report.Data.CostPerStudent:F2}, " +
                    $"across {report.Data.Routes.Count} routes. " +
                    $"Analyze ridership patterns and transportation accessibility."
                );

                // Cost Analysis
                report.CostAnalysis = await CallXAIAPIAsync(
                    $"Generate cost insights: fuel costs ${report.Data.FuelCosts:F2}, " +
                    $"maintenance costs ${report.Data.MaintenanceCosts:F2}, " +
                    $"total operating costs ${report.Data.TotalOperatingCosts:F2}. " +
                    $"Identify cost optimization opportunities and spending patterns."
                );

                // Financial Contributions Context
                report.FinancialContributions = await CallXAIAPIAsync(
                    $"Contextualize transportation funding: State contribution ~$5.1B (Public School Finance Act), " +
                    $"Local contribution ~$4.3B property taxes + $241.7M vehicle taxes. " +
                    $"Our district costs ${report.Data.TotalOperatingCosts:F2} serving {report.Data.TotalPupilCount} students. " +
                    $"Demonstrate value and efficiency relative to state funding levels."
                );

                Console.WriteLine("✅ AI insights generated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating AI insights: {ex.Message}, using fallback");
                GenerateFallbackInsights(report);
            }
        }

        /// <summary>
        /// Call xAI Grok 3 API for text generation
        /// </summary>
        private async Task<string> CallXAIAPIAsync(string prompt)
        {
            var request = new
            {
                model = "grok-beta",
                messages = new[]
                {
                    new { role = "system", content = "You are a transportation analyst specializing in Colorado school district CDE-40 reporting. Provide concise, data-driven insights for regulatory compliance and funding justification." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
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
        private void GenerateFallbackInsights(CDE40Report report)
        {
            var data = report.Data;

            report.ExecutiveSummary = $"CDE-40 Transportation Summary: This district operates {data.VehicleCount} vehicles " +
                $"across {data.Routes.Count} routes, transporting {data.TotalPupilCount} students over {data.TotalMiles:F1} miles " +
                $"with total operating costs of ${data.TotalOperatingCosts:F2}. Cost efficiency of ${data.CostPerStudent:F2} per student " +
                $"demonstrates effective resource utilization in support of educational access.";

            report.MileageAnalysis = $"Route Analysis: Total mileage of {data.TotalMiles:F1} miles across {data.Routes.Count} routes " +
                $"with cost efficiency of ${data.CostPerMile:F2} per mile indicates operational effectiveness in route planning and execution.";

            report.PupilCountAnalysis = $"Student Transportation: {data.TotalPupilCount} students served with cost per student of " +
                $"${data.CostPerStudent:F2}, demonstrating commitment to educational access and transportation equity.";

            report.CostAnalysis = $"Financial Overview: Operating costs totaling ${data.TotalOperatingCosts:F2} include " +
                $"${data.FuelCosts:F2} in fuel costs and ${data.MaintenanceCosts:F2} in maintenance, reflecting responsible " +
                $"fleet management and operational efficiency.";

            report.FinancialContributions = $"Funding Context: This district's transportation costs of ${data.TotalOperatingCosts:F2} " +
                $"represent efficient utilization of state funding (~$5.1B Public School Finance Act) and local contributions " +
                $"(~$4.3B property taxes, $241.7M vehicle taxes) to ensure student transportation access.";

            report.Recommendations = "Continue current operational practices while monitoring fuel costs and route efficiency. " +
                "Consider vehicle replacement planning for maintenance cost optimization.";
        }

        /// <summary>
        /// Validate CDE-40 report data for completeness and accuracy
        /// </summary>
        public List<string> ValidateCDE40Data(CDE40ReportData reportData)
        {
            var errors = new List<string>();

            if (reportData.TotalMiles <= 0)
                errors.Add("Total mileage must be greater than zero");

            if (reportData.TotalPupilCount <= 0)
                errors.Add("Total pupil count must be greater than zero");

            if (reportData.VehicleCount <= 0)
                errors.Add("Vehicle count must be greater than zero");

            if (reportData.Routes.Count == 0)
                errors.Add("At least one route record is required");

            if (reportData.SchoolDays <= 0)
                errors.Add("School days count must be greater than zero");

            // Data consistency checks
            if (reportData.TotalOperatingCosts < 0)
                errors.Add("Operating costs cannot be negative");

            if (reportData.CostPerStudent > 50) // Reasonable upper limit
                errors.Add($"Cost per student (${reportData.CostPerStudent:F2}) appears unusually high");

            return errors;
        }

        /// <summary>
        /// Export CDE-40 report to various formats
        /// </summary>
        public async Task<bool> ExportReportAsync(CDE40Report report, string format, string filePath)
        {
            try
            {
                // For now, implement a simple text export
                // In a full implementation, this would support PDF, Excel, etc.
                var reportText = GenerateReportText(report);
                await System.IO.File.WriteAllTextAsync(filePath, reportText);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error exporting report: {ex.Message}");
                return false;
            }
        }

        private string GenerateReportText(CDE40Report report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CDE-40 TRANSPORTATION REPORT ===");
            sb.AppendLine($"Generated: {report.GeneratedDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Report ID: {report.ReportId}");
            sb.AppendLine();

            sb.AppendLine("EXECUTIVE SUMMARY");
            sb.AppendLine(report.ExecutiveSummary);
            sb.AppendLine();

            sb.AppendLine("MILEAGE ANALYSIS");
            sb.AppendLine(report.MileageAnalysis);
            sb.AppendLine();

            sb.AppendLine("PUPIL COUNT ANALYSIS");
            sb.AppendLine(report.PupilCountAnalysis);
            sb.AppendLine();

            sb.AppendLine("COST ANALYSIS");
            sb.AppendLine(report.CostAnalysis);
            sb.AppendLine();

            sb.AppendLine("FINANCIAL CONTRIBUTIONS");
            sb.AppendLine(report.FinancialContributions);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(report.Recommendations))
            {
                sb.AppendLine("RECOMMENDATIONS");
                sb.AppendLine(report.Recommendations);
                sb.AppendLine();
            }

            sb.AppendLine("=== DATA SUMMARY ===");
            sb.AppendLine($"Total Miles: {report.Data.TotalMiles:F1}");
            sb.AppendLine($"Total Pupils: {report.Data.TotalPupilCount}");
            sb.AppendLine($"Operating Costs: ${report.Data.TotalOperatingCosts:F2}");
            sb.AppendLine($"Cost per Student: ${report.Data.CostPerStudent:F2}");
            sb.AppendLine($"Vehicle Count: {report.Data.VehicleCount}");

            return sb.ToString();
        }

        // Placeholder methods for data access - these would be implemented with actual database calls
        private async Task<List<Route>> GetRoutesForSchoolYearAsync(string schoolYear)
        {
            // In a real implementation, this would query the database for routes within the school year
            await Task.Delay(10); // Simulate async operation
            return new List<Route>(); // Placeholder
        }

        private async Task<List<Vehicle>> GetVehiclesAsync()
        {
            await Task.Delay(10);
            return new List<Vehicle>();
        }

        private async Task<List<Fuel>> GetFuelRecordsForSchoolYearAsync(string schoolYear)
        {
            await Task.Delay(10);
            return new List<Fuel>();
        }

        private async Task<List<Maintenance>> GetMaintenanceRecordsForSchoolYearAsync(string schoolYear)
        {
            await Task.Delay(10);
            return new List<Maintenance>();
        }

        private async Task<int> GetSchoolDaysCountAsync(string schoolYear)
        {
            await Task.Delay(10);
            return 180; // Typical school year
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
