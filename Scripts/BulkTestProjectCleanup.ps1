# Bulk Cleanup Script for BusBuddy Test Project
# Removes redundant test files and updates dependencies for BusBuddy.UI.Tests

# Remove StandaloneTests.cs (redundant test class)
$standaloneTest = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.UI.Tests\StandaloneTests.cs"
if (Test-Path $standaloneTest) {
    Remove-Item $standaloneTest -Force
    Write-Host "Removed redundant StandaloneTests.cs" -ForegroundColor Green
}

# Update NuGet dependencies for BusBuddy.UI.Tests
$testProj = "c:\Users\steve.mckitrick\Desktop\BusBuddy\BusBuddy.UI.Tests\BusBuddy.UI.Tests.csproj"
if (Test-Path $testProj) {
    dotnet restore $testProj
    dotnet add $testProj package xunit --version 2.6.6
    dotnet add $testProj package xunit.runner.visualstudio --version 2.6.6
    dotnet add $testProj package Microsoft.NET.Test.Sdk --version 17.10.0
    Write-Host "Updated test dependencies in BusBuddy.UI.Tests.csproj" -ForegroundColor Cyan
}

# Lock test task to only run BusBuddy.UI.Tests
$tasksFile = "c:\Users\steve.mckitrick\Desktop\BusBuddy\.vscode\tasks.json"
if (Test-Path $tasksFile) {
    $tasks = Get-Content $tasksFile -Raw | ConvertFrom-Json
    foreach ($task in $tasks.tasks) {
        if ($task.label -like '*test*') {
            $task.command = 'dotnet'
            $task.args = @('test', 'BusBuddy.UI.Tests/BusBuddy.UI.Tests.csproj')
        }
    }
    $tasks | ConvertTo-Json -Depth 10 | Set-Content $tasksFile -Encoding UTF8
    Write-Host "Locked test tasks to BusBuddy.UI.Tests only" -ForegroundColor Yellow
}

Write-Host "Bulk test project cleanup and dependency update complete." -ForegroundColor Green
