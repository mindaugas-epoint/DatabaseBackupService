# Azure SAS Token Configuration Guide

## Overview
The Database Backup Service now supports **Shared Access Signature (SAS)** tokens for Azure Blob Storage authentication. This is the **recommended method** for multi-client scenarios and production environments.

## Why Use SAS Tokens?

### Advantages Over Connection Strings

| Feature | Connection String | SAS Token |
|---------|------------------|-----------|
| **Security** | Full account access | Limited permissions |
| **Scope** | Entire storage account | Specific container |
| **Time Limit** | Permanent (until rotated) | Can expire automatically |
| **Revocation** | Requires key rotation | Simply delete/expire token |
| **Client Isolation** | All clients share same key | Each client gets unique token |
| **Least Privilege** | All permissions | Only required permissions |
| **Audit Trail** | Limited | Better tracking per token |

### Perfect For
- ✅ **Multiple clients** - Each client gets their own token
- ✅ **Service providers** - Issue tokens to customers without sharing account keys
- ✅ **Time-limited access** - Tokens can expire automatically
- ✅ **Granular permissions** - Only grant write access, not read/delete
- ✅ **Security compliance** - Follow principle of least privilege

## Generating SAS Tokens

### Important: Create Container First!

**CRITICAL**: When using container-scoped SAS tokens, you **must create the container in Azure Portal BEFORE generating the SAS token**.

Container-scoped SAS tokens cannot create containers - they can only access existing containers. This is by design for security.

**Steps**:
1. **Create Container First** (in Azure Portal)
2. **Then Generate SAS Token** for that container
3. **Then Configure** the backup service

### Option 1: Azure Portal (Recommended for beginners)

1. **Navigate to Storage Account**
   - Go to Azure Portal (portal.azure.com)
   - Open your Storage Account

2. **Select Container**
   - Go to **Data storage** → **Containers**
   - Click on your container (e.g., "database-backups")
   - Or create new container

3. **Generate SAS**
   - Click **Generate SAS** in the right menu
   - Or right-click container → **Generate SAS**

4. **Configure SAS Settings**:
   - **Permissions**: Select **Write**, **Create**, **List** (uncheck Read and Delete for security)
   - **Start time**: Now (or future date)
   - **Expiry time**: Set appropriate expiration (e.g., 1 year)
   - **Allowed IP addresses**: (Optional) Restrict to specific IPs
   - **Allowed protocols**: HTTPS only
   - **Signing method**: Account key

5. **Generate and Copy**
   - Click **Generate SAS token and URL**
   - Copy the **SAS token** (starts with `?sv=`)
   - Do NOT copy the full URL, just the token part

**Example SAS Token**:
```
?sv=2021-06-08&ss=b&srt=sco&sp=wcl&se=2025-12-31T23:59:59Z&st=2024-01-28T00:00:00Z&spr=https&sig=AbCd...XyZ%3D
```

### Option 2: Azure CLI

```bash
# Set variables
STORAGE_ACCOUNT="mystorageaccount"
CONTAINER_NAME="database-backups"
EXPIRY_DATE="2025-12-31"

# Generate SAS token for container
az storage container generate-sas \
  --account-name $STORAGE_ACCOUNT \
  --name $CONTAINER_NAME \
  --permissions wcl \
  --expiry $EXPIRY_DATE \
  --https-only \
  --output tsv
```

### Option 3: PowerShell

```powershell
# Set variables
$StorageAccountName = "mystorageaccount"
$ContainerName = "database-backups"
$ExpiryDate = (Get-Date).AddYears(1)

# Get storage context
$Context = New-AzStorageContext -StorageAccountName $StorageAccountName -UseConnectedAccount

# Generate SAS token
$SasToken = New-AzStorageContainerSASToken `
    -Name $ContainerName `
    -Context $Context `
    -Permission "wcl" `
    -ExpiryTime $ExpiryDate `
    -Protocol HttpsOnly
    
