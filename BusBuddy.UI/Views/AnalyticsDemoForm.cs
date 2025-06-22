using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusBuddy.Business;
using BusBuddy.Models;
using BusBuddy.UI.Base;
using BusBuddy.UI.Helpers;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;

namespace BusBuddy.UI.Views
{    /// <summary>
    /// Analytics Demo Form - Enhanced Syncfusion Implementation
    /// Showcases analytics and optimization features with advanced SfDataGrid
    /// Displays route efficiency, predictive maintenance, and time card improvements
    /// </summary>
    public class AnalyticsDemoFormSyncfusion : SyncfusionBaseForm
    {
        private readonly IRouteAnalyticsService _routeAnalyticsService;
        private readonly IPredictiveMaintenanceService _predictiveMaintenanceService;

        private TabControl _tabControl;
        private TabPage _routeAnalyticsTab;
        private TabPage _maintenanceTab;
        private TabPage _timeCardTab;        // Route Analytics Controls - Enhanced with SfDataGrid
        private SfDataGrid? _routeEfficiencyGrid;
        private SfDataGrid? _optimizationSuggestionsGrid;
        private Panel? _fleetSummaryPanel;

        // Maintenance Controls - Enhanced with SfDataGrid
        private SfDataGrid? _maintenancePredictionsGrid;
        private SfDataGrid? _vehicleHealthGrid;
        private SfDataGrid? _maintenanceAlertsGrid;

        // Time Card Controls
        private Button? _validateTimeCardButton;
        private Panel? _validationResultsPanel;

        public AnalyticsDemoFormSyncfusion()
        {
            _routeAnalyticsService = new RouteAnalyticsService();
            _predictiveMaintenanceService = new PredictiveMaintenanceService();

            InitializeComponent();
            LoadDemoData();
        }

        private void InitializeComponent()
        {
            Text = "BusBuddy Analytics & Optimization Demo";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;

            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;

            CreateRouteAnalyticsTab();
            CreateMaintenanceTab();
            CreateTimeCardTab();

            _tabControl.TabPages.Add(_routeAnalyticsTab);
            _tabControl.TabPages.Add(_maintenanceTab);
            _tabControl.TabPages.Add(_timeCardTab);

            Controls.Add(_tabControl);
        }

