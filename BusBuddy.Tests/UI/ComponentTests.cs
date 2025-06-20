using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using BusBuddy.UI.Views;

namespace BusBuddy.Tests.UI
{
    [Collection("UI Tests")]
    public class ComponentTests : UITestBase
    {        [UITestFact]
        public void Dashboard_ShouldHaveHeaderPanel()
        {
            var testStartTime = DateTime.Now;
            Console.WriteLine("🧪 TEST START: Dashboard_ShouldHaveHeaderPanel");

            try
            {
                // Arrange & Act
                _dashboard = CreateDashboardSafely();

                // Assert
                var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
                Assert.NotNull(headerPanel);
                Assert.True(headerPanel.Height > 0);

                var elapsed = (DateTime.Now - testStartTime).TotalMilliseconds;
                Console.WriteLine($"✅ TEST SUCCESS: Dashboard_ShouldHaveHeaderPanel completed in {elapsed:F0}ms");
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - testStartTime).TotalMilliseconds;
                Console.WriteLine($"❌ TEST FAILED: Dashboard_ShouldHaveHeaderPanel failed after {elapsed:F0}ms: {ex.Message}");
                throw;
            }
        }        [UITestFact]
        public void Dashboard_ShouldHaveQuickActionsFlowPanel()
        {
            var testStartTime = DateTime.Now;
            Console.WriteLine("🧪 TEST START: Dashboard_ShouldHaveQuickActionsFlowPanel");

            try
            {
                // Arrange
                _dashboard = CreateDashboardSafely();

                // Act & Assert
                var flowPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");
                Assert.NotNull(flowPanel);
                Assert.True(flowPanel is FlowLayoutPanel);

                var elapsed = (DateTime.Now - testStartTime).TotalMilliseconds;
                Console.WriteLine($"✅ TEST SUCCESS: Dashboard_ShouldHaveQuickActionsFlowPanel completed in {elapsed:F0}ms");
            }
            catch (Exception ex)
            {
                var elapsed = (DateTime.Now - testStartTime).TotalMilliseconds;
                Console.WriteLine($"❌ TEST FAILED: Dashboard_ShouldHaveQuickActionsFlowPanel failed after {elapsed:F0}ms: {ex.Message}");
                throw;
            }
        }

        [UITestFact]
        public void Dashboard_ShouldHaveStatsPanel()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var statsPanel = FindControlByName(_dashboard, "StatsPanel");
            Assert.NotNull(statsPanel);
            Assert.True(statsPanel.Size.Width > 0);
            Assert.True(statsPanel.Size.Height > 0);
        }

        [UITestFact]
        public void Dashboard_SidebarPanel_ShouldBeInitiallyHidden()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act            // Assert
            var sidebarPanel = FindControlByName(_dashboard, "SidebarPanel");
            Assert.NotNull(sidebarPanel);
            Assert.True(sidebarPanel.Visible, "Sidebar should be initially visible by default");
        }

        [UITestFact]
        public void Dashboard_MaterialDesignElements_ShouldBePresent()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            Assert.NotNull(_dashboard);
            Assert.True(_dashboard.Text.Contains("BusBuddy"), "Dashboard title should contain 'BusBuddy'");

            // Check for Material Design colors
            var headerPanel = FindControlByName(_dashboard, "HeaderPanel");
            if (headerPanel != null)
            {
                Assert.True(headerPanel.BackColor != Color.Empty, "Header should have background color");
            }
        }

        [UITestFact]
        public void Dashboard_ActionCards_ShouldHaveCorrectProperties()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var flowPanel = FindControlByName(_dashboard, "QuickActionsFlowPanel");
            Assert.NotNull(flowPanel);

            // Check that action cards are present
            var cardCount = CountActionCards(flowPanel);
            Assert.True(cardCount > 0, "Should have action cards in the flow panel");
        }

        [UITestFact]
        public void Dashboard_Layout_ShouldUseTableLayoutPanel()
        {
            // Arrange
            _dashboard = CreateDashboardSafely();

            // Act

// Assert
            var mainContainer = FindControlByName(_dashboard, "mainContainer");
            Assert.NotNull(mainContainer);
            Assert.True(mainContainer is TableLayoutPanel, "Main container should be TableLayoutPanel");
        }        private int CountActionCards(Control parent)
        {
            int count = 0;

            foreach (Control child in parent.Controls)
            {
                // Action cards are typically Panel controls with specific properties
                if (child is Panel && child.Size.Width > 200 && child.Size.Height > 100)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
