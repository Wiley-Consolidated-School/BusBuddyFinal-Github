# This script will create all BusBuddy tables in a SQLite database (test_busbuddy.db by default)
# Usage: pwsh ./Db/create-busbuddy-tables.ps1 [optional: -DatabasePath <path>]

param(
    [string]$DatabasePath = "test_busbuddy.db",
    [switch]$ForceCreate
)

# First, check if we're in the main app or test context
$mainAppConfig = Join-Path $PSScriptRoot "..\App.config"
$testAppConfig = Join-Path $PSScriptRoot "..\BusBuddy.Tests\App.config"

# Check if we can find the configuration files
if (Test-Path $mainAppConfig) {
    Write-Host "Found main application config at $mainAppConfig"
    $configPath = $mainAppConfig
} elseif (Test-Path $testAppConfig) {
    Write-Host "Found test application config at $testAppConfig"
    $configPath = $testAppConfig
} else {
    Write-Error "Could not find App.config in either main or test directories"
    exit 1
}

# If no database path was specified, try to get it from config
if ($DatabasePath -eq "test_busbuddy.db" -and (Test-Path $configPath)) {
    try {
        # Load the config file to check connection string
        [xml]$config = Get-Content $configPath
        $connectionString = $config.SelectSingleNode("//connectionStrings/add[@name='DefaultConnection']").connectionString

        # Extract database path from connection string
        if ($connectionString -match "Data Source=([^;]+)") {
            $configDbPath = $Matches[1]
            Write-Host "Found database path in config: $configDbPath"
            $DatabasePath = $configDbPath
        }
    } catch {
        Write-Warning "Could not parse connection string from config. Using default: $DatabasePath"
    }
}

Write-Host "Using database path: $DatabasePath"

# Find the schema file
$schemaLocations = @(
    "BusBuddy.Data\DatabaseScript.sql",
    "..\BusBuddy.Data\DatabaseScript.sql",
    "Db\DatabaseScript.sql",
    "..\Db\DatabaseScript.sql",
    "DatabaseScript.sql"
)

$schemaPath = $null
foreach ($loc in $schemaLocations) {
    $fullPath = Join-Path $PSScriptRoot $loc
    if (Test-Path $fullPath) {
        $schemaPath = $fullPath
        Write-Host "Found schema file at: $schemaPath"
        break
    }
}

if (!$schemaPath) {
    Write-Error "Schema file not found in any of the expected locations"
    exit 1
}

# Check if database exists
$dbExists = Test-Path $DatabasePath
if ($dbExists -and !$ForceCreate) {
    Write-Host "Database already exists at $DatabasePath. Use -ForceCreate to recreate it."
} else {
    if ($dbExists -and $ForceCreate) {
        Write-Host "Removing existing database at $DatabasePath"
        Remove-Item $DatabasePath -Force
    }

    Write-Host "Creating new SQLite database: $DatabasePath"
    New-Item -ItemType File -Path $DatabasePath -Force | Out-Null
}

# Try to use sqlite3.exe if available
$sqliteExe = Get-Command sqlite3 -ErrorAction SilentlyContinue
if ($sqliteExe) {
    Write-Host "Using sqlite3.exe to create database tables"

    # Modify the schema SQL to handle existing tables by adding "IF NOT EXISTS" to each table
    $schemaContent = Get-Content $schemaPath -Raw

    # Check if IF NOT EXISTS is already present in the schema
    if ($schemaContent -notmatch "CREATE TABLE IF NOT EXISTS") {
        $schemaContent = $schemaContent -replace "CREATE TABLE (\w+) \(", "CREATE TABLE IF NOT EXISTS `$1 ("
    }

    $tempSchemaPath = Join-Path $env:TEMP "TempDatabaseScript.sql"
    $schemaContent | Out-File -FilePath $tempSchemaPath -Encoding UTF8

    # If ForceCreate, drop all tables first
    if ($ForceCreate) {
        Write-Host "Force creating database - dropping existing tables first"
        $dropScript = @"
DROP TABLE IF EXISTS TimeCard;
DROP TABLE IF EXISTS ActivitySchedule;
DROP TABLE IF EXISTS SchoolCalendar;
DROP TABLE IF EXISTS Maintenance;
DROP TABLE IF EXISTS Fuel;
DROP TABLE IF EXISTS Activities;
DROP TABLE IF EXISTS Routes;
DROP TABLE IF EXISTS Drivers;
DROP TABLE IF EXISTS Vehicles;
"@
        $dropScriptPath = Join-Path $env:TEMP "DropTablesScript.sql"
        $dropScript | Out-File -FilePath $dropScriptPath -Encoding UTF8
        & sqlite3 $DatabasePath ".read $dropScriptPath"
        Remove-Item $dropScriptPath -Force -ErrorAction SilentlyContinue
    }

    & sqlite3 $DatabasePath ".read $tempSchemaPath"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "BusBuddy tables created in $DatabasePath using sqlite3.exe."
        Remove-Item $tempSchemaPath -Force -ErrorAction SilentlyContinue
        exit 0
    } else {
        Write-Warning "sqlite3.exe returned error code $LASTEXITCODE"
        Remove-Item $tempSchemaPath -Force -ErrorAction SilentlyContinue
    }
}

