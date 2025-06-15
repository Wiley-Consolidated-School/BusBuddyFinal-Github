using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
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
        private TextBox _searchBox;
        private Button _searchButton;
        private List<Route> _routes = new List<Route>();
        private List<Vehicle> _vehicles = new List<Vehicle>();
        private List<Driver> _drivers = new List<Driver>();

        // Fields for add/edit
        private Panel _editPanel = null!;
        private DateTimePicker _datePicker = null!;
        private TextBox _routeNameTextBox = null!;
        private ComboBox _amVehicleComboBox = null!;
        private TextBox _amBeginMilesTextBox = null!;
        private TextBox _amEndMilesTextBox = null!;
        private TextBox _amRidersTextBox = null!;
        private ComboBox _amDriverComboBox = null!;
        private ComboBox _pmVehicleComboBox = null!;
        private TextBox _pmBeginMilesTextBox = null!;
        private TextBox _pmEndMilesTextBox = null!;
        private TextBox _pmRidersTextBox = null!;
        private ComboBox _pmDriverComboBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Route? _currentRoute;
        private bool _isEditing = false;

        public RouteManagementForm() : this(new RouteRepository(), new VehicleRepository(), new DriverRepository()) { }

        public RouteManagementForm(IRouteRepository routeRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadVehiclesAndDrivers();
            LoadRoutes();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Route Management"
            this.Text = "Route Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewRoute());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedRoute());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedRoute());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewRouteDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchRoutes());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _routeGrid = new DataGridView();
            _routeGrid.Location = new System.Drawing.Point(20, 60);
            _routeGrid.Size = new System.Drawing.Size(1150, 650);
            _routeGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _routeGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _routeGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _routeGrid.ReadOnly = true;
            _routeGrid.AllowUserToAddRows = false;
            _routeGrid.AllowUserToDeleteRows = false;
            _routeGrid.MultiSelect = false;
            _routeGrid.AllowUserToResizeColumns = true;
            _routeGrid.AllowUserToResizeRows = true;
            _routeGrid.ScrollBars = ScrollBars.Both;
            _routeGrid.DataBindingComplete += (s, e) => {
                if (_routeGrid.Columns.Contains("RouteID"))
                    _routeGrid.Columns["RouteID"].Visible = false;
            };
            this.Controls.Add(_routeGrid);
            _routeGrid.CellDoubleClick += (s, e) => EditSelectedRoute();
            _routeGrid.SelectionChanged += RouteGrid_SelectionChanged;

            // Initialize edit panel (1150x120, y=730, hidden)
            InitializeEditPanel();

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles().ToList();
                _drivers = _driverRepository.GetAllDrivers().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles and drivers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRoutes()
        {
            try
            {
                _routes = _routeRepository.GetAllRoutes().ToList();
                PopulateRouteGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading routes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateRouteGrid()
        {
            _routeGrid.DataSource = null;

            if (_routes?.Any() == true)
            {
                var displayData = _routes.Select(r => new
                {
                    RouteID = r.RouteID,
                    Date = r.Date.ToString("yyyy-MM-dd"),
                    RouteName = r.RouteName ?? "",
                    AMVehicle = r.AMVehicleNumber ?? "",
                    AMDriver = r.AMDriverName ?? "",
                    AMBeginMiles = r.AMBeginMiles?.ToString("N0") ?? "",
                    AMEndMiles = r.AMEndMiles?.ToString("N0") ?? "",
                    AMRiders = r.AMRiders?.ToString() ?? "",
                    PMVehicle = r.PMVehicleNumber ?? "",
                    PMDriver = r.PMDriverName ?? "",
                    PMBeginMiles = r.PMBeginMiles?.ToString("N0") ?? "",
                    PMEndMiles = r.PMEndMiles?.ToString("N0") ?? "",
                    PMRiders = r.PMRiders?.ToString() ?? ""
                }).ToList();

                _routeGrid.DataSource = displayData;

                // Hide ID column and adjust column widths
                if (_routeGrid.Columns.Contains("RouteID"))
                    _routeGrid.Columns["RouteID"].Visible = false;

                // Adjust column widths for better display
                if (_routeGrid.Columns.Contains("Date"))
                    _routeGrid.Columns["Date"].Width = 100;
                if (_routeGrid.Columns.Contains("RouteName"))
                    _routeGrid.Columns["RouteName"].Width = 150;
            }
        }
        private void InitializeEditPanel()
        {
            // Create edit panel (1150x120, y=730, hidden)
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 730);
            _editPanel.Size = new System.Drawing.Size(1150, 120);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);

            // Route form-specific fields: Date, Route Name, AM/PM Vehicle, Driver, Begin/End Miles, Riders
            // Row 1: Date, Route Name, AM Vehicle, AM Driver
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(50, 10);
            _datePicker.Size = new System.Drawing.Size(120, 23);
            _datePicker.Value = DateTime.Today;
            _editPanel.Controls.Add(_datePicker);

            var routeNameLabel = CreateLabel("Route:", 180, 15);
            _editPanel.Controls.Add(routeNameLabel);
            _routeNameTextBox = new TextBox();
            _routeNameTextBox.Location = new System.Drawing.Point(230, 10);
            _routeNameTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_routeNameTextBox);

            var amVehicleLabel = CreateLabel("AM Vehicle:", 340, 15);
            _editPanel.Controls.Add(amVehicleLabel);
            _amVehicleComboBox = new ComboBox();
            _amVehicleComboBox.Location = new System.Drawing.Point(410, 10);
            _amVehicleComboBox.Size = new System.Drawing.Size(100, 23);
            _amVehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_amVehicleComboBox);

            var amDriverLabel = CreateLabel("AM Driver:", 520, 15);
            _editPanel.Controls.Add(amDriverLabel);
            _amDriverComboBox = new ComboBox();
            _amDriverComboBox.Location = new System.Drawing.Point(580, 10);
            _amDriverComboBox.Size = new System.Drawing.Size(120, 23);
            _amDriverComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_amDriverComboBox);

            // Row 2: AM Begin/End Miles, Riders, PM Vehicle, PM Driver
            var amBeginLabel = CreateLabel("AM Begin:", 10, 55);
            _editPanel.Controls.Add(amBeginLabel);
            _amBeginMilesTextBox = new TextBox();
            _amBeginMilesTextBox.Location = new System.Drawing.Point(80, 50);
            _amBeginMilesTextBox.Size = new System.Drawing.Size(60, 23);
            _editPanel.Controls.Add(_amBeginMilesTextBox);

            var amEndLabel = CreateLabel("End:", 150, 55);
            _editPanel.Controls.Add(amEndLabel);
            _amEndMilesTextBox = new TextBox();
            _amEndMilesTextBox.Location = new System.Drawing.Point(180, 50);
            _amEndMilesTextBox.Size = new System.Drawing.Size(60, 23);
            _editPanel.Controls.Add(_amEndMilesTextBox);

            var amRidersLabel = CreateLabel("Riders:", 250, 55);
            _editPanel.Controls.Add(amRidersLabel);
            _amRidersTextBox = new TextBox();
            _amRidersTextBox.Location = new System.Drawing.Point(290, 50);
            _amRidersTextBox.Size = new System.Drawing.Size(50, 23);
            _editPanel.Controls.Add(_amRidersTextBox);

            var pmVehicleLabel = CreateLabel("PM Vehicle:", 350, 55);
            _editPanel.Controls.Add(pmVehicleLabel);
            _pmVehicleComboBox = new ComboBox();
            _pmVehicleComboBox.Location = new System.Drawing.Point(420, 50);
            _pmVehicleComboBox.Size = new System.Drawing.Size(100, 23);
            _pmVehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_pmVehicleComboBox);

            var pmDriverLabel = CreateLabel("PM Driver:", 530, 55);
            _editPanel.Controls.Add(pmDriverLabel);
            _pmDriverComboBox = new ComboBox();
            _pmDriverComboBox.Location = new System.Drawing.Point(590, 50);
            _pmDriverComboBox.Size = new System.Drawing.Size(120, 23);
            _pmDriverComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_pmDriverComboBox);

            // PM Miles (compact layout)
            var pmBeginLabel = CreateLabel("PM Begin:", 720, 55);
            _editPanel.Controls.Add(pmBeginLabel);
            _pmBeginMilesTextBox = new TextBox();
            _pmBeginMilesTextBox.Location = new System.Drawing.Point(780, 50);
            _pmBeginMilesTextBox.Size = new System.Drawing.Size(60, 23);
            _editPanel.Controls.Add(_pmBeginMilesTextBox);

            var pmEndLabel = CreateLabel("End:", 850, 55);
            _editPanel.Controls.Add(pmEndLabel);
            _pmEndMilesTextBox = new TextBox();
            _pmEndMilesTextBox.Location = new System.Drawing.Point(880, 50);
            _pmEndMilesTextBox.Size = new System.Drawing.Size(60, 23);
            _editPanel.Controls.Add(_pmEndMilesTextBox);

            var pmRidersLabel = CreateLabel("Riders:", 950, 55);
            _editPanel.Controls.Add(pmRidersLabel);
            _pmRidersTextBox = new TextBox();
            _pmRidersTextBox.Location = new System.Drawing.Point(990, 50);
            _pmRidersTextBox.Size = new System.Drawing.Size(50, 23);
            _editPanel.Controls.Add(_pmRidersTextBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveRoute());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);

            // Populate combo boxes
            PopulateComboBoxes();
        }

        private void RouteGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _routeGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void PopulateComboBoxes()
        {
            // Populate vehicle combo boxes
            var vehicleItems = _vehicles.Select(v => new { Text = v.VehicleNumber, Value = v }).ToList();

            // Add a default item if list is empty to prevent binding issues
            if (vehicleItems.Count == 0)
            {
                vehicleItems.Add(new { Text = "No vehicles available", Value = default(Vehicle) });
            }

            _amVehicleComboBox.DataSource = new List<object>(vehicleItems);
            _amVehicleComboBox.DisplayMember = "Text";
            _amVehicleComboBox.ValueMember = "Value";

            _pmVehicleComboBox.DataSource = new List<object>(vehicleItems);
            _pmVehicleComboBox.DisplayMember = "Text";
            _pmVehicleComboBox.ValueMember = "Value";

            // Populate driver combo boxes
            var driverItems = _drivers.Select(d => new { Text = d.DriverName, Value = d }).ToList();

            // Add a default item if list is empty to prevent binding issues
            if (driverItems.Count == 0)
            {
                driverItems.Add(new { Text = "No drivers available", Value = default(Driver) });
            }

            _amDriverComboBox.DataSource = new List<object>(driverItems);
            _amDriverComboBox.DisplayMember = "Text";
            _amDriverComboBox.ValueMember = "Value";

            _pmDriverComboBox.DataSource = new List<object>(driverItems);
            _pmDriverComboBox.DisplayMember = "Text";
            _pmDriverComboBox.ValueMember = "Value";
        }

        private void AddNewRoute()
        {
            _currentRoute = new Route();
            _isEditing = false;
            ClearEditPanel();
            _editPanel.Visible = true;
        }

        private void EditSelectedRoute()
        {
            if (_routeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a route to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRouteId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
            _currentRoute = _routes.FirstOrDefault(r => r.RouteID == selectedRouteId);

            if (_currentRoute != null)
            {
                _isEditing = true;
                PopulateEditPanel(_currentRoute);
                _editPanel.Visible = true;
            }
        }

        private void DeleteSelectedRoute()
        {
            if (_routeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a route to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRouteId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
            var routeToDelete = _routes.FirstOrDefault(r => r.RouteID == selectedRouteId);

            if (routeToDelete != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the route '{routeToDelete.RouteName}' for {routeToDelete.Date:yyyy-MM-dd}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _routeRepository.DeleteRoute(routeToDelete.RouteID);
                        MessageBox.Show("Route deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRoutes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewRouteDetails()
        {
            if (_routeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a route to view.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedRouteId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
            var route = _routes.FirstOrDefault(r => r.RouteID == selectedRouteId);

            if (route != null)
            {
                var details = $"Route Details\n\n" +
                    $"Date: {route.Date:yyyy-MM-dd}\n" +
                    $"Route Name: {route.RouteName ?? "N/A"}\n\n" +
                    $"AM SCHEDULE:\n" +
                    $"Vehicle: {route.AMVehicleNumber ?? "N/A"}\n" +
                    $"Driver: {route.AMDriverName ?? "N/A"}\n" +
                    $"Begin Miles: {route.AMBeginMiles?.ToString("N0") ?? "N/A"}\n" +
                    $"End Miles: {route.AMEndMiles?.ToString("N0") ?? "N/A"}\n" +
                    $"Riders: {route.AMRiders?.ToString() ?? "N/A"}\n\n" +
                    $"PM SCHEDULE:\n" +
                    $"Vehicle: {route.PMVehicleNumber ?? "N/A"}\n" +
                    $"Driver: {route.PMDriverName ?? "N/A"}\n" +
                    $"Begin Miles: {route.PMBeginMiles?.ToString("N0") ?? "N/A"}\n" +
                    $"End Miles: {route.PMEndMiles?.ToString("N0") ?? "N/A"}\n" +
                    $"Riders: {route.PMRiders?.ToString() ?? "N/A"}";

                MessageBox.Show(details, "Route Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SearchRoutes()
        {
            var searchTerm = _searchBox.Text?.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateRouteGrid();
                return;
            }

            var filteredRoutes = _routes.Where(r =>
                (r.RouteName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.AMVehicleNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.PMVehicleNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.AMDriverName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (r.PMDriverName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            _routeGrid.DataSource = null;

            if (filteredRoutes.Any())
            {
                var displayData = filteredRoutes.Select(r => new
                {
                    RouteID = r.RouteID,
                    Date = r.Date.ToString("yyyy-MM-dd"),
                    RouteName = r.RouteName ?? "",
                    AMVehicle = r.AMVehicleNumber ?? "",
                    AMDriver = r.AMDriverName ?? "",
                    AMBeginMiles = r.AMBeginMiles?.ToString("N0") ?? "",
                    AMEndMiles = r.AMEndMiles?.ToString("N0") ?? "",
                    AMRiders = r.AMRiders?.ToString() ?? "",
                    PMVehicle = r.PMVehicleNumber ?? "",
                    PMDriver = r.PMDriverName ?? "",
                    PMBeginMiles = r.PMBeginMiles?.ToString("N0") ?? "",
                    PMEndMiles = r.PMEndMiles?.ToString("N0") ?? "",
                    PMRiders = r.PMRiders?.ToString() ?? ""
                }).ToList();

                _routeGrid.DataSource = displayData;

                // Hide ID column
                if (_routeGrid.Columns.Contains("RouteID"))
                    _routeGrid.Columns["RouteID"].Visible = false;
            }
        }

        private void SaveRoute()
        {
            try
            {
                var route = _currentRoute ?? new Route();

                // Basic information
                route.Date = _datePicker.Value.Date;
                route.RouteName = _routeNameTextBox.Text?.Trim();

                // AM Schedule
                if (_amVehicleComboBox.SelectedValue is Vehicle amVehicle)
                    route.AMVehicleID = amVehicle.VehicleID;

                if (_amDriverComboBox.SelectedValue is Driver amDriver)
                    route.AMDriverID = amDriver.DriverID;

                if (decimal.TryParse(_amBeginMilesTextBox.Text, out var amBeginMiles))
                    route.AMBeginMiles = amBeginMiles;

                if (decimal.TryParse(_amEndMilesTextBox.Text, out var amEndMiles))
                    route.AMEndMiles = amEndMiles;

                if (int.TryParse(_amRidersTextBox.Text, out var amRiders))
                    route.AMRiders = amRiders;

                // PM Schedule
                if (_pmVehicleComboBox.SelectedValue is Vehicle pmVehicle)
                    route.PMVehicleID = pmVehicle.VehicleID;

                if (_pmDriverComboBox.SelectedValue is Driver pmDriver)
                    route.PMDriverID = pmDriver.DriverID;

                if (decimal.TryParse(_pmBeginMilesTextBox.Text, out var pmBeginMiles))
                    route.PMBeginMiles = pmBeginMiles;

                if (decimal.TryParse(_pmEndMilesTextBox.Text, out var pmEndMiles))
                    route.PMEndMiles = pmEndMiles;

                if (int.TryParse(_pmRidersTextBox.Text, out var pmRiders))
                    route.PMRiders = pmRiders;

                // Validation
                if (string.IsNullOrWhiteSpace(route.RouteName))
                {
                    MessageBox.Show("Please enter a route name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save to repository
                if (_isEditing)
                {
                    _routeRepository.UpdateRoute(route);
                    MessageBox.Show("Route updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _routeRepository.AddRoute(route);
                    MessageBox.Show("Route added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                _editPanel.Visible = false;
                LoadRoutes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
            ClearEditPanel();
        }

        private void ClearEditPanel()
        {
            _datePicker.Value = DateTime.Today;
            _routeNameTextBox.Clear();
            _amVehicleComboBox.SelectedIndex = -1;
            _amDriverComboBox.SelectedIndex = -1;
            _amBeginMilesTextBox.Clear();
            _amEndMilesTextBox.Clear();
            _amRidersTextBox.Clear();
            _pmVehicleComboBox.SelectedIndex = -1;
            _pmDriverComboBox.SelectedIndex = -1;
            _pmBeginMilesTextBox.Clear();
            _pmEndMilesTextBox.Clear();
            _pmRidersTextBox.Clear();
        }

        private void PopulateEditPanel(Route route)
        {
            _datePicker.Value = route.Date;
            _routeNameTextBox.Text = route.RouteName;

            // AM Schedule
            var amVehicle = _vehicles.FirstOrDefault(v => v.VehicleID == route.AMVehicleID);
            if (amVehicle != null)
                _amVehicleComboBox.SelectedValue = amVehicle;

            var amDriver = _drivers.FirstOrDefault(d => d.DriverID == route.AMDriverID);
            if (amDriver != null)
                _amDriverComboBox.SelectedValue = amDriver;

            _amBeginMilesTextBox.Text = route.AMBeginMiles?.ToString() ?? "";
            _amEndMilesTextBox.Text = route.AMEndMiles?.ToString() ?? "";
            _amRidersTextBox.Text = route.AMRiders?.ToString() ?? "";

            // PM Schedule
            var pmVehicle = _vehicles.FirstOrDefault(v => v.VehicleID == route.PMVehicleID);
            if (pmVehicle != null)
                _pmVehicleComboBox.SelectedValue = pmVehicle;

            var pmDriver = _drivers.FirstOrDefault(d => d.DriverID == route.PMDriverID);
            if (pmDriver != null)
                _pmDriverComboBox.SelectedValue = pmDriver;

            _pmBeginMilesTextBox.Text = route.PMBeginMiles?.ToString() ?? "";
            _pmEndMilesTextBox.Text = route.PMEndMiles?.ToString() ?? "";
            _pmRidersTextBox.Text = route.PMRiders?.ToString() ?? "";
        }
    }
}
