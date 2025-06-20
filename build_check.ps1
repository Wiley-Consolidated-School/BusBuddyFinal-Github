# Build and capture specific errors
Write-Host "Building to identify exact issues..." -ForegroundColor Yellow
$output = dotnet build 2>&1
Write-Host $output
Write-Host "`nLooking for TimeCard errors specifically:" -ForegroundColor Yellow
$output | Where-Object { $_ -match "TimeCard.*namespace" }
