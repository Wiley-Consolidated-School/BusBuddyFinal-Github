using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class FuelEditForm : StandardDataForm
    {
        public Fuel Fuel { get; private set; }

        private MaterialComboBox cboVehicle;
        private DateTimePicker? dtpFuelDate;
        private MaterialTextBox txtFuelAmount;
        private MaterialTextBox txtFuelCost;
        private MaterialTextBox txtNotes;
        private MaterialButton btnSave;
        private MaterialButton btnCancel;

        public FuelEditForm(Fuel? fuel = null)
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
            txtFuelAmount.Hint = "Enter gallons (e.g., 15.50)";
            y += spacing;

            // Fuel Cost
            var lblFuelCost = CreateLabel("💰 Total Cost:", labelX, y);
            txtFuelCost = CreateTextBox(controlX, y, controlWidth);
            txtFuelCost.Hint = "Enter cost (e.g., 45.75)";
            y += spacing;

            // Notes
            var lblNotes = CreateLabel("📝 Notes:", labelX, y);
            txtNotes = CreateTextBox(controlX, y, controlWidth);
            txtNotes.Hint = "Optional notes";
            txtNotes.Multiline = true;
            txtNotes.Height = 60;
            y += 80;

            // Buttons
            btnSave = CreateButton("💾 Save", controlX, y, btnSave_Click);
            btnSave.UseAccentColor = true;
            btnSave.Type = MaterialButton.MaterialButtonType.Contained;
            btnSave.Size = new Size(120, 36);

            btnCancel = CreateButton("❌ Cancel", controlX + 130, y, btnCancel_Click);
            btnCancel.Type = MaterialButton.MaterialButtonType.Outlined;
            btnCancel.Size = new Size(120, 36);

            // Apply Material Design styling
            ApplyMaterialStyling();
        }

        private void ApplyMaterialStyling()
        {
            // Configure date picker for Material Design
            dtpFuelDate.Font = new Font("Roboto", 10F);
            dtpFuelDate.BackColor = Color.White;

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
    }
}
