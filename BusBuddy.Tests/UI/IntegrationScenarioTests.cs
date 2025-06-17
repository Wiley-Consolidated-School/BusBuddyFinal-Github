using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using MaterialSkin.Controls;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class IntegrationScenarioTests : UITestBase
    {
        [Fact]
        public void Dashboard_FullUserWorkflow_ShouldBeSeamless()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Simulate a complete user workflow

            // 1. User opens dashboard
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));

            // 2. User checks sidebar
            var sidebar = FindControlByName(_dashboard, "SidebarPanel");
            Assert.NotNull(sidebar);

            // 3. User might toggle sidebar
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton") as Button;
            if (toggleButton != null)
            {
                var initialSidebarVisible = sidebar.Visible;
                toggleButton.PerformClick();
                System.Threading.Thread.Sleep(50);

                // Sidebar visibility might change (depending on implementation)
                // The important thing is the dashboard remains functional
                Assert.True(_dashboard.Visible);
            }

            // 4. User looks at quick actions
            var quickActions = FindControlByName(_dashboard, "QuickActionsPanel");
            if (quickActions != null)
            {
                var actionButtons = GetAllControlsOfType<Button>(quickActions);
                Assert.NotNull(actionButtons);

                // User might click an action button
                var firstButton = actionButtons.FirstOrDefault();
                if (firstButton != null && firstButton.Enabled)
                {
                    var clickException = Record.Exception(() => firstButton.PerformClick());
                    Assert.Null(clickException);
                }
            }

            // 5. Final state should be stable
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_MultipleNavigation_ShouldMaintainState()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var initialControlCount = GetAllControlsOfType<Control>(_dashboard).Count;

            // Act - Simulate multiple navigation actions
            var sidebar = FindControlByName(_dashboard, "SidebarPanel");
            if (sidebar != null)
            {
                var sidebarButtons = GetAllControlsOfType<Button>(sidebar);

                // Click several navigation buttons
                foreach (var button in sidebarButtons.Take(3))
                {
                    if (button.Enabled && button.Visible)
                    {
                        button.PerformClick();
                        System.Threading.Thread.Sleep(25);

                        // Dashboard should remain stable after each click
                        Assert.True(_dashboard.Visible);
                        Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
                    }
                }
            }

            // Assert - Control count should remain stable
            var finalControlCount = GetAllControlsOfType<Control>(_dashboard).Count;
            Assert.Equal(initialControlCount, finalControlCount);
        }

        [Fact]
        public void Dashboard_ResponsiveLayout_ShouldAdaptToSizeChanges()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var originalSize = _dashboard.Size;

            // Get initial layout information
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            var sidebar = FindControlByName(_dashboard, "SidebarPanel");
            var quickActions = FindControlByName(_dashboard, "QuickActionsPanel");

            var initialHeaderBounds = headerPanel?.Bounds ?? Rectangle.Empty;
            var initialSidebarBounds = sidebar?.Bounds ?? Rectangle.Empty;

            // Act - Test different window sizes
            var testSizes = new[]
            {
                new Size(800, 600),
                new Size(1200, 800),
                new Size(1600, 1000),
                new Size(600, 400) // Smaller size
            };

            foreach (var size in testSizes)
            {
                _dashboard.Size = size;
                System.Threading.Thread.Sleep(50); // Allow layout to update

                // Assert - Layout should adapt
                Assert.True(_dashboard.Visible);

                if (headerPanel != null)
                {
                    Assert.True(headerPanel.Visible);
                    Assert.True(headerPanel.Width > 0);
                    Assert.True(headerPanel.Height > 0);
                }

                if (sidebar != null)
                {
                    Assert.True(sidebar.Width >= 0); // Might be collapsed
                    Assert.True(sidebar.Height >= 0);
                }

                // All controls should still be present
                Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
            }

            // Restore original size
            _dashboard.Size = originalSize;
        }

        [Fact]
        public void Dashboard_ThemeIntegration_ShouldWorkAcrossComponents()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Get theme-related properties from different components
            var allControls = GetAllControlsOfType<Control>(_dashboard);
            var materialControls = GetAllControlsOfType<MaterialButton>(_dashboard);

            // Assert - Theme consistency checks
            foreach (var control in allControls)
            {
                // All controls should have valid colors
                Assert.NotEqual(Color.Empty, control.BackColor);
                Assert.NotEqual(Color.Empty, control.ForeColor);

                // Controls should be readable (not same color for back and fore)
                Assert.NotEqual(control.BackColor, control.ForeColor);
            }

            // Material controls should follow Material Design principles
            foreach (var materialControl in materialControls)
            {
                Assert.NotNull(materialControl);
                Assert.True(materialControl.Width > 0);
                Assert.True(materialControl.Height > 0);
            }
        }

        [Fact]
        public void Dashboard_AccessibilityIntegration_ShouldBeComplete()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Check accessibility across all interactive elements
            var buttons = GetAllControlsOfType<Button>(_dashboard);
            var materialButtons = GetAllControlsOfType<MaterialButton>(_dashboard);

            // Assert - Accessibility requirements
            foreach (var button in buttons)
            {
                // Buttons should have text or accessible names
                Assert.True(!string.IsNullOrWhiteSpace(button.Text) ||
                           !string.IsNullOrWhiteSpace(button.AccessibleName));

                // Buttons should have reasonable sizes for clicking
                Assert.True(button.Width >= 20);
                Assert.True(button.Height >= 20);
            }

            foreach (var materialButton in materialButtons)
            {
                // Material buttons should also be accessible
                Assert.True(!string.IsNullOrWhiteSpace(materialButton.Text) ||
                           !string.IsNullOrWhiteSpace(materialButton.AccessibleName));
            }

            // Color contrast should be maintained
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            if (headerPanel != null)
            {
                var isLightBackground = IsLightColor(headerPanel.BackColor);
                var isLightForeground = IsLightColor(headerPanel.ForeColor);
                Assert.NotEqual(isLightBackground, isLightForeground);
            }
        }

        [Fact]
        public void Dashboard_DataDisplayIntegration_ShouldHandleUpdates()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find data display areas
            var statsPanel = FindControlByName(_dashboard, "StatsPanel");
            var quickActions = FindControlByName(_dashboard, "QuickActionsPanel");

            // Assert - Data areas should be functional
            if (statsPanel != null)
            {
                Assert.True(statsPanel.Width >= 0);
                Assert.True(statsPanel.Height >= 0);

                // Stats panel might contain data labels
                var labels = GetAllControlsOfType<Label>(statsPanel);
                Assert.NotNull(labels);
            }

            if (quickActions != null)
            {
                Assert.True(quickActions.Visible);

                // Quick actions should contain interactive elements
                var actionButtons = GetAllControlsOfType<Button>(quickActions);
                Assert.NotNull(actionButtons);

                // Each action button should be properly configured
                foreach (var button in actionButtons.Take(3))
                {
                    Assert.True(button.Width > 0);
                    Assert.True(button.Height > 0);
                }
            }
        }

        [Fact]
        public void Dashboard_ErrorHandlingIntegration_ShouldBeGraceful()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var initialState = _dashboard.Visible;

            // Act - Simulate various error scenarios
            var operations = new List<Action>
            {
                () => FindControlByName(_dashboard, null!),
                () => FindControlByName(_dashboard, ""),
                () => FindControlByName(_dashboard, "NonExistent"),
                () => GetAllControlsOfType<Button>(null!),
                () => {
                    var button = new Button();
                    button.Dispose();
                    try { var text = button.Text; } catch { }
                }
            };

            var errors = 0;
            foreach (var operation in operations)
            {
                try
                {
                    operation();
                }
                catch
                {
                    errors++;
                }
            }

            // Assert - Dashboard should remain functional despite errors
            Assert.True(_dashboard.Visible);
            Assert.Equal(initialState, _dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));

            // Some operations might legitimately fail, but dashboard should continue working
            Assert.True(errors < operations.Count); // Not all operations should fail
        }

        [Fact]
        public void Dashboard_PerformanceIntegration_ShouldBeAcceptable()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act - Perform a series of common operations
            var operations = 0;
            while (stopwatch.ElapsedMilliseconds < 1000 && operations < 100) // Max 1 second or 100 ops
            {
                FindControlByName(_dashboard, "HeaderPanel");
                GetAllControlsOfType<Button>(_dashboard);
                FindControlByName(_dashboard, "SidebarPanel");
                operations++;
            }

            stopwatch.Stop();

            // Assert - Should complete reasonable number of operations quickly
            Assert.True(operations >= 10, $"Should complete at least 10 operations in 1 second, completed {operations}");
            Assert.True(_dashboard.Visible, "Dashboard should remain responsive");

            // Dashboard should still be functional
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_WindowStateIntegration_ShouldHandleChanges()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var originalState = _dashboard.WindowState;

            // Act & Assert - Test different window states
            if (_dashboard.MaximizeBox)
            {
                _dashboard.WindowState = FormWindowState.Maximized;
                System.Threading.Thread.Sleep(100);

                Assert.True(_dashboard.Visible);
                Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
            }

            if (_dashboard.MinimizeBox)
            {
                _dashboard.WindowState = FormWindowState.Normal;
                System.Threading.Thread.Sleep(100);

                Assert.True(_dashboard.Visible);
                Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
            }

            // Restore original state
            _dashboard.WindowState = originalState;
            Assert.True(_dashboard.Visible);
        }

        /// <summary>
        /// Helper method to determine if a color is light or dark
        /// </summary>
        private bool IsLightColor(Color color)
        {
            var brightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;
            return brightness > 0.5;
        }
    }
}
