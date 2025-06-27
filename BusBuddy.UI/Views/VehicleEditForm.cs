using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.ListView;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Vehicle Edit Form - Shell Structure
    /// Handles adding and editing vehicle records
    /// Based on Syncfusion controls documentation
    ///
    /// 📖 SYNCFUSION DOCUMENTATION:
    /// - SfButton: https://help.syncfusion.com/windowsforms/button/getting-started
    /// - TextBoxExt: https://help.syncfusion.com/windowsforms/textbox/getting-started
    /// </summary>
    public partial class VehicleEditForm : SyncfusionBaseForm
    {
        #region Private Fields
        private readonly IMessageService _messageService;
        private bool _isEditMode;

        // Form controls - Shell implementation
        private SfButton _saveButton;
        private SfButton _cancelButton;
        private TextBoxExt _vehicleNumberTextBox;
        private TextBoxExt _vinTextBox;
        private SfComboBox _vehicleTypeComboBox;
        private SfComboBox _statusComboBox;
        #endregion

        #region Properties
        public Vehicle Vehicle { get; private set; }
        public bool IsEditMode => _isEditMode;
        #endregion

        #region Constructors
        public VehicleEditForm() : this(null, new MessageBoxService())
        {
        }

        public VehicleEditForm(Vehicle vehicle) : this(vehicle, new MessageBoxService())
        {
        }

        public VehicleEditForm(Vehicle vehicle, IMessageService messageService) : base()
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            Vehicle = vehicle;
            _isEditMode = vehicle != null;

            InitializeComponent();
            SetupForm();
        }
        #endregion

        #region Form Setup - Shell Implementation
        private void SetupForm()
        {
            Text = _isEditMode ? "Edit Vehicle" : "Add New Vehicle";

            if (_isEditMode && Vehicle != null)
            {
                LoadVehicleData();
            }
        }

        private void InitializeComponent()
        {
            SetupFormProperties();
            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void SetupFormProperties()
        {
            this.Text = _isEditMode ? "Edit Vehicle" : "Add Vehicle";
            this.ClientSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void CreateControls()
        {
            // Save Button - SfButton per documentation
            _saveButton = new SfButton
            {
                Text = "Save",
                Size = new Size(75, 30)
            };

            // Cancel Button - SfButton
            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Size = new Size(75, 30)
            };

            // Vehicle Number - TextBoxExt
            _vehicleNumberTextBox = new TextBoxExt
            {
                Size = new Size(150, 23)
            };

            // VIN - TextBoxExt
            _vinTextBox = new TextBoxExt
            {
                Size = new Size(200, 23)
            };

            // Vehicle Type - SfComboBox
            _vehicleTypeComboBox = new SfComboBox
            {
                Size = new Size(150, 23),
                DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList
            };

            // Status - SfComboBox
            _statusComboBox = new SfComboBox
            {
                Size = new Size(150, 23),
                DropDownStyle = Syncfusion.WinForms.ListView.Enums.DropDownStyle.DropDownList
            };

            this.Controls.Add(_saveButton);
            this.Controls.Add(_cancelButton);
            this.Controls.Add(_vehicleNumberTextBox);
            this.Controls.Add(_vinTextBox);
            this.Controls.Add(_vehicleTypeComboBox);
            this.Controls.Add(_statusComboBox);
        }

        private void LayoutControls()
        {
            // Basic layout implementation
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        private void LoadVehicleData()
        {
            if (Vehicle == null) return;

            try
            {
                _vehicleNumberTextBox.Text = Vehicle.VehicleNumber ?? string.Empty;
                _vinTextBox.Text = Vehicle.VINNumber ?? string.Empty;

                // Populate vehicle type - for now use Make + Model as type
                var vehicleType = $"{Vehicle.Make} {Vehicle.Model}".Trim();
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    _vehicleTypeComboBox.Text = vehicleType;
                }

                // Set status
                _statusComboBox.Text = Vehicle.Status ?? "Active";
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error loading vehicle data: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers - Shell Implementation
        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                var vehicleToSave = CreateVehicleFromForm();

                // For now, just show success message
                // In a full implementation, this would save to repository
                _messageService?.ShowInfo($"Vehicle {vehicleToSave.VehicleNumber} would be saved");

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _messageService?.ShowError($"Error saving vehicle: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion

        #region Validation - Shell Implementation
        protected override bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(_vehicleNumberTextBox.Text))
            {
                _messageService?.ShowError("Vehicle number is required.");
                _vehicleNumberTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_vinTextBox.Text))
            {
                _messageService?.ShowError("VIN number is required.");
                _vinTextBox.Focus();
                return false;
            }

            return true;
        }

        private Vehicle CreateVehicleFromForm()
        {
            var vehicle = new Vehicle();

            if (_isEditMode && Vehicle != null)
            {
                vehicle.VehicleID = Vehicle.VehicleID;
            }

            vehicle.VehicleNumber = _vehicleNumberTextBox.Text?.Trim();
            vehicle.VINNumber = _vinTextBox.Text?.Trim();
            vehicle.Status = _statusComboBox.Text?.Trim() ?? "Active";

            // Parse vehicle type back to Make/Model if needed
            var vehicleType = _vehicleTypeComboBox.Text?.Trim();
            if (!string.IsNullOrEmpty(vehicleType))
            {
                var parts = vehicleType.Split(' ', 2);
                vehicle.Make = parts.Length > 0 ? parts[0] : vehicleType;
                vehicle.Model = parts.Length > 1 ? parts[1] : "";
            }

            return vehicle;
        }
        #endregion

        #region Disposal
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of form-specific resources
                _saveButton?.Dispose();
                _cancelButton?.Dispose();
                _vehicleNumberTextBox?.Dispose();
                _vinTextBox?.Dispose();
                _vehicleTypeComboBox?.Dispose();
                _statusComboBox?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
