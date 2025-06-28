using System;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Enhanced Navigation Service interface for BusBuddy Dashboard
    /// Provides navigation functionality for dashboard modules supporting CDE-40 reporting,
    /// financial contributions tracking, and transportation value demonstration.
    /// Supports Office2016Black theme and seamless module access.
    /// </summary>
    public interface INavigationService
    {
        void ShowVehicleManagement();
        void ShowDriverManagement();
        void ShowRouteManagement();
        void ShowActivityManagement();
        void ShowFuelManagement();
        void ShowMaintenanceManagement();
        void ShowCalendarManagement();
        void ShowScheduleManagement();
        void ShowTimeCardManagement();
        void ShowReportsManagement();
        void ShowSchoolCalendarManagement();
        void ShowActivityScheduleManagement();
        void ShowAnalyticsDemo();
        void ShowReports();

        /// <summary>
        /// Enhanced Navigate method with improved error handling and CDE-40 support
        /// Navigate to a specific module by name with optional parameters
        /// Supports CDE-40 reporting, financial analytics, and transportation value modules
        /// </summary>
        /// <param name="moduleName">Name of the module to navigate to</param>
        /// <param name="parameters">Optional parameters to pass to the module</param>
        /// <returns>True if navigation was successful, false otherwise</returns>
        bool Navigate(string moduleName, params object[] parameters);

        /// <summary>
        /// Check if a specific module is available in the current configuration
        /// </summary>
        /// <param name="moduleName">Name of the module to check</param>
        /// <returns>True if the module is available, false otherwise</returns>
        bool IsModuleAvailable(string moduleName);

        DialogResult ShowDialog<T>() where T : Form;
        DialogResult ShowDialog<T>(params object[] parameters) where T : Form;
    }
}