# Try to use Microsoft.Data.Sqlite via PowerShell
try {
    Write-Host "Using Microsoft.Data.Sqlite to create database tables"
    Add-Type -AssemblyName System.Data

    # Try multiple locations for Microsoft.Data.Sqlite.dll
    $dllLocations = @(
        (Join-Path $PSScriptRoot "..\bin\Debug\net8.0-windows\Microsoft.Data.Sqlite.dll"),
        (Join-Path $PSScriptRoot "..\bin\Release\net8.0-windows\Microsoft.Data.Sqlite.dll"),
        (Join-Path $PSScriptRoot "..\BusBuddy.Tests\bin\Debug\net8.0-windows\Microsoft.Data.Sqlite.dll"),
        (Join-Path $PSScriptRoot "..\BusBuddy.Tests\bin\Release\net8.0-windows\Microsoft.Data.Sqlite.dll")
    )

    $sqliteDllFound = $false
    foreach ($dllPath in $dllLocations) {
        if (Test-Path $dllPath) {
            Write-Host "Found Microsoft.Data.Sqlite.dll at: $dllPath"
            try {
                Add-Type -Path $dllPath -ErrorAction Stop
                $sqliteDllFound = $true
                break
            } catch {
                Write-Warning "Failed to load $dllPath: $($_)"
            }
        }
    }

    if (-not $sqliteDllFound -and -not ("Microsoft.Data.Sqlite.SqliteConnection" -as [type])) {
        # Try to find it recursively if not found in standard locations
        Write-Host "Searching for Microsoft.Data.Sqlite.dll recursively..."
        $sqliteDll = Get-ChildItem -Path (Join-Path $PSScriptRoot "..") -Filter "Microsoft.Data.Sqlite.dll" -Recurse |
                    Select-Object -First 1 -ExpandProperty FullName
        if ($sqliteDll) {
            Write-Host "Found Microsoft.Data.Sqlite.dll at: $sqliteDll"
            Add-Type -Path $sqliteDll -ErrorAction Stop
        } else {
            throw "Could not find Microsoft.Data.Sqlite.dll"
        }
    }

    # Create connection and execute script
    $connStr = "Data Source=$DatabasePath"
    $conn = New-Object Microsoft.Data.Sqlite.SqliteConnection $connStr
    $conn.Open()

    # Modify the schema SQL to handle existing tables
    $scriptContent = Get-Content $schemaPath -Raw
    $scriptContent = $scriptContent -replace "CREATE TABLE (\w+) \(", "CREATE TABLE IF NOT EXISTS `$1 ("

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $scriptContent
    $cmd.ExecuteNonQuery()
    $conn.Close()

    Write-Host "BusBuddy tables created in $DatabasePath using Microsoft.Data.Sqlite."
    exit 0
} catch {
    $errorMessage = $_.Exception.Message
    Write-Error "Could not create tables using Microsoft.Data.Sqlite: $errorMessage"

    # Fallback to direct SQLite commands
    try {
        Write-Host "Attempting fallback method using System.Data.SQLite if available..."

        # Try using in-memory modification of the SQL script and direct execution
        $scriptContent = Get-Content $schemaPath -Raw
        $scriptContent = $scriptContent -replace "CREATE TABLE (\w+) \(", "CREATE TABLE IF NOT EXISTS `$1 ("
        $tempSchemaPath = Join-Path $env:TEMP "ModifiedDatabaseScript.sql"
        $scriptContent | Out-File -FilePath $tempSchemaPath -Encoding UTF8

        Write-Host "Modified SQL script saved to: $tempSchemaPath"
        Write-Host "You can manually run: sqlite3 $DatabasePath '.read $tempSchemaPath'"

        # Attempt to directly run the modified script if sqlite3 is available
        $sqliteExe = Get-Command sqlite3 -ErrorAction SilentlyContinue
        if ($sqliteExe) {
            Write-Host "Retrying with modified script using sqlite3.exe..."
            & sqlite3 $DatabasePath ".read $tempSchemaPath"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "BusBuddy tables created in $DatabasePath using sqlite3.exe (fallback)."
                Remove-Item $tempSchemaPath -Force -ErrorAction SilentlyContinue
                exit 0
            }
        }

        throw "Could not create tables. Manual intervention may be required."
    } catch {
        $finalErrorMessage = $_.Exception.Message
        Write-Error "All methods failed. Please install sqlite3.exe or ensure Microsoft.Data.Sqlite is available. $finalErrorMessage"
        Write-Host "Suggested manual command: sqlite3 $DatabasePath '.read $tempSchemaPath'"
        exit 1
    }
}
