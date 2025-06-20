# BusBuddy Complete Test Suite Runner
# Executes the full testing strategy including all test types and generates comprehensive reports

param(
    [Parameter(Mandatory=$false)]
    [switch]$Quick,

    [Parameter(Mandatory=$false)]
    [switch]$FullSuite,

    [Parameter(Mandatory=$false)]
    [switch]$ContinuousIntegration,

    [Parameter(Mandatory=$false)]
    [string]$OutputDirectory = "./ComprehensiveTestResults",

    [Parameter(Mandatory=$false)]
    [switch]$GenerateReports,

    [Parameter(Mandatory=$false)]
    [switch]$RunSecurityScan,

    [Parameter(Mandatory=$false)]
    [switch]$Verbose
)

# Set execution policy and error handling
$ErrorActionPreference = "Continue"
if ($Verbose) { $VerbosePreference = "Continue" }

# Color definitions
$Colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
    Header = "Magenta"
}

function Write-TestSuiteMessage {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )

    $icon = switch ($Type) {
        "Success" { "✅" }
        "Error" { "❌" }
        "Warning" { "⚠️" }
        "Header" { "🧪" }
        default { "ℹ️" }
    }

    Write-Host "$icon $Message" -ForegroundColor $Colors[$Type]
}

function Write-TestSuiteHeader {
    param([string]$Message)
    Write-Host "`n" -NoNewline
    Write-Host "=" * 80 -ForegroundColor $Colors.Header
    Write-Host "🧪 $Message" -ForegroundColor $Colors.Header
    Write-Host "=" * 80 -ForegroundColor $Colors.Header
    Write-Host "Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
}

# Initialize test suite
Write-TestSuiteHeader "BusBuddy Comprehensive Test Suite"

# Determine test strategy based on parameters
$testStrategy = if ($Quick) {
    @{
        Name = "Quick Test Suite"
        Categories = @("Unit", "UI")
        RunSecurity = $false
        GenerateFullReports = $false
        Timeout = 300 # 5 minutes
    }
} elseif ($ContinuousIntegration) {
    @{
        Name = "CI/CD Test Suite"
        Categories = @("Unit", "Integration", "UI", "Security")
        RunSecurity = $true
        GenerateFullReports = $true
        Timeout = 1800 # 30 minutes
    }
} else {
    @{
        Name = "Full Test Suite"
        Categories = @("Unit", "Integration", "UI", "EndToEnd", "Performance", "Security", "Accessibility")
        RunSecurity = $true
        GenerateFullReports = $true
        Timeout = 3600 # 60 minutes
    }
}

Write-TestSuiteMessage "Executing: $($testStrategy.Name)" "Info"
Write-TestSuiteMessage "Categories: $($testStrategy.Categories -join ', ')" "Info"
Write-TestSuiteMessage "Output Directory: $OutputDirectory" "Info"

# Create output directory structure
$directories = @(
    $OutputDirectory,
    "$OutputDirectory/TestResults",
    "$OutputDirectory/CoverageReports",
    "$OutputDirectory/SecurityResults",
    "$OutputDirectory/PerformanceResults",
    "$OutputDirectory/AccessibilityResults"
)

