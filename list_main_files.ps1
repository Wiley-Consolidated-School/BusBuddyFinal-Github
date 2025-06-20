# First, let's see what's actually in the main BusBuddy directory
Write-Host "Contents of main BusBuddy directory:" -ForegroundColor Yellow
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -File | Where-Object { $_.Extension -eq ".cs" } | ForEach-Object {
    Write-Host "📄 $($_.Name)" -ForegroundColor Cyan
}

# Check if there are any DI-related files that might have test content
Write-Host "`nLooking for DI-related files:" -ForegroundColor Yellow
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*DI*.cs" -File
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*Dependency*.cs" -File
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*Injection*.cs" -File
Get-ChildItem -Path "c:\Users\steve.mckitrick\Desktop\BusBuddy" -Filter "*Container*.cs" -File
