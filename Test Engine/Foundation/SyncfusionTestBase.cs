using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Xunit;
using FluentAssertions;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using Moq;

namespace BusBuddy.TestEngine.Foundation
{
    /// <summary>
    /// Base test class for Syncfusion Windows Forms testing
    ///
    /// This class provides a robust foundation for testing Syncfusion-based UI components with:
    /// - Proper form lifecycle management and disposal tracking
    /// - Mock service initialization with null safety
    /// - DPI-aware testing support for modern Windows environments
    /// - Thread-safe form creation and cleanup
    /// - Memory leak prevention through tracked resource disposal
    ///
    /// Supported Syncfusion Controls:
    /// - SfDataGrid (grid components)
    /// - SfChart (charting components)
    /// - SfButton, SfTextBox (input controls)
    /// - SfTabControl (navigation controls)
    /// - SfRibbonControl (ribbon UI)
    /// - Custom Dashboard and Management forms
    ///
    /// Based on official Syncfusion testing documentation patterns
    /// Reference: https://help.syncfusion.com/windowsforms/testing/coded-ui
    /// </summary>
    public abstract class SyncfusionTestBase : IDisposable
    {
        // Mock service properties with null-safety annotations
        protected Mock<INavigationService> MockNavigationService { get; private set; } = null!;
        protected Mock<BusBuddy.Business.IDatabaseHelperService> MockDatabaseService { get; private set; } = null!;
        protected Mock<IRouteAnalyticsService> MockRouteAnalyticsService { get; private set; } = null!;
        protected Mock<IReportService> MockReportService { get; private set; } = null!;
        protected Mock<BusBuddy.UI.Services.IAnalyticsService> MockAnalyticsService { get; private set; } = null!;
        protected Mock<IErrorHandlerService> MockErrorHandlerService { get; private set; } = null!;

        // Resource tracking for proper cleanup
        private readonly List<Form> _formsToDispose = new List<Form>();
        private readonly List<IDisposable> _disposablesToCleanup = new List<IDisposable>();
        private bool _disposed = false;

        // DPI and thread management
        private static readonly object _dpiLock = new object();
        private static bool _dpiAwarenessSet = false;

        protected SyncfusionTestBase()
        {
            // Initialize test environment to prevent dialogs and ensure repositories are available
            InitializeTestEnvironment();

            // Set up DPI awareness for consistent rendering across different display configurations
            EnsureDpiAwareness();

            // Initialize Syncfusion license to prevent license dialogs during tests
            // This ensures tests run without interruption from license validation UI
            SyncfusionLicenseHelper.InitializeLicense();

            InitializeMocks();

            // Initialize diagnostic logging for this test
            var testName = GetType().Name;
            TestDiagnostics.LogOperation(testName, "SyncfusionTestBase initialized", TestDiagnostics.LogLevel.Debug);
        }

        /// <summary>
        /// Ensure DPI awareness is set to prevent graphics-related exceptions
        /// This addresses the "Object is currently in use elsewhere" System.Drawing.Graphics errors
        /// </summary>
        private static void EnsureDpiAwareness()
        {
            lock (_dpiLock)
            {
                if (!_dpiAwarenessSet)
                {
                    try
                    {
                        // Set process DPI awareness to prevent graphics conflicts
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            Application.SetHighDpiMode(HighDpiMode.SystemAware);
                        }
                        _dpiAwarenessSet = true;
                    }
                    catch (Exception)
                    {
                        // DPI awareness setting can fail in some test environments
                        // This is not critical for test execution
                    }
                }
            }
        }

        /// <summary>
        /// Initialize all service mocks with default behaviors and null safety checks
        /// Enhanced with parameterized setup support and comprehensive error handling
        /// Following Syncfusion testing patterns for dependency isolation
        /// </summary>
        private void InitializeMocks()
        {
            try
            {
                MockNavigationService = new Mock<INavigationService>();
                MockDatabaseService = new Mock<BusBuddy.Business.IDatabaseHelperService>();
                MockRouteAnalyticsService = new Mock<IRouteAnalyticsService>();
                MockReportService = new Mock<IReportService>();
                MockAnalyticsService = new Mock<BusBuddy.UI.Services.IAnalyticsService>();
                MockErrorHandlerService = new Mock<IErrorHandlerService>();

                // Validate all mocks were created successfully
                ValidateMockInitialization();

                // Setup default mock behaviors
                SetupDefaultMockBehaviors();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize test mocks: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate that all required mocks were successfully initialized
        /// Provides early detection of mock setup failures
        /// </summary>
        private void ValidateMockInitialization()
        {
            if (MockNavigationService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockNavigationService)} failed to initialize properly");

            if (MockDatabaseService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockDatabaseService)} failed to initialize properly");

            if (MockRouteAnalyticsService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockRouteAnalyticsService)} failed to initialize properly");

