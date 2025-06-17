using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Vehicle Management Form following the standard ActivityManagementForm template
    /// Provides comprehensive vehicle fleet management with consistent DataGridView interface
    /// </summary>
    public class VehicleManagementForm : StandardDataForm
    {
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _vehicleGrid;
        private Button? _addButton;
        private Button? _editButton;
        private Button? _deleteButton;
        private Button? _detailsButton;
        private MaterialSkin.Controls.MaterialTextBox _searchBox;
        private Button? _searchButton;
        private List<Vehicle> _vehicles;

        // VehicleManagementForm uses VehicleForm dialog instead of edit panel

        public VehicleManagementForm() : this(new VehicleRepository()) { }

        public VehicleManagementForm(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            InitializeComponent();
            LoadVehicles();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Vehicle Management"
            this.Text = "Vehicle Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewVehicle());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedVehicle());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedVehicle());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewVehicleDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchVehicles());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _vehicleGrid = new DataGridView();
            _vehicleGrid.Location = new System.Drawing.Point(20, 60);
            _vehicleGrid.Size = new System.Drawing.Size(1150, 650);
            _vehicleGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _vehicleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _vehicleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _vehicleGrid.ReadOnly = true;
            _vehicleGrid.AllowUserToAddRows = false;
            _vehicleGrid.AllowUserToDeleteRows = false;
            _vehicleGrid.MultiSelect = false;
            _vehicleGrid.AllowUserToResizeColumns = true;
            _vehicleGrid.AllowUserToResizeRows = true;
            _vehicleGrid.ScrollBars = ScrollBars.Both;
            _vehicleGrid.DataBindingComplete += (s, e) => {
                if (_vehicleGrid.Columns.Contains("Id"))
                    _vehicleGrid.Columns["Id"].Visible = false;
            };
            this.Controls.Add(_vehicleGrid);
            _vehicleGrid.CellDoubleClick += (s, e) => EditSelectedVehicle();
            _vehicleGrid.SelectionChanged += VehicleGrid_SelectionChanged;

            // Note: VehicleManagementForm uses VehicleForm dialog instead of edit panel

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void VehicleGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _vehicleGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
                _vehicleGrid.DataSource = null;
                _vehicleGrid.DataSource = _vehicles;
                if (_vehicleGrid.Columns.Count > 0)
                {
                    _vehicleGrid.Columns["Id"].HeaderText = "ID";
                    _vehicleGrid.Columns["VehicleNumber"].HeaderText = "Vehicle #";
                    _vehicleGrid.Columns["Make"].HeaderText = "Make";
                    _vehicleGrid.Columns["Model"].HeaderText = "Model";
                    _vehicleGrid.Columns["Year"].HeaderText = "Year";
                    _vehicleGrid.Columns["Capacity"].HeaderText = "Capacity";
                    _vehicleGrid.Columns["FuelType"].HeaderText = "Fuel Type";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
            }
        }

        private void AddNewVehicle()
        {
            // Show VehicleForm dialog for add new
            using (var vehicleForm = new VehicleFormSyncfusion())
            {
                if (vehicleForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _vehicleRepository.AddVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        ShowSuccessMessage("Vehicle added successfully.");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error adding vehicle: {ex.Message}");
                    }
                }
            }
        }

        private void EditSelectedVehicle()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
            var vehicle = _vehicleRepository.GetVehicleById(selectedId);
            if (vehicle == null)
            {
                ShowErrorMessage("Could not find the selected vehicle.");
                return;
            }

            // Show VehicleForm dialog for edit
            using (var vehicleForm = new VehicleFormSyncfusion(vehicle))
            {
                if (vehicleForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _vehicleRepository.UpdateVehicle(vehicleForm.Vehicle);
                        LoadVehicles();
                        ShowSuccessMessage("Vehicle updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error updating vehicle: {ex.Message}");
                    }
                }
            }
        }

        private void DeleteSelectedVehicle()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
            if (!ConfirmDelete())
                return;
            try
            {
                _vehicleRepository.DeleteVehicle(selectedId);
                LoadVehicles();
                ShowSuccessMessage("Vehicle deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting vehicle: {ex.Message}");
            }
        }

        private void ViewVehicleDetails()
        {
            if (_vehicleGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_vehicleGrid.SelectedRows[0].Cells["Id"].Value;
            var vehicle = _vehicleRepository.GetVehicleById(selectedId);
            if (vehicle != null)
            {
                MessageBox.Show($"Vehicle Details:\nNumber: {vehicle.VehicleNumber}\nMake: {vehicle.Make}\nModel: {vehicle.Model}\nYear: {vehicle.Year}\nCapacity: {vehicle.Capacity}\nFuel Type: {vehicle.FuelType}",
                    "Vehicle Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowErrorMessage("Could not load vehicle details.");
            }
        }

        private void SearchVehicles()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadVehicles();
                return;
            }
            List<Vehicle> filtered = _vehicles.FindAll(v =>
                (v.VehicleNumber?.ToLower().Contains(searchTerm) == true) ||
                (v.Make?.ToLower().Contains(searchTerm) == true) ||
                (v.Model?.ToLower().Contains(searchTerm) == true) ||
                (v.FuelType?.ToLower().Contains(searchTerm) == true)
            );
            _vehicleGrid.DataSource = null;
            _vehicleGrid.DataSource = filtered;
        }
    }
}
