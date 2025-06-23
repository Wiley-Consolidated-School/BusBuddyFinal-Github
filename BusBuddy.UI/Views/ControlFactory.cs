using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.UI.Services;
using BusBuddy.UI.Helpers;
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
        #region Label Creation        /// <summary>
        /// Creates an AutoLabel with consistent styling and theming
        /// </summary>
        public static AutoLabel CreateLabel(string text, Font font = null, Color? foreColor = null, bool autoSize = true)
        {
            var autoLabel = new AutoLabel
            {
                Text = text,
                Font = font ?? BusBuddyThemeManager.Typography.GetDefaultFont(),
                ForeColor = foreColor ?? BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
                AutoSize = autoSize,
                BackColor = Color.Transparent
            };
            BusBuddyThemeManager.ApplyTheme(autoLabel, BusBuddyThemeManager.CurrentTheme);
            return autoLabel;
        }

        /// <summary>
        /// Creates a header label with larger font and accent color
        /// </summary>
        public static AutoLabel CreateHeaderLabel(string text)
        {
            return CreateLabel(text, BusBuddyThemeManager.Typography.GetHeaderFont(), BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme));
        }

        #endregion

        #region Button Creation        /// <summary>
        /// Creates a Syncfusion SfButton with consistent styling
        /// </summary>
        public static SfButton CreateButton(string text, Size size, EventHandler clickHandler = null)
        {
            var sfButton = new SfButton
            {
                Text = text,
                Size = size,
                Font = BusBuddyThemeManager.Typography.GetDefaultFont(),
                BackColor = BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = Color.White // Button text is typically white on colored backgrounds
            };
            if (clickHandler != null) sfButton.Click += clickHandler;
            BusBuddyThemeManager.ApplyTheme(sfButton, BusBuddyThemeManager.CurrentTheme);
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
            button.BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme);
            button.ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme);
            return button;
        }

        #endregion

        #region TextBox Creation        /// <summary>
        /// Creates a TextBoxExt with watermark (banner text) support
        /// </summary>
        public static TextBoxExt CreateTextBox(BannerTextProvider bannerProvider, string watermark, bool multiline = false)
        {
            var textBox = new TextBoxExt
            {
                Multiline = multiline,
                Size = new Size(200, multiline ? 60 : 30),
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
                BorderColor = Color.Gray, // Simple border color
                BorderStyle = BorderStyle.FixedSingle
            };

            // Apply theme using the theme manager
            BusBuddyThemeManager.ApplyTheme(textBox, BusBuddyThemeManager.CurrentTheme);

            var bannerTextInfo = new BannerTextInfo();
            bannerTextInfo.Text = watermark;
            bannerTextInfo.Visible = true;
            bannerTextInfo.Font = BusBuddyThemeManager.Typography.GetDefaultFont();
            bannerTextInfo.Color = Color.Gray; // Secondary text color
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
            textBox.BorderColor = Color.Orange; // Warning color
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

        #region ComboBox Creation        /// <summary>
        /// Creates a Syncfusion ComboBoxAdv with consistent styling
        /// </summary>
        public static ComboBoxAdv CreateComboBox(object dataSource = null)
        {
            var comboBox = new ComboBoxAdv
            {
                Size = new Size(200, 30),
                Font = BusBuddyThemeManager.Typography.GetDefaultFont(),
                DataSource = dataSource
            };
            BusBuddyThemeManager.ApplyTheme(comboBox, BusBuddyThemeManager.CurrentTheme);
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

        #region DateTimePicker Creation        /// <summary>
        /// Creates a Syncfusion SfDateTimeEdit control with consistent styling
        /// </summary>
        public static SfDateTimeEdit CreateDateTimePicker()
        {
            var dateTimePicker = new SfDateTimeEdit
            {
                Size = new Size(200, 30),
                Font = BusBuddyThemeManager.Typography.GetDefaultFont(),
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
            };
            BusBuddyThemeManager.ApplyTheme(dateTimePicker, BusBuddyThemeManager.CurrentTheme);
            return dateTimePicker;
        }

        #endregion

        #region NumericTextBox Creation        /// <summary>
        /// Creates a Syncfusion SfNumericTextBox with consistent styling and value range
        /// </summary>
        public static SfNumericTextBox CreateNumericTextBox(double minValue, double maxValue)
        {
            var numericTextBox = new SfNumericTextBox
            {
                Size = new Size(200, 30),
                Font = BusBuddyThemeManager.Typography.GetDefaultFont(),
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
                MinValue = minValue,
                MaxValue = maxValue,
                Value = minValue
            };
            BusBuddyThemeManager.ApplyTheme(numericTextBox, BusBuddyThemeManager.CurrentTheme);
            return numericTextBox;
        }

        #endregion

        #region DataGrid Creation        /// <summary>
        /// Creates a Syncfusion SfDataGrid with consistent styling and basic configuration
        /// </summary>
        public static Syncfusion.WinForms.DataGrid.SfDataGrid CreateDataGrid()
        {
            var dataGrid = new Syncfusion.WinForms.DataGrid.SfDataGrid
            {
                Font = BusBuddyThemeManager.Typography.GetDefaultFont(),
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
                AllowEditing = false,
                AllowSorting = true,
                AllowFiltering = true,
                AutoGenerateColumns = false, // Fix excessive columns issue - manually configure columns per grid
                HeaderRowHeight = 35,
                RowHeight = 30
            };
            BusBuddyThemeManager.ApplyTheme(dataGrid, BusBuddyThemeManager.CurrentTheme);
            return dataGrid;
        }

        #endregion

        #region Utility Methods        /// <summary>
        /// Creates a standard Panel with consistent theming
        /// </summary>
        public static Panel CreatePanel(Color? backgroundColor = null)
        {
            var panel = new Panel
            {
                BackColor = backgroundColor ?? BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme)
            };
            BusBuddyThemeManager.ApplyTheme(panel, BusBuddyThemeManager.CurrentTheme);
            return panel;
        }

        #endregion
    }
}
