# BusBuddy - School Bus Management System v1.0.0

## 📄 License and Intellectual Property

**Copyright © 2025 Steve McKitrick. All rights reserved.**

This software is the intellectual property of Steve McKitrick, developed independently for personal professional use. See [LICENSE.md](LICENSE.md) for complete licensing terms.

**Important**: This project is NOT the property of Wiley School District and was developed using only personal funds and open source technologies.

[![.NET Build and Test](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github/branch/master/graph/badge.svg)](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)

A comprehensive school bus management system built with .NET 8 and Windows Forms, designed to help school districts manage their transportation operations efficiently.

**🎉 Version 1.0.0 Release - Production Ready**
- ✅ Complete Syncfusion UI migration (18/18 forms)
- ✅ Advanced analytics and reporting
- ✅ SQL Server Express integration
- ✅ Professional dashboard with docking manager
- ✅ Comprehensive cost analytics system

## 🚌 Features

### Core Functionality
- **Vehicle Management**: Track buses, maintenance records, and fuel consumption
- **Driver Management**: Manage driver information, licenses, and training records
- **Route Planning**: Organize and schedule bus routes for optimal efficiency
- **Activity Tracking**: Monitor special trips and extracurricular activities
- **Time Card Management**: Track driver hours and overtime calculations
- **School Calendar Integration**: Sync with academic calendar and events
- **Maintenance Scheduling**: Proactive vehicle maintenance tracking
- **Fuel Management**: Monitor fuel usage and costs across the fleet

### 🆕 Enhanced Analytics & Optimization (NEW)
- **Route Efficiency Analytics**:
  - Real-time route performance metrics and efficiency scoring
  - Miles per rider calculations and cost analysis
  - Fleet-wide utilization tracking and optimization suggestions
- **Predictive Maintenance**:
  - AI-powered maintenance scheduling based on usage patterns
  - Vehicle health scoring with comprehensive analytics
  - Cost trend analysis and budget forecasting
- **Smart Time Card Validation**:
  - Intelligent conflict detection and resolution
  - Auto-fix capabilities for common time entry issues
  - Enhanced validation with confidence-based suggestions

### 📊 Advanced Reporting
- **Performance Dashboards**: Visual analytics for administrators
- **Cost Optimization Reports**: Fuel efficiency and maintenance cost analysis
- **Driver Performance Metrics**: Route efficiency and safety tracking
- **Maintenance Alerts**: Priority-based maintenance scheduling

## 🔧 New Analytics & Optimization Features

BusBuddy now includes advanced analytics and optimization capabilities designed to help administrators make data-driven decisions:

### 📈 Route Analytics Service
```csharp
// Calculate route efficiency metrics
var analytics = new RouteAnalyticsService();
var metrics = await analytics.GetRouteEfficiencyMetricsAsync(startDate, endDate);

// Get optimization suggestions
var suggestions = await analytics.AnalyzeRouteOptimizationsAsync(DateTime.Now);

// Fleet-wide performance summary
var summary = await analytics.GetFleetAnalyticsSummaryAsync(startDate, endDate);
```

**Key Features:**
- Route efficiency scoring (0-100%) based on riders per mile and cost factors
- Optimization suggestions with potential savings calculations
- Fleet utilization analysis and vehicle assignment recommendations
- Driver performance metrics and comparative analysis

### 🔮 Predictive Maintenance Service
```csharp
// Get predictive maintenance schedule
var maintenance = new PredictiveMaintenanceService();
var predictions = await maintenance.GetMaintenancePredictionsAsync(vehicleId);

// Calculate vehicle health score
var healthScore = await maintenance.CalculateVehicleHealthScoreAsync(vehicleId);

// Get priority alerts
var alerts = await maintenance.GetMaintenanceAlertsAsync();
```

**Key Features:**
- Predictive maintenance scheduling based on mileage, time, and usage patterns
- Vehicle health scoring with component-level analysis
- Cost trend analysis and budget forecasting
- Priority-based maintenance alerts and recommendations

### ⚡ Enhanced Time Card Validation
```csharp
// Smart validation with auto-fix suggestions
var validator = new TimeEntryValidationService();
var result = await validator.ValidateTimeCardWithResolutionAsync(timeCard, isNew);

// Check for driver schedule conflicts
var conflicts = await validator.CheckDriverScheduleConflictsAsync(driverId, date);
```

