#!/usr/bin/env pwsh
# Syncfusion Migration Helper Script
# This script helps track and manage the Syncfusion migration process

Write-Host "🚀 BusBuddy Syncfusion Migration Helper" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check current migration status
Write-Host "`n📊 CURRENT STATUS:" -ForegroundColor Yellow

# Dynamically discover migrated forms (those with Syncfusion suffix)
$viewsPath = "BusBuddy.UI\Views"
$migratedForms = @()
$allForms = @()
$pendingForms = @()

if (Test-Path $viewsPath) {
    # Get all Syncfusion forms (already migrated)
    $syncfusionFiles = Get-ChildItem -Path $viewsPath -Name "*Syncfusion*.cs" | Sort-Object
    $migratedForms = $syncfusionFiles

    # Get all forms that are NOT Syncfusion variants
    $allCsFiles = Get-ChildItem -Path $viewsPath -Name "*.cs" | Where-Object {
        $_ -notlike "*Syncfusion*.cs" -and
        $_ -ne ".gitkeep" -and
        $_ -ne "AnalyticsDemoForm.cs"  # Exclude demo/test forms
    } | Sort-Object

    # Check which non-Syncfusion forms don't have a corresponding Syncfusion version
    foreach ($file in $allCsFiles) {
        $baseName = $file -replace '\.cs$', ''
        $syncfusionVersion = "${baseName}Syncfusion.cs"

        if ($syncfusionFiles -notcontains $syncfusionVersion) {
            $pendingForms += $file
        }
    }

    $allForms = $allCsFiles + $syncfusionFiles
} else {
    Write-Host "⚠️  Views directory not found at: $viewsPath" -ForegroundColor Yellow
}

Write-Host "✅ Migrated Forms: $($migratedForms.Count)" -ForegroundColor Green
foreach ($form in $migratedForms) {
    Write-Host "   - $form" -ForegroundColor Green
}

Write-Host "`n❌ Pending Migration: $($pendingForms.Count)" -ForegroundColor Red
if ($pendingForms.Count -gt 0) {
    foreach ($form in $pendingForms) {
        Write-Host "   - $form" -ForegroundColor Red
    }
} else {
    Write-Host "   🎉 All forms have been migrated!" -ForegroundColor Green
}

# Check for forms that have both original and Syncfusion versions (potential cleanup candidates)
Write-Host "`n🔄 FORMS WITH BOTH VERSIONS (Review for cleanup):" -ForegroundColor Yellow
$formsWithBothVersions = @()

foreach ($syncfusionForm in $migratedForms) {
    # Handle different Syncfusion naming patterns
    $originalName = ""
    if ($syncfusionForm -match "(.+)Syncfusion(.*)\.cs$") {
        $baseName = $matches[1]
        $suffix = $matches[2]
        $originalName = "${baseName}${suffix}.cs"
    }

    if ($originalName -and $originalName -ne ".cs") {
        $originalPath = Join-Path $viewsPath $originalName

        if (Test-Path $originalPath) {
            $formsWithBothVersions += @{
                Original = $originalName
                Syncfusion = $syncfusionForm
            }
        }
    }
}

if ($formsWithBothVersions.Count -gt 0) {
    foreach ($formPair in $formsWithBothVersions) {
        Write-Host "   - $($formPair.Original) + $($formPair.Syncfusion)" -ForegroundColor Yellow
    }
    Write-Host "`n   💡 Consider removing original versions after verifying Syncfusion versions work correctly" -ForegroundColor Cyan
} else {
    Write-Host "   None found - clean migration state!" -ForegroundColor Green
}

# Check for files that can be cleaned up
Write-Host "`n🧹 CLEANUP CANDIDATES:" -ForegroundColor Cyan

$cleanupFiles = @()

# Check if SchoolCalendarEditFormTest.cs still exists
$testFile = "BusBuddy.UI\Views\SchoolCalendarEditFormTest.cs"
if (-not (Test-Path $testFile)) {
    Write-Host "✅ SchoolCalendarEditFormTest.cs - Already deleted" -ForegroundColor Green
} else {
    $cleanupFiles += $testFile
}

# Show material files that might be candidates for cleanup
$materialFiles = @(
    "BusBuddy.UI\Extensions\MaterialControlExtensions.cs",
    "BusBuddy.UI\Controls\MaterialDataGridView.cs",
    "BusBuddy.UI\Components\MaterialMessageBox.cs",
    "BusBuddy.UI\Components\MaterialEditPanel.cs",
    "BusBuddy.UI\Base\StandardMaterialForm.cs",
    "BusBuddy.UI\Base\StandardMaterialManagementForm.cs"
)

Write-Host "`n⚠️  Material files (review before deletion):" -ForegroundColor Yellow
foreach ($file in $materialFiles) {
    if (Test-Path $file) {
        Write-Host "   - $file" -ForegroundColor Yellow
    }
}

# Show next recommended actions
Write-Host "`n🎯 NEXT ACTIONS:" -ForegroundColor Magenta

if ($pendingForms.Count -gt 0) {
    Write-Host "📋 Forms pending migration:" -ForegroundColor White

    # Prioritize core entity forms first
    $coreEntityForms = $pendingForms | Where-Object {
        $_ -like "*EditForm.cs" -and $_ -notlike "*Management*.cs"
    }

    $managementForms = $pendingForms | Where-Object {
        $_ -like "*ManagementForm.cs"
    }

    $otherForms = $pendingForms | Where-Object {
        $_ -notlike "*EditForm.cs" -and $_ -notlike "*ManagementForm.cs"
    }

    if ($coreEntityForms.Count -gt 0) {
        Write-Host "`n1. Priority: Core Entity Forms" -ForegroundColor Yellow
        foreach ($form in $coreEntityForms) {
            $baseName = $form -replace '\.cs$', ''
            Write-Host "   - $form → ${baseName}Syncfusion.cs" -ForegroundColor White
        }
    }

    if ($managementForms.Count -gt 0) {
        Write-Host "`n2. Management Forms" -ForegroundColor Yellow
        foreach ($form in $managementForms) {
            $baseName = $form -replace '\.cs$', ''
            Write-Host "   - $form → ${baseName}Syncfusion.cs" -ForegroundColor White
        }
    }

    if ($otherForms.Count -gt 0) {
        Write-Host "`n3. Other Forms" -ForegroundColor Yellow
        foreach ($form in $otherForms) {
            $baseName = $form -replace '\.cs$', ''
            Write-Host "   - $form → ${baseName}Syncfusion.cs" -ForegroundColor White
        }
    }
} else {
    Write-Host "🎉 All forms have been migrated to Syncfusion!" -ForegroundColor Green
}

Write-Host "`n📝 General Steps:" -ForegroundColor White
Write-Host "   1. Copy original form to new Syncfusion version" -ForegroundColor White
Write-Host "   2. Update references to use Syncfusion controls" -ForegroundColor White
Write-Host "   3. Test the migrated form thoroughly" -ForegroundColor White
Write-Host "   4. Update management forms to use new Syncfusion version" -ForegroundColor White
Write-Host "   5. Run build and all tests after each migration" -ForegroundColor White

# Calculate progress
$totalForms = $migratedForms.Count + $pendingForms.Count
$progress = [math]::Round(($migratedForms.Count / $totalForms) * 100, 1)

Write-Host "`n📈 PROGRESS: $progress% complete ($($migratedForms.Count)/$totalForms forms)" -ForegroundColor Cyan

Write-Host "`n📖 For detailed analysis, see: SYNCFUSION_MIGRATION_ANALYSIS.md" -ForegroundColor Blue
