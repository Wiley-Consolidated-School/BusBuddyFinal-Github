# BusBuddy Performance Analysis Script
param(
    [int]$RuntimeSeconds = 30,
    [string]$OutputFile = "performance-analysis.txt"
)

Write-Host "🔍 BusBuddy Performance Analysis" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan

# Create performance-focused debug session
Write-Host "⚙️ Configuring for granular performance analysis..." -ForegroundColor Yellow

# Backup and modify config for granular analysis
$configPath = "App.config"
$backupPath = "App.config.perf_backup"
Copy-Item $configPath $backupPath -Force

$configContent = Get-Content $configPath -Raw
$configContent = $configContent -replace '<add key="DebugLevel" value="[^"]*"', '<add key="DebugLevel" value="Verbose"'
$configContent = $configContent -replace '<add key="PerformanceThresholdMs" value="[^"]*"', '<add key="PerformanceThresholdMs" value="25"'
$configContent = $configContent -replace '<add key="EnablePerformanceMetrics" value="[^"]*"', '<add key="EnablePerformanceMetrics" value="true"'

Set-Content $configPath $configContent -Encoding UTF8

Write-Host "🔨 Building for performance analysis..." -ForegroundColor Cyan
dotnet build BusBuddy.sln --configuration Debug --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful" -ForegroundColor Green

    # Start timed analysis
    Write-Host "🚀 Running performance analysis ($RuntimeSeconds seconds)..." -ForegroundColor Cyan

    $logFile = "logs\BusBuddy.log"
    $startTime = Get-Date

    # Clear existing logs
    if (Test-Path $logFile) {
        Remove-Item $logFile -Force
    }

    # Start application
    $process = Start-Process -FilePath "dotnet" -ArgumentList "run --project BusBuddy.csproj" -PassThru -NoNewWindow

    # Monitor for specified time
    Start-Sleep -Seconds $RuntimeSeconds

    # Stop application
    if (!$process.HasExited) {
        $process.CloseMainWindow()
        Start-Sleep -Seconds 2
        if (!$process.HasExited) {
            $process.Kill()
        }
    }

    # Analyze performance data
    Write-Host "`n📊 Performance Analysis Results:" -ForegroundColor Green
    Write-Host "=================================" -ForegroundColor Green

    if (Test-Path $logFile) {
        # Extract performance metrics
        $performanceLines = Get-Content $logFile | Where-Object { $_ -match "Performance.*ms" }
        $slowOperations = $performanceLines | Where-Object { $_ -match "exceeded threshold" }

        Write-Host "`n⚠️ Slow Operations (>25ms):" -ForegroundColor Yellow
        $slowOperations | ForEach-Object {
            if ($_ -match "Step '([^']+)' exceeded threshold: ([0-9.]+)ms") {
                $operation = $matches[1]
                $time = $matches[2]
                Write-Host "   $operation : ${time}ms" -ForegroundColor Red
            }
        }

        # Detailed performance breakdown
        Write-Host "`n📈 Performance Breakdown:" -ForegroundColor Cyan
        $performanceLines | ForEach-Object {
            if ($_ -match "\[Performance\] ([^:]+): ([0-9.]+)ms") {
                $operation = $matches[1]
                $time = $matches[2]
                $color = if ([double]$time -gt 25) { "Red" } elseif ([double]$time -gt 10) { "Yellow" } else { "Green" }
                Write-Host "   $operation : ${time}ms" -ForegroundColor $color
            }
        }

        # Memory analysis
        $memoryLines = Get-Content $logFile | Where-Object { $_ -match "Memory.*MB" }
        if ($memoryLines) {
            Write-Host "`n🧠 Memory Usage:" -ForegroundColor Cyan
            $memoryLines | Select-Object -Last 5 | ForEach-Object {
                Write-Host "   $_" -ForegroundColor White
            }
        }

        # Save detailed analysis
        $analysisContent = @"
BusBuddy Performance Analysis Report
Generated: $(Get-Date)
Runtime: $RuntimeSeconds seconds

=== SLOW OPERATIONS (>25ms) ===
$($slowOperations -join "`n")

=== ALL PERFORMANCE DATA ===
$($performanceLines -join "`n")

=== MEMORY DATA ===
$($memoryLines -join "`n")

=== FULL LOG ===
$(Get-Content $logFile -Raw)
"@

        Set-Content $OutputFile $analysisContent -Encoding UTF8
        Write-Host "`n📄 Detailed analysis saved to: $OutputFile" -ForegroundColor Green

    } else {
        Write-Host "❌ No log file found - application may not have started properly" -ForegroundColor Red
    }

} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
}

# Restore original config
if (Test-Path $backupPath) {
    Move-Item $backupPath $configPath -Force
    Write-Host "`n🔄 Restored original App.config" -ForegroundColor Yellow
}

Write-Host "`n✅ Performance analysis complete!" -ForegroundColor Green
Write-Host "💡 Focus optimization efforts on operations >25ms" -ForegroundColor Cyan
