# Database Backup Service - Quick Reference

## Installation Commands

### Easy Install (Double-Click)
```
Install.bat          → Installs the service
Uninstall.bat        → Removes the service
```

### PowerShell (Run as Administrator)
```powershell
.\install-service.ps1              # Install service
.\install-service.ps1 -AutoStart   # Install and start immediately
.\uninstall-service.ps1            # Remove service
.\uninstall-service.ps1 -RemoveConfig  # Remove service + configuration
```

---

## Service Management

### Start/Stop Service
```powershell
Start-Service -Name "DatabaseBackupService"
Stop-Service -Name "DatabaseBackupService"
Restart-Service -Name "DatabaseBackupService"
```

### Check Service Status
```powershell
Get-Service -Name "DatabaseBackupService"
Get-Service -Name "DatabaseBackupService" | Format-List *
```

### View Service Logs
```powershell
Get-EventLog -LogName Application -Source "DatabaseBackupService" -Newest 20
```

---

## Configuration

### Launch Configuration UI
```
DatabaseBackupService.ConfigUI.exe
```

### Registry Location
```
HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService
```

### View Configuration
```powershell
Get-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService"
```

---

## File Locations

### Service Installation
```
C:\Program Files\DatabaseBackupService\
```

### Backup Files
```
{BackupFolderPath}\{DatabaseName}_{timestamp}.bak     # MSSQL
{BackupFolderPath}\{DatabaseName}_{timestamp}.sql     # MySQL
```

### Azure Backup
```
{ContainerName}/{DatabaseName}_{timestamp}.bak        # MSSQL
{ContainerName}/{DatabaseName}_{timestamp}.sql        # MySQL
```

---

## Common Tasks

### After Configuration Change
```powershell
Restart-Service -Name "DatabaseBackupService"
```

### Test Database Connection
1. Open Configuration UI
2. Click "Test Connection"
3. Verify success message

### Test Azure Connection
1. Open Configuration UI
2. Enable Azure Backup
3. Click "Test Azure"
4. Verify success message

### Check Last Backup
```powershell
# Check local backups
Get-ChildItem -Path "C:\Backups" | Sort-Object LastWriteTime -Descending | Select-Object -First 5

# Check service status
Get-Service -Name "DatabaseBackupService"
```

---

## Troubleshooting Quick Fixes

### Service Won't Start
```powershell
# Check configuration exists
Get-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService" -ErrorAction SilentlyContinue

# Check service status
Get-Service -Name "DatabaseBackupService" | Format-List *

# View recent errors
Get-EventLog -LogName Application -EntryType Error -Newest 10 | Where-Object {$_.Source -eq "DatabaseBackupService"}
```

### Backup Not Running
1. Verify service is running: `Get-Service -Name "DatabaseBackupService"`
2. Check scheduled time in Configuration UI
3. Ensure backup folder exists and is writable
4. Verify database credentials

### Azure Upload Fails
1. Test Azure connection in Configuration UI
2. Verify connection string/SAS token
3. Check container name (lowercase only)
4. Ensure internet connectivity

---

## Installation Paths

| Component | Default Path |
|-----------|-------------|
| Service Binary | `C:\Program Files\DatabaseBackupService\` |
| Configuration | `HKCU:\SOFTWARE\DatabaseBackupService` |
| Logs | Service folder or backup folder |
| Local Backups | User-configured path |
| Azure Backups | Azure Blob Storage container |

---

## Support Files

| File | Description |
|------|-------------|
| `INSTALLATION.md` | Complete installation guide |
| `README.md` | Main documentation |
| `DatabaseBackupService.ConfigUI\README.md` | Config UI guide |
| `EMAIL_SETUP_GUIDE.md` | Email configuration |
| `AZURE_*.md` | Azure-related guides |

---

## One-Liners

```powershell
# Quick service restart
Restart-Service -Name "DatabaseBackupService"

# View service details
Get-Service -Name "DatabaseBackupService" | Select-Object Name, Status, StartType

# Check if service exists
Get-Service -Name "DatabaseBackupService" -ErrorAction SilentlyContinue

# Force stop service
Stop-Service -Name "DatabaseBackupService" -Force

# List recent backups
Get-ChildItem "C:\Backups\*.bak" | Sort-Object LastWriteTime -Descending | Select-Object Name, LastWriteTime -First 10
```

---

**Quick Help**: For detailed information, see [INSTALLATION.md](INSTALLATION.md)
