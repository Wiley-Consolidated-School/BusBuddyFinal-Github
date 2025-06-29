using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;

namespace YourNamespace
{
    public partial class AnalyticsDemoFormSyncfusion : MetroForm
    {
        private System.IServiceProvider _serviceProvider;
        public AnalyticsDemoFormSyncfusion(System.IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Your initialization code here
        }
    }
}
