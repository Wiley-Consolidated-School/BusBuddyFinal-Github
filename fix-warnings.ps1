# PowerShell script to fix common CS8618 and CS8622 warnings
Write-Host "Fixing nullability warnings..." -ForegroundColor Green

# Get all C# files in the UI Views directory
$files = Get-ChildItem -Path "BusBuddy.UI\Views" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Fix CS8622 warnings - EventHandler parameters
    $content = $content -replace 'private void (\w+_Click)\(object sender,', 'private void $1(object? sender,'
    $content = $content -replace 'private void (\w+_SelectionChanged)\(object sender,', 'private void $1(object? sender,'
    $content = $content -replace 'private void (\w+_Changed)\(object sender,', 'private void $1(object? sender,'

    # Fix common field declarations to be nullable
    $content = $content -replace 'private (Control|Button|TextBox|ComboBox|DateTimePicker|DataGridView|Panel|Label|CheckBox) (\w+);', 'private $1? $2;'
    $content = $content -replace 'private readonly (Control|Button|TextBox|ComboBox|DateTimePicker|DataGridView|Panel|Label|CheckBox) (\w+);', 'private readonly $1? $2;'

    # Only write if content changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Fixed: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host "Completed fixing nullable warnings!" -ForegroundColor Green