        private void CreateRouteAnalyticsTab()
        {
            _routeAnalyticsTab = new TabPage("Route Analytics & Optimization");
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 3;
            layout.ColumnCount = 2;

            // Row styles
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            // Column styles
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Route Efficiency Grid - Enhanced SfDataGrid with specific columns only
            var efficiencyGroup = new GroupBox();
            efficiencyGroup.Text = "📊 Route Efficiency Metrics";
            _routeEfficiencyGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _routeEfficiencyGrid.Dock = DockStyle.Fill;
            _routeEfficiencyGrid.AutoGenerateColumns = false; // Fix excessive columns issue
            _routeEfficiencyGrid.AllowEditing = false;

            // Define only relevant columns for route efficiency
            _routeEfficiencyGrid.Columns.Add(new GridTextColumn { MappingName = "RouteName", HeaderText = "Route", Width = 120 });
            _routeEfficiencyGrid.Columns.Add(new GridNumericColumn { MappingName = "EfficiencyScore", HeaderText = "Efficiency (%)", Width = 100 });
            _routeEfficiencyGrid.Columns.Add(new GridNumericColumn { MappingName = "CostPerRider", HeaderText = "Cost/Rider ($)", Width = 110 });
            _routeEfficiencyGrid.Columns.Add(new GridNumericColumn { MappingName = "MilesPerRider", HeaderText = "Miles/Rider", Width = 100 });
            _routeEfficiencyGrid.Columns.Add(new GridNumericColumn { MappingName = "FuelEfficiency", HeaderText = "MPG", Width = 80 });

            SyncfusionThemeHelper.SfDataGridEnhancements(_routeEfficiencyGrid);
            efficiencyGroup.Controls.Add(_routeEfficiencyGrid);
            layout.Controls.Add(efficiencyGroup, 0, 0);

            // Optimization Suggestions Grid - Enhanced SfDataGrid with specific columns
            var optimizationGroup = new GroupBox();
            optimizationGroup.Text = "🎯 Route Optimization Suggestions";
            _optimizationSuggestionsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _optimizationSuggestionsGrid.Dock = DockStyle.Fill;
            _optimizationSuggestionsGrid.AutoGenerateColumns = false; // Fix excessive columns issue
            _optimizationSuggestionsGrid.AllowEditing = false;

            // Define only relevant columns for optimization suggestions
            _optimizationSuggestionsGrid.Columns.Add(new GridTextColumn { MappingName = "SuggestionType", HeaderText = "Type", Width = 100 });
            _optimizationSuggestionsGrid.Columns.Add(new GridTextColumn { MappingName = "Description", HeaderText = "Suggestion", Width = 200 });
            _optimizationSuggestionsGrid.Columns.Add(new GridNumericColumn { MappingName = "PotentialSavings", HeaderText = "Savings ($)", Width = 100 });
            _optimizationSuggestionsGrid.Columns.Add(new GridTextColumn { MappingName = "Priority", HeaderText = "Priority", Width = 80 });
            _optimizationSuggestionsGrid.Columns.Add(new GridTextColumn { MappingName = "Implementation", HeaderText = "Implementation", Width = 120 });

            SyncfusionThemeHelper.SfDataGridEnhancements(_optimizationSuggestionsGrid);
            optimizationGroup.Controls.Add(_optimizationSuggestionsGrid);
            layout.Controls.Add(optimizationGroup, 0, 1);

            // Fleet Summary Panel
            _fleetSummaryPanel = new Panel();
            _fleetSummaryPanel.BackColor = Color.LightBlue;
            _fleetSummaryPanel.BorderStyle = BorderStyle.FixedSingle;
            layout.Controls.Add(_fleetSummaryPanel, 1, 0);
            layout.SetRowSpan(_fleetSummaryPanel, 2);

            // Demo Actions Panel
            var actionsPanel = new Panel();
            var refreshButton = new Button();
            refreshButton.Text = "Refresh Analytics";
            refreshButton.Size = new Size(120, 30);
            refreshButton.Location = new Point(10, 10);
            refreshButton.Click += RefreshRouteAnalytics_Click;
            actionsPanel.Controls.Add(refreshButton);

            var exportButton = new Button();
            exportButton.Text = "Export Report";
            exportButton.Size = new Size(120, 30);
            exportButton.Location = new Point(140, 10);
            exportButton.Click += ExportAnalytics_Click;
            actionsPanel.Controls.Add(exportButton);

            layout.Controls.Add(actionsPanel, 0, 2);
            layout.SetColumnSpan(actionsPanel, 2);

            _routeAnalyticsTab.Controls.Add(layout);
        }

