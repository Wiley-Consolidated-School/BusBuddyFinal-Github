#!/usr/bin/env pwsh
# format-check.ps1 - Check code formatting for BusBuddy

Write-Host "🎨 Checking code formatting..."
dotnet format BusBuddy.sln --verify-no-changes
exit $LASTEXITCODE
