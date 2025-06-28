# Script to update the BusBuddyContext.cs file with the fixed version
# This will backup the original file and replace it with our fixed version

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "BusBuddy - Updating BusBuddyContext.cs with fixed version" -ForegroundColor Cyan

$contextFilePath = Join-Path (Get-Location) "BusBuddy.Data\BusBuddyContext.cs"
$newContextFilePath = Join-Path (Get-Location) "BusBuddy.Data\BusBuddyContext.cs.new"
$backupFilePath = Join-Path (Get-Location) "BusBuddy.Data\BusBuddyContext.cs.backup_$timestamp"

# Check if files exist
if (-not (Test-Path $contextFilePath)) {
    Write-Host "❌ Error: Original context file not found at: $contextFilePath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $newContextFilePath)) {
    Write-Host "❌ Error: New context file not found at: $newContextFilePath" -ForegroundColor Red
    exit 1
}

# Create backup of original file
try {
    Copy-Item -Path $contextFilePath -Destination $backupFilePath -Force
    Write-Host "✅ Created backup of original file at: $backupFilePath" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error creating backup: $_" -ForegroundColor Red
    exit 1
}

# Replace original file with new version
try {
    Copy-Item -Path $newContextFilePath -Destination $contextFilePath -Force
    Write-Host "✅ Successfully updated BusBuddyContext.cs with fixed version" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error updating context file: $_" -ForegroundColor Red
    exit 1
}

# Delete temporary new file
try {
    Remove-Item -Path $newContextFilePath -Force
    Write-Host "✅ Cleaned up temporary file" -ForegroundColor Green
}
catch {
    Write-Host "⚠️ Warning: Could not remove temporary file: $_" -ForegroundColor Yellow
}

Write-Host "`nContext file update completed successfully!" -ForegroundColor Cyan
Write-Host "Please rebuild the solution with 'dotnet build BusBuddy.sln'" -ForegroundColor White
