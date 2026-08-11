# Database Backup Service Configuration UI

This Windows Forms application provides a user-friendly interface to configure the Database Backup Service.

## Installation

Before using this Configuration UI, install the Database Backup Service:

**Easy Installation:**
1. Double-click **`Install.bat`** in the root folder
2. Or run **`install-service.ps1`** in PowerShell (as Administrator)

See **[INSTALLATION.md](../INSTALLATION.md)** for complete installation instructions.

## Features

### Database Configuration
- **Database Type Selection**: Choose between MSSQL or MySQL
- **Connection Details**:
  - Server Name
  - Port (auto-populated with default values: 1433 for MSSQL, 3306 for MySQL)
  - Database Name
  - Username
  - Password (displayed with password masking)

### Backup Settings
- **Backup Schedule**: Set the time of day for automatic backups (format: HH:mm:ss, e.g., 02:00:00)
- **Backup Folder Path**: Specify where backup files should be stored locally

### Azure Blob Storage (Optional)
- **Enable Azure Backup**: Enable cloud-based backups to Azure Blob Storage
- **Connection String**: Azure Storage account connection string (encrypted)
- **Container Name**: Blob container name for storing backups
- **Test Connection**: Verify Azure connectivity before saving

### Additional Features
- **Test Connection**: Verify database connectivity before saving
- **Test Azure**: Verify Azure Blob Storage connectivity
- **Secure Storage**: All configuration is saved to Windows Registry
- **Password Encryption**: Passwords and Azure connection strings are encrypted using Windows Data Protection API (DPAPI)
- **Dual Backup**: Supports both local and Azure backups simultaneously

## Registry Storage

Configuration is stored in:
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

### Registry Values:
- `DatabaseType` (String): "mssql" or "mysql"
- `ServerName` (String): Database server name or IP
- `Port` (String): Database server port
- `DatabaseName` (String): Name of the database to backup
- `UserName` (String): Database username
- `Password` (Binary): Encrypted password using Windows DPAPI
- `BackupSchedule` (String): Time of day for backups (HH:mm:ss format)
- `BackupFolderPath` (String): Path where backups are stored locally
- `EnableAzureBackup` (String): "True" or "False"
- `AzureStorageConnectionString` (Binary): Encrypted Azure connection string
- `AzureContainerName` (String): Azure Blob container name

## Security

### Password Encryption
Passwords are encrypted using the Windows Data Protection API (DPAPI) with `DataProtectionScope.CurrentUser`. This means:
- Passwords can only be decrypted by the same user account on the same machine
- No encryption key management is required
- Provides Windows-native security

### Encryption Implementation
```csharp
// Encryption
byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
byte[] encryptedBytes = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);

// Decryption
byte[] decryptedBytes = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);
string password = Encoding.UTF8.GetString(decryptedBytes);
```

## Usage

1. **Launch the Configuration UI**
   - Run `DatabaseBackupService.ConfigUI.exe`

2. **Configure Database Connection**
   - Select database type (MSSQL or MySQL)
   - Enter server name, port, database name
   - Enter username and password

3. **Configure Backup Settings**
   - Set backup schedule time (e.g., 02:00:00 for 2 AM)
   - Select backup folder path (for local backups)

4. **Configure Azure Blob Storage** (Optional)
   - Check "Enable Azure Blob Storage Backup"
   - Enter Azure Storage connection string
   - Enter container name (e.g., "database-backups")
   - Click "Test" to verify Azure connection

5. **Test Connection** (Optional)
   - Click "Test Connection" to verify database connectivity

6. **Save Configuration**
   - Click "Save" to store configuration in Windows Registry

7. **Start the Backup Service**
   - Run `DatabaseBackupService.exe` to start the backup service
   - The service will read configuration from the registry and perform backups according to the schedule

## Backup Options

### Local Only
- Enter backup folder path
- Leave Azure option unchecked
- Backups saved to local folder

### Azure Only
- Check "Enable Azure Blob Storage Backup"
- Configure Azure settings
- Leave backup folder path empty or provide one for dual backup

### Both Local and Azure
- Enter backup folder path
- Check "Enable Azure Blob Storage Backup"
- Configure Azure settings
- Service performs both backups at scheduled time

## Requirements

- Windows Operating System
- .NET 10 Runtime
- Appropriate database connectivity (MSSQL or MySQL client libraries)
- User account must have write permissions to backup folder path

## Architecture

### Components

1. **MainForm.cs**: Main UI form with validation and connection testing
2. **RegistryConfigManager.cs**: Handles registry read/write operations and password encryption
3. **BackupConfig.cs**: Data model for configuration

### Integration with Backup Service

The Database Backup Service (`DatabaseBackupService.exe`) reads configuration from the registry using `RegistryConfigReader` class:
- Loads connection details
- Decrypts password
- Builds connection string
- Executes backups according to schedule

## Validation

The application validates:
- All required fields are filled
- Port is a valid number
- Backup schedule is in valid time format (HH:mm:ss)
- Database connection is valid (when using Test Connection)

## Error Handling

- Configuration load/save errors are displayed to the user
- Connection test failures show detailed error messages
- Invalid input is caught with clear validation messages
