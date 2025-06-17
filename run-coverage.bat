@echo off
title BusBuddy Code Coverage Runner

echo 🚌 BusBuddy Code Coverage Runner
echo ================================

REM Check if PowerShell is available
powershell -Command "Write-Host 'PowerShell is available'" >nul 2>&1
if errorlevel 1 (
    echo ❌ PowerShell is not available. Please install PowerShell.
    pause
    exit /b 1
)

echo ℹ️  Running PowerShell coverage script...
powershell -ExecutionPolicy Bypass -File ".\run-coverage.ps1" -Clean

echo.
echo 📋 Press any key to continue...
pause >nul
