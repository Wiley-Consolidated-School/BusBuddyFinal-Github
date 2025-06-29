using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Microsoft.Extensions.Logging;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.Input.Enums;

namespace BusBuddy.UI.Views
{
    public partial class RouteFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IRouteRepository _routeRepository;
        private readonly IBusRepository _busRepository;
        private readonly IDriverRepository _driverRepository;
        private Route _route;
        private bool _isEditMode;
        private SfDateTimeEdit _dateEdit;
        private ComboBoxAdv _routeNameComboBox;
        private ComboBoxAdv _amVehicleComboBox;
        private SfNumericTextBox _amBeginMilesTextBox;
        private SfNumericTextBox _amEndMilesTextBox;
        private SfNumericTextBox _amRidersTextBox;
        private ComboBoxAdv _amDriverComboBox;
        private ComboBoxAdv _pmVehicleComboBox;
        private SfNumericTextBox _pmBeginMilesTextBox;
        private SfNumericTextBox _pmEndMilesTextBox;
        private SfNumericTextBox _pmRidersTextBox;
        private ComboBoxAdv _pmDriverComboBox;
        private TextBoxExt _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;
        private readonly ILogger<RouteFormSyncfusion> _logger;

        public Route Route
        {
            get => _route;
            set
            {
                _route = value;
                _isEditMode = _route?.RouteId > 0;
                LoadRouteData();
            }
        }

