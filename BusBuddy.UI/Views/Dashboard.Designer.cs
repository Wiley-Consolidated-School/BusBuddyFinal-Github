namespace BusBuddy.UI
{
    partial class Dashboard
    {
        private System.ComponentModel.IContainer components = null;
        private Syncfusion.Windows.Forms.Tools.DockingManager dockingManager1;
        private Panel navigationPanel;
        private Panel dataGridPanel;
        private Panel managementPanel;
        private Syncfusion.WinForms.DataGrid.SfDataGrid sfDataGrid1;
        private Syncfusion.WinForms.Controls.SfButton sfButtonRefresh;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dockingManager1 = new Syncfusion.Windows.Forms.Tools.DockingManager(this.components);
            this.navigationPanel = new Panel();
            this.dataGridPanel = new Panel();
            this.managementPanel = new Panel();
            this.sfDataGrid1 = new Syncfusion.WinForms.DataGrid.SfDataGrid();
            this.sfButtonRefresh = new Syncfusion.WinForms.Controls.SfButton();
            this.dataGridPanel.SuspendLayout();
            this.SuspendLayout();
            // navigationPanel
            this.navigationPanel.Name = "navigationPanel";
            this.navigationPanel.Size = new System.Drawing.Size(200, 600);
            // dataGridPanel
            this.dataGridPanel.Controls.Add(this.sfDataGrid1);
            this.dataGridPanel.Controls.Add(this.sfButtonRefresh);
            this.dataGridPanel.Name = "dataGridPanel";
            this.dataGridPanel.Size = new System.Drawing.Size(400, 600);
            // managementPanel
            this.managementPanel.Name = "managementPanel";
            this.managementPanel.Size = new System.Drawing.Size(200, 600);
            // sfDataGrid1
            this.sfDataGrid1.AllowEditing = true;
            this.sfDataGrid1.Location = new System.Drawing.Point(10, 40);
            this.sfDataGrid1.Name = "sfDataGrid1";
            this.sfDataGrid1.Size = new System.Drawing.Size(380, 500);
            // Commented out event handler for missing types
            // this.sfDataGrid1.SelectionChanged += (s, e) =>
            // {
            //     if (sfDataGrid1.SelectedItem is Vehicle vehicle)
            //     {
            //         var editForm = new VehicleEditForm(vehicle);
            //         editForm.FormClosed += async (s2, e2) => await ((Dashboard)this).LoadVehicleData();
            //         editForm.ShowDialog();
            //     }
            //     else if (sfDataGrid1.SelectedItem is Driver driver)
            //     {
            //         var editForm = new DriverEditForm(driver);
            //         editForm.FormClosed += async (s2, e2) => await ((Dashboard)this).LoadDriverData();
            //         editForm.ShowDialog();
            //     }
            // };
            // sfButtonRefresh
            this.sfButtonRefresh.Location = new System.Drawing.Point(10, 10);
            this.sfButtonRefresh.Name = "sfButtonRefresh";
            this.sfButtonRefresh.Size = new System.Drawing.Size(100, 30);
            this.sfButtonRefresh.Text = "Refresh";
            // Dashboard
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.navigationPanel);
            this.Controls.Add(this.dataGridPanel);
            this.Controls.Add(this.managementPanel);
            this.Name = "Dashboard";
            this.Text = "BusBuddy Dashboard";
            this.dataGridPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            // Dock navigationPanel to the left using Syncfusion v31 API
            this.dockingManager1.DockControl(this.navigationPanel, this, Syncfusion.Windows.Forms.Tools.DockingStyle.Left, 200);
        }
    }
}
