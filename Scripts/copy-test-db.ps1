# Copy the seeded test database to the test output directory before running tests
$source = "test_busbuddy.db"
$dest = "BusBuddy.Tests/bin/Debug/net8.0-windows/test_busbuddy.db"

if (Test-Path $source) {
    Copy-Item $source $dest -Force
    Write-Host "Copied $source to $dest."
} else {
    Write-Error "Seeded test database $source not found!"
}
