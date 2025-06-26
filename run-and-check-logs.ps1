# Start BusBuddy and immediately check for log files
param(
    [int]$TimeoutSeconds = 10
)

Write-Host "🚌 Starting BusBuddy and monitoring for log files..." -ForegroundColor Green

# Start the process in the background
$process = Start-Process -FilePath "bin\Debug\net8.0-windows\BusBuddy.exe" -PassThru -WindowStyle Minimized

Write-Host "Process started with ID: $($process.Id)" -ForegroundColor Yellow

# Wait a few seconds for the log file to be created
Start-Sleep -Seconds 3

# Check for log files in the desktop directory (where our logging goes)
$logPattern = "C:\Users\steve.mckitrick\Desktop\BusBuddy\dashboard_startup_*.log"
$attempts = 0
$maxAttempts = $TimeoutSeconds

while ($attempts -lt $maxAttempts) {
    $logFiles = Get-ChildItem -Path "C:\Users\steve.mckitrick\Desktop\BusBuddy" -Name "dashboard_startup_*.log" -ErrorAction SilentlyContinue | Sort-Object -Descending

    if ($logFiles) {
        $latestLog = Join-Path "C:\Users\steve.mckitrick\Desktop\BusBuddy" $logFiles[0]
        Write-Host "📝 Found log file: $latestLog" -ForegroundColor Green

        # Display the log contents
        Write-Host "`n=== DASHBOARD STARTUP LOG ===" -ForegroundColor Yellow
        Get-Content $latestLog | ForEach-Object { Write-Host $_ -ForegroundColor White }
        Write-Host "=== END OF LOG ===" -ForegroundColor Yellow
        break
    }

    Write-Host "Waiting for log file... ($attempts/$maxAttempts)" -ForegroundColor Cyan
    Start-Sleep -Seconds 1
    $attempts++
}

if (-not $logFiles) {
    Write-Host "❌ No log files found after $TimeoutSeconds seconds" -ForegroundColor Red
}

# Stop the process
if ($process -and !$process.HasExited) {
    Write-Host "Stopping BusBuddy process..." -ForegroundColor Orange
    $process.Kill()
    $process.WaitForExit(5000)
}
