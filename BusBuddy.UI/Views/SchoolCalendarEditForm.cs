using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using MaterialSkin.Controls;
using System.Collections.Generic;

namespace BusBuddy.UI.Views
{
    public partial class SchoolCalendarEditForm : StandardDataForm
    {
        private MaterialTextBox2 _descriptionTextBox;
        private MaterialTextBox2 _notesTextBox;
        private MaterialComboBox _categoryComboBox;
        private MaterialCheckbox _routeNeededCheckBox;
        private DateTimePicker? _startDatePicker;
        private DateTimePicker? _endDatePicker;
        private MaterialButton _saveButton;
        private MaterialButton _cancelButton;

        public SchoolCalendar? SchoolCalendar { get; private set; }

        public SchoolCalendarEditForm() : this(null)
        {
        }

        public SchoolCalendarEditForm(SchoolCalendar? schoolCalendar)
        {
            SchoolCalendar = schoolCalendar;
            InitializeComponent();
            if (schoolCalendar != null)
            {
                PopulateFields(schoolCalendar);
            }
        }

        private void InitializeComponent()
        {
            this.Text = SchoolCalendar == null ? "Add School Calendar Event" : "Edit School Calendar Event";
            this.ClientSize = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(20),
                AutoSize = true
            };

            // Configure column styles
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Configure row styles
            for (int i = 0; i < 8; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Start Date
            var startDateLabel = CreateLabel("Start Date:", Color.FromArgb(33, 33, 33));
            _startDatePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Value = DateTime.Today,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(startDateLabel, 0, 0);
            tableLayout.Controls.Add(_startDatePicker, 1, 0);

            // End Date
            var endDateLabel = CreateLabel("End Date:", Color.FromArgb(33, 33, 33));
            _endDatePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Value = DateTime.Today,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(endDateLabel, 0, 1);
            tableLayout.Controls.Add(_endDatePicker, 1, 1);

            // Category
            var categoryLabel = CreateLabel("Category:", Color.FromArgb(33, 33, 33));
            _categoryComboBox = new MaterialComboBox
            {
                Dock = DockStyle.Fill,
                Hint = "Select category",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            // Add predefined categories
            _categoryComboBox.Items.AddRange(new string[]
            {
                "School Day",
                "Holiday",
                "Thanksgiving Break",
                "Christmas Break",
                "Spring Break",
                "Key Event",
                "Professional Development",
                "Teacher Workday",
                "Half Day",
                "Early Release",
                "Other"
            });

            tableLayout.Controls.Add(categoryLabel, 0, 2);
            tableLayout.Controls.Add(_categoryComboBox, 1, 2);

            // Description
            var descriptionLabel = CreateLabel("Description:", Color.FromArgb(33, 33, 33));
            _descriptionTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Enter event description",
                Font = new Font("Roboto", 11F),
                Height = 36
            };

            tableLayout.Controls.Add(descriptionLabel, 0, 3);
            tableLayout.Controls.Add(_descriptionTextBox, 1, 3);

            // Route Needed
            var routeLabel = CreateLabel("Routes:", Color.FromArgb(33, 33, 33));
            _routeNeededCheckBox = new MaterialCheckbox
            {
                Text = "Routes needed for this event",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Height = 36,
                Font = new Font("Roboto", 10F)
            };

            tableLayout.Controls.Add(routeLabel, 0, 4);
            tableLayout.Controls.Add(_routeNeededCheckBox, 1, 4);

            // Notes
            var notesLabel = CreateLabel("Notes:", Color.FromArgb(33, 33, 33));
            _notesTextBox = new MaterialTextBox2
            {
                Dock = DockStyle.Fill,
                Hint = "Additional notes (optional)",
                Font = new Font("Roboto", 11F),
                Height = 80
            };

            tableLayout.Controls.Add(notesLabel, 0, 5);
            tableLayout.Controls.Add(_notesTextBox, 1, 5);

            // Button panel
            var buttonPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Height = 50
            };

            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            _saveButton = new MaterialButton
            {
                Text = "SAVE",
                Dock = DockStyle.Fill,
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained,
                Margin = new Padding(5)
            };
            _saveButton.Click += SaveButton_Click;

            _cancelButton = new MaterialButton
            {
                Text = "CANCEL",
                Dock = DockStyle.Fill,
                Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Outlined,
                Margin = new Padding(5)
            };
            _cancelButton.Click += CancelButton_Click;

            buttonPanel.Controls.Add(_saveButton, 0, 0);
            buttonPanel.Controls.Add(_cancelButton, 1, 0);

            // Add some spacing before buttons
            var spacer = new Panel { Height = 20 };
            tableLayout.Controls.Add(spacer, 0, 6);
            tableLayout.SetColumnSpan(spacer, 2);

            tableLayout.Controls.Add(buttonPanel, 0, 7);
            tableLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(tableLayout);

            // Set tab order
            _startDatePicker.TabIndex = 0;
            _endDatePicker.TabIndex = 1;
            _categoryComboBox.TabIndex = 2;
            _descriptionTextBox.TabIndex = 3;
            _routeNeededCheckBox.TabIndex = 4;
            _notesTextBox.TabIndex = 5;
            _saveButton.TabIndex = 6;
            _cancelButton.TabIndex = 7;
        }

        private void PopulateFields(SchoolCalendar schoolCalendar)
        {
            if (schoolCalendar.DateAsDateTime.HasValue)
                _startDatePicker.Value = schoolCalendar.DateAsDateTime.Value;

            if (schoolCalendar.EndDateAsDateTime.HasValue)
                _endDatePicker.Value = schoolCalendar.EndDateAsDateTime.Value;
            else
                _endDatePicker.Value = _startDatePicker.Value;

            _categoryComboBox.Text = schoolCalendar.Category ?? "";
            _descriptionTextBox.Text = schoolCalendar.Description ?? "";
            _routeNeededCheckBox.Checked = schoolCalendar.IsRouteNeeded;
            _notesTextBox.Text = schoolCalendar.Notes ?? "";
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var calendar = SchoolCalendar ?? new SchoolCalendar();

                calendar.DateAsDateTime = _startDatePicker.Value.Date;
                calendar.EndDateAsDateTime = _endDatePicker.Value.Date;
                calendar.Category = _categoryComboBox.Text?.Trim();
                calendar.Description = _descriptionTextBox.Text?.Trim();
                calendar.IsRouteNeeded = _routeNeededCheckBox.Checked;
                calendar.Notes = _notesTextBox.Text?.Trim();

                SchoolCalendar = calendar;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving school calendar event: {ex.Message}", "Error",
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

            if (string.IsNullOrWhiteSpace(_categoryComboBox.Text))
                errors.Add("Category is required.");

            if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
                errors.Add("Description is required.");

            if (_startDatePicker.Value > _endDatePicker.Value)
                errors.Add("Start date cannot be after end date.");

            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private Label CreateLabel(string text, Color foreColor)
        {
            return new Label
            {
                Text = text,
                ForeColor = foreColor,
                Font = new Font("Roboto", 11F, FontStyle.Regular),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 8, 0, 0)
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _descriptionTextBox?.Dispose();
                _notesTextBox?.Dispose();
                _categoryComboBox?.Dispose();
                _routeNeededCheckBox?.Dispose();
                _startDatePicker?.Dispose();
                _endDatePicker?.Dispose();
                _saveButton?.Dispose();
                _cancelButton?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

