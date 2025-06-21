# Fix Date Column Script for BusBuddy
# This script specifically focuses on fixing 'Date' column issues in all tables

$ErrorActionPreference = "Stop"
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"

Write-Host "🚌 BusBuddy Date Column Fix" -ForegroundColor Cyan
Write-Host "Checking and fixing Date columns in all tables..." -ForegroundColor Cyan

try {
    # Connect to the database
    Write-Host "Connecting to database $database on $server..." -ForegroundColor Yellow

    # Get all tables
    $tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = '$database'"
    $tables = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tablesQuery -TrustServerCertificate

    Write-Host "Found $(($tables | Measure-Object).Count) tables. Checking each for Date columns..." -ForegroundColor Yellow

    # Check each table for Date or date-like columns
    foreach ($table in $tables) {
        $tableName = $table.TABLE_NAME
        Write-Host "Checking table [$tableName]..." -ForegroundColor Cyan

        # Get all columns containing 'date' in their name (case insensitive)
        $dateColumnsQuery = @"
        SELECT COLUMN_NAME, DATA_TYPE
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = '$tableName'
        AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME = 'Date')
        AND TABLE_CATALOG = '$database'
"@
        $dateColumns = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $dateColumnsQuery -TrustServerCertificate

        if (($dateColumns | Measure-Object).Count -eq 0) {
            Write-Host "  No date columns found in [$tableName]" -ForegroundColor DarkGray
            continue
        }

        foreach ($column in $dateColumns) {
            $columnName = $column.COLUMN_NAME
            $dataType = $column.DATA_TYPE

            Write-Host "  Found column [$columnName] with type [$dataType]" -ForegroundColor Yellow

            # Check if column is not nvarchar and needs to be converted
            if ($dataType -ne "nvarchar") {
                Write-Host "  ⚠️ Column [$columnName] has incorrect type [$dataType]. Converting to NVARCHAR(50)..." -ForegroundColor Red

                # Create temporary column, copy data, swap
                $tempColumnName = "${columnName}_temp"

                # Create temp column
                Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "ALTER TABLE [$tableName] ADD [$tempColumnName] NVARCHAR(50)" -TrustServerCertificate

                # Copy data with conversion
                Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "UPDATE [$tableName] SET [$tempColumnName] = CONVERT(NVARCHAR(50), [$columnName], 120)" -TrustServerCertificate

                # Drop original column
                Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "ALTER TABLE [$tableName] DROP COLUMN [$columnName]" -TrustServerCertificate

                # Rename temp column to original name
                Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "EXEC sp_rename '[$tableName].[$tempColumnName]', '$columnName', 'COLUMN'" -TrustServerCertificate

                Write-Host "  ✅ Successfully converted column [$columnName] to NVARCHAR(50)" -ForegroundColor Green
            }
            else {
                Write-Host "  ✅ Column [$columnName] already has correct type NVARCHAR" -ForegroundColor Green
            }
        }
    }

    # Now specifically check for tables that should have a Date column but don't
    $expectedTables = @("Activities", "ActivitySchedule", "Routes", "Maintenance")

    foreach ($tableName in $expectedTables) {
        # Check if table exists
        $tableExistsQuery = "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName' AND TABLE_CATALOG = '$database'"
        $tableExists = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tableExistsQuery -TrustServerCertificate).TableCount -gt 0

        if (-not $tableExists) {
            Write-Host "⚠️ Table [$tableName] doesn't exist and should be created with the main schema script" -ForegroundColor Yellow
            continue
        }

        # Check if Date column exists
        $dateColumnQuery = "SELECT COUNT(*) AS ColumnCount FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$tableName' AND COLUMN_NAME = 'Date' AND TABLE_CATALOG = '$database'"
        $dateColumnExists = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $dateColumnQuery -TrustServerCertificate).ColumnCount -gt 0

        if (-not $dateColumnExists) {
            Write-Host "⚠️ Table [$tableName] is missing the 'Date' column. Adding it now..." -ForegroundColor Red
            Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "ALTER TABLE [$tableName] ADD [Date] NVARCHAR(50)" -TrustServerCertificate
            Write-Host "✅ Added 'Date' column to [$tableName]" -ForegroundColor Green
        }
    }

    Write-Host "`n✅ Date column check and fix complete." -ForegroundColor Green
    Write-Host "All date columns should now be properly formatted as NVARCHAR(50)." -ForegroundColor Cyan

} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
