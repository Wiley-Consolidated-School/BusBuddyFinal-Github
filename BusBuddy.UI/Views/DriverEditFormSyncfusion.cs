using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Views;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Driver Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Example of complete form migration demonstrating Syncfusion control usage
    /// </summary>
    public partial class DriverEditFormSyncfusion : SyncfusionBaseForm
    {
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private TextBox _phoneTextBox;
        private TextBox _emailTextBox;
        private TextBox _addressTextBox;
        private TextBox _cityTextBox;
        private TextBox _stateTextBox;
        private TextBox _zipTextBox;
        private TextBox _notesTextBox;
        private ComboBox _licenseTypeComboBox;
        private ComboBox _statusComboBox;
        private CheckBox _trainingCompleteCheckBox;
        private DateTimePicker _cdlExpirationDatePicker;
        private Button _saveButton;
        private Button _cancelButton;
        public Driver? Driver { get; set; }
        public DriverEditFormSyncfusion(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Driver = null;
            InitializeComponent();
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
            var firstNameLabel = ControlFactory.CreateLabel("First Name:", 20, 25, 100, 30);
            firstNameLabel.Location = new Point(20, 25);
            _mainPanel.Controls.Add(firstNameLabel);
            var lastNameLabel = ControlFactory.CreateLabel("Last Name:", 300, 25, 100, 30);
            lastNameLabel.Location = new Point(300, 25);
            _mainPanel.Controls.Add(lastNameLabel);
            var phoneLabel = ControlFactory.CreateLabel("Phone:", 20, 95, 100, 30);
            phoneLabel.Location = new Point(20, 95);
            _mainPanel.Controls.Add(phoneLabel);
            var emailLabel = ControlFactory.CreateLabel("Email:", 300, 95, 100, 30);
            emailLabel.Location = new Point(300, 95);
            _mainPanel.Controls.Add(emailLabel);
            var addressLabel = ControlFactory.CreateLabel("Address:", 20, 165, 100, 30);
            addressLabel.Location = new Point(20, 165);
            _mainPanel.Controls.Add(addressLabel);
            var cityLabel = ControlFactory.CreateLabel("City:", 20, 235, 100, 30);
            cityLabel.Location = new Point(20, 235);
            _mainPanel.Controls.Add(cityLabel);
            var stateLabel = ControlFactory.CreateLabel("State:", 240, 235, 80, 30);
            stateLabel.Location = new Point(240, 235);
            _mainPanel.Controls.Add(stateLabel);
            var zipLabel = ControlFactory.CreateLabel("ZIP:", 340, 235, 100, 30);
            zipLabel.Location = new Point(340, 235);
            _mainPanel.Controls.Add(zipLabel);
            var licenseTypeLabel = ControlFactory.CreateLabel("License Type:", 20, 305, 120, 30);
            licenseTypeLabel.Location = new Point(20, 305);
            _mainPanel.Controls.Add(licenseTypeLabel);
            var statusLabel = ControlFactory.CreateLabel("Status:", 300, 305, 100, 30);
            statusLabel.Location = new Point(300, 305);
            _mainPanel.Controls.Add(statusLabel);
            var notesLabel = ControlFactory.CreateLabel("Notes:", 20, 375, 100, 30);
            notesLabel.Location = new Point(20, 375);
            _mainPanel.Controls.Add(notesLabel);
            var cdlExpirationLabel = ControlFactory.CreateLabel("CDL Expiration:", 300, 445, 120, 30);
            cdlExpirationLabel.Location = new Point(300, 445);
            _mainPanel.Controls.Add(cdlExpirationLabel);
            // Text boxes
            _firstNameTextBox = ControlFactory.CreateTextBox("Enter first name", 20, 50, 200, 30);
            _lastNameTextBox = ControlFactory.CreateTextBox("Enter last name", 300, 50, 200, 30);
            _phoneTextBox = ControlFactory.CreateTextBox("Enter phone number", 20, 120, 200, 30);
            _emailTextBox = ControlFactory.CreateTextBox("Enter email address", 300, 120, 200, 30);
            _addressTextBox = ControlFactory.CreateTextBox("Enter address", 20, 190, 530, 30);
            _cityTextBox = ControlFactory.CreateTextBox("Enter city", 20, 260, 200, 30);
            _stateTextBox = ControlFactory.CreateTextBox("Enter state", 240, 260, 80, 30);
            _zipTextBox = ControlFactory.CreateTextBox("Enter ZIP code", 340, 260, 100, 30);
            _notesTextBox = ControlFactory.CreateTextBox("Enter notes", 20, 400, 530, 60);
            // Combo boxes
            _licenseTypeComboBox = new ComboBox { Location = new Point(20, 330), Size = new Size(200, 30) };
            _statusComboBox = new ComboBox { Location = new Point(300, 330), Size = new Size(200, 30) };
            // Checkbox
            _trainingCompleteCheckBox = new CheckBox { Text = "Training Complete", AutoSize = true, BackColor = Color.Transparent, Location = new Point(20, 470) };
            // Date picker
            _cdlExpirationDatePicker = new DateTimePicker { Location = new Point(300, 470), Size = new Size(200, 30) };
            // Buttons
            _saveButton = ControlFactory.CreatePrimaryButton("Save", 20, 550, 100, 35);
            _cancelButton = ControlFactory.CreateSecondaryButton("Cancel", 150, 550, 100, 35);
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
            // Setup combo box items with error handling
            try
            {
                _licenseTypeComboBox.DataSource = new[] { "CDL Class A", "CDL Class B", "CDL Class C", "Regular License" };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading license type options: {ex.Message}", "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                DriverId = Driver?.DriverId ?? 0,
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

