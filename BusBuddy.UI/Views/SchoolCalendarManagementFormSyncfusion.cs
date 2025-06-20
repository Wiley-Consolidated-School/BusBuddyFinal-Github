using System;
using System.Collections.Generic;
using System.Data;
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
    /// School Calendar Management Form - Enhanced Syncfusion Implementation
    /// Form for managing school calendar with advanced SfDataGrid features and visual calendar
    /// </summary>
    public class SchoolCalendarManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly ISchoolCalendarRepository _calendarRepository;
        private SfDataGrid? _calendarGrid;
        private ComboBox? _monthComboBox;
        private NumericUpDown _yearUpDown;
        private Panel? _editPanel;
        private ComboBox? _dayTypeComboBox;
        private Control? _notesTextBox;
        private Control? _saveButton;
        private Label? _legendLabel;
        private List<SchoolCalendar> _calendarEntries;
        private DateTime _selectedDate;
        private int _displayYear;
        private int _displayMonth;
        private readonly string[] _dayTypes = { "School Day", "Holiday", "Vacation", "Half Day", "Non-Student Day" };
        private readonly Color _defaultColor = Color.White;
        private readonly Dictionary<string, Color> _dayTypeColors = new()
        {
            { "School Day", Color.LightGreen },
            { "Holiday", Color.LightCoral },
            { "Vacation", Color.Khaki },
            { "Half Day", Color.LightYellow },
            { "Non-Student Day", Color.LightGray }
        };

        public SchoolCalendarManagementFormSyncfusion() : this(new SchoolCalendarRepository()) { }

        public SchoolCalendarManagementFormSyncfusion(ISchoolCalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository ?? throw new ArgumentNullException(nameof(calendarRepository));
            _displayYear = DateTime.Now.Year;
            _displayMonth = DateTime.Now.Month;
            _selectedDate = DateTime.Now.Date;
            _calendarEntries = new List<SchoolCalendar>();

            InitializeComponent();
            LoadCalendarEntries();
        }

        private void InitializeComponent()
        {
            // Set form size to 1400x900, title to "School Calendar Management"
            this.Text = "📅 School Calendar Management";
            this.ClientSize = GetDpiAwareSize(new Size(1400, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(1000, 700));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            Console.WriteLine($"🎨 ENHANCED SYNCFUSION FORM: {this.Text} initialized with advanced SfDataGrid features");
            Console.WriteLine($"✨ Features enabled: Calendar Grid, Filtering, Sorting, Color Coding");
        }

        private void CreateControls()
        {
            // Create navigation controls
            var navigationPanel = new Panel
            {
                Size = GetDpiAwareSize(new Size(1350, 60)),
                Location = new Point(GetDpiAwareX(20), GetDpiAwareY(20)),
                BackColor = Color.LightBlue
            };

            var monthLabel = ControlFactory.CreateLabel("📅 Month:");
            monthLabel.Location = new Point(10, 20);
            navigationPanel.Controls.Add(monthLabel);

            _monthComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = GetDpiAwareSize(new Size(120, 30)),
                Location = new Point(70, 15)
            };
            _monthComboBox.Items.AddRange(new[] {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            });
            _monthComboBox.SelectedIndex = _displayMonth - 1;
            navigationPanel.Controls.Add(_monthComboBox);

            var yearLabel = ControlFactory.CreateLabel("📅 Year:");
            yearLabel.Location = new Point(210, 20);
            navigationPanel.Controls.Add(yearLabel);

            _yearUpDown = new NumericUpDown
            {
                Minimum = 2020,
                Maximum = 2030,
                Value = _displayYear,
                Size = GetDpiAwareSize(new Size(80, 30)),
                Location = new Point(260, 15)
            };
            navigationPanel.Controls.Add(_yearUpDown);

            var refreshButton = SyncfusionThemeHelper.CreateStyledButton("🔄 Refresh");
            refreshButton.Size = GetDpiAwareSize(new Size(100, 35));
            refreshButton.Location = new Point(360, 12);
            refreshButton.Click += (s, e) => RefreshCalendar();
            navigationPanel.Controls.Add(refreshButton);

            // Create legend
            _legendLabel = ControlFactory.CreateLabel("Legend: ");
            _legendLabel.Location = new Point(480, 20);
            _legendLabel.Size = GetDpiAwareSize(new Size(800, 20));
            UpdateLegend();
            navigationPanel.Controls.Add(_legendLabel);

            _mainPanel.Controls.Add(navigationPanel);

            // Create main calendar grid with enhanced styling
            _calendarGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _calendarGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(90));
            _calendarGrid.Size = GetDpiAwareSize(new Size(900, 600));
            _calendarGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply BusBuddy standards and enhanced theming
            SyncfusionThemeHelper.SfDataGridEnhancements(_calendarGrid);

            _mainPanel.Controls.Add(_calendarGrid);

            // Create edit panel
            CreateEditPanel();

            // Configure grid columns
            SetupDataGridColumns();
        }

        private void CreateEditPanel()
        {
            _editPanel = new Panel
            {
                Size = GetDpiAwareSize(new Size(450, 600)),
                Location = new Point(GetDpiAwareX(950), GetDpiAwareY(90)),
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            var editTitle = ControlFactory.CreateLabel("📝 Edit Calendar Entry");
            editTitle.Font = new Font(editTitle.Font, FontStyle.Bold);
            editTitle.Location = new Point(10, 10);
            editTitle.Size = GetDpiAwareSize(new Size(400, 25));
            _editPanel.Controls.Add(editTitle);

            var selectedDateLabel = ControlFactory.CreateLabel($"Selected Date: {_selectedDate:MM/dd/yyyy}");
            selectedDateLabel.Location = new Point(10, 45);
            selectedDateLabel.Size = GetDpiAwareSize(new Size(400, 20));
            selectedDateLabel.Name = "SelectedDateLabel";
            _editPanel.Controls.Add(selectedDateLabel);

            var dayTypeLabel = ControlFactory.CreateLabel("Day Type:");
            dayTypeLabel.Location = new Point(10, 80);
            _editPanel.Controls.Add(dayTypeLabel);

            _dayTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = GetDpiAwareSize(new Size(400, 30)),
                Location = new Point(10, 105)
            };
            _dayTypeComboBox.Items.AddRange(_dayTypes);
            _dayTypeComboBox.SelectedIndex = 0; // Default to "School Day"
            _editPanel.Controls.Add(_dayTypeComboBox);

            var notesLabel = ControlFactory.CreateLabel("Notes:");
            notesLabel.Location = new Point(10, 150);
            _editPanel.Controls.Add(notesLabel);

            _notesTextBox = new TextBox
            {
                Multiline = true,
                Size = GetDpiAwareSize(new Size(400, 100)),
                Location = new Point(10, 175),
                ScrollBars = ScrollBars.Vertical
            };
            _editPanel.Controls.Add(_notesTextBox);

            _saveButton = SyncfusionThemeHelper.CreateStyledButton("💾 Save Entry");
            _saveButton.Size = GetDpiAwareSize(new Size(120, 35));
            _saveButton.Location = new Point(10, 290);
            _editPanel.Controls.Add(_saveButton);

            var deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            deleteButton.Size = GetDpiAwareSize(new Size(120, 35));
            deleteButton.Location = new Point(140, 290);
            deleteButton.Click += (s, e) => DeleteSelectedEntry();
            _editPanel.Controls.Add(deleteButton);

            _mainPanel.Controls.Add(_editPanel);
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            if (_monthComboBox != null)
                _monthComboBox.SelectedIndexChanged += (s, e) => RefreshCalendar();

            _yearUpDown.ValueChanged += (s, e) => RefreshCalendar();

            if (_calendarGrid != null)
            {
                _calendarGrid.SelectionChanged += CalendarGrid_SelectionChanged;
                _calendarGrid.CellDoubleClick += (s, e) => EditSelectedDate();
            }

            if (_saveButton != null)
                _saveButton.Click += (s, e) => SaveCalendarEntry();

            if (_dayTypeComboBox != null)
                _dayTypeComboBox.SelectedIndexChanged += (s, e) => UpdateDateDisplay();
        }

        private void SetupDataGridColumns()
        {
            if (_calendarGrid == null) return;

            _calendarGrid.Columns.Clear();
            _calendarGrid.AutoGenerateColumns = false;

            // Define columns for Calendar display
            var columns = new[]
            {
                new { Name = "Date", Header = "📅 Date", Width = 120, Visible = true },
                new { Name = "DayOfWeek", Header = "📆 Day", Width = 100, Visible = true },
                new { Name = "DayType", Header = "📝 Type", Width = 130, Visible = true },
                new { Name = "IsSchoolDay", Header = "🏫 School Day", Width = 100, Visible = true },
                new { Name = "Notes", Header = "📝 Notes", Width = 300, Visible = true },
                new { Name = "ColorIndicator", Header = "🎨 Color", Width = 80, Visible = true }
            };

            foreach (var col in columns)
            {
                var gridColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn();
                gridColumn.MappingName = col.Name;
                gridColumn.HeaderText = col.Header;
                gridColumn.Width = GetDpiAwareSize(new Size(col.Width, 0)).Width;
                gridColumn.Visible = col.Visible;

                _calendarGrid.Columns.Add(gridColumn);
            }

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_calendarGrid.Columns.Count} columns for {this.Text}");
        }

        private void LoadCalendarEntries()
        {
            try
            {
                _calendarEntries = _calendarRepository.GetAllCalendarEntries().ToList();
                RefreshCalendar();
                Console.WriteLine($"📊 Loaded {_calendarEntries.Count} calendar entries");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calendar entries: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshCalendar()
        {
            if (_monthComboBox == null || _calendarGrid == null) return;

            try
            {
                _displayMonth = _monthComboBox.SelectedIndex + 1;
                _displayYear = (int)_yearUpDown.Value;

                // Get all days for the selected month
                var daysInMonth = DateTime.DaysInMonth(_displayYear, _displayMonth);
                var monthlyData = new List<object>();

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var currentDate = new DateTime(_displayYear, _displayMonth, day);
                    var calendarEntry = _calendarEntries.FirstOrDefault(c => c.DateAsDateTime?.Date == currentDate.Date);

                    var dayType = calendarEntry?.DayType ?? "School Day";
                    var isSchoolDay = dayType == "School Day" || dayType == "Half Day";

                    monthlyData.Add(new
                    {
                        Date = currentDate.ToString("MM/dd/yyyy"),
                        DayOfWeek = currentDate.ToString("dddd"),
                        DayType = dayType,
                        IsSchoolDay = isSchoolDay ? "Yes" : "No",
                        Notes = calendarEntry?.Notes ?? "",
                        ColorIndicator = GetColorName(dayType)
                    });
                }

                _calendarGrid.DataSource = monthlyData;
                Console.WriteLine($"📅 Refreshed calendar for {_displayMonth}/{_displayYear} with {monthlyData.Count} days");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing calendar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetColorName(string dayType)
        {
            return dayType switch
            {
                "School Day" => "Green",
                "Holiday" => "Red",
                "Vacation" => "Yellow",
                "Half Day" => "Light Yellow",
                "Non-Student Day" => "Gray",
                _ => "White"
            };
        }

        private void UpdateLegend()
        {
            if (_legendLabel == null) return;

            var legendText = "Legend: ";
            foreach (var kvp in _dayTypeColors)
            {
                legendText += $"{kvp.Key} ({GetColorName(kvp.Key)}) | ";
            }
            _legendLabel.Text = legendText.TrimEnd(' ', '|');
        }

        private void CalendarGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateSelectedDate();
        }

        private void UpdateSelectedDate()
        {
            if (_calendarGrid?.SelectedItem != null)
            {
                var selectedItem = _calendarGrid.SelectedItem;
                var dateProperty = selectedItem.GetType().GetProperty("Date");
                if (dateProperty != null)
                {
                    var dateString = (string)dateProperty.GetValue(selectedItem);
                    if (DateTime.TryParse(dateString, out DateTime parsedDate))
                    {
                        _selectedDate = parsedDate;
                        UpdateEditPanel();
                    }
                }
            }
        }

        private void UpdateEditPanel()
        {
            if (_editPanel == null) return;

            // Update selected date label
            var dateLabel = _editPanel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "SelectedDateLabel");
            if (dateLabel != null)
            {
                dateLabel.Text = $"Selected Date: {_selectedDate:MM/dd/yyyy} ({_selectedDate:dddd})";
            }

            // Load existing calendar entry
            var existingEntry = _calendarEntries.FirstOrDefault(c => c.DateAsDateTime?.Date == _selectedDate.Date);
            if (existingEntry != null)
            {
                if (_dayTypeComboBox != null)
                {
                    var index = Array.IndexOf(_dayTypes, existingEntry.DayType);
                    _dayTypeComboBox.SelectedIndex = index >= 0 ? index : 0;
                }

                if (_notesTextBox is TextBox notesBox)
                {
                    notesBox.Text = existingEntry.Notes ?? "";
                }
            }
            else
            {
                // Default values for new entry
                if (_dayTypeComboBox != null)
                    _dayTypeComboBox.SelectedIndex = 0; // "School Day"

                if (_notesTextBox is TextBox notesBox)
                    notesBox.Text = "";
            }
        }

        private void EditSelectedDate()
        {
            UpdateSelectedDate();
            // Focus is automatically on the edit panel
        }

        private void SaveCalendarEntry()
        {
            try
            {
                if (_dayTypeComboBox == null || _notesTextBox is not TextBox notesBox) return;

                var dayType = _dayTypeComboBox.SelectedItem?.ToString() ?? "School Day";
                var notes = notesBox.Text.Trim();

                // Check if entry already exists
                var existingEntry = _calendarEntries.FirstOrDefault(c => c.DateAsDateTime?.Date == _selectedDate.Date);

                if (existingEntry != null)
                {
                    // Update existing entry
                    existingEntry.Category = dayType; // Use Category instead of DayType
                    existingEntry.Notes = notes;
                    _calendarRepository.Update(existingEntry);
                }
                else
                {
                    // Create new entry
                    var newEntry = new SchoolCalendar
                    {
                        DateAsDateTime = _selectedDate,
                        Category = dayType, // Use Category instead of DayType
                        Notes = notes
                    };
                    _calendarRepository.Add(newEntry);
                    _calendarEntries.Add(newEntry);
                }

                RefreshCalendar();
                MessageBox.Show("Calendar entry saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedEntry()
        {
            try
            {
                var existingEntry = _calendarEntries.FirstOrDefault(c => c.DateAsDateTime?.Date == _selectedDate.Date);
                if (existingEntry == null)
                {
                    MessageBox.Show("No calendar entry exists for the selected date.", "No Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete the calendar entry for {_selectedDate:MM/dd/yyyy}?",
                                   "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _calendarRepository.Delete(existingEntry.CalendarID);
                    _calendarEntries.Remove(existingEntry);
                    RefreshCalendar();
                    UpdateEditPanel();
                    MessageBox.Show("Calendar entry deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDateDisplay()
        {
            // This method can be used to update UI based on day type selection
            UpdateLegend();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources if needed
            base.OnFormClosing(e);
        }
    }
}