foreach ($dir in $directories) {
    if (!(Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
}

# Initialize test tracking
$testExecution = @{
    StartTime = Get-Date
    Results = @{}
    Summary = @{
        TotalTests = 0
        PassedTests = 0
        FailedTests = 0
        SkippedTests = 0
        TotalDuration = 0
    }
    Errors = @()
}

# Function to execute test category
function Invoke-TestCategory {
    param(
        [string]$Category,
        [hashtable]$TestExecution
    )

    Write-TestSuiteHeader "Running $Category Tests"
    $categoryStart = Get-Date

    try {        # Note: testFilter defined for reference, used by comprehensive test script internally
        $testFilter = switch ($Category) {
            "Unit" { "TestCategory=Unit" }
            "Integration" { "TestCategory=Integration" }
            "UI" { "TestCategory=UIComponent|TestCategory=Navigation|TestCategory=Theme" }
            "EndToEnd" { "TestCategory=EndToEnd|TestCategory=UserJourney" }
            "Performance" { "TestCategory=Performance" }
            "Security" { "TestCategory=Security" }
            "Accessibility" { "TestCategory=Accessibility|TestCategory=WCAG" }
            default { "" }
        }

        # Execute comprehensive test script
        $testArgs = @(
            "-TestCategory", $Category
            "-OutputPath", "$OutputDirectory/TestResults"
            "-GenerateReport"
        )

        if ($Verbose) { $testArgs += "-Verbose" }

        Write-TestSuiteMessage "Executing: .\Scripts\run-comprehensive-tests.ps1 $($testArgs -join ' ')" "Info"

        # Run the comprehensive test script
        $testOutput = & .\Scripts\run-comprehensive-tests.ps1 @testArgs 2>&1
        $testExitCode = $LASTEXITCODE

        # Parse results
        $categoryDuration = (Get-Date) - $categoryStart

        # Extract test metrics from output
        $passed = 0
        $failed = 0
        $skipped = 0

        if ($testOutput -match "Passed:\s*(\d+).*Failed:\s*(\d+).*Skipped:\s*(\d+)") {
            $passed = [int]$matches[1]
            $failed = [int]$matches[2]
            $skipped = [int]$matches[3]
        }

        # Store results
        $TestExecution.Results[$Category] = @{
            Passed = $passed
            Failed = $failed
            Skipped = $skipped
            Duration = $categoryDuration.TotalSeconds
            ExitCode = $testExitCode
            Output = $testOutput
        }

        # Update summary
        $TestExecution.Summary.TotalTests += ($passed + $failed + $skipped)
        $TestExecution.Summary.PassedTests += $passed
        $TestExecution.Summary.FailedTests += $failed
        $TestExecution.Summary.SkippedTests += $skipped
        $TestExecution.Summary.TotalDuration += $categoryDuration.TotalSeconds

        if ($testExitCode -eq 0) {
            Write-TestSuiteMessage "$Category tests completed successfully - P:$passed F:$failed S:$skipped" "Success"
        } else {
            Write-TestSuiteMessage "$Category tests failed - P:$passed F:$failed S:$skipped" "Error"
            $TestExecution.Errors += "$Category tests failed with exit code $testExitCode"
        }

    }
    catch {
        $categoryDuration = (Get-Date) - $categoryStart
        Write-TestSuiteMessage "$Category tests failed with exception: $($_.Exception.Message)" "Error"

        $TestExecution.Results[$Category] = @{
            Passed = 0
            Failed = 1
            Skipped = 0
            Duration = $categoryDuration.TotalSeconds
            ExitCode = -1
            Error = $_.Exception.Message
        }

        $TestExecution.Errors += "$Category tests failed: $($_.Exception.Message)"
    }
}

# Execute test categories
foreach ($category in $testStrategy.Categories) {
    Invoke-TestCategory -Category $category -TestExecution $testExecution

    # Check timeout
    $elapsed = (Get-Date) - $testExecution.StartTime
    if ($elapsed.TotalSeconds -gt $testStrategy.Timeout) {
        Write-TestSuiteMessage "Test suite timeout reached ($($testStrategy.Timeout) seconds)" "Warning"
        break
    }
}

# Run security scan if requested
if ($testStrategy.RunSecurity -or $RunSecurityScan) {
    Write-TestSuiteHeader "Security Scanning"

    try {
        $securityArgs = @(
            "-ScanType", "All"
            "-OutputPath", "$OutputDirectory/SecurityResults"
            "-ContinueOnFindings"
        )

        if ($Verbose) { $securityArgs += "-Verbose" }

        & .\Scripts\run-security-scan.ps1 @securityArgs
        $securityExitCode = $LASTEXITCODE

        if ($securityExitCode -eq 0) {
            Write-TestSuiteMessage "Security scan completed successfully" "Success"
        } else {
            Write-TestSuiteMessage "Security scan completed with findings (exit code: $securityExitCode)" "Warning"
        }
    }
    catch {
        Write-TestSuiteMessage "Security scan failed: $($_.Exception.Message)" "Error"
        $testExecution.Errors += "Security scan failed: $($_.Exception.Message)"
    }
}

# Generate comprehensive reports
if ($testStrategy.GenerateFullReports -or $GenerateReports) {
    Write-TestSuiteHeader "Generating Comprehensive Reports"

    try {
        # Generate consolidated coverage report
        Write-TestSuiteMessage "Generating consolidated coverage report..." "Info"

        $coverageFiles = Get-ChildItem -Path "$OutputDirectory/TestResults" -Filter "coverage.cobertura.xml" -Recurse
        if ($coverageFiles.Count -gt 0) {
            # Install ReportGenerator if needed
            if (!(Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
                dotnet tool install --global dotnet-reportgenerator-globaltool
            }

            $coverageFileList = ($coverageFiles | ForEach-Object { $_.FullName }) -join ";"
            reportgenerator "-reports:$coverageFileList" "-targetdir:$OutputDirectory/CoverageReports" "-reporttypes:Html;Badges;Cobertura;JsonSummary"

            Write-TestSuiteMessage "Coverage report generated successfully" "Success"
        }

        # Generate test summary dashboard
        Write-TestSuiteMessage "Generating test summary dashboard..." "Info"

        $dashboardHtml = New-TestDashboard -TestExecution $testExecution -Strategy $testStrategy
        $dashboardHtml | Out-File -FilePath "$OutputDirectory/test-dashboard.html" -Encoding utf8

        Write-TestSuiteMessage "Test dashboard generated successfully" "Success"

    }
    catch {
        Write-TestSuiteMessage "Report generation failed: $($_.Exception.Message)" "Error"
    }
}

# Generate final summary
Write-TestSuiteHeader "Test Suite Summary"

$totalDuration = (Get-Date) - $testExecution.StartTime
$testExecution.Summary.TotalDuration = $totalDuration.TotalSeconds

# Display results table
Write-Host "`nTest Results Summary:" -ForegroundColor $Colors.Info
Write-Host $("-" * 80) -ForegroundColor Gray
Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12} {5,8}" -f "Category", "Passed", "Failed", "Skipped", "Duration(s)", "Status") -ForegroundColor $Colors.Header

foreach ($category in $testExecution.Results.Keys) {
    $result = $testExecution.Results[$category]
    $status = if ($result.Failed -eq 0 -and $result.ExitCode -eq 0) { "PASS" } else { "FAIL" }
    $statusColor = if ($status -eq "PASS") { $Colors.Success } else { $Colors.Error }

    Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12:F2} {5,8}" -f
        $category,
        $result.Passed,
        $result.Failed,
        $result.Skipped,
        $result.Duration,
        $status
    ) -ForegroundColor $statusColor
}

