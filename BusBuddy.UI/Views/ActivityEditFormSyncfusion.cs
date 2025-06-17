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
            var lblDate = CreateLabel("📅 Date:", labelX, y);
            dtpDate = new DateTimePicker
            {
                Location = new Point(controlX, y),
                Size = new Size(controlWidth, 30),
                Format = DateTimePickerFormat.Short
            };
            this.Controls.Add(dtpDate);
            y += spacing;

            // Activity Type
            var lblActivityType = CreateLabel("🎭 Activity Type:", labelX, y);
            cboActivityType = CreateComboBox(controlX, y, controlWidth);
            cboActivityType.Items.AddRange(new[] { "Sports Trip", "Activity Trip", "Field Trip", "Special Event" });
            y += spacing;

            // Destination
            var lblDestination = CreateLabel("📍 Destination:", labelX, y);
            txtDestination = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtDestination, "Enter destination");
            y += spacing;

            // Leave Time
            var lblLeaveTime = CreateLabel("🕐 Leave Time:", labelX, y);
            txtLeaveTime = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtLeaveTime, "HH:MM (e.g., 08:30)");
            y += spacing;

            // Event Time
            var lblEventTime = CreateLabel("🕒 Event Time:", labelX, y);
            txtEventTime = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtEventTime, "HH:MM (e.g., 10:00)");
            y += spacing;

            // Return Time
            var lblReturnTime = CreateLabel("🕕 Return Time:", labelX, y);
            txtReturnTime = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtReturnTime, "HH:MM (e.g., 15:30)");
            y += spacing;

            // Requested By
            var lblRequestedBy = CreateLabel("👤 Requested By:", labelX, y);
            txtRequestedBy = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtRequestedBy, "Enter requester name");
            y += spacing;

            // Notes
            var lblNotes = CreateLabel("📝 Notes:", labelX, y);
            txtNotes = CreateTextBox(controlX, y, controlWidth);
            SetPlaceholderText(txtNotes, "Additional notes");
            SetTextBoxMultiline(txtNotes, 60);
            y += 80;

            // Buttons
            btnSave = CreateButton("💾 Save", controlX, y, btnSave_Click);
            btnSave.BackColor = SyncfusionThemeHelper.MaterialColors.Primary;
            btnSave.Size = new Size(120, 36);

            btnCancel = CreateButton("❌ Cancel", controlX + 140, y, btnCancel_Click);
            btnCancel.BackColor = SyncfusionThemeHelper.MaterialColors.Background;
            btnCancel.Size = new Size(120, 36);

            // Apply Syncfusion Material Design styling
            ApplySyncfusionStyling();
        }

        private void ApplySyncfusionStyling()
        {
            // Apply Syncfusion Material theme to the form
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            // Configure date picker for Material Design
            dtpDate.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;
            dtpDate.BackColor = Color.White;

            // Style all labels
            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    label.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;
                    label.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
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

        // Helper methods for validation and form management
        private void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        private void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Helper methods for Syncfusion form creation
        private ComboBox CreateComboBox(int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Apply Material theming
            comboBox.BackColor = SyncfusionThemeHelper.MaterialColors.Surface;
            comboBox.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
            comboBox.Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont;

            this.Controls.Add(comboBox);
            return comboBox;
        }

        private void SetPlaceholderText(Control textBox, string placeholder)
        {
            if (textBox is TextBox tb)
            {
                tb.Text = placeholder;
                tb.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Text = "";
                        tb.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.ForeColor = SyncfusionThemeHelper.MaterialColors.TextSecondary;
                    }
                };
            }
        }

        private void SetTextBoxMultiline(Control textBox, int height)
        {
            if (textBox is TextBox tb)
            {
                tb.Multiline = true;
                tb.Height = height;
                tb.ScrollBars = ScrollBars.Vertical;
            }
        }

        private Control CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 36),
                Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont,
                BackColor = SyncfusionThemeHelper.MaterialColors.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            this.Controls.Add(button);
            return button;
        }
    }
}

