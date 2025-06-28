using System;
using System.Collections.Generic;

namespace BusBuddy.UI.Layout
{
    /// <summary>
    /// Configuration class for responsive grid layouts.
    /// Provides a flexible way to define grid dimensions and cell sizing.
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
        /// If null or fewer items than Rows, remaining rows will have equal distribution.
        /// </summary>
        public List<float> RowSizes { get; set; }

        /// <summary>
        /// Gets or sets the percentage sizes for each column.
        /// If null or fewer items than Columns, remaining columns will have equal distribution.
        /// </summary>
        public List<float> ColumnSizes { get; set; }

        /// <summary>
        /// Creates a new layout configuration with the specified dimensions.
        /// </summary>
        /// <param name="rows">Number of rows in the grid.</param>
        /// <param name="columns">Number of columns in the grid.</param>
        public LayoutConfiguration(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
        }

        /// <summary>
        /// Creates a predefined 2x2 grid layout with equal cell sizes.
        /// </summary>
        /// <returns>A layout configuration with 2 rows and 2 columns of equal size.</returns>
        public static LayoutConfiguration CreateEqual2x2Grid()
        {
            return new LayoutConfiguration(2, 2);
        }

        /// <summary>
        /// Creates a predefined dashboard layout with a header row and content area.
        /// The header row takes 20% of height, and the content area takes 80%.
        /// </summary>
        /// <returns>A layout configuration optimized for dashboard interfaces.</returns>
        public static LayoutConfiguration CreateDashboardLayout()
        {
            return new LayoutConfiguration(2, 1)
            {
                RowSizes = new List<float> { 20f, 80f }
            };
        }
    }
}

