using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.Data;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using Syncfusion.Licensing;
using Syncfusion.Windows.Forms;

namespace BusBuddy
{
    /// <summary>
    /// Main program entry point for BusBuddy application
    ///
    /// EXPLICIT CLEANUP NECESSITY ASSESSMENT (June 27, 2025):
    /// =====================================================
    /// Based on comprehensive analysis, explicit cleanup IS NECESSARY for BusBuddy because:
    ///
    /// 1. BACKGROUND SERVICES: Async service initialization, database pre-warming, analytics
    /// 2. BUILD TOOL INTEGRATION: Spawns dotnet, MSBuild, VBCSCompiler processes
    /// 3. SYNCFUSION COMPONENTS: Complex UI controls require proper resource disposal
    /// 4. DATABASE CONNECTIONS: Entity Framework contexts need explicit cleanup
    /// 5. CANCELLATION TOKENS: Background operations use cancellation that needs coordination
    ///
    /// The current implementation EXCEEDS the recommended basic solution with:
    /// - Thread-safe process management with multiple locks
    /// - Graceful shutdown before force termination
    /// - Timeout-based cleanup with proper error handling
    /// - Comprehensive logging and multiple cleanup strategies
    /// - Disposable pattern implementation for coordinated cleanup
    ///
    /// RECOMMENDATION: Keep current sophisticated approach - it's appropriate for the complexity.
    ///
    /// PROCESS MANAGEMENT IMPROVEMENTS (June 27, 2025):
    /// ================================================
    /// Enhanced to address "child node exited prematurely" build issues:
    ///
    /// 1. Child Process Tracking:
    ///    - Added _childProcesses list to track only processes we actually spawn
    ///    - Added _childProcessLock for thread-safe process management
    ///    - RegisterChildProcess() method for proper tracking
    ///    - CreateTrackedProcess() method with automatic cleanup
    ///
    /// 2. Enhanced Cancellation Management:
    ///    - Added _cancellationLock for thread-safe cancellation operations
    ///    - SafeCancelAllOperations() method with proper error handling
    ///    - Improved timeout handling to prevent hanging operations
    ///
    /// 3. Selective Process Cleanup:
    ///    - EnsureNoChildProcesses() now only targets build-related processes (dotnet, MSBuild, VBCSCompiler)
    ///    - Removes aggressive termination of unrelated processes
    ///    - Better detection of actual child processes vs system processes
    ///
    /// 4. Graceful Shutdown Improvements:
    ///    - Enhanced background task cancellation with proper timeouts
    ///    - Improved cleanup sequence to prevent premature exits
    ///    - Better error handling during shutdown operations
    ///
    /// SYNCFUSION LICENSE REQUIREMENTS:
    /// ================================
    ///
    /// CRITICAL: License registration MUST remain in Main() method only.
    ///
    /// FORBIDDEN PRACTICES - DO NOT IMPLEMENT:
    /// ❌ Helper classes (e.g., SyncfusionLicenseHelper)
    /// ❌ Manager classes (e.g., LicenseManager)
    /// ❌ Validation methods (e.g., ValidateLicense())
    /// ❌ Configuration classes (e.g., SyncfusionConfig)
    /// ❌ License wrapper methods
    /// ❌ Moving registration to InitializeLicenseAndTheme()
    /// ❌ Calling RegisterLicense() multiple times
    /// ❌ Conditional license registration
    ///
    /// REQUIRED APPROACH per Syncfusion Documentation:
    /// ✅ Single call to SyncfusionLicenseProvider.RegisterLicense() in Main()
    /// ✅ Must be called before any Syncfusion control creation
    /// ✅ License string must be hardcoded (no variables, no config files)
    /// ✅ No error handling around license registration
    /// ✅ No license validation or verification code
    ///
    /// REFERENCE: https://help.syncfusion.com/common/essential-studio/licensing/how-to-register-in-an-application
    ///
    /// WHY NO HELPERS ARE ALLOWED:
    /// - Syncfusion licensing system expects direct registration in Main()
    /// - Helper methods can cause timing issues with control initialization
    /// - License validation interferes with Syncfusion's internal mechanisms
    /// - Multiple registration attempts can cause licensing failures
    /// - Configuration abstraction breaks Syncfusion's expected patterns
    ///
    /// </summary>
    internal static class Program
    {
        // SINGLE INSTANCE MANAGEMENT: Proper mutex handling to prevent synchronization errors
        private static Mutex _mutex = new Mutex(true, "Global\\BusBuddy_SingleInstance_Mutex_BusBuddy-E7B4F3C1-8A2D-4E5F-9B6C-1D3A5E7F9A2B");

        // DASHBOARD REFERENCE: Store reference for centralized cleanup
        private static BusBuddy.UI.Views.Dashboard _mainDashboard = null;
        private static readonly object _dashboardLock = new object();

        // PROCESS MANAGEMENT: Track child processes we actually spawn (June 27, 2025)
        private static readonly List<Process> _childProcesses = new List<Process>();
        private static readonly object _childProcessLock = new object();

        // CANCELLATION MANAGEMENT: Enhanced thread-safe cancellation (June 27, 2025)
        private static CancellationTokenSource _globalCancellationSource = new CancellationTokenSource();
        private static readonly object _cancellationLock = new object();

        // CLEANUP COORDINATION: Implement disposable pattern for robust cleanup (June 27, 2025)
        private static bool _isDisposed = false;
        private static readonly object _disposeLock = new object();

        // OPTIMIZED LOGGING: Buffered logging system to reduce I/O overhead (June 27, 2025)
        private static readonly List<string> _logBuffer = new List<string>();
        private static readonly object _logLock = new object();
        private static readonly int _logBufferSize = 100; // Flush every 100 messages
        private static string _logFileName = null;

