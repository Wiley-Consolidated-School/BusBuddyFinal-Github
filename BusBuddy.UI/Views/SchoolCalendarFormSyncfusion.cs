using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BusBuddy.Data;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    public partial class SchoolCalendarFormSyncfusion : SyncfusionBaseForm
    {
        private SchoolCalendarRepository _schoolCalendarRepository;
        private SchoolCalendar _schoolCalendar;
        private bool _isEditMode;

        // Form controls
        private SfDateTimeEdit _dateEdit;
        private SfDateTimeEdit _endDateEdit;
        private ComboBox _categoryComboBox;
        private TextBox _descriptionTextBox;
        private CheckBox _routeNeededCheckBox;
        private TextBox _notesTextBox;
        private SfButton _saveButton;
        private SfButton _cancelButton;

        public SchoolCalendarFormSyncfusion(System.IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();
            _schoolCalendarRepository = new SchoolCalendarRepository();
            _schoolCalendar = new SchoolCalendar();
            _isEditMode = false;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form properties
            Text = "School Calendar Entry";
            Size = new Size(500, 550);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = System.Drawing.Color.White;

            // Start Date
            var dateLabel = new Label
            {
                Text = "Start Date:",
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

            // End Date
            var endDateLabel = new Label
            {
                Text = "End Date:",
                Location = new Point(20, 80),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(endDateLabel);

            _endDateEdit = new SfDateTimeEdit
            {
                Location = new Point(130, 80),
                Size = new Size(300, 30),
                Value = DateTime.Today,
                AllowNull = true
            };
            Controls.Add(_endDateEdit);

            // Category
            var categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(20, 130),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(categoryLabel);

            _categoryComboBox = new ComboBox
            {
                Location = new Point(130, 130),
                Size = new Size(300, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryComboBox.Items.Add("School Day");
            _categoryComboBox.Items.Add("Holiday");
            _categoryComboBox.Items.Add("Thanksgiving Break");
            _categoryComboBox.Items.Add("Christmas Break");
            _categoryComboBox.Items.Add("Spring Break");
            _categoryComboBox.Items.Add("Key Event");
            Controls.Add(_categoryComboBox);

            // Description
            var descriptionLabel = new Label
            {
                Text = "Description:",
                Location = new Point(20, 180),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(descriptionLabel);

            _descriptionTextBox = new TextBox
            {
                Location = new Point(130, 180),
                Size = new Size(300, 30)
            };
            Controls.Add(_descriptionTextBox);

            // Route Needed
            _routeNeededCheckBox = new CheckBox
            {
                Text = "Route Needed",
                Location = new Point(130, 230),
                Size = new Size(200, 23)
            };
            Controls.Add(_routeNeededCheckBox);

            // Notes
            var notesLabel = new Label
            {
                Text = "Notes:",
                Location = new Point(20, 280),
                Size = new Size(100, 23),
                Font = new System.Drawing.Font("Segoe UI", 9F)
            };
            Controls.Add(notesLabel);

            _notesTextBox = new TextBox
            {
                Location = new Point(130, 280),
                Size = new Size(300, 100),
                Multiline = true
            };
            Controls.Add(_notesTextBox);

            // Buttons
            _saveButton = new SfButton
            {
                Text = "Save",
                Location = new Point(260, 410),
                Size = new Size(80, 35),
                BackColor = BusBuddyThemeManager.ThemeColors.GetSuccessColor(BusBuddyThemeManager.CurrentTheme)
            };
            _saveButton.Click += SaveButton_Click;
            Controls.Add(_saveButton);

            _cancelButton = new SfButton
            {
                Text = "Cancel",
                Location = new Point(350, 410),
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
            LoadSchoolCalendarData();
        }

        private void LoadSchoolCalendarData()
        {
            if (_schoolCalendar != null)
            {
                _dateEdit.Value = _schoolCalendar.DateAsDateTime ?? DateTime.Today;
                _endDateEdit.Value = _schoolCalendar.EndDateAsDateTime;
                _categoryComboBox.SelectedItem = _schoolCalendar.Category;
                _descriptionTextBox.Text = _schoolCalendar.Description ?? "";
                _routeNeededCheckBox.Checked = _schoolCalendar.RouteNeeded == 1;
                _notesTextBox.Text = _schoolCalendar.Notes ?? "";
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(_categoryComboBox.SelectedItem?.ToString()))
                {
                    MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Update school calendar object
                _schoolCalendar.DateAsDateTime = _dateEdit.Value;
                _schoolCalendar.EndDateAsDateTime = _endDateEdit.Value;
                _schoolCalendar.Category = _categoryComboBox.SelectedItem?.ToString();
                _schoolCalendar.Description = _descriptionTextBox.Text;
                _schoolCalendar.RouteNeeded = _routeNeededCheckBox.Checked ? 1 : 0;
                _schoolCalendar.Notes = _notesTextBox.Text;

                // Save to database
                if (_isEditMode)
                {
                    _schoolCalendarRepository.Update(_schoolCalendar);
                }
                else
                {
                    _schoolCalendarRepository.Add(_schoolCalendar);
                }

                MessageBox.Show("School calendar entry saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving school calendar entry: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

