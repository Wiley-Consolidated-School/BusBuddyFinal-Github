using System;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Helpers
{
    /// <summary>
    /// Utility class providing common DPI-aware layout patterns and configurations
    /// Simplifies creation of responsive, properly scaled UI layouts
    /// </summary>
    public static class DpiAwareLayoutHelper
    {
        /// <summary>
        /// Standard spacing values scaled for DPI
        /// </summary>
        public static class StandardSpacing
        {
            public static int GetTiny(Control? control = null) => DpiScaleHelper.ScaleSize(2, control);
            public static int GetSmall(Control? control = null) => DpiScaleHelper.ScaleSize(4, control);
            public static int GetMedium(Control? control = null) => DpiScaleHelper.ScaleSize(8, control);
            public static int GetLarge(Control? control = null) => DpiScaleHelper.ScaleSize(16, control);
            public static int GetExtraLarge(Control? control = null) => DpiScaleHelper.ScaleSize(24, control);
        }

        /// <summary>
        /// Standard touch target sizes for accessibility
        /// </summary>
        public static class TouchTargets
        {
            public static int GetMinimum(Control? control = null) => DpiScaleHelper.ScaleSize(44, control);
            public static int GetComfortable(Control? control = null) => DpiScaleHelper.ScaleSize(48, control);
            public static int GetLarge(Control? control = null) => DpiScaleHelper.ScaleSize(56, control);
        }

        /// <summary>
        /// Create a properly spaced form layout with DPI awareness
        /// </summary>
        /// <param name="form">Form to configure</param>
        /// <param name="title">Form title</param>
        /// <param name="minWidth">Minimum width at 96 DPI</param>
        /// <param name="minHeight">Minimum height at 96 DPI</param>
        public static void ConfigureForm(Form form, string title, int minWidth = 800, int minHeight = 600)
        {
            if (form == null) return;

            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(form);

            // Basic form configuration
            form.Text = title;
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.StartPosition = FormStartPosition.CenterScreen;

            // Set DPI-aware minimum size
            form.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(minWidth, scaleFactor),
                DpiScaleHelper.ScaleSize(minHeight, scaleFactor)
            );

            // Apply Material Design colors if it's a MaterialForm
            if (form is MaterialForm materialForm)
            {
                form.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;
                form.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            }

            // Set appropriate font
            form.Font = DpiScaleHelper.CreateFont(
                SystemFonts.DefaultFont.FontFamily,
                8.25f,
                FontStyle.Regular,
                form
            );
        }

        /// <summary>
        /// Create a responsive grid layout for forms
        /// </summary>
        /// <param name="parent">Parent container</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="rows">Number of rows</param>
        /// <param name="fillParent">Whether to fill the parent container</param>
        /// <returns>Configured TableLayoutPanel</returns>
        public static TableLayoutPanel CreateResponsiveGrid(Control parent, int columns, int rows, bool fillParent = true)
        {
            var layout = new TableLayoutPanel
            {
                AutoSize = !fillParent,
                AutoSizeMode = fillParent ? AutoSizeMode.GrowOnly : AutoSizeMode.GrowAndShrink,
                ColumnCount = columns,
                RowCount = rows,
                Dock = fillParent ? DockStyle.Fill : DockStyle.None,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                BackColor = Color.Transparent
            };

            // Configure proportional column widths
            layout.ColumnStyles.Clear();
            for (int i = 0; i < columns; i++)
            {
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            // Configure proportional row heights
            layout.RowStyles.Clear();
            for (int i = 0; i < rows; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Apply standard spacing
            layout.Padding = new Padding(StandardSpacing.GetMedium(parent));
            layout.Margin = new Padding(StandardSpacing.GetSmall(parent));

            return layout;
        }

        /// <summary>
        /// Create a form section with title and content area
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="title">Section title</param>
        /// <param name="content">Content control</param>
        /// <returns>Configured section panel</returns>
        public static Panel CreateFormSection(Control parent, string title, Control content)
        {
            var sectionPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Padding = new Padding(StandardSpacing.GetMedium(parent))
            };

            // Create section title
            var titleLabel = new MaterialLabel
            {
                Text = title,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = MaterialDesignThemeManager.DarkTheme.Primary,
                Font = DpiScaleHelper.CreateFont(
                    SystemFonts.DefaultFont.FontFamily,
                    10f,
                    FontStyle.Bold,
                    parent
                ),
                Margin = new Padding(0, 0, 0, StandardSpacing.GetSmall(parent))
            };

            // Configure content
            content.Dock = DockStyle.Top;
            content.Margin = new Padding(StandardSpacing.GetSmall(parent), 0, 0, StandardSpacing.GetMedium(parent));

            // Add controls in reverse order for proper docking
            sectionPanel.Controls.Add(content);
            sectionPanel.Controls.Add(titleLabel);

            return sectionPanel;
        }

        /// <summary>
        /// Create a button row with proper spacing
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="buttons">Buttons to add</param>
        /// <param name="alignment">Button alignment</param>
        /// <returns>Configured button panel</returns>
        public static Panel CreateButtonRow(Control parent, MaterialButton[] buttons, ContentAlignment alignment = ContentAlignment.MiddleRight)
        {
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = TouchTargets.GetComfortable(parent) + (StandardSpacing.GetMedium(parent) * 2),
                BackColor = Color.Transparent,
                Padding = new Padding(StandardSpacing.GetMedium(parent))
            };

            var flowLayout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent
            };

            // Set alignment
            switch (alignment)
            {
                case ContentAlignment.MiddleLeft:
                    flowLayout.Dock = DockStyle.Left;
                    break;
                case ContentAlignment.MiddleCenter:
                    flowLayout.Anchor = AnchorStyles.None;
                    break;
                case ContentAlignment.MiddleRight:
                default:
                    flowLayout.Dock = DockStyle.Right;
                    break;
            }

            // Configure buttons
            foreach (var button in buttons)
            {
                button.Height = TouchTargets.GetComfortable(parent);
                button.MinimumSize = new Size(
                    DpiScaleHelper.ScaleSize(100, parent), // Minimum button width
                    TouchTargets.GetComfortable(parent)
                );
                button.Margin = new Padding(StandardSpacing.GetSmall(parent));
                button.AutoSize = false;

                flowLayout.Controls.Add(button);
            }

            buttonPanel.Controls.Add(flowLayout);
            return buttonPanel;
        }

        /// <summary>
        /// Create a labeled input control with proper spacing
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="labelText">Label text</param>
        /// <param name="inputControl">Input control</param>
        /// <param name="isRequired">Whether the field is required</param>
        /// <returns>Configured input panel</returns>
        public static Panel CreateLabeledInput(Control parent, string labelText, Control inputControl, bool isRequired = false)
        {
            var inputPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.Transparent,
                Margin = new Padding(0, StandardSpacing.GetSmall(parent), 0, StandardSpacing.GetMedium(parent))
            };

            // Create label
            var label = new MaterialLabel
            {
                Text = isRequired ? $"{labelText} *" : labelText,
                Dock = DockStyle.Top,
                AutoSize = true,
                ForeColor = isRequired ? MaterialDesignThemeManager.DarkTheme.Error : MaterialDesignThemeManager.DarkTheme.OnSurface,
                Font = DpiScaleHelper.CreateFont(
                    SystemFonts.DefaultFont.FontFamily,
                    9f,
                    FontStyle.Regular,
                    parent
                ),
                Margin = new Padding(0, 0, 0, StandardSpacing.GetTiny(parent))
            };

            // Configure input control
            inputControl.Dock = DockStyle.Top;
            if (inputControl.Height < TouchTargets.GetMinimum(parent))
            {
                inputControl.Height = TouchTargets.GetMinimum(parent);
            }

            // Add controls in reverse order for proper docking
            inputPanel.Controls.Add(inputControl);
            inputPanel.Controls.Add(label);

            return inputPanel;
        }

        /// <summary>
        /// Create a card container with proper Material Design styling
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <param name="title">Optional card title</param>
        /// <param name="content">Card content</param>
        /// <returns>Configured MaterialCard</returns>
        public static MaterialCard CreateMaterialCard(Control parent, string? title, Control content)
        {
            var card = new MaterialCard
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer,
                Margin = new Padding(StandardSpacing.GetMedium(parent)),
                Padding = new Padding(StandardSpacing.GetLarge(parent))
            };

            if (!string.IsNullOrEmpty(title))
            {
                var titleLabel = new MaterialLabel
                {
                    Text = title,
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface,
                    Font = DpiScaleHelper.CreateFont(
                        SystemFonts.DefaultFont.FontFamily,
                        12f,
                        FontStyle.Bold,
                        parent
                    ),
                    Margin = new Padding(0, 0, 0, StandardSpacing.GetMedium(parent))
                };

                content.Dock = DockStyle.Fill;
                card.Controls.Add(content);
                card.Controls.Add(titleLabel);
            }
            else
            {
                content.Dock = DockStyle.Fill;
                card.Controls.Add(content);
            }

            return card;
        }

        /// <summary>
        /// Apply consistent spacing to a collection of controls
        /// </summary>
        /// <param name="controls">Controls to space</param>
        /// <param name="spacing">Spacing size</param>
        /// <param name="parent">Parent for DPI context</param>
        public static void ApplyConsistentSpacing(Control[] controls, int spacing, Control? parent = null)
        {
            var dpiSpacing = DpiScaleHelper.ScaleSize(spacing, parent);
            var padding = new Padding(dpiSpacing);

            foreach (var control in controls)
            {
                control.Margin = padding;
            }
        }

        /// <summary>
        /// Create a responsive two-column layout for form fields
        /// </summary>
        /// <param name="parent">Parent control</param>
        /// <returns>Configured two-column layout</returns>
        public static TableLayoutPanel CreateTwoColumnFormLayout(Control parent)
        {
            var layout = CreateResponsiveGrid(parent, 2, 1, true);

            // Configure columns: 30% for labels, 70% for inputs
            layout.ColumnStyles.Clear();
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));

            return layout;
        }
    }
}
