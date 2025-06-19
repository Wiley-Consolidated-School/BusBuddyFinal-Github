# BusBuddy - School Bus Management System

## 📄 License and Intellectual Property

**Copyright © 2025 Steve McKitrick. All rights reserved.**

This software is the intellectual property of Steve McKitrick, developed independently for personal professional use. See [LICENSE.md](LICENSE.md) for complete licensing terms.

**Important**: This project is NOT the property of Wiley School District and was developed using only personal funds and open source technologies.

[![.NET Build and Test](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github/branch/master/graph/badge.svg)](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)

A comprehensive school bus management system built with .NET 8 and Windows Forms, designed to help school districts manage their transportation operations efficiently.

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

## 🗄️ Database Setup

### SQLite (Default)
- The app uses SQLite by default. No setup required; the database file will be created automatically.

### SQL Server (Optional)
1. Start a SQL Server instance (local or Docker):
   ```pwsh
   docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2019-latest
   ```
2. Run the setup script to initialize the database:
   ```pwsh
   cd Db
   ./setup.ps1 -SqlServer localhost -User sa -Password YourStrong@Passw0rd -Script setup.sql
   ```
3. In `BusBuddy.Tests/App.config`, comment out the SQLite connection string and uncomment the SQL Server one.

### Schema & ERD
- See [`Db/SCHEMA.md`](Db/SCHEMA.md) for a full schema reference.
- (ER diagram: `Db/ERD.png` to be added)

### Troubleshooting
- Ensure SQL Server is running and accessible.
- For Docker, use `localhost,1433` as the server.
- If you see connection errors, check firewall and authentication settings.

## 🧪 Testing

### Run All Tests
```bash
dotnet test BusBuddy.sln --configuration Release
```

### Run Tests with Local Coverage Analysis
```powershell
# Easy one-click coverage (recommended for single developer)
.\run-coverage-local.ps1
```

This will:
- Run all tests with coverage collection
- Generate HTML coverage report
- Automatically open the report in your browser
- Show detailed line-by-line coverage analysis

### Manual Coverage Commands
```bash
# Run tests with coverage
dotnet test BusBuddy.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults

# Install ReportGenerator (one time)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator -reports:"TestResults\**\coverage.cobertura.xml" -targetdir:"TestResults\CoverageReport" -reporttypes:Html
```

### Current Test Status
- **19 tests passing**, 1 skipped (SQL Server test)
- **Coverage focus**: Core business logic and data access layers
- **Test categories**: Unit tests, integration tests, database tests

### Current Test Status
- ✅ **19 tests passing**
- ⏭️ **1 test skipped** (SQL Server integration - requires database instance)
- 🎯 **Code Coverage**: Tracked via [Codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)

## 📊 Code Quality & CI/CD

### GitHub Actions Workflow
- ✅ **Automated builds** on every push/PR
- ✅ **Test execution** with results reporting
- ✅ **Code coverage** analysis via Codecov
- ✅ **Deployment artifacts** for releases

### Code Coverage
We use Codecov to track test coverage and maintain code quality:

- **Coverage reports** generated on every CI run
- **PR comments** with coverage changes
- **Coverage trends** tracked over time
- **Quality gates** ensure coverage standards

## 🛠️ Development

### Project Structure

- **Models**: Data entities representing vehicles, drivers, routes, etc.
- **Repositories**: Data access layer with interface-based design
- **Services**: Business logic and domain operations
- **Forms**: Windows Forms UI for user interaction
- **Tests**: Comprehensive test suite with xUnit

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Standards

- Follow C# coding conventions
- Maintain test coverage above 70%
- All tests must pass before merging
- Use meaningful commit messages

### Code Formatting

- Run `dotnet format BusBuddy.sln` to format code according to project standards
- Verify formatting with `dotnet format BusBuddy.sln --verify-no-changes`
- CI pipeline will enforce code formatting standards

### Pre-Commit Screening

The project uses pre-commit hooks to ensure code quality:

1. **Install pre-commit**:
   ```
   pip install pre-commit
   ```

2. **Install the hook**:
   ```
   pre-commit install
   ```

3. **Update pre-commit config**:
   ```
   pre-commit migrate-config
   ```

3. **Pre-commit checks include**:
   - Code formatting verification
   - Unit tests execution
   - Test coverage (70%+ threshold)
   - Code secrets detection

4. **Fix common issues**:
   - Formatting: `dotnet format BusBuddy.sln`
   - Tests: Fix failing tests in BusBuddy.Tests
   - Coverage: Add more unit tests to increase coverage
   - Secrets: Move sensitive data to secure storage

## 📈 Monitoring & Reporting

- **Build Status**: ![Build Status](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml/badge.svg)
- **Code Coverage**: [![codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github/branch/master/graph/badge.svg)](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)
- **Test Results**: Available in GitHub Actions runs

## 🧹 Repository Maintenance

### Cleanup and Housekeeping
Run the cleanup script regularly to maintain repository health:

```powershell
# General cleanup
.\cleanup-repository.ps1

# Fix trailing whitespace (preview changes)
.\fix-whitespace.ps1 -WhatIf

# Fix trailing whitespace (apply changes)
.\fix-whitespace.ps1
```

This script removes:
- Build artifacts (`bin/`, `obj/`, `TestResults/`)
- Temporary files (`*.tmp`, `*.bak`, `*_new.*`, etc.)
- Backup directories (`Migration_Backups/`, etc.)
- Empty directories
- Untracked git files
- Reports trailing whitespace issues

### File Management Best Practices
- Remove temporary files (`.new`, `.bak`, `.backup`) after use
- Clean up migration backups after successful migrations
- Use `.gitignore` to prevent tracking build artifacts
- Monitor repository size with `git count-objects -vH`
- **Remove trailing whitespace** from all source files
- Ensure files end with exactly one newline character

## � Intellectual Property and Ownership

**IMPORTANT LEGAL NOTICE**

This software is the **exclusive intellectual property of Steve McKitrick** and was developed independently using only personal resources:

### 🏠 Personal Development
- ✅ **Personal funds only** - No tax money used
- ✅ **Open source technologies** - Properly licensed components
- ✅ **Personal time and equipment** - Developed outside work hours
- ✅ **Personally purchased tools** - All software licenses paid personally

### 🚫 What This Is NOT
- ❌ **NOT property of Wiley School District**
- ❌ **NOT funded by tax dollars**
- ❌ **NOT a work-for-hire project**
- ❌ **NOT using school district resources**

### 📋 Legal Documentation
- See [LICENSE.md](LICENSE.md) for complete licensing terms
- See [NOTICE.txt](NOTICE.txt) for detailed ownership statement
- Copyright notices embedded in source code and assembly metadata

## 📝 License

This project is licensed under a Personal Use License - see the [LICENSE.md](LICENSE.md) file for details.

**Copyright © 2025 Steve McKitrick. All rights reserved.**

## 🤝 Support

For support and questions:
- Contact: Steve McKitrick (project owner)
- Check project documentation
- Review source code comments

## 🎯 Roadmap

- [ ] Web-based dashboard interface
- [ ] Mobile app for drivers
- [ ] Advanced analytics and reporting
- [ ] GPS integration for real-time tracking
- [ ] Parent notification system
- [ ] API for third-party integrations

---

**Built with ❤️ by Wiley Consolidated School District**
