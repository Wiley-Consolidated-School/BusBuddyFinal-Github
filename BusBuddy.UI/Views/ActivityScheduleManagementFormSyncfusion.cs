using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Schedule Management Form - Enhanced Syncfusion Implementation
    /// Form for managing activity schedules with advanced SfDataGrid features
    /// </summary>
    public class ActivityScheduleManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IActivityScheduleRepository _activityScheduleRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private SfDataGrid? _activityScheduleGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<ActivitySchedule> _activitySchedules = new List<ActivitySchedule>();
        private List<Vehicle> _vehicles = new List<Vehicle>();
        private List<Driver> _drivers = new List<Driver>();

        public ActivityScheduleManagementFormSyncfusion() : this(new ActivityScheduleRepository(), new VehicleRepository(), new DriverRepository()) { }

        public ActivityScheduleManagementFormSyncfusion(IActivityScheduleRepository activityScheduleRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            _activityScheduleRepository = activityScheduleRepository ?? throw new ArgumentNullException(nameof(activityScheduleRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadVehiclesAndDrivers();
            LoadActivitySchedules();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Activity Schedule Management"
            this.Text = "📅 Activity Schedule Management";
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
                Text = "Search schedules...",
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
            _mainPanel.Controls.Add(_searchButton);

            // Create SfDataGrid with enhanced material styling and advanced features
            _activityScheduleGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _activityScheduleGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _activityScheduleGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _activityScheduleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply BusBuddy standards and enhanced theming
            SyncfusionThemeHelper.SfDataGridEnhancements.ConfigureBusBuddyStandards(_activityScheduleGrid);

            _mainPanel.Controls.Add(_activityScheduleGrid);

            // Configure grid columns
            SetupDataGridColumns();
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewActivitySchedule();
            _editButton.Click += (s, e) => EditSelectedActivitySchedule();
            _deleteButton.Click += (s, e) => DeleteSelectedActivitySchedule();
            _detailsButton.Click += (s, e) => ViewActivityScheduleDetails();
            _searchButton.Click += (s, e) => SearchActivitySchedules();

            if (_activityScheduleGrid != null)
            {
                _activityScheduleGrid.SelectionChanged += ActivityScheduleGrid_SelectionChanged;
                _activityScheduleGrid.CellDoubleClick += (s, e) => EditSelectedActivitySchedule();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBox searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchActivitySchedules();
                        e.Handled = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_activityScheduleGrid == null) return;

            _activityScheduleGrid.Columns.Clear();
            _activityScheduleGrid.AutoGenerateColumns = false;

            // Define columns for Activity Schedules
            var columns = new[]
            {
                new { Name = "ScheduleID", Header = "Schedule ID", Width = 80, Visible = false },
                new { Name = "Date", Header = "📅 Date", Width = 120, Visible = true },
                new { Name = "TripType", Header = "🚌 Trip Type", Width = 120, Visible = true },
                new { Name = "ScheduledVehicleID", Header = "🚐 Vehicle", Width = 100, Visible = true },
                new { Name = "ScheduledDestination", Header = "📍 Destination", Width = 180, Visible = true },
                new { Name = "ScheduledLeaveTime", Header = "⏰ Leave Time", Width = 120, Visible = true },
                new { Name = "ScheduledEventTime", Header = "📅 Event Time", Width = 120, Visible = true },
                new { Name = "ScheduledRiders", Header = "👥 Riders", Width = 80, Visible = true },
                new { Name = "ScheduledDriverID", Header = "👨‍💼 Driver", Width = 100, Visible = true }
            };

            foreach (var col in columns)
            {
                var gridColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn();
                gridColumn.MappingName = col.Name;
                gridColumn.HeaderText = col.Header;
                gridColumn.Width = GetDpiAwareSize(new Size(col.Width, 0)).Width;
                gridColumn.Visible = col.Visible;

                _activityScheduleGrid.Columns.Add(gridColumn);
            }

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_activityScheduleGrid.Columns.Count} columns for {this.Text}");
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                _drivers = _driverRepository.GetAllDrivers();
                Console.WriteLine($"📊 Loaded {_vehicles.Count} vehicles and {_drivers.Count} drivers");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles and drivers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadActivitySchedules()
        {
            try
            {
                _activitySchedules = _activityScheduleRepository.GetAllScheduledActivities();

                // Create display objects with vehicle and driver names
                var displaySchedules = _activitySchedules.Select(schedule => new
                {
                    schedule.ScheduleID,
                    Date = schedule.DateAsDateTime?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? schedule.Date ?? "N/A",
                    schedule.TripType,
                    ScheduledVehicleID = GetVehicleName(schedule.ScheduledVehicleID),
                    schedule.ScheduledDestination,
                    ScheduledLeaveTime = schedule.ScheduledLeaveTime?.ToString(@"hh\:mm") ?? "N/A",
                    ScheduledEventTime = schedule.ScheduledEventTime?.ToString(@"hh\:mm") ?? "N/A",
                    schedule.ScheduledRiders,
                    ScheduledDriverID = GetDriverName(schedule.ScheduledDriverID)
                }).ToList();

                if (_activityScheduleGrid != null)
                {
                    _activityScheduleGrid.DataSource = displaySchedules;
                }

                UpdateButtonStates();
                Console.WriteLine($"📊 Loaded {_activitySchedules.Count} activity schedules");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activity schedules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetVehicleName(int? vehicleId)
        {
            if (vehicleId == null) return "N/A";
            var vehicle = _vehicles.FirstOrDefault(v => v.VehicleID == vehicleId);
            return vehicle != null ? $"{vehicle.VehicleNumber} - {vehicle.Make} {vehicle.Model}" : "Unknown";
        }

        private string GetDriverName(int? driverId)
        {
            if (driverId == null) return "N/A";
            var driver = _drivers.FirstOrDefault(d => d.DriverID == driverId);
            return driver != null ? $"{driver.FirstName} {driver.LastName}" : "Unknown";
        }

        private void AddNewActivitySchedule()
        {
            try
            {
                using var editForm = new ActivityScheduleEditFormSyncfusion(_vehicles, _drivers);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add schedule form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedActivitySchedule()
        {
            var selectedSchedule = GetSelectedActivitySchedule();
            if (selectedSchedule == null)
            {
                MessageBox.Show("Please select a schedule to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var editForm = new ActivityScheduleEditFormSyncfusion(_vehicles, _drivers, selectedSchedule);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening edit schedule form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedActivitySchedule()
        {
            var selectedSchedule = GetSelectedActivitySchedule();
            if (selectedSchedule == null)
            {
                MessageBox.Show("Please select a schedule to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete the schedule for {selectedSchedule.ScheduledDestination} on {selectedSchedule.Date:MM/dd/yyyy}?",
                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _activityScheduleRepository.DeleteScheduledActivity(selectedSchedule.ScheduleID);
                    LoadActivitySchedules();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewActivityScheduleDetails()
        {
            var selectedSchedule = GetSelectedActivitySchedule();
            if (selectedSchedule == null)
            {
                MessageBox.Show("Please select a schedule to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var vehicleName = GetVehicleName(selectedSchedule.ScheduledVehicleID);
            var driverName = GetDriverName(selectedSchedule.ScheduledDriverID);

            var details = $"Activity Schedule Details:\n\n" +
                         $"Date: {selectedSchedule.Date:MM/dd/yyyy}\n" +
                         $"Trip Type: {selectedSchedule.TripType}\n" +
                         $"Vehicle: {vehicleName}\n" +
                         $"Destination: {selectedSchedule.ScheduledDestination}\n" +
                         $"Leave Time: {selectedSchedule.ScheduledLeaveTime?.ToString("HH:mm") ?? "N/A"}\n" +
                         $"Event Time: {selectedSchedule.ScheduledEventTime?.ToString("HH:mm") ?? "N/A"}\n" +
                         $"Riders: {selectedSchedule.ScheduledRiders}\n" +
                         $"Driver: {driverName}";

            MessageBox.Show(details, "Schedule Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SearchActivitySchedules()
        {
            if (_searchBox is TextBox searchTextBox && _activityScheduleGrid != null)
            {
                string searchTerm = searchTextBox.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm) || searchTerm == "Search schedules...")
                {
                    LoadActivitySchedules();
                    return;
                }

                var filtered = _activitySchedules.Where(s =>
                    s.ScheduledDestination?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    s.TripType?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
                    GetVehicleName(s.ScheduledVehicleID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    GetDriverName(s.ScheduledDriverID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).Select(schedule => new
                {
                    schedule.ScheduleID,
                    Date = schedule.DateAsDateTime?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) ?? schedule.Date ?? "N/A",
                    schedule.TripType,
                    ScheduledVehicleID = GetVehicleName(schedule.ScheduledVehicleID),
                    schedule.ScheduledDestination,
                    ScheduledLeaveTime = schedule.ScheduledLeaveTime?.ToString(@"hh\:mm") ?? "N/A",
                    ScheduledEventTime = schedule.ScheduledEventTime?.ToString(@"hh\:mm") ?? "N/A",
                    schedule.ScheduledRiders,
                    ScheduledDriverID = GetDriverName(schedule.ScheduledDriverID)
                }).ToList();

                _activityScheduleGrid.DataSource = filtered;
                UpdateButtonStates();
            }
        }

        private ActivitySchedule? GetSelectedActivitySchedule()
        {
            if (_activityScheduleGrid?.SelectedItem != null)
            {
                // Get the selected item from SfDataGrid
                var selectedItem = _activityScheduleGrid.SelectedItem;
                if (selectedItem != null)
                {
                    // Extract ScheduleID from the selected item
                    var scheduleIdProperty = selectedItem.GetType().GetProperty("ScheduleID");
                    if (scheduleIdProperty != null)
                    {
                        var scheduleId = (int)(scheduleIdProperty.GetValue(selectedItem) ?? 0);
                        return _activitySchedules.FirstOrDefault(s => s.ScheduleID == scheduleId);
                    }
                }
            }
            return null;
        }

        private void ActivityScheduleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _activityScheduleGrid?.SelectedItem != null;

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
