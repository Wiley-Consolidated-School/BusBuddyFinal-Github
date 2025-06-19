using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.Data;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Fuel Management Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for managing fuel records with grid view and CRUD operations
    /// </summary>
    public class FuelManagementFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IFuelRepository _fuelRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private DataGridView? _fuelGrid;
        private Control? _addButton;
        private Control? _editButton;
        private Control? _deleteButton;
        private Control? _detailsButton;
        private Control? _searchBox;
        private Control? _searchButton;
        private List<Fuel> _fuels = new List<Fuel>();
        private List<Vehicle> _vehicles = new List<Vehicle>();

        public FuelManagementFormSyncfusion() : this(new FuelRepository(), new VehicleRepository()) { }

        public FuelManagementFormSyncfusion(IFuelRepository fuelRepository, IVehicleRepository vehicleRepository)
        {
            _fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            InitializeComponent();
            LoadFuels();
        }

        private void InitializeComponent()
        {
            // Set form size to 1200x900, title to "Fuel Management"
            this.Text = "⛽ Fuel Management";
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
            _searchBox = SyncfusionThemeHelper.CreateStyledTextBox("Search fuel records...");

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
            _mainPanel.Controls.Add(_searchButton);
            _mainPanel.Controls.Add(searchLabel);

            // Create and configure the data grid
            SetupDataGrid();
        }

        private void SetupDataGrid()
        {
            // Create DataGridView with modern styling
            _fuelGrid = new DataGridView();
            _fuelGrid.Dock = DockStyle.None;
            _fuelGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(60));
            _fuelGrid.Size = GetDpiAwareSize(new Size(1150, 650));
            _fuelGrid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            // Grid styling
            _fuelGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _fuelGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _fuelGrid.ReadOnly = true;
            _fuelGrid.AllowUserToAddRows = false;
            _fuelGrid.AllowUserToDeleteRows = false;
            _fuelGrid.MultiSelect = false;
            _fuelGrid.AllowUserToResizeColumns = true;
            _fuelGrid.AllowUserToResizeRows = true;
            _fuelGrid.ScrollBars = ScrollBars.Both;

            // Apply Material theme to grid
            SyncfusionThemeHelper.ApplyMaterialTheme(_fuelGrid);

            _mainPanel.Controls.Add(_fuelGrid);
        }

        private void LayoutControls()
        {
            // Layout is handled in CreateControls method for better organization
        }

        private void SetupEventHandlers()
        {
            // Button click events
            _addButton.Click += (s, e) => AddNewFuel();
            _editButton.Click += (s, e) => EditSelectedFuel();
            _deleteButton.Click += (s, e) => DeleteSelectedFuel();
            _detailsButton.Click += (s, e) => ViewFuelDetails();
            _searchButton.Click += (s, e) => SearchFuels();

            // Grid event handlers
            _fuelGrid.CellDoubleClick += (s, e) => EditSelectedFuel();
            _fuelGrid.SelectionChanged += FuelGrid_SelectionChanged;
            _fuelGrid.DataBindingComplete += FuelGrid_DataBindingComplete;

            // Search box enter key handler
            if (_searchBox is TextBox searchTextBox)
            {
                searchTextBox.KeyPress += (s, e) => {
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        SearchFuels();
                        e.Handled = true;
                    }
                };
            }

            // Initial button states
            UpdateButtonStates();
        }

        private void FuelGrid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_fuelGrid.Columns.Contains("FuelID"))
                _fuelGrid.Columns["FuelID"].Visible = false;

            // Configure column headers and widths
            ConfigureGridColumns();
        }

        private void ConfigureGridColumns()
        {
            if (_fuelGrid.Columns.Count == 0) return;            var columnConfig = new Dictionary<string, (string Header, int Width)>
            {
                ["FuelID"] = ("ID", 60),
                ["FuelDate"] = ("Date", 100),
                ["FuelLocation"] = ("Location", 120),
                ["VehicleFueledID"] = ("Vehicle", 80),
                ["VehicleOdometerReading"] = ("Odometer", 100),
                ["FuelType"] = ("Type", 80),
                ["FuelAmount"] = ("Amount", 80),
                ["FuelCost"] = ("Cost", 80)
            };

            foreach (var config in columnConfig)
            {
                if (_fuelGrid.Columns.Contains(config.Key))
                {
                    var column = _fuelGrid.Columns[config.Key];
                    column.HeaderText = config.Value.Header;
                    column.Width = GetDpiAwareSize(new Size(config.Value.Width, 0)).Width;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                }
            }

            // Set remaining columns to fill
            if (_fuelGrid.Columns.Count > 0)
            {
                _fuelGrid.Columns[_fuelGrid.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void LoadVehiclesForDropdown()
        {
            try
            {
                _vehicles = _vehicleRepository.GetAllVehicles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _vehicles = new List<Vehicle>();
            }
        }

        private void LoadFuels()
        {
            LoadVehiclesForDropdown(); // Ensure vehicles are loaded for dropdown
            try
            {
                _fuels = _fuelRepository.GetAllFuelRecords();
                _fuelGrid.DataSource = null;
                _fuelGrid.DataSource = _fuels;

                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fuel records: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewFuel()
        {
            try
            {
                using var addForm = new FuelEditFormSyncfusion();
                addForm.StartPosition = FormStartPosition.CenterParent;

                if (addForm.ShowDialog(this) == DialogResult.OK && addForm.Fuel != null)
                {
                    var fuelId = _fuelRepository.AddFuelRecord(addForm.Fuel);
                    if (fuelId > 0)
                    {
                        LoadFuels();
                        MessageBox.Show("Fuel record added successfully!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to add fuel record. Please try again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding fuel record: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditSelectedFuel()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a fuel record to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
                var selectedFuel = _fuelRepository.GetFuelRecordById(selectedId);

                if (selectedFuel == null)
                {
                    MessageBox.Show("Could not find the selected fuel record.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var editForm = new FuelEditFormSyncfusion(selectedFuel);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog(this) == DialogResult.OK && editForm.Fuel != null)
                {
                    if (_fuelRepository.UpdateFuelRecord(editForm.Fuel))
                    {
                        LoadFuels();
                        MessageBox.Show("Fuel record updated successfully!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to update fuel record. Please try again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing fuel record: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedFuel()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;

            var result = MessageBox.Show(
                "Are you sure you want to delete this fuel record?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            try
            {
                _fuelRepository.DeleteFuelRecord(selectedId);
                LoadFuels();
                MessageBox.Show("Fuel record deleted successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting fuel record: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewFuelDetails()
        {
            if (_fuelGrid.SelectedRows.Count == 0)
                return;

            int selectedId = (int)_fuelGrid.SelectedRows[0].Cells["FuelID"].Value;
            var fuel = _fuelRepository.GetFuelRecordById(selectedId);

            if (fuel != null)
            {
                var vehicleNumber = GetVehicleNumber(fuel.VehicleFueledID ?? 0);                var details = $"Fuel Record Details:\n\n" +
                             $"📅 Date: {fuel.FuelDateAsDateTime:yyyy-MM-dd}\n" +
                             $"📍 Location: {fuel.FuelLocation}\n" +
                             $"🚐 Vehicle: {vehicleNumber}\n" +
                             $"🔢 Odometer: {fuel.VehicleOdometerReading:N0} miles\n" +
                             $"⛽ Type: {fuel.FuelType}\n" +
                             $"📊 Amount: {fuel.FuelAmount:N2} gallons\n" +
                             $"💰 Cost: ${fuel.FuelCost:N2}" +
                             (string.IsNullOrEmpty(fuel.Notes) ? "" : $"\n📝 Notes: {fuel.Notes}");

                MessageBox.Show(details, "Fuel Record Details",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Could not load fuel record details.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FuelGrid_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _fuelGrid.SelectedRows.Count > 0;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }

        private void SearchFuels()
        {
            if (_searchBox is not TextBox searchTextBox)
                return;

            string searchTerm = searchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadFuels();
                return;
            }            var filtered = _fuels.Where(f =>
                (f.FuelLocation?.ToLower().Contains(searchTerm) == true) ||
                (f.FuelType?.ToLower().Contains(searchTerm) == true) ||
                (GetVehicleNumber(f.VehicleFueledID ?? 0).ToLower().Contains(searchTerm)) ||
                (f.FuelDate?.Contains(searchTerm) == true)
            ).ToList();

            _fuelGrid.DataSource = null;
            _fuelGrid.DataSource = filtered;

            UpdateButtonStates();
        }

        private string GetVehicleNumber(int vehicleId)
        {
            var vehicle = _vehicles?.FirstOrDefault(v => v.Id == vehicleId);
            return vehicle?.VehicleNumber ?? vehicleId.ToString();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources if needed
            base.OnFormClosing(e);
        }
    }
}
