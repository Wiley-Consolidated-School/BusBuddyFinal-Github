using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    public abstract class BaseManagementForm<T> : SyncfusionBaseForm where T : class
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
        #endregion

        #region Abstract Properties and Methods
        protected abstract string FormTitle { get; }
        protected abstract string SearchPlaceholder { get; }
        protected abstract string EntityName { get; }

        protected abstract void LoadData();
        protected abstract void AddNewEntity();
        protected abstract void EditSelectedEntity();
        protected abstract void DeleteSelectedEntity();
        protected abstract void ViewEntityDetails();
        protected abstract void SearchEntities();
        protected abstract void SetupDataGridColumns();
        #endregion

        #region Constructor and Initialization
        protected BaseManagementForm()
        {
            InitializeComponent();
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
            SyncfusionThemeHelper.ApplyMaterialTheme(this);

            Console.WriteLine($"🎨 SYNCFUSION FORM: {this.Text} initialized with standardized controls");
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
            _dataGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            SyncfusionThemeHelper.SfDataGridEnhancements(_dataGrid);

            SetupDataGridColumns();
        }
        #endregion

        #region Standard Layout
        protected virtual void LayoutControls()
        {
            var buttonY = GetDpiAwareY(20);

            // Standard button positioning with consistent 110px spacing
            _addButton!.Location = new Point(GetDpiAwareX(20), buttonY);
            _editButton!.Location = new Point(GetDpiAwareX(130), buttonY);
            _deleteButton!.Location = new Point(GetDpiAwareX(240), buttonY);
            _detailsButton!.Location = new Point(GetDpiAwareX(350), buttonY);

            // Search controls
            var searchLabel = ControlFactory.CreateLabel("🔍 Search:");
            searchLabel.Location = new Point(GetDpiAwareX(500), GetDpiAwareY(25));
            this.Controls.Add(searchLabel);

            _searchBox!.Location = new Point(GetDpiAwareX(550), GetDpiAwareY(20));
            _searchBox.Size = GetDpiAwareSize(new Size(150, 30));

            _searchButton!.Location = new Point(GetDpiAwareX(710), buttonY);

            // Grid positioning - standard layout
            _dataGrid!.Location = new Point(GetDpiAwareX(20), GetDpiAwareY(70));
            _dataGrid.Size = GetDpiAwareSize(new Size(1150, 800));
            _dataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                _addButton, _editButton, _deleteButton, _detailsButton, _searchButton,
                _searchBox, _dataGrid
            });

            // Standard initial button state - edit/delete/details disabled
            _editButton.Enabled = false;
            _deleteButton.Enabled = false;
            _detailsButton.Enabled = false;
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
            LoadData();
        }

        protected virtual void ShowErrorMessage(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected virtual void ShowInfoMessage(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }        protected new virtual bool ConfirmDelete(string entityName)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete this {entityName.ToLower()}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }
        #endregion
    }
}
