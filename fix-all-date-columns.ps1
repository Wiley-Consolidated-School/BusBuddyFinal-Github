# Comprehensive Date Column Fix Script for BusBuddy
# This script thoroughly checks and fixes ALL date columns across ALL tables

$ErrorActionPreference = "Stop"
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"

Write-Host "🚌 BusBuddy Comprehensive Date Column Fix" -ForegroundColor Cyan
Write-Host "Checking and fixing ALL date columns in ALL tables..." -ForegroundColor Cyan

try {
    # Connect to the database
    Write-Host "Connecting to database $database on $server..." -ForegroundColor Yellow

    # Get all tables
    $tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = '$database'"
    $tables = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tablesQuery -TrustServerCertificate

    Write-Host "Found $(($tables | Measure-Object).Count) tables. Checking each for Date columns..." -ForegroundColor Yellow

    # Define a list of column names that might store dates
    $dateColumnPatterns = @(
        "Date",
        "%Date%",
        "Created%",
        "Modified%",
        "Updated%",
        "%Time%",
        "Calendar%"
    )
      # Create a WHERE clause for the column query
    $whereClause = $dateColumnPatterns | ForEach-Object { "COLUMN_NAME LIKE '$_'" } | Join-String -Separator " OR "

    # Track all date columns found and fixed
    $allDateColumns = @()
    $fixedColumns = @()

    # Check each table
    foreach ($table in $tables) {
        $tableName = $table.TABLE_NAME
        Write-Host "`nChecking table [$tableName]..." -ForegroundColor Cyan
          # Get all columns that might be date columns
        $dateColumnsQuery = @"
        SELECT
            c.COLUMN_NAME as COLUMN_NAME,
            c.DATA_TYPE as DATA_TYPE,
            c.CHARACTER_MAXIMUM_LENGTH as CHARACTER_MAXIMUM_LENGTH,
            CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey
        FROM INFORMATION_SCHEMA.COLUMNS c
        LEFT JOIN (
            SELECT ku.TABLE_CATALOG, ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
        ) pk
            ON c.TABLE_CATALOG = pk.TABLE_CATALOG
            AND c.TABLE_SCHEMA = pk.TABLE_SCHEMA
            AND c.TABLE_NAME = pk.TABLE_NAME
            AND c.COLUMN_NAME = pk.COLUMN_NAME
        WHERE c.TABLE_NAME = '$tableName'
        AND ($whereClause)
        AND c.TABLE_CATALOG = '$database'
"@
        $dateColumns = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $dateColumnsQuery -TrustServerCertificate

        if (($dateColumns | Measure-Object).Count -eq 0) {
            Write-Host "  No potential date columns found in [$tableName]" -ForegroundColor DarkGray
            continue
        }

        Write-Host "  Found $(($dateColumns | Measure-Object).Count) potential date columns in [$tableName]" -ForegroundColor Yellow

        foreach ($column in $dateColumns) {
            $columnName = $column.COLUMN_NAME
            $dataType = $column.DATA_TYPE
            $maxLength = $column.CHARACTER_MAXIMUM_LENGTH
            $isPrimaryKey = $column.IsPrimaryKey -eq 1

            $allDateColumns += [PSCustomObject]@{
                Table = $tableName
                Column = $columnName
                Type = $dataType
                MaxLength = $maxLength
                IsPrimaryKey = $isPrimaryKey
            }

            Write-Host "  Checking column [$columnName] with type [$dataType]" -ForegroundColor Yellow

            # Skip primary key columns
            if ($isPrimaryKey) {
                Write-Host "  ⚠️ Column [$columnName] is a PRIMARY KEY. Skipping conversion." -ForegroundColor Red
                continue
            }

            # Check if column is not nvarchar or not long enough for date strings
            if ($dataType -ne "nvarchar" -or ($maxLength -lt 50 -and $maxLength -ne -1)) {
                Write-Host "  ⚠️ Column [$columnName] has type [$dataType]($maxLength). Converting to NVARCHAR(50)..." -ForegroundColor Red

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

                $fixedColumns += [PSCustomObject]@{
                    Table = $tableName
                    Column = $columnName
                    OldType = $dataType
                    NewType = "nvarchar(50)"
                }
            }
            else {
                Write-Host "  ✅ Column [$columnName] already has correct type NVARCHAR($maxLength)" -ForegroundColor Green
            }
        }
    }

    # Now specifically check for key tables that must have a Date column
    $expectedDateColumns = @(
        @{Table="Activities"; Column="Date"},
        @{Table="ActivitySchedule"; Column="Date"},
        @{Table="Routes"; Column="Date"},
        @{Table="Maintenance"; Column="Date"},
        @{Table="TimeCards"; Column="Date"},
        @{Table="Fuel"; Column="FuelDate"},
        @{Table="Vehicles"; Column="DateLastInspection"},
        @{Table="SchoolCalendar"; Column="CalendarDate"}
    )

    Write-Host "`nChecking for required date columns in key tables..." -ForegroundColor Cyan

    foreach ($item in $expectedDateColumns) {
        $tableName = $item.Table
        $columnName = $item.Column

        # Check if table exists
        $tableExistsQuery = "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName' AND TABLE_CATALOG = '$database'"
        $tableExists = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tableExistsQuery -TrustServerCertificate).TableCount -gt 0

        if (-not $tableExists) {
            Write-Host "⚠️ Table [$tableName] doesn't exist and should be created with the main schema script" -ForegroundColor Yellow
            continue
        }

        # Check if Date column exists
        $dateColumnQuery = "SELECT COUNT(*) AS ColumnCount FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$tableName' AND COLUMN_NAME = '$columnName' AND TABLE_CATALOG = '$database'"
        $dateColumnExists = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $dateColumnQuery -TrustServerCertificate).ColumnCount -gt 0

        if (-not $dateColumnExists) {
            Write-Host "⚠️ Table [$tableName] is missing the required '$columnName' column. Adding it now..." -ForegroundColor Red
            Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "ALTER TABLE [$tableName] ADD [$columnName] NVARCHAR(50)" -TrustServerCertificate
            Write-Host "✅ Added '$columnName' column to [$tableName]" -ForegroundColor Green

            $fixedColumns += [PSCustomObject]@{
                Table = $tableName
                Column = $columnName
                OldType = "MISSING"
                NewType = "nvarchar(50)"
            }
        }
        else {
            # Check if the existing column has the correct type
            $columnTypeQuery = "SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$tableName' AND COLUMN_NAME = '$columnName' AND TABLE_CATALOG = '$database'"
            $columnInfo = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $columnTypeQuery -TrustServerCertificate
            $dataType = $columnInfo.DATA_TYPE
            $maxLength = $columnInfo.CHARACTER_MAXIMUM_LENGTH

            if ($dataType -ne "nvarchar" -or ($maxLength -lt 50 -and $maxLength -ne -1)) {
                Write-Host "⚠️ Column [$columnName] in [$tableName] has incorrect type [$dataType]($maxLength). Converting to NVARCHAR(50)..." -ForegroundColor Red

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

                Write-Host "✅ Successfully converted column [$columnName] to NVARCHAR(50)" -ForegroundColor Green

                $fixedColumns += [PSCustomObject]@{
                    Table = $tableName
                    Column = $columnName
                    OldType = "$dataType($maxLength)"
                    NewType = "nvarchar(50)"
                }
            }
            else {
                Write-Host "✅ Column [$columnName] in [$tableName] already has correct type NVARCHAR($maxLength)" -ForegroundColor Green
            }
        }
    }

    # Display summary report
    Write-Host "`n📊 Date Column Fix Summary Report" -ForegroundColor Cyan
    Write-Host "Total tables checked: $(($tables | Measure-Object).Count)" -ForegroundColor Cyan
    Write-Host "Total date columns found: $(($allDateColumns | Measure-Object).Count)" -ForegroundColor Cyan
    Write-Host "Columns fixed: $(($fixedColumns | Measure-Object).Count)" -ForegroundColor Cyan

    if (($fixedColumns | Measure-Object).Count -gt 0) {
        Write-Host "`nDetails of fixed columns:" -ForegroundColor Yellow
        $fixedColumns | Format-Table -AutoSize
    }

    Write-Host "`n✅ Comprehensive date column check and fix complete." -ForegroundColor Green
    Write-Host "All date columns should now be properly formatted as NVARCHAR(50)." -ForegroundColor Cyan

} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
