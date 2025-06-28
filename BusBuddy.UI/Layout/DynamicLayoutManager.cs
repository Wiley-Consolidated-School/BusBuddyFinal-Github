using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using System.Collections.Generic;

namespace BusBuddy.UI.Layout
{
    public static class DynamicLayoutManager
    {
        // Constants for consistent styling
        private static readonly Color DefaultBackgroundColor = Color.Transparent;

        /// <summary>
        /// Configuration class for responsive grid layouts.
        /// Used to define the structure and sizing of grid cells.
        /// </summary>
        public class LayoutConfiguration
        {
            /// <summary>
            /// Gets or sets the number of rows in the grid.
            /// </summary>
            public int Rows { get; set; }

            /// <summary>
            /// Gets or sets the number of columns in the grid.
            /// </summary>
            public int Columns { get; set; }

            /// <summary>
            /// Gets or sets the percentage sizes for each row.
            /// If null or incomplete, remaining rows will be sized equally.
            /// </summary>
            public List<float> RowSizes { get; set; }

            /// <summary>
            /// Gets or sets the percentage sizes for each column.
            /// If null or incomplete, remaining columns will be sized equally.
            /// </summary>
            public List<float> ColumnSizes { get; set; }

            /// <summary>
            /// Initializes a new instance of the LayoutConfiguration class.
            /// </summary>
            /// <param name="rows">The number of rows in the grid.</param>
            /// <param name="columns">The number of columns in the grid.</param>
            public LayoutConfiguration(int rows, int columns)
            {
                if (rows < 1)
                    throw new ArgumentOutOfRangeException(nameof(rows), "Number of rows must be at least 1.");

                if (columns < 1)
                    throw new ArgumentOutOfRangeException(nameof(columns), "Number of columns must be at least 1.");

                Rows = rows;
                Columns = columns;

                // Initialize with equal distribution by default
                RowSizes = new List<float>();
                for (int i = 0; i < rows; i++)
                {
                    RowSizes.Add(100f / rows);
                }

                ColumnSizes = new List<float>();
                for (int i = 0; i < columns; i++)
                {
                    ColumnSizes.Add(100f / columns);
                }
            }
        }

        #region Flow Layout Methods

        /// <summary>
        /// Creates a flow layout container using Syncfusion FlowLayout or FlowLayoutPanel for wrapping.
        /// Uses only documented Syncfusion methods and properties.
        /// </summary>
        public static Panel CreateFlowLayoutContainer(Control parent, bool wrapContents = false, int hGap = 10, int vGap = 10)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DefaultBackgroundColor
            };

