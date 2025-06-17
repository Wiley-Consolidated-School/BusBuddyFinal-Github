using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Views
{
    public partial class ActivityScheduleEditForm : StandardDataForm
    {
        private MaterialTextBox2 _destinationTextBox;
        private MaterialTextBox2 _notesTextBox;
        private MaterialTextBox2 _ridersTextBox;
        private MaterialComboBox _tripTypeComboBox;
        private MaterialComboBox _vehicleComboBox;
        private MaterialComboBox _driverComboBox;
        private DateTimePicker? _datePicker;
        private DateTimePicker? _leaveTimePicker;
        private DateTimePicker? _eventTimePicker;
        private DateTimePicker? _returnTimePicker;
        private MaterialButton _saveButton;
        private MaterialButton _cancelButton;

        private readonly List<Vehicle> _vehicles;
        private readonly List<Driver> _drivers;

        public ActivitySchedule? ActivitySchedule { get; private set; }

        public ActivityScheduleEditForm(List<Vehicle> vehicles, List<Driver> drivers) : this(vehicles, drivers, null)
        {
        }

        public ActivityScheduleEditForm(List<Vehicle> vehicles, List<Driver> drivers, ActivitySchedule? activitySchedule)
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
            this.Text = ActivitySchedule == null ? "Add Activity Schedule" : "Edit Activity Schedule";
            this.ClientSize = new Size(600, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11,
                Padding = new Padding(20),
                AutoSize = true
            };

            // Configure column styles
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Configure row styles
            for (int i = 0; i < 11; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Date
            var dateLabel = CreateLabel("Date:", Color.FromArgb(33, 33, 33));
            _datePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Value = DateTime.Today,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(dateLabel, 0, 0);
            tableLayout.Controls.Add(_datePicker, 1, 0);

            // Trip Type
            var tripTypeLabel = CreateLabel("Trip Type:", Color.FromArgb(33, 33, 33));
            _tripTypeComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select trip type",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            _tripTypeComboBox.Items.AddRange(new string[]
            {
                "Sports Trip",
                "Activity Trip",
                "Field Trip",
                "Competition",
                "Tournament",
                "School Event",
                "Other"
            });

            tableLayout.Controls.Add(tripTypeLabel, 0, 1);
            tableLayout.Controls.Add(_tripTypeComboBox, 1, 1);

            // Vehicle
            var vehicleLabel = CreateLabel("Vehicle:", Color.FromArgb(33, 33, 33));
            _vehicleComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select vehicle",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(vehicleLabel, 0, 2);
            tableLayout.Controls.Add(_vehicleComboBox, 1, 2);

            // Driver
            var driverLabel = CreateLabel("Driver:", Color.FromArgb(33, 33, 33));
            _driverComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select driver",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(driverLabel, 0, 3);
            tableLayout.Controls.Add(_driverComboBox, 1, 3);

            // Destination
            var destinationLabel = CreateLabel("Destination:", Color.FromArgb(33, 33, 33));
            _destinationTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter destination",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(destinationLabel, 0, 4);
            tableLayout.Controls.Add(_destinationTextBox, 1, 4);

            // Leave Time
            var leaveTimeLabel = CreateLabel("Leave Time:", Color.FromArgb(33, 33, 33));
            _leaveTimePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(leaveTimeLabel, 0, 5);
            tableLayout.Controls.Add(_leaveTimePicker, 1, 5);

            // Event Time
            var eventTimeLabel = CreateLabel("Event Time:", Color.FromArgb(33, 33, 33));
            _eventTimePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(eventTimeLabel, 0, 6);
            tableLayout.Controls.Add(_eventTimePicker, 1, 6);

            // Return Time
            var returnTimeLabel = CreateLabel("Return Time:", Color.FromArgb(33, 33, 33));
            _returnTimePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(returnTimeLabel, 0, 7);
            tableLayout.Controls.Add(_returnTimePicker, 1, 7);

            // Riders
            var ridersLabel = CreateLabel("Riders:", Color.FromArgb(33, 33, 33));
            _ridersTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Number of riders",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(ridersLabel, 0, 8);
            tableLayout.Controls.Add(_ridersTextBox, 1, 8);

            // Notes
            var notesLabel = CreateLabel("Notes:", Color.FromArgb(33, 33, 33));
            _notesTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Additional notes (optional)",
                Font = new Font("Roboto", 11F),
                Height = 80
            };

            tableLayout.Controls.Add(notesLabel, 0, 9);
            tableLayout.Controls.Add(_notesTextBox, 1, 9);

            // Button panel
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Height = 50
            };

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _saveButton = new MaterialButton
            {
                Text = "SAVE",
                Dock = DockStyle.Fill,
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false,
                Margin = new Padding(5)
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new MaterialButton
            {
                Text = "CANCEL",
                Dock = DockStyle.Fill,
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,
                UseAccentColor = false,
                Margin = new Padding(5)
            };
            _cancelButton.Click += CancelButton_Click;

            buttonPanel.Controls.Add(_saveButton, 0, 0);
            buttonPanel.Controls.Add(_cancelButton, 1, 0);

            tableLayout.Controls.Add(buttonPanel, 0, 10);
            tableLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(tableLayout);

            // Set tab order
            _datePicker.TabIndex = 0;
            _tripTypeComboBox.TabIndex = 1;
            _vehicleComboBox.TabIndex = 2;
            _driverComboBox.TabIndex = 3;
            _destinationTextBox.TabIndex = 4;
            _leaveTimePicker.TabIndex = 5;
            _eventTimePicker.TabIndex = 6;
            _returnTimePicker.TabIndex = 7;
            _ridersTextBox.TabIndex = 8;
            _notesTextBox.TabIndex = 9;
            _saveButton.TabIndex = 10;
            _cancelButton.TabIndex = 11;
        }

        private void PopulateDropdowns()
        {
            // Populate vehicle dropdown
            _vehicleComboBox.Items.Clear();
            foreach (var vehicle in _vehicles)
            {
                _vehicleComboBox.Items.Add(new { Text = vehicle.VehicleNumber, Value = vehicle });
            }
            _vehicleComboBox.DisplayMember = "Text";
            _vehicleComboBox.ValueMember = "Value";

            // Populate driver dropdown
            _driverComboBox.Items.Clear();
            foreach (var driver in _drivers)
            {
                _driverComboBox.Items.Add(new { Text = driver.DriverName, Value = driver });
            }
            _driverComboBox.DisplayMember = "Text";
            _driverComboBox.ValueMember = "Value";
        }

        private void PopulateFields(ActivitySchedule activitySchedule)
        {
            if (activitySchedule.DateAsDateTime.HasValue)
                _datePicker.Value = activitySchedule.DateAsDateTime.Value;

            _tripTypeComboBox.Text = activitySchedule.TripType ?? "";
            _destinationTextBox.Text = activitySchedule.ScheduledDestination ?? "";
            _ridersTextBox.Text = activitySchedule.ScheduledRiders?.ToString() ?? "";
            _notesTextBox.Text = activitySchedule.Notes ?? "";

            // Set times
            if (activitySchedule.ScheduledLeaveTime.HasValue)
            {
                var leaveTime = DateTime.Today.Add(activitySchedule.ScheduledLeaveTime.Value);
                _leaveTimePicker.Value = leaveTime;
            }

            if (activitySchedule.ScheduledEventTime.HasValue)
            {
                var eventTime = DateTime.Today.Add(activitySchedule.ScheduledEventTime.Value);
                _eventTimePicker.Value = eventTime;
            }

            if (activitySchedule.ScheduledReturnTime.HasValue)
            {
                var returnTime = DateTime.Today.Add(activitySchedule.ScheduledReturnTime.Value);
                _returnTimePicker.Value = returnTime;
            }

            // Set vehicle selection
            if (activitySchedule.ScheduledVehicleID.HasValue)
            {
                var vehicle = _vehicles.FirstOrDefault(v => v.VehicleID == activitySchedule.ScheduledVehicleID.Value);
                if (vehicle != null)
                {
                    _vehicleComboBox.SelectedValue = vehicle;
                }
            }

            // Set driver selection
            if (activitySchedule.ScheduledDriverID.HasValue)
            {
                var driver = _drivers.FirstOrDefault(d => d.DriverID == activitySchedule.ScheduledDriverID.Value);
                if (driver != null)
                {
                    _driverComboBox.SelectedValue = driver;
                }
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var schedule = ActivitySchedule ?? new ActivitySchedule();

                schedule.DateAsDateTime = _datePicker.Value.Date;
                schedule.TripType = _tripTypeComboBox.Text?.Trim();
                schedule.ScheduledDestination = _destinationTextBox.Text?.Trim();
                schedule.Notes = _notesTextBox.Text?.Trim();

                // Parse riders
                if (int.TryParse(_ridersTextBox.Text, out var riders))
                    schedule.ScheduledRiders = riders;

                // Set times
                schedule.ScheduledLeaveTime = _leaveTimePicker.Value.TimeOfDay;
                schedule.ScheduledEventTime = _eventTimePicker.Value.TimeOfDay;
                schedule.ScheduledReturnTime = _returnTimePicker.Value.TimeOfDay;

                // Set vehicle
                if (_vehicleComboBox.SelectedValue is Vehicle selectedVehicle)
                    schedule.ScheduledVehicleID = selectedVehicle.VehicleID;

                // Set driver
                if (_driverComboBox.SelectedValue is Driver selectedDriver)
                    schedule.ScheduledDriverID = selectedDriver.DriverID;

                ActivitySchedule = schedule;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving activity schedule: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_tripTypeComboBox.Text))
                errors.Add("Trip type is required.");

            if (string.IsNullOrWhiteSpace(_destinationTextBox.Text))
                errors.Add("Destination is required.");

            if (_vehicleComboBox.SelectedValue == null)
                errors.Add("Vehicle selection is required.");

            if (_driverComboBox.SelectedValue == null)
                errors.Add("Driver selection is required.");

            if (!string.IsNullOrWhiteSpace(_ridersTextBox.Text) && !int.TryParse(_ridersTextBox.Text, out _))
                errors.Add("Riders must be a valid number.");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private Label CreateLabel(string text, Color foreColor)
        {
            return new Label
            {
                Text = text,
                ForeColor = foreColor,
                Font = new Font("Roboto", 11F, FontStyle.Regular),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 8, 0, 0)
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _destinationTextBox?.Dispose();
                _notesTextBox?.Dispose();
                _ridersTextBox?.Dispose();
                _tripTypeComboBox?.Dispose();
                _vehicleComboBox?.Dispose();
                _driverComboBox?.Dispose();
                _datePicker?.Dispose();
                _leaveTimePicker?.Dispose();
                _eventTimePicker?.Dispose();
                _returnTimePicker?.Dispose();
                _saveButton?.Dispose();
                _cancelButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