Write-Host $("-" * 80) -ForegroundColor Gray
Write-Host ("{0,-15} {1,8} {2,8} {3,8} {4,12:F2} {5,8}" -f
    "TOTAL",
    $testExecution.Summary.PassedTests,
    $testExecution.Summary.FailedTests,
    $testExecution.Summary.SkippedTests,
    $testExecution.Summary.TotalDuration,
    $(if ($testExecution.Summary.FailedTests -eq 0) { "PASS" } else { "FAIL" })
) -ForegroundColor $(if ($testExecution.Summary.FailedTests -eq 0) { $Colors.Success } else { $Colors.Error })

# Calculate and display metrics
$totalTests = $testExecution.Summary.TotalTests
if ($totalTests -gt 0) {
    $successRate = ($testExecution.Summary.PassedTests / $totalTests) * 100
    Write-Host "`nSuccess Rate: $($successRate.ToString('F2'))%" -ForegroundColor $(
        if ($successRate -ge 95) { $Colors.Success }
        elseif ($successRate -ge 85) { $Colors.Warning }
        else { $Colors.Error }
    )
}

Write-Host "Total Execution Time: $($totalDuration.ToString('hh\:mm\:ss'))" -ForegroundColor $Colors.Info

# Display any errors
if ($testExecution.Errors.Count -gt 0) {
    Write-Host "`nErrors Encountered:" -ForegroundColor $Colors.Error
    foreach ($error in $testExecution.Errors) {
        Write-Host "  • $error" -ForegroundColor $Colors.Error
    }
}

# Recommendations based on results
Write-Host "`nRecommendations:" -ForegroundColor $Colors.Info
if ($testExecution.Summary.FailedTests -eq 0) {
    Write-Host "  ✅ All tests passed! Your code is ready for deployment." -ForegroundColor $Colors.Success
} else {
    Write-Host "  ❌ $($testExecution.Summary.FailedTests) test(s) failed. Review failures before deployment." -ForegroundColor $Colors.Error
}

if ($testStrategy.RunSecurity) {
    Write-Host "  🔒 Security scan completed. Review security report for any findings." -ForegroundColor $Colors.Info
}

