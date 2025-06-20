# Quick fix for TimeCard namespace issues
Get-Content "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\TimeCardRepository.cs" | Select-Object -First 10
Write-Host "`n--- Around line 137 ---" -ForegroundColor Yellow
Get-Content "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\TimeCardRepository.cs" | Select-Object -Skip 135 -First 5
Write-Host "`n--- Around line 155 ---" -ForegroundColor Yellow
Get-Content "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\TimeCardRepository.cs" | Select-Object -Skip 153 -First 5
