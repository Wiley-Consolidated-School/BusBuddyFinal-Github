using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Components
{
    /// <summary>
    /// Reusable Material Design edit panel with common form fields
    /// Provides consistent UI across all management forms
    /// </summary>
    public partial class MaterialEditPanel : UserControl
    {
        #region Private Fields

        private TableLayoutPanel _mainLayout;
        private MaterialCard _contentCard;
        private TableLayoutPanel _fieldsLayout;
        private Panel _buttonPanel;
        private MaterialButton _saveButton;
        private MaterialButton _cancelButton;
        private Dictionary<string, Control> _fieldControls;
        private bool _isVisible = false;

        #endregion

        #region Events

        /// <summary>
        /// Raised when the save button is clicked
        /// </summary>
        public event EventHandler<EventArgs> SaveClicked;

        /// <summary>
        /// Raised when the cancel button is clicked
        /// </summary>
        public event EventHandler<EventArgs> CancelClicked;

        /// <summary>
        /// Raised when field values change
        /// </summary>
        public event EventHandler<FieldChangedEventArgs> FieldChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the panel title
        /// </summary>
        public string PanelTitle { get; set; } = "Edit Item";

        /// <summary>
        /// Gets or sets whether the panel is in edit mode or add mode
        /// </summary>
        public bool IsEditMode { get; set; } = false;

        /// <summary>
        /// Gets whether the panel is currently visible
        /// </summary>
        public bool IsEditPanelVisible => _isVisible;

        /// <summary>
        /// Gets the collection of field controls
        /// </summary>
        public IReadOnlyDictionary<string, Control> FieldControls => _fieldControls;

        #endregion

        #region Constructor

        public MaterialEditPanel()
        {
            _fieldControls = new Dictionary<string, Control>();
            InitializeComponent();
            SetupEventHandlers();
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Configure user control
            this.Size = new Size(800, 200);
            this.BackColor = Color.Transparent;
            this.Visible = false;

            // Create main layout
            CreateMainLayout();

            // Create content card
            CreateContentCard();

            // Create fields layout
            CreateFieldsLayout();

            // Create button panel
            CreateButtonPanel();

            // Apply DPI scaling
            ApplyDpiScaling();

            this.ResumeLayout(false);
        }

        private void CreateMainLayout()
        {
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this.Controls.Add(_mainLayout);
        }

        private void CreateContentCard()
        {
            _contentCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface,
                Margin = new Padding(8)
            };

            _mainLayout.Controls.Add(_contentCard, 0, 0);
        }

        private void CreateFieldsLayout()
        {
            _fieldsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4, // 4 columns for flexible layout
                AutoSize = true,
                BackColor = Color.Transparent
            };

            // Configure column styles for responsive layout
            _fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            _fieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            _contentCard.Controls.Add(_fieldsLayout);
        }

        private void CreateButtonPanel()
        {
            _buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent
            };

            // Create save button
            _saveButton = new MaterialButton
            {
                Text = "Save",
                Size = new Size(100, 36),
                UseAccentColor = true,
                Type = MaterialButton.MaterialButtonType.Contained,
                AutoSize = false,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            // Create cancel button
            _cancelButton = new MaterialButton
            {
                Text = "Cancel",
                Size = new Size(100, 36),
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = false,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };

            // Position buttons
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            var spacing = DpiScaleHelper.ScaleSize(10, scaleFactor);

            _cancelButton.Location = new Point(_buttonPanel.Width - _cancelButton.Width - spacing, 7);
            _saveButton.Location = new Point(_cancelButton.Left - _saveButton.Width - spacing, 7);

            _buttonPanel.Controls.Add(_saveButton);
            _buttonPanel.Controls.Add(_cancelButton);
            _contentCard.Controls.Add(_buttonPanel);
        }

        private void SetupEventHandlers()
        {
            _saveButton.Click += (s, e) => SaveClicked?.Invoke(this, EventArgs.Empty);
            _cancelButton.Click += (s, e) => CancelClicked?.Invoke(this, EventArgs.Empty);

            // Handle resize to reposition buttons
            this.Resize += (s, e) => RepositionButtons();
        }

        private void ApplyDpiScaling()
        {
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);

            // Scale padding and margins
            var padding = DpiScaleHelper.ScaleSize(16, scaleFactor);
            _contentCard.Padding = new Padding(padding);

            var margin = DpiScaleHelper.ScaleSize(8, scaleFactor);
            _contentCard.Margin = new Padding(margin);

            // Scale button panel height
            _buttonPanel.Height = DpiScaleHelper.ScaleSize(50, scaleFactor);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a text field to the panel
        /// </summary>
        public MaterialTextBox AddTextField(string fieldName, string labelText, int row, int column, int columnSpan = 1)
        {
            var label = CreateFieldLabel(labelText);
            var textBox = new MaterialTextBox
            {
                Hint = labelText,
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };

            AddFieldToLayout(fieldName, label, textBox, row, column, columnSpan);
            return textBox;
        }

        /// <summary>
        /// Add a combo box field to the panel
        /// </summary>
        public MaterialComboBox AddComboBoxField(string fieldName, string labelText, int row, int column, int columnSpan = 1)
        {
            var label = CreateFieldLabel(labelText);
            var comboBox = new MaterialComboBox
            {
                Hint = labelText,
                Dock = DockStyle.Fill,
                Margin = new Padding(4)
            };

            AddFieldToLayout(fieldName, label, comboBox, row, column, columnSpan);
            return comboBox;
        }

        /// <summary>
        /// Add a date picker field to the panel
        /// </summary>
        public DateTimePicker AddDateField(string fieldName, string labelText, int row, int column, int columnSpan = 1)
        {
            var label = CreateFieldLabel(labelText);
            var datePicker = new DateTimePicker
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                Format = DateTimePickerFormat.Short
            };

            AddFieldToLayout(fieldName, label, datePicker, row, column, columnSpan);
            return datePicker;
        }

        /// <summary>
        /// Add a numeric field to the panel
        /// </summary>
        public MaterialTextBox AddNumericField(string fieldName, string labelText, int row, int column, int columnSpan = 1)
        {
            var textBox = AddTextField(fieldName, labelText, row, column, columnSpan);

            // Add numeric validation
            textBox.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Only allow one decimal point
                var textBox = s as TextBox;
                if (e.KeyChar == '.' && textBox?.Text?.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
            };

            return textBox;
        }

        /// <summary>
        /// Get the value of a field
        /// </summary>
        public object GetFieldValue(string fieldName)
        {
            if (!_fieldControls.ContainsKey(fieldName))
                return null;

            var control = _fieldControls[fieldName];

            return control switch
            {
                MaterialTextBox textBox => textBox.Text,
                MaterialComboBox comboBox => comboBox.SelectedItem,
                DateTimePicker datePicker => datePicker.Value,
                _ => null
            };
        }

        /// <summary>
        /// Set the value of a field
        /// </summary>
        public void SetFieldValue(string fieldName, object value)
        {
            if (!_fieldControls.ContainsKey(fieldName))
                return;

            var control = _fieldControls[fieldName];

            switch (control)
            {
                case MaterialTextBox textBox:
                    textBox.Text = value?.ToString() ?? string.Empty;
                    break;
                case MaterialComboBox comboBox:
                    comboBox.SelectedItem = value;
                    break;
                case DateTimePicker datePicker when value is DateTime dateValue:
                    datePicker.Value = dateValue;
                    break;
            }
        }

        /// <summary>
        /// Clear all field values
        /// </summary>
        public void ClearFields()
        {
            foreach (var control in _fieldControls.Values)
            {
                switch (control)
                {
                    case MaterialTextBox textBox:
                        textBox.Clear();
                        break;
                    case MaterialComboBox comboBox:
                        comboBox.SelectedIndex = -1;
                        break;
                    case DateTimePicker datePicker:
                        datePicker.Value = DateTime.Now;
                        break;
                }
            }
        }

        /// <summary>
        /// Show the edit panel
        /// </summary>
        public void ShowPanel()
        {
            _isVisible = true;
            this.Visible = true;
            this.BringToFront();
        }

        /// <summary>
        /// Hide the edit panel
        /// </summary>
        public void HidePanel()
        {
            _isVisible = false;
            this.Visible = false;
        }

        /// <summary>
        /// Set the button text
        /// </summary>
        public void SetButtonText(string saveText, string cancelText)
        {
            _saveButton.Text = saveText;
            _cancelButton.Text = cancelText;
        }

        #endregion

        #region Private Methods

        private MaterialLabel CreateFieldLabel(string text)
        {
            return new MaterialLabel
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(4, 8, 4, 2),
                Font = new System.Drawing.Font("Roboto", 9, System.Drawing.FontStyle.Regular)
            };
        }

        private void AddFieldToLayout(string fieldName, Control label, Control field, int row, int column, int columnSpan)
        {
            // Ensure we have enough rows
            while (_fieldsLayout.RowCount <= row + 1)
            {
                _fieldsLayout.RowCount++;
                _fieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Add label
            _fieldsLayout.Controls.Add(label, column, row);

            // Add field
            _fieldsLayout.Controls.Add(field, column, row + 1);

            // Set column span if needed
            if (columnSpan > 1)
            {
                _fieldsLayout.SetColumnSpan(field, columnSpan);
            }

            // Store field reference
            _fieldControls[fieldName] = field;

            // Setup change notification
            SetupFieldChangeNotification(field);
        }

        private void SetupFieldChangeNotification(Control field)
        {
            switch (field)
            {
                case MaterialTextBox textBox:
                    textBox.TextChanged += (s, e) => OnFieldChanged(textBox.Name);
                    break;
                case MaterialComboBox comboBox:
                    comboBox.SelectedIndexChanged += (s, e) => OnFieldChanged(comboBox.Name);
                    break;
                case DateTimePicker datePicker:
                    datePicker.ValueChanged += (s, e) => OnFieldChanged(datePicker.Name);
                    break;
            }
        }

        private void OnFieldChanged(string fieldName)
        {
            FieldChanged?.Invoke(this, new FieldChangedEventArgs(fieldName));
        }

        private void RepositionButtons()
        {
            if (_buttonPanel == null || _saveButton == null || _cancelButton == null)
                return;

            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            var spacing = DpiScaleHelper.ScaleSize(10, scaleFactor);

            _cancelButton.Location = new Point(_buttonPanel.Width - _cancelButton.Width - spacing, 7);
            _saveButton.Location = new Point(_cancelButton.Left - _saveButton.Width - spacing, 7);
        }

        #endregion
    }

    /// <summary>
    /// Event arguments for field change events
    /// </summary>
    public class FieldChangedEventArgs : EventArgs
    {
        public string FieldName { get; }

        public FieldChangedEventArgs(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
