using BusBuddy.Models;
using BusBuddy.UI.Base;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Views
{
    public partial class DriverFormSyncfusion : SyncfusionBaseForm
    {
        private TextBoxExt _firstNameTextBox;
        private TextBoxExt _lastNameTextBox;
        private ComboBoxAdv _licenseTypeComboBox;
        private SfDateTimeEdit _cdlExpiryDatePicker;
        private TextBoxExt _phoneTextBox;
        private TextBoxExt _emailTextBox;
        private ComboBoxAdv _statusComboBox;
        private TextBoxExt _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public Driver Driver { get; private set; }

        public DriverFormSyncfusion(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Driver = new Driver();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = Driver.DriverId == 0 ? "Add Driver" : "Edit Driver";
            this.ClientSize = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            // Create labels
            var firstNameLabel = new Label { Text = "First Name:", Location = new Point(20, 25), Size = new Size(100, 20) };
            var lastNameLabel = new Label { Text = "Last Name:", Location = new Point(300, 25), Size = new Size(100, 20) };
            var licenseTypeLabel = new Label { Text = "License Type:", Location = new Point(20, 95), Size = new Size(100, 20) };
            var cdlExpiryLabel = new Label { Text = "CDL Expiry:", Location = new Point(300, 95), Size = new Size(100, 20) };
            var phoneLabel = new Label { Text = "Phone:", Location = new Point(20, 165), Size = new Size(100, 20) };
            var emailLabel = new Label { Text = "Email:", Location = new Point(300, 165), Size = new Size(100, 20) };
            var statusLabel = new Label { Text = "Status:", Location = new Point(20, 235), Size = new Size(100, 20) };
            var notesLabel = new Label { Text = "Notes:", Location = new Point(20, 305), Size = new Size(100, 20) };

            _mainPanel.Controls.AddRange(new Control[] {
                firstNameLabel, lastNameLabel, licenseTypeLabel, cdlExpiryLabel,
                phoneLabel, emailLabel, statusLabel, notesLabel
            });

            // Create input controls
            _firstNameTextBox = new TextBoxExt
            {
                Location = new Point(20, 50),
                Size = new Size(250, 25)
            };

            _lastNameTextBox = new TextBoxExt
            {
                Location = new Point(300, 50),
                Size = new Size(250, 25)
            };

            _licenseTypeComboBox = new ComboBoxAdv
            {
                Location = new Point(20, 120),
                Size = new Size(250, 25)
            };
            _licenseTypeComboBox.Items.AddRange(new[] { "CDL", "Passenger", "Regular" });

            _cdlExpiryDatePicker = new SfDateTimeEdit
            {
                Location = new Point(300, 120),
                Size = new Size(250, 25),
                Value = DateTime.Now.AddYears(1)
            };

            _phoneTextBox = new TextBoxExt
            {
                Location = new Point(20, 190),
                Size = new Size(250, 25)
            };

            _emailTextBox = new TextBoxExt
            {
                Location = new Point(300, 190),
                Size = new Size(250, 25)
            };

            _statusComboBox = new ComboBoxAdv
            {
                Location = new Point(20, 260),
                Size = new Size(250, 25)
            };
            _statusComboBox.Items.AddRange(new[] { "Active", "Inactive", "On Leave", "Terminated" });

            _notesTextBox = new TextBoxExt
            {
                Location = new Point(20, 330),
                Size = new Size(530, 100),
                Multiline = true
            };

            _mainPanel.Controls.AddRange(new Control[] {
                _firstNameTextBox, _lastNameTextBox, _licenseTypeComboBox, _cdlExpiryDatePicker,
                _phoneTextBox, _emailTextBox, _statusComboBox, _notesTextBox
            });
        }

        private void LayoutControls()
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60
            };

            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(375, 15),
                Size = new Size(80, 30)
            };

            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(470, 15),
                Size = new Size(80, 30)
            };

            buttonPanel.Controls.AddRange(new Control[] { _saveButton, _cancelButton });
            this.Controls.Add(buttonPanel);
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        private void PopulateFields(Driver driver)
        {
            _firstNameTextBox.Text = driver.FirstName ?? "";
            _lastNameTextBox.Text = driver.LastName ?? "";
            _licenseTypeComboBox.Text = driver.DriversLicenseType ?? "CDL";
            _cdlExpiryDatePicker.Value = driver.CDLExpirationDate ?? DateTime.Now.AddYears(1);
            _phoneTextBox.Text = driver.DriverPhone ?? "";
            _emailTextBox.Text = driver.DriverEmail ?? "";
            _statusComboBox.Text = driver.Status ?? "Active";
            _notesTextBox.Text = driver.Notes ?? "";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                Driver.FirstName = _firstNameTextBox.Text;
                Driver.LastName = _lastNameTextBox.Text;
                Driver.DriversLicenseType = _licenseTypeComboBox.Text;
                Driver.CDLExpirationDate = _cdlExpiryDatePicker.Value;
                Driver.DriverPhone = _phoneTextBox.Text;
                Driver.DriverEmail = _emailTextBox.Text;
                Driver.Status = _statusComboBox.Text;
                Driver.Notes = _notesTextBox.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving driver: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

