# This script will create and initialize the SQLite test database for BusBuddy integration tests.
# It requires the 'System.Data.SQLite' PowerShell module or the sqlite3.exe CLI in your PATH.

$schemaPath = "BusBuddy.Data/DatabaseScript.sql"
$dbPath = "BusBuddy.Tests/bin/Debug/net8.0-windows/test_busbuddy.db"

# Remove any existing test database
i f (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
}

# Try to use sqlite3.exe if available
$sqliteExe = Get-Command sqlite3 -ErrorAction SilentlyContinue
if ($sqliteExe) {
    & sqlite3 $dbPath ".read $schemaPath"
    Write-Host "Database created using sqlite3.exe."
} else {
    # Try to use System.Data.SQLite via PowerShell
    try {
        Add-Type -Path (Get-ChildItem -Recurse -Filter System.Data.SQLite.dll | Select-Object -First 1).FullName
        $connStr = "Data Source=$dbPath;Version=3;"
        $conn = New-Object System.Data.SQLite.SQLiteConnection $connStr
        $conn.Open()
        $script = Get-Content $schemaPath -Raw
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = $script
        $cmd.ExecuteNonQuery()
        $conn.Close()
        Write-Host "Database created using System.Data.SQLite."
    } catch {
        $errorMsg = $_.Exception.Message
        Write-Error "Could not create database. Please install sqlite3.exe or System.Data.SQLite. $errorMsg"
    }
}
