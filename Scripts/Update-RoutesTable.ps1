## Update Routes Table - Add Date Column
## This script executes the SQL script to add the Date column to the Routes table if it doesn't exist

# Get the connection string from the app config
$configPath = Join-Path $PSScriptRoot "..\App.config"
$config = [xml](Get-Content $configPath)
$connectionString = $config.configuration.connectionStrings.add | Where-Object { $_.name -eq "DefaultConnection" } | Select-Object -ExpandProperty connectionString

# Path to the SQL script
$sqlScriptPath = Join-Path $PSScriptRoot "update_routes_table.sql"
$sqlScript = Get-Content -Path $sqlScriptPath -Raw

# Execute the SQL script
try {
    Write-Host "📊 Connecting to database..." -ForegroundColor Cyan
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()

    Write-Host "🔧 Executing SQL script to update Routes table..." -ForegroundColor Yellow
    $command = New-Object System.Data.SqlClient.SqlCommand
    $command.Connection = $connection
    $command.CommandText = $sqlScript

    # Execute and capture output
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter
    $adapter.SelectCommand = $command
    $dataSet = New-Object System.Data.DataSet
    $adapter.Fill($dataSet) | Out-Null

    Write-Host "✅ SQL script executed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error executing SQL script: $_" -ForegroundColor Red
}
finally {
    if ($connection -and $connection.State -eq [System.Data.ConnectionState]::Open) {
        $connection.Close()
        Write-Host "📊 Database connection closed." -ForegroundColor Cyan
    }
}
