using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.Tests.TestHelpers
{
    /// <summary>
    /// Test wrapper for MockBaseDataForm to expose methods for testing
    /// </summary>
    public class TestableBaseDataForm : MockBaseDataForm
    {
        public Label TestCreateLabel(string text, int x, int y) => CreateLabel(text, x, y);
        public TextBox TestCreateTextBox(int x, int y, int width) => CreateTextBox(x, y, width);
        public Button TestCreateButton(string text, int x, int y) => CreateButton(text, x, y);
        public DataGridView TestCreateDataGridView(int x, int y, int width, int height) => CreateDataGridView(x, y, width, height);
        public ComboBox TestCreateComboBox(int x, int y, int width) => CreateComboBox(x, y, width);
        public DateTimePicker TestCreateDatePicker(int x, int y, int width) => CreateDatePicker(x, y, width);
        public CheckBox TestCreateCheckBox(string text, int x, int y) => CreateCheckBox(text, x, y);
    }

    /// <summary>
    /// Common utility methods for UI testing
    /// </summary>
    public static class UITestHelpers
    {
        /// <summary>
        /// Find a control of specific type by name recursively
        /// </summary>
        public static T? FindControl<T>(Control parent, string? name) where T : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is T && (name == null || control.Name == name)) return (T)control;
                var found = FindControl<T>(control, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Check if a control of specific type exists in the form
        /// </summary>
        public static bool HasControlOfType<T>(Control parent) where T : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is T) return true;
                if (HasControlOfType<T>(control)) return true;
            }
            return false;
        }

        /// <summary>
        /// Find a button by its text
        /// </summary>
        public static Button? FindButtonByText(Control parent, string text)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button button && button.Text == text) return button;
                var found = FindButtonByText(control, text);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Find a panel that contains a child control with the specified name
        /// </summary>
        public static Panel? FindPanelByChildControl(Control parent, string childControlName)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Panel panel)
                {
                    if (FindControl<Control>(panel, childControlName) != null)
                        return panel;
                }
                var found = FindPanelByChildControl(control, childControlName);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Capture a screenshot of a form for visual regression testing
        /// </summary>
        public static Bitmap CaptureFormScreenshot(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
            return bitmap;
        }

        /// <summary>
        /// Compare two bitmaps for visual regression testing
        /// </summary>
        public static bool AreImagesEqual(Bitmap image1, Bitmap image2, double tolerance = 0.0)
        {
            if (image1.Size != image2.Size) return false;

            int differences = 0;
            int totalPixels = image1.Width * image1.Height;

            for (int x = 0; x < image1.Width; x++)
            {
                for (int y = 0; y < image1.Height; y++)
                {
                    if (image1.GetPixel(x, y) != image2.GetPixel(x, y))
                    {
                        differences++;
                    }
                }
            }

            double differencePercentage = (double)differences / totalPixels * 100;
            return differencePercentage <= tolerance;
        }
    }

    /// <summary>
    /// Mock MainForm for testing that doesn't require database connection
    /// </summary>
    public class MockMainForm : Form
    {
        public MockMainForm(string? customTitle = null, Size? customSize = null)
        {
            AutoScaleMode = AutoScaleMode.Font;
            var size = customSize ?? new Size(1024, 768); // Default for most tests
            Size = size;
            ClientSize = size;
            Text = customTitle ?? "BusBuddy - Bus Tracking Companion";
            StartPosition = FormStartPosition.CenterScreen;

            // Add some basic controls for testing
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var dataMenu = new ToolStripMenuItem("Data Management");
            var reportsMenu = new ToolStripMenuItem("Reports");

            menuStrip.Items.AddRange(new[] { fileMenu, dataMenu, reportsMenu });

            var vehicleButton = new Button
            {
                Text = "Manage Vehicles",
                Location = new Point(50, 50),
                Size = new Size(150, 30),
                Name = "btnVehicles"
            };

            Controls.Add(menuStrip);
            Controls.Add(vehicleButton);
            MainMenuStrip = menuStrip;

            // Add mock chart controls for tests
            var chartGrossRevenue = new System.Windows.Forms.DataVisualization.Charting.Chart { Name = "chartGrossRevenue", Size = new Size(400, 300), Location = new Point(200, 100) };
            chartGrossRevenue.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea());
            var series1 = new System.Windows.Forms.DataVisualization.Charting.Series { ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column, IsValueShownAsLabel = true };
            chartGrossRevenue.Series.Add(series1);
            chartGrossRevenue.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend());
            var chartTopProducts = new System.Windows.Forms.DataVisualization.Charting.Chart { Name = "chartTopProducts", Size = new Size(400, 300), Location = new Point(650, 100) };
            chartTopProducts.ChartAreas.Add(new System.Windows.Forms.DataVisualization.Charting.ChartArea());
            var series2 = new System.Windows.Forms.DataVisualization.Charting.Series { ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut, IsValueShownAsLabel = true };
            chartTopProducts.Series.Add(series2);
            chartTopProducts.Legends.Add(new System.Windows.Forms.DataVisualization.Charting.Legend());
            Controls.Add(chartGrossRevenue);
            Controls.Add(chartTopProducts);

            // Add a mock refresh button for Form1 tests
            var refreshButton = new Button { Text = "Refresh", Name = "btnRefresh", Location = new Point(50, 100), Size = new Size(100, 30), Enabled = true };
            Controls.Add(refreshButton);
        }
    }

    /// <summary>
    /// Mock VehicleManagementForm for testing without database dependencies
    /// </summary>
    public class MockVehicleManagementForm : Form
    {
        public MockVehicleManagementForm(Size? customSize = null)
        {
            AutoScaleMode = AutoScaleMode.Font;
            var size = customSize ?? new Size(800, 600);
            Size = size;
            ClientSize = size;
            Text = "Vehicle Management";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Add basic controls for testing
            var dataGrid = new DataGridView
            {
                Name = "vehicleDataGrid",
                Location = new Point(10, 10),
                Size = new Size(600, 400),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            var addButton = new Button { Text = "Add New", Location = new Point(620, 10), Size = new Size(80, 30) };
            var editButton = new Button { Text = "Edit", Location = new Point(620, 50), Size = new Size(80, 30) };
            var deleteButton = new Button { Text = "Delete", Location = new Point(620, 90), Size = new Size(80, 30) };
            var detailsButton = new Button { Text = "Details", Location = new Point(620, 130), Size = new Size(80, 30) };

            Controls.AddRange(new Control[] { dataGrid, addButton, editButton, deleteButton, detailsButton });
        }
    }

    /// <summary>
    /// Mock VehicleForm for testing without database dependencies
    /// </summary>
    public class MockVehicleForm : Form
    {
        public MockVehicleForm()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Vehicle Details";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterParent;

            // Add basic form controls for testing
            var busNumberLabel = new Label { Text = "Bus Number:", Location = new Point(10, 10) };
            var busNumberTextBox = new TextBox { Name = "txtBusNumber", Location = new Point(100, 10), Width = 200 };

            var makeLabel = new Label { Text = "Make:", Location = new Point(10, 40) };
            var makeTextBox = new TextBox { Name = "txtMake", Location = new Point(100, 40), Width = 200 };

            var saveButton = new Button { Text = "Save", Location = new Point(100, 200), Size = new Size(80, 30) };
            var cancelButton = new Button { Text = "Cancel", Location = new Point(190, 200), Size = new Size(80, 30) };

            Controls.AddRange(new Control[] {
                busNumberLabel, busNumberTextBox,
                makeLabel, makeTextBox,
                saveButton, cancelButton
            });
        }
    }

    /// <summary>
    /// Mock DriverManagementForm for testing
    /// </summary>
    public class MockDriverManagementForm : Form
    {
        public MockDriverManagementForm()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Driver Management";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var editPanel = new Panel
            {
                Name = "_editPanel",
                Location = new Point(10, 10),
                Size = new Size(300, 200),
                Visible = false // Initially hidden
            };

            Controls.Add(editPanel);
        }
    }

    /// <summary>
    /// Mock RouteManagementForm for testing
    /// </summary>
    public class MockRouteManagementForm : Form
    {
        public MockRouteManagementForm()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Text = "Route Management";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
        }
    }

    /// <summary>
    /// Mock BaseDataForm for testing that doesn't require database connection
    /// </summary>
    public class MockBaseDataForm : Form
    {
        public Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        public TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23)
            };
        }

        public Button CreateButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(100, 30)
            };
        }

        public DataGridView CreateDataGridView(int x, int y, int width, int height)
        {
            return new DataGridView
            {
                Location = new Point(x, y),
                Size = new Size(width, height)
            };
        }

        public ComboBox CreateComboBox(int x, int y, int width)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 23)
            };
        }

        public DateTimePicker CreateDatePicker(int x, int y, int width)
        {
            return new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, 23)
            };
        }

        public CheckBox CreateCheckBox(string text, int x, int y)
        {
            return new CheckBox
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            };
        }
    }

    /// <summary>
    /// Test wrapper for MockBaseDataForm to expose methods
    /// </summary>
    public class TestableMockBaseDataForm : MockBaseDataForm
    {
        public Label TestCreateLabel(string text, int x, int y) => CreateLabel(text, x, y);
        public TextBox TestCreateTextBox(int x, int y, int width) => CreateTextBox(x, y, width);
        public Button TestCreateButton(string text, int x, int y) => CreateButton(text, x, y);
        public DataGridView TestCreateDataGridView(int x, int y, int width, int height) => CreateDataGridView(x, y, width, height);
        public ComboBox TestCreateComboBox(int x, int y, int width) => CreateComboBox(x, y, width);
        public DateTimePicker TestCreateDatePicker(int x, int y, int width) => CreateDatePicker(x, y, width);
        public CheckBox TestCreateCheckBox(string text, int x, int y) => CreateCheckBox(text, x, y);
    }
}
