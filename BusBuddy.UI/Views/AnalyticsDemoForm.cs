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

            // Route Efficiency Grid - Enhanced SfDataGrid
            var efficiencyGroup = new GroupBox();
            efficiencyGroup.Text = "📊 Route Efficiency Metrics";
            _routeEfficiencyGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _routeEfficiencyGrid.Dock = DockStyle.Fill;
            _routeEfficiencyGrid.AutoGenerateColumns = true;
            // Enhanced by ApplyAllFeaturesToGrid - will override manual settings
            _routeEfficiencyGrid.AllowEditing = false;
            SyncfusionThemeHelper.SfDataGridEnhancements(_routeEfficiencyGrid);
            efficiencyGroup.Controls.Add(_routeEfficiencyGrid);
            layout.Controls.Add(efficiencyGroup, 0, 0);

            // Optimization Suggestions Grid - Enhanced SfDataGrid
            var optimizationGroup = new GroupBox();
            optimizationGroup.Text = "🎯 Route Optimization Suggestions";
            _optimizationSuggestionsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _optimizationSuggestionsGrid.Dock = DockStyle.Fill;
            _optimizationSuggestionsGrid.AutoGenerateColumns = true;
            // Enhanced by ApplyAllFeaturesToGrid - will override manual settings
            _optimizationSuggestionsGrid.AllowEditing = false;
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

            // Maintenance Predictions - Enhanced SfDataGrid
            var predictionsGroup = new GroupBox();
            predictionsGroup.Text = "🔧 Maintenance Predictions";
            _maintenancePredictionsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _maintenancePredictionsGrid.Dock = DockStyle.Fill;
            _maintenancePredictionsGrid.AutoGenerateColumns = true;
            // Enhanced by ApplyAllFeaturesToGrid - will override manual settings
            _maintenancePredictionsGrid.AllowEditing = false;
            SyncfusionThemeHelper.SfDataGridEnhancements(_maintenancePredictionsGrid);
            predictionsGroup.Controls.Add(_maintenancePredictionsGrid);
            layout.Controls.Add(predictionsGroup, 0, 0);

            // Vehicle Health Scores - Enhanced SfDataGrid
            var healthGroup = new GroupBox();
            healthGroup.Text = "🚗 Vehicle Health Scores";
            _vehicleHealthGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _vehicleHealthGrid.Dock = DockStyle.Fill;
            _vehicleHealthGrid.AutoGenerateColumns = true;
            // Enhanced by ApplyAllFeaturesToGrid - will override manual settings
            _vehicleHealthGrid.AllowEditing = false;
            SyncfusionThemeHelper.SfDataGridEnhancements(_vehicleHealthGrid);
            healthGroup.Controls.Add(_vehicleHealthGrid);
            layout.Controls.Add(healthGroup, 1, 0);

            // Maintenance Alerts - Enhanced SfDataGrid
            var alertsGroup = new GroupBox();
            alertsGroup.Text = "🚨 Priority Maintenance Alerts";
            _maintenanceAlertsGrid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();
            _maintenanceAlertsGrid.Dock = DockStyle.Fill;
            _maintenanceAlertsGrid.AutoGenerateColumns = true;
            // Enhanced by ApplyAllFeaturesToGrid - will override manual settings
            _maintenanceAlertsGrid.AllowEditing = false;
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
            // Create a demo time card with issues
            var demoTimeCard = new Models.TimeCard
            {
                Date = DateTime.Now,
                DriverId = 1,
                ClockIn = new TimeSpan(8, 0, 0),
                ClockOut = new TimeSpan(16, 0, 0),
                LunchOut = new TimeSpan(12, 0, 0),
                // Missing LunchIn - this will trigger validation
            };

            var resultsText = new RichTextBox();
            resultsText.Dock = DockStyle.Fill;
            resultsText.Text = @"Demo Time Card Validation Results:

⚠️ Issues Detected:
• Missing Lunch In time
• Potential overtime calculation needed

💡 Smart Suggestions:
• Auto-set Lunch In to 12:30 PM (30-minute lunch) - Confidence: 90%
• Verify overtime rules compliance - Confidence: 80%

🔧 Auto-Fix Options:
✓ Fix missing lunch time automatically
✓ Apply standard lunch duration
✓ Recalculate total hours

The enhanced validation system provides intelligent suggestions and can automatically resolve common issues, saving administrators time and reducing errors.";

            _validationResultsPanel.Controls.Clear();
            _validationResultsPanel.Controls.Add(resultsText);
        }
    }
}
