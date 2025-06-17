using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;

namespace BusBuddy.UI.Views
{
    public class DriverManagementForm : StandardDataForm
    {
        private readonly IDriverRepository _driverRepository;
        private DataGridView? _driverGrid;
        private Button? _addButton;
        private Button? _editButton;
        private Button? _deleteButton;
        private Button? _detailsButton;
        private MaterialSkin.Controls.MaterialTextBox _searchBox;
        private Button? _searchButton;
        private List<Driver> _drivers = new List<Driver>();

        public DriverManagementForm() : this(new DriverRepository()) { }

        public DriverManagementForm(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadDrivers();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Driver Management"
            this.Text = "Driver Management";
            this.ClientSize = new System.Drawing.Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create toolbar (y=20) with buttons: Add New, Edit, Delete, Details, Search
            _addButton = CreateButton("Add New", 20, 20, (s, e) => AddNewDriver());
            _editButton = CreateButton("Edit", 130, 20, (s, e) => EditSelectedDriver());
            _deleteButton = CreateButton("Delete", 240, 20, (s, e) => DeleteSelectedDriver());
            _detailsButton = CreateButton("Details", 350, 20, (s, e) => ViewDriverDetails());

            // Search textbox at x=550, width=150
            CreateLabel("Search:", 500, 25);
            _searchBox = CreateTextBox(550, 20, 150);
            _searchButton = CreateButton("Search", 710, 20, (s, e) => SearchDrivers());

            // Create DataGridView (1150x650, y=60, DPI-aware, auto-size columns, full-row select, read-only)
            _driverGrid = new DataGridView();
            _driverGrid.Location = new System.Drawing.Point(20, 60);
            _driverGrid.Size = new System.Drawing.Size(1150, 650);
            _driverGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _driverGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _driverGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _driverGrid.ReadOnly = true;
            _driverGrid.AllowUserToAddRows = false;
            _driverGrid.AllowUserToDeleteRows = false;
            _driverGrid.MultiSelect = false;
            _driverGrid.AllowUserToResizeColumns = true;
            _driverGrid.AllowUserToResizeRows = true;
            _driverGrid.ScrollBars = ScrollBars.Both;
            _driverGrid.DataBindingComplete += (s, e) => {
                if (_driverGrid.Columns.Contains("DriverID"))
                    _driverGrid.Columns["DriverID"].Visible = false;
            };
            this.Controls.Add(_driverGrid);
            _driverGrid.CellDoubleClick += (s, e) => EditSelectedDriver();
            _driverGrid.SelectionChanged += DriverGrid_SelectionChanged;

            // Disable edit/delete/details buttons initially
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
        }

        private void DriverGrid_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = _driverGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;
            _detailsButton.Enabled = hasSelection;
        }

        private void LoadDrivers()
        {
            try
            {
                _drivers = _driverRepository.GetAllDrivers();
                _driverGrid.DataSource = null;
                _driverGrid.DataSource = _drivers;
                if (_driverGrid.Columns.Count > 0)
                {
                    _driverGrid.Columns["DriverID"].HeaderText = "ID";                    _driverGrid.Columns["DriverName"].HeaderText = "Name";
                    _driverGrid.Columns["DriverPhone"].HeaderText = "Phone";
                    _driverGrid.Columns["DriverEmail"].HeaderText = "Email";
                    _driverGrid.Columns["DriversLicenseType"].HeaderText = "License";
                    _driverGrid.Columns["TrainingComplete"].HeaderText = "Training";
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error loading drivers: {ex.Message}");
            }
        }

        private void AddNewDriver()
        {
            try
            {
                using var editForm = new DriverEditFormSyncfusion();
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Driver != null)
                {
                    _driverRepository.AddDriver(editForm.Driver);
                    MessageBox.Show("Driver added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDrivers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding driver: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedDriver()
        {
            if (_driverGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a driver to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
                var driverToEdit = _driverRepository.GetDriverById(selectedId);

                if (driverToEdit == null)
                {
                    MessageBox.Show("Could not find the selected driver.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new DriverEditFormSyncfusion(driverToEdit);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Driver != null)
                {
                    _driverRepository.UpdateDriver(editForm.Driver);
                    MessageBox.Show("Driver updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDrivers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing driver: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DeleteSelectedDriver()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;

            var result = MessageBox.Show("Are you sure you want to delete this driver?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            try
            {
                _driverRepository.DeleteDriver(selectedId);
                LoadDrivers();
                MessageBox.Show("Driver deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting driver: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewDriverDetails()
        {
            if (_driverGrid.SelectedRows.Count == 0)
                return;
            int selectedId = (int)_driverGrid.SelectedRows[0].Cells["DriverID"].Value;
            var driver = _driverRepository.GetDriverById(selectedId);
            if (driver != null)
            {
                MessageBox.Show($"Driver Details:\nName: {driver.DriverName}\nPhone: {driver.DriverPhone}\nEmail: {driver.DriverEmail}\nAddress: {driver.Address}, {driver.City}, {driver.State} {driver.Zip}\nLicense: {driver.DriversLicenseType}\nTraining Complete: {driver.IsTrainingComplete}",
                    "Driver Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not load driver details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchDrivers()
        {
            string searchTerm = _searchBox.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadDrivers();
                return;
            }

            List<Driver> filtered = _drivers.Where(d =>
                (d.DriverName?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverPhone?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverEmail?.ToLower().Contains(searchTerm) == true) ||
                (d.DriversLicenseType?.ToLower().Contains(searchTerm) == true)
            ).ToList();
            _driverGrid.DataSource = null;
            _driverGrid.DataSource = filtered;
        }
    }
}

