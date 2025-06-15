using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public partial class VehicleForm : StandardDataForm
    {
        public Vehicle Vehicle { get; private set; }

        // UI Controls
        private MaterialTextBox txtBusNumber;
        private NumericUpDown numYear;
        private MaterialTextBox txtMake;
        private MaterialTextBox txtModel;
        private NumericUpDown numSeatingCapacity;
        private MaterialTextBox txtVINNumber;
        private MaterialTextBox txtLicenseNumber;
        private DateTimePicker dtpDateLastInspection;
        private ComboBox cboFuelType;
        private ComboBox cboStatus;
        private MaterialTextBox txtNotes;
        private MaterialButton btnSave;
        private MaterialButton btnCancel;

        public VehicleForm(Vehicle? vehicle = null)
        {
            InitializeComponent();
            Vehicle = vehicle != null ? new Vehicle
            {
                VehicleID = vehicle.VehicleID,
                BusNumber = vehicle.BusNumber,
                VehicleNumber = vehicle.VehicleNumber,
                Year = vehicle.Year,
                Make = vehicle.Make,
                Model = vehicle.Model,
                SeatingCapacity = vehicle.SeatingCapacity,
                VINNumber = vehicle.VINNumber,
                LicenseNumber = vehicle.LicenseNumber,
                DateLastInspection = vehicle.DateLastInspection,
                FuelType = vehicle.FuelType,
                Status = vehicle.Status,
                Notes = vehicle.Notes
            } : new Vehicle();
            LoadVehicleData();
        }

        private void InitializeComponent()
        {
            this.txtBusNumber = CreateTextBox(130, 20, 200);
            this.numYear = new NumericUpDown();
            this.txtMake = CreateTextBox(130, 90, 200);
            this.txtModel = CreateTextBox(130, 125, 200);
            this.numSeatingCapacity = new NumericUpDown();
            this.txtVINNumber = CreateTextBox(130, 195, 200);
            this.txtLicenseNumber = CreateTextBox(130, 230, 200);
            this.dtpDateLastInspection = new DateTimePicker();
            this.cboFuelType = new ComboBox();
            this.cboStatus = new ComboBox();
            this.txtNotes = CreateTextBox(130, 335, 200);
            this.btnSave = CreateButton("💾 Save", 130, 380, btnSave_Click);
            this.btnCancel = CreateButton("❌ Cancel", 240, 380, btnCancel_Click);

            // Form properties - Enhanced with KillerSampleForm styling
            this.Text = "🚌 Vehicle Details";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);

            SetupFormLayout();
        }

        private void SetupFormLayout()
        {
            // Layout controls
            int y = 20;
            int labelX = 20;
            int controlX = 130;
            int spacing = 35;

            // Bus Number
            var lblBusNumber = new Label { Text = "🚌 Bus Number:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtBusNumber.Location = new Point(controlX, y);
            txtBusNumber.Size = new Size(200, 23);
            this.Controls.Add(lblBusNumber);
            this.Controls.Add(txtBusNumber);
            y += spacing;

            // Year
            var lblYear = new Label { Text = "📅 Year:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numYear.Location = new Point(controlX, y);
            numYear.Size = new Size(200, 23);
            numYear.Minimum = 1990;
            numYear.Maximum = DateTime.Now.Year + 2;
            numYear.Value = DateTime.Now.Year;
            this.Controls.Add(lblYear);
            this.Controls.Add(numYear);
            y += spacing;

            // Make
            var lblMake = new Label { Text = "🏭 Make:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtMake.Location = new Point(controlX, y);
            txtMake.Size = new Size(200, 23);
            this.Controls.Add(lblMake);
            this.Controls.Add(txtMake);
            y += spacing;

            // Model
            var lblModel = new Label { Text = "🚗 Model:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtModel.Location = new Point(controlX, y);
            txtModel.Size = new Size(200, 23);
            this.Controls.Add(lblModel);
            this.Controls.Add(txtModel);
            y += spacing;

            // Seating Capacity
            var lblCapacity = new Label { Text = "👥 Capacity:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            numSeatingCapacity.Location = new Point(controlX, y);
            numSeatingCapacity.Size = new Size(200, 23);
            numSeatingCapacity.Minimum = 1;
            numSeatingCapacity.Maximum = 100;
            this.Controls.Add(lblCapacity);
            this.Controls.Add(numSeatingCapacity);
            y += spacing;

            // VIN Number
            var lblVIN = new Label { Text = "🔢 VIN Number:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtVINNumber.Location = new Point(controlX, y);
            txtVINNumber.Size = new Size(200, 23);
            this.Controls.Add(lblVIN);
            this.Controls.Add(txtVINNumber);
            y += spacing;

            // License Number
            var lblLicense = new Label { Text = "📄 License:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtLicenseNumber.Location = new Point(controlX, y);
            txtLicenseNumber.Size = new Size(200, 23);
            this.Controls.Add(lblLicense);
            this.Controls.Add(txtLicenseNumber);
            y += spacing;

            // Last Inspection Date
            var lblInspection = new Label { Text = "🔧 Inspection:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            dtpDateLastInspection.Location = new Point(controlX, y);
            dtpDateLastInspection.Size = new Size(200, 23);
            this.Controls.Add(lblInspection);
            this.Controls.Add(dtpDateLastInspection);
            y += spacing;

            // Fuel Type
            var lblFuelType = new Label { Text = "⛽ Fuel Type:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboFuelType.Location = new Point(controlX, y);
            cboFuelType.Size = new Size(200, 23);
            cboFuelType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFuelType.Items.AddRange(new[] { "Gasoline", "Diesel", "Electric", "Hybrid", "CNG", "Other" });
            this.Controls.Add(lblFuelType);
            this.Controls.Add(cboFuelType);
            y += spacing;

            // Status
            var lblStatus = new Label { Text = "📊 Status:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cboStatus.Location = new Point(controlX, y);
            cboStatus.Size = new Size(200, 23);
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.AddRange(new[] { "Active", "Maintenance", "Inactive", "Retired" });
            this.Controls.Add(lblStatus);
            this.Controls.Add(cboStatus);
            y += spacing;

            // Notes
            var lblNotes = new Label { Text = "📝 Notes:", Location = new Point(labelX, y), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtNotes.Location = new Point(controlX, y);
            txtNotes.Size = new Size(200, 23);
            this.Controls.Add(lblNotes);
            this.Controls.Add(txtNotes);
        }

        private void LoadVehicleData()
        {
            txtBusNumber.Text = Vehicle.BusNumber ?? Vehicle.VehicleNumber ?? string.Empty;
            numYear.Value = Vehicle.Year > 0 ? Vehicle.Year : DateTime.Now.Year;
            txtMake.Text = Vehicle.Make ?? string.Empty;
            txtModel.Text = Vehicle.Model ?? string.Empty;
            numSeatingCapacity.Value = Vehicle.SeatingCapacity > 0 ? Vehicle.SeatingCapacity : 1;
            txtVINNumber.Text = Vehicle.VINNumber ?? string.Empty;
            txtLicenseNumber.Text = Vehicle.LicenseNumber ?? string.Empty;
            dtpDateLastInspection.Value = Vehicle.DateLastInspection ?? DateTime.Now;

            // Set combo box values with defaults
            cboFuelType.SelectedItem = Vehicle.FuelType ?? "Diesel";
            cboStatus.SelectedItem = Vehicle.Status ?? "Active";
            txtNotes.Text = Vehicle.Notes ?? string.Empty;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Apply KillerSampleForm validation patterns
            if (!ValidateVehicle())
                return;

            Vehicle.BusNumber = txtBusNumber.Text.Trim();
            Vehicle.VehicleNumber = txtBusNumber.Text.Trim(); // Ensure both properties are set
            Vehicle.Year = (int)numYear.Value;
            Vehicle.Make = txtMake.Text.Trim();
            Vehicle.Model = txtModel.Text.Trim();
            Vehicle.SeatingCapacity = (int)numSeatingCapacity.Value;
            Vehicle.VINNumber = txtVINNumber.Text.Trim();
            Vehicle.LicenseNumber = txtLicenseNumber.Text.Trim();
            Vehicle.DateLastInspection = dtpDateLastInspection.Value;
            Vehicle.FuelType = cboFuelType.SelectedItem?.ToString();
            Vehicle.Status = cboStatus.SelectedItem?.ToString();
            Vehicle.Notes = txtNotes.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Enhanced validation following KillerSampleForm patterns
        /// </summary>
        private bool ValidateVehicle()
        {
            // Clear any previous error highlighting
            ResetValidationStyles();

            // Vehicle Number validation
            if (string.IsNullOrWhiteSpace(txtBusNumber.Text))
            {
                ShowValidationError(txtBusNumber, "Vehicle/Bus number is required.", "Validation Error");
                return false;
            }

            // Make validation
            if (string.IsNullOrWhiteSpace(txtMake.Text))
            {
                ShowValidationError(txtMake, "Vehicle manufacturer is required.", "Validation Error");
                return false;
            }

            // Model validation
            if (string.IsNullOrWhiteSpace(txtModel.Text))
            {
                ShowValidationError(txtModel, "Vehicle model is required.", "Validation Error");
                return false;
            }

            // Year validation
            if (numYear.Value < 1990 || numYear.Value > DateTime.Now.Year + 2)
            {
                ShowValidationError(numYear, $"Invalid year. Please enter a year between 1990 and {DateTime.Now.Year + 2}.", "Validation Error");
                return false;
            }

            // Capacity validation
            if (numSeatingCapacity.Value <= 0)
            {
                ShowValidationError(numSeatingCapacity, "Vehicle capacity must be greater than 0.", "Validation Error");
                return false;
            }

            if (numSeatingCapacity.Value > 100)
            {
                ShowValidationError(numSeatingCapacity, "Vehicle capacity seems unusually high. Please verify.", "Validation Warning");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Show validation error with enhanced UX
        /// </summary>
        private void ShowValidationError(Control control, string message, string title)
        {
            // Highlight the problematic field
            control.BackColor = Color.FromArgb(255, 240, 240);
            control.Focus();

            // Show user-friendly error message
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Reset validation styles
        /// </summary>
        private void ResetValidationStyles()
        {
            var normalBackColor = Color.White;
            txtBusNumber.BackColor = normalBackColor;
            txtMake.BackColor = normalBackColor;
            txtModel.BackColor = normalBackColor;
            txtVINNumber.BackColor = normalBackColor;
            txtLicenseNumber.BackColor = normalBackColor;
            txtNotes.BackColor = normalBackColor;
            numYear.BackColor = normalBackColor;
            numSeatingCapacity.BackColor = normalBackColor;
            cboFuelType.BackColor = normalBackColor;
            cboStatus.BackColor = normalBackColor;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
