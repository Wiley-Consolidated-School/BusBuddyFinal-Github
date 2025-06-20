#!/usr/bin/env pwsh

# BusBuddy Git Push Script
# Handles staging, committing, and pushing changes to GitHub

Write-Host "🚀 BusBuddy Git Push Process" -ForegroundColor Cyan

# Navigate to project directory
Set-Location "c:\Users\steve.mckitrick\Desktop\BusBuddy"

try {
    # Check git status
    Write-Host "📋 Checking git status..." -ForegroundColor Yellow
    git status --porcelain

    # Stage all changes
    Write-Host "📦 Staging changes..." -ForegroundColor Yellow
    git add .

    # Show what will be committed
    Write-Host "📝 Changes to be committed:" -ForegroundColor Yellow
    git diff --cached --name-only

    # Commit with descriptive message
    $commitMessage = "feat: Enhanced BusBuddyDashboardSyncfusion with Syncfusion controls

- Replaced MaterialSkin2 with professional Syncfusion WinForms controls
- Added theme toggle functionality with light/dark mode support
- Implemented responsive layout with DPI scaling
- Enhanced analytics with ChartControl and RadialGauge components
- Added vector-based icon system for crisp, scalable UI elements
- Improved navigation with dictionary-based method mapping
- Added comprehensive error handling and fallback layouts
- Integrated real-time data from DatabaseHelperService
- Enhanced UI/UX with Material Design theming
- Fixed compilation issues and improved code reliability

This represents a major upgrade to the dashboard UI, leveraging Syncfusion's
enterprise-grade controls for a more professional user experience."

    Write-Host "💾 Committing changes..." -ForegroundColor Yellow
    git commit -m $commitMessage

    # Push to remote repository
    Write-Host "🌐 Pushing to GitHub..." -ForegroundColor Yellow
    git push origin main

    Write-Host "✅ Successfully pushed to GitHub!" -ForegroundColor Green
    Write-Host "🎉 BusBuddy dashboard enhancements are now live on GitHub" -ForegroundColor Green

} catch {
    Write-Host "❌ Error during git operations: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
