using System;
using System.Runtime.InteropServices;
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
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [STAThread]
        static void Main()
        {
            // Allocate console for debugging output
            AllocConsole();
            Console.WriteLine("🚀 BusBuddy starting with debug console...");

            // Initialize Syncfusion license with proper fallback handling
            Console.WriteLine("📝 Initializing Syncfusion license...");
            SyncfusionLicenseHelper.InitializeLicense();

            // Initialize Syncfusion theming system
            Console.WriteLine("🎨 Initializing Syncfusion theming system...");
            SyncfusionThemeHelper.InitializeGlobalTheme();

            // Configure high DPI support for the application
            Console.WriteLine("📱 Configuring high DPI support...");
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Console.WriteLine("🔧 Creating service container...");
                var serviceContainer = ServiceContainerInstance.Instance;
                var navigationService = serviceContainer.GetService<INavigationService>();
                var databaseHelperService = serviceContainer.GetService<IDatabaseHelperService>();

                Console.WriteLine("🚌 Creating dashboard...");
                // Use the Syncfusion migrated dashboard - theme is already applied globally
                var dashboard = new BusBuddyDashboardSyncfusion(navigationService, databaseHelperService);

                Console.WriteLine("▶️ Running application...");
                Application.Run(dashboard);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");
                Console.WriteLine($"📍 Stack Trace: {ex.StackTrace}");
                MessageBox.Show($"An error occurred: {ex.Message}\n\nCheck console for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Console.WriteLine("🧹 Cleaning up service container...");
                ServiceContainerInstance.Reset();
                Console.WriteLine("✅ Application shutdown complete. Press any key to close console...");
                Console.ReadKey();
            }
        }
    }
}
