# Quick fix for remaining build errors
$files = @(
    "BusBuddy.UI\Views\FuelEditFormSyncfusion.cs",
    "BusBuddy.UI\Views\MaintenanceEditFormSyncfusion.cs"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Fixing $file..."

        $content = Get-Content $file

        # Fix CreateLabel calls that remain
        $content = $content -replace '(?<!ControlFactory\.)CreateLabel\(', 'ControlFactory.CreateLabel('

        # Fix ControlFactory.CreateTextBox calls with wrong number of parameters
        # Pattern: ControlFactory.CreateTextBox(_bannerTextProvider, "", x, y, width)
        # Should be: ControlFactory.CreateTextBox(_bannerTextProvider, "placeholder")
        $content = $content -replace 'ControlFactory\.CreateTextBox\(_bannerTextProvider, "", \d+, \d+, \d+\)', 'ControlFactory.CreateTextBox(_bannerTextProvider, "")'

        Set-Content $file $content
    }
}

Write-Host "Fixed remaining CreateLabel and CreateTextBox issues!"
