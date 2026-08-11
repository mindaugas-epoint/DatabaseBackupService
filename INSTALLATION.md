# DatabaseBackupService - Installation Guide

This guide provides multiple methods to install and run the DatabaseBackupService as a Windows Service.

## Prerequisites

- .NET 10.0 Runtime (or .NET 10.0 SDK for building from source)
- Windows Operating System (for Windows Service deployment)
- Administrator privileges (for service installation)

## Installation Methods

### Method 1: Manual Installation (Quick Setup)

#### Step 1: Build the Service
```powershell
# Navigate to the service project directory
cd DatabaseBackupService

# Publish the service as a self-contained executable
dotnet publish -c Release -r win-x64 --self-contained -o "C:\Program Files\DatabaseBackupService"
```

#### Step 2: Configure the Service
1. Run the Configuration UI application (`DatabaseBackupService.ConfigUI.exe`)
2. Configure your database connection, backup schedule, and Azure settings (if needed)
3. Click "Save Configuration" - settings are stored in the Windows Registry

#### Step 3: Install as Windows Service
```powershell
# Open PowerShell as Administrator
# Create the Windows Service
sc.exe create "DatabaseBackupService" binPath="C:\Program Files\DatabaseBackupService\DatabaseBackupService.exe" start=auto DisplayName="Database Backup Service"

# Set service description
sc.exe description "DatabaseBackupService" "Automated database backup service with Azure Blob Storage support"

# Start the service
sc.exe start "DatabaseBackupService"
```

#### Step 4: Verify Installation
```powershell
# Check service status
sc.exe query "DatabaseBackupService"

# Or use PowerShell
Get-Service -Name "DatabaseBackupService"
```

### Method 2: Using PowerShell Script (Recommended)

We provide an automated installation script that handles all the steps above.

#### Installation Script Usage
```powershell
# Run as Administrator
.\install-service.ps1
```

See the included `install-service.ps1` for automated installation.

---

## Uninstallation

### Manual Uninstallation
```powershell
# Open PowerShell as Administrator
# Stop the service
sc.exe stop "DatabaseBackupService"

# Delete the service
sc.exe delete "DatabaseBackupService"

# Optional: Remove the program files
Remove-Item -Path "C:\Program Files\DatabaseBackupService" -Recurse -Force
```

### Using PowerShell Script
```powershell
# Run as Administrator
.\uninstall-service.ps1
```

---

## Configuration

After installation, use the **DatabaseBackupService.ConfigUI.exe** application to configure:

1. **Database Settings**
   - Database Type (MSSQL or MySQL)
   - Server Name
   - Port
   - Database Name
   - Credentials

2. **Backup Settings**
   - Backup Schedule (time of day in HH:mm:ss format)
   - Local Backup Folder Path

3. **Azure Blob Storage (Optional)**
   - Enable Azure Backup
   - Connection String
   - Container Name

Configuration is stored in the Windows Registry at:
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

**Note:** After changing configuration, restart the service for changes to take effect:
```powershell
Restart-Service -Name "DatabaseBackupService"
```

---

## Troubleshooting

### Service won't start
1. Check that configuration exists by running the Configuration UI
2. Verify database connectivity using "Test Connection" in Configuration UI
3. Check Windows Event Viewer for errors:
   ```
   Event Viewer > Windows Logs > Application
   ```

### Service Status Commands
```powershell
# Check service status
Get-Service -Name "DatabaseBackupService"

# View service details
Get-Service -Name "DatabaseBackupService" | Format-List *

# Check recent service events
Get-EventLog -LogName Application -Source "DatabaseBackupService" -Newest 10
```

### Logs Location
Service logs are typically written to the configured backup folder or the service's working directory.

---

## Advanced: Installing as Multiple Instances

If you need to backup multiple databases, you can install multiple service instances:

```powershell
# Create second instance
sc.exe create "DatabaseBackupService2" binPath="C:\Program Files\DatabaseBackupService2\DatabaseBackupService.exe" start=auto DisplayName="Database Backup Service 2"
```

Note: Each instance needs its own configuration in the registry.

---

## Creating a Simple Installer

For easier distribution, consider these options:

### Option 1: PowerShell Installation Script
The simplest approach - included in this repository:
- `install-service.ps1` - Automated installation
- `uninstall-service.ps1` - Automated removal

### Option 2: WiX Toolset (MSI Installer)
For a professional installer, use WiX to create an MSI package:
- Installs the service
- Runs the Configuration UI on first launch
- Provides Add/Remove Programs integration

### Option 3: Inno Setup
Free installer creator with a simple script-based approach:
- Easy to configure
- Creates a professional setup.exe
- Supports custom dialogs

### Option 4: ClickOnce or MSIX
Modern deployment options for Windows applications:
- Automatic updates
- Easy distribution
- Sandboxed installation

For most scenarios, **Option 1 (PowerShell scripts)** provides the simplest installation experience.

---

## Support

For issues or questions:
- Check the GitHub repository: https://github.com/mindaugas-epoint/DatabaseBackupService
- Review the Configuration UI README: `DatabaseBackupService.ConfigUI\README.md`
- Check Email Setup Guide: `DatabaseBackupService.ConfigUI\EMAIL_SETUP_GUIDE.md`
