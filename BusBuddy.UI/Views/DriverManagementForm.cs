using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class DriverManagementForm : BaseDataForm
    {
        private readonly IDriverRepository _driverRepository;
        private DataGridView _driverGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private TextBox _searchBox;
        private Button _searchButton;
        private List<Driver> _drivers = new List<Driver>();

        // Fields for add/edit
        private Panel _editPanel = null!;
        private TextBox _driverNameTextBox = null!;
        private TextBox _driverPhoneTextBox = null!;
        private TextBox _driverEmailTextBox = null!;
        private TextBox _addressTextBox = null!;
        private TextBox _cityTextBox = null!;
        private TextBox _stateTextBox = null!;
        private TextBox _zipTextBox = null!;
        private ComboBox _driversLicenseTypeComboBox = null!;
        private CheckBox _trainingCompleteCheckBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Driver? _currentDriver;
        private bool _isEditing = false;

        public DriverManagementForm() : this(new DriverRepository()) { }

        public DriverManagementForm(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadDrivers();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Driver Management"
            this.Text = "Driver Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewDriver());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedDriver());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedDriver());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewDriverDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchDrivers());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _driverGrid = new DataGridView();
            _driverGrid.Location = new System.Drawing.Point(20, 60);
            _driverGrid.Size = new System.Drawing.Size(1150, 650);
            _driverGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _driverGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _driverGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _driverGrid.ReadOnly = true;
            _driverGrid.AllowUserToAddRows = false;
            _driverGrid.AllowUserToDeleteRows = false;
            _driverGrid.MultiSelect = false;
            _driverGrid.AllowUserToResizeColumns = true;
            _driverGrid.AllowUserToResizeRows = true;
            _driverGrid.ScrollBars = ScrollBars.Both;
            _driverGrid.DataBindingComplete += (s, e) => {
                if (_driverGrid.Columns.Contains("DriverID"))
                    _driverGrid.Columns["DriverID"].Visible = false;
            };
            this.Controls.Add(_driverGrid);
            _driverGrid.CellDoubleClick += (s, e) => EditSelectedDriver();
            _driverGrid.SelectionChanged += DriverGrid_SelectionChanged;

            // Initialize edit panel (1150x120, y=730, hidden)
            InitializeEditPanel();

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            // Create edit panel (1150x120, y=730, hidden)
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 730);
            _editPanel.Size = new System.Drawing.Size(1150, 120);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);

            // Driver form-specific fields: Name, Phone, Email, Address, City, State, Zip, License Type, Training Complete
            var nameLabel = CreateLabel("Name:", 10, 15);
            _editPanel.Controls.Add(nameLabel);
            _driverNameTextBox = new TextBox();
            _driverNameTextBox.Location = new System.Drawing.Point(60, 10);
            _driverNameTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_driverNameTextBox);

            var phoneLabel = CreateLabel("Phone:", 280, 15);
            _editPanel.Controls.Add(phoneLabel);
            _driverPhoneTextBox = new TextBox();
            _driverPhoneTextBox.Location = new System.Drawing.Point(330, 10);
            _driverPhoneTextBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_driverPhoneTextBox);

            var emailLabel = CreateLabel("Email:", 500, 15);
            _editPanel.Controls.Add(emailLabel);
            _driverEmailTextBox = new TextBox();
            _driverEmailTextBox.Location = new System.Drawing.Point(550, 10);
            _driverEmailTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_driverEmailTextBox);

            var addressLabel = CreateLabel("Address:", 10, 55);
            _editPanel.Controls.Add(addressLabel);
            _addressTextBox = new TextBox();
            _addressTextBox.Location = new System.Drawing.Point(70, 50);
            _addressTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_addressTextBox);

            var cityLabel = CreateLabel("City:", 290, 55);
            _editPanel.Controls.Add(cityLabel);
            _cityTextBox = new TextBox();
            _cityTextBox.Location = new System.Drawing.Point(330, 50);
            _cityTextBox.Size = new System.Drawing.Size(120, 23);
            _editPanel.Controls.Add(_cityTextBox);

            var stateLabel = CreateLabel("State:", 470, 55);
            _editPanel.Controls.Add(stateLabel);
            _stateTextBox = new TextBox();
            _stateTextBox.Location = new System.Drawing.Point(510, 50);
            _stateTextBox.Size = new System.Drawing.Size(60, 23);
            _editPanel.Controls.Add(_stateTextBox);

            var zipLabel = CreateLabel("ZIP:", 590, 55);
            _editPanel.Controls.Add(zipLabel);
            _zipTextBox = new TextBox();
            _zipTextBox.Location = new System.Drawing.Point(620, 50);
            _zipTextBox.Size = new System.Drawing.Size(80, 23);
            _editPanel.Controls.Add(_zipTextBox);

            var licenseLabel = CreateLabel("License:", 720, 55);
            _editPanel.Controls.Add(licenseLabel);
            _driversLicenseTypeComboBox = new ComboBox();
            _driversLicenseTypeComboBox.Location = new System.Drawing.Point(780, 50);
            _driversLicenseTypeComboBox.Size = new System.Drawing.Size(100, 23);
            _driversLicenseTypeComboBox.Items.AddRange(new object[] { "CDL", "Regular", "Commercial" });
            _editPanel.Controls.Add(_driversLicenseTypeComboBox);

            _trainingCompleteCheckBox = new CheckBox();
            _trainingCompleteCheckBox.Text = "Training Complete";
            _trainingCompleteCheckBox.Location = new System.Drawing.Point(900, 50);
            _trainingCompleteCheckBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_trainingCompleteCheckBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveDriver());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void DriverGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _driverGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void LoadDrivers()
        {
            try
            {
                _drivers = _driverRepository.GetAllDrivers();
                _driverGrid.DataSource = null;
                _driverGrid.DataSource = _drivers;
                if (_driverGrid.Columns.Count > 0)
                {
                    _driverGrid.Columns["DriverID"].HeaderText = "ID";                    _driverGrid.Columns["DriverName"].HeaderText = "Name";
                    _driverGrid.Columns["DriverPhone"].HeaderText = "Phone";
                    _driverGrid.Columns["DriverEmail"].HeaderText = "Email";
                    _driverGrid.Columns["DriversLicenseType"].HeaderText = "License";
                    _driverGrid.Columns["TrainingComplete"].HeaderText = "Training";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading drivers: {ex.Message}");
            }
        }

        private void AddNewDriver()
        {
            _isEditing = false;
            _currentDriver = new Driver();
            _driverNameTextBox.Text = string.Empty;
            _driverPhoneTextBox.Text = string.Empty;
            _driverEmailTextBox.Text = string.Empty;
            _addressTextBox.Text = string.Empty;
            _cityTextBox.Text = string.Empty;            _stateTextBox.Text = string.Empty;
            _zipTextBox.Text = string.Empty;
            _driversLicenseTypeComboBox.SelectedIndex = -1;
            _trainingCompleteCheckBox.Checked = false;
            _editPanel.Visible = true;
        }

        private void EditSelectedDriver()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
            _isEditing = true;
            int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
            _currentDriver = _driverRepository.GetDriverById(selectedId);
            if (_currentDriver == null)
            {
                ShowErrorMessage("Could not find the selected driver.");
                return;
            }
            _driverNameTextBox.Text = _currentDriver.DriverName ?? string.Empty;
            _driverPhoneTextBox.Text = _currentDriver.DriverPhone ?? string.Empty;
            _driverEmailTextBox.Text = _currentDriver.DriverEmail ?? string.Empty;
            _addressTextBox.Text = _currentDriver.Address ?? string.Empty;            _cityTextBox.Text = _currentDriver.City ?? string.Empty;
            _stateTextBox.Text = _currentDriver.State ?? string.Empty;
            _zipTextBox.Text = _currentDriver.Zip ?? string.Empty;
            _driversLicenseTypeComboBox.SelectedItem = _currentDriver.DriversLicenseType ?? string.Empty;
            _trainingCompleteCheckBox.Checked = _currentDriver.IsTrainingComplete;
            _editPanel.Visible = true;
        }

        private void DeleteSelectedDriver()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _driverRepository.DeleteDriver(selectedId);
                LoadDrivers();
                ShowSuccessMessage("Driver deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting driver: {ex.Message}");
            }
        }

        private void ViewDriverDetails()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
            var driver = _driverRepository.GetDriverById(selectedId);
            if (driver != null)
            {
                MessageBox.Show($"Driver Details:\nName: {driver.DriverName}\nPhone: {driver.DriverPhone}\nEmail: {driver.DriverEmail}\nAddress: {driver.Address}, {driver.City}, {driver.State} {driver.Zip}\nLicense: {driver.DriversLicenseType}\nTraining Complete: {driver.IsTrainingComplete}",
                    "Driver Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load driver details.");
            }
        }

        private void SaveDriver()
        {
            if (_currentDriver == null || !ValidateDriverForm())
                return;
            try
            {
                var driver = _currentDriver; // Null-checked above
                driver.DriverName = _driverNameTextBox.Text.Trim();
                driver.DriverPhone = _driverPhoneTextBox.Text.Trim();
                driver.DriverEmail = _driverEmailTextBox.Text.Trim();
                driver.Address = _addressTextBox.Text.Trim();
                driver.City = _cityTextBox.Text.Trim();
                driver.State = _stateTextBox.Text.Trim();
                driver.Zip = _zipTextBox.Text.Trim();
                driver.DriversLicenseType = _driversLicenseTypeComboBox.SelectedItem?.ToString();
                driver.IsTrainingComplete = _trainingCompleteCheckBox.Checked;

                if (_isEditing)
                {
                    _driverRepository.UpdateDriver(driver);
                    ShowSuccessMessage("Driver updated successfully.");
                }
                else
                {
                    _driverRepository.AddDriver(driver);
                    ShowSuccessMessage("Driver added successfully.");
                }
                LoadDrivers();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving driver: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void SearchDrivers()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadDrivers();
                return;
            }            List<Driver> filtered = _drivers.FindAll(d =>
                (d.DriverName?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverPhone?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverEmail?.ToLower().Contains(searchTerm) == true) ||
                (d.DriversLicenseType?.ToLower().Contains(searchTerm) == true)
            );
            _driverGrid.DataSource = null;
            _driverGrid.DataSource = filtered;
        }

        private bool ValidateDriverForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (string.IsNullOrWhiteSpace(_driverNameTextBox.Text))
            {
                _errorProvider.SetError(_driverNameTextBox, "Enter a driver name");
                valid = false;
            }            if (_driversLicenseTypeComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_driversLicenseTypeComboBox, "Select a license type");
                valid = false;
            }
            return valid;
        }
    }
}
