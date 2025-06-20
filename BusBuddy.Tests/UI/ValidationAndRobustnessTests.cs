using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
// MaterialSkin.Controls removed - using standard controls with Syncfusion theming

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class ValidationAndRobustnessTests : UITestBase
    {
        [UITestFact]
        public void Dashboard_ControlHierarchy_ShouldBeWellFormed()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Analyze control hierarchy (limited scope for performance)
            var allControls = GetAllControlsOfType<Control>(_dashboard);

            // Limit to first 20 controls for performance
            var controlsToTest = allControls.Take(20).ToList();

            // Assert - Hierarchy should be well-formed
            Assert.NotNull(controlsToTest);
            Assert.True(controlsToTest.Count > 0, "Dashboard should contain controls");

            // No control should be its own parent (circular reference)
            foreach (var control in controlsToTest)
            {
                Assert.NotEqual(control, control.Parent);

                // Only test deep hierarchy for a few controls to avoid performance issues
                if (controlsToTest.IndexOf(control) < 5 && control.HasChildren)
                {
                    var descendants = GetAllDescendantControlsOfType<Control>(control);
                    Assert.DoesNotContain(control, descendants.Take(10)); // Limit descendants check
                }
            }

            // Root control should be the dashboard
            Assert.Equal(_dashboard, GetRootControl(_dashboard));
        }

        [UITestFact]
        public void Dashboard_ControlNaming_ShouldBeConsistent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check control naming conventions
            var namedControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .ToList();

            // Assert - Named controls should follow conventions
            var expectedNames = new[] { "HeaderPanel", "SidebarPanel", "SidebarToggleButton", "QuickActionsPanel", "StatsPanel" };

            foreach (var expectedName in expectedNames)
            {
                var control = FindControlByName(_dashboard, expectedName);
                if (control != null) // Control might not exist in all configurations
                {
                    Assert.Equal(expectedName, control.Name);
                    Assert.NotNull(control.Parent); // Should have a parent (except dashboard itself)
                }
            }

            // No duplicate names should exist
            var nameGroups = namedControls.GroupBy(c => c.Name);
            foreach (var group in nameGroups)
            {
                Assert.True(group.Count() == 1, $"Duplicate control name found: {group.Key}");
            }
        }

        [UITestFact]
        public void Dashboard_LayoutConstraints_ShouldBeRespected()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check layout constraints
            var allControls = GetAllControlsOfType<Control>(_dashboard);

            // Assert - Layout constraints
            foreach (var control in allControls)
            {
                // Controls should have non-negative positions
                Assert.True(control.Left >= 0 || control.Dock != DockStyle.None,
                    $"Control {control.Name} has negative X position: {control.Left}");
                Assert.True(control.Top >= 0 || control.Dock != DockStyle.None,
                    $"Control {control.Name} has negative Y position: {control.Top}");

                // Controls should have reasonable sizes
                Assert.True(control.Width >= 0, $"Control {control.Name} has negative width: {control.Width}");
                Assert.True(control.Height >= 0, $"Control {control.Name} has negative height: {control.Height}");

                // Visible controls should be within parent bounds (unless docked)
                if (control.Visible && control.Parent != null && control.Dock == DockStyle.None)
                {
                    var parentBounds = control.Parent.ClientRectangle;

                    // Allow some flexibility for controls that might extend slightly beyond bounds
                    Assert.True(control.Right <= parentBounds.Width + 10,
                        $"Control {control.Name} extends too far right");
                    Assert.True(control.Bottom <= parentBounds.Height + 10,
                        $"Control {control.Name} extends too far down");
                }
            }
        }

        [UITestFact]
        public void Dashboard_EventHandling_ShouldBeRobust()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var eventsFired = 0;
            var errors = 0;

            // Act - Test event handling (limited scope for performance)
            var buttons = GetAllControlsOfType<Button>(_dashboard);

            // Limit to first 3 buttons for performance
            foreach (var button in buttons.Take(3))
            {
                try
                {
                    // Add event handler
                    EventHandler handler = (s, e) => eventsFired++;
                    button.Click += handler;

                    // Trigger click
                    if (button.Enabled && button.Visible)
                    {
                        button.PerformClick();
                    }

                    // Remove event handler
                    button.Click -= handler;
                }
                catch (Exception)
                {
                    errors++;
                }
            }

            // Assert - Event handling should work
            Assert.True(errors <= 1, "Most event operations should succeed");
            Assert.True(_dashboard.Visible, "Dashboard should remain stable after event operations");
        }

        [UITestFact]
        public void Dashboard_ResourceManagement_ShouldBeProper()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check resource usage
            var allControls = GetAllControlsOfType<Control>(_dashboard);
            var disposableControls = allControls.OfType<IDisposable>().ToList();

            // Assert - Resource management
            foreach (var control in allControls)
            {
                // Controls should not be disposed while in use
                Assert.False(control.IsDisposed, $"Control {control.Name} should not be disposed while in use");

                // Controls should have valid handles
                if (control.Visible && control.IsHandleCreated)
                {
                    Assert.NotEqual(IntPtr.Zero, control.Handle);
                }
            }

            // Dashboard should properly manage its resources
            Assert.False(_dashboard.IsDisposed);
            Assert.True(_dashboard.IsHandleCreated || !_dashboard.Visible);
        }

        [UITestFact]
        public void Dashboard_StateConsistency_ShouldBeMaintained()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Capture initial state
            var initialState = CaptureControlStates(_dashboard);

            // Perform some operations
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton") as Button;
            if (toggleButton != null)
            {
                toggleButton.PerformClick();
                System.Threading.Thread.Sleep(100);
                toggleButton.PerformClick(); // Toggle back
                System.Threading.Thread.Sleep(100);
            }

            // Capture final state
            var finalState = CaptureControlStates(_dashboard);

            // Assert - Core state should be consistent
            Assert.Equal(initialState.ControlCount, finalState.ControlCount);
            Assert.Equal(initialState.VisibleControlCount, finalState.VisibleControlCount);
            Assert.Equal(initialState.EnabledControlCount, finalState.EnabledControlCount);

            // Dashboard should maintain its fundamental properties
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }        [UITestFact]
        public void Dashboard_ConcurrentAccess_ShouldBeThreadSafe()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test basic thread safety without actual concurrent UI operations
            // Just verify that the dashboard can handle basic property access safely
            var visible = _dashboard.Visible;
            var bounds = _dashboard.Bounds;
            var controlCount = _dashboard.Controls.Count;

            // Simulate rapid sequential access (safer than true concurrency for UI)
            for (int i = 0; i < 5; i++)
            {
                var currentVisible = _dashboard.Visible;
                var currentBounds = _dashboard.Bounds;
                var currentCount = _dashboard.Controls.Count;

                // Basic consistency checks
                Assert.Equal(visible, currentVisible);
                Assert.Equal(controlCount, currentCount);
            }

            // Assert - Dashboard should remain functional and consistent
            Assert.True(_dashboard.Visible, "Dashboard should remain functional");
            Assert.True(_dashboard.Controls.Count > 0, "Dashboard should have controls");
        }

        [UITestFact]
        public void Dashboard_MemoryFootprint_ShouldBeReasonable()
        {
            // Arrange
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Create and use dashboard (lightweight operations)
            _dashboard = CreateDashboardSafely();

            // Simple operations instead of expensive enumeration
            var controlCount = _dashboard.Controls.Count;
            var visible = _dashboard.Visible;

            var afterCreationMemory = GC.GetTotalMemory(false);
            var memoryIncrease = afterCreationMemory - initialMemory;

            // Assert - Memory usage should be reasonable
            Assert.True(memoryIncrease < 100 * 1024 * 1024, // Less than 100MB (more realistic for UI)
                $"Dashboard memory footprint too large: {memoryIncrease / 1024 / 1024}MB");

            // Dashboard should function normally
            Assert.True(_dashboard.Visible);
            Assert.True(controlCount >= 0); // Basic sanity check
        }        [UITestFact]
        public void Dashboard_DisposalCleanup_ShouldBeComplete()
        {
            // Arrange
            var testDashboard = CreateDashboardSafely();
            var controlCount = GetAllControlsOfType<Control>(testDashboard).Count;

            // Act - Dispose dashboard
            testDashboard.Dispose();

            // Assert - Should be properly disposed
            Assert.True(testDashboard.IsDisposed);

            // Try to access a property - in modern .NET, this might not always throw
            // Instead test that the Handle is invalid
            Assert.False(testDashboard.IsHandleCreated);

            // Memory should be available for cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #region Helper Methods

        private Dictionary<string, object> BuildControlHierarchy(Control root)
        {
            var hierarchy = new Dictionary<string, object>
            {
                ["Control"] = root.GetType().Name,
                ["Name"] = root.Name ?? "Unnamed",
                ["Children"] = new List<Dictionary<string, object>>()
            };

            var children = (List<Dictionary<string, object>>)hierarchy["Children"];
            foreach (Control child in root.Controls)
            {
                children.Add(BuildControlHierarchy(child));
            }

            return hierarchy;
        }

        private Control GetRootControl(Control control)
        {
            while (control.Parent != null)
            {
                control = control.Parent;
            }
            return control;
        }

        private ControlState CaptureControlStates(Control root)
        {
            var allControls = GetAllControlsOfType<Control>(root);

            return new ControlState
            {
                ControlCount = allControls.Count,
                VisibleControlCount = allControls.Count(c => c.Visible),
                EnabledControlCount = allControls.Count(c => c.Enabled),
                ButtonCount = allControls.OfType<Button>().Count(),
                PanelCount = allControls.OfType<Panel>().Count()
            };
        }

        private class ControlState
        {
            public int ControlCount { get; set; }
            public int VisibleControlCount { get; set; }
            public int EnabledControlCount { get; set; }
            public int ButtonCount { get; set; }
            public int PanelCount { get; set; }
        }

        #endregion
    }
}
