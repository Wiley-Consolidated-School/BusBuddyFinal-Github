using System;
using System.Drawing;
using System.Windows.Forms;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Utility class for DPI-aware scaling and layout adjustments
    /// Provides consistent scaling across different screen DPI settings
    /// </summary>
    public static class DpiScaleHelper
    {
        /// <summary>
        /// Standard DPI baseline (96 DPI = 100% scaling)
        /// </summary>
        private const float BaseDpi = 96.0f;

        /// <summary>
        /// Scale factor structure for both width and height scaling
        /// </summary>
        public struct ScaleFactor
        {
            public float Width { get; }
            public float Height { get; }

            public ScaleFactor(float width, float height)
            {
                Width = width;
                Height = height;
            }

            public ScaleFactor(float uniformScale) : this(uniformScale, uniformScale) { }
        }

        /// <summary>
        /// Get the current system DPI scale factor
        /// </summary>
        public static ScaleFactor GetSystemScaleFactor()
        {
            using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                float scaleX = graphics.DpiX / BaseDpi;
                float scaleY = graphics.DpiY / BaseDpi;
                return new ScaleFactor(scaleX, scaleY);
            }
        }

        /// <summary>
        /// Get scale factor for a specific control
        /// </summary>
        /// <param name="control">Control to get scale factor for</param>
        /// <returns>Scale factor for the control's display</returns>
        public static ScaleFactor GetControlScaleFactor(Control control)
        {
            if (control == null)
                return GetSystemScaleFactor();

            float scaleX = control.DeviceDpi / BaseDpi;
            float scaleY = control.DeviceDpi / BaseDpi;
            return new ScaleFactor(scaleX, scaleY);
        }

        /// <summary>
        /// Scale a size value based on DPI
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="scaleFactor">Scale factor to apply</param>
        /// <returns>Scaled size</returns>
        public static int ScaleSize(int baseSize, ScaleFactor scaleFactor)
        {
            return (int)Math.Round(baseSize * scaleFactor.Width);
        }

        /// <summary>
        /// Scale a size value based on system DPI
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <returns>Scaled size</returns>
        public static int ScaleSize(int baseSize)
        {
            return ScaleSize(baseSize, GetSystemScaleFactor());
        }

        /// <summary>
        /// Scale a size value for a specific control
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="control">Control to scale for</param>
        /// <returns>Scaled size</returns>
        public static int ScaleSize(int baseSize, Control control)
        {
            return ScaleSize(baseSize, GetControlScaleFactor(control));
        }

        /// <summary>
        /// Scale padding values based on DPI
        /// </summary>
        /// <param name="basePadding">Base padding at 96 DPI</param>
        /// <param name="scaleFactor">Scale factor to apply</param>
        /// <returns>Scaled padding</returns>
        public static Padding ScalePadding(Padding basePadding, ScaleFactor scaleFactor)
        {
            return new Padding(
                (int)Math.Round(basePadding.Left * scaleFactor.Width),
                (int)Math.Round(basePadding.Top * scaleFactor.Height),
                (int)Math.Round(basePadding.Right * scaleFactor.Width),
                (int)Math.Round(basePadding.Bottom * scaleFactor.Height)
            );
        }

        /// <summary>
        /// Scale padding values based on system DPI
        /// </summary>
        /// <param name="basePadding">Base padding at 96 DPI</param>
        /// <returns>Scaled padding</returns>
        public static Padding ScalePadding(Padding basePadding)
        {
            return ScalePadding(basePadding, GetSystemScaleFactor());
        }

        /// <summary>
        /// Create DPI-aware padding with uniform values
        /// </summary>
        /// <param name="uniformPadding">Uniform padding value</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>DPI-scaled padding</returns>
        public static Padding CreatePadding(int uniformPadding, Control? control = null)
        {
            var scaleFactor = control != null ? GetControlScaleFactor(control) : GetSystemScaleFactor();
            int scaled = ScaleSize(uniformPadding, scaleFactor);
            return new Padding(scaled);
        }

        /// <summary>
        /// Create DPI-aware padding with different horizontal and vertical values
        /// </summary>
        /// <param name="horizontal">Horizontal padding</param>
        /// <param name="vertical">Vertical padding</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>DPI-scaled padding</returns>
        public static Padding CreatePadding(int horizontal, int vertical, Control? control = null)
        {
            var scaleFactor = control != null ? GetControlScaleFactor(control) : GetSystemScaleFactor();
            int h = ScaleSize(horizontal, scaleFactor);
            int v = ScaleSize(vertical, scaleFactor);
            return new Padding(h, v, h, v);
        }

        /// <summary>
        /// Create DPI-aware padding with specific values for each side
        /// </summary>
        /// <param name="left">Left padding</param>
        /// <param name="top">Top padding</param>
        /// <param name="right">Right padding</param>
        /// <param name="bottom">Bottom padding</param>
        /// <param name="control">Control for DPI context</param>
        /// <returns>DPI-scaled padding</returns>
        public static Padding CreatePadding(int left, int top, int right, int bottom, Control? control = null)
        {
            var scaleFactor = control != null ? GetControlScaleFactor(control) : GetSystemScaleFactor();
            return new Padding(
                ScaleSize(left, scaleFactor),
                ScaleSize(top, scaleFactor),
                ScaleSize(right, scaleFactor),
                ScaleSize(bottom, scaleFactor)
            );
        }

        /// <summary>
        /// Scale font size based on DPI
        /// </summary>
        /// <param name="baseFontSize">Base font size at 96 DPI</param>
        /// <param name="scaleFactor">Scale factor to apply</param>
        /// <returns>Scaled font size</returns>
        public static float ScaleFontSize(float baseFontSize, ScaleFactor scaleFactor)
        {
            return baseFontSize * scaleFactor.Width;
        }

        /// <summary>
        /// Scale font size based on system DPI
        /// </summary>
        /// <param name="baseFontSize">Base font size at 96 DPI</param>
        /// <returns>Scaled font size</returns>
        public static float ScaleFontSize(float baseFontSize)
        {
            return ScaleFontSize(baseFontSize, GetSystemScaleFactor());
        }

        /// <summary>
        /// Create a DPI-aware font
        /// </summary>
        /// <param name="fontFamily">Font family</param>
        /// <param name="baseFontSize">Base font size at 96 DPI</param>
        /// <param name="style">Font style</param>
        /// <param name="control">Optional control for context</param>
        /// <returns>DPI-scaled font</returns>
        public static Font CreateFont(FontFamily fontFamily, float baseFontSize, FontStyle style = FontStyle.Regular, Control? control = null)
        {
            var scaleFactor = control != null ? GetControlScaleFactor(control) : GetSystemScaleFactor();
            float scaledSize = ScaleFontSize(baseFontSize, scaleFactor);
            return new Font(fontFamily, scaledSize, style);
        }

        /// <summary>
        /// Scale a Rectangle based on DPI
        /// </summary>
        /// <param name="baseRect">Base rectangle at 96 DPI</param>
        /// <param name="scaleFactor">Scale factor to apply</param>
        /// <returns>Scaled rectangle</returns>
        public static Rectangle ScaleRectangle(Rectangle baseRect, ScaleFactor scaleFactor)
        {
            return new Rectangle(
                (int)Math.Round(baseRect.X * scaleFactor.Width),
                (int)Math.Round(baseRect.Y * scaleFactor.Height),
                (int)Math.Round(baseRect.Width * scaleFactor.Width),
                (int)Math.Round(baseRect.Height * scaleFactor.Height)
            );
        }

        /// <summary>
        /// Scale a Size based on DPI
        /// </summary>
        /// <param name="baseSize">Base size at 96 DPI</param>
        /// <param name="scaleFactor">Scale factor to apply</param>
        /// <returns>Scaled size</returns>
        public static Size ScaleSize(Size baseSize, ScaleFactor scaleFactor)
        {
            return new Size(
                (int)Math.Round(baseSize.Width * scaleFactor.Width),
                (int)Math.Round(baseSize.Height * scaleFactor.Height)
            );
        }
    }
}
