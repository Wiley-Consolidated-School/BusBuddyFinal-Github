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
        public Maintenance Maintenance { get; private set; }

        private ComboBox? cboVehicle;
        private TextBox? txtMaintenanceCompleted;
        private DateTimePicker? dtpDate;
        private TextBox? txtVendor;
        private TextBox? txtCost;
        private ComboBox? cboMaintenanceType;
        private TextBox? txtNotes;
        private Button? btnSave;
        private Button? btnCancel;

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
            txtMaintenanceCompleted = (TextBox)CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtMaintenanceCompleted, "Describe the maintenance performed");
            SetTextBoxMultiline(txtMaintenanceCompleted, true);
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
            txtVendor = (TextBox)CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtVendor, "Maintenance vendor/shop");
            y += spacing;

            // Cost
            var lblCost = CreateLabel("💰 Cost:", labelX, y);
            txtCost = (TextBox)CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtCost, "Total cost (e.g., 125.50)");
            y += spacing;

            // Notes
            var lblNotes = CreateLabel("📝 Notes:", labelX, y);
            txtNotes = (TextBox)CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtNotes, "Additional notes");
            SetTextBoxMultiline(txtNotes, true);
            txtNotes.Height = 60;
            y += 80;

            // Buttons
            btnSave = (Button)CreateButton("💾 Save", controlX, y, btnSave_Click);
            btnSave.Size = new Size(120, 36);

            btnCancel = (Button)CreateButton("❌ Cancel", controlX + 140, y, btnCancel_Click);
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

        // Helper methods for validation and form management
        private void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        private void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Helper methods for Syncfusion form creation
        private ComboBox CreateComboBox(string placeholder, int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Apply Material theming
            comboBox.BackColor = SyncfusionThemeHelper.MaterialColors.Surface;
            comboBox.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
            comboBox.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;

            this.Controls.Add(comboBox);
            return comboBox;
        }

        private void SetPlaceholderText(Control textBox, string placeholder)
        {
            if (textBox is TextBox tb)
            {
                tb.Text = placeholder;
                tb.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Text = "";
                        tb.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;
                    }
                };
            }
        }

        private void SetTextBoxMultiline(Control textBox, bool multiline)
        {
            if (textBox is TextBox tb)
            {
                tb.Multiline = multiline;
                if (multiline)
                {
                    tb.ScrollBars = ScrollBars.Vertical;
                }
            }
        }

        private Control CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 36),
                Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont,
                BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            this.Controls.Add(button);
            return button;
        }
    }
}
