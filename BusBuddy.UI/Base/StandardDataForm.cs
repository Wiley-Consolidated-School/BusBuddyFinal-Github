using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using BusBuddy.Business;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Base class for all data forms providing standardized Material Design UI
    /// </summary>
    public class StandardDataForm : MaterialForm
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly DatabaseHelperService _databaseService;
        protected MaterialSkinManager _materialSkinManager;

        // Common UI elements
        protected Panel _mainPanel;
        protected Panel _buttonPanel;

        public StandardDataForm()
        {
            // Initialize services
            _errorProvider = new ErrorProvider();
            _databaseService = new DatabaseHelperService();

            // Initialize Material Design
            InitializeMaterialDesign();

            // Set common form properties
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Size = new Size(800, 600);
            this.BackColor = AppTheme.BackgroundWhite;

            // Initialize layout
            InitializeLayout();
        }

        private void InitializeMaterialDesign()
        {
            _materialSkinManager = MaterialSkinManager.Instance;
            _materialSkinManager.AddFormToManage(this);
            _materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            _materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600, Primary.Blue700,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.WHITE);
        }

        protected virtual void InitializeLayout()
        {
            // Create main panel
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent
            };
            this.Controls.Add(_mainPanel);

            // Create button panel
            _buttonPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent,
                Padding = new Padding(20, 10, 20, 10)
            };
            this.Controls.Add(_buttonPanel);
        }

        protected MaterialLabel CreateLabel(string text, int x, int y)
        {
            MaterialLabel label = new MaterialLabel
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            _mainPanel.Controls.Add(label);
            return label;
        }

        protected MaterialTextBox CreateTextBox(int x, int y, int width = 200)
        {
            MaterialTextBox textBox = new MaterialTextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.OUT
            };
            _mainPanel.Controls.Add(textBox);
            return textBox;
        }

        protected MaterialTextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            MaterialTextBox textBox = new MaterialTextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.OUT
            };
            parent.Controls.Add(textBox);
            return textBox;
        }

        protected MaterialComboBox CreateComboBox(string hint, int x, int y, int width = 200)
        {
            MaterialComboBox comboBox = new MaterialComboBox
            {
                Hint = hint,
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.OUT
            };
            _mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        protected MaterialComboBox CreateComboBox(int x, int y, int width = 200)
        {
            MaterialComboBox comboBox = new MaterialComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.OUT
            };
            _mainPanel.Controls.Add(comboBox);
            return comboBox;
        }

        protected DateTimePicker CreateDatePicker(int x, int y, int width = 200)
        {
            DateTimePicker picker = new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Roboto", 10)
            };
            _mainPanel.Controls.Add(picker);
            return picker;
        }

        protected NumericUpDown CreateNumericUpDown(int x, int y, int width = 100, decimal minimum = 0, decimal maximum = 9999)
        {
            NumericUpDown numericUpDown = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Minimum = minimum,
                Maximum = maximum,
                Font = new Font("Roboto", 10)
            };
            _mainPanel.Controls.Add(numericUpDown);
            return numericUpDown;
        }

        protected MaterialButton CreateButton(string text, int x, int y, EventHandler? clickHandler = null)
        {
            MaterialButton button = new MaterialButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 36),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Font = new Font("Roboto", 10F, FontStyle.Bold),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                UseVisualStyleBackColor = true
            };
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            _mainPanel.Controls.Add(button);
            return button;
        }

        protected MaterialButton CreateButton(string text, int x, int y, EventHandler clickHandler, Control parent)
        {
            MaterialButton button = new MaterialButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(120, 36),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Font = new Font("Roboto", 10F, FontStyle.Bold),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                UseVisualStyleBackColor = true
            };
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            parent.Controls.Add(button);
            return button;
        }

        // Overloaded methods for compatibility with existing forms
        protected MaterialLabel CreateLabel(string text, int x, int y, Control parent)
        {
            MaterialLabel label = new MaterialLabel
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            parent.Controls.Add(label);
            return label;
        }

        protected MaterialComboBox CreateComboBox(int x, int y, int width, Control parent)
        {
            MaterialComboBox comboBox = new MaterialComboBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 50),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.OUT
            };
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        protected MaterialButton CreateButton(string text, int x, int y, int width, Control parent)
        {
            MaterialButton button = new MaterialButton
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 36),
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER,
                UseAccentColor = false,
                UseVisualStyleBackColor = true
            };
            parent.Controls.Add(button);
            return button;
        }

        protected void SetValidationError(Control control, string message)
        {
            _errorProvider.SetError(control, message);
        }

        protected void ClearValidationError(Control control)
        {
            _errorProvider.SetError(control, string.Empty);
        }

        protected void ClearAllValidationErrors()
        {
            _errorProvider.Clear();
        }

        protected virtual bool ValidateForm()
        {
            ClearAllValidationErrors();
            return true;
        }

        protected virtual void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected virtual void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected virtual void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        protected virtual bool ConfirmDelete(string itemName = "item")
        {
            var result = MessageBox.Show($"Are you sure you want to delete this {itemName}?",
                                       "Confirm Delete",
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        protected virtual bool ConfirmAction(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        // Additional helper methods for forms that might need them
        protected MaterialLabel CreateMaterialLabel(string text, int x, int y)
        {
            MaterialLabel label = new MaterialLabel
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            _mainPanel.Controls.Add(label);
            return label;
        }

        protected MaterialLabel CreateMaterialLabel(string text)
        {
            MaterialLabel label = new MaterialLabel
            {
                Text = text,
                AutoSize = true,
                Depth = 0,
                MouseState = MaterialSkin.MouseState.HOVER
            };
            return label;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle common keyboard shortcuts
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            base.OnKeyDown(e);
        }        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _errorProvider?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