**Key Features:**
- Intelligent conflict detection and resolution suggestions
- Auto-fix capabilities with confidence scoring
- Schedule conflict analysis across routes and activities
- Enhanced user experience with actionable validation messages

### 🎯 Demo & Testing
Try out the new features using the analytics demo form:
```csharp
var demoForm = new AnalyticsDemoForm();
demoForm.ShowDialog();
```

## 🏗️ Architecture

The application follows a layered architecture pattern:

```
BusBuddy/
├── BusBuddy.UI/          # Windows Forms user interface
├── BusBuddy.Business/    # Business logic and services
├── BusBuddy.Data/        # Data access layer and repositories
├── BusBuddy.Models/      # Domain models and entities
├── BusBuddy.Tests/       # Unit and integration tests
└── Db/                   # Database initialization and scripts
```

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- SQLite (embedded) or SQL Server (optional)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github.git
   cd BusBuddyFinal-Github
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore BusBuddy.sln
   ```

3. **Build the solution**
   ```bash
   dotnet build BusBuddy.sln --configuration Release
   ```

4. **Run the application**
   ```bash
   dotnet run --project . --configuration Release
   ```

## 🛠️ Database Setup

### SQL Server Express Setup
BusBuddy requires SQL Server Express for data storage. Follow these steps:

1. **Install SQL Server Express**:
   - Download from [Microsoft SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
   - Install with default settings
   - Note your server instance name (e.g., `ST-LPTP9-23\SQLEXPRESS01`)

2. **Configure Connection**:
   ```xml
   <!-- App.config -->
   <connectionStrings>
     <add name="DefaultConnection"
          connectionString="Server=YOUR_SERVER\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;"
          providerName="Microsoft.Data.SqlClient" />
   </connectionStrings>
   ```

3. **Create Database**:
   ```sql
   -- Connect to SQL Server Express
   sqlcmd -S YOUR_SERVER\SQLEXPRESS01 -Q "CREATE DATABASE BusBuddy;"

   -- Run database scripts
   sqlcmd -S YOUR_SERVER\SQLEXPRESS01 -d BusBuddy -i "BusBuddy.Data\DatabaseScript.SqlServer.sql"
   sqlcmd -S YOUR_SERVER\SQLEXPRESS01 -d BusBuddy -i "BusBuddy.Data\TestSeedData.sql"
   ```

### 💾 Database Backup and Restore

**Backup Database**:
```sql
BACKUP DATABASE BusBuddy TO DISK = 'C:\Backups\BusBuddy.bak'
WITH FORMAT, INIT, NAME = 'BusBuddy-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10;
```

**Restore Database**:
```sql
RESTORE DATABASE BusBuddy FROM DISK = 'C:\Backups\BusBuddy.bak'
WITH REPLACE, STATS = 10;
```

**Automated Backup Script**:
```powershell
# backup-database.ps1
$backupPath = "C:\Backups\BusBuddy_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
sqlcmd -S YOUR_SERVER\SQLEXPRESS01 -Q "BACKUP DATABASE BusBuddy TO DISK = '$backupPath'"
```

## 📊 Cost Analytics Usage

The cost analytics system provides real-time insights into transportation costs:

**Route Cost Analysis** (Expected values for June 2025):
- Regular Routes: ~$2.70 per student per day
- Sports/Activities: ~$5.67 per student per day
- Special Events: Varies based on distance and group size

**Usage Examples**:
```csharp
// Get monthly cost breakdown
var analytics = new RouteAnalyticsService();
var costs = await analytics.CalculateCostPerStudentMetricsAsync(
    DateTime.Now.AddDays(-30),
    DateTime.Now
);

Console.WriteLine($"Route Cost/Student/Day: ${costs.RouteCostPerStudentPerDay:F2}");
Console.WriteLine($"Sports Cost/Student/Day: ${costs.SportsCostPerStudentPerDay:F2}");
```

## 🎨 UI Features

- **Syncfusion Enhanced Interface**: Professional Windows Forms controls with Material Design theming
- **High-DPI Support**: Automatic scaling for modern displays
- **Responsive Layouts**: Docking manager with professional panel organization
- **Diagnostic Logging**: Real-time debugging and performance monitoring
- **Fallback Strategies**: Graceful degradation for compatibility
````
