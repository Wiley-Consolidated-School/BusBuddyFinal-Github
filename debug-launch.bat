@echo off
echo Starting BusBuddy with enhanced debugging...
cd /d "C:\Users\steve.mckitrick\Desktop\BusBuddy"
set DOTNET_ENVIRONMENT=Development
set BUSBUDDY_DEBUG=true
dotnet run
pause
