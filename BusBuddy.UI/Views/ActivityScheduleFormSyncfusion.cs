using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    public partial class ActivityScheduleFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IActivityScheduleRepository _activityScheduleRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IDriverRepository _driverRepository;
        private ActivitySchedule _activitySchedule;
        private bool _isEditMode;

        // Form controls
        private SfDateTimeEdit _dateEdit;
        private ComboBox _tripTypeComboBox;
        private ComboBox _vehicleComboBox;
        private TextBox _destinationTextBox;
        private DateTimePickerAdv _leaveTimePicker;
        private DateTimePickerAdv _eventTimePicker;
        private DateTimePickerAdv _returnTimePicker;
        private SfNumericTextBox _ridersTextBox;
        private ComboBox _driverComboBox;
        private TextBox _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public ActivityScheduleFormSyncfusion() : this(new ActivitySchedule())
        {
        }

        public ActivityScheduleFormSyncfusion(ActivitySchedule activitySchedule)
        {
            var serviceContainer = new BusBuddy.UI.Services.ServiceContainer();
            _activityScheduleRepository = serviceContainer.GetService<IActivityScheduleRepository>();
            _vehicleRepository = serviceContainer.GetService<IVehicleRepository>();
            _driverRepository = serviceContainer.GetService<IDriverRepository>();
            _activitySchedule = activitySchedule ?? new ActivitySchedule();
            _isEditMode = activitySchedule != null && activitySchedule.ScheduleID > 0;
            InitializeComponent();
        }

        public ActivityScheduleFormSyncfusion(ActivitySchedule activitySchedule, IActivityScheduleRepository activityScheduleRepository, IVehicleRepository vehicleRepository, IDriverRepository driverRepository)
        {
            _activityScheduleRepository = activityScheduleRepository ?? throw new ArgumentNullException(nameof(activityScheduleRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            _activitySchedule = activitySchedule ?? new ActivitySchedule();
            _isEditMode = activitySchedule != null && activitySchedule.ScheduleID > 0;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "Activity Schedule Entry";
            Size = new Size(500, 650);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = System.Drawing.Color.White;

            // Date
            var dateLabel = new Label
            {
                Text = "Date:",
                Location = new Point(20, 30),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(dateLabel);

            _dateEdit = new SfDateTimeEdit
            {
                Location = new Point(130, 30),
                Size = new Size(300, 30),
                Value = DateTime.Today
            };
            Controls.Add(_dateEdit);

            // Trip Type
            var tripTypeLabel = new Label
            {
                Text = "Trip Type:",
                Location = new Point(20, 80),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(tripTypeLabel);

            _tripTypeComboBox = new ComboBox
            {
                Location = new Point(130, 80),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _tripTypeComboBox.Items.Add("Sports Trip");
            _tripTypeComboBox.Items.Add("Activity Trip");
            Controls.Add(_tripTypeComboBox);

            // Vehicle
            var vehicleLabel = new Label
            {
                Text = "Vehicle:",
                Location = new Point(20, 130),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(vehicleLabel);

            _vehicleComboBox = new ComboBox
            {
                Location = new Point(130, 130),
                Size = new Size(300, 30),
                DisplayMember = "VehicleNumber",
                ValueMember = "VehicleID",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_vehicleComboBox);

            // Destination
            var destinationLabel = new Label
            {
                Text = "Destination:",
                Location = new Point(20, 180),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(destinationLabel);

            _destinationTextBox = new TextBox
            {
                Location = new Point(130, 180),
                Size = new Size(300, 23)
            };
            Controls.Add(_destinationTextBox);

            // Leave Time
            var leaveTimeLabel = new Label
            {
                Text = "Leave Time:",
                Location = new Point(20, 230),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(leaveTimeLabel);

            _leaveTimePicker = new DateTimePickerAdv
            {
                Location = new Point(130, 230),
                Size = new Size(300, 30),
                Format = System.Windows.Forms.DateTimePickerFormat.Time
            };
            Controls.Add(_leaveTimePicker);

            // Event Time
            var eventTimeLabel = new Label
            {
                Text = "Event Time:",
                Location = new Point(20, 280),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(eventTimeLabel);

            _eventTimePicker = new DateTimePickerAdv
            {
                Location = new Point(130, 280),
                Size = new Size(300, 30),
                Format = System.Windows.Forms.DateTimePickerFormat.Time
            };
            Controls.Add(_eventTimePicker);

            // Return Time
            var returnTimeLabel = new Label
            {
                Text = "Return Time:",
                Location = new Point(20, 330),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(returnTimeLabel);

            _returnTimePicker = new DateTimePickerAdv
            {
                Location = new Point(130, 330),
                Size = new Size(300, 30),
                Format = System.Windows.Forms.DateTimePickerFormat.Time
            };
            Controls.Add(_returnTimePicker);

            // Riders
            var ridersLabel = new Label
            {
                Text = "Riders:",
                Location = new Point(20, 380),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(ridersLabel);

            _ridersTextBox = new SfNumericTextBox
            {
                Location = new Point(130, 380),
                Size = new Size(300, 30)
            };
            Controls.Add(_ridersTextBox);

            // Driver
            var driverLabel = new Label
            {
                Text = "Driver:",
                Location = new Point(20, 430),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(driverLabel);

            _driverComboBox = new ComboBox
            {
                Location = new Point(130, 430),
                Size = new Size(300, 30),
                DisplayMember = "FullName",
                ValueMember = "DriverID",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_driverComboBox);

            // Notes
            var notesLabel = new Label
            {
                Text = "Notes:",
                Location = new Point(20, 480),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(notesLabel);

            _notesTextBox = new TextBox
            {
                Location = new Point(130, 480),
                Size = new Size(300, 60),
                Multiline = true
            };
            Controls.Add(_notesTextBox);

            // Buttons
            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(260, 560),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme)
            };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);

            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(350, 560),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetErrorColor(BusBuddyThemeManager.CurrentTheme)
            };
            _cancelButton.Click += CancelButton_Click;
            Controls.Add(_cancelButton);

            ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadVehicles();
            LoadDrivers();
            LoadActivityScheduleData();
        }

        private void LoadVehicles()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAllVehicles().ToList();
                _vehicleComboBox.DataSource = vehicles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDrivers()
        {
            try
            {
                var drivers = _driverRepository.GetAllDrivers().ToList();
                _driverComboBox.DataSource = drivers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadActivityScheduleData()
        {
            if (_activitySchedule != null)
            {
                _dateEdit.Value = _activitySchedule.DateAsDateTime ?? DateTime.Today;
                _tripTypeComboBox.SelectedItem = _activitySchedule.TripType;
                _vehicleComboBox.SelectedValue = _activitySchedule.ScheduledVehicleID;
                _destinationTextBox.Text = _activitySchedule.ScheduledDestination ?? "";

                if (_activitySchedule.ScheduledLeaveTime.HasValue)
                {
                    _leaveTimePicker.Value = DateTime.Today.Add(_activitySchedule.ScheduledLeaveTime.Value);
                }

                if (_activitySchedule.ScheduledEventTime.HasValue)
                {
                    _eventTimePicker.Value = DateTime.Today.Add(_activitySchedule.ScheduledEventTime.Value);
                }

                if (_activitySchedule.ScheduledReturnTime.HasValue)
                {
                    _returnTimePicker.Value = DateTime.Today.Add(_activitySchedule.ScheduledReturnTime.Value);
                }

                _ridersTextBox.Value = _activitySchedule.ScheduledRiders;
                _driverComboBox.SelectedValue = _activitySchedule.ScheduledDriverID;
                _notesTextBox.Text = _activitySchedule.Notes ?? "";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(_tripTypeComboBox.SelectedItem?.ToString()))
                {
                    MessageBox.Show("Please select a trip type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update activity schedule object
                _activitySchedule.DateAsDateTime = _dateEdit.Value;
                _activitySchedule.TripType = _tripTypeComboBox.SelectedItem?.ToString();
                _activitySchedule.ScheduledVehicleID = (int?)_vehicleComboBox.SelectedValue;
                _activitySchedule.ScheduledDestination = _destinationTextBox.Text;
                _activitySchedule.ScheduledLeaveTime = _leaveTimePicker.Value.TimeOfDay;
                _activitySchedule.ScheduledEventTime = _eventTimePicker.Value.TimeOfDay;
                _activitySchedule.ScheduledReturnTime = _returnTimePicker.Value.TimeOfDay;
                _activitySchedule.ScheduledRiders = (int?)_ridersTextBox.Value;
                _activitySchedule.ScheduledDriverID = (int?)_driverComboBox.SelectedValue;
                _activitySchedule.Notes = _notesTextBox.Text;

                // Save to database
                if (_isEditMode)
                {
                    _activityScheduleRepository.UpdateScheduledActivity(_activitySchedule);
                }
                else
                {
                    _activityScheduleRepository.AddScheduledActivity(_activitySchedule);
                }

                MessageBox.Show("Activity schedule saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving activity schedule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Repository disposal removed - handled by dependency injection
            }
            base.Dispose(disposing);
        }
    }
}
