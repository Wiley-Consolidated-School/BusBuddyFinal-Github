#!/usr/bin/env pwsh
#####################################################
# BusBuddy - Build Artifacts Cleanup Script
# Removes build artifacts, temporary files, and ensures .gitignore compliance
#####################################################

param(
    [switch]$DryRun,
    [switch]$Verbose
)

Write-Host "🧹 BusBuddy Build Artifacts Cleanup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

if ($DryRun) {
    Write-Host "🔍 DRY RUN MODE - No files will be deleted" -ForegroundColor Yellow
}

# Build artifact directories to remove
$buildDirs = @("bin", "obj", "TestResults")

# File patterns to remove (following .gitignore)
$filePatterns = @(
    "*.tmp", "*.temp", "*_temp", "*_tmp", "*_test",
    "*.backup", "*.bak", "*.old", "*_backup*", "*_new",
    "*.crdownload", "*.user", "*.suo", "*.dll", "*.exe"
)

# Special directories to remove
$specialDirs = @("Migration_Backups", ".vs")

$totalCleaned = 0

function Remove-ItemSafely {
    param([string]$Path, [string]$Description)

    if (Test-Path $Path) {
        if ($Verbose) { Write-Host "  Found: $Description" -ForegroundColor Gray }

        if (-not $DryRun) {
            try {
                Remove-Item -Path $Path -Recurse -Force -ErrorAction Stop
                Write-Host "  ✅ Removed: $Description" -ForegroundColor Green
                $script:totalCleaned++
            }
            catch {
                Write-Host "  ❌ Failed to remove: $Description - $($_.Exception.Message)" -ForegroundColor Red
            }
        }
        else {
            Write-Host "  🔍 Would remove: $Description" -ForegroundColor Yellow
            $script:totalCleaned++
        }
    }
}

# Clean build directories recursively
Write-Host "`n🏗️  Cleaning build directories..." -ForegroundColor Magenta
foreach ($dir in $buildDirs) {
    Get-ChildItem -Path . -Name $dir -Directory -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-ItemSafely -Path $_ -Description "Build directory: $_"
    }
}

# Clean file patterns
Write-Host "`n📄 Cleaning temporary files..." -ForegroundColor Magenta
foreach ($pattern in $filePatterns) {
    Get-ChildItem -Path . -Name $pattern -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-ItemSafely -Path $_ -Description "Temporary file: $_"
    }
}

# Clean special directories
Write-Host "`n📁 Cleaning special directories..." -ForegroundColor Magenta
foreach ($dir in $specialDirs) {
    Get-ChildItem -Path . -Name $dir -Directory -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
        Remove-ItemSafely -Path $_ -Description "Special directory: $_"
    }
}

# Git cleanup
Write-Host "`n🔄 Git repository cleanup..." -ForegroundColor Magenta
if (-not $DryRun) {
    try {
        # Remove any accidentally tracked build artifacts
        git ls-files | Where-Object {
            $_ -match "bin/" -or $_ -match "obj/" -or $_ -match "TestResults/" -or
            $_ -match "\.tmp$" -or $_ -match "\.temp$" -or $_ -match "_temp" -or
            $_ -match "\.dll$" -or $_ -match "\.exe$"
        } | ForEach-Object {
            Write-Host "  🗑️  Removing tracked build artifact: $_" -ForegroundColor Yellow
            git rm --cached $_ -ErrorAction SilentlyContinue
        }

        # Run git garbage collection
        Write-Host "  🧹 Running git garbage collection..." -ForegroundColor Gray
        git gc --prune=now | Out-Null
        Write-Host "  ✅ Git cleanup complete" -ForegroundColor Green
    }
    catch {
        Write-Host "  ⚠️  Git cleanup encountered issues: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# Summary
Write-Host "`n📊 Cleanup Summary" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan
if ($DryRun) {
    Write-Host "Items that would be cleaned: $totalCleaned" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to actually clean files" -ForegroundColor Yellow
}
else {
    Write-Host "Items cleaned: $totalCleaned" -ForegroundColor Green
    Write-Host "Repository is now clean and follows .gitignore rules" -ForegroundColor Green
}

# Check current git status
Write-Host "`n📋 Current git status:" -ForegroundColor Cyan
git status --short
