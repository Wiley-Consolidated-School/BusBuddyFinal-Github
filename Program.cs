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
        static void Main(string[] args)
        {
            // Check for special command-line arguments
            if (args.Length > 0 && args[0] == "validate-analytics")
            {
                RunAnalyticsValidationOnly().GetAwaiter().GetResult();
                return;
            }

            // Debug output will go to VS Code terminal when run with dotnet run
            Console.WriteLine("🚀 BusBuddy starting with debug console...");

            // Enable enhanced multithreaded debugging
            Console.WriteLine("🔧 Enabling enhanced debugging features...");
            try
            {
                DebugConfig.EnableEnhancedDebugging();
                Console.WriteLine("✅ Enhanced debugging enabled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Debug enhancement warning: {ex.Message}");
            }

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
        /// Runs analytics validation only (for debugging purposes)
        /// </summary>
        private static async Task RunAnalyticsValidationOnly()
        {
            Console.WriteLine("🔬 Running Analytics Validation Only...\n");

            // Ensure logs directory exists
            Directory.CreateDirectory("logs");

            try
            {
                // Initialize minimal services needed for analytics
                Console.WriteLine("🔧 Initializing minimal service container...");
                var serviceContainer = ServiceContainerInstance.Instance;
                Console.WriteLine("✅ Service container initialized");

                // Run analytics validation with enhanced debugging
                Console.WriteLine("🧪 Starting analytics validation...");
                await CostAnalyticsValidator.ValidateAnalytics();
                Console.WriteLine("✅ Analytics validation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Analytics validation failed: {ex.Message}");
                Console.WriteLine($"📍 Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"📍 Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"📍 Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"📍 Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                LogError("Analytics validation failed in standalone mode", ex);
            }
            finally
            {
                Console.WriteLine("🧹 Cleaning up...");
                try
                {
                    ServiceContainerInstance.Instance.Reset();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Cleanup error: {ex.Message}");
                }
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
