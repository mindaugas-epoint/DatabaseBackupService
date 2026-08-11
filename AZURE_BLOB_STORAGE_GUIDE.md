# Azure Blob Storage Backup Feature

## Overview
The Database Backup Service now supports backing up databases to Azure Blob Storage in addition to (or instead of) local file backups.

## Features

### Dual Backup Support
- **Local Backup**: Traditional backup to local folder
- **Azure Blob Storage Backup**: Cloud-based backup to Azure
- **Both Options**: Can be configured to backup to both local and Azure simultaneously

### Security
- Azure Storage connection string is encrypted using Windows DPAPI
- Same security level as database passwords
- Stored securely in Windows Registry

### Automatic Container Management
- Automatically creates Azure Blob container if it doesn't exist
- No manual Azure portal configuration required (after initial setup)

## Configuration

### Azure Blob Storage Setup

1. **Create Azure Storage Account** (if not already created)
   - Go to Azure Portal (portal.azure.com)
   - Create a new Storage Account or use existing one
   - Note the storage account name

2. **Get Connection String**
   - In Azure Portal, go to your Storage Account
   - Navigate to: Security + networking → Access keys
   - Copy one of the connection strings (either key1 or key2)
   - Format: `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net`

3. **Configure in the UI**
   - Run `DatabaseBackupService.ConfigUI.exe`
   - Check "Enable Azure Blob Storage Backup"
   - Paste connection string
   - Enter container name (e.g., "database-backups")
   - Click "Test" to verify connection
   - Click "Save"

### Configuration UI Elements

#### Azure Blob Storage Section
- **Enable Azure Blob Storage Backup**: Checkbox to enable/disable Azure backups
- **Authentication Method**: Choose between Connection String or SAS Token (Recommended)
- **Connection String**: Azure Storage connection string (encrypted when saved)
- **Storage Account**: Storage account name (for SAS token method)
- **SAS Token**: Shared Access Signature token (for SAS token method, encrypted)
- **Container Name**: Name of the blob container (lowercase, no spaces)
- **Test Button**: Verifies Azure connection and creates container if needed

#### Authentication Methods

**Connection String** (Traditional):
- Full storage account access
- Simpler setup
- Suitable for single-user scenarios

**SAS Token** (Recommended):
- Limited, time-bound access
- Granular permissions
- Perfect for multiple clients
- Better security
- Easy revocation

See [AZURE_SAS_TOKEN_GUIDE.md](AZURE_SAS_TOKEN_GUIDE.md) for detailed SAS token setup.

### Container Naming Rules
Azure Blob container names must:
- Be 3-63 characters long
- Start with a letter or number
- Contain only lowercase letters, numbers, and hyphens
- Not contain consecutive hyphens
- Not end with a hyphen

Good examples:
- `database-backups`
- `prod-db-backup`
- `mycompany-sql-backups`

Bad examples:
- `Database_Backups` (uppercase not allowed)
- `db` (too short)
- `my--backups` (consecutive hyphens)

## Backup Behavior

### Both Local and Azure Enabled
When both local backup path and Azure are configured:
1. Service performs Azure backup first
2. Service performs local backup second
3. Both backups are independent (one can succeed while other fails)
4. Errors are logged separately

### Azure Only
When only Azure is enabled (no local backup path):
1. Service performs only Azure backup
2. No local files are created (except temporary file during upload)
3. Temporary files are automatically cleaned up

### Local Only
When only local backup is enabled:
1. Traditional behavior (no Azure functionality used)

## Backup File Naming

Files uploaded to Azure follow the same naming convention:

**MSSQL**: `{DatabaseName}_{yyyyMMddTHHmmss}.bak`
- Example: `ProductionDB_20240128T020000.bak`

**MySQL**: `{DatabaseName}_{yyyyMMddTHHmmss}.sql`
- Example: `MyAppDB_20240128T020000.sql`

## Technical Details

### Backup Process

#### MSSQL Azure Backup
1. Perform database backup to temporary local file
2. Upload file to Azure Blob Storage
3. Delete temporary local file
4. Log success/failure

#### MySQL Azure Backup
1. Export database to temporary local file
2. Upload file to Azure Blob Storage
3. Delete temporary local file
4. Log success/failure

### Temporary Files
- Created in system temp directory: `Path.GetTempPath()`
- Automatically deleted after upload (even if upload fails)
- No manual cleanup required

### Azure SDK
Uses `Azure.Storage.Blobs` (v12.24.0) package:
- Modern Azure SDK for .NET
- Supports async operations
- Automatic retry logic
- Connection pooling

## Registry Storage

Azure configuration is stored in Windows Registry:

**Location**: `HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService`

**New Values**:
- `EnableAzureBackup` (String): "True" or "False"
- `AzureStorageConnectionString` (Binary): Encrypted connection string
- `AzureContainerName` (String): Container name

