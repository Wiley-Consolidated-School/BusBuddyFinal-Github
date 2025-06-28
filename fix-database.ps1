# BusBuddy Database Fix Script
# This script diagnoses and repairs SQL Server database issues in BusBuddy

[CmdletBinding()]
param (
    [switch]$Force,
    [switch]$Backup,
    [switch]$Recreate
)

$ErrorActionPreference = "Stop"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$logFile = "database_repair_$timestamp.log"

function Write-LogMessage {
    param (
        [string]$Message,
        [string]$Color = "White",
        [switch]$NoConsole,
        [switch]$NoLog
    )

    if (-not $NoConsole) {
        Write-Host $Message -ForegroundColor $Color
    }

    if (-not $NoLog) {
        $logEntry = "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') - $Message"
        Add-Content -Path $logFile -Value $logEntry
    }
}

function Test-SqlConnection {
    param (
        [string]$ConnectionString,
        [string]$Name
    )

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        Write-LogMessage "✅ Connected to $Name database successfully!" -Color Green
        $connection.Close()
        return $true
    }
    catch {
        Write-LogMessage "❌ Failed to connect to $Name database: $_" -Color Red
        return $false
    }
}

# Display header
Write-LogMessage "=========================================================" -Color Cyan
Write-LogMessage "      BusBuddy Database Repair Tool (June 27, 2025)      " -Color Cyan
Write-LogMessage "=========================================================" -Color Cyan
Write-LogMessage "Log file: $logFile" -Color Gray

# Step 1: Check SQL Server service
Write-LogMessage "`n[Step 1] Checking SQL Server service status..." -Color Yellow

try {
    $sqlServices = Get-Service -Name MSSQL* -ErrorAction SilentlyContinue
    $sqlBrowserService = Get-Service -Name SQLBrowser -ErrorAction SilentlyContinue
}
catch {
    Write-LogMessage "⚠️ Warning: Limited permissions to check services: $_" -Color Yellow
    $sqlServices = @()
    $sqlBrowserService = $null
}

if ($sqlServices.Count -eq 0) {
    Write-LogMessage "❌ No SQL Server services found! SQL Server may not be installed." -Color Red
    Write-LogMessage "Please install SQL Server Express or verify your installation." -Color Yellow
    exit 1
}

foreach ($service in $sqlServices) {
    Write-LogMessage "Found SQL Server service: $($service.Name) - Status: $($service.Status)" -Color $(if ($service.Status -eq 'Running') { 'Green' } else { 'Yellow' })

    if ($service.Status -ne 'Running') {
        Write-LogMessage "Starting SQL Server service $($service.Name)..." -Color Yellow
        try {
            Start-Service $service.Name
            Start-Sleep -Seconds 2
            $service.Refresh()

            if ($service.Status -eq 'Running') {
                Write-LogMessage "✅ SQL Server service started successfully" -Color Green
            } else {
                Write-LogMessage "❌ Failed to start SQL Server service" -Color Red
                Write-LogMessage "Please start SQL Server manually through Services (services.msc)" -Color Yellow
            }
        }
        catch {
            Write-LogMessage "❌ Error starting SQL Server service: $_" -Color Red
        }
    }
}

# Check SQL Browser service (required for named instances)
if ($sqlBrowserService -and $sqlBrowserService.Status -ne 'Running') {
    Write-LogMessage "SQL Browser service is not running (required for named instances)" -Color Yellow
    try {
        Start-Service $sqlBrowserService.Name
        Start-Sleep -Seconds 2
        $sqlBrowserService.Refresh()

        if ($sqlBrowserService.Status -eq 'Running') {
            Write-LogMessage "✅ SQL Browser service started successfully" -Color Green
        } else {
            Write-LogMessage "⚠️ SQL Browser service could not be started" -Color Yellow
        }
    }
    catch {
        Write-LogMessage "⚠️ Error starting SQL Browser service: $_" -Color Yellow
    }
}

# Step 2: Check connection string in App.config
Write-LogMessage "`n[Step 2] Checking connection string in App.config..." -Color Yellow
$appConfigPath = Join-Path (Get-Location) "App.config"

