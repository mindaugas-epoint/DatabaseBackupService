# Azure SAS Token - Quick Troubleshooting Guide

## ❌ Error: "The request is not authorized to perform this operation"

This error usually means your SAS token is missing required permissions.

### Root Causes

**1. Missing Write/Create Permissions** (Most Common)
- SAS token was generated without Write (w) or Create (c) permissions
- Solution: Regenerate token with correct permissions

**2. Container Doesn't Exist**
- Trying to write to a non-existent container
- SAS tokens cannot create containers
- Solution: Create container in Azure Portal first

**3. Missing List Permission**
- Some operations require List (l) permission
- Older versions of this tool needed List permission
- **Current version**: Only Write and Create are required

### Solution Steps

**Step 1: Create Container (if not exists)**
```
1. Go to portal.azure.com
2. Navigate to your Storage Account
3. Click "Containers"
4. Click "+ Container"
5. Name it exactly as you'll use in config (e.g., "database-backups")
6. Click "Create"
```

**Step 2: Generate SAS Token with Correct Permissions**
```
1. Click on the container you just created
2. Click "Generate SAS" (or right-click → Generate SAS)
3. Set these permissions:
   ☑ Write (w)    - REQUIRED
   ☑ Create (c)   - REQUIRED
   ☐ List (l)     - Optional (not needed for backups)
   ☐ Read (r)     - Optional (not needed for backups)
   ☐ Delete (d)   - Optional (not recommended)
4. Set expiry date (e.g., 1 year from now)
5. Click "Generate SAS token and URL"
6. Copy ONLY the SAS token (starts with ?)
```

**Step 3: Test in ConfigUI**
```
1. Run DatabaseBackupService.ConfigUI.exe
2. Enable Azure Blob Storage Backup
3. Select "SAS Token (Recommended)"
4. Enter:
   - Storage Account: mystorageaccount
   - SAS Token: [paste the token]
   - Container Name: database-backups (exact match!)
5. Click "Test"
```

### What "Test" Button Does Now

The test button will:
1. ✓ Try to upload a small test file
2. ✓ If successful → Shows success message
3. ✗ If 404 error → Container doesn't exist
4. ✗ If 403 error → Missing permissions or expired token

**No List permission needed!** The test directly attempts a write operation.

## ❌ Error: "Not authorized to perform this operation"

**Step 1: Create Container in Azure Portal**
```
1. Go to portal.azure.com
2. Navigate to your Storage Account
3. Click "Containers" in the left menu
4. Click "+ Container" button
5. Enter container name (e.g., "database-backups")
6. Set Public access level: "Private"
7. Click "Create"
```

**Step 2: Generate SAS Token for Existing Container**
```
1. Click on the container you just created
2. Click "Generate SAS" in the right menu
3. Set permissions: ☑ Write, ☑ Create, ☑ List
4. Set expiry date (e.g., 1 year from now)
5. Click "Generate SAS token and URL"
6. Copy the SAS token (starts with ?)
```

**Step 3: Update Configuration**
```
1. Run DatabaseBackupService.ConfigUI.exe
2. Enable Azure Blob Storage Backup
3. Select "SAS Token (Recommended)"
4. Enter:
   - Storage Account: mystorageaccount
   - SAS Token: [paste token]
   - Container Name: database-backups (exact match!)
5. Click "Test" - should succeed now
6. Click "Save"
```

### Why This Happens

**Container Creation Requires Different Permissions**:
- **Container-scoped SAS**: Can read/write blobs in existing container
- **Account-scoped SAS**: Can create containers (but less secure)
- **Connection String**: Full permissions (can create containers)

**Our Recommendation**: Create container manually, use container-scoped SAS token

## ✅ Verification Checklist

Before testing, verify:

- [ ] Container exists in Azure Portal
- [ ] Container name matches exactly (case-sensitive)
- [ ] SAS token generated AFTER container creation
- [ ] SAS token has Write (w) and Create (c) permissions
- [ ] SAS token not expired
- [ ] Storage account name is correct
- [ ] No typos in any field

## 🔍 Step-by-Step Debug Process

### 1. Verify Container Exists
```
Portal > Storage Account > Containers > [your-container-name]
```
If not found → **Create it first!**

### 2. Check SAS Token Permissions
When generating SAS token, ensure these are checked:
- ☑ **Write** (w)
- ☑ **Create** (c)  
- ☑ **List** (l) - optional but recommended

### 3. Check SAS Token Expiry
Look at the token:
```
?sv=2021-06-08&se=2025-12-31T23:59:59Z...
                   ^^^^^^^^^^^^^^^^^^^
                   This is expiry date
```
Must be in the future!

### 4. Verify Container Name Match
```
Token scope: database-backups
Config:      database-backups  ✓ Match

Token scope: database-backups
Config:      Database-Backups  ✗ Case mismatch!

Token scope: db-backups
Config:      database-backups  ✗ Different name!
```

