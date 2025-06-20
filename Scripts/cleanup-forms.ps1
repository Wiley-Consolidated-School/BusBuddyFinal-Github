# Clean Up Unneeded Forms Script
# This script identifies and optionally removes MaterialSkin-based or unused forms

param(
    [switch]$DryRun = $false,
    [switch]$Verbose = $false
)

Write-Host "🧹 BusBuddy Form Cleanup Script" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

$rootPath = Get-Location
$logFile = "logs/form-cleanup.log"

# Ensure logs directory exists
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" -Force | Out-Null
}

function Write-Log {
    param($Message, $Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"
    Write-Host $logEntry
    Add-Content -Path $logFile -Value $logEntry
}

function Test-MaterialSkinForm {
    param($FilePath)

    if (!(Test-Path $FilePath)) { return $false }

    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if (!$content) { return $false }

    return $content -match "using MaterialSkin" -or
           $content -match "MaterialForm" -or
           $content -match "MaterialSkin\.Controls"
}

function Test-FormReferenced {
    param($FormName)

    # Check if form is referenced in navigation service or other files
    $searchPaths = @(
        "BusBuddy.UI\Services\*.cs",
        "BusBuddy.UI\Views\*.cs",
        "*.csproj"
    )

    foreach ($path in $searchPaths) {
        $files = Get-ChildItem -Path $path -Recurse -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            if ($content -and $content -match $FormName) {
                return $true
            }
        }
    }

    return $false
}

# Start cleanup process
Write-Log "Starting form cleanup analysis..." "INFO"
Write-Log "Root path: $rootPath" "INFO"
Write-Log "Dry run mode: $DryRun" "INFO"

# Find all .cs files in root directory (not in subdirectories)
$rootForms = Get-ChildItem -Path "*.cs" -ErrorAction SilentlyContinue | Where-Object {
    $_.Name -match "Form\.cs$" -or $_.Name -match "UserControl\.cs$"
}

Write-Log "Found $($rootForms.Count) potential form files in root directory" "INFO"

$materialSkinForms = @()
$unreferencedForms = @()
$safeForms = @()

foreach ($form in $rootForms) {
    $formName = $form.BaseName
    Write-Log "Analyzing form: $formName" "INFO"

    # Check if it's a MaterialSkin form
    $isMaterialSkin = Test-MaterialSkinForm -FilePath $form.FullName

    # Check if it's referenced
    $isReferenced = Test-FormReferenced -FormName $formName

    if ($isMaterialSkin) {
        $materialSkinForms += $form
        Write-Log "  ❌ MaterialSkin form detected: $($form.Name)" "WARNING"
    }
    elseif (!$isReferenced) {
        $unreferencedForms += $form
        Write-Log "  ⚠️  Unreferenced form detected: $($form.Name)" "WARNING"
    }
    else {
        $safeForms += $form
        Write-Log "  ✅ Form appears to be in use: $($form.Name)" "INFO"
    }
}

# Report findings
Write-Host "`n📊 ANALYSIS RESULTS:" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow
Write-Host "MaterialSkin forms found: $($materialSkinForms.Count)" -ForegroundColor Red
Write-Host "Unreferenced forms found: $($unreferencedForms.Count)" -ForegroundColor Yellow
Write-Host "Forms that appear safe: $($safeForms.Count)" -ForegroundColor Green

if ($materialSkinForms.Count -gt 0) {
    Write-Host "`n🚫 MATERIALSKIN FORMS (should be removed):" -ForegroundColor Red
    foreach ($form in $materialSkinForms) {
        Write-Host "  - $($form.Name)" -ForegroundColor Red

        # Look for associated files
        $baseName = $form.BaseName
        $associatedFiles = Get-ChildItem -Path "$baseName.*" -ErrorAction SilentlyContinue
        foreach ($file in $associatedFiles) {
            if ($file.Extension -in @(".Designer.cs", ".resx")) {
                Write-Host "    + $($file.Name)" -ForegroundColor Red
            }
        }
    }
}

