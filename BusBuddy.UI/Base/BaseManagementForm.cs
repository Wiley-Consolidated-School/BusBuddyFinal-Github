using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Services;
using BusBuddy.UI.Views;
using Syncfusion.WinForms.Controls;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.Windows.Forms.Tools;

namespace BusBuddy.UI.Base
{    /// <summary>
    /// Base class for all management forms to ensure consistency across the application
    /// Provides standard layout, controls, and behavior patterns
    /// </summary>
    public abstract class BaseManagementForm<T> : SyncfusionBaseForm, IDisposable where T : class
    {
        #region Protected Fields
        protected SfDataGrid? _dataGrid;
        protected SfButton? _addButton;
        protected SfButton? _editButton;
        protected SfButton? _deleteButton;
        protected SfButton? _detailsButton;
        protected SfButton? _searchButton;
        protected TextBoxExt? _searchBox;
        protected List<T> _entities = new List<T>();

        // Disposal support
        private bool _disposed = false;
        private readonly object _disposalLock = new object();

        // Message service for testable error handling
        protected readonly IMessageService _messageService;

        // Repository support
        private object? _repository;

        // Async operation support
        protected CancellationTokenSource? _cancellationTokenSource;
        private readonly object _cancellationLock = new object();
        #endregion

        #region Abstract Properties and Methods
        protected abstract string FormTitle { get; }
        protected abstract string SearchPlaceholder { get; }
        protected abstract string EntityName { get; }

        protected abstract void LoadDataFromRepository();
        protected abstract void AddNewEntity();
        protected abstract void EditSelectedEntity();
        protected abstract void DeleteSelectedEntity();
        protected abstract void ViewEntityDetails();
        protected abstract void SearchEntities();
        protected abstract void SetupDataGridColumns();

        // Async data loading support - optional override
        protected virtual async Task LoadDataFromRepositoryAsync(CancellationToken cancellationToken = default)
        {
            // Default implementation calls synchronous method
            await Task.Run(() => LoadDataFromRepository(), cancellationToken);
        }
        #endregion

        #region Constructor and Initialization
        protected BaseManagementForm() : this(new MessageBoxService())
        {
        }

        protected BaseManagementForm(IMessageService messageService)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            InitializeComponent();

            // Register this form with the shutdown manager for proper cleanup
            try
            {
                // Use reflection to find and call TestSafeApplicationShutdownManager.RegisterForm
                var shutdownManagerType = Type.GetType("BusBuddy.UI.Services.TestSafeApplicationShutdownManager");
                if (shutdownManagerType != null)
                {
                    var registerMethod = shutdownManagerType.GetMethod("RegisterForm",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    registerMethod?.Invoke(null, new object[] { this });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not register BaseManagementForm with shutdown manager: {ex.Message}");
            }
        }

        private void InitializeComponent()
        {
            // Standard form properties
            this.Text = FormTitle;
            this.ClientSize = GetDpiAwareSize(new Size(1200, 900));
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = GetDpiAwareSize(new Size(800, 600));

            CreateControls();
            LayoutControls();
            SetupEventHandlers();

            // Apply theming
            BusBuddyThemeManager.ApplyTheme(this, BusBuddyThemeManager.SupportedThemes.Office2016White);

            // CRITICAL FIX: Load data after all controls are initialized
            try
            {
                LoadDataAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error loading {FormTitle} data: {ex.Message}";
                _messageService.ShowError(errorMessage);
                _entities = new List<T>();

                // Throw a specific exception for test verification
                throw new InvalidOperationException(errorMessage, ex);
            }

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with standardized controls and data loaded");
        }
        #endregion

        #region Standard Control Creation
        protected virtual void CreateControls()
        {
            var buttonSize = GetDpiAwareSize(new Size(100, 35));

            // Standard CRUD buttons using ControlFactory for consistency
            _addButton = ControlFactory.CreateButton($"➕ Add New", buttonSize, (s, e) => AddNewEntity());
            _editButton = ControlFactory.CreateButton("✏️ Edit", buttonSize, (s, e) => EditSelectedEntity());
            _deleteButton = ControlFactory.CreateButton("🗑️ Delete", buttonSize, (s, e) => DeleteSelectedEntity());
            _detailsButton = ControlFactory.CreateButton("👁️ Details", buttonSize, (s, e) => ViewEntityDetails());
            _searchButton = ControlFactory.CreateButton("🔍 Search", GetDpiAwareSize(new Size(80, 35)), (s, e) => SearchEntities());

            // Standard search box using ControlFactory
            _searchBox = ControlFactory.CreateSearchBox(SearchPlaceholder);

            // Standard grid creation
            _dataGrid = BusBuddyThemeManager.CreateEnhancedMaterialSfDataGrid();
            if (_dataGrid != null)
            {
                BusBuddyThemeManager.SfDataGridEnhancements(_dataGrid);
                SetupDataGridColumns();
            }
            else if (BusBuddyThemeManager.IsTestMode)
            {
                Console.WriteLine("🧪 BaseManagementForm: DataGrid creation skipped - test mode enabled");
            }
        }
        #endregion

        #region Standard Layout
        protected virtual void LayoutControls()
        {
            var buttonY = GetDpiAwareY(20);

            // Standard button positioning with consistent 110px spacing
            if (_addButton != null) _addButton.Location = new Point(GetDpiAwareX(20), buttonY);
            if (_editButton != null) _editButton.Location = new Point(GetDpiAwareX(130), buttonY);
            if (_deleteButton != null) _deleteButton.Location = new Point(GetDpiAwareX(240), buttonY);
            if (_detailsButton != null) _detailsButton.Location = new Point(GetDpiAwareX(350), buttonY);

            // Search controls
            var searchLabel = ControlFactory.CreateLabel("🔍 Search:");
            if (searchLabel != null)
            {
                searchLabel.Location = new Point(GetDpiAwareX(500), GetDpiAwareY(25));
                this.Controls.Add(searchLabel);
            }

            if (_searchBox != null)
            {
                _searchBox.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));
                _searchBox.Size = GetDpiAwareSize(new Size(150, 30));
            }

