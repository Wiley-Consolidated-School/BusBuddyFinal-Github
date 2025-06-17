using System;
using System.Windows.Forms;
using BusBuddy.UI.Services;
using BusBuddy.TimeCard;

namespace BusBuddy.Services
{
    /// <summary>
    /// Main application navigation service that extends UI navigation with TimeCard functionality
    /// </summary>
    public class MainNavigationService : INavigationService
    {
        private readonly INavigationService _uiNavigationService;

        public MainNavigationService(INavigationService uiNavigationService)
        {
            _uiNavigationService = uiNavigationService ?? throw new ArgumentNullException(nameof(uiNavigationService));
        }

        // Delegate all standard navigation to the UI service
        public void ShowVehicleManagement() => _uiNavigationService.ShowVehicleManagement();
        public void ShowDriverManagement() => _uiNavigationService.ShowDriverManagement();
        public void ShowRouteManagement() => _uiNavigationService.ShowRouteManagement();
        public void ShowActivityManagement() => _uiNavigationService.ShowActivityManagement();
        public void ShowFuelManagement() => _uiNavigationService.ShowFuelManagement();
        public void ShowMaintenanceManagement() => _uiNavigationService.ShowMaintenanceManagement();
        public void ShowCalendarManagement() => _uiNavigationService.ShowCalendarManagement();
        public void ShowScheduleManagement() => _uiNavigationService.ShowScheduleManagement();
        public void ShowReportsManagement() => _uiNavigationService.ShowReportsManagement();

        // Additional navigation methods for new forms
        public void ShowSchoolCalendarManagement() => _uiNavigationService.ShowSchoolCalendarManagement();
        public void ShowActivityScheduleManagement() => _uiNavigationService.ShowActivityScheduleManagement();
        public void ShowAnalyticsDemo() => _uiNavigationService.ShowAnalyticsDemo();
        public void ShowReports() => _uiNavigationService.ShowReports();

        // Handle TimeCard management directly using the TimeCard module
        public void ShowTimeCardManagement()
        {
            try
            {
                TimeCardModule.LaunchTimeCardManagementForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening TimeCard management: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delegate dialog methods
        public DialogResult ShowDialog<T>() where T : Form => _uiNavigationService.ShowDialog<T>();
        public DialogResult ShowDialog<T>(params object[] parameters) where T : Form => _uiNavigationService.ShowDialog<T>(parameters);
    }
}
