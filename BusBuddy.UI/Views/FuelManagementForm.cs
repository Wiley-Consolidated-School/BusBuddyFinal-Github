using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;

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
    /// - Uses Dapper/SQL Server for database infrastructure (see BusBuddy.Tests)
    ///
    /// To update schema/data for tests, see BusBuddy.Data/DatabaseScript.SqlServer.sql and BusBuddy.Tests/TestBase.cs
    ///
    /// Author: BusBuddy Modernization
    /// </summary>
    public class FuelManagementForm : StandardDataForm
    {
        private readonly IFuelRepository _fuelRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _fuelGrid;
        private Button? _addButton;
        private Button? _editButton;
        private Button? _deleteButton;
        private Button? _detailsButton;
        private MaterialSkin.Controls.MaterialTextBox _searchBox;
        private Button? _searchButton;
        private List<Fuel> _fuels = new List<Fuel>();
        private List<Vehicle> _vehicles = new List<Vehicle>();
        // Add/edit fields
        private Panel _editPanel = null!;
        private DateTimePicker _fuelDatePicker = null!;
        private ComboBox _fuelLocationComboBox = null!;
        private ComboBox _vehicleComboBox = null!;
        private TextBox _odometerTextBox = null!;
        private ComboBox _fuelTypeComboBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Fuel? _currentFuel = null;
        private bool _isEditing = false;        public FuelManagementForm() : this(new FuelRepository(), new VehicleRepository()) { }

        public FuelManagementForm(IFuelRepository fuelRepository, IVehicleRepository vehicleRepository)
        {
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            InitializeComponent();
            LoadFuels();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Fuel Management"
            this.Text = "Fuel Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewFuel());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedFuel());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedFuel());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewFuelDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchFuels());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _fuelGrid = new DataGridView();
            _fuelGrid.Location = new System.Drawing.Point(20, 60);
            _fuelGrid.Size = new System.Drawing.Size(1150, 650);
            _fuelGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _fuelGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _fuelGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _fuelGrid.ReadOnly = true;
            _fuelGrid.AllowUserToAddRows = false;
            _fuelGrid.AllowUserToDeleteRows = false;
            _fuelGrid.MultiSelect = false;
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

            // Initialize edit panel (1150x120, y=730, hidden)
            InitializeEditPanel();

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            // Create edit panel (1150x120, y=730, hidden)
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 730);
            _editPanel.Size = new System.Drawing.Size(1150, 120);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);

            // Fuel form-specific fields: Date, Location, Vehicle, Odometer, Fuel Type
            var dateLabel = CreateLabel("Fuel Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _fuelDatePicker = new DateTimePicker();
            _fuelDatePicker.Location = new System.Drawing.Point(90, 10);
            _fuelDatePicker.Size = new System.Drawing.Size(150, 23);
            _fuelDatePicker.Format = DateTimePickerFormat.Short;
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

            var fuelTypeLabel = CreateLabel("Fuel Type:", 10, 55);
            _editPanel.Controls.Add(fuelTypeLabel);
            _fuelTypeComboBox = new ComboBox();
            _fuelTypeComboBox.Location = new System.Drawing.Point(90, 50);
            _fuelTypeComboBox.Size = new System.Drawing.Size(120, 23);
            _fuelTypeComboBox.Items.AddRange(new object[] { "Gasoline", "Diesel" });
            _editPanel.Controls.Add(_fuelTypeComboBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveFuel());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
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
            try
            {
                using (var addForm = new FuelEditFormSyncfusion())
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        var newFuel = addForm.Fuel;
                        var fuelId = _fuelRepository.AddFuelRecord(newFuel);
                        if (fuelId > 0)
                        {
                            LoadFuels();
                            ShowSuccessMessage("Fuel record added successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to add fuel record. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding fuel record: {ex.Message}");
            }
        }

        private void EditSelectedFuel()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
            {
                ShowWarningMessage("Please select a fuel record to edit.");
                return;
            }

            try
            {
                int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
                var selectedFuel = _fuelRepository.GetFuelRecordById(selectedId);

                if (selectedFuel == null)
                {
                    ShowErrorMessage("Could not find the selected fuel record.");
                    return;
                }

                using (var editForm = new FuelEditFormSyncfusion(selectedFuel))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        var updatedFuel = editForm.Fuel;
                        if (_fuelRepository.UpdateFuelRecord(updatedFuel))
                        {
                            LoadFuels();
                            ShowSuccessMessage("Fuel record updated successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to update fuel record. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing fuel record: {ex.Message}");
            }
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
            if (_currentFuel == null || !ValidateFuelForm())
                return;
            try
            {
                var fuel = _currentFuel; // Null-checked above
                fuel.FuelDateAsDateTime = _fuelDatePicker.Value;
                fuel.FuelLocation = _fuelLocationComboBox.SelectedItem?.ToString() ?? string.Empty;
                fuel.VehicleFueledID = _vehicleComboBox.SelectedValue is int vid ? vid : (int?)null;
                fuel.VehicleOdometerReading = decimal.TryParse(_odometerTextBox.Text.Trim(), out decimal odo) ? odo : (decimal?)null;
                fuel.FuelType = _fuelTypeComboBox.SelectedItem?.ToString();
                if (_isEditing)
                {
                    _fuelRepository.UpdateFuelRecord(fuel);
                    ShowSuccessMessage("Fuel record updated successfully.");
                }
                else
                {
                    _fuelRepository.AddFuelRecord(fuel);
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

        private bool ValidateFuelForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (_vehicleComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_vehicleComboBox, "Select a vehicle");
                valid = false;
            }
            if (_fuelLocationComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_fuelLocationComboBox, "Select a location");
                valid = false;
            }
            if (_fuelTypeComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_fuelTypeComboBox, "Select a fuel type");
                valid = false;
            }
            return valid;
        }

        private void FuelGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _fuelGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void SearchFuels()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadFuels();
                return;
            }
            List<Fuel> filtered = _fuels.FindAll(f =>
                (f.FuelLocation?.ToLower().Contains(searchTerm) == true) ||
                (f.FuelType?.ToLower().Contains(searchTerm) == true) ||
                (GetVehicleNumber(f.VehicleFueledID ?? 0).ToLower().Contains(searchTerm))
            );
            _fuelGrid.DataSource = null;
            _fuelGrid.DataSource = filtered;
        }

        private string GetVehicleNumber(int vehicleId)
        {
            var vehicle = _vehicles?.FirstOrDefault(v => v.Id == vehicleId);
            return vehicle?.VehicleNumber ?? vehicleId.ToString();
        }
    }
}


