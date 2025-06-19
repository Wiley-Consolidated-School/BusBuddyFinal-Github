using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Theme;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Views
{
    public partial class VehicleFormSyncfusion : SyncfusionBaseForm
    {
        private TextBoxExt _vehicleNumberTextBox;
        private TextBoxExt _makeTextBox;
        private TextBoxExt _modelTextBox;
        private SfNumericTextBox _yearNumericTextBox;
        private SfNumericTextBox _capacityNumericTextBox;
        private ComboBoxAdv _fuelTypeComboBox;
        private ComboBoxAdv _statusComboBox;
        private TextBoxExt _vinNumberTextBox;
        private TextBoxExt _licenseNumberTextBox;
        private SfDateTimeEdit _lastInspectionDatePicker;
        private TextBoxExt _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public Vehicle Vehicle { get; private set; }

        public VehicleFormSyncfusion() : this(new Vehicle())
        {
        }

        public VehicleFormSyncfusion(Vehicle vehicle)
        {
            Vehicle = vehicle;
            InitializeComponent();
            if (vehicle != null)
            {
                PopulateFields(vehicle);
            }
        }

        private void InitializeComponent()
        {
            this.Text = Vehicle.Id == 0 ? "Add Vehicle" : "Edit Vehicle";
            this.ClientSize = new Size(600, 750);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Create labels and place them directly
            var vehicleNumberLabel = ControlFactory.CreateLabel("Vehicle Number:");
            vehicleNumberLabel.Location = new Point(20, 25);
            _mainPanel.Controls.Add(vehicleNumberLabel);

            var makeLabel = ControlFactory.CreateLabel("Make:");
            makeLabel.Location = new Point(300, 25);
            _mainPanel.Controls.Add(makeLabel);

            var modelLabel = ControlFactory.CreateLabel("Model:");
            modelLabel.Location = new Point(20, 95);
            _mainPanel.Controls.Add(modelLabel);

            var yearLabel = ControlFactory.CreateLabel("Year:");
            yearLabel.Location = new Point(300, 95);
            _mainPanel.Controls.Add(yearLabel);

            var capacityLabel = ControlFactory.CreateLabel("Capacity:");
            capacityLabel.Location = new Point(20, 165);
            _mainPanel.Controls.Add(capacityLabel);

            var fuelTypeLabel = ControlFactory.CreateLabel("Fuel Type:");
            fuelTypeLabel.Location = new Point(300, 165);
            _mainPanel.Controls.Add(fuelTypeLabel);

            var vinNumberLabel = ControlFactory.CreateLabel("VIN Number:");
            vinNumberLabel.Location = new Point(20, 235);
            _mainPanel.Controls.Add(vinNumberLabel);

            var licenseNumberLabel = ControlFactory.CreateLabel("License Number:");
            licenseNumberLabel.Location = new Point(300, 235);
            _mainPanel.Controls.Add(licenseNumberLabel);

            var statusLabel = ControlFactory.CreateLabel("Status:");
            statusLabel.Location = new Point(20, 305);
            _mainPanel.Controls.Add(statusLabel);

            var lastInspectionLabel = ControlFactory.CreateLabel("Last Inspection:");
            lastInspectionLabel.Location = new Point(300, 305);
            _mainPanel.Controls.Add(lastInspectionLabel);

            var notesLabel = ControlFactory.CreateLabel("Notes:");
            notesLabel.Location = new Point(20, 375);
            _mainPanel.Controls.Add(notesLabel);

            // Text boxes
            _vehicleNumberTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter vehicle number");
            _makeTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter make");
            _modelTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter model");
            _vinNumberTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter VIN");
            _licenseNumberTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter license plate");
            _notesTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter notes", multiline: true);

            // Numeric Text boxes
            _yearNumericTextBox = ControlFactory.CreateNumericTextBox(1990, DateTime.Now.Year + 1);
            _capacityNumericTextBox = ControlFactory.CreateNumericTextBox(1, 100);

            // Combo boxes
            _fuelTypeComboBox = ControlFactory.CreateComboBox();
            _statusComboBox = ControlFactory.CreateStatusComboBox();

            // Date picker
            _lastInspectionDatePicker = ControlFactory.CreateDateTimePicker();

            // Buttons
            _saveButton = ControlFactory.CreatePrimaryButton("Save");
            _cancelButton = ControlFactory.CreateSecondaryButton("Cancel");
        }

        private void LayoutControls()
        {
            // Set locations
            _vehicleNumberTextBox.Location = new Point(20, 50);
            _makeTextBox.Location = new Point(300, 50);
            _modelTextBox.Location = new Point(20, 120);
            _yearNumericTextBox.Location = new Point(300, 120);
            _capacityNumericTextBox.Location = new Point(20, 190);
            _fuelTypeComboBox.Location = new Point(300, 190);
            _vinNumberTextBox.Location = new Point(20, 260);
            _licenseNumberTextBox.Location = new Point(300, 260);
            _statusComboBox.Location = new Point(20, 330);
            _lastInspectionDatePicker.Location = new Point(300, 330);
            _notesTextBox.Location = new Point(20, 400);
            _notesTextBox.Size = new Size(560, 100);

            _saveButton.Location = new Point(20, 650);
            _cancelButton.Location = new Point(150, 650);

            _mainPanel.Controls.AddRange(new Control[]
            {
                _vehicleNumberTextBox, _makeTextBox, _modelTextBox, _yearNumericTextBox,
                _capacityNumericTextBox, _fuelTypeComboBox, _statusComboBox, _vinNumberTextBox,
                _licenseNumberTextBox, _lastInspectionDatePicker, _notesTextBox,
                _saveButton, _cancelButton
            });

            // Setup combo box items
            _fuelTypeComboBox.DataSource = new[] { "Diesel", "Gasoline", "Electric", "Hybrid", "Propane" };
        }



        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        private void PopulateFields(Vehicle vehicle)
        {
            _vehicleNumberTextBox.Text = vehicle.VehicleNumber ?? string.Empty;
            _makeTextBox.Text = vehicle.Make ?? string.Empty;
            _modelTextBox.Text = vehicle.Model ?? string.Empty;
            _yearNumericTextBox.Value = vehicle.Year;
            _capacityNumericTextBox.Value = vehicle.Capacity;
            _fuelTypeComboBox.SelectedItem = vehicle.FuelType;
            _statusComboBox.SelectedItem = vehicle.Status;
            _vinNumberTextBox.Text = vehicle.VINNumber ?? string.Empty;
            _licenseNumberTextBox.Text = vehicle.LicenseNumber ?? string.Empty;
            _lastInspectionDatePicker.Value = vehicle.DateLastInspectionAsDateTime;
            _notesTextBox.Text = vehicle.Notes ?? string.Empty;
        }        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (ValidateChildren())
            {
                Vehicle.VehicleNumber = _vehicleNumberTextBox.Text;
                Vehicle.Make = _makeTextBox.Text;
                Vehicle.Model = _modelTextBox.Text;
                Vehicle.Year = _yearNumericTextBox.Value.HasValue ? (int)_yearNumericTextBox.Value : DateTime.Now.Year;
                Vehicle.Capacity = _capacityNumericTextBox.Value.HasValue ? (int)_capacityNumericTextBox.Value : 0;
                Vehicle.FuelType = _fuelTypeComboBox.SelectedItem?.ToString();
                Vehicle.Status = _statusComboBox.SelectedItem?.ToString();
                Vehicle.VINNumber = _vinNumberTextBox.Text;
                Vehicle.LicenseNumber = _licenseNumberTextBox.Text;
                Vehicle.DateLastInspectionAsDateTime = _lastInspectionDatePicker.Value;
                Vehicle.Notes = _notesTextBox.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
