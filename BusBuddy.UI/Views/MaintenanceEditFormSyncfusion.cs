using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers; // Ensure correct logger namespace
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.UI.Views
{
    public class MaintenanceEditFormSyncfusion : Form
    {
        public Maintenance Maintenance { get; set; }
        private ComboBox cboVehicle;
        private TextBox txtMaintenanceCompleted;
        private DateTimePicker dtpDate;
        private TextBox txtVendor;
        private TextBox txtCost;
        private ComboBox cboMaintenanceType;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider _errorProvider = new ErrorProvider();

        public MaintenanceEditFormSyncfusion(System.IServiceProvider serviceProvider) : base()
        {
            Maintenance = new Maintenance();
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
            var lblBus = CreateLabel("🚌 Vehicle:");
            lblBus.Location = new Point(labelX, y);
            this.Controls.Add(lblBus);
            cboVehicle = CreateComboBox();
            cboVehicle.Location = new Point(controlX, y);
            cboVehicle.Size = new Size(controlWidth, 30);
            this.Controls.Add(cboVehicle);
            y += spacing;
            // Maintenance Type
            var lblMaintenanceType = CreateLabel("🔧 Type:");
            lblMaintenanceType.Location = new Point(labelX, y);
            this.Controls.Add(lblMaintenanceType);
            cboMaintenanceType = CreateComboBox();
            cboMaintenanceType.Location = new Point(controlX, y);
            cboMaintenanceType.Size = new Size(controlWidth, 30);
            cboMaintenanceType.Items.AddRange(new[] {
                "Oil Change", "Tire Replacement", "Brake Service", "Engine Repair",
                "Transmission Service", "Inspection", "Battery Replacement", "Other"
            });
            this.Controls.Add(cboMaintenanceType);
            y += spacing;
            // Maintenance Completed
            var lblMaintenanceCompleted = CreateLabel("📋 Description:");
            lblMaintenanceCompleted.Location = new Point(labelX, y);
            this.Controls.Add(lblMaintenanceCompleted);
            txtMaintenanceCompleted = CreateTextBox("Describe the maintenance performed", true);
            txtMaintenanceCompleted.Location = new Point(controlX, y);
            txtMaintenanceCompleted.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtMaintenanceCompleted);
            y += 80;
            // Date
            var lblDate = CreateLabel("📅 Date:");
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
            var lblVendor = CreateLabel("🏢 Vendor:");
            lblVendor.Location = new Point(labelX, y);
            this.Controls.Add(lblVendor);
            txtVendor = CreateTextBox("Maintenance vendor/shop");
            txtVendor.Location = new Point(controlX, y);
            txtVendor.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtVendor);
            y += spacing;
            // Cost
            var lblCost = CreateLabel("💰 Cost:");
            lblCost.Location = new Point(labelX, y);
            this.Controls.Add(lblCost);
            txtCost = CreateTextBox("Total cost (e.g., 125.50)");
            txtCost.Location = new Point(controlX, y);
            txtCost.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtCost);
            y += spacing;
            // Notes
            var lblNotes = CreateLabel("📝 Notes:");
            lblNotes.Location = new Point(labelX, y);
            this.Controls.Add(lblNotes);
            txtNotes = CreateTextBox("Additional notes", true);
            txtNotes.Location = new Point(controlX, y);
            txtNotes.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtNotes);
            y += 80;
            // Buttons
            btnSave = CreatePrimaryButton("💾 Save", btnSave_Click);
            btnSave.Location = new Point(controlX, y);
            btnSave.Size = new Size(120, 36);
            this.Controls.Add(btnSave);
            btnCancel = CreateSecondaryButton("❌ Cancel", btnCancel_Click);
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
                // For demonstration, map BusId to display text
                if (Maintenance.BusId.HasValue)
                {
                    var vehicleIndex = Maintenance.BusId.Value - 1;
                    if (vehicleIndex >= 0 && vehicleIndex < cboVehicle.Items.Count)
                    {
                        cboVehicle.SelectedIndex = vehicleIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                BusBuddyLogger.Info("MaintenanceEditForm", $"Error loading vehicles: {ex.Message}");
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
            }
        }

        private void LoadMaintenanceData()
        {
            txtMaintenanceCompleted.Text = Maintenance.MaintenanceCompleted ?? string.Empty;
            dtpDate.Value = Maintenance.DateAsDateTime ?? DateTime.Today;
            txtVendor.Text = Maintenance.Vendor ?? string.Empty;
            txtNotes.Text = Maintenance.Notes ?? string.Empty;
            // Set default maintenance type if adding new
            if (Maintenance.MaintenanceID == 0)
            {
                cboMaintenanceType.SelectedIndex = 0; // Default to first item
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateMaintenance())
                return;
            // Extract vehicle ID from selection (in real implementation,
            // this would get the actual vehicle ID)
            Maintenance.BusId = cboVehicle.SelectedIndex + 1;
            Maintenance.MaintenanceCompleted = txtMaintenanceCompleted.Text.Trim();
            Maintenance.DateAsDateTime = dtpDate.Value;
            Maintenance.Vendor = txtVendor.Text.Trim();
            Maintenance.Notes = txtNotes.Text.Trim();
            BusBuddyLogger.Info("MaintenanceEditForm", "Maintenance record saved.");
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            BusBuddyLogger.Info("MaintenanceEditForm", "Maintenance edit cancelled.");
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // Helper methods for validation and form management
        protected void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        protected void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        protected void ShowErrorMessage(string message)
        {
            BusBuddyLogger.Info("MaintenanceEditForm", $"Validation error: {message}");
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
        }

        private TextBox CreateTextBox(string placeholder = "", bool multiline = false)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Multiline = multiline,
                PlaceholderText = placeholder
            };
        }

        private Button CreatePrimaryButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 36)
            };
            btn.Click += onClick;
            return btn;
        }

        private Button CreateSecondaryButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.LightGray,
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 36)
            };
            btn.Click += onClick;
            return btn;
        }
    }
}