Write-Host "  📊 Detailed reports available in: $OutputDirectory" -ForegroundColor $Colors.Info
Write-Host "  🌐 Open test-dashboard.html for interactive results" -ForegroundColor $Colors.Info

# Generate final JSON report for CI/CD
$finalReport = @{
    timestamp = Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ"
    strategy = $testStrategy.Name
    duration = $totalDuration.TotalSeconds
    summary = $testExecution.Summary
    results = $testExecution.Results
    errors = $testExecution.Errors
    recommendations = @(
        if ($testExecution.Summary.FailedTests -eq 0) { "All tests passed - ready for deployment" } else { "Fix failing tests before deployment" },
        "Review coverage reports for any gaps",
        "Check security scan results for vulnerabilities",
        "Ensure performance benchmarks are met"
    )
} | ConvertTo-Json -Depth 4

$finalReport | Out-File -FilePath "$OutputDirectory/final-test-report.json" -Encoding utf8

# Function to generate test dashboard HTML
function New-TestDashboard {
    param($TestExecution, $Strategy)

    $html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BusBuddy Test Results Dashboard</title>
    <style>
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }
        .header { background: #2196F3; color: white; padding: 20px; border-radius: 8px; margin-bottom: 20px; }
        .summary { display: flex; gap: 20px; margin-bottom: 20px; }
        .metric { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); flex: 1; text-align: center; }
        .metric h3 { margin: 0; color: #666; }
        .metric .value { font-size: 2em; margin: 10px 0; font-weight: bold; }
        .pass { color: #4CAF50; }
        .fail { color: #F44336; }
        .warn { color: #FF9800; }
        .results-table { background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .results-table table { width: 100%; border-collapse: collapse; }
        .results-table th, .results-table td { padding: 12px; text-align: left; border-bottom: 1px solid #eee; }
        .results-table th { background: #f8f9fa; font-weight: 600; }
        .status-pass { color: #4CAF50; font-weight: bold; }
        .status-fail { color: #F44336; font-weight: bold; }
    </style>
</head>
<body>
    <div class="header">
        <h1>🧪 BusBuddy Test Results Dashboard</h1>
        <p>Test Strategy: $($Strategy.Name) | Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</p>
    </div>

    <div class="summary">
        <div class="metric">
            <h3>Total Tests</h3>
            <div class="value">$($TestExecution.Summary.TotalTests)</div>
        </div>
        <div class="metric">
            <h3>Passed</h3>
            <div class="value pass">$($TestExecution.Summary.PassedTests)</div>
        </div>
        <div class="metric">
            <h3>Failed</h3>
            <div class="value $(if ($TestExecution.Summary.FailedTests -eq 0) { 'pass' } else { 'fail' })">$($TestExecution.Summary.FailedTests)</div>
        </div>
        <div class="metric">
            <h3>Duration</h3>
            <div class="value">$([math]::Round($TestExecution.Summary.TotalDuration, 1))s</div>
        </div>
    </div>

    <div class="results-table">
        <table>
            <thead>
                <tr>
                    <th>Category</th>
                    <th>Passed</th>
                    <th>Failed</th>
                    <th>Skipped</th>
                    <th>Duration</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
"@

    foreach ($category in $TestExecution.Results.Keys) {
        $result = $TestExecution.Results[$category]
        $status = if ($result.Failed -eq 0 -and $result.ExitCode -eq 0) { "PASS" } else { "FAIL" }
        $statusClass = if ($status -eq "PASS") { "status-pass" } else { "status-fail" }

        $html += @"
                <tr>
                    <td>$category</td>
                    <td>$($result.Passed)</td>
                    <td>$($result.Failed)</td>
                    <td>$($result.Skipped)</td>
                    <td>$([math]::Round($result.Duration, 1))s</td>
                    <td class="$statusClass">$status</td>
                </tr>
"@
    }

    $html += @"
            </tbody>
        </table>
    </div>
</body>
</html>
"@

    return $html
}

# Exit with appropriate code
$exitCode = if ($testExecution.Summary.FailedTests -eq 0) { 0 } else { 1 }
Write-TestSuiteMessage "Test suite completed with exit code: $exitCode" $(if ($exitCode -eq 0) { "Success" } else { "Error" })

exit $exitCode
