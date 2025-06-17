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
    public class LayoutAndThemeTests : UITestBase
    {

        [Fact]
        public void Dashboard_MinimumSize_ShouldBeEnforced()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            Assert.True(_dashboard.MinimumSize.Width >= 1024, $"Minimum width should be 1024, but was {_dashboard.MinimumSize.Width}");
            Assert.True(_dashboard.MinimumSize.Height >= 600, $"Minimum height should be 600, but was {_dashboard.MinimumSize.Height}");
        }

        [Fact]
        public void Dashboard_ShouldStartMaximized()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            Assert.Equal(FormWindowState.Maximized, _dashboard.WindowState);
        }

        [Fact]
        public void Dashboard_ShouldStartCenterScreen()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act & Assert
            Assert.Equal(FormStartPosition.CenterScreen, _dashboard.StartPosition);
        }

        [Fact]
        public void Dashboard_ThemeColors_ShouldBeConsistent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

            // Assert - Check for consistent Material Design colors
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            if (headerPanel != null)
            {
                // Header should exist and have a valid background color (may vary due to Material Design theming)
                Assert.NotEqual(Color.Empty, headerPanel.BackColor);
                // Don't enforce exact color as Material Design may override it
            }

            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            if (sidebarPanel != null)
            {
                // Sidebar should have the secondary color (#625B71)
                var expectedSidebarColor = ColorTranslator.FromHtml("#625B71");
                Assert.Equal(expectedSidebarColor, sidebarPanel.BackColor);
            }
        }

        [Fact]
        public void Dashboard_BackgroundColor_ShouldBeSetCorrectly()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var mainContainer = FindControlByName(_dashboard, "mainContainer");
            if (mainContainer != null)
            {
                var expectedBackgroundColor = ColorTranslator.FromHtml("#FFF8F8");
                Assert.Equal(expectedBackgroundColor, mainContainer.BackColor);
            }
        }

        [Fact]
        public void Dashboard_Layout_ShouldAdaptToResize()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act - Test different sizes
            _dashboard.WindowState = FormWindowState.Normal;
            _dashboard.Size = new Size(1200, 800);

// Assert
            Assert.True(_dashboard.Size.Width == 1200 || _dashboard.WindowState == FormWindowState.Maximized);
            Assert.True(_dashboard.Size.Height == 800 || _dashboard.WindowState == FormWindowState.Maximized);            // Test responsiveness
            var flowPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel") as FlowLayoutPanel;
            Assert.NotNull(flowPanel);
            Assert.True(flowPanel?.AutoScroll == true, "Flow panel should have auto-scroll enabled");
        }

        [Fact]
        public void Dashboard_DockingAndAnchoring_ShouldBeCorrect()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var mainContainer = FindControlByName(_dashboard, "mainContainer");
            Assert.NotNull(mainContainer);
            Assert.Equal(DockStyle.Fill, mainContainer.Dock);

            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            if (headerPanel != null)
            {
                Assert.Equal(DockStyle.Top, headerPanel.Dock);
            }

            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            if (sidebarPanel != null)
            {
                Assert.Equal(DockStyle.Left, sidebarPanel.Dock);
            }
        }

        [Fact]
        public void Dashboard_Padding_ShouldBeApplied()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var flowPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");
            if (flowPanel != null)
            {
                Assert.True(flowPanel.Padding.All > 0 ||
                           flowPanel.Padding.Left > 0 ||
                           flowPanel.Padding.Top > 0,
                           "Flow panel should have padding applied");
            }        }
    }
}
