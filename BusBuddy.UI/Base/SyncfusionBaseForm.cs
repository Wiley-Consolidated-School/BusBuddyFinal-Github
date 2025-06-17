using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Syncfusion-based replacement for StandardDataForm
    /// Provides standardized Material Design UI using Syncfusion controls instead of MaterialSkin2
    /// </summary>
    public class SyncfusionBaseForm : Form
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly DatabaseHelperService _databaseService;

        // Common UI elements
        protected Panel _mainPanel;
        protected Panel _buttonPanel;

        // Theme and DPI support
        protected float _dpiScale;
        protected bool _isHighDpi;

        public SyncfusionBaseForm()
        {
            // Initialize services
            _errorProvider = new ErrorProvider();
            _databaseService = new DatabaseHelperService();

            // Initialize DPI awareness
            InitializeDpiAwareness();

            // Initialize Syncfusion theming
            InitializeSyncfusionDesign();

            // Set common form properties
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Size = GetDpiAwareSize(new Size(800, 600));

            // Initialize layout
            InitializeLayout();
        }

        #region Initialization

        private void InitializeDpiAwareness()
        {
            _dpiScale = SyncfusionThemeHelper.HighDpiSupport.GetDpiScale(this);
            _isHighDpi = SyncfusionThemeHelper.HighDpiSupport.IsHighDpiMode(this);

            Console.WriteLine($"🔍 SYNCFUSION FORM: DPI Scale: {_dpiScale:F2}x, High DPI: {_isHighDpi}");
        }

        private void InitializeSyncfusionDesign()
        {
            // Load dark theme DLL if available
            SyncfusionThemeHelper.LoadDarkTheme();

            // Apply current theme to the form
            SyncfusionThemeHelper.ApplyCurrentTheme(this);

            Console.WriteLine("🎨 SYNCFUSION FORM: Current theme applied");
        }

        protected virtual void InitializeLayout()
        {
            // Create main panel with DPI-aware sizing
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = GetDpiAwarePadding(new Padding(20)),
                BackColor = Color.Transparent
            };
            SyncfusionThemeHelper.ApplyMaterialPanel(_mainPanel);
            this.Controls.Add(_mainPanel);

            // Create button panel with DPI-aware sizing
            _buttonPanel = new Panel
            {
                Height = GetDpiAwareHeight(60),
                Dock = DockStyle.Bottom,
                BackColor = Color.Transparent,
                Padding = GetDpiAwarePadding(new Padding(20, 10, 20, 10))
            };
            SyncfusionThemeHelper.ApplyMaterialPanel(_buttonPanel);
            this.Controls.Add(_buttonPanel);
        }

        #endregion

        #region Control Creation Helpers        /// <summary>
        /// Create a Syncfusion Material Design label
        /// Replaces MaterialLabel creation
        /// </summary>
        protected Control CreateLabel(string text, int x, int y)
        {
            var label = SyncfusionThemeHelper.CreateMaterialLabel(text,
                GetDpiAwareX(x), GetDpiAwareY(y));
            _mainPanel.Controls.Add(label);
            return label;
        }

        /// <summary>
        /// Create a Syncfusion Material Design text box
        /// Replaces MaterialTextBox creation
        /// </summary>
        protected Control CreateTextBox(int x, int y, int width = 200)
        {
            var textBox = SyncfusionThemeHelper.CreateMaterialTextBox(
                GetDpiAwareX(x), GetDpiAwareY(y), GetDpiAwareWidth(width));
            _mainPanel.Controls.Add(textBox);
            return textBox;
        }

        /// <summary>
        /// Create a Syncfusion Material Design button
        /// Replaces MaterialButton creation
        /// </summary>
        protected Control CreateButton(string text, int x, int y, int width = 120)
        {
            var button = SyncfusionThemeHelper.CreateMaterialButton(text,
                GetDpiAwareX(x), GetDpiAwareY(y), GetDpiAwareWidth(width));
            _buttonPanel.Controls.Add(button);
            return button;
        }

        /// <summary>
        /// Create a Material Design styled data grid
        /// </summary>
        protected DataGridView CreateDataGrid()
        {
            var dataGrid = SyncfusionThemeHelper.CreateMaterialDataGrid();
            return dataGrid;
        }

        #endregion

        #region DPI Awareness Helpers

        protected Size GetDpiAwareSize(Size originalSize)
        {
            return SyncfusionThemeHelper.HighDpiSupport.GetDpiAwareSize(originalSize, _dpiScale);
        }

        protected Padding GetDpiAwarePadding(Padding originalPadding)
        {
            return SyncfusionThemeHelper.HighDpiSupport.GetDpiAwarePadding(originalPadding, _dpiScale);
        }

        protected int GetDpiAwareX(int x) => (int)(x * _dpiScale);
        protected int GetDpiAwareY(int y) => (int)(y * _dpiScale);
        protected int GetDpiAwareWidth(int width) => (int)(width * _dpiScale);
        protected int GetDpiAwareHeight(int height) => (int)(height * _dpiScale);

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Apply high-DPI theming after load
            if (_isHighDpi)
            {
                SyncfusionThemeHelper.ApplyHighDpiMaterialTheme(this);
            }

            // Apply theme recursively to all controls
            SyncfusionThemeHelper.ApplyMaterialThemeRecursive(this);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.GetType().Name} loaded with Material theme");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Clean up resources
            _errorProvider?.Dispose();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply Material theme to all controls on the form
        /// Call this after adding new controls
        /// </summary>
        public void RefreshMaterialTheme()
        {
            SyncfusionThemeHelper.ApplyMaterialThemeRecursive(this);
        }

        /// <summary>
        /// Get current DPI information for debugging
        /// </summary>
        public string GetDpiInfo()
        {
            return $"DPI Scale: {_dpiScale:F2}x, Description: {SyncfusionThemeHelper.HighDpiSupport.GetDpiDescription(this)}";
        }

        /// <summary>
        /// Switch the form theme dynamically
        /// </summary>
        public virtual void SwitchTheme(SyncfusionThemeHelper.ThemeMode newTheme)
        {
            SyncfusionThemeHelper.SetThemeMode(newTheme);
            SyncfusionThemeHelper.ApplyCurrentTheme(this);

            // Refresh child forms if this is a parent form
            RefreshMaterialTheme();

            Console.WriteLine($"🔄 Theme switched to: {newTheme}");
        }

        /// <summary>
        /// Get current theme information
        /// </summary>
        public virtual string GetThemeInfo()
        {
            var info = $"Theme: {SyncfusionThemeHelper.GetCurrentThemeMode()}, DPI: {_dpiScale:F2}x";
            if (SyncfusionThemeHelper.IsDarkThemeDllLoaded())
            {
                info += " (Dark Theme DLL Available)";
            }
            return info;
        }

        #endregion
    }
}
