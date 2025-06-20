# Quick Fix Script for BusBuddy Diagnostic Issues
# Addresses the critical issues found by the diagnostic script

param(
    [switch]$SkipPackages = $false,
    [switch]$SkipLicense = $false
)

Write-Host "🔧 BusBuddy Quick Fix Script" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# 1. Install missing Syncfusion packages
if (!$SkipPackages) {
    Write-Host "📦 Installing Syncfusion packages..." -ForegroundColor Yellow

    $packages = @(
        "Syncfusion.SfForm.WinForms",
        "Syncfusion.Themes.MaterialDesign.WinForms",
        "Syncfusion.Tools.Windows",
        "Syncfusion.Grid.Windows",
        "Syncfusion.SfDataGrid.WinForms",
        "Syncfusion.Chart.Windows",
        "Syncfusion.Gauge.Windows",
        "Syncfusion.Shared.Base",
        "System.Configuration.ConfigurationManager"
    )

    foreach ($package in $packages) {
        Write-Host "  Installing $package..." -ForegroundColor Gray
        $result = dotnet add package $package --version "26.1.35" 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ $package installed" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Failed to install $package" -ForegroundColor Red
            Write-Host "     $result" -ForegroundColor Red
        }
    }
}

# 2. Setup Syncfusion license
if (!$SkipLicense) {
    Write-Host "`n🔑 Setting up Syncfusion license..." -ForegroundColor Yellow

    # Check if App.config has license key placeholder
    if (Test-Path "App.config") {
        $configContent = Get-Content "App.config" -Raw
        if ($configContent -match "YOUR_SYNCFUSION_LICENSE_KEY_HERE") {
            Write-Host "⚠️  Syncfusion license key needs to be configured" -ForegroundColor Yellow
            Write-Host "   1. Go to https://www.syncfusion.com/account/downloads" -ForegroundColor White
            Write-Host "   2. Get your license key" -ForegroundColor White
            Write-Host "   3. Replace 'YOUR_SYNCFUSION_LICENSE_KEY_HERE' in App.config" -ForegroundColor White
            Write-Host "   OR" -ForegroundColor Cyan
            Write-Host "   4. Set environment variable: SYNCFUSION_LICENSE_KEY" -ForegroundColor White
        } else {
            Write-Host "✅ App.config license configuration found" -ForegroundColor Green
        }
    }
}

# 3. Restore packages
Write-Host "`n🔄 Restoring NuGet packages..." -ForegroundColor Yellow
$restoreResult = dotnet restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package restore completed" -ForegroundColor Green
} else {
    Write-Host "❌ Package restore failed:" -ForegroundColor Red
    Write-Host $restoreResult -ForegroundColor Red
    exit 1
}

# 4. Test build
Write-Host "`n🔨 Testing build..." -ForegroundColor Yellow
$buildResult = dotnet build --configuration Release --verbosity quiet 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed:" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    Write-Host "`n🔍 Common issues:" -ForegroundColor Yellow
    Write-Host "  - Ensure Syncfusion license is properly configured" -ForegroundColor White
    Write-Host "  - Check that all package references are compatible" -ForegroundColor White
    exit 1
}

# 5. Summary and next steps
Write-Host "`n🎉 Quick fixes completed!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green

Write-Host "`n📋 NEXT STEPS:" -ForegroundColor Cyan
Write-Host "1. Configure Syncfusion license key if not done yet" -ForegroundColor White
Write-Host "2. Run the diagnostics again: pwsh scripts/diagnostics.ps1" -ForegroundColor White
Write-Host "3. If all checks pass, run: dotnet run --configuration Release" -ForegroundColor White

Write-Host "`n🔑 SYNCFUSION LICENSE SETUP:" -ForegroundColor Magenta
Write-Host "• Community Edition: Works with limited controls" -ForegroundColor White
Write-Host "• Licensed Edition: Full feature access" -ForegroundColor White
Write-Host "• Get license: https://www.syncfusion.com/account" -ForegroundColor White

Write-Host "`n✅ Quick fix script completed!" -ForegroundColor Green
