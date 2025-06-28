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
    /// Route Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for creating and editing route information
    /// </summary>
    public partial class RouteEditFormSyncfusion : SyncfusionBaseForm
    {
        // Repositories for data access
        private readonly IRouteRepository _routeRepository;
        private readonly IBusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;

        // Controls matching Route model properties
        private DateTimePicker? _datePicker;
        private Control? _routeNameTextBox;
        private ComboBox? _routeTypeComboBox; // Task 6.5: Added for driver pay calculations
        private ComboBox? _amVehicleComboBox;
        private ComboBox? _amDriverComboBox;
        private Control? _amBeginMilesTextBox;
        private Control? _amEndMilesTextBox;
        private Control? _amRidersTextBox;
        private ComboBox? _pmVehicleComboBox;
        private ComboBox? _pmDriverComboBox;
        private Control? _pmBeginMilesTextBox;
        private Control? _pmEndMilesTextBox;
        private Control? _pmRidersTextBox;
        private Control? _notesTextBox;
        private Control? _saveButton;
        private Control? _cancelButton;        // Labels
        private Label? _dateLabel;
        private Label? _routeNameLabel;
        private Label? _routeTypeLabel; // Task 6.5: Added for RouteType dropdown
        private Label? _amSectionLabel;
        private Label? _amVehicleLabel;
        private Label? _amDriverLabel;
        private Label? _amBeginMilesLabel;
        private Label? _amEndMilesLabel;
        private Label? _amRidersLabel;
        private Label? _pmSectionLabel;
        private Label? _pmVehicleLabel;
        private Label? _pmDriverLabel;
        private Label? _pmBeginMilesLabel;
        private Label? _pmEndMilesLabel;
        private Label? _pmRidersLabel;
        private Label? _notesLabel;

        // Layout panels
        private new Panel _mainPanel;
        private TableLayoutPanel _formLayout;
        private new Panel _buttonPanel;

        public Route? Route { get; private set; }

        public RouteEditFormSyncfusion() : this(null)
        {
        }

        public RouteEditFormSyncfusion(Route? route)
        {
            _routeRepository = new RouteRepository();
            _busRepository = new BusRepository();
            _driverRepository = new DriverRepository();

            Route = route;
            InitializeComponent();
            SetupForm();
            PopulateFields();
        }

        private void InitializeComponent()
        {
            this.Text = Route == null ? "Add Route" : "Edit Route";
            this.Size = new Size(500, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // Main panel
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                Padding = new Padding(20)
            };

            // Form layout
            _formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 10,
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme)
            };

            // Configure form layout - 4 columns for AM/PM sections
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // Labels
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); // AM controls
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); // PM Labels
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); // PM controls

            for (int i = 0; i < 10; i++)
            {
                _formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            }            // Labels
            _dateLabel = BusBuddyThemeManager.CreateStyledLabel("Date:");
            _routeNameLabel = BusBuddyThemeManager.CreateStyledLabel("Route Name:");
            _routeTypeLabel = BusBuddyThemeManager.CreateStyledLabel("Route Type:"); // Task 6.5: Added RouteType label
            _amSectionLabel = BusBuddyThemeManager.CreateStyledLabel("AM SECTION");
            _amVehicleLabel = BusBuddyThemeManager.CreateStyledLabel("Bus:");
            _amDriverLabel = BusBuddyThemeManager.CreateStyledLabel("Driver:");
            _amBeginMilesLabel = BusBuddyThemeManager.CreateStyledLabel("Begin Miles:");
            _amEndMilesLabel = BusBuddyThemeManager.CreateStyledLabel("End Miles:");
            _amRidersLabel = BusBuddyThemeManager.CreateStyledLabel("Riders:");
            _pmSectionLabel = BusBuddyThemeManager.CreateStyledLabel("PM SECTION");
            _pmVehicleLabel = BusBuddyThemeManager.CreateStyledLabel("Bus:");
            _pmDriverLabel = BusBuddyThemeManager.CreateStyledLabel("Driver:");
            _pmBeginMilesLabel = BusBuddyThemeManager.CreateStyledLabel("Begin Miles:");
            _pmEndMilesLabel = BusBuddyThemeManager.CreateStyledLabel("End Miles:");
            _pmRidersLabel = BusBuddyThemeManager.CreateStyledLabel("Riders:");
            _notesLabel = BusBuddyThemeManager.CreateStyledLabel("Notes:");

            // Style section labels
            _amSectionLabel.Font = new Font(_amSectionLabel.Font, FontStyle.Bold);
            _pmSectionLabel.Font = new Font(_pmSectionLabel.Font, FontStyle.Bold);

            // Controls
            _datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Fill
            };            _routeNameTextBox = BusBuddyThemeManager.CreateStyledTextBox("Enter route name");

            // Task 6.5: RouteType ComboBox for driver pay calculations
            _routeTypeComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _routeTypeComboBox.Items.AddRange(new[] { "CDL", "SmallBus", "SPED" });
            _routeTypeComboBox.SelectedIndex = 0; // Default to CDL

            // ComboBoxes for vehicles and drivers
            _amVehicleComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _amDriverComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _pmVehicleComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _pmDriverComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Text boxes for numeric values
            _amBeginMilesTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");
            _amEndMilesTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");
            _amRidersTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");
            _pmBeginMilesTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");
            _pmEndMilesTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");
            _pmRidersTextBox = BusBuddyThemeManager.CreateStyledTextBox("0");

            // Notes text box (multiline)
            _notesTextBox = BusBuddyThemeManager.CreateStyledTextBox("Enter any notes about this route");
            if (_notesTextBox is TextBox notesTextBox)
            {
                notesTextBox.Multiline = true;
                notesTextBox.Height = 80;
                notesTextBox.ScrollBars = ScrollBars.Vertical;
            }

            // Button panel
            _buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                Padding = new Padding(20, 10, 20, 10)
            };

            // Buttons
            _saveButton = new Button { Text = "Save" } /* CreateStyledButton method removed */;
            _cancelButton = new Button { Text = "Cancel" } /* CreateStyledButton method removed */;

            // Configure buttons
            _saveButton.Size = new Size(100, 35);
            _saveButton.Anchor = AnchorStyles.Right;
            _saveButton.Location = new Point(_buttonPanel.Width - 220, 12);
            _saveButton.Click += SaveButton_Click;

            _cancelButton.Size = new Size(100, 35);
            _cancelButton.Anchor = AnchorStyles.Right;
            _cancelButton.Location = new Point(_buttonPanel.Width - 110, 12);
            _cancelButton.Click += CancelButton_Click;

            // Style cancel button differently
            if (_cancelButton is Button cancelBtn)
            {
                cancelBtn.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
                cancelBtn.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            }
        }

        private void LayoutControls()
        {
            // Add controls to form layout - Row by row
            int row = 0;

            // Date row
            _formLayout.Controls.Add(_dateLabel, 0, row);
            _formLayout.Controls.Add(_datePicker, 1, row);
            row++;            // Route name row
            _formLayout.Controls.Add(_routeNameLabel, 0, row);
            _formLayout.Controls.Add(_routeNameTextBox, 1, row);
            row++;

            // Task 6.5: Route type row for driver pay calculations
            _formLayout.Controls.Add(_routeTypeLabel, 0, row);
            _formLayout.Controls.Add(_routeTypeComboBox, 1, row);
            row++;

            // AM Section header
            _formLayout.Controls.Add(_amSectionLabel, 0, row);
            _formLayout.Controls.Add(_pmSectionLabel, 2, row);
            row++;

            // Vehicle row
            _formLayout.Controls.Add(_amVehicleLabel, 0, row);
            _formLayout.Controls.Add(_amVehicleComboBox, 1, row);
            _formLayout.Controls.Add(_pmVehicleLabel, 2, row);
            _formLayout.Controls.Add(_pmVehicleComboBox, 3, row);
            row++;

            // Driver row
            _formLayout.Controls.Add(_amDriverLabel, 0, row);
            _formLayout.Controls.Add(_amDriverComboBox, 1, row);
            _formLayout.Controls.Add(_pmDriverLabel, 2, row);
            _formLayout.Controls.Add(_pmDriverComboBox, 3, row);
            row++;

            // Begin miles row
            _formLayout.Controls.Add(_amBeginMilesLabel, 0, row);
            _formLayout.Controls.Add(_amBeginMilesTextBox, 1, row);
            _formLayout.Controls.Add(_pmBeginMilesLabel, 2, row);
            _formLayout.Controls.Add(_pmBeginMilesTextBox, 3, row);
            row++;

            // End miles row
            _formLayout.Controls.Add(_amEndMilesLabel, 0, row);
            _formLayout.Controls.Add(_amEndMilesTextBox, 1, row);
            _formLayout.Controls.Add(_pmEndMilesLabel, 2, row);
            _formLayout.Controls.Add(_pmEndMilesTextBox, 3, row);
            row++;

            // Riders row
            _formLayout.Controls.Add(_amRidersLabel, 0, row);
            _formLayout.Controls.Add(_amRidersTextBox, 1, row);
            _formLayout.Controls.Add(_pmRidersLabel, 2, row);
            _formLayout.Controls.Add(_pmRidersTextBox, 3, row);
            row++;

            // Notes row (spanning multiple columns)
            _formLayout.Controls.Add(_notesLabel, 0, row);
            _formLayout.SetColumnSpan(_notesTextBox, 3);
            _formLayout.Controls.Add(_notesTextBox, 1, row);

            // Add buttons to button panel
            _buttonPanel.Controls.Add(_saveButton);
            _buttonPanel.Controls.Add(_cancelButton);

            // Add panels to main panel
            _mainPanel.Controls.Add(_formLayout);
            _mainPanel.Controls.Add(_buttonPanel);

            // Add main panel to form
            this.Controls.Add(_mainPanel);

            // Apply theming
            BusBuddyThemeManager.ApplyThemeToControl(_mainPanel, BusBuddyThemeManager.CurrentTheme);
        }

        private void SetupForm()
        {
            // Apply Syncfusion theming
            BusBuddyThemeManager.ApplyThemeToControl(this, BusBuddyThemeManager.CurrentTheme);

            // Populate ComboBoxes with data
            PopulateComboBoxes();
        }        private void PopulateComboBoxes()
        {
            try
            {
                // Populate vehicle ComboBoxes
                var buses = _busRepository.GetAllBuses().ToList();

                if (_amVehicleComboBox != null)
                {
                    _amVehicleComboBox.Items.Clear();
                    _amVehicleComboBox.Items.Add(""); // Empty option
                    foreach (var bus in buses)
                    {
                        _amVehicleComboBox.Items.Add($"{bus.BusNumber} - {bus.Make} {bus.Model}");
                    }
                }

                if (_pmVehicleComboBox != null)
                {
                    _pmVehicleComboBox.Items.Clear();
                    _pmVehicleComboBox.Items.Add(""); // Empty option
                    foreach (var bus in buses)
                    {
                        _pmVehicleComboBox.Items.Add($"{bus.BusNumber} - {bus.Make} {bus.Model}");
                    }
                }

                // Populate driver ComboBoxes
                var drivers = _driverRepository.GetAllDrivers().ToList();

                if (_amDriverComboBox != null)
                {
                    _amDriverComboBox.Items.Clear();
                    _amDriverComboBox.Items.Add(""); // Empty option
                    foreach (var driver in drivers)
                    {
                        _amDriverComboBox.Items.Add(driver.Name);
                    }
                }

                if (_pmDriverComboBox != null)
                {
                    _pmDriverComboBox.Items.Clear();
                    _pmDriverComboBox.Items.Add(""); // Empty option
                    foreach (var driver in drivers)
                    {
                        _pmDriverComboBox.Items.Add(driver.Name);
                    }
                }

                // Populate RouteType ComboBox
                if (_routeTypeComboBox != null)
                {
                    _routeTypeComboBox.Items.Clear();
                    _routeTypeComboBox.Items.Add("CDL");
                    _routeTypeComboBox.Items.Add("SmallBus");
                    _routeTypeComboBox.Items.Add("SPED");
                    _routeTypeComboBox.SelectedIndex = 0; // Default to CDL
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PopulateFields()
        {
            if (Route == null) return;

            try
            {
                // Set date
                _datePicker.Value = Route.DateAsDateTime;                // Set route name
                if (_routeNameTextBox is TextBox routeNameTb)
                    routeNameTb.Text = Route.RouteName ?? string.Empty;

                // Task 6.5: Set route type for driver pay calculations
                if (_routeTypeComboBox != null)
                {
                    var routeType = Route.RouteType ?? "CDL";
                    var index = _routeTypeComboBox.Items.IndexOf(routeType);
                    _routeTypeComboBox.SelectedIndex = index >= 0 ? index : 0;
                }

                // AM fields
                if (_amBeginMilesTextBox is TextBox amBeginMilesTb)
                    amBeginMilesTb.Text = Route.AMBeginMiles?.ToString() ?? string.Empty;

                if (_amEndMilesTextBox is TextBox amEndMilesTb)
                    amEndMilesTb.Text = Route.AMEndMiles?.ToString() ?? string.Empty;

                if (_amRidersTextBox is TextBox amRidersTb)
                    amRidersTb.Text = Route.AMRiders?.ToString() ?? string.Empty;

                // PM fields
                if (_pmBeginMilesTextBox is TextBox pmBeginMilesTb)
                    pmBeginMilesTb.Text = Route.PMBeginMiles?.ToString() ?? string.Empty;

                if (_pmEndMilesTextBox is TextBox pmEndMilesTb)
                    pmEndMilesTb.Text = Route.PMEndMiles?.ToString() ?? string.Empty;

                if (_pmRidersTextBox is TextBox pmRidersTb)
                    pmRidersTb.Text = Route.PMRiders?.ToString() ?? string.Empty;

                // Notes
                if (_notesTextBox is TextBox notesTb)
                    notesTb.Text = Route.Notes ?? string.Empty;

                // Set selected vehicles and drivers
                SetSelectedVehicleAndDriver();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating route data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetSelectedVehicleAndDriver()
        {
            try
            {
                var buses = _busRepository.GetAllBuses().ToList();
                var drivers = _driverRepository.GetAllDrivers().ToList();

                // Set AM Vehicle
                if (Route?.AMBusId.HasValue == true)
                {
                    var amBus = buses.FirstOrDefault(b => b.BusId == Route.AMBusId.Value);
                    if (amBus != null && _amVehicleComboBox != null)
                    {
                        var busText = $"{amBus.BusNumber} - {amBus.Make} {amBus.Model}";
                        var index = _amVehicleComboBox.Items.IndexOf(busText);
                        if (index >= 0) _amVehicleComboBox.SelectedIndex = index;
                    }
                }

                // Set PM Vehicle
                if (Route?.PMBusId.HasValue == true)
                {
                    var pmBus = buses.FirstOrDefault(b => b.BusId == Route.PMBusId.Value);
                    if (pmBus != null && _pmVehicleComboBox != null)
                    {
                        var busText = $"{pmBus.BusNumber} - {pmBus.Make} {pmBus.Model}";
                        var index = _pmVehicleComboBox.Items.IndexOf(busText);
                        if (index >= 0) _pmVehicleComboBox.SelectedIndex = index;
                    }
                }

                // Set AM Driver
                if (Route?.AMDriverId.HasValue == true)
                {
                    var amDriver = drivers.FirstOrDefault(d => d.DriverId == Route.AMDriverId.Value);
                    if (amDriver != null && _amDriverComboBox != null)
                    {
                        var index = _amDriverComboBox.Items.IndexOf(amDriver.Name);
                        if (index >= 0) _amDriverComboBox.SelectedIndex = index;
                    }
                }

                // Set PM Driver
                if (Route?.PMDriverId.HasValue == true)
                {
                    var pmDriver = drivers.FirstOrDefault(d => d.DriverId == Route.PMDriverId.Value);
                    if (pmDriver != null && _pmDriverComboBox != null)
                    {
                        var index = _pmDriverComboBox.Items.IndexOf(pmDriver.Name);
                        if (index >= 0) _pmDriverComboBox.SelectedIndex = index;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't show error for selection issues
                Console.WriteLine($"Error setting vehicle/driver selections: {ex.Message}");
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var route = GetRouteFromForm();

                // Save route using repository
                int RouteId;
                if (Route?.RouteId > 0)
                {
                    // Update existing route
                    route.RouteId = Route.RouteId;
                    _routeRepository.UpdateRoute(route);
                    RouteId = route.RouteId;
                }
                else
                {
                    // Add new route
                    RouteId = _routeRepository.AddRoute(route);
                }

                MessageBox.Show($"Route {route.RouteName} saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving route: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override bool ValidateForm()
        {
            var errors = new List<string>();

            var routeName = (_routeNameTextBox as TextBox)?.Text?.Trim();

            if (string.IsNullOrEmpty(routeName))
                errors.Add("Route name is required");

            // Validate numeric fields
            var amBeginMilesText = (_amBeginMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amBeginMilesText))
            {
                if (!decimal.TryParse(amBeginMilesText, out decimal amBeginMiles) || amBeginMiles < 0)
                {
                    errors.Add("AM begin miles must be a non-negative number");
                }
            }

            var amEndMilesText = (_amEndMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amEndMilesText))
            {
                if (!decimal.TryParse(amEndMilesText, out decimal amEndMiles) || amEndMiles < 0)
                {
                    errors.Add("AM end miles must be a non-negative number");
                }
            }

            var amRidersText = (_amRidersTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amRidersText))
            {
                if (!int.TryParse(amRidersText, out int amRiders) || amRiders < 0)
                {
                    errors.Add("AM riders must be a non-negative number");
                }
            }

            var pmBeginMilesText = (_pmBeginMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmBeginMilesText))
            {
                if (!decimal.TryParse(pmBeginMilesText, out decimal pmBeginMiles) || pmBeginMiles < 0)
                {
                    errors.Add("PM begin miles must be a non-negative number");
                }
            }

            var pmEndMilesText = (_pmEndMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmEndMilesText))
            {
                if (!decimal.TryParse(pmEndMilesText, out decimal pmEndMiles) || pmEndMiles < 0)
                {
                    errors.Add("PM end miles must be a non-negative number");
                }
            }

            var pmRidersText = (_pmRidersTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmRidersText))
            {
                if (!int.TryParse(pmRidersText, out int pmRiders) || pmRiders < 0)
                {
                    errors.Add("PM riders must be a non-negative number");
                }
            }

            if (errors.Count > 0)
            {
                MessageBox.Show($"Please correct the following errors:\n\n{string.Join("\n", errors)}",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private Route GetRouteFromForm()
        {            var route = new Route
            {
                RouteId = Route?.RouteId ?? 0,
                DateAsDateTime = _datePicker.Value,
                RouteName = (_routeNameTextBox as TextBox)?.Text?.Trim(),
                Notes = (_notesTextBox as TextBox)?.Text?.Trim(),
                RouteType = _routeTypeComboBox?.SelectedItem?.ToString() ?? "CDL" // Task 6.5: Added RouteType from dropdown
            };

            // Parse AM values
            var amBeginMilesText = (_amBeginMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amBeginMilesText) && decimal.TryParse(amBeginMilesText, out decimal amBeginMiles))
            {
                route.AMBeginMiles = (int?)Math.Round(amBeginMiles);
            }

            var amEndMilesText = (_amEndMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amEndMilesText) && decimal.TryParse(amEndMilesText, out decimal amEndMiles))
            {
                route.AMEndMiles = (int?)Math.Round(amEndMiles);
            }

            var amRidersText = (_amRidersTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(amRidersText) && int.TryParse(amRidersText, out int amRiders))
            {
                route.AMRiders = amRiders;
            }

            // Parse PM values
            var pmBeginMilesText = (_pmBeginMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmBeginMilesText) && decimal.TryParse(pmBeginMilesText, out decimal pmBeginMiles))
            {
                route.PMBeginMiles = (int?)Math.Round(pmBeginMiles);
            }

            var pmEndMilesText = (_pmEndMilesTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmEndMilesText) && decimal.TryParse(pmEndMilesText, out decimal pmEndMiles))
            {
                route.PMEndMiles = (int?)Math.Round(pmEndMiles);
            }

            var pmRidersText = (_pmRidersTextBox as TextBox)?.Text?.Trim();
            if (!string.IsNullOrEmpty(pmRidersText) && int.TryParse(pmRidersText, out int pmRiders))
            {
                route.PMRiders = pmRiders;
            }

            // Get selected vehicle/driver IDs from combo boxes
            route.AMBusId = GetSelectedVehicleId(_amVehicleComboBox);
            route.AMDriverId = GetSelectedDriverId(_amDriverComboBox);
            route.PMBusId = GetSelectedVehicleId(_pmVehicleComboBox);
            route.PMDriverId = GetSelectedDriverId(_pmDriverComboBox);

            return route;
        }

        private int? GetSelectedVehicleId(ComboBox? comboBox)
        {
            if (comboBox?.SelectedIndex > 0) // Skip index 0 which is empty
            {
                try
                {
                    var buses = _busRepository.GetAllBuses().ToList();
                    if (comboBox.SelectedIndex <= buses.Count)
                    {
                        return buses[comboBox.SelectedIndex - 1].BusId; // Using BusId from Bus model
                    }
                }
                catch { }
            }
            return null;
        }

        private int? GetSelectedDriverId(ComboBox? comboBox)
        {
            if (comboBox?.SelectedIndex > 0) // Skip index 0 which is empty
            {
                try
                {
                    var drivers = _driverRepository.GetAllDrivers().ToList();
                    if (comboBox.SelectedIndex <= drivers.Count)
                    {
                        return drivers[comboBox.SelectedIndex - 1].DriverId; // -1 because of empty option at index 0
                    }
                }
                catch { }
            }
            return null;
        }
    }
}

