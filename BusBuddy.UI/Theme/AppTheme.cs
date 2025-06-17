using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;

namespace BusBuddy.UI.Theme
{
    public static class AppTheme
    {
        // Primary Colors
        public static Color PrimaryColor => Color.FromArgb(71, 85, 105);
        public static Color PrimaryColorDark => Color.FromArgb(51, 65, 85);
        public static Color PrimaryColorLight => Color.FromArgb(148, 163, 184);

        // Secondary Colors
        public static Color SecondaryGray => Color.FromArgb(107, 114, 128);
        public static Color SecondaryGrayLight => Color.FromArgb(243, 244, 246);
        public static Color SecondaryGrayDark => Color.FromArgb(75, 85, 99);

        // Status Colors
        public static Color SuccessGreen => Color.FromArgb(34, 197, 94);
        public static Color SuccessGreenLight => Color.FromArgb(236, 253, 245);
        public static Color WarningAmber => Color.FromArgb(245, 158, 11);
        public static Color WarningAmberLight => Color.FromArgb(255, 251, 235);
        public static Color ErrorRed => Color.FromArgb(239, 68, 68);
        public static Color ErrorRedLight => Color.FromArgb(254, 242, 242);

        // Background Colors
        public static Color BackgroundWhite => Color.FromArgb(255, 255, 255);
        public static Color BackgroundGray => Color.FromArgb(249, 250, 251);
        public static Color BackgroundLight => Color.FromArgb(248, 250, 252);
        public static Color BackgroundCard => Color.FromArgb(255, 255, 255);

        // Text Colors
        public static Color TextPrimary => Color.FromArgb(15, 23, 42);
        public static Color TextSecondary => Color.FromArgb(100, 116, 139);
        public static Color TextOnPrimary => Color.White;
        public static Color TextOnDark => Color.White;

        // Grid Colors
        public static Color GridHeaderBackground => PrimaryColor;
        public static Color GridHeaderText => TextOnPrimary;
        public static Color GridAlternateRow => Color.FromArgb(248, 250, 252);
        public static Color GridBorder => Color.FromArgb(226, 232, 240);

        // Fonts
        public static Font LabelFont => new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font LabelBoldFont => new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font HeaderFont => new Font("Segoe UI", 12F, FontStyle.Bold);
        public static Font TitleFont => new Font("Segoe UI", 14F, FontStyle.Bold);
        public static Font InputFont => new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font ButtonFont => new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font ButtonBoldFont => new Font("Segoe UI", 10F, FontStyle.Bold);
        public static Font SummaryFont => new Font("Segoe UI", 11F, FontStyle.Bold);

        // Button Styling
        public static void ApplyPrimaryButtonStyle(Button button)
        {
            button.BackColor = PrimaryColor;
            button.ForeColor = TextOnPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = ButtonFont;
        }

        public static void ApplySecondaryButtonStyle(Button button)
        {
            button.BackColor = SecondaryGrayLight;
            button.ForeColor = TextPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = SecondaryGray;
            button.Cursor = Cursors.Hand;
            button.Font = ButtonFont;
        }

        public static void ApplySuccessButtonStyle(Button button)
        {
            button.BackColor = SuccessGreen;
            button.ForeColor = TextOnPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = ButtonFont;
        }

        public static void ApplyWarningButtonStyle(Button button)
        {
            button.BackColor = WarningAmber;
            button.ForeColor = TextOnPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = ButtonFont;
        }

        public static void ApplyDangerButtonStyle(Button button)
        {
            button.BackColor = ErrorRed;
            button.ForeColor = TextOnPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = ButtonFont;
        }

        // Control Styling
        public static void ApplyDataGridViewStyle(DataGridView dataGridView)
        {
            dataGridView.BackgroundColor = BackgroundWhite;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.GridColor = GridBorder;
            dataGridView.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackground;
            dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderText;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = GridAlternateRow;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;
            dataGridView.EnableHeadersVisualStyles = false;
        }

        public static void ApplyGroupBoxStyle(GroupBox groupBox)
        {
            groupBox.ForeColor = PrimaryColor;
            groupBox.BackColor = BackgroundLight;
            groupBox.FlatStyle = FlatStyle.Flat;
            groupBox.Font = HeaderFont;
        }

        public static void ApplyLabelStyle(Label label, bool bold = false)
        {
            label.ForeColor = PrimaryColorDark;
            label.BackColor = Color.Transparent;
            label.Font = bold ? LabelBoldFont : LabelFont;
            label.AutoSize = true;
        }

