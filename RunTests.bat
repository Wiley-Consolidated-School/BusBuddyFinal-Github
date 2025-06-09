@echo off
echo Running BusBuddy tests...
dotnet test BusBuddy.Tests\BusBuddy.Tests.csproj --verbosity normal
pause