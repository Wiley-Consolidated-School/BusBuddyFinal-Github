using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class ActivityManagementForm : StandardDataForm
    {
        private readonly IActivityRepository _activityRepository;
        private DataGridView? _activityGrid;
        private Button? _addButton;
        private Button? _editButton;
        private Button? _deleteButton;
        private Button? _detailsButton;
        private MaterialSkin.Controls.MaterialTextBox _searchBox;
        private Button? _searchButton;
        private List<Activity> _activities = new List<Activity>();        public ActivityManagementForm() : this(new ActivityRepository()) { }

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

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void ActivityGrid_SelectionChanged(object? sender, EventArgs e)
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
            try
            {
                using (var addForm = new ActivityEditFormSyncfusion())
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        var newActivity = addForm.Activity;
                        var activityId = _activityRepository.AddActivity(newActivity);
                        if (activityId > 0)
                        {
                            LoadActivities();
                            ShowSuccessMessage("Activity added successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to add activity. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding activity: {ex.Message}");
            }
        }

        private void EditSelectedActivity()
        {
            if (_activityGrid.SelectedRows.Count == 0)
            {
                ShowWarningMessage("Please select an activity to edit.");
                return;
            }

            try
            {
                int selectedId = (int)_activityGrid.SelectedRows[0].Cells["ActivityID"].Value;
                var selectedActivity = _activityRepository.GetActivityById(selectedId);

                if (selectedActivity == null)
                {
                    ShowErrorMessage("Could not find the selected activity.");
                    return;
                }

                using (var editForm = new ActivityEditFormSyncfusion(selectedActivity))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        var updatedActivity = editForm.Activity;
                        if (_activityRepository.UpdateActivity(updatedActivity))
                        {
                            LoadActivities();
                            ShowSuccessMessage("Activity updated successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to update activity. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing activity: {ex.Message}");
            }
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
    }
}
