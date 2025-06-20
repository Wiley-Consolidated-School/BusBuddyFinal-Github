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
    /// School Calendar Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing school calendar events with grid view and CRUD operations
    /// </summary>
    public class SchoolCalendarManagementFormSyncfusion : BaseManagementForm<SchoolCalendar>
    {
        private readonly ISchoolCalendarRepository _schoolCalendarRepository;

        #region Properties Override
        protected override string FormTitle => "📅 School Calendar Management";
        protected override string SearchPlaceholder => "Search calendar events...";
        protected override string EntityName => "SchoolCalendar";
        #endregion

        #region Constructors
        public SchoolCalendarManagementFormSyncfusion() : this(new SchoolCalendarRepository()) { }

        public SchoolCalendarManagementFormSyncfusion(ISchoolCalendarRepository schoolCalendarRepository)
        {
            _schoolCalendarRepository = schoolCalendarRepository ?? throw new ArgumentNullException(nameof(schoolCalendarRepository));
            LoadData();
        }
        #endregion

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                _entities = _schoolCalendarRepository.GetAllCalendarEvents().ToList();
                PopulateCalendarGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading school calendar events: {ex.Message}");
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                var calendarForm = new SchoolCalendarEditFormSyncfusion();
                if (calendarForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding new calendar event: {ex.Message}");
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfoMessage("Please select a calendar event to edit.");
                return;
            }

            try
            {
                var calendarForm = new SchoolCalendarEditFormSyncfusion(selectedEvent);
                if (calendarForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing calendar event: {ex.Message}");
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfoMessage("Please select a calendar event to delete.");
                return;
            }

            if (!ConfirmDelete("calendar event")) return;

            try
            {
                _schoolCalendarRepository.DeleteCalendarEvent(selectedEvent.CalendarID);
                RefreshGrid();
                ShowInfoMessage("Calendar event deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting calendar event: {ex.Message}");
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfoMessage("Please select a calendar event to view details.");
                return;
            }

            try
            {
                var details = $"School Calendar Event Details:\n\n" +
                            $"ID: {selectedEvent.CalendarID}\n" +
                            $"Event: {selectedEvent.EventName}\n" +
                            $"Date: {selectedEvent.EventDate}\n" +
                            $"Type: {selectedEvent.EventType}\n" +
                            $"Description: {selectedEvent.EventDescription}\n" +
                            $"Location: {selectedEvent.EventLocation}\n" +
                            $"All Day: {(selectedEvent.IsAllDay ? "Yes" : "No")}\n" +
                            $"Start Time: {selectedEvent.StartTime}\n" +
                            $"End Time: {selectedEvent.EndTime}";

                ShowInfoMessage(details, "Calendar Event Details");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error viewing calendar event details: {ex.Message}");
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

                var filteredEvents = _entities.Where(e =>
                    (e.EventName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (e.EventType?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (e.EventDescription?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (e.EventLocation?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                ).ToList();

                _entities = filteredEvents;
                PopulateCalendarGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error searching calendar events: {ex.Message}");
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();

            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "CalendarID", HeaderText = "ID", Visible = false });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EventName", HeaderText = "Event Name", Width = GetDpiAwareWidth(180) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EventDate", HeaderText = "Date", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EventType", HeaderText = "Type", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EventLocation", HeaderText = "Location", Width = GetDpiAwareWidth(150) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "StartTime", HeaderText = "Start Time", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "EndTime", HeaderText = "End Time", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "IsAllDay", HeaderText = "All Day", Width = GetDpiAwareWidth(80) });

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #endregion

        #region Helper Methods
        private void PopulateCalendarGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                var calendarData = _entities.Select(e => new
                {
                    CalendarID = e.CalendarID,
                    EventName = e.EventName ?? "Unknown",
                    EventDate = e.EventDate ?? "Unknown",
                    EventType = e.EventType ?? "Unknown",
                    EventLocation = e.EventLocation ?? "Unknown",
                    StartTime = e.StartTime ?? "Unknown",
                    EndTime = e.EndTime ?? "Unknown",
                    IsAllDay = e.IsAllDay ? "Yes" : "No"
                }).ToList();

                _dataGrid.DataSource = calendarData;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error populating calendar grid: {ex.Message}");
            }        }
        #endregion
    }
}
