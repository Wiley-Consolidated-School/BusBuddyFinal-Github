using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Activity Trip Edit Form - Shell Structure
    /// Form for creating and editing individual activity trip records
    /// Based on Syncfusion input controls documentation
    ///
    /// 📖 SYNCFUSION DOCUMENTATION:
    /// - TextBoxExt: https://help.syncfusion.com/windowsforms/textbox/getting-started
    /// - SfDateTimeEdit: https://help.syncfusion.com/windowsforms/datetime-picker/getting-started
    /// - ComboBoxAdv: https://help.syncfusion.com/windowsforms/combobox/getting-started
    /// - SfButton: https://help.syncfusion.com/windowsforms/button/getting-started
    /// </summary>
    public partial class ActivityEditForm : SyncfusionBaseForm
    {
        #region Private Fields
        private readonly IActivityRepository _activityRepository;
        private readonly BusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;
        private readonly IMessageService _messageService;

        private Activity _activity;
        private bool _isEditMode;
        private bool _isReadOnlyMode;

        // Form controls - using Syncfusion controls per documentation
        private TextBoxExt _activityIdTextBox;
        private SfDateTimeEdit _activityDatePicker;
        private ComboBoxAdv _activityTypeComboBox;
        private TextBoxExt _destinationTextBox;
        private DateTimePickerAdv _leaveTimePicker;
        private DateTimePickerAdv _eventTimePicker;
        private DateTimePickerAdv _returnTimePicker;
        private SfNumericTextBox _ridersNumericBox;
        private ComboBoxAdv _busComboBox;
        private ComboBoxAdv _driverComboBox;
        private TextBoxExt _notesTextBox;

        // Action buttons
        private SfButton _saveButton;
        private SfButton _cancelButton;
        private SfButton _deleteButton;

        // Layout panels
        private new Panel _mainPanel;
        private new Panel _buttonPanel;
        private GroupBox _basicInfoGroup;
        private GroupBox _timingGroup;
        private GroupBox _assignmentGroup;
        #endregion

        #region Properties
        public Activity Activity => _activity;
        public bool IsEditMode => _isEditMode;
        public bool IsReadOnlyMode => _isReadOnlyMode;
        #endregion

        #region Constructors
        public ActivityEditForm() : this(new Activity())
        {
        }

        public ActivityEditForm(Activity activity) : this(activity, false)
        {
        }

        public ActivityEditForm(Activity activity, bool readOnlyMode)
        {
            _activity = activity ?? new Activity();
            _isEditMode = activity != null && activity.ActivityID > 0;
            _isReadOnlyMode = readOnlyMode;

            // Initialize repositories through service container
            var serviceContainer = new ServiceContainer();
            _activityRepository = serviceContainer.GetService<IActivityRepository>();
            _busRepository = serviceContainer.GetService<BusRepository>();
            _driverRepository = serviceContainer.GetService<IDriverRepository>();
            _messageService = new MessageBoxService();

            InitializeComponent();
            PopulateComboBoxes();

            if (_activity != null)
            {
                PopulateFields();
            }
        }
        #endregion

        #region Form Initialization - Shell Implementation
        private void InitializeComponent()
        {
            SetupFormProperties();
            CreateControls();
            LayoutControls();
            SetupEventHandlers();
            ApplyTheme();
        }

        private void SetupFormProperties()
        {
            this.Text = GetFormTitle();
            this.ClientSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CreateControls()
        {
            CreateLayoutPanels();
            CreateInputControls();
            CreateActionButtons();
        }

        private void LayoutControls()
        {
            LayoutMainPanels();
            LayoutInputControls();
            LayoutActionButtons();
        }

        private void SetupEventHandlers()
        {
            SetupButtonEvents();
            SetupValidationEvents();
            SetupDataChangeEvents();
        }
        #endregion

        #region Control Creation - Shell Methods
        private void CreateLayoutPanels()
        {
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            _buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(12, 8, 12, 12)
            };

            _basicInfoGroup = new GroupBox
            {
                Text = "Basic Information",
                Height = 120
            };

            _timingGroup = new GroupBox
            {
                Text = "Timing",
                Height = 100
            };

            _assignmentGroup = new GroupBox
            {
                Text = "Assignment",
                Height = 100
            };

            this.Controls.Add(_mainPanel);
            this.Controls.Add(_buttonPanel);
        }

        private void CreateInputControls()
        {
            // Activity ID - TextBoxExt per Syncfusion documentation
            _activityIdTextBox = new TextBoxExt
            {
                ReadOnly = true,
                Size = new Size(100, 23)
            };

            // Activity Date - SfDateTimeEdit per documentation
            _activityDatePicker = new SfDateTimeEdit
            {
                Size = new Size(150, 23)
            };

            // Activity Type - ComboBoxAdv per documentation
            _activityTypeComboBox = new ComboBoxAdv
            {
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Destination - TextBoxExt
            _destinationTextBox = new TextBoxExt
            {
                Size = new Size(200, 23)
            };

            // Time pickers - DateTimePickerAdv per documentation
            _leaveTimePicker = new DateTimePickerAdv
            {
                Format = DateTimePickerFormat.Time,
                Size = new Size(100, 23)
            };

            _eventTimePicker = new DateTimePickerAdv
            {
                Format = DateTimePickerFormat.Time,
                Size = new Size(100, 23)
            };

            _returnTimePicker = new DateTimePickerAdv
            {
                Format = DateTimePickerFormat.Time,
                Size = new Size(100, 23)
            };

            // Riders - SfNumericTextBox per documentation
            _ridersNumericBox = new SfNumericTextBox
            {
                Size = new Size(80, 23)
            };

            // bus - ComboBoxAdv
            _busComboBox = new ComboBoxAdv
            {
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Driver - ComboBoxAdv
            _driverComboBox = new ComboBoxAdv
            {
                Size = new Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Notes - TextBoxExt
            _notesTextBox = new TextBoxExt
            {
                Multiline = true,
                Size = new Size(300, 60)
            };
        }

        private void CreateActionButtons()
        {
            // Save Button - SfButton per documentation
            _saveButton = new SfButton
            {
                Text = "Save",
                Size = new Size(75, 30)
            };

            // Cancel Button - SfButton
            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Size = new Size(75, 30)
            };

            // Delete Button - SfButton
            _deleteButton = new SfButton
            {
                Text = "Delete",
                Size = new Size(75, 30),
                Visible = _isEditMode && !_isReadOnlyMode
            };

            _buttonPanel.Controls.Add(_deleteButton);
            _buttonPanel.Controls.Add(_cancelButton);
            _buttonPanel.Controls.Add(_saveButton);
        }
        #endregion

        #region Layout Methods - Shell Implementation
        private void LayoutMainPanels()
        {
            // Add group boxes to main panel with proper spacing
            _basicInfoGroup.Location = new Point(12, 12);
            _basicInfoGroup.Width = _mainPanel.ClientSize.Width - 24;
            _basicInfoGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _timingGroup.Location = new Point(12, _basicInfoGroup.Bottom + 12);
            _timingGroup.Width = _mainPanel.ClientSize.Width - 24;
            _timingGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _assignmentGroup.Location = new Point(12, _timingGroup.Bottom + 12);
            _assignmentGroup.Width = _mainPanel.ClientSize.Width - 24;
            _assignmentGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // Add group boxes to main panel
            _mainPanel.Controls.Add(_basicInfoGroup);
            _mainPanel.Controls.Add(_timingGroup);
            _mainPanel.Controls.Add(_assignmentGroup);
        }

        private void LayoutInputControls()
        {
            // Basic Information Group Layout
            var lblActivityId = new Label { Text = "Activity ID:", Location = new Point(12, 25), Size = new Size(80, 23) };
            var lblActivityDate = new Label { Text = "Date:", Location = new Point(200, 25), Size = new Size(40, 23) };
            var lblActivityType = new Label { Text = "Type:", Location = new Point(400, 25), Size = new Size(40, 23) };
            var lblDestination = new Label { Text = "Destination:", Location = new Point(12, 55), Size = new Size(80, 23) };

            _activityIdTextBox.Location = new Point(100, 22);
            _activityDatePicker.Location = new Point(250, 22);
            _activityTypeComboBox.Location = new Point(450, 22);
            _destinationTextBox.Location = new Point(100, 52);

            _basicInfoGroup.Controls.AddRange(new Control[] {
                lblActivityId, _activityIdTextBox,
                lblActivityDate, _activityDatePicker,
                lblActivityType, _activityTypeComboBox,
                lblDestination, _destinationTextBox
            });

            // Timing Group Layout
            var lblLeaveTime = new Label { Text = "Leave:", Location = new Point(12, 25), Size = new Size(60, 23) };
            var lblEventTime = new Label { Text = "Event:", Location = new Point(150, 25), Size = new Size(60, 23) };
            var lblReturnTime = new Label { Text = "Return:", Location = new Point(290, 25), Size = new Size(60, 23) };
            var lblRiders = new Label { Text = "Riders:", Location = new Point(430, 25), Size = new Size(50, 23) };

            _leaveTimePicker.Location = new Point(80, 22);
            _eventTimePicker.Location = new Point(220, 22);
            _returnTimePicker.Location = new Point(360, 22);
            _ridersNumericBox.Location = new Point(490, 22);

            _timingGroup.Controls.AddRange(new Control[] {
                lblLeaveTime, _leaveTimePicker,
                lblEventTime, _eventTimePicker,
                lblReturnTime, _returnTimePicker,
                lblRiders, _ridersNumericBox
            });

            // Assignment Group Layout
            var lblBus = new Label { Text = "Bus:", Location = new Point(12, 25), Size = new Size(60, 23) };
            var lblDriver = new Label { Text = "Driver:", Location = new Point(200, 25), Size = new Size(50, 23) };
            var lblNotes = new Label { Text = "Notes:", Location = new Point(400, 25), Size = new Size(50, 23) };

            _busComboBox.Location = new Point(80, 22);
            _driverComboBox.Location = new Point(260, 22);
            _notesTextBox.Location = new Point(460, 22);

            _assignmentGroup.Controls.AddRange(new Control[] {
                lblBus, _busComboBox,
                lblDriver, _driverComboBox,
                lblNotes, _notesTextBox
            });
        }

        private void LayoutActionButtons()
        {
            // Right-align buttons with proper spacing
            int buttonSpacing = 8;
            int rightMargin = 12;

            _saveButton.Location = new Point(
                _buttonPanel.ClientSize.Width - _saveButton.Width - rightMargin,
                (_buttonPanel.ClientSize.Height - _saveButton.Height) / 2
            );
            _saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            _cancelButton.Location = new Point(
                _saveButton.Left - _cancelButton.Width - buttonSpacing,
                (_buttonPanel.ClientSize.Height - _cancelButton.Height) / 2
            );
            _cancelButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            _deleteButton.Location = new Point(
                _cancelButton.Left - _deleteButton.Width - buttonSpacing,
                (_buttonPanel.ClientSize.Height - _deleteButton.Height) / 2
            );
            _deleteButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        }
        #endregion

        #region Data Methods - Shell Implementation
        private void PopulateFields()
        {
            if (_activity == null) return;

            try
            {
                // Basic Information
                _activityIdTextBox.Text = _activity.ActivityID.ToString();

                if (_activity.DateAsDateTime.HasValue)
                    _activityDatePicker.Value = _activity.DateAsDateTime.Value;

                if (!string.IsNullOrEmpty(_activity.ActivityType))
                {
                    for (int i = 0; i < _activityTypeComboBox.Items.Count; i++)
                    {
                        if (_activityTypeComboBox.Items[i].ToString() == _activity.ActivityType)
                        {
                            _activityTypeComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                _destinationTextBox.Text = _activity.Destination ?? string.Empty;

                // Timing - Convert TimeSpan to DateTime for time pickers
                if (_activity.LeaveTimeSpan.HasValue)
                {
                    var baseDate = DateTime.Today;
                    _leaveTimePicker.Value = baseDate.Add(_activity.LeaveTimeSpan.Value);
                }
                if (_activity.EventTimeSpan.HasValue)
                {
                    var baseDate = DateTime.Today;
                    _eventTimePicker.Value = baseDate.Add(_activity.EventTimeSpan.Value);
                }
                if (_activity.ReturnTimeSpan.HasValue)
                {
                    var baseDate = DateTime.Today;
                    _returnTimePicker.Value = baseDate.Add(_activity.ReturnTimeSpan.Value);
                }

                // Set riders to 0 if not specified (Activity model doesn't have NumberOfRiders)
                _ridersNumericBox.Value = 0;

                // Assignment
                if (_activity.AssignedBusID.HasValue)
                {
                    for (int i = 0; i < _busComboBox.Items.Count; i++)
                    {
                        if (_busComboBox.Items[i] is ComboBoxItem item &&
                            item.Value.Equals(_activity.AssignedBusID.Value))
                        {
                            _busComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                if (_activity.DriverId.HasValue)
                {
                    for (int i = 0; i < _driverComboBox.Items.Count; i++)
                    {
                        if (_driverComboBox.Items[i] is ComboBoxItem item &&
                            item.Value.Equals(_activity.DriverId.Value))
                        {
                            _driverComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                }

                _notesTextBox.Text = _activity.Notes ?? string.Empty;

                // Set read-only mode if specified
                if (_isReadOnlyMode)
                {
                    SetReadOnlyMode();
                }
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error loading activity data: {ex.Message}");
            }
        }

        private void SetReadOnlyMode()
        {
            _activityDatePicker.Enabled = false;
            _activityTypeComboBox.Enabled = false;
            _destinationTextBox.ReadOnly = true;
            _leaveTimePicker.Enabled = false;
            _eventTimePicker.Enabled = false;
            _returnTimePicker.Enabled = false;
            _ridersNumericBox.Enabled = false;
            _busComboBox.Enabled = false;
            _driverComboBox.Enabled = false;
            _notesTextBox.ReadOnly = true;
            _saveButton.Enabled = false;
            _deleteButton.Enabled = false;
        }

        private void PopulateComboBoxes()
        {
            try
            {
                // Populate Activity Type ComboBox
                _activityTypeComboBox.Items.Clear();
                _activityTypeComboBox.Items.AddRange(new string[]
                {
                    "Field Trip",
                    "Sports Event",
                    "Academic Competition",
                    "Community Service",
                    "Special Event",
                    "Transportation Service"
                });

                // Bus ComboBox
                _busComboBox.Items.Clear();
                if (_busRepository != null)
                {
                    var buses = _busRepository.GetAllBuses();
                    foreach (var bus in buses)
                    {
                        _busComboBox.Items.Add(new ComboBoxItem
                        {
                            Text = $"{bus.BusNumber} - {bus.Make} {bus.Model}",
                            Value = bus.BusId
                        });
                    }
                }

                // Populate Driver ComboBox
                _driverComboBox.Items.Clear();
                if (_driverRepository != null)
                {
                    var drivers = _driverRepository.GetAllDrivers();
                    foreach (var driver in drivers)
                    {
                        _driverComboBox.Items.Add(new ComboBoxItem
                        {
                            Text = $"{driver.FirstName} {driver.LastName}",
                            Value = driver.DriverId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error loading dropdown data: {ex.Message}");
            }
        }

        // Helper class for ComboBox items
        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }

        private Activity GetActivityFromForm()
        {
            var activity = new Activity();

            // Preserve existing ActivityID if in edit mode
            if (_isEditMode && _activity != null)
            {
                activity.ActivityID = _activity.ActivityID;
            }

            // Basic Information
            activity.DateAsDateTime = _activityDatePicker.Value;
            activity.ActivityType = _activityTypeComboBox.SelectedIndex >= 0 ?
                _activityTypeComboBox.SelectedItem.ToString() : null;
            activity.Destination = string.IsNullOrWhiteSpace(_destinationTextBox.Text) ?
                null : _destinationTextBox.Text.Trim();

            // Timing - Convert DateTime time to TimeSpan
            activity.LeaveTimeSpan = _leaveTimePicker.Value.TimeOfDay;
            activity.EventTimeSpan = _eventTimePicker.Value.TimeOfDay;
            activity.ReturnTimeSpan = _returnTimePicker.Value.TimeOfDay;

            // Assignment
            if (_busComboBox.SelectedItem is ComboBoxItem busItem)
            {
                activity.AssignedBusID = (int)busItem.Value;
            }

            if (_driverComboBox.SelectedItem is ComboBoxItem driverItem)
            {
                activity.DriverId = (int)driverItem.Value;
            }

            // Additional fields
            activity.Notes = string.IsNullOrWhiteSpace(_notesTextBox.Text) ?
                null : _notesTextBox.Text.Trim();

            // Set RequestedBy from current user context
            // For now, use a default value
            if (string.IsNullOrEmpty(activity.RequestedBy))
            {
                activity.RequestedBy = Environment.UserName;
            }

            return activity;
        }

        private new bool ValidateForm()
        {
            var validationErrors = new List<string>();

            // Validate required fields
            if (_activityDatePicker.Value == null)
                validationErrors.Add("Activity date is required.");

            if (_activityTypeComboBox.SelectedIndex == -1)
                validationErrors.Add("Activity type is required.");

            if (string.IsNullOrWhiteSpace(_destinationTextBox.Text))
                validationErrors.Add("Destination is required.");

            // Validate time logic
            var leaveTime = _leaveTimePicker.Value.TimeOfDay;
            var eventTime = _eventTimePicker.Value.TimeOfDay;
            var returnTime = _returnTimePicker.Value.TimeOfDay;

            if (eventTime <= leaveTime)
                validationErrors.Add("Event time must be after leave time.");

            if (returnTime <= eventTime)
                validationErrors.Add("Return time must be after event time.");

            // Validate riders count
            if (_ridersNumericBox.Value < 0 || _ridersNumericBox.Value > 999)
                validationErrors.Add("Number of riders must be between 0 and 999.");

            // Validate bus capacity if bus is selected
            if (_busComboBox.SelectedIndex != -1 && _ridersNumericBox.Value > 0)
            {
                if (_busComboBox.SelectedItem is ComboBoxItem busItem)
                {
                    try
                    {
                        var bus = _busRepository?.GetBusById((int)busItem.Value);
                        if (bus != null && _ridersNumericBox.Value > bus.Capacity)
                        {
                            validationErrors.Add($"Number of riders ({_ridersNumericBox.Value}) exceeds bus capacity ({bus.Capacity}).");
                        }
                    }
                    catch
                    {
                        // If we can't validate capacity, just warn
                        validationErrors.Add("Could not validate bus capacity. Please verify manually.");
                    }
                }
            }

            // Display validation errors
            if (validationErrors.Count > 0)
            {
                var errorMessage = "Please correct the following errors:\n\n" +
                                 string.Join("\n", validationErrors);
                _messageService?.ShowError(errorMessage);
                return false;
            }

            return true;
        }
        #endregion

        #region Event Handlers - Shell Implementation
        private void SetupButtonEvents()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
            _deleteButton.Click += DeleteButton_Click;
        }

        private void SetupValidationEvents()
        {
            // Add validation on field changes for immediate feedback
            _destinationTextBox.TextChanged += (s, e) => ValidateDestination();
            _ridersNumericBox.ValueChanged += (s, e) => ValidateRidersCount();
            _activityTypeComboBox.SelectedIndexChanged += (s, e) => ValidateActivityType();
        }

        private void ValidateDestination()
        {
            if (string.IsNullOrWhiteSpace(_destinationTextBox.Text))
            {
                _destinationTextBox.BackColor = Color.LightPink;
            }
            else
            {
                _destinationTextBox.BackColor = SystemColors.Window;
            }
        }

        private void ValidateRidersCount()
        {
            if (_ridersNumericBox.Value < 0)
            {
                _ridersNumericBox.BackColor = Color.LightPink;
            }
            else
            {
                _ridersNumericBox.BackColor = SystemColors.Window;
            }
        }

        private void ValidateActivityType()
        {
            if (_activityTypeComboBox.SelectedIndex == -1)
            {
                _activityTypeComboBox.BackColor = Color.LightPink;
            }
            else
            {
                _activityTypeComboBox.BackColor = SystemColors.Window;
            }
        }

        private void SetupDataChangeEvents()
        {
            // Track form changes for unsaved changes warning
            _activityDatePicker.ValueChanged += OnFormDataChanged;
            _activityTypeComboBox.SelectedIndexChanged += OnFormDataChanged;
            _destinationTextBox.TextChanged += OnFormDataChanged;
            _leaveTimePicker.ValueChanged += OnFormDataChanged;
            _eventTimePicker.ValueChanged += OnFormDataChanged;
            _returnTimePicker.ValueChanged += OnFormDataChanged;
            _ridersNumericBox.ValueChanged += OnFormDataChanged;
            _busComboBox.SelectedIndexChanged += OnFormDataChanged;
            _driverComboBox.SelectedIndexChanged += OnFormDataChanged;
            _notesTextBox.TextChanged += OnFormDataChanged;
        }

        private bool _hasUnsavedChanges = false;
        private void OnFormDataChanged(object sender, EventArgs e)
        {
            _hasUnsavedChanges = true;
            // Update form title to indicate unsaved changes
            if (!this.Text.EndsWith("*"))
            {
                this.Text += "*";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form before saving
                if (!ValidateForm())
                    return;

                // Get activity data from form
                var activityToSave = GetActivityFromForm();

                // Save the activity
                if (_isEditMode)
                {
                    // Update existing activity
                    var updateResult = _activityRepository.UpdateActivity(activityToSave);
                    if (updateResult)
                    {
                        _activity = activityToSave; // Update local copy
                        _messageService?.ShowInfo("Activity updated successfully.");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        _messageService?.ShowError("Failed to update activity. Please try again.");
                    }
                }
                else
                {
                    // Create new activity
                    var newActivityId = _activityRepository.AddActivity(activityToSave);
                    if (newActivityId > 0)
                    {
                        activityToSave.ActivityID = newActivityId;
                        _activity = activityToSave; // Update local copy with new ID
                        _messageService?.ShowInfo("Activity created successfully.");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        _messageService?.ShowError("Failed to create activity. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error saving activity: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Check for unsaved changes
            if (_hasUnsavedChanges && !_isReadOnlyMode)
            {
                var confirmed = _messageService?.ShowConfirmation(
                    "You have unsaved changes. Are you sure you want to cancel?") ?? false;
                if (!confirmed)
                    return;
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Check for unsaved changes when closing form
            if (_hasUnsavedChanges && !_isReadOnlyMode && this.DialogResult != DialogResult.OK)
            {
                var confirmed = _messageService?.ShowConfirmation(
                    "You have unsaved changes. Are you sure you want to close?") ?? false;
                if (!confirmed)
                {
                    e.Cancel = true;
                    return;
                }
            }
            base.OnFormClosing(e);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_isEditMode || _activity == null || _activity.ActivityID <= 0)
                {
                    _messageService?.ShowError("Cannot delete: Invalid activity.");
                    return;
                }

                // Confirm deletion
                var confirmed = _messageService?.ShowConfirmation(
                    $"Are you sure you want to delete this activity?\n\n" +
                    $"Type: {_activity.ActivityType}\n" +
                    $"Destination: {_activity.Destination}\n" +
                    $"Date: {_activity.Date}\n\n" +
                    "This action cannot be undone.") ?? false;

                if (!confirmed)
                    return;

                // Delete the activity
                var deleteResult = _activityRepository.DeleteActivity(_activity.ActivityID);
                if (deleteResult)
                {
                    _messageService?.ShowInfo("Activity deleted successfully.");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    _messageService?.ShowError("Failed to delete activity. It may be referenced by other records.");
                }
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error deleting activity: {ex.Message}");
            }
        }
        #endregion

        #region Helper Methods
        private string GetFormTitle()
        {
            if (_isReadOnlyMode)
                return "Activity Trip Details";
            else if (_isEditMode)
                return "Edit Activity Trip";
            else
                return "Add New Activity Trip";
        }

        private void ApplyTheme()
        {
            // Apply BusBuddy theme colors and styling
            this.BackColor = Color.White;

            // Group box styling
            _basicInfoGroup.ForeColor = Color.DarkBlue;
            _timingGroup.ForeColor = Color.DarkBlue;
            _assignmentGroup.ForeColor = Color.DarkBlue;

            // SfButton styling with BusBuddy colors
            _saveButton.BackColor = Color.FromArgb(0, 122, 204); // Blue
            _saveButton.ForeColor = Color.White;
            _saveButton.Style.BackColor = Color.FromArgb(0, 122, 204);
            _saveButton.Style.ForeColor = Color.White;

            _cancelButton.BackColor = Color.FromArgb(108, 117, 125); // Gray
            _cancelButton.ForeColor = Color.White;
            _cancelButton.Style.BackColor = Color.FromArgb(108, 117, 125);
            _cancelButton.Style.ForeColor = Color.White;

            _deleteButton.BackColor = Color.FromArgb(220, 53, 69); // Red
            _deleteButton.ForeColor = Color.White;
            _deleteButton.Style.BackColor = Color.FromArgb(220, 53, 69);
            _deleteButton.Style.ForeColor = Color.White;
        }
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of all controls and resources
                _activityIdTextBox?.Dispose();
                _activityDatePicker?.Dispose();
                _activityTypeComboBox?.Dispose();
                _destinationTextBox?.Dispose();
                _leaveTimePicker?.Dispose();
                _eventTimePicker?.Dispose();
                _returnTimePicker?.Dispose();
                _ridersNumericBox?.Dispose();
                _busComboBox?.Dispose();
                _driverComboBox?.Dispose();
                _notesTextBox?.Dispose();
                _saveButton?.Dispose();
                _cancelButton?.Dispose();
                _deleteButton?.Dispose();
                _mainPanel?.Dispose();
                _buttonPanel?.Dispose();
                _basicInfoGroup?.Dispose();
                _timingGroup?.Dispose();
                _assignmentGroup?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}

