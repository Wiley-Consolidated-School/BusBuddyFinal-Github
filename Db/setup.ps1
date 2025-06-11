param(
    [string]$SqlServer = "localhost",
    [string]$User = "sa",
    [string]$Password = "YourStrong@Passw0rd",
    [string]$Script = "setup.sql"
)

Write-Host "Running SQL Server setup script on $SqlServer..."

$sqlcmd = "sqlcmd -S $SqlServer -U $User -P $Password -i $Script"
Invoke-Expression $sqlcmd

Write-Host "Database setup complete."
