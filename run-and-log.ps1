# PowerShell script to run BusBuddy and capture console output
param(
    [int]$TimeoutSeconds = 30
)

Write-Host "🚌 Starting BusBuddy with logging capture..." -ForegroundColor Green

# Create log file with timestamp
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$logFile = "BusBuddy_startup_log_$timestamp.txt"

try {
    # Start the process and capture output
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "dotnet"
    $psi.Arguments = "run --project BusBuddy.csproj"
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $psi

    # Event handlers for output
    $outputHandler = {
        param([object]$procSender, [System.Diagnostics.DataReceivedEventArgs]$e)
        if ($e.Data) {
            Write-Host $e.Data -ForegroundColor Cyan
            Add-Content -Path $logFile -Value "STDOUT: $($e.Data)"
        }
    }

    $errorHandler = {
        param([object]$procSender, [System.Diagnostics.DataReceivedEventArgs]$e)
        if ($e.Data) {
            Write-Host $e.Data -ForegroundColor Red
            Add-Content -Path $logFile -Value "STDERR: $($e.Data)"
        }
    }

    $process.add_OutputDataReceived($outputHandler)
    $process.add_ErrorDataReceived($errorHandler)

    Write-Host "Starting BusBuddy process..." -ForegroundColor Yellow
    $process.Start()
    $process.BeginOutputReadLine()
    $process.BeginErrorReadLine()

    Write-Host "Waiting for $TimeoutSeconds seconds to capture startup logs..." -ForegroundColor Yellow

    # Wait for the specified timeout or until process exits
    if ($process.WaitForExit($TimeoutSeconds * 1000)) {
        Write-Host "Process completed within timeout" -ForegroundColor Green
    } else {
        Write-Host "Timeout reached, stopping process..." -ForegroundColor Orange
        $process.Kill()
    }

    # Display log file location
    $fullLogPath = Join-Path (Get-Location) $logFile
    Write-Host "📝 Logs saved to: $fullLogPath" -ForegroundColor Green

    # Show the captured logs
    if (Test-Path $logFile) {
        Write-Host "`n=== CAPTURED STARTUP LOGS ===" -ForegroundColor Yellow
        Get-Content $logFile | ForEach-Object { Write-Host $_ -ForegroundColor White }
        Write-Host "=== END OF LOGS ===" -ForegroundColor Yellow
    }

} catch {
    Write-Host "❌ Error running BusBuddy: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    if ($process -and !$process.HasExited) {
        $process.Kill()
    }
}
