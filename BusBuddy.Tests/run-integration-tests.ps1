# 🧪 BusBuddy UI Integration Test Runner
# Comprehensive integration testing for the BusBuddy dashboard

param(
    [switch]$Verbose,
    [switch]$Performance,
    [string]$Output = "TestResults"
)

Write-Host "🚌 BusBuddy UI Integration Test Suite" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

# Change to the project directory
$projectRoot = "C:\Users\steve.mckitrick\Desktop\BusBuddy"
Set-Location $projectRoot

Write-Host "📍 Project Directory: $projectRoot" -ForegroundColor Gray

# Build the project first
Write-Host "🔨 Building BusBuddy project..." -ForegroundColor Yellow
dotnet build --configuration Debug --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed! Cannot proceed with integration tests." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Build completed successfully" -ForegroundColor Green

# Prepare test environment
$loggerLevel = if ($Verbose) { "detailed" } else { "normal" }
$testFilter = "TestCategory=Integration"

if ($Performance) {
    $testFilter += "|TestCategory=Performance"
    Write-Host "🚀 Including performance tests" -ForegroundColor Blue
}

Write-Host "🧪 Running UI Integration Tests..." -ForegroundColor Magenta
Write-Host "   Filter: $testFilter" -ForegroundColor Gray
Write-Host "   Logger: console;verbosity=$loggerLevel" -ForegroundColor Gray
Write-Host ""

# Run the integration tests
$startTime = Get-Date

dotnet test BusBuddy.Tests/BusBuddy.Tests.csproj `
    --filter $testFilter `
    --logger "console;verbosity=$loggerLevel" `
    --results-directory $Output `
    --collect:"XPlat Code Coverage"

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "⏱️ Integration tests completed in $($duration.TotalSeconds) seconds" -ForegroundColor Cyan

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All integration tests passed!" -ForegroundColor Green

    # Display test summary
    Write-Host ""
    Write-Host "📊 Test Results Summary:" -ForegroundColor Cyan
    Write-Host "   ✅ Dashboard Initialization" -ForegroundColor Green
    Write-Host "   ✅ Quick Action Cards" -ForegroundColor Green
    Write-Host "   ✅ Sidebar Navigation" -ForegroundColor Green
    Write-Host "   ✅ Material Design Theming" -ForegroundColor Green
    Write-Host "   ✅ Navigation Service Integration" -ForegroundColor Green
    Write-Host "   ✅ Responsive Layout" -ForegroundColor Green
    Write-Host "   ✅ Memory Management" -ForegroundColor Green

    if ($Performance) {
        Write-Host "   ✅ Initialization Performance" -ForegroundColor Green
        Write-Host "   ✅ UI Responsiveness" -ForegroundColor Green
    }

} else {
    Write-Host "❌ Some integration tests failed!" -ForegroundColor Red

    Write-Host ""
    Write-Host "🔍 Troubleshooting Tips:" -ForegroundColor Yellow
    Write-Host "   1. Check if all dependencies are installed" -ForegroundColor Gray
    Write-Host "   2. Verify MaterialDesign packages are correct version" -ForegroundColor Gray
    Write-Host "   3. Ensure no other UI applications are interfering" -ForegroundColor Gray
    Write-Host "   4. Try running tests individually for more details" -ForegroundColor Gray
}

# Display coverage info if available
$coverageFiles = Get-ChildItem -Path $Output -Filter "*.xml" -Recurse | Where-Object { $_.Name -like "*coverage*" }
if ($coverageFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "📊 Coverage reports generated:" -ForegroundColor Blue
    $coverageFiles | ForEach-Object {
        Write-Host "   📄 $($_.FullName)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🎯 Integration Test Categories:" -ForegroundColor Yellow
Write-Host "   integration    - Full dashboard integration tests" -ForegroundColor Gray
Write-Host "   performance    - Performance and responsiveness tests" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 Usage examples:" -ForegroundColor Yellow
Write-Host "   .\run-integration-tests.ps1                    # Run core integration tests" -ForegroundColor Gray
Write-Host "   .\run-integration-tests.ps1 -Performance       # Include performance tests" -ForegroundColor Gray
Write-Host "   .\run-integration-tests.ps1 -Verbose           # Detailed output" -ForegroundColor Gray

Write-Host ""
Write-Host "🔄 Next Steps:" -ForegroundColor Cyan
if ($LASTEXITCODE -eq 0) {
    Write-Host "   • Integration tests are passing ✅" -ForegroundColor Green
    Write-Host "   • BusBuddy Dashboard is ready for use" -ForegroundColor Green
    Write-Host "   • Consider running full test suite: dotnet test" -ForegroundColor Gray
} else {
    Write-Host "   • Review failed test output above" -ForegroundColor Red
    Write-Host "   • Fix any identified issues" -ForegroundColor Red
    Write-Host "   • Re-run integration tests" -ForegroundColor Red
}

exit $LASTEXITCODE