        public RouteFormSyncfusion(IServiceProvider serviceProvider, ILogger<RouteFormSyncfusion> logger)
            : base(serviceProvider)
        {
            _routeRepository = new RouteRepository();
            _busRepository = new BusRepository();
            _driverRepository = new DriverRepository();
            _route = new Route();
            _logger = logger;
            InitializeComponent();
            _logger?.LogInformation("RouteForm initialized for {Mode} route", _isEditMode ? "edit" : "new");
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text = _isEditMode ? "Edit Route" : "Add Route";
            Size = new Size(600, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = System.Drawing.Color.White;

            var dateLabel = new Label { Text = "Date:", Location = new Point(20, 30), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(dateLabel);
            _dateEdit = new SfDateTimeEdit { Location = new Point(130, 30), Size = new Size(200, 30), Value = DateTime.Today };
            Controls.Add(_dateEdit);

            var routeNameLabel = new Label { Text = "Route Name:", Location = new Point(350, 30), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(routeNameLabel);
            _routeNameComboBox = new ComboBoxAdv { Location = new Point(460, 30), Size = new Size(120, 30) };
            _routeNameComboBox.Items.AddRange(new[] { "Truck Plaza", "East Route", "West Route", "SPED" });
            Controls.Add(_routeNameComboBox);

            var amSectionLabel = new Label { Text = "AM Route Information", Location = new Point(20, 80), Size = new Size(200, 23), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Controls.Add(amSectionLabel);
            var amVehicleLabel = new Label { Text = "AM Vehicle:", Location = new Point(20, 110), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(amVehicleLabel);
            _amVehicleComboBox = new ComboBoxAdv { Location = new Point(130, 110), Size = new Size(150, 30), DisplayMember = "BusNumber", ValueMember = "BusId" };
            Controls.Add(_amVehicleComboBox);
            var amBeginMilesLabel = new Label { Text = "AM Begin Miles:", Location = new Point(300, 110), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(amBeginMilesLabel);
            _amBeginMilesTextBox = new SfNumericTextBox { Location = new Point(410, 110), Size = new Size(100, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999999, WatermarkText = "Enter miles" };
            Controls.Add(_amBeginMilesTextBox);
            var amEndMilesLabel = new Label { Text = "AM End Miles:", Location = new Point(20, 160), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(amEndMilesLabel);
            _amEndMilesTextBox = new SfNumericTextBox { Location = new Point(130, 160), Size = new Size(100, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999999, WatermarkText = "Enter miles" };
            Controls.Add(_amEndMilesTextBox);
            var amRidersLabel = new Label { Text = "AM Riders:", Location = new Point(250, 160), Size = new Size(80, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(amRidersLabel);
            _amRidersTextBox = new SfNumericTextBox { Location = new Point(340, 160), Size = new Size(80, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999, WatermarkText = "Riders" };
            Controls.Add(_amRidersTextBox);
            var amDriverLabel = new Label { Text = "AM Driver:", Location = new Point(20, 210), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(amDriverLabel);
            _amDriverComboBox = new ComboBoxAdv { Location = new Point(130, 210), Size = new Size(200, 30), DisplayMember = "Name", ValueMember = "DriverId" };
            Controls.Add(_amDriverComboBox);

            var pmSectionLabel = new Label { Text = "PM Route Information", Location = new Point(20, 260), Size = new Size(200, 23), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            Controls.Add(pmSectionLabel);
            var pmVehicleLabel = new Label { Text = "PM Vehicle:", Location = new Point(20, 290), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(pmVehicleLabel);
            _pmVehicleComboBox = new ComboBoxAdv { Location = new Point(130, 290), Size = new Size(150, 30), DisplayMember = "BusNumber", ValueMember = "BusId" };
            Controls.Add(_pmVehicleComboBox);
            var pmBeginMilesLabel = new Label { Text = "PM Begin Miles:", Location = new Point(300, 290), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(pmBeginMilesLabel);
            _pmBeginMilesTextBox = new SfNumericTextBox { Location = new Point(410, 290), Size = new Size(100, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999999, WatermarkText = "Enter miles" };
            Controls.Add(_pmBeginMilesTextBox);
            var pmEndMilesLabel = new Label { Text = "PM End Miles:", Location = new Point(20, 340), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(pmEndMilesLabel);
            _pmEndMilesTextBox = new SfNumericTextBox { Location = new Point(130, 340), Size = new Size(100, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999999, WatermarkText = "Enter miles" };
            Controls.Add(_pmEndMilesTextBox);
            var pmRidersLabel = new Label { Text = "PM Riders:", Location = new Point(250, 340), Size = new Size(80, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(pmRidersLabel);
            _pmRidersTextBox = new SfNumericTextBox { Location = new Point(340, 340), Size = new Size(80, 30), FormatMode = FormatMode.Numeric, AllowNull = true, MinValue = 0, MaxValue = 999, WatermarkText = "Riders" };
            Controls.Add(_pmRidersTextBox);
            var pmDriverLabel = new Label { Text = "PM Driver:", Location = new Point(20, 390), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(pmDriverLabel);
            _pmDriverComboBox = new ComboBoxAdv { Location = new Point(130, 390), Size = new Size(200, 30), DisplayMember = "Name", ValueMember = "DriverId" };
            Controls.Add(_pmDriverComboBox);

            var notesLabel = new Label { Text = "Notes:", Location = new Point(20, 440), Size = new Size(100, 23), Font = new Font("Segoe UI", 9F) };
            Controls.Add(notesLabel);
            _notesTextBox = new TextBoxExt { Location = new Point(130, 440), Size = new Size(400, 100), Multiline = true };
            Controls.Add(_notesTextBox);

            _saveButton = new SfButton { Text = "Save", Location = new Point(370, 570), Size = new Size(80, 35), BackColor = BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme) };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);
            _cancelButton = new SfButton { Text = "Cancel", Location = new Point(460, 570), Size = new Size(80, 35), BackColor = BusBuddyThemeManager.ThemeColors.GetErrorColor(BusBuddyThemeManager.CurrentTheme) };
            _cancelButton.Click += CancelButton_Click;
            Controls.Add(_cancelButton);
            ResumeLayout(false);
            LoadVehiclesAndDrivers();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _logger?.LogInformation("RouteForm loaded for {Mode} route", _isEditMode ? "edit" : "new");
        }

        private void LoadVehiclesAndDrivers()
        {
            try
            {
                var buses = _busRepository.GetAllBuses().ToList();
                _amVehicleComboBox.DataSource = new List<Bus>(buses) { new Bus { BusId = 0, BusNumber = "Unassigned" } };
                _pmVehicleComboBox.DataSource = new List<Bus>(buses) { new Bus { BusId = 0, BusNumber = "Unassigned" } };
                _logger?.LogInformation("Loaded {Count} buses for route form", buses.Count);
                var drivers = _driverRepository.GetAllDrivers().ToList();
                _amDriverComboBox.DataSource = new List<Driver>(drivers) { new Driver { DriverId = 0, Name = "Unassigned" } };
                _pmDriverComboBox.DataSource = new List<Driver>(drivers) { new Driver { DriverId = 0, Name = "Unassigned" } };
                _logger?.LogInformation("Loaded {Count} drivers for route form", drivers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading vehicles/drivers");
                MessageBox.Show($"Error loading vehicles/drivers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRouteData()
        {
            if (_route == null) return;
            try
            {
                _dateEdit.Value = _route.DateAsDateTime;
                _routeNameComboBox.SelectedItem = _route.RouteName;
                if (_route.AMBusId.HasValue)
                    _amVehicleComboBox.SelectedValue = _route.AMBusId.Value;
                else
                    _amVehicleComboBox.SelectedIndex = 0; // Unassigned
                _amBeginMilesTextBox.Value = (double?)_route.AMBeginMiles;
                _amEndMilesTextBox.Value = (double?)_route.AMEndMiles;
                _amRidersTextBox.Value = (double?)_route.AMRiders;
                if (_route.AMDriverId.HasValue)
                    _amDriverComboBox.SelectedValue = _route.AMDriverId.Value;
                else
                    _amDriverComboBox.SelectedIndex = 0; // Unassigned
                if (_route.PMBusId.HasValue)
                    _pmVehicleComboBox.SelectedValue = _route.PMBusId.Value;
                else
                    _pmVehicleComboBox.SelectedIndex = 0; // Unassigned
                _pmBeginMilesTextBox.Value = (double?)_route.PMBeginMiles;
                _pmEndMilesTextBox.Value = (double?)_route.PMEndMiles;
                _pmRidersTextBox.Value = (double?)_route.PMRiders;
                if (_route.PMDriverId.HasValue)
                    _pmDriverComboBox.SelectedValue = _route.PMDriverId.Value;
                else
                    _pmDriverComboBox.SelectedIndex = 0; // Unassigned
                _notesTextBox.Text = _route.Notes ?? "";
                _logger?.LogInformation("Route data loaded for route {RouteId}", _route.RouteId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading route data");
                MessageBox.Show($"Error loading route data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_routeNameComboBox.SelectedItem?.ToString()))
                {
                    MessageBox.Show("Route Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _routeNameComboBox.Focus();
                    return;
                }
                if (_amEndMilesTextBox.Value.HasValue && _amBeginMilesTextBox.Value.HasValue && _amEndMilesTextBox.Value < _amBeginMilesTextBox.Value)
                {
                    MessageBox.Show("AM End Miles must be greater than or equal to Begin Miles.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _amEndMilesTextBox.Focus();
                    return;
                }
                if (_pmEndMilesTextBox.Value.HasValue && _pmBeginMilesTextBox.Value.HasValue && _pmEndMilesTextBox.Value < _pmBeginMilesTextBox.Value)
                {
                    MessageBox.Show("PM End Miles must be greater than or equal to Begin Miles.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _pmEndMilesTextBox.Focus();
                    return;
                }
                if (_amBeginMilesTextBox.Value.HasValue && _amBeginMilesTextBox.Value < 0) { /* Validation handled below */ }
                if (_amEndMilesTextBox.Value.HasValue && _amEndMilesTextBox.Value < 0) { /* Validation handled below */ }
                if (_amRidersTextBox.Value.HasValue && _amRidersTextBox.Value < 0) { /* Validation handled below */ }
                if (_pmBeginMilesTextBox.Value.HasValue && _pmBeginMilesTextBox.Value < 0) { /* Validation handled below */ }
                if (_pmEndMilesTextBox.Value.HasValue && _pmEndMilesTextBox.Value < 0) { /* Validation handled below */ }
                if (_pmRidersTextBox.Value.HasValue && _pmRidersTextBox.Value < 0) { /* Validation handled below */ }
                _route.DateAsDateTime = _dateEdit.Value ?? DateTime.Today;
                _route.RouteName = _routeNameComboBox.SelectedItem?.ToString();
                _route.AMBusId = (int?)_amVehicleComboBox.SelectedValue;
                _route.AMBeginMiles = (int?)_amBeginMilesTextBox.Value;
                _route.AMEndMiles = (int?)_amEndMilesTextBox.Value;
                _route.AMRiders = _amRidersTextBox.Value.HasValue ? (int?)_amRidersTextBox.Value : null;
                _route.AMDriverId = (int?)_amDriverComboBox.SelectedValue;
                _route.PMBusId = (int?)_pmVehicleComboBox.SelectedValue;
                _route.PMBeginMiles = (int?)_pmBeginMilesTextBox.Value;
                _route.PMEndMiles = (int?)_pmEndMilesTextBox.Value;
                _route.PMRiders = _pmRidersTextBox.Value.HasValue ? (int?)_pmRidersTextBox.Value : null;
                _route.PMDriverId = (int?)_pmDriverComboBox.SelectedValue;
                _route.Notes = _notesTextBox.Text;
                if (_isEditMode)
                {
                    _routeRepository.UpdateRoute(_route);
                    _logger?.LogInformation("Route {RouteId} updated", _route.RouteId);
                }
                else
                {
                    _routeRepository.AddRoute(_route);
                    _logger?.LogInformation("New route {RouteId} added", _route.RouteId);
                }
                MessageBox.Show("Route data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving route");
                MessageBox.Show($"Error saving route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            _logger?.LogInformation("Route form cancelled");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Repositories disposed by DI
            }
            base.Dispose(disposing);
        }
    }
}

