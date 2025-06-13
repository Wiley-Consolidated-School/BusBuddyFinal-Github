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
        private List<ActivitySchedule> _activitySchedules;
        // Add/edit fields
        private Panel _editPanel;
        private DateTimePicker _datePicker;
        private ComboBox _tripTypeComboBox;
        private ComboBox _vehicleComboBox;
        private TextBox _destinationTextBox;
        private TextBox _leaveTimeTextBox;
        private TextBox _eventTimeTextBox;
        private TextBox _ridersTextBox;
        private ComboBox _driverComboBox;
        private Button _saveButton;
        private Button _cancelButton;
        private ActivitySchedule _currentActivitySchedule;
        private bool _isEditing = false;

        public ActivityScheduleManagementForm()
        {
            _activityScheduleRepository = new ActivityScheduleRepository();
            InitializeComponent();
            LoadActivitySchedules();
        }        private void InitializeComponent()
        {
            this.Text = "Activity Schedule Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewActivitySchedule());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedActivitySchedule());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedActivitySchedule());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewActivityScheduleDetails());
            _activityScheduleGrid = new DataGridView();
            _activityScheduleGrid.Location = new System.Drawing.Point(20, 60);
            _activityScheduleGrid.Size = new System.Drawing.Size(1150, 650);
            _activityScheduleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _activityScheduleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _activityScheduleGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
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
            InitializeEditPanel();
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 730);
            _editPanel.Size = new System.Drawing.Size(1150, 120);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);
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
            _saveButton = CreateButton("Save", 800, 60, (s, e) => SaveActivitySchedule());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 60, (s, e) => CancelEdit());
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
            if (!ValidateActivityScheduleForm())
                return;
            try
            {
                _currentActivitySchedule.Date = _datePicker.Value;
                _currentActivitySchedule.TripType = _tripTypeComboBox.SelectedItem?.ToString();
                _currentActivitySchedule.ScheduledVehicleID = int.TryParse(_vehicleComboBox.SelectedItem?.ToString(), out int vid) ? vid : (int?)null;
                _currentActivitySchedule.ScheduledDestination = _destinationTextBox.Text.Trim();
                _currentActivitySchedule.ScheduledLeaveTime = TimeSpan.TryParse(_leaveTimeTextBox.Text.Trim(), out TimeSpan leave) ? leave : (TimeSpan?)null;
                _currentActivitySchedule.ScheduledEventTime = TimeSpan.TryParse(_eventTimeTextBox.Text.Trim(), out TimeSpan evt) ? evt : (TimeSpan?)null;
                _currentActivitySchedule.ScheduledRiders = int.TryParse(_ridersTextBox.Text.Trim(), out int riders) ? riders : (int?)null;
                _currentActivitySchedule.ScheduledDriverID = int.TryParse(_driverComboBox.SelectedItem?.ToString(), out int did) ? did : (int?)null;
                if (_isEditing)
                {
                    _activityScheduleRepository.UpdateScheduledActivity(_currentActivitySchedule);
                    ShowSuccessMessage("Activity schedule updated successfully.");
                }
                else
                {
                    _activityScheduleRepository.AddScheduledActivity(_currentActivitySchedule);
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
        }

        private bool ValidateActivityScheduleForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (_tripTypeComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_tripTypeComboBox, "Select a trip type");
                valid = false;
            }
            if (_vehicleComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_vehicleComboBox, "Select a vehicle");
                valid = false;
            }
            if (_driverComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_driverComboBox, "Select a driver");
                valid = false;
            }
            return valid;
        }
    }
}
