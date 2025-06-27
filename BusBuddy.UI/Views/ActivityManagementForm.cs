using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.ListView;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Trips Management Form - Shell Structure
    /// Manages activity trip records with CRUD operations
    /// Based on Syncfusion SfDataGrid documentation
    ///
    /// 📖 SYNCFUSION DOCUMENTATION:
    /// - SfDataGrid: https://help.syncfusion.com/windowsforms/datagrid/getting-started
    /// - SfButton: https://help.syncfusion.com/windowsforms/button/getting-started
    /// </summary>
    public partial class ActivityManagementForm : BaseManagementForm<Activity>
    {
        #region Private Fields
        private readonly IActivityRepository _activityRepository;

        // Additional controls specific to Activity management
        private SfButton _exportButton;
        private SfButton _importButton;
        private ComboBoxAdv _activityTypeFilter;
        private SfDateTimeEdit _dateFromFilter;
        private SfDateTimeEdit _dateToFilter;
        #endregion

        #region Properties Override
        protected override string FormTitle => "🎯 Activity Trips Management";
        protected override string SearchPlaceholder => "Search activity trips...";
        protected override string EntityName => "Activity Trip";
        #endregion

        #region Constructors
        public ActivityManagementForm() : this(new ActivityRepository(), new MessageBoxService())
        {
        }

        public ActivityManagementForm(IActivityRepository activityRepository) : this(activityRepository, new MessageBoxService())
        {
        }

        public ActivityManagementForm(IActivityRepository activityRepository, IMessageService messageService) : base(messageService)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            SetRepository(_activityRepository);

            // Initialize additional controls (implementation will be added in next prompt)
            InitializeActivitySpecificControls();
        }
        #endregion

        #region Base Implementation Override - Shell Methods
        protected override void LoadDataFromRepository()
        {
            try
            {
                _entities = _activityRepository.GetAllActivities().ToList();
                PopulateDataGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error loading activities: {ex.Message}", "Activity Error", ex);
                _entities = new List<Activity>();
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                using (var addForm = new ActivityEditForm())
                {
                    if (addForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadDataFromRepository(); // Refresh the grid
                        _messageService.ShowInfo("Activity added successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error adding new activity: {ex.Message}", "Add Activity Error", ex);
            }
        }

        protected override void EditSelectedEntity()
        {
            try
            {
                var selectedActivity = GetSelectedEntity();
                if (selectedActivity == null)
                {
                    _messageService.ShowWarning("Please select an activity to edit.");
                    return;
                }

                using (var editForm = new ActivityEditForm(selectedActivity))
                {
                    if (editForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadDataFromRepository(); // Refresh the grid
                        _messageService.ShowInfo("Activity updated successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error editing activity: {ex.Message}", "Edit Activity Error", ex);
            }
        }

        protected override void DeleteSelectedEntity()
        {
            try
            {
                var selectedActivity = GetSelectedEntity();
                if (selectedActivity == null)
                {
                    _messageService.ShowWarning("Please select an activity to delete.");
                    return;
                }

                var confirmMessage = $"Are you sure you want to delete this activity?\n\n" +
                                   $"Type: {selectedActivity.ActivityType}\n" +
                                   $"Destination: {selectedActivity.Destination}\n" +
                                   $"Date: {selectedActivity.Date}\n\n" +
                                   "This action cannot be undone.";

                if (_messageService.ShowConfirmation(confirmMessage))
                {
                    var success = _activityRepository.DeleteActivity(selectedActivity.ActivityID);
                    if (success)
                    {
                        LoadDataFromRepository(); // Refresh the grid
                        _messageService.ShowInfo("Activity deleted successfully.");
                    }
                    else
                    {
                        _messageService.ShowError("Failed to delete activity. It may be referenced by other records.");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting activity: {ex.Message}", "Delete Activity Error", ex);
            }
        }

        protected override void ViewEntityDetails()
        {
            try
            {
                var selectedActivity = GetSelectedEntity();
                if (selectedActivity == null)
                {
                    _messageService.ShowWarning("Please select an activity to view.");
                    return;
                }

                using (var viewForm = new ActivityEditForm(selectedActivity, readOnlyMode: true))
                {
                    viewForm.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error viewing activity details: {ex.Message}", "View Activity Error", ex);
            }
        }

        protected override void SearchEntities()
        {
            try
            {
                var searchText = GetSearchText();
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _entities = _activityRepository.GetAllActivities().ToList();
                }
                else
                {
                    _entities = _activityRepository.GetAllActivities()
                        .Where(a =>
                            (a.ActivityType?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (a.Destination?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (a.Notes?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                            (a.RequestedBy?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        .ToList();
                }

                ApplyFilters(); // Apply any additional filters
                PopulateDataGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error searching activities: {ex.Message}", "Search Error", ex);
            }
        }

        private string GetSearchText()
        {
            return _searchBox?.Text?.Trim() ?? string.Empty;
        }

        private void ApplyFilters()
        {
            try
            {
                if (_entities == null) return;

                var filteredEntities = _entities.AsEnumerable();

                // Apply activity type filter
                if (_activityTypeFilter?.SelectedIndex > 0 && _activityTypeFilter.SelectedItem != null)
                {
                    var selectedType = _activityTypeFilter.SelectedItem.ToString();
                    if (selectedType != "All Types")
                    {
                        filteredEntities = filteredEntities.Where(a => a.ActivityType == selectedType);
                    }
                }

                // Apply date range filter
                if (_dateFromFilter != null && _dateToFilter != null)
                {
                    var fromDate = _dateFromFilter.Value?.Date ?? DateTime.MinValue;
                    var toDate = _dateToFilter.Value?.Date.AddDays(1) ?? DateTime.MaxValue;

                    filteredEntities = filteredEntities.Where(a =>
                    {
                        var activityDate = a.DateAsDateTime?.Date ?? DateTime.MinValue;
                        return activityDate >= fromDate && activityDate < toDate;
                    });
                }

                _entities = filteredEntities.ToList();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying filters: {ex.Message}", "Filter Error", ex);
            }
        }

        protected override void SetupDataGridColumns()
        {
            try
            {
                if (_dataGrid != null)
                {
                    _dataGrid.Columns.Clear();

                    // Activity ID column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "ActivityID",
                        HeaderText = "ID",
                        Width = 60,
                        AllowResizing = false
                    });

                    // Date column
                    _dataGrid.Columns.Add(new GridDateTimeColumn()
                    {
                        MappingName = "Date",
                        HeaderText = "Date",
                        Width = 100,
                        Format = "d"
                    });

                    // Activity Type column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "ActivityType",
                        HeaderText = "Type",
                        Width = 120
                    });

                    // Destination column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "Destination",
                        HeaderText = "Destination",
                        Width = 150
                    });

                    // Leave Time column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "LeaveTime",
                        HeaderText = "Leave Time",
                        Width = 80
                    });

                    // Event Time column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "EventTime",
                        HeaderText = "Event Time",
                        Width = 80
                    });

                    // Return Time column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "ReturnTime",
                        HeaderText = "Return Time",
                        Width = 80
                    });

                    // Requested By column
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "RequestedBy",
                        HeaderText = "Requested By",
                        Width = 120
                    });

                    // Notes column (truncated for display)
                    _dataGrid.Columns.Add(new GridTextColumn()
                    {
                        MappingName = "Notes",
                        HeaderText = "Notes",
                        Width = 200
                    });

                    // Configure grid appearance
                    _dataGrid.AllowSorting = true;
                    _dataGrid.AllowFiltering = true;
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error setting up data grid columns: {ex.Message}", "Grid Setup Error", ex);
            }
        }
        #endregion

        #region Activity-Specific Methods - Shell Implementation
        private void InitializeActivitySpecificControls()
        {
            // Export Button - SfButton per documentation
            _exportButton = new SfButton
            {
                Text = "Export",
                Size = new Size(75, 30)
            };

            // Import Button - SfButton
            _importButton = new SfButton
            {
                Text = "Import",
                Size = new Size(75, 30)
            };

            // Activity Type Filter - ComboBoxAdv
            _activityTypeFilter = new ComboBoxAdv
            {
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Date From Filter - SfDateTimeEdit
            _dateFromFilter = new SfDateTimeEdit
            {
                Size = new Size(120, 23)
            };

            // Date To Filter - SfDateTimeEdit
            _dateToFilter = new SfDateTimeEdit
            {
                Size = new Size(120, 23)
            };
        }

        private void PopulateDataGrid()
        {
            try
            {
                if (_dataGrid != null && _entities != null)
                {
                    // Create display objects for better grid binding
                    var displayData = _entities.Select(activity => new
                    {
                        ActivityID = activity.ActivityID,
                        Date = activity.DateAsDateTime?.ToString("d") ?? activity.Date,
                        ActivityType = activity.ActivityType ?? "Not Specified",
                        Destination = activity.Destination ?? "Not Specified",
                        LeaveTime = activity.LeaveTimeSpan?.ToString(@"hh\:mm") ?? activity.LeaveTime,
                        EventTime = activity.EventTimeSpan?.ToString(@"hh\:mm") ?? activity.EventTime,
                        ReturnTime = activity.ReturnTimeSpan?.ToString(@"hh\:mm") ?? activity.ReturnTime,
                        RequestedBy = activity.RequestedBy ?? "Unknown",
                        Notes = string.IsNullOrEmpty(activity.Notes) ? "" :
                               (activity.Notes.Length > 50 ? activity.Notes.Substring(0, 47) + "..." : activity.Notes)
                    }).ToList();

                    _dataGrid.DataSource = displayData;
                    _dataGrid.Refresh();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error populating data grid: {ex.Message}", "Grid Population Error", ex);
            }
        }

        private void SetupFilters()
        {
            try
            {
                // Populate activity type filter
                _activityTypeFilter.Items.Clear();
                _activityTypeFilter.Items.Add("All Types");
                _activityTypeFilter.Items.Add("Field Trip");
                _activityTypeFilter.Items.Add("Sports Event");
                _activityTypeFilter.Items.Add("Academic Competition");
                _activityTypeFilter.Items.Add("Community Service");
                _activityTypeFilter.Items.Add("Special Event");
                _activityTypeFilter.Items.Add("Transportation Service");
                _activityTypeFilter.SelectedIndex = 0;

                // Set default date range (last 30 days to future)
                _dateFromFilter.Value = DateTime.Today.AddDays(-30);
                _dateToFilter.Value = DateTime.Today.AddDays(365);

                // Wire up events
                _activityTypeFilter.SelectedIndexChanged += ActivityTypeFilter_SelectionChanged;
                _dateFromFilter.ValueChanged += DateFilter_ValueChanged;
                _dateToFilter.ValueChanged += DateFilter_ValueChanged;
            }
            catch (Exception ex)
            {
                HandleError($"Error setting up filters: {ex.Message}", "Filter Setup Error", ex);
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
                    saveDialog.DefaultExt = "csv";
                    saveDialog.FileName = $"Activities_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // TODO: Implement actual export logic here
                        _messageService.ShowInfo($"Export functionality not yet implemented.\nWould export to: {saveDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error exporting activities: {ex.Message}", "Export Error", ex);
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
                    openDialog.DefaultExt = "csv";

                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        // TODO: Implement actual import logic here
                        _messageService.ShowInfo($"Import functionality not yet implemented.\nWould import from: {openDialog.FileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error importing activities: {ex.Message}", "Import Error", ex);
            }
        }

        private void ActivityTypeFilter_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Reload data with filters applied
                LoadDataFromRepository();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying activity type filter: {ex.Message}", "Filter Error", ex);
            }
        }

        private void DateFilter_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Reload data with filters applied
                LoadDataFromRepository();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying date filter: {ex.Message}", "Filter Error", ex);
            }
        }
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of activity-specific resources
                _exportButton?.Dispose();
                _importButton?.Dispose();
                _activityTypeFilter?.Dispose();
                _dateFromFilter?.Dispose();
                _dateToFilter?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
