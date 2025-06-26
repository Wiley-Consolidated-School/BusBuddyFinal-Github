using System;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;
using Syncfusion.WinForms.ListView;
using BusBuddy.UI.Helpers;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Views; // Add this for ControlFactory access

namespace BusBuddy.UI.Views
{
    public class ActivityEditFormSyncfusion : SyncfusionBaseForm
    {
        public Activity Activity { get; private set; }

        private DateTimePicker? dtpDate;
        private ComboBox? cboActivityType;
        private Control? txtDestination;
        private Control? txtLeaveTime;
        private Control? txtEventTime;
        private Control? txtReturnTime;
        private Control? txtRequestedBy;
        private Control? txtNotes;
        private Control? btnSave;
        private Control? btnCancel;

        public ActivityEditFormSyncfusion(Activity? activity = null)
        {
            Activity = activity != null ? new Activity
            {
                ActivityID = activity.ActivityID,
                Date = activity.Date,
                ActivityType = activity.ActivityType,
                Destination = activity.Destination,
                LeaveTime = activity.LeaveTime,
                EventTime = activity.EventTime,
                ReturnTime = activity.ReturnTime,
                RequestedBy = activity.RequestedBy,
                AssignedVehicleID = activity.AssignedVehicleID,
                DriverID = activity.DriverID,
                Notes = activity.Notes
            } : new Activity();

            InitializeComponent();
            LoadActivityData();
        }

        private void InitializeComponent()
        {
            this.Text = Activity.ActivityID == 0 ? "🎭 Add Activity" : "🎭 Edit Activity";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Configure for high DPI
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);

            SetupFormLayout();
        }

        private void SetupFormLayout()
        {
            int y = 30;
            int labelX = 30;
            int controlX = 150;
            int spacing = 50;
            int controlWidth = 280;

            // Date
            var lblDate = ControlFactory.CreateLabel("📅 Date:");
            lblDate.Location = new Point(labelX, y);
            this.Controls.Add(lblDate);
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpDate);
            y += spacing;

            // Activity Type
            var lblActivityType = ControlFactory.CreateLabel("🎭 Activity Type:");
            lblActivityType.Location = new Point(labelX, y);
            this.Controls.Add(lblActivityType);
            cboActivityType = new ComboBox
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboActivityType.Items.AddRange(new[] { "Sports Trip", "Activity Trip", "Field Trip", "Special Event" });
            this.Controls.Add(cboActivityType);
            y += spacing;

