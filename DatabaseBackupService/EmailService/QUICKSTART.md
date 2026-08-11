# Quick Start - Gmail Email Notifications

## Step 1: Get Gmail App Password
1. Enable 2FA: https://myaccount.google.com/security
2. Create App Password: https://myaccount.google.com/apppasswords
3. Copy the 16-character password (e.g., `abcd efgh ijkl mnop`)

## Step 2: Configure Registry Settings

### Option A: Using PowerShell (Recommended)

Run PowerShell as Administrator and execute:

```powershell
# Set email sender (your Gmail address)
New-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService" -Name "EmailSenderAddress" -Value "yourname@gmail.com" -PropertyType String -Force

# Set email recipient (who receives alerts)
New-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService" -Name "EmailRecipientAddress" -Value "admin@yourcompany.com" -PropertyType String -Force

# Set encrypted password (replace YOUR_APP_PASSWORD with the 16-char password from Gmail)
$password = "abcd efgh ijkl mnop"  # Remove spaces: abcdefghijklmnop
$securePassword = $password | ConvertTo-SecureString -AsPlainText -Force
$encryptedPassword = [System.Security.Cryptography.ProtectedData]::Protect(
    [System.Text.Encoding]::UTF8.GetBytes($password),
    $null,
    [System.Security.Cryptography.DataProtectionScope]::CurrentUser
)
New-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService" -Name "EmailSenderPassword" -Value $encryptedPassword -PropertyType Binary -Force

Write-Host "Email configuration saved successfully!" -ForegroundColor Green
```

### Option B: Manual Registry Edit

1. Press `Win + R`, type `regedit`, press Enter
2. Navigate to: `HKEY_CURRENT_USER\SOFTWARE\DatabaseBackupService`
3. Add String values:
   - Name: `EmailSenderAddress`, Value: `yourname@gmail.com`
   - Name: `EmailRecipientAddress`, Value: `admin@yourcompany.com`
4. For the password, it's recommended to use the PowerShell method above for encryption

## Step 3: Restart the Service

```powershell
Restart-Service -Name "DatabaseBackupService"
```

## Step 4: Test

To verify the configuration is working:
1. Check the service logs for any email-related errors
2. You can manually trigger a backup failure to test email notifications
3. The email will be sent only when a backup actually fails

## Example Email Configuration

```
Sender: yourname@gmail.com
Recipient: admin@yourcompany.com
SMTP Server: smtp.gmail.com (default)
SMTP Port: 587 (default)
```

## Verification Checklist

✅ Gmail 2FA enabled  
✅ App Password created (16 characters)  
✅ `EmailSenderAddress` set in registry  
✅ `EmailSenderPassword` set in registry (encrypted)  
✅ `EmailRecipientAddress` set in registry  
✅ Service restarted  
✅ Firewall allows outbound port 587  

## What Happens When Backup Fails?

When a database backup fails, you'll receive an HTML email with:
- **Subject**: "Database Backup Service - Backup Failure Alert"
- **Content**: 
  - Timestamp of failure
  - Number of failed backups
  - Database name
  - Error details

The email will look professional with color-coded alerts and formatted error messages.

## Need Help?

Check the service logs for detailed error messages:
```powershell
Get-EventLog -LogName Application -Source "DatabaseBackupService" -Newest 20
```

Or check the README.md in the EmailService folder for complete documentation.
