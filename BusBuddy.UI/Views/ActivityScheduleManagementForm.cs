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
        private DataGridView _activityScheduleGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private TextBox _searchBox;
        private Button _searchButton;
        private List<ActivitySchedule> _activitySchedules = new List<ActivitySchedule>();
        // Add/edit fields
        private Panel _editPanel = null!;
        private DateTimePicker _datePicker = null!;
        private ComboBox _tripTypeComboBox = null!;
        private ComboBox _vehicleComboBox = null!;
        private TextBox _destinationTextBox = null!;
        private TextBox _leaveTimeTextBox = null!;
        private TextBox _eventTimeTextBox = null!;
        private TextBox _ridersTextBox = null!;
        private ComboBox _driverComboBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private ActivitySchedule? _currentActivitySchedule;
        private bool _isEditing = false;

        public ActivityScheduleManagementForm()
        {
            _activityScheduleRepository = new ActivityScheduleRepository();
            InitializeComponent();
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

            // ActivitySchedule form-specific fields: Date, Trip Type, Vehicle, Destination, Leave/Event Times, Riders, Driver
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(70, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_datePicker);

            var tripTypeLabel = CreateLabel("Trip Type:", 250, 15);
            _editPanel.Controls.Add(tripTypeLabel);
            _tripTypeComboBox = new ComboBox();
            _tripTypeComboBox.Location = new System.Drawing.Point(330, 10);
            _tripTypeComboBox.Size = new System.Drawing.Size(120, 23);
            _tripTypeComboBox.Items.AddRange(new object[] { "Sports Trip", "Activity Trip" });
            _editPanel.Controls.Add(_tripTypeComboBox);

            var vehicleLabel = CreateLabel("Vehicle:", 480, 15);
            _editPanel.Controls.Add(vehicleLabel);
            _vehicleComboBox = new ComboBox();
            _vehicleComboBox.Location = new System.Drawing.Point(540, 10);
            _vehicleComboBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_vehicleComboBox);

            var destLabel = CreateLabel("Destination:", 710, 15);
            _editPanel.Controls.Add(destLabel);
            _destinationTextBox = new TextBox();
            _destinationTextBox.Location = new System.Drawing.Point(800, 10);
            _destinationTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_destinationTextBox);

            var leaveLabel = CreateLabel("Leave Time:", 10, 55);
            _editPanel.Controls.Add(leaveLabel);
            _leaveTimeTextBox = new TextBox();
            _leaveTimeTextBox.Location = new System.Drawing.Point(90, 50);
            _leaveTimeTextBox.Size = new System.Drawing.Size(80, 23);
            _editPanel.Controls.Add(_leaveTimeTextBox);

            var eventLabel = CreateLabel("Event Time:", 200, 55);
            _editPanel.Controls.Add(eventLabel);
            _eventTimeTextBox = new TextBox();
            _eventTimeTextBox.Location = new System.Drawing.Point(280, 50);
            _eventTimeTextBox.Size = new System.Drawing.Size(80, 23);
            _editPanel.Controls.Add(_eventTimeTextBox);

            var ridersLabel = CreateLabel("Riders:", 390, 55);
            _editPanel.Controls.Add(ridersLabel);
            _ridersTextBox = new TextBox();
            _ridersTextBox.Location = new System.Drawing.Point(450, 50);
            _ridersTextBox.Size = new System.Drawing.Size(80, 23);
            _editPanel.Controls.Add(_ridersTextBox);

            var driverLabel = CreateLabel("Driver:", 560, 55);
            _editPanel.Controls.Add(driverLabel);
            _driverComboBox = new ComboBox();
            _driverComboBox.Location = new System.Drawing.Point(620, 50);
            _driverComboBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_driverComboBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveActivitySchedule());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void LoadActivitySchedules()
        {
            try
            {
                _activitySchedules = _activityScheduleRepository.GetAllScheduledActivities();
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
                ShowErrorMessage($"Error loading activity schedules: {ex.Message}");
            }
        }

        private void AddNewActivitySchedule()
        {
            _isEditing = false;
            _currentActivitySchedule = new ActivitySchedule();
            _datePicker.Value = DateTime.Today;
            _tripTypeComboBox.SelectedIndex = -1;
            _vehicleComboBox.SelectedIndex = -1;
            _destinationTextBox.Text = string.Empty;
            _leaveTimeTextBox.Text = string.Empty;
            _eventTimeTextBox.Text = string.Empty;
            _ridersTextBox.Text = string.Empty;
            _driverComboBox.SelectedIndex = -1;
            _editPanel.Visible = true;
        }

        private void EditSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;
            _isEditing = true;
            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
            _currentActivitySchedule = _activityScheduleRepository.GetScheduledActivityById(selectedId);
            if (_currentActivitySchedule == null)
            {
                ShowErrorMessage("Could not find the selected activity schedule.");
                return;
            }
            _datePicker.Value = _currentActivitySchedule.Date ?? DateTime.Today;
            _tripTypeComboBox.SelectedItem = _currentActivitySchedule.TripType ?? string.Empty;
            _vehicleComboBox.SelectedItem = _currentActivitySchedule.ScheduledVehicleID?.ToString() ?? string.Empty;
            _destinationTextBox.Text = _currentActivitySchedule.ScheduledDestination ?? string.Empty;
            _leaveTimeTextBox.Text = _currentActivitySchedule.ScheduledLeaveTime?.ToString() ?? string.Empty;
            _eventTimeTextBox.Text = _currentActivitySchedule.ScheduledEventTime?.ToString() ?? string.Empty;
            _ridersTextBox.Text = _currentActivitySchedule.ScheduledRiders?.ToString() ?? string.Empty;
            _driverComboBox.SelectedItem = _currentActivitySchedule.ScheduledDriverID?.ToString() ?? string.Empty;
            _editPanel.Visible = true;
        }

        private void DeleteSelectedActivitySchedule()
        {
            if (_activityScheduleGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_activityScheduleGrid.SelectedRows[0].Cells["ScheduleID"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _activityScheduleRepository.DeleteScheduledActivity(selectedId);
                LoadActivitySchedules();
                ShowSuccessMessage("Activity schedule deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting activity schedule: {ex.Message}");
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
                ShowErrorMessage("Could not load activity schedule details.");
            }
        }

        private void SaveActivitySchedule()
        {
            if (_currentActivitySchedule == null || !ValidateActivityScheduleForm())
                return;
            try
            {
                var schedule = _currentActivitySchedule; // Null-checked above
                schedule.Date = _datePicker.Value;
                schedule.TripType = _tripTypeComboBox.SelectedItem?.ToString();
                schedule.ScheduledVehicleID = int.TryParse(_vehicleComboBox.SelectedItem?.ToString(), out int vid) ? vid : (int?)null;
                schedule.ScheduledDestination = _destinationTextBox.Text.Trim();
                schedule.ScheduledLeaveTime = TimeSpan.TryParse(_leaveTimeTextBox.Text.Trim(), out TimeSpan leave) ? leave : (TimeSpan?)null;
                schedule.ScheduledEventTime = TimeSpan.TryParse(_eventTimeTextBox.Text.Trim(), out TimeSpan evt) ? evt : (TimeSpan?)null;
                schedule.ScheduledRiders = int.TryParse(_ridersTextBox.Text.Trim(), out int riders) ? riders : (int?)null;
                schedule.ScheduledDriverID = int.TryParse(_driverComboBox.SelectedItem?.ToString(), out int did) ? did : (int?)null;
                if (_isEditing)
                {
                    _activityScheduleRepository.UpdateScheduledActivity(schedule);
                    ShowSuccessMessage("Activity schedule updated successfully.");
                }
                else
                {
                    _activityScheduleRepository.AddScheduledActivity(schedule);
                    ShowSuccessMessage("Activity schedule added successfully.");
                }
                LoadActivitySchedules();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving activity schedule: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void ActivityScheduleGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _activityScheduleGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }        private bool ValidateActivityScheduleForm()
        {
            _errorProvider.Clear();
            bool valid = true;

            // Validate required fields using FormValidator
            valid &= FormValidator.ValidateRequiredField(_tripTypeComboBox, "Trip Type", _errorProvider);
            valid &= FormValidator.ValidateRequiredField(_vehicleComboBox, "Vehicle", _errorProvider);
            valid &= FormValidator.ValidateRequiredField(_driverComboBox, "Driver", _errorProvider);
            valid &= FormValidator.ValidateRequiredField(_destinationTextBox, "Destination", _errorProvider);

            // Validate time format for leave time
            valid &= FormValidator.ValidateTime(_leaveTimeTextBox, "Leave Time", _errorProvider);

            // Validate time format for event time
            valid &= FormValidator.ValidateTime(_eventTimeTextBox, "Event Time", _errorProvider);

            // Validate time range (leave time should be before event time)
            if (!string.IsNullOrWhiteSpace(_leaveTimeTextBox.Text) && !string.IsNullOrWhiteSpace(_eventTimeTextBox.Text))
            {
                valid &= FormValidator.ValidateTimeRange(_leaveTimeTextBox, _eventTimeTextBox, _errorProvider);
            }

            // Validate riders count
            valid &= FormValidator.ValidateIntegerField(_ridersTextBox, "Riders", _errorProvider);

            // Validate riders count range (reasonable number)
            if (!string.IsNullOrWhiteSpace(_ridersTextBox.Text) && int.TryParse(_ridersTextBox.Text, out int riders))
            {
                if (riders < 0 || riders > 100)
                {
                    _errorProvider.SetError(_ridersTextBox, "Riders count must be between 0 and 100");
                    valid = false;
                }
            }

            return valid;
        }

        private void SearchActivitySchedules()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadActivitySchedules();
                return;
            }
            List<ActivitySchedule> filtered = _activitySchedules.FindAll(a =>
                (a.TripType?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledDestination?.ToLower().Contains(searchTerm) == true) ||
                (a.ScheduledVehicleID?.ToString().Contains(searchTerm) == true) ||
                (a.ScheduledDriverID?.ToString().Contains(searchTerm) == true)
            );
            _activityScheduleGrid.DataSource = null;
            _activityScheduleGrid.DataSource = filtered;
        }
    }
}
