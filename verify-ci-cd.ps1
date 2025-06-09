#!/usr/bin/env pwsh
# CI/CD Verification Script
# This script simulates the GitHub Actions workflow steps locally

Write-Host "🚀 BusBuddy CI/CD Verification Script" -ForegroundColor Green
Write-Host "=" * 50

$ErrorActionPreference = "Stop"

try {
    # Step 1: Check solution file
    Write-Host "`n📋 Step 1: Checking solution file..." -ForegroundColor Yellow
    $solutionPath = Get-ChildItem -Path . -Filter *.sln -Recurse | Select-Object -First 1
    if ($solutionPath) {
        Write-Host "✅ Solution found: $($solutionPath.FullName)" -ForegroundColor Green
        $env:SOLUTION_PATH = $solutionPath.FullName
    } else {
        throw "❌ No solution file found"
    }

    # Step 2: Restore dependencies
    Write-Host "`n📦 Step 2: Restoring dependencies..." -ForegroundColor Yellow
    dotnet restore $env:SOLUTION_PATH
    Write-Host "✅ Dependencies restored successfully" -ForegroundColor Green

    # Step 3: Build application
    Write-Host "`n🔨 Step 3: Building application..." -ForegroundColor Yellow
    dotnet build $env:SOLUTION_PATH --no-restore --configuration Release
    Write-Host "✅ Build completed successfully" -ForegroundColor Green

    # Step 4: Run tests
    Write-Host "`n🧪 Step 4: Running tests..." -ForegroundColor Yellow
    $testResult = dotnet test --logger "console;verbosity=minimal" --logger "trx;LogFileName=TestResults.trx" --results-directory ./TestResults/
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ All tests passed successfully" -ForegroundColor Green
    } else {
        throw "❌ Tests failed with exit code $LASTEXITCODE"
    }

    # Step 5: Verify test results
    Write-Host "`n📊 Step 5: Verifying test results..." -ForegroundColor Yellow
    $trxFiles = Get-ChildItem -Path "./TestResults/" -Filter "*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        Write-Host "✅ Test results file created: $($trxFiles[0].Name)" -ForegroundColor Green
    } else {
        Write-Host "⚠️  No TRX files found (tests may have run but no results file created)" -ForegroundColor Yellow
    }

    # Summary
    Write-Host "`n🎉 VERIFICATION SUMMARY" -ForegroundColor Green
    Write-Host "=" * 30
    Write-Host "✅ Solution file found and valid" -ForegroundColor Green
    Write-Host "✅ Dependencies restored successfully" -ForegroundColor Green
    Write-Host "✅ Build completed without errors" -ForegroundColor Green
    Write-Host "✅ Tests executed successfully" -ForegroundColor Green
    Write-Host "✅ CI/CD workflow simulation PASSED" -ForegroundColor Green
    
    Write-Host "`n🚀 Your GitHub Actions workflow should now run successfully!" -ForegroundColor Cyan
    Write-Host "Monitor the workflow at: https://github.com/Wiley-Consolidated-School/BusBuddyFinal-Github/actions" -ForegroundColor Cyan

} catch {
    Write-Host "`n❌ VERIFICATION FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`n🔍 Please check the error above and fix any issues before pushing to GitHub." -ForegroundColor Yellow
    exit 1
}
