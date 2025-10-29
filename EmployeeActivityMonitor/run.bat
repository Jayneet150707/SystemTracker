@echo off
echo ========================================
echo Employee Activity Monitor - Launcher
echo ========================================
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed!
    echo Please download and install .NET 8.0 from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo .NET SDK detected
echo.
echo Building application...
dotnet build -c Release

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo Build successful! Starting monitor...
echo.
dotnet run --project . -c Release

pause

