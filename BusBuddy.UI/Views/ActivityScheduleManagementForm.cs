using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class ActivityScheduleManagementForm : BaseDataForm
    {
        private readonly IActivityScheduleRepository _activityScheduleRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private DataGridView _activityScheduleGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private TextBox _searchBox;
        private Button _searchButton;
        private List<ActivitySchedule> _activitySchedules = new List<ActivitySchedule>();
        private List<Vehicle> _vehicles = new List<Vehicle>();
        private List<Driver> _drivers = new List<Driver>();

        public ActivityScheduleManagementForm() : this(new ActivityScheduleRepository(), new VehicleRepository(), new DriverRepository()) { }

        public ActivityScheduleManagementForm(IActivityScheduleRepository activityScheduleRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            _activityScheduleRepository = activityScheduleRepository ?? throw new ArgumentNullException(nameof(activityScheduleRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadVehiclesAndDrivers();
            LoadActivitySchedules();
        }        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Activity Schedule Management"
            this.Text = "Activity Schedule Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewActivitySchedule());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedActivitySchedule());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedActivitySchedule());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewActivityScheduleDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchActivitySchedules());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _activityScheduleGrid = new DataGridView();
            _activityScheduleGrid.Location = new System.Drawing.Point(20, 60);
            _activityScheduleGrid.Size = new System.Drawing.Size(1150, 650);
            _activityScheduleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _activityScheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _activityScheduleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _activityScheduleGrid.ReadOnly = true;
            _activityScheduleGrid.AllowUserToAddRows = false;
            _activityScheduleGrid.AllowUserToDeleteRows = false;
            _activityScheduleGrid.MultiSelect = false;
            _activityScheduleGrid.AllowUserToResizeColumns = true;
            _activityScheduleGrid.AllowUserToResizeRows = true;
            _activityScheduleGrid.ScrollBars = ScrollBars.Both;
            _activityScheduleGrid.DataBindingComplete += (s, e) => {
                if (_activityScheduleGrid.Columns.Contains("ActivityScheduleID"))
                    _activityScheduleGrid.Columns["ActivityScheduleID"].Visible = false;
            };
            this.Controls.Add(_activityScheduleGrid);
            _activityScheduleGrid.CellDoubleClick += (s, e) => EditSelectedActivitySchedule();
            _activityScheduleGrid.SelectionChanged += ActivityScheduleGrid_SelectionChanged;

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

        private void LoadActivitySchedules()
        {
            try
            {
                _activitySchedules = _activityScheduleRepository.GetAllScheduledActivities().ToList();
                _activityScheduleGrid.DataSource = null;
                _activityScheduleGrid.DataSource = _activitySchedules;
                if (_activityScheduleGrid.Columns.Count > 0)
                {
                    _activityScheduleGrid.Columns["ScheduleID"].HeaderText = "ID";
                    _activityScheduleGrid.Columns["Date"].HeaderText = "Date";
                    _activityScheduleGrid.Columns["TripType"].HeaderText = "Trip Type";
                    _activityScheduleGrid.Columns["ScheduledVehicleID"].HeaderText = "Vehicle";
                    _activityScheduleGrid.Columns["ScheduledDestination"].HeaderText = "Destination";
                    _activityScheduleGrid.Columns["ScheduledLeaveTime"].HeaderText = "Leave";
                    _activityScheduleGrid.Columns["ScheduledEventTime"].HeaderText = "Event";
                    _activityScheduleGrid.Columns["ScheduledRiders"].HeaderText = "Riders";
                    _activityScheduleGrid.Columns["ScheduledDriverID"].HeaderText = "Driver";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading activity schedules: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewActivitySchedule()
        {
            try
            {
                using var editForm = new ActivityScheduleEditForm(_vehicles, _drivers);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.ActivitySchedule != null)
                {
                    _activityScheduleRepository.AddScheduledActivity(editForm.ActivitySchedule);
                    MessageBox.Show("Activity schedule added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding activity schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an activity schedule to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
                var scheduleToEdit = _activityScheduleRepository.GetScheduledActivityById(selectedId);

                if (scheduleToEdit == null)
                {
                    MessageBox.Show("Could not find the selected activity schedule.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new ActivityScheduleEditForm(_vehicles, _drivers, scheduleToEdit);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.ActivitySchedule != null)
                {
                    _activityScheduleRepository.UpdateScheduledActivity(editForm.ActivitySchedule);
                    MessageBox.Show("Activity schedule updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActivitySchedules();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing activity schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;

            var result = MessageBox.Show("Are you sure you want to delete this activity schedule?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                _activityScheduleRepository.DeleteScheduledActivity(selectedId);
                LoadActivitySchedules();
                MessageBox.Show("Activity schedule deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting activity schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewActivityScheduleDetails()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
            var schedule = _activityScheduleRepository.GetScheduledActivityById(selectedId);
            if (schedule != null)
            {
                MessageBox.Show($"Activity Schedule Details:\nDate: {schedule.Date}\nTrip Type: {schedule.TripType}\nVehicle: {schedule.ScheduledVehicleID}\nDestination: {schedule.ScheduledDestination}\nLeave: {schedule.ScheduledLeaveTime}\nEvent: {schedule.ScheduledEventTime}\nRiders: {schedule.ScheduledRiders}\nDriver: {schedule.ScheduledDriverID}",
                    "Activity Schedule Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not load activity schedule details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActivityScheduleGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _activityScheduleGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }        private void SearchActivitySchedules()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadActivitySchedules();
                return;
            }
            List<ActivitySchedule> filtered = _activitySchedules.Where(a =>
                (a.TripType?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledDestination?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledVehicleID?.ToString().Contains(searchTerm) == true) ||
                (a.ScheduledDriverID?.ToString().Contains(searchTerm) == true)
            ).ToList();
            _activityScheduleGrid.DataSource = null;
            _activityScheduleGrid.DataSource = filtered;
        }
    }
}
