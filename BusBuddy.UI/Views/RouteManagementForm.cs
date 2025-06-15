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
                    Date = r.Date,
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
        // Removed legacy edit panel - now using RouteEditForm modal dialog

        private void RouteGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _routeGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void AddNewRoute()
        {
            try
            {
                using var editForm = new RouteEditForm();
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Route != null)
                {
                    _routeRepository.AddRoute(editForm.Route);
                    MessageBox.Show("Route added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRoutes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedRoute()
        {
            if (_routeGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a route to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var selectedRouteId = (int)_routeGrid.SelectedRows[0].Cells["RouteID"].Value;
                var routeToEdit = _routes.FirstOrDefault(r => r.RouteID == selectedRouteId);

                if (routeToEdit != null)
                {
                    using var editForm = new RouteEditForm(routeToEdit);
                    editForm.StartPosition = FormStartPosition.CenterParent;

                    if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Route != null)
                    {
                        _routeRepository.UpdateRoute(editForm.Route);
                        MessageBox.Show("Route updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRoutes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    Date = r.Date,
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
    }
}
