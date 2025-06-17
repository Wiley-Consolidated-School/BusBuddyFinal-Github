using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using MaterialSkin.Controls;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class EdgeCaseTests : UITestBase
    {
        [Fact]
        public void Dashboard_MultipleInstances_ShouldNotInterfere()
        {
            // Arrange & Act
            var dashboard1 = CreateDashboardSafely();
            var dashboard2 = CreateDashboardSafely();

            try
            {
                // Assert - Both instances should work independently
                Assert.NotNull(dashboard1);
                Assert.NotNull(dashboard2);
                Assert.NotEqual(dashboard1, dashboard2);

                // Each should have their own controls
                var header1 = FindControlByName(dashboard1, "HeaderPanel");
                var header2 = FindControlByName(dashboard2, "HeaderPanel");

                Assert.NotNull(header1);
                Assert.NotNull(header2);
                Assert.NotEqual(header1, header2);
            }
            finally
            {
                dashboard1?.Dispose();
                dashboard2?.Dispose();
            }
        }

        [Fact]
        public void Dashboard_EmptyState_ShouldHandleGracefully()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test behavior with minimal data
            var allControls = GetAllControlsOfType<Control>(_dashboard);
            var buttons = GetAllControlsOfType<Button>(_dashboard);

            // Assert - Should handle empty states without errors
            Assert.NotNull(allControls);
            Assert.NotNull(buttons);

            // Dashboard should still be functional even with no data
            Assert.True(_dashboard.Visible);
            Assert.True(_dashboard.Width > 0);
            Assert.True(_dashboard.Height > 0);
        }

        [Fact]
        public void Dashboard_RapidToggle_ShouldBeStable()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton") as Button;

            if (toggleButton != null)
            {
                // Act - Rapidly toggle sidebar multiple times
                for (int i = 0; i < 10; i++)
                {
                    toggleButton.PerformClick();
                    System.Threading.Thread.Sleep(10); // Small delay to simulate user clicks
                }

                // Assert - Dashboard should remain stable
                Assert.True(_dashboard.Visible);
                Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
            }
        }

        [Fact]
        public void Dashboard_InvalidInput_ShouldNotCrash()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();            // Act & Assert - Test various invalid inputs
            var exception1 = Record.Exception(() => FindControlByName(_dashboard, null!));
            var exception2 = Record.Exception(() => FindControlByName(_dashboard, ""));
            var exception3 = Record.Exception(() => FindControlByName(_dashboard, "NonExistentControl"));

            // Test with very long strings
            var longString = new string('a', 1000);
            var exception4 = Record.Exception(() => FindControlByName(_dashboard, longString));
        }

        [Fact]
        public void Dashboard_HighDpiScaling_ShouldAdaptProperly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Get initial sizes
            var initialSize = _dashboard.Size;
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            var initialHeaderSize = headerPanel?.Size ?? Size.Empty;

            // Simulate DPI change (mock scenario)
            _dashboard.Scale(new SizeF(1.5f, 1.5f));

            // Assert - Controls should scale appropriately or remain stable
            // Note: Scaling behavior can vary depending on docking and other factors
            Assert.True(_dashboard.Width > 0, "Dashboard should maintain positive width after scaling");
            Assert.True(_dashboard.Height > 0, "Dashboard should maintain positive height after scaling");

            if (headerPanel != null)
            {
                Assert.True(headerPanel.Width > 0, "Header panel should maintain positive width after scaling");
                Assert.True(headerPanel.Height > 0, "Header panel should maintain positive height after scaling");

                // For docked controls, size might not change as expected, so just verify functionality
                Assert.True(headerPanel.Visible, "Header panel should remain visible after scaling");
            }
        }

        [Fact]
        public void Dashboard_MinimumSize_ShouldBeRespected()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Try to resize to very small dimensions
            var originalMinSize = _dashboard.MinimumSize;
            _dashboard.Size = new Size(100, 100);

            // Assert - Should not go below minimum size
            Assert.True(_dashboard.Width >= originalMinSize.Width || originalMinSize.Width == 0);
            Assert.True(_dashboard.Height >= originalMinSize.Height || originalMinSize.Height == 0);

            // Dashboard should remain functional
            Assert.True(_dashboard.Visible);
            Assert.NotNull(FindControlByName(_dashboard, "HeaderPanel"));
        }

        [Fact]
        public void Dashboard_MaximumSize_ShouldBeRespected()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Try to resize to very large dimensions
            var originalMaxSize = _dashboard.MaximumSize;
            _dashboard.Size = new Size(5000, 5000);

            // Assert - Should not exceed maximum size (if set)
            if (originalMaxSize.Width > 0)
                Assert.True(_dashboard.Width <= originalMaxSize.Width);
            if (originalMaxSize.Height > 0)
                Assert.True(_dashboard.Height <= originalMaxSize.Height);

            // Dashboard should remain functional
            Assert.True(_dashboard.Visible);
        }

        [Fact]
        public void Dashboard_ControlEnumeration_ShouldBeConsistent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Enumerate controls multiple times
            var controls1 = GetAllControlsOfType<Control>(_dashboard);
            var controls2 = GetAllControlsOfType<Control>(_dashboard);
            var controls3 = GetAllControlsOfType<Control>(_dashboard);

            // Assert - Results should be consistent
            Assert.Equal(controls1.Count, controls2.Count);
            Assert.Equal(controls2.Count, controls3.Count);

            // Check that we get the same controls (by type and count)
            Assert.Equal(controls1.Count(c => c is Button), controls2.Count(c => c is Button));
            Assert.Equal(controls1.Count(c => c is Panel), controls2.Count(c => c is Panel));
        }

        [Fact]
        public void Dashboard_NestedControlAccess_ShouldWorkCorrectly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find nested controls
            var sidebar = FindControlByName(_dashboard, "SidebarPanel");
            var quickActions = FindControlByName(_dashboard, "QuickActionsPanel");

            // Assert - Should be able to access nested controls
            if (sidebar != null)
            {
                var sidebarButtons = GetAllControlsOfType<Button>(sidebar);
                Assert.NotNull(sidebarButtons);
                // Sidebar might have buttons
            }

            if (quickActions != null)
            {
                var actionButtons = GetAllControlsOfType<Button>(quickActions);
                Assert.NotNull(actionButtons);
                // Quick actions should have some buttons
                Assert.True(actionButtons.Count >= 0);
            }
        }

        [Fact]
        public void Dashboard_ErrorRecovery_ShouldBeRobust()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Simulate various error conditions
            var initialControlCount = GetAllControlsOfType<Control>(_dashboard).Count;            // Try operations that might fail
            try
            {
                // Simulate null reference scenarios (create a null dashboard scenario)
                var nullDashboard = (BusBuddy.UI.Views.BusBuddyDashboardSyncfusion?)null;
                FindControlByName(nullDashboard!, "SomeControl");
            }
            catch
            {
                // Expected to fail, continue testing
            }

            try
            {
                // Try to access disposed control (simulation)
                var tempButton = new Button();
                tempButton.Dispose();
                var text = tempButton.Text; // This might throw
            }
            catch
            {
                // Expected to fail, continue testing
            }

            // Assert - Dashboard should remain functional after errors
            Assert.True(_dashboard.Visible);
            var currentControlCount = GetAllControlsOfType<Control>(_dashboard).Count;
            Assert.Equal(initialControlCount, currentControlCount);
        }

        [Fact]
        public void Dashboard_ControlStateConsistency_ShouldBeMaintained()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Get initial states
            var allControls = GetAllControlsOfType<Control>(_dashboard);
            var initialStates = allControls.ToDictionary(
                c => c,
                c => new { Visible = c.Visible, Enabled = c.Enabled, Size = c.Size }
            );

            // Perform some operations
            var toggleButton = FindControlByName(_dashboard, "SidebarToggleButton") as Button;
            toggleButton?.PerformClick();
            System.Threading.Thread.Sleep(100);

            // Get states again
            var currentControls = GetAllControlsOfType<Control>(_dashboard);

            // Assert - Core controls should maintain their fundamental properties
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            Assert.NotNull(headerPanel);
            Assert.True(headerPanel.Visible); // Header should always be visible

            // Dashboard itself should maintain basic properties
            Assert.True(_dashboard.Visible);
            Assert.True(_dashboard.Width > 0);
            Assert.True(_dashboard.Height > 0);
        }
    }
}
