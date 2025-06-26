using BusBuddy.Models;
using BusBuddy.UI.Base;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.Input;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Views
{
    public partial class ActivityFormSyncfusion : SyncfusionBaseForm
    {
        private TextBoxExt _activityNameTextBox;
        private TextBoxExt _descriptionTextBox;
        private SfDateTimeEdit _activityDatePicker;
        private TextBoxExt _locationTextBox;
        private ComboBoxAdv _statusComboBox;
        private TextBoxExt _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public Activity Activity { get; private set; }

        public ActivityFormSyncfusion() : this(new Activity())
        {
        }

        public ActivityFormSyncfusion(Activity activity)
        {
            Activity = activity;
            InitializeComponent();
            if (activity != null)
            {
                PopulateFields(activity);
            }
        }

        private void InitializeComponent()
        {
            this.Text = Activity.ActivityID == 0 ? "Add Activity" : "Edit Activity";
            this.ClientSize = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();
        }

        private void CreateControls()
        {
            var activityNameLabel = new Label { Text = "Activity Name:", Location = new Point(20, 25), Size = new Size(100, 20) };
            var descriptionLabel = new Label { Text = "Description:", Location = new Point(300, 25), Size = new Size(100, 20) };
            var activityDateLabel = new Label { Text = "Activity Date:", Location = new Point(20, 95), Size = new Size(100, 20) };
            var locationLabel = new Label { Text = "Location:", Location = new Point(300, 95), Size = new Size(100, 20) };
            var statusLabel = new Label { Text = "Status:", Location = new Point(20, 165), Size = new Size(100, 20) };
            var notesLabel = new Label { Text = "Notes:", Location = new Point(20, 235), Size = new Size(100, 20) };

            _mainPanel.Controls.AddRange(new Control[] {
                activityNameLabel, descriptionLabel, activityDateLabel, locationLabel,
                statusLabel, notesLabel
            });

            _activityNameTextBox = new TextBoxExt
            {
                Location = new Point(20, 50),
                Size = new Size(250, 25)
            };

            _descriptionTextBox = new TextBoxExt
            {
                Location = new Point(300, 50),
                Size = new Size(250, 25)
            };

            // Reference: https://help.syncfusion.com/windowsforms/datetime-picker/getting-started
            _activityDatePicker = new SfDateTimeEdit
            {
                Location = new Point(20, 120),
                Size = new Size(250, 25),
                Value = DateTime.Now
            };

            _locationTextBox = new TextBoxExt
            {
                Location = new Point(300, 120),
                Size = new Size(250, 25)
            };

            _statusComboBox = new ComboBoxAdv
            {
                Location = new Point(20, 190),
                Size = new Size(250, 25)
            };
            _statusComboBox.Items.AddRange(new[] { "Scheduled", "In Progress", "Completed", "Cancelled" });

            _notesTextBox = new TextBoxExt
            {
                Location = new Point(20, 260),
                Size = new Size(530, 100),
                Multiline = true
            };

            _mainPanel.Controls.AddRange(new Control[] {
                _activityNameTextBox, _descriptionTextBox, _activityDatePicker, _locationTextBox,
                _statusComboBox, _notesTextBox
            });
        }

        private void LayoutControls()
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60
            };

            // Reference: https://help.syncfusion.com/windowsforms/button/getting-started
            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(375, 15),
                Size = new Size(80, 30)
            };

            // Reference: https://help.syncfusion.com/windowsforms/button/getting-started
            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(470, 15),
                Size = new Size(80, 30)
            };

            buttonPanel.Controls.AddRange(new Control[] { _saveButton, _cancelButton });
            this.Controls.Add(buttonPanel);
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += SaveButton_Click;
            _cancelButton.Click += CancelButton_Click;
        }

        private void PopulateFields(Activity activity)
        {
            _activityNameTextBox.Text = activity.ActivityType ?? "";
            _descriptionTextBox.Text = activity.RequestedBy ?? "";
            _activityDatePicker.Value = activity.DateAsDateTime ?? DateTime.Now;
            _locationTextBox.Text = activity.Destination ?? "";
            _statusComboBox.Text = "Scheduled"; // Status not in model, use default
            _notesTextBox.Text = activity.Notes ?? "";
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                Activity.ActivityType = _activityNameTextBox.Text;
                Activity.RequestedBy = _descriptionTextBox.Text;
                Activity.DateAsDateTime = _activityDatePicker.Value;
                Activity.Destination = _locationTextBox.Text;
                // Status not in model - skip
                Activity.Notes = _notesTextBox.Text;

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving activity: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
