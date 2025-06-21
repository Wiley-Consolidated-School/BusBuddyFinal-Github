# Quick Memory Leak Test Runner
# Tests BusBuddy.UI for process and memory leaks

Write-Host "🔍 BusBuddy Memory Leak Detection" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Show current dotnet processes before test
Write-Host "`n📊 Dotnet processes BEFORE test:" -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -match "dotnet|testhost" } | Select-Object Id, ProcessName, StartTime | Format-Table -AutoSize

# Run the process cleanup test (safest test)
Write-Host "`n🧪 Running process cleanup test..." -ForegroundColor Green
dotnet test BusBuddy.Tests --filter "FullyQualifiedName~MemoryLeakDetectionTests.ProcessCleanup_KillHangingDotnetProcesses_ShouldCleanup" --verbosity minimal --logger console

# Show dotnet processes after test
Write-Host "`n📊 Dotnet processes AFTER test:" -ForegroundColor Yellow
Get-Process | Where-Object { $_.ProcessName -match "dotnet|testhost" } | Select-Object Id, ProcessName, StartTime | Format-Table -AutoSize

Write-Host "`n✅ Memory leak detection complete!" -ForegroundColor Green
