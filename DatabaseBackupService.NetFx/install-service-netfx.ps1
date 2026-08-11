# Database Backup Service (.NET Framework 4.8) Installation Script
# This script installs the Database Backup Windows Service

param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "DatabaseBackupService",

    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "Database Backup Service (.NET Framework)",

    [Parameter(Mandatory=$false)]
    [string]$Description = "Automated database backup service for SQL Server and MySQL (.NET Framework 4.8)",

    [Parameter(Mandatory=$false)]
    [ValidateSet("Install", "Uninstall", "Start", "Stop", "Restart")]
    [string]$Action = "Install"
)

# Require Administrator
if (-NOT ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {
    Write-Error "This script must be run as Administrator"
    exit 1
}

$ServicePath = Join-Path $PSScriptRoot "bin\Release\DatabaseBackupService.NetFx.exe"

if (-not (Test-Path $ServicePath)) {
    $ServicePath = Join-Path $PSScriptRoot "bin\Debug\DatabaseBackupService.NetFx.exe"
}

if (-not (Test-Path $ServicePath)) {
    Write-Error "Service executable not found. Please build the project first."
    exit 1
}

function Install-Service {
    Write-Host "Installing service: $ServiceName" -ForegroundColor Green

    # Check if service already exists
    $existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($existingService) {
        Write-Warning "Service already exists. Uninstalling first..."
        Uninstall-Service
    }

    # Install using sc.exe
    $result = sc.exe create $ServiceName binPath= $ServicePath start= auto DisplayName= $DisplayName

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Service installed successfully" -ForegroundColor Green

        # Set description
        sc.exe description $ServiceName $Description

        # Configure service recovery options (restart on failure)
        sc.exe failure $ServiceName reset= 86400 actions= restart/60000/restart/60000/restart/60000

        Write-Host "Service configured to restart automatically on failure" -ForegroundColor Green
        Write-Host ""
        Write-Host "To start the service, run:" -ForegroundColor Yellow
        Write-Host "  sc.exe start $ServiceName" -ForegroundColor Yellow
        Write-Host "  or use: .\install-service-netfx.ps1 -Action Start" -ForegroundColor Yellow
    } else {
        Write-Error "Failed to install service"
        exit 1
    }
}

function Uninstall-Service {
    Write-Host "Uninstalling service: $ServiceName" -ForegroundColor Green

    # Check if service exists
    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Warning "Service '$ServiceName' not found. It may already be uninstalled."
        return
    }

    # Stop service if running
    if ($service.Status -eq 'Running') {
        Write-Host "Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
    }

    # Delete service
    Write-Host "Deleting service..." -ForegroundColor Yellow
    $result = sc.exe delete $ServiceName

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Service uninstalled successfully" -ForegroundColor Green
        Write-Host ""
        Write-Host "NOTE: The service may still appear in the Services console until you:" -ForegroundColor Yellow
        Write-Host "  1. Close and reopen the Services console (services.msc), or" -ForegroundColor Yellow
        Write-Host "  2. Press F5 to refresh the Services console, or" -ForegroundColor Yellow
        Write-Host "  3. Restart your computer" -ForegroundColor Yellow
        Write-Host ""

        # Wait a moment and verify deletion
        Start-Sleep -Seconds 2
        $serviceCheck = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
        if ($serviceCheck) {
            Write-Warning "Service is marked for deletion but still appears in the service list."
            Write-Host "This is normal if the Services console (services.msc) is open." -ForegroundColor Yellow
            Write-Host "Close all Services consoles and the service will be removed." -ForegroundColor Yellow
        }
    } else {
        Write-Error "Failed to uninstall service (Exit code: $LASTEXITCODE)"
        Write-Host "Common reasons:" -ForegroundColor Yellow
        Write-Host "  - Services console (services.msc) is open - close it and try again" -ForegroundColor Yellow
        Write-Host "  - Another process has a handle to the service" -ForegroundColor Yellow
        exit 1
    }
}

function Start-ServiceWrapper {
    Write-Host "Starting service: $ServiceName" -ForegroundColor Green

    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Error "Service not found. Please install it first."
        exit 1
    }

    if ($service.Status -eq 'Running') {
        Write-Warning "Service is already running"
        return
    }

    Start-Service -Name $ServiceName
    Start-Sleep -Seconds 2

    $service = Get-Service -Name $ServiceName
    if ($service.Status -eq 'Running') {
        Write-Host "Service started successfully" -ForegroundColor Green
    } else {
        Write-Error "Failed to start service"
        exit 1
    }
}

function Stop-ServiceWrapper {
    Write-Host "Stopping service: $ServiceName" -ForegroundColor Green

    $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if (-not $service) {
        Write-Error "Service not found"
        exit 1
    }

    if ($service.Status -eq 'Stopped') {
        Write-Warning "Service is already stopped"
        return
    }

    Stop-Service -Name $ServiceName -Force
    Start-Sleep -Seconds 2

    $service = Get-Service -Name $ServiceName
    if ($service.Status -eq 'Stopped') {
        Write-Host "Service stopped successfully" -ForegroundColor Green
    } else {
        Write-Error "Failed to stop service"
        exit 1
    }
}

function Restart-ServiceWrapper {
    Write-Host "Restarting service: $ServiceName" -ForegroundColor Green
    Stop-ServiceWrapper
    Start-ServiceWrapper
}

# Execute action
switch ($Action) {
    "Install" { Install-Service }
    "Uninstall" { Uninstall-Service }
    "Start" { Start-ServiceWrapper }
    "Stop" { Stop-ServiceWrapper }
    "Restart" { Restart-ServiceWrapper }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