## Testing Azure Connection

### Test Button Functionality
The "Test" button in the configuration UI:
1. Validates connection string format
2. Attempts to connect to Azure Storage
3. Creates container if it doesn't exist
4. Verifies container accessibility
5. Shows success/failure message

### Common Test Failures

**"The remote name could not be resolved"**
- Check internet connectivity
- Verify firewall/proxy settings

**"Authentication failed"**
- Verify connection string is correct
- Check Azure Storage access keys are valid
- Ensure storage account is not disabled

**"Container name is invalid"**
- Check container naming rules
- Use lowercase letters, numbers, hyphens only

## Monitoring and Logs

### Log Messages

**Azure Backup Start**:
```
[Information] Starting Azure backup of ProductionDB database
```

**Azure Backup Success**:
```
[Information] Completed Azure backup of ProductionDB database
```

**Azure Backup Failure**:
```
[Error] Failed Azure backup of ProductionDB database. Error: [error message]
```

**Local Backup Messages** (same format as before):
```
[Information] Starting local backup of ProductionDB database
[Information] Completed local backup of ProductionDB database
```

### Viewing Backups in Azure Portal

1. Go to Azure Portal
2. Navigate to your Storage Account
3. Go to Data storage → Containers
4. Click on your container name
5. View list of backup files with timestamps
6. Download or delete files as needed

## Cost Considerations

### Azure Storage Costs
- **Storage**: Pay for data stored per GB/month
- **Transactions**: Small cost per upload operation
- **Bandwidth**: Egress charges if downloading backups

### Cost Optimization Tips
1. Use **Cool** or **Archive** access tier for backups
2. Configure lifecycle management to delete old backups
3. Use **Locally Redundant Storage (LRS)** instead of GRS if regional redundancy not needed
4. Enable soft delete with retention period appropriate for your needs

### Example Monthly Cost (approximate)
For 10 GB of backups:
- LRS Cool tier: ~$0.10/month storage + minimal transactions
- GRS Hot tier: ~$0.50/month storage + minimal transactions

## Backup Retention

### Manual Management
1. Go to Azure Portal → Storage Account → Containers
2. View backup files
3. Delete old backups manually

### Automatic Management (Lifecycle Policy)
Create a lifecycle management rule in Azure:

```json
{
  "rules": [
    {
      "name": "DeleteOldBackups",
      "enabled": true,
      "type": "Lifecycle",
      "definition": {
        "filters": {
          "blobTypes": ["blockBlob"],
          "prefixMatch": ["database-backups/"]
        },
        "actions": {
          "baseBlob": {
            "delete": {
              "daysAfterModificationGreaterThan": 30
            }
          }
        }
      }
    }
  ]
}
```

This automatically deletes backups older than 30 days.

## Disaster Recovery

### Restoring from Azure Blob Storage

#### Download Backup File
```powershell
# Using Azure CLI
az storage blob download \
  --account-name mystorageaccount \
  --container-name database-backups \
  --name ProductionDB_20240128T020000.bak \
  --file C:\Restore\backup.bak
```

#### Restore MSSQL Database
```sql
RESTORE DATABASE [ProductionDB]
FROM DISK = 'C:\Restore\backup.bak'
WITH REPLACE
```

#### Restore MySQL Database
```bash
mysql -u username -p database_name < backup.sql
```

## Troubleshooting

### "Temporary file access denied"
- Ensure service account has write access to temp directory
- Check disk space availability

### "Upload failed: Timeout"
- Check internet connection speed
- Increase command timeout if backing up large databases
- Consider compression for large backups

### "Container not found after creation"
- Wait a few seconds and retry
- Verify storage account is not locked
- Check Azure subscription is active

### "Backup succeeds but not visible in portal"
- Refresh browser
- Check correct storage account and container
- Verify time zone differences

## Security Best Practices

1. **Limit Storage Account Access**
   - Use Azure RBAC to limit who can access storage
   - Consider using SAS tokens instead of account keys
   - Rotate access keys regularly

2. **Network Security**
   - Enable storage account firewall
   - Allow only specific IP addresses/ranges
   - Consider using Azure Private Link

3. **Encryption**
   - Azure Storage encrypts data at rest by default
   - Consider client-side encryption for extra security
   - Use HTTPS (enforced in connection string)

4. **Backup Validation**
   - Periodically test restore procedures
   - Verify backup file integrity
   - Monitor backup sizes for anomalies

## Limitations

1. **File Size**: Supports up to 4.75 TB per blob
2. **Upload Speed**: Depends on internet bandwidth
3. **Concurrent Uploads**: One backup at a time per service instance
4. **Container Access**: Requires internet connectivity

## Future Enhancements

Potential future features:
- Incremental backups
- Backup compression before upload
- Direct backup to Azure (SQL Server URL backup)
- Azure Managed Identity authentication
- Multi-region backup replication
- Automated restore testing
