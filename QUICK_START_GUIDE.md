# Quick Start Guide - Database Backup Service

## Step 1: Configure the Backup Service

1. **Run the Configuration UI**
   ```
   DatabaseBackupService.ConfigUI.exe
   ```

2. **Fill in Database Connection Details**
   - **Database Type**: Select "mssql" or "mysql"
   - **Server Name**: Enter your database server (e.g., "localhost", "192.168.1.100")
   - **Port**: Auto-filled (1433 for MSSQL, 3306 for MySQL)
   - **Database Name**: Enter the database to backup
   - **Username**: Database user with backup permissions
   - **Password**: Database password (will be encrypted)

3. **Configure Backup Settings**
   - **Backup Schedule**: Enter time in HH:mm:ss format (e.g., "02:00:00" for 2 AM)
   - **Backup Path**: Click "Browse..." to select backup folder (for local backups)

4. **Configure Azure Blob Storage** (Optional - for cloud backups)
   - Check "Enable Azure Blob Storage Backup"
   - **Choose Authentication Method**:
     - **SAS Token (Recommended)**: For production, multiple clients, better security
     - **Connection String**: For simple scenarios
   - **If using SAS Token**:
     - **Storage Account**: Enter storage account name (e.g., "mystorageaccount")
     - **SAS Token**: Paste SAS token (see AZURE_SAS_TOKEN_GUIDE.md for generation)
     - **Container Name**: Enter container name (e.g., "database-backups")
   - **If using Connection String**:
     - **Connection String**: Paste Azure Storage connection string
     - **Container Name**: Enter container name
   - Click "Test" to verify Azure connection

5. **Test Connection** (Recommended)
   - Click "Test Connection" to verify database connectivity
   - Ensure successful connection before saving

6. **Save Configuration**
   - Click "Save"
   - Configuration is saved to Windows Registry with encrypted password

## Step 2: Run the Backup Service

1. **Start the Service**
   ```
   DatabaseBackupService.exe
   ```

2. **What Happens**
   - Service reads configuration from Windows Registry
   - Service connects to database
   - Service waits for scheduled backup time
   - Service performs backup at specified time daily
   - Service logs all operations

## Example Configuration

### MSSQL with Local Backup
```
Database Type: mssql
Server Name: localhost
Port: 1433
Database Name: MyProductionDB
Username: sa
Password: ********
Backup Schedule: 02:00:00
Backup Path: C:\DatabaseBackups
Azure Backup: Disabled
```

### MySQL with Azure Backup (SAS Token)
```
Database Type: mysql
Server Name: 192.168.1.100
Port: 3306
Database Name: myapp_db
Username: backup_user
Password: ********
Backup Schedule: 03:00:00
Backup Path: (empty - Azure only)
Azure Backup: Enabled
Auth Method: SAS Token (Recommended)
Storage Account: clientstorage
SAS Token: ?sv=2021-06-08&ss=b&srt=sco&sp=wcl...
Azure Container: myapp-backups
```

### MSSQL with Both Local and Azure
```
Database Type: mssql
Server Name: sqlserver.example.com
Port: 1433
Database Name: ProductionDB
Username: backup_admin
Password: ********
Backup Schedule: 01:00:00
Backup Path: D:\Backups
Azure Backup: Enabled
Azure Connection String: DefaultEndpointsProtocol=https;AccountName=...
Azure Container: prod-db-backups
```

## Backup File Naming

### Local Backups
Backups are saved with timestamp in filename:
```
{DatabaseName}_{yyyyMMddTHHmmss}.bak  (MSSQL)
{DatabaseName}_{yyyyMMddTHHmmss}.sql  (MySQL)

Examples:
MyProductionDB_20240128T020000.bak
myapp_db_20240128T030000.sql
```

### Azure Blob Storage Backups
Same naming convention, stored in the specified container:
```
Container: database-backups
Blobs:
  - MyProductionDB_20240128T020000.bak
  - MyProductionDB_20240129T020000.bak
  - MyProductionDB_20240130T020000.bak
```

## Troubleshooting

### "No configuration found in registry"
- Run the Configuration UI and save settings first

### "Connection failed"
- Verify server name and port
- Check database user credentials
- Ensure database server is accessible
- Check firewall settings

### "Invalid backup schedule format"
- Use HH:mm:ss format (e.g., 02:00:00)
- Hours: 00-23, Minutes: 00-59, Seconds: 00-59

### Backup not running
- Ensure backup path exists and is writable (for local backups)
- Check Azure connection and credentials (for Azure backups)
- Check service is running under correct user account
- Verify scheduled time is correct

### Azure connection failed
- Verify Azure connection string is correct
- Check internet connectivity
- Ensure storage account is accessible
- Verify container name follows Azure naming rules (lowercase, no spaces)

## Security Notes

- ✅ Password encrypted using Windows DPAPI
- ✅ Configuration stored in Windows Registry
- ⚠️ Service must run under same user account that configured it
- ⚠️ Backup folder must be accessible by service account

## Service Installation (Optional)

To install as Windows Service:

1. Open PowerShell as Administrator
2. Run:
   ```powershell
   sc.exe create "DatabaseBackupService" binPath= "C:\Path\To\DatabaseBackupService.exe"
   sc.exe start "DatabaseBackupService"
   ```

3. Configure service to run under your user account:
   - Open Services (services.msc)
   - Find "DatabaseBackupService"
   - Properties → Log On tab
   - Select "This account" and enter your credentials

## Logs

Check service logs for backup status and errors.
Log location depends on your logging configuration in the service.

## Support

For issues or questions, refer to:
- `README.md` in DatabaseBackupService.ConfigUI folder
- `IMPLEMENTATION_SUMMARY.md` for technical details
- `AZURE_BLOB_STORAGE_GUIDE.md` for Azure-specific information and troubleshooting
- `AZURE_SAS_TOKEN_GUIDE.md` for SAS token setup and best practices
