using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.Business;
using System.Diagnostics;

namespace BusBuddy.UI.Views
{
    public class TimeCardManagementForm : BaseDataForm
    {
        private readonly ITimeCardRepository _timeCardRepository;
        private readonly IPTOBalanceRepository _ptoBalanceRepository;
        private readonly TimeEntryValidationService _validationService;
        private DataGridView _timeCardGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        // Add/edit fields
        private Panel _editPanel;
        private DateTimePicker _datePicker;
        private TextBox _clockInTextBox;
        private TextBox _lunchOutTextBox;
        private TextBox _lunchInTextBox;
        private TextBox _clockOutTextBox;
        private TextBox _routeAMOutTextBox;
        private TextBox _routeAMInTextBox;
        private TextBox _routePMOutTextBox;
        private TextBox _routePMInTextBox;
        private TextBox _totalTimeTextBox;
        private TextBox _overtimeTextBox;
        private ComboBox _ptoHoursComboBox;
        private Label _ptoRemainingLabel;
        private CheckBox _routeDayCheckBox;
        private Button _saveButton;
        private Button _cancelButton;
        private FlowLayoutPanel _summaryPanel;
        private Label _lblWeekTotal;
        private Label _lblMonthTotal;

        public TimeCardManagementForm() : this(new TimeCardRepository(), new PTOBalanceRepository()) { }

