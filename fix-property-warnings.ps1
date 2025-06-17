# Fix property nullability warnings
Write-Host "Fixing property nullability warnings..." -ForegroundColor Green

$files = Get-ChildItem -Path "BusBuddy.UI\Views" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content

    # Fix property declarations to be nullable
    $content = $content -replace 'public (Control|Button|TextBox|ComboBox|DateTimePicker|DataGridView|Panel|Label|CheckBox) (\w+) \{ get; set; \}', 'public $1? $2 { get; set; }'
    $content = $content -replace 'public (Control|Button|TextBox|ComboBox|DateTimePicker|DataGridView|Panel|Label|CheckBox) (\w+) \{ get; private set; \}', 'public $1? $2 { get; private set; }'
    $content = $content -replace 'public (Control|Button|TextBox|ComboBox|DateTimePicker|DataGridView|Panel|Label|CheckBox) (\w+) \{ get; \}', 'public $1? $2 { get; }'

    # Fix specific Syncfusion controls
    $content = $content -replace 'private (SfDataGrid|SfTextBox|SfComboBox|SfButton|SfDateTimePicker) (\w+);', 'private $1? $2;'

    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host "Fixed properties: $($file.Name)" -ForegroundColor Yellow
    }
}

Write-Host "Completed fixing property warnings!" -ForegroundColor Green
