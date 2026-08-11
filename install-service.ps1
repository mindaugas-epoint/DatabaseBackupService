#Requires -RunAsAdministrator

<#
.SYNOPSIS
    Installs the Database Backup Service as a Windows Service.

.DESCRIPTION
    This script automates the installation of DatabaseBackupService:
    1. Publishes the service as a self-contained executable
    2. Copies files to Program Files
    3. Registers the Windows Service
    4. Optionally launches the Configuration UI

.PARAMETER ServiceName
    The name of the Windows Service. Default: "DatabaseBackupService"

.PARAMETER InstallPath
    The installation directory. Default: "C:\Program Files\DatabaseBackupService"

.PARAMETER AutoStart
    Whether to automatically start the service after installation. Default: $false

.EXAMPLE
    .\install-service.ps1

.EXAMPLE
    .\install-service.ps1 -ServiceName "DatabaseBackupService" -AutoStart $true
#>

param(
    [string]$ServiceName = "DatabaseBackupService",
    [string]$InstallPath = "C:\Program Files\DatabaseBackupService",
    [switch]$AutoStart = $false
)

$ErrorActionPreference = "Stop"

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Database Backup Service - Installation Script" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check if .NET 10 SDK is installed
Write-Host "[1/6] Checking for .NET 10 SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✓ Found .NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ✗ .NET SDK not found. Please install .NET 10 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check if service already exists
Write-Host "[2/6] Checking for existing service..." -ForegroundColor Yellow
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "  ! Service '$ServiceName' already exists." -ForegroundColor Yellow
    $response = Read-Host "  Do you want to uninstall and reinstall? (Y/N)"
    if ($response -eq 'Y' -or $response -eq 'y') {
        Write-Host "  Stopping service..." -ForegroundColor Yellow
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2

        Write-Host "  Removing service..." -ForegroundColor Yellow
        sc.exe delete $ServiceName
        Start-Sleep -Seconds 2
        Write-Host "  ✓ Existing service removed" -ForegroundColor Green
    } else {
        Write-Host "  Installation cancelled." -ForegroundColor Yellow
        exit 0
    }
} else {
    Write-Host "  ✓ No existing service found" -ForegroundColor Green
}

# Build and publish the service
Write-Host "[3/6] Building and publishing the service..." -ForegroundColor Yellow
$projectPath = Join-Path $PSScriptRoot "DatabaseBackupService\DatabaseBackupService.csproj"

if (-not (Test-Path $projectPath)) {
    Write-Host "  ✗ Project file not found at: $projectPath" -ForegroundColor Red
    exit 1
}

try {
    $publishOutput = Join-Path $PSScriptRoot "publish"

    Write-Host "  Publishing to: $publishOutput" -ForegroundColor Gray
    dotnet publish $projectPath -c Release -r win-x64 --self-contained -o $publishOutput

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Service built successfully" -ForegroundColor Green
    } else {
        throw "Build failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host "  ✗ Failed to build service: $_" -ForegroundColor Red
    exit 1
}

# Create installation directory and copy files
Write-Host "[4/6] Installing service files..." -ForegroundColor Yellow
try {
    if (Test-Path $InstallPath) {
        Write-Host "  Removing existing installation directory..." -ForegroundColor Gray
        Remove-Item -Path $InstallPath -Recurse -Force
    }

    Write-Host "  Creating installation directory: $InstallPath" -ForegroundColor Gray
    New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null

    Write-Host "  Copying files..." -ForegroundColor Gray
    Copy-Item -Path "$publishOutput\*" -Destination $InstallPath -Recurse -Force

    Write-Host "  ✓ Service files installed" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Failed to install files: $_" -ForegroundColor Red
    exit 1
}

# Register the Windows Service
Write-Host "[5/6] Registering Windows Service..." -ForegroundColor Yellow
try {
    $serviceBinaryPath = Join-Path $InstallPath "DatabaseBackupService.exe"

    if (-not (Test-Path $serviceBinaryPath)) {
        throw "Service executable not found at: $serviceBinaryPath"
    }

    sc.exe create $ServiceName binPath=$serviceBinaryPath start=auto DisplayName="Database Backup Service"

    if ($LASTEXITCODE -eq 0) {
        sc.exe description $ServiceName "Automated database backup service with Azure Blob Storage support"
        Write-Host "  ✓ Windows Service registered" -ForegroundColor Green
    } else {
        throw "Failed to create service with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host "  ✗ Failed to register service: $_" -ForegroundColor Red
    exit 1
}

# Start the service if requested
Write-Host "[6/6] Finalizing installation..." -ForegroundColor Yellow
if ($AutoStart) {
    try {
        Write-Host "  Starting service..." -ForegroundColor Gray
        Start-Service -Name $ServiceName
        Write-Host "  ✓ Service started successfully" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Failed to start service: $_" -ForegroundColor Yellow
        Write-Host "  You can start it manually after configuration." -ForegroundColor Yellow
    }
} else {
    Write-Host "  ℹ Service installed but not started (use -AutoStart to start automatically)" -ForegroundColor Gray
}

# Clean up publish folder
Write-Host "  Cleaning up temporary files..." -ForegroundColor Gray
Remove-Item -Path $publishOutput -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Installation Complete!" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "  1. Run the Configuration UI to set up your database connection:" -ForegroundColor White
Write-Host "     $InstallPath\..\DatabaseBackupService.ConfigUI.exe" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. After configuration, start the service:" -ForegroundColor White
Write-Host "     Start-Service -Name '$ServiceName'" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Check service status:" -ForegroundColor White
Write-Host "     Get-Service -Name '$ServiceName'" -ForegroundColor Gray
Write-Host ""
Write-Host "Service Name: $ServiceName" -ForegroundColor Cyan
Write-Host "Install Path: $InstallPath" -ForegroundColor Cyan
Write-Host ""

# Ask if user wants to launch Configuration UI
$configUIPath = Join-Path $PSScriptRoot "DatabaseBackupService.ConfigUI\bin\Release\net10.0-windows\DatabaseBackupService.ConfigUI.exe"
if (Test-Path $configUIPath) {
    $response = Read-Host "Would you like to launch the Configuration UI now? (Y/N)"
    if ($response -eq 'Y' -or $response -eq 'y') {
        Start-Process $configUIPath
    }
} else {
    Write-Host "Note: Build the Configuration UI project to configure the service." -ForegroundColor Yellow
}
