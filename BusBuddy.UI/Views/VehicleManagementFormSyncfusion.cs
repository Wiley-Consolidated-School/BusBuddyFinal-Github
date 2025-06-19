using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Vehicle Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing vehicle fleet with grid view and CRUD operations
    /// </summary>
    public class VehicleManagementFormSyncfusion : SyncfusionBaseForm
    {        private readonly IVehicleRepository _vehicleRepository;
        private SfDataGrid? _vehicleGrid;
        private SfButton? _addButton;
        private SfButton? _editButton;
        private SfButton? _deleteButton;
        private SfButton? _detailsButton;
        private TextBoxExt? _searchBox;
        private SfButton? _searchButton;
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

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            var buttonSize = GetDpiAwareSize(new Size(100, 35));
            var buttonY = GetDpiAwareY(20);

            // Create toolbar buttons
            _addButton = ControlFactory.CreateButton("➕ Add New", buttonSize, (s, e) => AddNewVehicle());
            _editButton = ControlFactory.CreateButton("✏️ Edit", buttonSize, (s, e) => EditSelectedVehicle());
            _deleteButton = ControlFactory.CreateButton("🗑️ Delete", buttonSize, (s, e) => DeleteSelectedVehicle());
            _detailsButton = ControlFactory.CreateButton("👁️ Details", buttonSize, (s, e) => ViewVehicleDetails());
            _searchButton = ControlFactory.CreateButton("🔍 Search", GetDpiAwareSize(new Size(80, 35)), (s, e) => SearchVehicles());

            // Create search textbox
            _searchBox = ControlFactory.CreateTextBox(_bannerTextProvider, "Search vehicles...");

            // Configure button positions
            _addButton.Location = new Point(GetDpiAwareX(20), buttonY);

            _editButton.Location = new Point(GetDpiAwareX(130), buttonY);
            _editButton.Enabled = false; // Initially disabled

            _deleteButton.Location = new Point(GetDpiAwareX(240), buttonY);
            _deleteButton.Enabled = false; // Initially disabled

            _detailsButton.Location = new Point(GetDpiAwareX(350), buttonY);
            _detailsButton.Enabled = false; // Initially disabled

            // Search controls
            var searchLabel = ControlFactory.CreateLabel("🔍 Search:");
            searchLabel.Location = new Point(GetDpiAwareX(500), GetDpiAwareY(25));
            _searchBox.Size = GetDpiAwareSize(new Size(150, 30));
            _searchBox.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));

            _searchButton.Location = new Point(GetDpiAwareX(710), buttonY);

            // Add buttons to main panel
            _mainPanel.Controls.Add(_addButton);
            _mainPanel.Controls.Add(_editButton);
            _mainPanel.Controls.Add(_deleteButton);
            _mainPanel.Controls.Add(_detailsButton);
            _mainPanel.Controls.Add(searchLabel);
            _mainPanel.Controls.Add(_searchBox);
            _mainPanel.Controls.Add(_searchButton);

            // Create SfDataGrid
            _vehicleGrid = new SfDataGrid
            {
                Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70)),
                Size = GetDpiAwareSize(new Size(1150, 800)),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,                AutoGenerateColumns = false,
                SelectionMode = Syncfusion.WinForms.DataGrid.Enums.GridSelectionMode.Single,
                NavigationMode = Syncfusion.WinForms.DataGrid.Enums.NavigationMode.Row
            };

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
            if (_vehicleGrid != null)
            {
                _vehicleGrid.SelectionChanged += VehicleGrid_SelectionChanged;
                _vehicleGrid.CellDoubleClick += (s, e) => EditSelectedVehicle();
            }

            // Handle Enter key in search box
            if (_searchBox is TextBoxExt searchTb)
            {
                searchTb.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchVehicles();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                };
            }
        }

        private void SetupDataGridColumns()
        {
            if (_vehicleGrid == null) return;

            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "Id", HeaderText = "ID", Visible = false });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "VehicleNumber", HeaderText = "Vehicle #", Width = GetDpiAwareWidth(120) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "Make", HeaderText = "Make", Width = GetDpiAwareWidth(130) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "Model", HeaderText = "Model", Width = GetDpiAwareWidth(150) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "Year", HeaderText = "Year", Width = GetDpiAwareWidth(80) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "Capacity", HeaderText = "Capacity", Width = GetDpiAwareWidth(100) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "FuelType", HeaderText = "Fuel Type", Width = GetDpiAwareWidth(120) });
            _vehicleGrid.Columns.Add(new GridTextColumn() { MappingName = "VIN", HeaderText = "VIN", Width = double.NaN }); // AutoSize
        }

        private void VehicleGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = _vehicleGrid?.SelectedItem != null;
            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                if (_vehicleGrid != null)
                {
                    _vehicleGrid.DataSource = _vehicles;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewVehicle()
        {
            try
            {
                using (var vehicleForm = new VehicleFormSyncfusion())
                {
                    if (vehicleForm.ShowDialog() == DialogResult.OK)
                    {
                        _vehicleRepository.AddVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        ShowSuccessMessage("Vehicle added successfully.");
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
            if (_vehicleGrid?.SelectedItem is not Vehicle selectedVehicle)
                return;

            try
            {
                var vehicle = _vehicleRepository.GetVehicleById(selectedVehicle.Id);
                if (vehicle == null)
                {
                    MessageBox.Show("Could not find the selected vehicle.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var vehicleForm = new VehicleFormSyncfusion(vehicle))
                {
                    if (vehicleForm.ShowDialog() == DialogResult.OK)
                    {
                        _vehicleRepository.UpdateVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        ShowSuccessMessage("Vehicle updated successfully.");
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
            if (_vehicleGrid?.SelectedItem is not Vehicle selectedVehicle)
                return;

            try
            {
                if (ConfirmDelete($"vehicle '{selectedVehicle.VehicleNumber}'"))
                {
                    _vehicleRepository.DeleteVehicle(selectedVehicle.Id);
                    LoadVehicles();
                    ShowSuccessMessage("Vehicle deleted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewVehicleDetails()
        {
            if (_vehicleGrid?.SelectedItem is not Vehicle vehicle)
                return;

            try
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing vehicle details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchVehicles()
        {
            if (_searchBox is not TextBoxExt searchBox) return;

            try
            {
                string searchTerm = searchBox.Text?.Trim().ToLower() ?? "";
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _vehicleGrid.DataSource = _vehicles;
                    return;
                }

                List<Vehicle> filtered = _vehicles.FindAll(v =>
                    (v.VehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                    (v.Make?.ToLower().Contains(searchTerm) == true) ||
                    (v.Model?.ToLower().Contains(searchTerm) == true) ||
                    (v.FuelType?.ToLower().Contains(searchTerm) == true) ||
                    (v.VIN?.ToLower().Contains(searchTerm) == true)
                );

                if(_vehicleGrid != null)
                {
                    _vehicleGrid.DataSource = filtered;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
