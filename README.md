# Database Backup Service

A comprehensive Windows service for automated database backups with support for MSSQL, MySQL, local storage, and Azure Blob Storage.

## 🎯 Features

### Core Functionality
- ✅ **MSSQL Support** - Full backup support for Microsoft SQL Server
- ✅ **MySQL Support** - Full backup support for MySQL databases
- ✅ **Scheduled Backups** - Automated daily backups at configured time
- ✅ **Local Storage** - Save backups to local or network folders
- ✅ **Azure Blob Storage** - Cloud backup to Azure (optional)
- ✅ **Dual Backup** - Simultaneously backup to local and Azure
- ✅ **Secure Configuration** - Encrypted passwords using Windows DPAPI
- ✅ **Windows Forms UI** - Easy-to-use configuration interface
- ✅ **Connection Testing** - Test database and Azure connectivity before saving
- ✅ **Comprehensive Logging** - Detailed operation logs using Serilog

### Security
- 🔒 Passwords encrypted with Windows Data Protection API (DPAPI)
- 🔒 Azure connection strings encrypted
- 🔒 Registry-based configuration (user-specific)
- 🔒 No plaintext credentials stored

## 📋 Prerequisites

- Windows OS (Windows 10/11, Windows Server 2016+)
- .NET 10 Runtime
- Database server (MSSQL or MySQL)
- Azure Storage Account (optional, for cloud backups)

## 🚀 Quick Start

### Option 1: One-Click Installation (Easiest)
1. Double-click **`Install.bat`**
2. Follow the prompts
3. Run Configuration UI to set up your database
4. Start the service

### Option 2: PowerShell Installation
```powershell
# Run PowerShell as Administrator
.\install-service.ps1
```

### Option 3: Manual Installation
See **[INSTALLATION.md](INSTALLATION.md)** for detailed manual installation steps.

---

### 1. Configure the Service

Run the configuration UI:
```
DatabaseBackupService.ConfigUI.exe
```

Fill in the required information:
- **Database Type**: Select MSSQL or MySQL
- **Connection Details**: Server, port, database, username, password
- **Backup Schedule**: Time of day (e.g., 02:00:00)
- **Local Backup Path**: Folder for local backups (optional)
- **Azure Settings**: Enable and configure for cloud backups (optional)

Click **Save** to store the configuration.

### 2. Start the Service

The service is automatically installed as a Windows Service. Manage it with:
```powershell
# Start the service
Start-Service -Name "DatabaseBackupService"

# Stop the service
Stop-Service -Name "DatabaseBackupService"

# Check status
Get-Service -Name "DatabaseBackupService"
```

The service will:
- Read configuration from Windows Registry
- Connect to the database
- Wait for the scheduled backup time
- Perform backups as configured
- Log all operations

## 📦 Projects

### DatabaseBackupService
Worker service that performs the actual database backups.
- Reads configuration from Windows Registry
- Executes scheduled backups
- Supports MSSQL and MySQL
- Backs up to local folder and/or Azure Blob Storage
- Uses Serilog for logging

### DatabaseBackupService.ConfigUI
Windows Forms application for configuring the backup service.
- User-friendly interface
- Database connection testing
- Azure connection testing
- Secure configuration storage
- Input validation

## 🔧 Configuration Options

### Database Connection
- Database type (MSSQL/MySQL)
- Server name/IP
- Port number
- Database name
- Username
- Password (encrypted)

### Backup Settings
- Schedule time (HH:mm:ss format)
- Local backup folder path
- Azure Blob Storage (optional)

### Azure Blob Storage (Optional)
- Enable/disable toggle
- **Two authentication methods**:
  - **Connection String**: Traditional full-access method
  - **SAS Token** (Recommended): Secure, time-limited, granular permissions
- Storage account name (for SAS tokens)
- SAS token or connection string (encrypted)
- Container name
- Automatic container creation
- Perfect for multi-client scenarios

## 💾 Backup Options

### Local Backup Only
```
✓ Backup Folder Path: C:\Backups
✗ Azure Blob Storage: Disabled
```

### Azure Backup Only
```
✗ Backup Folder Path: (empty)
✓ Azure Blob Storage: Enabled
  - Connection String: DefaultEndpointsProtocol=https;...
  - Container: database-backups
```

### Both Local and Azure
```
✓ Backup Folder Path: C:\Backups
✓ Azure Blob Storage: Enabled
  - Connection String: DefaultEndpointsProtocol=https;...
  - Container: database-backups
```

## 📁 Backup File Naming

**MSSQL**: `{DatabaseName}_{yyyyMMddTHHmmss}.bak`
- Example: `ProductionDB_20240128T020000.bak`

**MySQL**: `{DatabaseName}_{yyyyMMddTHHmmss}.sql`
- Example: `MyAppDB_20240128T030000.sql`

## 🗂️ Configuration Storage

All settings are stored in Windows Registry:
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

**Registry Values**:
- `DatabaseType` - Database type (mssql/mysql)
- `ServerName` - Database server
- `Port` - Database port
- `DatabaseName` - Database name
- `UserName` - Database username
- `Password` - Encrypted database password
- `BackupSchedule` - Backup time
- `BackupFolderPath` - Local backup path
- `EnableAzureBackup` - Azure enabled flag
- `AzureStorageConnectionString` - Encrypted Azure connection string
- `AzureContainerName` - Azure container name

