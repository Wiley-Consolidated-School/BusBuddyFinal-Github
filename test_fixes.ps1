# Test the fixes
Write-Host "Testing the fixes..." -ForegroundColor Yellow
$result = dotnet build --verbosity quiet 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
} else {
    Write-Host "❌ Build still has errors:" -ForegroundColor Red
    Write-Host $result
}
