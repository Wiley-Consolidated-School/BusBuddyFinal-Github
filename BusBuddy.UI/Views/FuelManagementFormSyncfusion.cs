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
using Syncfusion.WinForms.DataGrid.Enums;
using System.IO;

namespace BusBuddy.UI.Views
{
    public partial class FuelManagementFormSyncfusion : Form
    {
        private readonly FuelRepository _fuelRepository;
        private readonly VehicleRepository _vehicleRepository;
        private List<Fuel> _fuels = new();
        private List<Vehicle> _vehicles = new();

        private SfDataGrid _fuelGrid;
        private TextBox _searchTextBox;
        private Button _addButton, _editButton, _deleteButton, _detailsButton, _searchButton;

        public FuelManagementFormSyncfusion()
        {
            _fuelRepository = new FuelRepository();
            _vehicleRepository = new VehicleRepository();

            InitializeComponents();
            SetupUI();
            LoadData();
        }

        private void InitializeComponents()
        {
            // Initialize the form components
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1200, 800);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            Text = "⛽ Fuel Management - Enhanced Syncfusion";
            Size = new Size(1200, 800);
            BackColor = EnhancedThemeService.BackgroundColor;

            SetupGrid();
            SetupControls();
            SetupLayout();
        }

        private void SetupGrid()
        {
            _fuelGrid = new SfDataGrid
            {
                Dock = DockStyle.Fill,
                AllowEditing = false,
                AllowFiltering = true,
                AllowSorting = true,
                ShowGroupDropArea = true,
                AutoSizeColumnsMode = AutoSizeColumnsMode.Fill,
                SelectionMode = GridSelectionMode.Single
            };

            // Apply visual styling
            _fuelGrid.Style.HeaderStyle.BackColor = EnhancedThemeService.HeaderColor;
            _fuelGrid.Style.HeaderStyle.TextColor = Color.White;

            _fuelGrid.Columns.Add(new GridTextColumn { MappingName = "FuelID", HeaderText = "ID", Width = 80 });
            _fuelGrid.Columns.Add(new GridTextColumn { MappingName = "FuelDate", HeaderText = "Date", Width = 120 });
            _fuelGrid.Columns.Add(new GridTextColumn { MappingName = "FuelLocation", HeaderText = "Location", Width = 150 });
        }

        private void SetupControls()
        {
            var topPanel = new Panel { Height = 60, Dock = DockStyle.Top, BackColor = EnhancedThemeService.BackgroundColor };

            _searchTextBox = new TextBox {
                PlaceholderText = "Search fuels...",
                Location = new Point(10, 20),
                Size = new Size(200, 25)
            };

            _searchButton = new Button {
                Text = "🔍 Search",
                Location = new Point(220, 18),
                Size = new Size(80, 30),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White
            };

            _addButton = new Button {
                Text = "➕ Add",
                Location = new Point(310, 18),
                Size = new Size(70, 30),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White
            };

            _editButton = new Button {
                Text = "✏️ Edit",
                Location = new Point(390, 18),
                Size = new Size(70, 30),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White
            };

            _deleteButton = new Button {
                Text = "🗑️ Delete",
                Location = new Point(470, 18),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White
            };

            _detailsButton = new Button {
                Text = "👁️ Details",
                Location = new Point(560, 18),
                Size = new Size(80, 30),
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = Color.White
            };

            topPanel.Controls.AddRange(new Control[] {
                _searchTextBox, _searchButton, _addButton, _editButton, _deleteButton, _detailsButton
            });

            Controls.Add(topPanel);
        }

        private void SetupLayout()
        {
            var gridPanel = new Panel { Dock = DockStyle.Fill };
            gridPanel.Controls.Add(_fuelGrid);
            Controls.Add(gridPanel);

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _addButton.Click += (s, e) => AddNewFuel();
            _editButton.Click += (s, e) => EditSelectedFuel();
            _deleteButton.Click += (s, e) => DeleteSelectedFuel();
            _detailsButton.Click += (s, e) => ViewFuelDetails();
            _searchButton.Click += (s, e) => SearchFuels();

            _searchTextBox.KeyPress += (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                    SearchFuels();
            };
        }

        private void LoadData()
        {
            LoadFuels();
        }

        private void LoadFuels()
        {
            try
            {
                _fuels = _fuelRepository.GetAllFuelRecords();
                _fuelGrid.DataSource = _fuels;
                UpdateStatusMessage($"Loaded {_fuels.Count} fuel records");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load fuel data: {ex.Message}");
            }
        }

        private void AddNewFuel()
        {
            // TODO: Implement add fuel functionality
            MessageBox.Show("Add fuel functionality will be implemented soon.", "Add Fuel",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditSelectedFuel()
        {
            // TODO: Implement edit fuel functionality
            MessageBox.Show("Edit fuel functionality will be implemented soon.", "Edit Fuel",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteSelectedFuel()
        {
            // TODO: Implement delete fuel functionality
            MessageBox.Show("Delete fuel functionality will be implemented soon.", "Delete Fuel",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ViewFuelDetails()
        {
            // TODO: Implement view details functionality
            MessageBox.Show("View details functionality will be implemented soon.", "Fuel Details",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SearchFuels()
        {
            var searchTerm = _searchTextBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                _fuelGrid.DataSource = _fuels;
                return;
            }

            var filteredFuels = _fuels.Where(f =>
                f.FuelLocation?.ToLower().Contains(searchTerm) == true ||
                f.FuelDate?.ToLower().Contains(searchTerm) == true
            ).ToList();

            _fuelGrid.DataSource = filteredFuels;
            UpdateStatusMessage($"Found {filteredFuels.Count} matching fuel records");
        }

        private void UpdateStatusMessage(string message)
        {
            // TODO: Implement status bar if needed
            Console.WriteLine($"Fuel Management: {message}");
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fuelGrid?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
