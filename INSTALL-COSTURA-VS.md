# Install Costura.Fody Using Visual Studio Package Manager Console

## Quick Install Steps

### 1. Open Package Manager Console
- In Visual Studio: **Tools** → **NuGet Package Manager** → **Package Manager Console**

### 2. Run Install Commands

```powershell
# Install Costura.Fody for DatabaseBackupService.NetFx
Install-Package Costura.Fody -ProjectName DatabaseBackupService.NetFx -Version 6.0.0

# Verify installation
Get-Package -ProjectName DatabaseBackupService.NetFx | Where-Object { $_.Id -like "*Fody*" }
```

### 3. Build the Project
```powershell
# Build in Release mode
msbuild DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj /p:Configuration=Release
```

---

## Alternative: Install via UI

1. **Right-click** on `DatabaseBackupService.NetFx` project
2. Select **Manage NuGet Packages...**
3. Click **Browse** tab
4. Search for `Costura.Fody`
5. Select version **6.0.0**
6. Click **Install**

---

## What Gets Installed

Installing `Costura.Fody 6.0.0` automatically installs:
- ✅ Costura.Fody 6.0.0
- ✅ Fody 6.9.3 (dependency)

And automatically updates:
- ✅ `packages.config`
- ✅ Project file (`.csproj`) with all necessary imports and targets

---

## Verification

After installation, check:

1. **packages.config** should contain:
   ```xml
   <package id="Costura.Fody" version="6.0.0" targetFramework="net48" developmentDependency="true" />
   <package id="Fody" version="6.9.3" targetFramework="net48" developmentDependency="true" />
   ```

2. **Project file** should have:
   - Costura.Fody.props import at the top
   - Costura reference in ItemGroup
   - Costura.Fody.targets import at the bottom

3. **FodyWeavers.xml** created in project root:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
     <Costura />
   </Weavers>
   ```

---

## Build and Test

1. **Clean Solution**: Build → Clean Solution
2. **Rebuild**: Build → Rebuild Solution
3. **Check Output**: `DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe`

The EXE should be larger now (contains all dependencies embedded).

---

## Testing Single EXE

```powershell
# Create test directory
New-Item -ItemType Directory -Path C:\Test\BackupService -Force

# Copy only the EXE
Copy-Item "DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe" -Destination "C:\Test\BackupService\"
Copy-Item "DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe.config" -Destination "C:\Test\BackupService\"

# Test run
cd C:\Test\BackupService
.\DatabaseBackupService.NetFx.exe --help
```

If it runs without "could not load assembly" errors → Success! ✅

---

## Current Project Status

- ✅ **DatabaseBackupService.ConfigUI.NetFx** - Already has Costura.Fody
- ⚠️ **DatabaseBackupService.NetFx** - Ready to install (run Install-Package command above)
