# Replace-VehicleWithBus.ps1
# Bulk replacement script to update deprecated vehicle terminology to bus terminology
# Created for BusBuddy project refactoring

param(
    [string]$Path = ".",
    [switch]$TestMode,
    [switch]$SkipBackup = $true,
    [string]$FilePattern = "*.cs",
    [string[]]$TargetFiles = @(
        "**/FuelManagementFormSyncfusion.cs",
        "**/DashboardViewModel.cs",
        "**/RouteManagementFormSyncfusion.cs",
        "**/DashboardDataModels.cs"
    )
)

# Color output functions
function Write-Success($message) { Write-Host $message -ForegroundColor Green }
function Write-Warning($message) { Write-Host $message -ForegroundColor Yellow }
function Write-Error($message) { Write-Host $message -ForegroundColor Red }
function Write-Info($message) { Write-Host $message -ForegroundColor Cyan }

# Define replacement patterns focused on AnalyticsService and DashboardViewModel (order matters - more specific first)
$replacementPatterns = @(
    # Variable declarations and assignments - most specific first
    @{ Pattern = 'var vehicles = '; Replacement = 'var buses = '; Description = 'Vehicles variable assignment' }
    @{ Pattern = 'List<Vehicle> vehicles'; Replacement = 'List<Bus> buses'; Description = 'Vehicles list declaration' }
    @{ Pattern = '_vehicles'; Replacement = '_buses'; Description = 'Private vehicles field' }

    # Variable usage in code - specific contexts
    @{ Pattern = '(?<=\s)vehicles(?=\s|\.|;|,|\)|\])'; Replacement = 'buses'; Description = 'Vehicles variable usage' }
    @{ Pattern = '(?<=\s)vehicle(?=\s|\.|;|,|\)|\])'; Replacement = 'bus'; Description = 'Vehicle variable usage' }
    @{ Pattern = '(?<=\()vehicle(?=\s|\.|;|,|\)|\])'; Replacement = 'bus'; Description = 'Vehicle parameter usage' }
    @{ Pattern = '(?<=,\s*)vehicle(?=\s|\.|;|,|\)|\])'; Replacement = 'bus'; Description = 'Vehicle in parameter list' }

    # Foreach loops - very specific pattern
    @{ Pattern = 'foreach\s*\(\s*var\s+vehicle\s+in\s+vehicles\s*\)'; Replacement = 'foreach (var bus in buses)'; Description = 'Foreach vehicle loop' }
    @{ Pattern = 'foreach\s*\(\s*Vehicle\s+vehicle\s+in\s+vehicles\s*\)'; Replacement = 'foreach (Bus bus in buses)'; Description = 'Foreach Vehicle loop typed' }

    # Property access patterns
    @{ Pattern = 'vehicle\.'; Replacement = 'bus.'; Description = 'Vehicle property access' }
    @{ Pattern = '\$\{vehicle\.'; Replacement = '${bus.'; Description = 'Vehicle string interpolation' }

    # Repository and interface references
    @{ Pattern = 'IVehicleRepository'; Replacement = 'IBusRepository'; Description = 'Vehicle Repository interface' }
    @{ Pattern = 'VehicleRepository'; Replacement = 'BusRepository'; Description = 'Vehicle Repository type' }
    @{ Pattern = '_vehicleRepository'; Replacement = '_busRepository'; Description = 'Vehicle Repository field' }
    @{ Pattern = 'vehicleRepository'; Replacement = 'busRepository'; Description = 'Vehicle Repository variable' }

    # Method calls
    @{ Pattern = 'GetAllVehicles\(\)'; Replacement = 'GetAllBuses()'; Description = 'GetAllVehicles method call' }
    @{ Pattern = 'GetVehicleById\('; Replacement = 'GetBusById('; Description = 'GetVehicleById method call' }

    # Type references
    @{ Pattern = 'List<Vehicle>'; Replacement = 'List<Bus>'; Description = 'List<Vehicle> type' }
    @{ Pattern = 'IEnumerable<Vehicle>'; Replacement = 'IEnumerable<Bus>'; Description = 'IEnumerable<Vehicle> type' }

    # Model property that needs to exist (this might need manual fix)
    @{ Pattern = 'AssignedVehicleID'; Replacement = 'AssignedBusID'; Description = 'AssignedVehicleID property' }

    # Form references
    @{ Pattern = 'VehicleManagementForm'; Replacement = 'BusManagementForm'; Description = 'VehicleManagementForm class' }

    # Comments and UI text
    @{ Pattern = 'Vehicle ComboBox'; Replacement = 'Bus ComboBox'; Description = 'Vehicle ComboBox reference' }
    @{ Pattern = '"Vehicle:"'; Replacement = '"Bus:"'; Description = 'Vehicle label string' }
    @{ Pattern = 'vehicle capacity'; Replacement = 'bus capacity'; Description = 'Vehicle capacity text' }
)

# Files to exclude from processing
$excludePatterns = @(
    "*.backup*",
    "*.old",
    "*.bak",
    "*_temp*",
    "*Migration*",
    "bin\*",
    "obj\*",
    ".vs\*"
)

function Test-ShouldExcludeFile {
    param([string]$FilePath)

    foreach ($pattern in $excludePatterns) {
        if ($FilePath -like $pattern) {
            return $true
        }
    }
    return $false
}

