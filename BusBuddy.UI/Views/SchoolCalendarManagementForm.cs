using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class SchoolCalendarManagementForm : BaseDataForm
    {
        private readonly ISchoolCalendarRepository _calendarRepository;
        private DataGridView _calendarGrid;
        private ComboBox _monthComboBox;
        private NumericUpDown _yearUpDown;
        private Panel _editPanel;
        private ComboBox _dayTypeComboBox;
        private TextBox _notesTextBox;
        private Button _saveButton;
        private Label _legendLabel;
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
        };        public SchoolCalendarManagementForm() : this(new SchoolCalendarRepository()) { }

        public SchoolCalendarManagementForm(ISchoolCalendarRepository calendarRepository)
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
            this.Text = "School Calendar Management";
            this.Size = new Size(1200, 900);
            // Month/year navigation
            _monthComboBox = new ComboBox { Location = new Point(20, 20), Size = new Size(120, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            _monthComboBox.Items.AddRange(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToArray());
            _monthComboBox.SelectedIndex = _displayMonth - 1;
            _monthComboBox.SelectedIndexChanged += (s, e) => { _displayMonth = _monthComboBox.SelectedIndex + 1; PopulateCalendarGrid(); };
            this.Controls.Add(_monthComboBox);
            _yearUpDown = new NumericUpDown { Location = new Point(150, 20), Size = new Size(80, 30), Minimum = 2000, Maximum = 2100, Value = _displayYear };
            _yearUpDown.ValueChanged += (s, e) => { _displayYear = (int)_yearUpDown.Value; PopulateCalendarGrid(); };
            this.Controls.Add(_yearUpDown);
            // Legend
            _legendLabel = new Label { Location = new Point(250, 20), Size = new Size(800, 30), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Italic) };
            _legendLabel.Text = "Legend: " + string.Join("  ", _dayTypeColors.Select(kvp => $"{kvp.Key} = ").ToArray());
            foreach (var kvp in _dayTypeColors)
                _legendLabel.Text += $"   [{kvp.Key}: {kvp.Value.Name}]";
            this.Controls.Add(_legendLabel);
            // Calendar grid
            _calendarGrid = new DataGridView();
            _calendarGrid.Location = new Point(20, 60);
            _calendarGrid.Size = new Size(1150, 600);
            _calendarGrid.RowHeadersVisible = false;
            _calendarGrid.ColumnHeadersHeight = 40;
            _calendarGrid.AllowUserToAddRows = false;
            _calendarGrid.AllowUserToDeleteRows = false;
            _calendarGrid.AllowUserToResizeRows = false;
            _calendarGrid.AllowUserToResizeColumns = false;
            _calendarGrid.ReadOnly = true;
            _calendarGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            _calendarGrid.MultiSelect = false;
            _calendarGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _calendarGrid.CellPainting += CalendarGrid_CellPainting;
            _calendarGrid.CellClick += CalendarGrid_CellClick;
            this.Controls.Add(_calendarGrid);
            // Edit panel
            _editPanel = new Panel { Location = new Point(20, 680), Size = new Size(1150, 100), Visible = false, BackColor = Color.AliceBlue };
            var dayTypeLabel = CreateLabel("Day Type:", 10, 15);
            _editPanel.Controls.Add(dayTypeLabel);
            _dayTypeComboBox = new ComboBox { Location = new Point(90, 10), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            _dayTypeComboBox.Items.AddRange(_dayTypes);
            _editPanel.Controls.Add(_dayTypeComboBox);
            var notesLabel = CreateLabel("Notes:", 300, 15);
            _editPanel.Controls.Add(notesLabel);
            _notesTextBox = new TextBox { Location = new Point(360, 10), Size = new Size(400, 30) };            _editPanel.Controls.Add(_notesTextBox);

            // Add CRUD buttons
            var addButton = CreateButton("Add New", 800, 10, (s, e) => AddNewEntry());
            _editPanel.Controls.Add(addButton);

            var editButton = CreateButton("Edit", 900, 10, (s, e) => EditEntry());
            _editPanel.Controls.Add(editButton);

            var deleteButton = CreateButton("Delete", 970, 10, (s, e) => DeleteEntry());
            _editPanel.Controls.Add(deleteButton);

            _saveButton = CreateButton("Save", 1040, 10, (s, e) => SaveDayType());
            _editPanel.Controls.Add(_saveButton);
            this.Controls.Add(_editPanel);
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
                _calendarGrid.Rows[i].Height = 60;
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
            TextRenderer.DrawText(e.Graphics, cellDate.Day.ToString(), e.CellStyle.Font, e.CellBounds, Color.Black, TextFormatFlags.Right | TextFormatFlags.Top);
            if (!string.IsNullOrEmpty(dayType))
                TextRenderer.DrawText(e.Graphics, dayType, e.CellStyle.Font, e.CellBounds, Color.DimGray, TextFormatFlags.Left | TextFormatFlags.Bottom);
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
                _notesTextBox.Text = string.Empty;
                _editPanel.Visible = true;
                return;
            }
            var entry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == cellDate.Date);
            _dayTypeComboBox.SelectedItem = entry?.Category ?? "School Day";
            _notesTextBox.Text = entry?.Notes ?? string.Empty;
            _editPanel.Visible = true;
        }        private void AddNewEntry()
        {
            try
            {
                using var editForm = new SchoolCalendarEditForm();
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
        }        private void EditEntry()
        {
            try
            {
                if (_selectedDate == default)
                {
                    MessageBox.Show("Please select a date to edit.", "No Date Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var existingEntry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);

                using var editForm = new SchoolCalendarEditForm(existingEntry);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.SchoolCalendar != null)
                {
                    if (existingEntry != null)
                    {
                        _calendarRepository.Update(editForm.SchoolCalendar);
                    }
                    else
                    {
                        _calendarRepository.Add(editForm.SchoolCalendar);
                    }

                    LoadCalendarEntries();
                    PopulateCalendarGrid();
                    MessageBox.Show("Calendar entry updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void DeleteEntry()
        {
            try
            {
                if (_selectedDate == default)
                {
                    MessageBox.Show("Please select a date to delete.", "No Date Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var existingEntry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);

                if (existingEntry == null)
                {
                    MessageBox.Show("No calendar entry found for the selected date.", "Entry Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the calendar entry for {_selectedDate.ToShortDateString()}?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _calendarRepository.Delete(existingEntry.Id);
                    LoadCalendarEntries();
                    PopulateCalendarGrid();
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
            if (_selectedDate == default) return;
            var entry = _calendarEntries.FirstOrDefault(x => x.DateAsDateTime.HasValue && x.DateAsDateTime.Value.Date == _selectedDate.Date);
            var selectedDayType = _dayTypeComboBox.SelectedItem?.ToString() ?? "School Day";
            var notes = _notesTextBox.Text;
            try
            {
                if (entry != null)
                {
                    entry.Category = selectedDayType;
                    entry.Notes = notes;
                    _calendarRepository.Update(entry);
                }
                else
                {
                    var newEntry = new SchoolCalendar
                    {
                        DateAsDateTime = _selectedDate,
                        Category = selectedDayType,
                        Notes = notes
                    };
                    _calendarRepository.Add(newEntry);
                }
                LoadCalendarEntries();
                PopulateCalendarGrid();
                MessageBox.Show("Day type saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving day type: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
