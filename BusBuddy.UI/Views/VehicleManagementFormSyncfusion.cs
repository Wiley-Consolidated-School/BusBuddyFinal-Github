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
    /// Vehicle Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing vehicle fleet with grid view and CRUD operations
    /// </summary>
    public class VehicleManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _vehicleGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Vehicle> _vehicles;

        // VehicleManagementForm uses VehicleForm dialog instead of edit panel

        public VehicleManagementFormSyncfusion() : this(new VehicleRepository()) { }

        public VehicleManagementFormSyncfusion(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            InitializeComponent();
            LoadVehicles();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Vehicle Management"
            this.Text = "🚗 Vehicle Management";
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
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search vehicles...");

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
            _vehicleGrid = CreateDataGrid();
            _vehicleGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _vehicleGrid.Size = GetDpiAwareSize(new Size(1150, 800));
            _vehicleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Apply Syncfusion theming to grid
            SyncfusionThemeHelper.ApplyMaterialDataGrid(_vehicleGrid);

            _mainPanel.Controls.Add(_vehicleGrid);

            // Configure grid columns
            SetupDataGridColumns();
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls for this form
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewVehicle();
            _editButton.Click += (s, e) => EditSelectedVehicle();
            _deleteButton.Click += (s, e) => DeleteSelectedVehicle();
            _detailsButton.Click += (s, e) => ViewVehicleDetails();
            _searchButton.Click += (s, e) => SearchVehicles();

            if (_vehicleGrid != null)
            {
                _vehicleGrid.SelectionChanged += VehicleGrid_SelectionChanged;
                _vehicleGrid.DoubleClick += (s, e) => EditSelectedVehicle();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBox searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchVehicles();
                        e.Handled = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_vehicleGrid == null) return;

            _vehicleGrid.AutoGenerateColumns = false;
            _vehicleGrid.Columns.Clear();

            // Add columns with DPI-aware widths
            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                HeaderText = "ID",
                Width = GetDpiAwareWidth(60),
                ReadOnly = true,
                Visible = false
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "VehicleNumber",
                DataPropertyName = "VehicleNumber",
                HeaderText = "Vehicle #",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Make",
                DataPropertyName = "Make",
                HeaderText = "Make",
                Width = GetDpiAwareWidth(130),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Model",
                DataPropertyName = "Model",
                HeaderText = "Model",
                Width = GetDpiAwareWidth(150),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Year",
                DataPropertyName = "Year",
                HeaderText = "Year",
                Width = GetDpiAwareWidth(80),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Capacity",
                DataPropertyName = "Capacity",
                HeaderText = "Capacity",
                Width = GetDpiAwareWidth(100),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FuelType",
                DataPropertyName = "FuelType",
                HeaderText = "Fuel Type",
                Width = GetDpiAwareWidth(120),
                ReadOnly = true
            });

            _vehicleGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "VIN",
                DataPropertyName = "VIN",
                HeaderText = "VIN",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
        }

        private void VehicleGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _vehicleGrid?.SelectedRows.Count > 0;
            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                _vehicleGrid.DataSource = null;
                _vehicleGrid.DataSource = _vehicles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewVehicle()
        {
            // Show VehicleForm dialog for add new
            try
            {
                using (var vehicleForm = new VehicleFormSyncfusion())
                {
                    if (vehicleForm.ShowDialog() == DialogResult.OK)
                    {
                        _vehicleRepository.AddVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        MessageBox.Show("Vehicle added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedVehicle()
        {
            if (_vehicleGrid?.SelectedRows.Count == 0)
                return;

            try
            {
                int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
                var vehicle = _vehicleRepository.GetVehicleById(selectedId);
                if (vehicle == null)
                {
                    MessageBox.Show("Could not find the selected vehicle.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show VehicleForm dialog for edit
                using (var vehicleForm = new VehicleFormSyncfusion(vehicle))
                {
                    if (vehicleForm.ShowDialog() == DialogResult.OK)
                    {
                        _vehicleRepository.UpdateVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        MessageBox.Show("Vehicle updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedVehicle()
        {
            if (_vehicleGrid?.SelectedRows.Count == 0)
                return;

            try
            {
                int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
                var vehicle = _vehicleRepository.GetVehicleById(selectedId);

                if (vehicle != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete vehicle '{vehicle.VehicleNumber}' ({vehicle.Make} {vehicle.Model})?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _vehicleRepository.DeleteVehicle(selectedId);
                        LoadVehicles();
                        MessageBox.Show("Vehicle deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewVehicleDetails()
        {
            if (_vehicleGrid?.SelectedRows.Count == 0)
                return;

            try
            {
                int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
                var vehicle = _vehicleRepository.GetVehicleById(selectedId);
                if (vehicle != null)
                {
                    var details = $"Vehicle Details:\n\n" +
                                $"Number: {vehicle.VehicleNumber}\n" +
                                $"Make: {vehicle.Make}\n" +
                                $"Model: {vehicle.Model}\n" +
                                $"Year: {vehicle.Year}\n" +
                                $"Capacity: {vehicle.Capacity}\n" +
                                $"Fuel Type: {vehicle.FuelType}\n" +
                                $"VIN: {vehicle.VIN}";

                    MessageBox.Show(details, "Vehicle Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not load vehicle details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing vehicle details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchVehicles()
        {
            if (_searchBox is TextBox searchBox)
            {
                string searchTerm = searchBox.Text?.Trim().ToLower() ?? "";

                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadVehicles();
                    return;
                }

                try
                {
                    List<Vehicle> filtered = _vehicles.FindAll(v =>
                        (v.VehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                        (v.Make?.ToLower().Contains(searchTerm) == true) ||
                        (v.Model?.ToLower().Contains(searchTerm) == true) ||
                        (v.FuelType?.ToLower().Contains(searchTerm) == true) ||
                        (v.VIN?.ToLower().Contains(searchTerm) == true)
                    );

                    _vehicleGrid.DataSource = null;
                    _vehicleGrid.DataSource = filtered;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error searching vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
