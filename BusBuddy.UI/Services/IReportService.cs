// Task 5: Create Report Service (DashboardRedesign.md)
using System.Threading.Tasks;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Provides reporting functionality for CDE-40 reports using xAI Grok 3 API integration
    /// Supports transportation value demonstration and financial contribution analysis
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates a CDE-40 report using xAI Grok 3 API with comprehensive data analysis
        /// Includes mileage analysis, pupil counts, cost breakdowns, and financial contributions
        /// </summary>
        /// <param name="schoolYear">School year for the report (e.g., "2024-25")</param>
        /// <returns>Formatted report data suitable for BoldReportViewer display</returns>
        Task<object> GenerateCDE40ReportAsync(string schoolYear = "2024-25");

        /// <summary>
        /// Validates CDE-40 report data for completeness and accuracy
        /// Ensures all required data points are present for state compliance
        /// </summary>
        /// <param name="reportData">Report data to validate</param>
        /// <returns>True if data is valid, false otherwise</returns>
        bool ValidateReportData(object reportData);

        /// <summary>
        /// Exports generated reports to various formats (PDF, Excel, CSV)
        /// Uses Syncfusion Bold Reports for professional formatting
        /// </summary>
        /// <param name="reportData">Report data to export</param>
        /// <param name="format">Export format (PDF, Excel, CSV)</param>
        /// <param name="filePath">Output file path</param>
        /// <returns>True if export was successful, false otherwise</returns>
        Task<bool> ExportReportAsync(object reportData, string format, string filePath);
    }
}
