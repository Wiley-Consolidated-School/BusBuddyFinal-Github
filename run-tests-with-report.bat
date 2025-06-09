@echo off
echo Running BusBuddy Tests with HTML Report...
cd %~dp0
dotnet test BusBuddy.Tests\BusBuddy.Tests.csproj --logger:"html;LogFileName=TestResults.html"
echo Test results saved to BusBuddy.Tests\TestResults\TestResults.html
start chrome "file:///%~dp0BusBuddy.Tests\TestResults\TestResults.html"
pause