            if (_searchButton != null) _searchButton.Location = new Point(GetDpiAwareX(710), buttonY);

            // Grid positioning - only if grid exists (not in test mode)
            if (_dataGrid != null)
            {
                _dataGrid.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
                _dataGrid.Size = GetDpiAwareSize(new Size(1150, 800));
                _dataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            }

            // Add controls to form - only add non-null controls
            var controlsToAdd = new List<Control>();
            if (_addButton != null) controlsToAdd.Add(_addButton);
            if (_editButton != null) controlsToAdd.Add(_editButton);
            if (_deleteButton != null) controlsToAdd.Add(_deleteButton);
            if (_detailsButton != null) controlsToAdd.Add(_detailsButton);
            if (_searchButton != null) controlsToAdd.Add(_searchButton);
            if (_searchBox != null) controlsToAdd.Add(_searchBox);
            if (_dataGrid != null) controlsToAdd.Add(_dataGrid);

            if (controlsToAdd.Count > 0)
            {
                this.Controls.AddRange(controlsToAdd.ToArray());
            }

            // Standard initial button state - edit/delete/details disabled
            if (_editButton != null) _editButton.Enabled = false;
            if (_deleteButton != null) _deleteButton.Enabled = false;
            if (_detailsButton != null) _detailsButton.Enabled = false;
        }
        #endregion