        private void CreateMaintenanceTab()
        {
            _maintenanceTab = new TabPage("Predictive Maintenance");
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 2;
            layout.ColumnCount = 2;

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Maintenance Predictions - Enhanced SfDataGrid with specific columns
            var predictionsGroup = new GroupBox();
            predictionsGroup.Text = "🔧 Maintenance Predictions";
            _maintenancePredictionsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _maintenancePredictionsGrid.Dock = DockStyle.Fill;
            _maintenancePredictionsGrid.AutoGenerateColumns = false; // Fix excessive columns issue
            _maintenancePredictionsGrid.AllowEditing = false;

            // Define only relevant columns for maintenance predictions
            _maintenancePredictionsGrid.Columns.Add(new GridNumericColumn { MappingName = "VehicleNumber", HeaderText = "Vehicle #", Width = 100 });
            _maintenancePredictionsGrid.Columns.Add(new GridTextColumn { MappingName = "MaintenanceType", HeaderText = "Maintenance Type", Width = 140 });
            _maintenancePredictionsGrid.Columns.Add(new GridDateTimeColumn { MappingName = "PredictedDate", HeaderText = "Predicted Date", Width = 120 });
            _maintenancePredictionsGrid.Columns.Add(new GridNumericColumn { MappingName = "HealthScore", HeaderText = "Health Score", Width = 100 });
            _maintenancePredictionsGrid.Columns.Add(new GridTextColumn { MappingName = "Priority", HeaderText = "Priority", Width = 80 });

            SyncfusionThemeHelper.SfDataGridEnhancements(_maintenancePredictionsGrid);
            predictionsGroup.Controls.Add(_maintenancePredictionsGrid);
            layout.Controls.Add(predictionsGroup, 0, 0);

            // Vehicle Health Scores - Enhanced SfDataGrid with specific columns
            var healthGroup = new GroupBox();
            healthGroup.Text = "🚗 Vehicle Health Scores";
            _vehicleHealthGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _vehicleHealthGrid.Dock = DockStyle.Fill;
            _vehicleHealthGrid.AutoGenerateColumns = false; // Fix excessive columns issue
            _vehicleHealthGrid.AllowEditing = false;

            // Define only relevant columns for vehicle health
            _vehicleHealthGrid.Columns.Add(new GridNumericColumn { MappingName = "VehicleNumber", HeaderText = "Vehicle #", Width = 100 });
            _vehicleHealthGrid.Columns.Add(new GridTextColumn { MappingName = "Model", HeaderText = "Model", Width = 120 });
            _vehicleHealthGrid.Columns.Add(new GridNumericColumn { MappingName = "OverallHealthScore", HeaderText = "Health Score", Width = 100 });
            _vehicleHealthGrid.Columns.Add(new GridNumericColumn { MappingName = "Mileage", HeaderText = "Mileage", Width = 100 });
            _vehicleHealthGrid.Columns.Add(new GridTextColumn { MappingName = "Status", HeaderText = "Status", Width = 100 });

            SyncfusionThemeHelper.SfDataGridEnhancements(_vehicleHealthGrid);
            healthGroup.Controls.Add(_vehicleHealthGrid);
            layout.Controls.Add(healthGroup, 1, 0);

            // Maintenance Alerts - Enhanced SfDataGrid with specific columns
            var alertsGroup = new GroupBox();
            alertsGroup.Text = "🚨 Priority Maintenance Alerts";
            _maintenanceAlertsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _maintenanceAlertsGrid.Dock = DockStyle.Fill;
            _maintenanceAlertsGrid.AutoGenerateColumns = false; // Fix excessive columns issue
            _maintenanceAlertsGrid.AllowEditing = false;

            // Define only relevant columns for maintenance alerts
            _maintenanceAlertsGrid.Columns.Add(new GridNumericColumn { MappingName = "VehicleNumber", HeaderText = "Vehicle #", Width = 100 });
            _maintenanceAlertsGrid.Columns.Add(new GridTextColumn { MappingName = "AlertType", HeaderText = "Alert Type", Width = 120 });
            _maintenanceAlertsGrid.Columns.Add(new GridTextColumn { MappingName = "Description", HeaderText = "Description", Width = 200 });
            _maintenanceAlertsGrid.Columns.Add(new GridTextColumn { MappingName = "Severity", HeaderText = "Severity", Width = 80 });
            _maintenanceAlertsGrid.Columns.Add(new GridDateTimeColumn { MappingName = "DueDate", HeaderText = "Due Date", Width = 100 });

            SyncfusionThemeHelper.SfDataGridEnhancements(_maintenanceAlertsGrid);
            alertsGroup.Controls.Add(_maintenanceAlertsGrid);
            layout.Controls.Add(alertsGroup, 0, 1);
            layout.SetColumnSpan(alertsGroup, 2);

            _maintenanceTab.Controls.Add(layout);
        }

