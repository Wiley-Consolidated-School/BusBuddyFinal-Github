using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class RouteEditForm : StandardDataForm
    {
        public Route Route { get; private set; }

        private DateTimePicker dtpDate;
        private MaterialTextBox txtRouteName;
        private MaterialComboBox cboAMVehicle;
        private MaterialComboBox cboAMDriver;
        private MaterialTextBox txtAMBeginMiles;
        private MaterialTextBox txtAMEndMiles;
        private MaterialTextBox txtAMRiders;
        private MaterialComboBox cboPMVehicle;
        private MaterialComboBox cboPMDriver;
        private MaterialTextBox txtPMBeginMiles;
        private MaterialTextBox txtPMEndMiles;
        private MaterialTextBox txtPMRiders;
        private MaterialTextBox txtNotes;
        private MaterialButton btnSave;
        private MaterialButton btnCancel;

        public RouteEditForm(Route? route = null)
        {
            Route = route != null ? new Route
            {
                RouteID = route.RouteID,
                Date = route.Date,
                RouteName = route.RouteName,
                AMVehicleID = route.AMVehicleID,
                AMDriverID = route.AMDriverID,
                AMBeginMiles = route.AMBeginMiles,
                AMEndMiles = route.AMEndMiles,
                AMRiders = route.AMRiders,
                PMVehicleID = route.PMVehicleID,
                PMDriverID = route.PMDriverID,
                PMBeginMiles = route.PMBeginMiles,
                PMEndMiles = route.PMEndMiles,
                PMRiders = route.PMRiders,
                Notes = route.Notes
            } : new Route { DateAsDateTime = DateTime.Today };

            InitializeComponent();
            LoadRouteData();
            LoadVehiclesAndDrivers();
        }

        private void InitializeComponent()
        {
            this.Text = Route.RouteID == 0 ? "🚌 Add Route" : "🚌 Edit Route";
            this.Size = new Size(550, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Configure for high DPI
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            SetupFormLayout();
        }

        private void SetupFormLayout()
        {
            int y = 30;
            int labelX = 30;
            int controlX = 150;
            int spacing = 50;
            int controlWidth = 320;

            // Date
            var lblDate = CreateLabel("📅 Date:", labelX, y);
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpDate);
            y += spacing;

            // Route Name
            var lblRouteName = CreateLabel("🚌 Route Name:", labelX, y);
            txtRouteName = CreateTextBox(controlX, y, controlWidth);
            txtRouteName.Hint = "Enter route name";
            y += spacing;

            // AM Section Header
            var lblAMHeader = CreateLabel("🌅 AM ROUTE", labelX, y);
            lblAMHeader.Font = new Font("Roboto", 12F, FontStyle.Bold);
            lblAMHeader.ForeColor = Color.FromArgb(25, 118, 210);
            y += 40;

            // AM Vehicle
            var lblAMVehicle = CreateLabel("🚌 AM Vehicle:", labelX, y);
            cboAMVehicle = CreateComboBox("Select AM Vehicle", controlX, y, controlWidth);
            y += spacing;

            // AM Driver
            var lblAMDriver = CreateLabel("👤 AM Driver:", labelX, y);
            cboAMDriver = CreateComboBox("Select AM Driver", controlX, y, controlWidth);
            y += spacing;

            // AM Miles
            var lblAMBeginMiles = CreateLabel("📏 AM Start Miles:", labelX, y);
            txtAMBeginMiles = CreateTextBox(controlX, y, 150);
            txtAMBeginMiles.Hint = "Start odometer";

            var lblAMEndMiles = CreateLabel("📏 End:", controlX + 160, y);
            txtAMEndMiles = CreateTextBox(controlX + 200, y, 150);
            txtAMEndMiles.Hint = "End odometer";
            y += spacing;

            // AM Riders
            var lblAMRiders = CreateLabel("👥 AM Riders:", labelX, y);
            txtAMRiders = CreateTextBox(controlX, y, controlWidth);
            txtAMRiders.Hint = "Number of riders";
            y += spacing + 20;

            // PM Section Header
            var lblPMHeader = CreateLabel("🌆 PM ROUTE", labelX, y);
            lblPMHeader.Font = new Font("Roboto", 12F, FontStyle.Bold);
            lblPMHeader.ForeColor = Color.FromArgb(245, 124, 0);
            y += 40;

            // PM Vehicle
            var lblPMVehicle = CreateLabel("🚌 PM Vehicle:", labelX, y);
            cboPMVehicle = CreateComboBox("Select PM Vehicle", controlX, y, controlWidth);
            y += spacing;

            // PM Driver
            var lblPMDriver = CreateLabel("👤 PM Driver:", labelX, y);
            cboPMDriver = CreateComboBox("Select PM Driver", controlX, y, controlWidth);
            y += spacing;

            // PM Miles
            var lblPMBeginMiles = CreateLabel("📏 PM Start Miles:", labelX, y);
            txtPMBeginMiles = CreateTextBox(controlX, y, 150);
            txtPMBeginMiles.Hint = "Start odometer";

            var lblPMEndMiles = CreateLabel("📏 End:", controlX + 160, y);
            txtPMEndMiles = CreateTextBox(controlX + 200, y, 150);
            txtPMEndMiles.Hint = "End odometer";
            y += spacing;

            // PM Riders
            var lblPMRiders = CreateLabel("👥 PM Riders:", labelX, y);
            txtPMRiders = CreateTextBox(controlX, y, controlWidth);
            txtPMRiders.Hint = "Number of riders";
            y += spacing + 20;

            // Notes
            var lblNotes = CreateLabel("📝 Notes:", labelX, y);
            txtNotes = CreateTextBox(controlX, y, controlWidth);
            txtNotes.Hint = "Additional notes";
            txtNotes.Multiline = true;
            txtNotes.Height = 60;
            y += 80;

            // Buttons
            btnSave = CreateButton("💾 Save", controlX, y, btnSave_Click);
            btnSave.UseAccentColor = true;
            btnSave.Type = MaterialButton.MaterialButtonType.Contained;
            btnSave.Size = new Size(120, 36);

            btnCancel = CreateButton("❌ Cancel", controlX + 140, y, btnCancel_Click);
            btnCancel.Type = MaterialButton.MaterialButtonType.Outlined;
            btnCancel.Size = new Size(120, 36);

            // Apply Material Design styling
            ApplyMaterialStyling();
        }

        private void ApplyMaterialStyling()
        {
            // Configure date picker for Material Design
            dtpDate.Font = new Font("Roboto", 10F);
            dtpDate.BackColor = Color.White;

            // Style all labels
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    if (!label.Font.Bold) // Don't override header styles
                    {
                        label.Font = new Font("Roboto", 10F, FontStyle.Bold);
                        label.ForeColor = Color.FromArgb(33, 33, 33);
                    }
                }
            }
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                // Add placeholder vehicles - in a real implementation,
                // this would load from the vehicle repository
                var vehicles = new[] {
                    "Vehicle 1 - Bus #001", "Vehicle 2 - Bus #002", "Vehicle 3 - Bus #003",
                    "Vehicle 4 - Bus #004", "Vehicle 5 - Bus #005"
                };

                cboAMVehicle.Items.AddRange(vehicles);
                cboPMVehicle.Items.AddRange(vehicles);

                // Add placeholder drivers - in a real implementation,
                // this would load from the driver repository
                var drivers = new[] {
                    "Driver 1 - John Smith", "Driver 2 - Jane Doe", "Driver 3 - Mike Johnson",
                    "Driver 4 - Sarah Wilson", "Driver 5 - Tom Brown"
                };

                cboAMDriver.Items.AddRange(drivers);
                cboPMDriver.Items.AddRange(drivers);

                // Set selections if editing existing route
                if (Route.AMVehicleID.HasValue)
                {
                    var amVehicleIndex = Route.AMVehicleID.Value - 1;
                    if (amVehicleIndex >= 0 && amVehicleIndex < cboAMVehicle.Items.Count)
                        cboAMVehicle.SelectedIndex = amVehicleIndex;
                }

                if (Route.AMDriverID.HasValue)
                {
                    var amDriverIndex = Route.AMDriverID.Value - 1;
                    if (amDriverIndex >= 0 && amDriverIndex < cboAMDriver.Items.Count)
                        cboAMDriver.SelectedIndex = amDriverIndex;
                }

                if (Route.PMVehicleID.HasValue)
                {
                    var pmVehicleIndex = Route.PMVehicleID.Value - 1;
                    if (pmVehicleIndex >= 0 && pmVehicleIndex < cboPMVehicle.Items.Count)
                        cboPMVehicle.SelectedIndex = pmVehicleIndex;
                }

                if (Route.PMDriverID.HasValue)
                {
                    var pmDriverIndex = Route.PMDriverID.Value - 1;
                    if (pmDriverIndex >= 0 && pmDriverIndex < cboPMDriver.Items.Count)
                        cboPMDriver.SelectedIndex = pmDriverIndex;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles and drivers: {ex.Message}");
            }
        }

        private void LoadRouteData()
        {
            dtpDate.Value = Route.DateAsDateTime;
            txtRouteName.Text = Route.RouteName ?? "";
            txtAMBeginMiles.Text = Route.AMBeginMiles?.ToString("F1") ?? "";
            txtAMEndMiles.Text = Route.AMEndMiles?.ToString("F1") ?? "";
            txtAMRiders.Text = Route.AMRiders?.ToString() ?? "";
            txtPMBeginMiles.Text = Route.PMBeginMiles?.ToString("F1") ?? "";
            txtPMEndMiles.Text = Route.PMEndMiles?.ToString("F1") ?? "";
            txtPMRiders.Text = Route.PMRiders?.ToString() ?? "";
            txtNotes.Text = Route.Notes ?? "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateRoute())
                return;

            Route.DateAsDateTime = dtpDate.Value;
            Route.RouteName = txtRouteName.Text.Trim();

            // AM Route data
            Route.AMVehicleID = cboAMVehicle.SelectedIndex >= 0 ? cboAMVehicle.SelectedIndex + 1 : null;
            Route.AMDriverID = cboAMDriver.SelectedIndex >= 0 ? cboAMDriver.SelectedIndex + 1 : null;

            if (decimal.TryParse(txtAMBeginMiles.Text, out decimal amBegin))
                Route.AMBeginMiles = amBegin;
            if (decimal.TryParse(txtAMEndMiles.Text, out decimal amEnd))
                Route.AMEndMiles = amEnd;
            if (int.TryParse(txtAMRiders.Text, out int amRiders))
                Route.AMRiders = amRiders;

            // PM Route data
            Route.PMVehicleID = cboPMVehicle.SelectedIndex >= 0 ? cboPMVehicle.SelectedIndex + 1 : null;
            Route.PMDriverID = cboPMDriver.SelectedIndex >= 0 ? cboPMDriver.SelectedIndex + 1 : null;

            if (decimal.TryParse(txtPMBeginMiles.Text, out decimal pmBegin))
                Route.PMBeginMiles = pmBegin;
            if (decimal.TryParse(txtPMEndMiles.Text, out decimal pmEnd))
                Route.PMEndMiles = pmEnd;
            if (int.TryParse(txtPMRiders.Text, out int pmRiders))
                Route.PMRiders = pmRiders;

            Route.Notes = txtNotes.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateRoute()
        {
            ClearAllValidationErrors();

            if (dtpDate.Value > DateTime.Today.AddDays(365))
            {
                SetValidationError(dtpDate, "Date cannot be more than a year in the future.");
                ShowErrorMessage("Date cannot be more than a year in the future.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRouteName.Text))
            {
                SetValidationError(txtRouteName, "Route name is required.");
                ShowErrorMessage("Route name is required.");
                return false;
            }

            // Validate AM miles if both are provided
            if (!string.IsNullOrWhiteSpace(txtAMBeginMiles.Text) && !string.IsNullOrWhiteSpace(txtAMEndMiles.Text))
            {
                if (decimal.TryParse(txtAMBeginMiles.Text, out decimal amBegin) &&
                    decimal.TryParse(txtAMEndMiles.Text, out decimal amEnd))
                {
                    if (amEnd < amBegin)
                    {
                        SetValidationError(txtAMEndMiles, "AM end miles cannot be less than AM begin miles.");
                        ShowErrorMessage("AM end miles cannot be less than AM begin miles.");
                        return false;
                    }
                }
            }

            // Validate PM miles if both are provided
            if (!string.IsNullOrWhiteSpace(txtPMBeginMiles.Text) && !string.IsNullOrWhiteSpace(txtPMEndMiles.Text))
            {
                if (decimal.TryParse(txtPMBeginMiles.Text, out decimal pmBegin) &&
                    decimal.TryParse(txtPMEndMiles.Text, out decimal pmEnd))
                {
                    if (pmEnd < pmBegin)
                    {
                        SetValidationError(txtPMEndMiles, "PM end miles cannot be less than PM begin miles.");
                        ShowErrorMessage("PM end miles cannot be less than PM begin miles.");
                        return false;
                    }
                }
            }

            // Validate rider counts if provided
            if (!string.IsNullOrWhiteSpace(txtAMRiders.Text) &&
                (!int.TryParse(txtAMRiders.Text, out int amRiders) || amRiders < 0))
            {
                SetValidationError(txtAMRiders, "AM riders must be a valid number (0 or greater).");
                ShowErrorMessage("AM riders must be a valid number (0 or greater).");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPMRiders.Text) &&
                (!int.TryParse(txtPMRiders.Text, out int pmRiders) || pmRiders < 0))
            {
                SetValidationError(txtPMRiders, "PM riders must be a valid number (0 or greater).");
                ShowErrorMessage("PM riders must be a valid number (0 or greater).");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
