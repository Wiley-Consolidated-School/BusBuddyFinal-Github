@echo off
echo Running BusBuddy Tests...
dotnet test BusBuddy.Tests\BusBuddy.Tests.csproj --verbosity normal
pause