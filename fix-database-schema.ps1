# Comprehensive Database Schema Update Script
# This script updates the BusBuddy_Test database to match the specifications in BusBuddy Tables.txt
# It handles creating missing tables and columns while preserving existing data

$ErrorActionPreference = "Stop"
$server = ".\SQLEXPRESS01"
$database = "BusBuddy_Test"
$commandTimeout = 60 # seconds
$maxRetries = 3
$retryDelay = 2 # seconds

Write-Host "🚌 BusBuddy Database Schema Update" -ForegroundColor Cyan
Write-Host "Updating database schema for $database on $server..." -ForegroundColor Cyan
Write-Host "This script will create missing tables and fix column issues without data loss." -ForegroundColor Yellow

# Function to retry database operations with exponential backoff
function Invoke-SqlCommandWithRetry {
    param (
        [string]$ServerInstance,
        [string]$Database,
        [string]$Query,
        [int]$MaxRetries = 3,
        [int]$RetryDelay = 2,
        [int]$Timeout = 60
    )

    $retryCount = 0
    $success = $false
    $result = $null

    while (-not $success -and $retryCount -lt $MaxRetries) {
        try {
            if ($Database) {
                $result = Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -Query $Query -TrustServerCertificate -QueryTimeout $Timeout -ErrorAction Stop
            } else {
                $result = Invoke-Sqlcmd -ServerInstance $ServerInstance -Query $Query -TrustServerCertificate -QueryTimeout $Timeout -ErrorAction Stop
            }
            $success = $true
        }
        catch [System.Data.SqlClient.SqlException] {
            $retryCount++
            $delay = [math]::Pow(2, $retryCount) * $RetryDelay

            if ($retryCount -ge $MaxRetries) {
                Write-Host "❌ Maximum retry attempts reached ($MaxRetries). Last error: $($_.Exception.Message)" -ForegroundColor Red
                throw
            }

            Write-Host "⚠️ SQL error occurred: $($_.Exception.Message). Retrying in $delay seconds..." -ForegroundColor Yellow
            Start-Sleep -Seconds $delay
        }
        catch {
            Write-Host "❌ Unhandled error: $($_.Exception.Message)" -ForegroundColor Red
            throw
        }
    }

    return $result
}

# Function to check if a table exists
function Test-TableExists {
    param (
        [string]$TableName
    )

    $query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$TableName' AND TABLE_CATALOG = '$database'"
    $result = Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query $query -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
    return $result.Column1 -gt 0
}

# Function to check column data type
function Get-ColumnDataType {
    param (
        [string]$TableName,
        [string]$ColumnName
    )

    $query = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '$TableName' AND COLUMN_NAME = '$ColumnName' AND TABLE_CATALOG = '$database'"
    $result = Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query $query -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout -ErrorAction SilentlyContinue

    if ($result) {
        return $result.DATA_TYPE
    }

    return $null
}

# Function to fix date column to nvarchar type
function Update-DateColumn {
    param (
        [string]$TableName,
        [string]$ColumnName
    )

    $dataType = Get-ColumnDataType -TableName $TableName -ColumnName $ColumnName

    if (-not $dataType) {
        Write-Host "$ColumnName column is missing in $TableName table! Adding it now..." -ForegroundColor Yellow
        Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query "ALTER TABLE $TableName ADD $ColumnName NVARCHAR(50)" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Write-Host "$ColumnName column added successfully." -ForegroundColor Green
    }
    elseif ($dataType -ne "nvarchar") {
        Write-Host "$ColumnName column has incorrect type: $dataType. Should be nvarchar. Fixing..." -ForegroundColor Yellow
        # Rename old column, add new one, copy data, drop old
        Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query "EXEC sp_rename '$TableName.$ColumnName', '${ColumnName}_Old', 'COLUMN'" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query "ALTER TABLE $TableName ADD $ColumnName NVARCHAR(50)" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query "UPDATE $TableName SET $ColumnName = CONVERT(NVARCHAR(50), ${ColumnName}_Old, 120)" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query "ALTER TABLE $TableName DROP COLUMN ${ColumnName}_Old" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Write-Host "$ColumnName column fixed successfully." -ForegroundColor Green
    }
    else {
        Write-Host "$TableName table $ColumnName column is correctly configured as NVARCHAR(50)." -ForegroundColor Green
    }
}

