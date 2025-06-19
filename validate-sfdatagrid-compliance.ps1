# 🔍 Enhanced SfDataGrid Compliance Validator for BusBuddy
# Validates all forms are using SfDataGrid and checks for Grid Control methods/features

param(
    [string]$Path = ".",
    [switch]$Detailed = $false,
    [switch]$FixMode = $false,
    [switch]$CheckFeatures
)

Write-Host "🔍 Enhanced BusBuddy SfDataGrid Compliance Validator" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Default CheckFeatures to true if not specified
if (-not $PSBoundParameters.ContainsKey('CheckFeatures')) {
    $CheckFeatures = $true
}

# Define Syncfusion DataGrid/Grid Control features and methods to check for
$syncfusionFeatures = @{
    "DataBinding" = @("DataSource", "AutoGenerateColumns", "Columns.Add")
    "Sorting" = @("SortColumnDescriptions", "AllowSorting", "SortBy", "Sort")
    "Filtering" = @("AllowFiltering", "FilterRowPosition", "FilterPredicates", "Filter")
    "Grouping" = @("AllowGrouping", "GroupColumnDescriptions", "GroupBy", "ShowGroupDropArea")
    "Selection" = @("SelectionMode", "SelectedItems", "CurrentCell", "SelectionChanged")
    "Editing" = @("AllowEditing", "EditMode", "CurrentCellBeginEdit", "CurrentCellEndEdit")
    "Export" = @("ExportToExcel", "ExportToPdf", "ExportToDataTable")
    "Virtualization" = @("EnableDataVirtualization", "VirtualizingCellsProvider")
    "Summaries" = @("SummaryRows", "TableSummaryRows", "GroupSummaryRows", "SummaryCalculation")
    "Styling" = @("Style", "CellStyle", "HeaderStyle", "AlternatingRowStyle")
    "Paging" = @("DataPager", "PageSize", "PageIndex")
    "ContextMenu" = @("ContextMenuOpening", "RecordContextMenu", "HeaderContextMenu")
    "ColumnTypes" = @("GridTextColumn", "GridNumericColumn", "GridDateTimeColumn", "GridComboBoxColumn", "GridCheckBoxColumn")
    "Events" = @("CellClick", "CellDoubleClick", "QueryCellInfo", "CurrentCellActivated")
    "RowOperations" = @("AddNewRow", "DeleteRow", "RowHeight", "AutoSizeRowsMode")
    "ColumnOperations" = @("ColumnWidth", "AutoSizeColumnsMode", "ColumnChooser", "FrozenRowsCount", "FrozenColumnsCount")
    "Validation" = @("CellValidating", "RowValidating", "DataValidation")
    "Search" = @("SearchHelper", "FindText", "HighlightText")
}

$complianceResults = @()
$nonCompliantFiles = @()
$compliantFiles = @()

# Get all relevant C# files
$csharpFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.cs" |
    Where-Object {
        $_.FullName -notlike "*\bin\*" -and
        $_.FullName -notlike "*\obj\*" -and
        $_.FullName -notlike "*Base*.cs" -and
        $_.FullName -notlike "*Helper*.cs" -and
        ($_.Name -like "*Form*.cs" -or $_.Name -like "*View*.cs")
    }

