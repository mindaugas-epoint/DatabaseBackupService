# Quick Fix: "The request is not authorized" Error

## 🔴 Problem
Getting error when testing Azure Blob Storage with SAS token:
```
"The request is not authorized to perform this operation"
```

## ✅ Solution

### Option 1: Minimal SAS Token (Recommended)

**1. Create Container First**
```
Azure Portal → Storage Account → Containers → + Container
Name: database-backups
```

**2. Generate SAS Token**
```
Click container → Generate SAS → Set:
☑ Write (w)
☑ Create (c)
☐ List (l)      - NOT REQUIRED
☐ Read (r)      - NOT REQUIRED
☐ Delete (d)    - NOT REQUIRED

Expiry: 1 year from now
Generate SAS token → Copy token
```

**3. Test in ConfigUI**
```
Enable Azure → Select SAS Token
Storage Account: mystorageaccount
SAS Token: ?sv=2021-06-08&...
Container: database-backups
Click "Test" → Should succeed!
```

### Option 2: If Still Failing

**Check These:**

1. **Container exists?**
   - Go to Azure Portal → Containers
   - Verify container with exact name exists

2. **Token has Write + Create?**
   - When generating, both boxes must be checked
   - Re-generate if unsure

3. **Token not expired?**
   - Check expiry date in token or portal
   - Generate new token if expired

4. **Correct storage account name?**
   - Verify in Azure Portal
   - Must match exactly (case-sensitive)

5. **Token copied completely?**
   - Should start with `?sv=`
   - No spaces or line breaks
   - Copy entire token

## 🎯 What Changed

**Before (caused the error)**:
- Tool tried to check if container exists using `ExistsAsync()`
- This requires **List** permission
- Many SAS tokens don't include List permission
- Result: "Not authorized" error

**After (fixed)**:
- Tool directly tries to upload a test file
- Only requires **Write** and **Create** permissions
- Clearer error messages:
  - 404 → Container doesn't exist
  - 403 → Missing permissions
- No List permission needed!

## 📋 Minimal Permissions Needed

| Operation | Write | Create | List | Read | Delete |
|-----------|-------|--------|------|------|--------|
| **Backup Files** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Test Connection** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **List Backups** | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Restore Files** | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Delete Old Backups** | ❌ | ❌ | ❌ | ❌ | ✅ |

**For this tool**: Only **Write** + **Create** required! ✅

## 🚀 Quick Test Checklist

Before clicking "Test" button:
- [ ] Container created in Azure Portal
- [ ] SAS token has Write permission
- [ ] SAS token has Create permission
- [ ] Container name matches exactly
- [ ] Storage account name correct
- [ ] Token not expired
- [ ] Token copied completely

## 💡 Pro Tip

**Use Connection String for quick testing:**
1. Switch to "Connection String" radio button
2. Paste full connection string from Azure Portal
3. Test → Will auto-create container if needed
4. Once working, switch to SAS token for production

**Why?**
- Connection strings have all permissions
- Can create containers automatically
- Easier for initial setup
- Then switch to SAS for security

## 📞 Still Not Working?

Check the exact error message:

**"Container not found" or 404**
→ Create container in Azure Portal first

**"Access Denied" or 403**
→ Regenerate SAS token with Write + Create permissions

**"Authentication failed"**
→ Token expired or wrong storage account name

**"Signature did not match"**
→ Token was truncated/corrupted, copy again

## ✅ Success Looks Like

When you click "Test", you should see:
```
✓ Container is accessible
✓ Write permission confirmed
✓ Ready for backups
```

Then you can safely click "Save" and start using it!

---

**Remember**: Only **Write (w)** and **Create (c)** permissions are required. You do NOT need List, Read, or Delete!
