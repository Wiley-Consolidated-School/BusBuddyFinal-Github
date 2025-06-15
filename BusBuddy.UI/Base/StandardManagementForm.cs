using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin.Controls;
using BusBuddy.Business;
using BusBuddy.UI.Extensions;
using BusBuddy.UI.Helpers;
using BusBuddy.UI.Theme;
using BusBuddy.UI.Configuration;
using BusBuddy.UI.Components;

namespace BusBuddy.UI.Base
{
    /// <summary>
    /// Base management form following Microsoft Windows Forms inheritance best practices
    /// Provides standardized Material Design UI for data management with enhanced DataGridView
    ///
    /// This form follows the Microsoft recommended patterns for Windows Forms inheritance:
    /// - Proper partial class structure
    /// - Designer support with InitializeComponent()
    /// - Virtual methods for customization
    /// - Protected fields for derived class access
    /// - Proper resource disposal
    /// </summary>
    [System.ComponentModel.DesignerCategory("Form")]
    [System.ComponentModel.DesignTimeVisible(false)]
    public partial class StandardManagementForm<T> : StandardMaterialForm where T : class, new()
    {
        #region Component Designer Variables

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        #endregion
        #region Protected Fields and Properties

        // Core UI Controls - following Microsoft naming conventions
        protected DataGridView dataGridView;
        protected MaterialButton btnAdd;
        protected MaterialButton btnEdit;
        protected MaterialButton btnDelete;
        protected MaterialButton btnRefresh;
        protected MaterialButton btnExport;
        protected MaterialTextBox txtSearch;
        protected MaterialLabel lblTitle;
        protected MaterialLabel lblSubtitle;
        protected MaterialLabel lblSearch;
        protected Panel toolbarPanel;
        protected Panel contentPanel;
        protected Panel actionPanel;
        protected System.Windows.Forms.Timer searchTimer;

        // Data source and business logic
        protected BindingList<T> _dataSource;
        protected readonly DatabaseHelperService _databaseService;

        // Convenience properties for derived classes
        protected DataGridView DataGridView => dataGridView;
        protected MaterialButton AddButton => btnAdd;
        protected MaterialButton EditButton => btnEdit;
        protected MaterialButton DeleteButton => btnDelete;
        protected MaterialButton RefreshButton => btnRefresh;
        protected MaterialButton ExportButton => btnExport;
        protected MaterialTextBox SearchTextBox => txtSearch;

        // Virtual properties for derived classes to override
        protected virtual string FormTitle => "Manage Items";
        protected virtual string FormSubtitle => "Add, edit, or remove items from the system";
        protected virtual string AddButtonText => "Add";
        protected virtual string EditButtonText => "Edit";
        protected virtual string DeleteButtonText => "Delete";
        protected virtual string RefreshButtonText => "Refresh";
        protected virtual string ExportButtonText => "Export";
        protected virtual string SearchHintText => "Search items...";

        #endregion

        #region Constructor

        public StandardManagementForm()
        {
            _databaseService = new DatabaseHelperService();
            _dataSource = new BindingList<T>();

            InitializeComponent();
            InitializeDataGrid();
            InitializeEventHandlers();

            // Apply initial styling after all controls are created
            ApplyStandardStyling();
        }

        #endregion

        #region Windows Forms Designer Generated Code        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            //
            // StandardManagementForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "StandardManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Data Management";

            // Form configuration
            ConfigureForm();

            // Create panels in proper order
            CreatePanels();

            // Create controls
            CreateLabels();
            CreateSearchControls();
            CreateDataGrid();
            CreateActionButtons();

            // Add controls to panels
            AddControlsToPanels();

            // Add panels to form
            AddPanelsToForm();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureForm()
        {
            this.Text = FormTitle;
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
        }

        private void CreatePanels()
        {
            // Toolbar panel for search and filters
            toolbarPanel = new Panel
            {
                Name = "toolbarPanel",
                Height = 80,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(250, 250, 250)
            };

            // Content panel for main data display
            contentPanel = new Panel
            {
                Name = "contentPanel",
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10)
            };

            // Action panel for buttons
            actionPanel = new Panel
            {
                Name = "actionPanel",
                Height = 70,
                Dock = DockStyle.Bottom,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(245, 245, 245)
            };
        }

