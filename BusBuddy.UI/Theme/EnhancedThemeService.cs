using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Theme
{
    public static class EnhancedThemeService
    {
        #region Color Scheme

        public static Color BackgroundColor => Color.FromArgb(245, 245, 245);
        public static Color SurfaceColor => Color.FromArgb(255, 255, 255);
        public static Color HeaderColor => Color.FromArgb(33, 150, 243);
        public static Color SidebarColor => Color.FromArgb(250, 250, 250);
        public static Color ButtonColor => Color.FromArgb(33, 150, 243);
        public static Color ButtonTextColor => Color.White;
        public static Color TextColor => Color.FromArgb(33, 33, 33);
        public static Color SecondaryTextColor => Color.FromArgb(117, 117, 117);
        public static Color BorderColor => Color.FromArgb(224, 224, 224);
        public static Color SuccessColor => Color.FromArgb(76, 175, 80);
        public static Color WarningColor => Color.FromArgb(255, 152, 0);
        public static Color ErrorColor => Color.FromArgb(244, 67, 54);

        #endregion

        #region Font Management

        public static Font DefaultFont => GetSafeFont(10F);
        public static Font HeaderFont => GetSafeFont(14F, FontStyle.Bold);
        public static Font ButtonFont => GetSafeFont(10F, FontStyle.Regular);
        public static Font LabelFont => GetSafeFont(9F);

        public static Font GetSafeFont(float size, FontStyle style = FontStyle.Regular)
        {
            try { return new Font("Segoe UI", size, style); }
            catch { return new Font(FontFamily.GenericSansSerif, size, style); }
        }

        #endregion

        #region DPI Awareness

        public static int ScaleByDpi(int value, float dpiScale) => (int)(value * dpiScale);
        public static Size GetDpiAwareSize(Size originalSize, float dpiScale) => new Size(ScaleByDpi(originalSize.Width, dpiScale), ScaleByDpi(originalSize.Height, dpiScale));
        public static Padding GetDpiAwarePadding(Padding originalPadding, float dpiScale) => new Padding(ScaleByDpi(originalPadding.Left, dpiScale), ScaleByDpi(originalPadding.Top, dpiScale), ScaleByDpi(originalPadding.Right, dpiScale), ScaleByDpi(originalPadding.Bottom, dpiScale));

        #endregion
    }
}
