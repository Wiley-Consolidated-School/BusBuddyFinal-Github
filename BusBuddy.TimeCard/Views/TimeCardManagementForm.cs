using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.TimeCard.Services;
using BusBuddy.TimeCard.Utilities;
using MaterialSkin.Controls;

namespace BusBuddy.TimeCard.Views
{
    /// <summary>
    /// TimeCard Management Form providing comprehensive time tracking with complex clock scheme
    /// Supports normal work hours, route-specific hours, and weekly/monthly summaries
    /// </summary>
    public class TimeCardManagementForm : Form
    {
        private readonly ITimeCardRepository _timeCardRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly TimeEntryValidationService _validationService;

        // Data grid and controls
        private DataGridView _timeCardGrid = null!;
        private Button _addButton = null!;
        private Button _editButton = null!;
        private Button _deleteButton = null!;
        private Button _detailsButton = null!;
        private TextBox _searchBox = null!;
        private Button _searchButton = null!;
        private ComboBox _driverFilter = null!;
        private DateTimePicker _dateFilter = null!;
        private Button _weeklyReportButton = null!;
        private Button _monthlyReportButton = null!;

        // Edit panel controls
        private Panel _editPanel = null!;
        private ComboBox _driverCombo = null!;
        private DateTimePicker _datePicker = null!;
        private CheckBox _routeDayCheckBox = null!;
        private CheckBox _ptoCheckBox = null!;

        // Normal work hours
        private DateTimePicker _clockInPicker = null!;
        private DateTimePicker _lunchOutPicker = null!;
        private DateTimePicker _lunchInPicker = null!;
        private DateTimePicker _clockOutPicker = null!;

        // Route hours
        private DateTimePicker _routeAMOutPicker = null!;
        private DateTimePicker _routeAMInPicker = null!;
        private DateTimePicker _routePMOutPicker = null!;
        private DateTimePicker _routePMInPicker = null!;

        // PTO and summary
        private NumericUpDown _ptoHoursUpDown = null!;
        private TextBox _notesTextBox = null!;
        private Label _totalHoursLabel = null!;
        private Label _overtimeLabel = null!;
        private Label _weeklyTotalLabel = null!;
        private Label _monthlyTotalLabel = null!;

        // Action buttons
        private Button _saveButton = null!;
        private Button _cancelButton = null!;

        private List<Models.TimeCard> _timeCards = null!;
        private List<Driver> _drivers = null!;
        private Models.TimeCard _currentTimeCard = null!;
        private bool _isEditMode;

        public TimeCardManagementForm() : this(
            new TimeCardRepository(new BusBuddyContext()),
            new DriverRepository())
        {
        }

