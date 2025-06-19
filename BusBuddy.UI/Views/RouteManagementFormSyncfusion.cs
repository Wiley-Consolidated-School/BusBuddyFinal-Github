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

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Route Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing routes with grid view and CRUD operations
    /// </summary>
    public class RouteManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private IDatabaseHelperService _databaseHelperService;
        private DataGridView? _routeGrid;
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
                MessageBox.Show($"Error initializing RouteManagementForm: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // Re-throw to prevent partial initialization
            }
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Route Management"
            this.Text = "🚌 Route Management";
            this.ClientSize = GetDpiAwareSize(new Size(1200, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(800, 600));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Create toolbar buttons
            _addButton = SyncfusionThemeHelper.CreateStyledButton("➕ Add New");
            _editButton = SyncfusionThemeHelper.CreateStyledButton("✏️ Edit");
            _deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            _detailsButton = SyncfusionThemeHelper.CreateStyledButton("👁️ Details");
            _searchButton = SyncfusionThemeHelper.CreateStyledButton("🔍 Search");

            // Create search textbox
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search routes...");

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
            _mainPanel.Controls.Add(_searchButton);

            // Create DataGridView
            _routeGrid = SyncfusionThemeHelper.CreateMaterialDataGrid();
            _routeGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _routeGrid.Size = GetDpiAwareSize(new Size(1150, 800));
            _routeGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply Syncfusion theming to grid
            SyncfusionThemeHelper.ApplyMaterialDataGrid(_routeGrid);

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
                _routeGrid.DoubleClick += (s, e) => EditSelectedRoute();
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
        }        private void SetupDataGridColumns()
        {
            if (_routeGrid == null) return;

            _routeGrid.AutoGenerateColumns = false;
            _routeGrid.Columns.Clear();

            // Add columns with DPI-aware widths
            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RouteID",
                DataPropertyName = "RouteID",
                HeaderText = "Route ID",
                Width = GetDpiAwareWidth(80),
                ReadOnly = true,
                Visible = false
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                DataPropertyName = "Date",
                HeaderText = "Date",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RouteName",
                DataPropertyName = "RouteName",
                HeaderText = "Route Name",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AMVehicleNumber",
                DataPropertyName = "AMVehicleNumber",
                HeaderText = "AM Vehicle",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AMDriverName",
                DataPropertyName = "AMDriverName",
                HeaderText = "AM Driver",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PMVehicleNumber",
                DataPropertyName = "PMVehicleNumber",
                HeaderText = "PM Vehicle",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PMDriverName",
                DataPropertyName = "PMDriverName",
                HeaderText = "PM Driver",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _routeGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                DataPropertyName = "Notes",
                HeaderText = "Notes",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
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
                System.Diagnostics.Debug.WriteLine("LoadRoutes: Starting to load routes...");

                if (_databaseHelperService == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadRoutes: DatabaseHelperService is null, creating new instance");
                    _databaseHelperService = new DatabaseHelperService();
                }

                System.Diagnostics.Debug.WriteLine("LoadRoutes: Calling GetAllRoutesWithDetails...");
                var loadedRoutes = _databaseHelperService?.GetAllRoutesWithDetails();
                _routes = loadedRoutes ?? new List<Route>();

                System.Diagnostics.Debug.WriteLine($"LoadRoutes: Loaded {_routes.Count} routes");

                System.Diagnostics.Debug.WriteLine("LoadRoutes: Calling PopulateRouteGrid...");
                PopulateRouteGrid();

                System.Diagnostics.Debug.WriteLine("LoadRoutes: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRoutes: Exception - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadRoutes: Stack trace - {ex.StackTrace}");
                _routes = new List<Route>(); // Ensure _routes is never null
                PopulateRouteGrid(); // Still populate the grid (empty)
                MessageBox.Show($"Error loading routes: {ex.Message}\n\nDetails: {ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void PopulateRouteGrid()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("PopulateRouteGrid: Starting...");

                // Add comprehensive null checks
                if (_routeGrid == null)
                {
                    Console.WriteLine("ERROR: RouteGrid is null in PopulateRouteGrid");
                    return;
                }

                // Initialize lists if they're null
                _routes = _routes ?? new List<Route>();
                _vehicles = _vehicles ?? new List<Vehicle>();
                _drivers = _drivers ?? new List<Driver>();

                System.Diagnostics.Debug.WriteLine($"PopulateRouteGrid: About to populate with {_routes.Count} routes");

                _routeGrid.DataSource = null;

                if (_routes.Any())
                {
                    var routeData = _routes.Select(r => new
                    {
                        RouteID = r.RouteID,
                        Date = r.Date ?? "",
                        RouteName = r.RouteName ?? "",
                        AMVehicleNumber = _vehicles.FirstOrDefault(v => v.Id == r.AMVehicleID)?.VehicleNumber ?? "",
                        AMDriverName = _drivers.FirstOrDefault(d => d.DriverID == r.AMDriverID)?.Name ?? "",
                        PMVehicleNumber = _vehicles.FirstOrDefault(v => v.Id == r.PMVehicleID)?.VehicleNumber ?? "",
                        PMDriverName = _drivers.FirstOrDefault(d => d.DriverID == r.PMDriverID)?.Name ?? "",
                        Notes = r.Notes ?? ""
                    }).ToList();

                    _routeGrid.DataSource = routeData;
                    System.Diagnostics.Debug.WriteLine($"PopulateRouteGrid: Successfully populated with {routeData.Count} display records");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("PopulateRouteGrid: No routes to display");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PopulateRouteGrid: Exception - {ex.Message}");
                MessageBox.Show($"Error populating route grid: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RouteGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _routeGrid?.SelectedRows.Count > 0;
            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void AddNewRoute()
        {
            try
            {
                using (var routeForm = new RouteEditFormSyncfusion())
                {
                    if (routeForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadRoutes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening route form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedRoute()
        {
            if (_routeGrid?.SelectedRows.Count > 0)
            {
                try
                {
                    var selectedRow = _routeGrid.SelectedRows[0];
                    var routeId = (int)selectedRow.Cells["RouteID"].Value;
                    var route = _routes.FirstOrDefault(r => r.RouteID == routeId);

                    if (route != null)
                    {
                        using (var routeForm = new RouteEditFormSyncfusion(route))
                        {
                            if (routeForm.ShowDialog(this) == DialogResult.OK)
                            {
                                LoadRoutes();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Selected route not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error editing route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteSelectedRoute()
        {
            if (_routeGrid?.SelectedRows.Count > 0)
            {
                try
                {
                    var selectedRow = _routeGrid.SelectedRows[0];
                    var routeId = (int)selectedRow.Cells["RouteID"].Value;
                    var route = _routes.FirstOrDefault(r => r.RouteID == routeId);

                    if (route != null)
                    {
                        var result = MessageBox.Show(
                            $"Are you sure you want to delete the route '{route.RouteName}'?",
                            "Confirm Delete",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            _routeRepository.DeleteRoute(routeId);
                            LoadRoutes();
                            MessageBox.Show("Route deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewRouteDetails()
        {
            if (_routeGrid?.SelectedRows.Count > 0)
            {
                try
                {
                    var selectedRow = _routeGrid.SelectedRows[0];
                    var routeId = (int)selectedRow.Cells["RouteID"].Value;
                    var route = _routes.FirstOrDefault(r => r.RouteID == routeId);

                    if (route != null)
                    {
                        using (var routeForm = new RouteEditFormSyncfusion(route))
                        {
                            routeForm.ShowDialog(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error viewing route details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }        private void SearchRoutes()
        {
            if (_searchBox is TextBox searchBox)
            {
                var searchTerm = searchBox.Text?.Trim().ToLower() ?? "";

                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadRoutes();
                    return;
                }

                try
                {
                    var allRoutes = _databaseHelperService?.GetAllRoutesWithDetails() ?? new List<Route>();
                    _routes = allRoutes.Where(r =>
                        (r.RouteName?.ToLower().Contains(searchTerm) == true) ||
                        (r.AMVehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                        (r.AMDriverName?.ToLower().Contains(searchTerm) == true) ||
                        (r.PMVehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                        (r.PMDriverName?.ToLower().Contains(searchTerm) == true) ||
                        (r.Notes?.ToLower().Contains(searchTerm) == true)
                    ).ToList();

                    PopulateRouteGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching routes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Properties for testing access
#if DEBUG || TESTING
        public Button? AddButton => _addButton as Button;
        public Button? EditButton => _editButton as Button;
        public Button? DeleteButton => _deleteButton as Button;
        public Button? DetailsButton => _detailsButton as Button;
#endif
    }
}
