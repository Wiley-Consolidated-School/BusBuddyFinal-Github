using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.UI.Helpers;

namespace BusBuddy.UI.Extensions
{
    /// <summary>
    /// Extension methods for MaterialSkin controls with vector graphics and DPI awareness support
    /// Provides both icon functionality and comprehensive DPI scaling
    /// </summary>
    public static class MaterialControlExtensions
    {
        /// <summary>
        /// Set a vector icon on a MaterialButton
        /// </summary>
        public static void SetVectorIcon(this MaterialButton button, string svgContent, int iconSize = 16, Color? iconColor = null)
        {
            try
            {
                // Use button's foreground color if no color specified
                var color = iconColor ?? button.ForeColor;
                if (color == Color.Empty || color == Color.Transparent)
                {
                    color = Color.Gray; // Fallback color
                }

                var icon = VectorGraphicsHelper.CreateDpiAwareIconFromSvg(svgContent, iconSize, button, color);
                button.Icon = icon;
                button.AutoSize = false;

                // Ensure proper button sizing for icon with DPI awareness
                var scaleFactor = DpiScaleHelper.GetControlScaleFactor(button);
                int minWidth = DpiScaleHelper.ScaleSize(iconSize + 40, scaleFactor);
                if (button.Size.Width < minWidth)
                {
                    button.Size = new Size(Math.Max(button.Size.Width, minWidth), button.Size.Height);
                }
            }
            catch (Exception)
            {
                // Silently fail - button will work without icon
            }
        }

        /// <summary>
        /// Set a vector icon on a MaterialFloatingActionButton
        /// </summary>
        public static void SetVectorIcon(this MaterialFloatingActionButton fab, string svgContent, int iconSize = 24, Color? iconColor = null)
        {
            try
            {
                // Use appropriate color for FAB
                var color = iconColor ?? Color.White;
                var icon = VectorGraphicsHelper.CreateDpiAwareIconFromSvg(svgContent, iconSize, fab, color);
                fab.Icon = icon;
            }
            catch (Exception)
            {
                // Silently fail - FAB will work without icon
            }
        }

        /// <summary>
        /// Apply DPI-aware spacing and padding to MaterialSkin controls
        /// </summary>
        /// <param name="control">Control to configure</param>
        /// <param name="marginSize">Base margin size (default: 8)</param>
        /// <param name="paddingSize">Base padding size (default: 4)</param>
        public static void ApplyDpiAwareSpacing(this Control control, int marginSize = 8, int paddingSize = 4)
        {
            if (control == null) return;

            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(control);

            // Apply scaled margin and padding
            control.Margin = DpiScaleHelper.CreatePadding(marginSize, control);
            control.Padding = DpiScaleHelper.CreatePadding(paddingSize, control);

            // Apply specific adjustments for MaterialSkin controls
            switch (control)
            {
                case MaterialButton button:
                    ConfigureMaterialButton(button, scaleFactor);
                    break;
                case MaterialTextBox textBox:
                    ConfigureMaterialTextBox(textBox, scaleFactor);
                    break;
                case MaterialComboBox comboBox:
                    ConfigureMaterialComboBox(comboBox, scaleFactor);
                    break;
                case MaterialCheckbox checkbox:
                    ConfigureMaterialCheckbox(checkbox, scaleFactor);
                    break;
                case MaterialRadioButton radioButton:
                    ConfigureMaterialRadioButton(radioButton, scaleFactor);
                    break;
                case MaterialCard card:
                    ConfigureMaterialCard(card, scaleFactor);
                    break;
                case MaterialTabControl tabControl:
                    ConfigureMaterialTabControl(tabControl, scaleFactor);
                    break;
            }
        }

        /// <summary>
        /// Configure MaterialButton for DPI awareness
        /// </summary>
        private static void ConfigureMaterialButton(MaterialButton button, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Increase button height for better touch targets
            int baseHeight = 36; // Material Design standard button height
            button.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(64, scaleFactor), // Minimum width
                DpiScaleHelper.ScaleSize(baseHeight, scaleFactor)
            );

            // Set appropriate font size
            if (button.Font != null)
            {
                float scaledFontSize = DpiScaleHelper.ScaleFontSize(10f, scaleFactor);
                button.Font = new Font(button.Font.FontFamily, scaledFontSize, button.Font.Style);
            }
        }

