# Database Backup Script with Enhanced sqlcmd Validation
# Fixes System.InvalidOperationException issues with sqlcmd process execution
# Author: GitHub Copilot
# Date: June 27, 2025

param(
    [string]$ServerName = ".\SQLEXPRESS01",
    [string]$DatabaseName = "BusBuddy",
    [string]$BackupPath = "C:\Backups"
)

# Ensure backup directory exists
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    Write-Host "✅ Created backup directory: $BackupPath" -ForegroundColor Green
}

# Generate timestamped backup filename
$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupFile = Join-Path $BackupPath "BusBuddy_$timestamp.bak"

Write-Host "🔍 Starting database backup process..." -ForegroundColor Cyan
Write-Host "   Server: $ServerName" -ForegroundColor Gray
Write-Host "   Database: $DatabaseName" -ForegroundColor Gray
Write-Host "   Backup file: $backupFile" -ForegroundColor Gray

try {
    # CRITICAL FIX: Test sqlcmd availability and connection first
    Write-Host "🔍 Validating sqlcmd and database connection..." -ForegroundColor Cyan

    $testQuery = "SELECT 1 as TestConnection"
    $testProcess = Start-Process -FilePath "sqlcmd" -ArgumentList @(
        "-S", $ServerName,
        "-Q", $testQuery,
        "-E"
    ) -Wait -PassThru -WindowStyle Hidden -RedirectStandardOutput "testoutput.tmp" -RedirectStandardError "testerror.tmp"

    # Check if sqlcmd process completed successfully
    if ($testProcess.ExitCode -ne 0) {
        $errorContent = Get-Content "testerror.tmp" -ErrorAction SilentlyContinue
        Write-Host "❌ sqlcmd connection test failed (Exit code: $($testProcess.ExitCode))" -ForegroundColor Red
        Write-Host "   Error: $errorContent" -ForegroundColor Red
        throw "Database connection validation failed"
    }

    Write-Host "✅ sqlcmd connection validated successfully" -ForegroundColor Green

    # Clean up test files
    Remove-Item "testoutput.tmp", "testerror.tmp" -ErrorAction SilentlyContinue

    # ENHANCED: Execute backup with proper process validation
    Write-Host "💾 Executing database backup..." -ForegroundColor Cyan

    $backupQuery = "BACKUP DATABASE [$DatabaseName] TO DISK = '$backupFile' WITH FORMAT, INIT, NAME = 'BusBuddy Full Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

    # Start backup process with proper error handling
    $backupProcess = Start-Process -FilePath "sqlcmd" -ArgumentList @(
        "-S", $ServerName,
        "-Q", $backupQuery,
        "-E"
    ) -Wait -PassThru -WindowStyle Hidden -RedirectStandardOutput "backupoutput.tmp" -RedirectStandardError "backuperror.tmp"

    # CRITICAL FIX: Validate process completion before accessing results
    if ($null -eq $backupProcess) {
        throw "Failed to start sqlcmd backup process"
    }

    if ($backupProcess.ExitCode -eq 0) {
        # Verify backup file was created
        if (Test-Path $backupFile) {
            $backupSize = (Get-Item $backupFile).Length / 1MB
            Write-Host "✅ Database backup completed successfully!" -ForegroundColor Green
            Write-Host "   Backup file: $backupFile" -ForegroundColor Gray
            Write-Host "   Size: $([math]::Round($backupSize, 2)) MB" -ForegroundColor Gray
        } else {
            Write-Host "⚠️ Backup process completed but file not found: $backupFile" -ForegroundColor Yellow
        }
    } else {
        $errorContent = Get-Content "backuperror.tmp" -ErrorAction SilentlyContinue
        Write-Host "❌ Database backup failed (Exit code: $($backupProcess.ExitCode))" -ForegroundColor Red
        Write-Host "   Error: $errorContent" -ForegroundColor Red
        throw "Backup process failed"
    }

    # Clean up output files
    Remove-Item "backupoutput.tmp", "backuperror.tmp" -ErrorAction SilentlyContinue

} catch [System.InvalidOperationException] {
    Write-Host "❌ InvalidOperationException during backup process: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   This usually indicates a problem with process execution or access" -ForegroundColor Yellow
    Write-Host "   Ensure SQL Server is running and accessible" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Backup failed with error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Exception type: $($_.Exception.GetType().Name)" -ForegroundColor Gray
} finally {
    # Clean up any remaining temporary files
    Remove-Item "testoutput.tmp", "testerror.tmp", "backupoutput.tmp", "backuperror.tmp" -ErrorAction SilentlyContinue
}

Write-Host "🏁 Backup process completed." -ForegroundColor Cyan
