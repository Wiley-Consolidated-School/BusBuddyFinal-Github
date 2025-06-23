using System;
using System.Reflection;
using System.Windows.Forms;
using Xunit;
using FluentAssertions;
using Moq;
using BusBuddy.Tests.Foundation;
using BusBuddy.UI.Views;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.Tests.DashboardPrototype
{
    /// <summary>
    /// Tests for Syncfusion NavigationDrawer functionality in DashboardPrototype
    /// Based on official Syncfusion testing documentation for NavigationDrawer
    /// Reference: https://help.syncfusion.com/windowsforms/testing/coded-ui
    /// Syncfusion Support Level: 2 Levels (per official documentation)
    /// </summary>
    public class DashboardNavigationTests : SyncfusionTestBase
    {
        [Fact]
        public void NavigationDrawer_Initialization_ShouldConfigureCorrectly()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Assert - Test NavigationDrawer properties (Level 2 Syncfusion support)
            var navigationDrawer = FindNavigationDrawer(dashboard);
            navigationDrawer.Should().NotBeNull("NavigationDrawer should be initialized");
            navigationDrawer!.Position.Should().Be(SlidePosition.Left, "NavigationDrawer should be positioned on the left");
            navigationDrawer.DrawerWidth.Should().Be(250, "NavigationDrawer width should be 250px");
        }

        [Fact]
        public void NavigationDrawer_MenuItems_ShouldBeConfiguredCorrectly()
        {
            // Arrange & Act
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();

            // Assert - Verify menu structure
            var navigationDrawer = FindNavigationDrawer(dashboard);
            navigationDrawer!.Items.Should().NotBeEmpty("NavigationDrawer should have menu items");

            // Verify header exists
            var header = navigationDrawer.Items[0] as DrawerHeader;
            header.Should().NotBeNull("First item should be DrawerHeader");
            header!.Text.Should().Be("Navigation Menu");

            // Verify menu items exist (following Syncfusion testing patterns)
            var menuItems = GetDrawerMenuItems(navigationDrawer);
            menuItems.Should().HaveCountGreaterOrEqualTo(8, "Should have at least 8 menu items");

            // Verify specific menu items
            var menuTexts = menuItems.ConvertAll(item => item.Text);
            menuTexts.Should().Contain("Dashboard");
            menuTexts.Should().Contain("Routes");
            menuTexts.Should().Contain("Vehicles");
            menuTexts.Should().Contain("Drivers");
            menuTexts.Should().Contain("Maintenance");
            menuTexts.Should().Contain("Activity Schedule");
            menuTexts.Should().Contain("CDE-40 Report");
            menuTexts.Should().Contain("Settings");
        }

        [Fact]
        public void NavigationDrawer_MenuItemClick_ShouldTriggerNavigation()
        {
            // Arrange
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();
            var navigationDrawer = FindNavigationDrawer(dashboard);
            var vehiclesMenuItem = GetMenuItemByText(navigationDrawer!, "Vehicles");

            // Act - Simulate menu item click by directly testing navigation
            vehiclesMenuItem.Should().NotBeNull("Vehicles menu item should exist");

            // Simulate the navigation that would occur on click
            System.Action navigationAction = () => {
                // Since we're testing the navigation behavior, simulate the call
                dashboard.GetType().GetMethod("NavigateToModule", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .Invoke(dashboard, new object[] { "Vehicles" });
            };

            // Assert - Should not throw exceptions
            navigationAction.Should().NotThrow("Menu item click should not cause errors");

            // Verify navigation service was called
            MockNavigationService.Verify(x => x.Navigate("vehicles", It.IsAny<object[]>()),
                Times.Once, "Navigation service should be called with 'vehicles' parameter");
        }

        [Fact]
        public void NavigationDrawer_CDE40MenuItem_ShouldNavigateToReport()
        {
            // Arrange
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();
            var navigationDrawer = FindNavigationDrawer(dashboard);
            var cde40MenuItem = GetMenuItemByText(navigationDrawer!, "CDE-40 Report");

            // Act
            cde40MenuItem.Should().NotBeNull("CDE-40 Report menu item should exist");
            System.Action clickAction = () => {
                // Simulate navigation by calling the navigation method directly
                dashboard.GetType().GetMethod("NavigateToModule", BindingFlags.NonPublic | BindingFlags.Instance)?
                    .Invoke(dashboard, new object[] { "CDE40" });
            };

            // Assert
            clickAction.Should().NotThrow("CDE-40 Report click should not cause errors");
            MockNavigationService.Verify(x => x.Navigate("cde40", It.IsAny<object[]>()),
                Times.Once, "Should navigate to CDE-40 report");
        }

        [Fact]
        public void NavigationDrawer_AllMenuItems_ShouldHaveClickHandlers()
        {
            // Arrange
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();
            var navigationDrawer = FindNavigationDrawer(dashboard);
            var menuItems = GetDrawerMenuItems(navigationDrawer!);

            // Act & Assert - Test each menu item can be clicked safely
            foreach (var menuItem in menuItems)
            {
                System.Action clickAction = () => {
                    // Simulate navigation by calling the navigation method directly
                    dashboard.GetType().GetMethod("NavigateToModule", BindingFlags.NonPublic | BindingFlags.Instance)?
                        .Invoke(dashboard, new object[] { menuItem.Text ?? "Default" });
                };
                clickAction.Should().NotThrow($"Menu item '{menuItem.Text}' should handle clicks safely");
            }

            // Verify navigation service was called for each menu item
            MockNavigationService.Verify(x => x.Navigate(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Exactly(menuItems.Count), "Each menu item should trigger navigation");
        }

        [Fact]
        public void NavigationDrawer_Visibility_ShouldBeControlled()
        {
            // Arrange
            using var dashboard = CreateDashboard();
            dashboard.CreateControl();
            var navigationDrawer = FindNavigationDrawer(dashboard);

            // Act & Assert - Test visibility control
            navigationDrawer!.Visible.Should().BeFalse("NavigationDrawer should start hidden");            // Test show/hide functionality
            System.Action showAction = () => navigationDrawer.Visible = true;
            showAction.Should().NotThrow("Should be able to show NavigationDrawer");

            System.Action hideAction = () => navigationDrawer.Visible = false;
            hideAction.Should().NotThrow("Should be able to hide NavigationDrawer");
        }

        /// <summary>
        /// Helper method to find NavigationDrawer control
        /// Following Syncfusion testing recommendations for control identification
        /// </summary>
        private NavigationDrawer? FindNavigationDrawer(Control parent)
        {
            if (parent is NavigationDrawer drawer)
                return drawer;

            foreach (Control child in parent.Controls)
            {
                var result = FindNavigationDrawer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Helper method to get DrawerMenuItem objects from NavigationDrawer
        /// Based on Syncfusion NavigationDrawer structure
        /// </summary>
        private System.Collections.Generic.List<DrawerMenuItem> GetDrawerMenuItems(NavigationDrawer drawer)
        {
            var menuItems = new System.Collections.Generic.List<DrawerMenuItem>();

            foreach (var item in drawer.Items)
            {
                if (item is DrawerMenuItem menuItem)
                {
                    menuItems.Add(menuItem);
                }
            }

            return menuItems;
        }

        /// <summary>
        /// Helper method to find a specific menu item by text
        /// </summary>
        private DrawerMenuItem? GetMenuItemByText(NavigationDrawer drawer, string text)
        {
            var menuItems = GetDrawerMenuItems(drawer);
            return menuItems.Find(item => item.Text == text);
        }
    }
}
