using System;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;

namespace BusBuddy.TestEngine.Foundation
{
    /// <summary>
    /// Utility class to suppress UI dialogs during test runs
    /// Enhanced with comprehensive dialog suppression and error logging
    /// </summary>
    public static class DialogUtil
    {
        // Track dialog suppression state
        private static bool _dialogSuppressionEnabled = false;
        private static readonly object _suppressionLock = new object();

        /// <summary>
        /// Suppress dialogs and message boxes to prevent test hangs
        /// Enhanced with robust suppression mechanisms and error logging
        /// </summary>
        public static void SuppressDialogs()
        {
            lock (_suppressionLock)
            {
                if (_dialogSuppressionEnabled)
                    return;

                try
                {
                    // Log start of dialog suppression
                    TestDiagnostics.LogOperation("DialogUtil", "Setting up comprehensive dialog suppression", TestDiagnostics.LogLevel.Info);

                    // Set application to throw exceptions rather than show dialogs
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                    // Set standard dialog defaults to auto-close/cancel
                    SetMessageBoxDefaults();

                    // Initialize test database if needed to prevent "unable to load" dialogs
                    try
                    {
                        BusBuddy.Data.TestDatabaseInitializer.EnsureTestDatabaseExists();
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue
                        TestDiagnostics.LogOperation("DialogUtil", $"Test database initialization failed: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                        TestDiagnostics.LogException("DialogUtil", ex);
                    }

                    // Enable test modes for UI components
                    EnableUITestModes();

                    // Setup Thread exception handling to redirect to logging
                    SetupThreadExceptionHandling();

                    // Set environment variables to signal test mode
                    Environment.SetEnvironmentVariable("BUSBUDDY_TEST_MODE", "true");
                    Environment.SetEnvironmentVariable("BUSBUDDY_SUPPRESS_DIALOGS", "true");

                    _dialogSuppressionEnabled = true;
                    TestDiagnostics.LogOperation("DialogUtil", "Dialog suppression configured successfully", TestDiagnostics.LogLevel.Info);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail - still try to continue tests
                    TestDiagnostics.LogOperation("DialogUtil", $"Error setting up dialog suppression: {ex.Message}", TestDiagnostics.LogLevel.Error);
                    TestDiagnostics.LogException("DialogUtil", ex);
                }
            }
        }

        /// <summary>
        /// Enable test modes for UI components that might show dialogs
        /// Enhanced with more comprehensive component test mode settings
        /// </summary>
        public static void EnableUITestModes()
        {
            try
            {
                TestDiagnostics.LogOperation("DialogUtil", "Enabling UI test modes for all components", TestDiagnostics.LogLevel.Debug);

                // Enable test modes for UI classes to suppress dialogs
                if (Type.GetType("BusBuddy.UI.Services.TestSafeApplicationShutdownManager, BusBuddy.UI") is Type shutdownType)
                {
                    var enableMethod = shutdownType.GetMethod("EnableTestMode", BindingFlags.Public | BindingFlags.Static);
                    enableMethod?.Invoke(null, null);
                    TestDiagnostics.LogOperation("DialogUtil", "Enabled test mode for ApplicationShutdownManager", TestDiagnostics.LogLevel.Debug);
                }

                if (Type.GetType("BusBuddy.UI.Base.SyncfusionBaseForm, BusBuddy.UI") is Type baseFormType)
                {
                    var enableMethod = baseFormType.GetMethod("EnableTestMode", BindingFlags.Public | BindingFlags.Static);
                    enableMethod?.Invoke(null, null);
                    TestDiagnostics.LogOperation("DialogUtil", "Enabled test mode for SyncfusionBaseForm", TestDiagnostics.LogLevel.Debug);
                }

                if (Type.GetType("BusBuddy.UI.Base.BaseManagementForm`1, BusBuddy.UI") is Type mgmtFormType)
                {
                    var enableMethod = mgmtFormType.GetMethod("EnableTestMode", BindingFlags.Public | BindingFlags.Static);
                    enableMethod?.Invoke(null, null);
                    TestDiagnostics.LogOperation("DialogUtil", "Enabled test mode for BaseManagementForm", TestDiagnostics.LogLevel.Debug);
                }

                // Add additional components that might need test mode
                if (Type.GetType("BusBuddy.UI.Controls.DialogManager, BusBuddy.UI") is Type dialogManagerType)
                {
                    var enableMethod = dialogManagerType.GetMethod("EnableTestMode", BindingFlags.Public | BindingFlags.Static);
                    enableMethod?.Invoke(null, null);
                    TestDiagnostics.LogOperation("DialogUtil", "Enabled test mode for DialogManager", TestDiagnostics.LogLevel.Debug);
                }

                if (Type.GetType("BusBuddy.UI.Services.ErrorHandlerService, BusBuddy.UI") is Type errorHandlerType)
                {
                    var enableMethod = errorHandlerType.GetMethod("EnableTestMode", BindingFlags.Public | BindingFlags.Static);
                    enableMethod?.Invoke(null, null);
                    TestDiagnostics.LogOperation("DialogUtil", "Enabled test mode for ErrorHandlerService", TestDiagnostics.LogLevel.Debug);
                }

                // Add any additional dialog suppression logic here
                TestDiagnostics.LogOperation("DialogUtil", "All UI components set to test mode", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                // Log error but continue testing
                TestDiagnostics.LogOperation("DialogUtil", $"Warning: Failed to enable UI test modes: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                LogExceptionToTestDiagnostics("DialogUtil", ex);
            }
        }

        /// <summary>
        /// Set message box and common dialog defaults to auto-close or auto-cancel
        /// </summary>
        private static void SetMessageBoxDefaults()
        {
            try
            {
                // For Windows Forms MessageBox, we can't easily suppress without mocking or hooking
                // Instead, we set environment variables to signal test mode that UI classes can check
                Environment.SetEnvironmentVariable("SUPPRESS_MESSAGEBOXES", "true");
                Environment.SetEnvironmentVariable("AUTO_CLOSE_DIALOGS", "true");

                // Special handling for date validation errors
                Environment.SetEnvironmentVariable("SUPPRESS_DATE_VALIDATION", "true");
                Environment.SetEnvironmentVariable("SKIP_DATE_PARSING", "true");

                TestDiagnostics.LogOperation("DialogUtil", "MessageBox defaults configured for auto-close", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogOperation("DialogUtil", $"Failed to set MessageBox defaults: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                LogExceptionToTestDiagnostics("DialogUtil", ex);
            }
        }

        /// <summary>
        /// Set up thread exception handling to redirect to logging
        /// </summary>
        private static void SetupThreadExceptionHandling()
        {
            try
            {
                // Subscribe to thread exception events
                Application.ThreadException += (sender, e) =>
                {
                    TestDiagnostics.LogOperation("DialogUtil", $"Thread exception caught: {e.Exception.Message}", TestDiagnostics.LogLevel.Error);
                    LogExceptionToTestDiagnostics("ThreadException", e.Exception);
                };

                // Set up unhandled exception handler
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        TestDiagnostics.LogOperation("DialogUtil", $"Unhandled exception caught: {ex.Message}", TestDiagnostics.LogLevel.Error);
                        LogExceptionToTestDiagnostics("UnhandledException", ex);
                    }
                };

                TestDiagnostics.LogOperation("DialogUtil", "Thread exception handling configured", TestDiagnostics.LogLevel.Debug);
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogOperation("DialogUtil", $"Failed to set up thread exception handling: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                LogExceptionToTestDiagnostics("DialogUtil", ex);
            }
        }

        /// <summary>
        /// Suppresses date picker dialogs specifically to prevent test hangs
        /// Targets both standard Windows Forms DateTimePicker and Syncfusion date controls
        /// </summary>
        public static void SuppressDatePickerDialogs()
        {
            try
            {
                TestDiagnostics.LogOperation("DialogUtil", "Setting up date picker dialog suppression", TestDiagnostics.LogLevel.Info);

                // Set environment variables specific to date pickers
                Environment.SetEnvironmentVariable("SUPPRESS_DATE_VALIDATION", "true");
                Environment.SetEnvironmentVariable("SKIP_DATE_PARSING", "true");
                Environment.SetEnvironmentVariable("SUPPRESS_DATE_DIALOGS", "true");
                Environment.SetEnvironmentVariable("DATE_PICKER_TEST_MODE", "true");

                // Find any active date picker dialogs using reflection and close them
                foreach (Form form in Application.OpenForms)
                {
                    // Check for common date picker dialog names
                    if (form.Name.Contains("DatePicker") ||
                        form.Name.Contains("Calendar") ||
                        form.Text.Contains("Date") ||
                        form.GetType().Name.Contains("DatePicker") ||
                        form.GetType().Name.Contains("Calendar"))
                    {
                        TestDiagnostics.LogOperation("DialogUtil", $"Closing date dialog: {form.Name}", TestDiagnostics.LogLevel.Info);
                        try
                        {
                            form.Close();
                        }
                        catch (Exception ex)
                        {
                            TestDiagnostics.LogOperation("DialogUtil", $"Failed to close date dialog: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                        }
                    }
                }

                // If we're using Syncfusion, try to suppress their date picker dialogs
                try
                {
                    if (Type.GetType("Syncfusion.Windows.Forms.Tools.DateTimePickerAdv, Syncfusion.Tools.Windows") is Type syncDatePickerType)
                    {
                        var enableTestModeMethod = syncDatePickerType.GetMethod("SuppressDialogs",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                        enableTestModeMethod?.Invoke(null, null);
                        TestDiagnostics.LogOperation("DialogUtil", "Suppressed Syncfusion DateTimePicker dialogs", TestDiagnostics.LogLevel.Info);
                    }
                }
                catch (Exception ex)
                {
                    TestDiagnostics.LogOperation("DialogUtil", $"Failed to suppress Syncfusion date dialogs: {ex.Message}", TestDiagnostics.LogLevel.Warning);
                }

                TestDiagnostics.LogOperation("DialogUtil", "Date picker dialog suppression configured successfully", TestDiagnostics.LogLevel.Info);
            }
            catch (Exception ex)
            {
                // Log error but don't fail - still try to continue tests
                TestDiagnostics.LogOperation("DialogUtil", $"Error setting up date picker dialog suppression: {ex.Message}", TestDiagnostics.LogLevel.Error);
                LogExceptionToTestDiagnostics("DialogUtil", ex);
            }
        }

        /// <summary>
        /// Logs all currently active dialogs to help with test diagnostics
        /// Helps identify what dialogs may be causing test hangs
        /// </summary>
        public static void LogActiveDialogs()
        {
            try
            {
                TestDiagnostics.LogOperation("DialogUtil", "Checking for active dialogs...", TestDiagnostics.LogLevel.Info);

                int dialogCount = 0;
                foreach (Form form in Application.OpenForms)
                {
                    // Log details about each open form
                    TestDiagnostics.LogOperation("DialogUtil",
                        $"Active form [{dialogCount}]: Name={form.Name}, Text=\"{form.Text}\", Type={form.GetType().FullName}, Modal={form.Modal}",
                        TestDiagnostics.LogLevel.Info);

                    // Check if it's likely a dialog
                    bool isDialog = form.FormBorderStyle == FormBorderStyle.FixedDialog ||
                                    form.Modal ||
                                    form.ControlBox == false ||
                                    form.MinimizeBox == false && form.MaximizeBox == false;

                    if (isDialog)
                    {
                        TestDiagnostics.LogOperation("DialogUtil",
                            $"DIALOG DETECTED: {form.Name} ({form.GetType().Name}) - \"{form.Text}\"",
                            TestDiagnostics.LogLevel.Warning);
                    }

                    dialogCount++;

                    // Log visible controls to help identify what the dialog contains
                    LogFormControls(form);
                }

                if (dialogCount == 0)
                {
                    TestDiagnostics.LogOperation("DialogUtil", "No active dialogs found", TestDiagnostics.LogLevel.Info);
                }
                else
                {
                    TestDiagnostics.LogOperation("DialogUtil", $"Found {dialogCount} active forms/dialogs", TestDiagnostics.LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                TestDiagnostics.LogOperation("DialogUtil", $"Error logging active dialogs: {ex.Message}", TestDiagnostics.LogLevel.Error);
                LogExceptionToTestDiagnostics("DialogUtil", ex);
            }
        }

        /// <summary>
        /// Recursively logs all controls in a form to help with dialog debugging
        /// </summary>
        private static void LogFormControls(Control control, string indent = "  ")
        {
            foreach (Control child in control.Controls)
            {
                try
                {
                    string visibilityFlag = child.Visible ? "[V]" : "[H]";
                    string enabledFlag = child.Enabled ? "[E]" : "[D]";

                    TestDiagnostics.LogOperation("DialogUtil",
                        $"{indent}{visibilityFlag}{enabledFlag} {child.GetType().Name}: \"{child.Text}\"",
                        TestDiagnostics.LogLevel.Debug);

                    // Recursively log child controls
                    LogFormControls(child, indent + "  ");
                }
                catch
                {
                    // Ignore errors in logging controls
                }
            }
        }

        /// <summary>
        /// Helper method to log exceptions to TestDiagnostics
        /// </summary>
        private static void LogExceptionToTestDiagnostics(string source, Exception ex)
        {
            try
            {
                // Log detailed exception information for test diagnostics
                TestDiagnostics.LogOperation(source, $"Exception: {ex.Message}", TestDiagnostics.LogLevel.Error);
                TestDiagnostics.LogOperation(source, $"Stack trace: {ex.StackTrace}", TestDiagnostics.LogLevel.Debug);

                if (ex.InnerException != null)
                {
                    TestDiagnostics.LogOperation(source, $"Inner exception: {ex.InnerException.Message}", TestDiagnostics.LogLevel.Error);
                    TestDiagnostics.LogOperation(source, $"Inner stack trace: {ex.InnerException.StackTrace}", TestDiagnostics.LogLevel.Debug);
                }
            }
            catch
            {
                // Last resort if logging fails
                Console.WriteLine($"[{source}] Exception: {ex.Message}");
                Console.WriteLine($"[{source}] Stack trace: {ex.StackTrace}");
            }
        }
    }
}
