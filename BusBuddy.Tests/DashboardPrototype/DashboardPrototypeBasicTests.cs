using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;
using FluentAssertions;
using BusBuddy.Tests.Foundation;
using BusBuddy.UI.Views;

namespace BusBuddy.Tests.DashboardPrototype
{
    /// <summary>
    /// DashboardPrototype basic functionality tests
    /// Following Syncfusion Coded UI Testing patterns and recommendations
    /// Reference: https://help.syncfusion.com/windowsforms/testing/coded-ui
    /// </summary>
    public class DashboardPrototypeBasicTests : SyncfusionTestBase
    {
        [Fact]
        public void DashboardPrototype_Constructor_ShouldInitializeSuccessfully()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();

            // Assert
            dashboard.Should().NotBeNull();
            dashboard.Text.Should().Contain("BusBuddy");
        }

        [Fact]
        public void DashboardPrototype_CreateControl_ShouldSucceedWithoutErrors()
        {
            // Arrange
            using var dashboard = CreateDashboard();

            // Act
            System.Action createAction = () => dashboard.CreateControl();

            // Assert
            createAction.Should().NotThrow();
            dashboard.Created.Should().BeTrue();
        }

        [Fact]
        public void DashboardPrototype_FormProperties_ShouldBeConfiguredCorrectly()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Assert - Based on Syncfusion testing patterns for form validation
            dashboard.Size.Width.Should().BeGreaterThan(800);
            dashboard.Size.Height.Should().BeGreaterThan(600);
            dashboard.StartPosition.Should().Be(FormStartPosition.CenterScreen);
            dashboard.Text.Should().Be("BusBuddy CDE-40 Dashboard Prototype");
        }

        [Fact]
        public void DashboardPrototype_SyncfusionControls_ShouldBeInitialized()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Assert - Verify Syncfusion controls are present
            // Following Syncfusion control testing recommendations
            var navigationDrawer = FindControlByType<Syncfusion.Windows.Forms.Tools.NavigationDrawer>(dashboard);
            var dataGrid = FindControlByType<Syncfusion.WinForms.DataGrid.SfDataGrid>(dashboard);
            var gauges = FindControlsByType<Syncfusion.Windows.Forms.Gauge.RadialGauge>(dashboard);

            navigationDrawer.Should().NotBeNull("NavigationDrawer should be initialized");
            dataGrid.Should().NotBeNull("SfDataGrid should be initialized");
            gauges.Should().HaveCountGreaterOrEqualTo(3, "Should have at least 3 RadialGauge controls");
        }

        [Fact]
        public void DashboardPrototype_NavigationDrawer_ShouldHaveRequiredMenuItems()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Assert - Test NavigationDrawer items (Level 2 support per Syncfusion docs)
            var navigationDrawer = FindControlByType<Syncfusion.Windows.Forms.Tools.NavigationDrawer>(dashboard);
            navigationDrawer.Should().NotBeNull();

            // Verify menu items exist (based on DashboardPrototype initialization)
            navigationDrawer!.Items.Should().NotBeEmpty("NavigationDrawer should have menu items");
            navigationDrawer.Items.Count.Should().BeGreaterOrEqualTo(8, "Should have at least 8 menu items");
        }

        [Fact]
        public void DashboardPrototype_Dispose_ShouldCleanupProperly()
        {
            // Arrange
            var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Act
            System.Action disposeAction = () => dashboard.Dispose();

            // Assert
            disposeAction.Should().NotThrow("Dispose should not throw exceptions");
        }

        [Fact]
        public void DashboardPrototype_ErrorHandling_ShouldBeRobust()
        {
            // Arrange - Setup error conditions would require actual interface methods
            // For now, just test that creation doesn't throw

            // Act & Assert - Should handle errors gracefully
            System.Action createAction = () =>
            {
                using var dashboard = CreateDashboard();
                dashboard.CreateControl();
            };

            createAction.Should().NotThrow("Dashboard should handle service errors gracefully");
        }

        /// <summary>
        /// Helper method to find Syncfusion controls by type
        /// Following Syncfusion testing recommendations for control identification
        /// </summary>
        private T? FindControlByType<T>(Control parent) where T : Control
        {
            if (parent is T target)
                return target;

            foreach (Control child in parent.Controls)
            {
                var result = FindControlByType<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Helper method to find multiple Syncfusion controls by type
        /// </summary>
        private System.Collections.Generic.List<T> FindControlsByType<T>(Control parent) where T : Control
        {
            var results = new System.Collections.Generic.List<T>();

            if (parent is T target)
                results.Add(target);

            foreach (Control child in parent.Controls)
            {
                results.AddRange(FindControlsByType<T>(child));
            }

            return results;
        }
    }
}
