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

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// School Calendar Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing school calendar with visual calendar grid and CRUD operations
    /// </summary>
    public class SchoolCalendarManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly ISchoolCalendarRepository _calendarRepository;
        private DataGridView? _calendarGrid;
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
            InitializeComponent();
            LoadCalendarEntries();
            PopulateCalendarGrid();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "School Calendar Management"
            this.Text = "📅 School Calendar Management";
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
            // Month/year navigation
            var monthLabel = CreateLabel("Month:", 20, 25);
            _mainPanel.Controls.Add(monthLabel);

            _monthComboBox = new ComboBox
            {
                Location = new Point(GetDpiAwareX(80), GetDpiAwareY(20)),
                Size = GetDpiAwareSize(new Size(120, 30)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _monthComboBox.Items.AddRange(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToArray());
            _monthComboBox.SelectedIndex = _displayMonth - 1;
            _mainPanel.Controls.Add(_monthComboBox);

            var yearLabel = CreateLabel("Year:", 220, 25);
            _mainPanel.Controls.Add(yearLabel);

            _yearUpDown = new NumericUpDown
            {
                Location = new Point(GetDpiAwareX(260), GetDpiAwareY(20)),
                Size = GetDpiAwareSize(new Size(80, 30)),
                Minimum = 2000,
                Maximum = 2100,
                Value = _displayYear
            };
            _mainPanel.Controls.Add(_yearUpDown);

            // Legend
            _legendLabel = new Label
            {
                Location = new Point(GetDpiAwareX(360), GetDpiAwareY(20)),
                Size = GetDpiAwareSize(new Size(800, 30)),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Italic)
            };
            _legendLabel.Text = "Legend: " + string.Join("  ", _dayTypeColors.Select(kvp => $"[{kvp.Key}]"));
            _mainPanel.Controls.Add(_legendLabel);

            // Calendar grid
            _calendarGrid = new DataGridView();
            _calendarGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(60));
            _calendarGrid.Size = GetDpiAwareSize(new Size(1150, 600));
            _calendarGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _calendarGrid.RowHeadersVisible = false;
            _calendarGrid.ColumnHeadersHeight = GetDpiAwareHeight(40);
            _calendarGrid.AllowUserToAddRows = false;
            _calendarGrid.AllowUserToDeleteRows = false;
            _calendarGrid.AllowUserToResizeRows = false;
            _calendarGrid.AllowUserToResizeColumns = false;
            _calendarGrid.ReadOnly = true;
            _calendarGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _calendarGrid.MultiSelect = false;
            _calendarGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Apply Syncfusion theming to grid
            SyncfusionThemeHelper.ApplyMaterialDataGrid(_calendarGrid);

            _mainPanel.Controls.Add(_calendarGrid);

            // Edit panel
            InitializeEditPanel();
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel
            {
                Location = new Point(GetDpiAwareX(20), GetDpiAwareY(680)),
                Size = GetDpiAwareSize(new Size(1150, 100)),
                Visible = false,
                BackColor = Color.AliceBlue,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var dayTypeLabel = CreateLabel("Day Type:", 10, 15);
            _editPanel.Controls.Add(dayTypeLabel);

            _dayTypeComboBox = new ComboBox
            {
                Location = new Point(GetDpiAwareX(90), GetDpiAwareY(10)),
                Size = GetDpiAwareSize(new Size(180, 30)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _dayTypeComboBox.Items.AddRange(_dayTypes);
            _editPanel.Controls.Add(_dayTypeComboBox);

            var notesLabel = CreateLabel("Notes:", 300, 15);
            _editPanel.Controls.Add(notesLabel);

            _notesTextBox = SyncfusionThemeHelper.CreateStyledTextBox("");
            _notesTextBox.Location = new Point(GetDpiAwareX(360), GetDpiAwareY(10));
            _notesTextBox.Size = GetDpiAwareSize(new Size(400, 30));
            _editPanel.Controls.Add(_notesTextBox);

            // Add CRUD buttons
            var addButton = SyncfusionThemeHelper.CreateStyledButton("➕ Add New");
            addButton.Location = new Point(GetDpiAwareX(800), GetDpiAwareY(10));
            addButton.Size = GetDpiAwareSize(new Size(90, 35));
            addButton.Click += (s, e) => AddNewEntry();
            _editPanel.Controls.Add(addButton);

            var editButton = SyncfusionThemeHelper.CreateStyledButton("✏️ Edit");
            editButton.Location = new Point(GetDpiAwareX(900), GetDpiAwareY(10));
            editButton.Size = GetDpiAwareSize(new Size(70, 35));
            editButton.Click += (s, e) => EditEntry();
            _editPanel.Controls.Add(editButton);

            var deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            deleteButton.Location = new Point(GetDpiAwareX(980), GetDpiAwareY(10));
            deleteButton.Size = GetDpiAwareSize(new Size(80, 35));
            deleteButton.Click += (s, e) => DeleteEntry();
            _editPanel.Controls.Add(deleteButton);

            _saveButton = SyncfusionThemeHelper.CreateStyledButton("💾 Save");
            _saveButton.Location = new Point(GetDpiAwareX(1070), GetDpiAwareY(10));
            _saveButton.Size = GetDpiAwareSize(new Size(70, 35));
            _saveButton.Click += (s, e) => SaveDayType();
            _editPanel.Controls.Add(_saveButton);

            _mainPanel.Controls.Add(_editPanel);
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            if (_monthComboBox != null)
            {
                _monthComboBox.SelectedIndexChanged += (s, e) =>
                {
                    _displayMonth = _monthComboBox.SelectedIndex + 1;
                    PopulateCalendarGrid();
                };
            }

            if (_yearUpDown != null)
            {
                _yearUpDown.ValueChanged += (s, e) =>
                {
                    _displayYear = (int)_yearUpDown.Value;
                    PopulateCalendarGrid();
                };
            }

            if (_calendarGrid != null)
            {
                _calendarGrid.CellPainting += CalendarGrid_CellPainting;
                _calendarGrid.CellClick += CalendarGrid_CellClick;
            }
        }

        private void LoadCalendarEntries()
        {
            _calendarEntries = _calendarRepository.GetAllCalendarEntries();
        }

        internal void PopulateCalendarGrid()
        {
            _calendarGrid.Columns.Clear();
            _calendarGrid.Rows.Clear();

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            foreach (var day in days)
                _calendarGrid.Columns.Add(day, day);

            DateTime firstOfMonth = new DateTime(_displayYear, _displayMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(_displayYear, _displayMonth);
            int firstDayOfWeek = ((int)firstOfMonth.DayOfWeek + 6) % 7; // Monday=0
            int totalCells = daysInMonth + firstDayOfWeek;
            int rows = (int)Math.Ceiling(totalCells / 7.0);

            _calendarGrid.Rows.Add(rows);

            for (int i = 0; i < rows; i++)
                _calendarGrid.Rows[i].Height = GetDpiAwareHeight(60);

            for (int day = 1; day <= daysInMonth; day++)
            {
                int cell = day + firstDayOfWeek - 1;
                int row = cell / 7;
                int col = cell % 7;
                _calendarGrid.Rows[row].Cells[col].Value = new DateTime(_displayYear, _displayMonth, day);
            }

            _calendarGrid.ClearSelection();
        }

        private void CalendarGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var cellValue = _calendarGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (cellValue is not DateTime cellDate || e.Graphics == null || e.CellStyle?.Font == null)
            {
                if (e.Graphics != null)
                    e.Graphics.FillRectangle(new SolidBrush(_defaultColor), e.CellBounds);
                e.Handled = true;
                return;
            }

            if (_calendarEntries == null)
            {
                e.Graphics.FillRectangle(new SolidBrush(_defaultColor), e.CellBounds);
                e.Handled = true;
                return;
            }

            var entry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == cellDate.Date);
            string dayType = entry?.Category ?? "School Day";
            Color backColor = _dayTypeColors.ContainsKey(dayType) ? _dayTypeColors[dayType] : _defaultColor;

            e.Graphics.FillRectangle(new SolidBrush(backColor), e.CellBounds);

            // Draw day number
            TextRenderer.DrawText(e.Graphics, cellDate.Day.ToString(), e.CellStyle.Font, e.CellBounds, Color.Black, TextFormatFlags.Right | TextFormatFlags.Top);

            // Draw day type
            if (!string.IsNullOrEmpty(dayType))
                TextRenderer.DrawText(e.Graphics, dayType, e.CellStyle.Font, e.CellBounds, Color.DimGray, TextFormatFlags.Left | TextFormatFlags.Bottom);

            // Draw notes
            if (entry != null && !string.IsNullOrEmpty(entry.Notes))
                TextRenderer.DrawText(e.Graphics, entry.Notes, new Font(e.CellStyle.Font, FontStyle.Italic), e.CellBounds, Color.Navy, TextFormatFlags.Left | TextFormatFlags.Top);

            e.Handled = true;
        }

        private void CalendarGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var cellValue = _calendarGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (cellValue is not DateTime cellDate) return;

            _selectedDate = cellDate;

            if (_calendarEntries == null)
            {
                _dayTypeComboBox.SelectedItem = "School Day";
                if (_notesTextBox is TextBox notesTb)
                    notesTb.Text = string.Empty;
                _editPanel.Visible = true;
                return;
            }

            var entry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == cellDate.Date);
            _dayTypeComboBox.SelectedItem = entry?.Category ?? "School Day";
            if (_notesTextBox is TextBox notesTb2)
                notesTb2.Text = entry?.Notes ?? string.Empty;
            _editPanel.Visible = true;
        }

        private void AddNewEntry()
        {
            try
            {
                using var editForm = new SchoolCalendarEditFormSyncfusion();
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.SchoolCalendar != null)
                {
                    _calendarRepository.Add(editForm.SchoolCalendar);
                    LoadCalendarEntries();
                    PopulateCalendarGrid();
                    MessageBox.Show("Calendar entry added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditEntry()
        {
            try
            {
                var entry = _calendarEntries?.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);
                if (entry == null)
                {
                    MessageBox.Show("No entry found for the selected date.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var editForm = new SchoolCalendarEditFormSyncfusion(entry);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.SchoolCalendar != null)
                {
                    _calendarRepository.Update(editForm.SchoolCalendar);
                    LoadCalendarEntries();
                    PopulateCalendarGrid();
                    MessageBox.Show("Calendar entry updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteEntry()
        {
            try
            {
                var entry = _calendarEntries?.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);
                if (entry == null)
                {
                    MessageBox.Show("No entry found for the selected date.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the calendar entry for {_selectedDate:MM/dd/yyyy}?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _calendarRepository.Delete(entry.CalendarID);
                    LoadCalendarEntries();
                    PopulateCalendarGrid();
                    _editPanel.Visible = false;
                    MessageBox.Show("Calendar entry deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveDayType()
        {
            try
            {
                if (_dayTypeComboBox?.SelectedItem == null)
                {
                    MessageBox.Show("Please select a day type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dayType = _dayTypeComboBox.SelectedItem.ToString();
                var notes = (_notesTextBox as TextBox)?.Text ?? string.Empty;

                var existingEntry = _calendarEntries?.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);

                if (existingEntry != null)
                {
                    // Update existing entry
                    existingEntry.Category = dayType;
                    existingEntry.Notes = notes;
                    _calendarRepository.Update(existingEntry);
                    MessageBox.Show("Calendar entry updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Create new entry
                    var newEntry = new SchoolCalendar
                    {
                        Date = _selectedDate.ToString("yyyy-MM-dd"),
                        Category = dayType,
                        Notes = notes
                    };
                    _calendarRepository.Add(newEntry);
                    MessageBox.Show("Calendar entry added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                LoadCalendarEntries();
                PopulateCalendarGrid();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