        public TimeCardManagementForm(ITimeCardRepository timeCardRepository, IDriverRepository driverRepository)
        {
            _timeCardRepository = timeCardRepository ?? throw new ArgumentNullException(nameof(timeCardRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _validationService = new TimeEntryValidationService(_timeCardRepository, _driverRepository);

            InitializeComponent();
            LoadInitialData();
        }

        private void InitializeComponent()
        {
            // Set form properties
            this.Text = "Time Card Management";
            this.ClientSize = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);

            // Enable high DPI support
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Font = new Font("Segoe UI", 9F);

            CreateMainLayout();
        }

        private void CreateMainLayout()
        {
            // Create main splitter
            var mainSplitter = new SplitContainer();
            mainSplitter.Dock = DockStyle.Fill;
            mainSplitter.Orientation = Orientation.Horizontal;
            mainSplitter.SplitterDistance = 600;
            mainSplitter.IsSplitterFixed = false;
            this.Controls.Add(mainSplitter);

            // Top panel: Data grid with toolbar
            CreateDataGridSection(mainSplitter.Panel1);

            // Bottom panel: Edit form
            CreateEditSection(mainSplitter.Panel2);
        }

        private void CreateDataGridSection(Control parent)
        {
            // Create toolbar
            var toolbar = new Panel();
            toolbar.Height = 60;
            toolbar.Dock = DockStyle.Top;
            toolbar.BackColor = SystemColors.Control;
            parent.Controls.Add(toolbar);

            // Row 1: Action buttons
            int buttonY = 10;
            _addButton = CreateToolbarButton("Add New", 20, buttonY, (s, e) => AddNewTimeCard());
            _editButton = CreateToolbarButton("Edit", 130, buttonY, (s, e) => EditSelectedTimeCard());
            _deleteButton = CreateToolbarButton("Delete", 240, buttonY, (s, e) => DeleteSelectedTimeCard());
            _detailsButton = CreateToolbarButton("Details", 350, buttonY, (s, e) => ViewTimeCardDetails());

            // Reports
            _weeklyReportButton = CreateToolbarButton("Weekly Report", 500, buttonY, (s, e) => ShowWeeklyReport());
            _monthlyReportButton = CreateToolbarButton("Monthly Report", 640, buttonY, (s, e) => ShowMonthlyReport());

            toolbar.Controls.AddRange(new Control[] {
                _addButton, _editButton, _deleteButton, _detailsButton,
                _weeklyReportButton, _monthlyReportButton
            });

            // Row 2: Filters
            buttonY = 35;
            CreateLabel("Driver:", 20, buttonY + 3, toolbar);
            _driverFilter = CreateComboBox(70, buttonY, 150, toolbar);
            _driverFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _driverFilter.SelectedIndexChanged += (s, e) => FilterTimeCards();

            CreateLabel("Date:", 240, buttonY + 3, toolbar);
            _dateFilter = CreateDatePicker(280, buttonY, 120, toolbar);
            _dateFilter.ValueChanged += (s, e) => FilterTimeCards();

            CreateLabel("Search:", 420, buttonY + 3, toolbar);
            _searchBox = CreateTextBox(470, buttonY, 150, toolbar);
            _searchButton = CreateToolbarButton("Go", 630, buttonY, (s, e) => FilterTimeCards());
            _searchButton.Size = new Size(50, 23);

            toolbar.Controls.AddRange(new Control[] {
                _driverFilter, _dateFilter, _searchBox, _searchButton
            });

            // Create data grid
            _timeCardGrid = new DataGridView();
            _timeCardGrid.Dock = DockStyle.Fill;
            _timeCardGrid.AutoGenerateColumns = false;
            _timeCardGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _timeCardGrid.MultiSelect = false;
            _timeCardGrid.ReadOnly = true;
            _timeCardGrid.AllowUserToAddRows = false;
            _timeCardGrid.AllowUserToDeleteRows = false;
            _timeCardGrid.RowHeadersVisible = false;

            // Add columns
            SetupDataGridColumns();

            _timeCardGrid.SelectionChanged += TimeCardGrid_SelectionChanged;
            _timeCardGrid.CellDoubleClick += (s, e) => EditSelectedTimeCard();

            parent.Controls.Add(_timeCardGrid);
        }

        private void SetupDataGridColumns()
        {
            _timeCardGrid.Columns.Clear();

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                HeaderText = "Date",
                DataPropertyName = "Date",
                Width = 100,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "yyyy-MM-dd" }
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Driver",
                HeaderText = "Driver",
                Width = 150
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ClockIn",
                HeaderText = "Clock In",
                DataPropertyName = "ClockIn",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm" }
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ClockOut",
                HeaderText = "Clock Out",
                DataPropertyName = "ClockOut",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm" }
            });

