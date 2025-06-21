# Quick Date Column Fix Script for BusBuddy
# This script focuses only on critical tables with Date columns that may be causing SQL exceptions

$ErrorActionPreference = "Stop"
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"

Write-Host "🚌 BusBuddy Quick Date Column Fix" -ForegroundColor Cyan
Write-Host "Checking and fixing critical Date columns..." -ForegroundColor Cyan

try {
    # Connect to the database
    Write-Host "Connecting to database $database on $server..." -ForegroundColor Yellow

    # List of critical tables and their date columns to check
    $criticalColumns = @(
        @{Table="Activities"; Column="Date"},
        @{Table="ActivitySchedule"; Column="Date"},
        @{Table="Routes"; Column="Date"},
        @{Table="Maintenance"; Column="Date"},
        @{Table="TimeCards"; Column="Date"},
        @{Table="SchoolCalendar"; Column="CalendarDate"}
    )

    # Check each critical column
    foreach ($item in $criticalColumns) {
        $tableName = $item.Table
        $columnName = $item.Column

        Write-Host "`nChecking [$tableName].$columnName..." -ForegroundColor Cyan

        # Check if table exists
        $tableExistsQuery = "SELECT COUNT(*) AS TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName' AND TABLE_CATALOG = '$database'"
        $tableExists = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tableExistsQuery -TrustServerCertificate).TableCount -gt 0

        if (-not $tableExists) {
            Write-Host "⚠️ Table [$tableName] doesn't exist. Please run the main schema script first." -ForegroundColor Yellow
            continue
        }

        # Check if the column exists
        $columnExistsQuery = "SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$tableName' AND COLUMN_NAME = '$columnName' AND TABLE_CATALOG = '$database'"
        $columnInfo = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $columnExistsQuery -TrustServerCertificate

        if (($columnInfo | Measure-Object).Count -eq 0) {
            Write-Host "⚠️ Column [$columnName] doesn't exist in table [$tableName]. Adding it now..." -ForegroundColor Yellow
            Invoke-Sqlcmd -ServerInstance $server -Database $database -Query "ALTER TABLE [$tableName] ADD [$columnName] NVARCHAR(50)" -TrustServerCertificate
            Write-Host "✅ Added column [$columnName] to table [$tableName]." -ForegroundColor Green
            continue
        }

        $dataType = $columnInfo.DATA_TYPE
        $maxLength = $columnInfo.CHARACTER_MAXIMUM_LENGTH

        # Check if the column has the correct type
        if ($dataType -ne "nvarchar" -or ($maxLength -lt 50 -and $maxLength -ne -1)) {
            Write-Host "⚠️ Column [$columnName] has incorrect type [$dataType]($maxLength). Should be NVARCHAR(50). Fixing..." -ForegroundColor Red

            # Check if it's a primary key
            $pkQuery = "SELECT COUNT(*) AS IsPK FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '$tableName' AND COLUMN_NAME = '$columnName' AND TABLE_CATALOG = '$database'"
            $isPrimaryKey = (Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $pkQuery -TrustServerCertificate).IsPK -gt 0

            if ($isPrimaryKey) {
                Write-Host "❌ Cannot modify column [$columnName] because it is a PRIMARY KEY." -ForegroundColor Red
                continue
            }

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
        }
        else {
            Write-Host "✅ Column [$columnName] already has correct type NVARCHAR($maxLength)" -ForegroundColor Green
        }
    }

    Write-Host "`n✅ Quick date column check and fix complete." -ForegroundColor Green
    Write-Host "All critical date columns should now be properly formatted as NVARCHAR(50)." -ForegroundColor Cyan
    Write-Host "Run the application to verify all SQL exceptions are resolved." -ForegroundColor Yellow

} catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
