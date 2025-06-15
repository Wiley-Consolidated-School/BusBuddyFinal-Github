using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Interface for route analytics and optimization services
    /// </summary>
    public interface IRouteAnalyticsService
    {
        /// <summary>
        /// Calculate efficiency metrics for a route
        /// </summary>
        RouteEfficiencyMetrics CalculateRouteEfficiency(Route route);

        /// <summary>
        /// Get efficiency metrics for all routes in a date range
        /// </summary>
        Task<List<RouteEfficiencyMetrics>> GetRouteEfficiencyMetricsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Analyze potential route optimizations
        /// </summary>
        Task<List<RouteOptimizationSuggestion>> AnalyzeRouteOptimizationsAsync(DateTime date);

        /// <summary>
        /// Calculate driver performance metrics
        /// </summary>
        Task<DriverPerformanceMetrics> CalculateDriverPerformanceAsync(int driverId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get fleet-wide analytics summary
        /// </summary>
        Task<FleetAnalyticsSummary> GetFleetAnalyticsSummaryAsync(DateTime startDate, DateTime endDate);
    }
}