        private void CreateLabels()
        {
            lblTitle = new MaterialLabel
            {
                Name = "lblTitle",
                Text = FormTitle,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Location = new Point(20, 15),
                Size = new Size(400, 30),
                UseAccent = true,
                AutoSize = true
            };

            lblSubtitle = new MaterialLabel
            {
                Name = "lblSubtitle",
                Text = FormSubtitle,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(20, 45),
                Size = new Size(600, 20),
                ForeColor = Color.FromArgb(117, 117, 117),
                AutoSize = true
            };

            lblSearch = new MaterialLabel
            {
                Name = "lblSearch",
                Text = "Search:",
                Location = new Point(20, 25),
                Size = new Size(60, 24),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
        }

        private void CreateSearchControls()
        {
            txtSearch = new MaterialTextBox
            {
                Name = "txtSearch",
                Location = new Point(90, 20),
                Size = new Size(350, 30),
                Hint = SearchHintText,
                Font = new Font("Segoe UI", 10F)
            };

            searchTimer = new System.Windows.Forms.Timer(this.components)
            {
                Interval = 500,
                Enabled = false
            };
        }

        private void CreateDataGrid()
        {
            dataGridView = CreateEnhancedDataGridView();
            dataGridView.Name = "dataGridView";
        }

        private void CreateActionButtons()
        {
            btnAdd = new MaterialButton
            {
                Name = "btnAdd",
                Text = AddButtonText,
                Size = new Size(120, 40),
                Location = new Point(20, 15),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };

            btnEdit = new MaterialButton
            {
                Name = "btnEdit",
                Text = EditButtonText,
                Size = new Size(120, 40),
                Location = new Point(150, 15),
                Type = MaterialButton.MaterialButtonType.Contained,
                Enabled = false
            };

            btnDelete = new MaterialButton
            {
                Name = "btnDelete",
                Text = DeleteButtonText,
                Size = new Size(120, 40),
                Location = new Point(280, 15),
                Type = MaterialButton.MaterialButtonType.Contained,
                Enabled = false
            };

            btnRefresh = new MaterialButton
            {
                Name = "btnRefresh",
                Text = RefreshButtonText,
                Size = new Size(120, 40),
                Location = new Point(410, 15),
                Type = MaterialButton.MaterialButtonType.Outlined
            };

            btnExport = new MaterialButton
            {
                Name = "btnExport",
                Text = ExportButtonText,
                Size = new Size(120, 40),
                Location = new Point(540, 15),
                Type = MaterialButton.MaterialButtonType.Outlined
            };
        }

        private void AddControlsToPanels()
        {
            // Add controls to toolbar
            toolbarPanel.Controls.AddRange(new Control[]
            {
                lblSearch,
                txtSearch
            });

            // Add title and subtitle to content panel
            contentPanel.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblSubtitle,
                dataGridView
            });

