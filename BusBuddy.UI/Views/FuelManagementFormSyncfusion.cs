using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.DataGrid.Enums;

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
        private readonly VehicleRepository _vehicleRepository;
        private List<Vehicle> _vehicles = new();

        #region Properties Override
        protected override string FormTitle => "⛽ Fuel Management";
        protected override string SearchPlaceholder => "Search fuels...";
        protected override string EntityName => "Fuel";
        #endregion

        #region Constructors
        public FuelManagementFormSyncfusion() : this(new FuelRepository(), new VehicleRepository()) { }

        public FuelManagementFormSyncfusion(FuelRepository fuelRepository, VehicleRepository vehicleRepository)
        {
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            LoadVehicles();
            LoadData();
        }
        #endregion

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                _entities = _fuelRepository.GetAllFuelRecords().ToList();
                PopulateFuelGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading fuels: {ex.Message}");
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                var editForm = new FuelEditFormSyncfusion();
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding fuel record: {ex.Message}");
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfoMessage("Please select a fuel record to edit.");
                return;
            }

            try
            {
                var editForm = new FuelEditFormSyncfusion(selectedFuel);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing fuel record: {ex.Message}");
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfoMessage("Please select a fuel record to delete.");
                return;
            }

            if (!ConfirmDelete("fuel record")) return;

            try
            {
                _fuelRepository.DeleteFuelRecord(selectedFuel.FuelID);
                RefreshGrid();
                ShowInfoMessage("Fuel record deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting fuel record: {ex.Message}");
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedFuel = GetSelectedEntity();
            if (selectedFuel == null)
            {
                ShowInfoMessage("Please select a fuel record to view details.");
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

                ShowInfoMessage(details, "Fuel Details");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error viewing fuel details: {ex.Message}");
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
                ShowErrorMessage($"Error searching fuels: {ex.Message}");
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
                new { Name = "VehicleNumber", Header = "🚐 Vehicle", Width = 120, Visible = true },
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
        #endregion

        #region Helper Methods
        private void LoadVehicles()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles().ToList();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
            }
        }

        private void PopulateFuelGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                var fuelData = _entities.Select(f => new
                {
                    FuelID = f.FuelID,
                    FuelDate = f.FuelDate?.ToString() ?? "Unknown",
                    FuelLocation = f.FuelLocation ?? "Unknown",
                    VehicleNumber = GetVehicleNumber(f.VehicleFueledID),
                    GallonsPurchased = f.FuelAmount?.ToString("F2") ?? "0.00",
                    FuelCost = f.FuelCost?.ToString("C") ?? "$0.00",
                    OdometerReading = f.VehicleOdometerReading?.ToString("N0") ?? "0",
                    FuelType = f.FuelType ?? "Gasoline"
                }).ToList();

                _dataGrid.DataSource = fuelData;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error populating fuel grid: {ex.Message}");
            }
        }

        private string GetVehicleNumber(int? vehicleId)
        {
            if (!vehicleId.HasValue) return "Unknown";

            var vehicle = _vehicles.FirstOrDefault(v => v.Id == vehicleId.Value);
            return vehicle?.VehicleNumber?.ToString() ?? "Unknown";
        }
        #endregion
    }
}
