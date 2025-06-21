# This script initializes or updates the BusBuddy_Test database
# with the correct schema including Activities and ActivitySchedule tables

# Connection strings
$masterConnectionString = "Data Source=.\SQLEXPRESS01;Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True"
$testDbConnectionString = "Data Source=.\SQLEXPRESS01;Initial Catalog=BusBuddy_Test;Integrated Security=True;TrustServerCertificate=True"

# Check if the database exists
$dbExists = $false
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $masterConnectionString
    $connection.Open()

    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = 'BusBuddy_Test'"
    $dbExists = [int]($command.ExecuteScalar()) -gt 0

    $connection.Close()
}
catch {
    Write-Host "Error checking database existence: $_" -ForegroundColor Red
    exit 1
}

# Create database if it doesn't exist
if (-not $dbExists) {
    Write-Host "Creating BusBuddy_Test database..." -ForegroundColor Yellow
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $masterConnectionString
        $connection.Open()

        $command = $connection.CreateCommand()
        $command.CommandText = "CREATE DATABASE [BusBuddy_Test]"
        $command.ExecuteNonQuery() | Out-Null

        $connection.Close()
        Write-Host "Database created successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Error creating database: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "BusBuddy_Test database already exists" -ForegroundColor Green
}

# Create or update tables in the database
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $testDbConnectionString
    $connection.Open()

    # Check which tables exist
    $command = $connection.CreateCommand()

    # Function to check if a table exists
    function TableExists {
        param(
            [string]$tableName
        )

        $command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '$tableName'"
        return [int]($command.ExecuteScalar()) -gt 0
    }

    # Create Vehicles table if it doesn't exist
    if (-not (TableExists -tableName "Vehicles")) {
        Write-Host "Creating Vehicles table..." -ForegroundColor Yellow
        $command.CommandText = @"
CREATE TABLE Vehicles (
    VehicleID INT PRIMARY KEY IDENTITY(1,1),
    VehicleNumber NVARCHAR(50) NOT NULL,
    Make NVARCHAR(50),
    Model NVARCHAR(50),
    Year INT,
    Capacity INT,
    Status NVARCHAR(20),
    LastMaintenance DATETIME,
    Notes NVARCHAR(MAX)
)
"@
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "Vehicles table created" -ForegroundColor Green
    }

    # Create Drivers table if it doesn't exist
    if (-not (TableExists -tableName "Drivers")) {
        Write-Host "Creating Drivers table..." -ForegroundColor Yellow
        $command.CommandText = @"
CREATE TABLE Drivers (
    DriverID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    LicenseType NVARCHAR(20),
    CDLExpiry DATETIME,
    PhoneNumber NVARCHAR(20),
    Email NVARCHAR(100),
    Status NVARCHAR(20),
    Notes NVARCHAR(MAX)
)
"@
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "Drivers table created" -ForegroundColor Green
    }

    # Create Routes table if it doesn't exist
    if (-not (TableExists -tableName "Routes")) {
        Write-Host "Creating Routes table..." -ForegroundColor Yellow
        $command.CommandText = @"
CREATE TABLE Routes (
    RouteID INT PRIMARY KEY IDENTITY(1,1),
    RouteName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    StartLocation NVARCHAR(100),
    EndLocation NVARCHAR(100),
    EstimatedDuration INT,
    Distance DECIMAL(10,2),
    Notes NVARCHAR(MAX)
)
"@
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "Routes table created" -ForegroundColor Green
    }

    # Create Activities table if it doesn't exist
    if (-not (TableExists -tableName "Activities")) {
        Write-Host "Creating Activities table..." -ForegroundColor Yellow
        $command.CommandText = @"
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
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "Activities table created" -ForegroundColor Green
    }

    # Create ActivitySchedule table if it doesn't exist
    if (-not (TableExists -tableName "ActivitySchedule")) {
        Write-Host "Creating ActivitySchedule table..." -ForegroundColor Yellow
        $command.CommandText = @"
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
        $command.ExecuteNonQuery() | Out-Null
        Write-Host "ActivitySchedule table created" -ForegroundColor Green
    }

    $connection.Close()
    Write-Host "Database schema setup completed successfully" -ForegroundColor Green
}
catch {
    Write-Host "Error setting up database schema: $_" -ForegroundColor Red
    if ($connection -and $connection.State -eq 'Open') {
        $connection.Close()
    }
    exit 1
}