if ($unreferencedForms.Count -gt 0) {
    Write-Host "`n⚠️  UNREFERENCED FORMS (review for removal):" -ForegroundColor Yellow
    foreach ($form in $unreferencedForms) {
        Write-Host "  - $($form.Name)" -ForegroundColor Yellow
    }
}

if ($safeForms.Count -gt 0) {
    Write-Host "`n✅ FORMS THAT APPEAR SAFE:" -ForegroundColor Green
    foreach ($form in $safeForms) {
        Write-Host "  - $($form.Name)" -ForegroundColor Green
    }
}

# Check .csproj for orphaned references
Write-Host "`n🔍 Checking .csproj for orphaned references..." -ForegroundColor Cyan
$csprojFiles = Get-ChildItem -Path "*.csproj" -ErrorAction SilentlyContinue
$orphanedReferences = @()

foreach ($csproj in $csprojFiles) {
    $content = Get-Content $csproj.FullName -Raw -ErrorAction SilentlyContinue
    if ($content) {
        # Look for EmbeddedResource or Compile references to non-existent files
        $matches = [regex]::Matches($content, '<EmbeddedResource.*Include="([^"]*\.resx)"')
        foreach ($match in $matches) {
            $referencedFile = $match.Groups[1].Value
            if (!(Test-Path $referencedFile)) {
                $orphanedReferences += $referencedFile
            }
        }
    }
}

if ($orphanedReferences.Count -gt 0) {
    Write-Host "❌ ORPHANED .CSPROJ REFERENCES:" -ForegroundColor Red
    foreach ($ref in $orphanedReferences) {
        Write-Host "  - $ref" -ForegroundColor Red
    }
}

# Cleanup actions
if (!$DryRun -and ($materialSkinForms.Count -gt 0 -or $orphanedReferences.Count -gt 0)) {
    Write-Host "`n🧹 PERFORMING CLEANUP..." -ForegroundColor Cyan

    # Remove MaterialSkin forms
    foreach ($form in $materialSkinForms) {
        try {
            $baseName = $form.BaseName
            $filesToRemove = Get-ChildItem -Path "$baseName.*" -ErrorAction SilentlyContinue

            foreach ($file in $filesToRemove) {
                Write-Log "Removing file: $($file.Name)" "INFO"
                Remove-Item $file.FullName -Force
            }
        }
        catch {
            Write-Log "Error removing $($form.Name): $($_.Exception.Message)" "ERROR"
        }
    }

    Write-Host "✅ Cleanup completed. Check logs for details." -ForegroundColor Green
}
elseif ($DryRun) {
    Write-Host "`n🔍 DRY RUN MODE - No files were actually removed" -ForegroundColor Cyan
    Write-Host "To perform actual cleanup, run without -DryRun flag" -ForegroundColor Cyan
}

# Final recommendations
Write-Host "`n💡 RECOMMENDATIONS:" -ForegroundColor Magenta
Write-Host "==================" -ForegroundColor Magenta

if ($materialSkinForms.Count -eq 0 -and $unreferencedForms.Count -eq 0) {
    Write-Host "✅ No cleanup needed - all forms appear to be properly organized" -ForegroundColor Green
}
else {
    Write-Host "1. Run 'dotnet build' after cleanup to verify no compilation errors" -ForegroundColor White
    Write-Host "2. Test the application to ensure all navigation works correctly" -ForegroundColor White
    Write-Host "3. Review unreferenced forms manually before removing" -ForegroundColor White
    Write-Host "4. Consider moving active forms to BusBuddy.UI/Views/ directory" -ForegroundColor White
}

Write-Host "`n📝 Detailed log saved to: $logFile" -ForegroundColor Cyan

# Generate cleanup script if needed
if ($materialSkinForms.Count -gt 0) {
    $cleanupScript = "# Generated cleanup commands`n"
    foreach ($form in $materialSkinForms) {
        $baseName = $form.BaseName
        $cleanupScript += "Remove-Item '$baseName.*' -Force`n"
    }

    Set-Content -Path "cleanup-forms.ps1" -Value $cleanupScript
    Write-Host "📜 Cleanup script generated: cleanup-forms.ps1" -ForegroundColor Cyan
}
