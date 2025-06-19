using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Views;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Driver Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Example of complete form migration demonstrating Syncfusion control usage
    /// </summary>
    public partial class DriverEditFormSyncfusion : SyncfusionBaseForm
    {
        private TextBoxExt _firstNameTextBox;
        private TextBoxExt _lastNameTextBox;
        private TextBoxExt _phoneTextBox;
        private TextBoxExt _emailTextBox;
        private TextBoxExt _addressTextBox;
        private TextBoxExt _cityTextBox;
        private TextBoxExt _stateTextBox;
        private TextBoxExt _zipTextBox;
        private TextBoxExt _notesTextBox;
        private ComboBoxAdv _licenseTypeComboBox;
        private ComboBoxAdv _statusComboBox;
        private CheckBox _trainingCompleteCheckBox; // Using standard CheckBox as Syncfusion CheckBox is not available
        private SfDateTimeEdit _cdlExpirationDatePicker;
        private SfButton _saveButton;
        private SfButton _cancelButton;

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
            this.ClientSize = new Size(600, 650); // DPI scaling is handled in SyncfusionBaseForm
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Labels - create and position manually
            var firstNameLabel = ControlFactory.CreateLabel("First Name:");
            firstNameLabel.Location = new Point(20, 25);
            _mainPanel.Controls.Add(firstNameLabel);

            var lastNameLabel = ControlFactory.CreateLabel("Last Name:");
            lastNameLabel.Location = new Point(300, 25);
            _mainPanel.Controls.Add(lastNameLabel);

            var phoneLabel = ControlFactory.CreateLabel("Phone:");
            phoneLabel.Location = new Point(20, 95);
            _mainPanel.Controls.Add(phoneLabel);

            var emailLabel = ControlFactory.CreateLabel("Email:");
            emailLabel.Location = new Point(300, 95);
            _mainPanel.Controls.Add(emailLabel);

            var addressLabel = ControlFactory.CreateLabel("Address:");
            addressLabel.Location = new Point(20, 165);
            _mainPanel.Controls.Add(addressLabel);

            var cityLabel = ControlFactory.CreateLabel("City:");
            cityLabel.Location = new Point(20, 235);
            _mainPanel.Controls.Add(cityLabel);

            var stateLabel = ControlFactory.CreateLabel("State:");
            stateLabel.Location = new Point(240, 235);
            _mainPanel.Controls.Add(stateLabel);

            var zipLabel = ControlFactory.CreateLabel("ZIP:");
            zipLabel.Location = new Point(340, 235);
            _mainPanel.Controls.Add(zipLabel);

            var licenseTypeLabel = ControlFactory.CreateLabel("License Type:");
            licenseTypeLabel.Location = new Point(20, 305);
            _mainPanel.Controls.Add(licenseTypeLabel);

            var statusLabel = ControlFactory.CreateLabel("Status:");
            statusLabel.Location = new Point(300, 305);
            _mainPanel.Controls.Add(statusLabel);

            var notesLabel = ControlFactory.CreateLabel("Notes:");
            notesLabel.Location = new Point(20, 375);
            _mainPanel.Controls.Add(notesLabel);

            var cdlExpirationLabel = ControlFactory.CreateLabel("CDL Expiration:");
            cdlExpirationLabel.Location = new Point(300, 445);
            _mainPanel.Controls.Add(cdlExpirationLabel);

            // Text boxes - use BannerTextProvider from base class
            _firstNameTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter first name");
            _lastNameTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter last name");
            _phoneTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter phone number");
            _emailTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter email address");
            _addressTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter address");
            _cityTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter city");
            _stateTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter state");
            _zipTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter ZIP code");
            _notesTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Enter notes", multiline: true);

            // Combo boxes
            _licenseTypeComboBox = ControlFactory.CreateComboBox();
            _statusComboBox = ControlFactory.CreateStatusComboBox();

            // Checkbox
            _trainingCompleteCheckBox = new CheckBox { Text = "Training Complete", AutoSize = true, BackColor = Color.Transparent };

            // Date picker
            _cdlExpirationDatePicker = ControlFactory.CreateDateTimePicker();

            // Buttons
            _saveButton = ControlFactory.CreatePrimaryButton("Save");
            _cancelButton = ControlFactory.CreateSecondaryButton("Cancel");

            // Set locations and add to panel
            _firstNameTextBox.Location = new Point(20, 50);
            _lastNameTextBox.Location = new Point(300, 50);
            _phoneTextBox.Location = new Point(20, 120);
            _emailTextBox.Location = new Point(300, 120);
            _addressTextBox.Location = new Point(20, 190);
            _addressTextBox.Size = new Size(530, 30);
            _cityTextBox.Location = new Point(20, 260);
            _stateTextBox.Location = new Point(240, 260);
            _stateTextBox.Size = new Size(80, 30);
            _zipTextBox.Location = new Point(340, 260);
            _zipTextBox.Size = new Size(100, 30);
            _notesTextBox.Location = new Point(20, 400);
            _notesTextBox.Size = new Size(530, 60);
            _licenseTypeComboBox.Location = new Point(20, 330);
            _statusComboBox.Location = new Point(300, 330);
            _trainingCompleteCheckBox.Location = new Point(20, 470);
            _cdlExpirationDatePicker.Location = new Point(300, 470);
            _saveButton.Location = new Point(20, 550);
            _cancelButton.Location = new Point(150, 550);

            _mainPanel.Controls.AddRange(new Control[]
            {
                _firstNameTextBox, _lastNameTextBox, _phoneTextBox, _emailTextBox,
                _addressTextBox, _cityTextBox, _stateTextBox, _zipTextBox, _notesTextBox,
                _licenseTypeComboBox, _statusComboBox, _trainingCompleteCheckBox,
                _cdlExpirationDatePicker, _saveButton, _cancelButton
            });
        }

        private void LayoutControls()
        {
            // Setup combo box items
            _licenseTypeComboBox.DataSource = new[] { "CDL Class A", "CDL Class B", "CDL Class C", "Regular License" };
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        #region Data Handling

        private void PopulateFields(Driver driver)
        {
            try
            {
                _firstNameTextBox.Text = driver.FirstName ?? string.Empty;
                _lastNameTextBox.Text = driver.LastName ?? string.Empty;
                _phoneTextBox.Text = driver.DriverPhone ?? string.Empty;
                _emailTextBox.Text = driver.DriverEmail ?? string.Empty;
                _addressTextBox.Text = driver.Address ?? string.Empty;
                _cityTextBox.Text = driver.City ?? string.Empty;
                _stateTextBox.Text = driver.State ?? string.Empty;
                _zipTextBox.Text = driver.Zip ?? string.Empty;
                _notesTextBox.Text = driver.Notes ?? string.Empty;

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
                FirstName = _firstNameTextBox.Text.Trim(),
                LastName = _lastNameTextBox.Text.Trim(),
                DriverPhone = _phoneTextBox.Text.Trim(),
                DriverEmail = _emailTextBox.Text.Trim(),
                Address = _addressTextBox.Text.Trim(),
                City = _cityTextBox.Text.Trim(),
                State = _stateTextBox.Text.Trim(),
                Zip = _zipTextBox.Text.Trim(),
                Notes = _notesTextBox.Text.Trim(),
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
            if (ValidateChildren())
            {
                Driver ??= new Driver();

                Driver.FirstName = _firstNameTextBox.Text;
                Driver.LastName = _lastNameTextBox.Text;
                Driver.DriverPhone = _phoneTextBox.Text;
                Driver.DriverEmail = _emailTextBox.Text;
                Driver.Address = _addressTextBox.Text;
                Driver.City = _cityTextBox.Text;
                Driver.State = _stateTextBox.Text;
                Driver.Zip = _zipTextBox.Text;
                Driver.Notes = _notesTextBox.Text;
                Driver.DriversLicenseType = _licenseTypeComboBox.SelectedItem?.ToString();
                Driver.Status = _statusComboBox.SelectedItem?.ToString();
                Driver.IsTrainingComplete = _trainingCompleteCheckBox.Checked;
                Driver.CDLExpirationDate = _cdlExpirationDatePicker.Value;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}
