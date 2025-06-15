using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI;

namespace BusBuddy
{
    public partial class VehicleForm : Form
    {
        public Vehicle Vehicle { get; private set; }
        private ErrorProvider _errorProvider;

        // UI Controls
        private TextBox txtBusNumber;
        private NumericUpDown numYear;
        private TextBox txtMake;
        private TextBox txtModel;
        private NumericUpDown numSeatingCapacity;
        private TextBox txtVINNumber;
        private TextBox txtLicenseNumber;
        private DateTimePicker dtpDateLastInspection;
        private Button btnSave;
        private Button btnCancel;        public VehicleForm(Vehicle? vehicle = null)
        {
            _errorProvider = new ErrorProvider();
            InitializeComponent();
            Vehicle = vehicle != null ? new Vehicle
            {
                VehicleID = vehicle.VehicleID,
                BusNumber = vehicle.BusNumber,
                Year = vehicle.Year,
                Make = vehicle.Make,
                Model = vehicle.Model,
                SeatingCapacity = vehicle.SeatingCapacity,
                VINNumber = vehicle.VINNumber,
                LicenseNumber = vehicle.LicenseNumber,
                DateLastInspection = vehicle.DateLastInspection
            } : new Vehicle();
            LoadVehicleData();
        }

        private void InitializeComponent()
        {
            this.txtBusNumber = new TextBox();
            this.numYear = new NumericUpDown();
            this.txtMake = new TextBox();
            this.txtModel = new TextBox();
            this.numSeatingCapacity = new NumericUpDown();
            this.txtVINNumber = new TextBox();
            this.txtLicenseNumber = new TextBox();
            this.dtpDateLastInspection = new DateTimePicker();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            // Form properties
            this.Text = "Vehicle Details";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Layout controls
            int y = 20;
            int labelX = 20;
            int controlX = 130;
            int spacing = 35;

            // Bus Number
            var lblBusNumber = new Label { Text = "Bus Number:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            txtBusNumber.Location = new Point(controlX, y);
            txtBusNumber.Size = new Size(200, 23);
            this.Controls.Add(lblBusNumber);
            this.Controls.Add(txtBusNumber);
            y += spacing;

            // Year
            var lblYear = new Label { Text = "Year:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            numYear.Location = new Point(controlX, y);
            numYear.Size = new Size(200, 23);
            numYear.Minimum = 1900;
            numYear.Maximum = 2100;
            this.Controls.Add(lblYear);
            this.Controls.Add(numYear);
            y += spacing;

            // Make
            var lblMake = new Label { Text = "Make:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            txtMake.Location = new Point(controlX, y);
            txtMake.Size = new Size(200, 23);
            this.Controls.Add(lblMake);
            this.Controls.Add(txtMake);
            y += spacing;

            // Model
            var lblModel = new Label { Text = "Model:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            txtModel.Location = new Point(controlX, y);
            txtModel.Size = new Size(200, 23);
            this.Controls.Add(lblModel);
            this.Controls.Add(txtModel);
            y += spacing;

            // Seating Capacity
            var lblCapacity = new Label { Text = "Seating Capacity:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            numSeatingCapacity.Location = new Point(controlX, y);
            numSeatingCapacity.Size = new Size(200, 23);
            numSeatingCapacity.Minimum = 0;
            numSeatingCapacity.Maximum = 1000;
            this.Controls.Add(lblCapacity);
            this.Controls.Add(numSeatingCapacity);
            y += spacing;

            // VIN Number
            var lblVIN = new Label { Text = "VIN Number:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            txtVINNumber.Location = new Point(controlX, y);
            txtVINNumber.Size = new Size(200, 23);
            this.Controls.Add(lblVIN);
            this.Controls.Add(txtVINNumber);
            y += spacing;

            // License Number
            var lblLicense = new Label { Text = "License Number:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            txtLicenseNumber.Location = new Point(controlX, y);
            txtLicenseNumber.Size = new Size(200, 23);
            this.Controls.Add(lblLicense);
            this.Controls.Add(txtLicenseNumber);
            y += spacing;

            // Last Inspection Date
            var lblInspection = new Label { Text = "Last Inspection:", Location = new Point(labelX, y), Size = new Size(100, 23) };
            dtpDateLastInspection.Location = new Point(controlX, y);
            dtpDateLastInspection.Size = new Size(200, 23);
            this.Controls.Add(lblInspection);
            this.Controls.Add(dtpDateLastInspection);
            y += spacing + 20;

            // Buttons
            btnSave.Text = "Save";
            btnSave.Location = new Point(175, y);
            btnSave.Size = new Size(75, 30);
            btnSave.Click += btnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(255, y);
            btnCancel.Size = new Size(75, 30);
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnCancel);
        }
        private void LoadVehicleData()
        {
            txtBusNumber.Text = Vehicle.BusNumber ?? string.Empty;
            numYear.Value = Vehicle.Year;
            txtMake.Text = Vehicle.Make ?? string.Empty;
            txtModel.Text = Vehicle.Model ?? string.Empty;
            numSeatingCapacity.Value = Vehicle.SeatingCapacity;
            txtVINNumber.Text = Vehicle.VINNumber ?? string.Empty;
            txtLicenseNumber.Text = Vehicle.LicenseNumber ?? string.Empty;
            dtpDateLastInspection.Value = Vehicle.DateLastInspection ?? DateTime.Now;
        }        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            Vehicle.BusNumber = txtBusNumber.Text;
            Vehicle.Year = (int)numYear.Value;
            Vehicle.Make = txtMake.Text;
            Vehicle.Model = txtModel.Text;
            Vehicle.SeatingCapacity = (int)numSeatingCapacity.Value;
            Vehicle.VINNumber = txtVINNumber.Text;
            Vehicle.LicenseNumber = txtLicenseNumber.Text;
            Vehicle.DateLastInspection = dtpDateLastInspection.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateForm()
        {
            _errorProvider.Clear();
            bool valid = true;

            // Validate required fields
            valid &= FormValidator.ValidateRequiredField(txtBusNumber, "Bus Number", _errorProvider);
            valid &= FormValidator.ValidateRequiredField(txtMake, "Make", _errorProvider);
            valid &= FormValidator.ValidateRequiredField(txtModel, "Model", _errorProvider);

            // Validate VIN number format
            valid &= FormValidator.ValidateVINNumber(txtVINNumber, "VIN Number", _errorProvider);

            // Validate year range
            if (numYear.Value < 1980 || numYear.Value > DateTime.Now.Year + 2)
            {
                _errorProvider.SetError(numYear, $"Year must be between 1980 and {DateTime.Now.Year + 2}");
                valid = false;
            }

            // Validate seating capacity range
            if (numSeatingCapacity.Value < 1 || numSeatingCapacity.Value > 150)
            {
                _errorProvider.SetError(numSeatingCapacity, "Seating capacity must be between 1 and 150");
                valid = false;
            }

            // Validate inspection date is not in the future beyond 1 year
            if (dtpDateLastInspection.Value > DateTime.Now.AddYears(1))
            {
                _errorProvider.SetError(dtpDateLastInspection, "Inspection date cannot be more than 1 year in the future");
                valid = false;
            }

            return valid;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
