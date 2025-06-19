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

namespace BusBuddy.Tests.UI
{
    /// <summary>
    /// Base class for all UI tests that provides common setup and utilities
    /// </summary>
    public abstract class UITestBase : IDisposable
    {
        protected BusBuddyDashboardSyncfusion? _dashboard;
        protected Mock<INavigationService> _mockNavigationService;
        protected Mock<IDatabaseHelperService> _mockDatabaseService;

        protected UITestBase()
        {
            // Ensure Windows Forms is initialized
            WindowsFormsTestInitializer.Initialize();

            // Create mocks
            _mockNavigationService = new Mock<INavigationService>();
            _mockDatabaseService = new Mock<IDatabaseHelperService>();
        }        protected BusBuddyDashboardSyncfusion CreateDashboardSafely()
        {
            try
            {
                Console.WriteLine("🧪 TEST: Creating dashboard with safety checks...");
                var dashboard = new BusBuddyDashboardSyncfusion(_mockNavigationService.Object, _mockDatabaseService.Object);

                // Set a reasonable timeout for test operations
                var timeout = DateTime.Now.AddSeconds(15); // Increased timeout for debugging
                var startTime = DateTime.Now;

                Console.WriteLine("🧪 TEST: Showing dashboard...");

                // Try to show the form with timeout protection
                dashboard.Show();

                // Wait for initialization to complete with better timeout handling
                while (DateTime.Now < timeout)
                {
                    // Check if dashboard is properly initialized
                    if (dashboard.Visible && dashboard.IsHandleCreated)
                    {
                        Console.WriteLine($"🧪 TEST: Dashboard initialized successfully in {(DateTime.Now - startTime).TotalMilliseconds:F0}ms");
                        break;
                    }

                    // Process pending messages to prevent UI freeze
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }

                // Final timeout check
                if (DateTime.Now >= timeout)
                {
                    Console.WriteLine("⏰ TEST: Dashboard initialization timeout reached");
                    throw new TimeoutException("Dashboard initialization took too long");
                }

                // Ensure the dashboard is in a good state
                if (!dashboard.Visible)
                {
                    Console.WriteLine("⚠️ TEST: Dashboard not visible after initialization");
                }

                Console.WriteLine("✅ TEST: Dashboard created successfully");
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
            var bgLuminance = GetLuminance(background);
            var fgLuminance = GetLuminance(foreground);

            var lighter = Math.Max(bgLuminance, fgLuminance);
            var darker = Math.Min(bgLuminance, fgLuminance);

            var contrast = (lighter + 0.05) / (darker + 0.05);
            return contrast >= 4.5; // WCAG AA standard
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
            if (_dashboard != null)
            {
                try
                {
                    if (_dashboard.Visible)
                        _dashboard.Hide();

                    _dashboard.Dispose();
                }
                catch (Exception)
                {
                    // Ignore disposal errors in tests
                }
                finally
                {
                    _dashboard = null;
                }
            }
        }

        public virtual void Dispose()
        {
            CleanupForm();
        }
    }
}
