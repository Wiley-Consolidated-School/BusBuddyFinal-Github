// Task 6: Create Analytics Service (DashboardRedesign.md)
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;
using BusBuddy.UI.Services;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Provides analytics functionality for CDE-40 reports and driver pay calculations
    /// Supports transportation value demonstration and financial analysis
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Gets total mileage statistics for a given period
        /// Includes Route AM/PM miles and Activity Schedule trip distances
        /// </summary>
        /// <param name="startDate">Start date for analysis period</param>
        /// <param name="endDate">End date for analysis period</param>
        /// <returns>Total mileage in decimal format</returns>
        Task<decimal> GetMileageStatsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets total pupil counts for a given period
        /// Includes Route AM/PM riders and Activity Schedule scheduled riders
        /// </summary>
        /// <param name="startDate">Start date for analysis period</param>
        /// <param name="endDate">End date for analysis period</param>
        /// <returns>Total pupil count</returns>
        Task<int> GetPupilCountsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates cost per student for a given period
        /// Based on fuel and maintenance costs divided by pupil counts
        /// </summary>
        /// <param name="startDate">Start date for analysis period</param>
        /// <param name="endDate">End date for analysis period</param>
        /// <returns>Cost per student in decimal format</returns>
        Task<decimal> GetCostPerStudentAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Generates comprehensive driver pay report with xAI Grok 3 API analysis
        /// Supports CDL ($33/trip), Small Bus ($15/trip), and SPED ($66/day) pay schemes
        /// </summary>
        /// <param name="startDate">Start date for pay period</param>
        /// <param name="endDate">End date for pay period</param>
        /// <returns>List of driver pay reports</returns>
        Task<List<DriverPayReport>> GenerateDriverPayReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets comprehensive management dashboard data
        /// Includes metrics for fleet utilization, costs, driver performance
        /// </summary>
        /// <param name="startDate">Start date for analysis period</param>
        /// <param name="endDate">End date for analysis period</param>
        /// <returns>Dictionary with dashboard metrics</returns>
        Task<Dictionary<string, object>> GetManagementDashboardDataAsync(DateTime startDate, DateTime endDate);
    }
}

