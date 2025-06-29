using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.Input.Enums;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    public partial class RouteFormSyncfusion : SyncfusionBaseForm
    {
        private RouteRepository _routeRepository;
        private BusRepository _busRepository;
        private DriverRepository _driverRepository;
        private Route _route;
        private bool _isEditMode;

        // Form controls
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

        public RouteFormSyncfusion(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();
            _routeRepository = new RouteRepository();
            _busRepository = new BusRepository();
            _driverRepository = new DriverRepository();
            _route = new Route();
            _isEditMode = false;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "Route Editor";
            Size = new Size(600, 700);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = System.Drawing.Color.White;

            // Date
            var dateLabel = new Label
            {
                Text = "Date:",
                Location = new Point(20, 30),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(dateLabel);

            _dateEdit = new SfDateTimeEdit
            {
                Location = new Point(130, 30),
                Size = new Size(200, 30),
                Value = DateTime.Today
            };
            Controls.Add(_dateEdit);

            // Route Name
            var routeNameLabel = new Label
            {
                Text = "Route Name:",
                Location = new Point(350, 30),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(routeNameLabel);

            _routeNameComboBox = new ComboBoxAdv
            {
                Location = new Point(460, 30),
                Size = new Size(120, 30)
            };
            _routeNameComboBox.Items.Add("Truck Plaza");
            _routeNameComboBox.Items.Add("East Route");
            _routeNameComboBox.Items.Add("West Route");
            _routeNameComboBox.Items.Add("SPED");
            Controls.Add(_routeNameComboBox);

            // AM Section
            var amSectionLabel = new Label
            {
                Text = "AM Route Information",
                Location = new Point(20, 80),
                Size = new Size(200, 23),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            Controls.Add(amSectionLabel);

            // AM Vehicle
            var amVehicleLabel = new Label
            {
                Text = "AM Vehicle:",
                Location = new Point(20, 110),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(amVehicleLabel);

            _amVehicleComboBox = new ComboBoxAdv
            {
                Location = new Point(130, 110),
                Size = new Size(150, 30),
                DisplayMember = "BusNumber",
                ValueMember = "BusId"
            };
            Controls.Add(_amVehicleComboBox);

            // AM Begin Miles
            var amBeginMilesLabel = new Label
            {
                Text = "AM Begin Miles:",
                Location = new Point(300, 110),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(amBeginMilesLabel);

            _amBeginMilesTextBox = new SfNumericTextBox
            {
                Location = new Point(410, 110),
                Size = new Size(100, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999999,
                WatermarkText = "Enter miles"
            };
            Controls.Add(_amBeginMilesTextBox);

            // AM End Miles
            var amEndMilesLabel = new Label
            {
                Text = "AM End Miles:",
                Location = new Point(20, 160),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(amEndMilesLabel);

            _amEndMilesTextBox = new SfNumericTextBox
            {
                Location = new Point(130, 160),
                Size = new Size(100, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999999,
                WatermarkText = "Enter miles"
            };
            Controls.Add(_amEndMilesTextBox);

            // AM Riders
            var amRidersLabel = new Label
            {
                Text = "AM Riders:",
                Location = new Point(250, 160),
                Size = new Size(80, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(amRidersLabel);

            _amRidersTextBox = new SfNumericTextBox
            {
                Location = new Point(340, 160),
                Size = new Size(80, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999,
                WatermarkText = "Riders"
            };
            Controls.Add(_amRidersTextBox);

            // AM Driver
            var amDriverLabel = new Label
            {
                Text = "AM Driver:",
                Location = new Point(20, 210),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(amDriverLabel);

            _amDriverComboBox = new ComboBoxAdv
            {
                Location = new Point(130, 210),
                Size = new Size(200, 30),
                DisplayMember = "Name",
                ValueMember = "DriverId"
            };
            Controls.Add(_amDriverComboBox);

            // PM Section
            var pmSectionLabel = new Label
            {
                Text = "PM Route Information",
                Location = new Point(20, 260),
                Size = new Size(200, 23),
                Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold)
            };
            Controls.Add(pmSectionLabel);

            // PM Vehicle
            var pmVehicleLabel = new Label
            {
                Text = "PM Vehicle:",
                Location = new Point(20, 290),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(pmVehicleLabel);

            _pmVehicleComboBox = new ComboBoxAdv
            {
                Location = new Point(130, 290),
                Size = new Size(150, 30),
                DisplayMember = "BusNumber",
                ValueMember = "BusId"
            };
            Controls.Add(_pmVehicleComboBox);

            // PM Begin Miles
            var pmBeginMilesLabel = new Label
            {
                Text = "PM Begin Miles:",
                Location = new Point(300, 290),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(pmBeginMilesLabel);

            _pmBeginMilesTextBox = new SfNumericTextBox
            {
                Location = new Point(410, 290),
                Size = new Size(100, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999999,
                WatermarkText = "Enter miles"
            };
            Controls.Add(_pmBeginMilesTextBox);

            // PM End Miles
            var pmEndMilesLabel = new Label
            {
                Text = "PM End Miles:",
                Location = new Point(20, 340),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(pmEndMilesLabel);

            _pmEndMilesTextBox = new SfNumericTextBox
            {
                Location = new Point(130, 340),
                Size = new Size(100, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999999,
                WatermarkText = "Enter miles"
            };
            Controls.Add(_pmEndMilesTextBox);

            // PM Riders
            var pmRidersLabel = new Label
            {
                Text = "PM Riders:",
                Location = new Point(250, 340),
                Size = new Size(80, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(pmRidersLabel);

            _pmRidersTextBox = new SfNumericTextBox
            {
                Location = new Point(340, 340),
                Size = new Size(80, 30),
                FormatMode = FormatMode.Numeric,
                AllowNull = true,
                MinValue = 0,
                MaxValue = 999,
                WatermarkText = "Riders"
            };
            Controls.Add(_pmRidersTextBox);

            // PM Driver
            var pmDriverLabel = new Label
            {
                Text = "PM Driver:",
                Location = new Point(20, 390),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(pmDriverLabel);

            _pmDriverComboBox = new ComboBoxAdv
            {
                Location = new Point(130, 390),
                Size = new Size(200, 30),
                DisplayMember = "Name",
                ValueMember = "DriverId"
            };
            Controls.Add(_pmDriverComboBox);

            // Notes
            var notesLabel = new Label
            {
                Text = "Notes:",
                Location = new Point(20, 440),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(notesLabel);

            _notesTextBox = new TextBoxExt
            {
                Location = new Point(130, 440),
                Size = new Size(400, 100),
                Multiline = true
            };
            Controls.Add(_notesTextBox);

            // Buttons
            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(370, 570),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme)
            };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);

            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(460, 570),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetErrorColor(BusBuddyThemeManager.CurrentTheme)
            };
            _cancelButton.Click += CancelButton_Click;
            Controls.Add(_cancelButton);

            ResumeLayout(false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadVehicles();
            LoadDrivers();
            LoadRouteData();
        }

        private void LoadVehicles()
        {
            try
            {
                var buses = _busRepository.GetAllBuses().ToList();
                _amVehicleComboBox.DataSource = buses.ToList();
                _pmVehicleComboBox.DataSource = buses.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDrivers()
        {
            try
            {
                var drivers = _driverRepository.GetAllDrivers().ToList();
                _amDriverComboBox.DataSource = drivers.ToList();
                _pmDriverComboBox.DataSource = drivers.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRouteData()
        {
            if (_route != null)
            {
                _dateEdit.Value = _route.DateAsDateTime;
                _routeNameComboBox.SelectedItem = _route.RouteName;

                // Handle null values for ComboBox SelectedValue - Syncfusion doesn't accept null
                // Set SelectedIndex to -1 (no selection) for null values
                if (_route.AMBusId.HasValue)
                    _amVehicleComboBox.SelectedValue = _route.AMBusId.Value;
                else
                    _amVehicleComboBox.SelectedIndex = -1;

                _amBeginMilesTextBox.Value = (double?)_route.AMBeginMiles;
                _amEndMilesTextBox.Value = (double?)_route.AMEndMiles;
                _amRidersTextBox.Value = (double?)_route.AMRiders;

                if (_route.AMDriverId.HasValue)
                    _amDriverComboBox.SelectedValue = _route.AMDriverId.Value;
                else
                    _amDriverComboBox.SelectedIndex = -1;

                if (_route.PMBusId.HasValue)
                    _pmVehicleComboBox.SelectedValue = _route.PMBusId.Value;
                else
                    _pmVehicleComboBox.SelectedIndex = -1;

                _pmBeginMilesTextBox.Value = (double?)_route.PMBeginMiles;
                _pmEndMilesTextBox.Value = (double?)_route.PMEndMiles;
                _pmRidersTextBox.Value = (double?)_route.PMRiders;

                if (_route.PMDriverId.HasValue)
                    _pmDriverComboBox.SelectedValue = _route.PMDriverId.Value;
                else
                    _pmDriverComboBox.SelectedIndex = -1;

                _notesTextBox.Text = _route.Notes ?? "";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate RouteName
                if (string.IsNullOrWhiteSpace(_routeNameComboBox.SelectedItem?.ToString()))
                {
                    MessageBox.Show("Route Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _routeNameComboBox.Focus();
                    return;
                }

                // Validate numeric inputs - BeginMiles and EndMiles
                if (_amBeginMilesTextBox.Value.HasValue && _amEndMilesTextBox.Value.HasValue)
                {
                    if (_amEndMilesTextBox.Value < _amBeginMilesTextBox.Value)
                    {
                        MessageBox.Show("End Miles must be greater than or equal to Begin Miles.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _amEndMilesTextBox.Focus();
                        return;
                    }
                }

                // Validate negative values for AM
                if (_amBeginMilesTextBox.Value.HasValue && _amBeginMilesTextBox.Value < 0)
                {
                    MessageBox.Show("Begin Miles cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _amBeginMilesTextBox.Focus();
                    return;
                }

                if (_amEndMilesTextBox.Value.HasValue && _amEndMilesTextBox.Value < 0)
                {
                    MessageBox.Show("End Miles cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _amEndMilesTextBox.Focus();
                    return;
                }

                if (_amRidersTextBox.Value.HasValue && _amRidersTextBox.Value < 0)
                {
                    MessageBox.Show("Number of Riders cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _amRidersTextBox.Focus();
                    return;
                }

                // Validate PM data if provided
                if (_pmBeginMilesTextBox.Value.HasValue && _pmEndMilesTextBox.Value.HasValue)
                {
                    if (_pmEndMilesTextBox.Value < _pmBeginMilesTextBox.Value)
                    {
                        MessageBox.Show("PM End Miles must be greater than or equal to PM Begin Miles.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _pmEndMilesTextBox.Focus();
                        return;
                    }
                }

                if (_pmBeginMilesTextBox.Value.HasValue && _pmBeginMilesTextBox.Value < 0)
                {
                    MessageBox.Show("PM Begin Miles cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _pmBeginMilesTextBox.Focus();
                    return;
                }

                if (_pmEndMilesTextBox.Value.HasValue && _pmEndMilesTextBox.Value < 0)
                {
                    MessageBox.Show("PM End Miles cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _pmEndMilesTextBox.Focus();
                    return;
                }

                if (_pmRidersTextBox.Value.HasValue && _pmRidersTextBox.Value < 0)
                {
                    MessageBox.Show("PM Number of Riders cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _pmRidersTextBox.Focus();
                    return;
                }

                // Update route object with form data (RouteDate, RouteName, BusId schema)
                _route.DateAsDateTime = _dateEdit.Value ?? DateTime.Today;
                _route.RouteName = _routeNameComboBox.SelectedItem?.ToString();
                _route.AMBusId = (int?)_amVehicleComboBox.SelectedValue;
                _route.AMBeginMiles = (int?)_amBeginMilesTextBox.Value;
                _route.AMEndMiles = (int?)_amEndMilesTextBox.Value;
                _route.AMRiders = _amRidersTextBox.Value.HasValue ? (int?)_amRidersTextBox.Value : null;
                _route.AMDriverId = (int?)_amDriverComboBox.SelectedValue;

                // Save PM data if provided
                _route.PMBusId = (int?)_pmVehicleComboBox.SelectedValue;
                _route.PMBeginMiles = (int?)_pmBeginMilesTextBox.Value;
                _route.PMEndMiles = (int?)_pmEndMilesTextBox.Value;
                _route.PMRiders = _pmRidersTextBox.Value.HasValue ? (int?)_pmRidersTextBox.Value : null;
                _route.PMDriverId = (int?)_pmDriverComboBox.SelectedValue;

                // Keep notes
                _route.Notes = _notesTextBox.Text;

                // Save to database using RouteRepository
                if (_isEditMode)
                {
                    _routeRepository.UpdateRoute(_route);
                }
                else
                {
                    _routeRepository.AddRoute(_route);
                }

                MessageBox.Show("Route data saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving route: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Repository disposal removed - handled by dependency injection
            }
            base.Dispose(disposing);
        }
    }
}

