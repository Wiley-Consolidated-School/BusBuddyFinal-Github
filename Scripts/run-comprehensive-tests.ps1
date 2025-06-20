# Comprehensive Test Runner for BusBuddy
# Runs all test categories with detailed reporting and coverage analysis

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Unit", "Integration", "UI", "EndToEnd", "Performance", "Security")]
    [string]$TestCategory = "All",

    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,

    [Parameter(Mandatory=$false)]
    [switch]$GenerateCoverage,

    [Parameter(Mandatory=$false)]
    [switch]$GenerateReport,

    [Parameter(Mandatory=$false)]
    [switch]$Verbose,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./TestResults",

    [Parameter(Mandatory=$false)]
    [switch]$ContinueOnFailure
)

# Set error handling
$ErrorActionPreference = "Continue"
if ($Verbose) { $VerbosePreference = "Continue" }

# Colors for output
$Green = "Green"
$Red = "Red"
$Yellow = "Yellow"
$Cyan = "Cyan"
$Blue = "Blue"

function Write-TestHeader {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor $Cyan
    Write-Host "Timestamp: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
}

function Write-TestSuccess {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-TestWarning {
    param([string]$Message)
    Write-Host "⚠️ $Message" -ForegroundColor $Yellow
}

function Write-TestError {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

function Write-TestInfo {
    param([string]$Message)
    Write-Host "ℹ️ $Message" -ForegroundColor $Blue
}

# Initialize test environment
Write-TestHeader "BusBuddy Comprehensive Test Suite"
Write-TestInfo "Test Category: $TestCategory"
Write-TestInfo "Output Path: $OutputPath"

# Create output directory
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    Write-TestInfo "Created output directory: $OutputPath"
}

# Test results tracking
$TestResults = @{
    Unit = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
    Integration = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
    UI = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
    EndToEnd = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
    Performance = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
    Security = @{ Passed = 0; Failed = 0; Skipped = 0; Duration = 0 }
}

# Build solution first (unless skipped)
if (!$SkipBuild) {
    Write-TestHeader "Building Solution"
    $buildStart = Get-Date

    try {
        dotnet build BusBuddy.sln --configuration Release --verbosity minimal
        if ($LASTEXITCODE -eq 0) {
            $buildDuration = (Get-Date) - $buildStart
            Write-TestSuccess "Build completed successfully in $($buildDuration.TotalSeconds.ToString('F2')) seconds"
        } else {
            Write-TestError "Build failed with exit code $LASTEXITCODE"
            if (!$ContinueOnFailure) { exit $LASTEXITCODE }
        }
    }
    catch {
        Write-TestError "Build failed: $($_.Exception.Message)"
        if (!$ContinueOnFailure) { exit 1 }
    }
} else {
    Write-TestInfo "Skipping build as requested"
}

# Function to run test category
function Invoke-TestCategory {
    param(
        [string]$Category,
        [string]$Filter,
        [string]$Description
    )

    Write-TestHeader "Running $Description Tests"
    $testStart = Get-Date
      # Prepare test command
    $verbosityLevel = if ($Verbose) { "detailed" } else { "normal" }
    $testArgs = @(
        "test"
        "BusBuddy.Tests/BusBuddy.Tests.csproj"
        "--configuration", "Release"
        "--verbosity", $verbosityLevel
        "--logger", "trx;LogFileName=$Category-results.trx"
        "--results-directory", $OutputPath
    )

    if ($Filter) {
        $testArgs += "--filter", $Filter
    }

    if ($GenerateCoverage) {
        $testArgs += @(
            "--collect", "XPlat Code Coverage"
            "--settings", "coverlet.runsettings"
        )
    }

    try {
        Write-TestInfo "Running: dotnet $($testArgs -join ' ')"
        $output = & dotnet @testArgs 2>&1
        $exitCode = $LASTEXITCODE

        # Parse test results from output
        $passed = 0
        $failed = 0
        $skipped = 0

        if ($output -match "Passed!\s*-\s*Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+)") {
            $failed = [int]$matches[1]
            $passed = [int]$matches[2]
            $skipped = [int]$matches[3]        } elseif ($output -match "Test Run (?:Successful|Failed)\.\s*Total tests:\s*(\d+)\s*(?:Passed:\s*(\d+))?\s*(?:Failed:\s*(\d+))?\s*(?:Skipped:\s*(\d+))?") {
            # $total = [int]$matches[1] # Total available if needed for other calculations
            if ($matches[2]) { $passed = [int]$matches[2] }
            if ($matches[3]) { $failed = [int]$matches[3] }
            if ($matches[4]) { $skipped = [int]$matches[4] }
        }

        $testDuration = (Get-Date) - $testStart

        # Update test results
        $TestResults[$Category].Passed = $passed
        $TestResults[$Category].Failed = $failed
        $TestResults[$Category].Skipped = $skipped
        $TestResults[$Category].Duration = $testDuration.TotalSeconds

        if ($exitCode -eq 0) {
            Write-TestSuccess "$Description tests completed - Passed: $passed, Failed: $failed, Skipped: $skipped"
        } else {
            Write-TestError "$Description tests failed - Passed: $passed, Failed: $failed, Skipped: $skipped"
            if ($Verbose) {
                Write-Host "Test output:" -ForegroundColor Gray
                $output | Write-Host -ForegroundColor Gray
            }
        }

        Write-TestInfo "Duration: $($testDuration.TotalSeconds.ToString('F2')) seconds"

    }
    catch {
        Write-TestError "$Description tests failed with exception: $($_.Exception.Message)"
        if (!$ContinueOnFailure) { throw }
    }
}

# Run tests based on category
switch ($TestCategory) {
    "All" {
        Invoke-TestCategory "Unit" "TestCategory=Unit" "Unit"
        Invoke-TestCategory "Integration" "TestCategory=Integration" "Integration"
        Invoke-TestCategory "UI" "TestCategory=UIComponent" "UI Component"
        Invoke-TestCategory "EndToEnd" "TestCategory=EndToEnd" "End-to-End"
        Invoke-TestCategory "Performance" "TestCategory=Performance" "Performance"
        Invoke-TestCategory "Security" "TestCategory=Security" "Security"
    }
    "Unit" {
        Invoke-TestCategory "Unit" "TestCategory=Unit" "Unit"
    }
    "Integration" {
        Invoke-TestCategory "Integration" "TestCategory=Integration" "Integration"
    }
    "UI" {
        Invoke-TestCategory "UI" "TestCategory=UIComponent|TestCategory=Navigation|TestCategory=Theme" "UI"
    }
    "EndToEnd" {
        Invoke-TestCategory "EndToEnd" "TestCategory=EndToEnd|TestCategory=UserJourney" "End-to-End"
    }
    "Performance" {
        Invoke-TestCategory "Performance" "TestCategory=Performance" "Performance"
    }
    "Security" {
        Invoke-TestCategory "Security" "TestCategory=Security" "Security"
    }
}

# Generate coverage report if requested
if ($GenerateCoverage) {
    Write-TestHeader "Generating Code Coverage Report"

    try {
        # Find coverage files
        $coverageFiles = Get-ChildItem -Path $OutputPath -Filter "coverage.cobertura.xml" -Recurse

        if ($coverageFiles.Count -gt 0) {
            # Install ReportGenerator if not available
            if (!(Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
                Write-TestInfo "Installing ReportGenerator..."
                dotnet tool install --global dotnet-reportgenerator-globaltool
            }

            # Generate HTML coverage report
            $coverageReportPath = Join-Path $OutputPath "coverage-report"
            $coverageFiles | ForEach-Object { $_.FullName } | Join-String -Separator ";" | Set-Variable coverageFileList

            reportgenerator "-reports:$coverageFileList" "-targetdir:$coverageReportPath" "-reporttypes:Html;Badges"

            Write-TestSuccess "Coverage report generated at: $coverageReportPath"

            # Display coverage summary
            $coverageSummary = Get-Content (Join-Path $coverageReportPath "index.html") -Raw
            if ($coverageSummary -match "Line coverage.*?(\d+(?:\.\d+)?)%") {
                $lineeCoverage = $matches[1]
                Write-TestInfo "Overall Line Coverage: $lineeCoverage%"

                if ([double]$lineeCoverage -ge 80) {
                    Write-TestSuccess "Coverage meets target (≥80%)"
                } else {
                    Write-TestWarning "Coverage below target (<80%)"
                }
            }
        } else {
            Write-TestWarning "No coverage files found"
        }
    }
    catch {
        Write-TestError "Coverage report generation failed: $($_.Exception.Message)"
    }
}

# Generate test summary report
Write-TestHeader "Test Execution Summary"

$totalPassed = 0
$totalFailed = 0
$totalSkipped = 0
$totalDuration = 0

Write-Host "`nTest Category Results:" -ForegroundColor $Cyan
Write-Host $("=" * 80) -ForegroundColor Gray
Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12}" -f "Category", "Passed", "Failed", "Skipped", "Duration(s)") -ForegroundColor $Blue

foreach ($category in $TestResults.Keys) {
    $result = $TestResults[$category]
    $color = if ($result.Failed -gt 0) { $Red } else { $Green }

    Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12:F2}" -f
        $category,
        $result.Passed,
        $result.Failed,
        $result.Skipped,
        $result.Duration
    ) -ForegroundColor $color

    $totalPassed += $result.Passed
    $totalFailed += $result.Failed
    $totalSkipped += $result.Skipped
    $totalDuration += $result.Duration
}