        public static void ApplyTextBoxStyle(TextBox textBox, bool readOnly = false)
        {
            textBox.ForeColor = TextPrimary;
            textBox.BackColor = readOnly ? WarningAmberLight : BackgroundWhite;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = InputFont;
            textBox.ReadOnly = readOnly;
        }

        public static void ApplyPanelStyle(Panel panel, bool asCard = false)
        {
            if (asCard)
            {
                panel.BackColor = BackgroundCard;
                panel.BorderStyle = BorderStyle.FixedSingle;
            }
            else
            {
                panel.BackColor = BackgroundLight;
            }
        }

        public static void ApplyComboBoxStyle(ComboBox comboBox)
        {
            comboBox.ForeColor = TextPrimary;
            comboBox.BackColor = BackgroundWhite;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.Font = InputFont;
        }

        public static void ApplyRichTextBoxStyle(RichTextBox richTextBox, bool readOnly = false)
        {
            richTextBox.ForeColor = TextPrimary;
            richTextBox.BackColor = readOnly ? BackgroundLight : BackgroundWhite;
            richTextBox.BorderStyle = BorderStyle.FixedSingle;
            richTextBox.Font = InputFont;
            richTextBox.ReadOnly = readOnly;
            richTextBox.TabStop = !readOnly;
        }

        public static void ApplyListViewStyle(ListView listView)
        {
            listView.BackColor = BackgroundWhite;
            listView.BorderStyle = BorderStyle.FixedSingle;
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;
            listView.MultiSelect = false;
            listView.OwnerDraw = true;
        }

        // Warning Dialog Colors
        public static Color GetWarningBackgroundColor(string severity)
        {
            return severity.ToLower() switch
            {
                "high" => ErrorRedLight,
                "medium" => WarningAmberLight,
                "low" => SuccessGreenLight,
                _ => BackgroundLight
            };
        }

        public static Color GetWarningTextColor(string severity)
        {
            return severity.ToLower() switch
            {
                "high" => ErrorRed,
                "medium" => WarningAmber,
                "low" => SuccessGreen,
                _ => TextPrimary
            };
        }

        // Accessibility
        public static bool IsAccessibleContrast(Color foreground, Color background)
        {
            var fLuminance = GetLuminance(foreground);
            var bLuminance = GetLuminance(background);
            var contrast = (Math.Max(fLuminance, bLuminance) + 0.05) / (Math.Min(fLuminance, bLuminance) + 0.05);
            return contrast >= 4.5; // WCAG AA standard
        }

        private static double GetLuminance(Color color)
        {
            var r = GetSRGBValue(color.R);
            var g = GetSRGBValue(color.G);
            var b = GetSRGBValue(color.B);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        private static double GetSRGBValue(int value)
        {
            var normalized = value / 255.0;
            return normalized <= 0.03928 ? normalized / 12.92 : Math.Pow((normalized + 0.055) / 1.055, 2.4);
        }

        // DPI Support
        public static float GetDpiScaleFactor(Control control)
        {
            if (control?.DeviceDpi > 0)
                return control.DeviceDpi / 96.0f;
            return 1.0f;
        }

        public static int ScaleForDpi(int value, Control control)
        {
            return (int)(value * GetDpiScaleFactor(control));
        }

        public static Size ScaleForDpi(Size size, Control control)
        {
            var factor = GetDpiScaleFactor(control);
            return new Size((int)(size.Width * factor), (int)(size.Height * factor));
        }

        public static float GetDpiAwareFontSize(float baseSize, Control control)
        {
            return baseSize * GetDpiScaleFactor(control);
        }

        public static class Spacing
        {
            public static int GetSmall(Control control) => ScaleForDpi(4, control);
            public static int GetMedium(Control control) => ScaleForDpi(8, control);
            public static int GetLarge(Control control) => ScaleForDpi(16, control);
        }

        public static class ControlSizes
        {
            public static Size GetButtonSize(Control control) => ScaleForDpi(new Size(80, 30), control);
            public static int GetTextBoxHeight(Control control) => ScaleForDpi(24, control);
        }

        // Form Styling
        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = BackgroundGray;
            form.AutoScaleMode = AutoScaleMode.Dpi;
        }

        // MaterialSkin Integration
        public static MaterialSkin.Primary GetMaterialPrimary()
        {
            return MaterialSkin.Primary.Blue600;
        }

        public static MaterialSkin.Accent GetMaterialAccent()
        {
            return MaterialSkin.Accent.LightBlue200;
        }
    }
}