if (Test-Path $appConfigPath) {
    try {
        $appConfig = [xml](Get-Content $appConfigPath)
        $connectionNode = $appConfig.SelectSingleNode("//connectionStrings/add[@name='DefaultConnection']")
        $connectionString = if ($connectionNode -ne $null) { $connectionNode.Attributes["connectionString"].Value } else { $null }

        if ($connectionString) {
            Write-LogMessage "Found connection string in App.config:" -Color Green
            Write-LogMessage "  $connectionString" -Color Gray

            # Parse connection string to get server and database
            if ($connectionString -match "Server=([^;]+);.*Database=([^;]+)") {
                $server = $matches[1]
                $database = $matches[2]
                Write-LogMessage "Server: $server, Database: $database" -Color White
            } else {
                Write-LogMessage "⚠️ Could not parse server and database from connection string" -Color Yellow
                $server = ".\SQLEXPRESS01"
                $database = "BusBuddy"
            }
        } else {
            Write-LogMessage "❌ DefaultConnection not found in App.config!" -Color Red
            $server = ".\SQLEXPRESS01"
            $database = "BusBuddy"
        }
    }
    catch {
        Write-LogMessage "❌ Error reading App.config: $_" -Color Red
        $server = ".\SQLEXPRESS01"
        $database = "BusBuddy"
    }
} else {
    Write-LogMessage "❌ App.config not found at $appConfigPath" -Color Red
    $server = ".\SQLEXPRESS01"
    $database = "BusBuddy"
}

# Step 3: Test SQL Server connectivity
Write-LogMessage "`n[Step 3] Testing SQL Server connectivity..." -Color Yellow

# Create connection strings
$masterConnectionString = "Server=$server;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=15;"
$dbConnectionString = "Server=$server;Database=$database;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=15;"

$masterConnected = Test-SqlConnection -ConnectionString $masterConnectionString -Name "master"
$dbConnected = Test-SqlConnection -ConnectionString $dbConnectionString -Name $database

# Step 4: Check database status and try to bring online if needed
Write-LogMessage "`n[Step 4] Checking database status..." -Color Yellow

if ($masterConnected -and -not $dbConnected) {
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
        $connection.Open()

        # Check if database exists
        $checkCommand = $connection.CreateCommand()
        $checkCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = '$database'"
        $dbExists = $checkCommand.ExecuteScalar() -gt 0

        if (-not $dbExists) {
            Write-LogMessage "Database $database does not exist - creating new database..." -Color Yellow
            $createCommand = $connection.CreateCommand()
            $createCommand.CommandText = "CREATE DATABASE $database"
            $createCommand.ExecuteNonQuery()
            Write-LogMessage "✅ Database $database created successfully!" -Color Green
        }
        else {
            # Check database state
            $stateCommand = $connection.CreateCommand()
            $stateCommand.CommandText = "SELECT state_desc FROM sys.databases WHERE name = '$database'"
            $state = $stateCommand.ExecuteScalar()

            Write-LogMessage "Database $database exists with state: $state" -Color Yellow

            if ($state -eq "OFFLINE") {
                Write-LogMessage "Database is OFFLINE - attempting to bring online..." -Color Yellow

                # Kill any existing connections to the database
                $killCommand = $connection.CreateCommand()
                $killCommand.CommandText = @"
DECLARE @kill varchar(8000) = '';
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID('$database');
EXEC(@kill);
"@
                $killCommand.ExecuteNonQuery()

                # Bring database online
                $onlineCommand = $connection.CreateCommand()
                $onlineCommand.CommandText = "ALTER DATABASE [$database] SET ONLINE WITH ROLLBACK IMMEDIATE"
                $onlineCommand.ExecuteNonQuery()
                Write-LogMessage "✅ Database brought online successfully!" -Color Green
            }
            elseif ($state -eq "SUSPECT") {
                Write-LogMessage "⚠️ Database is in SUSPECT state - attempting emergency repair..." -Color Yellow

                # Emergency repair mode
                $repairCommands = @(
                    "ALTER DATABASE [$database] SET EMERGENCY",
                    "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                    "DBCC CHECKDB ([$database], REPAIR_ALLOW_DATA_LOSS)",
                    "ALTER DATABASE [$database] SET MULTI_USER",
                    "ALTER DATABASE [$database] SET ONLINE"
                )

                foreach ($cmdText in $repairCommands) {
                    $cmd = $connection.CreateCommand()
                    $cmd.CommandText = $cmdText
                    try {
                        $cmd.ExecuteNonQuery()
                        Write-LogMessage "  $cmdText - Success" -Color Gray
                    }
                    catch {
                        Write-LogMessage "  $cmdText - Failed: $_" -Color Red
                    }
                }

                Write-LogMessage "✅ Emergency repair completed" -Color Green
            }
            elseif ($Recreate -or $Force) {
                Write-LogMessage "Recreating database as requested..." -Color Yellow

                if ($Backup) {
                    # Backup the database first
                    $backupPath = Join-Path (Get-Location) "Backups"
                    if (-not (Test-Path $backupPath)) {
                        New-Item -Path $backupPath -ItemType Directory | Out-Null
                    }

                    $backupFile = Join-Path $backupPath "$($database)_backup_$timestamp.bak"
                    $backupCommand = $connection.CreateCommand()
                    $backupCommand.CommandText = "BACKUP DATABASE [$database] TO DISK = '$backupFile'"
                    $backupCommand.ExecuteNonQuery()
                    Write-LogMessage "✅ Database backed up to $backupFile" -Color Green
                }

                # Drop and recreate
                $dropCommand = $connection.CreateCommand()
                $dropCommand.CommandText = "ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]"
                $dropCommand.ExecuteNonQuery()

                $createCommand = $connection.CreateCommand()
                $createCommand.CommandText = "CREATE DATABASE [$database]"
                $createCommand.ExecuteNonQuery()

                Write-LogMessage "✅ Database recreated successfully!" -Color Green
            }
        }

        $connection.Close()
    }
    catch {
        Write-LogMessage "❌ Error checking/fixing database: $_" -Color Red
    }
}

