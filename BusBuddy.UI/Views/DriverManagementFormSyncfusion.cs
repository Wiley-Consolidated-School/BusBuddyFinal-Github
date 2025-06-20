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
using Syncfusion.WinForms.DataGrid.Enums;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Driver Management Form - Standardized implementation using BaseManagementForm
    /// Form for managing drivers with grid view and CRUD operations
    /// </summary>
    public class DriverManagementFormSyncfusion : BaseManagementForm<Driver>
    {
        private readonly IDriverRepository _driverRepository;

        #region Properties Override
        protected override string FormTitle => "👨‍✈️ Driver Management";
        protected override string SearchPlaceholder => "Search drivers...";
        protected override string EntityName => "Driver";
        #endregion

        #region Constructors
        public DriverManagementFormSyncfusion() : this(new DriverRepository()) { }

        public DriverManagementFormSyncfusion(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            LoadData();
        }
        #endregion

        #region Base Implementation Override
        protected override void LoadData()
        {
            try
            {
                _entities = _driverRepository.GetAllDrivers().ToList();
                PopulateDriverGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading drivers: {ex.Message}");
            }
        }

        protected override void AddNewEntity()
        {
            try
            {
                var driverForm = new DriverEditFormSyncfusion();
                if (driverForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error adding new driver: {ex.Message}");
            }
        }

        protected override void EditSelectedEntity()
        {
            var selectedDriver = GetSelectedEntity();
            if (selectedDriver == null)
            {
                ShowInfoMessage("Please select a driver to edit.");
                return;
            }

            try
            {
                var driverForm = new DriverEditFormSyncfusion(selectedDriver);
                if (driverForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshGrid();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error editing driver: {ex.Message}");
            }
        }

        protected override void DeleteSelectedEntity()
        {
            var selectedDriver = GetSelectedEntity();
            if (selectedDriver == null)
            {
                ShowInfoMessage("Please select a driver to delete.");
                return;
            }

            if (!ConfirmDelete("driver")) return;

            try
            {
                _driverRepository.DeleteDriver(selectedDriver.DriverID);
                RefreshGrid();
                ShowInfoMessage("Driver deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error deleting driver: {ex.Message}");
            }
        }

        protected override void ViewEntityDetails()
        {
            var selectedDriver = GetSelectedEntity();
            if (selectedDriver == null)
            {
                ShowInfoMessage("Please select a driver to view details.");
                return;
            }

            try
            {
                var details = $"Driver Details:\n\n" +
                            $"ID: {selectedDriver.DriverID}\n" +
                            $"Name: {selectedDriver.DriverName}\n" +
                            $"Phone: {selectedDriver.DriverPhone}\n" +
                            $"Email: {selectedDriver.DriverEmail}\n" +
                            $"Address: {selectedDriver.Address}\n" +
                            $"City: {selectedDriver.City}, {selectedDriver.State} {selectedDriver.Zip}\n" +
                            $"License Type: {selectedDriver.DriversLicenseType}\n" +
                            $"Training Complete: {(selectedDriver.IsTrainingComplete ? "Yes" : "No")}";

                ShowInfoMessage(details, "Driver Details");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error viewing driver details: {ex.Message}");
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

                var filteredDrivers = _entities.Where(d =>
                    (d.DriverName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.DriverPhone?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.DriverEmail?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                    (d.DriversLicenseType?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                ).ToList();

                _entities = filteredDrivers;
                PopulateDriverGrid();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error searching drivers: {ex.Message}");
            }
        }

        protected override void SetupDataGridColumns()
        {
            if (_dataGrid == null) return;

            _dataGrid.AutoGenerateColumns = false;
            _dataGrid.Columns.Clear();

            _dataGrid.Columns.Add(new GridNumericColumn() { MappingName = "DriverID", HeaderText = "ID", Visible = false });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "DriverName", HeaderText = "Name", Width = GetDpiAwareWidth(150) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "DriverPhone", HeaderText = "Phone", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "DriverEmail", HeaderText = "Email", Width = GetDpiAwareWidth(180) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "City", HeaderText = "City", Width = GetDpiAwareWidth(100) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "State", HeaderText = "State", Width = GetDpiAwareWidth(80) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "DriversLicenseType", HeaderText = "License Type", Width = GetDpiAwareWidth(120) });
            _dataGrid.Columns.Add(new GridTextColumn() { MappingName = "TrainingComplete", HeaderText = "Training", Width = GetDpiAwareWidth(100) });

            Console.WriteLine($"✅ ENHANCED GRID: Setup {_dataGrid.Columns.Count} columns for {this.Text}");
        }
        #endregion

        #region Helper Methods
        private void PopulateDriverGrid()
        {
            if (_dataGrid == null) return;

            try
            {
                var driverData = _entities.Select(d => new
                {
                    DriverID = d.DriverID,
                    DriverName = d.DriverName ?? "Unknown",
                    DriverPhone = d.DriverPhone ?? "Unknown",
                    DriverEmail = d.DriverEmail ?? "Unknown",
                    City = d.City ?? "Unknown",
                    State = d.State ?? "Unknown",
                    DriversLicenseType = d.DriversLicenseType ?? "Unknown",
                    TrainingComplete = d.IsTrainingComplete ? "Yes" : "No"
                }).ToList();

                _dataGrid.DataSource = driverData;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error populating driver grid: {ex.Message}");
            }        }
        #endregion
    }
}
