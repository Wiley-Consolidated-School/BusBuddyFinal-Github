using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Vehicle Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing vehicle fleet with grid view and CRUD operations
    /// </summary>
    public class VehicleManagementFormSyncfusion : BaseManagementForm<Vehicle>
    {
        private readonly IVehicleRepository _vehicleRepository;
        #region Properties Override
        protected override string FormTitle => "🚗 Vehicle Management";
        protected override string SearchPlaceholder => "Search vehicles...";
        protected override string EntityName => "Vehicle";
        #endregion

#region Constructors
        public VehicleManagementFormSyncfusion()
            : this(new VehicleRepository(), new MessageBoxService())
        {
        }

        public VehicleManagementFormSyncfusion(IVehicleRepository vehicleRepository) : this(vehicleRepository, new MessageBoxService())
        {
        }

        public VehicleManagementFormSyncfusion(IVehicleRepository vehicleRepository, IMessageService messageService) : base(messageService)
        {
            try
            {
                Console.WriteLine("🔍 Creating VehicleManagementFormSyncfusion using dependency injection");

                _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));

                // Set the repository in the base class before any initialization
                SetRepository(_vehicleRepository);

                // Force test the repository to ensure it's working
                var vehicles = _vehicleRepository.GetAllVehicles();
                var count = vehicles?.Count() ?? 0;
                Console.WriteLine($"✅ VehicleRepository working: {count} vehicles available");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in VehicleManagementFormSyncfusion constructor: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                var errorMessage = $"Failed to initialize vehicle repository: {ex.Message}";
                _messageService.ShowError($"{errorMessage}\n\nPlease check database connection.", "Repository Initialization Error");
                throw new InvalidOperationException(errorMessage, ex);
            }
            // NOTE: LoadData() is called by the base class after all controls are initialized
        }
        #endregion

#region Base Implementation Override
        protected override void LoadDataFromRepository()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAllVehicles() ?? new List<Vehicle>();
                _entities = vehicles.ToList();

                if (_dataGrid != null && !BusBuddyThemeManager.IsTestMode)
                {
                    PopulateVehicleGrid();
                }

                Console.WriteLine($"✅ Loaded {vehicles.Count()} vehicles successfully");
            }
            catch (Exception ex)
            {
                HandleError($"Error loading vehicles: {ex.Message}", "🚗 Vehicle Management", ex);
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                var vehicleForm = new VehicleFormSyncfusion();
                if (vehicleForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error adding new vehicle: {ex.Message}", "Add Vehicle Error", ex);
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfo("Please select a vehicle to edit.");
                return;
            }

            try
            {
                var vehicleForm = new VehicleFormSyncfusion(selectedVehicle);
                if (vehicleForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error editing vehicle: {ex.Message}", "Edit Vehicle Error", ex);
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfo("Please select a vehicle to delete.");
                return;
            }

            if (!ConfirmAction($"Are you sure you want to delete vehicle {selectedVehicle.VehicleNumber}?", "Confirm Delete")) return;

            try
            {
                _vehicleRepository.DeleteVehicle(selectedVehicle.Id);
                RefreshGrid();
                ShowInfo("Vehicle deleted successfully.");
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting vehicle: {ex.Message}", "Delete Vehicle Error", ex);
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfo("Please select a vehicle to view details.");
                return;
            }

            try
            {
                var details = $"Vehicle Details:\n\n" +
                            $"ID: {selectedVehicle.Id}\n" +
                            $"Vehicle Number: {selectedVehicle.VehicleNumber}\n" +
                            $"Make: {selectedVehicle.Make}\n" +
                            $"Model: {selectedVehicle.Model}\n" +
                            $"Year: {selectedVehicle.Year}\n" +
                            $"Capacity: {selectedVehicle.Capacity}\n" +
                            $"Fuel Type: {selectedVehicle.FuelType}\n" +
                            $"Status: {selectedVehicle.Status}";

                ShowInfo(details, "Vehicle Details");
            }
            catch (Exception ex)
            {
                HandleError($"Error viewing vehicle details: {ex.Message}", "View Vehicle Details Error", ex);
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

                // Ensure _entities is never null before LINQ operations
                if (_entities == null)
                {
                    _entities = new List<Vehicle>();
                }

                var filteredVehicles = _entities.Where(v =>
                    (v.VehicleNumber?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (v.Make?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (v.Model?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (v.Status?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                ).ToList();

                _entities = filteredVehicles;
                PopulateVehicleGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error searching vehicles: {ex.Message}", "Search Vehicles Error", ex);
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null)
            {
                if (BusBuddyThemeManager.IsTestMode)
                    Console.WriteLine("🧪 VehicleManagementForm: Skipping column setup - test mode enabled");
                return;
            }

            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();

            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Id", HeaderText = "ID", Visible = false });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "VehicleNumber", HeaderText = "Vehicle #", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Make", HeaderText = "Make", Width = GetDpiAwareWidth(130) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Model", HeaderText = "Model", Width = GetDpiAwareWidth(150) });
            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Year", HeaderText = "Year", Width = GetDpiAwareWidth(80) });
            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "Capacity", HeaderText = "Capacity", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "FuelType", HeaderText = "Fuel Type", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "Status", HeaderText = "Status", Width = GetDpiAwareWidth(100) });

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #endregion

#region Helper Methods
        private void PopulateVehicleGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                // Ensure _entities is never null
                if (_entities == null)
                {
                    _entities = new List<Vehicle>();
                }

                var vehicleData = _entities.Select(v => new
                {
                    Id = v.Id,
                    VehicleNumber = v.VehicleNumber ?? "Unknown",
                    Make = v.Make ?? "Unknown",
                    Model = v.Model ?? "Unknown",
                    Year = v.Year,
                    Capacity = v.Capacity,
                    FuelType = v.FuelType ?? "Unknown",
                    Status = v.Status ?? "Unknown"
                }).ToList();

                _dataGrid.DataSource = vehicleData;
            }
            catch (Exception ex)
            {
                HandleError($"Error populating vehicle grid: {ex.Message}", "Grid Population Error", ex);
            }
        }
        #endregion

#region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // CRITICAL: Suppress finalization to prevent hanging processes
                    System.GC.SuppressFinalize(this);

                    // Dispose repository if it implements IDisposable
                    if (_vehicleRepository is IDisposable disposableRepository)
                    {
                        disposableRepository.Dispose();
                        System.Console.WriteLine("🧽 VehicleRepository disposed");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine($"⚠️ Error disposing VehicleManagementForm: {ex.Message}");
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}

