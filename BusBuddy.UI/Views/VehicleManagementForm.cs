using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class VehicleManagementForm : BaseDataForm
    {
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView _vehicleGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private TextBox _searchBox;
        private Button _searchButton;
        private List<Vehicle> _vehicles;

        // Fields for add/edit
        private Panel _editPanel;
        private TextBox _vehicleNumberTextBox;
        private TextBox _busNumberTextBox;
        private TextBox _makeTextBox;
        private TextBox _modelTextBox;
        private TextBox _yearTextBox;
        private TextBox _seatingCapacityTextBox;
        private TextBox _vinTextBox;
        private TextBox _licenseNumberTextBox;
        private DateTimePicker _lastInspectionDatePicker;
        private ComboBox _fuelTypeComboBox;
        private ComboBox _statusComboBox;
        private Button _saveButton;
        private Button _cancelButton;

        private Vehicle _currentVehicle;
        private bool _isEditing = false;

        public VehicleManagementForm()
        {
            _vehicleRepository = new VehicleRepository();
            InitializeComponent();
            LoadVehicles();
        }

        private void InitializeComponent()
        {
            this.Text = "Vehicle Management";
            this.Size = new System.Drawing.Size(1200, 900);

            // Create buttons
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewVehicle());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedVehicle());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedVehicle());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewVehicleDetails());

            // Create search box
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchVehicles());

            // Create main grid (move down to leave space for buttons)
            _vehicleGrid = new DataGridView();
            _vehicleGrid.Location = new System.Drawing.Point(20, 60);
            _vehicleGrid.Size = new System.Drawing.Size(1150, 650);
            _vehicleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // DataGridView dynamic settings
            _vehicleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _vehicleGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _vehicleGrid.AllowUserToResizeColumns = true;
            _vehicleGrid.AllowUserToResizeRows = true;
            _vehicleGrid.ScrollBars = ScrollBars.Both;
            // Hide primary key column if present
            _vehicleGrid.DataBindingComplete += (s, e) => {
                if (_vehicleGrid.Columns.Contains("Id"))
                    _vehicleGrid.Columns["Id"].Visible = false;
            };

            this.Controls.Add(_vehicleGrid);

            _vehicleGrid.CellDoubleClick += (s, e) => EditSelectedVehicle();
            _vehicleGrid.SelectionChanged += VehicleGrid_SelectionChanged;

            // Initialize edit panel (hidden initially)
            InitializeEditPanel();

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.BorderStyle = BorderStyle.FixedSingle;
            _editPanel.Location = new System.Drawing.Point(20, 370);
            _editPanel.Size = new System.Drawing.Size(750, 180);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);

            // Left column
            CreateLabel("Vehicle Number:", 10, 15, _editPanel);
            _vehicleNumberTextBox = CreateTextBox(130, 10, 200, _editPanel);

            CreateLabel("Bus Number:", 10, 45, _editPanel);
            _busNumberTextBox = CreateTextBox(130, 40, 200, _editPanel);

            CreateLabel("Make:", 10, 75, _editPanel);
            _makeTextBox = CreateTextBox(130, 70, 200, _editPanel);

            CreateLabel("Model:", 10, 105, _editPanel);
            _modelTextBox = CreateTextBox(130, 100, 200, _editPanel);

            CreateLabel("Year:", 10, 135, _editPanel);
            _yearTextBox = CreateTextBox(130, 130, 200, _editPanel);

            // Right column
            CreateLabel("Seating Capacity:", 350, 15, _editPanel);
            _seatingCapacityTextBox = CreateTextBox(470, 10, 200, _editPanel);

            CreateLabel("VIN Number:", 350, 45, _editPanel);
            _vinTextBox = CreateTextBox(470, 40, 200, _editPanel);
            CreateLabel("License Number:", 350, 75, _editPanel);
            _licenseNumberTextBox = CreateTextBox(470, 70, 200, _editPanel);

            CreateLabel("Last Inspection:", 350, 105, _editPanel);
            _lastInspectionDatePicker = CreateDatePicker(470, 100, 200, _editPanel);

            CreateLabel("Fuel Type:", 350, 135, _editPanel);
            _fuelTypeComboBox = CreateComboBox(470, 130, 100, _editPanel);
            _fuelTypeComboBox.Items.AddRange(new object[] { "Diesel", "Gasoline", "Propane", "Electric", "CNG" });

            CreateLabel("Status:", 580, 135, _editPanel);
            _statusComboBox = CreateComboBox(630, 130, 80, _editPanel);
            _statusComboBox.Items.AddRange(new object[] { "Active", "Inactive", "Maintenance", "Retired" });

            // Buttons
            _saveButton = CreateButton("Save", 550, 145, (s, e) => SaveVehicle(), _editPanel);
            _cancelButton = CreateButton("Cancel", 660, 145, (s, e) => CancelEdit(), _editPanel);
        }

        private Label CreateLabel(string text, int x, int y, Control? parent = null)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new System.Drawing.Point(x, y);
            label.AutoSize = true;

            if (parent != null)
                parent.Controls.Add(label);
            else
                this.Controls.Add(label);

            return label;
        }

        private TextBox CreateTextBox(int x, int y, int width, Control? parent = null)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new System.Drawing.Point(x, y);
            textBox.Size = new System.Drawing.Size(width, 23);

            if (parent != null)
                parent.Controls.Add(textBox);
            else
                this.Controls.Add(textBox);

            return textBox;
        }

        private DateTimePicker CreateDatePicker(int x, int y, int width, Control? parent = null)
        {
            DateTimePicker picker = new DateTimePicker();
            picker.Location = new System.Drawing.Point(x, y);
            picker.Size = new System.Drawing.Size(width, 23);
            picker.Format = DateTimePickerFormat.Short;

            if (parent != null)
                parent.Controls.Add(picker);
            else
                this.Controls.Add(picker);

            return picker;
        }

        private ComboBox CreateComboBox(int x, int y, int width, Control? parent = null)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Location = new System.Drawing.Point(x, y);
            comboBox.Size = new System.Drawing.Size(width, 23);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;

            if (parent != null)
                parent.Controls.Add(comboBox);
            else
                this.Controls.Add(comboBox);

            return comboBox;
        }

        private Button CreateButton(string text, int x, int y, EventHandler? clickHandler = null, Control? parent = null)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(100, 30);

            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }

            if (parent != null)
                parent.Controls.Add(button);
            else
                this.Controls.Add(button);

            return button;
        }

        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();

                _vehicleGrid.DataSource = null;
                _vehicleGrid.DataSource = _vehicles;

                // Configure columns
                if (_vehicleGrid.Columns.Count > 0)
                {
                    _vehicleGrid.Columns["Id"].HeaderText = "ID";
                    _vehicleGrid.Columns["VehicleNumber"].HeaderText = "Vehicle #";
                    _vehicleGrid.Columns["BusNumber"].HeaderText = "Bus #";
                    _vehicleGrid.Columns["SeatingCapacity"].HeaderText = "Capacity";
                    _vehicleGrid.Columns["VINNumber"].HeaderText = "VIN";
                    _vehicleGrid.Columns["LicenseNumber"].HeaderText = "License";
                    _vehicleGrid.Columns["DateLastInspection"].HeaderText = "Last Inspection";

                    // Hide NavigationProperties
                    if (_vehicleGrid.Columns.Contains("Routes"))
                        _vehicleGrid.Columns["Routes"].Visible = false;
                    if (_vehicleGrid.Columns.Contains("Activities"))
                        _vehicleGrid.Columns["Activities"].Visible = false;
                    if (_vehicleGrid.Columns.Contains("FuelRecords"))
                        _vehicleGrid.Columns["FuelRecords"].Visible = false;
                    if (_vehicleGrid.Columns.Contains("MaintenanceRecords"))
                        _vehicleGrid.Columns["MaintenanceRecords"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
            }
        }

        private void VehicleGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _vehicleGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void AddNewVehicle()
        {
            _isEditing = false;
            _currentVehicle = new Vehicle();

            // Clear all fields
            _vehicleNumberTextBox.Text = string.Empty;
            _busNumberTextBox.Text = string.Empty;
            _makeTextBox.Text = string.Empty;
            _modelTextBox.Text = string.Empty;
            _yearTextBox.Text = string.Empty;
            _seatingCapacityTextBox.Text = string.Empty;
            _vinTextBox.Text = string.Empty;
            _licenseNumberTextBox.Text = string.Empty;
            _lastInspectionDatePicker.Value = DateTime.Today;
            _fuelTypeComboBox.SelectedIndex = -1;
            _statusComboBox.SelectedIndex = 0; // Default to Active

            _editPanel.Visible = true;
        }

        private void EditSelectedVehicle()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;

            _isEditing = true;
            int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
            var vehicle = _vehicleRepository.GetVehicleById(selectedId);
            if (vehicle == null)
            {
                ShowErrorMessage("Could not find the selected vehicle.");
                return;
            }
            _currentVehicle = vehicle;

            // Populate fields
            _vehicleNumberTextBox.Text = _currentVehicle.VehicleNumber ?? string.Empty;
            _busNumberTextBox.Text = _currentVehicle.BusNumber ?? string.Empty;
            _makeTextBox.Text = _currentVehicle.Make ?? string.Empty;
            _modelTextBox.Text = _currentVehicle.Model ?? string.Empty;
            _yearTextBox.Text = _currentVehicle.Year.ToString();
            _seatingCapacityTextBox.Text = _currentVehicle.SeatingCapacity.ToString();
            _vinTextBox.Text = _currentVehicle.VINNumber ?? string.Empty;
            _licenseNumberTextBox.Text = _currentVehicle.LicenseNumber ?? string.Empty;
            if (_currentVehicle.DateLastInspection.HasValue)
            {
                _lastInspectionDatePicker.Value = _currentVehicle.DateLastInspection.Value;
            }
            else
            {
                _lastInspectionDatePicker.Value = DateTime.Today;
            }

            _fuelTypeComboBox.SelectedItem = _currentVehicle.FuelType ?? string.Empty;
            _statusComboBox.SelectedItem = _currentVehicle.Status ?? string.Empty;

            _editPanel.Visible = true;
        }

        private void DeleteSelectedVehicle()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;

            if (!ConfirmDelete())
                return;

            try
            {
                int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
                bool success = _vehicleRepository.DeleteVehicle(selectedId);

                if (success)
                {
                    ShowSuccessMessage("Vehicle deleted successfully.");
                    LoadVehicles();
                }
                else
                {
                    ShowErrorMessage("Could not delete the vehicle. It may be referenced by other records.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting vehicle: {ex.Message}");
            }
        }

        private void ViewVehicleDetails()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
            var vehicleDetails = _databaseService.GetVehicleDetails(selectedId);

            if (vehicleDetails != null)
            {
                // TODO: Create and show a VehicleDetailsForm
                MessageBox.Show($"Vehicle Details for {vehicleDetails.Vehicle.VehicleNumber}\n" +
                                $"AM Routes: {vehicleDetails.AMRoutes.Count}\n" +
                                $"PM Routes: {vehicleDetails.PMRoutes.Count}\n" +
                                $"Activities: {vehicleDetails.Activities.Count}\n" +
                                $"Fuel Records: {vehicleDetails.FuelRecords.Count}\n" +
                                $"Maintenance Records: {vehicleDetails.MaintenanceRecords.Count}",
                                "Vehicle Details",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load vehicle details.");
            }
        }

        private void SaveVehicle()
        {
            if (!ValidateVehicleForm())
                return;

            try
            {
                // Transfer form values to vehicle object
                _currentVehicle.VehicleNumber = _vehicleNumberTextBox.Text;
                _currentVehicle.BusNumber = _busNumberTextBox.Text;
                _currentVehicle.Make = _makeTextBox.Text;
                _currentVehicle.Model = _modelTextBox.Text;

                if (int.TryParse(_yearTextBox.Text, out int year))
                    _currentVehicle.Year = year;

                if (int.TryParse(_seatingCapacityTextBox.Text, out int capacity))
                    _currentVehicle.SeatingCapacity = capacity;
                _currentVehicle.VINNumber = _vinTextBox.Text;
                _currentVehicle.LicenseNumber = _licenseNumberTextBox.Text;
                _currentVehicle.DateLastInspection = _lastInspectionDatePicker.Value;
                _currentVehicle.FuelType = _fuelTypeComboBox.SelectedItem?.ToString();
                _currentVehicle.Status = _statusComboBox.SelectedItem?.ToString();

                bool success;
                if (_isEditing)
                {
                    success = _vehicleRepository.UpdateVehicle(_currentVehicle);
                    if (success)
                    {
                        ShowSuccessMessage("Vehicle updated successfully.");
                    }
                }
                else
                {
                    int result = _vehicleRepository.AddVehicle(_currentVehicle);
                    success = result > 0;
                    if (success)
                    {
                        ShowSuccessMessage("Vehicle added successfully.");
                    }
                }

                if (success)
                {
                    _editPanel.Visible = false;
                    LoadVehicles();
                }
                else
                {
                    ShowErrorMessage("Failed to save vehicle. Please check your input and try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving vehicle: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void SearchVehicles()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadVehicles();
                return;
            }

            List<Vehicle> filteredVehicles = _vehicles.FindAll(v =>
                (v.VehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                (v.BusNumber?.ToLower().Contains(searchTerm) == true) ||
                (v.Make?.ToLower().Contains(searchTerm) == true) ||
                (v.Model?.ToLower().Contains(searchTerm) == true) ||
                (v.VINNumber?.ToLower().Contains(searchTerm) == true) ||
                (v.LicenseNumber?.ToLower().Contains(searchTerm) == true)
            );

            _vehicleGrid.DataSource = null;
            _vehicleGrid.DataSource = filteredVehicles;
        }

        private bool ValidateVehicleForm()
        {
            _errorProvider.Clear();
            bool isValid = true;

            // Validate required fields
            if (!FormValidator.ValidateRequiredField(_vehicleNumberTextBox, "Vehicle Number", _errorProvider))
                isValid = false;

            // Validate numeric fields
            if (!FormValidator.ValidateIntegerField(_yearTextBox, "Year", _errorProvider))
                isValid = false;

            if (!FormValidator.ValidateIntegerField(_seatingCapacityTextBox, "Seating Capacity", _errorProvider))
                isValid = false;

            return isValid;
        }
    }
}
