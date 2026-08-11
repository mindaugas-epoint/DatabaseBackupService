#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Uninstalls the Database Backup Service from Windows.

.DESCRIPTION
    This script removes the DatabaseBackupService:
    1. Stops the Windows Service
    2. Unregisters the service
    3. Optionally removes program files
    4. Optionally removes registry configuration

.PARAMETER ServiceName
    The name of the Windows Service to remove. Default: "DatabaseBackupService"

.PARAMETER InstallPath
    The installation directory to remove. Default: "C:\Program Files\DatabaseBackupService"

.PARAMETER RemoveConfig
    Whether to remove the configuration from registry. Default: $false

.EXAMPLE
    .\uninstall-service.ps1

.EXAMPLE
    .\uninstall-service.ps1 -RemoveConfig
#>

param(
    [string]$ServiceName = "DatabaseBackupService",
    [string]$InstallPath = "C:\Program Files\DatabaseBackupService",
    [switch]$RemoveConfig = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Database Backup Service - Uninstallation Script" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check if service exists
Write-Host "[1/4] Checking for service..." -ForegroundColor Yellow
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Host "  ! Service '$ServiceName' not found" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ Service found: $ServiceName" -ForegroundColor Green

    # Stop the service
    Write-Host "[2/4] Stopping service..." -ForegroundColor Yellow
    try {
        if ($service.Status -eq 'Running') {
            Stop-Service -Name $ServiceName -Force
            Start-Sleep -Seconds 2
            Write-Host "  ✓ Service stopped" -ForegroundColor Green
        } else {
            Write-Host "  ℹ Service already stopped" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  ✗ Failed to stop service: $_" -ForegroundColor Yellow
        Write-Host "  Continuing with uninstallation..." -ForegroundColor Gray
    }

    # Remove the service
    Write-Host "[3/4] Removing service..." -ForegroundColor Yellow
    try {
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Service unregistered" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Failed to delete service (exit code: $LASTEXITCODE)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "  ✗ Failed to remove service: $_" -ForegroundColor Yellow
    }
}

# Remove installation files
Write-Host "[4/4] Removing installation files..." -ForegroundColor Yellow
if (Test-Path $InstallPath) {
    try {
        $response = Read-Host "  Remove installation directory '$InstallPath'? (Y/N)"
        if ($response -eq 'Y' -or $response -eq 'y') {
            Remove-Item -Path $InstallPath -Recurse -Force
            Write-Host "  ✓ Installation files removed" -ForegroundColor Green
        } else {
            Write-Host "  ℹ Installation files kept" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  ✗ Failed to remove installation files: $_" -ForegroundColor Yellow
        Write-Host "  You may need to manually delete: $InstallPath" -ForegroundColor Gray
    }
} else {
    Write-Host "  ℹ Installation directory not found: $InstallPath" -ForegroundColor Gray
}

# Remove configuration from registry
if ($RemoveConfig) {
    Write-Host ""
    Write-Host "Removing configuration from registry..." -ForegroundColor Yellow
    try {
        $registryPath = "HKCU:\SOFTWARE\DatabaseBackupService"
        if (Test-Path $registryPath) {
            $response = Read-Host "  Remove registry configuration at '$registryPath'? (Y/N)"
            if ($response -eq 'Y' -or $response -eq 'y') {
                Remove-Item -Path $registryPath -Recurse -Force
                Write-Host "  ✓ Registry configuration removed" -ForegroundColor Green
            } else {
                Write-Host "  ℹ Registry configuration kept" -ForegroundColor Gray
            }
        } else {
            Write-Host "  ℹ No registry configuration found" -ForegroundColor Gray
        }
    } catch {
        Write-Host "  ✗ Failed to remove registry configuration: $_" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Uninstallation Complete!" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

if (-not $RemoveConfig) {
    Write-Host "Note: Configuration remains in the registry." -ForegroundColor Yellow
    Write-Host "      To remove it, run: .\uninstall-service.ps1 -RemoveConfig" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "Service '$ServiceName' has been removed from your system." -ForegroundColor Cyan
Write-Host ""
