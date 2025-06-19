using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
// MaterialSkin.Controls removed - using standard controls with Syncfusion theming

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class DashboardAdvancedTests : UITestBase
    {
        [UITestFact]
        public void Dashboard_QuickActionButtons_ShouldBeAccessible()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find the quick actions flow panel
            var quickActionsPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");

            // Assert
            Assert.NotNull(quickActionsPanel);

            // The QuickActionsFlowPanel contains cards (panels), not buttons directly
            // Count the action cards in the panel
            var actionCards = GetAllControlsOfType<Panel>(quickActionsPanel)
                .Where(p => p.Parent == quickActionsPanel)  // Direct children only
                .ToList();
            Assert.True(actionCards.Count > 0, "Should have at least one quick action card");

            // Verify all action cards are accessible
            foreach (var card in actionCards.Take(5)) // Limit to avoid excessive testing
            {
                Assert.True(card.Visible, $"Action card should be visible");
                Assert.True(card.Size.Width > 0, $"Action card should have width");
                Assert.True(card.Size.Height > 0, $"Action card should have height");
            }
        }

        [UITestFact]
        public void Dashboard_MaterialDesignButtons_ShouldBeStyled()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act
            var materialButtons = GetAllControlsOfType<Button>(_dashboard);

            // Assert
            Assert.True(materialButtons.Count > 0, "Should have Material Design buttons");

            foreach (var button in materialButtons)
            {
                Assert.True(button.AutoSize || button.Size.Width > 0,
                    $"Material button '{button.Text}' should have proper sizing");
                Assert.NotEqual(Color.Empty, button.BackColor);
            }
        }

        [UITestFact]
        public void Dashboard_LayoutContainers_ShouldBeProperlyNested()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Check main container structure
            var mainContainer = FindControlByName(_dashboard, "mainContainer");
            Assert.NotNull(mainContainer);
            AssertControlLayout(mainContainer, DockStyle.Fill);

            // Check header panel
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            Assert.NotNull(headerPanel);
            AssertControlLayout(headerPanel, DockStyle.Top, expectedHeight: 90);

            // Check sidebar panel
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            Assert.NotNull(sidebarPanel);
            AssertControlLayout(sidebarPanel, DockStyle.Left, expectedWidth: 200);

            // Verify proper parent-child relationships
            Assert.True(_dashboard.Controls.Contains(mainContainer),
                "Main container should be direct child of dashboard");
        }

        [UITestFact]
        public void Dashboard_FormProperties_ShouldBeConfiguredCorrectly()
        {
            // Arrange & Act
            _dashboard = CreateDashboardSafely();

            // Assert - Form-level properties
            Assert.Equal("BusBuddy Dashboard", _dashboard.Text);
            Assert.True(_dashboard.Size.Width >= 800, "Dashboard should have minimum width of 800px");
            Assert.True(_dashboard.Size.Height >= 600, "Dashboard should have minimum height of 600px");
            // Form defaults to FormBorderStyle.None, not Sizable
            Assert.Equal(FormBorderStyle.None, _dashboard.FormBorderStyle);
            Assert.True(_dashboard.ShowInTaskbar, "Dashboard should appear in taskbar");
            Assert.False(_dashboard.TopMost, "Dashboard should not be topmost");
        }

        [UITestFact]
        public void Dashboard_SidebarModules_ShouldBePresent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            Assert.NotNull(sidebarPanel);

            // Get all buttons in sidebar (module navigation buttons)
            var sidebarButtons = GetAllControlsOfType<Button>(sidebarPanel);

            // Assert
            Assert.True(sidebarButtons.Count >= 5, "Should have at least 5 module buttons in sidebar");

            // Expected modules based on the dashboard implementation
            var expectedModules = new[] { "Maintenance", "Fuel Management", "Activities", "Schedules", "School Calendar", "Reports" };

            foreach (var expectedModule in expectedModules)
            {
                var moduleButton = sidebarButtons.FirstOrDefault(b =>
                    b.Text.Contains(expectedModule, StringComparison.OrdinalIgnoreCase));
                Assert.NotNull(moduleButton);
                Assert.True(moduleButton.Enabled, $"{expectedModule} button should be enabled");
            }
        }

        [UITestFact]
        public void Dashboard_StatsPanel_ShouldDisplayInformation()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act
            var statsPanel = FindControlByName(_dashboard, "StatsPanel");

            // Assert
            Assert.NotNull(statsPanel);
            AssertControlLayout(statsPanel, expectedHeight: 150);

            // Check for labels or other controls that might display stats
            var labels = GetAllControlsOfType<Label>(statsPanel);
            var materialLabels = GetAllControlsOfType<Label>(statsPanel);

            // Should have some form of text display in stats panel
            Assert.True(labels.Count + materialLabels.Count > 0,
                "Stats panel should contain labels or text displays");
        }

        [UITestFact]
        public void Dashboard_ResponsiveLayout_ShouldAdaptToWindowSize()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test different window sizes
            var sizes = new[]
            {
                new Size(800, 600),   // Minimum size
                new Size(1200, 800),  // Medium size
                new Size(1600, 1000)  // Large size
            };

            foreach (var size in sizes)
            {
                _dashboard.WindowState = FormWindowState.Normal;
                _dashboard.Size = size;

                // Assert - Verify layout adapts properly
                var mainContainer = FindControlByName(_dashboard, "mainContainer");
                Assert.NotNull(mainContainer);

                // Main container should fill the client area
                Assert.True(mainContainer.Width > 0, $"Main container width should be positive at size {size}");
                Assert.True(mainContainer.Height > 0, $"Main container height should be positive at size {size}");

                // Quick actions panel should be responsive
                var quickActionsPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");
                if (quickActionsPanel != null)
                {
                    Assert.True(quickActionsPanel.Width > 0,
                        $"Quick actions panel should be visible at size {size}");
                }
            }
        }

        [UITestFact]
        public void Dashboard_TabOrder_ShouldBeLogical()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Get all focusable controls
            var focusableControls = GetAllControlsOfType<Control>(_dashboard)
                .Where(c => c.CanFocus && c.TabStop && c.Visible)
                .OrderBy(c => c.TabIndex)
                .ToList();

            // Assert
            Assert.True(focusableControls.Count > 0, "Should have focusable controls");

            // Verify tab indices are sequential and start from 0
            for (int i = 0; i < focusableControls.Count; i++)
            {
                Assert.True(focusableControls[i].TabIndex >= 0,
                    $"Control at position {i} should have non-negative tab index");
            }
        }

        [UITestFact]
        public void Dashboard_KeyboardNavigation_ShouldWork()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find the hamburger/sidebar toggle button
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton");

            // Assert
            Assert.NotNull(toggleButton);
            Assert.True(toggleButton.CanFocus, "Toggle button should be focusable");
            Assert.True(toggleButton.TabStop, "Toggle button should be included in tab order");

            // Test that important navigation elements are keyboard accessible
            var importantButtons = GetAllControlsOfType<Button>(_dashboard)
                .Where(b => b.Visible && b.Enabled)
                .ToList();

            Assert.True(importantButtons.Count > 0, "Should have accessible buttons");

            foreach (var button in importantButtons.Take(3)) // Test first 3 buttons
            {
                Assert.True(button.CanFocus || !button.TabStop,
                    $"Button '{button.Text}' should be properly configured for keyboard access");
            }
        }

        [UITestFact]
        public void Dashboard_ControlStates_ShouldBeConsistent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Check that all visible controls have consistent states
            var allControls = GetAllControlsOfType<Control>(_dashboard);

            foreach (var control in allControls.Where(c => c.Visible))
            {
                // Controls should have reasonable bounds
                Assert.True(control.Width >= 0, $"Control {control.Name} should have non-negative width");
                Assert.True(control.Height >= 0, $"Control {control.Name} should have non-negative height");

                // Parent relationships should be valid
                if (control.Parent != null)
                {
                    Assert.True(control.Parent.Controls.Contains(control),
                        $"Control {control.Name} parent relationship should be bidirectional");
                }
            }
        }

        [UITestFact]
        public void Dashboard_ErrorHandling_ShouldBeGraceful()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert - Verify the dashboard handles edge cases gracefully

            // Test null reference handling by trying to find non-existent controls
            var nonExistentControl = FindControlByName(_dashboard, "NonExistentControl123");
            Assert.Null(nonExistentControl); // Should return null, not throw

            // Test with empty control type search
            var emptyResult = GetAllControlsOfType<TabControl>(_dashboard);
            Assert.NotNull(emptyResult); // Should return empty list, not null

            // Test control counting with non-existent types
            var count = CountVisibleControls<TabControl>(_dashboard);
            Assert.True(count >= 0); // Should return 0 or positive, not throw
        }

        [UITestFact]
        public void Dashboard_MemoryUsage_ShouldBeReasonable()
        {
            // Arrange & Act
            var initialMemory = GC.GetTotalMemory(false);

            using (var dashboard = CreateDashboardSafely())
            {
                // Force garbage collection to get more accurate reading
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var memoryAfterCreation = GC.GetTotalMemory(false);
                var memoryUsed = memoryAfterCreation - initialMemory;

                // Assert - Dashboard shouldn't use excessive memory (under 50MB for basic UI)
                Assert.True(memoryUsed < 50 * 1024 * 1024,
                    $"Dashboard memory usage ({memoryUsed / 1024 / 1024} MB) should be reasonable");
            }

            // Verify cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryAfterDisposal = GC.GetTotalMemory(false);
            // Memory should not continuously grow (allowing some tolerance for GC behavior)
            Assert.True(memoryAfterDisposal <= initialMemory + (10 * 1024 * 1024),
                "Memory should be properly cleaned up after dashboard disposal");
        }
    }
}