### 5. Test Configuration
1. Click "Test" button in ConfigUI
2. Should show: "Container is accessible" ✓
3. If error → check logs for specific message

## 📋 Common Scenarios & Solutions

### Scenario 1: Fresh Setup
```
Problem: Getting "not authorized" on first test
Solution: 
  1. Create container in Azure Portal first
  2. Then generate SAS token
  3. Then test connection
```

### Scenario 2: Existing Container, New Token
```
Problem: Token worked before, now getting "not authorized"
Possible causes:
  - Token expired
  - Regenerated storage account keys (invalidates old tokens)
  - Changed container name
  
Solution:
  1. Check token expiry date
  2. Generate new SAS token
  3. Update configuration
```

### Scenario 3: Multiple Clients
```
Problem: Client A works, Client B gets "not authorized"
Possible causes:
  - Client B's container doesn't exist
  - Client B's token is for different container
  
Solution:
  1. Create container for Client B
  2. Generate separate SAS token for Client B
  3. Verify container names match
```

## 🛠️ Alternative: Use Connection String Temporarily

If you need to test quickly and container creation is the issue:

**Option 1: Use Connection String**
- Select "Connection String" in ConfigUI (not SAS Token)
- Paste full connection string
- Connection strings CAN create containers automatically
- Test, verify it works
- Then switch to SAS token later for production

**Option 2: Account-Level SAS Token**
- Generate SAS token at **account level** (not container level)
- Has permission to create containers
- Less secure, only for testing
- For production, use container-level SAS

## 📊 Permission Levels Comparison

| Permission Level | Can Create Container? | Recommended For |
|-----------------|----------------------|-----------------|
| **Container SAS** | ❌ No | ✅ **Production** (most secure) |
| **Account SAS** | ✅ Yes | ⚠️ Testing only (less secure) |
| **Connection String** | ✅ Yes | ⚠️ Testing only (full access) |

## 🎯 Best Practice Workflow

**For Production**:
```
1. Admin creates container in Azure Portal
2. Admin generates container-scoped SAS token
3. Admin provides to backup service
4. Service uses SAS token (cannot create/delete containers)
5. Service can only write backup files
```

**Benefits**:
- ✅ Least privilege access
- ✅ Cannot accidentally delete container
- ✅ Cannot create unwanted containers
- ✅ Easy to revoke (just delete/expire token)

## 🆘 Still Having Issues?

### Get More Details
1. Check the exact error message in the ConfigUI
2. Look at the service logs for detailed error
3. Try connection test in ConfigUI first

### Common Error Messages

**"Container not found"**
→ Create container in Azure Portal

**"Signature did not match"**
→ SAS token corrupted during copy/paste, regenerate

**"Authentication failed"**
→ Wrong storage account name or expired token

**"Forbidden"**
→ Missing Write or Create permissions in SAS token

**"This request is not authorized"**
→ Container doesn't exist (most common!)

### Enable Detailed Logging
Check service logs for full error details:
```
[Error] Failed Azure backup of ProductionDB database. 
Error: Container 'database-backups' does not exist. 
Container-scoped SAS tokens cannot create containers. 
Please create the container in Azure Portal first.
```

## 📞 Quick Support Checklist

When asking for help, provide:
1. ✓ Error message (exact text)
2. ✓ Container exists? (yes/no)
3. ✓ Container name used
4. ✓ SAS token generated date
5. ✓ Token expiry date
6. ✓ Permissions selected when generating token
7. ✓ Using ConfigUI test button or actual backup?

## 💡 Pro Tips

1. **Always create container first** before generating SAS token
2. **Test immediately** after generating token (while portal is still open)
3. **Name containers lowercase** with hyphens (Azure naming rules)
4. **Set expiry dates** on calendar reminders
5. **Document** which client has which token
6. **Test both** "Test" button AND actual backup
7. **Keep old token** valid for 24 hours when rotating (overlap period)

## 🎓 Understanding the Architecture

```
ConfigUI Test Flow:
1. User enters SAS token
2. Click "Test"
3. Code checks: Does container exist?
4. If NO → Show helpful error message
5. If YES → Upload test blob
6. Delete test blob
7. Show success

Worker Service Backup Flow:
1. Service starts at scheduled time
2. Check: Does container exist?
3. If NO → Log error, skip backup
4. If YES → Upload backup file
5. Log success
```

**Key Point**: We check container existence BEFORE attempting operations that would fail with "not authorized" error. This provides better error messages!

## ✅ Success Indicators

You know it's working when:
- ✓ Test button shows "Container is accessible"
- ✓ Test button shows "Write permission confirmed"
- ✓ Actual backup completes successfully
- ✓ Backup file appears in Azure Portal
- ✓ Logs show "Completed Azure backup"

---

**Remember**: The #1 solution is almost always: **Create the container in Azure Portal first!**
