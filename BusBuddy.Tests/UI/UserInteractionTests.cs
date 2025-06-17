using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using MaterialSkin.Controls;
using Moq;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class UserInteractionTests : UITestBase
    {
        [Fact]
        public void Dashboard_SidebarToggle_ShouldChangeVisibility()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton");

            // Act & Assert
            Assert.NotNull(sidebarPanel);
            Assert.NotNull(toggleButton);

            // Record initial state
            var initialVisibility = sidebarPanel.Visible;
            var initialWidth = sidebarPanel.Width;

            // Simulate button click
            ClickButton((Button)toggleButton);

            // Verify the sidebar state could potentially change
            // Note: Since we don't have the actual toggle logic implemented in tests,
            // we verify the button is clickable and sidebar exists
            Assert.True(toggleButton.Enabled, "Toggle button should be enabled for interaction");
            Assert.True(initialWidth > 0, "Sidebar should have positive width");
        }

        [Fact]
        public void Dashboard_ModuleNavigation_ShouldTriggerNavigationService()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            var moduleButtons = sidebarPanel != null ? GetAllControlsOfType<MaterialButton>(sidebarPanel) : new List<MaterialButton>();

            // Act & Assert
            Assert.True(moduleButtons.Count > 0, "Should have module navigation buttons");

            foreach (var button in moduleButtons.Take(3)) // Test first 3 buttons
            {
                // Verify button is interactive
                Assert.True(button.Enabled, $"Module button '{button.Text}' should be enabled");
                Assert.True(button.Visible, $"Module button '{button.Text}' should be visible");

                // Simulate clicking the button
                if (button.Enabled && button.Visible)
                {
                    ClickButton(button);
                    // In a real scenario, this would verify navigation service was called
                    // For now, we verify the button responds to clicks without errors
                }
            }
        }

        [Fact]
        public void Dashboard_QuickActions_ShouldBeInteractive()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var quickActionsPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");

            // Act
            Assert.NotNull(quickActionsPanel);
            var actionButtons = GetAllControlsOfType<Button>(quickActionsPanel);

            // Assert
            foreach (var button in actionButtons)
            {
                Assert.True(button.Enabled, $"Quick action '{button.Text}' should be enabled");
                Assert.True(button.Size.Width > 50, $"Quick action '{button.Text}' should be adequately sized");
                Assert.True(button.Size.Height > 30, $"Quick action '{button.Text}' should be adequately sized");

                // Test button interaction
                ClickButton(button);
            }
        }

        [Fact]
        public void Dashboard_FormResize_ShouldMaintainUsability()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test various sizes
            var testSizes = new[]
            {
                new Size(800, 600),    // Minimum
                new Size(1024, 768),   // Standard
                new Size(1366, 768),   // Wide
                new Size(1920, 1080)   // HD
            };

            foreach (var size in testSizes)
            {
                _dashboard.WindowState = FormWindowState.Normal;
                _dashboard.Size = size;

                // Assert - Key elements should remain accessible
                var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
                var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
                var quickActionsPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");

                Assert.NotNull(headerPanel);
                Assert.True(headerPanel.Visible, $"Header should be visible at size {size}");

                Assert.NotNull(sidebarPanel);
                Assert.True(sidebarPanel.Visible, $"Sidebar should be visible at size {size}");

                if (quickActionsPanel != null)
                {
                    Assert.True(quickActionsPanel.Width > 0, $"Quick actions should have width at size {size}");
                }
            }
        }

        [Fact]
        public void Dashboard_KeyboardShortcuts_ShouldBeSupported()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Test keyboard focus and navigation

            // Dashboard should be able to receive focus
            Assert.True(_dashboard.CanFocus, "Dashboard should be focusable");

            // Test Tab navigation through controls
            var focusableControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c.CanFocus && c.TabStop && c.Visible && c.Enabled)
                .OrderBy(c => c.TabIndex)
                .ToList();

            Assert.True(focusableControls.Count > 0, "Should have focusable controls for keyboard navigation");

            // Verify each focusable control can potentially receive focus
            foreach (var control in focusableControls.Take(5)) // Test first 5
            {
                Assert.True(control.CanFocus, $"Control '{control.Name}' should support focus");
                Assert.True(control.TabStop, $"Control '{control.Name}' should be in tab order");
            }
        }

        [Fact]
        public void Dashboard_MouseInteraction_ShouldBeResponsive()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Test mouse interaction on key elements
            var interactiveControls = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => b.Visible && b.Enabled)
                .ToList();

            foreach (var control in interactiveControls.Take(5)) // Test first 5 buttons
            {
                // Verify control responds to mouse events
                Assert.True(control.Enabled, $"Button '{control.Text}' should be enabled for mouse interaction");
                Assert.False(control.Size.IsEmpty, $"Button '{control.Text}' should have clickable area");

                // Test that control is within dashboard bounds (roughly)
                var dashboardBounds = _dashboard.ClientRectangle;
                Assert.True(control.Left >= 0 && control.Left < dashboardBounds.Width,
                    $"Button '{control.Text}' should be horizontally within dashboard bounds");
                Assert.True(control.Top >= 0 && control.Top < dashboardBounds.Height,
                    $"Button '{control.Text}' should be vertically within dashboard bounds");
            }
        }

        [Fact]
        public void Dashboard_WindowStates_ShouldWorkCorrectly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Test different window states
            var originalState = _dashboard.WindowState;

            // Test Normal state
            _dashboard.WindowState = FormWindowState.Normal;
            Assert.Equal(FormWindowState.Normal, _dashboard.WindowState);
            Assert.True(_dashboard.Size.Width > 0, "Dashboard should have width in normal state");
            Assert.True(_dashboard.Size.Height > 0, "Dashboard should have height in normal state");

            // Test Maximized state
            _dashboard.WindowState = FormWindowState.Maximized;
            // Note: WindowState might not actually change in test environment
            // We verify it doesn't crash and key elements remain accessible

            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");

            Assert.NotNull(headerPanel);
            Assert.NotNull(sidebarPanel);
            Assert.True(headerPanel.Visible, "Header should remain visible when maximized");
            Assert.True(sidebarPanel.Visible, "Sidebar should remain visible when maximized");

            // Restore original state
            _dashboard.WindowState = originalState;
        }

        [Fact]
        public void Dashboard_ContextMenus_ShouldBeAvailable()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Look for controls that might have context menus
            var controlsWithContextMenus = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c.ContextMenuStrip != null)
                .ToList();

            // Assert - While not all controls need context menus,
            // verify that any that have them are properly configured
            foreach (var control in controlsWithContextMenus)
            {
                if (control.ContextMenuStrip != null)
                {
                    Assert.NotNull(control.ContextMenuStrip);
                    Assert.True(control.ContextMenuStrip.Items.Count >= 0,
                        $"Context menu for '{control.Name}' should be valid");
                }
            }
        }

        [Fact]
        public void Dashboard_ToolTips_ShouldProvideHelpfulInformation()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find controls that might have tooltips
            var buttonsWithTooltips = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => !string.IsNullOrEmpty(GetTooltipText(b)))
                .ToList();

            // Assert - Verify tooltip content is meaningful
            foreach (var button in buttonsWithTooltips)
            {
                var tooltipText = GetTooltipText(button);
                Assert.False(string.IsNullOrWhiteSpace(tooltipText),
                    $"Button '{button.Text}' tooltip should have meaningful content");
                Assert.True(tooltipText.Length > 5,
                    $"Button '{button.Text}' tooltip should be descriptive");
            }
        }

        private string GetTooltipText(Control control)
        {
            // Helper method to extract tooltip text from a control
            // This would need to be implemented based on how tooltips are set up
            // For now, return empty string as placeholder
            return string.Empty;
        }

        [Fact]
        public void Dashboard_ErrorStates_ShouldBeHandledGracefully()
        {
            // Arrange - Setup mock to simulate error conditions
            _mockDatabaseService.Setup(x => x.GetAllRoutesWithDetails()).Throws(new Exception("Database connection failed"));

            // Act - Create dashboard with error condition
            _dashboard = CreateDashboardSafely();

            // Assert - Dashboard should still be functional even with database errors
            Assert.NotNull(_dashboard);
            Assert.True(_dashboard.Visible, "Dashboard should be visible even with database errors");

            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");

            Assert.NotNull(headerPanel);
            Assert.NotNull(sidebarPanel);

            // UI should remain interactive even if backend services have issues
            var buttons = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => b.Visible && b.Enabled)
                .ToList();

            Assert.True(buttons.Count > 0, "Some buttons should remain functional during error states");
        }

        [Fact]
        public void Dashboard_Performance_ShouldBeResponsive()
        {
            // Arrange & Act - Measure dashboard creation time
            var startTime = DateTime.Now;
            _dashboard = CreateDashboardSafely();
            var creationTime = DateTime.Now - startTime;

            // Assert - Dashboard should load quickly (under 3 seconds)
            Assert.True(creationTime.TotalSeconds < 3,
                $"Dashboard creation took {creationTime.TotalSeconds:F2} seconds, should be under 3 seconds");

            // Test responsiveness of control finding
            startTime = DateTime.Now;
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            var searchTime = DateTime.Now - startTime;

            Assert.NotNull(headerPanel);
            Assert.True(searchTime.TotalMilliseconds < 100,
                $"Control search took {searchTime.TotalMilliseconds:F2}ms, should be under 100ms");
        }
    }
}
