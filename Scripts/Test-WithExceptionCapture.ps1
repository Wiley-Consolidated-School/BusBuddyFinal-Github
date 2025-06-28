#!/usr/bin/env pwsh
<#
.SYNOPSIS
Test Runner with Exception Capture for BusBuddy
#>

param([switch]$BuildFirst)

if ($BuildFirst) {
    Write-Host "🔨 Building with exception capture..."
    dotnet build
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

Write-Host "🧪 Running tests with exception capture..."

# Clear previous exception logs
Remove-Item "logs\exception_*.log" -ErrorAction SilentlyContinue
Remove-Item "logs\exception_*.json" -ErrorAction SilentlyContinue

# Run application with exception capture
Write-Host "▶️ Starting BusBuddy with exception monitoring..."

$job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --no-build 2>&1
} -Name "BusBuddyWithCapture"

# Monitor for exception files
$timeout = 30
$elapsed = 0

while ($elapsed -lt $timeout -and $job.State -eq "Running") {
    Start-Sleep -Seconds 1
    $elapsed++

    # Check for new exception files
    $exceptionFiles = Get-ChildItem "logs\exception_*.log" -ErrorAction SilentlyContinue
    if ($exceptionFiles) {
        Write-Host "🚨 Exception detected! Files:"
        $exceptionFiles | ForEach-Object { Write-Host "   📄 $($_.Name)" }

        # Show recent content
        $latestLog = $exceptionFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Write-Host "📋 Latest exception content:"
        Get-Content $latestLog.FullName -Tail 10 | ForEach-Object { Write-Host "   $_" }
        break
    }

    Write-Progress -Activity "Monitoring for exceptions" -SecondsRemaining ($timeout - $elapsed)
}

# Stop the job
Stop-Job $job -ErrorAction SilentlyContinue
Remove-Job $job -ErrorAction SilentlyContinue

# Summary
$allExceptionFiles = Get-ChildItem "logs\exception_*" -ErrorAction SilentlyContinue
if ($allExceptionFiles) {
    Write-Host "📊 Exception Summary:"
    Write-Host "   Total files: $($allExceptionFiles.Count)"
    $allExceptionFiles | ForEach-Object {
        Write-Host "   📄 $($_.Name) ($($_.Length) bytes)"
    }
} else {
    Write-Host "✅ No exceptions detected during test run"
}
