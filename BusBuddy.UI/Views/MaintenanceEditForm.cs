using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class MaintenanceEditForm : StandardDataForm
    {
        public Maintenance Maintenance { get; private set; }

        private MaterialComboBox cboVehicle;
        private MaterialTextBox txtMaintenanceCompleted;
        private DateTimePicker? dtpDate;
        private MaterialTextBox txtVendor;
        private MaterialTextBox txtCost;
        private MaterialComboBox cboMaintenanceType;
        private MaterialTextBox txtNotes;
        private MaterialButton btnSave;
        private MaterialButton btnCancel;

        public MaintenanceEditForm(Maintenance? maintenance = null)
        {
            Maintenance = maintenance != null ? new Maintenance
            {
                MaintenanceID = maintenance.MaintenanceID,
                VehicleID = maintenance.VehicleID,
                MaintenanceCompleted = maintenance.MaintenanceCompleted,
                Date = maintenance.Date,
                Vendor = maintenance.Vendor,
                Notes = maintenance.Notes
            } : new Maintenance();

            InitializeComponent();
            LoadMaintenanceData();
            LoadVehicles();
        }

        private void InitializeComponent()
        {
            this.Text = Maintenance.MaintenanceID == 0 ? "🔧 Add Maintenance Record" : "🔧 Edit Maintenance Record";
            this.Size = new Size(500, 550);
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
            int spacing = 60;
            int controlWidth = 280;

            // Vehicle
            var lblVehicle = CreateLabel("🚌 Vehicle:", labelX, y);
            cboVehicle = CreateComboBox("Select Vehicle", controlX, y, controlWidth);
            y += spacing;

            // Maintenance Type
            var lblMaintenanceType = CreateLabel("🔧 Type:", labelX, y);
            cboMaintenanceType = CreateComboBox("Maintenance Type", controlX, y, controlWidth);
            cboMaintenanceType.Items.AddRange(new[] {
                "Oil Change", "Tire Replacement", "Brake Service", "Engine Repair",
                "Transmission Service", "Inspection", "Battery Replacement", "Other"
            });
            y += spacing;

            // Maintenance Completed
            var lblMaintenanceCompleted = CreateLabel("📋 Description:", labelX, y);
            txtMaintenanceCompleted = CreateTextBox(controlX, y, controlWidth);
            txtMaintenanceCompleted.Hint = "Describe the maintenance performed";
            txtMaintenanceCompleted.Multiline = true;
            txtMaintenanceCompleted.Height = 60;
            y += 80;

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

            // Vendor
            var lblVendor = CreateLabel("🏢 Vendor:", labelX, y);
            txtVendor = CreateTextBox(controlX, y, controlWidth);
            txtVendor.Hint = "Maintenance vendor/shop";
            y += spacing;

            // Cost
            var lblCost = CreateLabel("💰 Cost:", labelX, y);
            txtCost = CreateTextBox(controlX, y, controlWidth);
            txtCost.Hint = "Total cost (e.g., 125.50)";
            y += spacing;

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
                    label.Font = new Font("Roboto", 10F, FontStyle.Bold);
                    label.ForeColor = Color.FromArgb(33, 33, 33);
                }
            }
        }

        private void LoadVehicles()
        {
            try
            {
                // Add placeholder vehicles - in a real implementation,
                // this would load from the vehicle repository
                cboVehicle.Items.Clear();
                cboVehicle.Items.Add("Vehicle 1 - Bus #001");
                cboVehicle.Items.Add("Vehicle 2 - Bus #002");
                cboVehicle.Items.Add("Vehicle 3 - Bus #003");
                cboVehicle.Items.Add("Vehicle 4 - Bus #004");
                cboVehicle.Items.Add("Vehicle 5 - Bus #005");

                // For demonstration, map VehicleID to display text
                if (Maintenance.VehicleID.HasValue)
                {
                    var vehicleIndex = Maintenance.VehicleID.Value - 1;
                    if (vehicleIndex >= 0 && vehicleIndex < cboVehicle.Items.Count)
                    {
                        cboVehicle.SelectedIndex = vehicleIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
            }
        }

        private void LoadMaintenanceData()
        {
            txtMaintenanceCompleted.Text = Maintenance.MaintenanceCompleted ?? "";
            dtpDate.Value = Maintenance.DateAsDateTime ?? DateTime.Today;
            txtVendor.Text = Maintenance.Vendor ?? "";
            txtNotes.Text = Maintenance.Notes ?? "";

            // Set default maintenance type if adding new
            if (Maintenance.MaintenanceID == 0)
            {
                cboMaintenanceType.SelectedIndex = 0; // Default to first item
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateMaintenance())
                return;

            // Extract vehicle ID from selection (in real implementation,
            // this would get the actual vehicle ID)
            Maintenance.VehicleID = cboVehicle.SelectedIndex + 1;
            Maintenance.MaintenanceCompleted = txtMaintenanceCompleted.Text.Trim();
            Maintenance.DateAsDateTime = dtpDate.Value;
            Maintenance.Vendor = txtVendor.Text.Trim();
            Maintenance.Notes = txtNotes.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateMaintenance()
        {
            ClearAllValidationErrors();

            if (cboVehicle.SelectedIndex < 0)
            {
                SetValidationError(cboVehicle, "Please select a vehicle.");
                ShowErrorMessage("Please select a vehicle.");
                return false;
            }

            if (cboMaintenanceType.SelectedIndex < 0)
            {
                SetValidationError(cboMaintenanceType, "Please select a maintenance type.");
                ShowErrorMessage("Please select a maintenance type.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMaintenanceCompleted.Text))
            {
                SetValidationError(txtMaintenanceCompleted, "Please describe the maintenance performed.");
                ShowErrorMessage("Please describe the maintenance performed.");
                return false;
            }

            if (dtpDate.Value > DateTime.Today)
            {
                SetValidationError(dtpDate, "Maintenance date cannot be in the future.");
                ShowErrorMessage("Maintenance date cannot be in the future.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtCost.Text))
            {
                if (!decimal.TryParse(txtCost.Text, out decimal cost) || cost < 0)
                {
                    SetValidationError(txtCost, "Please enter a valid cost (0 or greater).");
                    ShowErrorMessage("Please enter a valid cost (0 or greater).");
                    return false;
                }
            }

            return true;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}

