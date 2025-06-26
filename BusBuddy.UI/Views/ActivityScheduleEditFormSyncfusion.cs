using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Schedule Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for creating and editing activity schedules with vehicle and driver assignments
    /// </summary>
    public partial class ActivityScheduleEditFormSyncfusion : SyncfusionBaseForm
    {
        private Control? _destinationTextBox;
        private Control? _notesTextBox;
        private Control? _ridersTextBox;
        private ComboBox? _tripTypeComboBox;
        private ComboBox? _vehicleComboBox;
        private ComboBox? _driverComboBox;
        private DateTimePicker? _datePicker;
        private DateTimePicker? _leaveTimePicker;
        private DateTimePicker? _eventTimePicker;
        private DateTimePicker? _returnTimePicker;
        private Control? _saveButton;
        private Control? _cancelButton;

        private readonly List<Vehicle> _vehicles;
        private readonly List<Driver> _drivers;

        public ActivitySchedule? ActivitySchedule { get; private set; }

        public ActivityScheduleEditFormSyncfusion(List<Vehicle> vehicles, List<Driver> drivers) : this(vehicles, drivers, null)
        {
        }

        public ActivityScheduleEditFormSyncfusion(List<Vehicle> vehicles, List<Driver> drivers, ActivitySchedule? activitySchedule)
        {
            _vehicles = vehicles ?? throw new ArgumentNullException(nameof(vehicles));
            _drivers = drivers ?? throw new ArgumentNullException(nameof(drivers));
            ActivitySchedule = activitySchedule;
            InitializeComponent();
            PopulateDropdowns();

            if (activitySchedule != null)
            {
                PopulateFields(activitySchedule);
            }
        }

        private void InitializeComponent()
        {
            this.Text = ActivitySchedule == null ? "📅 Add Activity Schedule" : "📅 Edit Activity Schedule";
            this.ClientSize = GetDpiAwareSize(new Size(600, 700));
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            BusBuddyThemeManager.ApplyTheme(this, BusBuddyThemeManager.SupportedThemes.Office2016White);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Create text boxes
            _destinationTextBox = ControlFactory.CreateTextBox("Enter destination");
            _notesTextBox = ControlFactory.CreateTextBox("Enter notes", true);
            _ridersTextBox = ControlFactory.CreateTextBox("Enter rider count");

            // Make notes textbox multiline
            if (_notesTextBox is TextBox notesTextBox)
            {
                notesTextBox.Multiline = true;
                notesTextBox.Height = 80;
                notesTextBox.ScrollBars = ScrollBars.Vertical;
            }

            // Create combo boxes
            _tripTypeComboBox = CreateComboBox("Select trip type", 150, 100, 200);
            _tripTypeComboBox.Items.AddRange(new[] {
                "Field Trip", "Athletic Event", "Academic Competition", "Performance", "Other"
            });

            _vehicleComboBox = CreateComboBox("Select vehicle", 150, 150, 200);
            _driverComboBox = CreateComboBox("Select driver", 150, 200, 200);

            // Create date/time pickers
            _datePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(150), GetDpiAwareY(250)),
                Size = GetDpiAwareSize(new Size(200, 35)),
                Format = DateTimePickerFormat.Short
            };
            BusBuddyThemeManager.ApplyTheme(_datePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_datePicker);

            _leaveTimePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(150), GetDpiAwareY(300)),
                Size = GetDpiAwareSize(new Size(150, 35)),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            BusBuddyThemeManager.ApplyTheme(_leaveTimePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_leaveTimePicker);

            _eventTimePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(150), GetDpiAwareY(350)),
                Size = GetDpiAwareSize(new Size(150, 35)),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            BusBuddyThemeManager.ApplyTheme(_eventTimePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_eventTimePicker);

            _returnTimePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(150), GetDpiAwareY(400)),
                Size = GetDpiAwareSize(new Size(150, 35)),
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };
            BusBuddyThemeManager.ApplyTheme(_returnTimePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_returnTimePicker);

            // Create buttons
            _saveButton = new Button { Text = "💾 Save" } /* CreateStyledButton method removed */;
            _cancelButton = new Button { Text = "❌ Cancel" } /* CreateStyledButton method removed */;

            // Configure buttons
            _saveButton.Size = GetDpiAwareSize(new Size(120, 35));
            _saveButton.Location = new Point(GetDpiAwareX(300), GetDpiAwareY(10));
            _saveButton.Click += SaveButton_Click;

            _cancelButton.Size = GetDpiAwareSize(new Size(120, 35));
            _cancelButton.Location = new Point(GetDpiAwareX(430), GetDpiAwareY(10));
            _cancelButton.Click += CancelButton_Click;

            // Style cancel button differently
            if (_cancelButton is Button cancelBtn)
            {
                cancelBtn.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
                cancelBtn.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            }

            _buttonPanel.Controls.Add(_saveButton);
            _buttonPanel.Controls.Add(_cancelButton);
        }

        private void LayoutControls()
        {
            // Create labels
            ControlFactory.CreateLabel("🎯 Destination:");
            ControlFactory.CreateLabel("🚌 Trip Type:");
            ControlFactory.CreateLabel("🚐 Vehicle:");
            ControlFactory.CreateLabel("👤 Driver:");
            ControlFactory.CreateLabel("📅 Date:");
            ControlFactory.CreateLabel("🕐 Leave Time:");
            ControlFactory.CreateLabel("🕑 Event Time:");
            ControlFactory.CreateLabel("🕒 Return Time:");
            ControlFactory.CreateLabel("👥 # Riders:");
            ControlFactory.CreateLabel("📝 Notes:");

            // Set placeholder text
            SetPlaceholderText(_destinationTextBox, "Enter destination");
            SetPlaceholderText(_ridersTextBox, "Number of riders");
            SetPlaceholderText(_notesTextBox, "Additional notes (optional)");
        }

        private void SetupEventHandlers()
        {
            // Vehicle selection change event
            if (_vehicleComboBox != null)
            {
                _vehicleComboBox.SelectedIndexChanged += VehicleComboBox_SelectedIndexChanged;
            }
        }
        #region Control Creation Helpers

        private ComboBox CreateComboBox(string placeholder, int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(GetDpiAwareX(x), GetDpiAwareY(y)),
                Size = GetDpiAwareSize(new Size(width, 35)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Apply Material theming
            comboBox.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            _mainPanel.Controls.Add(comboBox);
            return comboBox;
        }        private void SetPlaceholderText(Control? textBox, string placeholder)
        {
            if (textBox is TextBox tb)
            {
                tb.Text = placeholder;
                tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Text = "";
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                    }
                };
            }
        }
        #endregion

        #region Data Population

        private void PopulateDropdowns()
        {
            try
            {
                // Populate vehicle dropdown
                if (_vehicleComboBox != null)
                {
                    _vehicleComboBox.DisplayMember = "DisplayText";
                    _vehicleComboBox.ValueMember = "VehicleID";

                    var vehicleItems = _vehicles.Select(v => new
                    {
                        VehicleID = v.VehicleID,
                        DisplayText = $"{v.BusNumber ?? v.VehicleNumber} - {v.Make} {v.Model}"
                    }).ToList();

                    try
                    {
                        _vehicleComboBox.DataSource = vehicleItems;
                        _vehicleComboBox.SelectedIndex = -1;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading vehicle data: {ex.Message}", "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                // Populate driver dropdown
                if (_driverComboBox != null)
                {
                    _driverComboBox.DisplayMember = "DisplayText";
                    _driverComboBox.ValueMember = "DriverID";

                    var driverItems = _drivers.Select(d => new
                    {
                        DriverID = d.DriverID,
                        DisplayText = $"{d.FirstName} {d.LastName}"
                    }).ToList();

                    _driverComboBox.DataSource = driverItems;
                    _driverComboBox.SelectedIndex = -1;
                }

                Console.WriteLine($"📋 SYNCFUSION FORM: Populated dropdowns - {_vehicles.Count} vehicles, {_drivers.Count} drivers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error populating dropdowns: {ex.Message}");
                MessageBox.Show($"Error loading data: {ex.Message}", "Data Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PopulateFields(ActivitySchedule activitySchedule)
        {
            try
            {
                if (_destinationTextBox is TextBox destinationTb)
                {
                    destinationTb.Text = activitySchedule.ScheduledDestination ?? string.Empty;
                    destinationTb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }

                if (_notesTextBox is TextBox notesTb)
                {
                    notesTb.Text = activitySchedule.Notes ?? string.Empty;
                    notesTb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }

                if (_ridersTextBox is TextBox ridersTb)
                {
                    ridersTb.Text = activitySchedule.ScheduledRiders?.ToString() ?? string.Empty;
                    ridersTb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }

                if (_tripTypeComboBox != null && !string.IsNullOrEmpty(activitySchedule.TripType))
                    _tripTypeComboBox.SelectedItem = activitySchedule.TripType;

                if (_vehicleComboBox != null && activitySchedule.ScheduledVehicleID > 0)
                    _vehicleComboBox.SelectedValue = activitySchedule.ScheduledVehicleID;

                if (_driverComboBox != null && activitySchedule.ScheduledDriverID > 0)
                    _driverComboBox.SelectedValue = activitySchedule.ScheduledDriverID;

                if (_datePicker != null && activitySchedule.DateAsDateTime.HasValue)
                    _datePicker.Value = activitySchedule.DateAsDateTime.Value;                if (_leaveTimePicker != null && activitySchedule.ScheduledLeaveTime.HasValue)
                    _leaveTimePicker.Value = DateTime.Today.Add(activitySchedule.ScheduledLeaveTime.Value);                if (_eventTimePicker != null && activitySchedule.ScheduledEventTime.HasValue)
                    _eventTimePicker.Value = DateTime.Today.Add(activitySchedule.ScheduledEventTime.Value);                if (_returnTimePicker != null && activitySchedule.ScheduledReturnTime.HasValue)
                    _returnTimePicker.Value = DateTime.Today.Add(activitySchedule.ScheduledReturnTime.Value);

                Console.WriteLine($"📋 SYNCFUSION FORM: Populated fields for schedule: {activitySchedule.ScheduledDestination}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error populating fields: {ex.Message}");
                MessageBox.Show($"Error loading schedule data: {ex.Message}", "Data Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event Handlers

        private void VehicleComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Could implement logic to filter drivers based on vehicle selection
            // or show additional vehicle information
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // Create or update activity schedule object
                if (ActivitySchedule == null)
                    ActivitySchedule = new ActivitySchedule();

                // Get values from controls
                if (_destinationTextBox is TextBox destinationTb)
                    ActivitySchedule.ScheduledDestination = destinationTb.Text.Trim();

                if (_notesTextBox is TextBox notesTb)
                    ActivitySchedule.Notes = notesTb.Text.Trim();

                if (_ridersTextBox is TextBox ridersTb && int.TryParse(ridersTb.Text, out int riders))
                    ActivitySchedule.ScheduledRiders = riders;

                if (_tripTypeComboBox?.SelectedItem != null)
                    ActivitySchedule.TripType = _tripTypeComboBox.SelectedItem.ToString();

                if (_vehicleComboBox?.SelectedValue != null)
                    ActivitySchedule.ScheduledVehicleID = (int)_vehicleComboBox.SelectedValue;

                if (_driverComboBox?.SelectedValue != null)
                    ActivitySchedule.ScheduledDriverID = (int)_driverComboBox.SelectedValue;

                if (_datePicker != null)
                    ActivitySchedule.DateAsDateTime = _datePicker.Value;

                if (_leaveTimePicker != null)
                    ActivitySchedule.ScheduledLeaveTime = _leaveTimePicker.Value.TimeOfDay;

                if (_eventTimePicker != null)
                    ActivitySchedule.ScheduledEventTime = _eventTimePicker.Value.TimeOfDay;

                if (_returnTimePicker != null)
                    ActivitySchedule.ScheduledReturnTime = _returnTimePicker.Value.TimeOfDay;

                DialogResult = DialogResult.OK;
                Close();

                Console.WriteLine($"💾 SYNCFUSION FORM: Saved activity schedule: {ActivitySchedule.ScheduledDestination}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error saving: {ex.Message}");
                MessageBox.Show($"Error saving schedule: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion

        #region Validation

        private bool ValidateInput()
        {
            ClearAllValidationErrors();

            // Validate destination
            if (_destinationTextBox is TextBox destinationTb &&
                (string.IsNullOrWhiteSpace(destinationTb.Text) ||
                 destinationTb.Text == "Enter destination"))
            {
                SetValidationError(_destinationTextBox, "Destination is required.");
                ShowErrorMessage("Destination is required.");
                return false;
            }

            // Validate trip type
            if (_tripTypeComboBox?.SelectedIndex < 0)
            {
                SetValidationError(_tripTypeComboBox, "Please select a trip type.");
                ShowErrorMessage("Please select a trip type.");
                return false;
            }

            // Validate vehicle
            if (_vehicleComboBox?.SelectedIndex < 0)
            {
                SetValidationError(_vehicleComboBox, "Please select a vehicle.");
                ShowErrorMessage("Please select a vehicle.");
                return false;
            }

            // Validate driver
            if (_driverComboBox?.SelectedIndex < 0)
            {
                SetValidationError(_driverComboBox, "Please select a driver.");
                ShowErrorMessage("Please select a driver.");
                return false;
            }

            // Validate riders count
            if (_ridersTextBox is TextBox ridersTb)
            {
                if (!string.IsNullOrWhiteSpace(ridersTb.Text) &&
                    ridersTb.Text != "Number of riders" &&
                    (!int.TryParse(ridersTb.Text, out int riders) || riders < 0))
                {
                    SetValidationError(_ridersTextBox, "Please enter a valid number of riders.");
                    ShowErrorMessage("Please enter a valid number of riders.");
                    return false;
                }
            }

            return true;
        }        // Helper methods for validation and form management
        private new void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        private new void SetValidationError(Control? control, string message)
        {
            if (control != null)
                _errorProvider.SetError(control, message);
        }private new void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Console.WriteLine($"🔄 SYNCFUSION FORM: {this.Text} closing with result: {this.DialogResult}");
            base.OnFormClosing(e);
        }
    }
}
