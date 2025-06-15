using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Views
{
    public partial class DriverEditForm : StandardDataForm
    {
        private MaterialTextBox2 _firstNameTextBox;
        private MaterialTextBox2 _lastNameTextBox;
        private MaterialTextBox2 _phoneTextBox;
        private MaterialTextBox2 _emailTextBox;
        private MaterialTextBox2 _addressTextBox;
        private MaterialTextBox2 _cityTextBox;
        private MaterialTextBox2 _stateTextBox;
        private MaterialTextBox2 _zipTextBox;
        private MaterialTextBox2 _notesTextBox;
        private MaterialComboBox _licenseTypeComboBox;
        private MaterialComboBox _statusComboBox;
        private MaterialCheckbox _trainingCompleteCheckBox;
        private DateTimePicker _cdlExpirationDatePicker;
        private MaterialButton _saveButton;
        private MaterialButton _cancelButton;

        public Driver? Driver { get; private set; }

        public DriverEditForm() : this(null)
        {
        }

        public DriverEditForm(Driver? driver)
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
            this.ClientSize = new Size(600, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 14,
                Padding = new Padding(20),
                AutoSize = true
            };

            // Configure column styles
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Configure row styles
            for (int i = 0; i < 14; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // First Name
            var firstNameLabel = CreateLabel("First Name:", Color.FromArgb(33, 33, 33));
            _firstNameTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter first name",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(firstNameLabel, 0, 0);
            tableLayout.Controls.Add(_firstNameTextBox, 1, 0);

            // Last Name
            var lastNameLabel = CreateLabel("Last Name:", Color.FromArgb(33, 33, 33));
            _lastNameTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter last name",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(lastNameLabel, 0, 1);
            tableLayout.Controls.Add(_lastNameTextBox, 1, 1);

            // Phone
            var phoneLabel = CreateLabel("Phone:", Color.FromArgb(33, 33, 33));
            _phoneTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter phone number",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(phoneLabel, 0, 2);
            tableLayout.Controls.Add(_phoneTextBox, 1, 2);

            // Email
            var emailLabel = CreateLabel("Email:", Color.FromArgb(33, 33, 33));
            _emailTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter email address",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(emailLabel, 0, 3);
            tableLayout.Controls.Add(_emailTextBox, 1, 3);

            // Address
            var addressLabel = CreateLabel("Address:", Color.FromArgb(33, 33, 33));
            _addressTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter street address",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(addressLabel, 0, 4);
            tableLayout.Controls.Add(_addressTextBox, 1, 4);

            // City
            var cityLabel = CreateLabel("City:", Color.FromArgb(33, 33, 33));
            _cityTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter city",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(cityLabel, 0, 5);
            tableLayout.Controls.Add(_cityTextBox, 1, 5);

            // State
            var stateLabel = CreateLabel("State:", Color.FromArgb(33, 33, 33));
            _stateTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter state",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(stateLabel, 0, 6);
            tableLayout.Controls.Add(_stateTextBox, 1, 6);

            // ZIP Code
            var zipLabel = CreateLabel("ZIP Code:", Color.FromArgb(33, 33, 33));
            _zipTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter ZIP code",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(zipLabel, 0, 7);
            tableLayout.Controls.Add(_zipTextBox, 1, 7);

            // License Type
            var licenseTypeLabel = CreateLabel("License Type:", Color.FromArgb(33, 33, 33));
            _licenseTypeComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select license type",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            _licenseTypeComboBox.Items.AddRange(new string[]
            {
                "CDL",
                "Passenger",
                "Regular",
                "Chauffeur",
                "Other"
            });

            tableLayout.Controls.Add(licenseTypeLabel, 0, 8);
            tableLayout.Controls.Add(_licenseTypeComboBox, 1, 8);

            // CDL Expiration Date
            var expirationLabel = CreateLabel("CDL Expires:", Color.FromArgb(33, 33, 33));
            _cdlExpirationDatePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(expirationLabel, 0, 9);
            tableLayout.Controls.Add(_cdlExpirationDatePicker, 1, 9);

            // Status
            var statusLabel = CreateLabel("Status:", Color.FromArgb(33, 33, 33));
            _statusComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select status",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            _statusComboBox.Items.AddRange(new string[]
            {
                "Active",
                "Inactive",
                "On Leave",
                "Terminated",
                "Retired"
            });

            tableLayout.Controls.Add(statusLabel, 0, 10);
            tableLayout.Controls.Add(_statusComboBox, 1, 10);

            // Training Complete
            var trainingLabel = CreateLabel("Training:", Color.FromArgb(33, 33, 33));
            _trainingCompleteCheckBox = new MaterialCheckbox
            {
                Text = "Training completed",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 36,
                Font = new Font("Roboto", 10F)
            };

            tableLayout.Controls.Add(trainingLabel, 0, 11);
            tableLayout.Controls.Add(_trainingCompleteCheckBox, 1, 11);

            // Notes
            var notesLabel = CreateLabel("Notes:", Color.FromArgb(33, 33, 33));
            _notesTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Additional notes (optional)",
                Font = new Font("Roboto", 11F),
                Height = 80
            };

            tableLayout.Controls.Add(notesLabel, 0, 12);
            tableLayout.Controls.Add(_notesTextBox, 1, 12);

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
                Margin = new Padding(5)
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new MaterialButton
            {
                Text = "CANCEL",
                Dock = DockStyle.Fill,
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,
                Margin = new Padding(5)
            };
            _cancelButton.Click += CancelButton_Click;

            buttonPanel.Controls.Add(_saveButton, 0, 0);
            buttonPanel.Controls.Add(_cancelButton, 1, 0);

            tableLayout.Controls.Add(buttonPanel, 0, 13);
            tableLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(tableLayout);

            // Set tab order
            _firstNameTextBox.TabIndex = 0;
            _lastNameTextBox.TabIndex = 1;
            _phoneTextBox.TabIndex = 2;
            _emailTextBox.TabIndex = 3;
            _addressTextBox.TabIndex = 4;
            _cityTextBox.TabIndex = 5;
            _stateTextBox.TabIndex = 6;
            _zipTextBox.TabIndex = 7;
            _licenseTypeComboBox.TabIndex = 8;
            _cdlExpirationDatePicker.TabIndex = 9;
            _statusComboBox.TabIndex = 10;
            _trainingCompleteCheckBox.TabIndex = 11;
            _notesTextBox.TabIndex = 12;
            _saveButton.TabIndex = 13;
            _cancelButton.TabIndex = 14;
        }

        private void PopulateFields(Driver driver)
        {
            _firstNameTextBox.Text = driver.FirstName ?? "";
            _lastNameTextBox.Text = driver.LastName ?? "";
            _phoneTextBox.Text = driver.DriverPhone ?? "";
            _emailTextBox.Text = driver.DriverEmail ?? "";
            _addressTextBox.Text = driver.Address ?? "";
            _cityTextBox.Text = driver.City ?? "";
            _stateTextBox.Text = driver.State ?? "";
            _zipTextBox.Text = driver.Zip ?? "";
            _licenseTypeComboBox.Text = driver.DriversLicenseType ?? "";
            _statusComboBox.Text = driver.Status ?? "";
            _trainingCompleteCheckBox.Checked = driver.IsTrainingComplete;
            _notesTextBox.Text = driver.Notes ?? "";

            if (driver.CDLExpirationDate.HasValue)
                _cdlExpirationDatePicker.Value = driver.CDLExpirationDate.Value;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var driver = Driver ?? new Driver();

                driver.FirstName = _firstNameTextBox.Text?.Trim();
                driver.LastName = _lastNameTextBox.Text?.Trim();
                driver.DriverName = $"{driver.FirstName} {driver.LastName}".Trim();
                driver.DriverPhone = _phoneTextBox.Text?.Trim();
                driver.DriverEmail = _emailTextBox.Text?.Trim();
                driver.Address = _addressTextBox.Text?.Trim();
                driver.City = _cityTextBox.Text?.Trim();
                driver.State = _stateTextBox.Text?.Trim();
                driver.Zip = _zipTextBox.Text?.Trim();
                driver.DriversLicenseType = _licenseTypeComboBox.Text?.Trim();
                driver.Status = _statusComboBox.Text?.Trim();
                driver.IsTrainingComplete = _trainingCompleteCheckBox.Checked;
                driver.Notes = _notesTextBox.Text?.Trim();
                driver.CDLExpirationDate = _cdlExpirationDatePicker.Value;

                Driver = driver;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving driver: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override bool ValidateForm()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_firstNameTextBox.Text))
                errors.Add("First name is required.");

            if (string.IsNullOrWhiteSpace(_lastNameTextBox.Text))
                errors.Add("Last name is required.");

            if (string.IsNullOrWhiteSpace(_licenseTypeComboBox.Text))
                errors.Add("License type is required.");

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(_emailTextBox.Text))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(_emailTextBox.Text);
                    if (addr.Address != _emailTextBox.Text)
                        errors.Add("Email address format is invalid.");
                }
                catch
                {
                    errors.Add("Email address format is invalid.");
                }
            }

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
                _firstNameTextBox?.Dispose();
                _lastNameTextBox?.Dispose();
                _phoneTextBox?.Dispose();
                _emailTextBox?.Dispose();
                _addressTextBox?.Dispose();
                _cityTextBox?.Dispose();
                _stateTextBox?.Dispose();
                _zipTextBox?.Dispose();
                _notesTextBox?.Dispose();
                _licenseTypeComboBox?.Dispose();
                _statusComboBox?.Dispose();
                _trainingCompleteCheckBox?.Dispose();
                _cdlExpirationDatePicker?.Dispose();
                _saveButton?.Dispose();
                _cancelButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
