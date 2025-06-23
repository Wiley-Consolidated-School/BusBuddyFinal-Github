using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// School Calendar Edit Form - Migrated to Syncfusion from MaterialSkin2
    /// Form for creating and editing school calendar events
    /// </summary>
    public partial class SchoolCalendarEditFormSyncfusion : SyncfusionBaseForm
    {
        private Control? _descriptionTextBox;
        private Control? _notesTextBox;
        private ComboBox? _categoryComboBox;
        private CheckBox? _routeNeededCheckBox;
        private DateTimePicker? _startDatePicker;
        private DateTimePicker? _endDatePicker;
        private Control? _saveButton;
        private Control? _cancelButton;

        public SchoolCalendar? SchoolCalendar { get; private set; }

        public SchoolCalendarEditFormSyncfusion() : this(null)
        {
        }

        public SchoolCalendarEditFormSyncfusion(SchoolCalendar? schoolCalendar)
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
            this.Text = SchoolCalendar == null ? "📅 Add School Calendar Event" : "📅 Edit School Calendar Event";
            this.ClientSize = GetDpiAwareSize(new Size(500, 450));
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply final theming
            BusBuddyThemeManager.ApplyTheme(this, BusBuddyThemeManager.SupportedThemes.Office2016White);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with Syncfusion controls");
        }

        private void CreateControls()
        {
            // Create text boxes
            _descriptionTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "");
            _notesTextBox = ControlFactory.CreateTextBox(_bannerTextProvider, "");

            // Make notes textbox multiline
            if (_notesTextBox is TextBox notesTextBox)
            {
                notesTextBox.Multiline = true;
                notesTextBox.Height = 80;
                notesTextBox.ScrollBars = ScrollBars.Vertical;
            }

            // Create combo box for category
            _categoryComboBox = CreateComboBox("Select category", 20, 120, 200);
            _categoryComboBox.Items.AddRange(new[] {
                "Academic", "Holiday", "Professional Development", "Event", "Other"
            });

            // Create checkbox
            _routeNeededCheckBox = CreateCheckBox("Route Transportation Needed", 250, 120);

            // Create date pickers
            _startDatePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(20), GetDpiAwareY(180)),
                Size = GetDpiAwareSize(new Size(200, 35)),
                Format = DateTimePickerFormat.Short
            };
            BusBuddyThemeManager.ApplyTheme(_startDatePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_startDatePicker);

            _endDatePicker = new DateTimePicker
            {
                Location = new Point(GetDpiAwareX(260), GetDpiAwareY(180)),
                Size = GetDpiAwareSize(new Size(200, 35)),
                Format = DateTimePickerFormat.Short
            };
            BusBuddyThemeManager.ApplyTheme(_endDatePicker, BusBuddyThemeManager.SupportedThemes.Office2016White);
            _mainPanel.Controls.Add(_endDatePicker);

            // Create buttons
            _saveButton = new Button { Text = "Save" } /* CreateStyledButton method removed */;
            _cancelButton = new Button { Text = "Cancel" } /* CreateStyledButton method removed */;

            // Configure buttons
            _saveButton.Size = GetDpiAwareSize(new Size(100, 35));
            _saveButton.Location = new Point(GetDpiAwareX(280), GetDpiAwareY(10));
            _saveButton.Click += SaveButton_Click;

            _cancelButton.Size = GetDpiAwareSize(new Size(100, 35));
            _cancelButton.Location = new Point(GetDpiAwareX(390), GetDpiAwareY(10));
            _cancelButton.Click += CancelButton_Click;

            // Style cancel button differently
            if (_cancelButton is Button cancelBtn)
            {
                cancelBtn.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
                cancelBtn.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            }

            _buttonPanel.Controls.Add(_saveButton);
            _buttonPanel.Controls.Add(_cancelButton);
        }

        private void LayoutControls()
        {
            // Labels
            ControlFactory.CreateLabel("Description:");
            ControlFactory.CreateLabel("Category:");
            ControlFactory.CreateLabel("Start Date:");
            ControlFactory.CreateLabel("End Date:");
            ControlFactory.CreateLabel("Notes:");

            // Set placeholder text
            SetPlaceholderText(_descriptionTextBox, "Enter event description");
            SetPlaceholderText(_notesTextBox, "Additional notes (optional)");
        }

        private void SetupEventHandlers()
        {
            // Event handlers are set up in CreateControls
        }

        #region Control Creation Helpers

        private ComboBox CreateComboBox(string placeholder, int x, int y, int width)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(GetDpiAwareX(x), GetDpiAwareY(y)),
                Size = GetDpiAwareSize(new Size(width, 35)),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Apply Material theming
            comboBox.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            comboBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            _mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        private CheckBox CreateCheckBox(string text, int x, int y)
        {
            var checkBox = new CheckBox
            {
                Text = text,
                Location = new Point(GetDpiAwareX(x), GetDpiAwareY(y)),
                AutoSize = true
            };

            // Apply Material theming
            checkBox.BackColor = Color.Transparent;
            checkBox.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            checkBox.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            _mainPanel.Controls.Add(checkBox);
            return checkBox;
        }

        private void SetPlaceholderText(Control? textBox, string placeholder)
        {            if (textBox is TextBox tb)
            {
                tb.Text = placeholder;
                tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);

                tb.GotFocus += (s, e) =>
                {
                    if (tb.Text == placeholder)
                    {
                        tb.Text = "";
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                    }
                };

                tb.LostFocus += (s, e) =>
                {
                    if (string.IsNullOrEmpty(tb.Text))
                    {
                        tb.Text = placeholder;
                        tb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                    }
                };
            }
        }

        #endregion

        #region Data Handling

        private void PopulateFields(SchoolCalendar schoolCalendar)
        {
            try
            {
                if (_descriptionTextBox is TextBox descriptionTb)
                {
                    descriptionTb.Text = schoolCalendar.Description ?? string.Empty;
                    descriptionTb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }

                if (_notesTextBox is TextBox notesTb)
                {
                    notesTb.Text = schoolCalendar.Notes ?? string.Empty;
                    notesTb.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
                }

                if (_categoryComboBox != null && !string.IsNullOrEmpty(schoolCalendar.Category))
                    _categoryComboBox.SelectedItem = schoolCalendar.Category;                if (_routeNeededCheckBox != null)
                    _routeNeededCheckBox.Checked = schoolCalendar.IsRouteNeeded;                if (_startDatePicker != null && schoolCalendar.DateAsDateTime.HasValue)
                    _startDatePicker.Value = schoolCalendar.DateAsDateTime.Value;

                if (_endDatePicker != null && schoolCalendar.EndDateAsDateTime.HasValue)
                    _endDatePicker.Value = schoolCalendar.EndDateAsDateTime.Value;

                Console.WriteLine($"📋 SYNCFUSION FORM: Populated fields for calendar event: {schoolCalendar.Description}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error populating fields: {ex.Message}");
                MessageBox.Show($"Error loading calendar event data: {ex.Message}", "Data Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Event Handlers

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // Create or update school calendar object
                if (SchoolCalendar == null)
                    SchoolCalendar = new SchoolCalendar();

                // Get values from controls
                if (_descriptionTextBox is TextBox descriptionTb)
                    SchoolCalendar.Description = descriptionTb.Text.Trim();

                if (_notesTextBox is TextBox notesTb)
                    SchoolCalendar.Notes = notesTb.Text.Trim();

                if (_categoryComboBox?.SelectedItem != null)
                    SchoolCalendar.Category = _categoryComboBox.SelectedItem.ToString();                if (_routeNeededCheckBox != null)
                    SchoolCalendar.IsRouteNeeded = _routeNeededCheckBox.Checked;                if (_startDatePicker != null)
                    SchoolCalendar.DateAsDateTime = _startDatePicker.Value;

                if (_endDatePicker != null)
                    SchoolCalendar.EndDateAsDateTime = _endDatePicker.Value;

                DialogResult = DialogResult.OK;
                Close();

                Console.WriteLine($"💾 SYNCFUSION FORM: Saved school calendar event: {SchoolCalendar.Description}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SYNCFUSION FORM: Error saving: {ex.Message}");
                MessageBox.Show($"Error saving calendar event: {ex.Message}", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Validation

        private bool ValidateInput()
        {
            ClearAllValidationErrors();

            // Validate description
            if (_descriptionTextBox is TextBox descriptionTb &&
                (string.IsNullOrWhiteSpace(descriptionTb.Text) ||
                 descriptionTb.Text == "Enter event description"))
            {
                SetValidationError(_descriptionTextBox, "Description is required.");
                ShowErrorMessage("Description is required.");
                return false;
            }

            // Validate category
            if (_categoryComboBox?.SelectedIndex < 0)
            {
                SetValidationError(_categoryComboBox, "Please select a category.");
                ShowErrorMessage("Please select a category.");
                return false;
            }

            // Validate date range
            if (_startDatePicker != null && _endDatePicker != null &&
                _startDatePicker.Value > _endDatePicker.Value)
            {
                SetValidationError(_endDatePicker, "End date must be after start date.");
                ShowErrorMessage("End date must be after start date.");
                return false;
            }

            return true;
        }        // Helper methods for validation and form management
        protected override void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        protected override void SetValidationError(Control control, string message)
        {
            if (control != null)
                _errorProvider.SetError(control, message);
        }

        protected new void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Console.WriteLine($"🔄 SYNCFUSION FORM: {this.Text} closing with result: {this.DialogResult}");
            base.OnFormClosing(e);
        }
    }
}
