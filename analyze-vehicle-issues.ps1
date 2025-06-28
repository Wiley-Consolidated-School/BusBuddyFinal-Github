#!/usr/bin/env pwsh
# Vehicle Issue Analysis Script for BusBuddy Test Files
# Scans for vehicle-related issues and builds a comprehensive report

param(
    [string]$ProjectPath = ".",
    [switch]$Detailed
)

Write-Host "🔍 Analyzing Vehicle-related issues in BusBuddy test files..." -ForegroundColor Cyan

# Define the test directory
$testDir = Join-Path $ProjectPath "BusBuddy.UI.Tests"

if (-not (Test-Path $testDir)) {
    Write-Host "❌ Test directory not found: $testDir" -ForegroundColor Red
    exit 1
}

# Initialize report arrays
$vehicleIssues = @()
$apiIssues = @()
$typeIssues = @()
$propertyIssues = @()

# Define patterns to search for
$vehiclePatterns = @{
    "VehicleRepository" = "Should be BusRepository"
    "Vehicle\s+\w+" = "Variable declaration - should be Bus"
    "new Vehicle\(" = "Constructor - should be new Bus("
    "CreateTestVehicle" = "Method name - should be CreateTestBus"
    "testVehicle" = "Variable name - should be testBus"
    "_testVehicleIds" = "Collection name - should be _testBusIds"
    "vehicleId" = "Variable name - should be busId"
    "VINNumber" = "Property name - should be VIN"
    "\.AddVehicle\(" = "API method - should be AddBus("
    "\.GetVehicleById\(" = "API method - should be GetBusById("
    "\.GetAllVehicles\(" = "API method - should be GetAllBuses("
    "\.UpdateVehicle\(" = "API method - should be UpdateBus("
    "\.DeleteVehicle\(" = "API method - should be DeleteBus("
    "ValidateVehicleAvailability" = "API method - should be ValidateBusAvailability"
    "IsValidVehicleNumber" = "API method - should be IsValidBusNumber"
}

# Get all C# test files
$testFiles = Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse

Write-Host "📁 Found $($testFiles.Count) test files to analyze" -ForegroundColor Green

foreach ($file in $testFiles) {
    Write-Host "  Analyzing: $($file.Name)" -ForegroundColor Gray

    $content = Get-Content -Path $file.FullName -Raw
    $lines = Get-Content -Path $file.FullName

    # Check each pattern
    foreach ($pattern in $vehiclePatterns.Keys) {
        $matches = [regex]::Matches($content, $pattern)

        foreach ($match in $matches) {
            # Find the line number
            $lineNumber = ($content.Substring(0, $match.Index) -split "`n").Count
            $lineContent = $lines[$lineNumber - 1].Trim()

            # Get surrounding context
            $startLine = [Math]::Max(0, $lineNumber - 3)
            $endLine = [Math]::Min($lines.Count - 1, $lineNumber + 1)
            $context = $lines[$startLine..$endLine] -join "`n"

            # Determine issue category
            $category = "Unknown"
            $severity = "Medium"
            $suggestedFix = $vehiclePatterns[$pattern]

            if ($pattern -match "Repository|API|Method") {
                $category = "API Issue"
                $severity = "High"
            }
            elseif ($pattern -match "Variable|Constructor|new") {
                $category = "Type Issue"
                $severity = "High"
            }
            elseif ($pattern -match "Property|VINNumber") {
                $category = "Property Issue"
                $severity = "Medium"
            }
            else {
                $category = "Naming Issue"
                $severity = "Low"
            }

            $issue = [PSCustomObject]@{
                File = $file.Name
                FullPath = $file.FullName
                LineNumber = $lineNumber
                Column = $match.Index
                Category = $category
                Severity = $severity
                Pattern = $pattern
                MatchedText = $match.Value
                LineContent = $lineContent
                SuggestedFix = $suggestedFix
                Context = $context
            }

            $vehicleIssues += $issue
        }
    }
}

# Generate summary statistics
$totalIssues = $vehicleIssues.Count
$highSeverity = ($vehicleIssues | Where-Object { $_.Severity -eq "High" }).Count
$mediumSeverity = ($vehicleIssues | Where-Object { $_.Severity -eq "Medium" }).Count
$lowSeverity = ($vehicleIssues | Where-Object { $_.Severity -eq "Low" }).Count

