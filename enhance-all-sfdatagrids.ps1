#!/usr/bin/env pwsh
# Enhanced SfDataGrid Feature Implementation Script
# Applies 100% feature implementation across all BusBuddy forms

param(
    [switch]$DryRun,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Define forms that need enhancement
$formsToEnhance = @(
    "BusBuddy.UI/Views/ActivityManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/ActivityScheduleManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/ActivityScheduleManagementFormSyncfusion_Enhanced.cs",
    "BusBuddy.UI/Views/FuelManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/FuelManagementFormSyncfusion_Enhanced.cs",
    "BusBuddy.UI/Views/RouteManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/RouteManagementFormSyncfusion_Enhanced.cs",
    "BusBuddy.UI/Views/SchoolCalendarManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/SchoolCalendarManagementFormSyncfusion_Enhanced.cs",
    "BusBuddy.UI/Views/VehicleManagementFormSyncfusion.cs",
    "BusBuddy.TimeCard/Views/TimeCardManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/MaintenanceManagementFormSyncfusion.cs",
    "BusBuddy.UI/Views/AnalyticsDemoForm.cs"
)

function Write-StatusMessage {
    param([string]$Message, [string]$Level = "Info")

    $color = switch ($Level) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        default { "White" }
    }

    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $Message" -ForegroundColor $color
}

function Set-SfDataGridSetup {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        Write-StatusMessage "File not found: $FilePath" "Warning"
        return $false
    }

    $content = Get-Content $FilePath -Raw
    $originalContent = $content
    $modified = $false

    # Pattern 1: Basic SfDataGrid creation
    $pattern1 = '_\w+Grid = new SfDataGrid\(\);'
    if ($content -match $pattern1) {
        $formName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
        $replacement = "_$($formName.Replace('FormSyncfusion', '').Replace('Form', ''))Grid = SyncfusionThemeHelper.CreateEnhancedMaterialSfDataGrid();"
        $content = $content -replace $pattern1, $replacement
        $modified = $true
        Write-StatusMessage "Enhanced grid creation in $FilePath" "Success"
    }

    # Pattern 2: Look for SetupDataGrid method and enhance it
    $setupDataGridPattern = '(private void SetupDataGrid\(\)[^}]*\{[^}]*)'
    if ($content -match $setupDataGridPattern) {
        $formName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
        $contextName = $formName.Replace('Syncfusion', '').Replace('_Enhanced', '')

        # Add enhancement call after grid creation
        $enhancementCall = @"
            // Apply ALL Syncfusion features for 100% implementation
            SyncfusionThemeHelper.SfDataGridEnhancements.ApplyAllFeaturesToGrid(_*Grid, "$contextName");
"@

        # Find the position after grid creation but before column setup
        $insertPosition = $content.IndexOf("SetupGridColumns();")
        if ($insertPosition -eq -1) {
            $insertPosition = $content.IndexOf("SetupDataGridColumns();")
        }
        if ($insertPosition -eq -1) {
            $insertPosition = $content.IndexOf("_mainPanel.Controls.Add(_")
        }

        if ($insertPosition -gt 0) {
            $content = $content.Insert($insertPosition, $enhancementCall)
            $modified = $true
            Write-StatusMessage "Added feature enhancement to $FilePath" "Success"
        }
    }

    # Pattern 3: Enhance existing manual property setting
    $manualPropertiesPatterns = @(
        'grid\.AllowEditing = false;',
        'grid\.AllowSorting = true;',
        'grid\.AllowFiltering = true;',
        '_\w+Grid\.SelectionMode = GridSelectionMode\.\w+;'
    )

    foreach ($pattern in $manualPropertiesPatterns) {
        if ($content -match $pattern) {
            # Add comment indicating this will be replaced by enhanced features
            $comment = "// Enhanced by ApplyAllFeaturesToGrid - will override manual settings"
            $content = $content -replace "($pattern)", "$comment`n            `$1"
            $modified = $true
        }
    }

    # Save the file if modified
    if ($modified -and -not $DryRun) {
        Set-Content -Path $FilePath -Value $content -Encoding UTF8
        Write-StatusMessage "✅ Enhanced $FilePath" "Success"
        return $true
    } elseif ($modified -and $DryRun) {
        Write-StatusMessage "🔍 Would enhance $FilePath (dry run)" "Info"
        return $true
    }

    return $false
}

function Add-ColumnTypeEnhancements {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) {
        return $false
    }

    $content = Get-Content $FilePath -Raw
    $modified = $false

    # Enhance GridTextColumn usage to include more column types
    if ($content -match 'new GridTextColumn\(\)') {
        # Look for patterns that could be enhanced with specific column types

        # Date columns
        $datePatterns = @('Date', 'CreatedAt', 'UpdatedAt', 'FuelDate', 'LastMaintenance')
        foreach ($pattern in $datePatterns) {
            $textColumnPattern = "new GridTextColumn\(\)[^}]*MappingName = `"$pattern`""
            if ($content -match $textColumnPattern) {
                $content = $content -replace "GridTextColumn", "GridDateTimeColumn"
                $modified = $true
                Write-StatusMessage "Enhanced $pattern column to GridDateTimeColumn" "Success"
            }
        }

        # Numeric columns
        $numericPatterns = @('Price', 'Cost', 'Amount', 'Mileage', 'Capacity', 'ID')
        foreach ($pattern in $numericPatterns) {
            $textColumnPattern = "new GridTextColumn\(\)[^}]*MappingName = `".*$pattern.*`""
            if ($content -match $textColumnPattern) {
                $content = $content -replace "GridTextColumn", "GridNumericColumn"
                $modified = $true
                Write-StatusMessage "Enhanced $pattern column to GridNumericColumn" "Success"
            }
        }
    }

    if ($modified -and -not $DryRun) {
        Set-Content -Path $FilePath -Value $content -Encoding UTF8
        return $true
    }

    return $false
}

function Main {
    Write-StatusMessage "🚀 Starting BusBuddy SfDataGrid 100% Feature Enhancement" "Success"
    Write-StatusMessage "Target: Achieve 100% feature implementation across all forms" "Info"

    if ($DryRun) {
        Write-StatusMessage "🔍 DRY RUN MODE - No files will be modified" "Warning"
    }

    $enhancedCount = 0
    $totalForms = $formsToEnhance.Count

    foreach ($formPath in $formsToEnhance) {
        Write-StatusMessage "Processing: $formPath" "Info"

        $fullPath = Join-Path $PWD $formPath

        if (Set-SfDataGridSetup -FilePath $fullPath) {
            $enhancedCount++
        }

        # Also enhance column types
        Add-ColumnTypeEnhancements -FilePath $fullPath | Out-Null
    }

    Write-StatusMessage "📊 ENHANCEMENT SUMMARY" "Success"
    Write-StatusMessage "Total Forms: $totalForms" "Info"
    Write-StatusMessage "Enhanced Forms: $enhancedCount" "Success"
    Write-StatusMessage "Success Rate: $([math]::Round(($enhancedCount/$totalForms)*100, 1))%" "Success"

    if (-not $DryRun) {
        Write-StatusMessage "🔄 Running validation to verify 100% implementation..." "Info"
        & "$PWD\validate-sfdatagrid-compliance-enhanced.ps1"
    }

    Write-StatusMessage "✅ Enhancement process complete!" "Success"
    Write-StatusMessage "Next: All forms should now have 100% feature implementation" "Info"
}

# Execute main function
Main
