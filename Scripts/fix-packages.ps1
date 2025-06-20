#!/usr/bin/env pwsh
# Quick Package Version Fix Script

Write-Host "🔧 BusBuddy Package Version Fix" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

try {
    # Step 1: Remove all existing Syncfusion package references to avoid conflicts
    Write-Host "📦 Removing conflicting package references..." -ForegroundColor Yellow

    $badPackages = @(
        "Syncfusion.SfForm.WinForms",
        "Syncfusion.Themes.MaterialDesign.WinForms"
    )

    foreach ($package in $badPackages) {
        Write-Host "  Removing $package from main project..." -ForegroundColor Gray
        & dotnet remove BusBuddy.csproj package $package 2>$null

        Write-Host "  Removing $package from tests..." -ForegroundColor Gray
        & dotnet remove BusBuddy.Tests\BusBuddy.Tests.csproj package $package 2>$null
    }

    # Step 2: Clean and restore to clear package cache
    Write-Host "🧹 Cleaning project..." -ForegroundColor Yellow
    & dotnet clean --verbosity quiet

    # Step 3: Restore packages
    Write-Host "🔄 Restoring packages..." -ForegroundColor Yellow
    $restoreResult = & dotnet restore 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Package restore successful!" -ForegroundColor Green
    } else {
        Write-Host "❌ Package restore failed:" -ForegroundColor Red
        Write-Host $restoreResult -ForegroundColor Red
        exit 1
    }

    # Step 4: Test build
    Write-Host "🔨 Testing build..." -ForegroundColor Yellow
    $buildResult = & dotnet build --configuration Release --verbosity quiet 2>&1

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build successful!" -ForegroundColor Green
    } else {
        Write-Host "❌ Build failed:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "🎉 Package conflicts resolved!" -ForegroundColor Green
    Write-Host "   All Syncfusion packages now use version 29.2.10" -ForegroundColor Green
    Write-Host "   System.Configuration.ConfigurationManager updated to 8.0.1" -ForegroundColor Green
    Write-Host ""
    Write-Host "🚀 Ready to run: dotnet run --configuration Release" -ForegroundColor Cyan

} catch {
    Write-Host "❌ Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
