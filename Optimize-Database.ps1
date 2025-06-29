# BusBuddy Database Optimization Runner
# PowerShell script to apply SQL Server Express optimizations
# Run with: pwsh -ExecutionPolicy Bypass -File "Optimize-Database.ps1"

param(
    [string]$ServerInstance = "localhost\SQLEXPRESS",
    [string]$Database = "BusBuddy",
    [switch]$SkipBackup,
    [switch]$SkipConnectionTest
)

Write-Host "BusBuddy Database Optimization Runner" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Target: $ServerInstance\$Database" -ForegroundColor Yellow
Write-Host ""

# Function to run SQL script and capture output
function Invoke-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$Description
    )

    Write-Host "Running: $Description" -ForegroundColor Green

    if (-not (Test-Path $ScriptPath)) {
        Write-Host "❌ Script not found: $ScriptPath" -ForegroundColor Red
        return $false
    }

    try {
        $outputFile = "$env:TEMP\sql_output_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

        $sqlcmdArgs = @(
            "-S", $ServerInstance
            "-d", $Database
            "-E"  # Windows Authentication
            "-i", $ScriptPath
            "-o", $outputFile
            "-W"  # Remove trailing spaces
        )

        & sqlcmd @sqlcmdArgs

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $Description completed successfully" -ForegroundColor Green

            # Show important output
            if (Test-Path $outputFile) {
                $output = Get-Content $outputFile -ErrorAction SilentlyContinue
                $importantLines = $output | Where-Object { $_ -match "✓|⚠|❌|Error|Warning|Complete" }
                if ($importantLines) {
                    $importantLines | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
                }
            }

            return $true
        } else {
            Write-Host "❌ $Description failed with exit code: $LASTEXITCODE" -ForegroundColor Red

            # Show error output
            if (Test-Path $outputFile) {
                $errors = Get-Content $outputFile | Where-Object { $_ -match "Error|Msg \d+" }
                $errors | ForEach-Object { Write-Host "   $_" -ForegroundColor Red }
            }

            return $false
        }
    }
    catch {
        Write-Host "❌ Exception running $Description`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        # Clean up temp file
        if (Test-Path $outputFile) {
            Remove-Item $outputFile -ErrorAction SilentlyContinue
        }
    }
}

# Function to test database connection
function Test-DatabaseConnection {
    Write-Host "Testing database connection..." -ForegroundColor Yellow

    try {
        $testQuery = "SELECT @@VERSION AS SQLVersion, DB_NAME() AS CurrentDatabase, GETDATE() AS CurrentTime"
        $outputFile = "$env:TEMP\connection_test.txt"

        & sqlcmd -S $ServerInstance -d $Database -E -Q $testQuery -o $outputFile -W

        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Database connection successful" -ForegroundColor Green

            if (Test-Path $outputFile) {
                $output = Get-Content $outputFile
                $versionLine = $output | Where-Object { $_ -match "Microsoft SQL Server" }
                if ($versionLine) {
                    Write-Host "   SQL Server: $($versionLine.Trim())" -ForegroundColor Gray
                }
            }

            return $true
        } else {
            Write-Host "❌ Database connection failed" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Connection test failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    finally {
        if (Test-Path "$env:TEMP\connection_test.txt") {
            Remove-Item "$env:TEMP\connection_test.txt" -ErrorAction SilentlyContinue
        }
    }
}

# Function to create backup
function New-DatabaseBackup {
    Write-Host "Creating database backup..." -ForegroundColor Yellow

    $backupDir = "C:\Backups\BusBuddy"
    $backupFile = "$backupDir\BusBuddy_PreOptimization_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"

    # Create backup directory if it doesn't exist
    if (-not (Test-Path $backupDir)) {
        try {
            New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
            Write-Host "   Created backup directory: $backupDir" -ForegroundColor Gray
        }
        catch {
            Write-Host "❌ Failed to create backup directory: $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }

    try {
        $backupQuery = "BACKUP DATABASE [$Database] TO DISK = '$backupFile' WITH FORMAT, INIT, COMPRESSION"
        & sqlcmd -S $ServerInstance -E -Q $backupQuery

        if ($LASTEXITCODE -eq 0 -and (Test-Path $backupFile)) {
            $size = [math]::Round((Get-Item $backupFile).Length / 1MB, 2)
            Write-Host "✅ Backup created: $backupFile ($size MB)" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ Backup failed or file not created" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ Backup failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main execution
Write-Host "Starting optimization process..." -ForegroundColor Cyan
Write-Host ""

# Step 1: Test connection
if (-not $SkipConnectionTest) {
    if (-not (Test-DatabaseConnection)) {
        Write-Host "❌ Aborting: Cannot connect to database" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

# Step 2: Create backup
if (-not $SkipBackup) {
    if (-not (New-DatabaseBackup)) {
        Write-Host "⚠️  Backup failed, but continuing with optimization..." -ForegroundColor Yellow
    }
    Write-Host ""
}

# Step 3: Run optimization scripts
$scripts = @(
    @{
        Path = ".\BusBuddy.Data\DatabaseOptimization.sql"
        Description = "SQL Server Express Optimization"
    },
    @{
        Path = ".\BusBuddy.Data\DatabaseHealthCheck.sql"
        Description = "Database Health Check"
    }
)

$successCount = 0
foreach ($script in $scripts) {
    if (Invoke-SqlScript -ScriptPath $script.Path -Description $script.Description) {
        $successCount++
    }
    Write-Host ""
}

# Step 4: Test application connectivity
Write-Host "Testing application connectivity..." -ForegroundColor Yellow
try {
    & dotnet test BusBuddy.Tests/BusBuddy.Tests.csproj --filter "Category=Database" --logger "console;verbosity=minimal" --nologo

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Application database tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Application database tests failed" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Failed to run application tests: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host ""
Write-Host "Optimization Summary" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
Write-Host "Scripts executed: $successCount/$($scripts.Count)" -ForegroundColor $(if ($successCount -eq $scripts.Count) { "Green" } else { "Yellow" })

if ($successCount -eq $scripts.Count) {
    Write-Host "🎉 All optimizations completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Monitor application performance" -ForegroundColor Gray
    Write-Host "2. Check Query Store for slow queries" -ForegroundColor Gray
    Write-Host "3. Set up regular maintenance schedule" -ForegroundColor Gray
    Write-Host "4. Consider implementing automated backups" -ForegroundColor Gray
} else {
    Write-Host "⚠️  Some optimizations may not have completed successfully" -ForegroundColor Yellow
    Write-Host "Review the output above for details" -ForegroundColor Gray
}

# Connection string recommendation
Write-Host ""
Write-Host "Enhanced Connection String:" -ForegroundColor Cyan
$connectionString = "Server=$ServerInstance;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=60;Encrypt=True;MultipleActiveResultSets=True;Max Pool Size=100;Min Pool Size=5;"
Write-Host $connectionString -ForegroundColor Green
