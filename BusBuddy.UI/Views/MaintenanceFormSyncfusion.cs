using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    public partial class MaintenanceFormSyncfusion : SyncfusionBaseForm
    {
        private MaintenanceRepository _maintenanceRepository;
        private VehicleRepository _vehicleRepository;
        private Maintenance _maintenance;
        private bool _isEditMode;

        // Form controls
        private SfDateTimeEdit _dateEdit;
        private ComboBox _vehicleComboBox;
        private SfNumericTextBox _odometerTextBox;
        private ComboBox _categoryComboBox;
        private TextBox _vendorTextBox;
        private SfNumericTextBox _repairCostTextBox;
        private TextBox _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public MaintenanceFormSyncfusion()
        {
            InitializeComponent();
            _maintenanceRepository = new MaintenanceRepository();
            _vehicleRepository = new VehicleRepository();
            _maintenance = new Maintenance();
            _isEditMode = false;
        }

        public MaintenanceFormSyncfusion(Maintenance maintenance) : this()
        {
            _maintenance = maintenance ?? new Maintenance();
            _isEditMode = maintenance != null && maintenance.MaintenanceID > 0;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "Maintenance Entry";
            Size = new Size(500, 600);
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
                Size = new Size(300, 30),
                Value = DateTime.Today
            };
            Controls.Add(_dateEdit);

            // Vehicle
            var vehicleLabel = new Label
            {
                Text = "Vehicle:",
                Location = new Point(20, 80),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(vehicleLabel);

            _vehicleComboBox = new ComboBox
            {
                Location = new Point(130, 80),
                Size = new Size(300, 30),
                DisplayMember = "VehicleNumber",
                ValueMember = "VehicleID",
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            Controls.Add(_vehicleComboBox);

            // Odometer Reading
            var odometerLabel = new Label
            {
                Text = "Odometer:",
                Location = new Point(20, 130),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(odometerLabel);

            _odometerTextBox = new SfNumericTextBox
            {
                Location = new Point(130, 130),
                Size = new Size(300, 30)
            };
            Controls.Add(_odometerTextBox);

            // Category
            var categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(20, 180),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(categoryLabel);

            _categoryComboBox = new ComboBox
            {
                Location = new Point(130, 180),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryComboBox.Items.Add("Tires");
            _categoryComboBox.Items.Add("Windshield");
            _categoryComboBox.Items.Add("Alignment");
            _categoryComboBox.Items.Add("Mechanical");
            _categoryComboBox.Items.Add("Car Wash");
            _categoryComboBox.Items.Add("Cleaning");
            _categoryComboBox.Items.Add("Accessory Install");
            Controls.Add(_categoryComboBox);

            // Vendor
            var vendorLabel = new Label
            {
                Text = "Vendor:",
                Location = new Point(20, 230),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(vendorLabel);

            _vendorTextBox = new TextBox
            {
                Location = new Point(130, 230),
                Size = new Size(300, 30)
            };
            Controls.Add(_vendorTextBox);

            // Repair Cost
            var costLabel = new Label
            {
                Text = "Repair Cost:",
                Location = new Point(20, 280),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(costLabel);

            _repairCostTextBox = new SfNumericTextBox
            {
                Location = new Point(130, 280),
                Size = new Size(300, 30)
            };
            Controls.Add(_repairCostTextBox);

            // Notes
            var notesLabel = new Label
            {
                Text = "Notes:",
                Location = new Point(20, 330),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(notesLabel);

            _notesTextBox = new TextBox
            {
                Location = new Point(130, 330),
                Size = new Size(300, 100),
                Multiline = true
            };
            Controls.Add(_notesTextBox);

            // Buttons
            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(260, 460),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme)
            };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);

            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(350, 460),
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
            LoadMaintenanceData();
        }

        private void LoadVehicles()
        {
            try
            {
                var vehicles = _vehicleRepository.GetAllVehicles().ToList();
                _vehicleComboBox.DataSource = vehicles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMaintenanceData()
        {
            if (_maintenance != null)
            {
                _dateEdit.Value = _maintenance.DateAsDateTime ?? DateTime.Today;
                _vehicleComboBox.SelectedValue = _maintenance.VehicleID;
                _odometerTextBox.Value = (double?)_maintenance.OdometerReading;
                _categoryComboBox.SelectedItem = _maintenance.MaintenanceCompleted;
                _vendorTextBox.Text = _maintenance.Vendor ?? "";
                _repairCostTextBox.Value = (double?)_maintenance.RepairCost;
                _notesTextBox.Text = _maintenance.Notes ?? "";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (_vehicleComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Please select a vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update maintenance object
                _maintenance.DateAsDateTime = _dateEdit.Value;
                _maintenance.VehicleID = (int)_vehicleComboBox.SelectedValue;
                _maintenance.OdometerReading = (decimal?)_odometerTextBox.Value;
                _maintenance.MaintenanceCompleted = _categoryComboBox.SelectedItem?.ToString();
                _maintenance.Vendor = _vendorTextBox.Text;
                _maintenance.RepairCost = (decimal?)_repairCostTextBox.Value;
                _maintenance.Notes = _notesTextBox.Text;

                // Save to database
                if (_isEditMode)
                {
                    _maintenanceRepository.UpdateMaintenance(_maintenance);
                }
                else
                {
                    _maintenanceRepository.Add(_maintenance);
                }

                MessageBox.Show("Maintenance record saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving maintenance record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
