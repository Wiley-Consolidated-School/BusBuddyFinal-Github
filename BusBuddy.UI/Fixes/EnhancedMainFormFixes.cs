using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;

namespace BusBuddy.UI.Fixes
{
    /// <summary>
    /// Enhanced version of BusBuddyDashboard with improved distortion fixes
    /// This class provides methods to fix common distortion issues
    /// </summary>
    public static class EnhancedMainFormFixes
    {
        /// <summary>
        /// Apply comprehensive distortion fixes to BusBuddyDashboard
        /// </summary>
        /// <param name="form">The form to fix</param>
        public static void ApplyDistortionFixes(Form form)
        {
            if (form == null) return;

            try
            {
                form.SuspendLayout();

                // Fix 1: Ensure proper DPI awareness
                FixDpiAwareness(form);

                // Fix 2: Fix TableLayoutPanel configuration
                FixTableLayoutPanelConfiguration(form);

                // Fix 3: Fix Material Design button sizing
                FixMaterialDesignButtonSizing(form);

                // Fix 4: Fix DataGridView configuration
                FixDataGridViewConfiguration(form);

                // Fix 5: Fix panel padding and margins
                FixPanelPaddingAndMargins(form);

                // Fix 6: Ensure proper control anchoring
                FixControlAnchoring(form);

                form.ResumeLayout(true);
                form.PerformLayout();
                form.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying distortion fixes: {ex.Message}");
                // Ensure layout is resumed even if fixes fail
                try
                {
                    form.ResumeLayout(true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Create an improved version of the CreateMainLayout method
        /// </summary>
        /// <param name="form">The form to create layout for</param>
        /// <param name="databaseService">Database service dependency</param>
        /// <param name="navigationService">Navigation service dependency</param>
        public static void CreateImprovedMainLayout(Form form, object databaseService, object navigationService)
        {
            form.SuspendLayout();

            try
            {
                // Clear existing controls
                form.Controls.Clear();

                // Create main container with improved configuration
                var mainContainer = CreateImprovedTableLayoutPanel();
                form.Controls.Add(mainContainer);

                // Create improved sections
                CreateImprovedHeaderSection(mainContainer);
                CreateImprovedDashboardSection(mainContainer, navigationService);

                // Apply final fixes
                ApplyDistortionFixes(form);
            }
            finally
            {
                form.ResumeLayout(true);
                form.PerformLayout();
            }
        }

        private static void FixDpiAwareness(Form form)
        {
            // Ensure proper DPI settings
            if (form.AutoScaleMode != AutoScaleMode.Dpi)
            {
                form.AutoScaleMode = AutoScaleMode.Dpi;
            }

            var expectedDimensions = new SizeF(96F, 96F);
            if (form.AutoScaleDimensions != expectedDimensions)
            {
                form.AutoScaleDimensions = expectedDimensions;
            }

            // Ensure proper minimum size scaling
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(form);
            var minWidth = DpiScaleHelper.ScaleSize(1024, scaleFactor);
            var minHeight = DpiScaleHelper.ScaleSize(768, scaleFactor);

            if (form.MinimumSize.Width < minWidth || form.MinimumSize.Height < minHeight)
            {
                form.MinimumSize = new Size(minWidth, minHeight);
            }
        }

        private static void FixTableLayoutPanelConfiguration(Form form)
        {
            var tableLayoutPanels = GetControlsByType<TableLayoutPanel>(form);

            foreach (var panel in tableLayoutPanels)
            {
                // Ensure proper dock style
                if (panel.Dock != DockStyle.Fill && panel.Parent == form)
                {
                    panel.Dock = DockStyle.Fill;
                }

                // Fix row styles
                if (panel.RowStyles.Count != panel.RowCount)
                {
                    panel.RowStyles.Clear();

                    // Add proper row styles based on intended layout
                    if (panel.RowCount == 3) // Main layout: Header, Spacer, Dashboard
                    {
                        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header

                        var spacerHeight = MaterialDesignThemeManager.GetDpiAwareSize(32, panel);
                        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, spacerHeight)); // Spacer

                        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Dashboard
                    }
                }

                // Fix column styles
                if (panel.ColumnStyles.Count != panel.ColumnCount)
                {
                    panel.ColumnStyles.Clear();
                    for (int i = 0; i < panel.ColumnCount; i++)
                    {
                        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / panel.ColumnCount));
                    }
                }
            }
        }

        private static void FixMaterialDesignButtonSizing(Form form)
        {
            var materialButtons = GetControlsByType<MaterialSkin.Controls.MaterialButton>(form);

            foreach (var button in materialButtons)
            {
                if (!button.AutoSize)
                {
                    var scaleFactor = DpiScaleHelper.GetControlScaleFactor(button);

                    // Apply standard Material Design button size
                    var standardWidth = DpiScaleHelper.ScaleSize(170, scaleFactor);
                    var standardHeight = DpiScaleHelper.ScaleSize(52, scaleFactor);

                    // Only resize if current size is significantly different
                    if (Math.Abs(button.Width - standardWidth) > 20 ||
                        Math.Abs(button.Height - standardHeight) > 10)
                    {
                        button.Size = new Size(standardWidth, standardHeight);
                    }
                }

                // Ensure proper minimum size
                var minSize = DpiScaleHelper.ScaleSize(50, DpiScaleHelper.GetControlScaleFactor(button));
                if (button.Width < minSize)
                    button.Width = minSize;
                if (button.Height < minSize / 2)
                    button.Height = minSize / 2;
            }
        }

