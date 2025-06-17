using System;
using System.Windows.Forms;
using BusBuddy.DependencyInjection;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;

namespace BusBuddy
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Register Syncfusion license from configuration file with fallback
            SyncfusionLicenseHelper.RegisterFromConfiguration();

            // Initialize Syncfusion theming system
            SyncfusionThemeHelper.InitializeGlobalTheme();

            // Configure high DPI support for the application
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var serviceContainer = ServiceContainerInstance.Instance;
                var navigationService = serviceContainer.GetService<INavigationService>();
                var databaseHelperService = serviceContainer.GetService<IDatabaseHelperService>();

                // Use the enhanced Syncfusion dashboard with comprehensive fixes
                var dashboard = new BusBuddyDashboardSyncfusion(navigationService, databaseHelperService);

                // Apply Syncfusion theming to the dashboard
                SyncfusionThemeHelper.ApplyMaterialTheme(dashboard);

                Application.Run(dashboard);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ServiceContainerInstance.Reset();
            }
        }
    }
}