            // Add buttons to action panel
            actionPanel.Controls.AddRange(new Control[]
            {
                btnAdd,
                btnEdit,
                btnDelete,
                btnRefresh,
                btnExport
            });
        }

        private void AddPanelsToForm()
        {
            this.Controls.AddRange(new Control[]
            {
                contentPanel,  // Fill - must be added first for proper docking
                actionPanel,   // Bottom
                toolbarPanel   // Top
            });
        }

        #endregion

        #region Enhanced DataGridView Creation

        /// <summary>
        /// Creates an enhanced kick-ass DataGridView with advanced styling and features
        /// </summary>
        private DataGridView CreateEnhancedDataGridView()
        {
            var grid = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(940, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.FromArgb(250, 250, 250),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.None,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing,
                ColumnHeadersHeight = 45,
                RowTemplate = { Height = 50 },
                GridColor = Color.FromArgb(240, 240, 240),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                VirtualMode = false
            };

            // Enhanced Header Styling with Material Design
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(63, 81, 181), // Material Indigo
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.FromArgb(63, 81, 181),
                SelectionForeColor = Color.White,
                Padding = new Padding(15, 5, 5, 5)
            };

            // Enhanced Row Styling
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 33, 33),
                Font = new Font("Segoe UI", 10F),
                SelectionBackColor = Color.FromArgb(63, 81, 181, 30),
                SelectionForeColor = Color.FromArgb(63, 81, 181),
                Padding = new Padding(15, 8, 5, 8),
                WrapMode = DataGridViewTriState.False
            };

            // Alternating Row Colors
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(33, 33, 33),
                Font = new Font("Segoe UI", 10F),
                SelectionBackColor = Color.FromArgb(63, 81, 181, 30),
                SelectionForeColor = Color.FromArgb(63, 81, 181),
                Padding = new Padding(15, 8, 5, 8)
            };

            return grid;
        }

        #endregion

        #region Initialization Methods

        private void InitializeDataGrid()
        {
            dataGridView.DataSource = _dataSource;
            dataGridView.AutoGenerateColumns = true;
        }

        private void InitializeEventHandlers()
        {
            // Button events
            btnAdd.Click += OnAddClick;
            btnEdit.Click += OnEditClick;
            btnDelete.Click += OnDeleteClick;
            btnRefresh.Click += OnRefreshClick;
            btnExport.Click += OnExportClick;

            // Search events
            txtSearch.TextChanged += OnSearchTextChanged;
            searchTimer.Tick += OnSearchTimerTick;

            // DataGridView events
            dataGridView.SelectionChanged += OnDataGridSelectionChanged;
            dataGridView.CellDoubleClick += OnDataGridCellDoubleClick;
        }

        private void ApplyStandardStyling()
        {
            // Apply any additional styling
            this.KeyPreview = true;
        }

        #endregion

        #region Virtual Event Handlers - Override in Derived Classes

        protected virtual void OnAddClick(object sender, EventArgs e)
        {
            // Override in derived classes
            MessageBox.Show("Add functionality - override in derived class", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected virtual void OnEditClick(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Override in derived classes
            MessageBox.Show("Edit functionality - override in derived class", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected virtual void OnDeleteClick(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this item?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Override in derived classes
                MessageBox.Show("Delete functionality - override in derived class", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected virtual void OnRefreshClick(object sender, EventArgs e)
        {
            LoadData();
        }

        protected virtual void OnExportClick(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = $"{typeof(T).Name}_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportToCsv(saveDialog.FileName);
                    MessageBox.Show($"Data exported successfully to: {saveDialog.FileName}", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export data: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected virtual void OnSearchTextChanged(object sender, EventArgs e)
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        protected virtual void OnSearchTimerTick(object sender, EventArgs e)
        {
            searchTimer.Stop();
            ApplySearchFilter(txtSearch.Text);
        }

        protected virtual void OnDataGridSelectionChanged(object sender, EventArgs e)
        {
            var hasSelection = dataGridView.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        protected virtual void OnDataGridCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                OnEditClick(sender, e);
            }
        }

        #endregion

        #region Virtual Methods for Data Management - Override in Derived Classes

        protected virtual void LoadData()
        {
            // Override in derived classes to load actual data
            _dataSource.Clear();

            // Example: Load from service
            // var items = _databaseService.GetAll<T>();
            // foreach (var item in items)
            // {
            //     _dataSource.Add(item);
            // }
        }

        protected virtual void ApplySearchFilter(string searchText)
        {
            // Override in derived classes for custom search logic
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadData(); // Reload all data
                return;
            }

            // Default search implementation
            PerformSearch(searchText.Trim());
        }

        protected virtual void PerformSearch(string searchText)
        {
            // Override in derived classes for specific search implementations
            // This is a placeholder for custom search logic
        }

        protected virtual bool ValidateAdd()
        {
            // Override in derived classes for add validation
            return true;
        }

        protected virtual bool ValidateEdit()
        {
            // Override in derived classes for edit validation
            return true;
        }

        protected virtual bool ValidateDelete()
        {
            // Override in derived classes for delete validation
            return true;
        }

        #endregion

        #region Export Functionality

        protected virtual void ExportToCsv(string fileName)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                // Write headers
                var headers = new List<string>();
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    if (column.Visible)
                    {
                        headers.Add($"\"{column.HeaderText}\"");
                    }
                }
                writer.WriteLine(string.Join(",", headers));

                // Write data rows
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var values = new List<string>();
                        foreach (DataGridViewColumn column in dataGridView.Columns)
                        {
                            if (column.Visible)
                            {
                                var cellValue = row.Cells[column.Index].Value?.ToString() ?? "";
                                values.Add($"\"{cellValue.Replace("\"", "\"\"")}\"");
                            }
                        }
                        writer.WriteLine(string.Join(",", values));
                    }
                }
            }
        }

        #endregion

        #region Keyboard Shortcuts

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    OnRefreshClick(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.N:
                    OnAddClick(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.E:
                    OnEditClick(this, EventArgs.Empty);
                    return true;
                case Keys.Delete:
                    OnDeleteClick(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.F:
                    txtSearch.Focus();
                    txtSearch.SelectAll();
                    return true;
                case Keys.Control | Keys.S:
                    OnExportClick(this, EventArgs.Empty);
                    return true;
                case Keys.Escape:
                    txtSearch.Clear();
                    ApplySearchFilter("");
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Helper Methods

        protected virtual void ShowSuccessMessage(string message)
        {
            BusBuddy.UI.Components.MaterialMessageBox.ShowSuccess(this, message);
        }

        protected virtual void ShowErrorMessage(string message)
        {
            BusBuddy.UI.Components.MaterialMessageBox.ShowError(this, message);
        }

        protected virtual void ShowWarningMessage(string message)
        {
            BusBuddy.UI.Components.MaterialMessageBox.ShowWarning(this, message);
        }

        protected virtual bool ConfirmDelete(string itemName = "item")
        {
            var result = BusBuddy.UI.Components.MaterialMessageBox.Show(this,
                $"Are you sure you want to delete this {itemName}?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        protected virtual bool ConfirmAction(string message, string title = "Confirm")
        {
            var result = BusBuddy.UI.Components.MaterialMessageBox.Show(this, message, title,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (components != null)
                {
                    components.Dispose();
                }

                // Dispose timer
                if (searchTimer != null)
                {
                    searchTimer.Stop();
                    searchTimer.Dispose();
                }

                // Dispose data source
                if (_dataSource != null)
                {
                    _dataSource.Clear();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
