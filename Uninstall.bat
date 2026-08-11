@echo off
REM Database Backup Service - Uninstallation Launcher
REM This batch file runs the PowerShell uninstallation script with administrator privileges

echo ============================================================
echo   Database Backup Service - Uninstallation
echo ============================================================
echo.
echo This will remove the Database Backup Service from your system.
echo.
echo Press any key to continue or Ctrl+C to cancel...
pause > nul

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with administrator privileges...
    echo.
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0uninstall-service.ps1"
) else (
    echo Requesting administrator privileges...
    echo.
    powershell.exe -Command "Start-Process powershell -ArgumentList '-ExecutionPolicy Bypass -File \"%~dp0uninstall-service.ps1\"' -Verb RunAs"
)

echo.
echo Uninstallation process initiated.
pause
