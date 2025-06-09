using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
    /// <summary>
    /// Represents a dashboard with summary information and metrics
    /// </summary>
    public class Dashboard
    {
        /// <summary>
        /// Gets or sets the dashboard title
        /// </summary>
        public string Title { get; set; } = "BusBuddy Dashboard";

        /// <summary>
        /// Gets or sets the last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the total number of vehicles
        /// </summary>
        public int TotalVehicles { get; set; }

        /// <summary>
        /// Gets or sets the number of active vehicles
        /// </summary>
        public int ActiveVehicles { get; set; }

        /// <summary>
        /// Gets or sets the number of vehicles in maintenance
        /// </summary>
        public int VehiclesInMaintenance { get; set; }

        /// <summary>
        /// Gets or sets the total number of drivers
        /// </summary>
        public int TotalDrivers { get; set; }

        /// <summary>
        /// Gets or sets the number of active drivers
        /// </summary>
        public int ActiveDrivers { get; set; }

        /// <summary>
        /// Gets or sets the total number of routes
        /// </summary>
        public int TotalRoutes { get; set; }

        /// <summary>
        /// Gets or sets recent activities
        /// </summary>
        public List<string> RecentActivities { get; set; } = new List<string>();

        /// <summary>
        /// Initializes a new instance of the Dashboard class
        /// </summary>
        public Dashboard()
        {
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Updates the dashboard metrics
        /// </summary>
        public void RefreshMetrics()
        {
            LastUpdated = DateTime.Now;
            // Additional refresh logic can be added here
        }
    }
}
