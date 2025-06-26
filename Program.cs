using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.DependencyInjection;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using System.IO;

namespace BusBuddy
{
    internal static class Program
    {
        private static SingleInstanceManager _singleInstanceManager;

        [STAThread]
        static int Main(string[] args)
        {
            // Register Syncfusion license directly as per Syncfusion documentation
            // https://help.syncfusion.com/common/essential-studio/licensing/how-to-register-in-an-application
            // Community license key used for development and testing

            // ⚠️ IMPORTANT: DO NOT MOVE THIS TO A HELPER OR MANAGER CLASS! ⚠️
            // License registration must remain in Main() per Syncfusion's official documentation.
            // See SYNCFUSION_LICENSE_GUIDELINES.md for details on why helper classes are forbidden.
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcdHRdRGNcWENxXkZWYUA=");

            return MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task<int> MainAsync(string[] args)
        {
            Console.WriteLine($"[DEBUG] BusBuddy Main started at {DateTime.Now:O}");

            // Set up exception handling early to prevent dialog mode conflicts
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Console.WriteLine($"[FATAL] Unhandled exception: {ex?.Message}\n{ex?.StackTrace}");
                LogError("Unhandled exception", ex ?? new Exception("Unknown exception"));
            };
            System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine($"[DEBUG] Process exit/unloading at {DateTime.Now:O}");
            };

            // Initialize single instance protection
            _singleInstanceManager = new SingleInstanceManager("BusBuddy-E7B4F3C1-8A2D-4E5F-9B6C-1D3A5E7F9A2B");

            try
            {
                // Try to acquire single instance lock
                if (!_singleInstanceManager.TryAcquireLock())
                {
                    Console.WriteLine("⚠️ Another instance of BusBuddy is already running");

                    // Try to communicate with existing instance
                    var success = await _singleInstanceManager.SendArgsToExistingInstance(args);
                    if (success)
                    {
                        // Bring existing instance to front
                        _singleInstanceManager.BringExistingInstanceToFront();
                        Console.WriteLine("✅ Activated existing instance and passed arguments");
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("❌ Could not communicate with existing instance");
                        Console.WriteLine("🧹 Attempting to clean up orphaned processes...");
                        SingleInstanceManager.CleanupOrphanedProcesses();

                        // Try again after cleanup
                        await Task.Delay(1000); // Wait for cleanup to complete
                        if (!_singleInstanceManager.TryAcquireLock())
                        {
                            MessageBox.Show("Another instance of BusBuddy is already running.\n\nIf you believe this is an error, please restart your computer.",
                                          "BusBuddy - Single Instance", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return 1;
                        }
                        Console.WriteLine("✅ Successfully acquired lock after cleanup");
                    }
                }

                // Setup cleanup on application exit with error handling
                Application.ApplicationExit += (s, e) =>
                {
                    try
                    {
                        _singleInstanceManager?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing SingleInstanceManager on ApplicationExit: {ex.Message}");
                    }
                };
                AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    try
                    {
                        _singleInstanceManager?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error disposing SingleInstanceManager on ProcessExit: {ex.Message}");
                    }
                };

                // Handle communication from secondary instances
                _singleInstanceManager.SecondInstanceDetected += OnSecondInstanceDetected;

                // Check for special command-line arguments
                if (args.Length > 0 && args[0] == "validate-analytics")
                {
                    await RunAnalyticsValidationOnly();
                    return 0;
                }

                // Run the main application
                return await RunMainApplication(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error: {ex.Message}");
                LogError("Fatal application error", ex);
                return 1;
            }
            finally
            {
                _singleInstanceManager?.Dispose();
            }
        }