            // Destination
            var lblDestination = ControlFactory.CreateLabel("📍 Destination:");
            lblDestination.Location = new Point(labelX, y);
            this.Controls.Add(lblDestination);
            txtDestination = ControlFactory.CreateTextBox("Enter destination");
            txtDestination.Location = new Point(controlX, y);
            txtDestination.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtDestination);
            y += spacing;

            // Leave Time
            var lblLeaveTime = ControlFactory.CreateLabel("🕐 Leave Time:");
            lblLeaveTime.Location = new Point(labelX, y);
            this.Controls.Add(lblLeaveTime);
            txtLeaveTime = ControlFactory.CreateTextBox("HH:MM (e.g., 08:30)");
            txtLeaveTime.Location = new Point(controlX, y);
            txtLeaveTime.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtLeaveTime);
            y += spacing;

            // Event Time
            var lblEventTime = ControlFactory.CreateLabel("🕒 Event Time:");
            lblEventTime.Location = new Point(labelX, y);
            this.Controls.Add(lblEventTime);
            txtEventTime = ControlFactory.CreateTextBox("HH:MM (e.g., 10:00)");
            txtEventTime.Location = new Point(controlX, y);
            txtEventTime.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtEventTime);
            y += spacing;

            // Return Time
            var lblReturnTime = ControlFactory.CreateLabel("🕕 Return Time:");
            lblReturnTime.Location = new Point(labelX, y);
            this.Controls.Add(lblReturnTime);
            txtReturnTime = ControlFactory.CreateTextBox("HH:MM (e.g., 15:30)");
            txtReturnTime.Location = new Point(controlX, y);
            txtReturnTime.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtReturnTime);
            y += spacing;

            // Requested By
            var lblRequestedBy = ControlFactory.CreateLabel("👤 Requested By:");
            lblRequestedBy.Location = new Point(labelX, y);
            this.Controls.Add(lblRequestedBy);
            txtRequestedBy = ControlFactory.CreateTextBox("Enter requester name");
            txtRequestedBy.Location = new Point(controlX, y);
            txtRequestedBy.Size = new Size(controlWidth, 30);
            this.Controls.Add(txtRequestedBy);
            y += spacing;

            // Notes
            var lblNotes = ControlFactory.CreateLabel("📝 Notes:");
            lblNotes.Location = new Point(labelX, y);
            this.Controls.Add(lblNotes);
            txtNotes = ControlFactory.CreateTextBox("Additional notes", true);
            txtNotes.Location = new Point(controlX, y);
            txtNotes.Size = new Size(controlWidth, 60);
            this.Controls.Add(txtNotes);
            y += 80;            // Buttons
            btnSave = ControlFactory.CreateButton("💾 Save", new Size(120, 36), btnSave_Click);
            btnSave.Location = new Point(controlX, y);
            btnSave.BackColor = BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme);
            this.Controls.Add(btnSave);

            btnCancel = ControlFactory.CreateButton("❌ Cancel", new Size(120, 36), btnCancel_Click);
            btnCancel.Location = new Point(controlX + 140, y);
            btnCancel.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            this.Controls.Add(btnCancel);

            // Apply BusBuddy Material Design styling
            ApplyBusBuddyStyling();
        }

        private void ApplyBusBuddyStyling()
        {
            // Apply BusBuddy theme to the form - safe approach
            BusBuddyThemeManager.ApplyTheme(this, BusBuddyThemeManager.SupportedThemes.Office2016White);

            // Configure date picker with safe defaults
            dtpDate.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            dtpDate.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);

            // Style all labels with safe approach
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                    label.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }
            }
        }

        private void LoadActivityData()
        {
            if (Activity.DateAsDateTime.HasValue)
                dtpDate.Value = Activity.DateAsDateTime.Value;
            else
                dtpDate.Value = DateTime.Today;

            cboActivityType.SelectedItem = Activity.ActivityType ?? "Sports Trip";
            txtDestination.Text = Activity.Destination ?? "";
            txtLeaveTime.Text = Activity.LeaveTime ?? "";
            txtEventTime.Text = Activity.EventTime ?? "";
            txtReturnTime.Text = Activity.ReturnTime ?? "";
            txtRequestedBy.Text = Activity.RequestedBy ?? "";
            txtNotes.Text = Activity.Notes ?? "";
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateActivity())
                return;

            Activity.DateAsDateTime = dtpDate.Value;
            Activity.ActivityType = cboActivityType.SelectedItem?.ToString();
            Activity.Destination = txtDestination.Text.Trim();
            Activity.LeaveTime = txtLeaveTime.Text.Trim();
            Activity.EventTime = txtEventTime.Text.Trim();
            Activity.ReturnTime = txtReturnTime.Text.Trim();
            Activity.RequestedBy = txtRequestedBy.Text.Trim();
            Activity.Notes = txtNotes.Text.Trim();

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool ValidateActivity()
        {
            ClearAllValidationErrors();

            if (cboActivityType.SelectedIndex < 0)
            {
                SetValidationError(cboActivityType, "Please select an activity type.");
                ShowErrorMessage("Please select an activity type.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDestination.Text))
            {
                SetValidationError(txtDestination, "Destination is required.");
                ShowErrorMessage("Destination is required.");
                return false;
            }

            // Validate time formats if provided
            if (!string.IsNullOrWhiteSpace(txtLeaveTime.Text) && !IsValidTimeFormat(txtLeaveTime.Text))
            {
                SetValidationError(txtLeaveTime, "Please enter time in HH:MM format (e.g., 08:30).");
                ShowErrorMessage("Please enter leave time in HH:MM format (e.g., 08:30).");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEventTime.Text) && !IsValidTimeFormat(txtEventTime.Text))
            {
                SetValidationError(txtEventTime, "Please enter time in HH:MM format (e.g., 10:00).");
                ShowErrorMessage("Please enter event time in HH:MM format (e.g., 10:00).");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtReturnTime.Text) && !IsValidTimeFormat(txtReturnTime.Text))
            {
                SetValidationError(txtReturnTime, "Please enter time in HH:MM format (e.g., 15:30).");
                ShowErrorMessage("Please enter return time in HH:MM format (e.g., 15:30).");
                return false;
            }

            return true;
        }

        private bool IsValidTimeFormat(string timeText)
        {
            if (TimeSpan.TryParse(timeText, out _))
                return true;

            // Try parsing with common formats
            if (DateTime.TryParseExact(timeText, new[] { "HH:mm", "H:mm", "HH:MM", "H:MM" },
                null, System.Globalization.DateTimeStyles.None, out _))
                return true;

            return false;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
