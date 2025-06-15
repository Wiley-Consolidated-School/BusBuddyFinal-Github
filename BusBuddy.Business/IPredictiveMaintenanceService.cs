using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusBuddy.Models;

namespace BusBuddy.Business
{
    /// <summary>
    /// Interface for predictive maintenance scheduling service
    /// </summary>
    public interface IPredictiveMaintenanceService
    {
        /// <summary>
        /// Get maintenance predictions for a specific vehicle
        /// </summary>
        Task<List<MaintenancePrediction>> GetMaintenancePredictionsAsync(int vehicleId);

        /// <summary>
        /// Get fleet-wide maintenance schedule recommendations
        /// </summary>
        Task<List<MaintenanceRecommendation>> GetFleetMaintenanceScheduleAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculate vehicle health score based on maintenance history and usage
        /// </summary>
        Task<VehicleHealthScore> CalculateVehicleHealthScoreAsync(int vehicleId);

        /// <summary>
        /// Analyze maintenance cost trends
        /// </summary>
        Task<MaintenanceCostAnalysis> AnalyzeMaintenanceCostsAsync(int vehicleId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get priority maintenance alerts
        /// </summary>
        Task<List<MaintenanceAlert>> GetMaintenanceAlertsAsync();
    }
}
