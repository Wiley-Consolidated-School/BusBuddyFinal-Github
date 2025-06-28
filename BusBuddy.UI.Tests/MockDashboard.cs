using System;
using System.Windows.Forms;
using System.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.Controls;
using BusBuddy.UI.Views;

namespace BusBuddy.UI.Tests
{
    /// <summary>
    /// A simplified mock dashboard for testing that avoids initialization issues
    /// </summary>
    public class MockDashboard : SfForm
    {
        public TableLayoutPanel MainTableLayout { get; private set; }
        public TabControlAdv TabControl { get; private set; }

        public MockDashboard()
        {
            this.Text = "BusBuddy Dashboard (Mock)";
            this.Size = new Size(1200, 800);

            // Create basic structure similar to a real dashboard
            MainTableLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                Dock = DockStyle.Fill
            };

            // Set column styles
            MainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            MainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            // Set row styles
            MainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            MainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Create tab control
            TabControl = new TabControlAdv
            {
                Dock = DockStyle.Fill
            };

            // Add some tabs
            var tab1 = new TabPageAdv("Dashboard");
            var tab2 = new TabPageAdv("Reports");
            var tab3 = new TabPageAdv("Settings");

            TabControl.TabPages.Add(tab1);
            TabControl.TabPages.Add(tab2);
            TabControl.TabPages.Add(tab3);

            // Add the tab control to the table layout
            MainTableLayout.Controls.Add(TabControl, 0, 0);
            MainTableLayout.SetColumnSpan(TabControl, 2);

            // Add the table layout to the form
            this.Controls.Add(MainTableLayout);
        }
    }
}