Write-Host "SAS Token: $SasToken"
```

## Configuration in Backup Service

### Step 1: Open Configuration UI
Run `DatabaseBackupService.ConfigUI.exe`

### Step 2: Enable Azure Backup
Check **"Enable Azure Blob Storage Backup"**

### Step 3: Select SAS Token Method
Select **"SAS Token (Recommended)"** radio button

### Step 4: Enter Credentials
- **Storage Account**: Your storage account name (e.g., `mystorageaccount`)
  - **NOT** the full URL
  - Just the account name
- **SAS Token**: Paste the token you generated
  - Can start with or without `?`
  - System will handle formatting
- **Container Name**: Container name (e.g., `database-backups`)

### Step 5: Test Connection
Click **"Test"** to verify:
- ✅ Storage account is accessible
- ✅ SAS token is valid
- ✅ Permissions are correct
- ✅ Container exists or can be created

### Step 6: Save
Click **"Save"** to store configuration (SAS token is encrypted)

## Example Configuration

### Good Examples

**Example 1: Production Database**
```
Storage Account: prodstorageaccount
SAS Token: ?sv=2021-06-08&ss=b&srt=sco&sp=wcl&se=2025-12-31T23:59:59Z...
Container Name: production-db-backups
```

**Example 2: Client-Specific**
```
Storage Account: clientstorage
SAS Token: ?sv=2021-06-08&ss=b&srt=sco&sp=wcl&se=2025-06-30T23:59:59Z...
Container Name: client-acme-backups
```

**Example 3: Department Backups**
```
Storage Account: companystorage
SAS Token: ?sv=2021-06-08&ss=b&srt=sco&sp=wcl&se=2025-12-31T23:59:59Z...
Container Name: finance-db-backups
```

## SAS Token Permissions

### Minimum Required Permissions
For backup operations, you **must** grant:
- ✅ **Write** (w) - Upload backup files
- ✅ **Create** (c) - Create new blobs

### Optional Permissions
These are **NOT required** but can be added:
- ⚪ **List** (l) - List container contents (not needed by current version)
- ⚪ **Read** (r) - Download backups (only if you need to restore from this service)
- ⚪ **Delete** (d) - Delete old backups (only if implementing auto-cleanup)

### NOT Recommended
Avoid granting:
- ❌ **Add** (a) - Not needed for backups
- ❌ Unnecessary permissions - Follow principle of least privilege

### Permission String

**Minimum (Recommended)**:
```
wc
```
Use this in Azure CLI/PowerShell or when manually building SAS tokens.

**With List** (if you want extra validation):
```
wcl
```

**For Restore Capability**:
```
wcr
```

## SAS Token Lifecycle

### Token Expiration
SAS tokens should have expiration dates:

**Short-term** (Testing):
- Duration: 1-7 days
- Use case: Testing, proof of concept

**Medium-term** (Production):
- Duration: 3-6 months
- Use case: Regular rotation policy

**Long-term** (Stable environments):
- Duration: 1-2 years
- Use case: Stable production with monitoring

### Token Rotation

When token is about to expire:

1. **Generate New Token**
   - Create new SAS token with new expiry date
   - Keep old token valid for overlap period

2. **Update Configuration**
   - Run ConfigUI
   - Enter new SAS token
   - Test and save

3. **Verify**
   - Check next backup succeeds
   - Monitor logs for any issues

4. **Old Token**
   - Will automatically expire
   - Or revoke immediately if needed

### Token Revocation

**Immediate Revocation** (if compromised):
1. Go to Azure Portal
2. Storage Account → Access keys
3. **Regenerate** the account key used for SAS
4. All SAS tokens created with that key become invalid
5. Generate new SAS token
6. Update all backup services

## Multi-Client Scenarios

### Scenario 1: Managed Service Provider

You manage database backups for multiple clients:

```
Client A:
  Storage Account: clientastorage
  Container: clienta-db-backups
  SAS Token: [unique token for client A]
  
Client B:
  Storage Account: clientbstorage
  Container: clientb-db-backups
  SAS Token: [unique token for client B]
```

**Benefits**:
- Each client has their own storage
- Tokens can be revoked independently
- Billing is separate per client
- No risk of cross-client access

### Scenario 2: Shared Storage, Multiple Containers

Multiple clients using same storage account:

```
Company Storage Account: companystorage

Client A Container: client-a-backups
  SAS Token: [scoped to client-a-backups container only]
  
Client B Container: client-b-backups
  SAS Token: [scoped to client-b-backups container only]
```

**Benefits**:
- Centralized storage management
- Per-container access control
- Shared costs
- Easier monitoring

### Scenario 3: Department Isolation

Different departments, one storage:

```
Finance Department:
  Container: finance-db-backups
  SAS Token: [finance team token, expires annually]
  
HR Department:
  Container: hr-db-backups
  SAS Token: [HR team token, expires annually]