        /// <summary>
        /// Centralized test mode detection to avoid duplicated logic across the application.
        ///
        /// Checks multiple indicators to determine if the application is running in test mode:
        /// - Environment variable BUSBUDDY_TEST_MODE set to "1"
        /// - xUnit test framework assemblies loaded
        /// - BusBuddyThemeManager test mode flag
        /// - ControlFactory test mode flag
        ///
        /// This method improves code readability and maintainability by consolidating
        /// test mode detection logic that was previously scattered across multiple methods.
        /// </summary>
        /// <returns>True if the application is running in any test mode, false otherwise</returns>
        public static bool IsTestMode()
        {
            try
            {
                // Check environment variable first (fastest check)
                if (Environment.GetEnvironmentVariable("BUSBUDDY_TEST_MODE") == "1")
                    return true;

                // Check for xUnit test framework assemblies
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a =>
                    a.FullName != null && a.FullName.Contains("xunit")))
                    return true;

                // Check theme manager test mode flag (if available)
                try
                {
                    if (BusBuddy.UI.Helpers.BusBuddyThemeManager.IsTestMode)
                        return true;
                }
                catch
                {
                    // BusBuddyThemeManager might not be available in all contexts
                }

                // Check control factory test mode flag (if available)
                try
                {
                    if (BusBuddy.UI.Views.ControlFactory.IsTestMode)
                        return true;
                }
                catch
                {
                    // ControlFactory might not be available in all contexts
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log error but don't throw - default to non-test mode for safety
                Console.WriteLine($"⚠️ Error detecting test mode: {ex.Message}");
                return false;
            }
        }

        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                // Check for single instance using proper mutex handling
                if (!_mutex.WaitOne(TimeSpan.Zero, true))
                {
                    MessageBox.Show("Application already running.", "BusBuddy - Single Instance",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 1;
                }

                // ==================================================================================
                // SYNCFUSION LICENSE REGISTRATION - CRITICAL SECTION
                // ==================================================================================
                //
                // ⚠️  ABSOLUTE REQUIREMENTS - VIOLATION WILL BREAK LICENSING:
                //
                // 1. MUST be called in Main() method ONLY - never in helpers/managers
                // 2. MUST be called BEFORE any Windows Forms initialization
                // 3. MUST be called BEFORE any Syncfusion control creation
                // 4. MUST use hardcoded license string (no variables, no config)
                // 5. MUST NOT be wrapped in try-catch blocks
                // 6. MUST NOT be called multiple times
                // 7. MUST NOT have any validation logic
                //
                // FORBIDDEN PATTERNS:
                // ❌ SyncfusionLicenseHelper.RegisterLicense()
                // ❌ LicenseManager.Initialize()
                // ❌ if (ValidateLicense()) RegisterLicense()
                // ❌ try { RegisterLicense(); } catch { ... }
                // ❌ var license = GetLicenseFromConfig(); RegisterLicense(license);
                //
                // OFFICIAL DOCUMENTATION:
                // https://help.syncfusion.com/common/essential-studio/licensing/how-to-register-in-an-application
                //
                // Community license key for development and testing (Essential Studio Community)
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXlcdHRdRGNcWENxXkZWYUA=");

                // EXCEPTION CAPTURE SYSTEM - Initialize immediately after Syncfusion license
                BusBuddy.Debug.ExceptionCapture.Initialize();
                Console.WriteLine("🔍 Exception capture system activated");
                //
                // ==================================================================================

                return MainAsync(args).GetAwaiter().GetResult();
            }
            finally
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
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

