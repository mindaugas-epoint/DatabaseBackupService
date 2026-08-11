# Database Backup Service - Installer Options Summary

This document summarizes the available installation methods and suggests the best approach for different scenarios.

## 📦 Available Installation Files

The repository now includes the following installation files:

| File | Type | Description | User Level |
|------|------|-------------|------------|
| `Install.bat` | Batch Script | One-click installer | Beginner |
| `Uninstall.bat` | Batch Script | One-click uninstaller | Beginner |
| `install-service.ps1` | PowerShell | Automated installation script | Intermediate |
| `uninstall-service.ps1` | PowerShell | Automated uninstallation script | Intermediate |
| `INSTALLATION.md` | Documentation | Complete manual installation guide | Advanced |
| `QUICK_REFERENCE.md` | Documentation | Command quick reference | All Levels |

---

## 🎯 Which Installation Method to Use?

### For End Users (Non-Technical)
**Recommended: Batch Files**

✅ **Pros:**
- Double-click to install
- Automatic administrator elevation
- No command-line knowledge needed
- Visual progress in console window

📝 **How to use:**
1. Download the repository
2. Double-click `Install.bat`
3. Follow the prompts
4. Run Configuration UI

---

### For IT Administrators
**Recommended: PowerShell Scripts**

✅ **Pros:**
- Full control over installation parameters
- Can be integrated into deployment scripts
- Supports automation
- Detailed logging and error handling

📝 **How to use:**
```powershell
# Basic installation
.\install-service.ps1

# Custom installation
.\install-service.ps1 -ServiceName "MyBackupService" -InstallPath "D:\Services\BackupService" -AutoStart

# Uninstall with config removal
.\uninstall-service.ps1 -RemoveConfig
```

---

### For Developers
**Recommended: Manual Installation**

✅ **Pros:**
- Full understanding of the process
- Easy debugging
- Can be adapted for different environments
- Learn the service architecture

📝 **How to use:**
See [INSTALLATION.md](INSTALLATION.md) for step-by-step manual installation.

---

## 🏢 Enterprise Deployment Options

### Option 1: Group Policy Deployment (Current Solution)
Use the PowerShell scripts with GPO:
```powershell
# Deploy via GPO startup script
\\domain\SYSVOL\domain\scripts\install-database-backup.ps1
```

### Option 2: SCCM/Intune Deployment
Package the PowerShell script as an SCCM application:
- Detection method: Service exists
- Installation: `powershell.exe -ExecutionPolicy Bypass -File install-service.ps1`
- Uninstallation: `powershell.exe -ExecutionPolicy Bypass -File uninstall-service.ps1`

### Option 3: MSI Installer (Future Enhancement)
Create a professional installer using WiX Toolset:
- Silent installation support
- Add/Remove Programs integration
- Automatic updates
- Prerequisites checking

**Status:** Not yet implemented (see "Creating an MSI Installer" below)

---

## 🛠️ Creating an MSI Installer (Advanced)

For mass deployment, you can create an MSI installer using **WiX Toolset**.

### Prerequisites
```powershell
# Install WiX Toolset
dotnet tool install --global wix
```

### Basic WiX Configuration Example

Create `installer.wxs`:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" 
           Name="Database Backup Service" 
           Language="1033" 
           Version="2.0.0.0" 
           Manufacturer="Your Company" 
           UpgradeCode="YOUR-GUID-HERE">

    <Package InstallerVersion="200" Compressed="yes" InstallScope="perMachine" />

    <MajorUpgrade DowngradeErrorMessage="A newer version is already installed." />
    <MediaTemplate EmbedCab="yes" />

    <Feature Id="ProductFeature" Title="Database Backup Service" Level="1">
      <ComponentGroupRef Id="ProductComponents" />
      <ComponentGroupRef Id="ServiceComponents" />
    </Feature>

    <!-- Custom actions for service installation -->
    <CustomAction Id="InstallService" 
                  BinaryKey="WixCA" 
                  DllEntry="CAQuietExec" 
                  Execute="deferred" 
                  Return="check" 
                  Impersonate="no" />
  </Product>

  <!-- Component definitions -->
  <Fragment>
    <Directory Id="TARGETDIR" Name="SourceDir">
      <Directory Id="ProgramFilesFolder">
        <Directory Id="INSTALLFOLDER" Name="Database Backup Service" />
      </Directory>
    </Directory>
  </Fragment>

  <Fragment>
    <ComponentGroup Id="ProductComponents" Directory="INSTALLFOLDER">
      <Component Id="ServiceExecutable">
        <File Source="$(var.PublishDir)\DatabaseBackupService.exe" />

        <ServiceInstall Id="DatabaseBackupServiceInstall"
                        Type="ownProcess"
                        Name="DatabaseBackupService"
                        DisplayName="Database Backup Service"
                        Description="Automated database backup service with Azure support"
                        Start="auto"
                        Account="LocalSystem"
                        ErrorControl="normal" />

        <ServiceControl Id="DatabaseBackupServiceControl"
                        Start="install"
                        Stop="both"
                        Remove="uninstall"
                        Name="DatabaseBackupService"
                        Wait="yes" />
      </Component>

      <!-- Additional files -->
      <Component Id="AppSettings">
        <File Source="$(var.PublishDir)\appsettings.json" />
      </Component>
    </ComponentGroup>
  </Fragment>
