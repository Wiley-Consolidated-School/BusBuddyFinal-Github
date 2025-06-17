using System;
using System.Drawing;
using System.Windows.Forms;
using Xunit;
using BusBuddy.UI.Views;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class DashboardIntegrationTests : UITestBase
    {
        [Fact]
        public void Dashboard_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            _dashboard = CreateDashboardSafely();

            // Assert
            Assert.NotNull(_dashboard);
            Assert.False(string.IsNullOrEmpty(_dashboard.Text));
        }

        [Fact]
        public void Dashboard_ShouldShowAndHideCorrectly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act


            // Assert
            Assert.True(_dashboard.Visible);
            Assert.True(_dashboard.Size.Width > 0);
            Assert.True(_dashboard.Size.Height > 0);

            // Act - Hide
            _dashboard.Hide();

            // Assert
            Assert.False(_dashboard.Visible);
        }

        [Fact]
        public void Dashboard_ShouldHandleResize()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();


            // Act
            var originalSize = _dashboard.Size;
            _dashboard.WindowState = FormWindowState.Normal; // Ensure not maximized
            _dashboard.Size = new Size(1200, 800);

            // Assert
            // Just verify the form accepts the size change (may be adjusted by system)
            Assert.True(_dashboard.Size.Width >= 800); // Minimum width should be respected
            Assert.True(_dashboard.Size.Height >= 600); // Minimum height should be respected
        }

        [Fact]
        public void Dashboard_Controls_ShouldBeAccessible()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();


            // Assert
            Assert.True(_dashboard.Controls.Count > 0, "Dashboard should have controls");

            // Check for main container
            var mainContainer = FindControlByName(_dashboard, "mainContainer");
            Assert.NotNull(mainContainer);
        }
    }
}
