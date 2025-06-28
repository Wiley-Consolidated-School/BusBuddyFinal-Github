using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
    /// <summary>
    /// Comprehensive reporting models for rural school district management
    /// Supports compliance, safety tracking, and operational reporting
    /// </summary>

    #region Monthly Reporting Models

    public class MonthlyReport
    {
        public DateTime ReportMonth { get; set; }
        public DateTime GeneratedDate { get; set; }
        public SafetyDashboard SafetyDashboard { get; set; } = new SafetyDashboard();
        public FleetSummary FleetSummary { get; set; } = new FleetSummary();
        public MaintenanceOverview MaintenanceOverview { get; set; } = new MaintenanceOverview();
        public RoutePerformance RoutePerformance { get; set; } = new RoutePerformance();
        public DriverSummary DriverSummary { get; set; } = new DriverSummary();
        public CostAnalysis CostAnalysis { get; set; } = new CostAnalysis();
        public ComplianceStatus ComplianceStatus { get; set; } = new ComplianceStatus();
        public List<string> AIRecommendations { get; set; } = new List<string>();
    }

    public class FleetSummary
    {
        public int TotalBuses { get; set; }
        public int OperationalBuses { get; set; }
        public int MaintenanceBuses { get; set; }
        public decimal AverageAge { get; set; }
        public decimal TotalMileage { get; set; }
        public decimal UtilizationRate { get; set; }
    }

    public class MaintenanceOverview
    {
        public int TotalMaintenanceEvents { get; set; }
        public int PreventiveMaintenance { get; set; }
        public int EmergencyRepairs { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AverageDowntime { get; set; }
        public List<MaintenanceAlert> MaintenanceAlerts { get; set; } = new List<MaintenanceAlert>();
    }

    public class RoutePerformance
    {
        public int TotalRoutes { get; set; }
        public decimal OnTimePerformance { get; set; }
        public decimal FuelEfficiency { get; set; }
        public int SafetyIncidents { get; set; }
        public int StudentCapacity { get; set; }
        public string RouteOptimization { get; set; } = string.Empty;
    }

    public class DriverSummary
    {
        public int TotalDrivers { get; set; }
        public int CertifiedDrivers { get; set; }
        public int DriversNeedingTraining { get; set; }
        public decimal AverageExperience { get; set; }
        public int SafetyIncidents { get; set; }
        public List<string> TrainingNeeds { get; set; } = new List<string>();
    }

    public class CostAnalysis
    {
        public decimal TotalOperatingCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal FuelCost { get; set; }
        public decimal DriverCost { get; set; }
        public decimal CostPerMile { get; set; }
        public decimal CostPerStudent { get; set; }
        public string BudgetVariance { get; set; } = string.Empty;
    }

    public class ComplianceStatus
    {
        public int BusesInspected { get; set; }
        public int BusesOverdue { get; set; }
        public int DriversCompliant { get; set; }
        public int DriversNeedingUpdate { get; set; }
        public decimal ComplianceScore { get; set; }
        public List<string> RequiredActions { get; set; } = new List<string>();
    }

    #endregion

    #region Compliance Reporting Models

    public class ComplianceReport
    {
        public DateTime ReportDate { get; set; }
        public string SchoolYear { get; set; } = string.Empty;
        public BusComplianceStatus BusInspections { get; set; } = new BusComplianceStatus();
        public DriverComplianceStatus DriverCertifications { get; set; } = new DriverComplianceStatus();
        public RouteComplianceStatus RouteSafety { get; set; } = new RouteComplianceStatus();
        public MaintenanceComplianceStatus MaintenanceCompliance { get; set; } = new MaintenanceComplianceStatus();
        public int OverallComplianceScore { get; set; }
        public List<string> RequiredActions { get; set; } = new List<string>();
    }

    public class BusComplianceStatus
    {
        public int TotalBuses { get; set; }
        public int PassedInspections { get; set; }
        public int FailedInspections { get; set; }
        public int OverdueInspections { get; set; }
        public List<string> IssuesFound { get; set; } = new List<string>();
    }

    public class DriverComplianceStatus
    {
        public int TotalDrivers { get; set; }
        public int CurrentCertifications { get; set; }
        public int ExpiredCertifications { get; set; }
        public int PendingTraining { get; set; }
        public List<string> TrainingRequirements { get; set; } = new List<string>();
    }

    public class RouteComplianceStatus
    {
        public int TotalRoutes { get; set; }
        public int SafetyApprovedRoutes { get; set; }
        public int RoutesNeedingReview { get; set; }
        public List<string> SafetyConcerns { get; set; } = new List<string>();
    }

    public class MaintenanceComplianceStatus
    {
        public int ScheduledMaintenance { get; set; }
        public int CompletedMaintenance { get; set; }
        public int OverdueMaintenance { get; set; }
        public decimal CompliancePercentage { get; set; }
    }

    #endregion

    #region Annual Reporting Models

    public class AnnualReport
    {
        public int Year { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string ExecutiveSummary { get; set; } = string.Empty;
        public AnnualSafetyMetrics SafetyMetrics { get; set; } = new AnnualSafetyMetrics();
        public AnnualFinancialSummary FinancialSummary { get; set; } = new AnnualFinancialSummary();
        public FleetLifecycleAnalysis FleetAnalysis { get; set; } = new FleetLifecycleAnalysis();
        public OperationalEfficiencyMetrics OperationalEfficiency { get; set; } = new OperationalEfficiencyMetrics();
        public List<string> ChallengesAndSolutions { get; set; } = new List<string>();
        public List<string> StrategicRecommendations { get; set; } = new List<string>();
    }

    public class AnnualSafetyMetrics
    {
        public int TotalSafetyIncidents { get; set; }
        public int PreventedIncidents { get; set; }
        public decimal SafetyImprovement { get; set; }
        public int TrainingHours { get; set; }
        public List<string> SafetyAchievements { get; set; } = new List<string>();
    }

    public class AnnualFinancialSummary
    {
        public decimal TotalOperatingCost { get; set; }
        public decimal BudgetVariance { get; set; }
        public decimal CostSavings { get; set; }
        public List<string> CostDrivers { get; set; } = new List<string>();
        public List<string> SavingOpportunities { get; set; } = new List<string>();
    }

    public class FleetLifecycleAnalysis
    {
        public decimal AverageFleetAge { get; set; }
        public int BusesRecommendedForReplacement { get; set; }
        public decimal ReplacementCost { get; set; }
        public List<string> LifecycleRecommendations { get; set; } = new List<string>();
    }

    public class OperationalEfficiencyMetrics
    {
        public decimal RouteEfficiency { get; set; }
        public decimal FuelEfficiency { get; set; }
        public decimal MaintenanceEfficiency { get; set; }
        public List<string> ImprovementOpportunities { get; set; } = new List<string>();
    }

    #endregion

    #region Executive Dashboard Models

    public class ExecutiveDashboard
    {
        public DateTime ReportDate { get; set; }
        public SafetyKPIs SafetyKPIs { get; set; } = new SafetyKPIs();
        public FinancialKPIs FinancialKPIs { get; set; } = new FinancialKPIs();
        public OperationalKPIs OperationalKPIs { get; set; } = new OperationalKPIs();
        public List<string> ExecutiveAlerts { get; set; } = new List<string>();
        public List<string> StrategicInsights { get; set; } = new List<string>();
        public List<string> BudgetImpacts { get; set; } = new List<string>();
        public RiskAssessment RiskAssessment { get; set; } = new RiskAssessment();
    }

    public class SafetyKPIs
    {
        public int DaysSinceLastIncident { get; set; }
        public decimal SafetyScore { get; set; }
        public int CriticalAlerts { get; set; }
        public decimal DriverComplianceRate { get; set; }
        public decimal BusInspectionRate { get; set; }
    }

    public class FinancialKPIs
    {
        public decimal MonthlyOperatingCost { get; set; }
        public decimal BudgetVariance { get; set; }
        public decimal CostPerMile { get; set; }
        public decimal MaintenanceCostTrend { get; set; }
        public decimal FuelCostTrend { get; set; }
    }

    public class OperationalKPIs
    {
        public decimal FleetUtilization { get; set; }
        public decimal OnTimePerformance { get; set; }
        public int ActiveRoutes { get; set; }
        public decimal AverageRouteTime { get; set; }
        public int StudentsTransported { get; set; }
    }

    public class RiskAssessment
    {
        public List<string> HighRiskFactors { get; set; } = new List<string>();
        public List<string> MitigationStrategies { get; set; } = new List<string>();
        public int OverallRiskScore { get; set; }
        public List<string> ImmediateActions { get; set; } = new List<string>();
    }

    #endregion
}

