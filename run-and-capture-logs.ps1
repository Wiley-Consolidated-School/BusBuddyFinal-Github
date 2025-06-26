# Run BusBuddy and capture logs with comprehensive error handling

Write-Host "Starting BusBuddy with comprehensive logging..." -ForegroundColor Green

# Change to project directory
Set-Location "c:\Users\steve.mckitrick\Desktop\BusBuddy"

# Clean up previous logs
$logFiles = Get-ChildItem -Path . -Filter "dashboard_startup_*.log" -ErrorAction SilentlyContinue
if ($logFiles) {
    Write-Host "Cleaning up $($logFiles.Count) previous log files..." -ForegroundColor Yellow
    $logFiles | Remove-Item -Force
}

# Also check Desktop\BusBuddy folder for logs
$desktopLogPath = Join-Path $env:USERPROFILE "Desktop\BusBuddy"
if (Test-Path $desktopLogPath) {
    $desktopLogFiles = Get-ChildItem -Path $desktopLogPath -Filter "dashboard_startup_*.log" -ErrorAction SilentlyContinue
    if ($desktopLogFiles) {
        Write-Host "Cleaning up $($desktopLogFiles.Count) previous log files from Desktop..." -ForegroundColor Yellow
        $desktopLogFiles | Remove-Item -Force
    }
}

Write-Host "Running: dotnet run" -ForegroundColor Cyan

# Start the application in the background and capture output
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -NoNewWindow -PassThru -RedirectStandardOutput "console_output.txt" -RedirectStandardError "console_error.txt"

# Wait a few seconds for the app to start and create logs
Write-Host "Waiting for application startup..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check if the process is still running
if ($process.HasExited) {
    Write-Host "Application exited early. Exit code: $($process.ExitCode)" -ForegroundColor Red
} else {
    Write-Host "Application is running (PID: $($process.Id))" -ForegroundColor Green

    # Wait a bit more for dashboard initialization
    Start-Sleep -Seconds 3

    # Try to stop the application gracefully
    Write-Host "Stopping application..." -ForegroundColor Yellow
    try {
        $process.CloseMainWindow()
        $process.WaitForExit(5000)  # Wait up to 5 seconds
    } catch {
        Write-Host "Graceful shutdown failed, force killing..." -ForegroundColor Yellow
        $process.Kill()
    }
}

# Display console output
Write-Host "`n=== CONSOLE OUTPUT ===" -ForegroundColor Magenta
if (Test-Path "console_output.txt") {
    Get-Content "console_output.txt"
} else {
    Write-Host "No console output file found" -ForegroundColor Red
}

Write-Host "`n=== CONSOLE ERRORS ===" -ForegroundColor Magenta
if (Test-Path "console_error.txt") {
    Get-Content "console_error.txt"
} else {
    Write-Host "No console error file found" -ForegroundColor Red
}

# Look for and display the log files
Write-Host "`n=== LOG FILES ===" -ForegroundColor Magenta

# Check current directory first
$currentDirLogs = Get-ChildItem -Path . -Filter "dashboard_startup_*.log" -ErrorAction SilentlyContinue
if ($currentDirLogs) {
    foreach ($logFile in $currentDirLogs) {
        Write-Host "Found log: $($logFile.FullName)" -ForegroundColor Green
        Write-Host "=== LOG CONTENT ===" -ForegroundColor Cyan
        Get-Content $logFile.FullName
        Write-Host "=== END LOG ===" -ForegroundColor Cyan
    }
} else {
    Write-Host "No log files found in current directory" -ForegroundColor Yellow
}

# Check Desktop\BusBuddy folder
if (Test-Path $desktopLogPath) {
    $desktopLogs = Get-ChildItem -Path $desktopLogPath -Filter "dashboard_startup_*.log" -ErrorAction SilentlyContinue
    if ($desktopLogs) {
        foreach ($logFile in $desktopLogs) {
            Write-Host "Found log: $($logFile.FullName)" -ForegroundColor Green
            Write-Host "=== LOG CONTENT ===" -ForegroundColor Cyan
            Get-Content $logFile.FullName
            Write-Host "=== END LOG ===" -ForegroundColor Cyan
        }
    } else {
        Write-Host "No log files found in Desktop\BusBuddy folder" -ForegroundColor Yellow
    }
} else {
    Write-Host "Desktop\BusBuddy folder does not exist" -ForegroundColor Yellow
}

Write-Host "`nLog capture complete!" -ForegroundColor Green