$fileStats = $vehicleIssues | Group-Object File | Sort-Object Count -Descending
$categoryStats = $vehicleIssues | Group-Object Category | Sort-Object Count -Descending

# Display Summary Report
Write-Host "`n" + "="*80 -ForegroundColor Yellow
Write-Host "🚌 BUSBUDDY VEHICLE ISSUE ANALYSIS REPORT" -ForegroundColor Yellow
Write-Host "="*80 -ForegroundColor Yellow

Write-Host "`n📊 SUMMARY STATISTICS:" -ForegroundColor Cyan
Write-Host "  Total Issues Found: $totalIssues" -ForegroundColor White
Write-Host "  High Severity: $highSeverity" -ForegroundColor Red
Write-Host "  Medium Severity: $mediumSeverity" -ForegroundColor Yellow
Write-Host "  Low Severity: $lowSeverity" -ForegroundColor Green

Write-Host "`n📁 ISSUES BY FILE:" -ForegroundColor Cyan
foreach ($stat in $fileStats) {
    $color = if ($stat.Count -gt 10) { "Red" } elseif ($stat.Count -gt 5) { "Yellow" } else { "Green" }
    Write-Host "  $($stat.Name): $($stat.Count) issues" -ForegroundColor $color
}

Write-Host "`n🏷️ ISSUES BY CATEGORY:" -ForegroundColor Cyan
foreach ($stat in $categoryStats) {
    $color = switch ($stat.Name) {
        "API Issue" { "Red" }
        "Type Issue" { "Red" }
        "Property Issue" { "Yellow" }
        "Naming Issue" { "Green" }
        default { "White" }
    }
    Write-Host "  $($stat.Name): $($stat.Count) issues" -ForegroundColor $color
}

# Display Top Priority Issues
Write-Host "`n🚨 TOP PRIORITY ISSUES (High Severity):" -ForegroundColor Red
$highPriorityIssues = $vehicleIssues | Where-Object { $_.Severity -eq "High" } | Sort-Object File, LineNumber

if ($highPriorityIssues.Count -eq 0) {
    Write-Host "  ✅ No high severity issues found!" -ForegroundColor Green
} else {
    foreach ($issue in $highPriorityIssues | Select-Object -First 10) {
        Write-Host "`n  📍 $($issue.File):$($issue.LineNumber)" -ForegroundColor Red
        Write-Host "     Category: $($issue.Category)" -ForegroundColor Gray
        Write-Host "     Found: '$($issue.MatchedText)'" -ForegroundColor Yellow
        Write-Host "     Fix: $($issue.SuggestedFix)" -ForegroundColor Green
        Write-Host "     Line: $($issue.LineContent)" -ForegroundColor Gray
    }

    if ($highPriorityIssues.Count -gt 10) {
        Write-Host "`n  ... and $($highPriorityIssues.Count - 10) more high priority issues" -ForegroundColor Red
    }
}

# Generate targeted fix recommendations
Write-Host "`n🔧 RECOMMENDED FIXES BY PRIORITY:" -ForegroundColor Green

Write-Host "`n1️⃣ CRITICAL API FIXES:" -ForegroundColor Red
$apiIssues = $vehicleIssues | Where-Object { $_.Category -eq "API Issue" } | Group-Object MatchedText
foreach ($apiGroup in $apiIssues) {
    $fix = switch ($apiGroup.Name) {
        "AddVehicle" { "AddBus" }
        "GetVehicleById" { "GetBusById" }
        "GetAllVehicles" { "GetAllBuses" }
        "UpdateVehicle" { "UpdateBus" }
        "DeleteVehicle" { "DeleteBus" }
        "ValidateVehicleAvailability" { "ValidateBusAvailability" }
        "IsValidVehicleNumber" { "IsValidBusNumber" }
        "VehicleRepository" { "BusRepository" }
        default { "Unknown fix needed" }
    }
    Write-Host "  Replace '$($apiGroup.Name)' with '$fix' ($($apiGroup.Count) occurrences)" -ForegroundColor Yellow
}

Write-Host "`n2️⃣ TYPE DECLARATION FIXES:" -ForegroundColor Red
$typeIssues = $vehicleIssues | Where-Object { $_.Category -eq "Type Issue" } | Group-Object MatchedText
foreach ($typeGroup in $typeIssues) {
    Write-Host "  Replace '$($typeGroup.Name)' with Bus equivalent ($($typeGroup.Count) occurrences)" -ForegroundColor Yellow
}

