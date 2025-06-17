using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Schedule Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing activity schedules with grid view and CRUD operations
    /// </summary>
    public class ActivityScheduleManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IActivityScheduleRepository _activityScheduleRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private DataGridView? _activityScheduleGrid;
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
            RefreshMaterialTheme();

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
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search schedules...");

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
            var searchLabel = CreateLabel("🔍 Search:", 500, 25);
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
            _mainPanel.Controls.Add(searchLabel);

            // Create and configure the data grid
            SetupDataGrid();
        }

        private void SetupDataGrid()
        {
            // Create DataGridView with modern styling
            _activityScheduleGrid = new DataGridView();
            _activityScheduleGrid.Dock = DockStyle.None;
            _activityScheduleGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(60));
            _activityScheduleGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _activityScheduleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Grid styling
            _activityScheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _activityScheduleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _activityScheduleGrid.ReadOnly = true;
            _activityScheduleGrid.AllowUserToAddRows = false;
            _activityScheduleGrid.AllowUserToDeleteRows = false;
            _activityScheduleGrid.MultiSelect = false;
            _activityScheduleGrid.AllowUserToResizeColumns = true;
            _activityScheduleGrid.AllowUserToResizeRows = true;
            _activityScheduleGrid.ScrollBars = ScrollBars.Both;

            // Apply Material theme to grid
            SyncfusionThemeHelper.ApplyMaterialTheme(_activityScheduleGrid);

            _mainPanel.Controls.Add(_activityScheduleGrid);
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls method for better organization
        }

        private void SetupEventHandlers()
        {
            // Button click events
            _addButton.Click += (s, e) => AddNewActivitySchedule();
            _editButton.Click += (s, e) => EditSelectedActivitySchedule();
            _deleteButton.Click += (s, e) => DeleteSelectedActivitySchedule();
            _detailsButton.Click += (s, e) => ViewActivityScheduleDetails();
            _searchButton.Click += (s, e) => SearchActivitySchedules();

            // Grid event handlers
            _activityScheduleGrid.CellDoubleClick += (s, e) => EditSelectedActivitySchedule();
            _activityScheduleGrid.SelectionChanged += ActivityScheduleGrid_SelectionChanged;
            _activityScheduleGrid.DataBindingComplete += ActivityScheduleGrid_DataBindingComplete;

            // Search box enter key handler
            if (_searchBox is TextBox searchTextBox)
            {
                searchTextBox.KeyPress += (s, e) => {
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        SearchActivitySchedules();
                        e.Handled = true;
                    }
                };
            }

            // Initial button states
            UpdateButtonStates();
        }

        private void ActivityScheduleGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_activityScheduleGrid.Columns.Contains("ActivityScheduleID"))
                _activityScheduleGrid.Columns["ActivityScheduleID"].Visible = false;

            // Configure column headers and widths
            ConfigureGridColumns();
        }

        private void ConfigureGridColumns()
        {
            if (_activityScheduleGrid.Columns.Count == 0) return;

            var columnConfig = new Dictionary<string, (string Header, int Width)>
            {
                ["ScheduleID"] = ("ID", 60),
                ["Date"] = ("Date", 100),
                ["TripType"] = ("Trip Type", 120),
                ["ScheduledVehicleID"] = ("Vehicle", 80),
                ["ScheduledDestination"] = ("Destination", 150),
                ["ScheduledLeaveTime"] = ("Leave Time", 100),
                ["ScheduledEventTime"] = ("Event Time", 100),
                ["ScheduledRiders"] = ("Riders", 80),
                ["ScheduledDriverID"] = ("Driver", 80)
            };

            foreach (var config in columnConfig)
            {
                if (_activityScheduleGrid.Columns.Contains(config.Key))
                {
                    var column = _activityScheduleGrid.Columns[config.Key];
                    column.HeaderText = config.Value.Header;
                    column.Width = GetDpiAwareSize(new Size(config.Value.Width, 0)).Width;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // Set remaining columns to fill
            if (_activityScheduleGrid.Columns.Count > 0)
            {
                _activityScheduleGrid.Columns[_activityScheduleGrid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
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
                MessageBox.Show($"Error loading vehicles and drivers: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadActivitySchedules()
        {
            try
            {
                _activitySchedules = _activityScheduleRepository.GetAllScheduledActivities().ToList();
                _activityScheduleGrid.DataSource = null;
                _activityScheduleGrid.DataSource = _activitySchedules;

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activity schedules: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewActivitySchedule()
        {
            try
            {
                using var editForm = new ActivityScheduleEditFormSyncfusion(_vehicles, _drivers);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.ActivitySchedule != null)
                {
                    _activityScheduleRepository.AddScheduledActivity(editForm.ActivitySchedule);
                    MessageBox.Show("Activity schedule added successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding activity schedule: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an activity schedule to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
                var scheduleToEdit = _activityScheduleRepository.GetScheduledActivityById(selectedId);

                if (scheduleToEdit == null)
                {
                    MessageBox.Show("Could not find the selected activity schedule.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new ActivityScheduleEditFormSyncfusion(_vehicles, _drivers, scheduleToEdit);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.ActivitySchedule != null)
                {
                    _activityScheduleRepository.UpdateScheduledActivity(editForm.ActivitySchedule);
                    MessageBox.Show("Activity schedule updated successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing activity schedule: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;

            var result = MessageBox.Show(
                "Are you sure you want to delete this activity schedule?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                _activityScheduleRepository.DeleteScheduledActivity(selectedId);
                LoadActivitySchedules();
                MessageBox.Show("Activity schedule deleted successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting activity schedule: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewActivityScheduleDetails()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
            var schedule = _activityScheduleRepository.GetScheduledActivityById(selectedId);

            if (schedule != null)
            {                var details = $"Activity Schedule Details:\n\n" +
                             $"📅 Date: {schedule.Date:yyyy-MM-dd}\n" +
                             $"🚌 Trip Type: {schedule.TripType}\n" +
                             $"🚐 Vehicle ID: {schedule.ScheduledVehicleID}\n" +
                             $"📍 Destination: {schedule.ScheduledDestination}\n" +
                             $"🕒 Leave Time: {schedule.ScheduledLeaveTime}\n" +
                             $"⏰ Event Time: {schedule.ScheduledEventTime}\n" +
                             $"👥 Riders: {schedule.ScheduledRiders}\n" +
                             $"👨‍✈️ Driver ID: {schedule.ScheduledDriverID}";

                MessageBox.Show(details, "Activity Schedule Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not load activity schedule details.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivityScheduleGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _activityScheduleGrid.SelectedRows.Count > 0;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void SearchActivitySchedules()
        {
            if (_searchBox is not TextBox searchTextBox)
                return;

            string searchTerm = searchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadActivitySchedules();
                return;
            }            var filtered = _activitySchedules.Where(a =>
                (a.TripType?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledDestination?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledVehicleID?.ToString().Contains(searchTerm) == true) ||
                (a.ScheduledDriverID?.ToString().Contains(searchTerm) == true) ||
                ($"{a.Date:yyyy-MM-dd}".Contains(searchTerm))
            ).ToList();

            _activityScheduleGrid.DataSource = null;
            _activityScheduleGrid.DataSource = filtered;

            UpdateButtonStates();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources if needed
            base.OnFormClosing(e);
        }
    }
}