        private static void FixDataGridViewConfiguration(Form form)
        {
            var dataGridViews = GetControlsByType<DataGridView>(form);

            foreach (var grid in dataGridViews)
            {
                var scaleFactor = DpiScaleHelper.GetControlScaleFactor(grid);

                // Fix row height for Material Design compliance
                var materialRowHeight = DpiScaleHelper.ScaleSize(56, scaleFactor);
                if (grid.RowTemplate.Height < materialRowHeight)
                {
                    grid.RowTemplate.Height = materialRowHeight;
                }

                // Fix column header height
                var headerHeight = DpiScaleHelper.ScaleSize(48, scaleFactor);
                if (grid.ColumnHeadersHeight < headerHeight)
                {
                    grid.ColumnHeadersHeight = headerHeight;
                }

                // Ensure minimum grid size
                var minWidth = DpiScaleHelper.ScaleSize(400, scaleFactor);
                var minHeight = DpiScaleHelper.ScaleSize(200, scaleFactor);

                if (grid.Width < minWidth && !grid.AutoSize)
                    grid.Width = minWidth;
                if (grid.Height < minHeight && !grid.AutoSize)
                    grid.Height = minHeight;

                // Fix anchoring for responsive behavior
                if (grid.Anchor == AnchorStyles.None)
                {
                    grid.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
            }
        }

        private static void FixPanelPaddingAndMargins(Form form)
        {
            var panels = GetControlsByType<Panel>(form);

            foreach (var panel in panels)
            {
                // Apply consistent DPI-aware padding
                if (panel.Padding == Padding.Empty && panel.Controls.Count > 0)
                {
                    var padding = MaterialDesignThemeManager.GetDpiAwarePadding(16, 16, panel);
                    panel.Padding = padding;
                }

                // Ensure proper minimum size for scrollable panels
                if (panel.AutoScroll && panel.Size.Width < 200)
                {
                    var scaleFactor = DpiScaleHelper.GetControlScaleFactor(panel);
                    var minWidth = DpiScaleHelper.ScaleSize(400, scaleFactor);
                    if (panel.Width < minWidth)
                        panel.Width = minWidth;
                }
            }
        }

        private static void FixControlAnchoring(Form form)
        {
            foreach (Control control in form.Controls)
            {
                FixControlAnchoringRecursive(control);
            }
        }

        private static void FixControlAnchoringRecursive(Control control)
        {
            // Fix anchoring for controls in TableLayoutPanel
            if (control.Parent is TableLayoutPanel)
            {
                if (control.Anchor == AnchorStyles.None && control.Dock == DockStyle.None)
                {
                    control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                }
            }

            // Recursively fix child controls
            foreach (Control child in control.Controls)
            {
                FixControlAnchoringRecursive(child);
            }
        }

        private static TableLayoutPanel CreateImprovedTableLayoutPanel()
        {
            var mainContainer = new TableLayoutPanel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;

            // Use system DPI for padding calculation
            var padding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 24, null);
            mainContainer.Padding = padding;

            mainContainer.RowCount = 3;
            mainContainer.ColumnCount = 1;

            // Improved row styles with better sizing
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header

            float spacerHeight = MaterialDesignThemeManager.GetDpiAwareSize(24, null); // Reduced spacer
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, spacerHeight)); // Spacer

            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Dashboard

            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            return mainContainer;
        }

        private static void CreateImprovedHeaderSection(TableLayoutPanel parent)
        {
            var headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer;
            headerPanel.AutoSize = true; // Allow auto-sizing for header

            var headerPadding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 20, headerPanel);
            headerPanel.Padding = headerPadding;

            // Create improved header labels with better positioning
            var welcomeLabel = new MaterialSkin.Controls.MaterialLabel();
            welcomeLabel.Text = "Welcome to BusBuddy!";
            welcomeLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.H3;
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new Point(0, 0);
            welcomeLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            welcomeLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            headerPanel.Controls.Add(welcomeLabel);

            var descriptionLabel = new MaterialSkin.Controls.MaterialLabel();
            descriptionLabel.Text = "The comprehensive school bus tracking and management system";
            descriptionLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new Point(0, welcomeLabel.Bottom + MaterialDesignThemeManager.GetDpiAwareSize(8, descriptionLabel));
            descriptionLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurfaceVariant;
            descriptionLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            headerPanel.Controls.Add(descriptionLabel);

            parent.Controls.Add(headerPanel, 0, 0);
        }

        private static void CreateImprovedDashboardSection(TableLayoutPanel parent, object navigationService)
        {
            var dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.AutoScroll = true;
            dashboardPanel.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;

            var padding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 24, dashboardPanel);
            dashboardPanel.Padding = padding;

            // Add improved content with better spacing
            // Note: This is a simplified version - in practice, you'd recreate the full dashboard content
            var sampleLabel = new MaterialSkin.Controls.MaterialLabel();
            sampleLabel.Text = "Dashboard content would be recreated here with improved layout";
            sampleLabel.AutoSize = true;
            sampleLabel.Location = new Point(0, 0);
            dashboardPanel.Controls.Add(sampleLabel);

            parent.Controls.Add(dashboardPanel, 0, 2);
        }

        private static System.Collections.Generic.IEnumerable<T> GetControlsByType<T>(Control parent) where T : Control
        {
            foreach (Control control in parent.Controls)
            {
                if (control is T targetControl)
                    yield return targetControl;

                foreach (var found in GetControlsByType<T>(control))
                    yield return found;
            }
        }
    }
}
