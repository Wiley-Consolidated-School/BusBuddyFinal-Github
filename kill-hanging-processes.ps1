# kill-hanging-processes.ps1
# Script to identify and terminate hanging .NET processes, particularly after dotnet run commands

function Write-ColorOutput($Message, $Color) {
    $currentColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $currentColor
}

Write-ColorOutput "🔍 Scanning for hanging .NET processes..." "Cyan"

# Find all dotnet processes
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($null -eq $dotnetProcesses -or $dotnetProcesses.Count -eq 0) {
    Write-ColorOutput "✅ No dotnet processes found running." "Green"
}
else {
    Write-ColorOutput "Found $($dotnetProcesses.Count) dotnet processes:" "Yellow"

    $i = 0
    foreach ($process in $dotnetProcesses) {
        $i++
        $age = (Get-Date) - $process.StartTime
        $ageFormatted = "{0:D2}:{1:D2}:{2:D2}" -f $age.Hours, $age.Minutes, $age.Seconds

        # Get command line if possible
        $commandLine = ""
        try {
            $commandLine = (Get-CimInstance Win32_Process -Filter "ProcessId = $($process.Id)" -ErrorAction SilentlyContinue).CommandLine
            if ($null -eq $commandLine -or $commandLine -eq "") {
                $commandLine = "Unknown"
            }
        }
        catch {
            $commandLine = "Access denied"
        }

        # Display each process with a number for easy selection
        Write-ColorOutput "[$i] PID: $($process.Id) | Age: $ageFormatted | Memory: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB" "White"
        Write-ColorOutput "    CMD: $commandLine" "Gray"
    }

    Write-ColorOutput "`nOptions:" "Cyan"
    Write-ColorOutput "1-$($i): Kill a specific process by number" "White"
    Write-ColorOutput "A: Kill ALL dotnet processes" "Red"
    Write-ColorOutput "O: Kill older processes (>5 minutes)" "Yellow"
    Write-ColorOutput "R: Kill processes likely from 'dotnet run'" "Yellow"
    Write-ColorOutput "Q: Quit without killing anything" "Green"

    $choice = Read-Host "`nEnter your choice"

    switch ($choice) {
        "A" {
            Write-ColorOutput "Killing ALL dotnet processes..." "Red"
            foreach ($process in $dotnetProcesses) {
                try {
                    Stop-Process -Id $process.Id -Force
                    Write-ColorOutput "✓ Killed process $($process.Id)" "Green"
                }
                catch {
                    Write-ColorOutput "✗ Failed to kill process $($process.Id): $_" "Red"
                }
            }
        }
        "O" {
            Write-ColorOutput "Killing older processes (>5 minutes)..." "Yellow"
            foreach ($process in $dotnetProcesses) {
                $age = (Get-Date) - $process.StartTime
                if ($age.TotalMinutes -gt 5) {
                    try {
                        Stop-Process -Id $process.Id -Force
                        Write-ColorOutput "✓ Killed old process $($process.Id) (Age: $([math]::Round($age.TotalMinutes, 1)) minutes)" "Green"
                    }
                    catch {
                        Write-ColorOutput "✗ Failed to kill process $($process.Id): $_" "Red"
                    }
                }
            }
        }
        "R" {
            Write-ColorOutput "Killing processes likely from 'dotnet run'..." "Yellow"
            foreach ($process in $dotnetProcesses) {
                try {
                    $cmd = (Get-CimInstance Win32_Process -Filter "ProcessId = $($process.Id)" -ErrorAction SilentlyContinue).CommandLine
                    if ($cmd -like "*run*" -or $cmd -like "*exec*" -or $cmd -like "*BusBuddy*") {
                        Stop-Process -Id $process.Id -Force
                        Write-ColorOutput "✓ Killed 'run' process $($process.Id)" "Green"
                    }
                }
                catch {
                    # Skip if we can't check the command line
                }
            }
        }
        "Q" {
            Write-ColorOutput "Exiting without killing any processes." "Green"
        }
        default {
            # Check if it's a number corresponding to a process
            if ([int]::TryParse($choice, [ref]$null) -and [int]$choice -ge 1 -and [int]$choice -le $dotnetProcesses.Count) {
                $selectedProcess = $dotnetProcesses[$choice - 1]
                try {
                    Stop-Process -Id $selectedProcess.Id -Force
                    Write-ColorOutput "✓ Killed process $($selectedProcess.Id)" "Green"
                }
                catch {
                    Write-ColorOutput "✗ Failed to kill process $($selectedProcess.Id): $_" "Red"
                }
            }
            else {
                Write-ColorOutput "Invalid choice. Exiting without killing any processes." "Yellow"
            }
        }
    }
}

# Also check for testhost processes
$testhostProcesses = Get-Process -Name "testhost" -ErrorAction SilentlyContinue
if ($null -ne $testhostProcesses -and $testhostProcesses.Count -gt 0) {
    Write-ColorOutput "`nFound $($testhostProcesses.Count) testhost processes. Kill them? (Y/N)" "Yellow"
    $killTestHosts = Read-Host

    if ($killTestHosts -eq "Y" -or $killTestHosts -eq "y") {
        foreach ($process in $testhostProcesses) {
            try {
                Stop-Process -Id $process.Id -Force
                Write-ColorOutput "✓ Killed testhost process $($process.Id)" "Green"
            }
            catch {
                Write-ColorOutput "✗ Failed to kill testhost process $($process.Id): $_" "Red"
            }
        }
    }
}

Write-ColorOutput "`n✅ Process cleanup completed." "Green"
Write-ColorOutput "Run this script again anytime you need to clean up hanging processes." "Cyan"