Write-Host $("=" * 80) -ForegroundColor Gray
Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12:F2}" -f
    "TOTAL",
    $totalPassed,
    $totalFailed,
    $totalSkipped,
    $totalDuration
) -ForegroundColor $(if ($totalFailed -gt 0) { $Red } else { $Green })

# Calculate success rate
$totalTests = $totalPassed + $totalFailed + $totalSkipped
if ($totalTests -gt 0) {
    $successRate = ($totalPassed / $totalTests) * 100
    Write-Host "`nSuccess Rate: $($successRate.ToString('F2'))%" -ForegroundColor $(if ($successRate -ge 95) { $Green } elseif ($successRate -ge 80) { $Yellow } else { $Red })
}

# Performance summary
if ($TestCategory -eq "All" -or $TestCategory -eq "Performance") {
    Write-Host "`nPerformance Metrics:" -ForegroundColor $Cyan
    Write-Host "- Dashboard initialization target: <2000ms"
    Write-Host "- Route analytics target: <5000ms for 1000 routes"
    Write-Host "- Navigation response target: <100ms average"
    Write-Host "- Memory usage target: <50MB growth during extended operation"
}

# Security summary
if ($TestCategory -eq "All" -or $TestCategory -eq "Security") {
    Write-Host "`nSecurity Validation:" -ForegroundColor $Cyan
    Write-Host "- Data encryption verification"
    Write-Host "- SQL injection prevention testing"
    Write-Host "- Access control validation"
    Write-Host "- Input sanitization verification"
}

# Final status
Write-TestHeader "Test Execution Complete"

if ($totalFailed -eq 0) {
    Write-TestSuccess "All tests passed successfully!"
    $exitCode = 0
} else {
    Write-TestError "$totalFailed test(s) failed"
    $exitCode = 1
}

Write-TestInfo "Total execution time: $($totalDuration.ToString('F2')) seconds"
Write-TestInfo "Results saved to: $OutputPath"

# Generate JSON summary for CI/CD
if ($GenerateReport) {
    $jsonSummary = @{
        timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
        testCategory = $TestCategory
        summary = @{
            totalTests = $totalTests
            passed = $totalPassed
            failed = $totalFailed
            skipped = $totalSkipped
            duration = $totalDuration
            successRate = if ($totalTests -gt 0) { ($totalPassed / $totalTests) * 100 } else { 0 }
        }
        categories = $TestResults
    } | ConvertTo-Json -Depth 3

    $jsonPath = Join-Path $OutputPath "test-summary.json"
    $jsonSummary | Out-File -FilePath $jsonPath -Encoding utf8
    Write-TestInfo "JSON summary saved to: $jsonPath"
}

exit $exitCode