        /// <summary>
        /// Configure MaterialTextBox for DPI awareness
        /// </summary>
        private static void ConfigureMaterialTextBox(MaterialTextBox textBox, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set minimum height for better usability
            int baseHeight = 32;
            textBox.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(120, scaleFactor),
                DpiScaleHelper.ScaleSize(baseHeight, scaleFactor)
            );

            // Adjust font size
            if (textBox.Font != null)
            {
                float scaledFontSize = DpiScaleHelper.ScaleFontSize(9f, scaleFactor);
                textBox.Font = new Font(textBox.Font.FontFamily, scaledFontSize, textBox.Font.Style);
            }
        }

        /// <summary>
        /// Configure MaterialComboBox for DPI awareness
        /// </summary>
        private static void ConfigureMaterialComboBox(MaterialComboBox comboBox, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set minimum size for better usability
            int baseHeight = 32;
            comboBox.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(120, scaleFactor),
                DpiScaleHelper.ScaleSize(baseHeight, scaleFactor)
            );
        }

        /// <summary>
        /// Configure MaterialCheckbox for DPI awareness
        /// </summary>
        private static void ConfigureMaterialCheckbox(MaterialCheckbox checkbox, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Ensure adequate touch target size
            int baseTouchTarget = 24;
            checkbox.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(baseTouchTarget, scaleFactor),
                DpiScaleHelper.ScaleSize(baseTouchTarget, scaleFactor)
            );

            // Adjust font size for label
            if (checkbox.Font != null)
            {
                float scaledFontSize = DpiScaleHelper.ScaleFontSize(9f, scaleFactor);
                checkbox.Font = new Font(checkbox.Font.FontFamily, scaledFontSize, checkbox.Font.Style);
            }
        }

        /// <summary>
        /// Configure MaterialRadioButton for DPI awareness
        /// </summary>
        private static void ConfigureMaterialRadioButton(MaterialRadioButton radioButton, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Ensure adequate touch target size
            int baseTouchTarget = 24;
            radioButton.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(baseTouchTarget, scaleFactor),
                DpiScaleHelper.ScaleSize(baseTouchTarget, scaleFactor)
            );

            // Adjust font size for label
            if (radioButton.Font != null)
            {
                float scaledFontSize = DpiScaleHelper.ScaleFontSize(9f, scaleFactor);
                radioButton.Font = new Font(radioButton.Font.FontFamily, scaledFontSize, radioButton.Font.Style);
            }
        }

        /// <summary>
        /// Configure MaterialCard for DPI awareness
        /// </summary>
        private static void ConfigureMaterialCard(MaterialCard card, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Apply scaled padding for better content spacing
            card.Padding = DpiScaleHelper.CreatePadding(16, 12, card);
        }

        /// <summary>
        /// Configure MaterialTabControl for DPI awareness
        /// </summary>
        private static void ConfigureMaterialTabControl(MaterialTabControl tabControl, DpiScaleHelper.ScaleFactor scaleFactor)
        {
            // Set minimum tab height for better touch targets
            int baseTabHeight = 48;
            tabControl.ItemSize = new Size(
                tabControl.ItemSize.Width, // Keep existing width
                DpiScaleHelper.ScaleSize(baseTabHeight, scaleFactor)
            );
        }

        /// <summary>
        /// Apply DPI-aware scaling to all MaterialSkin controls in a container recursively
        /// </summary>
        /// <param name="container">Container to process</param>
        /// <param name="marginSize">Base margin size</param>
        /// <param name="paddingSize">Base padding size</param>
        public static void ApplyDpiAwareSpacingToAll(this Control container, int marginSize = 8, int paddingSize = 4)
        {
            if (container == null) return;

            // Add protection against infinite recursion
            if (container.Tag?.ToString() == "DPI_PROCESSED") return;

            try
            {
                // Mark container as processed to prevent infinite recursion
                var originalTag = container.Tag;
                container.Tag = "DPI_PROCESSED";

                // Apply to the container itself if it's a MaterialSkin control
                if (IsMaterialSkinControl(container))
                {
                    container.ApplyDpiAwareSpacing(marginSize, paddingSize);
                }

                // Recursively apply to all child controls
                foreach (Control child in container.Controls)
                {
                    child.ApplyDpiAwareSpacingToAll(marginSize, paddingSize);
                }

                // Restore original tag if it wasn't null
                if (originalTag != null && originalTag.ToString() != "DPI_PROCESSED")
                {
                    container.Tag = originalTag;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ApplyDpiAwareSpacingToAll: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if control is a MaterialSkin control
        /// </summary>
        private static bool IsMaterialSkinControl(Control control)
        {
            return control is MaterialButton ||
                   control is MaterialTextBox ||
                   control is MaterialComboBox ||
                   control is MaterialCheckbox ||
                   control is MaterialRadioButton ||
                   control is MaterialCard ||
                   control is MaterialTabControl ||
                   control is MaterialLabel ||
                   control is MaterialListView ||
                   control.GetType().Name.StartsWith("Material");
        }

        /// <summary>
        /// Configure form AutoScaleMode for optimal DPI handling
        /// </summary>
        /// <param name="form">Form to configure</param>
        /// <param name="scaleMode">AutoScale mode (default: Dpi)</param>
        public static void ConfigureAutoScaling(this Form form, AutoScaleMode scaleMode = AutoScaleMode.Dpi)
        {
            if (form == null) return;

            form.AutoScaleMode = scaleMode;

            // Set base font size for scaling reference
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(form);
            float baseFontSize = 8.25f; // Standard Windows Forms base font size

            form.Font = DpiScaleHelper.CreateFont(
                form.Font?.FontFamily ?? SystemFonts.DefaultFont.FontFamily,
                baseFontSize,
                form.Font?.Style ?? FontStyle.Regular,
                form
            );
        }

        /// <summary>
        /// Apply responsive layout using adaptive containers
        /// </summary>
        /// <param name="container">Container to make responsive</param>
        /// <param name="columns">Number of columns for TableLayoutPanel</param>
        /// <param name="rows">Number of rows for TableLayoutPanel</param>
        /// <returns>Configured TableLayoutPanel</returns>
        public static TableLayoutPanel MakeResponsive(this Control container, int columns = 2, int rows = 2)
        {
            var layoutPanel = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = columns,
                RowCount = rows,
                Dock = DockStyle.Fill,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Configure column styles for proportional sizing
            for (int i = 0; i < columns; i++)
            {
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            // Configure row styles for proportional sizing
            for (int i = 0; i < rows; i++)
            {
                layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            }

            // Apply DPI-aware spacing
            layoutPanel.ApplyDpiAwareSpacing();

            // Move existing controls to the layout panel
            var controlsToMove = new Control[container.Controls.Count];
            container.Controls.CopyTo(controlsToMove, 0);

            container.Controls.Clear();
            container.Controls.Add(layoutPanel);

            // Add controls to layout panel
            for (int i = 0; i < controlsToMove.Length && i < columns * rows; i++)
            {
                int col = i % columns;
                int row = i / columns;
                layoutPanel.Controls.Add(controlsToMove[i], col, row);

                // Configure control anchoring for responsive behavior
                controlsToMove[i].Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            return layoutPanel;
        }
    }

    /// <summary>
    /// Enhanced MaterialButton with built-in vector icon support
    /// </summary>
    public class VectorMaterialButton : MaterialButton
    {
        private string _svgIcon;
        private int _iconSize = 16;
        private Color? _iconColor;

        public string SvgIcon
        {
            get => _svgIcon;
            set
            {
                _svgIcon = value;
                UpdateVectorIcon();
            }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                UpdateVectorIcon();
            }
        }

        public Color? IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                UpdateVectorIcon();
            }
        }

        private void UpdateVectorIcon()
        {
            if (!string.IsNullOrEmpty(_svgIcon))
            {
                this.SetVectorIcon(_svgIcon, _iconSize, _iconColor);
            }
        }
    }

    /// <summary>
    /// Enhanced MaterialFloatingActionButton with built-in vector icon support
    /// </summary>
    public class VectorMaterialFAB : MaterialFloatingActionButton
    {
        private string _svgIcon;
        private int _iconSize = 24;
        private Color? _iconColor;

        public string SvgIcon
        {
            get => _svgIcon;
            set
            {
                _svgIcon = value;
                UpdateVectorIcon();
            }
        }

        public int IconSize
        {
            get => _iconSize;
            set
            {
                _iconSize = value;
                UpdateVectorIcon();
            }
        }

        public Color? IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
                UpdateVectorIcon();
            }
        }

        private void UpdateVectorIcon()
        {
            if (!string.IsNullOrEmpty(_svgIcon))
            {
                this.SetVectorIcon(_svgIcon, _iconSize, _iconColor);
            }
        }
    }
}