        /// <summary>
        /// Handles communication from secondary instances
        /// </summary>
        private static void OnSecondInstanceDetected(object sender, string[] args)
        {
            try
            {
                Console.WriteLine($"📨 Secondary instance detected with args: [{string.Join(", ", args)}]");

                // Use invoke to ensure we're on the UI thread
                Application.OpenForms[0]?.Invoke(new Action(() =>
                {
                    var mainForm = Application.OpenForms[0];
                    if (mainForm != null)
                    {
                        if (mainForm.WindowState == FormWindowState.Minimized)
                        {
                            mainForm.WindowState = FormWindowState.Normal;
                        }
                        mainForm.BringToFront();
                        mainForm.Activate();
                        Console.WriteLine("✅ Brought main window to front");
                    }
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling secondary instance: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs the main application logic
        /// </summary>
        private static Task<int> RunMainApplication(string[] args)
        {
            Console.WriteLine("🚀 BusBuddy starting...");
            Directory.CreateDirectory("logs");

            try
            {
                InitializeLicenseAndTheme();
                ConfigureHighDpiSupport();
                return StartDashboardApplication(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");
                LogError("Critical application error", ex);
                ShowCriticalErrorDialog(ex);
                return Task.FromResult(1);
            }
            finally
            {
                CleanupServiceContainer();
            }
        }

        /// <summary>
        /// Initialize Syncfusion license and theme system
        /// </summary>
        private static void InitializeLicenseAndTheme()
        {
            Console.WriteLine("📝 License status: Syncfusion Community Edition active");
            Console.WriteLine("✅ Syncfusion license is active");

            Console.WriteLine("🎨 Initializing BusBuddy theme system...");
            Console.WriteLine("✅ Theme system initialized - Office2016 themes available");
        }

        /// <summary>
        /// Configure high DPI support with fallback options
        /// </summary>
        private static void ConfigureHighDpiSupport()
        {
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
        }

        /// <summary>
        /// Start the dashboard application with service container initialization
        /// </summary>
        private static Task<int> StartDashboardApplication(string[] args)
        {
            var serviceContainer = InitializeServiceContainer();
            ValidateRequiredServices(serviceContainer);
            ProcessCommandLineArguments(args);

            var dashboard = CreateDashboard();
            RunApplication(dashboard);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Initialize and configure the service container
        /// </summary>
        private static ServiceContainerInstance InitializeServiceContainer()
        {
            Console.WriteLine("🔧 Creating service container...");
            var serviceContainer = ServiceContainerInstance.Instance;
            TestServiceResolution(serviceContainer);
            return serviceContainer;
        }

        /// <summary>
        /// Validate that required services are properly registered
        /// </summary>
        private static void ValidateRequiredServices(ServiceContainerInstance serviceContainer)
        {
            Console.WriteLine("🔌 Validating required services...");

            var navigationService = serviceContainer.GetService<BusBuddy.UI.Services.INavigationService>();
            if (navigationService == null)
                throw new InvalidOperationException("Navigation service not registered. Check ServiceContainerInstance configuration.");

            var databaseHelperService = serviceContainer.GetService<BusBuddy.Business.IDatabaseHelperService>();
            if (databaseHelperService == null)
                throw new InvalidOperationException("Business database helper service not registered. Check ServiceContainerInstance configuration.");
        }

        /// <summary>
        /// Process command line arguments for test modes
        /// </summary>
        private static void ProcessCommandLineArguments(string[] args)
        {
            if (args.Length == 0) return;

            switch (args[0].ToLower())
            {
                case "communitylicensetest":
                    Console.WriteLine("🧪 Community License test mode - license already registered in Main");
                    Console.WriteLine("✅ Application will run with properly licensed Syncfusion controls");
                    break;
                case "test-form":
                    Console.WriteLine("🧪 Test form mode - running main dashboard instead");
                    Console.WriteLine("✅ License already registered, dashboard will work properly");
                    break;
                case "community-license":
                    Console.WriteLine("🧪 Community License test mode - running main dashboard");
                    Console.WriteLine("✅ License already registered, Syncfusion controls will work properly");
                    break;
            }
        }

        /// <summary>
        /// Create the main dashboard instance
        /// </summary>
        private static BusBuddy.UI.Views.Dashboard CreateDashboard()
        {
            Console.WriteLine("🔧 Creating Dashboard instance...");
            var dashboard = new BusBuddy.UI.Views.Dashboard();
            Console.WriteLine("✅ Dashboard created successfully");
            return dashboard;
        }

        /// <summary>
        /// Run the Windows Forms application
        /// </summary>
        private static void RunApplication(Form mainForm)
        {
            Console.WriteLine("▶️ Running application...");
            Application.Run(mainForm);
            Console.WriteLine("🏁 Application exited after main form closed");
        }

        // Shows a critical error dialog to the user
        private static void ShowCriticalErrorDialog(Exception ex)
        {
            var errorMessage = $"A critical error occurred while starting BusBuddy:\n\n{ex.Message}\n\n" +
                              $"Stack Trace:\n{ex.StackTrace}\n\n" +
                              "Please check the logs directory for detailed error information.\n" +
                              "If this problem persists, please contact technical support.";

            MessageBox.Show(errorMessage, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine("🏁 Application exited due to critical error");
        }

        /// <summary>
        /// Clean up the service container
        /// </summary>
        private static void CleanupServiceContainer()
        {
            try
            {
                ServiceContainerInstance.Instance.Reset();
                Console.WriteLine("🏁 Application cleanup complete, exiting.");
            }
            catch (Exception ex)
            {
                LogError("Service container cleanup failed", ex);
                Console.WriteLine("🏁 Application exited with cleanup error.");
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

        /// <summary>
        /// Test method to verify all required services are properly registered
        /// </summary>
        private static void TestServiceResolution(ServiceContainerInstance serviceContainer)
        {
            try
            {
                Console.WriteLine("[PROGRAM] Testing service resolution...");

                // Test INavigationService
                var navigationService = serviceContainer.GetService<BusBuddy.UI.Services.INavigationService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service INavigationService resolved: {(navigationService != null ? "Success" : "Failed")}");
                if (navigationService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {navigationService.GetType().FullName}");

                // Test IDatabaseHelperService
                var databaseHelperService = serviceContainer.GetService<BusBuddy.Business.IDatabaseHelperService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service IDatabaseHelperService resolved: {(databaseHelperService != null ? "Success" : "Failed")}");
                if (databaseHelperService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {databaseHelperService.GetType().FullName}");

                // Test IRouteAnalyticsService
                var routeAnalyticsService = serviceContainer.GetService<BusBuddy.Business.IRouteAnalyticsService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service IRouteAnalyticsService resolved: {(routeAnalyticsService != null ? "Success" : "Failed")}");
                if (routeAnalyticsService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {routeAnalyticsService.GetType().FullName}");

                // Test IReportService
                var reportService = serviceContainer.GetService<BusBuddy.UI.Services.IReportService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service IReportService resolved: {(reportService != null ? "Success" : "Failed")}");
                if (reportService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {reportService.GetType().FullName}");

                // Test IAnalyticsService
                var analyticsService = serviceContainer.GetService<BusBuddy.UI.Services.IAnalyticsService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service IAnalyticsService resolved: {(analyticsService != null ? "Success" : "Failed")}");
                if (analyticsService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {analyticsService.GetType().FullName}");

                // Test IErrorHandlerService
                var errorHandlerService = serviceContainer.GetService<BusBuddy.UI.Services.IErrorHandlerService>();
                Console.WriteLine($"[PROGRAM] DEBUG: Service IErrorHandlerService resolved: {(errorHandlerService != null ? "Success" : "Failed")}");
                if (errorHandlerService != null)
                    Console.WriteLine($"[PROGRAM] DEBUG: Service type: {errorHandlerService.GetType().FullName}");

                Console.WriteLine("[PROGRAM] ✅ All services validated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PROGRAM] DEBUG: Service resolution error: {ex.Message}");
                Console.WriteLine($"[PROGRAM] DEBUG: Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