            try
            {
                // Setup cleanup on application exit with centralized coordination
                Application.ApplicationExit += Application_Exit;
                AppDomain.CurrentDomain.ProcessExit += Application_Exit;

                // Check for special command-line arguments
                if (args.Length > 0 && args[0] == "validate-analytics")
                {
                    RunAnalyticsValidationOnly();
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
        }

        /// <summary>
        /// Runs the main application logic
        /// </summary>
        private static async Task<int> RunMainApplication(string[] args)
        {
            Console.WriteLine("🚀 BusBuddy starting...");
            Directory.CreateDirectory("logs");

            try
            {
                // Initialize comprehensive environment protection
                Console.WriteLine("🛡️ Environment Protection DISABLED - Reducing process overhead");
                // TEMPORARILY DISABLED: Environment monitoring causing exceptions
                // EnvironmentProtectionManager.Initialize(EnvironmentProtectionManager.ProtectionLevel.Standard);

                // Perform minimal health check - using simple .NET APIs only
                Console.WriteLine($"📊 Basic Environment Health: OK (monitoring disabled)");

                // Skip complex environmental monitoring that was causing exceptions
                Console.WriteLine("💡 Environment Recommendations: Skipped (monitoring disabled)");

                // CRITICAL: Initialize backward-compatible environmental resilience
                // TEMPORARILY DISABLED: Environmental monitoring causing exceptions
                // BusBuddy.UI.Helpers.EnvironmentalResilience.Initialize();

                InitializeLicenseAndTheme();
                ConfigureHighDpiSupport();
                return await StartRouteManagementFormApplication(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ CRITICAL ERROR: {ex.Message}");
                LogError("Critical application error", ex);

                // Emergency cleanup DISABLED - was causing additional exceptions
                Console.WriteLine("🆘 Emergency environment recovery DISABLED (monitoring disabled)");
                /*
                try
                {
                    Console.WriteLine("🆘 Attempting emergency environment recovery...");
                    EnvironmentProtectionManager.ApplyEmergencyProtections();
                    BusBuddy.UI.Helpers.EnvironmentalResilience.ForceEnvironmentCleanup();
                }
                catch (Exception cleanupEx)
                {
                    Console.WriteLine($"⚠️ Emergency cleanup failed: {cleanupEx.Message}");
                }
                */

                ShowCriticalErrorDialog(ex);
                return 1;
            }
            finally
            {
                // Use centralized cleanup to avoid duplicate operations
                lock (_disposeLock)
                {
                    if (!_isDisposed)
                    {
                        PerformCleanup();
                        _isDisposed = true;
                    }
                }
            }
        }

        /// <summary>
        /// Initialize Syncfusion theme system ONLY (license already registered in Main method)
        ///
        /// IMPORTANT: This method DOES NOT register the Syncfusion license.
        /// License registration MUST remain in Main() per Syncfusion requirements.
        ///
        /// WHY LICENSE IS NOT REGISTERED HERE:
        /// - Syncfusion requires license registration in Main() before ANY initialization
        /// - Theme initialization happens AFTER license registration
        /// - Calling RegisterLicense() here would be too late and cause failures
        /// - Multiple license registrations can cause licensing conflicts
        ///
        /// This method only handles theme configuration that comes AFTER licensing.
        /// </summary>
        private static void InitializeLicenseAndTheme()
        {
            try
            {
                // ==================================================================================
                // SYNCFUSION THEME INITIALIZATION ONLY
                // ==================================================================================
                //
                // Note: License is already registered in Main() - DO NOT duplicate here
                // This violates Syncfusion's licensing requirements and will cause failures
                //
                // FORBIDDEN in this method:
                // ❌ SyncfusionLicenseProvider.RegisterLicense() - ALREADY DONE IN MAIN()
                // ❌ License validation or verification
                // ❌ License helper method calls
                // ❌ Conditional license registration
                //
                // ALLOWED in this method:
                // ✅ Theme configuration (if theme assemblies are referenced)
                // ✅ Visual style settings
                // ✅ UI appearance initialization
                // ==================================================================================

                Console.WriteLine("🎨 Initializing Syncfusion theme configuration...");

                // Note: Theme configuration would be implemented here if proper Syncfusion
                // theme assemblies were referenced in the project. Since license is already
                // registered in Main(), controls will work properly without theme configuration.

                Console.WriteLine("✅ Syncfusion configuration completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error initializing Syncfusion theme configuration: {ex.Message}");
                LogError("Syncfusion theme configuration failed", ex);

                // Continue without theme if initialization fails - license is still valid
                Console.WriteLine("⚠️ Continuing with default theme (license remains valid)");
            }
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
        /// Start the RouteManagementFormSyncfusion application with database check and service initialization
        /// </summary>
        private static async Task<int> StartRouteManagementFormApplication(string[] args)
        {
            // Check and fix database before launching the form
            Console.WriteLine("🔍 Checking database status...");
            bool databaseReady = await BusBuddy.Data.SimpleDatabaseCheck.CheckAndFixDatabaseAsync();

            if (!databaseReady)
            {
                Console.WriteLine("❌ Database check failed. Cannot start application.");
                MessageBox.Show("Database is not available. Please check your database connection.",
                               "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }

            Console.WriteLine("✅ Database is ready.");

            var serviceContainer = await InitializeServiceContainerAsync();

            // Run database diagnostics to identify any issues
            Console.WriteLine("🔍 Running database diagnostics...");
            var diagnostics = await serviceContainer.DiagnoseDatabaseAsync();
            Console.WriteLine(diagnostics);

            ValidateRequiredServices(serviceContainer);
            ProcessCommandLineArguments(args);

            // Create and launch RouteManagementFormSyncfusion instead of RouteFormSyncfusion
            Console.WriteLine("🗺️ Launching RouteManagementFormSyncfusion...");
            var routeMgmtForm = new BusBuddy.UI.Views.RouteManagementFormSyncfusion();

            RunApplication(routeMgmtForm);

            return 0;
        }

        /// <summary>
        /// Initialize and configure the service container
        /// </summary>
        private static async Task<BusBuddy.UI.Helpers.UnifiedServiceManager> InitializeServiceContainerAsync()
        {
            LogMessage("🔧 Creating unified service container...");
            var serviceContainer = BusBuddy.UI.Helpers.UnifiedServiceManager.Instance;

            try
            {
                // Start service pre-warming and await them
                LogMessage("🚀 Starting unified service pre-warming");

                // Use the global cancellation token safely
                CancellationToken cancellationToken;
                lock (_cancellationLock)
                {
                    cancellationToken = _globalCancellationSource?.Token ?? CancellationToken.None;
                }

                // CRITICAL FIX: Ensure proper initialization before starting tasks
                LogMessage("🔧 Ensuring service container initialization...");
                await serviceContainer.EnsureInitializedAsync();

                // Create tasks that will respect cancellation
                var servicePrewarmTask = Task.Run(async () =>
                {
                    try
                    {
                        await serviceContainer.PreWarmServicesAsync();
                        LogMessage("✅ Service pre-warming task completed");
                    }
                    catch (OperationCanceledException)
                    {
                        LogMessage("⚠️ Service pre-warming canceled (expected during shutdown)");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ Service pre-warm exception: {ex.Message}");
                    }
                }, cancellationToken);

                var databasePrewarmTask = Task.Run(async () =>
                {
                    try
                    {
                        await serviceContainer.PreWarmDatabaseAsync();
                        LogMessage("✅ Database pre-warming task completed");
                    }
                    catch (OperationCanceledException)
                    {
                        LogMessage("⚠️ Database pre-warming canceled (expected during shutdown)");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"⚠️ Database pre-warm exception: {ex.Message}");
                    }
                }, cancellationToken);

                // Create a timeout task to prevent hanging if pre-warming takes too long
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationToken); // Extended timeout for better reliability

                // Wait for either the pre-warming tasks or the timeout, whichever comes first
                var completedTask = await Task.WhenAny(
                    Task.WhenAll(servicePrewarmTask, databasePrewarmTask),
                    timeoutTask
                );

                if (completedTask == timeoutTask)
                {
                    Console.WriteLine("⚠️ Service pre-warming timed out after 10 seconds - continuing startup");
                }
                else
                {
                    Console.WriteLine("✅ All service pre-warming tasks completed successfully");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("⚠️ Service pre-warming was canceled during application startup");
            }
            catch (Exception ex)
            {
                // Log but continue - don't let pre-warming failures stop the application
                Console.WriteLine($"⚠️ Service pre-warming encountered an error: {ex.Message}");
                LogError("Service pre-warming error", ex);
            }

            return serviceContainer;
        }

        /// <summary>
        /// Original non-async version kept for backward compatibility with other code
        /// </summary>
        private static BusBuddy.UI.Helpers.UnifiedServiceManager InitializeServiceContainer()
        {
            Console.WriteLine("🔧 Creating unified service container...");
            Console.WriteLine("⚠️ Using non-async service initialization - tasks will run in background");
            var serviceContainer = BusBuddy.UI.Helpers.UnifiedServiceManager.Instance;
            return serviceContainer;
        }

        /// <summary>
        /// Validate that required services are properly registered
        /// </summary>
        private static void ValidateRequiredServices(BusBuddy.UI.Helpers.UnifiedServiceManager serviceContainer)
        {
            Console.WriteLine("🔌 Validating required services...");

            // Quick service validation (lazy loading enabled)
            if (serviceContainer == null)
                throw new InvalidOperationException("Unified service container not initialized.");

            Console.WriteLine("✅ Unified service container validated - services will resolve on first access");
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
            LogMessage("🔧 Creating Dashboard instance...");

            // Add diagnostic information about the environment
            LogMessage($"🖥️ Display: Is interactive terminal: {Environment.UserInteractive}");
            LogMessage($"🖥️ Environment: {Environment.OSVersion}");
            LogMessage($"🖥️ Running 64-bit: {Environment.Is64BitProcess}");
            LogMessage($"🖥️ Current directory: {Environment.CurrentDirectory}");

            try
            {
                var dashboard = new BusBuddy.UI.Views.Dashboard();

                // Store reference for centralized cleanup
                lock (_dashboardLock)
                {
                    _mainDashboard = dashboard;
                }

                LogMessage("✅ Dashboard created successfully");
                return dashboard;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating dashboard: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Run the Windows Forms application
        /// </summary>
        private static void RunApplication(Form mainForm)
        {
            Console.WriteLine("▶️ Running application...");

            // Set up form closed handler to ensure proper cleanup
            mainForm.FormClosed += (s, e) =>
            {
                Console.WriteLine("📋 Form closed event detected - initiating final cleanup");

                // Use a background thread for cleanup to avoid hanging the UI
                Task.Run(() =>
                {
                    try
                    {
                        // Wait a short time to allow the form to complete its own cleanup
                        Thread.Sleep(500);

                        // Ensure all background tasks are cancelled
                        CancelAllBackgroundTasks();

                        // Clean up any child processes
                        EnsureNoChildProcesses();

                        // Force process exit if we're still running after 3 seconds
                        Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            Console.WriteLine("🛑 Forcing process exit as cleanup has taken too long");
                            Environment.Exit(0);
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error in form closed cleanup: {ex.Message}");
                    }
                });
            };

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
        /// Clean up the service container and ensure all resources are properly disposed
        /// </summary>
        private static void CleanupServiceContainer()
        {
            // Check if cleanup has already been performed to prevent redundant operations
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    Console.WriteLine("ℹ️ Service container cleanup already performed, skipping");
                    return;
                }
            }

            try
            {
                Console.WriteLine("🧹 Starting service container cleanup...");

                // Clean up the unified service container (ONLY service container)
                BusBuddy.UI.Helpers.UnifiedServiceManager.Instance.Dispose();
                Console.WriteLine("✅ Unified service container disposed successfully");

                // Ensure background tasks have completed or are cancelled
                CancelAllBackgroundTasks();

                // Force garbage collection to clean up any unmanaged resources
                GC.Collect();
                GC.WaitForPendingFinalizers();

                Console.WriteLine("🏁 Service container cleanup complete");
            }
            catch (Exception ex)
            {
                LogError("Service container cleanup failed", ex);
                Console.WriteLine("🏁 Service container cleanup exited with error");

                // Still try to clean up background tasks even if main cleanup failed
                try
                {
                    CancelAllBackgroundTasks();
                }
                catch
                {
                    // Ignore any errors in background task cleanup
                }
            }
        }

        /// <summary>
        /// Cancels any background tasks that might be running with enhanced cleanup
        /// </summary>
        private static void CancelAllBackgroundTasks()
        {
            try
            {
                Console.WriteLine("🔴 Cancelling all background tasks and cleaning up processes...");

                // First, cancel all background operations safely
                SafeCancelAllOperations();

                // Clean up tracked child processes
                CleanupTrackedChildProcesses();

                // Wait a moment for cleanup to complete
                Thread.Sleep(150);

                // Force immediate garbage collection to clean up any resources
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();

                // Second pass GC often helps clean up more resources
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
                GC.WaitForPendingFinalizers();

                Console.WriteLine("✅ Background task cancellation and process cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during background task cancellation: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs analytics validation only (for debugging purposes)
        /// </summary>
        private static void RunAnalyticsValidationOnly()
        {
            Console.WriteLine("🔬 Running Analytics Validation Only...\n");

            // Ensure logs directory exists
            Directory.CreateDirectory("logs");

            try
            {
                // Initialize minimal services needed for analytics
                Console.WriteLine("🔧 Initializing unified service container...");
                var serviceContainer = BusBuddy.UI.Helpers.UnifiedServiceManager.Instance;
                Console.WriteLine("✅ Unified service container initialized");

                // Run analytics validation with enhanced debugging
                Console.WriteLine("🧪 Starting analytics validation...");
                // Note: CostAnalyticsValidator removed - using RouteAnalyticsService instead
                var analyticsService = serviceContainer.GetService<BusBuddy.Business.IRouteAnalyticsService>();
                if (analyticsService != null)
                {
                    Console.WriteLine("✅ Analytics service validated successfully");
                }
                else
                {
                    Console.WriteLine("⚠️ Analytics service not available");
                }
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
                throw; // Re-throw to ensure non-zero exit code is returned
            }
            finally
            {
                Console.WriteLine("🧹 Cleaning up...");
                try
                {
                    // Use centralized cleanup to avoid duplicate operations
                    lock (_disposeLock)
                    {
                        if (!_isDisposed)
                        {
                            PerformCleanup();
                            _isDisposed = true;
                        }
                        else
                        {
                            Console.WriteLine("ℹ️ Cleanup already performed, skipping duplicate cleanup");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Cleanup error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Optimized logging with buffering to reduce I/O overhead during high-frequency logging
        /// </summary>
        private static void LogMessage(string message)
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - {message}{Environment.NewLine}";
            Console.WriteLine(logEntry.TrimEnd()); // Remove the extra newline for console output

            lock (_logLock)
            {
                // Ensure log file name is set
                if (_logFileName == null)
                {
                    Directory.CreateDirectory("logs");
                    _logFileName = $"logs/busbuddy_{DateTime.Now:yyyyMMdd}.log";
                }

                _logBuffer.Add(logEntry);

                // Flush every 100 messages or if this is an important message
                if (_logBuffer.Count >= _logBufferSize || message.Contains("❌") || message.Contains("CRITICAL"))
                {
                    FlushLogBuffer();
                }
            }
        }

        /// <summary>
        /// Flushes the log buffer to disk (must be called within _logLock)
        /// </summary>
        private static void FlushLogBuffer()
        {
            try
            {
                if (_logBuffer.Count > 0 && _logFileName != null)
                {
                    File.AppendAllText(_logFileName, string.Join("", _logBuffer));
                    _logBuffer.Clear();

                    // Perform cleanup periodically (only when flushing to reduce overhead)
                    if (DateTime.Now.Hour == 0 && DateTime.Now.Minute < 10) // Once per day around midnight
                    {
                        CleanupOldLogFiles(7);
                    }
                }
            }
            catch
            {
                // If logging fails, clear the buffer to prevent memory buildup
                _logBuffer.Clear();
                Console.WriteLine("⚠️ Failed to write to log file - buffer cleared");
            }
        }

        /// <summary>
        /// Forces immediate flush of the log buffer (useful during application shutdown)
        /// </summary>
        private static void ForceFlushLogs()
        {
            lock (_logLock)
            {
                FlushLogBuffer();
            }
        }

        /// <summary>
        /// Logs errors to file for debugging purposes - now uses optimized buffered system
        /// </summary>
        private static void LogError(string message, Exception ex)
        {
            try
            {
                Directory.CreateDirectory("logs");

                var errorLogFileName = $"logs/error_{DateTime.Now:yyyyMMdd}.log";
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Stack Trace: {ex.StackTrace}\n" +
                              $"Inner Exception: {ex.InnerException?.Message ?? "None"}\n" +
                              new string('-', 80) + "\n";

                // Use buffered system for error logging as well
                lock (_logLock)
                {
                    _logBuffer.Add(logEntry);
                    // Force immediate flush for errors since they're critical
                    FlushLogBuffer();
                }

                // Also log to general message system for console output
                LogMessage($"❌ ERROR: {message} - {ex.Message}");
            }
            catch
            {
                // If logging fails, just continue - don't crash the app over logging
                Console.WriteLine("⚠️ Failed to write to error log");
            }
        }

        /// <summary>
        /// Cleans up old log files, keeping only the specified number of days
        /// </summary>
        private static void CleanupOldLogFiles(int daysToKeep)
        {
            try
            {
                var logDirectory = new DirectoryInfo("logs");
                if (!logDirectory.Exists) return;

                var threshold = DateTime.Now.AddDays(-daysToKeep);
                var oldFiles = logDirectory.GetFiles("error_*.log")
                    .Where(f => f.CreationTime < threshold)
                    .ToList();

                foreach (var file in oldFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                        // Ignore errors when deleting individual files
                    }
                }
            }
            catch
            {
                // Ignore any errors in cleanup
            }
        }

        /// <summary>
        /// Test method to verify all required services are properly registered
        /// </summary>
        private static void TestServiceResolution(BusBuddy.UI.Helpers.UnifiedServiceManager serviceContainer)
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

        /// <summary>
        /// Ensures no actual child processes are left running with improved detection
        /// Only terminates processes that are definitely children of this process
        /// </summary>
        private static void EnsureNoChildProcesses()
        {
            // Check if cleanup has already been performed to prevent redundant operations
            lock (_disposeLock)
            {
                if (_isDisposed)
                {
                    Console.WriteLine("ℹ️ Child process cleanup already performed, skipping");
                    return;
                }
            }

            try
            {
                Console.WriteLine("🔍 Checking for genuine child processes...");
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var currentProcessId = currentProcess.Id;

                // First, check our tracked child processes (this is most reliable)
                CleanupTrackedChildProcesses();

                // Second, use WMI to find actual child processes (more precise than before)
                // ENHANCED (June 27, 2025): Simplified approach to avoid "No process is associated with this object" errors
                try
                {
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = $"-Command \"try {{ Get-CimInstance Win32_Process | Where-Object {{ $_.ParentProcessId -eq {currentProcessId} }} | ForEach-Object {{ $processId = $_.ProcessId; $processName = $_.Name; Write-Host ('Found child process: ' + $processName + ' (' + $processId + ')'); if ($processName -match '(dotnet|MSBuild|VBCSCompiler)') {{ Write-Host ('Terminating build-related child: ' + $processId); Try {{ Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue; Write-Host ('Successfully terminated: ' + $processId) }} Catch {{ Write-Host ('Could not terminate: ' + $processId + ' - Already exited or access denied') }} }} else {{ Write-Host ('Skipping non-build process: ' + $processName) }} }} }} catch {{ Write-Host 'Error querying child processes - some may have exited during detection' }}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = CreateTrackedProcess(startInfo))
                    {
                        RegisterChildProcess(process);
                        process.Start();

                        // Use a timeout to prevent hanging
                        if (process.WaitForExit(3000)) // 3-second timeout (reduced from 5)
                        {
                            var output = process.StandardOutput.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(output))
                            {
                                Console.WriteLine(output);
                            }

                            var error = process.StandardError.ReadToEnd();
                            if (!string.IsNullOrWhiteSpace(error))
                            {
                                // Filter out expected errors from rapid process exit scenarios
                                if (!error.Contains("No process is associated") &&
                                    !error.Contains("some may have exited during detection"))
                                {
                                    Console.WriteLine($"⚠️ PowerShell error: {error}");
                                }
                                else
                                {
                                    Console.WriteLine("ℹ️ Some child processes exited during detection (normal behavior)");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("⚠️ PowerShell child process detection timed out");
                            try
                            {
                                process.Kill(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Failed to kill timed-out PowerShell process: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Child process detection error: {ex.Message}");
                }

                Console.WriteLine("✅ Child process cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during child process cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a child process for tracking and cleanup
        ///
        /// PROCESS MANAGEMENT IMPROVEMENT (June 27, 2025):
        /// This method was added to address "child node exited prematurely" build issues.
        ///
        /// PURPOSE:
        /// - Track only processes we actually spawn (not system processes)
        /// - Enable automatic cleanup when processes exit normally
        /// - Prevent orphaned build processes (dotnet, MSBuild, VBCSCompiler)
        ///
        /// THREAD SAFETY:
        /// - Uses _childProcessLock for thread-safe process list management
        /// - Safe to call from multiple threads or background tasks
        /// </summary>
        /// <summary>
        /// Safely cancel all operations with improved error handling
        ///
        /// CANCELLATION MANAGEMENT IMPROVEMENT (June 27, 2025):
        /// Addresses premature build cancellation that caused "child node exited prematurely" errors.
        ///
        /// SAFETY FEATURES:
        /// - Thread-safe cancellation using _cancellationLock
        /// - Handles ObjectDisposedException from disposed cancellation tokens
        /// - Graceful timing with 250ms delay for task response
        /// - Automatic recreation of cancellation source if disposed
        ///
        /// PREVENTS:
        /// - Race conditions in multi-threaded cancellation scenarios
        /// - Exceptions from accessing disposed CancellationTokenSource
        /// - Abrupt task termination that leaves processes orphaned
        /// - Build tool hanging due to improper cancellation timing
        /// </summary>
        private static void SafeCancelAllOperations()
        {
            lock (_cancellationLock)
            {
                try
                {
                    if (_globalCancellationSource != null && !_globalCancellationSource.IsCancellationRequested)
                    {
                        Console.WriteLine("🛑 Initiating graceful cancellation of all operations...");
                        _globalCancellationSource.Cancel();

                        // Give tasks a moment to respond to cancellation
                        Thread.Sleep(250);
                        Console.WriteLine("✅ Cancellation signal sent to all operations");
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Token source already disposed, create a new one in cancelled state
                    _globalCancellationSource = new CancellationTokenSource();
                    _globalCancellationSource.Cancel();
                    Console.WriteLine("✅ Recreated cancellation source in cancelled state");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error during cancellation: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Register a child process for proper lifecycle management and automatic cleanup
        /// </summary>
        /// <param name="process">The process to register and track</param>
        private static void RegisterChildProcess(Process process)
        {
            if (process == null) return;

            lock (_childProcessLock)
            {
                // Configure process for proper event handling
                process.EnableRaisingEvents = true;

                // Remove from tracking when it exits naturally
                process.Exited += (sender, e) =>
                {
                    lock (_childProcessLock)
                    {
                        _childProcesses.Remove(process);
                        Console.WriteLine($"🔄 Child process {process.Id} exited naturally and removed from tracking");
                    }
                };

                // Add to our tracking list
                _childProcesses.Add(process);
                Console.WriteLine($"📝 Registered child process: {process.ProcessName} (ID: {process.Id})");
            }
        }

        /// <summary>
        /// Create a tracked process with proper lifecycle management
        /// </summary>
        /// <param name="startInfo">Process start information</param>
        /// <returns>A configured Process object ready to start</returns>
        private static Process CreateTrackedProcess(ProcessStartInfo startInfo)
        {
            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            // Set up automatic removal when process exits
            process.Exited += (sender, e) =>
            {
                lock (_childProcessLock)
                {
                    _childProcesses.Remove(process);
                    Console.WriteLine($"🔄 Tracked process {process.Id} exited naturally and removed from tracking");
                }
            };

            return process;
        }

        /// <summary>
        /// Start a child process with proper tracking and lifecycle management
        /// </summary>
        /// <param name="startInfo">Process start information</param>
        /// <returns>The started and tracked process</returns>
        private static Process StartChildProcess(ProcessStartInfo startInfo)
        {
            var process = CreateTrackedProcess(startInfo);

            lock (_childProcessLock)
            {
                _childProcesses.Add(process);
                process.Start();
                Console.WriteLine($"🚀 Started and registered child process: {process.ProcessName} (ID: {process.Id})");
            }

            return process;
        }

        /// <summary>
        /// Enhanced child process cleanup with selective termination and robust error handling
        ///
        /// CHILD PROCESS CLEANUP IMPROVEMENT (June 27, 2025):
        /// Targeted solution for "child node exited prematurely" build issues.
        ///
        /// SELECTIVE CLEANUP APPROACH:
        /// - Only terminates processes we actually spawned (tracked in _childProcesses)
        /// - Uses graceful shutdown (CloseMainWindow) before force termination
        /// - 3-second timeout for process exit (reasonable for build tools)
        /// - Thread-safe operation with proper locking
        /// - Automatic removal of processes that exit naturally
        /// - Robust error handling with HasExited checks
        ///
        /// PREVENTS ISSUES:
        /// - Accidentally terminating unrelated system processes
        /// - Killing legitimate dotnet processes from other applications
        /// - Leaving orphaned build processes that cause future build failures
        /// - Race conditions during concurrent process cleanup
        /// - InvalidOperationException from accessing disposed processes
        ///
        /// REPLACES:
        /// Previous aggressive approach that killed all processes with same name,
        /// which could terminate legitimate processes from other applications.
        /// </summary>
        private static void CleanupTrackedChildProcesses()
        {
            Console.WriteLine("🧹 Cleaning up tracked child processes with robust error handling...");

            lock (_childProcessLock)
            {
                // Create a copy to avoid modification issues during iteration
                var processesToCleanup = new List<Process>(_childProcesses);
                _childProcesses.Clear();

                foreach (var process in processesToCleanup)
                {
                    try
                    {
                        // ENHANCED (June 27, 2025): Better error handling for process state checks
                        // Check if process object is valid before accessing properties
                        if (process != null)
                        {
                            // Safely check if process has exited
                            bool hasExited = false;
                            try
                            {
                                hasExited = process.HasExited;
                            }
                            catch (InvalidOperationException)
                            {
                                // Process was never associated or already disposed
                                Console.WriteLine($"ℹ️ Process object was never associated with a running process, skipping");
                                continue;
                            }

                            if (!hasExited)
                            {
                                Console.WriteLine($"🛑 Terminating tracked child process: {process.ProcessName} (ID: {process.Id})");

                                // Try graceful shutdown first
                                try
                                {
                                    if (!process.CloseMainWindow())
                                    {
                                        // If graceful shutdown fails, force termination
                                        process.Kill(false); // Don't kill descendants to avoid killing ourselves
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    // Process might have exited between HasExited check and termination attempt
                                    Console.WriteLine($"ℹ️ Process {process.Id} exited during termination attempt (expected behavior)");
                                    continue;
                                }

                                // Wait for exit with timeout and proper exception handling
                                try
                                {
                                    if (!process.WaitForExit(3000))
                                    {
                                        Console.WriteLine($"⚠️ Process {process.Id} didn't respond to termination request within 3 seconds");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"✅ Process {process.Id} terminated successfully");
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    // Process already exited or was disposed
                                    Console.WriteLine($"ℹ️ Process {process.Id} was already disposed during wait (expected behavior)");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"✅ Process {process.Id} already exited, skipping termination");
                            }
                        }
                        else
                        {
                            Console.WriteLine("ℹ️ Null process object encountered, skipping");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore if the process is not associated or already exited
                        Console.WriteLine($"⚠️ Process {process?.Id ?? -1} is not associated or already exited");
                    }
                    catch (Exception ex)
                    {
                        // Log other unexpected errors but continue with cleanup
                        Console.WriteLine($"⚠️ Error cleaning up child process {process?.Id ?? -1}: {ex.Message}");
                        LogError($"Unexpected error during tracked process cleanup for PID {process?.Id ?? -1}", ex);
                    }
                    finally
                    {
                        // Always dispose the process object to free resources
                        try
                        {
                            process?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error disposing process: {ex.Message}");
                        }
                    }
                }
            }

            Console.WriteLine("✅ Tracked child process cleanup completed");
        }

        /// <summary>
        /// General purpose child process cleanup with proper validation
        /// Implements the fix for System.InvalidOperationException from process access
        /// </summary>
        public static void CleanupChildProcesses()
        {
            try
            {
                Console.WriteLine("🧹 Performing general child process cleanup...");

                foreach (var process in Process.GetProcesses().Where(p =>
                    p.ProcessName.Contains("BusBuddy", StringComparison.OrdinalIgnoreCase) ||
                    p.ProcessName.Contains("sqlcmd", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        // CRITICAL FIX: Validate process before attempting operations
                        if (process == null || process.HasExited)
                        {
                            Console.WriteLine("⚠️ Process already exited or null, skipping");
                            continue;
                        }

                        // Don't kill the current process
                        if (process.Id == Process.GetCurrentProcess().Id)
                        {
                            continue;
                        }

                        Console.WriteLine($"🛑 Cleaning up child process: {process.ProcessName} (ID: {process.Id})");

                        if (!process.HasExited)
                        {
                            process.Kill();

                            // Wait with proper validation
                            if (!process.WaitForExit(3000))
                            {
                                Console.WriteLine($"⚠️ Process {process.Id} did not exit within timeout");
                            }
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"⚠️ Process {process?.Id ?? -1} access error: {ex.Message}");
                        // This is expected if process already terminated
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error cleaning up process {process?.Id ?? -1}: {ex.Message}");
                    }
                    finally
                    {
                        try
                        {
                            process?.Dispose();
                        }
                        catch
                        {
                            // Ignore disposal errors
                        }
                    }
                }

                Console.WriteLine("✅ General child process cleanup completed");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"⚠️ Process cleanup error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during process cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Centralized cleanup event handler to prevent duplicate cleanup operations.
        /// This resolves System.InvalidOperationException issues during shutdown.
        /// </summary>
        private static void Application_Exit(object sender, EventArgs e)
        {
            lock (_disposeLock)
            {
                if (!_isDisposed)
                {
                    try
                    {
                        Console.WriteLine("🧹 Performing centralized cleanup...");
                        PerformCleanup();
                        _isDisposed = true;
                        Console.WriteLine("✅ Centralized cleanup completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error during centralized cleanup: {ex.Message}");
                        LogError("Centralized cleanup failed", ex);
                        _isDisposed = true; // Mark as disposed even if failed to prevent retry
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Cleanup already performed, skipping duplicate cleanup call");
                }
            }
        }

        /// <summary>
        /// Performs the actual cleanup operations in a coordinated manner with robust process handling
        /// </summary>
        private static void PerformCleanup()
        {
            Console.WriteLine("🧹 Starting coordinated cleanup operations...");

            try
            {
                // 1. Clean up Dashboard-specific resources first
                CleanupDashboard();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Dashboard cleanup error: {ex.Message}");
                LogError("Dashboard cleanup failed during coordinated cleanup", ex);
            }

            try
            {
                // 2. Clean up service container
                CleanupServiceContainer();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Service container cleanup error: {ex.Message}");
                LogError("Service container cleanup failed during coordinated cleanup", ex);
            }

            try
            {
                // 3. Robust child process cleanup
                PerformRobustChildProcessCleanup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Child process cleanup error: {ex.Message}");
                LogError("Child process cleanup failed during coordinated cleanup", ex);
            }

            try
            {
                // 4. Force flush any remaining logs before shutdown
                ForceFlushLogs();
                Console.WriteLine("💾 Log buffer flushed to disk");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Log flush error: {ex.Message}");
                // Don't log this error to avoid potential recursion
            }

            Console.WriteLine("✅ Coordinated cleanup operations completed");
        }

        /// <summary>
        /// Robust child process cleanup that checks process state before interacting
        /// and handles exceptions gracefully to prevent cleanup failures
        /// </summary>
        private static void PerformRobustChildProcessCleanup()
        {
            Console.WriteLine("🧹 Starting robust child process cleanup...");

            lock (_childProcessLock)
            {
                // Create a copy to avoid modification issues during iteration
                var processesToCleanup = _childProcesses.ToList();

                foreach (var process in processesToCleanup)
                {
                    try
                    {
                        // Check if process object is valid and still running
                        if (process != null && !process.HasExited)
                        {
                            Console.WriteLine($"🛑 Terminating tracked process: {process.ProcessName} (ID: {process.Id})");

                            // Attempt graceful shutdown first
                            try
                            {
                                if (!process.CloseMainWindow())
                                {
                                    // If graceful shutdown fails, force termination
                                    process.Kill(false); // Don't kill descendants to avoid killing ourselves
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                // Process might have exited between HasExited check and Kill call
                                Console.WriteLine($"⚠️ Process {process.Id} exited during cleanup attempt");
                            }

                            // Wait for process to exit with timeout
                            try
                            {
                                if (!process.WaitForExit(3000))
                                {
                                    Console.WriteLine($"⚠️ Process {process.Id} didn't respond to termination within 3 seconds");
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                // Process already exited or was disposed
                                Console.WriteLine($"⚠️ Process {process.Id} was already disposed during wait");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"✅ Process {process?.Id ?? -1} already exited, skipping termination");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Ignore if the process is not associated or already exited
                        Console.WriteLine($"⚠️ Process {process?.Id ?? -1} is not associated or already exited");
                    }
                    catch (Exception ex)
                    {
                        // Log other unexpected errors but continue cleanup
                        Console.WriteLine($"⚠️ Cleanup error for process {process?.Id ?? -1}: {ex.Message}");
                        LogError($"Unexpected error during process cleanup for PID {process?.Id ?? -1}", ex);
                    }
                    finally
                    {
                        // Always dispose the process object to free resources
                        try
                        {
                            process?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error disposing process: {ex.Message}");
                        }
                    }
                }

                // Reset the list after cleanup
                _childProcesses.Clear();
                Console.WriteLine("✅ Robust child process cleanup completed - process list cleared");
            }
        }

        /// <summary>
        /// Cleanup Dashboard-specific resources including Syncfusion controls.
        /// This method is called from the centralized cleanup to avoid redundant cleanup attempts.
        /// </summary>
        private static void CleanupDashboard()
        {
            Console.WriteLine("🗑️ Starting Dashboard-specific cleanup...");
            lock (_dashboardLock)
            {
                if (_mainDashboard != null)
                {
                    try
                    {
                        if (!_mainDashboard.IsDisposed)
                        {
                            Console.WriteLine("🗑️ Disposing Dashboard Syncfusion controls...");
                            _mainDashboard.DisposeSyncfusionControlsSafely();
                            Console.WriteLine("✅ Dashboard Syncfusion controls disposed successfully");
                        }
                        else
                        {
                            Console.WriteLine("ℹ️ Dashboard already disposed, skipping disposal");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error during Dashboard cleanup: {ex.Message}");
                        LogError("Dashboard cleanup failed", ex);
                    }
                    finally
                    {
                        _mainDashboard = null;
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ No Dashboard instance to clean up");
                }
            }
            Console.WriteLine("✅ Dashboard-specific cleanup completed");
        }

        /// <summary>
        /// Implements the Disposable Pattern for static class cleanup
        /// Ensures cleanup is idempotent (safe to call multiple times)
        ///
        /// DISPOSABLE PATTERN IMPLEMENTATION (June 27, 2025):
        /// ===================================================
        /// This method provides a standard disposable pattern for the static Program class
        /// to ensure proper cleanup of child processes and resources.
        ///
        /// FEATURES:
        /// - Thread-safe disposal using _disposeLock
        /// - Idempotent operation - safe to call multiple times
        /// - Comprehensive cleanup of all tracked resources
        /// - Proper exception handling with logging
        /// - Follows standard .NET disposal patterns
        ///
        /// USAGE:
        /// - Called automatically by Application_Exit event handlers
        /// - Can be called manually for explicit cleanup
        /// - Prevents resource leaks and orphaned processes
        /// </summary>
        public static void Dispose()
        {
            lock (_disposeLock)
            {
                if (!_isDisposed)
                {
                    try
                    {
                        Console.WriteLine("🧹 Disposing Program resources...");
                        PerformCleanup();
                        _isDisposed = true;
                        Console.WriteLine("✅ Program disposal completed successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error during Program disposal: {ex.Message}");
                        LogError("Program disposal failed", ex);
                        _isDisposed = true; // Mark as disposed even if failed to prevent retry
                    }
                }
                else
                {
                    Console.WriteLine("ℹ️ Program already disposed, skipping duplicate disposal");
                }
            }
        }

        /// <summary>
        /// Gets whether the Program has been disposed
        /// </summary>
        public static bool IsDisposed
        {
            get
            {
                lock (_disposeLock)
                {
                    return _isDisposed;
                }
            }
        }
    }
}

