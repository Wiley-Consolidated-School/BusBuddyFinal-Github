using System;
using System.Collections.Generic;

namespace BusBuddy.Models
{
    /// <summary>
    /// Represents efficiency metrics for a specific route
    /// </summary>
    public class RouteEfficiencyMetrics
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        // AM Period Data
        public double AMTotalMiles { get; set; }
        public int AMRiders { get; set; }
        public int? AMBusId { get; set; }
        public int? AMDriverId { get; set; }

        // PM Period Data
        public double PMTotalMiles { get; set; }
        public int PMRiders { get; set; }
        public int? PMBusId { get; set; }
        public int? PMDriverId { get; set; }

        // Calculated Metrics
        public double TotalMiles { get; set; }
        public int TotalRiders { get; set; }
        public double MilesPerRider { get; set; }
        public double EfficiencyScore { get; set; }
        public decimal EstimatedFuelCost { get; set; }
    }

    /// <summary>
    /// Represents a route optimization suggestion
    /// </summary>
    public class RouteOptimizationSuggestion
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public OptimizationType SuggestionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public Priority Priority { get; set; }
    }

    /// <summary>
    /// Driver performance metrics for analytics
    /// </summary>
    public class DriverPerformanceMetrics
    {
        public int DriverId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalRoutes { get; set; }
        public double TotalMiles { get; set; }
        public int TotalRiders { get; set; }
        public double AverageMilesPerRoute { get; set; }
        public double AverageRidersPerRoute { get; set; }
        public double OverallEfficiencyScore { get; set; }
        public PerformanceRating PerformanceRating { get; set; }
    }

    /// <summary>
    /// Fleet-wide analytics summary
    /// </summary>
    public class FleetAnalyticsSummary
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public int TotalRoutes { get; set; }
        public double TotalMiles { get; set; }
        public int TotalRiders { get; set; }
        public double AverageEfficiencyScore { get; set; }
        public double AverageMilesPerRider { get; set; }
        public double VehicleUtilizationRate { get; set; }
        public decimal EstimatedFuelCosts { get; set; }

        public List<string> TopPerformingRoutes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Types of route optimizations
    /// </summary>
    public enum OptimizationType
    {
        EfficiencyImprovement,
        MileageReduction,
        VehicleUtilization,
        DriverOptimization,
        CostReduction
    }

    /// <summary>
    /// Priority levels for optimization suggestions
    /// </summary>
    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Driver performance rating levels
    /// </summary>
    public enum PerformanceRating
    {
        NeedsImprovement,
        BelowAverage,
        Average,
        Good,
        Excellent
    }

    /// <summary>
    /// Represents a maintenance prediction for a bus
    /// </summary>
    public class MaintenancePrediction
    {
        public int BusId { get; set; }
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime PredictedDate { get; set; }
        public MaintenancePriority Priority { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool BasedOnMileage { get; set; }
        public int PredictedMileage { get; set; }
    }

    /// <summary>
    /// Represents a maintenance recommendation for fleet scheduling
    /// </summary>
    public class MaintenanceRecommendation
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime RecommendedDate { get; set; }
        public MaintenancePriority Priority { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal CurrentMileage { get; set; }
    }

    /// <summary>
    /// Represents a comprehensive bus health score
    /// </summary>
    public class BusHealthScore
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public DateTime CalculatedDate { get; set; }

        // Component scores (0-100)
        public int MaintenanceComplianceScore { get; set; }
        public int AgeScore { get; set; }
        public int MileageScore { get; set; }
        public int ReliabilityScore { get; set; }
        public int CostEfficiencyScore { get; set; }

        // Overall health
        public int OverallScore { get; set; }
        public BusHealthStatus HealthStatus { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents maintenance cost analysis for a bus
    /// </summary>
    public class MaintenanceCostAnalysis
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCostPerService { get; set; }
        public int ServiceCount { get; set; }
        public Dictionary<string, decimal> CostByCategory { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> MonthlyCosts { get; set; } = new Dictionary<string, decimal>();
        public decimal ProjectedAnnualCost { get; set; }
        public string CostTrend { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a maintenance alert
    /// </summary>
    public class MaintenanceAlert
    {
        public int BusId { get; set; }
        public string BusNumber { get; set; } = string.Empty;
        public AlertType AlertType { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    /// <summary>
    /// Maintenance priority levels
    /// </summary>
    public enum MaintenancePriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Bus health status levels
    /// </summary>
    public enum BusHealthStatus
    {
        Critical,
        Poor,
        Fair,
        Good,
        Excellent
    }

    /// <summary>
    /// Alert type levels
    /// </summary>
    public enum AlertType
    {
        Information,
        Warning,
        Urgent,
        Critical
    }

    /// <summary>
    /// Simplified cost metrics focused on cost per student calculations
    /// </summary>
    public class CostPerStudentMetrics
    {
        /// <summary>
        /// Cost per student per day for regular bus routes
        /// </summary>
        public decimal RouteCostPerStudentPerDay { get; set; }

        /// <summary>
        /// Cost per student for sports activities/trips
        /// </summary>
        public decimal SportsCostPerStudent { get; set; }

        /// <summary>
        /// Cost per student for field trip activities
        /// </summary>
        public decimal FieldTripCostPerStudent { get; set; }

        /// <summary>
        /// Date range these metrics cover
        /// </summary>
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total number of regular route student-days
        /// </summary>
        public int TotalRouteStudentDays { get; set; }

        /// <summary>
        /// Total number of sports trip students
        /// </summary>
        public int TotalSportsStudents { get; set; }

        /// <summary>
        /// Total number of field trip students
        /// </summary>
        public int TotalFieldTripStudents { get; set; }

        /// <summary>
        /// Total costs broken down by category
        /// </summary>
        public decimal TotalRouteCosts { get; set; }
        public decimal TotalSportsCosts { get; set; }
        public decimal TotalFieldTripCosts { get; set; }
    }

    /// <summary>
    /// Driver pay report data structure
    /// </summary>
    public class DriverPayReport
    {
        public int DriverId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DriverType { get; set; } = string.Empty; // CDL, Small Bus, SPED
        public string LicenseType { get; set; } = string.Empty; // CDL or Small Bus
        public int TotalTrips { get; set; }
        public int SPEDDays { get; set; }
        public decimal TripRate { get; set; }
        public decimal SPEDDayRate { get; set; }
        public decimal TotalPay { get; set; }
        public decimal PayAmount { get; set; }
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Analytics efficiency metrics
    /// </summary>
    public class EfficiencyMetrics
    {
        public decimal CostPerMile { get; set; }
        public decimal UtilizationRate { get; set; }
        public List<string> Recommendations { get; set; } = new List<string>();
        public Dictionary<string, decimal> KeyMetrics { get; set; } = new Dictionary<string, decimal>();
        public string AIInsights { get; set; } = string.Empty;
    }

    /// <summary>
    /// Route safety analysis for rural district challenges
    /// </summary>
    public class RouteSafetyAnalysis
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public decimal TotalMiles { get; set; }
        public TimeSpan EstimatedDuration { get; set; }

        // Safety metrics
        public int SafetyScore { get; set; }
        public List<string> SafetyConcerns { get; set; } = new List<string>();
        public List<string> WeatherRisks { get; set; } = new List<string>();
        public bool HasBackupBus { get; set; }
        public DateTime LastSafetyReview { get; set; }
    }

    /// <summary>
    /// Driver safety and performance tracking
    /// </summary>
    public class DriverSafetyRecord
    {
        public int DriverId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int YearsExperience { get; set; }
        public DateTime LastTraining { get; set; }
        public DateTime LastPhysical { get; set; }

        // Safety metrics
        public int SafetyScore { get; set; }
        public int IncidentCount { get; set; }
        public List<string> Certifications { get; set; } = new List<string>();
        public bool RequiresRetraining { get; set; }
    }

    /// <summary>
    /// Emergency preparedness assessment model
    /// </summary>
    public class EmergencyReadiness
    {
        public DateTime AssessmentDate { get; set; }
        public int BackupBusCount { get; set; }
        public int QualifiedDriverCount { get; set; }
        public string MaintenanceCapacity { get; set; } = string.Empty;
        public List<string> EmergencySupplies { get; set; } = new List<string>();
        public bool CommunicationSystems { get; set; }
        public int ReadinessScore { get; set; }
        public List<string> ImprovementActions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Safety dashboard overview
    /// </summary>
    public class SafetyDashboard
    {
        public DateTime AnalysisDate { get; set; }
        public int OverallSafetyScore { get; set; }
        public List<string> CriticalAlerts { get; set; } = new List<string>();
        public string FleetStatus { get; set; } = string.Empty;
        public List<string> RouteRisks { get; set; } = new List<string>();
        public List<string> DriverAlerts { get; set; } = new List<string>();
        public string WeatherPreparednes { get; set; } = string.Empty;
        public List<string> MaintenanceUrgency { get; set; } = new List<string>();
    }
}

