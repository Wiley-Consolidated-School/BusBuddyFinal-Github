using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Maintenance Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing maintenance records with grid view and CRUD operations
    /// </summary>
    public class MaintenanceManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _maintenanceGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Maintenance> _maintenances = new List<Maintenance>();
        private List<Vehicle> _vehicles = new List<Vehicle>();

        // Fields for add/edit
        private Panel _editPanel = null!;
        private DateTimePicker _datePicker = null!;
        private ComboBox _vehicleComboBox = null!;
        private Control _odometerTextBox = null!;
        private ComboBox _categoryComboBox = null!;
        private Control _vendorTextBox = null!;
        private Control _costTextBox = null!;
        private Control _descriptionTextBox = null!;
        private Control _saveButton = null!;
        private Control _cancelButton = null!;
        private Maintenance? _currentMaintenance = null;
        private bool _isEditing = false;

        public MaintenanceManagementFormSyncfusion() : this(new MaintenanceRepository()) { }

        public MaintenanceManagementFormSyncfusion(IMaintenanceRepository maintenanceRepository)
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
            this.Text = "🔧 Maintenance Management";
            this.ClientSize = GetDpiAwareSize(new Size(1200, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(800, 600));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            RefreshMaterialTheme();

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Create toolbar buttons
            _addButton = SyncfusionThemeHelper.CreateStyledButton("➕ Add New");
            _editButton = SyncfusionThemeHelper.CreateStyledButton("✏️ Edit");
            _deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            _detailsButton = SyncfusionThemeHelper.CreateStyledButton("👁️ Details");
            _searchButton = SyncfusionThemeHelper.CreateStyledButton("🔍 Search");

            // Create search textbox
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search maintenance records...");

            // Configure button sizes and positions
            var buttonSize = GetDpiAwareSize(new Size(100, 35));
            var buttonY = GetDpiAwareY(20);

            _addButton.Size = buttonSize;
            _addButton.Location = new Point(GetDpiAwareX(20), buttonY);

            _editButton.Size = buttonSize;
            _editButton.Location = new Point(GetDpiAwareX(130), buttonY);
            _editButton.Enabled = false; // Initially disabled

            _deleteButton.Size = buttonSize;
            _deleteButton.Location = new Point(GetDpiAwareX(240), buttonY);
            _deleteButton.Enabled = false; // Initially disabled

            _detailsButton.Size = buttonSize;
            _detailsButton.Location = new Point(GetDpiAwareX(350), buttonY);
            _detailsButton.Enabled = false; // Initially disabled

            // Search controls
            var searchLabel = CreateLabel("🔍 Search:", 500, 25);
            _searchBox.Size = GetDpiAwareSize(new Size(150, 30));
            _searchBox.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));

            _searchButton.Size = GetDpiAwareSize(new Size(80, 35));
            _searchButton.Location = new Point(GetDpiAwareX(710), buttonY);

            // Add buttons to main panel
            _mainPanel.Controls.Add(_addButton);
            _mainPanel.Controls.Add(_editButton);
            _mainPanel.Controls.Add(_deleteButton);
            _mainPanel.Controls.Add(_detailsButton);
            _mainPanel.Controls.Add(_searchBox);
            _mainPanel.Controls.Add(_searchButton);

            // Create DataGridView
            _maintenanceGrid = CreateDataGrid();
            _maintenanceGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _maintenanceGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _maintenanceGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply Syncfusion theming to grid
            SyncfusionThemeHelper.ApplyMaterialDataGrid(_maintenanceGrid);

            _mainPanel.Controls.Add(_maintenanceGrid);

            // Configure grid columns
            SetupDataGridColumns();

            // Initialize edit panel
            InitializeEditPanel();
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewMaintenance();
            _editButton.Click += (s, e) => EditSelectedMaintenance();
            _deleteButton.Click += (s, e) => DeleteSelectedMaintenance();
            _detailsButton.Click += (s, e) => ViewMaintenanceDetails();
            _searchButton.Click += (s, e) => SearchMaintenances();

            if (_maintenanceGrid != null)
            {
                _maintenanceGrid.SelectionChanged += MaintenanceGrid_SelectionChanged;
                _maintenanceGrid.DoubleClick += (s, e) => EditSelectedMaintenance();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBox searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchMaintenances();
                        e.Handled = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_maintenanceGrid == null) return;

            _maintenanceGrid.AutoGenerateColumns = false;
            _maintenanceGrid.Columns.Clear();

            // Add columns with DPI-aware widths
            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaintenanceID",
                DataPropertyName = "MaintenanceID",
                HeaderText = "ID",
                Width = GetDpiAwareWidth(60),
                ReadOnly = true,
                Visible = false
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Date",
                DataPropertyName = "Date",
                HeaderText = "Date",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Vehicle",
                DataPropertyName = "Vehicle",
                HeaderText = "Vehicle",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Odometer",
                DataPropertyName = "Odometer",
                HeaderText = "Odometer",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                DataPropertyName = "Category",
                HeaderText = "Category",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Vendor",
                DataPropertyName = "Vendor",
                HeaderText = "Vendor",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cost",
                DataPropertyName = "Cost",
                HeaderText = "Cost",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _maintenanceGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                DataPropertyName = "Description",
                HeaderText = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
        }

        private void InitializeEditPanel()
        {
            // Create edit panel (1150x120, y=730, hidden)
            _editPanel = new Panel();
            _editPanel.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(730));
            _editPanel.Size = GetDpiAwareSize(new Size(1150, 120));
            _editPanel.Visible = false;
            _editPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _mainPanel.Controls.Add(_editPanel);

            // Maintenance form-specific fields: Date, Vehicle, Odometer, Category, Vendor, Cost, Description
            var dateLabel = CreateLabel("Date:", 10, 15);
            _editPanel.Controls.Add(dateLabel);
            _datePicker = new DateTimePicker();
            _datePicker.Location = new Point(GetDpiAwareX(60), GetDpiAwareY(10));
            _datePicker.Size = GetDpiAwareSize(new Size(150, 23));
            _datePicker.Value = DateTime.Today;
            _editPanel.Controls.Add(_datePicker);

            var vehicleLabel = CreateLabel("Vehicle:", 230, 15);
            _editPanel.Controls.Add(vehicleLabel);
            _vehicleComboBox = new ComboBox();
            _vehicleComboBox.Location = new Point(GetDpiAwareX(290), GetDpiAwareY(10));
            _vehicleComboBox.Size = GetDpiAwareSize(new Size(150, 23));
            _vehicleComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _editPanel.Controls.Add(_vehicleComboBox);

            var odometerLabel = CreateLabel("Odometer:", 460, 15);
            _editPanel.Controls.Add(odometerLabel);
            _odometerTextBox = SyncfusionThemeHelper.CreateStyledTextBox("");
            _odometerTextBox.Location = new Point(GetDpiAwareX(530), GetDpiAwareY(10));
            _odometerTextBox.Size = GetDpiAwareSize(new Size(100, 23));
            _editPanel.Controls.Add(_odometerTextBox);

            var categoryLabel = CreateLabel("Category:", 650, 15);
            _editPanel.Controls.Add(categoryLabel);
            _categoryComboBox = new ComboBox();
            _categoryComboBox.Location = new Point(GetDpiAwareX(720), GetDpiAwareY(10));
            _categoryComboBox.Size = GetDpiAwareSize(new Size(120, 23));
            _categoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _categoryComboBox.Items.AddRange(new object[] { "Routine", "Repair", "Inspection", "Preventive" });
            _editPanel.Controls.Add(_categoryComboBox);

            var vendorLabel = CreateLabel("Vendor:", 10, 55);
            _editPanel.Controls.Add(vendorLabel);
            _vendorTextBox = SyncfusionThemeHelper.CreateStyledTextBox("");
            _vendorTextBox.Location = new Point(GetDpiAwareX(70), GetDpiAwareY(50));
            _vendorTextBox.Size = GetDpiAwareSize(new Size(150, 23));
            _editPanel.Controls.Add(_vendorTextBox);

            var costLabel = CreateLabel("Cost:", 240, 55);
            _editPanel.Controls.Add(costLabel);
            _costTextBox = SyncfusionThemeHelper.CreateStyledTextBox("");
            _costTextBox.Location = new Point(GetDpiAwareX(280), GetDpiAwareY(50));
            _costTextBox.Size = GetDpiAwareSize(new Size(100, 23));
            _editPanel.Controls.Add(_costTextBox);

            var descLabel = CreateLabel("Description:", 400, 55);
            _editPanel.Controls.Add(descLabel);
            _descriptionTextBox = SyncfusionThemeHelper.CreateStyledTextBox("");
            _descriptionTextBox.Location = new Point(GetDpiAwareX(480), GetDpiAwareY(50));
            _descriptionTextBox.Size = GetDpiAwareSize(new Size(200, 23));
            _editPanel.Controls.Add(_descriptionTextBox);

            // Save/Cancel buttons at x=800, x=910
            _saveButton = SyncfusionThemeHelper.CreateStyledButton("💾 Save");
            _saveButton.Location = new Point(GetDpiAwareX(800), GetDpiAwareY(30));
            _saveButton.Size = GetDpiAwareSize(new Size(80, 35));
            _saveButton.Click += (s, e) => SaveMaintenance();
            _editPanel.Controls.Add(_saveButton);

            _cancelButton = SyncfusionThemeHelper.CreateStyledButton("❌ Cancel");
            _cancelButton.Location = new Point(GetDpiAwareX(910), GetDpiAwareY(30));
            _cancelButton.Size = GetDpiAwareSize(new Size(80, 35));
            _cancelButton.Click += (s, e) => CancelEdit();
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
            bool hasSelection = _maintenanceGrid?.SelectedRows.Count > 0;
            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void AddNewMaintenance()
        {
            _currentMaintenance = null;
            _isEditing = false;
            ShowEditPanel();
        }

        private void EditSelectedMaintenance()
        {
            if (_maintenanceGrid?.SelectedRows.Count > 0)
            {
                var selectedRow = _maintenanceGrid.SelectedRows[0];
                var maintenanceId = (int)selectedRow.Cells["MaintenanceID"].Value;
                _currentMaintenance = _maintenances.FirstOrDefault(m => m.MaintenanceID == maintenanceId);

                if (_currentMaintenance != null)
                {
                    _isEditing = true;
                    ShowEditPanel();
                }
            }
        }

        private void DeleteSelectedMaintenance()
        {
            if (_maintenanceGrid?.SelectedRows.Count > 0)
            {
                var selectedRow = _maintenanceGrid.SelectedRows[0];
                var maintenanceId = (int)selectedRow.Cells["MaintenanceID"].Value;
                var maintenance = _maintenances.FirstOrDefault(m => m.MaintenanceID == maintenanceId);

                if (maintenance != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete the maintenance record for {maintenance.VehicleNumber} on {maintenance.Date}?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            _maintenanceRepository.DeleteMaintenance(maintenanceId);
                            LoadMaintenances();
                            MessageBox.Show("Maintenance record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ViewMaintenanceDetails()
        {
            if (_maintenanceGrid?.SelectedRows.Count > 0)
            {
                var selectedRow = _maintenanceGrid.SelectedRows[0];
                var maintenanceId = (int)selectedRow.Cells["MaintenanceID"].Value;
                var maintenance = _maintenances.FirstOrDefault(m => m.MaintenanceID == maintenanceId);

                if (maintenance != null)
                {
                    using (var detailForm = new MaintenanceEditFormSyncfusion(maintenance))
                    {
                        detailForm.ShowDialog(this);
                    }
                }
            }
        }

        private void SearchMaintenances()
        {
            if (_searchBox is TextBox searchBox)
            {
                var searchTerm = searchBox.Text?.Trim().ToLower() ?? "";

                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadMaintenances();
                    return;
                }

                try
                {
                    var allMaintenances = _maintenanceRepository.GetAllMaintenances().ToList();
                    _maintenances = allMaintenances.Where(m =>
                        (m.VehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                        (m.MaintenanceCompleted?.ToLower().Contains(searchTerm) == true) ||
                        (m.Vendor?.ToLower().Contains(searchTerm) == true) ||
                        (m.Notes?.ToLower().Contains(searchTerm) == true)
                    ).ToList();

                    PopulateMaintenanceGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching maintenance records: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ShowEditPanel()
        {
            _editPanel.Visible = true;
            PopulateComboBoxes();            if (_isEditing && _currentMaintenance != null)
            {
                // Populate form with current maintenance data
                _datePicker.Value = _currentMaintenance.DateAsDateTime ?? DateTime.Today;

                // Set vehicle selection
                if (!string.IsNullOrEmpty(_currentMaintenance.VehicleNumber))
                {
                    var vehicle = _vehicles.FirstOrDefault(v => v.VehicleNumber == _currentMaintenance.VehicleNumber);
                    if (vehicle != null)
                    {
                        _vehicleComboBox.SelectedValue = vehicle;
                    }
                }

                if (_odometerTextBox is TextBox odometerTb)
                    odometerTb.Text = _currentMaintenance.OdometerReading?.ToString() ?? "";

                if (_categoryComboBox.Items.Contains(_currentMaintenance.MaintenanceCompleted))
                    _categoryComboBox.SelectedItem = _currentMaintenance.MaintenanceCompleted;

                if (_vendorTextBox is TextBox vendorTb)
                    vendorTb.Text = _currentMaintenance.Vendor ?? "";

                if (_costTextBox is TextBox costTb)
                    costTb.Text = _currentMaintenance.RepairCost?.ToString("F2") ?? "";

                if (_descriptionTextBox is TextBox descTb)
                    descTb.Text = _currentMaintenance.Notes ?? "";
            }
            else
            {
                // Clear form for new maintenance
                _datePicker.Value = DateTime.Today;
                _vehicleComboBox.SelectedIndex = -1;

                if (_odometerTextBox is TextBox odometerTb)
                    odometerTb.Text = "";

                _categoryComboBox.SelectedIndex = -1;

                if (_vendorTextBox is TextBox vendorTb)
                    vendorTb.Text = "";

                if (_costTextBox is TextBox costTb)
                    costTb.Text = "";

                if (_descriptionTextBox is TextBox descTb)
                    descTb.Text = "";
            }
        }

        private void SaveMaintenance()
        {
            try
            {
                var maintenance = _currentMaintenance ?? new Maintenance();

                // Validate required fields
                if (_vehicleComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }                // Update maintenance object
                maintenance.DateAsDateTime = _datePicker.Value;
                maintenance.VehicleID = ((Vehicle)_vehicleComboBox.SelectedValue).VehicleID;
                maintenance.VehicleNumber = ((Vehicle)_vehicleComboBox.SelectedValue).VehicleNumber;

                if (_odometerTextBox is TextBox odometerTb && int.TryParse(odometerTb.Text, out int odometer))
                    maintenance.OdometerReading = odometer;

                if (_categoryComboBox.SelectedItem != null)
                    maintenance.MaintenanceCompleted = _categoryComboBox.SelectedItem.ToString();

                if (_vendorTextBox is TextBox vendorTb)
                    maintenance.Vendor = vendorTb.Text;

                if (_costTextBox is TextBox costTb && decimal.TryParse(costTb.Text, out decimal cost))
                    maintenance.RepairCost = cost;

                if (_descriptionTextBox is TextBox descTb)
                    maintenance.Notes = descTb.Text;

                // Save to repository
                if (_isEditing)
                {
                    _maintenanceRepository.UpdateMaintenance(maintenance);
                    MessageBox.Show("Maintenance record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _maintenanceRepository.AddMaintenance(maintenance);
                    MessageBox.Show("Maintenance record added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CancelEdit();
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
            _currentMaintenance = null;
            _isEditing = false;
        }
    }
}
