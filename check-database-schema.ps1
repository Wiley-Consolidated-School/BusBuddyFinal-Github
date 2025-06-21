# Check database structure without making changes
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"

Write-Host "Checking database structure for $database on $server..." -ForegroundColor Cyan

# First check if database exists
$dbCheckQuery = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = '$database'"
$dbExists = (Invoke-Sqlcmd -ServerInstance $server -Query $dbCheckQuery -TrustServerCertificate).Column1 -gt 0

if (-not $dbExists) {
    Write-Host "Database $database does not exist!" -ForegroundColor Red
} else {
    Write-Host "Database $database exists." -ForegroundColor Green

    # Get list of tables
    $tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = '$database'"
    $tables = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $tablesQuery -TrustServerCertificate

    Write-Host "Tables in database:" -ForegroundColor Cyan
    foreach ($table in $tables) {
        Write-Host "- $($table.TABLE_NAME)" -ForegroundColor White

        # Get columns for each table
        $columnsQuery = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$($table.TABLE_NAME)' AND TABLE_CATALOG = '$database' ORDER BY ORDINAL_POSITION"
        $columns = Invoke-Sqlcmd -ServerInstance $server -Database $database -Query $columnsQuery -TrustServerCertificate

        foreach ($column in $columns) {
            $lengthInfo = if ($column.CHARACTER_MAXIMUM_LENGTH -eq -1) { "MAX" } elseif ($column.CHARACTER_MAXIMUM_LENGTH) { $column.CHARACTER_MAXIMUM_LENGTH } else { "" }
            Write-Host "  - $($column.COLUMN_NAME): $($column.DATA_TYPE)$(if ($lengthInfo) { "($lengthInfo)" })" -ForegroundColor Gray
        }
    }
}

Write-Host "Database check complete." -ForegroundColor Green

Write-Host "Press any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
