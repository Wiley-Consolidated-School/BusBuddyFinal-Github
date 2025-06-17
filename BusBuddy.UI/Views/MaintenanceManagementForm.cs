using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class MaintenanceManagementForm : StandardDataForm
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _maintenanceGrid;
        private Button? _addButton;
        private Button? _editButton;
        private Button? _deleteButton;
        private Button? _detailsButton;
        private MaterialSkin.Controls.MaterialTextBox _searchBox;
        private Button? _searchButton;
        private List<Maintenance> _maintenances = new List<Maintenance>();
        private List<Vehicle> _vehicles = new List<Vehicle>();

        // Fields for add/edit
        private Panel _editPanel = null!;
        private DateTimePicker _datePicker = null!;
        private ComboBox _vehicleComboBox = null!;
        private TextBox _odometerTextBox = null!;
        private ComboBox _categoryComboBox = null!;
        private TextBox _vendorTextBox = null!;
        private TextBox _costTextBox = null!;
        private TextBox _descriptionTextBox = null!;
        private Button _saveButton = null!;
        private Button _cancelButton = null!;
        private Maintenance? _currentMaintenance = null;
        private bool _isEditing = false;

        public MaintenanceManagementForm() : this(new MaintenanceRepository()) { }

        public MaintenanceManagementForm(IMaintenanceRepository maintenanceRepository)
        {
            _maintenanceRepository = maintenanceRepository ?? throw new ArgumentNullException(nameof(maintenanceRepository));
            _vehicleRepository = new VehicleRepository();
            InitializeComponent();
            LoadVehicles();
            LoadMaintenances();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Maintenance Management"
            this.Text = "Maintenance Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewMaintenance());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedMaintenance());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedMaintenance());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewMaintenanceDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchMaintenances());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _maintenanceGrid = new DataGridView();
            _maintenanceGrid.Location = new System.Drawing.Point(20, 60);
            _maintenanceGrid.Size = new System.Drawing.Size(1150, 650);
            _maintenanceGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _maintenanceGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _maintenanceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _maintenanceGrid.ReadOnly = true;
            _maintenanceGrid.AllowUserToAddRows = false;
            _maintenanceGrid.AllowUserToDeleteRows = false;
            _maintenanceGrid.MultiSelect = false;
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

            // Maintenance form-specific fields: Date, Vehicle, Odometer, Category, Vendor, Cost, Description
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new System.Drawing.Point(60, 10);
            _datePicker.Size = new System.Drawing.Size(150, 23);
            _datePicker.Value = DateTime.Today;
            _editPanel.Controls.Add(_datePicker);

            var vehicleLabel = CreateLabel("Vehicle:", 230, 15);
            _editPanel.Controls.Add(vehicleLabel);
            _vehicleComboBox = new ComboBox();
            _vehicleComboBox.Location = new System.Drawing.Point(290, 10);
            _vehicleComboBox.Size = new System.Drawing.Size(150, 23);
            _vehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_vehicleComboBox);

            var odometerLabel = CreateLabel("Odometer:", 460, 15);
            _editPanel.Controls.Add(odometerLabel);
            _odometerTextBox = new TextBox();
            _odometerTextBox.Location = new System.Drawing.Point(530, 10);
            _odometerTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_odometerTextBox);

            var categoryLabel = CreateLabel("Category:", 650, 15);
            _editPanel.Controls.Add(categoryLabel);
            _categoryComboBox = new ComboBox();
            _categoryComboBox.Location = new System.Drawing.Point(720, 10);
            _categoryComboBox.Size = new System.Drawing.Size(120, 23);
            _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _categoryComboBox.Items.AddRange(new object[] { "Routine", "Repair", "Inspection", "Preventive" });
            _editPanel.Controls.Add(_categoryComboBox);

            var vendorLabel = CreateLabel("Vendor:", 10, 55);
            _editPanel.Controls.Add(vendorLabel);
            _vendorTextBox = new TextBox();
            _vendorTextBox.Location = new System.Drawing.Point(70, 50);
            _vendorTextBox.Size = new System.Drawing.Size(150, 23);
            _editPanel.Controls.Add(_vendorTextBox);

            var costLabel = CreateLabel("Cost:", 240, 55);
            _editPanel.Controls.Add(costLabel);
            _costTextBox = new TextBox();
            _costTextBox.Location = new System.Drawing.Point(280, 50);
            _costTextBox.Size = new System.Drawing.Size(100, 23);
            _editPanel.Controls.Add(_costTextBox);

            var descLabel = CreateLabel("Description:", 400, 55);
            _editPanel.Controls.Add(descLabel);
            _descriptionTextBox = new TextBox();
            _descriptionTextBox.Location = new System.Drawing.Point(480, 50);
            _descriptionTextBox.Size = new System.Drawing.Size(200, 23);
            _editPanel.Controls.Add(_descriptionTextBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = CreateButton("Save", 800, 30, (s, e) => SaveMaintenance());
            _editPanel.Controls.Add(_saveButton);
            _cancelButton = CreateButton("Cancel", 910, 30, (s, e) => CancelEdit());
            _editPanel.Controls.Add(_cancelButton);
        }

        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaintenances()
        {
            try
            {
                _maintenances = _maintenanceRepository.GetAllMaintenances().ToList();
                PopulateMaintenanceGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading maintenance records: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateMaintenanceGrid()
        {
            _maintenanceGrid.DataSource = null;

            if (_maintenances?.Any() == true)
            {
                var displayData = _maintenances.Select(m => new
                {
                    MaintenanceID = m.MaintenanceID,
                    Date = m.Date ?? "",
                    Vehicle = m.VehicleNumber ?? "",
                    Odometer = m.OdometerReading?.ToString("N0") ?? "",
                    Category = m.MaintenanceCompleted ?? "",
                    Vendor = m.Vendor ?? "",
                    Cost = m.RepairCost?.ToString("C2") ?? "",
                    Description = m.Notes ?? ""
                }).ToList();

                _maintenanceGrid.DataSource = displayData;

                // Hide ID column
                if (_maintenanceGrid.Columns.Contains("MaintenanceID"))
                    _maintenanceGrid.Columns["MaintenanceID"].Visible = false;
            }
        }

        private void PopulateComboBoxes()
        {
            // Populate vehicle combo box
            _vehicleComboBox.DataSource = null;
            var vehicleItems = _vehicles.Select(v => new { Text = v.VehicleNumber, Value = v }).ToList();
            _vehicleComboBox.DataSource = vehicleItems;
            _vehicleComboBox.DisplayMember = "Text";
            _vehicleComboBox.ValueMember = "Value";

            // Populate category combo box
            var categories = new[]
            {
                "Tires",
                "Windshield",
                "Alignment",
                "Mechanical",
                "Car Wash",
                "Cleaning",
                "Accessory Install",
                "Oil Change",
                "Brake Service",
                "Engine Service",
                "Transmission Service",
                "Electrical",
                "Body Work",
                "Safety Inspection",
                "Other"
            };

            _categoryComboBox.DataSource = categories.ToList();
        }

        private void MaintenanceGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _maintenanceGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void AddNewMaintenance()
        {
            try
            {
                using (var addForm = new MaintenanceEditFormSyncfusion())
                {
                    if (addForm.ShowDialog() == DialogResult.OK)
                    {
                        var newMaintenance = addForm.Maintenance;
                        var maintenanceId = _maintenanceRepository.AddMaintenance(newMaintenance);
                        if (maintenanceId > 0)
                        {
                            LoadMaintenances();
                            ShowSuccessMessage("Maintenance record added successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to add maintenance record. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding maintenance record: {ex.Message}");
            }
        }

        private void EditSelectedMaintenance()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
            {
                ShowWarningMessage("Please select a maintenance record to edit.");
                return;
            }

            try
            {
                var selectedMaintenanceId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
                var selectedMaintenance = _maintenances.FirstOrDefault(m => m.MaintenanceID == selectedMaintenanceId);

                if (selectedMaintenance == null)
                {
                    ShowErrorMessage("Could not find the selected maintenance record.");
                    return;
                }

                using (var editForm = new MaintenanceEditFormSyncfusion(selectedMaintenance))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        var updatedMaintenance = editForm.Maintenance;
                        if (_maintenanceRepository.UpdateMaintenance(updatedMaintenance))
                        {
                            LoadMaintenances();
                            ShowSuccessMessage("Maintenance record updated successfully!");
                        }
                        else
                        {
                            ShowErrorMessage("Failed to update maintenance record. Please try again.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing maintenance record: {ex.Message}");
            }
        }

        private void DeleteSelectedMaintenance()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a maintenance record to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedMaintenanceId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
            var maintenanceToDelete = _maintenances.FirstOrDefault(m => m.MaintenanceID == selectedMaintenanceId);

            if (maintenanceToDelete != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete this maintenance record for {maintenanceToDelete.VehicleNumber}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _maintenanceRepository.DeleteMaintenance(maintenanceToDelete.MaintenanceID);
                        MessageBox.Show("Maintenance record deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadMaintenances();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewMaintenanceDetails()
        {
            if (_maintenanceGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a maintenance record to view.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedMaintenanceId = (int)_maintenanceGrid.SelectedRows[0].Cells["MaintenanceID"].Value;
            var maintenance = _maintenances.FirstOrDefault(m => m.MaintenanceID == selectedMaintenanceId);

            if (maintenance != null)
            {
                var details = $"Maintenance Details\n\n" +
                    $"Date: {maintenance.Date ?? "N/A"}\n" +
                    $"Vehicle: {maintenance.VehicleNumber ?? "N/A"}\n" +
                    $"Odometer: {maintenance.OdometerReading?.ToString("N0") ?? "N/A"}\n" +
                    $"Category: {maintenance.MaintenanceCompleted ?? "N/A"}\n" +
                    $"Vendor: {maintenance.Vendor ?? "N/A"}\n" +
                    $"Cost: {maintenance.RepairCost?.ToString("C2") ?? "N/A"}\n" +
                    $"Description: {maintenance.Notes ?? "N/A"}";

                MessageBox.Show(details, "Maintenance Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SearchMaintenances()
        {
            var searchTerm = _searchBox.Text?.Trim();

            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateMaintenanceGrid();
                return;
            }

            var filteredMaintenances = _maintenances.Where(m =>
                (m.VehicleNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (m.MaintenanceCompleted?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (m.Vendor?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (m.Notes?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            _maintenanceGrid.DataSource = null;

            if (filteredMaintenances.Any())
            {
                var displayData = filteredMaintenances.Select(m => new
                {
                    MaintenanceID = m.MaintenanceID,
                    Date = m.Date ?? "",
                    Vehicle = m.VehicleNumber ?? "",
                    Odometer = m.OdometerReading?.ToString("N0") ?? "",
                    Category = m.MaintenanceCompleted ?? "",
                    Vendor = m.Vendor ?? "",
                    Cost = m.RepairCost?.ToString("C2") ?? "",
                    Description = m.Notes ?? ""
                }).ToList();

                _maintenanceGrid.DataSource = displayData;

                // Hide ID column
                if (_maintenanceGrid.Columns.Contains("MaintenanceID"))
                    _maintenanceGrid.Columns["MaintenanceID"].Visible = false;
            }
        }

        private void SaveMaintenance()
        {
            try
            {
                var maintenance = _currentMaintenance ?? new Maintenance();

                // Basic information
                maintenance.DateAsDateTime = _datePicker.Value.Date;

                // Vehicle
                if (_vehicleComboBox.SelectedValue is Vehicle vehicle)
                {
                    maintenance.VehicleID = vehicle.VehicleID;
                    maintenance.VehicleNumber = vehicle.VehicleNumber;
                }

                // Odometer
                if (decimal.TryParse(_odometerTextBox.Text, out var odometer))
                    maintenance.OdometerReading = odometer;

                // Category
                maintenance.MaintenanceCompleted = _categoryComboBox.SelectedItem?.ToString();

                // Vendor
                maintenance.Vendor = _vendorTextBox.Text?.Trim();

                // Cost
                if (decimal.TryParse(_costTextBox.Text, out var cost))
                    maintenance.RepairCost = cost;

                // Description
                maintenance.Notes = _descriptionTextBox.Text?.Trim();

                // Validation
                if (maintenance.VehicleID == 0)
                {
                    MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(maintenance.MaintenanceCompleted))
                {
                    MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Save to repository
                if (_isEditing)
                {
                    _maintenanceRepository.UpdateMaintenance(maintenance);
                    MessageBox.Show("Maintenance record updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _maintenanceRepository.AddMaintenance(maintenance);
                    MessageBox.Show("Maintenance record added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                _editPanel.Visible = false;
                LoadMaintenances();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelEdit()
        {
            _editPanel.Visible = false;
            ClearEditPanel();
        }

        private void ClearEditPanel()
        {
            _datePicker.Value = DateTime.Today;
            _vehicleComboBox.SelectedIndex = -1;
            _odometerTextBox.Clear();
            _categoryComboBox.SelectedIndex = -1;
            _vendorTextBox.Clear();
            _costTextBox.Clear();
            _descriptionTextBox.Clear();
        }

        private void PopulateEditPanel(Maintenance maintenance)
        {
            _datePicker.Value = maintenance.DateAsDateTime ?? DateTime.Today;

            // Vehicle
            var vehicle = _vehicles.FirstOrDefault(v => v.VehicleID == maintenance.VehicleID);
            if (vehicle != null)
                _vehicleComboBox.SelectedValue = vehicle;

            _odometerTextBox.Text = maintenance.OdometerReading?.ToString() ?? "";

            // Category
            if (!string.IsNullOrEmpty(maintenance.MaintenanceCompleted))
                _categoryComboBox.SelectedItem = maintenance.MaintenanceCompleted;

            _vendorTextBox.Text = maintenance.Vendor ?? "";
            _costTextBox.Text = maintenance.RepairCost?.ToString() ?? "";
            _descriptionTextBox.Text = maintenance.Notes ?? "";
        }
    }
}


