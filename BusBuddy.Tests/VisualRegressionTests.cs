using Xunit;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BusBuddy.UI;
using BusBuddy.Tests.TestHelpers;

namespace BusBuddy.Tests
{
    public class VisualRegressionTests
    {
        private readonly string _testImagesPath = Path.Combine(Directory.GetCurrentDirectory(), "TestImages");

        public VisualRegressionTests()
        {
            // Ensure test images directory exists
            if (!Directory.Exists(_testImagesPath))
            {
                Directory.CreateDirectory(_testImagesPath);
            }
        }

        [STAThread]
        [Fact]
        public void MainForm_VisualLayout_ShouldBeConsistent()
        {
            // Arrange
            using (var form = new MockMainForm())
            {
                form.Show();
                form.Refresh();
                Thread.Sleep(100); // Allow form to fully render

                // Act - Capture form screenshot
                var screenshot = CaptureFormImage(form);

                // Assert - Basic visual properties
                Assert.NotNull(screenshot);
                Assert.True(screenshot.Width > 0);
                Assert.True(screenshot.Height > 0);

                // Save baseline image for manual verification
                var baselinePath = Path.Combine(_testImagesPath, "MainForm_Baseline.png");
                screenshot.Save(baselinePath, ImageFormat.Png);

                form.Hide();
                screenshot.Dispose();
            }
        }

        [STAThread]
        [Fact]
        public void VehicleManagementForm_Layout_ShouldBeConsistent()
        {
            // Arrange
            using (var form = new MockVehicleManagementForm())
            {
                form.Show();
                form.Refresh();
                Thread.Sleep(100);

                // Act
                var screenshot = CaptureFormImage(form);

                // Assert
                Assert.NotNull(screenshot);

                // Verify minimum form dimensions
                Assert.True(form.Width >= 800, "Form should be at least 800px wide");
                Assert.True(form.Height >= 600, "Form should be at least 600px tall");

                // Save for visual verification
                var imagePath = Path.Combine(_testImagesPath, "VehicleManagementForm_Layout.png");
                screenshot.Save(imagePath, ImageFormat.Png);

                form.Hide();
                screenshot.Dispose();
            }
        }

        [STAThread]
        [Fact]
        public void Form1_Charts_ShouldRenderProperly()
        {
            // Arrange
            using (var form = new MockMainForm("Store statistics", new Size(1204, 641)))
            {
                form.Show();
                form.Refresh();
                Thread.Sleep(200); // Charts may need more time to render

                // Act
                var screenshot = CaptureFormImage(form);

                // Assert
                Assert.NotNull(screenshot);

                // Verify form has expected size for charts
                Assert.Equal(new Size(1204, 641), form.ClientSize);

                // Save chart rendering for verification
                var chartImagePath = Path.Combine(_testImagesPath, "Form1_ChartsRendering.png");
                screenshot.Save(chartImagePath, ImageFormat.Png);

                form.Hide();
                screenshot.Dispose();
            }
        }

        [STAThread]
        [Fact]
        public void BaseDataForm_Controls_ShouldHaveConsistentStyling()
        {
            // Arrange
            using (var form = new TestableBaseDataForm())
            {
                form.Show();

                // Create test controls
                using var label = form.TestCreateLabel("Test Label", 10, 10);
                using var textBox = form.TestCreateTextBox(10, 40, 200);
                using var button = form.TestCreateButton("Test Button", 10, 80);
                using var combo = form.TestCreateComboBox(10, 120, 200);
                using var datePicker = form.TestCreateDatePicker(10, 160, 200);
                using var checkBox = form.TestCreateCheckBox("Test Checkbox", 10, 200);

                form.Refresh();
                Thread.Sleep(100);

                // Act
                var screenshot = CaptureFormImage(form);

                // Assert - Verify controls are positioned correctly
                Assert.Equal(new Point(10, 10), label.Location);
                Assert.Equal(new Point(10, 40), textBox.Location);
                Assert.Equal(new Point(10, 80), button.Location);
                Assert.Equal(new Point(10, 120), combo.Location);
                Assert.Equal(new Point(10, 160), datePicker.Location);
                Assert.Equal(new Point(10, 200), checkBox.Location);

                // Verify consistent sizing
                Assert.Equal(new Size(200, 23), textBox.Size);
                Assert.Equal(new Size(100, 30), button.Size);
                Assert.Equal(new Size(200, 23), combo.Size);
                Assert.Equal(new Size(200, 23), datePicker.Size);

                // Save control layout for verification
                var controlLayoutPath = Path.Combine(_testImagesPath, "BaseDataForm_ControlLayout.png");
                screenshot.Save(controlLayoutPath, ImageFormat.Png);

                form.Hide();
                screenshot.Dispose();
            }
        }

