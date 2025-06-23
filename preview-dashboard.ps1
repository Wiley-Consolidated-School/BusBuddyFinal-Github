#!/usr/bin/env pwsh
# Task 8 Dashboard Preview Script
# Run this to test the new Office2016Black/White theme toggle

Write-Host "🚌 Starting BusBuddy Dashboard Preview..." -ForegroundColor Cyan
Write-Host "📋 Task 8: Theme Toggle Testing" -ForegroundColor Yellow

# Build first
Write-Host "🔨 Building solution..." -ForegroundColor Yellow
dotnet build BusBuddy.sln --configuration Debug

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green

    Write-Host "🎨 Starting dashboard with theme toggle..." -ForegroundColor Cyan
    Write-Host "💡 Look for the 'Switch to Light' button to test theme toggle" -ForegroundColor Magenta
    Write-Host "🎯 Default theme: Office2016Black (dark)" -ForegroundColor White -BackgroundColor DarkGray
    Write-Host "🎯 Toggle theme: Office2016White (light)" -ForegroundColor Black -BackgroundColor White

    # Run the main BusBuddy executable project
    dotnet run --project BusBuddy.csproj --configuration Debug
} else {
    Write-Host "❌ Build failed. Please check errors above." -ForegroundColor Red
}
