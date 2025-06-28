using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.ListView;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Vehicle Management Form - Shell Structure
    /// Manages vehicle records with CRUD operations
    /// Based on Syncfusion SfDataGrid documentation
    ///
    /// 📖 SYNCFUSION DOCUMENTATION:
    /// - SfDataGrid: https://help.syncfusion.com/windowsforms/datagrid/getting-started
    /// - SfButton: https://help.syncfusion.com/windowsforms/button/getting-started
    /// </summary>
    public partial class VehicleManagementForm : BaseManagementForm<Vehicle>
    {
        #region Private Fields
        private readonly IVehicleRepository _vehicleRepository;

        // Additional controls specific to Vehicle management
        private SfButton _exportButton;
        private SfButton _importButton;
        private ComboBoxAdv _vehicleTypeFilter;
        private ComboBoxAdv _statusFilter;
        private TextBoxExt _vinSearchBox;

        // Search timer for VIN search
        private System.Windows.Forms.Timer _vinSearchTimer;
        #endregion

        #region Properties Override
        protected override string FormTitle => "🚍 Vehicle Management";
        protected override string SearchPlaceholder => "Search vehicles...";
        protected override string EntityName => "Vehicle";
        #endregion

        #region Constructors
        public VehicleManagementForm() : this(new VehicleRepository(), new MessageBoxService())
        {
        }

        public VehicleManagementForm(IVehicleRepository vehicleRepository) : this(vehicleRepository, new MessageBoxService())
        {
        }

        public VehicleManagementForm(IVehicleRepository vehicleRepository, IMessageService messageService) : base(messageService)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            SetRepository(_vehicleRepository);

            // Initialize additional controls and setup filters
            InitializeVehicleSpecificControls();
            SetupFilters();
        }
        #endregion

        #region Base Implementation Override - Shell Methods
        protected override void LoadDataFromRepository()
        {
            try
            {
                _entities = _vehicleRepository.GetAllVehicles();
                PopulateDataGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error loading vehicles: {ex.Message}", "Vehicle Error", ex);
                _entities = new List<Vehicle>();
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                using (var editForm = new VehicleEditForm()) // Add mode - no vehicle provided
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadDataFromRepository(); // Refresh the grid
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error opening add vehicle form: {ex.Message}", "Form Error", ex);
            }
        }

        protected override void EditSelectedEntity()
        {
            try
            {
                var selectedVehicle = GetSelectedEntity();
                if (selectedVehicle == null)
                {
                    _messageService.ShowWarning("Please select a vehicle to edit.", "No Selection");
                    return;
                }

                using (var editForm = new VehicleEditForm(selectedVehicle))
                {
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadDataFromRepository(); // Refresh the grid
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error opening edit vehicle form: {ex.Message}", "Form Error", ex);
            }
        }

        protected override void DeleteSelectedEntity()
        {
            try
            {
                var selectedVehicle = GetSelectedEntity();
                if (selectedVehicle == null)
                {
                    _messageService.ShowWarning("Please select a vehicle to delete.", "No Selection");
                    return;
                }

                var confirmed = _messageService.ShowConfirmation(
                    $"Are you sure you want to delete vehicle '{selectedVehicle.VehicleNumber}'?\nThis action cannot be undone.",
                    "Confirm Delete"
                );

                if (confirmed)
                {
                    if (_vehicleRepository.DeleteVehicle(selectedVehicle.VehicleID))
                    {
                        _messageService.ShowInfo("Vehicle deleted successfully.", "Delete Complete");
                        LoadDataFromRepository(); // Refresh the grid
                    }
                    else
                    {
                        _messageService.ShowError("Failed to delete vehicle. Please try again.", "Delete Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting vehicle: {ex.Message}", "Delete Error", ex);
            }
        }

        protected override void ViewEntityDetails()
        {
            try
            {
                var selectedVehicle = GetSelectedEntity();
                if (selectedVehicle == null)
                {
                    _messageService.ShowWarning("Please select a vehicle to view.", "No Selection");
                    return;
                }

                using (var editForm = new VehicleEditForm(selectedVehicle))
                {
                    // TODO: Set form to read-only mode when VehicleEditForm supports it
                    // For now, show the form normally
                    editForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error opening vehicle details: {ex.Message}", "Form Error", ex);
            }
        }

        protected override void SearchEntities()
        {
            var searchText = _searchBox?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If search is empty, show all vehicles
                LoadDataFromRepository();
                return;
            }

            try
            {
                var searchTerm = searchText.ToLower();
                var filteredVehicles = _entities.Where(v =>
                    v.VehicleNumber?.ToLower().Contains(searchTerm) == true ||
                    v.BusNumber?.ToLower().Contains(searchTerm) == true ||
                    v.Make?.ToLower().Contains(searchTerm) == true ||
                    v.Model?.ToLower().Contains(searchTerm) == true ||
                    v.VIN?.ToLower().Contains(searchTerm) == true ||
                    v.Status?.ToLower().Contains(searchTerm) == true ||
                    v.Year.ToString().Contains(searchTerm) ||
                    v.Capacity.ToString().Contains(searchTerm)
                ).ToList();

                _dataGrid.DataSource = null;
                _dataGrid.DataSource = filteredVehicles;
                _dataGrid.Refresh();
            }
            catch (Exception ex)
            {
                HandleError($"Error searching vehicles: {ex.Message}", "Search Error", ex);
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

            _dataGrid.Columns.Clear();

            // Based on Vehicle model properties
            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "VehicleID",
                HeaderText = "ID",
                Width = 60,
                AllowEditing = false
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "VehicleNumber",
                HeaderText = "Vehicle #",
                Width = 100
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "BusNumber",
                HeaderText = "Bus #",
                Width = 80
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Make",
                HeaderText = "Make",
                Width = 100
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Model",
                HeaderText = "Model",
                Width = 100
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Year",
                HeaderText = "Year",
                Width = 60
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Capacity",
                HeaderText = "Capacity",
                Width = 80
            });

            _dataGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Status",
                HeaderText = "Status",
                Width = 100
            });
        }
        #endregion

        #region Vehicle-Specific Methods - Shell Implementation
        private void InitializeVehicleSpecificControls()
        {
            // Initialize additional vehicle-specific filter controls
            try
            {
                var buttonSize = GetDpiAwareSize(new Size(100, 35));

                // Export/Import buttons
                _exportButton = ControlFactory.CreateButton("📤 Export", buttonSize, ExportButton_Click);
                _importButton = ControlFactory.CreateButton("📥 Import", buttonSize, ImportButton_Click);

                // Vehicle type filter using ControlFactory
                _vehicleTypeFilter = ControlFactory.CreateComboBox(new[] { "All Types", "Bus", "Van", "Truck" });
                _vehicleTypeFilter.Size = GetDpiAwareSize(new Size(120, 30));
                _vehicleTypeFilter.SelectedIndex = 0;
                _vehicleTypeFilter.SelectedIndexChanged += VehicleTypeFilter_SelectionChanged;

                // Status filter using ControlFactory
                _statusFilter = ControlFactory.CreateComboBox(new[] { "All Status", "Active", "Maintenance", "Out of Service", "Retired" });
                _statusFilter.Size = GetDpiAwareSize(new Size(120, 30));
                _statusFilter.SelectedIndex = 0;
                _statusFilter.SelectedIndexChanged += StatusFilter_SelectionChanged;

                // VIN search box
                _vinSearchBox = ControlFactory.CreateTextBox("Search by VIN...");
                _vinSearchBox.Size = GetDpiAwareSize(new Size(150, 30));
                _vinSearchBox.TextChanged += VinSearchBox_TextChanged;

                // Add controls to form
                if (_exportButton != null) this.Controls.Add(_exportButton);
                if (_importButton != null) this.Controls.Add(_importButton);
                if (_vehicleTypeFilter != null) this.Controls.Add(_vehicleTypeFilter);
                if (_statusFilter != null) this.Controls.Add(_statusFilter);
                if (_vinSearchBox != null) this.Controls.Add(_vinSearchBox);

                LayoutVehicleSpecificControls();
            }
            catch (Exception ex)
            {
                HandleError($"Error initializing vehicle controls: {ex.Message}", "Control Error", ex);
            }
        }

        private void LayoutVehicleSpecificControls()
        {
            // Position the additional controls in the toolbar area
            if (_exportButton != null)
                _exportButton.Location = new Point(GetDpiAwareX(850), GetDpiAwareY(20));

            if (_importButton != null)
                _importButton.Location = new Point(GetDpiAwareX(960), GetDpiAwareY(20));

            if (_vehicleTypeFilter != null)
                _vehicleTypeFilter.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));

            if (_statusFilter != null)
                _statusFilter.Location = new Point(GetDpiAwareX(150), GetDpiAwareY(70));

            if (_vinSearchBox != null)
                _vinSearchBox.Location = new Point(GetDpiAwareX(280), GetDpiAwareY(70));
        }

        private void PopulateDataGrid()
        {
            if (_dataGrid == null)
            {
                Console.WriteLine("⚠️ DataGrid is null - cannot populate data");
                return;
            }

            try
            {
                Console.WriteLine($"Populating data grid with {_entities?.Count ?? 0} vehicles");

                _dataGrid.DataSource = null;
                _dataGrid.DataSource = _entities;
                _dataGrid.Refresh();

                Console.WriteLine("✅ Data grid populated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error populating vehicle grid: {ex.Message}");
                HandleError($"Error populating vehicle grid: {ex.Message}", "Display Error", ex);
            }
        }

        private void SetupFilters()
        {
            try
            {
                // Apply current filter settings to the data
                ApplyFilters();
            }
            catch (Exception ex)
            {
                HandleError($"Error setting up filters: {ex.Message}", "Filter Error", ex);
            }
        }

        private void ApplyFilters()
        {
            try
            {
                var filteredVehicles = _entities.AsEnumerable();

                // Apply vehicle type filter
                if (_vehicleTypeFilter?.SelectedItem?.ToString() != "All Types")
                {
                    var selectedType = _vehicleTypeFilter.SelectedItem?.ToString();
                    filteredVehicles = filteredVehicles.Where(v =>
                        string.Equals(v.Status, selectedType, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(v.Make, selectedType, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply status filter
                if (_statusFilter?.SelectedItem?.ToString() != "All Status")
                {
                    var selectedStatus = _statusFilter.SelectedItem?.ToString();
                    filteredVehicles = filteredVehicles.Where(v =>
                        string.Equals(v.Status, selectedStatus, StringComparison.OrdinalIgnoreCase)
                    );
                }

                // Apply VIN search
                var vinSearch = _vinSearchBox?.Text?.Trim();
                if (!string.IsNullOrEmpty(vinSearch))
                {
                    filteredVehicles = filteredVehicles.Where(v =>
                        v.VIN?.Contains(vinSearch, StringComparison.OrdinalIgnoreCase) == true
                    );
                }

                // Update grid
                _dataGrid.DataSource = null;
                _dataGrid.DataSource = filteredVehicles.ToList();
                _dataGrid.Refresh();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying filters: {ex.Message}", "Filter Error", ex);
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
                    saveFileDialog.Title = "Export Vehicle Data";
                    saveFileDialog.FileName = $"VehicleData_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // TODO: Implement actual export functionality
                        // For now, show a placeholder message
                        _messageService.ShowInfo($"Export functionality will be implemented.\nSelected file: {saveFileDialog.FileName}", "Export");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error during export: {ex.Message}", "Export Error", ex);
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
                    openFileDialog.Title = "Import Vehicle Data";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // TODO: Implement actual import functionality
                        // For now, show a placeholder message
                        _messageService.ShowInfo($"Import functionality will be implemented.\nSelected file: {openFileDialog.FileName}", "Import");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error during import: {ex.Message}", "Import Error", ex);
            }
        }

        private void VehicleTypeFilter_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying vehicle type filter: {ex.Message}", "Filter Error", ex);
            }
        }

        private void StatusFilter_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ApplyFilters();
            }
            catch (Exception ex)
            {
                HandleError($"Error applying status filter: {ex.Message}", "Filter Error", ex);
            }
        }

        private void VinSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Use a timer to avoid filtering on every keystroke
                if (_vinSearchTimer == null)
                {
                    _vinSearchTimer = new System.Windows.Forms.Timer();
                    _vinSearchTimer.Interval = 500; // 500ms delay
                    _vinSearchTimer.Tick += (s, args) =>
                    {
                        _vinSearchTimer.Stop();
                        ApplyFilters();
                    };
                }

                _vinSearchTimer.Stop();
                _vinSearchTimer.Start();
            }
            catch (Exception ex)
            {
                HandleError($"Error during VIN search: {ex.Message}", "Search Error", ex);
            }
        }
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of vehicle-specific resources
                _exportButton?.Dispose();
                _importButton?.Dispose();
                _vehicleTypeFilter?.Dispose();
                _statusFilter?.Dispose();
                _vinSearchBox?.Dispose();
                _vinSearchTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