Write-Host "`n3️⃣ PROPERTY NAME FIXES:" -ForegroundColor Yellow
$propIssues = $vehicleIssues | Where-Object { $_.Category -eq "Property Issue" } | Group-Object MatchedText
foreach ($propGroup in $propIssues) {
    $fix = if ($propGroup.Name -eq "VINNumber") { "VIN" } else { "Unknown" }
    Write-Host "  Replace '$($propGroup.Name)' with '$fix' ($($propGroup.Count) occurrences)" -ForegroundColor Yellow
}

# Export detailed report if requested
if ($Detailed) {
    $reportPath = Join-Path $ProjectPath "vehicle-issues-detailed-report.json"
    $vehicleIssues | ConvertTo-Json -Depth 3 | Out-File -FilePath $reportPath -Encoding UTF8
    Write-Host "`n💾 Detailed report exported to: $reportPath" -ForegroundColor Green
}

# Generate PowerShell fix script
$fixScriptPath = Join-Path $ProjectPath "fix-vehicle-issues.ps1"
$fixScript = @"
#!/usr/bin/env pwsh
# Auto-generated Vehicle Issue Fix Script
# Generated on $(Get-Date)

Write-Host "🔧 Applying targeted fixes for vehicle issues..." -ForegroundColor Cyan

`$testDir = "BusBuddy.UI.Tests"
`$filesFixed = 0
`$issuesFixed = 0

# API Method Replacements (Critical Priority)
`$apiReplacements = @{
"@

foreach ($apiGroup in ($vehicleIssues | Where-Object { $_.Category -eq "API Issue" } | Group-Object MatchedText)) {
    $fix = switch ($apiGroup.Name) {
        "AddVehicle" { "AddBus" }
        "GetVehicleById" { "GetBusById" }
        "GetAllVehicles" { "GetAllBuses" }
        "UpdateVehicle" { "UpdateBus" }
        "DeleteVehicle" { "DeleteBus" }
        "ValidateVehicleAvailability" { "ValidateBusAvailability" }
        "IsValidVehicleNumber" { "IsValidBusNumber" }
        "VehicleRepository" { "BusRepository" }
        default { $null }
    }
    if ($fix) {
        $fixScript += "`n    '$($apiGroup.Name)' = '$fix'"
    }
}

$fixScript += @"

}

# Apply fixes to each file
Get-ChildItem -Path `$testDir -Filter "*.cs" -Recurse | ForEach-Object {
    `$file = `$_
    `$content = Get-Content -Path `$file.FullName -Raw
    `$originalContent = `$content
    `$fileChanged = `$false

    foreach (`$find in `$apiReplacements.Keys) {
        `$replace = `$apiReplacements[`$find]
        if (`$content -match `$find) {
            `$content = `$content -replace `$find, `$replace
            `$fileChanged = `$true
            `$issuesFixed++
            Write-Host "  Fixed: `$find -> `$replace in `$(`$file.Name)" -ForegroundColor Green
        }
    }

    if (`$fileChanged) {
        Set-Content -Path `$file.FullName -Value `$content -Encoding UTF8
        `$filesFixed++
        Write-Host "✅ Updated: `$(`$file.Name)" -ForegroundColor Cyan
    }
}

Write-Host "`n🎉 Fix Complete!" -ForegroundColor Green
Write-Host "  Files Updated: `$filesFixed" -ForegroundColor White
Write-Host "  Issues Fixed: `$issuesFixed" -ForegroundColor White
Write-Host "`n💡 Run 'dotnet build BusBuddy.sln' to verify fixes" -ForegroundColor Yellow
"@

Set-Content -Path $fixScriptPath -Value $fixScript -Encoding UTF8
Write-Host "`n🛠️ Auto-fix script generated: $fixScriptPath" -ForegroundColor Green

Write-Host "`n" + "="*80 -ForegroundColor Yellow
Write-Host "📋 NEXT STEPS:" -ForegroundColor Yellow
Write-Host "="*80 -ForegroundColor Yellow
Write-Host "1. Review the issues above" -ForegroundColor White
Write-Host "2. Run the auto-fix script: pwsh $fixScriptPath" -ForegroundColor White
Write-Host "3. Build to verify: dotnet build BusBuddy.sln" -ForegroundColor White
Write-Host "4. Address any remaining issues manually" -ForegroundColor White
Write-Host "`n🚀 Total issues to fix: $totalIssues" -ForegroundColor Cyan
