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
    public class FuelEditFormSyncfusion : SyncfusionBaseForm
    {
        public Fuel Fuel { get; private set; }

        private ComboBox? cboVehicle;
        private DateTimePicker? dtpFuelDate;
        private Control? txtFuelAmount;
        private Control? txtFuelCost;
        private Control? txtNotes;
        private Control? btnSave;
        private Control? btnCancel;

        public FuelEditFormSyncfusion(Fuel? fuel = null)
        {
            Fuel = fuel != null ? new Fuel
            {
                FuelID = fuel.FuelID,
                VehicleFueledID = fuel.VehicleFueledID,
                FuelDate = fuel.FuelDate,
                FuelAmount = fuel.FuelAmount,
                FuelCost = fuel.FuelCost
            } : new Fuel();

            InitializeComponent();
            LoadFuelData();
            LoadVehicles();
        }

        private void InitializeComponent()
        {
            this.Text = Fuel.FuelID == 0 ? "⛽ Add Fuel Record" : "⛽ Edit Fuel Record";
            this.Size = new Size(450, 400);
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
            int controlWidth = 250;

            // Vehicle
            var lblVehicle = CreateLabel("🚌 Vehicle:", labelX, y);
            cboVehicle = CreateComboBox("Select Vehicle", controlX, y, controlWidth);
            y += spacing;

            // Fuel Date
            var lblFuelDate = CreateLabel("📅 Date:", labelX, y);
            dtpFuelDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpFuelDate);
            y += spacing;

            // Fuel Amount
            var lblFuelAmount = CreateLabel("⛽ Gallons:", labelX, y);
            txtFuelAmount = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtFuelAmount, "Enter gallons (e.g., 15.50)");
            y += spacing;

            // Fuel Cost
            var lblFuelCost = CreateLabel("💰 Total Cost:", labelX, y);
            txtFuelCost = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtFuelCost, "Enter cost (e.g., 45.75)");
            y += spacing;

            // Notes
            var lblNotes = CreateLabel("📝 Notes:", labelX, y);
            txtNotes = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtNotes, "Optional notes");
            SetTextBoxMultiline(txtNotes, 60);
            y += 80;

            // Buttons
            btnSave = CreateButton("💾 Save", controlX, y, btnSave_Click);
            btnSave.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
            btnSave.Size = new Size(120, 36);

            btnCancel = CreateButton("❌ Cancel", controlX + 130, y, btnCancel_Click);
            btnCancel.BackColor = SyncfusionThemeHelper.MaterialColors.Background;
            btnCancel.Size = new Size(120, 36);

            // Apply Syncfusion Material Design styling
            ApplySyncfusionStyling();
        }

        private void ApplySyncfusionStyling()
        {
            // Apply Syncfusion Material theme to the form
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            // Configure date picker for Material Design
            dtpFuelDate.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;
            dtpFuelDate.BackColor = Color.White;

            // Style all labels
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;
                    label.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
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

                // For demonstration, map VehicleFueledID to display text
                if (Fuel.VehicleFueledID.HasValue)
                {
                    var vehicleIndex = Fuel.VehicleFueledID.Value - 1;
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

        private void LoadFuelData()
        {
            dtpFuelDate.Value = Fuel.FuelDateAsDateTime ?? DateTime.Today;
            txtFuelAmount.Text = Fuel.FuelAmount?.ToString("F2") ?? "";
            txtFuelCost.Text = Fuel.FuelCost?.ToString("F2") ?? "";
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateFuel())
                return;

            // Extract vehicle ID from selection (in real implementation,
            // this would get the actual vehicle ID)
            Fuel.VehicleFueledID = cboVehicle.SelectedIndex + 1;
            Fuel.FuelDateAsDateTime = dtpFuelDate.Value;

            if (decimal.TryParse(txtFuelAmount.Text, out decimal amount))
                Fuel.FuelAmount = amount;

            if (decimal.TryParse(txtFuelCost.Text, out decimal cost))
                Fuel.FuelCost = cost;

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateFuel()
        {
            ClearAllValidationErrors();

            if (cboVehicle.SelectedIndex < 0)
            {
                SetValidationError(cboVehicle, "Please select a vehicle.");
                ShowErrorMessage("Please select a vehicle.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFuelAmount.Text) ||
                !decimal.TryParse(txtFuelAmount.Text, out decimal amount) ||
                amount <= 0)
            {
                SetValidationError(txtFuelAmount, "Please enter a valid fuel amount greater than 0.");
                ShowErrorMessage("Please enter a valid fuel amount greater than 0.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFuelCost.Text) ||
                !decimal.TryParse(txtFuelCost.Text, out decimal cost) ||
                cost < 0)
            {
                SetValidationError(txtFuelCost, "Please enter a valid fuel cost (0 or greater).");
                ShowErrorMessage("Please enter a valid fuel cost (0 or greater).");
                return false;
            }

            if (dtpFuelDate.Value > DateTime.Today)
            {
                SetValidationError(dtpFuelDate, "Fuel date cannot be in the future.");
                ShowErrorMessage("Fuel date cannot be in the future.");
                return false;
            }

            return true;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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

        private void SetTextBoxMultiline(Control textBox, int height)
        {
            if (textBox is TextBox tb)
            {
                tb.Multiline = true;
                tb.Height = height;
                tb.ScrollBars = ScrollBars.Vertical;
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

        // Validation methods
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
    }
}

