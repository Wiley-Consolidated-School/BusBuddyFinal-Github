#!/usr/bin/env pwsh
# Syncfusion Migration Automation Tool
# This script automates the migration of Material forms to Syncfusion

param(
    [Parameter(Mandatory=$false)]
    [string]$FormName,

    [Parameter(Mandatory=$false)]
    [switch]$ListForms,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [switch]$All,

    [Parameter(Mandatory=$false)]
    [switch]$Phase1,

    [Parameter(Mandatory=$false)]
    [switch]$Phase2,

    [Parameter(Mandatory=$false)]
    [switch]$Phase3
)

# Configuration
$BaseUIPath = "BusBuddy.UI"
$ViewsPath = "$BaseUIPath\Views"
$ComponentsPath = "$BaseUIPath\Components"
$HelpersPath = "$BaseUIPath\Helpers"

# Define migration phases
$Phase1Forms = @(
    "ActivityEditForm.cs",
    "VehicleForm.cs",
    "FuelEditForm.cs",
    "MaintenanceEditForm.cs",
    "SchoolCalendarEditForm.cs"
)

$Phase2Forms = @(
    "ActivityManagementForm.cs",
    "VehicleManagementForm.cs",
    "FuelManagementForm.cs",
    "MaintenanceManagementForm.cs",
    "SchoolCalendarManagementForm.cs"
)

$Phase3Forms = @(
    "RouteManagementForm.cs",
    "DriverManagementForm.cs",
    "ActivityScheduleEditForm.cs",
    "ActivityScheduleManagementForm.cs"
)

$AllForms = $Phase1Forms + $Phase2Forms + $Phase3Forms

# Already migrated forms
$MigratedForms = @(
    "BusBuddyDashboardSyncfusion.cs",
    "DriverEditFormSyncfusion.cs",
    "RouteEditFormSyncfusion.cs",
    "RouteEditFormSyncfusionSimple.cs"
)

function Write-Header {
    Write-Host "`n🚀 BusBuddy Syncfusion Migration Tool" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Green
}

function Write-Status {
    param($Message, $Color = "White")
    Write-Host "📋 $Message" -ForegroundColor $Color
}

function Write-Error {
    param($Message)
    Write-Host "❌ ERROR: $Message" -ForegroundColor Red
}

