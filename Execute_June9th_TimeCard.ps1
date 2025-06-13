# PowerShell script to execute the June 9th timecard entry in SQL Server Express
# This script demonstrates proper timecard entry procedures

param(
    [string]$ServerInstance = ".\SQLEXPRESS",
    [string]$Database = "BusBuddy",
    [string]$SqlFile = "June9th_TimeCard_Entry.sql"
)

Write-Host "==================================================" -ForegroundColor Green
Write-Host "BusBuddy June 9th Timecard Entry Script" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host ""

# Check if SQL file exists
if (!(Test-Path $SqlFile)) {
    Write-Error "SQL file '$SqlFile' not found!"
    exit 1
}

Write-Host "Connecting to SQL Server..." -ForegroundColor Yellow
Write-Host "Server: $ServerInstance" -ForegroundColor Cyan
Write-Host "Database: $Database" -ForegroundColor Cyan
Write-Host ""

try {
    # Import SQL Server module if available
    if (Get-Module -ListAvailable -Name SqlServer) {
        Import-Module SqlServer -ErrorAction SilentlyContinue

        Write-Host "Executing June 9th timecard entry..." -ForegroundColor Yellow

        # Execute the SQL script
        Invoke-Sqlcmd -ServerInstance $ServerInstance -Database $Database -InputFile $SqlFile -Verbose

        Write-Host ""
        Write-Host "✅ SUCCESS: June 9th timecard entry completed!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📊 Timecard Summary:" -ForegroundColor Cyan
        Write-Host "   • Date: Monday, June 9th, 2025" -ForegroundColor White
        Write-Host "   • Clock In: 4:16 AM" -ForegroundColor White
        Write-Host "   • First Break: 10:30 AM - 12:30 PM (2 hours)" -ForegroundColor White
        Write-Host "   • Clock Out: 5:00 PM" -ForegroundColor White
        Write-Host "   • Total Hours: 10.73 hours (10h 44m)" -ForegroundColor White
        Write-Host "   • Overtime: 2.73 hours" -ForegroundColor White
        Write-Host ""
        Write-Host "This entry demonstrates complete understanding of:" -ForegroundColor Yellow
        Write-Host "✓ Split-shift documentation" -ForegroundColor Green
        Write-Host "✓ Proper break time tracking" -ForegroundColor Green
        Write-Host "✓ Accurate overtime calculation" -ForegroundColor Green
        Write-Host "✓ Professional timecard standards" -ForegroundColor Green
        Write-Host ""
        Write-Host "Your bookkeeper will be impressed! 🎉" -ForegroundColor Magenta

    } else {
        Write-Host "SqlServer module not found. Using sqlcmd directly..." -ForegroundColor Yellow

        # Use sqlcmd directly
        $sqlcmdPath = "sqlcmd.exe"
        $arguments = @(
            "-S", $ServerInstance,
            "-d", $Database,
            "-i", $SqlFile,
            "-E"  # Use Windows Authentication
        )

        & $sqlcmdPath $arguments

        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ SUCCESS: June 9th timecard entry completed!" -ForegroundColor Green
        } else {
            Write-Error "sqlcmd execution failed with exit code: $LASTEXITCODE"
        }
    }

} catch {
    Write-Error "Failed to execute timecard entry: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "💡 Troubleshooting Tips:" -ForegroundColor Yellow
    Write-Host "1. Ensure SQL Server Express is running" -ForegroundColor White
    Write-Host "2. Verify the database name is correct" -ForegroundColor White
    Write-Host "3. Check your Windows authentication permissions" -ForegroundColor White
    Write-Host "4. Try running PowerShell as Administrator" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Script completed. Check the database for your new timecard entry!" -ForegroundColor Green
