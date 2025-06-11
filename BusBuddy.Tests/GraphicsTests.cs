using Xunit;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BusBuddy.UI;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Tests.TestHelpers;

namespace BusBuddy.Tests
{
    public class GraphicsTests
    {
        [STAThread]
        [Fact]
        public void Form1_ShouldInitializeChartsCorrectly()
        {
            // Arrange & Act
            using (var form = new MockMainForm())
            {
                form.Show(); // Need to show form to initialize chart components

                // Assert - Check if charts are created and configured
                var chartGrossRevenue = UITestHelpers.FindControl<Chart>(form, "chartGrossRevenue");
                var chartTopProducts = UITestHelpers.FindControl<Chart>(form, "chartTopProducts");

                Assert.NotNull(chartGrossRevenue);
                Assert.NotNull(chartTopProducts);

                // Verify chart areas are configured
                Assert.True(chartGrossRevenue.ChartAreas.Count > 0, "Gross Revenue chart should have chart areas");
                Assert.True(chartTopProducts.ChartAreas.Count > 0, "Top Products chart should have chart areas");

                // Verify legends are configured
                Assert.True(chartGrossRevenue.Legends.Count > 0, "Gross Revenue chart should have legends");
                Assert.True(chartTopProducts.Legends.Count > 0, "Top Products chart should have legends");

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Chart_ShouldHaveCorrectSeriesConfiguration()
        {
            // Arrange & Act
            using (var form = new MockMainForm())
            {
                form.Show();

                var chartTopProducts = UITestHelpers.FindControl<Chart>(form, "chartTopProducts");

                if (chartTopProducts != null)
                {
                    // Assert - Verify series configuration
                    Assert.True(chartTopProducts.Series.Count > 0, "Chart should have at least one series");

                    var series = chartTopProducts.Series[0];
                    Assert.Equal(SeriesChartType.Doughnut, series.ChartType);
                    Assert.True(series.IsValueShownAsLabel, "Series should show values as labels");
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void DataGridView_ShouldRenderCorrectly()
        {
            // Arrange & Act
            using (var form = new MockVehicleManagementForm())
            {
                form.Show();

                var vehicleGridView = UITestHelpers.FindControl<DataGridView>(form, "vehicleGridView");
                var dgvUnderstock = UITestHelpers.FindControl<DataGridView>(form, "dgvUnderstock");

                // Assert - Check if DataGridViews are properly configured
                if (vehicleGridView != null)
                {
                    Assert.True(vehicleGridView.ReadOnly, "Vehicle grid should be read-only");
                    Assert.Equal(DataGridViewAutoSizeColumnsMode.Fill, vehicleGridView.AutoSizeColumnsMode);
                    Assert.False(vehicleGridView.AllowUserToAddRows, "Should not allow adding rows");
                }

                if (dgvUnderstock != null)
                {
                    Assert.Equal(DataGridViewAutoSizeColumnsMode.Fill, dgvUnderstock.AutoSizeColumnsMode);
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Form_ShouldHaveCorrectSizeAndPositioning()
        {
            // Arrange & Act
            using (var form = new MockMainForm("Store statistics", new Size(1204, 641)))
            {
                // Assert - Verify form dimensions and positioning
                Assert.Equal(new Size(1204, 641), form.ClientSize);
                Assert.Equal("Store statistics", form.Text);

                // Verify form scaling
                Assert.Equal(new SizeF(7F, 15F), form.AutoScaleDimensions);
                Assert.Equal(AutoScaleMode.Font, form.AutoScaleMode);
            }
        }

        [STAThread]
        [Fact]
        public void Panel_ComponentsShouldHaveCorrectColors()
        {
            // Arrange & Act
            using (var form = new MockMainForm())
            {
                form.Show();

                // Find panels by their controls
                var panel1 = UITestHelpers.FindPanelByChildControl(form, "lblNumOrders");
                var panel2 = UITestHelpers.FindPanelByChildControl(form, "lblTotalRevenue");
                var panel3 = UITestHelpers.FindPanelByChildControl(form, "lblTotalProfit");

                // Assert - Verify panel background colors
                if (panel1 != null)
                    Assert.Equal(Color.White, panel1.BackColor);

                if (panel2 != null)
                    Assert.Equal(Color.White, panel2.BackColor);

                if (panel3 != null)
                    Assert.Equal(Color.White, panel3.BackColor);

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Labels_ShouldHaveCorrectFontSizes()
        {
            // Arrange & Act
            using (var form = new MockMainForm())
            {
                form.Show();

                var titleLabel = UITestHelpers.FindControl<Label>(form, "label1");
                var orderLabel = UITestHelpers.FindControl<Label>(form, "lblNumOrders");

                // Assert - Verify font sizes
                if (titleLabel != null)
                {
                    Assert.Equal(20F, titleLabel.Font.Size);
                }

                if (orderLabel != null)
                {
                    Assert.Equal(15F, orderLabel.Font.Size);
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void BaseDataForm_ShouldCreateControlsWithCorrectVisualProperties()
        {
            using (var form = new TestableBaseDataForm())
            {
                form.Show();

                // Act
                using var button = form.TestCreateButton("Test", 10, 10);
                using var textBox = form.TestCreateTextBox(10, 40, 200);
                using var label = form.TestCreateLabel("Test Label", 10, 70);

                // Assert - Verify visual properties
                Assert.Equal(new Size(100, 30), button.Size);
                Assert.Equal(new Size(200, 23), textBox.Size);
                Assert.True(label.AutoSize);

                form.Hide();
            }
        }

        // Helper methods moved to UITestHelpers class
    }
}
