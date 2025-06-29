using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.Input.Events;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// School Calendar Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing school calendar events with grid view and CRUD operations
    /// </summary>
    public class SchoolCalendarManagementFormSyncfusion : BaseManagementForm<SchoolCalendar>
    {
        private readonly ISchoolCalendarRepository _schoolCalendarRepository;

        // Additional Syncfusion Calendar Control
        private SfCalendar? _sfCalendar;
        private Panel? _calendarPanel;
        private Splitter? _splitter;
        #region Properties Override
        protected override string FormTitle => "📅 School Calendar Management";
        protected override string SearchPlaceholder => "Search calendar events...";
        protected override string EntityName => "SchoolCalendar";
        #region Constructors
        public SchoolCalendarManagementFormSyncfusion(System.IServiceProvider serviceProvider, ISchoolCalendarRepository schoolCalendarRepository, IMessageService messageService)
            : base(serviceProvider, messageService)
        {
            _schoolCalendarRepository = schoolCalendarRepository ?? throw new ArgumentNullException(nameof(schoolCalendarRepository));
        }

        private void InitializeComponent()
        {
            // Initialization code for components
        }

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                if (_schoolCalendarRepository == null)
                {
                    ShowErrorMessage("Error loading school calendar events: Repository not initialized.");
                    _entities = new List<SchoolCalendar>();
                    return;
                }
                var calendarEvents = _schoolCalendarRepository.GetAllCalendarEvents();
                _entities = calendarEvents?.ToList() ?? new List<SchoolCalendar>();
                PopulateCalendarGrid();
                UpdateCalendarSpecialDates();
            }
            catch (Exception ex)
            {
                HandleError($"Error loading school calendar events: {ex.Message}", "$($EntityName) Error", ex);
                _entities = new List<SchoolCalendar>(); // Ensure _entities is never null
            }
        }

        protected override void LoadDataFromRepository()
        {
            LoadData(); // Delegate to existing LoadData implementation
        }

        #region Enhanced Calendar Layout
        private void CreateEnhancedCalendarLayout()
        {
            try
            {
                // Create calendar panel
                _calendarPanel = new Panel
                {
                    Width = GetDpiAwareWidth(320),
                    Dock = DockStyle.Left,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = EnhancedThemeService.BackgroundColor
                };

                // Create SfCalendar
                _sfCalendar = new SfCalendar
                {
                    Location = new Point(10, 10),
                    Size = GetDpiAwareSize(new Size(300, 300)),
                    FirstDayOfWeek = DayOfWeek.Monday,
                    ShowWeekNumbers = true,
                    AllowMultipleSelection = false
                };

                // Configure calendar appearance and behavior
                _sfCalendar.SelectionChanged += SfCalendar_SelectionChanged;

                // Create splitter for resizing
                _splitter = new Splitter
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = EnhancedThemeService.HeaderColor
                };

                // Add calendar to panel
                _calendarPanel.Controls.Add(_sfCalendar);

                // Add to form before existing controls
                this.Controls.Add(_splitter);
                this.Controls.Add(_calendarPanel);

                // Adjust existing grid layout
                if (_dataGrid != null)
                {
                    _dataGrid.Location = new Point(GetDpiAwareX(340), _dataGrid.Location.Y);
                    _dataGrid.Width = this.ClientSize.Width - GetDpiAwareWidth(350);
                }

                Console.WriteLine("✅ ENHANCED CALENDAR: SfCalendar integration complete");
            }
            catch (Exception ex)
            {
                HandleError($"Error creating enhanced calendar layout: {ex.Message}", "$($EntityName) Error", ex);
                // Fallback to standard layout
            }
        }

        private void SfCalendar_SelectionChanged(SfCalendar sender, Syncfusion.WinForms.Input.Events.SelectionChangedEventArgs e)
        {
            try
            {
                if (e.NewValue.HasValue)
                {
                    var selectedDate = e.NewValue.Value.Date;
                    FilterEventsByDate(selectedDate);
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error handling calendar selection: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        private void FilterEventsByDate(DateTime selectedDate)
        {
            try
            {
                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<SchoolCalendar>();
                    return;
                }

                var dateString = selectedDate.ToString("yyyy-MM-dd");
                var filteredEvents = _entities.Where(e =>
                    e.Date == dateString ||
                    (e.EndDate != null &&
                     DateTime.TryParse(e.Date, out var startDate) &&
                     DateTime.TryParse(e.EndDate, out var endDate) &&
                     selectedDate >= startDate.Date && selectedDate <= endDate.Date)
                ).ToList();

                var originalEntities = _entities;
                _entities = filteredEvents;
                PopulateCalendarGrid();

                // Show message if no events found
                if (!filteredEvents.Any())
                {
                    ShowInfo($"No events scheduled for {selectedDate:MMMM dd, yyyy}");
                }

                // Restore for future operations
                _entities = originalEntities;
            }
            catch (Exception ex)
            {
                HandleError($"Error filtering events by date: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        private void UpdateCalendarSpecialDates()
        {
            try
            {
                if (_sfCalendar == null) return;

                // Clear existing special dates
                _sfCalendar.SpecialDates.Clear();

                // Add school calendar events as special dates
                foreach (var calendarEvent in _entities)
                {
                    if (DateTime.TryParse(calendarEvent.Date, out var eventDate))
                    {
                        var specialDate = new SpecialDate
                        {
                            Value = eventDate,
                            Description = calendarEvent.Description ?? calendarEvent.Category ?? "Event",
                            BackColor = GetEventColor(calendarEvent.Category),
                            ForeColor = Color.White,
                            Font = EnhancedThemeService.GetSafeFont(8, FontStyle.Bold),
                            IsDateVisible = true,
                            TextAlign = ContentAlignment.BottomCenter
                        };

                        _sfCalendar.SpecialDates.Add(specialDate);
                    }
                }

                Console.WriteLine($"✅ CALENDAR EVENTS: Added {_sfCalendar.SpecialDates.Count} special dates");
            }
            catch (Exception ex)
            {
                HandleError($"Error updating calendar special dates: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        private Color GetEventColor(string? category)
        {
            return category?.ToLower() switch
            {
                "holiday" => BusBuddyThemeManager.ThemeColors.GetErrorColor(BusBuddyThemeManager.CurrentTheme),        // Red
                "school day" => BusBuddyThemeManager.ThemeColors.GetInfoColor(BusBuddyThemeManager.CurrentTheme),    // Blue
                "thanksgiving break" => BusBuddyThemeManager.ThemeColors.GetWarningColor(BusBuddyThemeManager.CurrentTheme), // Orange
                "christmas break" => BusBuddyThemeManager.ThemeColors.GetDarkColor(BusBuddyThemeManager.CurrentTheme), // Dark Red
                "spring break" => BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme),  // Green
                "key event" => BusBuddyThemeManager.ThemeColors.GetSecondaryColor(BusBuddyThemeManager.CurrentTheme),     // Purple
                _ => BusBuddyThemeManager.ThemeColors.GetMutedColor(BusBuddyThemeManager.CurrentTheme)               // Gray
            };
        }

        protected override void AddNewEntity()
        {
            try
            {
                var calendarForm = new SchoolCalendarEditFormSyncfusion(this._serviceProvider, this._messageService);
                if (calendarForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGridAndCalendar();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error adding new calendar event: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfo("Please select a calendar event to edit.");
                return;
            }

            try
            {
                var calendarForm = new SchoolCalendarEditFormSyncfusion(this._serviceProvider, this._messageService, selectedEvent);
                if (calendarForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGridAndCalendar();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error editing calendar event: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfo("Please select a calendar event to delete.");
                return;
            }

            if (!ConfirmDelete("calendar event")) return;

            try
            {
                _schoolCalendarRepository.DeleteCalendarEvent(selectedEvent.CalendarID);
                RefreshGridAndCalendar();
                ShowInfo("Calendar event deleted successfully.");
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting calendar event: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedEvent = GetSelectedEntity();
            if (selectedEvent == null)
            {
                ShowInfo("Please select a calendar event to view details.");
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

                ShowInfo(details, "Calendar Event Details");
            }
            catch (Exception ex)
            {
                HandleError($"Error viewing calendar event details: {ex.Message}", "$($EntityName) Error", ex);
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

                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<SchoolCalendar>();
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
                HandleError($"Error searching calendar events: {ex.Message}", "$($EntityName) Error", ex);
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

        #endregion

        #endregion

        #endregion

        #region Helper Methods
        private void RefreshGridAndCalendar()
        {
            LoadData(); // This will refresh both grid and calendar
        }

        private void PopulateCalendarGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<SchoolCalendar>();
                }

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

                Console.WriteLine($"📅 CALENDAR GRID: Populated with {calendarData.Count} events");
            }
            catch (Exception ex)
            {
                HandleError($"Error populating calendar grid: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Clean up any resources being used.
        /// Ensures proper disposal of Syncfusion controls
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Dispose Syncfusion controls properly
                    _sfCalendar?.Dispose();
                    _calendarPanel?.Dispose();
                    _splitter?.Dispose();
                }
                catch (Exception ex)
                {
                    // Log disposal errors but don't throw
                    System.Diagnostics.Debug.WriteLine($"Error during SchoolCalendarManagementForm disposal: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}

