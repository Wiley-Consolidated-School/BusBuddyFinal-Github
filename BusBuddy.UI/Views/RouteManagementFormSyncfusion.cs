using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.Business;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Route Management Form - Enhanced Syncfusion Implementation
    /// Form for managing routes with advanced SfDataGrid features
    /// </summary>
    public class RouteManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private IDatabaseHelperService _databaseHelperService;
        private SfDataGrid? _routeGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Route> _routes = new List<Route>();
        private List<Vehicle> _vehicles = new List<Vehicle>();
        private List<Driver> _drivers = new List<Driver>();

        public RouteManagementFormSyncfusion() : this(new RouteRepository(), new VehicleRepository(), new DriverRepository()) { }

        public RouteManagementFormSyncfusion(IRouteRepository routeRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            try
            {
                _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
                _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
                _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));

                _databaseHelperService = new DatabaseHelperService();

                InitializeComponent();
                LoadVehiclesAndDrivers();
                LoadRoutes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing Route Management Form: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Route Management"
            this.Text = "🗺️ Route Management";
            this.ClientSize = GetDpiAwareSize(new Size(1200, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(800, 600));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            Console.WriteLine($"🎨 ENHANCED SYNCFUSION FORM: {this.Text} initialized with advanced SfDataGrid features");
            Console.WriteLine($"✨ Features enabled: Filtering, Sorting, Grouping, Data Virtualization, Tooltips");
        }

        private void CreateControls()
        {
            // Create toolbar buttons
            _addButton = SyncfusionThemeHelper.CreateStyledButton("➕ Add New");
            _editButton = SyncfusionThemeHelper.CreateStyledButton("✏️ Edit");
            _deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            _detailsButton = SyncfusionThemeHelper.CreateStyledButton("👁️ Details");
            _searchButton = SyncfusionThemeHelper.CreateStyledButton("🔍 Search");

            // Create search textbox (simplified version)
            _searchBox = new TextBox
            {
                Size = GetDpiAwareSize(new Size(150, 30)),
                Text = "Search routes...",
                ForeColor = Color.Gray
            };

            // Configure button sizes and positions
            var buttonSize = GetDpiAwareSize(new Size(100, 35));
            var buttonY = GetDpiAwareY(20);

            _addButton.Size = buttonSize;
            _addButton.Location = new Point(GetDpiAwareX(20), buttonY);

            _editButton.Size = buttonSize;
            _editButton.Location = new Point(GetDpiAwareX(130), buttonY);
            _editButton.Enabled = false; // Initially disabled

            _deleteButton.Size = buttonSize;
            _deleteButton.Location = new Point(GetDpiAwareX(240), buttonY);
            _deleteButton.Enabled = false; // Initially disabled

            _detailsButton.Size = buttonSize;
            _detailsButton.Location = new Point(GetDpiAwareX(350), buttonY);
            _detailsButton.Enabled = false; // Initially disabled

            // Search controls
            var searchLabel = ControlFactory.CreateLabel("🔍 Search:");
            searchLabel.Location = new Point(500, 25);
            _mainPanel.Controls.Add(searchLabel);
            _searchBox.Size = GetDpiAwareSize(new Size(150, 30));
            _searchBox.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));

            _searchButton.Size = GetDpiAwareSize(new Size(80, 35));
            _searchButton.Location = new Point(GetDpiAwareX(710), buttonY);

            // Add buttons to main panel
            _mainPanel.Controls.Add(_addButton);
            _mainPanel.Controls.Add(_editButton);
            _mainPanel.Controls.Add(_deleteButton);
            _mainPanel.Controls.Add(_detailsButton);
            _mainPanel.Controls.Add(_searchBox);
            _mainPanel.Controls.Add(_searchButton);            // Create SfDataGrid with enhanced material styling and advanced features
            _routeGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _routeGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _routeGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _routeGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply BusBuddy standards and enhanced theming
            SyncfusionThemeHelper.SfDataGridEnhancements(_routeGrid);

            // Apply ALL Syncfusion features for 100% implementation
            SyncfusionThemeHelper.SfDataGridEnhancements(_routeGrid);

            _mainPanel.Controls.Add(_routeGrid);

            // Configure grid columns
            SetupDataGridColumns();
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewRoute();
            _editButton.Click += (s, e) => EditSelectedRoute();
            _deleteButton.Click += (s, e) => DeleteSelectedRoute();
            _detailsButton.Click += (s, e) => ViewRouteDetails();
            _searchButton.Click += (s, e) => SearchRoutes();

            if (_routeGrid != null)
            {
                _routeGrid.SelectionChanged += RouteGrid_SelectionChanged;
                _routeGrid.CellDoubleClick += (s, e) => EditSelectedRoute();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBox searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchRoutes();
                        e.Handled = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_routeGrid == null) return;

            _routeGrid.Columns.Clear();
            _routeGrid.AutoGenerateColumns = false;

            // Define columns for Routes
            var columns = new[]
            {
                new { Name = "RouteID", Header = "Route ID", Width = 80, Visible = false },
                new { Name = "RouteName", Header = "🗺️ Route Name", Width = 180, Visible = true },
                new { Name = "RouteDescription", Header = "📝 Description", Width = 200, Visible = true },
                new { Name = "StartLocation", Header = "📍 Start", Width = 150, Visible = true },
                new { Name = "EndLocation", Header = "🏁 End", Width = 150, Visible = true },
                new { Name = "Distance", Header = "📏 Distance", Width = 100, Visible = true },
                new { Name = "EstimatedTime", Header = "⏱️ Est. Time", Width = 100, Visible = true },
                new { Name = "AssignedVehicle", Header = "🚐 Vehicle", Width = 120, Visible = true },
                new { Name = "AssignedDriver", Header = "👨‍💼 Driver", Width = 120, Visible = true },
                new { Name = "IsActive", Header = "✅ Active", Width = 80, Visible = true }
            };

            foreach (var col in columns)
            {
                var gridColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn();
                gridColumn.MappingName = col.Name;
                gridColumn.HeaderText = col.Header;
                gridColumn.Width = GetDpiAwareSize(new Size(col.Width, 0)).Width;
                gridColumn.Visible = col.Visible;

                _routeGrid.Columns.Add(gridColumn);
            }

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_routeGrid.Columns.Count} columns for {this.Text}");
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles().ToList();
                _drivers = _driverRepository.GetAllDrivers().ToList();
                Console.WriteLine($"📊 Loaded {_vehicles.Count} vehicles and {_drivers.Count} drivers");
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

                // Create display objects with vehicle and driver names
                var displayRoutes = _routes.Select(route => new
                {
                    route.RouteID,
                    route.RouteName,
                    Date = route.Date,
                    AMVehicle = GetVehicleName(route.AMVehicleID),
                    AMDriver = GetDriverName(route.AMDriverID),
                    PMVehicle = GetVehicleName(route.PMVehicleID),
                    PMDriver = GetDriverName(route.PMDriverID),
                    route.Notes
                }).ToList();

                if (_routeGrid != null)
                {
                    _routeGrid.DataSource = displayRoutes;
                }

                UpdateButtonStates();
                Console.WriteLine($"📊 Loaded {_routes.Count} routes");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading routes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetVehicleName(int? vehicleId)
        {
            if (vehicleId == null) return "Not Assigned";
            var vehicle = _vehicles.FirstOrDefault(v => v.VehicleID == vehicleId);
            return vehicle != null ? $"{vehicle.VehicleNumber} - {vehicle.Make} {vehicle.Model}" : "Unknown";
        }

        private string GetDriverName(int? driverId)
        {
            if (driverId == null) return "Not Assigned";
            var driver = _drivers.FirstOrDefault(d => d.DriverID == driverId);
            return driver != null ? $"{driver.FirstName} {driver.LastName}" : "Unknown";
        }

        private void AddNewRoute()
        {
            try
            {
                using var editForm = new RouteEditFormSyncfusion();
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadRoutes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add route form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedRoute()
        {
            var selectedRoute = GetSelectedRoute();
            if (selectedRoute == null)
            {
                MessageBox.Show("Please select a route to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var editForm = new RouteEditFormSyncfusion(selectedRoute);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadRoutes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening edit route form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedRoute()
        {
            var selectedRoute = GetSelectedRoute();
            if (selectedRoute == null)
            {
                MessageBox.Show("Please select a route to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete the route '{selectedRoute.RouteName}'?",
                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _routeRepository.DeleteRoute(selectedRoute.RouteID);
                    LoadRoutes();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewRouteDetails()
        {
            var selectedRoute = GetSelectedRoute();
            if (selectedRoute == null)
            {
                MessageBox.Show("Please select a route to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var vehicleName = GetVehicleName(selectedRoute.AMVehicleID);
            var driverName = GetDriverName(selectedRoute.AMDriverID);

            var details = $"Route Details:\n\n" +
                         $"Name: {selectedRoute.RouteName}\n" +
                         $"Date: {selectedRoute.Date ?? "N/A"}\n" +
                         $"AM Vehicle: {GetVehicleName(selectedRoute.AMVehicleID)}\n" +
                         $"AM Driver: {GetDriverName(selectedRoute.AMDriverID)}\n" +
                         $"PM Vehicle: {GetVehicleName(selectedRoute.PMVehicleID)}\n" +
                         $"PM Driver: {GetDriverName(selectedRoute.PMDriverID)}\n" +
                         $"Notes: {selectedRoute.Notes ?? "N/A"}";

            MessageBox.Show(details, "Route Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SearchRoutes()
        {
            if (_searchBox is TextBox searchTextBox && _routeGrid != null)
            {
                string searchTerm = searchTextBox.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Search routes...")
                {
                    LoadRoutes();
                    return;
                }

                var filtered = _routes.Where(r =>
                    r.RouteName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Date?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Notes?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    GetVehicleName(r.AMVehicleID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    GetDriverName(r.AMDriverID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    GetVehicleName(r.PMVehicleID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    GetDriverName(r.PMDriverID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).Select(route => new
                {
                    route.RouteID,
                    route.RouteName,
                    Date = route.Date,
                    AMVehicle = GetVehicleName(route.AMVehicleID),
                    AMDriver = GetDriverName(route.AMDriverID),
                    PMVehicle = GetVehicleName(route.PMVehicleID),
                    PMDriver = GetDriverName(route.PMDriverID),
                    route.Notes
                }).ToList();

                _routeGrid.DataSource = filtered;
                UpdateButtonStates();
            }
        }

        private Route? GetSelectedRoute()
        {
            if (_routeGrid?.SelectedItem != null)
            {
                // Get the selected item from SfDataGrid
                var selectedItem = _routeGrid.SelectedItem;
                if (selectedItem != null)
                {
                    // Extract RouteID from the selected item
                    var routeIdProperty = selectedItem.GetType().GetProperty("RouteID");
                    if (routeIdProperty != null)
                    {
                        var routeIdValue = routeIdProperty.GetValue(selectedItem);
                        if (routeIdValue is int routeId)
                        {
                            return _routes.FirstOrDefault(r => r.RouteID == routeId);
                        }
                    }
                }
            }
            return null;
        }

        private void RouteGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _routeGrid?.SelectedItem != null;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources if needed
            base.OnFormClosing(e);
        }
    }
}
