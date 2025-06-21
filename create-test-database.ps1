# Create BusBuddy_Test database for development
# Run this script with appropriate permissions

$server = ".\SQLEXPRESS01"  # Local SQL Server Express instance
$database = "BusBuddy_Test"

Write-Host "Creating test database: $database on server: $server" -ForegroundColor Cyan

# SQL command to create database if it doesn't exist
$createDbSql = @"
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'$database')
BEGIN
    CREATE DATABASE [$database]
    PRINT 'Database created successfully.'
END
ELSE
BEGIN
    PRINT 'Database already exists.'
END
"@

try {
    # Create the database using sqlcmd with TrustServerCertificate
    Invoke-Sqlcmd -ServerInstance $server -Query $createDbSql -TrustServerCertificate -ErrorAction Stop

    # Grant permissions to current user
    $currentUser = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
    $grantAccessSql = @"
    USE [$database]
    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '$currentUser')
    BEGIN
        CREATE USER [$currentUser] FOR LOGIN [$currentUser]
        EXEC sp_addrolemember 'db_owner', '$currentUser'
        PRINT 'User added with db_owner permissions.'
    END
    ELSE
    BEGIN
        PRINT 'User already exists in database.'
    END
"@

    Invoke-Sqlcmd -ServerInstance $server -Query $grantAccessSql -TrustServerCertificate -ErrorAction Stop

    Write-Host "Database setup complete. You should now be able to connect to $database." -ForegroundColor Green
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host "Try running this script as administrator or with SQL Server permissions." -ForegroundColor Yellow

    # Check if the SqlServer module is installed
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "The SqlServer PowerShell module is not installed." -ForegroundColor Red
        Write-Host "Install it with: Install-Module -Name SqlServer -AllowClobber -Force" -ForegroundColor Yellow
    }
}

Write-Host "Press any key to continue..."
$null = $host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
