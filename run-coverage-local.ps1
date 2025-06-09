#!/usr/bin/env pwsh
# Local Code Coverage Script for BusBuddy
# Run this to get coverage reports on your local machine

Write-Host "🔍 Running BusBuddy Code Coverage Analysis..." -ForegroundColor Cyan

# Clean previous results
if (Test-Path "TestResults") {
    Remove-Item "TestResults" -Recurse -Force
    Write-Host "✅ Cleaned previous test results" -ForegroundColor Green
}

# Run tests with coverage
Write-Host "🧪 Running tests with coverage collection..." -ForegroundColor Yellow
dotnet test BusBuddy.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults --logger "trx;LogFileName=coverage-test.trx"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Tests completed successfully!" -ForegroundColor Green
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
    
    if ($coverageFiles.Count -gt 0) {
        Write-Host "📊 Coverage files generated:" -ForegroundColor Cyan
        foreach ($file in $coverageFiles) {
            Write-Host "  - $($file.FullName)" -ForegroundColor White
        }
        
        # Try to install and use reportgenerator for HTML reports
        Write-Host "📈 Generating HTML coverage report..." -ForegroundColor Yellow
        
        try {
            # Install ReportGenerator if not already installed
            dotnet tool install -g dotnet-reportgenerator-globaltool 2>$null
            
            # Generate HTML report
            $latestCoverage = $coverageFiles[0].FullName
            reportgenerator -reports:"$latestCoverage" -targetdir:"TestResults\CoverageReport" -reporttypes:Html
            
            if (Test-Path "TestResults\CoverageReport\index.html") {
                Write-Host "✅ HTML coverage report generated!" -ForegroundColor Green
                Write-Host "📂 Open: TestResults\CoverageReport\index.html" -ForegroundColor Cyan
                
                # Ask if user wants to open the report
                $openReport = Read-Host "Open coverage report in browser? (y/n)"
                if ($openReport -eq "y" -or $openReport -eq "Y") {
                    Start-Process "TestResults\CoverageReport\index.html"
                }
            }
        }
        catch {
            Write-Host "⚠️  Could not generate HTML report, but XML coverage data is available" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "⚠️  No coverage files found" -ForegroundColor Yellow
    }
}
else {
    Write-Host "❌ Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "🎉 Coverage analysis complete!" -ForegroundColor Green
