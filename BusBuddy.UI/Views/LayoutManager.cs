using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Layout Manager for BusBuddy Dashboard - Simplified version for Phase 4
    /// Handles creation of basic dashboard layouts using Syncfusion controls
    /// </summary>
    public static class LayoutManager
    {
        /// <summary>
        /// Creates a basic layout for the dashboard
        /// </summary>
        public static Control CreateBasicLayout(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var mainPanel = new Panel
            {
                Name = "MainLayoutPanel",
                Dock = DockStyle.Fill,
                BackColor = EnhancedThemeService.BackgroundColor,
                Padding = new Padding(20)
            };
            SfSkinManager.SetVisualStyle(mainPanel, "MaterialLight");

            var titleLabel = ControlFactory.CreateLabel("🚌 BusBuddy Dashboard - Syncfusion Layout",
                EnhancedThemeService.HeaderFont, EnhancedThemeService.TextColor);
            titleLabel.Dock = DockStyle.Top;
            titleLabel.Height = 60;
            mainPanel.Controls.Add(titleLabel);

            var contentLabel = ControlFactory.CreateLabel("Dashboard loaded successfully with Syncfusion controls.",
                EnhancedThemeService.DefaultFont, EnhancedThemeService.TextColor);
            contentLabel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(contentLabel);

            form.Controls.Add(mainPanel);
            return mainPanel;
        }

        /// <summary>
        /// Creates a fallback layout for error scenarios
        /// </summary>
        public static Control CreateFallbackLayout(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));

            var fallbackPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = EnhancedThemeService.BackgroundColor,
                Padding = new Padding(20)
            };
            SfSkinManager.SetVisualStyle(fallbackPanel, "MaterialLight");

            var fallbackLabel = ControlFactory.CreateLabel(
                "BusBuddy Dashboard - Fallback Mode\nSyncfusion controls available.\nBasic layout active.",
                EnhancedThemeService.GetSafeFont(12F, FontStyle.Bold),
                EnhancedThemeService.TextColor
            );
            fallbackLabel.Dock = DockStyle.Fill;
            fallbackLabel.AutoSize = false;
            fallbackPanel.Controls.Add(fallbackLabel);

            form.Controls.Add(fallbackPanel);
            return fallbackPanel;
        }
    }
}