## 📖 Documentation

- **[INSTALLATION.md](INSTALLATION.md)** - **NEW!** Complete installation and uninstallation guide
- **[INSTALLER_OPTIONS.md](INSTALLER_OPTIONS.md)** - **NEW!** Installation methods comparison and MSI creation guide
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - **NEW!** Quick command reference
- **[README.md](DatabaseBackupService.ConfigUI/README.md)** - Configuration UI documentation
- **[QUICK_START_GUIDE.md](QUICK_START_GUIDE.md)** - Step-by-step setup guide
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Technical implementation details
- **[AZURE_BLOB_STORAGE_GUIDE.md](AZURE_BLOB_STORAGE_GUIDE.md)** - Complete Azure feature guide
- **[AZURE_SAS_TOKEN_GUIDE.md](AZURE_SAS_TOKEN_GUIDE.md)** - SAS Token setup and best practices
- **[AZURE_SAS_TROUBLESHOOTING.md](AZURE_SAS_TROUBLESHOOTING.md)** - Fix "Not authorized" and other SAS errors
- **[AZURE_IMPLEMENTATION_SUMMARY.md](AZURE_IMPLEMENTATION_SUMMARY.md)** - Azure implementation details
- **[EMAIL_SETUP_GUIDE.md](DatabaseBackupService.ConfigUI/EMAIL_SETUP_GUIDE.md)** - Email notification configuration

## 📦 Installation Files

- **`Install.bat`** - One-click installer (double-click to install)
- **`Uninstall.bat`** - One-click uninstaller  
- **`install-service.ps1`** - PowerShell installation script
- **`uninstall-service.ps1`** - PowerShell uninstallation script

## 🔍 Troubleshooting

### Database Connection Issues
- Verify server name and port
- Check database credentials
- Ensure database server is accessible
- Check firewall settings

### Azure Connection Issues
- Verify Azure connection string
- Check internet connectivity
- Ensure storage account is accessible
- Validate container name format

### Backup Not Running
- Check backup path exists and is writable
- Verify service is running
- Check scheduled time is correct
- Review service logs

## 📊 Logging

The service uses Serilog for comprehensive logging:
- Console output for real-time monitoring
- File logging for historical records
- Structured logging for easy parsing

Log messages include:
- Service start/stop
- Backup start/completion
- Errors and exceptions
- Azure upload status

## 🛠️ Installation as Windows Service

The installation scripts handle Windows Service setup automatically. For manual installation:

```powershell
# Publish the service
dotnet publish DatabaseBackupService\DatabaseBackupService.csproj -c Release -r win-x64 --self-contained -o "C:\Program Files\DatabaseBackupService"

# Create Windows Service
sc.exe create "DatabaseBackupService" binPath="C:\Program Files\DatabaseBackupService\DatabaseBackupService.exe" start=auto

# Start service
sc.exe start "DatabaseBackupService"
```

**Important**: The service reads configuration from the Windows Registry under `HKEY_CURRENT_USER`. Ensure the service runs under the account that configured the service.

See **[INSTALLATION.md](INSTALLATION.md)** for complete installation options including:
- One-click installation
- PowerShell scripts
- Manual installation
- Multiple service instances
- Creating MSI installers

## 🔐 Security Best Practices

1. **Use Dedicated Database Account**
   - Create specific user for backups
   - Grant only backup permissions
   - Use strong passwords

2. **Secure Local Backups**
   - Use encrypted drives
   - Set appropriate NTFS permissions
   - Regular security audits

3. **Secure Azure Backups**
   - Use Azure Storage firewall
   - Enable Azure Storage encryption
   - Rotate access keys regularly
   - Consider Azure Managed Identity (future enhancement)

4. **Monitor Service Account**
   - Use dedicated service account
   - Restrict account permissions
   - Regular password rotation

## 📈 Azure Cost Optimization

- Use **Cool** or **Archive** access tier for backups
- Configure lifecycle management to auto-delete old backups
- Use **Locally Redundant Storage (LRS)** if multi-region not needed
- Monitor storage usage and costs regularly

## 🚀 Future Enhancements

Potential features:
- [ ] Multiple database backup support
- [ ] Backup compression
- [ ] Incremental backups
- [x] Email notifications (implemented)
- [ ] Backup verification
- [ ] Azure Managed Identity authentication
- [ ] Web-based configuration interface
- [ ] Backup retention policies
- [ ] Restore functionality
- [ ] Backup encryption
- [x] Windows Service installer scripts (implemented)
- [ ] MSI/WiX installer package

## 🤝 Contributing

This is a private repository. For issues or suggestions, contact the repository owner.

## 📄 License

Proprietary - All rights reserved

## 👥 Authors

- mindaugas-epoint

## 🆘 Support

For support:
1. Check the documentation files listed above
2. Review service logs for error details
3. Test connections using the configuration UI
4. Contact repository maintainer

---

**Version**: 2.0  
**Last Updated**: 2024-01-28  
**Status**: Production Ready ✅
