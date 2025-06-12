using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;

namespace BusBuddy.UI.Views
{
    public class MaintenanceManagementForm : BaseDataForm
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private DataGridView _maintenanceGrid;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _detailsButton;
        private List<Maintenance> _maintenances;
        // Add/edit fields
        private Panel _editPanel;
        private DateTimePicker _datePicker;
        private ComboBox _vehicleComboBox;
        private TextBox _odometerTextBox;
        private ComboBox _categoryComboBox;
        private TextBox _vendorTextBox;
        private TextBox _costTextBox;
        private TextBox _descriptionTextBox;
        private Button _saveButton;
        private Button _cancelButton;
        private Maintenance _currentMaintenance;
        private bool _isEditing = false;

        public MaintenanceManagementForm()
        {
            _maintenanceRepository = new MaintenanceRepository();
            InitializeComponent();
            LoadMaintenances();
        }

        private void InitializeComponent()
        {
            this.Text = "Maintenance Management";
            this.Size = new Size(1200, 900);
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewMaintenance());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedMaintenance());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedMaintenance());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewMaintenanceDetails());
            _maintenanceGrid = new DataGridView();
            _maintenanceGrid.Location = new System.Drawing.Point(20, 60);
            _maintenanceGrid.Size = new System.Drawing.Size(1150, 650);
            _maintenanceGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _maintenanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _maintenanceGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _maintenanceGrid.AllowUserToResizeColumns = true;
            _maintenanceGrid.AllowUserToResizeRows = true;
            _maintenanceGrid.ScrollBars = ScrollBars.Both;
            _maintenanceGrid.DataBindingComplete += (s, e) => {
                if (_maintenanceGrid.Columns.Contains("MaintenanceID"))
                    _maintenanceGrid.Columns["MaintenanceID"].Visible = false;
            };
            this.Controls.Add(_maintenanceGrid);
            _maintenanceGrid.CellDoubleClick += (s, e) => EditSelectedMaintenance();
            _maintenanceGrid.SelectionChanged += MaintenanceGrid_SelectionChanged;
            InitializeEditPanel();
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void InitializeEditPanel()
        {
            _editPanel = new Panel();
            _editPanel.Location = new System.Drawing.Point(20, 730);
            _editPanel.Size = new System.Drawing.Size(1150, 140);
            _editPanel.Visible = false;
            this.Controls.Add(_editPanel);
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(70, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_datePicker);
            var vehicleLabel = CreateLabel("Vehicle:", 250, 15);
            _editPanel.Controls.Add(vehicleLabel);
            _vehicleComboBox = new ComboBox();
            _vehicleComboBox.Location = new System.Drawing.Point(320, 10);
            _vehicleComboBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_vehicleComboBox);
            var odoLabel = CreateLabel("Odometer:", 500, 15);
            _editPanel.Controls.Add(odoLabel);
            _odometerTextBox = new TextBox();
            _odometerTextBox.Location = new System.Drawing.Point(580, 10);
            _odometerTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_odometerTextBox);
            var categoryLabel = CreateLabel("Category:", 700, 15);
            _editPanel.Controls.Add(categoryLabel);
            _categoryComboBox = new ComboBox();
            _categoryComboBox.Location = new System.Drawing.Point(780, 10);
            _categoryComboBox.Size = new System.Drawing.Size(150, 23);
            _categoryComboBox.Items.AddRange(new object[] { "Tires", "Windshield", "Alignment", "Mechanical", "Car Wash", "Cleaning", "Accessory Install" });
            _editPanel.Controls.Add(_categoryComboBox);
            var vendorLabel = CreateLabel("Vendor:", 10, 55);
            _editPanel.Controls.Add(vendorLabel);
            _vendorTextBox = new TextBox();
            _vendorTextBox.Location = new System.Drawing.Point(70, 50);
            _vendorTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_vendorTextBox);
            var costLabel = CreateLabel("Repair Cost:", 300, 55);
            _editPanel.Controls.Add(costLabel);
            _costTextBox = new TextBox();
            _costTextBox.Location = new System.Drawing.Point(390, 50);
            _costTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_costTextBox);
            var descLabel = CreateLabel("Description:", 520, 55);
            _editPanel.Controls.Add(descLabel);
            _descriptionTextBox = new TextBox();
            _descriptionTextBox.Location = new System.Drawing.Point(610, 50);
            _descriptionTextBox.Size = new System.Drawing.Size(320, 23);
            _editPanel.Controls.Add(_descriptionTextBox);
            _saveButton = CreateButton("Save", 980, 50, (s, e) => SaveMaintenance());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 1080, 50, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void LoadMaintenances()
        {
            try
            {
                _maintenances = _maintenanceRepository.GetAllMaintenanceRecords();
                _maintenanceGrid.DataSource = null;
                _maintenanceGrid.DataSource = _maintenances;
                if (_maintenanceGrid.Columns.Count > 0)
                {
                    _maintenanceGrid.Columns["MaintenanceID"].HeaderText = "ID";
                    _maintenanceGrid.Columns["Date"].HeaderText = "Date";
                    _maintenanceGrid.Columns["VehicleID"].HeaderText = "Vehicle";
                    _maintenanceGrid.Columns["OdometerReading"].HeaderText = "Odometer";
                    _maintenanceGrid.Columns["MaintenanceCompleted"].HeaderText = "Category";
                    _maintenanceGrid.Columns["Vendor"].HeaderText = "Vendor";
                    _maintenanceGrid.Columns["RepairCost"].HeaderText = "Cost";
                    _maintenanceGrid.Columns["Notes"].HeaderText = "Description";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading maintenance records: {ex.Message}");
            }
        }

        private void AddNewMaintenance()
        {
            _isEditing = false;
            _currentMaintenance = new Maintenance();
            _datePicker.Value = DateTime.Today;
            _vehicleComboBox.SelectedIndex = -1;
            _odometerTextBox.Text = string.Empty;
            _categoryComboBox.SelectedIndex = -1;
            _vendorTextBox.Text = string.Empty;
            _costTextBox.Text = string.Empty;
            _descriptionTextBox.Text = string.Empty;
            _editPanel.Visible = true;
        }

        private void EditSelectedMaintenance()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
                return;
            _isEditing = true;
            int selectedId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
            _currentMaintenance = _maintenanceRepository.GetMaintenanceById(selectedId);
            if (_currentMaintenance == null)
            {
                ShowErrorMessage("Could not find the selected maintenance record.");
                return;
            }
            _datePicker.Value = _currentMaintenance.Date ?? DateTime.Today;
            _vehicleComboBox.SelectedItem = _currentMaintenance.VehicleID?.ToString() ?? string.Empty;
            _odometerTextBox.Text = _currentMaintenance.OdometerReading?.ToString() ?? string.Empty;
            _categoryComboBox.SelectedItem = _currentMaintenance.MaintenanceCompleted ?? string.Empty;
            _vendorTextBox.Text = _currentMaintenance.Vendor ?? string.Empty;
            _costTextBox.Text = _currentMaintenance.RepairCost?.ToString() ?? string.Empty;
            _descriptionTextBox.Text = _currentMaintenance.Notes ?? string.Empty;
            _editPanel.Visible = true;
        }

        private void DeleteSelectedMaintenance()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _maintenanceRepository.DeleteMaintenance(selectedId);
                LoadMaintenances();
                ShowSuccessMessage("Maintenance record deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting maintenance record: {ex.Message}");
            }
        }

        private void ViewMaintenanceDetails()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
            var maintenance = _maintenanceRepository.GetMaintenanceById(selectedId);
            if (maintenance != null)
            {
                MessageBox.Show($"Maintenance Details:\nDate: {maintenance.Date}\nVehicle: {maintenance.VehicleID}\nOdometer: {maintenance.OdometerReading}\nCategory: {maintenance.MaintenanceCompleted}\nVendor: {maintenance.Vendor}\nCost: {maintenance.RepairCost}\nDescription: {maintenance.Notes}",
                    "Maintenance Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load maintenance details.");
            }
        }

        private void SaveMaintenance()
        {
            if (!ValidateMaintenanceForm())
                return;
            try
            {
                _currentMaintenance.Date = _datePicker.Value;
                _currentMaintenance.VehicleID = int.TryParse(_vehicleComboBox.SelectedItem?.ToString(), out int vid) ? vid : (int?)null;
                _currentMaintenance.OdometerReading = decimal.TryParse(_odometerTextBox.Text.Trim(), out decimal odo) ? odo : (decimal?)null;
                _currentMaintenance.MaintenanceCompleted = _categoryComboBox.SelectedItem?.ToString();
                _currentMaintenance.Vendor = _vendorTextBox.Text.Trim();
                _currentMaintenance.RepairCost = decimal.TryParse(_costTextBox.Text.Trim(), out decimal cost) ? cost : (decimal?)null;
                _currentMaintenance.Notes = _descriptionTextBox.Text.Trim();
                if (_isEditing)
                {
                    _maintenanceRepository.UpdateMaintenance(_currentMaintenance);
                    ShowSuccessMessage("Maintenance record updated successfully.");
                }
                else
                {
                    _maintenanceRepository.AddMaintenance(_currentMaintenance);
                    ShowSuccessMessage("Maintenance record added successfully.");
                }
                LoadMaintenances();
                _editPanel.Visible = false;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error saving maintenance record: {ex.Message}");
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
        }

        private void MaintenanceGrid_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = _maintenanceGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private bool ValidateMaintenanceForm()
        {
            _errorProvider.Clear();
            bool valid = true;
            if (_vehicleComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_vehicleComboBox, "Select a vehicle");
                valid = false;
            }
            if (_categoryComboBox.SelectedIndex < 0)
            {
                _errorProvider.SetError(_categoryComboBox, "Select a category");
                valid = false;
            }
            return valid;
        }
    }
}
