# Check directory structure and file locations
Write-Host "BusBuddy Project Structure:" -ForegroundColor Yellow
Get-ChildItem -Path "." -Directory | ForEach-Object {
    Write-Host "📁 $($_.Name)" -ForegroundColor Green
    if ($_.Name -eq "BusBuddy.Tests") {
        Get-ChildItem -Path $_.FullName -Recurse | ForEach-Object {
            $indent = "  " * (($_.FullName.Split('\').Count) - 3)
            if ($_.PSIsContainer) {
                Write-Host "$indent📁 $($_.Name)" -ForegroundColor Cyan
            } else {
                Write-Host "$indent📄 $($_.Name)" -ForegroundColor White
            }
        }
    }
}

Write-Host "`nChecking for any .cs files in main BusBuddy directory:" -ForegroundColor Yellow
Get-ChildItem -Path "." -Filter "*.cs" | ForEach-Object {
    Write-Host "  📄 $($_.Name)" -ForegroundColor White
    $content = Get-Content $_.FullName -Head 10
    if ($content -match "using Xunit|using Moq|\[Fact\]|\[Theory\]") {
        Write-Host "    ⚠️ Contains test content!" -ForegroundColor Red
    }
}
