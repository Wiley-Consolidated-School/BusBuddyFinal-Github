Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*.cs" -File | ForEach-Object {
    Write-Host "Found: $($_.Name)"
    $content = Get-Content $_.FullName -Head 5
    if ($content -match "using Xunit|Test|Mock") {
        Write-Host "  -> CONTAINS TEST CONTENT!" -ForegroundColor Red
        Write-Host "  -> First few lines:"
        $content | ForEach-Object { Write-Host "     $_" }
    }
}
