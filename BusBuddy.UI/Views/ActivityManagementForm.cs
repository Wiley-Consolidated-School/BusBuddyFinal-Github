using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class ActivityManagementForm : BaseDataForm
    {
        private readonly IActivityRepository _activityRepository;
        private DataGridView _activityGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private TextBox _searchBox;
        private Button _searchButton;
        private List<Activity> _activities = new List<Activity>();

        // Fields for add/edit
        private Panel _editPanel = null!;
        private DateTimePicker _datePicker = null!;
        private ComboBox _activityTypeComboBox = null!;
        private TextBox _destinationTextBox = null!;
        private TextBox _leaveTimeTextBox = null!;
        private TextBox _eventTimeTextBox = null!;
        private TextBox _returnTimeTextBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Activity? _currentActivity;
        private bool _isEditing = false;        public ActivityManagementForm() : this(new ActivityRepository()) { }

        public ActivityManagementForm(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            InitializeComponent();
            LoadActivities();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Activity Management"
            this.Text = "Activity Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewActivity());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedActivity());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedActivity());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewActivityDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchActivities());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _activityGrid = new DataGridView();
            _activityGrid.Location = new System.Drawing.Point(20, 60);
            _activityGrid.Size = new System.Drawing.Size(1150, 650);
            _activityGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _activityGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _activityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _activityGrid.ReadOnly = true;
            _activityGrid.AllowUserToAddRows = false;
            _activityGrid.AllowUserToDeleteRows = false;
            _activityGrid.MultiSelect = false;
            _activityGrid.AllowUserToResizeColumns = true;
            _activityGrid.AllowUserToResizeRows = true;
            _activityGrid.ScrollBars = ScrollBars.Both;
            _activityGrid.DataBindingComplete += (s, e) => {
                if (_activityGrid.Columns.Contains("ActivityID"))
                    _activityGrid.Columns["ActivityID"].Visible = false;
            };
            this.Controls.Add(_activityGrid);
            _activityGrid.CellDoubleClick += (s, e) => EditSelectedActivity();
            _activityGrid.SelectionChanged += ActivityGrid_SelectionChanged;

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

            // Activity form-specific fields: Date, Type, Destination, Leave/Event/Return Times
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(70, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_datePicker);

            var typeLabel = CreateLabel("Type:", 250, 15);
            _editPanel.Controls.Add(typeLabel);
            _activityTypeComboBox = new ComboBox();
            _activityTypeComboBox.Location = new System.Drawing.Point(300, 10);
            _activityTypeComboBox.Size = new System.Drawing.Size(150, 23);
            _activityTypeComboBox.Items.AddRange(new object[] { "Sports Trip", "Activity Trip" });
            _editPanel.Controls.Add(_activityTypeComboBox);

            var destLabel = CreateLabel("Destination:", 480, 15);
            _editPanel.Controls.Add(destLabel);
            _destinationTextBox = new TextBox();
            _destinationTextBox.Location = new System.Drawing.Point(570, 10);
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

            var returnLabel = CreateLabel("Return Time:", 390, 55);
            _editPanel.Controls.Add(returnLabel);
            _returnTimeTextBox = new TextBox();
            _returnTimeTextBox.Location = new System.Drawing.Point(480, 50);
            _returnTimeTextBox.Size = new System.Drawing.Size(80, 23);
            _editPanel.Controls.Add(_returnTimeTextBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveActivity());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void ActivityGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _activityGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void LoadActivities()
        {
            try
            {
                _activities = _activityRepository.GetAllActivities();
                _activityGrid.DataSource = null;
                _activityGrid.DataSource = _activities;
                if (_activityGrid.Columns.Count > 0)
                {
                    _activityGrid.Columns["ActivityID"].HeaderText = "ID";
                    _activityGrid.Columns["Date"].HeaderText = "Date";
                    _activityGrid.Columns["ActivityType"].HeaderText = "Type";
                    _activityGrid.Columns["Destination"].HeaderText = "Destination";
                    _activityGrid.Columns["LeaveTime"].HeaderText = "Leave";
                    _activityGrid.Columns["EventTime"].HeaderText = "Event";
                    _activityGrid.Columns["ReturnTime"].HeaderText = "Return";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading activities: {ex.Message}");
            }
        }

        private void AddNewActivity()
        {
            _isEditing = false;
            _currentActivity = new Activity();
            _datePicker.Value = DateTime.Today;
            _activityTypeComboBox.SelectedIndex = -1;
            _destinationTextBox.Text = string.Empty;
            _leaveTimeTextBox.Text = string.Empty;
            _eventTimeTextBox.Text = string.Empty;
            _returnTimeTextBox.Text = string.Empty;
            _editPanel.Visible = true;
        }

        private void EditSelectedActivity()
        {
            if (_activityGrid.SelectedRows.Count == 0)
                return;
            _isEditing = true;
            int selectedId = (int)_activityGrid.SelectedRows[0].Cells["ActivityID"].Value;
            _currentActivity = _activityRepository.GetActivityById(selectedId);
            if (_currentActivity == null)
            {
                ShowErrorMessage("Could not find the selected activity.");
                return;
            }
            _datePicker.Value = _currentActivity.Date ?? DateTime.Today;
            _activityTypeComboBox.SelectedItem = _currentActivity.ActivityType ?? string.Empty;
            _destinationTextBox.Text = _currentActivity.Destination ?? string.Empty;
            _leaveTimeTextBox.Text = _currentActivity.LeaveTime ?? string.Empty;
            _eventTimeTextBox.Text = _currentActivity.EventTime ?? string.Empty;
            _returnTimeTextBox.Text = _currentActivity.ReturnTime ?? string.Empty;
            _editPanel.Visible = true;
        }

        private void DeleteSelectedActivity()
        {
            if (_activityGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_activityGrid.SelectedRows[0].Cells["ActivityID"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _activityRepository.DeleteActivity(selectedId);
                LoadActivities();
                ShowSuccessMessage("Activity deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting activity: {ex.Message}");
            }
        }

        private void ViewActivityDetails()
        {
            if (_activityGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_activityGrid.SelectedRows[0].Cells["ActivityID"].Value;
            var activity = _activityRepository.GetActivityById(selectedId);
            if (activity != null)
            {
                MessageBox.Show($"Activity Details:\nType: {activity.ActivityType}\nDestination: {activity.Destination}\nLeave: {activity.LeaveTime}\nEvent: {activity.EventTime}\nReturn: {activity.ReturnTime}",
                    "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load activity details.");
            }
        }

        private void SaveActivity()
        {
            if (_currentActivity == null || !ValidateActivityForm())
                return;
            try
            {
                var activity = _currentActivity; // Null-checked above
                activity.Date = _datePicker.Value;
                activity.ActivityType = _activityTypeComboBox.SelectedItem?.ToString();
                activity.Destination = _destinationTextBox.Text.Trim();
                activity.LeaveTime = _leaveTimeTextBox.Text.Trim();
                activity.EventTime = _eventTimeTextBox.Text.Trim();
                activity.ReturnTime = _returnTimeTextBox.Text.Trim();
                if (_isEditing)
                {
                    _activityRepository.UpdateActivity(activity);
                    ShowSuccessMessage("Activity updated successfully.");
                }
                else
                {
                    _activityRepository.AddActivity(activity);
                    ShowSuccessMessage("Activity added successfully.");
                }
                LoadActivities();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving activity: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void SearchActivities()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadActivities();
                return;
            }
            List<Activity> filtered = _activities.FindAll(a =>
                (a.ActivityType?.ToLower().Contains(searchTerm) == true) ||
                (a.Destination?.ToLower().Contains(searchTerm) == true)
            );
            _activityGrid.DataSource = null;
            _activityGrid.DataSource = filtered;
        }

        private bool ValidateActivityForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (_activityTypeComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_activityTypeComboBox, "Select a type");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(_destinationTextBox.Text))
            {
                _errorProvider.SetError(_destinationTextBox, "Enter a destination");
                valid = false;
            }
            // Optionally validate time fields
            return valid;
        }
    }
}
