using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing activities with grid view and CRUD operations
    /// </summary>
    public class ActivityManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IActivityRepository _activityRepository;
        private DataGridView? _activityGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Activity> _activities = new List<Activity>();

        public ActivityManagementFormSyncfusion() : this(new ActivityRepository()) { }

        public ActivityManagementFormSyncfusion(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            InitializeComponent();
            LoadActivities();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Activity Management"
            this.Text = "🎯 Activity Management";
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
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search activities...");

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

            // Create DataGridView
            _activityGrid = CreateDataGrid();
            _activityGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _activityGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _activityGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply Syncfusion theming to grid
            SyncfusionThemeHelper.ApplyMaterialDataGrid(_activityGrid);

            _mainPanel.Controls.Add(_activityGrid);

            // Configure grid columns
            SetupDataGridColumns();
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewActivity();
            _editButton.Click += (s, e) => EditSelectedActivity();
            _deleteButton.Click += (s, e) => DeleteSelectedActivity();
            _detailsButton.Click += (s, e) => ViewActivityDetails();
            _searchButton.Click += (s, e) => SearchActivities();

            if (_activityGrid != null)
            {
                _activityGrid.SelectionChanged += ActivityGrid_SelectionChanged;
                _activityGrid.DoubleClick += (s, e) => EditSelectedActivity();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBox searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchActivities();
                        e.Handled = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_activityGrid == null) return;

            _activityGrid.AutoGenerateColumns = false;
            _activityGrid.Columns.Clear();

            // Add columns
            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActivityID",
                DataPropertyName = "ActivityID",
                HeaderText = "ID",
                Width = GetDpiAwareWidth(60),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                DataPropertyName = "Date",
                HeaderText = "Date",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ActivityType",
                DataPropertyName = "ActivityType",
                HeaderText = "Type",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Destination",
                DataPropertyName = "Destination",
                HeaderText = "Destination",
                Width = GetDpiAwareWidth(200),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LeaveTime",
                DataPropertyName = "LeaveTime",
                HeaderText = "Leave Time",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "EventTime",
                DataPropertyName = "EventTime",
                HeaderText = "Event Time",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ReturnTime",
                DataPropertyName = "ReturnTime",
                HeaderText = "Return Time",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestedBy",
                DataPropertyName = "RequestedBy",
                HeaderText = "Requested By",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _activityGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                DataPropertyName = "Notes",
                HeaderText = "Notes",
                Width = GetDpiAwareWidth(200),
                ReadOnly = true
            });

            // Configure grid behavior
            _activityGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _activityGrid.MultiSelect = false;
            _activityGrid.ReadOnly = true;
            _activityGrid.AllowUserToAddRows = false;
            _activityGrid.AllowUserToDeleteRows = false;
            _activityGrid.RowHeadersVisible = false;
        }

        private void LoadActivities()
        {            try
            {
                _activities = _activityRepository.GetAllActivities().ToList();
                UpdateGrid();
                Console.WriteLine($"📊 SYNCFUSION FORM: Loaded {_activities.Count} activities");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error loading activities: {ex.Message}");
                MessageBox.Show($"Error loading activities: {ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateGrid()
        {
            if (_activityGrid == null) return;

            _activityGrid.DataSource = null;
            _activityGrid.DataSource = _activities;
            _activityGrid.Refresh();

            // Update button states
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _activityGrid?.SelectedRows.Count > 0;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void ActivityGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void AddNewActivity()
        {
            try
            {
                using var form = new ActivityEditFormSyncfusion();
                if (form.ShowDialog() == DialogResult.OK && form.Activity != null)
                {
                    var newActivityId = _activityRepository.AddActivity(form.Activity);
                    LoadActivities(); // Refresh the grid
                    Console.WriteLine($"✅ SYNCFUSION FORM: Added new activity with ID: {newActivityId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error adding activity: {ex.Message}");
                MessageBox.Show($"Error adding activity: {ex.Message}", "Add Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedActivity()
        {
            if (_activityGrid?.SelectedRows.Count != 1) return;

            try
            {
                var selectedActivity = (Activity)_activityGrid.SelectedRows[0].DataBoundItem;
                using var form = new ActivityEditFormSyncfusion(selectedActivity);

                if (form.ShowDialog() == DialogResult.OK && form.Activity != null)
                {
                    _activityRepository.UpdateActivity(form.Activity);
                    LoadActivities(); // Refresh the grid
                    Console.WriteLine($"✅ SYNCFUSION FORM: Updated activity: {form.Activity.Destination}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error editing activity: {ex.Message}");
                MessageBox.Show($"Error editing activity: {ex.Message}", "Edit Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedActivity()
        {
            if (_activityGrid?.SelectedRows.Count != 1) return;

            try
            {
                var selectedActivity = (Activity)_activityGrid.SelectedRows[0].DataBoundItem;

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the activity to '{selectedActivity.Destination}' on {selectedActivity.Date}?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _activityRepository.DeleteActivity(selectedActivity.ActivityID);
                    LoadActivities(); // Refresh the grid
                    Console.WriteLine($"🗑️ SYNCFUSION FORM: Deleted activity: {selectedActivity.Destination}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error deleting activity: {ex.Message}");
                MessageBox.Show($"Error deleting activity: {ex.Message}", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewActivityDetails()
        {
            if (_activityGrid?.SelectedRows.Count != 1) return;

            try
            {
                var selectedActivity = (Activity)_activityGrid.SelectedRows[0].DataBoundItem;

                var details = $"Activity Details:\n\n" +
                             $"ID: {selectedActivity.ActivityID}\n" +
                             $"Date: {selectedActivity.Date}\n" +
                             $"Type: {selectedActivity.ActivityType}\n" +
                             $"Destination: {selectedActivity.Destination}\n" +
                             $"Leave Time: {selectedActivity.LeaveTime}\n" +
                             $"Event Time: {selectedActivity.EventTime}\n" +
                             $"Return Time: {selectedActivity.ReturnTime}\n" +
                             $"Requested By: {selectedActivity.RequestedBy}\n" +
                             $"Notes: {selectedActivity.Notes}";

                MessageBox.Show(details, "Activity Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error viewing details: {ex.Message}");
                MessageBox.Show($"Error viewing activity details: {ex.Message}", "Details Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchActivities()
        {
            try
            {
                if (_searchBox is TextBox searchTb)
                {
                    string searchTerm = searchTb.Text.Trim().ToLower();

                    if (string.IsNullOrEmpty(searchTerm) || searchTerm == "search activities...")
                    {
                        // Show all activities
                        _activities = _activityRepository.GetAllActivities().ToList();
                    }
                    else
                    {
                        // Filter activities
                        var allActivities = _activityRepository.GetAllActivities().ToList();
                        _activities = allActivities.Where(a =>
                            (a.Destination?.ToLower().Contains(searchTerm) ?? false) ||
                            (a.ActivityType?.ToLower().Contains(searchTerm) ?? false) ||
                            (a.RequestedBy?.ToLower().Contains(searchTerm) ?? false) ||
                            (a.Notes?.ToLower().Contains(searchTerm) ?? false) ||
                            a.Date.ToLower().Contains(searchTerm)
                        ).ToList();
                    }

                    UpdateGrid();
                    Console.WriteLine($"🔍 SYNCFUSION FORM: Search returned {_activities.Count} activities");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error searching activities: {ex.Message}");
                MessageBox.Show($"Error searching activities: {ex.Message}", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Console.WriteLine($"🔄 SYNCFUSION FORM: {this.Text} closing");
            base.OnFormClosing(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Adjust grid size when form is resized
            if (_activityGrid != null && this.WindowState != FormWindowState.Minimized)
            {
                var newSize = new Size(
                    this.ClientSize.Width - GetDpiAwareWidth(40),
                    this.ClientSize.Height - GetDpiAwareHeight(120)
                );
                _activityGrid.Size = newSize;
            }
        }
    }
}
