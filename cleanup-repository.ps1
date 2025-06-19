# BusBuddy Repository Cleanup Script
# Run this periodically to clean up temporary files and build artifacts

Write-Host "🧹 Starting BusBuddy Repository Cleanup..." -ForegroundColor Green

# Remove build artifacts
Write-Host "Removing build artifacts..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Directory -Name "bin", "obj", "TestResults" | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $_" -ForegroundColor Gray
    }
}

# Remove temporary files
Write-Host "Removing temporary files..." -ForegroundColor Yellow
$tempPatterns = @("*.tmp", "*.temp", "*.bak", "*.backup", "*_new.*", "*_old.*", "*.crdownload")
foreach ($pattern in $tempPatterns) {
    Get-ChildItem -Recurse -File -Name $pattern | ForEach-Object {
        Remove-Item $_ -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $_" -ForegroundColor Gray
    }
}

# Remove backup directories
Write-Host "Removing backup directories..." -ForegroundColor Yellow
$backupDirs = @("*_Backup*", "*_backups*", "Migration_Backups")
foreach ($dir in $backupDirs) {
    Get-ChildItem -Recurse -Directory -Name $dir | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  Removed: $_" -ForegroundColor Gray
        }
    }
}

# Remove empty directories
Write-Host "Removing empty directories..." -ForegroundColor Yellow
Get-ChildItem -Recurse -Directory | Where-Object {
    (Get-ChildItem $_.FullName -Force | Measure-Object).Count -eq 0
} | ForEach-Object {
    Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
    Write-Host "  Removed empty: $($_.Name)" -ForegroundColor Gray
}

# Git cleanup
Write-Host "Running git cleanup..." -ForegroundColor Yellow
try {
    git clean -fd 2>$null
    Write-Host "  Git clean completed" -ForegroundColor Gray
} catch {
    Write-Host "  Git clean skipped (not in git repo)" -ForegroundColor Gray
}

# Check for trailing whitespace (report only)
Write-Host "Checking for trailing whitespace..." -ForegroundColor Yellow
$sourceFilePatterns = @("*.cs", "*.md", "*.json", "*.xml", "*.config", "*.ps1")
$filesWithTrailingWS = @()

foreach ($pattern in $sourceFilePatterns) {
    Get-ChildItem -Recurse -File -Include $pattern | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -ErrorAction SilentlyContinue
        if ($content -and ($content -match '[ \t]+\r?\n' -or $content -match '[ \t]+$')) {
            $filesWithTrailingWS += $_.Name
            Write-Host "  Found trailing whitespace: $($_.Name)" -ForegroundColor Yellow
        }
    }
}

if ($filesWithTrailingWS.Count -eq 0) {
    Write-Host "  No trailing whitespace found ✓" -ForegroundColor Gray
} else {
    Write-Host "  ⚠️ Found $($filesWithTrailingWS.Count) files with trailing whitespace" -ForegroundColor Yellow
    Write-Host "  💡 Consider running a code formatter to fix these automatically" -ForegroundColor Blue
}

# Show summary
Write-Host "`n📊 Repository Status:" -ForegroundColor Green
if (Get-Command git -ErrorAction SilentlyContinue) {
    try {
        $objCount = git count-objects -v | Select-String "count" | ForEach-Object { $_.ToString().Split(" ")[1] }
        $repoSize = git count-objects -vH | Select-String "size-pack" | ForEach-Object { ($_.ToString() -split ": ")[1] }
        Write-Host "  Git objects: $objCount" -ForegroundColor Cyan
        Write-Host "  Repository size: $repoSize" -ForegroundColor Cyan
    } catch {
        Write-Host "  Git status unavailable" -ForegroundColor Gray
    }
}

$totalFiles = (Get-ChildItem -Recurse -File | Measure-Object).Count
$totalDirs = (Get-ChildItem -Recurse -Directory | Measure-Object).Count
Write-Host "  Total files: $totalFiles" -ForegroundColor Cyan
Write-Host "  Total directories: $totalDirs" -ForegroundColor Cyan

Write-Host "`n✅ Cleanup completed!" -ForegroundColor Green
Write-Host "💡 Tip: Run this script regularly to keep the repository clean." -ForegroundColor Blue