function Backup-File {
    param([string]$FilePath)

    $backupPath = "$FilePath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item $FilePath $backupPath
    Write-Info "  Backed up to: $backupPath"
    return $backupPath
}

function Invoke-FileProcessing {
    param(
        [string]$FilePath,
        [bool]$TestMode
    )

    if (Test-ShouldExcludeFile $FilePath) {
        Write-Warning "Skipping excluded file: $FilePath"
        return @{ Changed = $false; Replacements = 0 }
    }

    Write-Info "Processing: $FilePath"

    try {
        $content = Get-Content $FilePath -Raw -Encoding UTF8
        $totalReplacements = 0
        $changeLog = @()

        foreach ($pattern in $replacementPatterns) {
            $patternMatches = [regex]::Matches($content, $pattern.Pattern)
            if ($patternMatches.Count -gt 0) {
                $content = $content -replace $pattern.Pattern, $pattern.Replacement
                $totalReplacements += $patternMatches.Count
                $changeLog += "  - $($pattern.Description): $($patternMatches.Count) replacements"
                Write-Info "    $($pattern.Description): $($patternMatches.Count) replacements"
            }
        }

        if ($totalReplacements -gt 0) {
            Write-Success "  Total replacements: $totalReplacements"

            if (-not $TestMode) {
                if (-not $SkipBackup) {
                    $backupPath = Backup-File $FilePath
                }

                Set-Content $FilePath $content -Encoding UTF8 -NoNewline
                Write-Success "  File updated successfully"
            } else {
                Write-Warning "  TEST MODE: File would be updated with $totalReplacements changes"
            }

            return @{
                Changed = $true
                Replacements = $totalReplacements
                ChangeLog = $changeLog
                BackupPath = if (-not $SkipBackup -and -not $TestMode) { $backupPath } else { $null }
            }
        } else {
            Write-Info "  No changes needed"
            return @{ Changed = $false; Replacements = 0 }
        }
    }
    catch {
        Write-Error "  Error processing file: $($_.Exception.Message)"
        return @{ Changed = $false; Replacements = 0; Error = $_.Exception.Message }
    }
}

# Main execution
Write-Info "=== BusBuddy Vehicle-to-Bus Terminology Replacement ==="
Write-Info "Path: $Path"
Write-Info "Pattern: $FilePattern"
Write-Info "Test Mode: $TestMode"
Write-Info "Skip Backup: $SkipBackup"
Write-Info ""

# Get all files matching the pattern or target files
if ($TargetFiles.Count -gt 0) {
    $files = @()
    foreach ($pattern in $TargetFiles) {
        $matchedFiles = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse | Where-Object {
            $_.FullName -like "*$($pattern.Replace('**/', '').Replace('*/', ''))"
        }
        $files += $matchedFiles
    }
    $files = $files | Sort-Object FullName | Get-Unique
} else {
    $files = Get-ChildItem -Path $Path -Filter $FilePattern -Recurse | Where-Object { -not $_.PSIsContainer }
}

if ($files.Count -eq 0) {
    Write-Warning "No files found matching pattern '$FilePattern' in path '$Path'"
    exit
}

Write-Info "Found $($files.Count) files to process"
Write-Info ""

$processedFiles = 0
$changedFiles = 0
$totalReplacements = 0
$allChanges = @()

foreach ($file in $files) {
    $result = Invoke-FileProcessing $file.FullName $TestMode
    $processedFiles++

    if ($result.Changed) {
        $changedFiles++
        $totalReplacements += $result.Replacements
        $allChanges += @{
            File = $file.FullName
            Replacements = $result.Replacements
            ChangeLog = $result.ChangeLog
            BackupPath = $result.BackupPath
        }
    }

    if ($result.Error) {
        Write-Error "Error in $($file.FullName): $($result.Error)"
    }
}

# Summary
Write-Info ""
Write-Info "=== SUMMARY ==="
Write-Success "Files processed: $processedFiles"
Write-Success "Files changed: $changedFiles"
Write-Success "Total replacements: $totalReplacements"

if ($TestMode) {
    Write-Warning "TEST MODE: No files were actually modified"
    Write-Info "Run without -TestMode to apply changes"
}

if ($changedFiles -gt 0) {
    Write-Info ""
    Write-Info "=== DETAILED CHANGES ==="
    foreach ($change in $allChanges) {
        Write-Info "File: $($change.File)"
        Write-Info "  Replacements: $($change.Replacements)"
        if ($change.BackupPath) {
            Write-Info "  Backup: $($change.BackupPath)"
        }
        foreach ($log in $change.ChangeLog) {
            Write-Info $log
        }
        Write-Info ""
    }
}

Write-Info "=== RECOMMENDATIONS ==="
Write-Info "1. Review changed files for any unintended replacements"
Write-Info "2. Test build after changes: dotnet build"
Write-Info "3. Run tests to ensure functionality: dotnet test"
Write-Info "4. Check git diff to review all changes"

if (-not $SkipBackup -and -not $TestMode -and $changedFiles -gt 0) {
    Write-Info "5. Remove backup files after verification: Remove-Item *.backup_* -Force"
}