        private void CreateTimeCardTab()
        {
            _timeCardTab = new TabPage("Enhanced Time Card Validation");
            var layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.RowCount = 3;
            layout.ColumnCount = 1;

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Demo Description
            var descLabel = new Label();
            descLabel.Text = "Enhanced Time Card Validation with Smart Conflict Resolution";
            descLabel.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            descLabel.AutoSize = true;
            descLabel.Padding = new Padding(10);
            layout.Controls.Add(descLabel, 0, 0);

            // Validation Button
            _validateTimeCardButton = new Button();
            _validateTimeCardButton.Text = "Demonstrate Time Card Validation";
            _validateTimeCardButton.Size = new Size(250, 40);
            _validateTimeCardButton.Margin = new Padding(10);
            _validateTimeCardButton.Click += ValidateTimeCard_Click;
            layout.Controls.Add(_validateTimeCardButton, 0, 1);

            // Results Panel
            _validationResultsPanel = new Panel();
            _validationResultsPanel.Dock = DockStyle.Fill;
            _validationResultsPanel.BorderStyle = BorderStyle.FixedSingle;
            _validationResultsPanel.Margin = new Padding(10);
            layout.Controls.Add(_validationResultsPanel, 0, 2);

            _timeCardTab.Controls.Add(layout);
        }

