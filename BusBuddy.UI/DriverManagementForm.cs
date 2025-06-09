using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Business;
using BusBuddy.Data;

namespace BusBuddy.UI
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
        private List<Driver> _drivers;
        
        // Fields for add/edit
        private Panel _editPanel;
        private TextBox _driverNameTextBox;
        private TextBox _driverPhoneTextBox;
        private TextBox _driverEmailTextBox;
        private TextBox _addressTextBox;
        private TextBox _cityTextBox;
        private TextBox _stateTextBox;
        private TextBox _zipTextBox;
        private ComboBox _licenseTypeComboBox;
        private CheckBox _trainingCompleteCheckBox;
        private Button _saveButton;
        private Button _cancelButton;
        
        private Driver _currentDriver;
        private bool _isEditing = false;
        
        public DriverManagementForm()
        {
            _driverRepository = new DriverRepository();
            InitializeComponent();
            LoadDrivers();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Driver Management";
            
            // Create main grid
            _driverGrid = CreateDataGridView(20, 60, 750, 300);
            _driverGrid.CellDoubleClick += (s, e) => EditSelectedDriver();
            _driverGrid.SelectionChanged += DriverGrid_SelectionChanged;
            
            // Create buttons
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewDriver());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedDriver());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedDriver());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewDriverDetails());
            
            // Create search box
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchDrivers());
            
            // Initialize edit panel (hidden initially)
            InitializeEditPanel();
            
            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }
        
        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.BorderStyle = BorderStyle.FixedSingle;
            _editPanel.Location = new System.Drawing.Point(20, 370);
            _editPanel.Size = new System.Drawing.Size(750, 180);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);
            
            // Left column
            CreateLabel("Driver Name:", 10, 15, _editPanel);
            _driverNameTextBox = CreateTextBox(130, 10, 200, _editPanel);
            
            CreateLabel("Phone:", 10, 45, _editPanel);
            _driverPhoneTextBox = CreateTextBox(130, 40, 200, _editPanel);
            
            CreateLabel("Email:", 10, 75, _editPanel);
            _driverEmailTextBox = CreateTextBox(130, 70, 200, _editPanel);
            
            CreateLabel("Address:", 10, 105, _editPanel);
            _addressTextBox = CreateTextBox(130, 100, 200, _editPanel);
            
            // Right column
            CreateLabel("City:", 350, 15, _editPanel);
            _cityTextBox = CreateTextBox(470, 10, 200, _editPanel);
            
            CreateLabel("State:", 350, 45, _editPanel);
            _stateTextBox = CreateTextBox(470, 40, 200, _editPanel);
            
            CreateLabel("Zip:", 350, 75, _editPanel);
            _zipTextBox = CreateTextBox(470, 70, 200, _editPanel);
            
            CreateLabel("License Type:", 350, 105, _editPanel);
            _licenseTypeComboBox = CreateComboBox(470, 100, 200, _editPanel);
            _licenseTypeComboBox.Items.AddRange(new object[] { "Class A CDL", "Class B CDL", "Class C CDL", "Regular License" });
            
            _trainingCompleteCheckBox = CreateCheckBox("Training Complete", 470, 135, _editPanel);
            
            // Buttons
            _saveButton = CreateButton("Save", 550, 145, (s, e) => SaveDriver(), _editPanel);
            _cancelButton = CreateButton("Cancel", 660, 145, (s, e) => CancelEdit(), _editPanel);
        }
        
        private Label CreateLabel(string text, int x, int y, Control? parent = null)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new System.Drawing.Point(x, y);
            label.AutoSize = true;
            
            if (parent != null)
                parent.Controls.Add(label);
            else
                this.Controls.Add(label);
                
            return label;
        }
        
        private TextBox CreateTextBox(int x, int y, int width, Control? parent = null)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new System.Drawing.Point(x, y);
            textBox.Size = new System.Drawing.Size(width, 23);
            
            if (parent != null)
                parent.Controls.Add(textBox);
            else
                this.Controls.Add(textBox);
                
            return textBox;
        }
        
        private ComboBox CreateComboBox(int x, int y, int width, Control? parent = null)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Location = new System.Drawing.Point(x, y);
            comboBox.Size = new System.Drawing.Size(width, 23);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            
            if (parent != null)
                parent.Controls.Add(comboBox);
            else
                this.Controls.Add(comboBox);
                
            return comboBox;
        }
        
        private CheckBox CreateCheckBox(string text, int x, int y, Control? parent = null)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Text = text;
            checkBox.Location = new System.Drawing.Point(x, y);
            checkBox.AutoSize = true;
            
            if (parent != null)
                parent.Controls.Add(checkBox);
            else
                this.Controls.Add(checkBox);
                
            return checkBox;
        }
        
        private Button CreateButton(string text, int x, int y, EventHandler? clickHandler = null, Control? parent = null)
        {
            Button button = new Button();
            button.Text = text;
            button.Location = new System.Drawing.Point(x, y);
            button.Size = new System.Drawing.Size(100, 30);
            
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            
            if (parent != null)
                parent.Controls.Add(button);
            else
                this.Controls.Add(button);
                
            return button;
        }
        
        private void LoadDrivers()
        {
            try
            {
                _drivers = _driverRepository.GetAllDrivers();
                
                _driverGrid.DataSource = null;
                _driverGrid.DataSource = _drivers;
                
                // Configure columns
                if (_driverGrid.Columns.Count > 0)
                {
                    _driverGrid.Columns["DriverID"].HeaderText = "ID";
                    _driverGrid.Columns["DriverName"].HeaderText = "Name";
                    _driverGrid.Columns["DriverPhone"].HeaderText = "Phone";
                    _driverGrid.Columns["DriverEmail"].HeaderText = "Email";
                    _driverGrid.Columns["DriversLicenseType"].HeaderText = "License Type";
                    _driverGrid.Columns["TrainingComplete"].HeaderText = "Training Complete";
                    
                    // Hide NavigationProperties
                    if (_driverGrid.Columns.Contains("AMRoutes"))
                        _driverGrid.Columns["AMRoutes"].Visible = false;
                    if (_driverGrid.Columns.Contains("PMRoutes"))
                        _driverGrid.Columns["PMRoutes"].Visible = false;
                    if (_driverGrid.Columns.Contains("Activities"))
                        _driverGrid.Columns["Activities"].Visible = false;
                    if (_driverGrid.Columns.Contains("ScheduledActivities"))
                        _driverGrid.Columns["ScheduledActivities"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading drivers: {ex.Message}");
            }
        }
        
        private void DriverGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _driverGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }
        
        private void AddNewDriver()
        {
            _isEditing = false;
            _currentDriver = new Driver();
            
            // Clear all fields
            _driverNameTextBox.Text = string.Empty;
            _driverPhoneTextBox.Text = string.Empty;
            _driverEmailTextBox.Text = string.Empty;
            _addressTextBox.Text = string.Empty;
            _cityTextBox.Text = string.Empty;
            _stateTextBox.Text = string.Empty;
            _zipTextBox.Text = string.Empty;
            _licenseTypeComboBox.SelectedIndex = -1;
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
            
            // Populate fields
            _driverNameTextBox.Text = _currentDriver.DriverName;
            _driverPhoneTextBox.Text = _currentDriver.DriverPhone;
            _driverEmailTextBox.Text = _currentDriver.DriverEmail;
            _addressTextBox.Text = _currentDriver.Address;
            _cityTextBox.Text = _currentDriver.City;
            _stateTextBox.Text = _currentDriver.State;
            _zipTextBox.Text = _currentDriver.Zip;
            _licenseTypeComboBox.SelectedItem = _currentDriver.DriversLicenseType;
            _trainingCompleteCheckBox.Checked = _currentDriver.TrainingComplete != 0;
            
            _editPanel.Visible = true;
        }
        
        private void DeleteSelectedDriver()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
                
            if (!ConfirmDelete())
                return;
                
            try
            {
                int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
                bool success = _driverRepository.DeleteDriver(selectedId);
                
                if (success)
                {
                    ShowSuccessMessage("Driver deleted successfully.");
                    LoadDrivers();
                }
                else
                {
                    ShowErrorMessage("Could not delete the driver. They may be referenced by other records.");
                }
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
            var driverDetails = _databaseService.GetDriverDetails(selectedId);
            
            if (driverDetails != null)
            {
                // TODO: Create and show a DriverDetailsForm
                MessageBox.Show($"Driver Details for {driverDetails.Driver.DriverName}\n" +
                                $"AM Routes: {driverDetails.AMRoutes.Count}\n" +
                                $"PM Routes: {driverDetails.PMRoutes.Count}\n" +
                                $"Activities: {driverDetails.Activities.Count}\n" +
                                $"Scheduled Activities: {driverDetails.ScheduledActivities.Count}",
                                "Driver Details",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load driver details.");
            }
        }
        
        private void SaveDriver()
        {
            if (!ValidateDriverForm())
                return;
                
            try
            {
                // Transfer form values to driver object
                _currentDriver.DriverName = _driverNameTextBox.Text;
                _currentDriver.DriverPhone = _driverPhoneTextBox.Text;
                _currentDriver.DriverEmail = _driverEmailTextBox.Text;
                _currentDriver.Address = _addressTextBox.Text;
                _currentDriver.City = _cityTextBox.Text;
                _currentDriver.State = _stateTextBox.Text;
                _currentDriver.Zip = _zipTextBox.Text;
                _currentDriver.DriversLicenseType = _licenseTypeComboBox.SelectedItem?.ToString();
                _currentDriver.TrainingComplete = _trainingCompleteCheckBox.Checked ? 1 : 0;
                
                bool success;
                if (_isEditing)
                {
                    success = _driverRepository.UpdateDriver(_currentDriver);
                    if (success)
                    {
                        ShowSuccessMessage("Driver updated successfully.");
                    }
                }                else
                {
                    int result = _driverRepository.AddDriver(_currentDriver);
                    success = result > 0;
                    if (success)
                    {
                        ShowSuccessMessage("Driver added successfully.");
                    }
                }
                
                if (success)
                {
                    _editPanel.Visible = false;
                    LoadDrivers();
                }
                else
                {
                    ShowErrorMessage("Failed to save driver. Please check your input and try again.");
                }
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
            }
            
            List<Driver> filteredDrivers = _drivers.FindAll(d => 
                (d.DriverName?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverPhone?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverEmail?.ToLower().Contains(searchTerm) == true) ||
                (d.Address?.ToLower().Contains(searchTerm) == true) ||
                (d.City?.ToLower().Contains(searchTerm) == true) ||
                (d.State?.ToLower().Contains(searchTerm) == true) ||
                (d.Zip?.ToLower().Contains(searchTerm) == true) ||
                (d.DriversLicenseType?.ToLower().Contains(searchTerm) == true)
            );
            
            _driverGrid.DataSource = null;
            _driverGrid.DataSource = filteredDrivers;
        }
        
        private bool ValidateDriverForm()
        {
            _errorProvider.Clear();
            bool isValid = true;
            
            // Validate required fields
            if (!FormValidator.ValidateRequiredField(_driverNameTextBox, "Driver Name", _errorProvider))
                isValid = false;
                
            // Validate phone number
            if (!FormValidator.ValidatePhoneNumber(_driverPhoneTextBox, "Phone", _errorProvider))
                isValid = false;
                
            // Validate email
            if (!FormValidator.ValidateEmail(_driverEmailTextBox, "Email", _errorProvider))
                isValid = false;
                
            return isValid;
        }
    }
}
