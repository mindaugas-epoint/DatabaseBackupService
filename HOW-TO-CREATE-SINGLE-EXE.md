# 🎯 How to Create Single EXE File - Final Steps

## Current Status
✅ Costura.Fody and Fody packages added to packages.config  
✅ FodyWeavers.xml created  
⚠️ Project file needs configuration

## 🚀 Quick Method (Recommended)

### Step 1: Close Visual Studio
Close Visual Studio completely.

### Step 2: Run Setup Script
In PowerShell (in your workspace directory):
```powershell
.\Complete-CosturaSetup.ps1
```

This script will:
- Backup your project file
- Add all necessary Costura imports and targets
- Restore NuGet packages

### Step 3: Reopen Visual Studio
Open your solution again.

### Step 4: Build in Release Mode
1. Set configuration to **Release** (top toolbar)
2. Build → Rebuild Solution (Ctrl+Shift+B)

### Step 5: Get Your Single EXE Files
Your standalone executables will be at:
```
DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe
DatabaseBackupService.ConfigUI.NetFx\bin\Release\DatabaseBackupService.ConfigUI.NetFx.exe
```

---

## 📋 Alternative: Using Package Manager Console (In Visual Studio)

If you prefer to stay in Visual Studio:

### Step 1: Open Package Manager Console
Tools → NuGet Package Manager → Package Manager Console

### Step 2: Select Project
In the "Default project" dropdown, select: **DatabaseBackupService.NetFx**

### Step 3: Reinstall Package
```powershell
Update-Package Costura.Fody -reinstall
```

This will configure the project automatically.

### Step 4: Build
Build → Rebuild Solution

---

## ✅ Verify Single EXE Works

Test your single executable:

```powershell
# Create test folder
New-Item -Path "C:\Test\BackupService" -ItemType Directory -Force

# Copy ONLY the EXE
Copy-Item "DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe" -Destination "C:\Test\BackupService\"

# Try to run it
cd C:\Test\BackupService
.\DatabaseBackupService.NetFx.exe
```

**Success criteria**: If it runs without "Could not load assembly" errors, your single EXE is working! ✅

---

## 📦 What Gets Embedded

The single EXE will contain:
- ✅ All .NET Framework assemblies (Azure, MySql, MailKit, etc.)
- ✅ All NuGet package DLLs
- ⚠️ Native DLLs (x64/x86 SNI.dll) - may need to be deployed separately

## 🎯 Final Deployment

To deploy your application, copy:
1. **DatabaseBackupService.NetFx.exe** (main file)
2. **DatabaseBackupService.NetFx.exe.config** (configuration)
3. **x64** folder (if SQL Server connectivity is needed)

---

## 🔧 Troubleshooting

### Build Error: "Could not find Fody"
- Close Visual Studio
- Run: `.\Complete-CosturaSetup.ps1`
- Reopen and rebuild

### EXE Still Requires DLLs
- Make sure you built in **Release** mode
- Check that FodyWeavers.xml exists in project root
- Rebuild (not just Build)

### Large EXE Size (30-50 MB)
- This is normal! All dependencies are embedded
- Original EXE + all DLLs ≈ Same total size

---

## ⚡ Quick Summary

**Fastest path to single EXE:**

1. Close Visual Studio
2. Run: `.\Complete-CosturaSetup.ps1`
3. Reopen Visual Studio  
4. Build in Release mode
5. Done! 🎉

Your single EXE files will be in `bin\Release\` folders.
