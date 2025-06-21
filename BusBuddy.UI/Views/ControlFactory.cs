using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.UI.Services;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.ListView;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Input;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Factory class for creating Syncfusion-based UI controls with consistent styling
    /// Follows Syncfusion best practices for Windows Forms controls with Material Light theme
    /// </summary>
    public static class ControlFactory
    {
        #region Label Creation

        /// <summary>
        /// Creates an AutoLabel with consistent styling and theming
        /// </summary>
        public static AutoLabel CreateLabel(string text, Font font = null, Color? foreColor = null, bool autoSize = true)
        {
            var autoLabel = new AutoLabel
            {
                Text = text,
                Font = font ?? EnhancedThemeService.DefaultFont,
                ForeColor = foreColor ?? EnhancedThemeService.TextColor,
                AutoSize = autoSize,
                BackColor = Color.Transparent
            };
            SfSkinManager.SetVisualStyle(autoLabel, "MaterialLight");
            return autoLabel;
        }

        /// <summary>
        /// Creates a header label with larger font and accent color
        /// </summary>
        public static AutoLabel CreateHeaderLabel(string text)
        {
            return CreateLabel(text, EnhancedThemeService.HeaderFont, EnhancedThemeService.HeaderColor);
        }

        #endregion

        #region Button Creation

        /// <summary>
        /// Creates a Syncfusion SfButton with consistent styling
        /// </summary>
        public static SfButton CreateButton(string text, Size size, EventHandler clickHandler = null)
        {
            var sfButton = new SfButton
            {
                Text = text,
                Size = size,
                Font = EnhancedThemeService.ButtonFont,
                BackColor = EnhancedThemeService.ButtonColor,
                ForeColor = EnhancedThemeService.ButtonTextColor
            };
            if (clickHandler != null) sfButton.Click += clickHandler;
            SfSkinManager.SetVisualStyle(sfButton, "MaterialLight");
            return sfButton;
        }

        /// <summary>
        /// Creates a primary action button with accent color
        /// </summary>
        public static SfButton CreatePrimaryButton(string text, EventHandler clickHandler = null)
        {
            return CreateButton(text, new Size(120, 35), clickHandler);
        }

        /// <summary>
        /// Creates a secondary action button with neutral styling
        /// </summary>
        public static SfButton CreateSecondaryButton(string text, EventHandler clickHandler = null)
        {
            var button = CreateButton(text, new Size(120, 35), clickHandler);
            button.BackColor = EnhancedThemeService.SurfaceColor;
            button.ForeColor = EnhancedThemeService.TextColor;
            return button;
        }

        #endregion

        #region TextBox Creation

        /// <summary>
        /// Creates a TextBoxExt with watermark (banner text) support
        /// </summary>
        public static TextBoxExt CreateTextBox(BannerTextProvider bannerProvider, string watermark, bool multiline = false)
        {
            var textBox = new TextBoxExt
            {
                Multiline = multiline,
                Size = new Size(200, multiline ? 60 : 30),
                BackColor = EnhancedThemeService.SurfaceColor,
                ForeColor = EnhancedThemeService.TextColor,
                BorderColor = EnhancedThemeService.BorderColor,
                BorderStyle = BorderStyle.FixedSingle
            };

            // THREAD SAFETY: Set theme name safely to prevent collection modification errors
            try
            {
                textBox.ThemeName = "MaterialLight";
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Collection was modified"))
            {
                Console.WriteLine($"⚠️ THEME: Collection modified during theme setting, retrying...");
                // Retry once after a brief delay
                System.Threading.Thread.Sleep(10);
                try
                {
                    textBox.ThemeName = "MaterialLight";
                }
                catch (Exception retryEx)
                {
                    Console.WriteLine($"⚠️ THEME: Retry failed, using default theme: {retryEx.Message}");
                    // Continue without theme - control will use default styling
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ THEME: Error setting TextBoxExt theme: {ex.Message}");
            }

            var bannerTextInfo = new BannerTextInfo();
            bannerTextInfo.Text = watermark;
            bannerTextInfo.Visible = true;
            bannerTextInfo.Font = EnhancedThemeService.DefaultFont;
            bannerTextInfo.Color = EnhancedThemeService.SecondaryTextColor;
            bannerTextInfo.Mode = BannerTextMode.EditMode;
            bannerProvider?.SetBannerText(textBox, bannerTextInfo);

            return textBox;
        }

        /// <summary>
        /// Creates a required field TextBoxExt with warning color border
        /// </summary>
        public static TextBoxExt CreateRequiredTextBox(BannerTextProvider bannerProvider, string watermark)
        {
            var textBox = CreateTextBox(bannerProvider, $"{watermark} *");
            textBox.BorderColor = EnhancedThemeService.WarningColor;
            return textBox;
        }

        /// <summary>
        /// Creates a search TextBoxExt with watermark for consistent search functionality
        /// </summary>
        public static TextBoxExt CreateSearchBox(string placeholder = "Search...")
        {
            var bannerProvider = new BannerTextProvider();
            return CreateTextBox(bannerProvider, placeholder, false);
        }

        #endregion

        #region ComboBox Creation

        /// <summary>
        /// Creates a Syncfusion ComboBoxAdv with consistent styling
        /// </summary>
        public static ComboBoxAdv CreateComboBox(object dataSource = null)
        {
            var comboBox = new ComboBoxAdv
            {
                Size = new Size(200, 30),
                Font = EnhancedThemeService.DefaultFont,
                DataSource = dataSource
            };
            comboBox.ThemeName = "MaterialLight";
            return comboBox;
        }

        /// <summary>
        /// Creates a status ComboBox with predefined status values
        /// </summary>
        public static ComboBoxAdv CreateStatusComboBox()
        {
            return CreateComboBox(new[] { "Active", "Inactive", "Suspended", "Pending" });
        }

        #endregion

        #region DateTimePicker Creation

        /// <summary>
        /// Creates a Syncfusion SfDateTimeEdit control with consistent styling
        /// </summary>
        public static SfDateTimeEdit CreateDateTimePicker()
        {
            var dateTimePicker = new SfDateTimeEdit
            {
                Size = new Size(200, 30),
                Font = EnhancedThemeService.DefaultFont,
                ThemeName = "MaterialLight",
                BackColor = EnhancedThemeService.SurfaceColor,
                ForeColor = EnhancedThemeService.TextColor,
            };
            return dateTimePicker;
        }

        #endregion

        #region NumericTextBox Creation

        /// <summary>
        /// Creates a Syncfusion SfNumericTextBox with consistent styling and value range
        /// </summary>
        public static SfNumericTextBox CreateNumericTextBox(double minValue, double maxValue)
        {
            var numericTextBox = new SfNumericTextBox
            {
                Size = new Size(200, 30),
                Font = EnhancedThemeService.DefaultFont,
                ThemeName = "MaterialLight",
                BackColor = EnhancedThemeService.SurfaceColor,
                ForeColor = EnhancedThemeService.TextColor,
                MinValue = minValue,
                MaxValue = maxValue,
                Value = minValue
            };
            return numericTextBox;
        }

        #endregion

        #region DataGrid Creation

        /// <summary>
        /// Creates a Syncfusion SfDataGrid with consistent styling and basic configuration
        /// </summary>
        public static Syncfusion.WinForms.DataGrid.SfDataGrid CreateDataGrid()
        {
            var dataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid
            {
                Font = EnhancedThemeService.DefaultFont,
                BackColor = EnhancedThemeService.SurfaceColor,
                ForeColor = EnhancedThemeService.TextColor,
                AllowEditing = false,
                AllowSorting = true,
                AllowFiltering = true,
                AutoGenerateColumns = false, // Fix excessive columns issue - manually configure columns per grid
                HeaderRowHeight = 35,
                RowHeight = 30
            };
            Syncfusion.WinForms.Controls.SfSkinManager.SetVisualStyle(dataGrid, "MaterialLight");
            return dataGrid;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Creates a standard Panel with consistent theming
        /// </summary>
        public static Panel CreatePanel(Color? backgroundColor = null)
        {
            var panel = new Panel
            {
                BackColor = backgroundColor ?? EnhancedThemeService.SurfaceColor
            };
            Syncfusion.WinForms.Controls.SfSkinManager.SetVisualStyle(panel, "MaterialLight");
            return panel;
        }

        #endregion
    }
}