        public TimeCardManagementForm(ITimeCardRepository timeCardRepository, IPTOBalanceRepository? ptoBalanceRepository = null)
            : base()
        {
            _timeCardRepository = timeCardRepository ?? throw new ArgumentNullException(nameof(timeCardRepository));
            _ptoBalanceRepository = ptoBalanceRepository ?? new PTOBalanceRepository();
            _validationService = new TimeEntryValidationService(_timeCardRepository);

            try
            {
                InitializeComponent();
                LoadTimeCards();

                // Edit panel is now always visible for easy access
                ClearEditFields(); // Start with clean fields
            }
            catch (Exception ex)
            {
                // Log initialization errors but don't show dialogs that could cause loops
                System.Diagnostics.Debug.WriteLine($"Error during form initialization: {ex.Message}");

                // Try to show the form anyway, even if data loading failed
                // Edit panel remains visible for user convenience
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Time Card Management";
            this.Size = new System.Drawing.Size(1200, 890);
            this.BackColor = Color.WhiteSmoke;
            this.KeyPreview = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScroll = true;

            // Summary panel with FlowLayoutPanel for better responsiveness
            _summaryPanel = new FlowLayoutPanel();
            _summaryPanel.Location = new Point(500, 10);
            _summaryPanel.Size = new Size(650, 40);
            _summaryPanel.BackColor = Color.FromArgb(240, 248, 255);
            _summaryPanel.FlowDirection = FlowDirection.LeftToRight;
            _summaryPanel.WrapContents = false;
            _summaryPanel.Padding = new Padding(10, 5, 10, 5);

            // Create and add main CRUD buttons to summary panel
            _addButton = CreateButton("Add New", 0, 0, (s, e) => AddNewTimeCard());
            _editButton = CreateButton("Edit", 0, 0, (s, e) => EditSelectedTimeCard());
            _deleteButton = CreateButton("Delete", 0, 0, (s, e) => DeleteSelectedTimeCard());
            _detailsButton = CreateButton("Details", 0, 0, (s, e) => ViewTimeCardDetails());

            _addButton.Margin = new Padding(0, 5, 10, 5);
            _editButton.Margin = new Padding(0, 5, 10, 5);
            _deleteButton.Margin = new Padding(0, 5, 10, 5);
            _detailsButton.Margin = new Padding(0, 5, 10, 5);

            _summaryPanel.Controls.Add(_addButton);
            _summaryPanel.Controls.Add(_editButton);
            _summaryPanel.Controls.Add(_deleteButton);
            _summaryPanel.Controls.Add(_detailsButton);

            _lblWeekTotal = new Label { Text = "Week Total: 0h", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, AutoSize = true, Margin = new Padding(0, 5, 20, 5) };
            _lblMonthTotal = new Label { Text = "Month Total: 0h", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, AutoSize = true, Margin = new Padding(0, 5, 20, 5) };

            _summaryPanel.Controls.Add(_lblWeekTotal);
            _summaryPanel.Controls.Add(_lblMonthTotal);
            this.Controls.Add(_summaryPanel);
            // Main grid - anchored Top, Left, Right (not Bottom to prevent overlap)
            _timeCardGrid = new DataGridView();
            _timeCardGrid.Location = new System.Drawing.Point(20, 60);
            _timeCardGrid.Size = new System.Drawing.Size(1150, 600);
            _timeCardGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _timeCardGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _timeCardGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _timeCardGrid.AllowUserToResizeColumns = true;
            _timeCardGrid.AllowUserToResizeRows = true;
            _timeCardGrid.ScrollBars = ScrollBars.Both;
            _timeCardGrid.DataBindingComplete += (s, e) => {
                if (_timeCardGrid.Columns.Contains("TimeCardID"))
                    _timeCardGrid.Columns["TimeCardID"].Visible = false;
            };
            this.Controls.Add(_timeCardGrid);
            _timeCardGrid.CellDoubleClick += (s, e) => EditSelectedTimeCard();
            _timeCardGrid.SelectionChanged += TimeCardGrid_SelectionChanged;
            InitializeEditPanel();
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;

            // Add resize event handler
            this.Resize += TimeCardManagementForm_Resize;

            // Diagnostic logging after initialization
            Debug.WriteLine($"TimeCardManagementForm Initialized: Size={this.ClientSize}, Grid={_timeCardGrid.Bounds}, EditPanel={_editPanel.Bounds}, SaveButton={_saveButton.Bounds}");
        }        private void TimeCardManagementForm_Resize(object sender, EventArgs e)
        {
            // Ensure _editPanel stays above bottom edge with dynamic positioning
            if (_timeCardGrid != null && _editPanel != null)
            {
                // Position edit panel at bottom with safe margin
                _editPanel.Location = new Point(20, Math.Max(60, this.ClientSize.Height - 240));
                _editPanel.Size = new Size(this.ClientSize.Width - 40, 220);

                // Validate _timeCardGrid height to avoid overlap with edit panel
                int gridHeight = Math.Max(200, _editPanel.Top - 80);
                _timeCardGrid.Size = new Size(this.ClientSize.Width - 40, gridHeight);

                // Ensure buttons are visible within edit panel
                if (_saveButton != null && _cancelButton != null)
                {
                    _saveButton.Location = new Point(Math.Max(10, _editPanel.Width - 360), 155);
                    _cancelButton.Location = new Point(Math.Max(10, _editPanel.Width - 240), 155);

                    // Find and adjust Quick Fix button and any other buttons
                    foreach (Control control in _editPanel.Controls)
                    {
                        if (control is Button button)
                        {
                            if (button.Text == "Quick Fix")
                            {
                                button.Location = new Point(Math.Max(10, _editPanel.Width - 120), 155);
                            }
                            else if (button.Text == "Help" || button.Text.Contains("Help"))
                            {
                                button.Location = new Point(Math.Max(10, _editPanel.Width - 80), 155);
                            }
                        }
                    }
                }

                // Diagnostic logging for resize events
                Debug.WriteLine($"Resize: FormSize={this.ClientSize}, Grid={_timeCardGrid.Bounds}, EditPanel={_editPanel.Bounds}, SaveButton={_saveButton?.Bounds}, SaveVisible={_saveButton?.Visible}");
            }
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, this.ClientSize.Height - 240);
            _editPanel.Size = new System.Drawing.Size(this.ClientSize.Width - 40, 220);
            _editPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _editPanel.Visible = true; // Always visible for easy access
            _editPanel.BackColor = Color.AliceBlue;
            _editPanel.BorderStyle = BorderStyle.FixedSingle;
            _editPanel.AutoScroll = true;
            this.Controls.Add(_editPanel);
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(70, 10);
            _datePicker.Size = new System.Drawing.Size(180, 23);
            _datePicker.Format = DateTimePickerFormat.Custom;
            _datePicker.CustomFormat = "dd MMM yyyy";
            _datePicker.Enter += (s, e) => ValidateCurrentTimeCardExists();
            _editPanel.Controls.Add(_datePicker);
            _routeDayCheckBox = new CheckBox { Text = "Route Day", Location = new Point(250, 12), AutoSize = true };
            _routeDayCheckBox.Click += OnCheckBoxClick;
            _routeDayCheckBox.CheckedChanged += (s, e) => CalculateTotals();
            _editPanel.Controls.Add(_routeDayCheckBox);
            // Normal workday group
            GroupBox normalGroup = new GroupBox { Text = "Normal Workday", Location = new Point(10, 45), Size = new Size(540, 60), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            normalGroup.Controls.Add(new Label { Text = "Clock In:", Location = new Point(10, 25), AutoSize = true });
            _clockInTextBox = new TextBox { Location = new Point(70, 22), Size = new Size(60, 23) };
            _clockInTextBox.Enter += OnTextBoxEnter;
            _clockInTextBox.Leave += (s, e) => FormatTimeInput(_clockInTextBox);
            _clockInTextBox.KeyPress += TimeTextBox_KeyPress;
            normalGroup.Controls.Add(_clockInTextBox);

            normalGroup.Controls.Add(new Label { Text = "Lunch Out:", Location = new Point(140, 25), AutoSize = true });
            _lunchOutTextBox = new TextBox { Location = new Point(210, 22), Size = new Size(60, 23) };
            _lunchOutTextBox.Enter += OnTextBoxEnter;
            _lunchOutTextBox.Leave += (s, e) => FormatTimeInput(_lunchOutTextBox);
            _lunchOutTextBox.KeyPress += TimeTextBox_KeyPress;
            normalGroup.Controls.Add(_lunchOutTextBox);

            normalGroup.Controls.Add(new Label { Text = "Lunch In:", Location = new Point(280, 25), AutoSize = true });
            _lunchInTextBox = new TextBox { Location = new Point(340, 22), Size = new Size(60, 23) };
            _lunchInTextBox.Enter += OnTextBoxEnter;
            _lunchInTextBox.Leave += (s, e) => FormatTimeInput(_lunchInTextBox);
            _lunchInTextBox.KeyPress += TimeTextBox_KeyPress;
            normalGroup.Controls.Add(_lunchInTextBox);

            normalGroup.Controls.Add(new Label { Text = "Clock Out:", Location = new Point(410, 25), AutoSize = true });
            _clockOutTextBox = new TextBox { Location = new Point(480, 22), Size = new Size(60, 23) };
            _clockOutTextBox.Enter += OnTextBoxEnter;
            _clockOutTextBox.Leave += (s, e) => FormatTimeInput(_clockOutTextBox);
            _clockOutTextBox.KeyPress += TimeTextBox_KeyPress;
            normalGroup.Controls.Add(_clockOutTextBox);
            _editPanel.Controls.Add(normalGroup);
            // Route day group
            GroupBox routeGroup = new GroupBox { Text = "Route Events (if Route Day)", Location = new Point(570, 45), Size = new Size(560, 60), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            routeGroup.Controls.Add(new Label { Text = "AM Out:", Location = new Point(10, 25), AutoSize = true });
            _routeAMOutTextBox = new TextBox { Location = new Point(70, 22), Size = new Size(60, 23) };
            _routeAMOutTextBox.Enter += OnTextBoxEnter;
            _routeAMOutTextBox.Leave += (s, e) => FormatTimeInput(_routeAMOutTextBox);
            _routeAMOutTextBox.KeyPress += TimeTextBox_KeyPress;
            routeGroup.Controls.Add(_routeAMOutTextBox);

            routeGroup.Controls.Add(new Label { Text = "AM In:", Location = new Point(140, 25), AutoSize = true });
            _routeAMInTextBox = new TextBox { Location = new Point(200, 22), Size = new Size(60, 23) };
            _routeAMInTextBox.Enter += OnTextBoxEnter;
            _routeAMInTextBox.Leave += (s, e) => FormatTimeInput(_routeAMInTextBox);
            _routeAMInTextBox.KeyPress += TimeTextBox_KeyPress;
            routeGroup.Controls.Add(_routeAMInTextBox);

            routeGroup.Controls.Add(new Label { Text = "PM Out:", Location = new Point(270, 25), AutoSize = true });
            _routePMOutTextBox = new TextBox { Location = new Point(340, 22), Size = new Size(60, 23) };
            _routePMOutTextBox.Enter += OnTextBoxEnter;
            _routePMOutTextBox.Leave += (s, e) => FormatTimeInput(_routePMOutTextBox);
            _routePMOutTextBox.KeyPress += TimeTextBox_KeyPress;
            routeGroup.Controls.Add(_routePMOutTextBox);

            routeGroup.Controls.Add(new Label { Text = "PM In:", Location = new Point(410, 25), AutoSize = true });
            _routePMInTextBox = new TextBox { Location = new Point(470, 22), Size = new Size(60, 23) };
            _routePMInTextBox.Enter += OnTextBoxEnter;
            _routePMInTextBox.Leave += (s, e) => FormatTimeInput(_routePMInTextBox);
            _routePMInTextBox.KeyPress += TimeTextBox_KeyPress;
            routeGroup.Controls.Add(_routePMInTextBox);
            _editPanel.Controls.Add(routeGroup);
            // Totals
            var totalLabel = CreateLabel("Total Time:", 10, 120);
            _editPanel.Controls.Add(totalLabel);
            _totalTimeTextBox = new TextBox { Location = new Point(90, 117), Size = new Size(80, 23), ReadOnly = true, BackColor = Color.WhiteSmoke };
            _editPanel.Controls.Add(_totalTimeTextBox);

            var overtimeLabel = CreateLabel("Overtime:", 200, 120);
            _editPanel.Controls.Add(overtimeLabel);
            _overtimeTextBox = new TextBox { Location = new Point(270, 117), Size = new Size(80, 23), ReadOnly = true, BackColor = Color.WhiteSmoke };
            _editPanel.Controls.Add(_overtimeTextBox);

            // PTO controls in a grouped area
            var ptoGroup = new GroupBox
            {
                Text = "Personal Time Off",
                Location = new Point(360, 90),
                Size = new Size(620, 55),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };

            // Apply PTO button for easier workflow
            var applyPTOButton = new Button
            {
                Text = "Apply PTO",
                Location = new Point(10, 22),
                Size = new Size(80, 25),
                BackColor = Color.LightGreen,
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            applyPTOButton.Click += ApplyPTO_Click;
            ptoGroup.Controls.Add(applyPTOButton);

            var ptoLabel = new Label
            {
                Text = "PTO Hours:",
                Location = new Point(100, 25),
                AutoSize = true
            };
            ptoGroup.Controls.Add(ptoLabel);

            _ptoHoursComboBox = new ComboBox
            {
                Location = new Point(180, 22),
                Size = new Size(80, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _ptoHoursComboBox.DropDown += OnComboBoxDropDown;
            // Add PTO options in 2-hour increments
            _ptoHoursComboBox.Items.AddRange(new object[] { 0, 2, 4, 6, 8 });
            _ptoHoursComboBox.SelectedIndex = 0;
            // Remove automatic PTO validation - only validate when user explicitly uses "Apply PTO"
            ptoGroup.Controls.Add(_ptoHoursComboBox);

            // PTO remaining display with better spacing
            _ptoRemainingLabel = new Label
            {
                Location = new Point(280, 25),
                Size = new Size(280, 23),
                Text = "PTO Remaining: Click 'Check PTO' to load",
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            ptoGroup.Controls.Add(_ptoRemainingLabel);

            // Check PTO button
            var checkPTOButton = new Button
            {
                Text = "Check PTO",
                Location = new Point(570, 22),
                Size = new Size(80, 25),
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            checkPTOButton.Click += (s, e) => UpdatePTORemainingDisplay();
            ptoGroup.Controls.Add(checkPTOButton);
            _editPanel.Controls.Add(ptoGroup);

            _saveButton = CreateButton("Save", _editPanel.Width - 320, 155, (s, e) => SaveTimeCard());
            _saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _editPanel.Controls.Add(_saveButton);

            _cancelButton = CreateButton("Cancel", _editPanel.Width - 210, 155, (s, e) => CancelEdit());
            _cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _editPanel.Controls.Add(_cancelButton);

            // Add Quick Fix button with better spacing
            var quickFixButton = CreateButton("Quick Fix", _editPanel.Width - 100, 155, (s, e) => ApplyCommonTimeFixes());
            quickFixButton.BackColor = Color.LightBlue;
            quickFixButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _editPanel.Controls.Add(quickFixButton);

            // Add Help button
            var helpButton = CreateButton("Help", 1310, 155, (s, e) => ShowQuickFixSuggestions());
            helpButton.BackColor = Color.LightGray;
            _editPanel.Controls.Add(helpButton);
        }

        // --- CRUD and event handlers ---
        private bool _isEditing = false;
        private bool _isUpdatingPTOSelection = false;
        private TimeCard _currentTimeCard;

        private void LoadTimeCards()
        {
            try
            {
                // Ensure grid is initialized
                if (_timeCardGrid == null)
                {
                    return; // Exit silently if grid not ready
                }

                var timeCards = _timeCardRepository.GetAllTimeCards();

                // Sort by date - oldest first (ascending order)
                timeCards = timeCards.OrderBy(tc => tc.Date ?? DateTime.MinValue).ToList();

                // Recalculate all time cards to ensure correct totals
                RecalculateAllTimeCards(timeCards);

                _timeCardGrid.DataSource = null;
                _timeCardGrid.DataSource = timeCards;

                if (_timeCardGrid.Columns.Count > 0)
                {
                    // Only set column properties if columns exist
                    if (_timeCardGrid.Columns.Contains("TimeCardID"))
                        _timeCardGrid.Columns["TimeCardID"].HeaderText = "ID";
                    if (_timeCardGrid.Columns.Contains("Date"))
                    {
                        _timeCardGrid.Columns["Date"].HeaderText = "Date";
                        _timeCardGrid.Columns["Date"].DefaultCellStyle.Format = "dd MMM yyyy";
                    }
                    if (_timeCardGrid.Columns.Contains("DayType"))
                        _timeCardGrid.Columns["DayType"].HeaderText = "Day Type";
                    if (_timeCardGrid.Columns.Contains("AMClockIn"))
                        _timeCardGrid.Columns["AMClockIn"].HeaderText = "Clock In";
                    if (_timeCardGrid.Columns.Contains("LunchClockOut"))
                        _timeCardGrid.Columns["LunchClockOut"].HeaderText = "Lunch Out";
                    if (_timeCardGrid.Columns.Contains("LunchClockIn"))
                        _timeCardGrid.Columns["LunchClockIn"].HeaderText = "Lunch In";
                    if (_timeCardGrid.Columns.Contains("PMClockOut"))
                        _timeCardGrid.Columns["PMClockOut"].HeaderText = "Clock Out";
                    if (_timeCardGrid.Columns.Contains("TotalTime"))
                    {
                        _timeCardGrid.Columns["TotalTime"].HeaderText = "Total";
                        _timeCardGrid.Columns["TotalTime"].DefaultCellStyle.Format = "F1";
                    }
                    if (_timeCardGrid.Columns.Contains("PTOHours"))
                    {
                        _timeCardGrid.Columns["PTOHours"].HeaderText = "PTO";
                        _timeCardGrid.Columns["PTOHours"].DefaultCellStyle.Format = "F1";
                    }
                    if (_timeCardGrid.Columns.Contains("Overtime"))
                    {
                        _timeCardGrid.Columns["Overtime"].HeaderText = "Overtime";
                        _timeCardGrid.Columns["Overtime"].DefaultCellStyle.Format = "F1";
                    }
                    if (_timeCardGrid.Columns.Contains("WeeklyTotal"))
                    {
                        _timeCardGrid.Columns["WeeklyTotal"].HeaderText = "Weekly Total";
                        _timeCardGrid.Columns["WeeklyTotal"].DefaultCellStyle.Format = "F1";
                    }
                    if (_timeCardGrid.Columns.Contains("MonthlyTotal"))
                    {
                        _timeCardGrid.Columns["MonthlyTotal"].HeaderText = "Monthly Total";
                        _timeCardGrid.Columns["MonthlyTotal"].DefaultCellStyle.Format = "F1";
                    }
                }

                UpdateSummary(timeCards);

                // After calculating running totals, update them in the database
                UpdateRunningTotalsInDatabase(timeCards);

                // Don't automatically update PTO display during form load - only update when explicitly needed
            }
            catch (Exception ex)
            {
                // Don't show dialog during initialization - just log the error
                if (_timeCardGrid != null && _timeCardGrid.Created)
                {
                    ShowErrorMessage($"Error loading time cards: {ex.Message}");
                }
                else
                {
                    // Form is still initializing, log but don't show dialog
                    System.Diagnostics.Debug.WriteLine($"Error during initialization: {ex.Message}");
                }
            }
        }

        private void UpdateSummary(List<TimeCard> timeCards)
        {
            // Calculate current week and month totals for display
            var thisWeek = GetWeekStart(DateTime.Today);
            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            decimal currentWeekTotal = 0;
            decimal currentMonthTotal = 0;

            // Sort all time cards by date for proper running totals
            var sortedTimeCards = timeCards.OrderBy(tc => tc.Date ?? DateTime.MinValue).ToList();

            // Calculate running totals for ALL time cards
            decimal runningWeeklyTotal = 0;
            DateTime? currentWeekStart = null;

            // Group time cards by month for monthly total calculation
            var timeCardsByMonth = sortedTimeCards
                .Where(tc => tc.Date.HasValue)
                .GroupBy(tc => new DateTime(tc.Date!.Value.Year, tc.Date!.Value.Month, 1))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var tc in sortedTimeCards)
            {
                if (tc.Date.HasValue)
                {
                    var date = tc.Date.Value;
                    var weekStart = GetWeekStart(date);
                    var monthStart = new DateTime(date.Year, date.Month, 1);

                    // Reset weekly total if we've moved to a new week
                    if (currentWeekStart == null || weekStart != currentWeekStart)
                    {
                        runningWeeklyTotal = 0;
                        currentWeekStart = weekStart;
                    }

                    // Add this day's total to weekly running total
                    decimal dailyTotal = (tc.TotalTime ?? 0) + (tc.PTOHours ?? 0);
                    runningWeeklyTotal += dailyTotal;

                    // Calculate monthly total for the entire calendar month
                    decimal monthlyTotal = 0;
                    if (timeCardsByMonth.ContainsKey(monthStart))
                    {
                        monthlyTotal = timeCardsByMonth[monthStart]
                            .Sum(card => (card.TotalTime ?? 0) + (card.PTOHours ?? 0));
                    }

                    // Update the time card with running totals
                    tc.WeeklyTotal = runningWeeklyTotal;
                    tc.MonthlyTotal = monthlyTotal;

                    // Track current week/month totals for summary display
                    if (date >= thisWeek && date < thisWeek.AddDays(7))
                    {
                        currentWeekTotal += dailyTotal;
                    }
                    if (date >= thisMonth && date < thisMonth.AddMonths(1))
                    {
                        currentMonthTotal += dailyTotal;
                    }
                }
            }

            _lblWeekTotal.Text = $"Week Total: {currentWeekTotal:F1}h";
            _lblMonthTotal.Text = $"Month Total: {currentMonthTotal:F1}h";
        }

        private DateTime GetWeekStart(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void AddNewTimeCard()
        {
            try
            {
                _isEditing = false;
                _currentTimeCard = new TimeCard();
                ClearEditFields();
                _datePicker.Value = DateTime.Today;
                // Don't automatically validate PTO options - only do this when user interacts with PTO

                // Enable the save button
                _saveButton.Enabled = true;
                _saveButton.Text = "Save New Entry";

                // Don't show automatic success message - let user work without interruption
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error initializing new time card: {ex.Message}");
            }
        }

        private void EditSelectedTimeCard()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            _isEditing = true;
            int selectedId = (int)_timeCardGrid.SelectedRows[0].Cells["TimeCardID"].Value;
            _currentTimeCard = _timeCardRepository.GetTimeCardById(selectedId);

            if (_currentTimeCard == null)
            {
                ShowErrorMessage("Could not find the selected time card.");
                return;
            }

            PopulateEditFields(_currentTimeCard);
            // Don't automatically validate PTO options - only do this when user interacts with PTO

            // Enable the save button for editing
            _saveButton.Enabled = true;
            _saveButton.Text = "Update Entry";
        }

        private void DeleteSelectedTimeCard()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            int selectedId = (int)_timeCardGrid.SelectedRows[0].Cells["TimeCardID"].Value;
            if (!ConfirmDelete()) return;

            try
            {
                _timeCardRepository.DeleteTimeCard(selectedId);
                LoadTimeCards();
                ShowSuccessMessage("Time card deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting time card: {ex.Message}");
            }
        }

        private void ViewTimeCardDetails()
        {
            if (_timeCardGrid.SelectedRows.Count == 0) return;

            int selectedId = (int)_timeCardGrid.SelectedRows[0].Cells["TimeCardID"].Value;
            var timeCard = _timeCardRepository.GetTimeCardById(selectedId);

            if (timeCard != null)
            {
                string details = $"Time Card Details:\n" +
                    $"Date: {timeCard.Date?.ToShortDateString()}\n" +
                    $"Day Type: {timeCard.DayType}\n" +
                    $"Clock In: {timeCard.AMClockIn}\n" +
                    $"Lunch Out: {timeCard.LunchClockOut}\n" +
                    $"Lunch In: {timeCard.LunchClockIn}\n" +
                    $"Clock Out: {timeCard.PMClockOut}\n" +
                    $"Total Time: {timeCard.TotalTime?.ToString("F1") ?? "0.0"}h\n" +
                    $"Overtime: {timeCard.Overtime?.ToString("F1") ?? "0.0"}h\n" +
                    $"PTO Hours: {timeCard.PTOHours?.ToString("F1") ?? "0.0"}h";

                MessageBox.Show(details, "Time Card Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load time card details.");
            }
        }

        private void TimeCardGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _timeCardGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void SaveTimeCard()
        {
            if (!ValidateTimeCardForm()) return;

            try
            {
                PopulateTimeCardFromFields();
                CalculateTotals();

                // Validate for potential clock-in/out issues
                var warnings = _validationService.ValidateTimeCard(_currentTimeCard, !_isEditing);

                if (warnings.Count > 0)
                {
                    var result = TimeEntryWarningDialog.ShowWarnings(warnings, out bool shouldFixIssues);

                    if (result == DialogResult.Cancel)
                    {
                        return; // User cancelled the save
                    }

                    if (shouldFixIssues)
                    {
                        // Auto-fix common time entry issues
                        ApplyAutoFixes(warnings);

                        // Recalculate totals after fixes
                        CalculateTotals();

                        // Show message about what was fixed
                        ShowSuccessMessage("Time entries have been automatically corrected. Please review and save again if satisfied.");
                        return; // Keep edit panel open for user to review
                    }

                    // User chose to accept as-is - proceed with save
                }

                if (_isEditing)
                {
                    _timeCardRepository.UpdateTimeCard(_currentTimeCard);
                    ShowSuccessMessage("Time card updated successfully.");
                }
                else
                {
                    _timeCardRepository.AddTimeCard(_currentTimeCard);
                    ShowSuccessMessage("Time card added successfully.");
                }

                // Don't automatically update PTO balance - only update when explicitly applying PTO
                // UpdatePTOBalance();

                LoadTimeCards();
                ClearEditFields(); // Clear fields for next entry
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving time card: {ex.Message}");
            }
        }

        private void UpdatePTOBalance()
        {
            try
            {
                var currentSchoolYear = GetSchoolYear(DateTime.Today);
                var ptoBalance = _ptoBalanceRepository.GetPTOBalanceForYear(currentSchoolYear);

                // Recalculate total PTO used for the school year
                var timeCards = _timeCardRepository.GetAllTimeCards();
                var schoolYearStart = GetSchoolYearStart(currentSchoolYear);
                var schoolYearEnd = GetSchoolYearEnd(currentSchoolYear);

                var totalPTOUsed = timeCards
                    .Where(tc => tc.Date.HasValue &&
                                tc.Date.Value >= schoolYearStart &&
                                tc.Date.Value <= schoolYearEnd)
                    .Sum(tc => tc.PTOHours ?? 0);

                // Update the PTO balance
                ptoBalance.PTOUsed = totalPTOUsed;
                ptoBalance.LastUpdated = DateTime.Now;

                _ptoBalanceRepository.UpdatePTOBalance(ptoBalance);
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Warning: PTO balance update failed: {ex.Message}");
            }
        }

        private void ClearEditFields()
        {
            // Temporarily disable PTO validation during field clearing
            _isUpdatingPTOSelection = true;

            try
            {
                _datePicker.Value = DateTime.Today;
                _clockInTextBox.Text = "";
                _lunchOutTextBox.Text = "";
                _lunchInTextBox.Text = "";
                _clockOutTextBox.Text = "";
                _routeAMOutTextBox.Text = "";
                _routeAMInTextBox.Text = "";
                _routePMOutTextBox.Text = "";
                _routePMInTextBox.Text = "";
                _totalTimeTextBox.Text = "";
                _overtimeTextBox.Text = "";
                _ptoHoursComboBox.SelectedIndex = 0; // This won't trigger validation now
                _routeDayCheckBox.Checked = false;
            }
            finally
            {
                _isUpdatingPTOSelection = false;
            }
        }

        private void PopulateEditFields(TimeCard timeCard)
        {
            try
            {
                _isUpdatingPTOSelection = true; // Prevent PTO validation during field population

                _datePicker.Value = timeCard.Date ?? DateTime.Today;
                _clockInTextBox.Text = timeCard.AMClockIn?.ToString(@"hh\:mm") ?? "";
                _lunchOutTextBox.Text = timeCard.LunchClockOut?.ToString(@"hh\:mm") ?? "";
                _lunchInTextBox.Text = timeCard.LunchClockIn?.ToString(@"hh\:mm") ?? "";
                _clockOutTextBox.Text = timeCard.PMClockOut?.ToString(@"hh\:mm") ?? "";
                _routeAMOutTextBox.Text = timeCard.RouteAMClockOut?.ToString(@"hh\:mm") ?? "";
                _routeAMInTextBox.Text = timeCard.RouteAMClockIn?.ToString(@"hh\:mm") ?? "";
                _routePMOutTextBox.Text = timeCard.RoutePMClockOut?.ToString(@"hh\:mm") ?? "";
                _routePMInTextBox.Text = timeCard.RoutePMClockIn?.ToString(@"hh\:mm") ?? "";
                _totalTimeTextBox.Text = timeCard.TotalTime?.ToString("F1") ?? "";
                _overtimeTextBox.Text = timeCard.Overtime?.ToString("F1") ?? "";

                // Set PTO hours
                decimal ptoHours = timeCard.PTOHours ?? 0;
                int ptoIndex = _ptoHoursComboBox.Items.IndexOf(Convert.ToInt32(ptoHours));
                _ptoHoursComboBox.SelectedIndex = ptoIndex >= 0 ? ptoIndex : 0;

                _routeDayCheckBox.Checked = timeCard.DayType == "Route Day";
            }
            finally
            {
                _isUpdatingPTOSelection = false;
            }
        }

        private void PopulateTimeCardFromFields()
        {
            _currentTimeCard.Date = _datePicker.Value.Date;
            _currentTimeCard.DayType = _routeDayCheckBox.Checked ? "Route Day" : "Regular Day";
            _currentTimeCard.AMClockIn = ParseTime(_clockInTextBox.Text.Trim());
            _currentTimeCard.LunchClockOut = ParseTime(_lunchOutTextBox.Text.Trim());
            _currentTimeCard.LunchClockIn = ParseTime(_lunchInTextBox.Text.Trim());
            _currentTimeCard.PMClockOut = ParseTime(_clockOutTextBox.Text.Trim());
            _currentTimeCard.RouteAMClockOut = ParseTime(_routeAMOutTextBox.Text.Trim());
            _currentTimeCard.RouteAMClockIn = ParseTime(_routeAMInTextBox.Text.Trim());
            _currentTimeCard.RoutePMClockOut = ParseTime(_routePMOutTextBox.Text.Trim());
            _currentTimeCard.RoutePMClockIn = ParseTime(_routePMInTextBox.Text.Trim());
            _currentTimeCard.PTOHours = Convert.ToDecimal(_ptoHoursComboBox.SelectedItem ?? 0);

            // CRITICAL FIX: Save the calculated totals from the textboxes
            if (decimal.TryParse(_totalTimeTextBox.Text, out decimal totalTime))
                _currentTimeCard.TotalTime = totalTime;

            if (decimal.TryParse(_overtimeTextBox.Text, out decimal overtime))
                _currentTimeCard.Overtime = overtime;
        }

        // Time formatting methods
        private void TimeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow numbers, backspace, delete, and colon
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ':')
            {
                e.Handled = true;
            }
        }

        private void FormatTimeInput(TextBox textBox)
        {
            string input = textBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
                return;

            string formattedTime = FormatTimeString(input);
            if (!string.IsNullOrEmpty(formattedTime))
            {
                textBox.Text = formattedTime;
                // Trigger calculation when time changes
                CalculateTotals();

                // Perform real-time validation check
                PerformRealTimeValidation(textBox);
            }
        }

        private void PerformRealTimeValidation(TextBox changedTextBox)
        {
            try
            {
                // Create a temporary time card with current values
                var tempTimeCard = new TimeCard();
                PopulateTimeCardFromCurrentFields(tempTimeCard);

                var warnings = _validationService.ValidateTimeCard(tempTimeCard, !_isEditing);

                // Show warning icons and tooltips for all warnings
                if (warnings.Count > 0)
                {
                    UpdateFieldWarningIcons(tempTimeCard, warnings);
                }
                else
                {
                    ClearAllWarningIcons();
                }
            }
            catch
            {
                // Ignore validation errors during real-time checking
            }
        }

        private void PopulateTimeCardFromCurrentFields(TimeCard timeCard)
        {
            timeCard.Date = _datePicker.Value.Date;
            timeCard.DayType = _routeDayCheckBox.Checked ? "Route Day" : "Regular Day";
            timeCard.AMClockIn = ParseTime(_clockInTextBox.Text.Trim());
            timeCard.LunchClockOut = ParseTime(_lunchOutTextBox.Text.Trim());
            timeCard.LunchClockIn = ParseTime(_lunchInTextBox.Text.Trim());
            timeCard.PMClockOut = ParseTime(_clockOutTextBox.Text.Trim());
            timeCard.RouteAMClockOut = ParseTime(_routeAMOutTextBox.Text.Trim());
            timeCard.RouteAMClockIn = ParseTime(_routeAMInTextBox.Text.Trim());
            timeCard.RoutePMClockOut = ParseTime(_routePMOutTextBox.Text.Trim());
            timeCard.RoutePMClockIn = ParseTime(_routePMInTextBox.Text.Trim());
            timeCard.PTOHours = (decimal)(_ptoHoursComboBox.SelectedItem ?? 0);
        }

        private void UpdateFieldWarningIcons(TimeCard timeCard, List<TimeEntryWarning> warnings)
        {
            // Clear existing warnings first
            ClearAllWarningIcons();

            foreach (var warning in warnings)
            {
                TextBox? targetTextBox = null;
                string tooltipMessage = $"{warning.Message}\nSuggested: {warning.SuggestedAction}";
                Color backColor, foreColor;

                // Set visual styles based on severity
                switch (warning.Severity)
                {
                    case WarningSeverity.High:
                        backColor = Color.LightYellow;
                        foreColor = Color.DarkRed;
                        break;
                    case WarningSeverity.Medium:
                        backColor = Color.PaleGoldenrod;
                        foreColor = Color.DarkOrange;
                        break;
                    case WarningSeverity.Low:
                        backColor = Color.LightBlue;
                        foreColor = Color.DarkBlue;
                        break;
                    default:
                        continue;
                }

                // Map warning types to textboxes
                switch (warning.Type)
                {
                    case WarningType.MissingClockIn:
                        targetTextBox = _clockInTextBox;
                        break;
                    case WarningType.VeryEarlyClockIn:
                        targetTextBox = _clockInTextBox;
                        break;
                    case WarningType.VeryLateClockOut:
                        targetTextBox = _clockOutTextBox;
                        break;
                    case WarningType.ShortLunchBreak:
                    case WarningType.LongLunchBreak:
                        // Apply to both lunch textboxes
                        SetFieldWarning(_lunchOutTextBox, tooltipMessage, backColor, foreColor);
                        SetFieldWarning(_lunchInTextBox, tooltipMessage, backColor, foreColor);
                        continue; // Skip setting targetTextBox since we handle both
                    case WarningType.IncompleteRouteEntry:
                        if (timeCard.RouteAMClockOut != null && timeCard.RouteAMClockIn == null)
                            targetTextBox = _routeAMInTextBox;
                        else if (timeCard.RoutePMClockOut != null && timeCard.RoutePMClockIn == null)
                            targetTextBox = _routePMInTextBox;
                        break;
                    case WarningType.ExcessiveWorkHours:
                        // Apply to both clock-in and clock-out
                        SetFieldWarning(_clockInTextBox, tooltipMessage, backColor, foreColor);
                        SetFieldWarning(_clockOutTextBox, tooltipMessage, backColor, foreColor);
                        continue;
                    case WarningType.MissingPreviousClockOut:
                        // No specific textbox; skip UI feedback (handled in dialog)
                        continue;
                }

                if (targetTextBox != null)
                {
                    SetFieldWarning(targetTextBox, tooltipMessage, backColor, foreColor);
                }
            }
        }

        private void SetFieldWarning(TextBox textBox, string message, Color backColor, Color foreColor)
        {
            textBox.BackColor = backColor;
            textBox.ForeColor = foreColor;

            // Add tooltip with warning message
            var toolTip = new ToolTip
            {
                AutoPopDelay = 5000, // Show for 5 seconds
                InitialDelay = 500,  // Appear after 0.5 seconds
                ReshowDelay = 500,   // Reappear quickly
                ShowAlways = true    // Show even if control is disabled
            };
            toolTip.SetToolTip(textBox, message);
        }

        private void ClearAllWarningIcons()
        {
            var timeTextBoxes = new[] { _clockInTextBox, _lunchOutTextBox, _lunchInTextBox, _clockOutTextBox,
                                      _routeAMOutTextBox, _routeAMInTextBox, _routePMOutTextBox, _routePMInTextBox };

            foreach (var textBox in timeTextBoxes)
            {
                textBox.BackColor = Color.White;
                textBox.ForeColor = Color.Black;

                // Remove tooltip
                var toolTip = new ToolTip();
                toolTip.SetToolTip(textBox, null);
            }
        }

        private string FormatTimeString(string input)
        {
            // Remove any non-digit characters except colon
            string digits = new string(input.Where(c => char.IsDigit(c) || c == ':').ToArray());

            // If already formatted (contains colon), validate and return
            if (digits.Contains(':'))
            {
                if (TimeSpan.TryParse(digits, out TimeSpan result))
                {
                    return result.ToString(@"h\:mm");
                }
                return digits; // Return as-is if invalid
            }

            // Format numeric input
            if (digits.Length == 0) return "";

            // Handle common formats
            switch (digits.Length)
            {
                case 1:
                case 2:
                    // 9 -> 9:00, 12 -> 12:00
                    if (int.TryParse(digits, out int hours) && hours >= 0 && hours <= 23)
                        return $"{hours}:00";
                    break;

                case 3:
                    // 900 -> 9:00, 130 -> 1:30
                    string h3 = digits.Substring(0, 1);
                    string m3 = digits.Substring(1, 2);
                    if (int.TryParse(h3, out int h3Val) && int.TryParse(m3, out int m3Val) &&
                        h3Val >= 0 && h3Val <= 23 && m3Val >= 0 && m3Val <= 59)
                        return $"{h3Val}:{m3Val:D2}";
                    break;

                case 4:
                    // 0900 -> 9:00, 1430 -> 14:30
                    string h4 = digits.Substring(0, 2);
                    string m4 = digits.Substring(2, 2);
                    if (int.TryParse(h4, out int h4Val) && int.TryParse(m4, out int m4Val) &&
                        h4Val >= 0 && h4Val <= 23 && m4Val >= 0 && m4Val <= 59)
                        return $"{h4Val}:{m4Val:D2}";
                    break;
            }

            return input; // Return original if can't format
        }

        private TimeSpan? ParseTime(string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
                return null;

            if (TimeSpan.TryParse(timeText, out TimeSpan result))
                return result;

            // Try parsing common formats like "8:30" or "08:30"
            if (DateTime.TryParse(timeText, out DateTime dateTime))
                return dateTime.TimeOfDay;

            return null;
        }

        private void CalculateTotals()
        {
            if (_currentTimeCard == null) return;

            decimal totalHours = 0;

            // Calculate regular work hours
            var clockIn = ParseTime(_clockInTextBox.Text.Trim());
            var lunchOut = ParseTime(_lunchOutTextBox.Text.Trim());
            var lunchIn = ParseTime(_lunchInTextBox.Text.Trim());
            var clockOut = ParseTime(_clockOutTextBox.Text.Trim());

            if (clockIn.HasValue && clockOut.HasValue)
            {
                // Handle overnight shifts - if clock out is before clock in, add 24 hours
                var workTime = clockOut.Value - clockIn.Value;
                if (workTime.TotalHours < 0)
                {
                    workTime = workTime.Add(TimeSpan.FromHours(24));
                }

                // Handle lunch break
                if (lunchOut.HasValue && lunchIn.HasValue)
                {
                    var lunchBreak = lunchIn.Value - lunchOut.Value;

                    // Handle overnight lunch break
                    if (lunchBreak.TotalHours < 0)
                    {
                        lunchBreak = lunchBreak.Add(TimeSpan.FromHours(24));
                    }

                    // Only subtract lunch if it's reasonable (between 15 minutes and 2 hours)
                    if (lunchBreak.TotalMinutes >= 15 && lunchBreak.TotalHours <= 2)
                    {
                        workTime = workTime - lunchBreak;
                    }
                }

                totalHours = (decimal)workTime.TotalHours;

                // Ensure no negative hours
                totalHours = Math.Max(0, totalHours);
            }

            // Add route time if it's a route day
            if (_routeDayCheckBox.Checked)
            {
                var routeAMOut = ParseTime(_routeAMOutTextBox.Text.Trim());
                var routeAMIn = ParseTime(_routeAMInTextBox.Text.Trim());
                var routePMOut = ParseTime(_routePMOutTextBox.Text.Trim());
                var routePMIn = ParseTime(_routePMInTextBox.Text.Trim());

                // Add AM route time
                if (routeAMOut.HasValue && routeAMIn.HasValue)
                {
                    var amRouteTime = routeAMIn.Value - routeAMOut.Value;

                    // Handle overnight route times
                    if (amRouteTime.TotalHours < 0)
                    {
                        amRouteTime = amRouteTime.Add(TimeSpan.FromHours(24));
                    }

                    // Only add reasonable route times (up to 4 hours)
                    if (amRouteTime.TotalHours > 0 && amRouteTime.TotalHours <= 4)
                    {
                        totalHours += (decimal)amRouteTime.TotalHours;
                    }
                }

                // Add PM route time
                if (routePMOut.HasValue && routePMIn.HasValue)
                {
                    var pmRouteTime = routePMIn.Value - routePMOut.Value;

                    // Handle overnight route times
                    if (pmRouteTime.TotalHours < 0)
                    {
                        pmRouteTime = pmRouteTime.Add(TimeSpan.FromHours(24));
                    }

                    // Only add reasonable route times (up to 4 hours)
                    if (pmRouteTime.TotalHours > 0 && pmRouteTime.TotalHours <= 4)
                    {
                        totalHours += (decimal)pmRouteTime.TotalHours;
                    }
                }
            }

            // Add PTO hours to the total
            decimal ptoHours = Convert.ToDecimal(_ptoHoursComboBox.SelectedItem ?? 0);
            totalHours += ptoHours;

            // Calculate overtime (anything over 8 hours)
            decimal overtime = Math.Max(0, totalHours - 8.0m);

            _currentTimeCard.TotalTime = totalHours;
            _currentTimeCard.Overtime = overtime;
            _currentTimeCard.PTOHours = ptoHours;

            _totalTimeTextBox.Text = totalHours.ToString("F1");
            _overtimeTextBox.Text = overtime.ToString("F1");

            // Update PTO remaining display
            UpdatePTORemainingDisplay();
        }

        private bool ValidateTimeCardForm()
        {
            // Basic validation - could be enhanced
            if (_datePicker.Value > DateTime.Today)
            {
                ShowErrorMessage("Date cannot be in the future.");
                return false;
            }
            return true;
        }
        private void CancelEdit()
        {
            ClearEditFields(); // Clear fields instead of hiding panel
        }

        private void UpdatePTORemainingDisplay()
        {
            try
            {
                // Ensure label exists before updating
                if (_ptoRemainingLabel == null)
                    return;

                var currentSchoolYear = GetSchoolYear(DateTime.Today);
                var ptoBalance = _ptoBalanceRepository.GetPTOBalanceForYear(currentSchoolYear);
                var ptoRemaining = ptoBalance.PTORemaining;

                _ptoRemainingLabel.Text = $"PTO Remaining: {ptoRemaining:F1}h ({ptoRemaining / 8:F1} days)";
                _ptoRemainingLabel.ForeColor = ptoRemaining > 16 ? Color.DarkGreen :
                                               ptoRemaining > 8 ? Color.Orange : Color.DarkRed;
            }
            catch (Exception ex)
            {
                // Safely handle errors without causing dialogs during initialization
                if (_ptoRemainingLabel != null)
                {
                    _ptoRemainingLabel.Text = "PTO Remaining: Error loading";
                    _ptoRemainingLabel.ForeColor = Color.Red;
                }

                // Only log the error, don't show dialog which could cause loops
                System.Diagnostics.Debug.WriteLine($"Error updating PTO display: {ex.Message}");
            }
        }

        private void ValidateAndUpdatePTOOptions()
        {
            try
            {
                _isUpdatingPTOSelection = true;

                var currentSchoolYear = GetSchoolYear(DateTime.Today);
                var ptoBalance = _ptoBalanceRepository.GetPTOBalanceForYear(currentSchoolYear);

                // Calculate current PTO used (excluding the current editing entry if updating)
                var timeCards = _timeCardRepository.GetAllTimeCards();
                var schoolYearStart = GetSchoolYearStart(currentSchoolYear);
                var schoolYearEnd = GetSchoolYearEnd(currentSchoolYear);

                var currentPTOUsed = timeCards
                    .Where(tc => tc.Date.HasValue &&
                                tc.Date.Value >= schoolYearStart &&
                                tc.Date.Value <= schoolYearEnd &&
                                (_isEditing ? tc.TimeCardID != _currentTimeCard.TimeCardID : true))
                    .Sum(tc => tc.PTOHours ?? 0);

                var availablePTO = ptoBalance.AnnualPTOHours - currentPTOUsed;

                // Clear and rebuild PTO options based on available balance
                _ptoHoursComboBox.Items.Clear();
                _ptoHoursComboBox.Items.Add(0); // Always allow 0

                // Add 2-hour increments up to available balance (max 8 hours per day)
                for (int hours = 2; hours <= Math.Min(8, availablePTO); hours += 2)
                {
                    _ptoHoursComboBox.Items.Add(hours);
                }

                // Reset selection if previously selected value is no longer available
                var currentSelection = _currentTimeCard?.PTOHours ?? 0;
                if (_ptoHoursComboBox.Items.Contains((int)currentSelection))
                {
                    _ptoHoursComboBox.SelectedItem = (int)currentSelection;
                }
                else
                {
                    _ptoHoursComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // Log the error silently instead of showing dialog
                System.Diagnostics.Debug.WriteLine($"Error validating PTO options: {ex.Message}");
                _ptoHoursComboBox.Items.Clear();
                _ptoHoursComboBox.Items.Add(0);
                _ptoHoursComboBox.SelectedIndex = 0;
            }
            finally
            {
                _isUpdatingPTOSelection = false;
            }
        }

        private void ValidatePTOSelection()
        {
            // Prevent recursive validation calls
            if (_isUpdatingPTOSelection)
                return;

            try
            {
                _isUpdatingPTOSelection = true;

                var selectedPTO = Convert.ToDecimal(_ptoHoursComboBox.SelectedItem ?? 0);

                // If user selected 0 PTO, no validation needed
                if (selectedPTO == 0)
                {
                    CalculateTotals();
                    return;
                }

                var currentSchoolYear = GetSchoolYear(DateTime.Today);
                var ptoBalance = _ptoBalanceRepository.GetPTOBalanceForYear(currentSchoolYear);

                // Calculate current PTO used (excluding current editing entry)
                var timeCards = _timeCardRepository.GetAllTimeCards();
                var schoolYearStart = GetSchoolYearStart(currentSchoolYear);
                var schoolYearEnd = GetSchoolYearEnd(currentSchoolYear);

                var currentPTOUsed = timeCards
                    .Where(tc => tc.Date.HasValue &&
                                tc.Date.Value >= schoolYearStart &&
                                tc.Date.Value <= schoolYearEnd &&
                                (_isEditing ? tc.TimeCardID != _currentTimeCard.TimeCardID : true))
                    .Sum(tc => tc.PTOHours ?? 0);

                var availablePTO = ptoBalance.AnnualPTOHours - currentPTOUsed;

                // Validate selection only if user actually selected PTO
                if (selectedPTO > availablePTO)
                {
                    ShowErrorMessage($"Insufficient PTO balance. Available: {availablePTO}h, Requested: {selectedPTO}h");
                    _ptoHoursComboBox.SelectedIndex = 0; // Reset to 0
                    return;
                }

                // If valid, proceed with calculation
                CalculateTotals();
            }
            catch (Exception ex)
            {
                // Log error silently instead of showing dialog for background validation
                System.Diagnostics.Debug.WriteLine($"Error validating PTO selection: {ex.Message}");
                // Only reset to 0 if user had actually selected PTO
                var selectedPTO = Convert.ToDecimal(_ptoHoursComboBox.SelectedItem ?? 0);
                if (selectedPTO > 0)
                {
                    _ptoHoursComboBox.SelectedIndex = 0;
                }
            }
            finally
            {
                _isUpdatingPTOSelection = false;
            }
        }

        // Quick fix suggestion methods
        private void ShowQuickFixSuggestions()
        {
            var quickFixForm = new Form
            {
                Text = "Quick Fix Suggestions",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var suggestionsText = @"Common Quick Fixes:

• Missing Clock In: Enter your usual start time (e.g., 8:00)
• Missing Clock Out: Enter your usual end time (e.g., 5:00)
• Short Lunch: Check if you entered lunch times correctly
• Long Work Day: Verify all times are in the correct 24-hour format
• Route Times: Make sure AM/PM route times are paired correctly

Tip: Use Tab key to move between fields quickly!";

            var textBox = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(340, 200),
                Multiline = true,
                ReadOnly = true,
                Text = suggestionsText,
                ScrollBars = ScrollBars.Vertical
            };

            var okButton = new Button
            {
                Location = new Point(160, 240),
                Size = new Size(80, 30),
                Text = "OK",
                DialogResult = DialogResult.OK
            };

            quickFixForm.Controls.AddRange(new Control[] { textBox, okButton });
            quickFixForm.AcceptButton = okButton;
            quickFixForm.ShowDialog();
        }

        private void ApplyCommonTimeFixes()
        {
            var fixes = new[]
            {
                "Set standard work hours (8:00-5:00)",
                "Add standard lunch break (12:00-1:00)",
                "Clear all route times",
                "Set typical route times (AM: 6:30-8:00, PM: 3:00-4:30)"
            };

            var fixDialog = new Form
            {
                Text = "Apply Common Time Patterns",
                Size = new Size(350, 200),
                StartPosition = FormStartPosition.CenterParent
            };

            var listBox = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(290, 100)
            };
            listBox.Items.AddRange(fixes);

            var applyButton = new Button
            {
                Location = new Point(180, 140),
                Size = new Size(80, 30),
                Text = "Apply",
                Enabled = false
            };

            var cancelButton = new Button
            {
                Location = new Point(270, 140),
                Size = new Size(60, 30),
                Text = "Cancel",
                DialogResult = DialogResult.Cancel
            };

            listBox.SelectedIndexChanged += (s, e) => applyButton.Enabled = listBox.SelectedIndex >= 0;

            applyButton.Click += (s, e) =>
            {
                switch (listBox.SelectedIndex)
                {
                    case 0: // Standard work hours
                        _clockInTextBox.Text = "8:00";
                        _clockOutTextBox.Text = "17:00";
                        break;
                    case 1: // Standard lunch
                        _lunchOutTextBox.Text = "12:00";
                        _lunchInTextBox.Text = "13:00";
                        break;
                    case 2: // Clear route times
                        _routeAMOutTextBox.Text = "";
                        _routeAMInTextBox.Text = "";
                        _routePMOutTextBox.Text = "";
                        _routePMInTextBox.Text = "";
                        break;
                    case 3: // Typical route times
                        _routeAMOutTextBox.Text = "6:30";
                        _routeAMInTextBox.Text = "8:00";
                        _routePMOutTextBox.Text = "15:00";
                        _routePMInTextBox.Text = "16:30";
                        break;
                }
                CalculateTotals();
                fixDialog.DialogResult = DialogResult.OK;
            };

            fixDialog.Controls.AddRange(new Control[] { listBox, applyButton, cancelButton });
            fixDialog.ShowDialog();
        }

        private void ApplyPTO_Click(object sender, EventArgs e)
        {
            try
            {
                // Ensure we have a current time card to apply PTO to
                if (_currentTimeCard == null)
                {
                    ShowErrorMessage("Please select a time card or create a new one before applying PTO.");
                    return;
                }

                // Check available PTO balance first
                var currentSchoolYear = GetSchoolYear(DateTime.Today);
                var ptoBalance = _ptoBalanceRepository.GetPTOBalanceForYear(currentSchoolYear);
                var timeCards = _timeCardRepository.GetAllTimeCards();
                var schoolYearStart = GetSchoolYearStart(currentSchoolYear);
                var schoolYearEnd = GetSchoolYearEnd(currentSchoolYear);

                var currentPTOUsed = timeCards
                    .Where(tc => tc.Date.HasValue &&
                                tc.Date.Value >= schoolYearStart &&
                                tc.Date.Value <= schoolYearEnd &&
                                (_isEditing ? tc.TimeCardID != _currentTimeCard.TimeCardID : true))
                    .Sum(tc => tc.PTOHours ?? 0);
                var availablePTO = ptoBalance.AnnualPTOHours - currentPTOUsed;

                if (availablePTO <= 0)
                {
                    ShowWarningMessage("No PTO balance available for this year.");
                    return;
                }

                // Show PTO application dialog
                using (var ptoDialog = new Form())
                {
                    ptoDialog.Text = "Apply Personal Time Off";
                    ptoDialog.Size = new Size(400, 250);
                    ptoDialog.StartPosition = FormStartPosition.CenterParent;
                    ptoDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    ptoDialog.MaximizeBox = false;
                    ptoDialog.MinimizeBox = false;

                    // Info label
                    var infoLabel = new Label
                    {
                        Text = $"Available PTO: {availablePTO:F1} hours ({availablePTO / 8:F1} days)",
                        Location = new Point(20, 20),
                        Size = new Size(350, 20),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.DarkBlue
                    };
                    ptoDialog.Controls.Add(infoLabel);

                    // PTO type selection
                    var typeLabel = new Label
                    {
                        Text = "PTO Type:",
                        Location = new Point(20, 60),
                        AutoSize = true
                    };
                    ptoDialog.Controls.Add(typeLabel);

                    var typeComboBox = new ComboBox
                    {
                        Location = new Point(100, 57),
                        Size = new Size(150, 23),
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                    typeComboBox.Items.AddRange(new string[] { "Full Day (8 hours)", "Half Day (4 hours)", "2 Hours", "6 Hours", "Custom" });
                    typeComboBox.SelectedIndex = 0;
                    ptoDialog.Controls.Add(typeComboBox);

                    // Hours input
                    var hoursLabel = new Label
                    {
                        Text = "Hours:",
                        Location = new Point(20, 100),
                        AutoSize = true
                    };
                    ptoDialog.Controls.Add(hoursLabel);

                    var hoursTextBox = new TextBox
                    {
                        Location = new Point(100, 97),
                        Size = new Size(80, 23),
                        Text = "8"
                    };
                    ptoDialog.Controls.Add(hoursTextBox);

                    // Update hours based on type selection
                    typeComboBox.SelectedIndexChanged += (s, ev) =>
                    {
                        switch (typeComboBox.SelectedIndex)
                        {
                            case 0: hoursTextBox.Text = "8"; break; // Full Day
                            case 1: hoursTextBox.Text = "4"; break; // Half Day
                            case 2: hoursTextBox.Text = "2"; break; // 2 Hours
                            case 3: hoursTextBox.Text = "6"; break; // 6 Hours
                            case 4: hoursTextBox.Text = ""; hoursTextBox.Focus(); break; // Custom
                        }
                    };

                    // Note about time entries
                    var noteLabel = new Label
                    {
                        Text = "Note: PTO will be applied and clock times will be adjusted automatically.",
                        Location = new Point(20, 135),
                        Size = new Size(350, 30),
                        Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                        ForeColor = Color.DarkGray
                    };
                    ptoDialog.Controls.Add(noteLabel);

                    // Buttons
                    var applyButton = new Button
                    {
                        Text = "Apply PTO",
                        Location = new Point(200, 180),
                        Size = new Size(80, 30),
                        DialogResult = DialogResult.OK,
                        BackColor = Color.LightGreen
                    };
                    ptoDialog.Controls.Add(applyButton);

                    var cancelButton = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(290, 180),
                        Size = new Size(80, 30),
                        DialogResult = DialogResult.Cancel
                    };
                    ptoDialog.Controls.Add(cancelButton);

                    ptoDialog.AcceptButton = applyButton;
                    ptoDialog.CancelButton = cancelButton;

                    // Show dialog and process result
                    if (ptoDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (decimal.TryParse(hoursTextBox.Text, out decimal ptoHours) && ptoHours > 0)
                        {
                            if (ptoHours > availablePTO)
                            {
                                ShowErrorMessage($"Insufficient PTO balance. Available: {availablePTO:F1}h, Requested: {ptoHours}h");
                                return;
                            }

                            // Apply the PTO
                            ApplyPTOToTimeCard(ptoHours);

                            // CRITICAL FIX: Save the time card with PTO applied
                            PopulateTimeCardFromFields();

                            if (_isEditing)
                            {
                                _timeCardRepository.UpdateTimeCard(_currentTimeCard);
                            }
                            else
                            {
                                _timeCardRepository.AddTimeCard(_currentTimeCard);
                            }

                            // Refresh the grid to show the updated PTO
                            LoadTimeCards();

                            ShowSuccessMessage($"{ptoHours} hours of PTO applied and saved successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Please enter a valid number of hours.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error applying PTO: {ex.Message}");
            }
        }

        private void ApplyPTOToTimeCard(decimal ptoHours)
        {
            try
            {
                _isUpdatingPTOSelection = true;

                // Set the PTO hours in the ComboBox
                var ptoHoursInt = (int)ptoHours;
                if (_ptoHoursComboBox.Items.Contains(ptoHoursInt))
                {
                    _ptoHoursComboBox.SelectedItem = ptoHoursInt;
                }
                else
                {
                    // Add custom PTO hours if not in standard list
                    _ptoHoursComboBox.Items.Add(ptoHoursInt);
                    _ptoHoursComboBox.SelectedItem = ptoHoursInt;
                }

                // Auto-adjust time entries for a PTO day
                if (ptoHours >= 8) // Full day PTO
                {
                    // Clear all time entries for full day PTO
                    _clockInTextBox.Text = "";
                    _lunchOutTextBox.Text = "";
                    _lunchInTextBox.Text = "";
                    _clockOutTextBox.Text = "";
                    _routeAMOutTextBox.Text = "";
                    _routeAMInTextBox.Text = "";
                    _routePMOutTextBox.Text = "";
                    _routePMInTextBox.Text = "";
                }
                else if (ptoHours >= 4) // Half day PTO
                {
                    // Set reduced hours for half day
                    _clockInTextBox.Text = "8:00";
                    _clockOutTextBox.Text = "12:00";
                    _lunchOutTextBox.Text = "";
                    _lunchInTextBox.Text = "";
                    _routeAMOutTextBox.Text = "";
                    _routeAMInTextBox.Text = "";
                    _routePMOutTextBox.Text = "";
                    _routePMInTextBox.Text = "";
                }
                // For partial PTO (2-6 hours), leave time entries for user to adjust

                // Recalculate totals
                CalculateTotals();
            }
            finally
            {
                _isUpdatingPTOSelection = false;
            }
        }

        private void CheckForWeeklyPTO()
        {
            try
            {
                // Check if it's Friday or end of work week
                var today = DateTime.Today;
                if (today.DayOfWeek == DayOfWeek.Friday)
                {
                    // Get this week's time cards
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Monday
                    var endOfWeek = startOfWeek.AddDays(4); // Friday

                    var thisWeekTimeCards = _timeCardRepository.GetAllTimeCards()
                        .Where(tc => tc.Date?.Date >= startOfWeek && tc.Date?.Date <= endOfWeek)
                        .ToList();

                    // Check if any PTO was recorded this week
                    var weeklyPTO = thisWeekTimeCards.Sum(tc => tc.PTOHours ?? 0);

                    if (weeklyPTO == 0)
                    {
                        // Ask if user took any PTO this week
                        var result = MessageBox.Show(
                            "Did you take any Personal Time Off (PTO) this week that wasn't recorded?",
                            "Weekly PTO Check",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            ShowSuccessMessage("Please use the 'Apply PTO' button to add PTO to the appropriate days.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent error - don't show dialog for this optional feature
                System.Diagnostics.Debug.WriteLine($"Error checking weekly PTO: {ex.Message}");
            }
        }

        private void ApplyAutoFixes(List<TimeEntryWarning> warnings)
        {
            foreach (var warning in warnings)
            {
                switch (warning.Type)
                {
                    case WarningType.VeryEarlyClockIn:
                        // Fix very early clock-in by setting to 6:00 AM
                        _clockInTextBox.Text = "6:00";
                        break;

                    case WarningType.VeryLateClockOut:
                        // Fix very late clock-out by setting to 6:00 PM
                        _clockOutTextBox.Text = "18:00";
                        break;

                    case WarningType.ShortLunchBreak:
                        // Fix short lunch by extending lunch break to 30 minutes
                        if (!string.IsNullOrEmpty(_lunchOutTextBox.Text) && string.IsNullOrEmpty(_lunchInTextBox.Text))
                        {
                            var lunchOut = ParseTime(_lunchOutTextBox.Text);
                            if (lunchOut.HasValue)
                            {
                                var lunchIn = lunchOut.Value.Add(TimeSpan.FromMinutes(30));
                                _lunchInTextBox.Text = lunchIn.ToString(@"h\:mm");
                            }
                        }
                        break;

                    case WarningType.MissingClockIn:
                        // Set default clock-in time if missing
                        if (string.IsNullOrEmpty(_clockInTextBox.Text))
                            _clockInTextBox.Text = "8:00";
                        break;

                    case WarningType.IncompleteRouteEntry:
                        // Clear incomplete route entries to avoid confusion
                        if (_routeDayCheckBox.Checked)
                        {
                            if (string.IsNullOrEmpty(_routeAMInTextBox.Text) && !string.IsNullOrEmpty(_routeAMOutTextBox.Text))
                                _routeAMOutTextBox.Text = "";
                            if (string.IsNullOrEmpty(_routePMInTextBox.Text) && !string.IsNullOrEmpty(_routePMOutTextBox.Text))
                                _routePMOutTextBox.Text = "";
                        }
                        break;
                }
            }
        }

        private void RecalculateAllTimeCards(List<TimeCard> timeCards)
        {
            foreach (var timeCard in timeCards)
            {
                if (timeCard.Date.HasValue)
                {
                    // Recalculate total time for this time card
                    decimal totalHours = 0;

                    // Calculate regular work hours
                    if (timeCard.AMClockIn.HasValue && timeCard.PMClockOut.HasValue)
                    {
                        var workTime = timeCard.PMClockOut.Value - timeCard.AMClockIn.Value;

                        // Handle overnight shifts
                        if (workTime.TotalHours < 0)
                        {
                            workTime = workTime.Add(TimeSpan.FromHours(24));
                        }

                        // Handle lunch break
                        if (timeCard.LunchClockOut.HasValue && timeCard.LunchClockIn.HasValue)
                        {
                            var lunchBreak = timeCard.LunchClockIn.Value - timeCard.LunchClockOut.Value;

                            // Handle overnight lunch break
                            if (lunchBreak.TotalHours < 0)
                            {
                                lunchBreak = lunchBreak.Add(TimeSpan.FromHours(24));
                            }

                            // Only subtract lunch if it's reasonable
                            if (lunchBreak.TotalMinutes >= 15 && lunchBreak.TotalHours <= 2)
                            {
                                workTime = workTime - lunchBreak;
                            }
                        }

                        totalHours = (decimal)workTime.TotalHours;
                    }

                    // Add route time if it's a route day
                    if (timeCard.DayType == "Route Day")
                    {
                        // Add AM route time
                        if (timeCard.RouteAMClockOut.HasValue && timeCard.RouteAMClockIn.HasValue)
                        {
                            var amRouteTime = timeCard.RouteAMClockIn.Value - timeCard.RouteAMClockOut.Value;
                            if (amRouteTime.TotalHours < 0)
                            {
                                amRouteTime = amRouteTime.Add(TimeSpan.FromHours(24));
                            }
                            if (amRouteTime.TotalHours > 0 && amRouteTime.TotalHours <= 4)
                            {
                                totalHours += (decimal)amRouteTime.TotalHours;
                            }
                        }

                        // Add PM route time
                        if (timeCard.RoutePMClockOut.HasValue && timeCard.RoutePMClockIn.HasValue)
                        {
                            var pmRouteTime = timeCard.RoutePMClockIn.Value - timeCard.RoutePMClockOut.Value;
                            if (pmRouteTime.TotalHours < 0)
                            {
                                pmRouteTime = pmRouteTime.Add(TimeSpan.FromHours(24));
                            }
                            if (pmRouteTime.TotalHours > 0 && pmRouteTime.TotalHours <= 4)
                            {
                                totalHours += (decimal)pmRouteTime.TotalHours;
                            }
                        }
                    }

                    // Add PTO hours
                    if (timeCard.PTOHours.HasValue)
                    {
                        totalHours += timeCard.PTOHours.Value;
                    }

                    // Ensure no negative hours
                    totalHours = Math.Max(0, totalHours);

                    // Calculate overtime
                    decimal overtime = Math.Max(0, totalHours - 8.0m);

                    // Update the time card
                    timeCard.TotalTime = totalHours;
                    timeCard.Overtime = overtime;
                }
            }
        }

        private void UpdateRunningTotalsInDatabase(List<TimeCard> timeCards)
        {
            try
            {
                foreach (var timeCard in timeCards)
                {
                    // Only update if the time card has running totals calculated
                    if (timeCard.WeeklyTotal.HasValue || timeCard.MonthlyTotal.HasValue)
                    {
                        _timeCardRepository.UpdateTimeCard(timeCard);
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't show error dialog for this background operation
                System.Diagnostics.Debug.WriteLine($"Error updating running totals: {ex.Message}");
            }
        }

        private string DetermineDayType()
        {
            bool isRouteDay = _routeDayCheckBox.Checked;
            decimal ptoHours = Convert.ToDecimal(_ptoHoursComboBox.SelectedItem ?? 0);

            // Calculate work hours (excluding PTO)
            decimal workHours = 0;
            var clockIn = ParseTime(_clockInTextBox.Text.Trim());
            var clockOut = ParseTime(_clockOutTextBox.Text.Trim());

            if (clockIn.HasValue && clockOut.HasValue)
            {
                var workTime = clockOut.Value - clockIn.Value;
                if (workTime.TotalHours < 0)
                {
                    workTime = workTime.Add(TimeSpan.FromHours(24));
                }

                // Subtract lunch if applicable
                var lunchOut = ParseTime(_lunchOutTextBox.Text.Trim());
                var lunchIn = ParseTime(_lunchInTextBox.Text.Trim());
                if (lunchOut.HasValue && lunchIn.HasValue)
                {
                    var lunchBreak = lunchIn.Value - lunchOut.Value;
                    if (lunchBreak.TotalHours < 0)
                    {
                        lunchBreak = lunchBreak.Add(TimeSpan.FromHours(24));
                    }
                    if (lunchBreak.TotalMinutes >= 15 && lunchBreak.TotalHours <= 2)
                    {
                        workTime = workTime - lunchBreak;
                    }
                }

                workHours = Math.Max(0, (decimal)workTime.TotalHours);
            }

            // Add route hours if route day
            if (isRouteDay)
            {
                // Add AM route time
                var routeAMOut = ParseTime(_routeAMOutTextBox.Text.Trim());
                var routeAMIn = ParseTime(_routeAMInTextBox.Text.Trim());
                if (routeAMOut.HasValue && routeAMIn.HasValue)
                {
                    var amRouteTime = routeAMIn.Value - routeAMOut.Value;
                    if (amRouteTime.TotalHours < 0)
                        amRouteTime = amRouteTime.Add(TimeSpan.FromHours(24));
                    if (amRouteTime.TotalHours > 0 && amRouteTime.TotalHours <= 4)
                        workHours += (decimal)amRouteTime.TotalHours;
                }

                // Add PM route time
                var routePMOut = ParseTime(_routePMOutTextBox.Text.Trim());
                var routePMIn = ParseTime(_routePMInTextBox.Text.Trim());
                if (routePMOut.HasValue && routePMIn.HasValue)
                {
                    var pmRouteTime = routePMIn.Value - routePMOut.Value;
                    if (pmRouteTime.TotalHours < 0)
                        pmRouteTime = pmRouteTime.Add(TimeSpan.FromHours(24));
                    if (pmRouteTime.TotalHours > 0 && pmRouteTime.TotalHours <= 4)
                        workHours += (decimal)pmRouteTime.TotalHours;
                }
            }

            // Determine day type based on work hours and PTO
            if (ptoHours > 0 && workHours > 0)
            {
                return isRouteDay ? "Route Day + PTO" : "Regular Day + PTO";
            }
            else if (ptoHours > 0 && workHours == 0)
            {
                return "PTO";
            }
            else
            {
                return isRouteDay ? "Route Day" : "Regular Day";
            }
        }

        /// <summary>
        /// Gets the school year for a given date. School year runs from July 1 to June 30.
        /// </summary>
        /// <param name="date">The date to get the school year for</param>
        /// <returns>The school year (e.g., 2025 for school year 2024-2025)</returns>
        private int GetSchoolYear(DateTime date)
        {
            // If the date is July 1 or later, the school year is the current year + 1
            // If the date is before July 1, the school year is the current year
            return date.Month >= 7 ? date.Year + 1 : date.Year;
        }

        /// <summary>
        /// Gets the start date of a school year (July 1)
        /// </summary>
        /// <param name="schoolYear">The school year</param>
        /// <returns>July 1st of the previous calendar year</returns>
        private DateTime GetSchoolYearStart(int schoolYear)
        {
            return new DateTime(schoolYear - 1, 7, 1);
        }

        /// <summary>
        /// Gets the end date of a school year (June 30)
        /// </summary>
        /// <param name="schoolYear">The school year</param>
        /// <returns>June 30th of the school year</returns>
        private DateTime GetSchoolYearEnd(int schoolYear)
        {
            return new DateTime(schoolYear, 6, 30);
        }

        /// <summary>
        /// Validates that a current time card exists before allowing user input
        /// </summary>
        /// <returns>True if valid, false if user needs to add a record first</returns>
        private bool ValidateCurrentTimeCardExists()
        {
            if (_currentTimeCard == null)
            {
                ShowErrorMessage("Please Add A Record First");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Event handler for text box enter events to validate current record exists
        /// </summary>
        private void OnTextBoxEnter(object sender, EventArgs e)
        {
            if (!ValidateCurrentTimeCardExists())
            {
                // Move focus away from the textbox to prevent input
                if (sender is TextBox textBox)
                {
                    textBox.Parent?.SelectNextControl(textBox, true, true, true, true);
                }
            }
        }

        /// <summary>
        /// Event handler for checkbox click events to validate current record exists
        /// </summary>
        private void OnCheckBoxClick(object sender, EventArgs e)
        {
            if (!ValidateCurrentTimeCardExists())
            {
                // Revert the checkbox state
                if (sender is CheckBox checkBox)
                {
                    checkBox.Checked = !checkBox.Checked;
                }
            }
        }

        /// <summary>
        /// Event handler for combo box dropdown events to validate current record exists
        /// </summary>
        private void OnComboBoxDropDown(object sender, EventArgs e)
        {
            if (!ValidateCurrentTimeCardExists())
            {
                // Close the dropdown
                if (sender is ComboBox comboBox)
                {
                    comboBox.DroppedDown = false;
                }
            }
        }
    }
}
