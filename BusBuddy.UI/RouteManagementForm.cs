using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.UI
{
    public class RouteManagementForm : BaseDataForm
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;

        private DataGridView _routeGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private DateTimePicker _dateFilterPicker;
        private Button _filterButton;
        private Button _clearFilterButton;
        private List<Route> _routes;

        // Fields for add/edit
        private Panel _editPanel;
        private DateTimePicker _datePicker;
        private TextBox _routeNameTextBox;
        private GroupBox _amGroupBox;
        private ComboBox _amVehicleComboBox;
        private TextBox _amBeginMilesTextBox;
        private TextBox _amEndMilesTextBox;
        private TextBox _amRidersTextBox;
        private ComboBox _amDriverComboBox;
        private GroupBox _pmGroupBox;
        private ComboBox _pmVehicleComboBox;
        private TextBox _pmBeginMilesTextBox;
        private TextBox _pmEndMilesTextBox;
        private TextBox _pmRidersTextBox;
        private ComboBox _pmDriverComboBox;
        private Button _saveButton;
        private Button _cancelButton;

        private List<Vehicle> _vehicles;
        private List<Driver> _drivers;
        private Route _currentRoute;
        private bool _isEditing = false;

        public RouteManagementForm()
        {
            _routeRepository = new RouteRepository();
            _vehicleRepository = new VehicleRepository();
            _driverRepository = new DriverRepository();
            InitializeComponent();
            LoadVehiclesAndDrivers();
            LoadRoutes();
        }

        private void InitializeComponent()
        {
            this.Text = "Route Management";
            this.Size = new System.Drawing.Size(1000, 700);

            // Create filter controls
            CreateLabel("Filter by Date:", 20, 25);
            _dateFilterPicker = CreateDatePicker(120, 20, 150);
            _filterButton = CreateButton("Filter", 280, 20, (s, e) => FilterRoutes());
            _clearFilterButton = CreateButton("Clear", 390, 20, (s, e) => ClearFilter());

            // Create main grid
            _routeGrid = CreateDataGridView(20, 60, 950, 300);
            _routeGrid.CellDoubleClick += (s, e) => EditSelectedRoute();
            _routeGrid.SelectionChanged += RouteGrid_SelectionChanged;

            // Create buttons
            _addButton = CreateButton("Add New", 20, 370, (s, e) => AddNewRoute());
            _editButton = CreateButton("Edit", 130, 370, (s, e) => EditSelectedRoute());
            _deleteButton = CreateButton("Delete", 240, 370, (s, e) => DeleteSelectedRoute());
            _detailsButton = CreateButton("Details", 350, 370, (s, e) => ViewRouteDetails());

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
            _editPanel.Location = new System.Drawing.Point(20, 410);
            _editPanel.Size = new System.Drawing.Size(950, 230);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);

            // Route info
            Label dateLabel = new Label();
            dateLabel.Text = "Date:";
            dateLabel.Location = new System.Drawing.Point(10, 15);
            dateLabel.AutoSize = true;
            _editPanel.Controls.Add(dateLabel);

            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(120, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _datePicker.Format = DateTimePickerFormat.Short;
            _editPanel.Controls.Add(_datePicker);

            Label routeNameLabel = new Label();
            routeNameLabel.Text = "Route Name:";
            routeNameLabel.Location = new System.Drawing.Point(300, 15);
            routeNameLabel.AutoSize = true;
            _editPanel.Controls.Add(routeNameLabel);

            _routeNameTextBox = new TextBox();
            _routeNameTextBox.Location = new System.Drawing.Point(380, 10);
            _routeNameTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_routeNameTextBox);

            // AM Section
            _amGroupBox = new GroupBox();
            _amGroupBox.Text = "AM Route";
            _amGroupBox.Location = new System.Drawing.Point(10, 40);
            _amGroupBox.Size = new System.Drawing.Size(450, 130);
            _editPanel.Controls.Add(_amGroupBox);
            Label amVehicleLabel = new Label();
            amVehicleLabel.Text = "Vehicle:";
            amVehicleLabel.Location = new System.Drawing.Point(10, 25);
            amVehicleLabel.AutoSize = true;
            _amGroupBox.Controls.Add(amVehicleLabel);

            _amVehicleComboBox = new ComboBox();
            _amVehicleComboBox.Location = new System.Drawing.Point(120, 20);
            _amVehicleComboBox.Size = new System.Drawing.Size(200, 23);
            _amVehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _amGroupBox.Controls.Add(_amVehicleComboBox);

            Label amBeginMilesLabel = new Label();
            amBeginMilesLabel.Text = "Begin Miles:";
            amBeginMilesLabel.Location = new System.Drawing.Point(10, 55);
            amBeginMilesLabel.AutoSize = true;
            _amGroupBox.Controls.Add(amBeginMilesLabel);

            _amBeginMilesTextBox = new TextBox();
            _amBeginMilesTextBox.Location = new System.Drawing.Point(120, 50);
            _amBeginMilesTextBox.Size = new System.Drawing.Size(100, 23);
            _amGroupBox.Controls.Add(_amBeginMilesTextBox);

            Label amEndMilesLabel = new Label();
            amEndMilesLabel.Text = "End Miles:";
            amEndMilesLabel.Location = new System.Drawing.Point(230, 55);
            amEndMilesLabel.AutoSize = true;
            _amGroupBox.Controls.Add(amEndMilesLabel);

            _amEndMilesTextBox = new TextBox();
            _amEndMilesTextBox.Location = new System.Drawing.Point(300, 50);
            _amEndMilesTextBox.Size = new System.Drawing.Size(100, 23);
            _amGroupBox.Controls.Add(_amEndMilesTextBox);

            Label amRidersLabel = new Label();
            amRidersLabel.Text = "Riders:";
            amRidersLabel.Location = new System.Drawing.Point(10, 85);
            amRidersLabel.AutoSize = true;
            _amGroupBox.Controls.Add(amRidersLabel);

            _amRidersTextBox = new TextBox();
            _amRidersTextBox.Location = new System.Drawing.Point(120, 80);
            _amRidersTextBox.Size = new System.Drawing.Size(100, 23);
            _amGroupBox.Controls.Add(_amRidersTextBox);

            Label amDriverLabel = new Label();
            amDriverLabel.Text = "Driver:";
            amDriverLabel.Location = new System.Drawing.Point(230, 85);
            amDriverLabel.AutoSize = true;
            _amGroupBox.Controls.Add(amDriverLabel);

            _amDriverComboBox = new ComboBox();
            _amDriverComboBox.Location = new System.Drawing.Point(300, 80);
            _amDriverComboBox.Size = new System.Drawing.Size(140, 23);
            _amDriverComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _amGroupBox.Controls.Add(_amDriverComboBox);
            // PM Section
            _pmGroupBox = new GroupBox();
            _pmGroupBox.Text = "PM Route";
            _pmGroupBox.Location = new System.Drawing.Point(480, 40);
            _pmGroupBox.Size = new System.Drawing.Size(450, 130);
            _editPanel.Controls.Add(_pmGroupBox);

            Label pmVehicleLabel = new Label();
            pmVehicleLabel.Text = "Vehicle:";
            pmVehicleLabel.Location = new System.Drawing.Point(10, 25);
            pmVehicleLabel.AutoSize = true;
            _pmGroupBox.Controls.Add(pmVehicleLabel);

            _pmVehicleComboBox = new ComboBox();
            _pmVehicleComboBox.Location = new System.Drawing.Point(120, 20);
            _pmVehicleComboBox.Size = new System.Drawing.Size(200, 23);
            _pmVehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _pmGroupBox.Controls.Add(_pmVehicleComboBox);

            Label pmBeginMilesLabel = new Label();
            pmBeginMilesLabel.Text = "Begin Miles:";
            pmBeginMilesLabel.Location = new System.Drawing.Point(10, 55);
            pmBeginMilesLabel.AutoSize = true;
            _pmGroupBox.Controls.Add(pmBeginMilesLabel);

            _pmBeginMilesTextBox = new TextBox();
            _pmBeginMilesTextBox.Location = new System.Drawing.Point(120, 50);
            _pmBeginMilesTextBox.Size = new System.Drawing.Size(100, 23);
            _pmGroupBox.Controls.Add(_pmBeginMilesTextBox);

            Label pmEndMilesLabel = new Label();
            pmEndMilesLabel.Text = "End Miles:";
            pmEndMilesLabel.Location = new System.Drawing.Point(230, 55);
            pmEndMilesLabel.AutoSize = true;
            _pmGroupBox.Controls.Add(pmEndMilesLabel);

            _pmEndMilesTextBox = new TextBox();
            _pmEndMilesTextBox.Location = new System.Drawing.Point(300, 50);
            _pmEndMilesTextBox.Size = new System.Drawing.Size(100, 23);
            _pmGroupBox.Controls.Add(_pmEndMilesTextBox);

            Label pmRidersLabel = new Label();
            pmRidersLabel.Text = "Riders:";
            pmRidersLabel.Location = new System.Drawing.Point(10, 85);
            pmRidersLabel.AutoSize = true;
            _pmGroupBox.Controls.Add(pmRidersLabel);
            _pmRidersTextBox = new TextBox();
            _pmRidersTextBox.Location = new System.Drawing.Point(120, 80);
            _pmRidersTextBox.Size = new System.Drawing.Size(100, 23);
            _pmGroupBox.Controls.Add(_pmRidersTextBox);

            Label pmDriverLabel = new Label();
            pmDriverLabel.Text = "Driver:";
            pmDriverLabel.Location = new System.Drawing.Point(230, 85);
            pmDriverLabel.AutoSize = true;
            _pmGroupBox.Controls.Add(pmDriverLabel);

            _pmDriverComboBox = new ComboBox();
            _pmDriverComboBox.Location = new System.Drawing.Point(300, 80);
            _pmDriverComboBox.Size = new System.Drawing.Size(140, 23);
            _pmDriverComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _pmGroupBox.Controls.Add(_pmDriverComboBox);

            // Buttons
            _saveButton = new Button();
            _saveButton.Text = "Save";
            _saveButton.Location = new System.Drawing.Point(750, 190);
            _saveButton.Size = new System.Drawing.Size(100, 30);
            _saveButton.Click += (s, e) => SaveRoute();
            _editPanel.Controls.Add(_saveButton);

            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Location = new System.Drawing.Point(860, 190);
            _cancelButton.Size = new System.Drawing.Size(100, 30);
            _cancelButton.Click += (s, e) => CancelEdit();
            _editPanel.Controls.Add(_cancelButton);
        }
        private GroupBox CreateGroupBox(string text, int x, int y, int width, int height, Control? parent = null)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Text = text;
            groupBox.Location = new System.Drawing.Point(x, y);
            groupBox.Size = new System.Drawing.Size(width, height);

            if (parent != null)
                parent.Controls.Add(groupBox);
            else
                this.Controls.Add(groupBox);

            return groupBox;
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                _drivers = _driverRepository.GetAllDrivers();

                // Populate vehicle dropdowns
                _amVehicleComboBox.DisplayMember = "VehicleNumber";
                _amVehicleComboBox.ValueMember = "Id";
                _amVehicleComboBox.DataSource = new List<Vehicle>(_vehicles);

                _pmVehicleComboBox.DisplayMember = "VehicleNumber";
                _pmVehicleComboBox.ValueMember = "Id";
                _pmVehicleComboBox.DataSource = new List<Vehicle>(_vehicles);

                // Populate driver dropdowns
                _amDriverComboBox.DisplayMember = "DriverName";
                _amDriverComboBox.ValueMember = "DriverID";
                _amDriverComboBox.DataSource = new List<Driver>(_drivers);

                _pmDriverComboBox.DisplayMember = "DriverName";
                _pmDriverComboBox.ValueMember = "DriverID";
                _pmDriverComboBox.DataSource = new List<Driver>(_drivers);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles and drivers: {ex.Message}");
            }
        }

        private void LoadRoutes()
        {
            try
            {
                _routes = _routeRepository.GetAllRoutes();

                // Load related entities
                foreach (var route in _routes)
                {
                    if (route.AMVehicleID.HasValue)
                    {
                        route.AMVehicle = _vehicleRepository.GetVehicleById(route.AMVehicleID.Value);
                    }

                    if (route.AMDriverID.HasValue)
                    {
                        route.AMDriver = _driverRepository.GetDriverById(route.AMDriverID.Value);
                    }

                    if (route.PMVehicleID.HasValue)
                    {
                        route.PMVehicle = _vehicleRepository.GetVehicleById(route.PMVehicleID.Value);
                    }

                    if (route.PMDriverID.HasValue)
                    {
                        route.PMDriver = _driverRepository.GetDriverById(route.PMDriverID.Value);
                    }
                }

                UpdateRouteGrid(_routes);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading routes: {ex.Message}");
            }
        }
        private void UpdateRouteGrid(List<Route> routes)
        {
            _routeGrid.DataSource = null;

            var displayData = routes.ConvertAll(r => new
            {
                r.RouteID,
                Date = r.Date,
                r.RouteName,
                AMVehicle = r.AMVehicle?.VehicleNumber ?? "None",
                AMDriver = r.AMDriver?.DriverName ?? "None",
                AMMiles = r.AMEndMiles.HasValue && r.AMBeginMiles.HasValue ?
                    (decimal)(r.AMEndMiles.Value - r.AMBeginMiles.Value) : (decimal?)null,
                AMRiders = r.AMRiders,
                PMVehicle = r.PMVehicle?.VehicleNumber ?? "None",
                PMDriver = r.PMDriver?.DriverName ?? "None",
                PMMiles = r.PMEndMiles.HasValue && r.PMBeginMiles.HasValue ?
                    (decimal)(r.PMEndMiles.Value - r.PMBeginMiles.Value) : (decimal?)null,
                PMRiders = r.PMRiders
            });

            _routeGrid.DataSource = displayData;

            if (_routeGrid.Columns.Count > 0)
            {
                _routeGrid.Columns["RouteID"].HeaderText = "ID";
                _routeGrid.Columns["Date"].HeaderText = "Date";
                _routeGrid.Columns["RouteName"].HeaderText = "Route";
                _routeGrid.Columns["AMVehicle"].HeaderText = "AM Vehicle";
                _routeGrid.Columns["AMDriver"].HeaderText = "AM Driver";
                _routeGrid.Columns["AMMiles"].HeaderText = "AM Miles";
                _routeGrid.Columns["AMRiders"].HeaderText = "AM Riders";
                _routeGrid.Columns["PMVehicle"].HeaderText = "PM Vehicle";
                _routeGrid.Columns["PMDriver"].HeaderText = "PM Driver";
                _routeGrid.Columns["PMMiles"].HeaderText = "PM Miles";
                _routeGrid.Columns["PMRiders"].HeaderText = "PM Riders";
            }
        }

        private void RouteGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _routeGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }
        private void AddNewRoute()
        {
            _isEditing = false;
            _currentRoute = new Route
            {
                Date = DateTime.Today
            };

            // Set default values
            _datePicker.Value = DateTime.Today;
            _routeNameTextBox.Text = string.Empty;
            _amBeginMilesTextBox.Text = string.Empty;
            _amEndMilesTextBox.Text = string.Empty;
            _amRidersTextBox.Text = string.Empty;
            _pmBeginMilesTextBox.Text = string.Empty;
            _pmEndMilesTextBox.Text = string.Empty;
            _pmRidersTextBox.Text = string.Empty;

            // Set default selections for dropdowns with null checks
            if (_amVehicleComboBox.Items.Count > 0)
                _amVehicleComboBox.SelectedIndex = 0;
            if (_amDriverComboBox.Items.Count > 0)
                _amDriverComboBox.SelectedIndex = 0;
            if (_pmVehicleComboBox.Items.Count > 0)
                _pmVehicleComboBox.SelectedIndex = 0;
            if (_pmDriverComboBox.Items.Count > 0)
                _pmDriverComboBox.SelectedIndex = 0;

            _editPanel.Visible = true;
        }
        private void EditSelectedRoute()
        {
            if (_routeGrid.SelectedRows.Count == 0)
                return;

            _isEditing = true;
            int selectedId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
            _currentRoute = _databaseService.GetRouteWithDetails(selectedId);

            if (_currentRoute == null)
            {
                ShowErrorMessage("Could not find the selected route.");
                return;
            }

            // Populate date and name fields
            _datePicker.Value = _currentRoute.Date;
            _routeNameTextBox.Text = _currentRoute.RouteName;

            // Populate AM fields
            if (_currentRoute.AMVehicleID.HasValue)
            {
                for (int i = 0; i < _amVehicleComboBox.Items.Count; i++)
                {
                    if (((_amVehicleComboBox.Items[i] as Vehicle)?.Id ?? 0) == _currentRoute.AMVehicleID)
                    {
                        _amVehicleComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            _amBeginMilesTextBox.Text = _currentRoute.AMBeginMiles?.ToString();
            _amEndMilesTextBox.Text = _currentRoute.AMEndMiles?.ToString();
            _amRidersTextBox.Text = _currentRoute.AMRiders?.ToString();

            if (_currentRoute.AMDriverID.HasValue)
            {
                for (int i = 0; i < _amDriverComboBox.Items.Count; i++)
                {
                    if (((_amDriverComboBox.Items[i] as Driver)?.DriverID ?? 0) == _currentRoute.AMDriverID)
                    {
                        _amDriverComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            // Populate PM fields
            if (_currentRoute.PMVehicleID.HasValue)
            {
                for (int i = 0; i < _pmVehicleComboBox.Items.Count; i++)
                {
                    if (((_pmVehicleComboBox.Items[i] as Vehicle)?.Id ?? 0) == _currentRoute.PMVehicleID)
                    {
                        _pmVehicleComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            _pmBeginMilesTextBox.Text = _currentRoute.PMBeginMiles?.ToString();
            _pmEndMilesTextBox.Text = _currentRoute.PMEndMiles?.ToString();
            _pmRidersTextBox.Text = _currentRoute.PMRiders?.ToString();

            if (_currentRoute.PMDriverID.HasValue)
            {
                for (int i = 0; i < _pmDriverComboBox.Items.Count; i++)
                {
                    if (((_pmDriverComboBox.Items[i] as Driver)?.DriverID ?? 0) == _currentRoute.PMDriverID)
                    {
                        _pmDriverComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }

            _editPanel.Visible = true;
        }

        private void DeleteSelectedRoute()
        {
            if (_routeGrid.SelectedRows.Count == 0)
                return;

            if (!ConfirmDelete())
                return;

            try
            {
                int selectedId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
                bool success = _routeRepository.DeleteRoute(selectedId);

                if (success)
                {
                    ShowSuccessMessage("Route deleted successfully.");
                    LoadRoutes();
                }
                else
                {
                    ShowErrorMessage("Could not delete the route.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting route: {ex.Message}");
            }
        }

        private void ViewRouteDetails()
        {
            if (_routeGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
            var routeDetails = _databaseService.GetRouteWithDetails(selectedId);

            if (routeDetails != null)
            {
                string amInfo = routeDetails.AMVehicle != null && routeDetails.AMDriver != null
                    ? $"AM: {routeDetails.AMVehicle.VehicleNumber} driven by {routeDetails.AMDriver.DriverName}\n" +
                      $"Miles: {routeDetails.AMBeginMiles} to {routeDetails.AMEndMiles}\n" +
                      $"Total: {(routeDetails.AMEndMiles.HasValue && routeDetails.AMBeginMiles.HasValue ? (routeDetails.AMEndMiles - routeDetails.AMBeginMiles).ToString() : "N/A")} miles with {routeDetails.AMRiders ?? 0} riders"
                    : "No AM route data";

                string pmInfo = routeDetails.PMVehicle != null && routeDetails.PMDriver != null
                    ? $"PM: {routeDetails.PMVehicle.VehicleNumber} driven by {routeDetails.PMDriver.DriverName}\n" +
                      $"Miles: {routeDetails.PMBeginMiles} to {routeDetails.PMEndMiles}\n" +
                      $"Total: {(routeDetails.PMEndMiles.HasValue && routeDetails.PMBeginMiles.HasValue ? (routeDetails.PMEndMiles - routeDetails.PMBeginMiles).ToString() : "N/A")} miles with {routeDetails.PMRiders ?? 0} riders"
                    : "No PM route data";

                MessageBox.Show($"Route Details for {routeDetails.RouteName} on {routeDetails.Date}\n\n" +
                                $"{amInfo}\n\n{pmInfo}",
                                "Route Details",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load route details.");
            }
        }
        private void SaveRoute()
        {
            if (!ValidateRouteForm())
                return;

            try
            {
                // Transfer form values to route object
                _currentRoute.Date = _datePicker.Value;
                _currentRoute.RouteName = _routeNameTextBox.Text;

                // AM Values
                if (_amVehicleComboBox.SelectedItem is Vehicle amVehicle)
                {
                    _currentRoute.AMVehicleID = amVehicle.Id;
                }

                if (double.TryParse(_amBeginMilesTextBox.Text, out double amBeginMiles))
                {
                    _currentRoute.AMBeginMiles = (decimal)amBeginMiles;
                }
                else
                {
                    _currentRoute.AMBeginMiles = null;
                }

                if (double.TryParse(_amEndMilesTextBox.Text, out double amEndMiles))
                {
                    _currentRoute.AMEndMiles = (decimal)amEndMiles;
                }
                else
                {
                    _currentRoute.AMEndMiles = null;
                }

                if (int.TryParse(_amRidersTextBox.Text, out int amRiders))
                {
                    _currentRoute.AMRiders = amRiders;
                }
                else
                {
                    _currentRoute.AMRiders = null;
                }

                if (_amDriverComboBox.SelectedItem is Driver amDriver)
                {
                    _currentRoute.AMDriverID = amDriver.DriverID;
                }

                // PM Values
                if (_pmVehicleComboBox.SelectedItem is Vehicle pmVehicle)
                {
                    _currentRoute.PMVehicleID = pmVehicle.Id;
                }

                if (double.TryParse(_pmBeginMilesTextBox.Text, out double pmBeginMiles))
                {
                    _currentRoute.PMBeginMiles = (decimal)pmBeginMiles;
                }
                else
                {
                    _currentRoute.PMBeginMiles = null;
                }

                if (double.TryParse(_pmEndMilesTextBox.Text, out double pmEndMiles))
                {
                    _currentRoute.PMEndMiles = (decimal)pmEndMiles;
                }
                else
                {
                    _currentRoute.PMEndMiles = null;
                }

                if (int.TryParse(_pmRidersTextBox.Text, out int pmRiders))
                {
                    _currentRoute.PMRiders = pmRiders;
                }
                else
                {
                    _currentRoute.PMRiders = null;
                }

                if (_pmDriverComboBox.SelectedItem is Driver pmDriver)
                {
                    _currentRoute.PMDriverID = pmDriver.DriverID;
                }

                bool success;
                if (_isEditing)
                {
                    success = _routeRepository.UpdateRoute(_currentRoute);
                    if (success)
                    {
                        ShowSuccessMessage("Route updated successfully.");
                    }
                }
                else
                {
                    success = _routeRepository.AddRoute(_currentRoute) > 0;
                    if (success)
                    {
                        ShowSuccessMessage("Route added successfully.");
                    }
                }

                if (success)
                {
                    _editPanel.Visible = false;
                    LoadRoutes();
                }
                else
                {
                    ShowErrorMessage("Failed to save route. Please check your input and try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving route: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }
        private void FilterRoutes()
        {
            DateTime filterDate = _dateFilterPicker.Value.Date;
            var filteredRoutes = _routes.FindAll(r => r.Date.Date == filterDate);

            UpdateRouteGrid(filteredRoutes);
        }

        private void ClearFilter()
        {
            UpdateRouteGrid(_routes);
        }

        private bool ValidateRouteForm()
        {
            _errorProvider.Clear();
            bool isValid = true;

            // Validate required fields
            if (!FormValidator.ValidateRequiredField(_routeNameTextBox, "Route Name", _errorProvider))
                isValid = false;

            // Validate AM mileage
            if (!string.IsNullOrEmpty(_amBeginMilesTextBox.Text) || !string.IsNullOrEmpty(_amEndMilesTextBox.Text))
            {
                if (!FormValidator.ValidateNumericField(_amBeginMilesTextBox, "AM Begin Miles", _errorProvider))
                    isValid = false;

                if (!FormValidator.ValidateNumericField(_amEndMilesTextBox, "AM End Miles", _errorProvider))
                    isValid = false;

                // Validate AM end miles is greater than begin miles
                if (isValid && double.TryParse(_amBeginMilesTextBox.Text, out double amBegin) &&
                    double.TryParse(_amEndMilesTextBox.Text, out double amEnd))
                {
                    if (amEnd < amBegin)
                    {
                        _errorProvider.SetError(_amEndMilesTextBox, "End miles must be greater than begin miles");
                        isValid = false;
                    }
                }
            }

            // Validate AM riders
            if (!string.IsNullOrEmpty(_amRidersTextBox.Text) &&
                !FormValidator.ValidateIntegerField(_amRidersTextBox, "AM Riders", _errorProvider))
            {
                isValid = false;
            }

            // Validate PM mileage
            if (!string.IsNullOrEmpty(_pmBeginMilesTextBox.Text) || !string.IsNullOrEmpty(_pmEndMilesTextBox.Text))
            {
                if (!FormValidator.ValidateNumericField(_pmBeginMilesTextBox, "PM Begin Miles", _errorProvider))
                    isValid = false;

                if (!FormValidator.ValidateNumericField(_pmEndMilesTextBox, "PM End Miles", _errorProvider))
                    isValid = false;

                // Validate PM end miles is greater than begin miles
                if (isValid && double.TryParse(_pmBeginMilesTextBox.Text, out double pmBegin) &&
                    double.TryParse(_pmEndMilesTextBox.Text, out double pmEnd))
                {
                    if (pmEnd < pmBegin)
                    {
                        _errorProvider.SetError(_pmEndMilesTextBox, "End miles must be greater than begin miles");
                        isValid = false;
                    }
                }
            }

            // Validate PM riders
            if (!string.IsNullOrEmpty(_pmRidersTextBox.Text) &&
                !FormValidator.ValidateIntegerField(_pmRidersTextBox, "PM Riders", _errorProvider))
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
