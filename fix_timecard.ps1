# Fix TimeCard namespace conflicts globally
Write-Host "Fixing TimeCard namespace conflicts..." -ForegroundColor Yellow

# Read the file
$filePath = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\TimeCardRepository.cs"
$content = Get-Content $filePath -Raw

# Replace problematic TimeCard references with fully qualified names
$content = $content -replace 'Task<TimeCard>', 'Task<BusBuddy.Models.TimeCard>'
$content = $content -replace 'IEnumerable<TimeCard>', 'IEnumerable<BusBuddy.Models.TimeCard>'
$content = $content -replace '\(TimeCard ', '(BusBuddy.Models.TimeCard '
$content = $content -replace 'async Task<TimeCard>', 'async Task<BusBuddy.Models.TimeCard>'

# Write back to file
Set-Content $filePath -Value $content

Write-Host "Fixed TimeCardRepository.cs" -ForegroundColor Green

# Fix ITimeCardRepository as well
$interfacePath = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\ITimeCardRepository.cs"
$interfaceContent = Get-Content $interfacePath -Raw

# Replace problematic TimeCard references with fully qualified names
$interfaceContent = $interfaceContent -replace 'Task<TimeCard>', 'Task<BusBuddy.Models.TimeCard>'
$interfaceContent = $interfaceContent -replace 'IEnumerable<TimeCard>', 'IEnumerable<BusBuddy.Models.TimeCard>'
$interfaceContent = $interfaceContent -replace '\(TimeCard ', '(BusBuddy.Models.TimeCard '

Set-Content $interfacePath -Value $interfaceContent

Write-Host "Fixed ITimeCardRepository.cs" -ForegroundColor Green

# Fix BusBuddyContext
$contextPath = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.Data\BusBuddyContext.cs"
$contextContent = Get-Content $contextPath -Raw

$contextContent = $contextContent -replace 'DbSet<TimeCard>', 'DbSet<BusBuddy.Models.TimeCard>'

Set-Content $contextPath -Value $contextContent

Write-Host "Fixed BusBuddyContext.cs" -ForegroundColor Green
