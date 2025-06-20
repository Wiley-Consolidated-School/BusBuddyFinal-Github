using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
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
        public VehicleManagementFormSyncfusion() : this(new VehicleRepository()) { }

        public VehicleManagementFormSyncfusion(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            LoadData();
        }
        #endregion

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                _entities = _vehicleRepository.GetAllVehicles().ToList();
                PopulateVehicleGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading vehicles: {ex.Message}");
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
                ShowErrorMessage($"Error adding new vehicle: {ex.Message}");
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfoMessage("Please select a vehicle to edit.");
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
                ShowErrorMessage($"Error editing vehicle: {ex.Message}");
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfoMessage("Please select a vehicle to delete.");
                return;
            }

            if (!ConfirmDelete("vehicle")) return;

            try
            {
                _vehicleRepository.DeleteVehicle(selectedVehicle.Id);
                RefreshGrid();
                ShowInfoMessage("Vehicle deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting vehicle: {ex.Message}");
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedVehicle = GetSelectedEntity();
            if (selectedVehicle == null)
            {
                ShowInfoMessage("Please select a vehicle to view details.");
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

                ShowInfoMessage(details, "Vehicle Details");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error viewing vehicle details: {ex.Message}");
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
                ShowErrorMessage($"Error searching vehicles: {ex.Message}");
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

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
                ShowErrorMessage($"Error populating vehicle grid: {ex.Message}");
            }        }
        #endregion
    }
}
