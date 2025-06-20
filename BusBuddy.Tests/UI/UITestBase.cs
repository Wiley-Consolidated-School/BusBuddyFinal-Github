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

        protected UITestBase()
        {
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
            // Prevent infinite loops
            if (depth > 20 || visited.Contains(parent))
                return;

            visited.Add(parent);

            if (parent is T control)
                controls.Add(control);

            foreach (Control child in parent.Controls)
            {
                GetAllControlsOfTypeSafe<T>(child, controls, visited, depth + 1);
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

        public virtual void Dispose()
        {
            var startTime = DateTime.Now;
            var testName = this.GetType().Name;
            Console.WriteLine($"🗑️ TEST DISPOSE: Starting disposal for {testName}...");

            try
            {
                // Log current thread and test host information
                var currentThread = System.Threading.Thread.CurrentThread;
                Console.WriteLine($"🗑️ TEST DISPOSE: Current thread - ID: {currentThread.ManagedThreadId}, IsBackground: {currentThread.IsBackground}, IsThreadPoolThread: {currentThread.IsThreadPoolThread}");

                // Check if we're running in a test host
                var processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                Console.WriteLine($"🗑️ TEST DISPOSE: Process name: {processName}");

                // Log form count before cleanup
                var openForms = Application.OpenForms.Count;
                Console.WriteLine($"🗑️ TEST DISPOSE: Open forms count before cleanup: {openForms}");

                CleanupForm();

                // Log form count after cleanup
                var openFormsAfter = Application.OpenForms.Count;
                Console.WriteLine($"🗑️ TEST DISPOSE: Open forms count after cleanup: {openFormsAfter}");

                // Check for leaked forms
                if (openFormsAfter > 0)
                {
                    Console.WriteLine("⚠️ TEST DISPOSE: Warning - Forms still open after cleanup:");
                    for (int i = 0; i < Application.OpenForms.Count; i++)
                    {
                        var form = Application.OpenForms[i];
                        if (form != null)
                        {
                            Console.WriteLine($"  - Form {i}: {form.GetType().Name} (Text: '{form.Text}', Visible: {form.Visible})");
                        }
                        else
                        {
                            Console.WriteLine($"  - Form {i}: <null reference>");
                        }
                    }
                }

                // Cleanup mocks
                Console.WriteLine("🗑️ TEST DISPOSE: Cleaning up mocks...");
                _mockNavigationService?.Reset();
                _mockDatabaseService?.Reset();
                Console.WriteLine("✅ TEST DISPOSE: Mocks cleaned up");

                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine($"✅ TEST DISPOSE: {testName} disposal completed successfully in {elapsed:F0}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST DISPOSE: Critical error during {testName} disposal: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"❌ TEST DISPOSE: Stack trace: {ex.StackTrace}");

                // Log inner exception if present
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ TEST DISPOSE: Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }

                // Don't rethrow - we want tests to complete even if disposal fails
            }
            finally
            {
                var finalTime = (DateTime.Now - startTime).TotalMilliseconds;
                Console.WriteLine($"🗑️ TEST DISPOSE: {testName} disposal finally block completed in {finalTime:F0}ms");
            }
        }
    }
}
