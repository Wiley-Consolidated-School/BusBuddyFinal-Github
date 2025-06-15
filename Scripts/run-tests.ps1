#!/usr/bin/env pwsh
# run-tests.ps1 - Run unit tests for BusBuddy

Write-Host "🧪 Running tests..."
dotnet test BusBuddy.sln --configuration Release
exit $LASTEXITCODE
