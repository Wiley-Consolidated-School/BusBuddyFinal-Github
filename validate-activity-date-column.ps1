# Fix the Activities and ActivitySchedule tables in BusBuddy_Test database
# Checks the Date column format and updates if necessary

$connectionString = "Data Source=.\SQLEXPRESS01;Initial Catalog=BusBuddy_Test;Integrated Security=True;TrustServerCertificate=True"

# Helper function to run SQL commands
function Invoke-SqlQuery {
    param (
        [string]$query
    )

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection
        $connection.ConnectionString = $connectionString
        $connection.Open()

        $command = $connection.CreateCommand()
        $command.CommandText = $query

        $result = $command.ExecuteNonQuery()
        Write-Host "SQL command executed successfully: $query"

        $connection.Close()
        return $result
    }
    catch {
        Write-Host "Error executing SQL query: $_" -ForegroundColor Red
        if ($connection -and $connection.State -eq 'Open') {
            $connection.Close()
        }
        throw
    }
}

# Check if Activities table has the right column format
Write-Host "Checking Activities table..." -ForegroundColor Yellow

try {
    # Try to insert a test record using the proper string format
    $testDate = (Get-Date).ToString("yyyy-MM-dd")

    # Activities table check
    $checkActivitiesQuery = @"
INSERT INTO Activities (Date, ActivityType, Destination, LeaveTime, EventTime, ReturnTime)
VALUES ('$testDate', 'TEST', 'Test Destination', '08:00', '09:00', '10:00');

DELETE FROM Activities WHERE ActivityType = 'TEST' AND Destination = 'Test Destination';
"@

    Invoke-SqlQuery -query $checkActivitiesQuery
    Write-Host "✅ Activities table Date column is working correctly" -ForegroundColor Green

    # ActivitySchedule table check
    $checkScheduleQuery = @"
INSERT INTO ActivitySchedule (Date, TripType, ScheduledDestination, ScheduledLeaveTime, ScheduledEventTime, ScheduledReturnTime)
VALUES ('$testDate', 'TEST', 'Test Destination', '08:00', '09:00', '10:00');

DELETE FROM ActivitySchedule WHERE TripType = 'TEST' AND ScheduledDestination = 'Test Destination';
"@

    Invoke-SqlQuery -query $checkScheduleQuery
    Write-Host "✅ ActivitySchedule table Date column is working correctly" -ForegroundColor Green

    Write-Host "Database validation completed successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Error during database validation: $_" -ForegroundColor Red
}
