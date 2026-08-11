# .NET Framework 4.8 Projects

This folder contains .NET Framework 4.8 versions of the Database Backup Service and Configuration UI.

## Projects

### DatabaseBackupService.NetFx
- **Type**: Windows Service (.NET Framework 4.8)
- **Purpose**: Background service for automated database backups
- **Based on**: Classic Windows Service using ServiceBase class

### DatabaseBackupService.ConfigUI.NetFx  
- **Type**: Windows Forms Application (.NET Framework 4.8)
- **Purpose**: Configuration UI for the backup service
- **Framework**: .NET Framework 4.8 WinForms

## Key Differences from .NET 10 Version

### Service Project
1. **Service Model**: Uses `ServiceBase` instead of `BackgroundService`
2. **Dependency Injection**: Manual instantiation instead of DI container
3. **Package Versions**: Older versions compatible with .NET Framework 4.8
   - Azure.Storage.Blobs 12.13.0 (vs 12.24.0)
   - Microsoft.Data.SqlClient 5.1.0 (vs 7.0.0)
   - MySql.Data 8.0.33 (vs 9.6.0)
   - Serilog 2.12.0 (vs 4.3.1)

### UI Project
1. **Project Format**: Classic .csproj format instead of SDK-style
2. **Initialization**: Uses `Application.EnableVisualStyles()` instead of `ApplicationConfiguration.Initialize()`
3. **Property Initializers**: Explicit constructor initialization instead of inline property initializers

## Installation

### Service Installation
```powershell
# Install the service
sc.exe create "DatabaseBackupService" binPath= "C:\Path\To\DatabaseBackupService.NetFx.exe" start= auto

# Or use InstallUtil
InstallUtil.exe DatabaseBackupService.NetFx.exe

# Start the service
sc.exe start "DatabaseBackupService"
```

### Service Uninstallation
```powershell
# Stop the service
sc.exe stop "DatabaseBackupService"

# Delete the service
sc.exe delete "DatabaseBackupService"

# Or use InstallUtil
InstallUtil.exe /u DatabaseBackupService.NetFx.exe
```

## Building

### Prerequisites
- Visual Studio 2019 or later (with .NET Framework 4.8 support)
- .NET Framework 4.8 Developer Pack

### Build Commands
```powershell
# Restore NuGet packages
nuget restore DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj
nuget restore DatabaseBackupService.ConfigUI.NetFx\DatabaseBackupService.ConfigUI.NetFx.csproj

# Build
msbuild DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj /p:Configuration=Release
msbuild DatabaseBackupService.ConfigUI.NetFx\DatabaseBackupService.ConfigUI.NetFx.csproj /p:Configuration=Release
```

## Shared Code

Both projects share the same business logic with the .NET 10 versions:
- Database backup implementations (SQL Server & MySQL)
- Registry configuration management
- Email notifications
- Azure Blob Storage integration
- Logging with Serilog

## Configuration

Configuration is stored in the Windows Registry under:
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

Use the Configuration UI (DatabaseBackupService.ConfigUI.NetFx.exe) to manage settings.

## Limitations

Compared to the .NET 10 version:
1. No cross-platform support (Windows only)
2. Older package versions with fewer features
3. No built-in dependency injection
4. No modern C# 10+ features (global usings, file-scoped namespaces, etc.)
5. Requires .NET Framework 4.8 runtime

## Why Use .NET Framework Version?

Consider the .NET Framework version if:
- You need to run on systems where modern .NET cannot be installed
- You have specific .NET Framework dependencies
- Your organization mandates .NET Framework 4.8

**Recommendation**: Use the .NET 10 version unless you have a specific requirement for .NET Framework.

## Support

Both versions use the same registry configuration, so the Configuration UI from either version can configure services from both versions.
