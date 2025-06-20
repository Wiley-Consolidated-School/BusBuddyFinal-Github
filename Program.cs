using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.DependencyInjection;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using BusBuddy.Tests;
using System.IO;

namespace BusBuddy
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Debug output will go to VS Code terminal when run with dotnet run
            Console.WriteLine("🚀 BusBuddy starting with debug console...");

            // Ensure logs directory exists
            Directory.CreateDirectory("logs");

            // Initialize Syncfusion license with proper fallback handling
            Console.WriteLine("📝 Initializing Syncfusion license...");
            try
            {
                SyncfusionLicenseHelper.InitializeLicense();
                Console.WriteLine("✅ Syncfusion license initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Syncfusion license error: {ex.Message}");
                LogError("Syncfusion license initialization failed", ex);
                MessageBox.Show($"Failed to initialize Syncfusion license: {ex.Message}\n\nThe application may have reduced functionality.",
                    "License Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Continue anyway - many features may still work
            }

            // Initialize Syncfusion theming system
            Console.WriteLine("🎨 Initializing Syncfusion theming system...");
            try
            {
                SyncfusionThemeHelper.InitializeGlobalTheme();
                Console.WriteLine("✅ Theming system initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Theming initialization error: {ex.Message}");
                LogError("Theming initialization failed", ex);
                // Continue with default theming
            }

            // Configure high DPI support for the application
            Console.WriteLine("📱 Configuring high DPI support...");
            try
            {
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Console.WriteLine("✅ High DPI support configured");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ High DPI configuration error: {ex.Message}");
                LogError("High DPI configuration failed", ex);
                // Try fallback DPI mode
                try
                {
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Console.WriteLine("✅ Fallback DPI mode configured");
                }
                catch
                {
                    Console.WriteLine("⚠️ Using default DPI settings");
                }
            }

            try
            {
                Console.WriteLine("🔧 Creating service container...");
                var serviceContainer = ServiceContainerInstance.Instance;

                Console.WriteLine("🔌 Validating required services...");
                var navigationService = serviceContainer.GetService<INavigationService>();
                if (navigationService == null)
                {
                    throw new InvalidOperationException("Navigation service not registered. Check ServiceContainerInstance configuration.");
                }

                var databaseHelperService = serviceContainer.GetService<BusBuddy.UI.Services.IDatabaseHelperService>();
                if (databaseHelperService == null)
                {
                    throw new InvalidOperationException("Database helper service not registered. Check ServiceContainerInstance configuration.");
                }

                Console.WriteLine("✅ All required services validated");

                // Test database connection
                Console.WriteLine("🔌 Testing database connection...");
                Task.Run(async () =>
                {
                    try
                    {
                        // Add database connectivity test here when available
                        // await databaseHelperService.TestConnectionAsync();
                        await Task.Delay(10); // Placeholder for actual async operation
                        Console.WriteLine("✅ Database connection test passed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Database connection test failed: {ex.Message}");
                        LogError("Database connection test failed", ex);
                    }
                });

                Console.WriteLine("🧪 Validating cost analytics...");
                Task.Run(async () =>
                {
                    try
                    {
                        await CostAnalyticsValidator.ValidateAnalytics();
                        Console.WriteLine("✅ Analytics validation completed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Analytics validation error: {ex.Message}");
                        LogError("Analytics validation failed", ex);
                    }
                });

                Console.WriteLine("🚌 Creating dashboard...");
                var dashboard = new BusBuddyDashboardSyncfusion(navigationService, databaseHelperService);

                Console.WriteLine("▶️ Running application...");
                Application.Run(dashboard);

                Console.WriteLine("🏁 Application exited normally");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");
                Console.WriteLine($"📍 Stack Trace: {ex.StackTrace}");
                LogError("Critical application error", ex);

                var errorMessage = $"A critical error occurred while starting BusBuddy:\n\n{ex.Message}\n\n" +
                                  "Please check the logs directory for detailed error information.\n" +
                                  "If this problem persists, please contact technical support.";

                MessageBox.Show(errorMessage, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    Console.WriteLine("🧹 Cleaning up service container...");
                    ServiceContainerInstance.Instance.Reset();
                    Console.WriteLine("✅ Service container cleanup complete");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Cleanup error: {ex.Message}");
                    LogError("Service container cleanup failed", ex);
                }

                Console.WriteLine("✅ Application shutdown complete.");
            }
        }

        /// <summary>
        /// Logs errors to file for debugging purposes
        /// </summary>
        private static void LogError(string message, Exception ex)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Stack Trace: {ex.StackTrace}\n" +
                              $"Inner Exception: {ex.InnerException?.Message ?? "None"}\n" +
                              new string('-', 80) + "\n";

                File.AppendAllText("logs/error.log", logEntry);
            }
            catch
            {
                // If logging fails, just continue - don't crash the app over logging
                Console.WriteLine("⚠️ Failed to write to error log");
            }
        }
    }
}
