using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Driver Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Example of complete form migration demonstrating Syncfusion control usage
    /// </summary>
    public partial class DriverEditFormSyncfusion : SyncfusionBaseForm
    {
        private Control? _firstNameTextBox;
        private Control? _lastNameTextBox;
        private Control? _phoneTextBox;
        private Control? _emailTextBox;
        private Control? _addressTextBox;
        private Control? _cityTextBox;
        private Control? _stateTextBox;
        private Control? _zipTextBox;
        private Control? _notesTextBox;
        private ComboBox? _licenseTypeComboBox;
        private ComboBox? _statusComboBox;
        private CheckBox? _trainingCompleteCheckBox;
        private DateTimePicker? _cdlExpirationDatePicker;
        private Control? _saveButton;
        private Control? _cancelButton;

        public Driver? Driver { get; private set; }

        public DriverEditFormSyncfusion() : this(null)
        {
        }

        public DriverEditFormSyncfusion(Driver? driver)
        {
            Driver = driver;
            InitializeComponent();
            if (driver != null)
            {
                PopulateFields(driver);
            }
        }

        private void InitializeComponent()
        {
            this.Text = Driver == null ? "Add Driver" : "Edit Driver";
            this.ClientSize = GetDpiAwareSize(new Size(600, 650));
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            RefreshMaterialTheme();

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Text boxes
            _firstNameTextBox = CreateTextBox(20, 50, 250);
            _lastNameTextBox = CreateTextBox(300, 50, 250);
            _phoneTextBox = CreateTextBox(20, 120, 250);
            _emailTextBox = CreateTextBox(300, 120, 250);
            _addressTextBox = CreateTextBox(20, 190, 530);
            _cityTextBox = CreateTextBox(20, 260, 200);
            _stateTextBox = CreateTextBox(240, 260, 80);
            _zipTextBox = CreateTextBox(340, 260, 100);
            _notesTextBox = CreateTextBox(20, 400, 530);

            // Combo boxes
            _licenseTypeComboBox = CreateComboBox(20, 330, 250);
            _statusComboBox = CreateComboBox(300, 330, 250);

            // Checkbox
            _trainingCompleteCheckBox = CreateCheckBox("Training Complete", 20, 470);

            // Date picker (standard control, will be themed)
            _cdlExpirationDatePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(300), GetDpiAwareY(470)),
                Size = new Size(GetDpiAwareWidth(250), GetDpiAwareHeight(35)),
                Format = DateTimePickerFormat.Short
            };
            Helpers.SyncfusionThemeHelper.ApplyMaterialTheme(_cdlExpirationDatePicker);
            _mainPanel.Controls.Add(_cdlExpirationDatePicker);

            // Buttons
            _saveButton = CreateButton("Save", 20, 20, 100);
            _cancelButton = CreateButton("Cancel", 140, 20, 100);

            // Labels
            CreateLabel("First Name:", 20, 25);
            CreateLabel("Last Name:", 300, 25);
            CreateLabel("Phone:", 20, 95);
            CreateLabel("Email:", 300, 95);
            CreateLabel("Address:", 20, 165);
            CreateLabel("City:", 20, 235);
            CreateLabel("State:", 240, 235);
            CreateLabel("ZIP:", 340, 235);
            CreateLabel("License Type:", 20, 305);
            CreateLabel("Status:", 300, 305);
            CreateLabel("Notes:", 20, 375);
            CreateLabel("CDL Expiration:", 300, 445);
        }

        private void LayoutControls()
        {
            // Additional layout adjustments if needed
            if (_notesTextBox is TextBox notesTextBox)
            {
                notesTextBox.Multiline = true;
                notesTextBox.Height = GetDpiAwareHeight(60);
            }

            // Setup combo box items
            _licenseTypeComboBox.Items.AddRange(new[] { "CDL Class A", "CDL Class B", "CDL Class C", "Regular License" });
            _statusComboBox.Items.AddRange(new[] { "Active", "Inactive", "On Leave", "Terminated" });
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        #region Control Creation Helpers

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(GetDpiAwareX(x), GetDpiAwareY(y)),
                Size = new Size(GetDpiAwareWidth(width), GetDpiAwareHeight(35)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Apply Material theming
            comboBox.BackColor = Helpers.SyncfusionThemeHelper.MaterialColors.Surface;
            comboBox.ForeColor = Helpers.SyncfusionThemeHelper.MaterialColors.Text;
            comboBox.Font = Helpers.SyncfusionThemeHelper.MaterialTheme.DefaultFont;

            _mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        private CheckBox CreateCheckBox(string text, int x, int y)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Location = new Point(GetDpiAwareX(x), GetDpiAwareY(y)),
                AutoSize = true
            };

            // Apply Material theming
            checkBox.BackColor = Color.Transparent;
            checkBox.ForeColor = Helpers.SyncfusionThemeHelper.MaterialColors.Text;
            checkBox.Font = Helpers.SyncfusionThemeHelper.MaterialTheme.DefaultFont;
              _mainPanel.Controls.Add(checkBox);
            return checkBox;
        }

        #endregion

        #region Data Handling

        private void PopulateFields(Driver driver)
        {
            try
            {
                if (_firstNameTextBox is TextBox firstNameTb) firstNameTb.Text = driver.FirstName ?? string.Empty;
                if (_lastNameTextBox is TextBox lastNameTb) lastNameTb.Text = driver.LastName ?? string.Empty;
                if (_phoneTextBox is TextBox phoneTb) phoneTb.Text = driver.DriverPhone ?? string.Empty;
                if (_emailTextBox is TextBox emailTb) emailTb.Text = driver.DriverEmail ?? string.Empty;
                if (_addressTextBox is TextBox addressTb) addressTb.Text = driver.Address ?? string.Empty;
                if (_cityTextBox is TextBox cityTb) cityTb.Text = driver.City ?? string.Empty;
                if (_stateTextBox is TextBox stateTb) stateTb.Text = driver.State ?? string.Empty;
                if (_zipTextBox is TextBox zipTb) zipTb.Text = driver.Zip ?? string.Empty;
                if (_notesTextBox is TextBox notesTb) notesTb.Text = driver.Notes ?? string.Empty;

                if (!string.IsNullOrEmpty(driver.DriversLicenseType))
                {
                    _licenseTypeComboBox.SelectedItem = driver.DriversLicenseType;
                }

                if (!string.IsNullOrEmpty(driver.Status))
                {
                    _statusComboBox.SelectedItem = driver.Status;
                }

                _trainingCompleteCheckBox.Checked = driver.IsTrainingComplete;
                _cdlExpirationDatePicker.Value = driver.CDLExpirationDate ?? DateTime.Now.AddYears(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating driver data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Driver GetDriverFromForm()
        {
            return new Driver
            {
                DriverID = Driver?.DriverID ?? 0,
                FirstName = (_firstNameTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                LastName = (_lastNameTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                DriverPhone = (_phoneTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                DriverEmail = (_emailTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                Address = (_addressTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                City = (_cityTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                State = (_stateTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                Zip = (_zipTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                Notes = (_notesTextBox as TextBox)?.Text?.Trim() ?? string.Empty,
                DriversLicenseType = _licenseTypeComboBox.SelectedItem?.ToString(),
                Status = _statusComboBox.SelectedItem?.ToString() ?? "Active",
                IsTrainingComplete = _trainingCompleteCheckBox.Checked,
                CDLExpirationDate = _cdlExpirationDatePicker.Value
            };
        }

        #endregion

        #region Event Handlers

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                var driver = GetDriverFromForm();

                // Here you would typically save to database
                // _databaseService.SaveDriver(driver);

                DialogResult = DialogResult.OK;
                Driver = driver;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving driver: {ex.Message}", "Error",
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

        private bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace((_firstNameTextBox as TextBox)?.Text))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace((_lastNameTextBox as TextBox)?.Text))
                errors.Add("Last name is required");

            if (string.IsNullOrWhiteSpace((_phoneTextBox as TextBox)?.Text))
                errors.Add("Phone number is required");

            if (_licenseTypeComboBox.SelectedItem == null)
                errors.Add("License type is required");

            if (_statusComboBox.SelectedItem == null)
                errors.Add("Status is required");

            if (errors.Count > 0)
            {
                MessageBox.Show($"Please correct the following errors:\n\n{string.Join("\n", errors)}",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        #endregion
    }
}