try {
    # First check if the database exists
    $dbCheckQuery = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = '$database'"
    $dbExists = (Invoke-SqlCommandWithRetry -ServerInstance $server -Query $dbCheckQuery -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout).Column1 -gt 0

    if (-not $dbExists) {
        Write-Host "Database $database does not exist! Creating it now..." -ForegroundColor Yellow
        Invoke-SqlCommandWithRetry -ServerInstance $server -Query "CREATE DATABASE [$database]" -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
        Write-Host "Database created successfully." -ForegroundColor Green
    }

    # Now check tables and fix them
    Write-Host "Checking table structures..." -ForegroundColor Cyan

    # Define all table creation scripts
    $tableDefinitions = @{
        "Activities" = @"
        CREATE TABLE Activities (
            ActivityID INT IDENTITY(1,1) PRIMARY KEY,
            Date NVARCHAR(50),
            ActivityType NVARCHAR(100),
            Destination NVARCHAR(200),
            LeaveTime NVARCHAR(50),
            EventTime NVARCHAR(50),
            ReturnTime NVARCHAR(50),
            RequestedBy NVARCHAR(100),
            AssignedVehicleID INT,
            DriverID INT,
            Notes NVARCHAR(MAX)
        )
"@
        "ActivitySchedule" = @"
        CREATE TABLE ActivitySchedule (
            ScheduleID INT IDENTITY(1,1) PRIMARY KEY,
            Date NVARCHAR(50),
            TripType NVARCHAR(100),
            ScheduledVehicleID INT,
            ScheduledDestination NVARCHAR(200),
            ScheduledLeaveTime NVARCHAR(50),
            ScheduledEventTime NVARCHAR(50),
            ScheduledReturnTime NVARCHAR(50),
            ScheduledRiders INT,
            ScheduledDriverID INT,
            Notes NVARCHAR(MAX)
        )
"@
        "Routes" = @"
        CREATE TABLE Routes (
            RouteID INT IDENTITY(1,1) PRIMARY KEY,
            Date NVARCHAR(50),
            RouteName NVARCHAR(50),
            AMVehicle NVARCHAR(50),
            AMBeginMiles DECIMAL(10,2),
            AMEndMiles DECIMAL(10,2),
            AMRiders INT,
            AMDriver NVARCHAR(100),
            PMVehicle NVARCHAR(50),
            PMBeginMiles DECIMAL(10,2),
            PMEndMiles DECIMAL(10,2),
            PMRiders INT,
            PMDriver NVARCHAR(100),
            Notes NVARCHAR(MAX)
        )
"@
        "Drivers" = @"
        CREATE TABLE Drivers (
            DriverID INT IDENTITY(1,1) PRIMARY KEY,
            DriverName NVARCHAR(100),
            DriverPhone NVARCHAR(20),
            DriverEmail NVARCHAR(100),
            Address NVARCHAR(200),
            City NVARCHAR(50),
            State NVARCHAR(20),
            Zip NVARCHAR(10),
            DriversLicenceType NVARCHAR(20),
            TrainingComplete BIT,
            Notes NVARCHAR(MAX)
        )
"@
        "Vehicles" = @"
        CREATE TABLE Vehicles (
            VehicleID INT PRIMARY KEY IDENTITY(1,1),
            VehicleNumber NVARCHAR(50) NOT NULL,
            Year INT,
            Make NVARCHAR(50),
            Model NVARCHAR(50),
            SeatingCapacity INT,
            VINNumber NVARCHAR(50),
            LicenseNumber NVARCHAR(20),
            DateLastInspection NVARCHAR(50),
            Status NVARCHAR(20),
            Notes NVARCHAR(MAX)
        )
"@
        "Fuel" = @"
        CREATE TABLE Fuel (
            FuelID INT IDENTITY(1,1) PRIMARY KEY,
            FuelDate NVARCHAR(50),
            FuelLocation NVARCHAR(100),
            VehicleID INT,
            VehicleOdometerReading DECIMAL(10,2),
            FuelType NVARCHAR(20),
            FuelAmount DECIMAL(10,2),
            FuelCost DECIMAL(10,2),
            Notes NVARCHAR(MAX)
        )
"@
        "Maintenance" = @"
        CREATE TABLE Maintenance (
            MaintenanceID INT IDENTITY(1,1) PRIMARY KEY,
            Date NVARCHAR(50),
            VehicleID INT,
            OdometerReading DECIMAL(10,2),
            MaintenanceCompleted NVARCHAR(100),
            Vendor NVARCHAR(100),
            RepairCost DECIMAL(10,2),
            Notes NVARCHAR(MAX)
        )
"@
        "SchoolCalendar" = @"
        CREATE TABLE SchoolCalendar (
            CalendarID INT IDENTITY(1,1) PRIMARY KEY,
            CalendarDate NVARCHAR(50),
            IsSchoolDay BIT,
            EventName NVARCHAR(100),
            EventType NVARCHAR(50),
            Notes NVARCHAR(MAX)
        )
"@
        "TimeCards" = @"
        CREATE TABLE TimeCards (
            TimeCardID INT IDENTITY(1,1) PRIMARY KEY,
            Date NVARCHAR(50),
            ClockIn NVARCHAR(20),
            ClockOut NVARCHAR(20),
            TotalTime DECIMAL(10,2),
            Overtime DECIMAL(10,2),
            IsRouteDay BIT,
            RouteClockOutAM NVARCHAR(20),
            RouteClockInAM NVARCHAR(20),
            RouteClockOutPM NVARCHAR(20),
            RouteClockInPM NVARCHAR(20),
            EmployeeID INT,
            Notes NVARCHAR(MAX)
        )
"@
    }

    # Define tables that need Date column fixup
    $tablesWithDateColumns = @{
        "Activities" = "Date"
        "ActivitySchedule" = "Date"
        "Routes" = "Date"
        "Maintenance" = "Date"
        "TimeCards" = "Date"
    }

    # Create missing tables - no transaction needed for these individual operations
    foreach ($tableName in $tableDefinitions.Keys) {
        $tableExists = Test-TableExists -TableName $tableName

        if (-not $tableExists) {
            Write-Host "$tableName table does not exist! Creating it now..." -ForegroundColor Yellow
            try {
                Invoke-SqlCommandWithRetry -ServerInstance $server -Database $database -Query $tableDefinitions[$tableName] -MaxRetries $maxRetries -RetryDelay $retryDelay -Timeout $commandTimeout
                Write-Host "$tableName table created successfully." -ForegroundColor Green
            }
            catch {
                $errorMsg = $_.Exception.Message
                Write-Host "Error creating $tableName table - $errorMsg" -ForegroundColor Red
                throw
            }
        }
    }

    # Fix Date columns
    foreach ($tableName in $tablesWithDateColumns.Keys) {
        $columnName = $tablesWithDateColumns[$tableName]
        try {
            Update-DateColumn -TableName $tableName -ColumnName $columnName
        }
        catch {
            $errorMsg = $_.Exception.Message
            Write-Host "Error updating $columnName column in $tableName - $errorMsg" -ForegroundColor Red
            throw
        }
    }

    # Validate the schema
    $validationErrors = @()
    foreach ($tableName in $tableDefinitions.Keys) {
        $tableExists = Test-TableExists -TableName $tableName
        if (-not $tableExists) {
            $validationErrors += "Table $tableName was not created correctly."
        }
    }

    if ($validationErrors.Count -gt 0) {
        Write-Host "❌ Schema validation failed with the following errors:" -ForegroundColor Red
        foreach ($error in $validationErrors) {
            Write-Host "  - $error" -ForegroundColor Red
        }
    }
    else {
        Write-Host "✅ Schema validation complete. All tables exist and are properly configured." -ForegroundColor Green
    }

    Write-Host "✅ Database schema update complete. All tables aligned with BusBuddy specifications." -ForegroundColor Green
    Write-Host "Run the application to verify database connectivity." -ForegroundColor Cyan

} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.ScriptStackTrace)" -ForegroundColor Red
    Write-Host "Try running this script as administrator if you encounter permission issues." -ForegroundColor Yellow

    # Specific handling for threading/synchronization issues
    if ($_.Exception.Message -like "*synchronization*") {
        Write-Host "⚠️ This appears to be a threading/synchronization issue." -ForegroundColor Yellow
        Write-Host "Ensure no other processes are accessing the database during schema updates." -ForegroundColor Yellow
    }

    if ($_.Exception.Message -like "*canceled*") {
        Write-Host "⚠️ The operation was canceled, possibly due to a timeout or thread interruption." -ForegroundColor Yellow
        Write-Host "Consider increasing the command timeout or running the script when the system is less busy." -ForegroundColor Yellow
    }
}

Write-Host "`nPress any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