            if (wrapContents)
            {
                // Use standard FlowLayoutPanel for wrapping as documented in Syncfusion guides
                var flowPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    AutoScroll = true,
                    BackColor = DefaultBackgroundColor
                };
                container.Controls.Add(flowPanel);
                container.Tag = flowPanel; // Store reference for later use
            }
            else
            {
                // Use Syncfusion FlowLayout for non-wrapping scenarios
                try
                {
                    var flowLayout = new FlowLayout
                    {
                        ContainerControl = container,
                        HGap = hGap,
                        VGap = vGap
                    };
                    container.Tag = flowLayout; // Store reference for later use
                }
                catch (Exception ex)
                {
                    // Fallback to standard layout if Syncfusion FlowLayout fails
                    // This can happen during testing or if licensing issues occur
                    BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                        $"FlowLayout creation failed, using standard layout: {ex.Message}");

                    // Use standard panel with manual layout as fallback
                    container.Tag = new { HGap = hGap, VGap = vGap }; // Store gap settings for manual layout
                }
            }

            parent.Controls.Add(container);
            return container;
        }

        /// <summary>
        /// Creates a flow layout panel with the specified flow direction and padding.
        /// Ideal for dynamic content that needs to automatically arrange in a flow pattern.
        /// </summary>
        /// <param name="parent">The parent control to add the flow layout to. Cannot be null.</param>
        /// <param name="flowDirection">The direction for controls to flow. Default is left to right.</param>
        /// <param name="padding">The padding to apply to the flow layout, in pixels. Default is 5.</param>
        /// <returns>A configured FlowLayoutPanel added to the parent control.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        public static FlowLayoutPanel CreateFlowLayoutPanel(Control parent, FlowDirection flowDirection = FlowDirection.LeftToRight, int padding = 5)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = flowDirection,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(padding),
                BackColor = DefaultBackgroundColor
            };

            // Add to parent
            parent.Controls.Add(flowPanel);

            return flowPanel;
        }

        /// <summary>
        /// Creates a flow layout panel specifically designed for card-based interfaces.
        /// Optimized for displaying multiple card controls with consistent spacing and arrangement.
        /// </summary>
        /// <param name="parent">The parent control to add the card flow layout to. Cannot be null.</param>
        /// <param name="cardSpacing">The spacing between cards, in pixels. Default is 10.</param>
        /// <returns>A configured FlowLayoutPanel optimized for card layouts.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        public static FlowLayoutPanel CreateCardFlowLayout(Control parent, int cardSpacing = 10)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var cardFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(cardSpacing),
                BackColor = DefaultBackgroundColor
            };

            // Add to parent
            parent.Controls.Add(cardFlow);

            return cardFlow;
        }

        /// <summary>
        /// Adds a visual break in a flow layout by setting increased top margin on the specified control.
        /// Useful for creating visual separation between groups of controls in a flow layout.
        /// </summary>
        /// <param name="container">The container with flow layout. Cannot be null.</param>
        /// <param name="control">The control to add a break before. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when container or control is null.</exception>
        public static void SetBreak(Control container, Control control)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            // Handle different flow layout types
            if (container.Tag is FlowLayoutPanel)
            {
                // For standard FlowLayoutPanel, use margins for spacing
                Padding currentMargin = control.Margin;
                control.Margin = new Padding(currentMargin.Left, 10, currentMargin.Right, currentMargin.Bottom);
            }
            else if (container.Tag is FlowLayout flowLayout)
            {
                // For Syncfusion FlowLayout, we can use documented methods
                // Create visual break through increased margins
                Padding currentMargin = control.Margin;
                control.Margin = new Padding(currentMargin.Left, 10, currentMargin.Right, currentMargin.Bottom);
            }
            else
            {
                // For any other container type, use standard margin approach
                Padding currentMargin = control.Margin;
                control.Margin = new Padding(currentMargin.Left, 10, currentMargin.Right, currentMargin.Bottom);
            }
        }

        #endregion

        #region Card Layout Methods

        /// <summary>
        /// Creates a card layout container using Syncfusion CardLayout.
        /// Uses only documented Syncfusion methods and properties.
        /// </summary>
        public static Panel CreateCardLayoutContainer(Control parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            // Use Dashboard's shared logging method
            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"CreateCardLayoutContainer called with parent: {parent.GetType().Name} '{parent.Name}'");
            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Parent state - Size: {parent.Size}, Controls: {parent.Controls.Count}, Visible: {parent.Visible}");

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DefaultBackgroundColor
            };

            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Created container panel - Size: {container.Size}, Dock: {container.Dock}");

            try
            {
                // Create Syncfusion CardLayout using documented method
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", "Creating Syncfusion CardLayout...");
                var cardLayout = new CardLayout
                {
                    ContainerControl = container
                };
                container.Tag = cardLayout; // Store reference for later use
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", "✅ CardLayout created and assigned to container");
            }
            catch (Exception ex)
            {
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"❌ Error creating CardLayout: {ex.GetType().Name}: {ex.Message}");
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"CardLayout error stack: {ex.StackTrace}");
            }

            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Adding container to parent '{parent.Name}'...");
            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Parent controls count before add: {parent.Controls.Count}");

            parent.Controls.Add(container);

            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"✅ Container added to parent. Parent controls count after add: {parent.Controls.Count}");
            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Container final state - Size: {container.Size}, Parent: {container.Parent?.Name ?? "null"}, Visible: {container.Visible}");

            return container;
        }

        /// <summary>
        /// Shows a specific card in a card layout container.
        /// Uses only documented Syncfusion CardLayout methods.
        /// </summary>
        public static void ShowCard(Control container, Control cardToShow)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (cardToShow == null)
                throw new ArgumentNullException(nameof(cardToShow));

            // Verify card is in the container
            if (!container.Controls.Contains(cardToShow))
                throw new ArgumentException("The specified card is not in the container.", nameof(cardToShow));

            var cardLayout = container.Tag as CardLayout;
            if (cardLayout == null)
                throw new InvalidOperationException("Container does not have a CardLayout.");

            // Get or assign card name using documented Syncfusion method
            string cardName = cardLayout.GetCardName(cardToShow);
            if (string.IsNullOrEmpty(cardName))
            {
                // Use documented GetNewCardName method to assign a unique name
                cardName = cardLayout.GetNewCardName();
                cardLayout.SetCardName(cardToShow, cardName);
            }

            // Show the card using documented SelectedCard property
            cardLayout.SelectedCard = cardName;
        }

        /// <summary>
        /// Creates a card container for displaying information in a card-like interface.
        /// Provides a consistent visual style for card-based UI components.
        /// </summary>
        /// <param name="title">The title text for the card. Cannot be null or empty.</param>
        /// <param name="width">The width of the card in pixels. Default is 250.</param>
        /// <param name="height">The height of the card in pixels. Default is 200.</param>
        /// <param name="borderColor">The color for the card border. Default is DarkGray.</param>
        /// <returns>A Panel configured as a card container with proper styling.</returns>
        /// <exception cref="ArgumentException">Thrown when title is null or empty.</exception>
        public static Panel CreateCardContainer(string title, int width = 250, int height = 200, Color? borderColor = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Card title cannot be null or empty.", nameof(title));

            Color cardBorder = borderColor ?? Color.DarkGray;

            // Create card panel
            var cardPanel = new Panel
            {
                Width = width,
                Height = height,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                Padding = new Padding(10)
            };

            // Add title label
            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Add content panel
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // Add controls to card
            cardPanel.Controls.Add(contentPanel);
            cardPanel.Controls.Add(titleLabel);

            // Store content panel reference for later access
            cardPanel.Tag = contentPanel;

            return cardPanel;
        }

        #endregion

        #region Table Layout Methods

        /// <summary>
        /// Creates a table layout panel container with the specified number of rows and columns.
        /// Provides a structured grid layout for organizing controls in rows and columns.
        /// </summary>
        /// <param name="parent">The parent control to add the table layout to. Cannot be null.</param>
        /// <param name="rows">The number of rows in the table layout.</param>
        /// <param name="columns">The number of columns in the table layout.</param>
        /// <param name="padding">The padding to apply to the table layout, in pixels. Default is 5.</param>
        /// <returns>A configured TableLayoutPanel added to the parent control.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when rows or columns are less than 1.</exception>
        public static TableLayoutPanel CreateTableLayoutContainer(Control parent, int rows, int columns, int padding = 5)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (rows < 1)
                throw new ArgumentOutOfRangeException(nameof(rows), "Number of rows must be at least 1.");

            if (columns < 1)
                throw new ArgumentOutOfRangeException(nameof(columns), "Number of columns must be at least 1.");

            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = rows,
                ColumnCount = columns,
                Padding = new Padding(padding),
                BackColor = DefaultBackgroundColor
            };

            // Configure equal-sized rows
            for (int i = 0; i < rows; i++)
            {
                tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / rows));
            }

            // Configure equal-sized columns
            for (int i = 0; i < columns; i++)
            {
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
            }

            // Add to parent
            parent.Controls.Add(tablePanel);

            return tablePanel;
        }

        /// <summary>
        /// Creates a specialized dashboard layout with a statistics row at the top and content area below.
        /// Provides a common dashboard structure with a narrow top section for statistics and larger bottom section for main content.
        /// </summary>
        /// <param name="parentControl">The parent control that will contain the dashboard layout. Cannot be null.</param>
        /// <param name="topRowHeight">Percentage height of the top statistics row (0-100). Default is 20%.</param>
        /// <returns>A configured TableLayoutPanel with dashboard-specific layout structure.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parentControl is null.</exception>
        /// <remarks>
        /// Creates a 2-row layout where the top row takes the specified percentage and the bottom row takes the remainder.
        /// The bottom row contains a nested TableLayoutPanel with two columns (60%/40% split) for main content areas.
        /// This layout is commonly used for dashboard interfaces with statistics at the top and charts/data below.
        /// </remarks>
        public static TableLayoutPanel CreateDashboardLayout(Control parentControl, float topRowHeight = 20f)
        {
            if (parentControl == null)
                throw new ArgumentNullException(nameof(parentControl));

            if (topRowHeight <= 0 || topRowHeight >= 100)
                throw new ArgumentOutOfRangeException(nameof(topRowHeight), "Top row height must be between 1 and 99 percent.");

            // Create main table layout panel
            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = DefaultBackgroundColor
            };

            // Configure row styles (top row for stats, bottom row for content)
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, topRowHeight));
            tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f - topRowHeight));

            // Create content row table
            var contentTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 2,
                BackColor = DefaultBackgroundColor
            };

            // Configure content columns (60/40 split)
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // Add content table to main table
            tablePanel.Controls.Add(contentTable, 0, 1);

            // Add the main table to parent
            parentControl.Controls.Add(tablePanel);

            return tablePanel;
        }

        #endregion

        #region Responsive Grid Layout Methods

        /// <summary>
        /// Creates a responsive grid layout using TableLayoutPanel with the specified configuration.
        /// The grid automatically adjusts column and row sizes based on the provided LayoutConfiguration.
        /// </summary>
        /// <param name="parent">The parent control to contain the grid layout. Cannot be null.</param>
        /// <param name="config">The layout configuration specifying grid dimensions and sizes. Cannot be null.</param>
        /// <returns>A Panel containing the configured TableLayoutPanel in its Tag property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent or config is null.</exception>
        public static Panel CreateResponsiveGridLayout(Control parent, LayoutConfiguration config)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // Create container panel
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DefaultBackgroundColor
            };

            // Create the table layout panel
            var tablePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = config.Rows,
                ColumnCount = config.Columns,
                BackColor = DefaultBackgroundColor
            };

            // Configure row styles
            for (int i = 0; i < config.Rows; i++)
            {
                float rowSize = (config.RowSizes != null && i < config.RowSizes.Count)
                    ? config.RowSizes[i]
                    : (100f / config.Rows);
                tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, rowSize));
            }

            // Configure column styles
            for (int i = 0; i < config.Columns; i++)
            {
                float columnSize = (config.ColumnSizes != null && i < config.ColumnSizes.Count)
                    ? config.ColumnSizes[i]
                    : (100f / config.Columns);
                tablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, columnSize));
            }

            // Add table panel to container and store reference in Tag
            container.Controls.Add(tablePanel);
            container.Tag = tablePanel;

            // Add container to parent
            parent.Controls.Add(container);

            return container;
        }

        #endregion

        #region Split Container Methods

        /// <summary>
        /// Configures a SplitContainer with safe splitter distance that prevents invalid value exceptions.
        /// Addresses the "SplitterDistance must be between Panel1MinSize and Width - Panel2MinSize" error.
        /// </summary>
        /// <param name="splitContainer">The SplitContainer to configure. Cannot be null.</param>
        /// <param name="desiredDistance">The desired splitter distance in pixels.</param>
        /// <param name="panel1MinSize">Minimum size for panel 1. Default is 50 pixels.</param>
        /// <param name="panel2MinSize">Minimum size for panel 2. Default is 50 pixels.</param>
        /// <returns>The configured SplitContainer with safely applied splitter distance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when splitContainer is null.</exception>
        /// <remarks>
        /// This method ensures that the splitter distance is set to a valid value that respects
        /// the minimum sizes of both panels, preventing the common runtime exception.
        /// </remarks>
        public static SplitContainer ConfigureSafeSplitContainer(SplitContainer splitContainer, int desiredDistance,
            int panel1MinSize = 50, int panel2MinSize = 50)
        {
            if (splitContainer == null)
                throw new ArgumentNullException(nameof(splitContainer));

            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"Configuring SplitContainer - Initial Size: {splitContainer.Width}x{splitContainer.Height}");

            // Set panel minimum sizes
            splitContainer.Panel1MinSize = panel1MinSize;
            splitContainer.Panel2MinSize = panel2MinSize;

            // Calculate safe bounds for splitter distance
            int minDistance = splitContainer.Panel1MinSize;
            int maxDistance = Math.Max(minDistance, splitContainer.Width - splitContainer.Panel2MinSize);

            // Ensure valid splitter distance within bounds
            int safeDistance = Math.Max(minDistance, Math.Min(desiredDistance, maxDistance));

            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                $"SplitContainer - Min: {minDistance}, Max: {maxDistance}, Desired: {desiredDistance}, Safe: {safeDistance}");

            // Apply safe distance only if the control has a valid size
            if (splitContainer.Width > panel1MinSize + panel2MinSize)
            {
                splitContainer.SplitterDistance = safeDistance;
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager", $"✅ SplitterDistance set to {safeDistance}");
            }
            else
            {
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                    $"⚠️ Deferring SplitterDistance - Container too small ({splitContainer.Width}px)");

                // Add resize handler to set splitter distance once the container has a valid size
                splitContainer.Resize += (sender, e) =>
                {
                    var container = sender as SplitContainer;
                    if (container != null && container.Width > container.Panel1MinSize + container.Panel2MinSize)
                    {
                        int dynamicMax = Math.Max(container.Panel1MinSize, container.Width - container.Panel2MinSize);
                        int dynamicSafe = Math.Max(container.Panel1MinSize, Math.Min(desiredDistance, dynamicMax));

                        if (container.SplitterDistance != dynamicSafe)
                        {
                            container.SplitterDistance = dynamicSafe;
                            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                                $"✅ SplitterDistance updated on resize to {dynamicSafe}");
                        }
                    }
                };
            }

            return splitContainer;
        }

        /// <summary>
        /// Creates a SplitContainer with the specified orientation and panel sizing.
        /// Provides a reliable way to create properly configured split layouts.
        /// </summary>
        /// <param name="parent">The parent control to add the SplitContainer to. Cannot be null.</param>
        /// <param name="orientation">The orientation of the splitter (Horizontal or Vertical).</param>
        /// <param name="panel1Percentage">The percentage of space for Panel1 (1-99). Default is 50%.</param>
        /// <param name="fixedPanel">Which panel should remain fixed during resize. Default is None.</param>
        /// <returns>A configured SplitContainer added to the parent control.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when panel1Percentage is not between 1 and 99.</exception>
        /// <remarks>
        /// This method creates a SplitContainer with safe configuration to prevent runtime exceptions
        /// related to splitter distance and panel sizing.
        /// </remarks>
        public static SplitContainer CreateSplitContainer(Control parent, Orientation orientation,
            int panel1Percentage = 50, FixedPanel fixedPanel = FixedPanel.None)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (panel1Percentage < 1 || panel1Percentage > 99)
                throw new ArgumentOutOfRangeException(nameof(panel1Percentage), "Panel1 percentage must be between 1 and 99.");

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = orientation,
                FixedPanel = fixedPanel,
                BackColor = DefaultBackgroundColor
            };

            // Calculate initial splitter distance based on percentage
            int splitterDistance;
            if (orientation == Orientation.Vertical)
            {
                // For vertical orientation, width determines the splitter distance
                splitterDistance = (parent.Width * panel1Percentage) / 100;
            }
            else
            {
                // For horizontal orientation, height determines the splitter distance
                splitterDistance = (parent.Height * panel1Percentage) / 100;
            }

            // Configure with safe splitter distance
            ConfigureSafeSplitContainer(splitContainer, splitterDistance);

            // Add to parent
            parent.Controls.Add(splitContainer);
            return splitContainer;
        }

        #endregion

        #region Responsive Dashboard Methods

        /// <summary>
        /// Creates a responsive dashboard container that adjusts layout based on available space.
        /// Automatically switches between horizontal and vertical layouts depending on container dimensions.
        /// </summary>
        /// <param name="parent">The parent control to add the responsive dashboard to. Cannot be null.</param>
        /// <param name="widthThreshold">The width threshold in pixels below which the layout switches to vertical. Default is 800px.</param>
        /// <param name="horizontalSplitRatio">The panel1 percentage when in horizontal layout. Default is 70%.</param>
        /// <param name="verticalSplitRatio">The panel1 percentage when in vertical layout. Default is 40%.</param>
        /// <returns>A responsive container with adaptive layout based on available space.</returns>
        /// <exception cref="ArgumentNullException">Thrown when parent is null.</exception>
        /// <remarks>
        /// This method creates a container that automatically adjusts its layout based on available space,
        /// providing optimal viewing experience across different screen sizes and resolutions.
        /// The returned Panel contains a SplitContainer that changes orientation when the container width
        /// crosses the specified threshold.
        /// </remarks>
        public static Panel CreateResponsiveDashboard(Control parent, int widthThreshold = 800,
            int horizontalSplitRatio = 70, int verticalSplitRatio = 40)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            // Create container panel
            var containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DefaultBackgroundColor
            };

            // Create split container (initially horizontal)
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                BackColor = DefaultBackgroundColor
            };

            // Log initial setup
            BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                $"Creating responsive dashboard - Width: {parent.Width}, Height: {parent.Height}, Threshold: {widthThreshold}");

            // Configure initial splitter distance based on orientation
            UpdateSplitterBasedOnSize(splitContainer, parent.Width, widthThreshold, horizontalSplitRatio, verticalSplitRatio);

            // Add resize handler to adjust layout based on container size
            containerPanel.Resize += (sender, e) =>
            {
                var panel = sender as Panel;
                if (panel != null && panel.Controls.Count > 0 && panel.Controls[0] is SplitContainer)
                {
                    var split = panel.Controls[0] as SplitContainer;
                    UpdateSplitterBasedOnSize(split, panel.Width, widthThreshold, horizontalSplitRatio, verticalSplitRatio);
                }
            };

            // Add split container to panel
            containerPanel.Controls.Add(splitContainer);

            // Add panel to parent
            parent.Controls.Add(containerPanel);

            return containerPanel;
        }

        /// <summary>
        /// Helper method to update splitter orientation and distance based on container width.
        /// </summary>
        private static void UpdateSplitterBasedOnSize(SplitContainer splitContainer, int containerWidth,
            int widthThreshold, int horizontalSplitRatio, int verticalSplitRatio)
        {
            try
            {
                bool isWideLayout = containerWidth >= widthThreshold;

                // Set orientation based on container width
                Orientation newOrientation = isWideLayout ? Orientation.Horizontal : Orientation.Vertical;

                // Log orientation change if needed
                if (splitContainer.Orientation != newOrientation)
                {
                    BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                        $"Changing orientation from {splitContainer.Orientation} to {newOrientation}");
                    splitContainer.Orientation = newOrientation;
                }

                // Calculate appropriate splitter distance based on orientation
                int splitterDistance;
                if (isWideLayout)
                {
                    // Horizontal layout - use horizontalSplitRatio
                    splitterDistance = (containerWidth * horizontalSplitRatio) / 100;
                }
                else
                {
                    // Vertical layout - use verticalSplitRatio
                    splitterDistance = (splitContainer.Height * verticalSplitRatio) / 100;
                }

                // Apply safe splitter distance if container has valid size
                if ((newOrientation == Orientation.Horizontal && containerWidth > splitContainer.Panel1MinSize + splitContainer.Panel2MinSize) ||
                    (newOrientation == Orientation.Vertical && splitContainer.Height > splitContainer.Panel1MinSize + splitContainer.Panel2MinSize))
                {
                    int minDistance = splitContainer.Panel1MinSize;
                    int maxDistance = newOrientation == Orientation.Horizontal
                        ? Math.Max(minDistance, containerWidth - splitContainer.Panel2MinSize)
                        : Math.Max(minDistance, splitContainer.Height - splitContainer.Panel2MinSize);

                    int safeDistance = Math.Max(minDistance, Math.Min(splitterDistance, maxDistance));

                    if (splitContainer.SplitterDistance != safeDistance)
                    {
                        splitContainer.SplitterDistance = safeDistance;
                        BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                            $"Updated SplitterDistance to {safeDistance} for {newOrientation} layout");
                    }
                }
            }
            catch (Exception ex)
            {
                BusBuddy.UI.Views.Dashboard.LogToSharedFile("DynamicLayoutManager",
                    $"Error updating splitter: {ex.GetType().Name}: {ex.Message}");
            }
        }

        #endregion

        #region Misc Layout Methods

        /// <summary>
        /// Applies a consistent margin to all controls within a container.
        /// Useful for ensuring uniform spacing in any layout type.
        /// </summary>
        /// <param name="container">The container with controls to adjust margins for. Cannot be null.</param>
        /// <param name="margin">The margin to apply to all sides, in pixels.</param>
        /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
        public static void ApplyUniformMargins(Control container, int margin)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            foreach (Control control in container.Controls)
            {
                control.Margin = new Padding(margin);
            }
        }

        /// <summary>
        /// Clears all controls from a layout container and properly disposes of them.
        /// </summary>
        /// <param name="container">The container to clear. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
        public static void ClearLayoutContainer(Control container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            // Properly dispose of all controls
            while (container.Controls.Count > 0)
            {
                var control = container.Controls[0];
                container.Controls.Remove(control);
                control.Dispose();
            }

            // Reset layout if applicable
            if (container.Tag is CardLayout cardLayout)
            {
                cardLayout.SelectedCard = null;
            }
            else if (container.Tag is FlowLayout)
            {
                // FlowLayout reset if needed
            }
        }

        #endregion
    }
}

