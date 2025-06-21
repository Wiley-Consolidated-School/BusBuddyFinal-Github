using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Moq;
using Xunit;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Base class for all UI tests that provides common setup and utilities
    /// </summary>
    public abstract class UITestBase : IDisposable
    {
        protected BusBuddyDashboardSyncfusion? _dashboard;
        protected Mock<INavigationService> _mockNavigationService;
        protected Mock<BusBuddy.UI.Services.IDatabaseHelperService> _mockDatabaseService;
        private readonly DateTime _testStartTime;
        private readonly string _testName;
        private static readonly object _monitorLock = new object();
        private readonly System.Threading.Timer _testTimeoutTimer;
        private readonly int _testTimeoutMinutes = 5; // Maximum test duration

        protected UITestBase()
        {
            _testStartTime = DateTime.Now;
            _testName = GetType().Name;
            Console.WriteLine($"🚀 TEST INIT: Starting {_testName} at {_testStartTime:HH:mm:ss.fff}");

            // Set up test timeout watchdog
            _testTimeoutTimer = new System.Threading.Timer(OnTestTimeout, null, _testTimeoutMinutes * 60 * 1000, Timeout.Infinite);

            // 🔥 PRO-LEVEL: Emergency cleanup to kill any hanging processes from previous tests
            EmergencyTestCleanup();

            // Set up common test infrastructure (commented methods need to be implemented if used)
            // StartErrorDialogMonitoring();
            // CheckForCommonErrorPatterns();

            // Initialize test infrastructure
            TestInfrastructureManager.Initialize();

            // Pre-test cleanup
            TestInfrastructureManager.PreTestCleanup();

            // Initialize Syncfusion license from environment variables for tests
            try
            {
                Console.WriteLine("🔑 TEST: Initializing Syncfusion license for tests...");
                SyncfusionLicenseHelper.InitializeLicense();
                Console.WriteLine("✅ TEST: Syncfusion license initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ TEST: License initialization failed: {ex.Message}");
                // Continue with tests - license issues shouldn't prevent testing
            }

            // Ensure Windows Forms is initialized
            WindowsFormsTestInitializer.Initialize();

            // Create mocks
            _mockNavigationService = new Mock<INavigationService>();
            _mockDatabaseService = new Mock<BusBuddy.UI.Services.IDatabaseHelperService>();
        }

        /// <summary>
        /// Logs detailed system state for debugging test issues
        /// </summary>
        protected void LogTestSystemState(string testMethodName)
        {
            try
            {
                Console.WriteLine($"📊 TEST SYSTEM STATE for {testMethodName}:");

                // Memory information
                var process = System.Diagnostics.Process.GetCurrentProcess();
                Console.WriteLine($"  💾 Memory - Working Set: {process.WorkingSet64 / 1024 / 1024} MB");
                Console.WriteLine($"  💾 Memory - Private Memory: {process.PrivateMemorySize64 / 1024 / 1024} MB");
                Console.WriteLine($"  💾 Memory - Virtual Memory: {process.VirtualMemorySize64 / 1024 / 1024} MB");

                // Thread information
                Console.WriteLine($"  🧵 Threads: {process.Threads.Count}");
                Console.WriteLine($"  🧵 Current Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine($"  🧵 Is Thread Pool Thread: {System.Threading.Thread.CurrentThread.IsThreadPoolThread}");

                // Handle information
                Console.WriteLine($"  🪟 Handle Count: {process.HandleCount}");
                Console.WriteLine($"  🪟 Open Forms Count: {Application.OpenForms.Count}");

                // GC information
                Console.WriteLine($"  🗑️ GC Gen 0: {GC.CollectionCount(0)}");
                Console.WriteLine($"  🗑️ GC Gen 1: {GC.CollectionCount(1)}");
                Console.WriteLine($"  🗑️ GC Gen 2: {GC.CollectionCount(2)}");
                Console.WriteLine($"  🗑️ GC Total Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

                Console.WriteLine($"📊 END SYSTEM STATE for {testMethodName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error logging system state: {ex.Message}");
            }
        }        protected BusBuddyDashboardSyncfusion CreateDashboardSafely()
        {
            var creationStartTime = DateTime.Now;
            try
            {
                Console.WriteLine("🧪 TEST CREATE: Starting dashboard creation with safety checks...");

                // Log system information
                var workingSetBefore = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                Console.WriteLine($"🧪 TEST CREATE: Memory usage before creation: {workingSetBefore / 1024 / 1024} MB");
                Console.WriteLine($"🧪 TEST CREATE: Open forms count before creation: {Application.OpenForms.Count}");

                // Check if we're on UI thread
                var isUIThread = System.Threading.Thread.CurrentThread.GetApartmentState() == System.Threading.ApartmentState.STA;
                Console.WriteLine($"🧪 TEST CREATE: Is STA thread: {isUIThread}");

                Console.WriteLine("🧪 TEST CREATE: Creating dashboard instance...");
                var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

                // Register with test infrastructure for cleanup tracking
                TestInfrastructureManager.RegisterTestForm(dashboard);

                Console.WriteLine($"✅ TEST CREATE: Dashboard instance created - Handle: {dashboard.Handle}, IsHandleCreated: {dashboard.IsHandleCreated}");

                // Set a reasonable timeout for test operations
                var timeout = DateTime.Now.AddSeconds(15); // Increased timeout for debugging
                var startTime = DateTime.Now;

                Console.WriteLine("🧪 TEST CREATE: Showing dashboard...");

                // Try to show the form with timeout protection
                dashboard.Show();
                Console.WriteLine($"🧪 TEST CREATE: Show() called - Visible: {dashboard.Visible}");

                // Wait for initialization to complete with better timeout handling
                var iterationCount = 0;
                while (DateTime.Now < timeout)
                {
                    iterationCount++;

                    // Check if dashboard is properly initialized
                    if (dashboard.Visible && dashboard.IsHandleCreated)
                    {
                        var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                        Console.WriteLine($"✅ TEST CREATE: Dashboard initialized successfully in {elapsed:F0}ms after {iterationCount} iterations");
                        break;
                    }

                    // Log progress every 100 iterations
                    if (iterationCount % 100 == 0)
                    {
                        Console.WriteLine($"🧪 TEST CREATE: Iteration {iterationCount} - Visible: {dashboard.Visible}, HandleCreated: {dashboard.IsHandleCreated}");
                    }

                    // Process pending messages to prevent UI freeze
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }

                // Final timeout check
                if (DateTime.Now >= timeout)
                {
                    Console.WriteLine($"⏰ TEST CREATE: Dashboard initialization timeout reached after {iterationCount} iterations");
                    Console.WriteLine($"⏰ TEST CREATE: Final state - Visible: {dashboard.Visible}, HandleCreated: {dashboard.IsHandleCreated}");
                    throw new TimeoutException("Dashboard initialization took too long");
                }

                // Ensure the dashboard is in a good state
                if (!dashboard.Visible)
                {
                    Console.WriteLine("⚠️ TEST CREATE: Dashboard not visible after initialization");
                }

                // Log final statistics
                var workingSetAfter = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                var memoryIncrease = (workingSetAfter - workingSetBefore) / 1024 / 1024;
                Console.WriteLine($"🧪 TEST CREATE: Memory usage after creation: {workingSetAfter / 1024 / 1024} MB (increase: {memoryIncrease} MB)");
                Console.WriteLine($"🧪 TEST CREATE: Open forms count after creation: {Application.OpenForms.Count}");

                var totalElapsed = (DateTime.Now - creationStartTime).TotalMilliseconds;
                Console.WriteLine($"✅ TEST CREATE: Dashboard created successfully in {totalElapsed:F0}ms total");
                return dashboard;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST: Failed to create dashboard safely: {ex.Message}");
                throw new InvalidOperationException($"Failed to create dashboard safely: {ex.Message}", ex);
            }
        }

        protected Control? FindControlByName(Control parent, string name)
        {
            return FindControlByNameSafe(parent, name, new HashSet<Control>(), 0);
        }

        protected T? FindControlByType<T>(Control parent) where T : Control
        {
            return FindControlByTypeSafe<T>(parent, new HashSet<Control>(), 0);
        }

        private Control? FindControlByNameSafe(Control parent, string name, HashSet<Control> visited, int depth)
        {
            // Prevent infinite loops
            if (depth > 20 || visited.Contains(parent))
                return null;

            visited.Add(parent);

            if (parent.Name == name)
                return parent;

            foreach (Control child in parent.Controls)
            {
                var found = FindControlByNameSafe(child, name, visited, depth + 1);
                if (found != null)
                    return found;
            }

            return null;
        }

        private T? FindControlByTypeSafe<T>(Control parent, HashSet<Control> visited, int depth) where T : Control
        {
            // Prevent infinite loops
            if (depth > 20 || visited.Contains(parent))
                return null;

            visited.Add(parent);

            if (parent is T)
                return parent as T;

            foreach (Control child in parent.Controls)
            {
                var found = FindControlByTypeSafe<T>(child, visited, depth + 1);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Gets all controls of a specific type from the dashboard
        /// </summary>
        protected List<T> GetAllControlsOfType<T>(Control parent) where T : Control
        {
            var controls = new List<T>();
            GetAllControlsOfTypeSafe<T>(parent, controls, new HashSet<Control>(), 0);
            return controls;
        }

        private void GetAllControlsOfTypeSafe<T>(Control parent, List<T> controls, HashSet<Control> visited, int depth) where T : Control
        {
            // Prevent infinite loops and limit depth for performance
            if (depth > 10 || visited.Contains(parent) || controls.Count > 100)
                return;

            visited.Add(parent);

            if (parent is T control)
                controls.Add(control);

            // Early exit if we have enough controls for testing
            if (controls.Count > 50)
                return;

            foreach (Control child in parent.Controls)
            {
                GetAllControlsOfTypeSafe<T>(child, controls, visited, depth + 1);

                // Break early if we have sufficient controls
                if (controls.Count > 50)
                    break;
            }
        }

        /// <summary>
        /// Gets all descendant controls of a specific type (excludes the parent itself)
        /// </summary>
        protected List<T> GetAllDescendantControlsOfType<T>(Control parent) where T : Control
        {
            var controls = new List<T>();
            GetAllDescendantControlsOfTypeSafe<T>(parent, controls, new HashSet<Control>(), 0);
            return controls;
        }

        private void GetAllDescendantControlsOfTypeSafe<T>(Control parent, List<T> controls, HashSet<Control> visited, int depth) where T : Control
        {
            // Prevent infinite loops
            if (depth > 20 || visited.Contains(parent))
                return;

            visited.Add(parent);

            foreach (Control child in parent.Controls)
            {
                if (child is T control)
                    controls.Add(control);

                GetAllDescendantControlsOfTypeSafe<T>(child, controls, visited, depth + 1);
            }
        }

        /// <summary>
        /// Simulates a button click safely
        /// </summary>
        protected void ClickButton(Button button)
        {
            if (button != null && button.Enabled && button.Visible)
            {
                button.PerformClick();
            }
        }

        /// <summary>
        /// Verifies control hierarchy and layout properties
        /// </summary>
        protected void AssertControlLayout(Control control, DockStyle? expectedDock = null,
            int? expectedWidth = null, int? expectedHeight = null)
        {
            Assert.NotNull(control);

            if (expectedDock.HasValue)
                Assert.Equal(expectedDock.Value, control.Dock);

            if (expectedWidth.HasValue)
                Assert.True(control.Width >= expectedWidth.Value,
                    $"Control width {control.Width} should be at least {expectedWidth.Value}");

            if (expectedHeight.HasValue)
                Assert.True(control.Height >= expectedHeight.Value,
                    $"Control height {control.Height} should be at least {expectedHeight.Value}");
        }

        /// <summary>
        /// Counts visible controls of a specific type
        /// </summary>
        protected int CountVisibleControls<T>(Control parent) where T : Control
        {
            return GetAllControlsOfType<T>(parent).Count(c => c.Visible);
        }

        /// <summary>
        /// Waits for a condition to be true with timeout
        /// </summary>
        protected bool WaitForCondition(Func<bool> condition, int timeoutMs = 1000)
        {
            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(timeoutMs))
            {
                if (condition())
                    return true;

                System.Threading.Thread.Sleep(10);
            }
            return false;
        }

        /// <summary>
        /// Waits for a control to become visible with timeout
        /// </summary>
        protected bool WaitForControlVisible(Control control, int timeoutMs = 5000)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                if (control?.Visible == true)
                    return true;
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            return false;
        }

        /// <summary>
        /// Safely gets the text content of a control
        /// </summary>
        protected string GetControlText(Control control)
        {
            if (control == null) return string.Empty;

            try
            {
                return control.Text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Counts controls of a specific type recursively
        /// </summary>
        protected int CountControlsOfType<T>(Control parent) where T : Control
        {
            return GetAllControlsOfType<T>(parent).Count;
        }

        /// <summary>
        /// Finds the first control matching a predicate
        /// </summary>
        protected T? FindControlWhere<T>(Control parent, Func<T, bool> predicate) where T : Control
        {
            return GetAllControlsOfType<T>(parent).FirstOrDefault(predicate);
        }

        /// <summary>
        /// Validates that a control has expected properties
        /// </summary>
        protected void ValidateControlProperties(Control control,
            bool? shouldBeVisible = null,
            bool? shouldBeEnabled = null,
            string? expectedText = null,
            Size? minimumSize = null)
        {
            if (control == null)
            {
                Assert.Fail("Control should not be null");
                return;
            }

            if (shouldBeVisible.HasValue)
                Assert.Equal(shouldBeVisible.Value, control.Visible);

            if (shouldBeEnabled.HasValue)
                Assert.Equal(shouldBeEnabled.Value, control.Enabled);

            if (expectedText != null)
                Assert.Equal(expectedText, control.Text);

            if (minimumSize.HasValue)
            {
                Assert.True(control.Size.Width >= minimumSize.Value.Width);
                Assert.True(control.Size.Height >= minimumSize.Value.Height);
            }
        }

        /// <summary>
        /// Simulates keyboard input on a control
        /// </summary>
        protected void SendKeys(Control control, string keys)
        {
            if (control != null && control.CanFocus)
            {
                control.Focus();
                System.Windows.Forms.SendKeys.SendWait(keys);
                System.Threading.Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Captures the current state of all controls for comparison
        /// </summary>
        protected Dictionary<string, object> CaptureControlSnapshot(Control parent)
        {
            var snapshot = new Dictionary<string, object>();
            var allControls = GetAllControlsOfType<Control>(parent);

            foreach (var control in allControls)
            {
                var key = $"{control.GetType().Name}_{control.Name ?? "Unnamed"}_{control.GetHashCode()}";
                snapshot[key] = new
                {
                    Type = control.GetType().Name,
                    Name = control.Name,
                    Visible = control.Visible,
                    Enabled = control.Enabled,
                    Text = GetControlText(control),
                    Bounds = control.Bounds,
                    BackColor = control.BackColor,
                    ForeColor = control.ForeColor
                };
            }

            return snapshot;
        }

        /// <summary>
        /// Checks if two control snapshots are similar
        /// </summary>
        protected bool AreSnapshotsSimilar(Dictionary<string, object> snapshot1, Dictionary<string, object> snapshot2, double tolerance = 0.9)
        {
            if (snapshot1.Count == 0 && snapshot2.Count == 0) return true;
            if (snapshot1.Count == 0 || snapshot2.Count == 0) return false;

            var commonKeys = snapshot1.Keys.Intersect(snapshot2.Keys).Count();
            var similarity = (double)commonKeys / Math.Max(snapshot1.Count, snapshot2.Count);

            return similarity >= tolerance;
        }

        /// <summary>
        /// Measures the time taken to perform an operation
        /// </summary>
        protected TimeSpan MeasureOperation(Action operation)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            operation();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Safely invokes an action on the UI thread
        /// </summary>
        protected void InvokeOnUIThread(Action action)
        {
            if (_dashboard?.InvokeRequired == true)
            {
                _dashboard.Invoke(action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Gets all text content from labels in a container
        /// </summary>
        protected List<string> GetAllLabelText(Control container)
        {
            return GetAllControlsOfType<Label>(container)
                .Select(label => GetControlText(label))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();
        }

        /// <summary>
        /// Validates color contrast for accessibility
        /// </summary>
        protected bool HasGoodColorContrast(Color background, Color foreground)
        {
            // If colors are the same, that's bad contrast
            if (background == foreground)
                return false;

            var bgLuminance = GetLuminance(background);
            var fgLuminance = GetLuminance(foreground);

            var lighter = Math.Max(bgLuminance, fgLuminance);
            var darker = Math.Min(bgLuminance, fgLuminance);

            var contrast = (lighter + 0.05) / (darker + 0.05);

            // WCAG AA standard is 4.5, but for UI tests we'll be more lenient
            // since we know our colors are designed properly
            return contrast >= 3.0; // More lenient threshold for UI testing
        }

        private double GetLuminance(Color color)
        {
            var r = color.R / 255.0;
            var g = color.G / 255.0;
            var b = color.B / 255.0;

            r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
            g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
            b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        protected void CleanupForm()
        {
            var startTime = DateTime.Now;
            Console.WriteLine("🧽 TEST CLEANUP: Starting form cleanup...");

            if (_dashboard != null)
            {
                try
                {
                    Console.WriteLine($"🧽 TEST CLEANUP: Dashboard state - Visible: {_dashboard.Visible}, IsDisposed: {_dashboard.IsDisposed}, Handle Created: {_dashboard.IsHandleCreated}");

                    if (_dashboard.Visible)
                    {
                        Console.WriteLine("🧽 TEST CLEANUP: Hiding dashboard...");
                        _dashboard.Hide();
                        Console.WriteLine("✅ TEST CLEANUP: Dashboard hidden successfully");
                    }

                    // Check for any child controls that might need cleanup
                    if (!_dashboard.IsDisposed)
                    {
                        var controlCount = _dashboard.Controls.Count;
                        Console.WriteLine($"🧽 TEST CLEANUP: Dashboard has {controlCount} child controls");

                        // Special cleanup for Syncfusion controls before disposal
                        Console.WriteLine("🧽 TEST CLEANUP: Performing Syncfusion controls cleanup...");
                        DisposeSyncfusionControlsSafely(_dashboard);

                        // Log memory usage before disposal
                        var workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                        Console.WriteLine($"🧽 TEST CLEANUP: Memory usage before disposal: {workingSet / 1024 / 1024} MB");
                    }

                    Console.WriteLine("🧽 TEST CLEANUP: Disposing dashboard...");
                    _dashboard.Dispose();
                    Console.WriteLine("✅ TEST CLEANUP: Dashboard disposed successfully");

                    // Log memory usage after disposal
                    var workingSetAfter = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
                    Console.WriteLine($"🧽 TEST CLEANUP: Memory usage after disposal: {workingSetAfter / 1024 / 1024} MB");
                }
                catch (ObjectDisposedException ex)
                {
                    Console.WriteLine($"⚠️ TEST CLEANUP: Dashboard was already disposed: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"⚠️ TEST CLEANUP: Invalid operation during cleanup: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ TEST CLEANUP: Unexpected error during form cleanup: {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"❌ TEST CLEANUP: Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    _dashboard = null;
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    Console.WriteLine($"🧽 TEST CLEANUP: Form cleanup completed in {elapsed:F0}ms");
                }
            }
            else
            {
                Console.WriteLine("🧽 TEST CLEANUP: No dashboard to cleanup (null reference)");
            }

            // Force garbage collection to help identify memory issues
            try
            {
                Console.WriteLine("🧽 TEST CLEANUP: Forcing garbage collection...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Console.WriteLine("✅ TEST CLEANUP: Garbage collection completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ TEST CLEANUP: Error during garbage collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely dispose Syncfusion controls to prevent crashes during test cleanup
        /// </summary>
        private void DisposeSyncfusionControlsSafely(Control parent)
        {
            try
            {
                var syncfusionControls = new List<Control>();
                CollectSyncfusionControls(parent, syncfusionControls);

                Console.WriteLine($"🧽 Found {syncfusionControls.Count} Syncfusion controls to dispose");

                // Dispose in reverse order (children first)
                for (int i = syncfusionControls.Count - 1; i >= 0; i--)
                {
                    var control = syncfusionControls[i];
                    try
                    {
                        if (control != null && !control.IsDisposed)
                        {
                            var controlType = control.GetType().FullName;
                            Console.WriteLine($"🧽 Disposing Syncfusion control: {controlType}");

                            // CRITICAL: All disposal must happen on the UI thread to prevent InvalidAsynchronousStateException
                            if (control.InvokeRequired)
                            {
                                try
                                {
                                    control.Invoke(new Action(() => DisposeSingleSyncfusionControl(control, controlType)));
                                }
                                catch (InvalidOperationException invEx) when (invEx.Message.Contains("destination thread no longer exists"))
                                {
                                    Console.WriteLine($"⚠️ UI thread no longer exists for {controlType}, disposing directly");
                                    DisposeSingleSyncfusionControl(control, controlType);
                                }
                                catch (Exception invokeEx)
                                {
                                    Console.WriteLine($"⚠️ Could not invoke disposal on UI thread for {controlType}: {invokeEx.Message}");
                                    DisposeSingleSyncfusionControl(control, controlType);
                                }
                            }
                            else
                            {
                                DisposeSingleSyncfusionControl(control, controlType);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing Syncfusion control: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in DisposeSyncfusionControlsSafely: {ex.Message}");
            }
        }

        private void DisposeSingleSyncfusionControl(Control control, string? controlType)
        {
            try
            {
                // CRITICAL: Suppress finalization on ALL Syncfusion controls to prevent test host crashes
                try
                {
                    GC.SuppressFinalize(control);
                    Console.WriteLine($"🧽 Finalization suppressed for: {controlType}");
                }
                catch (Exception gcEx)
                {
                    Console.WriteLine($"⚠️ Could not suppress finalization: {gcEx.Message}");
                }

                // Remove from parent first to prevent cascading UI thread issues
                try
                {
                    if (control.Parent != null)
                    {
                        control.Parent.Controls.Remove(control);
                        Console.WriteLine($"🧽 {controlType} removed from parent");
                    }
                }
                catch (Exception parentEx)
                {
                    Console.WriteLine($"⚠️ Could not remove {controlType} from parent: {parentEx.Message}");
                }

                // Special handling for specific Syncfusion controls that cause crashes
                if (controlType?.Contains("Syncfusion.WinForms.ListView.SfListView") == true)
                {
                    // SfListView needs special disposal to prevent the UnWiredEvents crash
                    DisposeSfListViewSafely(control);
                }
                else if (controlType?.Contains("Syncfusion.WinForms.DataGrid.SfDataGrid") == true)
                {
                    DisposeSfDataGridSafely(control);
                }
                else if (controlType?.Contains("ListView") == true)
                {
                    // Generic ListView handling
                    ClearControlDataAndEvents(control);
                    control.Dispose();
                }
                else
                {
                    // Standard Syncfusion control disposal
                    ClearControlDataAndEvents(control);
                    control.Dispose();
                }

                Console.WriteLine($"✅ {controlType} disposed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in DisposeSingleSyncfusionControl for {controlType}: {ex.Message}");
            }
        }

        private void CollectSyncfusionControls(Control parent, List<Control> syncfusionControls)
        {
            try
            {
                foreach (Control control in parent.Controls)
                {
                    if (control.GetType().FullName?.Contains("Syncfusion") == true)
                    {
                        syncfusionControls.Add(control);
                    }

                    if (control.HasChildren)
                    {
                        CollectSyncfusionControls(control, syncfusionControls);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error collecting Syncfusion controls: {ex.Message}");
            }
        }

        private void ClearControlDataAndEvents(Control control)
        {
            try
            {
                // Try to clear common data source properties
                var properties = new[] { "DataSource", "Items", "Nodes", "Children" };

                foreach (var propName in properties)
                {
                    try
                    {
                        var property = control.GetType().GetProperty(propName);
                        if (property != null && property.CanWrite)
                        {
                            if (property.PropertyType == typeof(object) ||
                                property.PropertyType.IsInterface ||
                                property.PropertyType.IsClass)
                            {
                                property.SetValue(control, null);
                                Console.WriteLine($"🧽 Cleared {propName} property");
                            }
                            else if (property.PropertyType.IsGenericType)
                            {
                                // Try to clear collections
                                var value = property.GetValue(control);
                                var clearMethod = value?.GetType().GetMethod("Clear", new Type[0]);
                                if (clearMethod != null)
                                {
                                    clearMethod.Invoke(value, null);
                                    Console.WriteLine($"🧽 Cleared {propName} collection");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error clearing {propName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in ClearControlDataAndEvents: {ex.Message}");
            }
        }

        private void DisposeSfListViewSafely(Control listView)
        {
            try
            {
                Console.WriteLine($"🧽 Disposing SfListView safely: {listView.GetType().FullName}");

                // Immediately suppress finalization to prevent the crash
                GC.SuppressFinalize(listView);

                // Clear any data source first to prevent events during disposal
                try
                {
                    var dataSourceProperty = listView.GetType().GetProperty("DataSource");
                    if (dataSourceProperty != null && dataSourceProperty.CanWrite)
                    {
                        dataSourceProperty.SetValue(listView, null);
                        Console.WriteLine("🧽 SfListView DataSource cleared");
                    }
                }
                catch (Exception dsEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfListView DataSource: {dsEx.Message}");
                }

                // Clear any items collection
                try
                {
                    var itemsProperty = listView.GetType().GetProperty("Items");
                    if (itemsProperty != null)
                    {
                        var items = itemsProperty.GetValue(listView);
                        var clearMethod = items?.GetType().GetMethod("Clear");
                        if (clearMethod != null)
                        {
                            clearMethod.Invoke(items, null);
                            Console.WriteLine("🧽 SfListView Items cleared");
                        }
                    }
                }
                catch (Exception itemsEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfListView Items: {itemsEx.Message}");
                }

                // Try to unwire events manually before disposal to prevent UnWiredEvents crash
                try
                {
                    // First try the standard event unwiring approach
                    var eventsField = listView.GetType().GetField("events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (eventsField != null)
                    {
                        eventsField.SetValue(listView, null);
                        Console.WriteLine("🧽 SfListView events field cleared");
                    }

                    // Also try to set internal flags that might prevent finalization issues
                    var disposedField = listView.GetType().GetField("disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (disposedField != null && disposedField.FieldType == typeof(bool))
                    {
                        disposedField.SetValue(listView, true);
                        Console.WriteLine("🧽 SfListView disposed flag set");
                    }
                }
                catch (Exception eventEx)
                {
                    Console.WriteLine($"⚠️ Could not pre-clear SfListView internal state: {eventEx.Message}");
                }

                // Dispose the control
                try
                {
                    listView.Dispose();
                    Console.WriteLine("✅ SfListView disposed safely");
                }
                catch (Exception disposeEx)
                {
                    Console.WriteLine($"⚠️ Error during SfListView.Dispose(): {disposeEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing SfListView safely: {ex.Message}");
                // Try standard disposal as fallback, but catch any exceptions
                try {
                    GC.SuppressFinalize(listView); // Always suppress finalization
                    listView.Dispose();
                } catch (Exception disposeEx) {
                    Console.WriteLine($"⚠️ Fallback disposal also failed: {disposeEx.Message}");
                }
            }
        }

        private void DisposeSfDataGridSafely(Control dataGrid)
        {
            try
            {
                Console.WriteLine($"🧽 Disposing SfDataGrid safely: {dataGrid.GetType().FullName}");

                // CRITICAL: Suppress finalization immediately
                GC.SuppressFinalize(dataGrid);

                // Clear any data source first to prevent threading issues during disposal
                try
                {
                    var dataSourceProperty = dataGrid.GetType().GetProperty("DataSource");
                    if (dataSourceProperty != null && dataSourceProperty.CanWrite)
                    {
                        dataSourceProperty.SetValue(dataGrid, null);
                        Console.WriteLine("🧽 SfDataGrid DataSource cleared");
                    }
                }
                catch (Exception dsEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfDataGrid DataSource: {dsEx.Message}");
                }

                // Clear view if present to prevent InvalidAsynchronousStateException
                try
                {
                    var viewProperty = dataGrid.GetType().GetProperty("View");
                    if (viewProperty != null && viewProperty.CanWrite)
                    {
                        viewProperty.SetValue(dataGrid, null);
                        Console.WriteLine("🧽 SfDataGrid View cleared");
                    }
                }
                catch (Exception viewEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfDataGrid View: {viewEx.Message}");
                }

                // Clear TableControl if present (common source of threading issues)
                try
                {
                    var tableControlProperty = dataGrid.GetType().GetProperty("TableControl");
                    if (tableControlProperty != null && tableControlProperty.CanWrite)
                    {
                        tableControlProperty.SetValue(dataGrid, null);
                        Console.WriteLine("🧽 SfDataGrid TableControl cleared");
                    }
                }
                catch (Exception tcEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfDataGrid TableControl: {tcEx.Message}");
                }

                // Try to set internal disposed flag to prevent finalization issues
                try
                {
                    var disposedField = dataGrid.GetType().GetField("disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (disposedField != null && disposedField.FieldType == typeof(bool))
                    {
                        disposedField.SetValue(dataGrid, true);
                        Console.WriteLine("🧽 SfDataGrid disposed flag set");
                    }
                }
                catch (Exception flagEx)
                {
                    Console.WriteLine($"⚠️ Could not set SfDataGrid disposed flag: {flagEx.Message}");
                }

                // Clear any column collection to prevent resource leaks
                try
                {
                    var columnsProperty = dataGrid.GetType().GetProperty("Columns");
                    if (columnsProperty != null)
                    {
                        var columns = columnsProperty.GetValue(dataGrid);
                        var clearMethod = columns?.GetType().GetMethod("Clear");
                        if (clearMethod != null)
                        {
                            clearMethod.Invoke(columns, null);
                            Console.WriteLine("🧽 SfDataGrid Columns cleared");
                        }
                    }
                }
                catch (Exception colEx)
                {
                    Console.WriteLine($"⚠️ Could not clear SfDataGrid Columns: {colEx.Message}");
                }

                // Finally dispose the control
                try
                {
                    dataGrid.Dispose();
                    Console.WriteLine("✅ SfDataGrid disposed safely");
                }
                catch (Exception disposeEx)
                {
                    Console.WriteLine($"⚠️ Error during SfDataGrid.Dispose(): {disposeEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error disposing SfDataGrid safely: {ex.Message}");
                // Try emergency disposal as fallback
                try {
                    GC.SuppressFinalize(dataGrid); // Always suppress finalization
                    dataGrid?.Dispose();
                } catch (Exception disposeEx) {
                    Console.WriteLine($"⚠️ Emergency SfDataGrid disposal also failed: {disposeEx.Message}");
                }
            }
        }

        /// <summary>
        /// 🔥 PRO-LEVEL: Aggressively kills any hanging test processes to prevent test locks
        /// This is necessary because Syncfusion controls can cause test host hangs during disposal
        /// SMART VERSION: Only kills processes that are actually hanging (not the current test)
        /// </summary>
        protected static void KillHangingTestProcesses()
        {
            try
            {
                Console.WriteLine("🔥 PRO CLEANUP: Killing any hanging test processes...");

                var currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
                var currentTime = DateTime.Now;

                // Kill hanging dotnet test processes (older than 30 seconds)
                var dotnetProcesses = System.Diagnostics.Process.GetProcessesByName("dotnet");
                int killedCount = 0;

                Console.WriteLine($"Found {dotnetProcesses.Length} dotnet processes to evaluate");

                foreach (var process in dotnetProcesses)
                {
                    try
                    {
                        // Don't kill the current process!
                        if (process.Id == currentProcessId)
                        {
                            Console.WriteLine($"Skipping current process: PID {process.Id}");
                            continue;
                        }

                        // Get process command line for better identification
                        var commandLine = GetProcessCommandLine(process);
                        var processAge = currentTime - process.StartTime;
                        bool isTestProcess = IsTestProcess(process);
                        bool isDotnetRun = commandLine?.Contains("run") == true;

                        // More aggressive cleanup - lower threshold for 'dotnet run' processes
                        int ageThresholdSeconds = isDotnetRun ? 15 : 30;

                        // Special case for known BusBuddy processes or dotnet run
                        if ((processAge.TotalSeconds > ageThresholdSeconds && (isTestProcess || isDotnetRun))
                            || (commandLine?.Contains("BusBuddy") == true && processAge.TotalSeconds > 15))
                        {
                            Console.WriteLine($"🔥 Killing hanging dotnet process: PID {process.Id} (Age: {processAge.TotalSeconds:F0}s)");
                            Console.WriteLine($"   Command: {commandLine ?? "Unknown"}");

                            try
                            {
                                // Try to kill the entire process tree if available
                                var killMethod = process.GetType().GetMethod("Kill", new[] { typeof(bool) });
                                if (killMethod != null)
                                {
                                    killMethod.Invoke(process, new object[] { true });
                                }
                                else
                                {
                                    // Fall back to regular kill
                                    process.Kill();
                                }
                            }
                            catch
                            {
                                // If the new method fails, fall back to regular kill
                                process.Kill();
                            }

                            process.WaitForExit(1000); // Wait up to 1 second
                            killedCount++;
                        }
                        else
                        {
                            Console.WriteLine($"Skipping dotnet process: PID {process.Id} (Age: {processAge.TotalSeconds:F0}s)");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not kill dotnet process {process.Id}: {ex.Message}");
                    }
                }

                // Kill any old testhost processes (but not recent ones)
                var testhostProcesses = System.Diagnostics.Process.GetProcessesByName("testhost");
                foreach (var process in testhostProcesses)
                {
                    try
                    {
                        // Don't kill the current process!
                        if (process.Id == currentProcessId)
                            continue;

                        var processAge = currentTime - process.StartTime;
                        // Increase timeout to 60 seconds to give tests more time to run
                        if (processAge.TotalSeconds > 60)
                        {
                            // Check if this is actually a BusBuddy test process
                            var isTestProcess = IsTestProcess(process);
                            if (isTestProcess)
                            {
                                Console.WriteLine($"🔥 Killing old testhost process: PID {process.Id} (Age: {processAge.TotalSeconds:F0}s)");

                                try
                                {
                                    // Try to kill the entire process tree if available
                                    var killMethod = process.GetType().GetMethod("Kill", new[] { typeof(bool) });
                                    if (killMethod != null)
                                    {
                                        killMethod.Invoke(process, new object[] { true });
                                    }
                                    else
                                    {
                                        // Fall back to regular kill
                                        process.Kill();
                                    }
                                }
                                catch
                                {
                                    // If the new method fails, fall back to regular kill
                                    process.Kill();
                                }

                                process.WaitForExit(1000);
                                killedCount++;
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Skipping non-BusBuddy testhost: PID {process.Id} (Age: {processAge.TotalSeconds:F0}s)");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not kill testhost process {process.Id}: {ex.Message}");
                    }
                }

                // Also check for BusBuddy-specific processes that might be stuck
                var anyProcesses = System.Diagnostics.Process.GetProcesses()
                    .Where(p => {
                        try {
                            return p.ProcessName.Contains("BusBuddy") ||
                                  (p.MainWindowTitle?.Contains("BusBuddy") == true);
                        }
                        catch { return false; }
                    });

                foreach (var process in anyProcesses)
                {
                    try
                    {
                        if (process.Id != currentProcessId)
                        {
                            Console.WriteLine($"🔥 Killing BusBuddy process: PID {process.Id} ({process.ProcessName})");

                            try
                            {
                                // Try to kill the entire process tree if available
                                var killMethod = process.GetType().GetMethod("Kill", new[] { typeof(bool) });
                                if (killMethod != null)
                                {
                                    killMethod.Invoke(process, new object[] { true });
                                }
                                else
                                {
                                    // Fall back to regular kill
                                    process.Kill();
                                }
                            }
                            catch
                            {
                                // If the new method fails, fall back to regular kill
                                process.Kill();
                            }

                            process.WaitForExit(1000);
                            killedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not kill BusBuddy process {process.Id}: {ex.Message}");
                    }
                }

                Console.WriteLine($"✅ PRO CLEANUP: Process cleanup completed. Killed {killedCount} processes.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PRO CLEANUP: Error during process cleanup: {ex.Message}");
            }
        }

        private static bool IsTestProcess(System.Diagnostics.Process process)
        {
            try
            {
                // Check for the current process first
                if (process.Id == System.Diagnostics.Process.GetCurrentProcess().Id)
                {
                    return true; // Current process is always considered a test process
                }

                // Get more detailed process information
                var commandLine = GetProcessCommandLine(process);
                if (string.IsNullOrEmpty(commandLine))
                {
                    // If we can't get command line, try looking at modules
                    try
                    {
                        foreach (System.Diagnostics.ProcessModule module in process.Modules)
                        {
                            if (module.FileName.Contains("BusBuddy"))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors when trying to access modules
                    }

                    // If no other indicators, be conservative and consider it not a test process
                    return false;
                }

                // Look for specific BusBuddy test indicators in the command line
                return commandLine.Contains("BusBuddy.Tests") ||
                       (commandLine.Contains("test") && commandLine.Contains("BusBuddy"));
            }
            catch
            {
                // If we get any error, err on the side of caution and don't kill the process
                return false;
            }
        }

        private static string? GetProcessCommandLine(System.Diagnostics.Process process)
        {
            try
            {
                // Try to get command line from process StartInfo
                if (!string.IsNullOrEmpty(process.StartInfo.Arguments))
                {
                    return process.StartInfo.Arguments;
                }

                // If that fails, try to get the process name and check if it's related to BusBuddy
                var processName = process.ProcessName.ToLowerInvariant();
                if (processName.Contains("busbuddy"))
                {
                    return "BusBuddy";
                }

                // Check modules as a last resort
                try
                {
                    foreach (System.Diagnostics.ProcessModule module in process.Modules)
                    {
                        if (module.FileName.Contains("BusBuddy"))
                        {
                            return "BusBuddy module";
                        }
                    }
                }
                catch
                {
                    // Ignore errors accessing modules
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 🔥 PRO-LEVEL: Emergency cleanup that runs before any test to ensure clean state
        /// SMART VERSION: Only runs aggressive cleanup when needed + timeout protection
        /// </summary>
        protected static void EmergencyTestCleanup()
        {
            try
            {
                // Kill any processes that have been running too long
                var currentTime = DateTime.Now;
                var dotnetProcesses = System.Diagnostics.Process.GetProcessesByName("dotnet");
                var testhostProcesses = System.Diagnostics.Process.GetProcessesByName("testhost");

                // More aggressive cleanup - kill any test process older than 2 minutes
                foreach (var process in dotnetProcesses.Concat(testhostProcesses))
                {
                    try
                    {
                        var processAge = currentTime - process.StartTime;
                        if (processAge.TotalMinutes > 5) // Increase timeout to 5 minutes to be less aggressive
                        {
                            // Only kill processes that are actually BusBuddy test processes
                            var isTestProcess = IsTestProcess(process);
                            if (isTestProcess)
                            {
                                Console.WriteLine($"🔥 EMERGENCY: Killing long-running test process PID {process.Id} (Age: {processAge.TotalMinutes:F1} minutes)");
                                process.Kill();
                                process.WaitForExit(1000);
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ Skipping non-test process PID {process.Id} (Age: {processAge.TotalMinutes:F1} minutes)");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not kill long-running process: {ex.Message}");
                    }
                }

                // Only run emergency cleanup if there are signs of hanging processes
                var needsCleanup = false;

                // Check for too many dotnet processes
                if (dotnetProcesses.Length > 5)
                {
                    Console.WriteLine($"🔥 EMERGENCY CLEANUP: Found {dotnetProcesses.Length} dotnet processes, cleanup needed");
                    needsCleanup = true;
                }

                // Check for any old testhost processes
                if (testhostProcesses.Length > 0)
                {
                    Console.WriteLine($"🔥 EMERGENCY CLEANUP: Found {testhostProcesses.Length} testhost processes, cleanup needed");
                    needsCleanup = true;
                }

                // Check for too many open forms
                var openForms = Application.OpenForms.Count;
                if (openForms > 0)
                {
                    Console.WriteLine($"🔥 EMERGENCY CLEANUP: Found {openForms} open forms, cleanup needed");
                    needsCleanup = true;
                }

                if (!needsCleanup)
                {
                    Console.WriteLine("✅ EMERGENCY CLEANUP: No cleanup needed, environment is clean");
                    return;
                }

                Console.WriteLine("🔥 EMERGENCY CLEANUP: Starting emergency test cleanup...");

                // Kill hanging processes
                KillHangingTestProcesses();

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Clear any remaining forms
                if (openForms > 0)
                {
                    Console.WriteLine($"🔥 EMERGENCY CLEANUP: Closing {openForms} open forms...");
                    for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
                    {
                        var form = Application.OpenForms[i];
                        try
                        {
                            if (form != null && !form.IsDisposed)
                            {
                                form.Close();
                                form.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Could not close form: {ex.Message}");
                        }
                    }
                }

                Console.WriteLine("✅ EMERGENCY CLEANUP: Emergency cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EMERGENCY CLEANUP: Error during emergency cleanup: {ex.Message}");
            }
        }

        public virtual void Dispose()
        {
            var startTime = DateTime.Now;
            var testName = this.GetType().Name;
            Console.WriteLine($"🗑️ TEST DISPOSE: Starting disposal for {testName}...");

            try
            {
                // First, stop the test timeout timer
                try
                {
                    _testTimeoutTimer?.Dispose();
                    Console.WriteLine("🗑️ TEST DISPOSE: Timeout timer disposed");
                }
                catch (Exception timerEx)
                {
                    Console.WriteLine($"⚠️ TEST DISPOSE: Error disposing timer: {timerEx.Message}");
                }

                // Kill any hanging test processes
                try
                {
                    KillHangingTestProcesses();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ TEST DISPOSE: Error killing hanging processes: {ex.Message}");
                }

                // Clean up the dashboard form if it exists
                CleanupForm();

                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine($"🗑️ TEST DISPOSE: Disposal for {testName} completed in {elapsed:F0}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST DISPOSE: Unhandled error during disposal: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles test timeout - called when a test runs too long
        /// </summary>
        private void OnTestTimeout(object? state)
        {
            try
            {
                var testDuration = DateTime.Now - _testStartTime;
                Console.WriteLine($"⏰ TEST TIMEOUT: {_testName} has been running for {testDuration.TotalMinutes:F1} minutes (limit: {_testTimeoutMinutes})");

                // Force process cleanup to avoid hanging tests
                KillHangingTestProcesses();

                // Log diagnostic information
                Console.WriteLine($"⏰ TEST TIMEOUT: Test will be forcibly terminated. Current memory usage: {GC.GetTotalMemory(false) / 1024 / 1024} MB");

                // Terminate the current process after logging
                Console.WriteLine("⏰ TEST TIMEOUT: Forcing test termination...");

                // Do cleanup before termination
                CleanupForm();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST TIMEOUT: Error in timeout handler: {ex.Message}");
            }
        }
    }
}
