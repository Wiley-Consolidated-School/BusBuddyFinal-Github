#!/usr/bin/env pwsh
# Script to run BusBuddy application for testing

Write-Host "🚀 Starting BusBuddy application..." -ForegroundColor Green

try {
    # Change to the application directory
    Set-Location "c:\Users\steve.mckitrick\Desktop\BusBuddy\bin\Debug\net8.0-windows"

    # Run the application
    Write-Host "📍 Current directory: $(Get-Location)" -ForegroundColor Blue
    Write-Host "🔍 Checking for BusBuddy.exe..." -ForegroundColor Blue

    if (Test-Path "BusBuddy.exe") {
        Write-Host "✅ Found BusBuddy.exe - Starting application..." -ForegroundColor Green
        Start-Process "BusBuddy.exe" -PassThru
        Write-Host "🎉 BusBuddy application started successfully!" -ForegroundColor Green
        Write-Host "💡 Check the application window to verify dashboard graphics rendering" -ForegroundColor Yellow
    } else {
        Write-Host "❌ BusBuddy.exe not found in current directory" -ForegroundColor Red
        Write-Host "📂 Directory contents:" -ForegroundColor Yellow
        Get-ChildItem | Select-Object Name, Length | Format-Table
    }
} catch {
    Write-Host "❌ Error starting BusBuddy: $($_.Exception.Message)" -ForegroundColor Red
}
