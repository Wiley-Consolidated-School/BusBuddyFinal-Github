# BusBuddy - School Bus Management System

[![.NET Build and Test](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml)
[![codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github/branch/master/graph/badge.svg)](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)

A comprehensive school bus management system built with .NET 8 and Windows Forms, designed to help school districts manage their transportation operations efficiently.

## 🚌 Features

- **Vehicle Management**: Track buses, maintenance records, and fuel consumption
- **Driver Management**: Manage driver information, licenses, and training records  
- **Route Planning**: Organize and schedule bus routes for optimal efficiency
- **Activity Tracking**: Monitor special trips and extracurricular activities
- **Time Card Management**: Track driver hours and overtime calculations
- **School Calendar Integration**: Sync with academic calendar and events
- **Maintenance Scheduling**: Proactive vehicle maintenance tracking
- **Fuel Management**: Monitor fuel usage and costs across the fleet

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

## 📈 Monitoring & Reporting

- **Build Status**: ![Build Status](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions/workflows/dotnet.yml/badge.svg)
- **Code Coverage**: [![codecov](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github/branch/master/graph/badge.svg)](https://codecov.io/gh/Wiley-Consolidated-School/BusBuddyFinal-Github)
- **Test Results**: Available in GitHub Actions runs

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Support

For support and questions:
- Create an [Issue](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/issues)
- Check the [Wiki](https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/wiki) for documentation
- Contact the development team

## 🎯 Roadmap

- [ ] Web-based dashboard interface
- [ ] Mobile app for drivers
- [ ] Advanced analytics and reporting
- [ ] GPS integration for real-time tracking
- [ ] Parent notification system
- [ ] API for third-party integrations

---

**Built with ❤️ by Wiley Consolidated School District**
