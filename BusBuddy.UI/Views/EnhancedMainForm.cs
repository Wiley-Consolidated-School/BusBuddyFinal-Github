using System;
using System.Drawing;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.UI.Base;
using BusBuddy.UI.Services;
using BusBuddy.UI.Theme;
using BusBuddy.UI.Helpers;
using MaterialSkin.Controls;

namespace BusBuddy.UI.Views
{
    /// <summary>
    /// Enhanced MainForm with dependency injection and better separation of concerns
    /// </summary>
    public partial class EnhancedMainForm : StandardMaterialForm
    {
        private readonly IDatabaseHelperService _databaseService;
        private readonly INavigationService _navigationService;

        public EnhancedMainForm(IDatabaseHelperService databaseService, INavigationService navigationService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            InitializeComponent();
            ConfigureWindow();
        }

        /// <summary>
        /// Configure window properties for optimal Material Design presentation with DPI awareness
        /// </summary>
        private void ConfigureWindow()
        {
            // Set window specific properties (base class handles standard styling)
            this.WindowState = FormWindowState.Maximized;
            this.Text = "BusBuddy - School Bus Management System";

            // Set larger minimum size for main form
            var scaleFactor = DpiScaleHelper.GetControlScaleFactor(this);
            this.MinimumSize = new Size(
                DpiScaleHelper.ScaleSize(1024, scaleFactor), // Larger minimum width for main form
                DpiScaleHelper.ScaleSize(768, scaleFactor)   // Larger minimum height for main form
            );
        }

        private void InitializeComponent()
        {
            this.Text = "BusBuddy - Bus Tracking Companion";
            this.Size = new System.Drawing.Size(1400, 900); // Larger default for modern displays
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new System.Drawing.Size(1200, 700); // Increased minimum size

            // Enhanced high-DPI support with proper scaling
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.Font = MaterialDesignThemeManager.Typography.GetBodyMedium(this);

            // Enable high-quality rendering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.DoubleBuffer |
                         ControlStyles.ResizeRedraw, true);

            CreateMainLayout();
        }

        private void CreateMainLayout()
        {
            // Create main container with Material Design styling
            var mainContainer = new TableLayoutPanel();
            mainContainer.Dock = DockStyle.Fill;
            mainContainer.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;

            // DPI-aware padding using Material Design spacing
            var padding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 24, this);
            mainContainer.Padding = padding;

            mainContainer.RowCount = 3;
            mainContainer.ColumnCount = 1;

