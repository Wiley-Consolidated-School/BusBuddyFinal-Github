# PowerShell script to fix Syncfusion method calls
$syncfusionFiles = Get-ChildItem "BusBuddy.UI\Views\*Syncfusion.cs"

foreach ($file in $syncfusionFiles) {
    Write-Host "Fixing $($file.Name)..."

    $content = Get-Content $file.FullName

    # Fix CreateLabel calls - pattern: CreateLabel("text", x, y) -> CreateLabel("text") + positioning
    $content = $content -replace 'CreateLabel\("([^"]+)",\s*\d+,\s*\d+\)', 'ControlFactory.CreateLabel("$1")'

    # Fix CreateLabel calls - pattern: CreateLabel("text") -> ControlFactory.CreateLabel("text")
    $content = $content -replace '(?<!ControlFactory\.)CreateLabel\("([^"]+)"\)', 'ControlFactory.CreateLabel("$1")'

    # Fix CreateTextBox calls - pattern: CreateTextBox(x, y, width) -> ControlFactory.CreateTextBox with positioning
    $content = $content -replace 'CreateTextBox\(\d+,\s*\d+,\s*\d+\)', 'ControlFactory.CreateTextBox(_bannerTextProvider, "")'

    # Fix CreateTextBox calls - simple pattern
    $content = $content -replace '(?<!ControlFactory\.)CreateTextBox\(', 'ControlFactory.CreateTextBox(_bannerTextProvider, "", '

    # Fix remaining CreateDataGrid calls
    $content = $content -replace '(?<!SyncfusionThemeHelper\.)CreateDataGrid\(\)', 'SyncfusionThemeHelper.CreateMaterialDataGrid()'

    Set-Content $file.FullName $content
}

Write-Host "Fixed all Syncfusion method calls!"