Write-Host "📊 Scanning $($csharpFiles.Count) form/view files..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $csharpFiles) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if (-not $content) { continue }

    $relativePath = $file.FullName.Replace((Get-Location).Path, "").TrimStart('\')

    # Check for DataGridView usage (non-compliant)
    $hasDataGridView = $content -match "DataGridView"
    $dataGridViewCount = ([regex]::Matches($content, "DataGridView")).Count

    # Check for SfDataGrid usage (compliant)
    $hasSfDataGrid = $content -match "SfDataGrid"
    $sfDataGridCount = ([regex]::Matches($content, "SfDataGrid")).Count

    # Check if this is a Syncfusion form
    $isSyncfusionForm = $content -match "SyncfusionBaseForm" -or $file.Name -like "*Syncfusion*.cs"

    # Feature compliance analysis
    $featuresFound = @{}
    $featureScore = 0
    $maxFeatureScore = 0

    if ($CheckFeatures -and ($hasSfDataGrid -or $hasDataGridView)) {
        foreach ($featureCategory in $syncfusionFeatures.Keys) {
            $featuresInCategory = $syncfusionFeatures[$featureCategory]
            $categoryFound = @()

            foreach ($feature in $featuresInCategory) {
                $maxFeatureScore++
                if ($content -match [regex]::Escape($feature)) {
                    $categoryFound += $feature
                    $featureScore++
                }
            }

            if ($categoryFound.Count -gt 0) {
                $featuresFound[$featureCategory] = $categoryFound
            }
        }
    }

    # Calculate feature compliance percentage
    $featureComplianceRate = if ($maxFeatureScore -gt 0) { [math]::Round(($featureScore / $maxFeatureScore) * 100, 1) } else { 0 }

    # Determine compliance status
    $isCompliant = $false
    $status = ""
    $priority = ""
    $recommendation = ""

    if ($hasDataGridView -and -not $hasSfDataGrid) {
        $status = "❌ NON-COMPLIANT"
        $priority = if ($file.Name -like "*Management*.cs") { "HIGH" } else { "MEDIUM" }
        $recommendation = "Migrate to SfDataGrid immediately"
        $nonCompliantFiles += $file
    }
    elseif ($hasSfDataGrid -and -not $hasDataGridView) {
        $status = "✅ COMPLIANT"
        $isCompliant = $true
        $compliantFiles += $file

        # Additional compliance checks for SfDataGrid forms
        if ($featureComplianceRate -lt 30) {
            $recommendation = "Enhance with more Syncfusion features"
        } elseif ($featureComplianceRate -lt 60) {
            $recommendation = "Good feature usage - consider advanced features"
        } else {
            $recommendation = "Excellent feature implementation"
        }
    }
    elseif ($hasSfDataGrid -and $hasDataGridView) {
        $status = "⚠️ MIXED"
        $priority = "HIGH"
        $recommendation = "Remove DataGridView, keep only SfDataGrid"
        $nonCompliantFiles += $file
    }
    elseif (-not $hasDataGridView -and -not $hasSfDataGrid) {
        $status = "ℹ️ NO GRID"
        $isCompliant = $true
        $recommendation = "No grid controls detected"
    }
    else {
        $status = "❓ UNKNOWN"
        $recommendation = "Manual review required"
    }

    # Create result object
    $result = [PSCustomObject]@{
        File = $relativePath
        Status = $status
        IsCompliant = $isCompliant
        IsSyncfusionForm = $isSyncfusionForm
        DataGridViewCount = $dataGridViewCount
        SfDataGridCount = $sfDataGridCount
        Priority = $priority
        FeaturesFound = $featuresFound
        FeatureScore = $featureScore
        MaxFeatureScore = $maxFeatureScore
        FeatureComplianceRate = $featureComplianceRate
        Recommendation = $recommendation
    }

    $complianceResults += $result

    # Display result
    $color = if ($isCompliant) { "Green" } else { "Red" }
    if ($status -eq "⚠️ MIXED") { $color = "Yellow" }
    if ($status -eq "ℹ️ NO GRID") { $color = "Gray" }

    Write-Host "  $status " -ForegroundColor $color -NoNewline
    Write-Host "$($file.Name)" -NoNewline

    if ($Detailed) {
        Write-Host " (DGV: $dataGridViewCount, SfDG: $sfDataGridCount, Features: $featureComplianceRate%)" -ForegroundColor Gray

        if ($CheckFeatures -and $featuresFound.Count -gt 0) {
            foreach ($category in $featuresFound.Keys) {
                Write-Host "    📋 $category`: " -ForegroundColor Cyan -NoNewline
                Write-Host "$($featuresFound[$category] -join ', ')" -ForegroundColor Gray
            }
        }
    } else {
        if ($CheckFeatures -and ($hasSfDataGrid -or $hasDataGridView)) {
            Write-Host " (Features: $featureComplianceRate%)" -ForegroundColor Gray
        } else {
            Write-Host ""
        }
    }
}

Write-Host ""
Write-Host "📈 COMPLIANCE SUMMARY" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

$totalForms = $complianceResults.Count
$compliantCount = ($complianceResults | Where-Object { $_.IsCompliant }).Count
$nonCompliantCount = $totalForms - $compliantCount
$complianceRate = if ($totalForms -gt 0) { [math]::Round(($compliantCount / $totalForms) * 100, 1) } else { 0 }

# Calculate average feature compliance for compliant forms
$compliantFormsWithGrids = $complianceResults | Where-Object { $_.IsCompliant -and $_.SfDataGridCount -gt 0 }
$averageFeatureCompliance = if ($compliantFormsWithGrids.Count -gt 0) {
    [math]::Round(($compliantFormsWithGrids | Measure-Object -Property FeatureComplianceRate -Average).Average, 1)
} else { 0 }

Write-Host "Total Forms Scanned: $totalForms" -ForegroundColor White
Write-Host "Compliant Forms: $compliantCount" -ForegroundColor Green
Write-Host "Non-Compliant Forms: $nonCompliantCount" -ForegroundColor Red
Write-Host "Basic Compliance Rate: $complianceRate%" -ForegroundColor $(if ($complianceRate -ge 80) { "Green" } else { "Red" })

