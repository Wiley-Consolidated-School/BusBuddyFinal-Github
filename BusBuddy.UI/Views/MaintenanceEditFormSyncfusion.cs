using System;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.ListView;
using BusBuddy.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;

using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class MaintenanceEditFormSyncfusion : SyncfusionBaseForm
    {
        public Maintenance Maintenance { get; private set; }        private ComboBoxAdv? cboVehicle;
        private TextBoxExt? txtMaintenanceCompleted;
        private DateTimePicker? dtpDate;
        private TextBoxExt? txtVendor;
        private TextBoxExt? txtCost;
        private ComboBoxAdv? cboMaintenanceType;        private TextBoxExt? txtNotes;
        private SfButton? btnSave;
        private SfButton? btnCancel;

        public MaintenanceEditFormSyncfusion(Maintenance? maintenance = null)
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
            int controlWidth = 280;            // Vehicle
            var lblVehicle = ControlFactory.CreateLabel("🚌 Vehicle:");
            lblVehicle.Location = new Point(labelX, y);
            this.Controls.Add(lblVehicle);
            cboVehicle = ControlFactory.CreateComboBox();
            cboVehicle.Location = new Point(controlX, y);
            cboVehicle.Size = new Size(controlWidth, 30);
            this.Controls.Add(cboVehicle);
            y += spacing;

            // Maintenance Type
            var lblMaintenanceType = ControlFactory.CreateLabel("🔧 Type:");
            lblMaintenanceType.Location = new Point(labelX, y);
            this.Controls.Add(lblMaintenanceType);
            cboMaintenanceType = ControlFactory.CreateComboBox();
            cboMaintenanceType.Location = new Point(controlX, y);
            cboMaintenanceType.Size = new Size(controlWidth, 30);
            cboMaintenanceType.Items.AddRange(new[] {
                "Oil Change", "Tire Replacement", "Brake Service", "Engine Repair",
                "Transmission Service", "Inspection", "Battery Replacement", "Other"
            });
            this.Controls.Add(cboMaintenanceType);
            y += spacing;

            // Maintenance Completed
            var lblMaintenanceCompleted = ControlFactory.CreateLabel("📋 Description:");
            lblMaintenanceCompleted.Location = new Point(labelX, y);
            this.Controls.Add(lblMaintenanceCompleted);
            txtMaintenanceCompleted = ControlFactory.CreateTextBox("Describe the maintenance performed", true);
            txtMaintenanceCompleted.Location = new Point(controlX, y);
            txtMaintenanceCompleted.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtMaintenanceCompleted);
            y += 80;

            // Date
            var lblDate = ControlFactory.CreateLabel("📅 Date:");
            lblDate.Location = new Point(labelX, y);
            this.Controls.Add(lblDate);
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpDate);
            y += spacing;

            // Vendor
            var lblVendor = ControlFactory.CreateLabel("🏢 Vendor:");
            lblVendor.Location = new Point(labelX, y);
            this.Controls.Add(lblVendor);
            txtVendor = ControlFactory.CreateTextBox("Maintenance vendor/shop");
            txtVendor.Location = new Point(controlX, y);
            txtVendor.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtVendor);
            y += spacing;

            // Cost
            var lblCost = ControlFactory.CreateLabel("💰 Cost:");
            lblCost.Location = new Point(labelX, y);
            this.Controls.Add(lblCost);
            txtCost = ControlFactory.CreateTextBox("Total cost (e.g., 125.50)");
            txtCost.Location = new Point(controlX, y);
            txtCost.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtCost);
            y += spacing;

            // Notes
            var lblNotes = ControlFactory.CreateLabel("📝 Notes:");
            lblNotes.Location = new Point(labelX, y);
            this.Controls.Add(lblNotes);
            txtNotes = ControlFactory.CreateTextBox("Additional notes", true);
            txtNotes.Location = new Point(controlX, y);
            txtNotes.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtNotes);
            y += 80;            // Buttons
            btnSave = ControlFactory.CreatePrimaryButton("💾 Save", btnSave_Click);
            btnSave.Location = new Point(controlX, y);
            btnSave.Size = new Size(120, 36);
            this.Controls.Add(btnSave);

            btnCancel = ControlFactory.CreateSecondaryButton("❌ Cancel", btnCancel_Click);
            btnCancel.Location = new Point(controlX + 140, y);
            btnCancel.Size = new Size(120, 36);
            this.Controls.Add(btnCancel);
            btnCancel.Size = new Size(120, 36);

            // Apply Material Design styling
            ApplyMaterialStyling();
        }

        private void ApplyMaterialStyling()
        {
            // Configure date picker for Material Design
            dtpDate.Font = new Font("Roboto", 10F);
            dtpDate.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);

            // Style all labels
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Roboto", 10F, FontStyle.Bold);
                    label.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
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
        }        // Helper methods for validation and form management
        protected override void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        protected override void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        protected new void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