            // Row 0: Header section (auto-size)
            mainContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            // Row 1: Spacer (DPI-aware Material Design spacing)
            float spacerHeight = MaterialDesignThemeManager.GetDpiAwareSize(32, this);
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, spacerHeight));
            // Row 2: Dashboard section (fill remaining space)
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            this.Controls.Add(mainContainer);

            // Create header section with Material Design styling
            CreateMaterialHeaderSection(mainContainer);

            // Create dashboard section with vector graphics
            CreateMaterialDashboardSection(mainContainer);
        }

        private void CreateMaterialHeaderSection(TableLayoutPanel parent)
        {
            // Create header panel with Material Design elevation
            var headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Fill;
            MaterialDesignThemeManager.ApplyCardElevation(headerPanel, 1);

            // DPI-aware padding for header
            var headerPadding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 20, this);
            headerPanel.Padding = headerPadding;

            // Welcome title with Material Design typography
            var welcomeLabel = new MaterialLabel();
            welcomeLabel.Text = "Welcome to BusBuddy!";
            welcomeLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.H3;
            welcomeLabel.AutoSize = true;
            welcomeLabel.Location = new Point(24, 20);
            welcomeLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            headerPanel.Controls.Add(welcomeLabel);

            // Description with proper Material Design spacing
            var descriptionLabel = new MaterialLabel();
            descriptionLabel.Text = "The comprehensive school bus tracking and management system";
            descriptionLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            descriptionLabel.AutoSize = true;
            descriptionLabel.Location = new Point(24, welcomeLabel.Bottom + MaterialDesignThemeManager.GetDpiAwareSize(12, this));
            descriptionLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurfaceVariant;
            headerPanel.Controls.Add(descriptionLabel);

            parent.Controls.Add(headerPanel, 0, 0);
        }

        private void CreateMaterialDashboardSection(TableLayoutPanel parent)
        {
            // Create main dashboard panel with Material Design surface
            var dashboardPanel = new Panel();
            dashboardPanel.Dock = DockStyle.Fill;
            dashboardPanel.AutoScroll = true;
            dashboardPanel.BackColor = MaterialDesignThemeManager.DarkTheme.Surface;

            var padding = MaterialDesignThemeManager.GetDpiAwarePadding(24, 24, this);
            dashboardPanel.Padding = padding;

            // Create sections using Material Design cards
            int yPosition = 0;
            int sectionSpacing = MaterialDesignThemeManager.GetDpiAwareSize(32, this);

            // Quick actions section - now featured prominently
            yPosition += CreateMaterialQuickActionsSection(dashboardPanel, yPosition);
            yPosition += sectionSpacing;

            // Management tools section
            yPosition += CreateMaterialManagementSection(dashboardPanel, yPosition);
            yPosition += sectionSpacing;

            // Admin tools section
            CreateMaterialAdminSection(dashboardPanel, yPosition);

            parent.Controls.Add(dashboardPanel, 0, 2);
        }

        private int CreateMaterialQuickActionsSection(Panel parent, int yPos)
        {
            // Enhanced section header with more prominent styling
            var sectionHeader = CreateSectionHeader("🚀 Primary Management Tools");
            sectionHeader.Location = new Point(0, yPos);
            parent.Controls.Add(sectionHeader);

            int currentY = yPos + sectionHeader.Height + MaterialDesignThemeManager.GetDpiAwareSize(20, this);

            // Create toolbar above grid
            var toolbarPanel = new Panel();
            toolbarPanel.Location = new Point(0, currentY);
            toolbarPanel.Size = new Size(
                MaterialDesignThemeManager.GetDpiAwareSize(680, this),
                MaterialDesignThemeManager.GetDpiAwareSize(48, this)
            );
            toolbarPanel.BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer;
            MaterialDesignThemeManager.ApplyCardElevation(toolbarPanel, 1);

            // Add toolbar buttons
            var addButton = CreateMaterialButtonWithIcon("add", "Add", MaterialButton.MaterialButtonType.Contained, true);
            addButton.Size = new Size(MaterialDesignThemeManager.GetDpiAwareSize(80, this), MaterialDesignThemeManager.GetDpiAwareSize(36, this));
            addButton.Location = new Point(8, 6);
            addButton.Click += AddNavigationAction;
            toolbarPanel.Controls.Add(addButton);

            var editButton = CreateMaterialButtonWithIcon("edit", "Edit", MaterialButton.MaterialButtonType.Outlined, false);
            editButton.Size = new Size(MaterialDesignThemeManager.GetDpiAwareSize(80, this), MaterialDesignThemeManager.GetDpiAwareSize(36, this));
            editButton.Location = new Point(96, 6);
            editButton.Click += EditNavigationAction;
            toolbarPanel.Controls.Add(editButton);

            var deleteButton = CreateMaterialButtonWithIcon("delete", "Delete", MaterialButton.MaterialButtonType.Outlined, false);
            deleteButton.Size = new Size(MaterialDesignThemeManager.GetDpiAwareSize(80, this), MaterialDesignThemeManager.GetDpiAwareSize(36, this));
            deleteButton.Location = new Point(184, 6);
            deleteButton.Click += DeleteNavigationAction;
            toolbarPanel.Controls.Add(deleteButton);

            parent.Controls.Add(toolbarPanel);
            currentY += toolbarPanel.Height + MaterialDesignThemeManager.GetDpiAwareSize(8, this);

            // Create DataGridView for navigation actions
            var navigationGrid = new DataGridView();
            navigationGrid.Location = new Point(0, currentY);
            navigationGrid.Size = new Size(
                MaterialDesignThemeManager.GetDpiAwareSize(680, this),
                MaterialDesignThemeManager.GetDpiAwareSize(200, this)
            );

            // Apply Material Design styling
            navigationGrid.BackgroundColor = MaterialDesignThemeManager.DarkTheme.Surface;
            navigationGrid.GridColor = MaterialDesignThemeManager.DarkTheme.Outline;
            navigationGrid.BorderStyle = BorderStyle.None;

            navigationGrid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MaterialDesignThemeManager.DarkTheme.Surface,
                ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface,
                SelectionBackColor = MaterialDesignThemeManager.DarkTheme.Primary,
                SelectionForeColor = MaterialDesignThemeManager.DarkTheme.OnPrimary,
                Font = MaterialDesignThemeManager.Typography.GetBodyMedium(this)
            };

            navigationGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainerHigh,
                ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface,
                Font = MaterialDesignThemeManager.Typography.GetLabelMedium(this)
            };

            navigationGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            navigationGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            navigationGrid.MultiSelect = false;
            navigationGrid.ReadOnly = true;
            navigationGrid.AllowUserToAddRows = false;
            navigationGrid.AllowUserToDeleteRows = false;
            navigationGrid.RowHeadersVisible = false;
            navigationGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // Add columns
            var actionColumn = new DataGridViewTextBoxColumn();
            actionColumn.Name = "Action";
            actionColumn.HeaderText = "Action";
            actionColumn.Width = MaterialDesignThemeManager.GetDpiAwareSize(200, this);
            actionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            navigationGrid.Columns.Add(actionColumn);

            var descriptionColumn = new DataGridViewTextBoxColumn();
            descriptionColumn.Name = "Description";
            descriptionColumn.HeaderText = "Description";
            descriptionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            navigationGrid.Columns.Add(descriptionColumn);

            // Populate with navigation actions
            var navigationActions = new[]
            {
                new { Action = "Vehicle Management", Description = "Manage your fleet of buses and vehicles", NavigationAction = new Action(() => _navigationService.ShowVehicleManagement()) },
                new { Action = "Driver Management", Description = "Manage driver information and records", NavigationAction = new Action(() => _navigationService.ShowDriverManagement()) },
                new { Action = "Time Card Management", Description = "Track driver hours and overtime", NavigationAction = new Action(() => _navigationService.ShowTimeCardManagement()) },
                new { Action = "Route Management", Description = "Plan and optimize bus routes", NavigationAction = new Action(() => _navigationService.ShowRouteManagement()) }
            };

            foreach (var item in navigationActions)
            {
                var row = navigationGrid.Rows.Add(item.Action, item.Description);
                navigationGrid.Rows[row].Tag = item.NavigationAction; // Store navigation action in row metadata
            }

            // Add click handler for navigation
            navigationGrid.CellDoubleClick += (s, e) => {
                if (e.RowIndex >= 0 && navigationGrid.Rows[e.RowIndex].Tag is Action action)
                {
                    action.Invoke();
                }
            };

            // Store reference for toolbar button handlers
            navigationGrid.Name = "NavigationGrid";
            parent.Controls.Add(navigationGrid);

            return currentY + navigationGrid.Height + MaterialDesignThemeManager.GetDpiAwareSize(10, this) - yPos;
        }

        private int CreateMaterialManagementSection(Panel parent, int yPos)
        {
            // Section header
            var sectionHeader = CreateSectionHeader("🔧 Additional Management Tools");
            sectionHeader.Location = new Point(0, yPos);
            parent.Controls.Add(sectionHeader);

            int currentY = yPos + sectionHeader.Height + MaterialDesignThemeManager.GetDpiAwareSize(18, this);

            // Management tool buttons in horizontal layout with improved sizing
            var tools = new[]
            {
                ("build", "Maintenance", new Action(() => _navigationService.ShowMaintenanceManagement())),
                ("local_gas_station", "Fuel Management", new Action(() => _navigationService.ShowFuelManagement())),
                ("event", "Activities", new Action(() => _navigationService.ShowActivityManagement())),
                ("today", "Schedules", new Action(() => _navigationService.ShowScheduleManagement()))
            };

            // Improved button sizing for better visual hierarchy
            var buttonSize = new Size(
                MaterialDesignThemeManager.GetDpiAwareSize(170, this),
                MaterialDesignThemeManager.GetDpiAwareSize(52, this)
            );
            var spacing = MaterialDesignThemeManager.GetDpiAwareSize(16, this);

            for (int i = 0; i < tools.Length; i++)
            {
                var toolButton = CreateMaterialButtonWithIcon(
                    tools[i].Item1,
                    tools[i].Item2,
                    MaterialButton.MaterialButtonType.Outlined,
                    false);
                toolButton.Size = buttonSize;
                toolButton.Location = new Point(i * (buttonSize.Width + spacing), currentY);

                // Capture the action by value to avoid closure issues
                var action = tools[i].Item3;
                toolButton.Click += (s, e) => action();
                parent.Controls.Add(toolButton);
            }

            return currentY + buttonSize.Height + MaterialDesignThemeManager.GetDpiAwareSize(28, this) - yPos;
        }

        private void CreateMaterialAdminSection(Panel parent, int yPos)
        {
            // Section header
            var sectionHeader = CreateSectionHeader("⚙️ Administrative Tools");
            sectionHeader.Location = new Point(0, yPos);
            parent.Controls.Add(sectionHeader);

            int currentY = yPos + sectionHeader.Height + MaterialDesignThemeManager.GetDpiAwareSize(18, this);

            // Admin tool buttons with consistent sizing
            var buttonSize = new Size(
                MaterialDesignThemeManager.GetDpiAwareSize(170, this),
                MaterialDesignThemeManager.GetDpiAwareSize(52, this)
            );
            var spacing = MaterialDesignThemeManager.GetDpiAwareSize(16, this);

            var calendarBtn = CreateMaterialButtonWithIcon(
                "event",
                "School Calendar",
                MaterialButton.MaterialButtonType.Outlined,
                false);
            calendarBtn.Size = buttonSize;
            calendarBtn.Location = new Point(0, currentY);
            calendarBtn.Click += (s, e) => _navigationService.ShowCalendarManagement();
            parent.Controls.Add(calendarBtn);

            var settingsBtn = CreateMaterialButtonWithIcon(
                "settings",
                "System Settings",
                MaterialButton.MaterialButtonType.Outlined,
                false);
            settingsBtn.Size = buttonSize;
            settingsBtn.Location = new Point(buttonSize.Width + spacing, currentY);
            settingsBtn.Click += (s, e) => Console.WriteLine("INFO: Settings coming soon!");
            parent.Controls.Add(settingsBtn);

            // Add Reports button for better functionality
            var reportsBtn = CreateMaterialButtonWithIcon(
                "assessment",
                "Reports",
                MaterialButton.MaterialButtonType.Outlined,
                false);
            reportsBtn.Size = buttonSize;
            reportsBtn.Location = new Point(2 * (buttonSize.Width + spacing), currentY);
            reportsBtn.Click += (s, e) => _navigationService.ShowReportsManagement();
            parent.Controls.Add(reportsBtn);
        }

        /// <summary>
        /// Create a Material Design button with vector icon
        /// </summary>
        private MaterialButton CreateMaterialButtonWithIcon(string icon, string text,
            MaterialButton.MaterialButtonType buttonType, bool useAccent)
        {
            var button = new MaterialButton();
            button.Text = text;
            button.Type = buttonType;
            button.UseAccentColor = useAccent;
            button.Font = MaterialDesignThemeManager.Typography.GetLabelMedium(this);
            button.Size = new Size(
                MaterialDesignThemeManager.GetDpiAwareSize(200, this),
                MaterialDesignThemeManager.GetDpiAwareSize(48, this)
            );

            return button;
        }

        /// <summary>
        /// Create a section header with Material Design typography
        /// </summary>
        private MaterialLabel CreateSectionHeader(string text)
        {
            var header = new MaterialLabel();
            header.Text = text;
            header.FontType = MaterialSkin.MaterialSkinManager.fontType.H5;
            header.AutoSize = true;
            header.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            return header;
        }

        /// <summary>
        /// Create an action card with icon, title, and description
        /// </summary>
        private Panel CreateActionCard(string icon, string title, string description, Size cardSize)
        {
            var card = new Panel();
            card.Size = cardSize;
            card.Cursor = Cursors.Hand;

            // Apply Material Design styling
            MaterialDesignThemeManager.ApplyCardElevation(card, 2);

            // Add hover effects
            card.MouseEnter += (s, e) => {
                card.BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainerHigh;
            };
            card.MouseLeave += (s, e) => {
                card.BackColor = MaterialDesignThemeManager.DarkTheme.SurfaceContainer;
            };

            var padding = MaterialDesignThemeManager.GetDpiAwareSize(16, this);

            // Add title
            var titleLabel = new MaterialLabel();
            titleLabel.Text = title;
            titleLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.Subtitle1;
            titleLabel.Location = new Point(padding, padding);
            titleLabel.AutoSize = true;
            titleLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurface;
            card.Controls.Add(titleLabel);

            // Add description
            var descLabel = new MaterialLabel();
            descLabel.Text = description;
            descLabel.FontType = MaterialSkin.MaterialSkinManager.fontType.Body2;
            descLabel.Location = new Point(padding, titleLabel.Bottom + 4);
            descLabel.Size = new Size(cardSize.Width - (padding * 2), MaterialDesignThemeManager.GetDpiAwareSize(40, this));
            descLabel.ForeColor = MaterialDesignThemeManager.DarkTheme.OnSurfaceVariant;
            card.Controls.Add(descLabel);

            return card;
        }

        /// <summary>
        /// Add a new navigation action to the grid
        /// </summary>
        private void AddNavigationAction(object sender, EventArgs e)
        {
            using (var dialog = new Form())
            {
                dialog.Text = "Add Navigation Action";
                dialog.Size = new Size(400, 200);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                var titleLabel = new Label { Text = "Title:", Location = new Point(20, 20), AutoSize = true };
                var titleTextBox = new TextBox { Location = new Point(20, 45), Size = new Size(340, 23) };

                var descLabel = new Label { Text = "Description:", Location = new Point(20, 80), AutoSize = true };
                var descTextBox = new TextBox { Location = new Point(20, 105), Size = new Size(340, 23) };

                var okButton = new Button { Text = "Add", Location = new Point(200, 140), Size = new Size(75, 23), DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Location = new Point(285, 140), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };

                dialog.Controls.AddRange(new Control[] { titleLabel, titleTextBox, descLabel, descTextBox, okButton, cancelButton });
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(titleTextBox.Text))
                {
                    var grid = FindNavigationGrid();
                    if (grid != null)
                    {
                        var row = grid.Rows.Add(titleTextBox.Text, descTextBox.Text);
                        grid.Rows[row].Tag = null; // No navigation action for custom entries
                    }
                }
            }
        }

        /// <summary>
        /// Edit the selected navigation action
        /// </summary>
        private void EditNavigationAction(object sender, EventArgs e)
        {
            var grid = FindNavigationGrid();
            if (grid?.CurrentRow != null)
            {
                using (var dialog = new Form())
                {
                    dialog.Text = "Edit Navigation Action";
                    dialog.Size = new Size(400, 200);
                    dialog.StartPosition = FormStartPosition.CenterParent;
                    dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;

                    var titleLabel = new Label { Text = "Title:", Location = new Point(20, 20), AutoSize = true };
                    var titleTextBox = new TextBox {
                        Location = new Point(20, 45),
                        Size = new Size(340, 23),
                        Text = grid.CurrentRow.Cells["Action"].Value?.ToString() ?? ""
                    };

                    var descLabel = new Label { Text = "Description:", Location = new Point(20, 80), AutoSize = true };
                    var descTextBox = new TextBox {
                        Location = new Point(20, 105),
                        Size = new Size(340, 23),
                        Text = grid.CurrentRow.Cells["Description"].Value?.ToString() ?? ""
                    };

                    var okButton = new Button { Text = "Update", Location = new Point(200, 140), Size = new Size(75, 23), DialogResult = DialogResult.OK };
                    var cancelButton = new Button { Text = "Cancel", Location = new Point(285, 140), Size = new Size(75, 23), DialogResult = DialogResult.Cancel };

                    dialog.Controls.AddRange(new Control[] { titleLabel, titleTextBox, descLabel, descTextBox, okButton, cancelButton });
                    dialog.AcceptButton = okButton;
                    dialog.CancelButton = cancelButton;

                    if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(titleTextBox.Text))
                    {
                        grid.CurrentRow.Cells["Action"].Value = titleTextBox.Text;
                        grid.CurrentRow.Cells["Description"].Value = descTextBox.Text;
                    }
                }
            }
        }

        /// <summary>
        /// Delete the selected navigation action
        /// </summary>
        private void DeleteNavigationAction(object sender, EventArgs e)
        {
            var grid = FindNavigationGrid();
            if (grid?.CurrentRow != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this navigation action?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    grid.Rows.Remove(grid.CurrentRow);
                }
            }
        }

        /// <summary>
        /// Find the navigation grid control
        /// </summary>
        private DataGridView FindNavigationGrid()
        {
            return FindControlByName("NavigationGrid") as DataGridView;
        }

        /// <summary>
        /// Recursively find a control by name
        /// </summary>
        private Control FindControlByName(string name)
        {
            return FindControlByName(this, name);
        }

        private Control FindControlByName(Control parent, string name)
        {
            if (parent.Name == name) return parent;

            foreach (Control child in parent.Controls)
            {
                var found = FindControlByName(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}
