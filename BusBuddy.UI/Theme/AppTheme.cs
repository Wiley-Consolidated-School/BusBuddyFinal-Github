using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Theme
{
    /// <summary>
    /// Centralized theme management for consistent visual styling across the BusBuddy application.
    /// Provides colors, fonts, and styling methods that support accessibility and modern UI design.
    /// </summary>
    public static class AppTheme
    {
        #region Colors - Refined Modern Theme (No Ugly Colors)

        // Primary Colors - Modern Slate Blue (Professional but not boring)
        public static Color PrimaryColor => Color.FromArgb(71, 85, 105);         // Modern slate-700
        public static Color PrimaryColorDark => Color.FromArgb(51, 65, 85);      // Modern slate-800
        public static Color PrimaryColorLight => Color.FromArgb(148, 163, 184);  // Modern slate-400

        // Secondary Colors - Neutral Grays (Never ugly)
        public static Color SecondaryGray => Color.FromArgb(107, 114, 128);      // Modern gray-500
        public static Color SecondaryGrayLight => Color.FromArgb(243, 244, 246); // Modern gray-100
        public static Color SecondaryGrayDark => Color.FromArgb(75, 85, 99);     // Modern gray-600

        // Status Colors - Refined Professional (No garish colors)
        public static Color SuccessGreen => Color.FromArgb(34, 197, 94);         // Modern emerald-500
        public static Color SuccessGreenLight => Color.FromArgb(236, 253, 245);  // Modern emerald-50
        public static Color WarningAmber => Color.FromArgb(245, 158, 11);        // Modern amber-500
        public static Color WarningAmberLight => Color.FromArgb(255, 251, 235);  // Modern amber-50
        public static Color ErrorRed => Color.FromArgb(239, 68, 68);             // Modern red-500
        public static Color ErrorRedLight => Color.FromArgb(254, 242, 242);      // Modern red-50

        // Background Colors - Clean Modern Neutrals
        public static Color BackgroundWhite => Color.FromArgb(255, 255, 255);    // Pure white
        public static Color BackgroundGray => Color.FromArgb(249, 250, 251);     // Modern gray-50
        public static Color BackgroundLight => Color.FromArgb(248, 250, 252);    // Modern slate-50
        public static Color BackgroundCard => Color.FromArgb(255, 255, 255);     // Pure white cards

        // Text Colors - Modern and Readable
        public static Color TextPrimary => Color.FromArgb(15, 23, 42);          // Modern slate-900 (excellent contrast)
        public static Color TextSecondary => Color.FromArgb(100, 116, 139);     // Modern slate-500
        public static Color TextOnPrimary => Color.White;                       // White on dark backgrounds
        public static Color TextOnDark => Color.White;                          // White on dark backgrounds

        // Grid Colors - Refined and Modern
        public static Color GridHeaderBackground => PrimaryColor;               // Slate blue headers
        public static Color GridHeaderText => TextOnPrimary;                    // White text
        public static Color GridAlternateRow => Color.FromArgb(248, 250, 252);  // Very subtle alternating
        public static Color GridBorder => Color.FromArgb(226, 232, 240);        // Modern slate-200

        #endregion

        #region Fonts

        // Base font family
        private static readonly string FontFamily = "Segoe UI";

        // Standard fonts with MUCH more pronounced sizing for visibility
        public static Font LabelFont => new Font(FontFamily, 10F, FontStyle.Regular);      // Increased from 9.5
        public static Font LabelBoldFont => new Font(FontFamily, 10F, FontStyle.Bold);     // Increased from 9.5
        public static Font HeaderFont => new Font(FontFamily, 12F, FontStyle.Bold);        // Increased from 11
        public static Font TitleFont => new Font(FontFamily, 14F, FontStyle.Bold);         // Increased from 12
        public static Font InputFont => new Font(FontFamily, 10F, FontStyle.Regular);      // Increased from 9.5
        public static Font ButtonFont => new Font(FontFamily, 10F, FontStyle.Regular);     // Increased from 9.5
        public static Font ButtonBoldFont => new Font(FontFamily, 10F, FontStyle.Bold);    // Increased from 9.5
        public static Font SummaryFont => new Font(FontFamily, 11F, FontStyle.Bold);       // Increased from 10

        #endregion

        #region Button Styling

        /// <summary>
        /// Applies primary button styling with DPI awareness
        /// </summary>
        public static void ApplyPrimaryButtonStyle(Button button)
        {
            button.BackColor = PrimaryColor;
            button.ForeColor = TextOnPrimary;
            button.Font = ButtonBoldFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.BorderColor = PrimaryColorDark;
            button.Cursor = Cursors.Hand;

            // DPI-aware sizing
            button.Size = ControlSizes.GetButtonSize(button);
            button.Margin = new Padding(Spacing.GetMedium(button));

            // Add hover effects
            button.MouseEnter += (s, e) => button.BackColor = PrimaryColorDark;
            button.MouseLeave += (s, e) => button.BackColor = PrimaryColor;
        }

        /// <summary>
        /// Applies secondary button styling - refined modern look
        /// </summary>
        public static void ApplySecondaryButtonStyle(Button button)
        {
            button.BackColor = SecondaryGrayLight;
            button.ForeColor = TextPrimary;
            button.Font = ButtonFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219); // Modern gray-300
            button.Cursor = Cursors.Hand;

            // Subtle modern hover effects
            button.MouseEnter += (s, e) => {
                button.BackColor = Color.FromArgb(229, 231, 235); // Modern gray-200
                button.FlatAppearance.BorderColor = SecondaryGray;
            };
            button.MouseLeave += (s, e) => {
                button.BackColor = SecondaryGrayLight;
                button.FlatAppearance.BorderColor = Color.FromArgb(209, 213, 219);
            };
        }

        /// <summary>
        /// Applies success button styling - modern refined green
        /// </summary>
        public static void ApplySuccessButtonStyle(Button button)
        {
            button.BackColor = SuccessGreen;
            button.ForeColor = TextOnPrimary;
            button.Font = ButtonBoldFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;

            // Refined hover effects
            var hoverColor = Color.FromArgb(22, 163, 74); // Modern emerald-600
            button.MouseEnter += (s, e) => button.BackColor = hoverColor;
            button.MouseLeave += (s, e) => button.BackColor = SuccessGreen;
        }

        /// <summary>
        /// Applies warning button styling (orange background, white text)
        /// </summary>
        public static void ApplyWarningButtonStyle(Button button)
        {
            button.BackColor = WarningAmber;
            button.ForeColor = TextOnPrimary;
            button.Font = ButtonBoldFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;

            // Add hover effects
            var hoverColor = Color.FromArgb(212, 172, 13); // Darker amber
            button.MouseEnter += (s, e) => button.BackColor = hoverColor;
            button.MouseLeave += (s, e) => button.BackColor = WarningAmber;
        }

        /// <summary>
        /// Applies danger button styling (red background, white text)
        /// </summary>
        public static void ApplyDangerButtonStyle(Button button)
        {
            button.BackColor = ErrorRed;
            button.ForeColor = TextOnPrimary;
            button.Font = ButtonBoldFont;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;

            // Add hover effects
            var hoverColor = Color.FromArgb(183, 28, 28); // Darker red
            button.MouseEnter += (s, e) => button.BackColor = hoverColor;
            button.MouseLeave += (s, e) => button.BackColor = ErrorRed;
        }

        #endregion

        #region Control Styling

        /// <summary>
        /// Applies modern styling to DataGridView with enhanced visual appeal
        /// </summary>
        public static void ApplyDataGridViewStyle(DataGridView grid)
        {
            // Basic styling
            grid.BackgroundColor = BackgroundWhite;
            grid.BorderStyle = BorderStyle.Fixed3D;
            grid.GridColor = GridBorder;
            grid.DefaultCellStyle.Font = InputFont;
            grid.DefaultCellStyle.SelectionBackColor = PrimaryColorLight;
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);

            // Header styling with DPI awareness
            grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackground;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderText;
            grid.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeight = ScaleForDpi(38, grid);
            var headerPadding = Spacing.GetMedium(grid);
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(headerPadding, Spacing.GetSmall(grid), headerPadding, Spacing.GetSmall(grid));

            // Row styling with DPI-aware spacing
            grid.AlternatingRowsDefaultCellStyle.BackColor = GridAlternateRow;
            grid.RowsDefaultCellStyle.BackColor = BackgroundWhite;
            grid.RowTemplate.Height = ScaleForDpi(32, grid);

            // Selection and behavior
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Enhanced visual effects
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;        }

        /// <summary>
        /// Applies refined modern styling to GroupBox controls with DPI awareness
        /// </summary>
        public static void ApplyGroupBoxStyle(GroupBox groupBox)
        {
            groupBox.Font = HeaderFont;                        // 12pt Bold
            groupBox.ForeColor = PrimaryColor;                 // Modern slate blue header text
            groupBox.BackColor = BackgroundLight;              // Very subtle background
            groupBox.FlatStyle = FlatStyle.Flat;

            // DPI-aware padding and spacing
            var largePadding = Spacing.GetLarge(groupBox);
            var mediumPadding = Spacing.GetMedium(groupBox);
            groupBox.Padding = new Padding(largePadding, mediumPadding, largePadding, largePadding);
            groupBox.Margin = new Padding(Spacing.GetMedium(groupBox));

            // Force invalidation to ensure visual changes are applied
            groupBox.Invalidate();
        }

        /// <summary>
        /// Applies refined modern styling to Label controls
        /// <summary>
        /// Applies refined modern styling to Label controls with DPI awareness
        /// </summary>
        public static void ApplyLabelStyle(Label label, bool isBold = false)
        {
            label.Font = isBold ? HeaderFont : LabelBoldFont;   // Bold for visibility, larger for emphasis
            label.ForeColor = PrimaryColorDark;                // Modern slate-800 for excellent contrast
            label.BackColor = Color.Transparent;

            // DPI-aware spacing
            var spacing = Spacing.GetSmall(label);
            label.AutoSize = true;
            label.Margin = new Padding(spacing, spacing + 1, spacing, spacing + 1);

            // Force invalidation
            label.Invalidate();
        }

        /// <summary>
        /// Applies refined modern styling to TextBox controls with DPI awareness
        /// </summary>
        public static void ApplyTextBoxStyle(TextBox textBox, bool isReadOnly = false)
        {
            textBox.Font = InputFont;
            textBox.ForeColor = TextPrimary;
            textBox.BackColor = isReadOnly ? WarningAmberLight : BackgroundWhite;
            textBox.BorderStyle = BorderStyle.FixedSingle;

            // DPI-aware sizing
            textBox.Height = ControlSizes.GetTextBoxHeight(textBox);
            textBox.Margin = new Padding(Spacing.GetMedium(textBox));

            // Add subtle focus effects for professional look
            textBox.Enter += (s, e) => {
                if (!isReadOnly)
                {
                    textBox.BackColor = BackgroundLight;          // Very subtle gray focus
                    textBox.ForeColor = PrimaryColorDark;         // Dark gray-blue text
                }
            };
            textBox.Leave += (s, e) => {
                textBox.BackColor = isReadOnly ? WarningAmberLight : BackgroundWhite;
                textBox.ForeColor = TextPrimary;
            };

            // Force invalidation
            textBox.Invalidate();
        }

        /// <summary>
        /// Applies enhanced styling to Panel controls
        /// </summary>
        public static void ApplyPanelStyle(Panel panel, bool isCard = false)
        {
            panel.BackColor = isCard ? BackgroundCard : BackgroundLight;
            if (isCard)
            {
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.Padding = new Padding(12, 8, 12, 8);
            }
        }

        /// <summary>
        /// Applies styling to ComboBox controls with enhanced visuals
        /// </summary>
        public static void ApplyComboBoxStyle(ComboBox comboBox)
        {
            comboBox.Font = InputFont;
            comboBox.ForeColor = TextPrimary;
            comboBox.BackColor = BackgroundWhite;
            comboBox.FlatStyle = FlatStyle.Flat;

            // Add focus indicators
            comboBox.Enter += (s, e) => comboBox.BackColor = BackgroundLight;
            comboBox.Leave += (s, e) => comboBox.BackColor = BackgroundWhite;
        }

        /// <summary>
        /// Applies styling to RichTextBox controls for enhanced readability
        /// </summary>
        public static void ApplyRichTextBoxStyle(RichTextBox richTextBox, bool isReadOnly = true)
        {
            richTextBox.Font = InputFont;
            richTextBox.ForeColor = TextPrimary;
            richTextBox.BackColor = isReadOnly ? BackgroundLight : BackgroundWhite;
            richTextBox.BorderStyle = BorderStyle.FixedSingle;
            richTextBox.ReadOnly = isReadOnly;

            if (isReadOnly)
            {
                richTextBox.TabStop = false;
                richTextBox.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Warning Dialog Styling

        /// <summary>
        /// Gets the background color for warning severity levels with enhanced palette
        /// </summary>
        public static Color GetWarningBackgroundColor(string severity)
        {
            return severity.ToLower() switch
            {
                "high" => ErrorRedLight,
                "medium" => WarningAmberLight,
                "low" => Color.FromArgb(250, 251, 252), // Very light gray
                _ => BackgroundWhite
            };
        }

        /// <summary>
        /// Gets the text color for warning severity levels
        /// </summary>
        public static Color GetWarningTextColor(string severity)
        {
            return severity.ToLower() switch
            {
                "high" => ErrorRed,
                "medium" => WarningAmber,
                "low" => PrimaryColor,
                _ => TextPrimary
            };
        }

        /// <summary>
        /// Applies modern styling to ListView controls for warnings
        /// </summary>
        public static void ApplyListViewStyle(ListView listView)
        {
            listView.Font = InputFont;
            listView.BackColor = BackgroundWhite;
            listView.BorderStyle = BorderStyle.FixedSingle;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.MultiSelect = false;
            listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            // Custom header drawing for modern appearance
            listView.OwnerDraw = true;
            listView.DrawColumnHeader += (s, e) => {
                using (var brush = new SolidBrush(GridHeaderBackground))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }

                if (e.Header != null)
                {
                    var textBounds = new Rectangle(e.Bounds.X + 8, e.Bounds.Y,
                                                 e.Bounds.Width - 16, e.Bounds.Height);
                    TextRenderer.DrawText(e.Graphics, e.Header.Text, HeaderFont, textBounds,
                        GridHeaderText, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
            };
            listView.DrawItem += (s, e) => e.DrawDefault = true;
            listView.DrawSubItem += (s, e) => e.DrawDefault = true;
        }

        #endregion

        #region Accessibility Helpers

        /// <summary>
        /// Ensures minimum contrast ratio for accessibility compliance
        /// </summary>
        public static bool IsAccessibleContrast(Color foreground, Color background)
        {
            // Simple luminance calculation for contrast checking
            double GetLuminance(Color color)
            {
                double r = color.R / 255.0;
                double g = color.G / 255.0;
                double b = color.B / 255.0;

                r = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
                g = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
                b = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

                return 0.2126 * r + 0.7152 * g + 0.0722 * b;
            }

            double l1 = GetLuminance(foreground);
            double l2 = GetLuminance(background);
            double contrast = (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);

            return contrast >= 4.5; // WCAG AA standard
        }

        #endregion

        #region Form Styling        /// <summary>
        /// Applies enhanced form-level styling with comprehensive high-DPI support
        /// </summary>
        public static void ApplyFormStyle(Control form)
        {
            form.BackColor = BackgroundGray;          // Light gray background
            form.Font = LabelFont;                    // Larger default font

            if (form is Form winForm)
            {
                winForm.AutoScaleMode = AutoScaleMode.Dpi;   // High-DPI support
            }

            // Enhanced DPI support
            var dpiScale = GetDpiScaleFactor(form);

            // Adjust minimum size for high-DPI
            if (form is Form f && f.MinimumSize != Size.Empty)
            {
                f.MinimumSize = ScaleForDpi(f.MinimumSize, form);
            }

            // Set padding for better spacing on all DPI levels
            form.Padding = new Padding(Spacing.GetMedium(form));

            // Force all child controls to invalidate for immediate visual updates
            foreach (Control control in form.Controls)
            {
                ApplyDpiAdjustments(control, dpiScale);
                control.Invalidate();
                InvalidateChildControls(control);
            }
        }

        /// <summary>
        /// Applies DPI adjustments to a control and its children
        /// </summary>
        private static void ApplyDpiAdjustments(Control control, float dpiScale)
        {
            // Adjust font sizes for very high DPI displays
            if (dpiScale > 1.5f && control.Font != null)
            {
                var currentFont = control.Font;
                var adjustedSize = Math.Max(currentFont.Size, 10f); // Ensure minimum readable size
                control.Font = new Font(currentFont.FontFamily, adjustedSize, currentFont.Style);
            }

            // Recursively apply to children
            foreach (Control child in control.Controls)
            {
                ApplyDpiAdjustments(child, dpiScale);
            }
        }

        /// <summary>
        /// Recursively invalidates child controls to force visual updates
        /// </summary>
        private static void InvalidateChildControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                child.Invalidate();
                InvalidateChildControls(child);
            }
        }

        #endregion

        #region High-DPI Support

        /// <summary>
        /// Gets the current DPI scale factor for proper high-DPI support
        /// </summary>
        /// <param name="control">Any control to get DPI from</param>
        /// <returns>Scale factor (1.0 = 96 DPI, 1.25 = 120 DPI, 1.5 = 144 DPI, etc.)</returns>
        public static float GetDpiScaleFactor(Control control)
        {
            using (var graphics = control.CreateGraphics())
            {
                return graphics.DpiX / 96f; // 96 DPI is the standard baseline
            }
        }

        /// <summary>
        /// Scales a size value based on current DPI
        /// </summary>
        public static int ScaleForDpi(int value, Control control)
        {
            return (int)(value * GetDpiScaleFactor(control));
        }

        /// <summary>
        /// Scales a size based on current DPI
        /// </summary>
        public static Size ScaleForDpi(Size size, Control control)
        {
            var scale = GetDpiScaleFactor(control);
            return new Size((int)(size.Width * scale), (int)(size.Height * scale));
        }

        /// <summary>
        /// Gets DPI-aware font size
        /// </summary>
        public static float GetDpiAwareFontSize(float baseFontSize, Control control)
        {
            return baseFontSize * GetDpiScaleFactor(control);
        }

        /// <summary>
        /// Standard spacing values that scale with DPI
        /// </summary>
        public static class Spacing
        {
            public static int GetSmall(Control control) => ScaleForDpi(4, control);
            public static int GetMedium(Control control) => ScaleForDpi(8, control);
            public static int GetLarge(Control control) => ScaleForDpi(16, control);
            public static int GetXLarge(Control control) => ScaleForDpi(24, control);
        }

        /// <summary>
        /// Standard control sizes that scale with DPI
        /// </summary>
        public static class ControlSizes
        {
            public static Size GetButtonSize(Control control) => ScaleForDpi(new Size(100, 35), control);
            public static Size GetTextBoxSize(Control control) => ScaleForDpi(new Size(70, 28), control);
            public static Size GetComboBoxSize(Control control) => ScaleForDpi(new Size(120, 28), control);
            public static int GetTextBoxHeight(Control control) => ScaleForDpi(28, control);
        }

        #endregion

        #region MaterialSkin Integration

        /// <summary>
        /// Configures MaterialSkin with BusBuddy theme colors
        /// </summary>
        public static void ConfigureMaterialSkin(MaterialForm form)
        {
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(form);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;

            // Use colors that match our AppTheme
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600,    // Primary color
                Primary.Blue700,    // Primary dark color
                Primary.Blue200,    // Primary light color
                Accent.LightBlue200, // Accent color
                TextShade.WHITE     // Text on primary
            );
        }

        /// <summary>
        /// Gets MaterialSkin-compatible primary color
        /// </summary>
        public static Primary GetMaterialPrimary()
        {
            // Convert our AppTheme primary color to closest Material primary
            return Primary.Blue600; // Closest to our PrimaryColor
        }

        /// <summary>
        /// Gets MaterialSkin-compatible accent color
        /// </summary>
        public static Accent GetMaterialAccent()
        {
            return Accent.LightBlue200; // Complements our color scheme
        }

        #endregion
    }
}
