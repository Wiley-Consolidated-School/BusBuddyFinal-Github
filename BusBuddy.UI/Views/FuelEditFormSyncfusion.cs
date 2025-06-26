using System;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Views;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class FuelEditFormSyncfusion : SyncfusionBaseForm
    {
        public Fuel Fuel { get; private set; }

        private ComboBoxAdv? cboVehicle;
        private DateTimePicker? dtpFuelDate;
        private TextBoxExt? txtFuelAmount;
        private TextBoxExt? txtFuelCost;
        private TextBoxExt? txtNotes;
        private SfButton? btnSave;
        private SfButton? btnCancel;

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
            int controlWidth = 250;            // Vehicle
            var lblVehicle = ControlFactory.CreateLabel("🚌 Vehicle:");
            lblVehicle.Location = new Point(labelX, y);
            this.Controls.Add(lblVehicle);
            cboVehicle = ControlFactory.CreateComboBox();
            cboVehicle.Location = new Point(controlX, y);
            cboVehicle.Size = new Size(controlWidth, 30);
            this.Controls.Add(cboVehicle);
            y += spacing;

            // Fuel Date
            var lblFuelDate = ControlFactory.CreateLabel("📅 Date:");
            lblFuelDate.Location = new Point(labelX, y);
            this.Controls.Add(lblFuelDate);
            dtpFuelDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpFuelDate);
            y += spacing;

            // Fuel Amount
            var lblFuelAmount = ControlFactory.CreateLabel("⛽ Gallons:");
            lblFuelAmount.Location = new Point(labelX, y);
            this.Controls.Add(lblFuelAmount);
            txtFuelAmount = ControlFactory.CreateTextBox("Enter gallons (e.g., 15.50)");
            txtFuelAmount.Location = new Point(controlX, y);
            txtFuelAmount.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtFuelAmount);
            y += spacing;

            // Fuel Cost
            var lblFuelCost = ControlFactory.CreateLabel("💰 Total Cost:");
            lblFuelCost.Location = new Point(labelX, y);
            this.Controls.Add(lblFuelCost);
            txtFuelCost = ControlFactory.CreateTextBox("Enter cost (e.g., 45.75)");
            txtFuelCost.Location = new Point(controlX, y);
            txtFuelCost.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtFuelCost);
            y += spacing;

            // Notes
            var lblNotes = ControlFactory.CreateLabel("📝 Notes:");
            lblNotes.Location = new Point(labelX, y);
            this.Controls.Add(lblNotes);
            txtNotes = ControlFactory.CreateTextBox("Optional notes", true);
            txtNotes.Location = new Point(controlX, y);
            txtNotes.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtNotes);
            y += 80;

            // Buttons
            btnSave = ControlFactory.CreatePrimaryButton("💾 Save", btnSave_Click);
            btnSave.Location = new Point(controlX, y);
            btnSave.BackColor = BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme);
            btnSave.Size = new Size(120, 36);
            this.Controls.Add(btnSave);

            btnCancel = ControlFactory.CreateSecondaryButton("❌ Cancel", btnCancel_Click);
            btnCancel.Location = new Point(controlX + 130, y);
            btnCancel.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            btnCancel.Size = new Size(120, 36);

            // Apply Syncfusion Material Design styling
            ApplySyncfusionStyling();
        }

        private void ApplySyncfusionStyling()
        {
            // Apply Syncfusion Material theme to the form
            BusBuddyThemeManager.ApplyTheme(this, BusBuddyThemeManager.SupportedThemes.Office2016White);

            // Configure date picker for Material Design
            dtpFuelDate.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            dtpFuelDate.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);

            // Style all labels
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Segoe UI", 9, FontStyle.Regular);
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
            comboBox.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            this.Controls.Add(comboBox);
            return comboBox;
        }

        private void SetPlaceholderText(Control textBox, string placeholder)
        {
            if (textBox is TextBox tb)
            {
                tb.Text = placeholder;
                tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Text = "";
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
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
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            this.Controls.Add(button);
            return button;
        }        // Validation methods
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