# Step 5: Run database schema script if available
Write-LogMessage "`n[Step 5] Checking for database scripts..." -Color Yellow

$schemaScriptPath = Join-Path (Get-Location) "BusBuddy.Data\DatabaseScript.SqlServer.sql"
$seedDataScriptPath = Join-Path (Get-Location) "BusBuddy.Data\TestSeedData.sql"

if (Test-Path $schemaScriptPath) {
    Write-LogMessage "Found database schema script - attempting to run..." -Color Yellow

    try {
        # Run the schema script
        $sqlcmdArgs = @(
            "-S", $server,
            "-d", $database,
            "-i", $schemaScriptPath
        )

        Write-LogMessage "Running: sqlcmd $($sqlcmdArgs -join ' ')" -Color Gray
        & sqlcmd $sqlcmdArgs

        if ($LASTEXITCODE -eq 0) {
            Write-LogMessage "✅ Database schema script executed successfully" -Color Green
        } else {
            Write-LogMessage "⚠️ Database schema script returned exit code: $LASTEXITCODE" -Color Yellow
        }

        # Run the seed data script if available
        if (Test-Path $seedDataScriptPath) {
            Write-LogMessage "Found seed data script - attempting to run..." -Color Yellow

            $sqlcmdArgs = @(
                "-S", $server,
                "-d", $database,
                "-i", $seedDataScriptPath
            )

            Write-LogMessage "Running: sqlcmd $($sqlcmdArgs -join ' ')" -Color Gray
            & sqlcmd $sqlcmdArgs

            if ($LASTEXITCODE -eq 0) {
                Write-LogMessage "✅ Seed data script executed successfully" -Color Green
            } else {
                Write-LogMessage "⚠️ Seed data script returned exit code: $LASTEXITCODE" -Color Yellow
            }
        }
    }
    catch {
        Write-LogMessage "❌ Error executing database scripts: $_" -Color Red
    }
}

# Step 6: Final connection test
Write-LogMessage "`n[Step 6] Testing final database connection..." -Color Yellow
$finalConnected = Test-SqlConnection -ConnectionString $dbConnectionString -Name $database

# Step 7: Final status and recommendations
Write-LogMessage "`n[Step 7] Final status and recommendations:" -Color Yellow

if ($finalConnected) {
    Write-LogMessage "`n✅ SUCCESS: Database is now online and accessible!" -Color Green
    Write-LogMessage "You can now run the BusBuddy application." -Color Cyan
}
else {
    Write-LogMessage "`n⚠️ WARNING: Database is still not accessible after repair attempts." -Color Yellow

    Write-LogMessage "`nTry these additional steps:" -Color Cyan
    Write-LogMessage "1. Run SQL Server Configuration Manager and verify SQL Server settings" -Color White
    Write-LogMessage "2. Check Windows Firewall settings for SQL Server exceptions" -Color White
    Write-LogMessage "3. Run the script with -Force to recreate the database: .\fix-database.ps1 -Force" -Color White
    Write-LogMessage "4. Restart your computer to clear any locked resources" -Color White
    Write-LogMessage "5. Consider uninstalling and reinstalling SQL Server Express" -Color White
}

Write-LogMessage "`nScript completed at $(Get-Date)" -Color Gray