function Write-Success {
    param($Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param($Message)
    Write-Host "⚠️  WARNING: $Message" -ForegroundColor Yellow
}

function Write-Info {
    param($Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Test-Prerequisites {
    Write-Status "Checking prerequisites..."

    # Check if we're in the right directory
    if (-not (Test-Path "BusBuddy.sln")) {
        Write-Error "Please run this script from the BusBuddy root directory"
        return $false
    }

    # Check if Syncfusion utilities exist
    $syncfusionUtils = "$HelpersPath\SyncfusionMigrationUtilities.cs"
    if (-not (Test-Path $syncfusionUtils)) {
        Write-Error "SyncfusionMigrationUtilities.cs not found at $syncfusionUtils"
        return $false
    }

    Write-Success "Prerequisites check passed"
    return $true
}

function Get-MaterialControlMappings {
    return @{
        "MaterialSkinManager" = "SfSkinManager"
        "MaterialForm" = "SfForm"
        "MaterialRaisedButton" = "SfButton"
        "MaterialFlatButton" = "SfButton"
        "MaterialSingleLineTextField" = "SfTextBox"
        "MaterialLabel" = "Label"
        "MaterialTabControl" = "SfTabControl"
        "MaterialTabPage" = "TabPageAdv"
        "MaterialComboBox" = "SfComboBox"
        "MaterialDateTimePicker" = "SfDateTimeEdit"
        "DataGridView" = "SfDataGrid"
        "MaterialCheckBox" = "SfCheckBox"
        "MaterialRadioButton" = "SfRadioButton"
        "MaterialContextMenuStrip" = "SfContextMenu"
        "MaterialProgressBar" = "SfProgressBar"
        "MaterialCard" = "SfGradientPanel"
        "MaterialDivider" = "SfSeparator"
    }
}

function Get-UsingStatements {
    return @(
        "using Syncfusion.Windows.Forms;",
        "using Syncfusion.Windows.Forms.Tools;",
        "using Syncfusion.Windows.Forms.Grid;",
        "using Syncfusion.WinForms.Controls;",
        "using Syncfusion.WinForms.DataGrid;",
        "using Syncfusion.WinForms.Input;",
        "using Syncfusion.WinForms.ListView;",
        "using BusBuddy.UI.Helpers;"
    )
}

function Backup-OriginalFile {
    param($SourcePath)

    $backupDir = "Migration_Backups"
    if (-not (Test-Path $backupDir)) {
        New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    }

    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $fileName = [System.IO.Path]::GetFileName($SourcePath)
    $backupPath = "$backupDir\${fileName}.backup_$timestamp"

    Copy-Item $SourcePath $backupPath -Force
    Write-Info "Backup created: $backupPath"
    return $backupPath
}

function Convert-MaterialToSyncfusion {
    param(
        [string]$SourcePath,
        [string]$TargetPath,
        [bool]$DryRun = $false
    )

    if (-not (Test-Path $SourcePath)) {
        Write-Error "Source file not found: $SourcePath"
        return $false
    }

    Write-Status "Converting $SourcePath to Syncfusion..."

    # Read the source file
    $content = Get-Content $SourcePath -Raw
    $originalContent = $content

    # Get mappings
    $mappings = Get-MaterialControlMappings
    $usingStatements = Get-UsingStatements

    # Remove Material-specific using statements
    $materialUsings = @(
        "using MaterialSkin;",
        "using MaterialSkin.Controls;",
        "using MaterialSkin.Animations;"
    )

    foreach ($using in $materialUsings) {
        $content = $content.Replace($using, "")
    }

    # Add Syncfusion using statements
    $firstUsingIndex = $content.IndexOf("using ")
    if ($firstUsingIndex -ge 0) {
        $insertPoint = $content.IndexOf("`n", $firstUsingIndex)
        if ($insertPoint -ge 0) {
            $syncfusionUsings = ($usingStatements -join "`n") + "`n"
            $content = $content.Insert($insertPoint + 1, $syncfusionUsings)
        }
    }

    # Replace control types
    foreach ($mapping in $mappings.GetEnumerator()) {
        $content = $content -replace "\b$($mapping.Key)\b", $mapping.Value
    }
    
    # Additional Material control replacements
    $content = $content -replace "\bMaterialTextBox\b", "SfTextBox"
    $content = $content -replace "\bMaterialButton\b", "SfButton"
    $content = $content -replace "\bMaterialLabel\b", "Label"
    $content = $content -replace "\bMaterialComboBox\b", "SfComboBox"
    $content = $content -replace "\bMaterialCheckBox\b", "SfCheckBox"
    $content = $content -replace "\bMaterialRadioButton\b", "SfRadioButton"
    $content = $content -replace "\bMaterialDateTimePicker\b", "SfDateTimeEdit"
    $content = $content -replace "\bMaterialTabControl\b", "SfTabControl"
    $content = $content -replace "\bMaterialTabPage\b", "TabPageAdv"
    $content = $content -replace "\bMaterialProgressBar\b", "SfProgressBar"
    $content = $content -replace "\bMaterialContextMenuStrip\b", "SfContextMenu"
    $content = $content -replace "\bMaterialCard\b", "SfGradientPanel"
    $content = $content -replace "\bMaterialDivider\b", "SfSeparator"
    
    # Replace property names and method calls
    $content = $content -replace "\.Hint\s*=", ".WatermarkText ="
    $content = $content -replace "\.UseAccent\s*=", ".UseVisualStyle ="
    $content = $content -replace "\.PasswordChar\s*=", ".PasswordChar ="

    # Replace class inheritance
    $content = $content -replace ":\s*MaterialForm", ": SfForm"
    $content = $content -replace ":\s*StandardMaterialForm", ": StandardSyncfusionForm"
    $content = $content -replace ":\s*StandardMaterialManagementForm", ": StandardSyncfusionManagementForm"    # Update class name
    $originalClassName = [System.IO.Path]::GetFileNameWithoutExtension($SourcePath)
    $newClassName = $originalClassName -replace "Form$", "FormSyncfusion"
    if ($newClassName -eq $originalClassName) {
        $newClassName += "Syncfusion"
    }
    
    $content = $content -replace "\bpublic\s+partial\s+class\s+$originalClassName\b", "public partial class $newClassName"
    $content = $content -replace "\bpublic\s+class\s+$originalClassName\b", "public class $newClassName"
    $content = $content -replace "\b$originalClassName\(\)", "$newClassName()"

    # Replace MaterialSkinManager initialization
    $content = $content -replace "MaterialSkinManager\.Instance\.AddFormToManage\(this\);", "SyncfusionHelper.ApplyTheme(this);"
    $content = $content -replace "MaterialSkinManager\.Instance\.Theme = MaterialSkinManager\.Themes\.LIGHT;", "// Theme applied via SyncfusionHelper"
    $content = $content -replace "MaterialSkinManager\.Instance\.ColorScheme = .*?;", "// Color scheme applied via SyncfusionHelper"

    # Update constructor calls in content
    $content = $content -replace "new $originalClassName\(", "new $newClassName("

    if ($DryRun) {
        Write-Info "DRY RUN: Would convert $SourcePath to $TargetPath"
        Write-Info "Changes detected: $($content.Length -ne $originalContent.Length)"
        return $true
    }
      # Create backup
    Backup-OriginalFile $SourcePath | Out-Null

    # Write the converted content
    $content | Out-File -FilePath $TargetPath -Encoding UTF8
    Write-Success "Converted: $TargetPath"

    return $true
}

function Update-References {
    param(
        [string]$OldClassName,
        [string]$NewClassName,
        [bool]$DryRun = $false
    )

    Write-Status "Updating references from $OldClassName to $NewClassName..."

    # Find all .cs files that might reference the old class
    $csFiles = Get-ChildItem -Path $BaseUIPath -Filter "*.cs" -Recurse

    $updatedFiles = @()

    foreach ($file in $csFiles) {
        $content = Get-Content $file.FullName -Raw
        $originalContent = $content

        # Update class instantiations
        $content = $content -replace "\bnew\s+$OldClassName\b", "new $NewClassName"

        # Update type declarations
        $content = $content -replace "\b$OldClassName\b(?=\s+\w+\s*[;=])", $NewClassName

        if ($content -ne $originalContent) {
            if ($DryRun) {
                Write-Info "DRY RUN: Would update references in $($file.FullName)"
            } else {
                Backup-OriginalFile $file.FullName
                $content | Out-File -FilePath $file.FullName -Encoding UTF8
                $updatedFiles += $file.FullName
            }
        }
    }

    if (-not $DryRun -and $updatedFiles.Count -gt 0) {
        Write-Success "Updated references in $($updatedFiles.Count) files"
        foreach ($file in $updatedFiles) {
            Write-Info "  - $file"
        }
    }
}

function Test-Migration {
    param([string]$FormPath)

    Write-Status "Testing migration for $FormPath..."

    # Run build test
    Write-Info "Running build test..."
    $buildResult = & dotnet build BusBuddy.sln --verbosity quiet

    if ($LASTEXITCODE -eq 0) {
        Write-Success "Build test passed"
        return $true
    } else {
        Write-Error "Build test failed"
        Write-Host $buildResult -ForegroundColor Red
        return $false
    }
}

function Convert-Form {
    param(
        [string]$FormName,
        [bool]$DryRun = $false
    )

    # Determine source path
    $sourcePath = ""
    $possiblePaths = @(
        "$ViewsPath\$FormName",
        "$ComponentsPath\$FormName",
        "$BaseUIPath\$FormName"
    )

    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            $sourcePath = $path
            break
        }
    }

    if (-not $sourcePath) {
        Write-Error "Form not found: $FormName"
        return $false
    }

    # Determine target path
    $targetDir = [System.IO.Path]::GetDirectoryName($sourcePath)
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($FormName)
    $newName = $baseName -replace "Form$", "FormSyncfusion"
    if ($newName -eq $baseName) {
        $newName += "Syncfusion"
    }
    $targetPath = "$targetDir\$newName.cs"

    Write-Status "Migrating: $FormName"
    Write-Info "Source: $sourcePath"
    Write-Info "Target: $targetPath"

    if ($DryRun) {
        Write-Info "DRY RUN: Would migrate $FormName"
        return Convert-MaterialToSyncfusion $sourcePath $targetPath $true
    }

    # Perform the migration
    if (Convert-MaterialToSyncfusion $sourcePath $targetPath $false) {
        Update-References $baseName $newName $false

        # Test the migration
        if (Test-Migration $targetPath) {
            Write-Success "Successfully migrated: $FormName"
            return $true
        } else {
            Write-Error "Migration test failed for: $FormName"
            return $false
        }
    }

    return $false
}

function Show-FormList {
    Write-Header

    Write-Host "`n📋 MIGRATION STATUS:" -ForegroundColor Yellow

    Write-Host "`n✅ Already Migrated:" -ForegroundColor Green
    foreach ($form in $MigratedForms) {
        Write-Host "   - $form" -ForegroundColor Green
    }

    Write-Host "`n🔄 Phase 1 (Core Entity Forms):" -ForegroundColor Cyan
    foreach ($form in $Phase1Forms) {
        Write-Host "   - $form" -ForegroundColor Cyan
    }

    Write-Host "`n🔄 Phase 2 (Management Forms):" -ForegroundColor Magenta
    foreach ($form in $Phase2Forms) {
        Write-Host "   - $form" -ForegroundColor Magenta
    }

    Write-Host "`n🔄 Phase 3 (Advanced Forms):" -ForegroundColor Yellow
    foreach ($form in $Phase3Forms) {
        Write-Host "   - $form" -ForegroundColor Yellow
    }

    $totalForms = $MigratedForms.Count + $AllForms.Count
    $progress = [math]::Round(($MigratedForms.Count / $totalForms) * 100, 1)
    Write-Host "`n📈 Progress: $progress% ($($MigratedForms.Count)/$totalForms)" -ForegroundColor Green
}

function Show-Usage {
    Write-Host @"

🎯 USAGE:
  .\migrate-to-syncfusion.ps1 -ListForms                    # Show all forms and status
  .\migrate-to-syncfusion.ps1 -FormName ActivityEditForm.cs # Migrate specific form
  .\migrate-to-syncfusion.ps1 -Phase1                       # Migrate all Phase 1 forms
  .\migrate-to-syncfusion.ps1 -Phase2                       # Migrate all Phase 2 forms
  .\migrate-to-syncfusion.ps1 -Phase3                       # Migrate all Phase 3 forms
  .\migrate-to-syncfusion.ps1 -All                          # Migrate all remaining forms
  .\migrate-to-syncfusion.ps1 -FormName Activity.cs -DryRun # Preview changes without applying

🔧 OPTIONS:
  -DryRun    : Preview changes without applying them
  -ListForms : Show migration status for all forms

📁 PHASES:
  Phase 1: Core entity edit forms (ActivityEditForm, VehicleForm, etc.)
  Phase 2: Management forms that use Phase 1 forms
  Phase 3: Advanced and schedule forms

"@ -ForegroundColor White
}

# Main execution
Write-Header

if (-not (Test-Prerequisites)) {
    exit 1
}

if ($ListForms) {
    Show-FormList
    exit 0
}

if (-not $FormName -and -not $All -and -not $Phase1 -and -not $Phase2 -and -not $Phase3) {
    Show-Usage
    exit 0
}

# Process single form
if ($FormName) {
    if (Convert-Form $FormName $DryRun) {
        Write-Success "Migration completed successfully!"
    } else {
        Write-Error "Migration failed!"
        exit 1
    }
    exit 0
}

# Process phase or all forms
$formsToMigrate = @()

if ($Phase1) { $formsToMigrate += $Phase1Forms }
if ($Phase2) { $formsToMigrate += $Phase2Forms }
if ($Phase3) { $formsToMigrate += $Phase3Forms }
if ($All) { $formsToMigrate = $AllForms }

if ($formsToMigrate.Count -eq 0) {
    Write-Warning "No forms selected for migration"
    Show-Usage
    exit 0
}

Write-Status "Migrating $($formsToMigrate.Count) forms..."

$successCount = 0
$failedForms = @()

foreach ($form in $formsToMigrate) {
    Write-Host "`n" + ("="*50) -ForegroundColor Gray

    if (Convert-Form $form $DryRun) {
        $successCount++
    } else {
        $failedForms += $form
    }
}

# Summary
Write-Host "`n" + ("="*50) -ForegroundColor Gray
Write-Host "`n📊 MIGRATION SUMMARY:" -ForegroundColor Yellow

if ($DryRun) {
    Write-Info "DRY RUN completed - no changes were made"
} else {
    Write-Success "Successfully migrated: $successCount forms"

    if ($failedForms.Count -gt 0) {
        Write-Error "Failed to migrate $($failedForms.Count) forms:"
        foreach ($form in $failedForms) {
            Write-Host "   - $form" -ForegroundColor Red
        }
    }
}

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Review generated Syncfusion forms" -ForegroundColor White
Write-Host "2. Test forms manually for UI consistency" -ForegroundColor White
Write-Host "3. Run full test suite: dotnet test" -ForegroundColor White
Write-Host "4. Update any remaining references" -ForegroundColor White

if ($failedForms.Count -eq 0 -and -not $DryRun) {
    Write-Success "All migrations completed successfully! 🎉"
} elseif ($failedForms.Count -gt 0) {
    Write-Warning "Some migrations failed. Check the error messages above."
    exit 1
}
