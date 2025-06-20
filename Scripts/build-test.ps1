# Quick Build and Test Script for BusBuddy
# Performs essential checks and builds the application

param(
    [switch]$Clean = $false,
    [switch]$Debug = $false
)

Write-Host "🚀 BusBuddy Quick Build & Test" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

$config = if ($Debug) { "Debug" } else { "Release" }

# Clean if requested
if ($Clean) {
    Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean --configuration $config
    Remove-Item -Path "bin", "obj" -Recurse -Force -ErrorAction SilentlyContinue
}

# Restore packages
Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Cyan
$restoreResult = dotnet restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Package restore failed:" -ForegroundColor Red
    Write-Host $restoreResult -ForegroundColor Red
    exit 1
}
Write-Host "✅ Packages restored successfully" -ForegroundColor Green

# Build
Write-Host "🔨 Building project ($config)..." -ForegroundColor Cyan
$buildResult = dotnet build --configuration $config --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed:" -ForegroundColor Red
    Write-Host $buildResult -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build completed successfully" -ForegroundColor Green

# Check for common issues
Write-Host "🔍 Performing basic checks..." -ForegroundColor Cyan

# Check if main executable exists
$exePath = "bin\$config\net8.0-windows\BusBuddy.exe"
if (Test-Path $exePath) {
    Write-Host "✅ Executable found: $exePath" -ForegroundColor Green
} else {
    Write-Host "❌ Executable not found: $exePath" -ForegroundColor Red
}

# Check for required configuration
if (Test-Path "App.config") {
    Write-Host "✅ App.config found" -ForegroundColor Green
} else {
    Write-Host "⚠️ App.config not found - creating basic version..." -ForegroundColor Yellow

    $basicConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <!-- Add your Syncfusion license key here -->
    <add key="SyncfusionLicenseKey" value="" />
  </appSettings>

  <connectionStrings>
    <!-- Default SQLite connection -->
    <add name="DefaultConnection"
         connectionString="Data Source=busbuddy.db;"
         providerName="Microsoft.Data.Sqlite" />

    <!-- SQL Server Express example (uncomment and modify as needed) -->
    <!--
    <add name="DefaultConnection"
         connectionString="Server=localhost\SQLEXPRESS;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;"
         providerName="Microsoft.Data.SqlClient" />
    -->
  </connectionStrings>

  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
</configuration>
"@

    Set-Content -Path "App.config" -Value $basicConfig
    Write-Host "✅ Basic App.config created" -ForegroundColor Green
}

Write-Host "`n🎯 READY TO RUN!" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green
Write-Host "To start the application:" -ForegroundColor White
Write-Host "  dotnet run --configuration $config" -ForegroundColor Yellow
Write-Host ""
Write-Host "To run diagnostics first:" -ForegroundColor White
Write-Host "  pwsh scripts/diagnostics.ps1" -ForegroundColor Yellow
Write-Host ""
Write-Host "To clean up old forms:" -ForegroundColor White
Write-Host "  pwsh scripts/cleanup-forms.ps1 -DryRun" -ForegroundColor Yellow

Write-Host "`n✅ Build and test completed successfully!" -ForegroundColor Green