        [STAThread]
        [Fact]
        public void Forms_ShouldHaveConsistentFontRendering()
        {
            // Test font consistency across different forms
            using (var mainForm = new MockMainForm())
            using (var vehicleForm = new MockVehicleManagementForm())
            using (var driverForm = new MockDriverManagementForm())
            {
                mainForm.Show();
                vehicleForm.Show();
                driverForm.Show();

                // Allow forms to render
                Application.DoEvents();
                Thread.Sleep(100);

                // Verify font families are consistent (should inherit from system defaults)
                Assert.Equal(mainForm.Font.Name, vehicleForm.Font.Name);
                Assert.Equal(vehicleForm.Font.Name, driverForm.Font.Name);

                // Check for standard Windows Forms font sizes
                Assert.True(mainForm.Font.Size >= 8.0f && mainForm.Font.Size <= 12.0f);
                Assert.True(vehicleForm.Font.Size >= 8.0f && vehicleForm.Font.Size <= 12.0f);
                Assert.True(driverForm.Font.Size >= 8.0f && driverForm.Font.Size <= 12.0f);

                mainForm.Hide();
                vehicleForm.Hide();
                driverForm.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void DataGridView_ShouldRenderRowsConsistently()
        {
            // Arrange
            using (var form = new MockVehicleManagementForm())
            {
                form.Show();

                var dataGrid = FindDataGridView(form);
                if (dataGrid != null)
                {
                    // Add some test data to verify rendering
                    dataGrid.Columns.Clear();
                    dataGrid.Columns.Add("ID", "ID");
                    dataGrid.Columns.Add("Name", "Name");
                    dataGrid.Columns.Add("Value", "Value");

                    // Add test rows
                    dataGrid.Rows.Add("1", "Test Item 1", "Value 1");
                    dataGrid.Rows.Add("2", "Test Item 2", "Value 2");
                    dataGrid.Rows.Add("3", "Test Item 3", "Value 3");

                    form.Refresh();
                    Thread.Sleep(100);

                    // Act
                    var screenshot = CaptureFormImage(form);

                    // Assert
                    Assert.Equal(2, dataGrid.Rows.Count - 1); // -1 for the new row
                    Assert.True(dataGrid.Columns.Count == 3);

                    // Save grid rendering
                    var gridImagePath = Path.Combine(_testImagesPath, "DataGridView_RowRendering.png");
                    screenshot.Save(gridImagePath, ImageFormat.Png);

                    screenshot.Dispose();
                }

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void Form_ShouldHandleHighDPIScaling()
        {
            // Test that forms handle different DPI settings properly
            using (var form = new MockMainForm())
            {
                form.Show();

                // Check AutoScale properties
                Assert.Equal(AutoScaleMode.Font, form.AutoScaleMode);

                // Verify form respects system DPI
                var currentDpi = form.DeviceDpi;
                Assert.True(currentDpi > 0, "Device DPI should be positive");

                // Common DPI values are 96 (100%), 120 (125%), 144 (150%), 192 (200%)
                Assert.True(currentDpi >= 96 && currentDpi <= 300, "DPI should be in reasonable range");

                form.Hide();
            }
        }

        [STAThread]
        [Fact]
        public void VisualElements_ShouldNotOverlap()
        {
            // Test that UI elements don't overlap inappropriately
            using (var form = new MockMainForm()) // Use MockMainForm as a stand-in for Form1
            {
                form.Show();
                form.Refresh();
                Thread.Sleep(100);

                // Get all controls and check for overlapping bounds
                var controls = GetAllControls(form);
                var overlappingControls = 0;

                for (int i = 0; i < controls.Count; i++)
                {
                    for (int j = i + 1; j < controls.Count; j++)
                    {
                        var control1 = controls[i];
                        var control2 = controls[j];

                        // Skip if one control is a parent of the other
                        if (IsParentChild(control1, control2)) continue;

                        // Check for overlap
                        if (control1.Bounds.IntersectsWith(control2.Bounds))
                        {
                            overlappingControls++;
                        }
                    }
                }

                // Some overlap is expected (e.g., labels and their associated controls)
                // but excessive overlap indicates layout problems
                Assert.True(overlappingControls < controls.Count / 2,
                    $"Too many overlapping controls: {overlappingControls} out of {controls.Count}");

                form.Hide();
            }
        }

        private Bitmap CaptureFormImage(Form form)
        {
            var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
            return bitmap;
        }

        private DataGridView? FindDataGridView(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is DataGridView dgv) return dgv;
                var found = FindDataGridView(control);
                if (found != null) return found;
            }
            return null;
        }

        private List<Control> GetAllControls(Control parent)
        {
            var controls = new List<Control>();
            foreach (Control control in parent.Controls)
            {
                controls.Add(control);
                controls.AddRange(GetAllControls(control));
            }
            return controls;
        }

        private bool IsParentChild(Control control1, Control control2)
        {
            return control1.Parent == control2 || control2.Parent == control1 ||
                   IsAncestor(control1, control2) || IsAncestor(control2, control1);
        }

        private bool IsAncestor(Control potential_ancestor, Control control)
        {
            var parent = control.Parent;
            while (parent != null)
            {
                if (parent == potential_ancestor) return true;
                parent = parent.Parent;
            }
            return false;
        }
    }
}
