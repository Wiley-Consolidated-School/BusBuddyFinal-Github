using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class TimeCardManagementForm : BaseDataForm
    {
        private readonly ITimeCardRepository _timeCardRepository;
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
        private CheckBox _routeDayCheckBox;
        private Button _saveButton;
        private Button _cancelButton;
        private Panel _summaryPanel;
        private Label _lblWeekTotal;
        private Label _lblMonthTotal;

        public TimeCardManagementForm()
        {
            _timeCardRepository = new TimeCardRepository();
            InitializeComponent();
            LoadTimeCards();
        }

        private void InitializeComponent()
        {
            this.Text = "Time Card Management";
            this.Size = new System.Drawing.Size(1200, 900);
            this.BackColor = Color.WhiteSmoke;
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewTimeCard());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedTimeCard());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedTimeCard());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewTimeCardDetails());
            // Summary panel
            _summaryPanel = new Panel();
            _summaryPanel.Location = new Point(500, 10);
            _summaryPanel.Size = new Size(650, 40);
            _summaryPanel.BackColor = Color.FromArgb(240, 248, 255);
            _lblWeekTotal = new Label { Text = "Week Total: 0h", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Location = new Point(10, 10), AutoSize = true };
            _lblMonthTotal = new Label { Text = "Month Total: 0h", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.DarkSlateBlue, Location = new Point(200, 10), AutoSize = true };
            _summaryPanel.Controls.Add(_lblWeekTotal);
            _summaryPanel.Controls.Add(_lblMonthTotal);
            this.Controls.Add(_summaryPanel);
            // Main grid
            _timeCardGrid = new DataGridView();
            _timeCardGrid.Location = new System.Drawing.Point(20, 60);
            _timeCardGrid.Size = new System.Drawing.Size(1150, 600);
            _timeCardGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
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
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 670);
            _editPanel.Size = new System.Drawing.Size(1150, 180);
            _editPanel.Visible = false;
            _editPanel.BackColor = Color.AliceBlue;
            this.Controls.Add(_editPanel);
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(70, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_datePicker);
            _routeDayCheckBox = new CheckBox { Text = "Route Day", Location = new Point(250, 12), AutoSize = true };
            _editPanel.Controls.Add(_routeDayCheckBox);
            // Normal workday group
            GroupBox normalGroup = new GroupBox { Text = "Normal Workday", Location = new Point(10, 45), Size = new Size(540, 60), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            normalGroup.Controls.Add(new Label { Text = "Clock In:", Location = new Point(10, 25), AutoSize = true });
            _clockInTextBox = new TextBox { Location = new Point(70, 22), Size = new Size(60, 23) };
            normalGroup.Controls.Add(_clockInTextBox);
            normalGroup.Controls.Add(new Label { Text = "Lunch Out:", Location = new Point(140, 25), AutoSize = true });
            _lunchOutTextBox = new TextBox { Location = new Point(210, 22), Size = new Size(60, 23) };
            normalGroup.Controls.Add(_lunchOutTextBox);
            normalGroup.Controls.Add(new Label { Text = "Lunch In:", Location = new Point(280, 25), AutoSize = true });
            _lunchInTextBox = new TextBox { Location = new Point(340, 22), Size = new Size(60, 23) };
            normalGroup.Controls.Add(_lunchInTextBox);
            normalGroup.Controls.Add(new Label { Text = "Clock Out:", Location = new Point(410, 25), AutoSize = true });
            _clockOutTextBox = new TextBox { Location = new Point(480, 22), Size = new Size(60, 23) };
            normalGroup.Controls.Add(_clockOutTextBox);
            _editPanel.Controls.Add(normalGroup);
            // Route day group
            GroupBox routeGroup = new GroupBox { Text = "Route Events (if Route Day)", Location = new Point(570, 45), Size = new Size(560, 60), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            routeGroup.Controls.Add(new Label { Text = "AM Out:", Location = new Point(10, 25), AutoSize = true });
            _routeAMOutTextBox = new TextBox { Location = new Point(70, 22), Size = new Size(60, 23) };
            routeGroup.Controls.Add(_routeAMOutTextBox);
            routeGroup.Controls.Add(new Label { Text = "AM In:", Location = new Point(140, 25), AutoSize = true });
            _routeAMInTextBox = new TextBox { Location = new Point(200, 22), Size = new Size(60, 23) };
            routeGroup.Controls.Add(_routeAMInTextBox);
            routeGroup.Controls.Add(new Label { Text = "PM Out:", Location = new Point(270, 25), AutoSize = true });
            _routePMOutTextBox = new TextBox { Location = new Point(340, 22), Size = new Size(60, 23) };
            routeGroup.Controls.Add(_routePMOutTextBox);
            routeGroup.Controls.Add(new Label { Text = "PM In:", Location = new Point(410, 25), AutoSize = true });
            _routePMInTextBox = new TextBox { Location = new Point(470, 22), Size = new Size(60, 23) };
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
            _saveButton = CreateButton("Save", 800, 120, (s, e) => SaveTimeCard());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 120, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        // --- CRUD and event handler stubs ---
        private void LoadTimeCards()
        {
            // TODO: Load time cards from repository and update grid, summary, etc.
        }
        private void AddNewTimeCard()
        {
            // TODO: Clear fields, set _isEditing = false, show _editPanel
        }
        private void EditSelectedTimeCard()
        {
            // TODO: Populate fields from selected row, set _isEditing = true, show _editPanel
        }
        private void DeleteSelectedTimeCard()
        {
            // TODO: Delete selected time card, reload grid
        }
        private void ViewTimeCardDetails()
        {
            // TODO: Show details for selected time card
        }
        private void TimeCardGrid_SelectionChanged(object sender, EventArgs e)
        {
            // TODO: Enable/disable buttons based on selection
        }
        private void SaveTimeCard()
        {
            // TODO: Validate and save time card, update grid and summary
        }
        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }
    }
}
