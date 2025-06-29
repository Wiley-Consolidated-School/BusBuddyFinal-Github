using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    public partial class FuelFormSyncfusion : SyncfusionBaseForm
    {
        private ComboBoxAdv _busComboBox;
        private SfDateTimeEdit _fillupDatePicker;
        private SfNumericTextBox _gallonsNumericTextBox;
        private SfNumericTextBox _costNumericTextBox;
        private SfNumericTextBox _odometerNumericTextBox;
        private ComboBoxAdv _fuelTypeComboBox;
        private TextBoxExt _locationTextBox;
        private TextBoxExt _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;
        public Fuel Fuel { get; private set; }

        public FuelFormSyncfusion(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Fuel = new Fuel();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = Fuel.FuelID == 0 ? "Add Fuel Record" : "Edit Fuel Record";
            this.ClientSize = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            var vehicleLabel = new Label { Text = "Bus:", Location = new Point(20, 25), Size = new Size(100, 20) };
            var fillupDateLabel = new Label { Text = "Fillup Date:", Location = new Point(300, 25), Size = new Size(100, 20) };
            var gallonsLabel = new Label { Text = "Gallons:", Location = new Point(20, 95), Size = new Size(100, 20) };
            var costLabel = new Label { Text = "Total Cost:", Location = new Point(300, 95), Size = new Size(100, 20) };
            var odometerLabel = new Label { Text = "Odometer:", Location = new Point(20, 165), Size = new Size(100, 20) };
            var fuelTypeLabel = new Label { Text = "Fuel Type:", Location = new Point(300, 165), Size = new Size(100, 20) };
            var locationLabel = new Label { Text = "Location:", Location = new Point(20, 235), Size = new Size(100, 20) };
            var notesLabel = new Label { Text = "Notes:", Location = new Point(20, 305), Size = new Size(100, 20) };
            _mainPanel.Controls.AddRange(new Control[] {
                vehicleLabel, fillupDateLabel, gallonsLabel, costLabel,
                odometerLabel, fuelTypeLabel, locationLabel, notesLabel
            });
            _busComboBox = new ComboBoxAdv { Location = new Point(20, 50), Size = new Size(250, 25) };
            _fillupDatePicker = new SfDateTimeEdit { Location = new Point(300, 50), Size = new Size(250, 25), Value = DateTime.Now };
            _gallonsNumericTextBox = new SfNumericTextBox { Location = new Point(20, 120), Size = new Size(250, 25), Value = 0 };
            _costNumericTextBox = new SfNumericTextBox { Location = new Point(300, 120), Size = new Size(250, 25), Value = 0 };
            _odometerNumericTextBox = new SfNumericTextBox { Location = new Point(20, 190), Size = new Size(250, 25), Value = 0 };
            _fuelTypeComboBox = new ComboBoxAdv { Location = new Point(300, 190), Size = new Size(250, 25) };
            _fuelTypeComboBox.Items.AddRange(new[] { "Diesel", "Gasoline", "CNG", "Electric" });
            _locationTextBox = new TextBoxExt { Location = new Point(20, 260), Size = new Size(530, 25) };
            _notesTextBox = new TextBoxExt { Location = new Point(20, 330), Size = new Size(530, 100), Multiline = true };
            _mainPanel.Controls.AddRange(new Control[] {
                _busComboBox, _fillupDatePicker, _gallonsNumericTextBox, _costNumericTextBox,
                _odometerNumericTextBox, _fuelTypeComboBox, _locationTextBox, _notesTextBox
            });
        }

        private void LayoutControls()
        {
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
            _saveButton = new SfButton { Text = "Save", Location = new Point(375, 15), Size = new Size(80, 30) };
            _cancelButton = new SfButton { Text = "Cancel", Location = new Point(470, 15), Size = new Size(80, 30) };
            buttonPanel.Controls.AddRange(new Control[] { _saveButton, _cancelButton });
            this.Controls.Add(buttonPanel);
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        private void PopulateFields(Fuel fuel)
        {
            _busComboBox.Text = fuel.VehicleFueledID?.ToString() ?? "";
            _fillupDatePicker.Value = fuel.FuelDateAsDateTime ?? DateTime.Now;
            _gallonsNumericTextBox.Value = (double?)(fuel.FuelAmount ?? 0);
            _costNumericTextBox.Value = (double?)(fuel.FuelCost ?? 0);
            _odometerNumericTextBox.Value = (double?)(fuel.VehicleOdometerReading ?? 0);
            _fuelTypeComboBox.Text = fuel.FuelType ?? "Diesel";
            _locationTextBox.Text = fuel.FuelLocation ?? "";
            _notesTextBox.Text = fuel.Notes ?? "";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Parse VehicleFueledID from combo box text if it's numeric
                if (int.TryParse(_busComboBox.Text, out int BusId))
                    Fuel.VehicleFueledID = BusId;
                Fuel.FuelDateAsDateTime = _fillupDatePicker.Value;
                Fuel.FuelAmount = (decimal?)_gallonsNumericTextBox.Value;
                Fuel.FuelCost = (decimal?)_costNumericTextBox.Value;
                Fuel.VehicleOdometerReading = (decimal?)_odometerNumericTextBox.Value;
                Fuel.FuelType = _fuelTypeComboBox.Text;
                Fuel.FuelLocation = _locationTextBox.Text;
                Fuel.Notes = _notesTextBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving fuel record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

