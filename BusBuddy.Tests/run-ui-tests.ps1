# 🧪 BusBuddy UI Test Runner
# PowerShell script to run comprehensive UI tests

param(
    [string]$Category = "All",
    [switch]$Detailed,
    [switch]$Coverage,
    [string]$Output = "TestResults"
)

Write-Host "🚌 BusBuddy UI Test Runner" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan

# Build the project first
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build --configuration Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Prepare test filter
$filter = switch ($Category.ToLower()) {
    "startup" { "TestCategory=Startup" }
    "lifecycle" { "TestCategory=FormLifecycle" }
    "theming" { "TestCategory=Theming" }
    "dpi" { "TestCategory=DPI" }
    "animation" { "TestCategory=Animation" }
    "integration" { "TestCategory=Integration" }
    "performance" { "TestCategory=Performance" }
    "mainsimulation" { "TestCategory=MainSimulation" }
    "errorhandling" { "TestCategory=ErrorHandling" }
    "responsiveness" { "TestCategory=Responsiveness" }
    "ui" { "TestCategory=Startup|TestCategory=FormLifecycle|TestCategory=Theming|TestCategory=DPI|TestCategory=Animation|TestCategory=Integration|TestCategory=Performance|TestCategory=MainSimulation|TestCategory=ErrorHandling|TestCategory=Responsiveness" }
    default { $null }
}

# Build test command
$testCommand = "dotnet test"

if ($filter) {
    $testCommand += " --filter `"$filter`""
    Write-Host "🎯 Running tests for category: $Category" -ForegroundColor Green
} else {
    Write-Host "🎯 Running all tests" -ForegroundColor Green
}

if ($Detailed) {
    $testCommand += " --logger `"console;verbosity=detailed`""
}

if ($Coverage) {
    $testCommand += " --collect:`"XPlat Code Coverage`" --results-directory $Output"
    Write-Host "📊 Code coverage enabled" -ForegroundColor Blue
}

$testCommand += " --results-directory $Output"

# Run tests
Write-Host "🏃‍♂️ Executing: $testCommand" -ForegroundColor Magenta
Write-Host ""

$startTime = Get-Date
Invoke-Expression $testCommand
$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host ""
Write-Host "⏱️ Test execution completed in $($duration.TotalSeconds) seconds" -ForegroundColor Cyan

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed!" -ForegroundColor Red
}

# Display coverage info if enabled
if ($Coverage -and $LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "📊 Coverage reports generated in: $Output" -ForegroundColor Blue

    # Try to find and display coverage summary
    $coverageFiles = Get-ChildItem -Path $Output -Filter "coverage.cobertura.xml" -Recurse
    if ($coverageFiles.Count -gt 0) {
        Write-Host "📋 Coverage files found:" -ForegroundColor Blue
        $coverageFiles | ForEach-Object { Write-Host "   $($_.FullName)" -ForegroundColor Gray }
    }
}

Write-Host ""
Write-Host "🎯 Test Categories Available:" -ForegroundColor Yellow
Write-Host "   startup       - Application initialization tests" -ForegroundColor Gray
Write-Host "   lifecycle     - Form creation/disposal tests" -ForegroundColor Gray
Write-Host "   theming       - Material Design theme tests" -ForegroundColor Gray
Write-Host "   dpi           - High DPI scaling tests" -ForegroundColor Gray
Write-Host "   animation     - Animation system tests" -ForegroundColor Gray
Write-Host "   integration   - Full integration tests" -ForegroundColor Gray
Write-Host "   performance   - Performance benchmarks" -ForegroundColor Gray
Write-Host "   mainsimulation - Program.Main() simulation" -ForegroundColor Gray
Write-Host "   errorhandling - Error recovery tests" -ForegroundColor Gray
Write-Host "   responsiveness - UI responsiveness tests" -ForegroundColor Gray
Write-Host "   ui            - All UI tests" -ForegroundColor Gray
Write-Host ""
Write-Host "📖 Usage examples:" -ForegroundColor Yellow
Write-Host "   .\run-ui-tests.ps1 -Category startup" -ForegroundColor Gray
Write-Host "   .\run-ui-tests.ps1 -Category ui -Detailed" -ForegroundColor Gray
Write-Host "   .\run-ui-tests.ps1 -Category performance -Coverage" -ForegroundColor Gray

exit $LASTEXITCODE
