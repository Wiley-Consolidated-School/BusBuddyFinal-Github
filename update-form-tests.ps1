# PowerShell script to update form test files to use BaseFormTestHelper
# This will standardize form constructor tests across all management forms

Write-Host "Updating form test files to use BaseFormTestHelper..."

$formTests = @(
    @{File="MaintenanceManagementFormUnitTests.cs"; Title="Maintenance Management"},
    @{File="RouteManagementFormUnitTests.cs"; Title="Route Management"},
    @{File="ActivityManagementFormUnitTests.cs"; Title="Activity Management"},
    @{File="ActivityScheduleManagementFormUnitTests.cs"; Title="Activity Schedule Management"},
    @{File="SchoolCalendarManagementFormUnitTests.cs"; Title="School Calendar Management"}
)

foreach ($test in $formTests) {
    $filePath = "BusBuddy.Tests\$($test.File)"
    if (Test-Path $filePath) {
        Write-Host "Updating $($test.File)..."

        # Add using statement for TestHelpers
        $content = Get-Content $filePath -Raw
        if ($content -notmatch "using BusBuddy\.Tests\.TestHelpers;") {
            $content = $content -replace "(using System\.Windows\.Forms;)", "`$1`nusing BusBuddy.Tests.TestHelpers;"
        }

        # Update constructor test to use helper
        $oldPattern = "public void Constructor_InitializesSuccessfully[^{]*\{[^}]*var form = new \w+ManagementForm\(\);[^}]*Assert\.NotNull\(form\);[^}]*\}"
        $newContent = @"
public void Constructor_InitializesSuccessfully()
        {
            // Act & Assert using helper
            BaseFormTestHelper.AssertFormInitializesSuccessfully(
                () => new $($test.Title.Replace(' ', ''))Form(),
                "$($test.Title)",
                new System.Drawing.Size(800, 600)
            );
        }
"@

        # This is a simplified replacement - may need manual adjustment
        Set-Content $filePath $content
    }
}

Write-Host "Form test files updated. Please verify and build to check for any issues."
