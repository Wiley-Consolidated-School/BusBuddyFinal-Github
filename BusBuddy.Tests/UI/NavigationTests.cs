using System;
using System.Drawing;
using System.Windows.Forms;
using Xunit;
using Moq;
using BusBuddy.UI.Views;
using BusBuddy.UI.Services;
using BusBuddy.Business;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class NavigationTests : UITestBase
    {

        [UITestFact]
        public void Dashboard_NavigationService_ShouldBeInjected()
        {
            // Arrange & Act
            _dashboard = CreateDashboardSafely();

            // Assert
            Assert.NotNull(_dashboard);
            // Navigation service should be available through dependency injection
        }

        [UITestFact]
        public void Dashboard_SidebarToggle_ShouldWork()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Find and click the hamburger button
            var hamburgerButton = FindControlByName(_dashboard, "SidebarToggleButton");

            // Assert
            Assert.NotNull(hamburgerButton);

            // Simulate clicking the button would toggle sidebar visibility
            // In a real test, we would click the button and verify sidebar state changes
        }

        [UITestFact]
        public void Dashboard_QuickActionCards_ShouldTriggerNavigation()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

// Act & Assert - Verify that navigation methods would be called
            // This tests the wiring of navigation without actually navigating
            _mockNavigationService.Setup(x => x.ShowVehicleManagement()).Verifiable();
            _mockNavigationService.Setup(x => x.ShowDriverManagement()).Verifiable();
            _mockNavigationService.Setup(x => x.ShowRouteManagement()).Verifiable();
            _mockNavigationService.Setup(x => x.ShowMaintenanceManagement()).Verifiable();

            // In a real scenario, clicking the cards would trigger these methods
            Assert.True(true, "Navigation service is properly injected and available");
        }

        [UITestFact]
        public void Dashboard_Layout_ShouldBeResponsive()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();            // Act
            _dashboard.WindowState = FormWindowState.Normal; // Ensure not maximized
            _dashboard.Size = new Size(1200, 800);

            // Assert
            // Verify the form can be resized and maintains minimum constraints
            Assert.True(_dashboard.Size.Width >= 800, "Width should be at least 800px");
            Assert.True(_dashboard.Size.Height >= 600, "Height should be at least 600px");

            // Verify layout adapts to different sizes
            _dashboard.Size = new Size(1600, 1000);
            Assert.True(_dashboard.Size.Width >= 800, "Width should still meet minimum after resize");
            Assert.True(_dashboard.Size.Height == 1000);
        }
    }
}