        private async void LoadDemoData()
        {
            try
            {
                await LoadRouteAnalytics();
                await LoadMaintenanceData();
                LoadTimeCardFeatures();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading demo data: {ex.Message}", "Demo Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadRouteAnalytics()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            // Load route efficiency metrics
            var routeMetrics = await _routeAnalyticsService.GetRouteEfficiencyMetricsAsync(startDate, endDate);
            _routeEfficiencyGrid.DataSource = routeMetrics.Take(10).ToList(); // Show top 10

            // Load optimization suggestions
            var suggestions = await _routeAnalyticsService.AnalyzeRouteOptimizationsAsync(DateTime.Now);
            _optimizationSuggestionsGrid.DataSource = suggestions;

            // Load fleet summary
            var fleetSummary = await _routeAnalyticsService.GetFleetAnalyticsSummaryAsync(startDate, endDate);
            DisplayFleetSummary(fleetSummary);
        }

        private async Task LoadMaintenanceData()
        {
            // Load maintenance alerts
            var alerts = await _predictiveMaintenanceService.GetMaintenanceAlertsAsync();
            _maintenanceAlertsGrid.DataSource = alerts;

            // Load sample vehicle health data
            var healthScores = new List<VehicleHealthScore>();
            var sampleVehicleIds = new[] { 1, 2, 3, 4, 5 }; // Demo vehicle IDs

            foreach (var vehicleId in sampleVehicleIds)
            {
                try
                {
                    var healthScore = await _predictiveMaintenanceService.CalculateVehicleHealthScoreAsync(vehicleId);
                    healthScores.Add(healthScore);
                }
                catch
                {
                    // Create demo data if vehicle doesn't exist
                    healthScores.Add(CreateDemoHealthScore(vehicleId));
                }
            }
            _vehicleHealthGrid.DataSource = healthScores;

            // Load sample maintenance predictions
            var predictions = new List<MaintenancePrediction>();
            foreach (var vehicleId in sampleVehicleIds)
            {
                try
                {
                    var vehiclePredictions = await _predictiveMaintenanceService.GetMaintenancePredictionsAsync(vehicleId);
                    predictions.AddRange(vehiclePredictions);
                }
                catch
                {
                    // Create demo data if vehicle doesn't exist
                    predictions.AddRange(CreateDemoPredictions(vehicleId));
                }
            }
            _maintenancePredictionsGrid.DataSource = predictions.Take(15).ToList();
        }

        private void LoadTimeCardFeatures()
        {
            var featuresText = new RichTextBox();
            featuresText.Dock = DockStyle.Fill;
            featuresText.ReadOnly = true;
            featuresText.Text = @"Enhanced Time Card Validation Features:

✓ Smart Conflict Resolution
  • Automatic detection of overlapping time entries
  • Intelligent suggestions for fixing time sequence issues
  • Auto-fix capabilities for common problems

✓ Advanced Validation Rules
  • Missing time detection with smart defaults
  • Excessive hours warnings with optimization suggestions
  • Driver availability conflict checking

✓ Improved User Experience
  • Enhanced warning dialog with resolution options
  • Confidence-based auto-fix recommendations
  • Better error messaging with actionable solutions

Click the demonstration button to see the enhanced validation in action!";

            _validationResultsPanel.Controls.Add(featuresText);
        }

        private void DisplayFleetSummary(FleetAnalyticsSummary summary)
        {
            _fleetSummaryPanel.Controls.Clear();

            var summaryText = new RichTextBox();
            summaryText.Dock = DockStyle.Fill;
            summaryText.ReadOnly = true;
            summaryText.Font = new Font("Segoe UI", 10F);

            summaryText.Text = $@"Fleet Analytics Summary
Period: {summary.PeriodStart:yyyy-MM-dd} to {summary.PeriodEnd:yyyy-MM-dd}

📊 Key Metrics:
• Total Routes: {summary.TotalRoutes:N0}
• Total Miles: {summary.TotalMiles:N0}
• Total Riders: {summary.TotalRiders:N0}
• Average Efficiency: {summary.AverageEfficiencyScore:F1}%
• Miles per Rider: {summary.AverageMilesPerRider:F2}

🚌 Fleet Status:
• Vehicle Utilization: {summary.VehicleUtilizationRate:F1}%
• Estimated Fuel Costs: ${summary.EstimatedFuelCosts:N2}

🏆 Top Performing Routes:
{string.Join("\n", summary.TopPerformingRoutes.Select(r => $"• {r}"))}

💡 Performance Insights:
{GetPerformanceInsights(summary)}";

            _fleetSummaryPanel.Controls.Add(summaryText);
        }

        private string GetPerformanceInsights(FleetAnalyticsSummary summary)
        {
            var insights = new List<string>();

            if (summary.AverageEfficiencyScore >= 80)
                insights.Add("Excellent fleet efficiency!");
            else if (summary.AverageEfficiencyScore >= 70)
                insights.Add("Good efficiency with room for improvement.");
            else
                insights.Add("Consider route optimization initiatives.");

            if (summary.VehicleUtilizationRate < 80)
                insights.Add("Opportunity to improve vehicle utilization.");

            if (summary.AverageMilesPerRider > 4.0)
                insights.Add("High miles per rider - consider route consolidation.");

            return string.Join("\n", insights.Select(i => $"• {i}"));
        }

        private VehicleHealthScore CreateDemoHealthScore(int vehicleId)
        {
            var random = new Random(vehicleId); // Consistent demo data
            return new VehicleHealthScore
            {
                VehicleId = vehicleId,
                VehicleNumber = $"BUS{vehicleId:000}",
                OverallScore = random.Next(60, 95),
                MaintenanceComplianceScore = random.Next(70, 100),
                AgeScore = random.Next(60, 90),
                MileageScore = random.Next(50, 85),
                ReliabilityScore = random.Next(70, 95),
                CostEfficiencyScore = random.Next(65, 90),
                HealthStatus = (VehicleHealthStatus)(vehicleId % 5),
                CalculatedDate = DateTime.Now,
                Recommendations = new List<string> { "Regular maintenance schedule", "Monitor tire wear" }
            };
        }

        private List<MaintenancePrediction> CreateDemoPredictions(int vehicleId)
        {
            return new List<MaintenancePrediction>
            {
                new MaintenancePrediction
                {
                    VehicleId = vehicleId,
                    MaintenanceType = "Oil Change",
                    PredictedDate = DateTime.Now.AddDays(vehicleId * 5),
                    Priority = (MaintenancePriority)(vehicleId % 3),
                    EstimatedCost = 75m,
                    Reason = $"Due based on mileage for BUS{vehicleId:000}",
                    BasedOnMileage = true
                }
            };
        }

        private async void RefreshRouteAnalytics_Click(object sender, EventArgs e)
        {
            await LoadRouteAnalytics();
            MessageBox.Show("Route analytics refreshed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportAnalytics_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Analytics export feature would generate comprehensive reports for administrators.",
                "Export Analytics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ValidateTimeCard_Click(object? sender, EventArgs e)
        {
            // TimeCard functionality is being phased out
            MessageBox.Show("TimeCard validation feature is being transitioned to a separate module.",
                "TimeCard Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
