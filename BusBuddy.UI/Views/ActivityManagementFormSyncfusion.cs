using System;
using System.Collections.Generic;
using System.Drawing;
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
    /// Activity Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing activities with grid view and CRUD operations
    /// </summary>
    public class ActivityManagementFormSyncfusion : BaseManagementForm<Activity>
    {
        private readonly IActivityRepository _activityRepository;

        #region Properties Override
        protected override string FormTitle => "🎯 Activity Management";
        protected override string SearchPlaceholder => "Search activities...";
        protected override string EntityName => "Activity";
        #endregion

        #region Constructors
        public ActivityManagementFormSyncfusion() : this(new ActivityRepository()) { }

        public ActivityManagementFormSyncfusion(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
            // NOTE: LoadData() is called by the base class after all controls are initialized
        }
        #endregion

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                var activities = _activityRepository.GetAllActivities();
                _entities = activities?.ToList() ?? new List<Activity>();
                PopulateActivityGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading activities: {ex.Message}");
                _entities = new List<Activity>(); // Ensure _entities is never null
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                var activityForm = new ActivityEditFormSyncfusion();
                if (activityForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding new activity: {ex.Message}");
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedActivity = GetSelectedEntity();
            if (selectedActivity == null)
            {
                ShowInfoMessage("Please select an activity to edit.");
                return;
            }

            try
            {
                var activityForm = new ActivityEditFormSyncfusion(selectedActivity);
                if (activityForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing activity: {ex.Message}");
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedActivity = GetSelectedEntity();
            if (selectedActivity == null)
            {
                ShowInfoMessage("Please select an activity to delete.");
                return;
            }

            if (!ConfirmDelete("activity")) return;

            try
            {
                _activityRepository.DeleteActivity(selectedActivity.ActivityID);
                RefreshGrid();
                ShowInfoMessage("Activity deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting activity: {ex.Message}");
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedActivity = GetSelectedEntity();
            if (selectedActivity == null)
            {
                ShowInfoMessage("Please select an activity to view details.");
                return;
            }

            try
            {
                var details = $"Activity Details:\n\n" +
                            $"ID: {selectedActivity.ActivityID}\n" +
                            $"Type: {selectedActivity.ActivityType}\n" +
                            $"Date: {selectedActivity.Date}\n" +
                            $"Event Time: {selectedActivity.EventTime}\n" +
                            $"Leave Time: {selectedActivity.LeaveTime}\n" +
                            $"Return Time: {selectedActivity.ReturnTime}\n" +
                            $"Destination: {selectedActivity.Destination}";

                ShowInfoMessage(details, "Activity Details");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error viewing activity details: {ex.Message}");
            }
        }

        protected override void SearchEntities()
        {
            if (_searchBox?.Text == null) return;

            try
            {
                var searchTerm = _searchBox.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm) || searchTerm == SearchPlaceholder)
                {
                    LoadData();
                    return;
                }

                // Ensure _entities is never null before LINQ operations
                if (_entities == null)
                {
                    _entities = new List<Activity>();
                }

                var filteredActivities = _entities.Where(a =>
                    (a.ActivityType?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (a.Destination?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (a.Date?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                ).ToList();

                _entities = filteredActivities;
                PopulateActivityGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error searching activities: {ex.Message}");
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();

            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "ActivityID", HeaderText = "ID", Visible = false });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "ActivityType", HeaderText = "Activity Type", Width = GetDpiAwareWidth(180) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Date", HeaderText = "Date", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EventTime", HeaderText = "Event Time", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "LeaveTime", HeaderText = "Leave Time", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "ReturnTime", HeaderText = "Return Time", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Destination", HeaderText = "Destination", Width = GetDpiAwareWidth(200) });

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #endregion

        #region Helper Methods
        private void PopulateActivityGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                // Ensure _entities is never null
                if (_entities == null)
                {
                    _entities = new List<Activity>();
                }

                var activityData = _entities.Select(a => new
                {
                    ActivityID = a.ActivityID,
                    ActivityType = a.ActivityType ?? "Unknown",
                    Date = a.Date ?? "Unknown",
                    EventTime = a.EventTime ?? "Unknown",
                    LeaveTime = a.LeaveTime ?? "Unknown",
                    ReturnTime = a.ReturnTime ?? "Unknown",
                    Destination = a.Destination ?? "Unknown"
                }).ToList();

                _dataGrid.DataSource = activityData;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error populating activity grid: {ex.Message}");
            }        }
        #endregion
    }
}
