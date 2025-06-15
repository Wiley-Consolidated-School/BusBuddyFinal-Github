@echo off
echo 🧪 Running tests with code coverage...

REM Clean previous results
if exist "TestResults" rmdir /s /q "TestResults"

REM Run tests with coverage
echo Running tests...
dotnet test BusBuddy.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults --logger "trx;LogFileName=coverage-report.trx"

REM Check if coverage files exist
dir /s /b TestResults\*.xml | findstr coverage.cobertura.xml >nul
if errorlevel 1 (
    echo ❌ No coverage files found!
    pause
    exit /b 1
)

echo ✅ Coverage files generated successfully!
echo 📖 Check TestResults folder for coverage data
echo 💡 Run generate-coverage.ps1 for detailed HTML reports

pause
