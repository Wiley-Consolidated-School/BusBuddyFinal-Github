using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Maintenance Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing maintenance records with grid view and CRUD operations
    /// </summary>
    public class MaintenanceManagementFormSyncfusion : BaseManagementForm<Maintenance>
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private List<Vehicle> _vehicles = new List<Vehicle>();
        #region Properties Override
        protected override string FormTitle => "🔧 Maintenance Management";
        protected override string SearchPlaceholder => "Search maintenance...";
        protected override string EntityName => "Maintenance";
        #region Constructors
        public MaintenanceManagementFormSyncfusion() : this(new MaintenanceRepository(), new VehicleRepository()) { }

        public MaintenanceManagementFormSyncfusion(IMaintenanceRepository maintenanceRepository, IVehicleRepository vehicleRepository)
        {
            _maintenanceRepository = maintenanceRepository ?? throw new ArgumentNullException(nameof(maintenanceRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            // NOTE: LoadData() and LoadVehicles() are called by the base class after all controls are initialized
        }
        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                if (_maintenanceRepository == null || _vehicleRepository == null)
                {
                    ShowErrorMessage("Error loading maintenance records: Repository not initialized.");
                    _entities = new List<Maintenance>();
                    return;
                }
                // Load vehicles first for dropdowns/lookups
                LoadVehicles();

                var maintenanceRecords = _maintenanceRepository.GetAllMaintenanceRecords();
                _entities = maintenanceRecords?.ToList() ?? new List<Maintenance>();
                PopulateMaintenanceGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error loading maintenance records: {ex.Message}", "$($EntityName) Error", ex);
                _entities = new List<Maintenance>(); // Ensure _entities is never null
            }
        }

        protected override void LoadDataFromRepository()
        {
            LoadData(); // Delegate to existing LoadData implementation
        }

        protected override void AddNewEntity()
        {
            try
            {
                var maintenanceForm = new MaintenanceEditFormSyncfusion();
                if (maintenanceForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error adding new maintenance record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedMaintenance = GetSelectedEntity();
            if (selectedMaintenance == null)
            {
                ShowInfo("Please select a maintenance record to edit.");
                return;
            }

            try
            {
                var maintenanceForm = new MaintenanceEditFormSyncfusion(selectedMaintenance);
                if (maintenanceForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error editing maintenance record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedMaintenance = GetSelectedEntity();
            if (selectedMaintenance == null)
            {
                ShowInfo("Please select a maintenance record to delete.");
                return;
            }

            if (!ConfirmDelete("maintenance record")) return;

            try
            {
                _maintenanceRepository.DeleteMaintenanceRecord(selectedMaintenance.MaintenanceID);
                RefreshGrid();
                ShowInfo("Maintenance record deleted successfully.");
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting maintenance record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedMaintenance = GetSelectedEntity();
            if (selectedMaintenance == null)
            {
                ShowInfo("Please select a maintenance record to view details.");
                return;
            }

            try
            {
                var details = $"Maintenance Record Details:\n\n" +
                            $"ID: {selectedMaintenance.MaintenanceID}\n" +
                            $"Date: {selectedMaintenance.Date}\n" +
                            $"Vehicle: {GetVehicleName(selectedMaintenance.VehicleID)}\n" +
                            $"Odometer: {selectedMaintenance.OdometerReading:N0}\n" +
                            $"Type: {selectedMaintenance.MaintenanceCompleted}\n" +
                            $"Vendor: {selectedMaintenance.Vendor}\n" +
                            $"Cost: {selectedMaintenance.RepairCost:C}\n" +
                            $"Notes: {selectedMaintenance.Notes}";

                ShowInfo(details, "Maintenance Details");
            }
            catch (Exception ex)
            {
                HandleError($"Error viewing maintenance details: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void SearchEntities()
        {
            if (_searchBox?.Text == null) return;

            try
            {
                var searchTerm = _searchBox.Text.Trim();

                if (string.IsNullOrEmpty(searchTerm) || searchTerm == SearchPlaceholder)
                {
                    LoadData();
                    return;
                }

                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<Maintenance>();
                    return;
                }

                var filteredMaintenance = _entities.Where(m =>
                    (m.MaintenanceCompleted?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (m.Vendor?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (GetVehicleName(m.VehicleID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                _entities = filteredMaintenance;
                PopulateMaintenanceGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error searching maintenance records: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();

            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "MaintenanceID", HeaderText = "ID", Visible = false });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Date", HeaderText = "Date", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "VehicleName", HeaderText = "Vehicle", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "OdometerReading", HeaderText = "Odometer", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "MaintenanceCompleted", HeaderText = "Type", Width = GetDpiAwareWidth(150) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Vendor", HeaderText = "Vendor", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "RepairCost", HeaderText = "Cost", Width = GetDpiAwareWidth(100) });

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #region Helper Methods
        private void LoadVehicles()
        {
            try
            {
                // Defensive programming: Handle null repository results
                var vehicles = _vehicleRepository.GetAllVehicles();
                _vehicles = vehicles?.ToList() ?? new List<Vehicle>();
                Console.WriteLine($"✅ Loaded {_vehicles.Count} vehicles for maintenance");
            }
            catch (Exception ex)
            {
                // Ensure collection is never null even on error
                _vehicles = new List<Vehicle>();
                HandleError($"Error loading vehicles: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        private void PopulateMaintenanceGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<Maintenance>();
                }

                var maintenanceData = _entities.Select(m => new
                {
                    MaintenanceID = m.MaintenanceID,
                    Date = m.Date ?? "Unknown",
                    VehicleName = GetVehicleName(m.VehicleID),
                    OdometerReading = m.OdometerReading ?? 0,
                    MaintenanceCompleted = m.MaintenanceCompleted ?? "Unknown",
                    Vendor = m.Vendor ?? "Unknown",
                    RepairCost = m.RepairCost?.ToString("C") ?? "$0.00"
                }).ToList();

                _dataGrid.DataSource = maintenanceData;
            }
            catch (Exception ex)
            {
                HandleError($"Error populating maintenance grid: {ex.Message}", "$($EntityName) Error", ex);
            }
        }

        private string GetVehicleName(int? vehicleId)
        {
            if (!vehicleId.HasValue) return "Unknown";

            var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId.Value);
            return vehicle?.VehicleNumber ?? "Unknown";        }    }

        #endregion

        #endregion

        #endregion

        #endregion
}
