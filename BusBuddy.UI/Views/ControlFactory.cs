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
        /// <summary>
        /// Test mode flag to prevent UI control creation during unit tests
        /// </summary>
        private static bool _testMode = false;

        /// <summary>
        /// Enable test mode to prevent UI control instantiation
        /// </summary>
        public static void EnableTestMode()
        {
            _testMode = true;
            Console.WriteLine("🧪 ControlFactory: Test mode enabled - UI controls will not be created");
        }

        /// <summary>
        /// Disable test mode to allow normal UI control creation
        /// </summary>
        public static void DisableTestMode()
        {
            _testMode = false;
            Console.WriteLine("🎨 ControlFactory: Test mode disabled - UI controls enabled");
        }

        /// <summary>
        /// Check if currently in test mode
        /// </summary>
        public static bool IsTestMode => _testMode;

        #region Label Creation        /// <summary>
        /// Creates an AutoLabel with consistent styling and theming
        /// </summary>
        public static AutoLabel CreateLabel(string text, Font font = null, Color? foreColor = null, bool autoSize = true)
        {
            if (_testMode)
            {
                Console.WriteLine($"🧪 ControlFactory: Skipping AutoLabel creation - test mode enabled");
                return null;
            }

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
            if (_testMode)
            {
                Console.WriteLine($"🧪 ControlFactory: Skipping HeaderLabel creation - test mode enabled");
                return null;
            }

            return CreateLabel(text, BusBuddyThemeManager.Typography.GetHeaderFont(), BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme));
        }

        #endregion

        #region Button Creation        /// <summary>
        /// Creates a Syncfusion SfButton with consistent styling
        /// </summary>
        public static SfButton CreateButton(string text, Size size, EventHandler clickHandler = null)
        {
            if (_testMode)
            {
                Console.WriteLine($"🧪 ControlFactory: Skipping SfButton creation - test mode enabled");
                return null;
            }

            // Reference: https://help.syncfusion.com/windowsforms/button/getting-started
            var sfButton = new SfButton
            {
                Text = text,
                Size = size,
                Font = BusBuddyThemeManager.Typography.GetDefaultFont()
            };

            // Apply theme first to ensure visual style is initialized
            BusBuddyThemeManager.ApplyTheme(sfButton, BusBuddyThemeManager.CurrentTheme);

            // Now safely set colors after theme is applied
            try
            {
                sfButton.BackColor = BusBuddyThemeManager.ThemeColors.GetPrimaryColor(BusBuddyThemeManager.CurrentTheme);
                sfButton.ForeColor = Color.White; // Button text is typically white on colored backgrounds
            }
            catch (NullReferenceException)
            {
                // Fallback to default colors if Syncfusion visual style isn't ready
                Console.WriteLine("⚠️ SfButton visual style not ready, using default colors");
                // Don't set BackColor/ForeColor - let Syncfusion use defaults
            }

            if (clickHandler != null) sfButton.Click += clickHandler;
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
        /// Creates a TextBoxExt with watermark support using PlaceholderText (BannerTextProvider removed)
        /// </summary>
        public static TextBoxExt CreateTextBox(string watermark, bool multiline = false)
        {
            if (_testMode)
            {
                Console.WriteLine($"🧪 ControlFactory: Skipping TextBox creation - test mode enabled");
                return null;
            }

            var textBox = new TextBoxExt
            {
                Multiline = multiline,
                Size = new Size(200, multiline ? 60 : 30),
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                ForeColor = BusBuddyThemeManager.ThemeColors.GetTextColor(BusBuddyThemeManager.CurrentTheme),
                BorderColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Use PlaceholderText instead of BannerTextProvider
            if (!string.IsNullOrEmpty(watermark))
            {
                textBox.PlaceholderText = watermark;
            }

            // Apply theme using the theme manager
            BusBuddyThemeManager.ApplyTheme(textBox, BusBuddyThemeManager.CurrentTheme);

            return textBox;
        }

        /// <summary>
        /// Creates a required field TextBoxExt with warning color border
        /// </summary>
        public static TextBoxExt CreateRequiredTextBox(string watermark)
        {
            var textBox = CreateTextBox($"{watermark} *");
            textBox.BorderColor = Color.Orange; // Warning color
            return textBox;
        }

        /// <summary>
        /// Creates a search TextBoxExt with watermark for consistent search functionality
        /// </summary>
        public static TextBoxExt CreateSearchBox(string placeholder = "Search...")
        {
            return CreateTextBox(placeholder, false);
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
            // Reference: https://help.syncfusion.com/windowsforms/datetime-picker/getting-started
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
            // Reference: https://help.syncfusion.com/windowsforms/numeric-textbox/getting-started
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

        #region ListView Creation

        /// <summary>
        /// Creates a Syncfusion SfListView with consistent styling and basic configuration
        /// </summary>
        public static SfListView CreateListView(object dataSource = null)
        {
            if (_testMode)
            {
                Console.WriteLine($"🧪 ControlFactory: Skipping SfListView creation - test mode enabled");
                return null;
            }

            // Reference: https://help.syncfusion.com/windowsforms/listview/getting-started
            var listView = new SfListView
            {
                BackColor = BusBuddyThemeManager.ThemeColors.GetBackgroundColor(BusBuddyThemeManager.CurrentTheme),
                DataSource = dataSource,
                Font = BusBuddyThemeManager.Typography.GetDefaultFont()
            };

            // Set selection mode - using One which is the standard Single mode
            try
            {
                listView.SelectionMode = SelectionMode.One;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting SelectionMode: {ex.Message}");
            }

            BusBuddyThemeManager.ApplyTheme(listView, BusBuddyThemeManager.CurrentTheme);
            return listView;
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
