# Safe database schema analyzer - non-destructive
# This script will analyze the BusBuddy_Test database structure without making changes

$ErrorActionPreference = "Stop"

Write-Host "🔍 Analyzing BusBuddy_Test database schema..." -ForegroundColor Cyan

# Get connection string from App.config
$appConfigPath = Join-Path $PSScriptRoot "App.config"
[xml]$configXml = Get-Content $appConfigPath

# Extract the connection string for BusBuddy_Test
$connectionString = $configXml.configuration.connectionStrings.add |
    Where-Object { $_.name -eq "TestConnection" } |
    Select-Object -ExpandProperty connectionString

# Extract server and database from connection string
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"

# Check if database exists
try {
    $checkDbQuery = "SELECT name FROM sys.databases WHERE name = 'BusBuddy_Test'"
    $result = Invoke-Sqlcmd -ServerInstance $server -Query $checkDbQuery -ErrorAction Stop

    if ($null -eq $result -or $result.Count -eq 0) {
        Write-Host "❌ Database BusBuddy_Test does not exist!" -ForegroundColor Red
        exit 1
    }

    Write-Host "✅ Database BusBuddy_Test exists" -ForegroundColor Green
} catch {
    Write-Host "❌ Error checking database: $_" -ForegroundColor Red
    exit 1
}

# Check table structure
$tables = @("Vehicles", "Drivers", "Routes", "Maintenance", "Fuel", "Activities", "ActivitySchedules", "TimeCards")

foreach ($table in $tables) {
    try {
        $query = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$table' AND TABLE_SCHEMA = 'dbo'"
        $tableExists = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $query -ErrorAction Stop

        if ($null -eq $tableExists -or $tableExists.Count -eq 0) {
            Write-Host "⚠️ Table [$table] does not exist" -ForegroundColor Yellow
            continue
        }

        Write-Host "✅ Table [$table] exists" -ForegroundColor Green

        # Get column information
        $columnsQuery = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$table'"
        $columns = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $columnsQuery -ErrorAction Stop

        Write-Host "   Columns in [$table]:" -ForegroundColor Cyan
        foreach ($column in $columns) {
            $length = if ($column.CHARACTER_MAXIMUM_LENGTH -ne -1) { "(" + $column.CHARACTER_MAXIMUM_LENGTH + ")" } else { "" }
            Write-Host "   - $($column.COLUMN_NAME): $($column.DATA_TYPE)$length" -ForegroundColor Gray
        }

        # Special check for "Date" column if this is the Activities table
        if ($table -eq "Activities" -and ($columns | Where-Object { $_.COLUMN_NAME -eq "Date" })) {
            Write-Host "   🔎 'Date' column found in Activities table with type: $(($columns | Where-Object { $_.COLUMN_NAME -eq "Date" }).DATA_TYPE)" -ForegroundColor Green
        } elseif ($table -eq "Activities") {
            Write-Host "   ❌ 'Date' column is MISSING from Activities table!" -ForegroundColor Red
        }

    } catch {
        Write-Host "❌ Error checking table [$table]: $_" -ForegroundColor Red
    }
}

Write-Host "`n📊 Database analysis complete" -ForegroundColor Cyan
