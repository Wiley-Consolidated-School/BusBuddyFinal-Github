using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.UI.Extensions;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Standardized base form with DPI-aware Material Design styling
    /// All forms should inherit from this to ensure consistent appearance and behavior
    /// </summary>
    public class StandardMaterialForm : MaterialForm
    {
        protected bool _isInitialized = false;

        public StandardMaterialForm()
        {
            InitializeStandardStyling();
        }

        /// <summary>
        /// Initialize standard DPI-aware Material Design styling
        /// </summary>
        protected virtual void InitializeStandardStyling()
        {
            if (_isInitialized) return;

            try
            {
                // Configure standard form properties first
                ConfigureStandardFormProperties();

                // Apply Material Design theme safely
                MaterialDesignThemeManager.ApplyDarkTheme(this);

                // Apply standard control styling (defer DPI spacing until form is loaded)
                ApplyStandardControlStyling();

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in InitializeStandardStyling: {ex.Message}");
                // Continue without styling rather than hanging
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Configure standard form properties for consistency
        /// </summary>
        protected virtual void ConfigureStandardFormProperties()
        {
            // Configure auto-scaling for DPI awareness
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // Set standard Material Design colors
            this.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;
            this.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;

            // Set DPI-aware font
            this.Font = DpiScaleHelper.CreateFont(
                SystemFonts.DefaultFont.FontFamily,
                8.25f, // Base font size
                FontStyle.Regular,
                this
            );

            // Set standard form sizing
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            this.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(600, scaleFactor), // Minimum width
                DpiScaleHelper.ScaleSize(400, scaleFactor)  // Minimum height
            );

            // Standard form behavior
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
        }

        /// <summary>
        /// Apply standard control styling to all controls
        /// </summary>
        protected virtual void ApplyStandardControlStyling()
        {
            // Apply DPI-aware spacing to all controls recursively
            this.ApplyDpiAwareSpacingToAll();
        }

        /// <summary>
        /// Override to ensure styling is applied after controls are added
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Reapply styling after all controls are loaded
            ApplyStandardControlStyling();
        }

        /// <summary>
        /// Override to handle DPI changes at runtime
        /// </summary>
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            // Reapply styling when DPI changes
            ApplyStandardControlStyling();
        }

        /// <summary>
        /// Create a standardized button panel for form actions
        /// </summary>
        /// <param name="buttons">Array of buttons to add</param>
        /// <param name="alignment">Button alignment</param>
        /// <returns>Configured button panel</returns>
        protected Panel CreateStandardButtonPanel(MaterialButton[] buttons, ContentAlignment alignment = ContentAlignment.MiddleRight)
        {
            return DpiAwareLayoutHelper.CreateButtonRow(this, buttons, alignment);
        }

        /// <summary>
        /// Create a standardized section with title and content
        /// </summary>
        /// <param name="title">Section title</param>
        /// <param name="content">Section content</param>
        /// <returns>Configured section panel</returns>
        protected Panel CreateStandardSection(string title, Control content)
        {
            return DpiAwareLayoutHelper.CreateFormSection(this, title, content);
        }

        /// <summary>
        /// Create a standardized input panel with label
        /// </summary>
        /// <param name="labelText">Label text</param>
        /// <param name="inputControl">Input control</param>
        /// <param name="isRequired">Whether the field is required</param>
        /// <returns>Configured input panel</returns>
        protected Panel CreateStandardInput(string labelText, Control inputControl, bool isRequired = false)
        {
            return DpiAwareLayoutHelper.CreateLabeledInput(this, labelText, inputControl, isRequired);
        }

        /// <summary>
        /// Create a standardized card container
        /// </summary>
        /// <param name="title">Optional card title</param>
        /// <param name="content">Card content</param>
        /// <returns>Configured MaterialCard</returns>
        protected MaterialCard CreateStandardCard(string? title, Control content)
        {
            return DpiAwareLayoutHelper.CreateMaterialCard(this, title, content);
        }

        /// <summary>
        /// Create a standardized responsive layout
        /// </summary>
        /// <param name="columns">Number of columns</param>
        /// <param name="rows">Number of rows</param>
        /// <returns>Configured responsive layout</returns>
        protected TableLayoutPanel CreateStandardLayout(int columns, int rows)
        {
            return DpiAwareLayoutHelper.CreateResponsiveGrid(this, columns, rows, true);
        }

        /// <summary>
        /// Apply standard button styling with vector icon
        /// </summary>
        /// <param name="button">Button to style</param>
        /// <param name="text">Button text</param>
        /// <param name="iconSvg">SVG icon content</param>
        /// <param name="buttonType">Material button type</param>
        protected void ConfigureStandardButton(MaterialButton button, string text, string? iconSvg = null,
            MaterialButton.MaterialButtonType buttonType = MaterialButton.MaterialButtonType.Contained)
        {
            button.Text = text;
            button.Type = buttonType;
            button.AutoSize = false;

            // Set DPI-aware size
            button.Size = new Size(
                DpiScaleHelper.ScaleSize(120, this),
                DpiAwareLayoutHelper.TouchTargets.GetComfortable(this)
            );

            // Add icon if provided
            if (!string.IsNullOrEmpty(iconSvg))
            {
                button.SetVectorIcon(iconSvg, 16);
            }

            // Apply DPI-aware spacing
            button.ApplyDpiAwareSpacing();
        }

        /// <summary>
        /// Apply standard text box styling
        /// </summary>
        /// <param name="textBox">TextBox to style</param>
        /// <param name="hint">Hint text</param>
        protected void ConfigureStandardTextBox(MaterialTextBox textBox, string? hint = null)
        {
            if (!string.IsNullOrEmpty(hint))
            {
                textBox.Hint = hint;
            }

            // Set minimum height for better usability
            textBox.Height = Math.Max(textBox.Height, DpiAwareLayoutHelper.TouchTargets.GetMinimum(this));

            // Apply DPI-aware spacing
            textBox.ApplyDpiAwareSpacing();
        }

        /// <summary>
        /// Apply standard combo box styling
        /// </summary>
        /// <param name="comboBox">ComboBox to style</param>
        protected void ConfigureStandardComboBox(MaterialComboBox comboBox)
        {
            // Set minimum height for better usability
            comboBox.Height = Math.Max(comboBox.Height, DpiAwareLayoutHelper.TouchTargets.GetMinimum(this));

            // Apply DPI-aware spacing
            comboBox.ApplyDpiAwareSpacing();
        }

        /// <summary>
        /// Apply standard checkbox styling
        /// </summary>
        /// <param name="checkbox">Checkbox to style</param>
        protected void ConfigureStandardCheckbox(MaterialCheckbox checkbox)
        {
            // Ensure adequate touch target size
            checkbox.MinimumSize = new Size(
                DpiAwareLayoutHelper.TouchTargets.GetMinimum(this),
                DpiAwareLayoutHelper.TouchTargets.GetMinimum(this)
            );

            // Apply DPI-aware spacing
            checkbox.ApplyDpiAwareSpacing();
        }

        /// <summary>
        /// Apply standard label styling
        /// </summary>
        /// <param name="label">Label to style</param>
        /// <param name="fontSize">Font size (default: 9f)</param>
        /// <param name="fontStyle">Font style (default: Regular)</param>
        protected void ConfigureStandardLabel(MaterialLabel label, float fontSize = 9f, FontStyle fontStyle = FontStyle.Regular)
        {
            label.Font = DpiScaleHelper.CreateFont(
                SystemFonts.DefaultFont.FontFamily,
                fontSize,
                fontStyle,
                this
            );

            label.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            label.AutoSize = true;
        }

        /// <summary>
        /// Apply standard DataGridView styling
        /// </summary>
        /// <param name="dataGridView">DataGridView to style</param>
        protected void ConfigureStandardDataGridView(DataGridView dataGridView)
        {
            // Set Material Design colors
            dataGridView.BackgroundColor = MaterialDesignThemeManager.DarkTheme.Surface;
            dataGridView.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            dataGridView.GridColor = MaterialDesignThemeManager.DarkTheme.Outline;

            // Set DPI-aware row height
            dataGridView.RowTemplate.Height = DpiAwareLayoutHelper.TouchTargets.GetMinimum(this);

            // Set DPI-aware font
            dataGridView.Font = DpiScaleHelper.CreateFont(
                SystemFonts.DefaultFont.FontFamily,
                9f,
                FontStyle.Regular,
                this
            );

            // Standard styling
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView.EnableHeadersVisualStyles = false;

            // Header styling
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = DpiScaleHelper.CreateFont(
                SystemFonts.DefaultFont.FontFamily,
                9f,
                FontStyle.Bold,
                this
            );

            // Row styling
            dataGridView.DefaultCellStyle.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;
            dataGridView.DefaultCellStyle.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            dataGridView.DefaultCellStyle.SelectionBackColor = MaterialDesignThemeManager.DarkTheme.Primary;
            dataGridView.DefaultCellStyle.SelectionForeColor = MaterialDesignThemeManager.DarkTheme.OnPrimary;

            // Apply DPI-aware spacing
            dataGridView.Margin = DpiScaleHelper.CreatePadding(8, this);
        }
    }
}
