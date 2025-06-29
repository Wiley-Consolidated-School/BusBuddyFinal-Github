using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;

namespace BusBuddy.UI.Views
{
    // Demo form for analytics dashboard using Syncfusion MetroForm
    public partial class AnalyticsDemoFormSyncfusion : MetroForm
    {
        private readonly IServiceProvider _serviceProvider;

        public AnalyticsDemoFormSyncfusion(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Analytics Demo";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            // Example: Add a Syncfusion ChartControl (documented usage)
            // See: https://help.syncfusion.com/windowsforms/chart/getting-started
            // var chart = new Syncfusion.Windows.Forms.Chart.ChartControl();
            // chart.Dock = DockStyle.Fill;
            // this.Controls.Add(chart);
            // Add additional Syncfusion controls as needed, following documentation
        }
    }
}

