using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.UI.Views
{
    public class FuelEditFormSyncfusion : Form
    {
        public Fuel Fuel { get; set; }

        private ComboBox? cboVehicle;
        private DateTimePicker? dtpFuelDate;
        private TextBox? txtFuelAmount;
        private TextBox? txtFuelCost;
        private TextBox? txtNotes;
        private Button? btnSave;
        private Button? btnCancel;
        private ErrorProvider _errorProvider = new ErrorProvider();

        public FuelEditFormSyncfusion(System.IServiceProvider serviceProvider) : base()
        {
            Fuel = new Fuel();
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
            var lblBus = ControlFactory.CreateLabel("🚌 Vehicle:", labelX, y, 110, 30);
            this.Controls.Add(lblBus);
            cboVehicle = ControlFactory.CreateComboBox(Array.Empty<string>(), controlX, y, controlWidth, 30);
            this.Controls.Add(cboVehicle);
            y += spacing;
            var lblFuelDate = ControlFactory.CreateLabel("📅 Date:", labelX, y, 110, 30);
            this.Controls.Add(lblFuelDate);
            dtpFuelDate = ControlFactory.CreateDateTimePicker(controlX, y, controlWidth, 30);
            this.Controls.Add(dtpFuelDate);
            y += spacing;
            var lblFuelAmount = ControlFactory.CreateLabel("⛽ Gallons:", labelX, y, 110, 30);
            this.Controls.Add(lblFuelAmount);
            txtFuelAmount = ControlFactory.CreateTextBox("", controlX, y, controlWidth, 30);
            this.Controls.Add(txtFuelAmount);
            y += spacing;
            var lblFuelCost = ControlFactory.CreateLabel("💰 Total Cost:", labelX, y, 110, 30);
            this.Controls.Add(lblFuelCost);
            txtFuelCost = ControlFactory.CreateTextBox("", controlX, y, controlWidth, 30);
            this.Controls.Add(txtFuelCost);
            y += spacing;
            var lblNotes = ControlFactory.CreateLabel("📝 Notes:", labelX, y, 110, 30);
            this.Controls.Add(lblNotes);
            txtNotes = ControlFactory.CreateTextBox("", controlX, y, controlWidth, 60);
            txtNotes.Multiline = true;
            txtNotes.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(txtNotes);
            y += 80;
            btnSave = ControlFactory.CreatePrimaryButton("💾 Save", controlX, y, 120, 36);
            btnSave.Click += btnSave_Click;
            this.Controls.Add(btnSave);
            btnCancel = ControlFactory.CreateSecondaryButton("❌ Cancel", controlX + 130, y, 120, 36);
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private void LoadVehicles()
        {
            try
            {
                cboVehicle.Items.Clear();
                cboVehicle.Items.Add("Vehicle 1 - Bus #001");
                cboVehicle.Items.Add("Vehicle 2 - Bus #002");
                cboVehicle.Items.Add("Vehicle 3 - Bus #003");
                cboVehicle.Items.Add("Vehicle 4 - Bus #004");
                cboVehicle.Items.Add("Vehicle 5 - Bus #005");
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

        protected virtual void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        protected virtual void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        protected void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