</Wix>
```

### Build MSI
```powershell
# Build the installer
wix build installer.wxs -out DatabaseBackupService.msi

# Install silently
msiexec /i DatabaseBackupService.msi /quiet /qn

# Uninstall silently
msiexec /x DatabaseBackupService.msi /quiet /qn
```

**Note:** This is a simplified example. Full WiX implementation would require additional configuration for dependencies, custom dialogs, and the Configuration UI.

---

## 🔄 Comparison of Installation Methods

| Feature | Batch File | PowerShell | Manual | MSI (Future) |
|---------|-----------|------------|---------|--------------|
| Ease of Use | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Customization | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| Automation | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Enterprise Ready | ⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| Silent Install | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐ | ⭐⭐⭐⭐⭐ |
| Uninstall Support | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Add/Remove Programs | ❌ | ❌ | ❌ | ✅ |
| Complexity | Low | Medium | High | High |

---

## 📋 Installation Checklist

### Before Installation
- [ ] Windows 10/11 or Windows Server 2016+
- [ ] .NET 10 Runtime (included in scripts)
- [ ] Administrator privileges
- [ ] Database server accessible
- [ ] Azure Storage Account (if using Azure backups)

### During Installation
- [ ] Run installer (batch file, PowerShell, or manual)
- [ ] Wait for service installation
- [ ] Note installation path

### After Installation
- [ ] Run Configuration UI
- [ ] Configure database connection
- [ ] Test database connection
- [ ] Configure backup schedule
- [ ] (Optional) Configure Azure backup
- [ ] (Optional) Test Azure connection
- [ ] Save configuration
- [ ] Start the service
- [ ] Verify service is running

### Verification
```powershell
# Check service status
Get-Service -Name "DatabaseBackupService"

# Check service is set to Automatic
Get-Service -Name "DatabaseBackupService" | Select-Object Name, StartType, Status

# View configuration
Get-ItemProperty -Path "HKCU:\SOFTWARE\DatabaseBackupService"
```

---

## 🎬 Recommended Installation Flow

### For Single Server
1. Download repository
2. Double-click `Install.bat`
3. Run `DatabaseBackupService.ConfigUI.exe`
4. Configure and save
5. Start service via Configuration UI or PowerShell

### For Multiple Servers
1. Create deployment package:
   ```powershell
   # Publish the service
   dotnet publish -c Release -r win-x64 --self-contained

   # Copy installer scripts
   # Copy to deployment share
   ```

2. Deploy via PowerShell remoting:
   ```powershell
   $servers = @("Server1", "Server2", "Server3")

   foreach ($server in $servers) {
       Invoke-Command -ComputerName $server -ScriptBlock {
           \\DeploymentShare\DatabaseBackupService\install-service.ps1 -AutoStart
       }
   }
   ```

3. Configure each server individually or use centralized configuration

---

## 🆘 Support

- **Installation Issues:** See [INSTALLATION.md](INSTALLATION.md)
- **Configuration Help:** See [DatabaseBackupService.ConfigUI/README.md](DatabaseBackupService.ConfigUI/README.md)
- **Quick Commands:** See [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
- **Azure Setup:** See [AZURE_BLOB_STORAGE_GUIDE.md](AZURE_BLOB_STORAGE_GUIDE.md)

---

## ✅ Summary

**Current Solution:**
- ✅ Batch file installer (`Install.bat`)
- ✅ PowerShell installer (`install-service.ps1`)
- ✅ Complete uninstallation support
- ✅ Comprehensive documentation

**Simple Installation:**
Most users should use **`Install.bat`** - it's the easiest method.

**Advanced Deployment:**
IT administrators should use **PowerShell scripts** for automation and customization.

**Future Enhancement:**
An **MSI installer** could be created using WiX Toolset for enterprise deployment with Add/Remove Programs integration.

The current scripts provide a **simple, professional installation experience** suitable for most deployment scenarios.
