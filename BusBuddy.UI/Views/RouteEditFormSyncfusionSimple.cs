using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Route Edit Form - Simplified Syncfusion Migration
    /// Form for creating and editing route information matching the actual Route model
    /// </summary>
    public partial class RouteEditFormSyncfusionSimple : SyncfusionBaseForm
    {
        private DateTimePicker? _datePicker;
        private Control? _routeNameTextBox;
        private Control? _notesTextBox;
        private Control? _saveButton;
        private Control? _cancelButton;

        // Labels
        private Label? _dateLabel;
        private Label? _routeNameLabel;
        private Label? _notesLabel;

        // Layout panels
        private new Panel _mainPanel;
        private TableLayoutPanel _formLayout;
        private new Panel _buttonPanel;

        public Route? Route { get; private set; }

        public RouteEditFormSyncfusionSimple() : this(null)
        {
        }

        public RouteEditFormSyncfusionSimple(Route? route)
        {
            Route = route;
            InitializeComponent();
            SetupForm();
            PopulateFields();
        }

        private void InitializeComponent()
        {
            this.Text = Route == null ? "Add Route" : "Edit Route";
            this.Size = new Size(400, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            CreateControls();
            LayoutControls();
        }

        private void CreateControls()
        {
            // Main panel
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SyncfusionThemeHelper.MaterialColors.Background,
                Padding = new Padding(20)
            };

            // Form layout
            _formLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = SyncfusionThemeHelper.MaterialColors.Background
            };

            // Configure form layout
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            _formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 3; i++)
            {
                _formLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            }

            // Labels
            _dateLabel = SyncfusionThemeHelper.CreateStyledLabel("Date:");
            _routeNameLabel = SyncfusionThemeHelper.CreateStyledLabel("Route Name:");
            _notesLabel = SyncfusionThemeHelper.CreateStyledLabel("Notes:");

            // Controls
            _datePicker = new DateTimePicker
            {
                Font = SyncfusionThemeHelper.MaterialTheme.DefaultFont,
                Height = SyncfusionThemeHelper.MaterialTheme.DefaultControlHeight,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            _routeNameTextBox = SyncfusionThemeHelper.CreateStyledTextBox("Enter route name");

            // Notes text box (multiline)
            _notesTextBox = SyncfusionThemeHelper.CreateStyledTextBox("Enter any notes about this route");
            if (_notesTextBox is TextBox notesTextBox)
            {
                notesTextBox.Multiline = true;
                notesTextBox.Height = 80;
                notesTextBox.ScrollBars = ScrollBars.Vertical;
            }

            // Button panel
            _buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = SyncfusionThemeHelper.MaterialColors.Background,
                Padding = new Padding(20, 10, 20, 10)
            };

            // Buttons
            _saveButton = SyncfusionThemeHelper.CreateStyledButton("Save");
            _cancelButton = SyncfusionThemeHelper.CreateStyledButton("Cancel");

            // Configure buttons
            _saveButton.Size = new Size(100, 35);
            _saveButton.Anchor = AnchorStyles.Right;
            _saveButton.Location = new Point(_buttonPanel.Width - 220, 12);
            _saveButton.Click += SaveButton_Click;

            _cancelButton.Size = new Size(100, 35);
            _cancelButton.Anchor = AnchorStyles.Right;
            _cancelButton.Location = new Point(_buttonPanel.Width - 110, 12);
            _cancelButton.Click += CancelButton_Click;

            // Style cancel button differently
            if (_cancelButton is Button cancelBtn)
            {
                cancelBtn.BackColor = SyncfusionThemeHelper.MaterialColors.Border;
                cancelBtn.ForeColor = SyncfusionThemeHelper.MaterialColors.Text;
            }
        }

        private void LayoutControls()
        {
            // Add controls to form layout
            _formLayout.Controls.Add(_dateLabel, 0, 0);
            _formLayout.Controls.Add(_datePicker, 1, 0);

            _formLayout.Controls.Add(_routeNameLabel, 0, 1);
            _formLayout.Controls.Add(_routeNameTextBox, 1, 1);

            _formLayout.Controls.Add(_notesLabel, 0, 2);
            _formLayout.Controls.Add(_notesTextBox, 1, 2);

            // Add buttons to button panel
            _buttonPanel.Controls.Add(_saveButton);
            _buttonPanel.Controls.Add(_cancelButton);

            // Add panels to main panel
            _mainPanel.Controls.Add(_formLayout);
            _mainPanel.Controls.Add(_buttonPanel);

            // Add main panel to form
            this.Controls.Add(_mainPanel);

            // Apply theming
            SyncfusionThemeHelper.ApplyThemeToControl(_mainPanel);
        }

        private void SetupForm()
        {
            // Apply Syncfusion theming
            SyncfusionThemeHelper.ApplyThemeToControl(this);
        }

        private void PopulateFields()
        {
            if (Route == null) return;

            try
            {
                _datePicker.Value = Route.DateAsDateTime;

                if (_routeNameTextBox is TextBox routeNameTb)
                    routeNameTb.Text = Route.RouteName ?? string.Empty;

                if (_notesTextBox is TextBox notesTb)
                    notesTb.Text = Route.Notes ?? string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating route data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var route = GetRouteFromForm();

                // TODO: Save route using business service
                MessageBox.Show($"Route {route.RouteName} saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving route: {ex.Message}", "Error",
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

            var routeName = (_routeNameTextBox as TextBox)?.Text?.Trim();

            if (string.IsNullOrEmpty(routeName))
                errors.Add("Route name is required");

            if (errors.Count > 0)
            {
                MessageBox.Show($"Please correct the following errors:\n\n{string.Join("\n", errors)}",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private Route GetRouteFromForm()
        {
            var route = new Route
            {
                RouteID = Route?.RouteID ?? 0,
                DateAsDateTime = _datePicker.Value,
                RouteName = (_routeNameTextBox as TextBox)?.Text?.Trim(),
                Notes = (_notesTextBox as TextBox)?.Text?.Trim()
            };

            return route;
        }
    }
}
