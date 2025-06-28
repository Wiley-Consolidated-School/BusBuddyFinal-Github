# BusBuddy - Cleanup Script for Temporary Files
# Cleans up .bak, .backup, .new, and other temporary files

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  BusBuddy - Cleaning up temporary files" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Define patterns to search for
$tempPatterns = @(
    "*.bak",
    "*.backup*",
    "*.new",
    "*_temp*",
    "*_tmp*",
    "*_test*"
)

$totalRemoved = 0

foreach ($pattern in $tempPatterns) {
    $files = Get-ChildItem -Path . -Recurse -Filter $pattern -File

    foreach ($file in $files) {
        Write-Host "Removing temporary file: $($file.FullName)" -ForegroundColor Yellow
        try {
            Remove-Item -Path $file.FullName -Force
            Write-Host "  ✅ Successfully removed" -ForegroundColor Green
            $totalRemoved++
        }
        catch {
            Write-Host "  ❌ Failed to remove: $_" -ForegroundColor Red
        }
    }
}

Write-Host "`nTotal temporary files removed: $totalRemoved" -ForegroundColor Cyan

# Check if any backup directories exist
$backupDirs = @(
    "Migration_Backups"
)

foreach ($dir in $backupDirs) {
    $dirs = Get-ChildItem -Path . -Recurse -Directory -Filter $dir

    foreach ($d in $dirs) {
        Write-Host "Found backup directory: $($d.FullName)" -ForegroundColor Yellow
        $remove = Read-Host "Do you want to remove this directory? (y/n)"

        if ($remove -eq "y") {
            try {
                Remove-Item -Path $d.FullName -Recurse -Force
                Write-Host "  ✅ Successfully removed backup directory" -ForegroundColor Green
            }
            catch {
                Write-Host "  ❌ Failed to remove directory: $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host "`nCleanup completed!" -ForegroundColor Cyan