if ($CheckFeatures) {
    Write-Host "Average Feature Usage: $averageFeatureCompliance%" -ForegroundColor $(if ($averageFeatureCompliance -ge 60) { "Green" } elseif ($averageFeatureCompliance -ge 30) { "Yellow" } else { "Red" })
}

Write-Host ""
Write-Host "🎯 NON-COMPLIANT FORMS (Require Action)" -ForegroundColor Red
Write-Host "=======================================" -ForegroundColor Red

$nonCompliantResults = $complianceResults | Where-Object { -not $_.IsCompliant -and $_.Status -ne "ℹ️ NO GRID" } | Sort-Object Priority, File

if ($nonCompliantResults.Count -eq 0) {
    Write-Host "🎉 ALL FORMS ARE COMPLIANT!" -ForegroundColor Green
} else {
    foreach ($result in $nonCompliantResults) {
        $priorityColor = if ($result.Priority -eq "HIGH") { "Red" } else { "Yellow" }
        Write-Host "  $($result.Status) " -ForegroundColor $priorityColor -NoNewline
        Write-Host "$($result.File) " -NoNewline
        Write-Host "[$($result.Priority)]" -ForegroundColor $priorityColor -NoNewline
        Write-Host " (DGV: $($result.DataGridViewCount), SfDG: $($result.SfDataGridCount))" -ForegroundColor Gray
        Write-Host "    💡 $($result.Recommendation)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "✅ COMPLIANT FORMS" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green

$compliantResults = $complianceResults | Where-Object { $_.IsCompliant -and $_.SfDataGridCount -gt 0 } | Sort-Object FeatureComplianceRate -Descending

if ($compliantResults.Count -eq 0) {
    Write-Host "  No compliant forms found" -ForegroundColor Gray
} else {
    foreach ($result in $compliantResults) {
        $featureColor = if ($result.FeatureComplianceRate -ge 60) { "Green" } elseif ($result.FeatureComplianceRate -ge 30) { "Yellow" } else { "Red" }
        Write-Host "  ✅ $($result.File) " -NoNewline -ForegroundColor Green
        Write-Host "(SfDG: $($result.SfDataGridCount), Features: $($result.FeatureComplianceRate)%)" -ForegroundColor $featureColor

        if ($Detailed -and $result.FeaturesFound.Count -gt 0) {
            foreach ($category in $result.FeaturesFound.Keys) {
                Write-Host "      📋 $category`: $($result.FeaturesFound[$category] -join ', ')" -ForegroundColor Gray
            }
        }

        Write-Host "      💡 $($result.Recommendation)" -ForegroundColor Cyan
    }
}

if ($CheckFeatures) {
    Write-Host ""
    Write-Host "🔧 FEATURE ENHANCEMENT RECOMMENDATIONS" -ForegroundColor Cyan
    Write-Host "======================================" -ForegroundColor Cyan

    # Analyze missing features across all compliant forms
    $allFoundFeatures = @{}
    $formsWithGrids = $complianceResults | Where-Object { $_.SfDataGridCount -gt 0 }

    foreach ($result in $formsWithGrids) {
        foreach ($category in $result.FeaturesFound.Keys) {
            if (-not $allFoundFeatures.ContainsKey($category)) {
                $allFoundFeatures[$category] = 0
            }
            $allFoundFeatures[$category]++
        }
    }

    Write-Host "Feature Implementation Statistics:" -ForegroundColor Yellow
    foreach ($featureCategory in $syncfusionFeatures.Keys) {
        $implementationCount = if ($allFoundFeatures.ContainsKey($featureCategory)) { $allFoundFeatures[$featureCategory] } else { 0 }
        $implementationRate = if ($formsWithGrids.Count -gt 0) { [math]::Round(($implementationCount / $formsWithGrids.Count) * 100, 1) } else { 0 }

        $rateColor = if ($implementationRate -ge 70) { "Green" } elseif ($implementationRate -ge 40) { "Yellow" } else { "Red" }
        Write-Host "  📊 $featureCategory`: $implementationRate% ($implementationCount/$($formsWithGrids.Count) forms)" -ForegroundColor $rateColor
    }

    Write-Host ""
    Write-Host "🎯 Priority Feature Recommendations:" -ForegroundColor Yellow

    # Recommend low-usage features
    $lowUsageFeatures = $syncfusionFeatures.Keys | Where-Object {
        $implementationCount = if ($allFoundFeatures.ContainsKey($_)) { $allFoundFeatures[$_] } else { 0 }
        $implementationRate = if ($formsWithGrids.Count -gt 0) { ($implementationCount / $formsWithGrids.Count) * 100 } else { 0 }
        $implementationRate -lt 50
    }

    foreach ($feature in $lowUsageFeatures) {
        Write-Host "  🔸 Consider implementing: $feature" -ForegroundColor Cyan
        Write-Host "     Methods/Properties: $($syncfusionFeatures[$feature] -join ', ')" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🔧 RECOMMENDATIONS" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

if ($nonCompliantCount -gt 0) {
    Write-Host "1. Priority Actions:" -ForegroundColor Yellow

    $highPriorityForms = $nonCompliantResults | Where-Object { $_.Priority -eq "HIGH" }
    if ($highPriorityForms.Count -gt 0) {
        Write-Host "   🔥 HIGH PRIORITY: Migrate these management forms immediately" -ForegroundColor Red
        foreach ($form in $highPriorityForms) {
            Write-Host "      - $($form.File)" -ForegroundColor Red
        }
    }

    $mediumPriorityForms = $nonCompliantResults | Where-Object { $_.Priority -eq "MEDIUM" }
    if ($mediumPriorityForms.Count -gt 0) {
        Write-Host "   ⚠️ MEDIUM PRIORITY: Schedule for next sprint" -ForegroundColor Yellow
        foreach ($form in $mediumPriorityForms) {
            Write-Host "      - $($form.File)" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "2. Migration Steps:" -ForegroundColor Yellow
    Write-Host "   a) Replace 'DataGridView' with 'SfDataGrid'" -ForegroundColor Gray
    Write-Host "   b) Add 'using Syncfusion.WinForms.DataGrid;'" -ForegroundColor Gray
    Write-Host "   c) Use 'CreateEnhancedMaterialSfDataGrid()'" -ForegroundColor Gray
    Write-Host "   d) Apply 'ConfigureBusBuddyStandards()'" -ForegroundColor Gray
    Write-Host "   e) Update event handlers for SfDataGrid" -ForegroundColor Gray
    Write-Host "   f) Implement recommended Syncfusion features" -ForegroundColor Gray

} else {
    Write-Host "🎉 All forms are compliant! Consider implementing advanced features:" -ForegroundColor Green
    Write-Host "   - Data virtualization for large datasets" -ForegroundColor Gray
    Write-Host "   - Advanced filtering and export capabilities" -ForegroundColor Gray
    Write-Host "   - Custom column templates and styling" -ForegroundColor Gray
    Write-Host "   - Grouping and summary calculations" -ForegroundColor Gray
    Write-Host "   - Master-detail relationships" -ForegroundColor Gray
    Write-Host "   - Real-time data updates" -ForegroundColor Gray
}

Write-Host ""
Write-Host "📊 Detailed results saved to: compliance-results.json" -ForegroundColor Cyan

# Save detailed results to JSON
$complianceResults | ConvertTo-Json -Depth 4 | Out-File "compliance-results.json" -Encoding UTF8

# If in fix mode, provide specific migration commands
if ($FixMode -and $nonCompliantCount -gt 0) {
    Write-Host ""
    Write-Host "🔧 AUTOMATED FIX SUGGESTIONS" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan

    foreach ($result in $nonCompliantResults) {
        if ($result.DataGridViewCount -gt 0) {
            Write-Host ""
            Write-Host "File: $($result.File)" -ForegroundColor Yellow
            Write-Host "Commands to run:" -ForegroundColor Gray
            Write-Host "  # Replace DataGridView with SfDataGrid" -ForegroundColor Gray
            Write-Host "  (Get-Content '$($result.File)') -replace 'DataGridView', 'SfDataGrid' | Set-Content '$($result.File)'" -ForegroundColor White
            Write-Host "  # Add Syncfusion using statement" -ForegroundColor Gray
            Write-Host "  # Manual step: Add 'using Syncfusion.WinForms.DataGrid;' to file" -ForegroundColor White
            Write-Host "  # Manual step: Update constructor and event handlers" -ForegroundColor White
            Write-Host "  # Manual step: Implement recommended features from analysis" -ForegroundColor White
        }
    }
}

Write-Host ""
Write-Host "✅ Enhanced compliance validation complete!" -ForegroundColor Green
Write-Host "📋 Summary: $complianceRate% basic compliance, $averageFeatureCompliance% feature usage" -ForegroundColor Cyan

# Return compliance results for scripting
return @{
    BasicComplianceRate = $complianceRate
    FeatureComplianceRate = $averageFeatureCompliance
    TotalForms = $totalForms
    CompliantForms = $compliantCount
    NonCompliantForms = $nonCompliantCount
}
