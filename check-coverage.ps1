#!/usr/bin/env pwsh
# Check-coverage.ps1 - Verifies test coverage meets threshold

Write-Host "🔍 Checking test coverage..."

# Run tests with coverage
dotnet test BusBuddy.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults

# Check if coverage file exists
if (-not (Test-Path "TestResults/*/coverage.cobertura.xml")) {
    Write-Host "❌ Coverage report not generated" -ForegroundColor Red
    exit 1
}

# Install reportgenerator if not already installed
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Write-Host "📦 Installing reportgenerator tool..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Generate summary report
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:TextSummary

# Get coverage from summary report
$summaryContent = Get-Content "TestResults/CoverageReport/Summary.txt"
$coverageLine = $summaryContent | Where-Object { $_ -match "Line coverage:" }
if ($coverageLine -match "([0-9\.]+)%") {
    $coverage = [double]$matches[1]

    # Check if coverage meets threshold
    if ($coverage -lt 70) {
        Write-Host "❌ Coverage $coverage% is below 70%" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Coverage $coverage% meets threshold" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ Could not parse coverage information" -ForegroundColor Red
    exit 1
}