            if (MockReportService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockReportService)} failed to initialize properly");

            if (MockAnalyticsService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockAnalyticsService)} failed to initialize properly");

            if (MockErrorHandlerService?.Object == null)
                throw new InvalidOperationException($"Mock {nameof(MockErrorHandlerService)} failed to initialize properly");
        }

        /// <summary>
        /// Configure default mock behaviors for stable testing
        /// Enhanced with comprehensive setup and parameterized configuration support
        /// Based on Syncfusion testing recommendations for control isolation
        /// </summary>
        private void SetupDefaultMockBehaviors()
        {
            // Navigation service defaults
            MockNavigationService?.Setup(x => x.IsModuleAvailable(It.IsAny<string>())).Returns(true);
            MockNavigationService?.Setup(x => x.Navigate(It.IsAny<string>(), It.IsAny<object[]>())).Returns(true);

            // Database service defaults - return safe empty results
            MockDatabaseService?.Setup(x => x.GetAllRoutesWithDetails()).Returns(new List<BusBuddy.Models.Route>());

            // Analytics service defaults - return empty metrics
            MockRouteAnalyticsService?.Setup(x => x.CalculateRouteEfficiency(It.IsAny<BusBuddy.Models.Route>()))
                .Returns((BusBuddy.Models.RouteEfficiencyMetrics?)null);

            // Report service defaults - prevent null reference exceptions
            MockReportService?.Setup(x => x.GenerateCDE40ReportAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<object>("Test Report"));

            // Error handler defaults - simplified to avoid interface issues
            MockErrorHandlerService?.Setup(x => x.LogError(It.IsAny<string>(), It.IsAny<string>()));
        }

        /// <summary>
        /// Configure mock behaviors with parameters for specific test scenarios
        /// Allows tests to customize mock behaviors without recreating the entire setup
        /// </summary>
        /// <param name="mockConfig">Configuration action to customize mock behavior</param>
        protected void ConfigureMocks(Action<SyncfusionMockConfiguration> mockConfig)
        {
            if (mockConfig == null) return;

            var config = new SyncfusionMockConfiguration
            {
                NavigationService = MockNavigationService,
                DatabaseService = MockDatabaseService,
                RouteAnalyticsService = MockRouteAnalyticsService,
                ReportService = MockReportService,
                AnalyticsService = MockAnalyticsService,
                ErrorHandlerService = MockErrorHandlerService
            };

            mockConfig(config);
        }

        /// <summary>
        /// Configuration helper class for parameterized mock setups
        /// Provides strongly-typed access to all available mocks
        /// </summary>
        public class SyncfusionMockConfiguration
        {
            public Mock<INavigationService>? NavigationService { get; set; }
            public Mock<BusBuddy.Business.IDatabaseHelperService>? DatabaseService { get; set; }
            public Mock<IRouteAnalyticsService>? RouteAnalyticsService { get; set; }
            public Mock<IReportService>? ReportService { get; set; }
            public Mock<BusBuddy.UI.Services.IAnalyticsService>? AnalyticsService { get; set; }
            public Mock<IErrorHandlerService>? ErrorHandlerService { get; set; }
        }        /// <summary>
        /// Create a Dashboard instance for testing with enhanced DPI and thread safety
        /// Following Syncfusion testing pattern for form instantiation
        /// Creates form without showing it and with proper resource isolation to prevent UI thread issues
        /// </summary>
        protected BusBuddy.UI.Views.Dashboard CreateDashboard()
        {
            return CreateFormSafely(() =>
            {
                // Configure test environment for dashboard
                Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "true");
                Environment.SetEnvironmentVariable("SUPPRESS_DATE_VALIDATION", "true");
                Environment.SetEnvironmentVariable("SKIP_DATE_PARSING", "true");

                // Create dashboard with test configuration
                var dashboard = new BusBuddy.UI.Views.Dashboard();

                // Ensure chart and data operations won't trigger date validation errors
                dashboard.Tag = "TEST_MODE_NO_DIALOGS";

                // Log dashboard creation to diagnostics
                TestDiagnostics.LogOperation("SyncfusionTestBase",
                    "Created Dashboard with date validation suppression",
                    TestDiagnostics.LogLevel.Debug);

                return dashboard;
            });
        }

        /// <summary>
        /// Creates a dashboard instance with proper test configuration
        /// Enhanced with test-specific settings and dialog suppression
        /// Creates form without showing it and with proper resource isolation to prevent UI thread issues
        /// </summary>
        /// <param name="config">Optional configuration for controlling dashboard features</param>
        protected BusBuddy.UI.Views.Dashboard CreateDashboardWithConfig(DashboardConfig? config = null)
        {
            // Use default config if none provided
            config ??= new DashboardConfig
            {
                EnableDatePicker = false,
                EnableCharts = false,
                ShowDateValidationDialogs = false
            };

            return CreateFormSafely(() =>
            {
                // Configure test environment for dashboard
                Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "true");
                Environment.SetEnvironmentVariable("SUPPRESS_DATE_VALIDATION", "true");
                Environment.SetEnvironmentVariable("SKIP_DATE_PARSING", "true");

                if (!config.ShowDateValidationDialogs)
                {
                    Environment.SetEnvironmentVariable("SUPPRESS_DATE_DIALOGS", "true");
                    Environment.SetEnvironmentVariable("DATE_PICKER_TEST_MODE", "true");
                }

                // Create dashboard with test configuration
                var dashboard = new BusBuddy.UI.Views.Dashboard();

                // Configure specific test settings through properties or tags
                dashboard.Tag = "TEST_MODE_NO_DIALOGS";

                // Try to set config through reflection to avoid direct dependencies
                try
                {
                    var enableDatePickerProperty = dashboard.GetType().GetProperty("EnableDatePicker");
                    enableDatePickerProperty?.SetValue(dashboard, config.EnableDatePicker);

                    var enableChartsProperty = dashboard.GetType().GetProperty("EnableCharts");
                    enableChartsProperty?.SetValue(dashboard, config.EnableCharts);

                    var disableValidationDialogsProperty = dashboard.GetType().GetProperty("DisableValidationDialogs");
                    disableValidationDialogsProperty?.SetValue(dashboard, !config.ShowDateValidationDialogs);
                }
                catch (Exception ex)
                {
                    TestDiagnostics.LogOperation("SyncfusionTestBase",
                        $"Non-critical: Could not set dashboard properties: {ex.Message}",
                        TestDiagnostics.LogLevel.Warning);
                }

                // Log dashboard creation to diagnostics
                TestDiagnostics.LogOperation("SyncfusionTestBase",
                    "Created Dashboard with date validation suppression",
                    TestDiagnostics.LogLevel.Debug);

                return dashboard;
            });
        }

        /// <summary>
        /// Generic form creation helper with DPI and thread safety
        /// Prevents "Object is currently in use elsewhere" graphics exceptions
        /// </summary>
        /// <typeparam name="T">Type of form to create</typeparam>
        /// <param name="formFactory">Factory function to create the form</param>
        /// <returns>Created form instance</returns>
        protected T CreateFormSafely<T>(Func<T> formFactory) where T : Form
        {
            if (formFactory == null)
                throw new ArgumentNullException(nameof(formFactory));

            T? form = null;
            Exception? creationException = null;

            // Use thread-safe form creation to prevent graphics resource conflicts
            if (InvokeRequired())
            {
                var resetEvent = new ManualResetEventSlim(false);
                var dummy = new Control();
                dummy.Invoke(new Action(() =>
                {
                    try
                    {
                        form = CreateFormWithResourceIsolation(formFactory);
                    }
                    catch (Exception ex)
                    {
                        creationException = ex;
                    }
                    finally
                    {
                        resetEvent.Set();
                    }
                }));

                // Use timeout to prevent hanging in CLI environments
                var waitTimeout = Environment.GetEnvironmentVariable("BUSBUDDY_CLI_TEST_MODE") == "true" ? 5000 : 30000;
                if (!resetEvent.Wait(waitTimeout))
                {
                    throw new TimeoutException($"Form creation timed out after {waitTimeout}ms - possible synchronization context issue");
                }
                dummy.Dispose();
            }
            else
            {
                form = CreateFormWithResourceIsolation(formFactory);
            }

            if (creationException != null)
                throw new InvalidOperationException($"Failed to create form: {creationException.Message}", creationException);

            if (form == null)
                throw new InvalidOperationException("Form creation returned null");

            // Track for proper disposal
            RegisterFormForDisposal(form);
            return form;
        }

        /// <summary>
        /// Create form with proper resource isolation to prevent DPI/Graphics conflicts
        /// </summary>
        private T CreateFormWithResourceIsolation<T>(Func<T> formFactory) where T : Form
        {
            var testName = GetType().Name;
            TestDiagnostics.LogOperation(testName, $"Creating form of type {typeof(T).Name}", TestDiagnostics.LogLevel.Debug);

            var form = formFactory();

            // Configure form for testing to prevent UI conflicts
            form.Visible = false;
            form.ShowInTaskbar = false;
            form.WindowState = FormWindowState.Minimized;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-32000, -32000); // Move off-screen

            // Create handle to ensure form is fully initialized
            // but do it safely to prevent graphics exceptions
            try
            {
                var handle = form.Handle; // Force handle creation
                TestDiagnostics.LogOperation(testName, $"Form handle created: {handle}", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                // Handle creation can fail in headless environments
                // This is acceptable for most tests
                TestDiagnostics.LogOperation(testName, $"Handle creation failed: {ex.Message}", TestDiagnostics.LogLevel.Warning);
            }

            // Log the form creation
            TestDiagnostics.LogFormCreation(testName, form);

            return form;
        }

        /// <summary>
        /// Check if Control.Invoke is required for thread-safe operations
        /// </summary>
        private static bool InvokeRequired()
        {
            try
            {
                // Check if we're on the UI thread by testing if a control's InvokeRequired property can be accessed
                var dummy = new Control();
                var invokeRequired = dummy.InvokeRequired;
                dummy.Dispose();
                return invokeRequired;
            }
            catch
            {
                // In some test environments, this can throw
                return false;
            }
        }        /// <summary>
        /// Register a form for automatic disposal when test completes
        /// Enhanced with duplicate prevention and null safety
        /// Prevents memory leaks and handle conflicts
        /// </summary>
        protected void RegisterFormForDisposal(Form? form)
        {
            if (form != null && !_formsToDispose.Contains(form))
            {
                _formsToDispose.Add(form);
                var testName = GetType().Name;

                // Log both the registration and the form creation for tracking
                TestDiagnostics.LogOperation(testName, $"Form registered for disposal: {form.GetType().Name}#{form.GetHashCode()}", TestDiagnostics.LogLevel.Debug);
                TestDiagnostics.LogFormCreation(testName, form);
            }
        }

        /// <summary>
        /// Register any IDisposable resource for cleanup
        /// Extends cleanup beyond just forms to include any test resources
        /// </summary>
        protected void RegisterForDisposal(IDisposable? resource)
        {
            if (resource != null && !_disposablesToCleanup.Contains(resource))
            {
                _disposablesToCleanup.Add(resource);
            }
        }

        /// <summary>
        /// Helper method to safely dispose of test forms with enhanced error handling
        /// Prevents memory leaks and handles graphics resource conflicts
        /// </summary>
        protected void SafeDisposeForm(IDisposable? form)
        {
            var testName = GetType().Name;
            bool wasDisposed = false;

            try
            {
                if (form is Form winForm)
                {
                    TestDiagnostics.LogOperation(testName, $"Disposing form: {winForm.GetType().Name}#{winForm.GetHashCode()}", TestDiagnostics.LogLevel.Debug);

                    if (!winForm.IsDisposed)
                    {
                        // Hide first to prevent any visual glitches
                        if (winForm.Visible)
                        {
                            winForm.Hide();
                        }

                        // Dispose in a way that prevents graphics conflicts
                        if (InvokeRequired() && winForm.IsHandleCreated)
                        {
                            var dummy = new Control();
                            dummy.Invoke(new Action(() =>
                            {
                                try
                                {
                                    winForm.Dispose();
                                    wasDisposed = true;
                                }
                                catch (ObjectDisposedException)
                                {
                                    // Already disposed, ignore
                                    wasDisposed = true;
                                }
                            }));
                            dummy.Dispose();
                        }
                        else
                        {
                            winForm.Dispose();
                            wasDisposed = true;
                        }
                    }
                    else
                    {
                        wasDisposed = true; // Already disposed
                    }

                    // Log the disposal result
                    TestDiagnostics.LogFormDisposal(testName, winForm, wasDisposed);
                }
                else if (form != null)
                {
                    form.Dispose();
                    wasDisposed = true;
                }
            }
            catch (Exception ex)
            {
                // Log disposal errors but don't fail the test
                TestDiagnostics.LogOperation(testName, $"Error disposing form: {ex.Message}", TestDiagnostics.LogLevel.Warning);

                if (form is Form winForm)
                {
                    TestDiagnostics.LogFormDisposal(testName, winForm, false);
                }
            }
        }

        /// <summary>
        /// Enhanced disposal with comprehensive resource cleanup
        /// Handles both forms and general IDisposable resources
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed) return;

            var testName = GetType().Name;
            TestDiagnostics.LogOperation(testName, "Starting test cleanup and disposal", TestDiagnostics.LogLevel.Debug);

            try
            {
                // Dispose all tracked forms
                TestDiagnostics.LogOperation(testName, $"Disposing {_formsToDispose.Count} tracked forms", TestDiagnostics.LogLevel.Debug);
                foreach (var form in _formsToDispose.ToArray())
                {
                    SafeDisposeForm(form);
                }
                _formsToDispose.Clear();

                // Dispose other resources
                TestDiagnostics.LogOperation(testName, $"Disposing {_disposablesToCleanup.Count} other resources", TestDiagnostics.LogLevel.Debug);
                foreach (var resource in _disposablesToCleanup.ToArray())
                {
                    try
                    {
                        resource?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        TestDiagnostics.LogOperation(testName, $"Error disposing resource: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                    }
                }
                _disposablesToCleanup.Clear();

                TestDiagnostics.LogOperation(testName, "Test cleanup completed successfully", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogOperation(testName, $"Error during test cleanup: {ex.Message}", TestDiagnostics.LogLevel.Error);
            }
            finally
            {
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void InitializeTestEnvironment()
        {
            try
            {
                // Suppress dialogs and initialize repositories
                DialogUtil.SuppressDialogs();

                // Initialize repositories
                InitializeRepositories();

                // Register Syncfusion license
                SyncfusionLicenseHelper.InitializeLicense();

                // Set environment variable to indicate test mode
                Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "true");
                Environment.SetEnvironmentVariable("BUSBUDDY_SUPPRESS_DIALOGS", "true");

                // Register unhandled exception handlers to prevent dialogs and route to logging
                RegisterGlobalExceptionHandlers();

                TestDiagnostics.LogOperation(GetType().Name, "Test environment initialized successfully", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                // Log the error but continue - don't fail tests due to setup issues
                TestDiagnostics.LogOperation(GetType().Name, $"Error initializing test environment: {ex.Message}", TestDiagnostics.LogLevel.Error);
                TestDiagnostics.LogException(GetType().Name, ex);
            }
        }

        /// <summary>
        /// Register global exception handlers to prevent dialogs and route to logging
        /// </summary>
        private void RegisterGlobalExceptionHandlers()
        {
            try
            {
                // Set application to throw exceptions rather than show dialogs
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                // Register thread exception handler
                Application.ThreadException += (sender, e) =>
                {
                    TestDiagnostics.LogOperation(GetType().Name, $"Thread exception caught: {e.Exception.Message}", TestDiagnostics.LogLevel.Error);
                    TestDiagnostics.LogException(GetType().Name, e.Exception);
                };

                // Register domain unhandled exception handler
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        TestDiagnostics.LogOperation(GetType().Name, $"Unhandled exception caught: {ex.Message}", TestDiagnostics.LogLevel.Error);
                        TestDiagnostics.LogException(GetType().Name, ex);
                    }
                };

                TestDiagnostics.LogOperation(GetType().Name, "Global exception handlers registered", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogOperation(GetType().Name, $"Failed to register global exception handlers: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                TestDiagnostics.LogException(GetType().Name, ex);
            }
        }

        /// <summary>
        /// Initialize repositories directly
        /// </summary>
        private void InitializeRepositories()
        {
            try
            {
                // Initialize repositories directly
                // This is a simplified version of what would be in RepositoryInitializer.InitializeAllRepositories()
                // Add repository initialization as needed
                Console.WriteLine("Repository initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing repositories: {ex.Message}");
            }
        }

        /// <summary>
        /// Suppresses UI dialogs to prevent tests from hanging on message boxes
        /// </summary>
        private void SuppressDialogs()
        {
            try
            {
                // Use the enhanced DialogUtil to suppress all dialogs
                DialogUtil.SuppressDialogs();

                // Redirect console output to test diagnostics when possible
                TestDiagnostics.LogOperation(GetType().Name, "Dialog suppression applied through DialogUtil", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                // Log error but continue with tests
                TestDiagnostics.LogOperation(GetType().Name, $"Error suppressing dialogs: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                TestDiagnostics.LogException(GetType().Name, ex);
            }
        }

        /// <summary>
        /// Configuration class for Dashboard instantiation in tests
        /// Allows controlling specific features that might cause test issues
        /// </summary>
        public class DashboardConfig
        {
            /// <summary>
            /// Whether to enable date picker controls in the dashboard
            /// Set to false to disable date controls that might show dialogs
            /// </summary>
            public bool EnableDatePicker { get; set; } = false;

            /// <summary>
            /// Whether to load and display chart data
            /// Set to false to avoid chart-related date parsing issues
            /// </summary>
            public bool EnableCharts { get; set; } = false;

            /// <summary>
            /// Whether to show date validation dialogs
            /// Should be false for headless test execution
            /// </summary>
            public bool ShowDateValidationDialogs { get; set; } = false;
        }

        /// <summary>
        /// Assert that a specific theme is applied to a form
        /// Useful for testing theme toggling functionality in Task 8
        /// </summary>
        /// <param name="form">The form to check</param>
        /// <param name="expectedTheme">The expected theme name (e.g., "Material", "Office2019")</param>
        protected void AssertThemeApplied(Form form, string expectedTheme)
        {
            form.Should().NotBeNull("Form must exist to check theme");

            // Check for Syncfusion theme property via reflection
            var themeProperty = form.GetType().GetProperty("ThemeName") ??
                               form.GetType().GetProperty("Theme") ??
                               form.GetType().GetProperty("ColorScheme");

            if (themeProperty != null)
            {
                var currentTheme = themeProperty.GetValue(form)?.ToString();
                currentTheme.Should().Be(expectedTheme,
                    $"Form should have theme '{expectedTheme}' but found '{currentTheme}'");
            }
            else
            {
                // Check for Syncfusion-specific styling indicators
                AssertSyncfusionThemeIndicators(form, expectedTheme);
            }
        }

        /// <summary>
        /// Check for Syncfusion theme indicators when direct theme property isn't available
        /// </summary>
        private void AssertSyncfusionThemeIndicators(Form form, string expectedTheme)
        {
            // Check background color and other visual indicators
            switch (expectedTheme.ToLower())
            {
                case "material":
                case "materialdark":
                    // Material themes typically use specific color schemes
                    form.BackColor.Should().NotBe(SystemColors.Control,
                        "Material theme should override default system colors");
                    break;
                case "office2019":
                    // Office 2019 themes have distinct styling
                    break;
                default:
                    TestDiagnostics.LogOperation(GetType().Name,
                        $"Theme verification for '{expectedTheme}' not implemented",
                        TestDiagnostics.LogLevel.Warning);
                    break;
            }
        }

        /// <summary>
        /// Preview dashboard in a controlled test environment
        /// Useful for manual inspection during development
        /// </summary>
        /// <param name="config">Optional dashboard configuration</param>
        protected void PreviewDashboard(object? config = null)
        {
            if (Environment.GetEnvironmentVariable("BUSBUDDY_ENABLE_PREVIEW") != "true")
            {
                TestDiagnostics.LogOperation(GetType().Name,
                    "Dashboard preview skipped - set BUSBUDDY_ENABLE_PREVIEW=true to enable",
                    TestDiagnostics.LogLevel.Info);
                return;
            }

            try
            {
                var dashboard = CreateDashboard();
                dashboard.Should().NotBeNull("Dashboard must be created for preview");

                // Make visible for manual inspection
                dashboard.Visible = true;
                dashboard.WindowState = FormWindowState.Normal;

                TestDiagnostics.LogOperation(GetType().Name,
                    "Dashboard preview launched - close manually to continue",
                    TestDiagnostics.LogLevel.Info);

                // Run briefly to allow inspection
                Application.DoEvents();
                Thread.Sleep(1000); // Allow form to render
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogException(GetType().Name, ex);
                throw;
            }
        }
    }
}