        #region Standard Event Handlers
        protected virtual void SetupEventHandlers()
        {
            if (_dataGrid != null)
            {
                _dataGrid.SelectionChanged += DataGrid_SelectionChanged;
                _dataGrid.CellDoubleClick += (s, e) => EditSelectedEntity();
            }

            // Search box Enter key handling
            if (_searchBox != null)
            {
                _searchBox.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        SearchEntities();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                };
            }
        }

        protected virtual void DataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var hasSelection = _dataGrid?.SelectedItems?.Count > 0;

            if (_editButton != null) _editButton.Enabled = hasSelection;
            if (_deleteButton != null) _deleteButton.Enabled = hasSelection;
            if (_detailsButton != null) _detailsButton.Enabled = hasSelection;
        }
        #endregion

        #region Helper Methods
        protected virtual T? GetSelectedEntity()
        {
            if (_dataGrid?.SelectedItems?.Count > 0)
            {
                var selectedItem = _dataGrid.SelectedItems[0];
                if (selectedItem is System.Data.DataRowView rowView)
                {
                    var idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty($"{EntityName}ID");
                    if (idProperty != null)
                    {
                        var idValue = Convert.ToInt32(rowView[idProperty.Name]);
                        return _entities.FirstOrDefault(e =>
                        {
                            var entityId = idProperty.GetValue(e);
                            return entityId != null && Convert.ToInt32(entityId) == idValue;
                        });
                    }
                }
            }
            return null;
        }

        protected virtual void RefreshGrid()
        {
            try
            {
                RefreshGridAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error refreshing data: {ex.Message}";
                _messageService.ShowError(errorMessage);
                _entities = new List<T>();

                // Throw a specific exception for test verification
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Async version of RefreshGrid with cancellation support
        /// </summary>
        protected virtual async Task RefreshGridAsync()
        {
            try
            {
                await LoadDataAsync();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"🚫 Data refresh cancelled for {FormTitle}");
                // Don't show error for cancellation, it's expected
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error refreshing data: {ex.Message}";
                _messageService.ShowError(errorMessage);
                _entities = new List<T>();
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Enhanced error handling that throws exceptions for testability
        /// </summary>
        protected virtual void HandleError(string message, string title = "Error", Exception? innerException = null)
        {
            _messageService.ShowError(message, title);
            throw new InvalidOperationException(message, innerException);
        }

        /// <summary>
        /// Enhanced confirmation handling that returns the user's choice
        /// </summary>
        protected virtual bool ConfirmAction(string message, string title = "Confirm")
        {
            return _messageService.ShowConfirmation(message, title);
        }

        /// <summary>
        /// Show information message through the message service
        /// </summary>
        protected virtual void ShowInfo(string message, string title = "Information")
        {
            _messageService.ShowInfo(message, title);
        }

        /// <summary>
        /// Show warning message through the message service
        /// </summary>
        protected virtual void ShowWarning(string message, string title = "Warning")
        {
            _messageService.ShowWarning(message, title);
        }
        #endregion

        #region Enhanced Shutdown and Disposal

        /// <summary>
        /// Enhanced FormClosing handler with comprehensive cleanup
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                Console.WriteLine($"🧽 BaseManagementForm closing: {this.GetType().Name}");

                // Cancel any pending async operations
                CancelCurrentOperation();

                // Clear data sources to prevent memory leaks
                ClearDataSources();

                // Dispose Syncfusion controls safely
                DisposeSyncfusionControlsSafely();

                // Clear event handlers to prevent memory leaks
                ClearEventHandlers();

                Console.WriteLine($"✅ BaseManagementForm cleanup completed: {this.GetType().Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during BaseManagementForm closing: {ex.Message}");
            }
            finally
            {
                base.OnFormClosing(e);
            }
        }

        /// <summary>
        /// Clear data sources from controls to prevent memory leaks
        /// </summary>
        private void ClearDataSources()
        {
            try
            {
                if (_dataGrid != null && !_dataGrid.IsDisposed)
                {
                    _dataGrid.DataSource = null;
                    Console.WriteLine("🧽 DataGrid data source cleared");
                }

                // Clear entities collection
                _entities?.Clear();
                Console.WriteLine("🧽 Entities collection cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error clearing data sources: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely dispose Syncfusion controls to prevent crashes
        /// </summary>
        private void DisposeSyncfusionControlsSafely()
        {
            try
            {
                // Dispose in reverse order to prevent dependency issues
                var controlsToDispose = new List<Control>();

                if (_dataGrid != null && !_dataGrid.IsDisposed) controlsToDispose.Add(_dataGrid);
                if (_searchBox != null && !_searchBox.IsDisposed) controlsToDispose.Add(_searchBox);
                if (_searchButton != null && !_searchButton.IsDisposed) controlsToDispose.Add(_searchButton);
                if (_detailsButton != null && !_detailsButton.IsDisposed) controlsToDispose.Add(_detailsButton);
                if (_deleteButton != null && !_deleteButton.IsDisposed) controlsToDispose.Add(_deleteButton);
                if (_editButton != null && !_editButton.IsDisposed) controlsToDispose.Add(_editButton);
                if (_addButton != null && !_addButton.IsDisposed) controlsToDispose.Add(_addButton);

                foreach (var control in controlsToDispose)
                {
                    try
                    {
                        // Suppress finalization to prevent disposal crashes
                        GC.SuppressFinalize(control);

                        // Remove from parent first
                        control.Parent?.Controls.Remove(control);

                        // Dispose the control
                        control.Dispose();
                        Console.WriteLine($"🧽 Disposed control: {control.GetType().Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error disposing control {control.GetType().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in DisposeSyncfusionControlsSafely: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear event handlers to prevent memory leaks
        /// </summary>
        private void ClearEventHandlers()
        {
            try
            {
                if (_dataGrid != null && !_dataGrid.IsDisposed)
                {
                    _dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                    Console.WriteLine("🧽 DataGrid event handlers cleared");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error clearing event handlers: {ex.Message}");
            }
        }

        /// <summary>
        /// Implement IDisposable pattern for proper resource cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_disposalLock)
                {
                    if (!_disposed)
                    {
                        try
                        {
                            if (disposing)
                            {
                                Console.WriteLine($"🧽 Disposing BaseManagementForm: {this.GetType().Name}");

                                // Clear data sources
                                ClearDataSources();

                                // Dispose Syncfusion controls
                                DisposeSyncfusionControlsSafely();

                                // Clear event handlers
                                ClearEventHandlers();

                                // Dispose managed resources
                                _entities?.Clear();
                                _entities = null;

                                Console.WriteLine($"✅ BaseManagementForm disposed: {this.GetType().Name}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error during BaseManagementForm disposal: {ex.Message}");
                        }
                        finally
                        {
                            _disposed = true;
                        }
                    }
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up
        /// </summary>
        ~BaseManagementForm()
        {
            Dispose(false);
        }

        #endregion

        #region Repository Support
        /// <summary>
        /// Set the repository instance for data operations
        /// </summary>
        protected void SetRepository(object repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get the repository instance for data operations
        /// </summary>
        protected virtual object? GetRepository()
        {
            return _repository;
        }

        /// <summary>
        /// Load data from the repository into the grid
        /// </summary>
        protected virtual void LoadData()
        {
            try
            {
                // Check if repository is initialized before attempting to load data
                if (_repository == null)
                {
                    // GRACEFUL DEGRADATION: Handle uninitialized repository gracefully
                    if (IsTestMode)
                    {
                        return;
                    }

                    // Instead of throwing exception, show user-friendly message and continue
                    _messageService.ShowWarning(
                        $"📊 {FormTitle}\n\n" +
                        "Database connection unavailable.\n" +
                        "The application is running in offline mode.",
                        "Offline Mode");
                    return;
                }

                // Call the derived class implementation to load data from repository
                LoadDataFromRepository();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"🚫 Data loading cancelled for {FormTitle}");
                // Don't show error for cancellation, it's expected
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error loading {FormTitle} data: {ex.Message}";
                _messageService.ShowError(errorMessage);
                _entities = new List<T>();

                // Throw a specific exception for test verification
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Async version of LoadData with cancellation support
        /// </summary>
        protected virtual async Task LoadDataAsync()
        {
            // Cancel any existing operation
            CancelCurrentOperation();

            lock (_cancellationLock)
            {
                _cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                // Check if repository is initialized before attempting to load data
                if (_repository == null)
                {
                    // GRACEFUL DEGRADATION: Handle uninitialized repository gracefully
                    if (IsTestMode)
                    {
                        return;
                    }

                    // Instead of throwing exception, show user-friendly message and continue
                    _messageService.ShowWarning(
                        $"📊 {FormTitle}\n\n" +
                        "Database connection unavailable.\n" +
                        "The application is running in offline mode.",
                        "Offline Mode");
                    return;
                }

                // Call the derived class implementation to load data from repository
                await LoadDataFromRepositoryAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"🚫 Data loading cancelled for {FormTitle}");
                // Don't show error for cancellation, it's expected
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error loading {FormTitle} data: {ex.Message}";
                _messageService.ShowError(errorMessage);
                _entities = new List<T>();

                // Throw a specific exception for test verification
                throw new InvalidOperationException(errorMessage, ex);
            }
            finally
            {
                lock (_cancellationLock)
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        /// <summary>
        /// Cancel any current async operation
        /// </summary>
        protected virtual void CancelCurrentOperation()
        {
            lock (_cancellationLock)
            {
                try
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
                catch (ObjectDisposedException)
                {
                    // Token source already disposed, ignore
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error cancelling operation: {ex.Message}");
                }
            }
        }
        #endregion

        #region Test Mode Support
        /// <summary>
        /// Determines if the application is running in test mode
        /// </summary>
        protected virtual bool IsTestMode
        {
            get
            {
                return Environment.CommandLine.Contains("testhost") ||
                       Environment.CommandLine.Contains("vstest") ||
                       AppDomain.CurrentDomain.FriendlyName.Contains("test", StringComparison.OrdinalIgnoreCase);
            }
        }
        #endregion
    }
}
