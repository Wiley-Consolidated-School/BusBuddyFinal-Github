using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// FuelManagementForm provides a CRUD interface for managing fuel records, including vehicle selection, date, location, odometer, and fuel type.
    /// UI/UX: CRUD panel is compact, moved up, date field uses short format, vehicle dropdown is populated from Vehicles table.
    /// Data: Uses IFuelRepository and IVehicleRepository for data access.
    ///
    /// Last updated: June 12, 2025
    /// - Modernized UI/UX for CRUD panel
    /// - Vehicle dropdown now populated from Vehicles table
    /// - Date field uses short format
    /// - Uses Dapper/SQLite for test infrastructure (see BusBuddy.Tests)
    ///
    /// To update schema/data for tests, see BusBuddy.Data/DatabaseScript.sql and BusBuddy.Tests/TestBase.cs
    ///
    /// Author: BusBuddy Modernization
    /// </summary>
    public class FuelManagementForm : BaseDataForm
    {
        private readonly IFuelRepository _fuelRepository;
        private readonly IVehicleRepository _vehicleRepository = new VehicleRepository();
        private DataGridView _fuelGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private List<Fuel> _fuels;
        private List<Vehicle> _vehicles;
        // Add/edit fields
        private Panel _editPanel;
        private DateTimePicker _fuelDatePicker;
        private ComboBox _fuelLocationComboBox;
        private ComboBox _vehicleComboBox;
        private TextBox _odometerTextBox;
        private ComboBox _fuelTypeComboBox;
        private Button _saveButton;
        private Button _cancelButton;
        private Fuel _currentFuel;
        private bool _isEditing = false;

        public FuelManagementForm() : this(new FuelRepository()) { }

        public FuelManagementForm(IFuelRepository fuelRepository)
        {
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            InitializeComponent();
            LoadFuels();
        }

        private void InitializeComponent()
        {
            this.Text = "Fuel Management";
            this.Size = new System.Drawing.Size(1200, 900);
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewFuel());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedFuel());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedFuel());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewFuelDetails());
            _fuelGrid = new DataGridView();
            _fuelGrid.Location = new System.Drawing.Point(20, 60);
            _fuelGrid.Size = new System.Drawing.Size(1150, 650);
            _fuelGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _fuelGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _fuelGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _fuelGrid.AllowUserToResizeColumns = true;
            _fuelGrid.AllowUserToResizeRows = true;
            _fuelGrid.ScrollBars = ScrollBars.Both;
            _fuelGrid.DataBindingComplete += (s, e) => {
                if (_fuelGrid.Columns.Contains("FuelID"))
                    _fuelGrid.Columns["FuelID"].Visible = false;
            };
            this.Controls.Add(_fuelGrid);
            _fuelGrid.CellDoubleClick += (s, e) => EditSelectedFuel();
            _fuelGrid.SelectionChanged += FuelGrid_SelectionChanged;
            InitializeEditPanel();
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 690); // Move up
            _editPanel.Size = new System.Drawing.Size(1150, 90); // Make shorter
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);
            var dateLabel = CreateLabel("Fuel Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _fuelDatePicker = new DateTimePicker();
            _fuelDatePicker.Location = new System.Drawing.Point(90, 10);
            _fuelDatePicker.Size = new System.Drawing.Size(150, 23);
            _fuelDatePicker.Format = DateTimePickerFormat.Short; // Ensure short format
            _editPanel.Controls.Add(_fuelDatePicker);
            var locationLabel = CreateLabel("Location:", 260, 15);
            _editPanel.Controls.Add(locationLabel);
            _fuelLocationComboBox = new ComboBox();
            _fuelLocationComboBox.Location = new System.Drawing.Point(330, 10);
            _fuelLocationComboBox.Size = new System.Drawing.Size(120, 23);
            _fuelLocationComboBox.Items.AddRange(new object[] { "Key Pumps", "Gas Station" });
            _editPanel.Controls.Add(_fuelLocationComboBox);
            var vehicleLabel = CreateLabel("Vehicle:", 480, 15);
            _editPanel.Controls.Add(vehicleLabel);
            _vehicleComboBox = new ComboBox();
            _vehicleComboBox.Location = new System.Drawing.Point(540, 10);
            _vehicleComboBox.Size = new System.Drawing.Size(150, 23);
            _vehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _vehicleComboBox.DisplayMember = "VehicleNumber";
            _vehicleComboBox.ValueMember = "Id";
            _editPanel.Controls.Add(_vehicleComboBox);
            var odoLabel = CreateLabel("Odometer:", 710, 15);
            _editPanel.Controls.Add(odoLabel);
            _odometerTextBox = new TextBox();
            _odometerTextBox.Location = new System.Drawing.Point(790, 10);
            _odometerTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_odometerTextBox);
            var fuelTypeLabel = CreateLabel("Fuel Type:", 910, 15);
            _editPanel.Controls.Add(fuelTypeLabel);
            _fuelTypeComboBox = new ComboBox();
            _fuelTypeComboBox.Location = new System.Drawing.Point(990, 10);
            _fuelTypeComboBox.Size = new System.Drawing.Size(100, 23);
            _fuelTypeComboBox.Items.AddRange(new object[] { "Gasoline", "Diesel" });
            _editPanel.Controls.Add(_fuelTypeComboBox);
            _saveButton = CreateButton("Save", 800, 60, (s, e) => SaveFuel());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 60, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void LoadVehiclesForDropdown()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                _vehicleComboBox.DataSource = null;
                _vehicleComboBox.DataSource = _vehicles;
                _vehicleComboBox.DisplayMember = "VehicleNumber";
                _vehicleComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
                _vehicles = new List<Vehicle>();
                _vehicleComboBox.DataSource = null;
            }
        }

        private void LoadFuels()
        {
            LoadVehiclesForDropdown(); // Ensure vehicles are loaded for dropdown
            try
            {
                _fuels = _fuelRepository.GetAllFuelRecords();
                _fuelGrid.DataSource = null;
                _fuelGrid.DataSource = _fuels;
                if (_fuelGrid.Columns.Count > 0)
                {
                    _fuelGrid.Columns["FuelID"].HeaderText = "ID";
                    _fuelGrid.Columns["FuelDate"].HeaderText = "Date";
                    _fuelGrid.Columns["FuelLocation"].HeaderText = "Location";
                    _fuelGrid.Columns["VehicleFueledID"].HeaderText = "Vehicle";
                    _fuelGrid.Columns["VehicleOdometerReading"].HeaderText = "Odometer";
                    _fuelGrid.Columns["FuelType"].HeaderText = "Type";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading fuel records: {ex.Message}");
            }
        }

        private void AddNewFuel()
        {
            _isEditing = false;
            _currentFuel = new Fuel();
            _fuelDatePicker.Value = DateTime.Today;
            _fuelLocationComboBox.SelectedIndex = -1;
            LoadVehiclesForDropdown();
            _vehicleComboBox.SelectedIndex = -1;
            _odometerTextBox.Text = string.Empty;
            _fuelTypeComboBox.SelectedIndex = -1;
            _editPanel.Visible = true;
        }

        private void EditSelectedFuel()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
                return;
            _isEditing = true;
            int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
            _currentFuel = _fuelRepository.GetFuelRecordById(selectedId);
            if (_currentFuel == null)
            {
                ShowErrorMessage("Could not find the selected fuel record.");
                return;
            }
            _fuelDatePicker.Value = _currentFuel.FuelDate ?? DateTime.Today;
            _fuelLocationComboBox.SelectedItem = _currentFuel.FuelLocation ?? string.Empty;
            LoadVehiclesForDropdown();
            if (_currentFuel.VehicleFueledID.HasValue)
                _vehicleComboBox.SelectedValue = _currentFuel.VehicleFueledID.Value;
            else
                _vehicleComboBox.SelectedIndex = -1;
            _odometerTextBox.Text = _currentFuel.VehicleOdometerReading?.ToString() ?? string.Empty;
            _fuelTypeComboBox.SelectedItem = _currentFuel.FuelType ?? string.Empty;
            _editPanel.Visible = true;
        }

        private void DeleteSelectedFuel()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _fuelRepository.DeleteFuelRecord(selectedId);
                LoadFuels();
                ShowSuccessMessage("Fuel record deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting fuel record: {ex.Message}");
            }
        }

        private void ViewFuelDetails()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
            var fuel = _fuelRepository.GetFuelRecordById(selectedId);
            if (fuel != null)
            {
                MessageBox.Show($"Fuel Details:\nDate: {fuel.FuelDate}\nLocation: {fuel.FuelLocation}\nVehicle: {fuel.VehicleFueledID}\nOdometer: {fuel.VehicleOdometerReading}\nType: {fuel.FuelType}",
                    "Fuel Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load fuel details.");
            }
        }

        private void SaveFuel()
        {
            if (!ValidateFuelForm())
                return;            try
            {
                _currentFuel.FuelDate = _fuelDatePicker.Value;
                _currentFuel.FuelLocation = _fuelLocationComboBox.SelectedItem?.ToString() ?? string.Empty;
                _currentFuel.VehicleFueledID = _vehicleComboBox.SelectedValue is int vid ? vid : (int?)null;
                _currentFuel.VehicleOdometerReading = decimal.TryParse(_odometerTextBox.Text.Trim(), out decimal odo) ? odo : (decimal?)null;
                _currentFuel.FuelType = _fuelTypeComboBox.SelectedItem?.ToString();
                if (_isEditing)
                {
                    _fuelRepository.UpdateFuelRecord(_currentFuel);
                    ShowSuccessMessage("Fuel record updated successfully.");
                }
                else
                {
                    _fuelRepository.AddFuelRecord(_currentFuel);
                    ShowSuccessMessage("Fuel record added successfully.");
                }
                LoadFuels();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving fuel record: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void FuelGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _fuelGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private bool ValidateFuelForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (_fuelLocationComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_fuelLocationComboBox, "Select a location");
                valid = false;
            }
            if (_vehicleComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_vehicleComboBox, "Select a vehicle");
                valid = false;
            }
            if (_fuelTypeComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_fuelTypeComboBox, "Select a fuel type");
                valid = false;
            }
            return valid;
        }
    }
}
