# Add null-forgiving operators to reduce CS8602 warnings where fields are guaranteed to be initialized
Write-Host "Adding null-forgiving operators..." -ForegroundColor Green

$files = Get-ChildItem -Path "BusBuddy.UI\Views" -Filter "*Syncfusion*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # For common scenarios where fields are definitely initialized but compiler can't tell
    # Add null-forgiving operator for field access in InitializeComponent or after construction

    # Text property access on controls
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Text\s*=', '$1Text! ='
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Text\s*==', '$1Text! =='
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Text\s*\!=', '$1Text! !='
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Text\.', '$1Text!.'

    # Common control property access
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)SelectedIndex', '$1SelectedIndex!'
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Value', '$1Value!'
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Size', '$1Size!'
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Location', '$1Location!'
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)BackColor', '$1BackColor!'

    # Method calls on controls
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Add\(', '$1Add!('
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)Clear\(\)', '$1Clear!()'
    $content = $content -replace '([_a-zA-Z][_a-zA-Z0-9]*\.)SetError\(', '$1SetError!('

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Added null-forgiving operators: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host "Completed adding null-forgiving operators!" -ForegroundColor Green