```

## Security Best Practices

### 1. Minimal Permissions
Only grant required permissions:
```
Backup Service: Write, Create, List only
Restore Service: Read, List only
Management: All permissions
```

### 2. Time-Limited Tokens
Set appropriate expiration:
```
Production: 6-12 months
Testing: 7-30 days
Demo: 1-7 days
```

### 3. IP Restrictions (Optional)
Limit SAS token to specific IPs:
```
- Server IP address
- VPN IP range
- Office IP range
```

### 4. HTTPS Only
Always enforce HTTPS:
```
In Azure Portal: Check "HTTPS only"
In CLI: Use --https-only flag
```

### 5. Monitor Usage
Regularly review:
- Storage account logs
- Backup success/failure rates
- Token expiration dates
- Unusual access patterns

### 6. Rotation Schedule
Establish rotation policy:
```
Every 6 months:
  - Review active tokens
  - Rotate expiring tokens
  - Revoke unused tokens
  - Update documentation
```

## Troubleshooting

### "Not authorized to perform this operation"

This is the **most common error** with SAS tokens!

**Causes**:
1. **Container doesn't exist** (most common)
   - Container-scoped SAS tokens cannot create containers
   - **Solution**: Create the container in Azure Portal first

2. **Missing permissions**
   - SAS token doesn't have Write (w) or Create (c) permissions
   - **Solution**: Regenerate token with correct permissions

3. **SAS token expired**
   - Check expiration date
   - **Solution**: Generate new token with future expiry

4. **Wrong container name**
   - Token is scoped to different container
   - **Solution**: Verify container name matches token scope

**Fix Steps**:
1. Go to Azure Portal
2. Navigate to Storage Account → Containers
3. Create container with exact name you're using (e.g., "database-backups")
4. Generate new SAS token for that container
5. Update configuration with new token
6. Test again

### "Authentication failed"
- **Check**: SAS token hasn't expired
- **Check**: Token copied completely (including `?`)
- **Check**: Storage account name is correct
- **Solution**: Regenerate token if expired

### "Forbidden" or "403 error"
- **Check**: Token has Write and Create permissions
- **Check**: Token is scoped to correct container
- **Solution**: Generate new token with correct permissions

### "The remote name could not be resolved"
- **Check**: Storage account name is correct
- **Check**: Internet connectivity
- **Check**: DNS settings
- **Solution**: Verify account name, test network

### "Token is malformed"
- **Check**: Token wasn't truncated when copying
- **Check**: No extra spaces or characters
- **Check**: Token starts with `?` or system adds it
- **Solution**: Regenerate and copy carefully

### "Container not found"
- **Check**: Container name is correct
- **Check**: Container exists or SAS allows creation
- **Solution**: Verify container name or create container first

## Cost Implications

SAS tokens themselves are **free**:
- No cost to generate
- No cost per token
- No cost per API call using token

You only pay for:
- Storage used (GB/month)
- Transactions (write operations)
- Data egress (downloads)

Same costs as using connection strings.

## Migration from Connection Strings

### For Existing Users

1. **Generate SAS Token** (as described above)

2. **Update Configuration**:
   - Open ConfigUI
   - Check "Enable Azure Blob Storage Backup"
   - Select **"SAS Token"** radio button
   - Enter storage account name
   - Enter SAS token
   - Enter container name (same as before)
   - Test
   - Save

3. **Verify**:
   - Old backups remain accessible
   - New backups use SAS token
   - Monitor logs

4. **Connection String**:
   - Can keep connection string saved
   - Or switch permanently to SAS
   - Both methods stored in registry

### Switching Back (if needed)

1. Open ConfigUI
2. Select **"Connection String"** radio button
3. Previous connection string is still saved
4. Test and save

## Comparison Table

| Aspect | Connection String | SAS Token |
|--------|------------------|-----------|
| Setup | Easier | Slightly more steps |
| Security | Lower | Higher |
| Multi-client | Difficult | Easy |
| Expiration | Never | Configurable |
| Permissions | All | Granular |
| Revocation | Rotate keys | Delete token |
| Best for | Single user, testing | Production, multiple clients |
| Recommended | No | **Yes** |

## Recommendations

### Use SAS Token When:
- ✅ Multiple clients/departments
- ✅ Production environment
- ✅ Security compliance required
- ✅ Service provider scenario
- ✅ Need time-limited access
- ✅ Want granular permissions

### Use Connection String When:
- ⚠️ Single internal user
- ⚠️ Testing/development only
- ⚠️ Quick setup needed
- ⚠️ Full storage account access required

## Summary

SAS tokens provide:
- ✅ **Better Security** - Limited permissions
- ✅ **Better Control** - Per-client tokens
- ✅ **Better Compliance** - Time-limited access
- ✅ **Better Management** - Easy revocation
- ✅ **Better Isolation** - Client-specific access

**Recommended for all production deployments!**
