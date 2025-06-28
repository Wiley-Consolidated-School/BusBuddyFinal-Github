#!/usr/bin/env pwsh
<#
.SYNOPSIS
    BusBuddy Optimized Build Script - Enhanced Performance Configuration
.DESCRIPTION
    Builds BusBuddy with advanced .NET 8 performance optimizations including:
    - ReadyToRun (R2R) AOT compilation
    - Self-contained deployment with trimming
    - Performance profiling and metrics
.PARAMETER Configuration
    Build configuration (Debug/Release). Default: Release
.PARAMETER Target
    Build target: Build, Publish, PublishSingleFile, or Profile
.PARAMETER RuntimeId
    Target runtime identifier. Default: win-x64
.PARAMETER Measure
    Measure build time and startup performance
.EXAMPLE
    .\build-optimized.ps1 -Target Publish -Measure
.EXAMPLE
    .\build-optimized.ps1 -Target PublishSingleFile -Configuration Release
#>

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("Build", "Publish", "PublishSingleFile", "Profile")]
    [string]$Target = "Build",

    [string]$RuntimeId = "win-x64",

    [switch]$Measure
)

# Performance tracking
$buildTimer = [System.Diagnostics.Stopwatch]::StartNew()
$logFile = "build-performance-$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry -ForegroundColor $(if($Level -eq "ERROR") {"Red"} elseif($Level -eq "WARN") {"Yellow"} else {"Green"})
    $logEntry | Out-File -FilePath $logFile -Append
}

function Measure-BuildStep {
    param([string]$StepName, [scriptblock]$ScriptBlock)

    Write-Log "Starting: $StepName"
    $stepTimer = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        & $ScriptBlock
        $stepTimer.Stop()
        Write-Log "Completed: $StepName in $($stepTimer.Elapsed.TotalSeconds.ToString('F1'))s"
    }
    catch {
        $stepTimer.Stop()
        Write-Log "Failed: $StepName after $($stepTimer.Elapsed.TotalSeconds.ToString('F1'))s - $($_.Exception.Message)" "ERROR"
        throw
    }
}

Write-Log "=== BusBuddy Optimized Build Started ==="
Write-Log "Configuration: $Configuration"
Write-Log "Target: $Target"
Write-Log "Runtime: $RuntimeId"

try {
    # Clean previous builds
    Measure-BuildStep "Clean Solution" {
        dotnet clean BusBuddy.sln -c $Configuration
        if (Test-Path "bin") { Remove-Item "bin" -Recurse -Force }
        if (Test-Path "obj") { Remove-Item "obj" -Recurse -Force }
    }

    # Build based on target
    switch ($Target) {
        "Build" {
            Measure-BuildStep "Standard Build" {
                dotnet build BusBuddy.sln -c $Configuration --verbosity minimal
            }
        }

        "Publish" {
            Measure-BuildStep "Optimized Publish (ReadyToRun Only)" {
                dotnet publish BusBuddy.sln -c $Configuration -r $RuntimeId --self-contained `
                    -p:PublishReadyToRun=true `
                    -p:PublishTrimmed=false `
                    --verbosity minimal
            }
        }

        "PublishSingleFile" {
            Measure-BuildStep "Single File Optimized Publish" {
                dotnet publish BusBuddy.sln -c $Configuration -r $RuntimeId --self-contained `
                    -p:PublishReadyToRun=true `
                    -p:PublishSingleFile=true `
                    -p:IncludeNativeLibrariesForSelfExtract=true `
                    --verbosity minimal
            }
        }

        "Profile" {
            Measure-BuildStep "Profile Build (Optimized with Debug Info)" {
                dotnet build BusBuddy.sln -c $Configuration -p:DebugType=portable --verbosity minimal
            }
        }
    }

    # Measure startup performance if requested
    if ($Measure) {
        Measure-BuildStep "Startup Performance Test" {
            $exePath = if ($Target -eq "Build") {
                "bin\$Configuration\net8.0-windows\BusBuddy.exe"
            } else {
                "bin\$Configuration\net8.0-windows\$RuntimeId\publish\BusBuddy.exe"
            }

            if (Test-Path $exePath) {
                Write-Log "Testing startup performance: $exePath"

                # Measure cold start (3 runs, take average)
                $startupTimes = @()
                for ($i = 1; $i -le 3; $i++) {
                    $startupTimer = [System.Diagnostics.Stopwatch]::StartNew()
                    $process = Start-Process -FilePath $exePath -PassThru -WindowStyle Hidden

                    # Wait for main window or 10 seconds max
                    $timeout = 10000
                    while ($timeout -gt 0 -and !$process.MainWindowHandle) {
                        Start-Sleep -Milliseconds 100
                        $timeout -= 100
                    }

                    $startupTimer.Stop()
                    $startupTimes += $startupTimer.Elapsed.TotalSeconds

                    if (!$process.HasExited) {
                        $process.CloseMainWindow()
                        Start-Sleep -Seconds 1
                        if (!$process.HasExited) {
                            $process.Kill()
                        }
                    }

                    Write-Log "Run $i startup time: $($startupTimer.Elapsed.TotalSeconds.ToString('F2'))s"
                }

                $avgStartup = ($startupTimes | Measure-Object -Average).Average
                Write-Log "Average startup time: $($avgStartup.ToString('F2'))s" "INFO"

                # Compare to target
                $targetTime = 2.0
                $improvement = if ($avgStartup -lt $targetTime) {
                    "🎯 ACHIEVED: Under ${targetTime}s target!"
                } else {
                    $remaining = $avgStartup - $targetTime
                    "⚡ Need $($remaining.ToString('F2'))s improvement for sub-${targetTime}s target"
                }
                Write-Log $improvement
            } else {
                Write-Log "Executable not found: $exePath" "WARN"
            }
        }
    }

    $buildTimer.Stop()
    Write-Log "=== Build Completed Successfully in $($buildTimer.Elapsed.TotalSeconds.ToString('F1'))s ==="

    # Build summary
    Write-Host "`n📊 Build Summary:" -ForegroundColor Cyan
    Write-Host "   Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "   Target: $Target" -ForegroundColor Gray
    Write-Host "   Runtime: $RuntimeId" -ForegroundColor Gray
    Write-Host "   Total Time: $($buildTimer.Elapsed.TotalSeconds.ToString('F1'))s" -ForegroundColor Gray
    Write-Host "   Log File: $logFile" -ForegroundColor Gray

    if ($Target -ne "Build") {
        $publishDir = "bin\$Configuration\net8.0-windows\$RuntimeId\publish"
        if (Test-Path $publishDir) {
            $size = (Get-ChildItem $publishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
            Write-Host "   Publish Size: $($size.ToString('F1')) MB" -ForegroundColor Gray
        }
    }

} catch {
    $buildTimer.Stop()
    Write-Log "Build failed after $($buildTimer.Elapsed.TotalSeconds.ToString('F1'))s: $($_.Exception.Message)" "ERROR"
    exit 1
}
