# Database Connection Fix Script for BusBuddy
# This script diagnoses and fixes database connectivity issues

Write-Host "=== BusBuddy Database Connection Repair Script ===" -ForegroundColor Cyan

# 1. Check if SQL Server is running
Write-Host "`n[Step 1] Checking SQL Server service status..." -ForegroundColor Yellow
$sqlService = Get-Service | Where-Object { $_.Name -like 'MSSQL*' }

if ($sqlService) {
    Write-Host "SQL Server service found: $($sqlService.Name) - Status: $($sqlService.Status)" -ForegroundColor Green

    if ($sqlService.Status -ne 'Running') {
        Write-Host "Starting SQL Server service..." -ForegroundColor Yellow
        try {
            Start-Service $sqlService.Name
            Write-Host "SQL Server service started successfully" -ForegroundColor Green
        }
        catch {
            Write-Host "Failed to start SQL Server service: $_" -ForegroundColor Red
            Write-Host "Please start SQL Server manually through Services (services.msc)" -ForegroundColor Yellow
        }
    }
}
else {
    Write-Host "No SQL Server service found. Please ensure SQL Server is installed." -ForegroundColor Red
}

# 2. Check connection string in App.config
Write-Host "`n[Step 2] Checking connection string in App.config..." -ForegroundColor Yellow
$appConfigPath = Join-Path (Get-Location) "App.config"

if (Test-Path $appConfigPath) {
    $appConfig = Get-Content $appConfigPath -Raw

    # Extract the connection string
    if ($appConfig -match '<add name="DefaultConnection"\s+connectionString="([^"]+)"\s+providerName="([^"]+)"\s+/>') {
        $connectionString = $matches[1]
        $providerName = $matches[2]

        Write-Host "Connection string found:" -ForegroundColor Green
        Write-Host "  $connectionString" -ForegroundColor Gray
        Write-Host "Provider: $providerName" -ForegroundColor Green

        # Parse connection string
        $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connectionString)

        Write-Host "`nConnection details:" -ForegroundColor Cyan
        Write-Host "  Server: $($builder.DataSource)" -ForegroundColor White
        Write-Host "  Database: $($builder.InitialCatalog)" -ForegroundColor White
        Write-Host "  Integrated Security: $($builder.IntegratedSecurity)" -ForegroundColor White
        Write-Host "  Timeout: $($builder.ConnectTimeout) seconds" -ForegroundColor White
    }
    else {
        Write-Host "DefaultConnection not found in App.config!" -ForegroundColor Red
    }
}
else {
    Write-Host "App.config not found at $appConfigPath" -ForegroundColor Red
}

# 3. Test database connectivity
Write-Host "`n[Step 3] Testing database connectivity..." -ForegroundColor Yellow

# Create the connection string to master database
$masterConnectionString = "Server=.\SQLEXPRESS01;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30"
$dbConnectionString = "Server=.\SQLEXPRESS01;Database=BusBuddy;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30"

function Test-SqlConnection {
    param (
        [string]$ConnectionString,
        [string]$Name
    )

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        Write-Host "✅ Connected to $Name database successfully!" -ForegroundColor Green
        $connection.Close()
        return $true
    }
    catch {
        Write-Host "❌ Failed to connect to $Name database: $_" -ForegroundColor Red
        return $false
    }
}

$masterConnected = Test-SqlConnection -ConnectionString $masterConnectionString -Name "master"
$dbConnected = Test-SqlConnection -ConnectionString $dbConnectionString -Name "BusBuddy"

# 4. Check if database exists and create if needed
if ($masterConnected -and -not $dbConnected) {
    Write-Host "`n[Step 4] Checking if BusBuddy database exists..." -ForegroundColor Yellow

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($masterConnectionString)
        $connection.Open()

        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = 'BusBuddy'"
        $dbExists = $command.ExecuteScalar() -gt 0

        if (-not $dbExists) {
            Write-Host "Creating BusBuddy database..." -ForegroundColor Yellow
            $createCommand = $connection.CreateCommand()
            $createCommand.CommandText = "CREATE DATABASE BusBuddy"
            $createCommand.ExecuteNonQuery()
            Write-Host "✅ BusBuddy database created successfully!" -ForegroundColor Green
        }
        else {
            Write-Host "BusBuddy database exists but connection failed." -ForegroundColor Yellow

            # Check if database is offline
            $offlineCommand = $connection.CreateCommand()
            $offlineCommand.CommandText = "SELECT state_desc FROM sys.databases WHERE name = 'BusBuddy'"
            $state = $offlineCommand.ExecuteScalar()

            if ($state -eq "OFFLINE") {
                Write-Host "BusBuddy database is OFFLINE. Attempting to bring online..." -ForegroundColor Yellow
                $onlineCommand = $connection.CreateCommand()
                $onlineCommand.CommandText = "ALTER DATABASE BusBuddy SET ONLINE"
                $onlineCommand.ExecuteNonQuery()
                Write-Host "✅ BusBuddy database brought online successfully!" -ForegroundColor Green
            }
        }

        $connection.Close()
    }
    catch {
        Write-Host "Error checking/creating database: $_" -ForegroundColor Red
    }
}

# 5. Report status and next steps
Write-Host "`n[Step 5] Status report and recommendations:" -ForegroundColor Yellow

if ($masterConnected -and $dbConnected) {
    Write-Host "`n✅ All database connections are working properly!" -ForegroundColor Green
    Write-Host "You can now run BusBuddy with 'dotnet run'" -ForegroundColor Cyan
}
elseif ($masterConnected) {
    Write-Host "`n⚠️ SQL Server is running but BusBuddy database connection failed." -ForegroundColor Yellow
    Write-Host "Recommendations:" -ForegroundColor Cyan
    Write-Host "1. Run 'dotnet build' to ensure all project files are compiled" -ForegroundColor White
    Write-Host "2. Try running the application with 'dotnet run'" -ForegroundColor White
    Write-Host "3. If issues persist, try restarting SQL Server service" -ForegroundColor White
    Write-Host "4. Check if any database locks exist using SQL Server Management Studio" -ForegroundColor White
}
else {
    Write-Host "`n❌ SQL Server connection failed completely." -ForegroundColor Red
    Write-Host "Recommendations:" -ForegroundColor Cyan
    Write-Host "1. Ensure SQL Server is installed and running" -ForegroundColor White
    Write-Host "2. Check Windows Services to confirm SQL Server service status" -ForegroundColor White
    Write-Host "3. Verify your SQL Server instance name (default is .\SQLEXPRESS01)" -ForegroundColor White
    Write-Host "4. If using a named instance, ensure SQL Browser service is running" -ForegroundColor White
    Write-Host "5. Check if Windows firewall is blocking SQL Server ports" -ForegroundColor White
}

Write-Host "`nScript completed. Press any key to exit..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
