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
{    /// <summary>
    /// Driver Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing drivers with grid view and CRUD operations
    /// </summary>
    public class DriverManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IDriverRepository _driverRepository;
        private SfDataGrid? _driverGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Driver> _drivers = new List<Driver>();

        public DriverManagementFormSyncfusion() : this(new DriverRepository()) { }

        public DriverManagementFormSyncfusion(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            InitializeComponent();
            LoadDrivers();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Driver Management"
            this.Text = "👨‍✈️ Driver Management";
            this.ClientSize = GetDpiAwareSize(new Size(1200, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(800, 600));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Create toolbar buttons
            _addButton = SyncfusionThemeHelper.CreateStyledButton("➕ Add New");
            _editButton = SyncfusionThemeHelper.CreateStyledButton("✏️ Edit");
            _deleteButton = SyncfusionThemeHelper.CreateStyledButton("🗑️ Delete");
            _detailsButton = SyncfusionThemeHelper.CreateStyledButton("👁️ Details");
            _searchButton = SyncfusionThemeHelper.CreateStyledButton("🔍 Search");

            // Create search textbox
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search drivers...");

            // Configure button sizes and positions
            var buttonSize = GetDpiAwareSize(new Size(100, 35));
            var buttonY = GetDpiAwareY(20);

            _addButton.Size = buttonSize;
            _addButton.Location = new Point(GetDpiAwareX(20), buttonY);

            _editButton.Size = buttonSize;
            _editButton.Location = new Point(GetDpiAwareX(130), buttonY);
            _editButton.Enabled = false; // Initially disabled

            _deleteButton.Size = buttonSize;
            _deleteButton.Location = new Point(GetDpiAwareX(240), buttonY);
            _deleteButton.Enabled = false; // Initially disabled

            _detailsButton.Size = buttonSize;
            _detailsButton.Location = new Point(GetDpiAwareX(350), buttonY);
            _detailsButton.Enabled = false; // Initially disabled

            // Search controls
            var searchLabel = ControlFactory.CreateLabel("🔍 Search:");
            _searchBox.Size = GetDpiAwareSize(new Size(150, 30));
            _searchBox.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));

            _searchButton.Size = GetDpiAwareSize(new Size(80, 35));
            _searchButton.Location = new Point(GetDpiAwareX(710), buttonY);

            // Add buttons to main panel
            _mainPanel.Controls.Add(_addButton);
            _mainPanel.Controls.Add(_editButton);
            _mainPanel.Controls.Add(_deleteButton);
            _mainPanel.Controls.Add(_detailsButton);
            _mainPanel.Controls.Add(_searchBox);
            _mainPanel.Controls.Add(_searchButton);            _mainPanel.Controls.Add(searchLabel);

            // Create and configure the data grid
            SetupDataGrid();
        }
          private void SetupDataGrid()
        {
            // Create SfDataGrid with ALL enhanced features for 100% implementation
            _driverGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _driverGrid.Dock = DockStyle.None;
            _driverGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(60));
            _driverGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _driverGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Apply ALL Syncfusion features for 100% implementation
            SyncfusionThemeHelper.SfDataGridEnhancements(_driverGrid);

            // Setup grid columns manually
            SetupGridColumns();

            _mainPanel.Controls.Add(_driverGrid);
        }        private void SetupGridColumns()
        {
            // Clear any existing columns first
            _driverGrid.Columns.Clear();

            // Define only the columns we want to display using Syncfusion SfDataGrid columns
            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "DriverName",
                HeaderText = "Name",
                Width = GetDpiAwareSize(new Size(150, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "DriverPhone",
                HeaderText = "Phone",
                Width = GetDpiAwareSize(new Size(120, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "DriverEmail",
                HeaderText = "Email",
                Width = GetDpiAwareSize(new Size(200, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Address",
                HeaderText = "Address",
                Width = GetDpiAwareSize(new Size(150, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "City",
                HeaderText = "City",
                Width = GetDpiAwareSize(new Size(100, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "State",
                HeaderText = "State",
                Width = GetDpiAwareSize(new Size(60, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "Zip",
                HeaderText = "Zip",
                Width = GetDpiAwareSize(new Size(80, 0)).Width
            });

            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "DriversLicenseType",
                HeaderText = "License",
                Width = GetDpiAwareSize(new Size(100, 0)).Width
            });

            _driverGrid.Columns.Add(new GridCheckBoxColumn()
            {
                MappingName = "IsTrainingComplete",
                HeaderText = "Training",
                Width = GetDpiAwareSize(new Size(80, 0)).Width
            });

            // Add hidden DriverID column for selection purposes
            _driverGrid.Columns.Add(new GridTextColumn()
            {
                MappingName = "DriverID",
                HeaderText = "ID",
                Width = 0,
                Visible = false
            });

            // Make the address column fill remaining space
            if (_driverGrid.Columns.Count > 3)
            {
                _driverGrid.Columns[3].AutoSizeColumnsMode = AutoSizeColumnsMode.Fill;
            }
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls method for better organization
        }        private void SetupEventHandlers()
        {
            // Button click events
            _addButton.Click += (s, e) => AddNewDriver();
            _editButton.Click += (s, e) => EditSelectedDriver();
            _deleteButton.Click += (s, e) => DeleteSelectedDriver();
            _detailsButton.Click += (s, e) => ViewDriverDetails();
            _searchButton.Click += (s, e) => SearchDrivers();

            // Grid event handlers - use SfDataGrid events
            _driverGrid.CellDoubleClick += (s, e) => EditSelectedDriver();
            _driverGrid.SelectionChanged += DriverGrid_SelectionChanged;

            // Search box enter key handler
            if (_searchBox is TextBox searchTextBox)
            {
                searchTextBox.KeyPress += (s, e) => {
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        SearchDrivers();
                        e.Handled = true;
                    }
                };
            }

            // Initial button states
            UpdateButtonStates();
        }        private void LoadDrivers()
        {
            try
            {
                _drivers = _driverRepository.GetAllDrivers();
                _driverGrid.DataSource = null;
                _driverGrid.DataSource = _drivers;

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("Driver added successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDrivers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding driver: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        private void EditSelectedDriver()
        {
            var selectedItem = GetSelectedDriver();
            if (selectedItem == null)
            {
                MessageBox.Show("Please select a driver to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var driverToEdit = _driverRepository.GetDriverById(selectedItem.DriverID);

                if (driverToEdit == null)
                {
                    MessageBox.Show("Could not find the selected driver.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new DriverEditFormSyncfusion(driverToEdit);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Driver != null)
                {
                    _driverRepository.UpdateDriver(editForm.Driver);
                    MessageBox.Show("Driver updated successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDrivers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing driver: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Driver? GetSelectedDriver()
        {
            if (_driverGrid.SelectedItems.Count == 0)
                return null;

            return _driverGrid.SelectedItems[0] as Driver;
        }

        private void DeleteSelectedDriver()
        {
            var selectedDriver = GetSelectedDriver();
            if (selectedDriver == null)
                return;

            var result = MessageBox.Show(
                "Are you sure you want to delete this driver?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                _driverRepository.DeleteDriver(selectedDriver.DriverID);
                LoadDrivers();
                MessageBox.Show("Driver deleted successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting driver: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewDriverDetails()
        {
            var selectedDriver = GetSelectedDriver();
            if (selectedDriver == null)
                return;

            var driver = _driverRepository.GetDriverById(selectedDriver.DriverID);

            if (driver != null)
            {
                var details = $"Driver Details:\n\n" +
                             $"👨‍✈️ Name: {driver.DriverName}\n" +
                             $"📞 Phone: {driver.DriverPhone}\n" +
                             $"✉️ Email: {driver.DriverEmail}\n" +
                             $"🏠 Address: {driver.Address}, {driver.City}, {driver.State} {driver.Zip}\n" +
                             $"📋 License: {driver.DriversLicenseType}\n" +
                             $"🎓 Training Complete: {(driver.IsTrainingComplete ? "Yes" : "No")}";

                MessageBox.Show(details, "Driver Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not load driver details.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DriverGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _driverGrid.SelectedItems.Count > 0;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void SearchDrivers()
        {
            if (_searchBox is not TextBox searchTextBox)
                return;

            string searchTerm = searchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadDrivers();
                return;
            }

            var filtered = _drivers.Where(d =>
                (d.DriverName?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverPhone?.ToLower().Contains(searchTerm) == true) ||
                (d.DriverEmail?.ToLower().Contains(searchTerm) == true) ||
                (d.DriversLicenseType?.ToLower().Contains(searchTerm) == true) ||
                (d.Address?.ToLower().Contains(searchTerm) == true) ||
                (d.City?.ToLower().Contains(searchTerm) == true) ||
                (d.State?.ToLower().Contains(searchTerm) == true)
            ).ToList();

            _driverGrid.DataSource = null;
            _driverGrid.DataSource = filtered;

            UpdateButtonStates();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources if needed
            base.OnFormClosing(e);
        }
    }
}