            _timeCardGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsRouteDay",
                HeaderText = "Route Day",
                DataPropertyName = "IsRouteDay",
                Width = 80
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalTime",
                HeaderText = "Total Hours",
                DataPropertyName = "TotalTime",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" }
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Overtime",
                HeaderText = "Overtime",
                DataPropertyName = "Overtime",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "F2" }
            });

            _timeCardGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "IsPaidTimeOff",
                HeaderText = "PTO",
                DataPropertyName = "IsPaidTimeOff",
                Width = 50
            });

            _timeCardGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Notes",
                HeaderText = "Notes",
                DataPropertyName = "Notes",
                Width = 200
            });
        }

        private void CreateEditSection(Control parent)
        {
            _editPanel = new Panel();
            _editPanel.Dock = DockStyle.Fill;
            _editPanel.BackColor = SystemColors.Control;
            _editPanel.Padding = new Padding(10);
            parent.Controls.Add(_editPanel);

            CreateEditControls();
        }

        private void CreateEditControls()
        {
            // Create a scroll panel for the edit form
            var scrollPanel = new Panel();
            scrollPanel.Dock = DockStyle.Fill;
            scrollPanel.AutoScroll = true;
            _editPanel.Controls.Add(scrollPanel);

            int col1X = 20, col2X = 300, col3X = 580, col4X = 860;
            int currentY = 20;
            int rowHeight = 35;

            // Header
            var headerLabel = new Label();
            headerLabel.Text = "Time Card Entry";
            headerLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            headerLabel.Location = new Point(col1X, currentY);
            headerLabel.AutoSize = true;
            scrollPanel.Controls.Add(headerLabel);
            currentY += 40;

            // Basic info row
            CreateLabel("Driver:", col1X, currentY, scrollPanel);
            _driverCombo = CreateComboBox(col1X, currentY + 18, 200, scrollPanel);
            _driverCombo.DropDownStyle = ComboBoxStyle.DropDownList;

            CreateLabel("Date:", col2X, currentY, scrollPanel);
            _datePicker = CreateDatePicker(col2X, currentY + 18, 150, scrollPanel);
            _datePicker.ValueChanged += (s, e) => UpdateSummaryLabels();

            _routeDayCheckBox = new CheckBox();
            _routeDayCheckBox.Text = "Route Day";
            _routeDayCheckBox.Location = new Point(col3X, currentY + 20);
            _routeDayCheckBox.AutoSize = true;
            scrollPanel.Controls.Add(_routeDayCheckBox);

            _ptoCheckBox = new CheckBox();
            _ptoCheckBox.Text = "Paid Time Off";
            _ptoCheckBox.Location = new Point(col4X, currentY + 20);
            _ptoCheckBox.AutoSize = true;
            _ptoCheckBox.CheckedChanged += (s, e) => TogglePTOMode();
            scrollPanel.Controls.Add(_ptoCheckBox);

            currentY += rowHeight + 10;

            // Normal work hours section
            var workHoursLabel = new Label();
            workHoursLabel.Text = "Normal Work Hours";
            workHoursLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            workHoursLabel.Location = new Point(col1X, currentY);
            workHoursLabel.AutoSize = true;
            scrollPanel.Controls.Add(workHoursLabel);
            currentY += 25;

            CreateLabel("Clock In:", col1X, currentY, scrollPanel);
            _clockInPicker = CreateTimePicker(col1X, currentY + 18, scrollPanel);

            CreateLabel("Lunch Out:", col2X, currentY, scrollPanel);
            _lunchOutPicker = CreateTimePicker(col2X, currentY + 18, scrollPanel);

            CreateLabel("Lunch In:", col3X, currentY, scrollPanel);
            _lunchInPicker = CreateTimePicker(col3X, currentY + 18, scrollPanel);

            CreateLabel("Clock Out:", col4X, currentY, scrollPanel);
            _clockOutPicker = CreateTimePicker(col4X, currentY + 18, scrollPanel);

            currentY += rowHeight + 10;

            // Route hours section
            var routeHoursLabel = new Label();
            routeHoursLabel.Text = "Route Hours (Truck Plaza Route)";
            routeHoursLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            routeHoursLabel.Location = new Point(col1X, currentY);
            routeHoursLabel.AutoSize = true;
            scrollPanel.Controls.Add(routeHoursLabel);
            currentY += 25;

            CreateLabel("AM Route Out:", col1X, currentY, scrollPanel);
            _routeAMOutPicker = CreateTimePicker(col1X, currentY + 18, scrollPanel);

            CreateLabel("AM Route In:", col2X, currentY, scrollPanel);
            _routeAMInPicker = CreateTimePicker(col2X, currentY + 18, scrollPanel);

            CreateLabel("PM Route Out:", col3X, currentY, scrollPanel);
            _routePMOutPicker = CreateTimePicker(col3X, currentY + 18, scrollPanel);

            CreateLabel("PM Route In:", col4X, currentY, scrollPanel);
            _routePMInPicker = CreateTimePicker(col4X, currentY + 18, scrollPanel);

            currentY += rowHeight + 10;

            // PTO and Notes section
            CreateLabel("PTO Hours:", col1X, currentY, scrollPanel);
            _ptoHoursUpDown = new NumericUpDown();
            _ptoHoursUpDown.Location = new Point(col1X, currentY + 18);
            _ptoHoursUpDown.Size = new Size(100, 23);
            _ptoHoursUpDown.DecimalPlaces = 2;
            _ptoHoursUpDown.Minimum = 0;
            _ptoHoursUpDown.Maximum = 24;
            _ptoHoursUpDown.ValueChanged += (s, e) => CalculateAndDisplayTotals();
            scrollPanel.Controls.Add(_ptoHoursUpDown);

            CreateLabel("Notes:", col2X, currentY, scrollPanel);
            _notesTextBox = new TextBox();
            _notesTextBox.Location = new Point(col2X, currentY + 18);
            _notesTextBox.Size = new Size(400, 23);
            _notesTextBox.Multiline = false;
            scrollPanel.Controls.Add(_notesTextBox);

            currentY += rowHeight + 10;

            // Summary section
            var summaryLabel = new Label();
            summaryLabel.Text = "Summary";
            summaryLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            summaryLabel.Location = new Point(col1X, currentY);
            summaryLabel.AutoSize = true;
            scrollPanel.Controls.Add(summaryLabel);
            currentY += 25;

            CreateLabel("Total Hours:", col1X, currentY, scrollPanel);
            _totalHoursLabel = CreateSummaryLabel(col1X + 80, currentY, scrollPanel);

            CreateLabel("Overtime:", col2X, currentY, scrollPanel);
            _overtimeLabel = CreateSummaryLabel(col2X + 70, currentY, scrollPanel);

            CreateLabel("Weekly Total:", col3X, currentY, scrollPanel);
            _weeklyTotalLabel = CreateSummaryLabel(col3X + 90, currentY, scrollPanel);

            CreateLabel("Monthly Total:", col4X, currentY, scrollPanel);
            _monthlyTotalLabel = CreateSummaryLabel(col4X + 100, currentY, scrollPanel);

            currentY += 40;

            // Action buttons
            _saveButton = new Button();
            _saveButton.Text = "Save";
            _saveButton.Size = new Size(100, 35);
            _saveButton.Location = new Point(col1X, currentY);
            _saveButton.BackColor = Color.Green;
            _saveButton.ForeColor = Color.White;
            _saveButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _saveButton.Click += SaveTimeCard_Click;
            scrollPanel.Controls.Add(_saveButton);

            _cancelButton = new Button();
            _cancelButton.Text = "Cancel";
            _cancelButton.Size = new Size(100, 35);
            _cancelButton.Location = new Point(col1X + 110, currentY);
            _cancelButton.BackColor = Color.Gray;
            _cancelButton.ForeColor = Color.White;
            _cancelButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _cancelButton.Click += (s, e) => CancelEdit();
            scrollPanel.Controls.Add(_cancelButton);

            // Wire up calculation events
            foreach (var picker in new[] { _clockInPicker, _lunchOutPicker, _lunchInPicker,
                _clockOutPicker, _routeAMOutPicker, _routeAMInPicker, _routePMOutPicker, _routePMInPicker })
            {
                picker.ValueChanged += (s, e) => CalculateAndDisplayTotals();
            }

            // Initially hide edit panel
            _editPanel.Visible = false;
        }

        private DateTimePicker CreateTimePicker(int x, int y, Control parent)
        {
            var picker = new DateTimePicker();
            picker.Location = new Point(x, y);
            picker.Size = new Size(120, 23);
            picker.Format = DateTimePickerFormat.Time;
            picker.ShowUpDown = true;
            parent.Controls.Add(picker);
            return picker;
        }

        private Label CreateSummaryLabel(int x, int y, Control parent)
        {
            var label = new Label();
            label.Location = new Point(x, y);
            label.Size = new Size(80, 20);
            label.BorderStyle = BorderStyle.Fixed3D;
            label.BackColor = Color.White;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Text = "0.00";
            parent.Controls.Add(label);
            return label;
        }

        // Continue with the rest of the methods...
        private async void LoadInitialData()
        {
            try
            {
                // Load drivers for combo boxes
                _drivers = _driverRepository.GetAllDrivers();

                var driverItems = new List<object>();
                driverItems.Add(new { Text = "All Drivers", Value = (Driver?)null });
                driverItems.AddRange(_drivers.Select(d => new {
                    Text = $"{d.FirstName} {d.LastName}",
                    Value = (Driver?)d
                }));

                _driverFilter.DataSource = driverItems;
                _driverFilter.DisplayMember = "Text";
                _driverFilter.ValueMember = "Value";

                _driverCombo.DataSource = _drivers.ToList();
                _driverCombo.DisplayMember = "LastName";
                _driverCombo.ValueMember = "DriverID";

                // Load time cards
                await LoadTimeCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadTimeCards()
        {
            try
            {
                _timeCards = (await _timeCardRepository.GetAllAsync()).ToList();
                FilterTimeCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading time cards: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterTimeCards()
        {
            if (_timeCards == null) return;

            var filtered = _timeCards.AsEnumerable();

            // Filter by driver
            if (_driverFilter.SelectedValue is Driver selectedDriver)
            {
                filtered = filtered.Where(tc => tc.DriverId == selectedDriver.DriverID);
            }

            // Filter by date (show week containing selected date)
            var selectedDate = _dateFilter.Value.Date;
            var weekStart = selectedDate.AddDays(-(int)selectedDate.DayOfWeek + 1); // Monday
            var weekEnd = weekStart.AddDays(6); // Sunday
            filtered = filtered.Where(tc => tc.Date >= weekStart && tc.Date <= weekEnd);

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(_searchBox.Text))
            {
                var searchText = _searchBox.Text.ToLower();
                filtered = filtered.Where(tc =>
                    tc.Notes?.ToLower().Contains(searchText) == true ||
                    tc.Driver?.LastName?.ToLower().Contains(searchText) == true);
            }

            var displayData = filtered.Select(tc => new
            {
                TimeCardId = tc.TimeCardId,
                Date = tc.Date,
                Driver = tc.Driver != null ? $"{tc.Driver.FirstName} {tc.Driver.LastName}" : "Unknown",
                ClockIn = tc.ClockIn,
                ClockOut = tc.ClockOut,
                IsRouteDay = tc.IsRouteDay,
                TotalTime = tc.TotalTime,
                Overtime = tc.Overtime,
                IsPaidTimeOff = tc.IsPaidTimeOff,
                Notes = tc.Notes
            }).ToList();

            _timeCardGrid.DataSource = displayData;
        }

        // Event handlers and remaining methods will continue in the next part...

        private void TimeCardGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _timeCardGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void AddNewTimeCard()
        {
            _currentTimeCard = new Models.TimeCard
            {
                Date = DateTime.Today,
                CreatedBy = Environment.UserName
            };
            _isEditMode = false;
            PopulateEditPanel();
            ShowEditPanel();
        }        private void EditSelectedTimeCard()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            var selectedData = _timeCardGrid.SelectedRows[0].DataBoundItem;
            var timeCardIdProperty = selectedData?.GetType().GetProperty("TimeCardId");
            if (timeCardIdProperty?.GetValue(selectedData) is not int timeCardId) return;

            _currentTimeCard = _timeCards.FirstOrDefault(tc => tc.TimeCardId == timeCardId);
            if (_currentTimeCard == null)
            {
                MessageBox.Show("Time card not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _isEditMode = true;
            PopulateEditPanel();
            ShowEditPanel();
        }        private async void DeleteSelectedTimeCard()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            var selectedData = _timeCardGrid.SelectedRows[0].DataBoundItem;
            var timeCardIdProperty = selectedData?.GetType().GetProperty("TimeCardId");
            if (timeCardIdProperty?.GetValue(selectedData) is not int timeCardId) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this time card entry?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await _timeCardRepository.DeleteAsync(timeCardId);
                    await LoadTimeCards();
                    MessageBox.Show("Time card deleted successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting time card: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }        private void ViewTimeCardDetails()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            var selectedData = _timeCardGrid.SelectedRows[0].DataBoundItem;
            var timeCardIdProperty = selectedData?.GetType().GetProperty("TimeCardId");
            if (timeCardIdProperty?.GetValue(selectedData) is not int timeCardId) return;

            var timeCard = _timeCards.FirstOrDefault(tc => tc.TimeCardId == timeCardId);
            if (timeCard == null) return;

            ShowTimeCardDetails(timeCard);
        }

        private void ShowTimeCardDetails(Models.TimeCard timeCard)
        {
            var details = $@"Time Card Details

Date: {timeCard.Date:yyyy-MM-dd}
Driver: {timeCard.Driver?.FirstName} {timeCard.Driver?.LastName}

Normal Work Hours:
  Clock In: {timeCard.ClockIn?.ToString(@"hh\:mm") ?? "Not recorded"}
  Lunch Out: {timeCard.LunchOut?.ToString(@"hh\:mm") ?? "Not recorded"}
  Lunch In: {timeCard.LunchIn?.ToString(@"hh\:mm") ?? "Not recorded"}
  Clock Out: {timeCard.ClockOut?.ToString(@"hh\:mm") ?? "Not recorded"}

Route Hours:
  AM Route Out: {timeCard.RouteAMOut?.ToString(@"hh\:mm") ?? "Not recorded"}
  AM Route In: {timeCard.RouteAMIn?.ToString(@"hh\:mm") ?? "Not recorded"}
  PM Route Out: {timeCard.RoutePMOut?.ToString(@"hh\:mm") ?? "Not recorded"}
  PM Route In: {timeCard.RoutePMIn?.ToString(@"hh\:mm") ?? "Not recorded"}

Summary:
  Total Hours: {timeCard.TotalTime:F2}
  Overtime: {timeCard.Overtime:F2}
  PTO Hours: {timeCard.PTOHours:F2}
  Route Day: {(timeCard.IsRouteDay ? "Yes" : "No")}
  Paid Time Off: {(timeCard.IsPaidTimeOff ? "Yes" : "No")}

Notes: {timeCard.Notes ?? "None"}

Created: {timeCard.CreatedDate:yyyy-MM-dd HH:mm} by {timeCard.CreatedBy}
{(timeCard.ModifiedDate.HasValue ? $"Modified: {timeCard.ModifiedDate:yyyy-MM-dd HH:mm} by {timeCard.ModifiedBy}" : "")}";

            MessageBox.Show(details, "Time Card Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void ShowWeeklyReport()
        {
            var driver = _driverFilter.SelectedValue as Driver;
            var date = _dateFilter.Value;

            if (driver == null)
            {
                MessageBox.Show("Please select a driver to generate weekly report.", "Driver Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var monday = date.AddDays(-(int)date.DayOfWeek + 1);
                var weeklyTimeCards = await _timeCardRepository.GetWeeklyTimeCardsAsync(monday);
                var driverTimeCards = weeklyTimeCards.Where(tc => tc.DriverId == driver.DriverID).ToList();

                ShowWeeklyReportDialog(driver, monday, driverTimeCards);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating weekly report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ShowMonthlyReport()
        {
            var driver = _driverFilter.SelectedValue as Driver;
            var date = _dateFilter.Value;

            if (driver == null)
            {
                MessageBox.Show("Please select a driver to generate monthly report.", "Driver Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var monthlyTimeCards = await _timeCardRepository.GetMonthlyTimeCardsAsync(date.Year, date.Month);
                var driverTimeCards = monthlyTimeCards.Where(tc => tc.DriverId == driver.DriverID).ToList();

                ShowMonthlyReportDialog(driver, date, driverTimeCards);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating monthly report: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowWeeklyReportDialog(Driver driver, DateTime weekStart, List<Models.TimeCard> timeCards)
        {
            var weekEnd = weekStart.AddDays(6);
            var totalHours = timeCards.Sum(tc => tc.TotalTime);
            var overtimeHours = timeCards.Sum(tc => tc.Overtime);

            var report = $@"Weekly Time Card Report

Driver: {driver.FirstName} {driver.LastName}
Week: {weekStart:yyyy-MM-dd} to {weekEnd:yyyy-MM-dd}

Daily Breakdown:";

            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var dayCard = timeCards.FirstOrDefault(tc => tc.Date?.Date == day.Date);

                if (dayCard != null)
                {
                    report += $@"
  {day:ddd MM/dd}: {dayCard.TotalTime:F2} hours {(dayCard.Overtime > 0 ? $"(OT: {dayCard.Overtime:F2})" : "")}";
                }
                else
                {
                    report += $@"
  {day:ddd MM/dd}: No entry";
                }
            }

            report += $@"

Weekly Totals:
  Regular Hours: {Math.Max(0, totalHours - overtimeHours):F2}
  Overtime Hours: {overtimeHours:F2}
  Total Hours: {totalHours:F2}

Export to Excel coming soon...";

            MessageBox.Show(report, "Weekly Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowMonthlyReportDialog(Driver driver, DateTime month, List<Models.TimeCard> timeCards)
        {
            var totalHours = timeCards.Sum(tc => tc.TotalTime);
            var overtimeHours = timeCards.Sum(tc => tc.Overtime);
            var workDays = timeCards.Count;

            var report = $@"Monthly Time Card Report

Driver: {driver.FirstName} {driver.LastName}
Month: {month:MMMM yyyy}

Summary:
  Work Days: {workDays}
  Regular Hours: {Math.Max(0, totalHours - overtimeHours):F2}
  Overtime Hours: {overtimeHours:F2}
  Total Hours: {totalHours:F2}
  Average Hours/Day: {(workDays > 0 ? totalHours / workDays : 0):F2}

Note: Month runs from 1st to last day, paid on 15th.
Export to Excel coming soon...";

            MessageBox.Show(report, "Monthly Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PopulateEditPanel()
        {
            if (_currentTimeCard == null) return;

            // Basic info
            if (_currentTimeCard.DriverId > 0)
                _driverCombo.SelectedValue = _currentTimeCard.DriverId;

            _datePicker.Value = _currentTimeCard.Date ?? DateTime.Today;
            _routeDayCheckBox.Checked = _currentTimeCard.IsRouteDay;
            _ptoCheckBox.Checked = _currentTimeCard.IsPaidTimeOff;

            // Normal work hours
            SetTimePickerValue(_clockInPicker, _currentTimeCard.ClockIn);
            SetTimePickerValue(_lunchOutPicker, _currentTimeCard.LunchOut);
            SetTimePickerValue(_lunchInPicker, _currentTimeCard.LunchIn);
            SetTimePickerValue(_clockOutPicker, _currentTimeCard.ClockOut);

            // Route hours
            SetTimePickerValue(_routeAMOutPicker, _currentTimeCard.RouteAMOut);
            SetTimePickerValue(_routeAMInPicker, _currentTimeCard.RouteAMIn);
            SetTimePickerValue(_routePMOutPicker, _currentTimeCard.RoutePMOut);
            SetTimePickerValue(_routePMInPicker, _currentTimeCard.RoutePMIn);

            // PTO and notes
            _ptoHoursUpDown.Value = (decimal)_currentTimeCard.PTOHours;
            _notesTextBox.Text = _currentTimeCard.Notes ?? string.Empty;

            CalculateAndDisplayTotals();
            UpdateSummaryLabels();
        }

        private void SetTimePickerValue(DateTimePicker picker, TimeSpan? timeValue)
        {
            if (timeValue.HasValue)
            {
                picker.Value = DateTime.Today.Add(timeValue.Value);
            }
            else
            {
                picker.Value = DateTime.Today.AddHours(8); // Default to 8 AM
            }
        }

        private void ShowEditPanel()
        {
            _editPanel.Visible = true;
            _driverCombo.Focus();
        }        private void CancelEdit()
        {
            _editPanel.Visible = false;
            _currentTimeCard = null!;
        }

        private void TogglePTOMode()
        {
            bool isPTO = _ptoCheckBox.Checked;

            // Disable time pickers when in PTO mode
            _clockInPicker.Enabled = !isPTO;
            _lunchOutPicker.Enabled = !isPTO;
            _lunchInPicker.Enabled = !isPTO;
            _clockOutPicker.Enabled = !isPTO;
            _routeAMOutPicker.Enabled = !isPTO;
            _routeAMInPicker.Enabled = !isPTO;
            _routePMOutPicker.Enabled = !isPTO;
            _routePMInPicker.Enabled = !isPTO;

            _ptoHoursUpDown.Enabled = isPTO;

            if (isPTO)
            {
                // Clear time values when switching to PTO
                SetTimePickerValue(_clockInPicker, null);
                SetTimePickerValue(_lunchOutPicker, null);
                SetTimePickerValue(_lunchInPicker, null);
                SetTimePickerValue(_clockOutPicker, null);
                SetTimePickerValue(_routeAMOutPicker, null);
                SetTimePickerValue(_routeAMInPicker, null);
                SetTimePickerValue(_routePMOutPicker, null);
                SetTimePickerValue(_routePMInPicker, null);
            }
            else
            {
                _ptoHoursUpDown.Value = 0;
            }

            CalculateAndDisplayTotals();
        }

        private void CalculateAndDisplayTotals()
        {
            if (_currentTimeCard == null) return;

            // Update current time card with form values
            UpdateTimeCardFromForm();

            // Calculate totals
            _currentTimeCard.CalculateTotalHours();

            // Display totals
            _totalHoursLabel.Text = _currentTimeCard.TotalTime.ToString("F2");
            _overtimeLabel.Text = _currentTimeCard.Overtime.ToString("F2");

            // Set colors based on values
            _overtimeLabel.ForeColor = _currentTimeCard.Overtime > 0 ? Color.Red : Color.Black;
            _totalHoursLabel.ForeColor = _currentTimeCard.TotalTime > 8 ? Color.Orange : Color.Black;
        }

        private async void UpdateSummaryLabels()
        {
            if (_currentTimeCard?.DriverId > 0 && _currentTimeCard.Date.HasValue)
            {
                try
                {
                    var monday = _currentTimeCard.Date.Value.AddDays(-(int)_currentTimeCard.Date.Value.DayOfWeek + 1);
                    var weeklyTotal = await _timeCardRepository.GetWeeklyTotalHoursAsync(_currentTimeCard.DriverId, monday);
                    _weeklyTotalLabel.Text = weeklyTotal.ToString("F2");

                    var monthlyTotal = await _timeCardRepository.GetMonthlyTotalHoursAsync(
                        _currentTimeCard.DriverId, _currentTimeCard.Date.Value.Year, _currentTimeCard.Date.Value.Month);
                    _monthlyTotalLabel.Text = monthlyTotal.ToString("F2");
                }                catch (Exception)
                {
                    _weeklyTotalLabel.Text = "Error";
                    _monthlyTotalLabel.Text = "Error";
                }
            }
        }

        private void UpdateTimeCardFromForm()
        {
            if (_currentTimeCard == null) return;

            _currentTimeCard.DriverId = (int)(_driverCombo.SelectedValue ?? 0);
            _currentTimeCard.Date = _datePicker.Value.Date;
            _currentTimeCard.IsRouteDay = _routeDayCheckBox.Checked;
            _currentTimeCard.IsPaidTimeOff = _ptoCheckBox.Checked;

            if (!_currentTimeCard.IsPaidTimeOff)
            {
                _currentTimeCard.ClockIn = GetTimeSpanFromPicker(_clockInPicker);
                _currentTimeCard.LunchOut = GetTimeSpanFromPicker(_lunchOutPicker);
                _currentTimeCard.LunchIn = GetTimeSpanFromPicker(_lunchInPicker);
                _currentTimeCard.ClockOut = GetTimeSpanFromPicker(_clockOutPicker);
                _currentTimeCard.RouteAMOut = GetTimeSpanFromPicker(_routeAMOutPicker);
                _currentTimeCard.RouteAMIn = GetTimeSpanFromPicker(_routeAMInPicker);
                _currentTimeCard.RoutePMOut = GetTimeSpanFromPicker(_routePMOutPicker);
                _currentTimeCard.RoutePMIn = GetTimeSpanFromPicker(_routePMInPicker);
                _currentTimeCard.PTOHours = 0;
            }
            else
            {
                _currentTimeCard.ClockIn = null;
                _currentTimeCard.LunchOut = null;
                _currentTimeCard.LunchIn = null;
                _currentTimeCard.ClockOut = null;
                _currentTimeCard.RouteAMOut = null;
                _currentTimeCard.RouteAMIn = null;
                _currentTimeCard.RoutePMOut = null;
                _currentTimeCard.RoutePMIn = null;
                _currentTimeCard.PTOHours = (double)_ptoHoursUpDown.Value;
            }

            _currentTimeCard.Notes = _notesTextBox.Text.Trim();
            if (_isEditMode)
                _currentTimeCard.ModifiedBy = Environment.UserName;
        }

        private TimeSpan? GetTimeSpanFromPicker(DateTimePicker picker)
        {
            // Only return a valid time if it's not the default (8 AM) or if other times are set
            return picker.Value.TimeOfDay;
        }

        private async void SaveTimeCard_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateTimeCardFromForm();

                // Validate the time card
                var warnings = await _validationService.ValidateTimeCardAsync(_currentTimeCard, !_isEditMode);

                if (warnings.Any())
                {
                    var result = TimeEntryWarningDialog.ShowWarnings(warnings, out bool shouldFixIssues);

                    if (result == DialogResult.Cancel)
                        return;
                      if (result == DialogResult.Retry || shouldFixIssues)
                    {
                        var wasFixed = _validationService.TryAutoFixIssues(_currentTimeCard, warnings);
                        if (wasFixed)
                        {
                            PopulateEditPanel(); // Refresh the form with fixed values
                            MessageBox.Show("Some issues have been automatically fixed. Please review and save again.",
                                "Issues Fixed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }

                    // Check if there are still high severity warnings
                    if (warnings.Any(w => w.Severity == WarningSeverity.High))
                    {
                        MessageBox.Show("Please fix the high severity issues before saving.",
                            "Cannot Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Save the time card
                if (_isEditMode)
                {
                    await _timeCardRepository.UpdateAsync(_currentTimeCard);
                    MessageBox.Show("Time card updated successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    await _timeCardRepository.AddAsync(_currentTimeCard);
                    MessageBox.Show("Time card added successfully.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                await LoadTimeCards();
                CancelEdit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving time card: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper methods from BaseDataForm pattern
        private Button CreateToolbarButton(string text, int x, int y, EventHandler clickHandler)
        {
            var button = new Button();
            button.Text = text;
            button.Location = new Point(x, y);
            button.Size = new Size(100, 25);
            button.Click += clickHandler;
            return button;
        }

        private ComboBox CreateComboBox(int x, int y, int width, Control parent)
        {
            var combo = new ComboBox();
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 23);
            parent.Controls.Add(combo);
            return combo;
        }

        private DateTimePicker CreateDatePicker(int x, int y, int width, Control parent)
        {
            var picker = new DateTimePicker();
            picker.Location = new Point(x, y);
            picker.Size = new Size(width, 23);
            picker.Format = DateTimePickerFormat.Short;
            parent.Controls.Add(picker);
            return picker;
        }

        private TextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            var textBox = new TextBox();
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 23);
            parent.Controls.Add(textBox);
            return textBox;
        }

        private Label CreateLabel(string text, int x, int y, Control parent)
        {
            var label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.AutoSize = true;
            parent.Controls.Add(label);
            return label;
        }
    }
}
