using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.DataGrid;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Fuel Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing fuel records with advanced SfDataGrid features
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class FuelManagementFormSyncfusion : BaseManagementForm<Fuel>
    {
        private readonly FuelRepository _fuelRepository;
        private readonly BusRepository _busRepository;
        private new readonly System.IServiceProvider _serviceProvider;
        private List<Bus> _buses = new();

        #region Properties Override
        protected override string FormTitle => "\u26fd Fuel Management";
        protected override string SearchPlaceholder => "Search fuels...";
        protected override string EntityName => "Fuel";
        #region Constructors
        public FuelManagementFormSyncfusion(System.IServiceProvider serviceProvider, FuelRepository fuelRepository, BusRepository busRepository, IMessageService messageService)
            : base(serviceProvider, messageService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            _busRepository = busRepository ?? throw new ArgumentNullException(nameof(busRepository));
        }
        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                // Load buses first for dropdowns/lookups
                LoadVehicles();
                var fuelRecords = _fuelRepository.GetAllFuelRecords();
                _entities = fuelRecords?.ToList() ?? new List<Fuel>();
                PopulateFuelGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error loading fuels: {ex.Message}", "$($EntityName) Error", ex);
                _entities = new List<Fuel>(); // Ensure _entities is never null
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
                var editForm = new FuelEditFormSyncfusion(_serviceProvider);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error adding fuel record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        protected override void EditSelectedEntity()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfo("Please select a fuel record to edit.");
                return;
            }
            try
            {
                var editForm = new FuelEditFormSyncfusion(_serviceProvider);
                editForm.Fuel = selectedFuel;
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                HandleError($"Error editing fuel record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        protected override void DeleteSelectedEntity()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfo("Please select a fuel record to delete.");
                return;
            }
            if (!ConfirmDelete("fuel record")) return;
            try
            {
                _fuelRepository.DeleteFuelRecord(selectedFuel.FuelID);
                RefreshGrid();
                ShowInfo("Fuel record deleted successfully.");
            }
            catch (Exception ex)
            {
                HandleError($"Error deleting fuel record: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        protected override void ViewEntityDetails()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfo("Please select a fuel record to view details.");
                return;
            }
            try
            {
                var details = $"Fuel Record Details:\n\n" +
                              $"ID: {selectedFuel.FuelID}\n" +
                              $"Date: {selectedFuel.FuelDate}\n" +
                              $"Location: {selectedFuel.FuelLocation}\n" +
                              $"Vehicle: {GetVehicleNumber(selectedFuel.VehicleFueledID)}\n" +
                              $"Gallons: {selectedFuel.FuelAmount:F2}\n" +
                              $"Cost: {selectedFuel.FuelCost:C}\n" +
                              $"Odometer: {selectedFuel.VehicleOdometerReading:N0}\n" +
                              $"Type: {selectedFuel.FuelType}";
                ShowInfo(details, "Fuel Details");
            }
            catch (Exception ex)
            {
                HandleError($"Error viewing fuel details: {ex.Message}", "$($EntityName) Error", ex);
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
                    _entities = new List<Fuel>();
                    return;
                }
                var filteredFuels = _entities.Where(f =>
                    (f.FuelLocation?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (f.FuelType?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (GetVehicleNumber(f.VehicleFueledID).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                _entities = filteredFuels;
                PopulateFuelGrid();
            }
            catch (Exception ex)
            {
                HandleError($"Error searching fuels: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;
            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();
            // Define columns for Fuel records
            var columns = new[]
            {
                new { Name = "FuelID", Header = "ID", Width = 60, Visible = false },
                new { Name = "FuelDate", Header = "📅 Date", Width = 120, Visible = true },
                new { Name = "FuelLocation", Header = "📍 Location", Width = 150, Visible = true },
                new { Name = "BusNumber", Header = "🚐 Vehicle", Width = 120, Visible = true },
                new { Name = "GallonsPurchased", Header = "⛽ Gallons", Width = 100, Visible = true },
                new { Name = "FuelCost", Header = "💰 Cost", Width = 100, Visible = true },
                new { Name = "OdometerReading", Header = "📊 Odometer", Width = 120, Visible = true },
                new { Name = "FuelType", Header = "🛢️ Type", Width = 100, Visible = true }
            };
            foreach (var col in columns)
            {
                var gridColumn = new Syncfusion.WinForms.DataGrid.GridTextColumn();
                gridColumn.MappingName = col.Name;
                gridColumn.HeaderText = col.Header;
                gridColumn.Width = GetDpiAwareSize(new Size(col.Width, 0)).Width;
                gridColumn.Visible = col.Visible;
                _dataGrid.Columns.Add(gridColumn);
            }
            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #region Helper Methods
        private void LoadVehicles()
        {
            try
            {
                // Defensive programming: Handle null repository results
                var buses = _busRepository.GetAllBuses();
                _buses = buses?.ToList() ?? new List<Bus>();
                Console.WriteLine($"✅ Loaded {_buses.Count} buses for fuel management");
            }
            catch (Exception ex)
            {
                // Ensure collection is never null even on error
                _buses = new List<Bus>();
                HandleError($"Error loading vehicles: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        private void PopulateFuelGrid()
        {
            if (_dataGrid == null) return;
            try
            {
                // Ensure _entities is not null before performing LINQ operations
                if (_entities == null)
                {
                    _entities = new List<Fuel>();
                }
                var fuelData = _entities.Select(f => new
                {
                    FuelID = f.FuelID,
                    FuelDate = f.FuelDate?.ToString() ?? "Unknown",
                    FuelLocation = f.FuelLocation ?? "Unknown",
                    BusNumber = GetVehicleNumber(f.VehicleFueledID),
                    GallonsPurchased = f.FuelAmount?.ToString("F2") ?? "0.00",
                    FuelCost = f.FuelCost?.ToString("C") ?? "$0.00",
                    OdometerReading = f.VehicleOdometerReading?.ToString("N0") ?? "0",
                    FuelType = f.FuelType ?? "Gasoline"
                }).ToList();
                _dataGrid.DataSource = fuelData;
            }
            catch (Exception ex)
            {
                HandleError($"Error populating fuel grid: {ex.Message}", "$($EntityName) Error", ex);
            }
        }
        private string GetVehicleNumber(int? BusId)
        {
            if (!BusId.HasValue) return "Unknown";
            var bus = _buses.FirstOrDefault(v => v.BusId == BusId.Value);
            return bus?.BusNumber?.ToString() ?? "Unknown";
        }
        #endregion // Helper Methods
        #endregion // Base Implementation Override
        #endregion // Constructors
        #endregion // Properties Override
    }
}

