using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Services
{
    /// <summary>
    /// Enhanced theme service for consistent theming across BusBuddy application
    /// </summary>
    public static class EnhancedThemeService
    {
        public static readonly Color PrimaryColor = ColorTranslator.FromHtml("#1976D2");
        public static readonly Color PrimaryDarkColor = ColorTranslator.FromHtml("#0D47A1");
        public static readonly Color BackgroundColor = ColorTranslator.FromHtml("#FAFAFA");
        public static readonly Color SurfaceColor = Color.White;
        public static readonly Color TextColor = ColorTranslator.FromHtml("#212121");
        public static readonly Color SecondaryTextColor = ColorTranslator.FromHtml("#757575");
        public static readonly Color BorderColor = ColorTranslator.FromHtml("#E0E0E0");
        public static readonly Color SuccessColor = ColorTranslator.FromHtml("#4CAF50");
        public static readonly Color WarningColor = ColorTranslator.FromHtml("#FF9800");
        public static readonly Color ErrorColor = ColorTranslator.FromHtml("#D32F2F");
        public static readonly Color SidebarColor = ColorTranslator.FromHtml("#263238");
        public static readonly Color HeaderColor = ColorTranslator.FromHtml("#1976D2");
        public static readonly Color HeaderTextColor = Color.White;
        public static readonly Color ButtonColor = ColorTranslator.FromHtml("#1976D2");
        public static readonly Color ButtonTextColor = Color.White;

        public static readonly string[] PreferredFonts = { "Segoe UI", "Arial", "Microsoft Sans Serif", "Tahoma" };
        public static readonly Font DefaultFont = GetSafeFont(10F, FontStyle.Regular);
        public static readonly Font HeaderFont = GetSafeFont(18F, FontStyle.Bold);
        public static readonly Font ButtonFont = GetSafeFont(10F, FontStyle.Bold);

        public static Font GetSafeFont(float size, FontStyle style = FontStyle.Regular)
        {
            float safeFontSize = Math.Max(size, 10F);
            foreach (var fontName in PreferredFonts)
            {
                try
                {
                    return new Font(fontName, safeFontSize, style);
                }
                catch { }
            }
            return new Font(SystemFonts.DefaultFont.FontFamily, safeFontSize, style);
        }

        public static void ApplyTheme(Control control)
        {
            if (control == null) return;
            control.BackColor = SurfaceColor;
            control.ForeColor = TextColor;
            control.Font = DefaultFont;

            switch (control)
            {
                case Button button:
                    button.BackColor = ButtonColor;
                    button.ForeColor = ButtonTextColor;
                    button.Font = ButtonFont;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 0;
                    button.MouseEnter += (s, e) => button.BackColor = PrimaryDarkColor;
                    button.MouseLeave += (s, e) => button.BackColor = ButtonColor;
                    break;
                case Label label:
                    label.BackColor = Color.Transparent;
                    label.ForeColor = TextColor;
                    label.Font = DefaultFont;
                    break;
                case Panel panel:
                    panel.BackColor = SurfaceColor;
                    break;
            }
        }
    }
}

