@echo off
REM Database Backup Service - Installation Launcher
REM This batch file runs the PowerShell installation script with administrator privileges

echo ============================================================
echo   Database Backup Service - Installation
echo ============================================================
echo.
echo This will install the Database Backup Service as a Windows Service.
echo.
echo Press any key to continue or Ctrl+C to cancel...
pause > nul

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    echo Running with administrator privileges...
    echo.
    powershell.exe -ExecutionPolicy Bypass -File "%~dp0install-service.ps1"
) else (
    echo Requesting administrator privileges...
    echo.
    powershell.exe -Command "Start-Process powershell -ArgumentList '-ExecutionPolicy Bypass -File \"%~dp0install-service.ps1\"' -Verb RunAs"
)

echo.
echo Installation process initiated.
pause
