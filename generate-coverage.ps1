#!/usr/bin/env pwsh
# Generate Code Coverage Report
# This script runs tests with coverage and generates a local HTML report

Write-Host "🧪 Running tests with code coverage..." -ForegroundColor Green

# Clean previous results
if (Test-Path "TestResults") {
    Remove-Item -Recurse -Force "TestResults"
    Write-Host "Cleaned previous test results" -ForegroundColor Yellow
}

# Run tests with coverage
Write-Host "Running tests..." -ForegroundColor Blue
dotnet test BusBuddy.sln --configuration Release `
    --collect:"XPlat Code Coverage" `
    --results-directory TestResults `
    --logger "trx;LogFileName=coverage-report.trx" `
    --verbosity normal

# Check if coverage files were generated
$coverageFiles = Get-ChildItem -Recurse TestResults -Filter "coverage.cobertura.xml"
if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Coverage files generated:" -ForegroundColor Green
$coverageFiles | ForEach-Object { 
    Write-Host "  📄 $($_.FullName)" -ForegroundColor Cyan
}

# Install reportgenerator tool if not present
Write-Host "📊 Checking for ReportGenerator tool..." -ForegroundColor Blue
$toolList = dotnet tool list --global
if ($toolList -notlike "*dotnet-reportgenerator-globaltool*") {
    Write-Host "Installing ReportGenerator tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-reportgenerator-globaltool
}

# Generate HTML report
Write-Host "📈 Generating HTML coverage report..." -ForegroundColor Blue
$coveragePath = $coverageFiles[0].FullName
reportgenerator -reports:"$coveragePath" -targetdir:"TestResults\CoverageReport" -reporttypes:Html

# Display results
if (Test-Path "TestResults\CoverageReport\index.html") {
    Write-Host "✅ Coverage report generated successfully!" -ForegroundColor Green
    Write-Host "📖 Open this file to view the report:" -ForegroundColor Cyan
    Write-Host "   $(Resolve-Path 'TestResults\CoverageReport\index.html')" -ForegroundColor White
    
    # Parse coverage percentage from Cobertura XML
    [xml]$coverage = Get-Content $coveragePath
    $lineRate = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 2)
    $branchRate = [math]::Round([double]$coverage.coverage.'branch-rate' * 100, 2)
    
    Write-Host "`n📊 Coverage Summary:" -ForegroundColor Green
    Write-Host "   Line Coverage:   $lineRate%" -ForegroundColor White
    Write-Host "   Branch Coverage: $branchRate%" -ForegroundColor White
    Write-Host "   Total Lines:     $($coverage.coverage.'lines-valid')" -ForegroundColor Gray
    Write-Host "   Covered Lines:   $($coverage.coverage.'lines-covered')" -ForegroundColor Gray
    
    # Ask if user wants to open the report
    $response = Read-Host "`nWould you like to open the coverage report in your browser? (y/N)"
    if ($response -eq 'y' -or $response -eq 'Y') {
        Start-Process "TestResults\CoverageReport\index.html"
    }
} else {
    Write-Host "❌ Failed to generate coverage report" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Code coverage analysis complete!" -ForegroundColor Green
