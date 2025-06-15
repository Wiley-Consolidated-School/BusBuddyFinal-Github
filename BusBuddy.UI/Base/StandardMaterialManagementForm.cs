using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using BusBuddy.UI.Base;
using BusBuddy.UI.Components;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using BusBuddy.UI.Extensions;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Standardized Material Design management form base class
    /// Provides consistent UI layout and functionality for all management forms
    /// </summary>
    public abstract class StandardMaterialManagementForm<T> : StandardMaterialForm where T : class
    {
        #region Protected Fields

        protected TableLayoutPanel _mainLayout;
        protected MaterialCard _headerCard;
        protected MaterialCard _contentCard;
        protected MaterialCard _actionCard;

        protected MaterialLabel _titleLabel;
        protected MaterialLabel _subtitleLabel;
        protected Panel _statusPanel;

        protected Panel _toolbarPanel;
        protected MaterialButton _addButton;
        protected MaterialButton _editButton;
        protected MaterialButton _deleteButton;
        protected MaterialButton _refreshButton;
        protected MaterialButton _exportButton;
        protected MaterialTextBox _searchTextBox;

        protected DataGridView _dataGrid;
        protected MaterialEditPanel _editPanel;

        protected List<T> _allItems;
        protected List<T> _filteredItems;
        protected T _currentItem;
        protected bool _isEditing = false;

        // Loading indicator
        protected MaterialProgressBar _loadingBar;
        protected MaterialLabel _loadingLabel;
        protected Panel _loadingPanel;

        #endregion

        #region Abstract Properties

        /// <summary>
        /// The title displayed at the top of the form
        /// </summary>
        protected abstract string FormTitle { get; }

        /// <summary>
        /// The subtitle displayed below the title
        /// </summary>
        protected abstract string FormSubtitle { get; }

        /// <summary>
        /// Text for the add button
        /// </summary>
        protected abstract string AddButtonText { get; }

        /// <summary>
        /// Text for the edit button
        /// </summary>
        protected abstract string EditButtonText { get; }

        /// <summary>
        /// Text for the delete button
        /// </summary>
        protected abstract string DeleteButtonText { get; }

        /// <summary>
        /// Placeholder text for the search box
        /// </summary>
        protected abstract string SearchHintText { get; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when an item is selected in the grid
        /// </summary>
        public event EventHandler<T> ItemSelected;

        /// <summary>
        /// Raised when data is refreshed
        /// </summary>
        public event EventHandler DataRefreshed;

        #endregion

        #region Constructor

        protected StandardMaterialManagementForm()
        {
            _allItems = new List<T>();
            _filteredItems = new List<T>();

            InitializeStandardComponents();
            SetupEventHandlers();
            ConfigureTooltips();
        }

        #endregion

        #region Initialization

        private void InitializeStandardComponents()
        {
            this.SuspendLayout();

            // Create main layout
            CreateMainLayout();

            // Create header section
            CreateHeaderSection();

            // Create content section
            CreateContentSection();

            // Create action section
            CreateActionSection();

            // Create loading overlay
            CreateLoadingOverlay();

            // Apply Material Design styling
            ApplyMaterialStyling();

            this.ResumeLayout(false);
        }

        private void CreateMainLayout()
        {
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.Transparent
            };

            // Configure row styles for responsive layout
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actions

            this.Controls.Add(_mainLayout);
        }

        private void CreateHeaderSection()
        {
            _headerCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 16, 24, 16),
                Margin = new Padding(8),
                BackColor = MaterialDesignThemeManager.DarkTheme.Primary
            };

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };

            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));            // Title
            _titleLabel = new MaterialLabel
            {
                Text = FormTitle,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8),
                Font = new System.Drawing.Font("Roboto", 16, System.Drawing.FontStyle.Bold)
            };

            // Subtitle
            _subtitleLabel = new MaterialLabel
            {
                Text = FormSubtitle,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                Font = new System.Drawing.Font("Roboto", 10, System.Drawing.FontStyle.Regular)
            };

            // Status panel for indicators
            _statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(8)
            };

            headerLayout.Controls.Add(_titleLabel, 0, 0);
            headerLayout.Controls.Add(_subtitleLabel, 0, 1);
            headerLayout.Controls.Add(_statusPanel, 1, 0);
            headerLayout.SetRowSpan(_statusPanel, 2);

            _headerCard.Controls.Add(headerLayout);
            _mainLayout.Controls.Add(_headerCard, 0, 0);

            // Add status indicators
            AddStatusIndicators();
        }

        private void CreateContentSection()
        {
            _contentCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(8, 4, 8, 4),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface
            };

            var contentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent
            };

            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Toolbar
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Data grid

            // Create toolbar
            CreateToolbar();
            contentLayout.Controls.Add(_toolbarPanel, 0, 0);

            // Create data grid
            CreateDataGrid();
            contentLayout.Controls.Add(_dataGrid, 0, 1);

            _contentCard.Controls.Add(contentLayout);
            _mainLayout.Controls.Add(_contentCard, 0, 1);
        }

        private void CreateActionSection()
        {
            _actionCard = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                Margin = new Padding(8),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface,
                Visible = false // Hidden by default
            };

            // Create edit panel
            _editPanel = new MaterialEditPanel
            {
                Dock = DockStyle.Fill,
                PanelTitle = "Edit Item"
            };

            _editPanel.SaveClicked += EditPanel_SaveClicked;
            _editPanel.CancelClicked += EditPanel_CancelClicked;

            _actionCard.Controls.Add(_editPanel);
            _mainLayout.Controls.Add(_actionCard, 0, 2);

            // Configure edit panel fields
            ConfigureEditPanelFields();
        }

        private void CreateToolbar()
        {
            _toolbarPanel = new Panel
            {
                Height = 56,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 16)
            };

            var toolbarLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };

            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            toolbarLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Action buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0)
            };

            // Create action buttons
            CreateActionButtons();
            buttonsPanel.Controls.AddRange(new Control[] { _addButton, _editButton, _deleteButton, _refreshButton, _exportButton });

            // Search panel
            var searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _searchTextBox = new MaterialTextBox
            {
                Hint = SearchHintText,
                Width = 300,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Margin = new Padding(8, 8, 0, 8)
            };

            _searchTextBox.Location = new Point(searchPanel.Width - _searchTextBox.Width - 8, 8);
            searchPanel.Controls.Add(_searchTextBox);

            toolbarLayout.Controls.Add(buttonsPanel, 0, 0);
            toolbarLayout.Controls.Add(searchPanel, 1, 0);

            _toolbarPanel.Controls.Add(toolbarLayout);
        }

        private void CreateActionButtons()
        {
            var buttonSize = new Size(120, 36);
            var margin = new Padding(0, 8, 12, 8);            _addButton = new MaterialButton
            {
                Text = AddButtonText,
                Size = buttonSize,
                UseAccentColor = true,
                Type = MaterialButton.MaterialButtonType.Contained,
                AutoSize = false,
                Margin = margin
                // Icon = Properties.Resources.add_icon // You'll need to add icon resources
            };

            _editButton = new MaterialButton
            {
                Text = EditButtonText,
                Size = buttonSize,
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = false,
                Margin = margin,
                Enabled = false
            };

            _deleteButton = new MaterialButton
            {
                Text = DeleteButtonText,
                Size = buttonSize,
                Type = MaterialButton.MaterialButtonType.Outlined,
                AutoSize = false,
                Margin = margin,
                Enabled = false
            };

            _refreshButton = new MaterialButton
            {
                Text = "Refresh",
                Size = buttonSize,
                Type = MaterialButton.MaterialButtonType.Text,
                AutoSize = false,
                Margin = margin
            };

            _exportButton = new MaterialButton
            {
                Text = "Export",
                Size = buttonSize,
                Type = MaterialButton.MaterialButtonType.Text,
                AutoSize = false,
                Margin = margin
            };
        }

        private void CreateDataGrid()
        {
            _dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = MaterialDesignThemeManager.DarkTheme.Surface,
                GridColor = MaterialDesignThemeManager.DarkTheme.OnSurface,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = MaterialDesignThemeManager.DarkTheme.Surface,
                    ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface,
                    Font = DpiScaleHelper.CreateFont(SystemFonts.DefaultFont.FontFamily, 9f, FontStyle.Regular, this)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = MaterialDesignThemeManager.DarkTheme.Primary,
                    ForeColor = MaterialDesignThemeManager.DarkTheme.OnPrimary,
                    Font = DpiScaleHelper.CreateFont(SystemFonts.DefaultFont.FontFamily, 9f, FontStyle.Bold, this)
                },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            // Configure grid columns
            ConfigureDataGridColumns();
        }

        private void CreateLoadingOverlay()
        {
            _loadingPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(128, 0, 0, 0), // Semi-transparent overlay
                Visible = false
            };

            var loadingCard = new MaterialCard
            {
                Size = new Size(200, 100),
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface,
                Anchor = AnchorStyles.None
            };

            loadingCard.Location = new Point(
                (_loadingPanel.Width - loadingCard.Width) / 2,
                (_loadingPanel.Height - loadingCard.Height) / 2
            );

            _loadingBar = new MaterialProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Dock = DockStyle.Top,
                Height = 4
            };

            _loadingLabel = new MaterialLabel
            {
                Text = "Loading...",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Roboto", 10, System.Drawing.FontStyle.Regular)
            };

            loadingCard.Controls.Add(_loadingLabel);
            loadingCard.Controls.Add(_loadingBar);
            _loadingPanel.Controls.Add(loadingCard);

            this.Controls.Add(_loadingPanel);
            _loadingPanel.BringToFront();
        }

        #endregion

        #region Event Handlers Setup

        private void SetupEventHandlers()
        {
            // Button events
            _addButton.Click += (s, e) => AddNewItem();
            _editButton.Click += (s, e) => EditSelectedItem();
            _deleteButton.Click += (s, e) => DeleteSelectedItem();
            _refreshButton.Click += (s, e) => RefreshData();
            _exportButton.Click += (s, e) => ExportData();

            // Search events
            _searchTextBox.TextChanged += (s, e) => FilterData();

            // Grid events
            _dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            _dataGrid.CellDoubleClick += (s, e) => EditSelectedItem();

            // Form events
            this.Load += (s, e) => OnFormLoad();
            this.Resize += (s, e) => RepositionControls();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Load data from the repository
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// Configure the data grid columns
        /// </summary>
        protected abstract void ConfigureDataGridColumns();

        /// <summary>
        /// Configure the edit panel fields
        /// </summary>
        protected abstract void ConfigureEditPanelFields();

        /// <summary>
        /// Save the current item
        /// </summary>
        protected abstract void SaveItem();

        /// <summary>
        /// Delete the selected item
        /// </summary>
        protected abstract void DeleteItem(T item);

        /// <summary>
        /// Filter data based on search criteria
        /// </summary>
        protected abstract List<T> FilterItems(List<T> items, string searchTerm);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Add status indicators to the header
        /// </summary>
        protected virtual void AddStatusIndicators()
        {
            // Default implementation - can be overridden
        }

        /// <summary>
        /// Configure tooltips for controls
        /// </summary>
        protected virtual void ConfigureTooltips()
        {
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_addButton, $"Add a new {typeof(T).Name.ToLower()}");
            toolTip.SetToolTip(_editButton, $"Edit the selected {typeof(T).Name.ToLower()}");
            toolTip.SetToolTip(_deleteButton, $"Delete the selected {typeof(T).Name.ToLower()}");
            toolTip.SetToolTip(_refreshButton, "Refresh the data");
            toolTip.SetToolTip(_exportButton, "Export data to file");
            toolTip.SetToolTip(_searchTextBox, SearchHintText);
        }

        /// <summary>
        /// Export data to file
        /// </summary>
        protected virtual void ExportData()
        {
            // Default implementation - can be overridden
            MessageBox.Show("Export functionality not implemented", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Protected Methods

        protected void ShowLoading(string message = "Loading...")
        {
            _loadingLabel.Text = message;
            _loadingPanel.Visible = true;
            _loadingPanel.BringToFront();
        }

        protected void HideLoading()
        {
            _loadingPanel.Visible = false;
        }

        protected void ShowEditPanel()
        {
            _actionCard.Visible = true;
            _editPanel.ShowPanel();
        }

        protected void HideEditPanel()
        {
            _actionCard.Visible = false;
            _editPanel.HidePanel();
        }

        protected void RefreshDataGrid()
        {
            _dataGrid.DataSource = null;
            _dataGrid.DataSource = _filteredItems;
            UpdateStatusIndicators();
        }

        protected virtual void UpdateStatusIndicators()
        {
            // Update status indicators based on data
            // Can be overridden by derived classes
        }

        #endregion

        #region Private Methods

        private void OnFormLoad()
        {
            LoadData();
        }

        private void AddNewItem()
        {
            _isEditing = false;
            _currentItem = default(T)!; // Using null-forgiving operator for generic default
            _editPanel.ClearFields();
            _editPanel.SetButtonText("Add", "Cancel");
            _editPanel.IsEditMode = false;
            ShowEditPanel();
        }

        private void EditSelectedItem()
        {
            if (_dataGrid.SelectedRows.Count == 0) return;

            _isEditing = true;
            _currentItem = (T)_dataGrid.SelectedRows[0].DataBoundItem;
            PopulateEditPanel(_currentItem);
            _editPanel.SetButtonText("Save", "Cancel");
            _editPanel.IsEditMode = true;
            ShowEditPanel();
        }

        private void DeleteSelectedItem()
        {
            if (_dataGrid.SelectedRows.Count == 0) return;

            var item = (T)_dataGrid.SelectedRows[0].DataBoundItem;
            var result = BusBuddy.UI.Components.MaterialMessageBox.Show(this,
                $"Are you sure you want to delete this {typeof(T).Name.ToLower()}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DeleteItem(item);
                RefreshData();
            }
        }

        private void RefreshData()
        {
            ShowLoading("Refreshing data...");
            LoadData();
            HideLoading();
            DataRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private void FilterData()
        {
            var searchTerm = _searchTextBox.Text;
            _filteredItems = string.IsNullOrWhiteSpace(searchTerm)
                ? new List<T>(_allItems)
                : FilterItems(_allItems, searchTerm);

            RefreshDataGrid();
        }

        private void DataGrid_SelectionChanged(object sender, EventArgs e)
        {
            var hasSelection = _dataGrid.SelectedRows.Count > 0;
            _editButton.Enabled = hasSelection;
            _deleteButton.Enabled = hasSelection;

            if (hasSelection)
            {
                var selectedItem = (T)_dataGrid.SelectedRows[0].DataBoundItem;
                ItemSelected?.Invoke(this, selectedItem);
            }
        }

        private void EditPanel_SaveClicked(object sender, EventArgs e)
        {
            SaveItem();
            HideEditPanel();
            RefreshData();
        }

        private void EditPanel_CancelClicked(object sender, EventArgs e)
        {
            HideEditPanel();
        }

        private void ApplyMaterialStyling()
        {
            // Apply DPI scaling to all controls
            this.ApplyDpiAwareSpacingToAll();
        }

        private void RepositionControls()
        {
            // Reposition search textbox on resize
            if (_searchTextBox?.Parent != null)
            {
                _searchTextBox.Location = new Point(
                    _searchTextBox.Parent.Width - _searchTextBox.Width - 8, 8);
            }

            // Reposition loading card
            if (_loadingPanel?.Controls.Count > 0)
            {
                var loadingCard = _loadingPanel.Controls[0];
                loadingCard.Location = new Point(
                    (_loadingPanel.Width - loadingCard.Width) / 2,
                    (_loadingPanel.Height - loadingCard.Height) / 2
                );
            }
        }

        /// <summary>
        /// Populate edit panel with item data - must be implemented by derived classes
        /// </summary>
        protected abstract void PopulateEditPanel(T item);

        #endregion
    }
}
