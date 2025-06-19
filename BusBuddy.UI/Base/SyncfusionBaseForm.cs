using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.WinForms.Controls;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Native Syncfusion base form using SfButton, SfTextBox, SfComboBox, and AutoLabel
    /// Provides standardized MaterialLight UI with proper theming and DPI support
    /// </summary>
    public class SyncfusionBaseForm : Form
    {
        protected readonly ErrorProvider _errorProvider;
        protected readonly DatabaseHelperService _databaseService;
        protected readonly BannerTextProvider _bannerTextProvider;

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
            _bannerTextProvider = new BannerTextProvider();

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
        }

        private void InitializeSyncfusionDesign()
        {
            // Load dark theme DLL if available
            SyncfusionThemeHelper.LoadDarkTheme();

            // Apply MaterialLight theme using SfSkinManager
            SfSkinManager.SetVisualStyle(this, "MaterialLight");
        }

        protected virtual void InitializeLayout()
        {
            // Create main panel with DPI-aware sizing and solid background
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = GetDpiAwarePadding(new Padding(20)),
                BackColor = EnhancedThemeService.SurfaceColor
            };
            this.Controls.Add(_mainPanel);

            // Create button panel with DPI-aware sizing and solid background
            _buttonPanel = new Panel
            {
                Height = GetDpiAwareHeight(60),
                Dock = DockStyle.Bottom,
                BackColor = EnhancedThemeService.SurfaceColor,
                Padding = GetDpiAwarePadding(new Padding(20, 10, 20, 10))
            };
            this.Controls.Add(_buttonPanel);
        }

        #endregion

        #region DPI Awareness Helpers

        protected Size GetDpiAwareSize(Size originalSize) => EnhancedThemeService.GetDpiAwareSize(originalSize, _dpiScale);
        protected Padding GetDpiAwarePadding(Padding originalPadding) => EnhancedThemeService.GetDpiAwarePadding(originalPadding, _dpiScale);
        protected int GetDpiAwareX(int x) => EnhancedThemeService.ScaleByDpi(x, _dpiScale);
        protected int GetDpiAwareY(int y) => EnhancedThemeService.ScaleByDpi(y, _dpiScale);
        protected int GetDpiAwareWidth(int width) => EnhancedThemeService.ScaleByDpi(width, _dpiScale);
        protected int GetDpiAwareHeight(int height) => EnhancedThemeService.ScaleByDpi(height, _dpiScale);

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Apply high DPI theming if necessary
            if (_isHighDpi) SyncfusionThemeHelper.ApplyHighDpiMaterialTheme(this);

            // Ensure MaterialLight theme is applied
            SfSkinManager.SetVisualStyle(this, "MaterialLight");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _errorProvider?.Dispose();
        }

        #endregion

        #region Validation and Messaging

        /// <summary>
        /// Override this method to implement form-specific validation
        /// </summary>
        protected virtual bool ValidateForm() => true;

        /// <summary>
        /// Clear all validation errors from the form
        /// </summary>
        protected virtual void ClearAllValidationErrors()
        {
            _errorProvider?.Clear();
        }

        /// <summary>
        /// Set validation error for a specific control
        /// </summary>
        protected virtual void SetValidationError(Control control, string message)
        {
            _errorProvider?.SetError(control, message);
        }

        /// <summary>
        /// Show validation errors to the user
        /// </summary>
        protected void ShowValidationErrors(List<string> errors)
        {
            if (errors?.Count > 0)
            {
                string message = "Please correct the following errors:\n\n" + string.Join("\n", errors);
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Show error message to the user
        /// </summary>
        protected void ShowErrorMessage(string message) => MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>
        /// Show success message to the user
        /// </summary>
        protected void ShowSuccessMessage(string message) => MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// Show confirmation dialog
        /// </summary>
        protected bool ConfirmDelete(string itemType) => MessageBox.Show($"Are you sure you want to delete this {itemType}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        #endregion
    }
}
