# Database Backup Service – Installer

This folder contains the [Inno Setup 6](https://jrsoftware.org/isinfo.php) installer project for the .NET Framework 4.8 build of **Database Backup Service**.

## What the installer does

| Step | Detail |
|------|--------|
| **Create install folder** | `C:\Program Files\ePoint\DatabaseBackup` (created if it does not exist) |
| **Copy files** | `DatabaseBackupService.NetFx.exe` and `DatabaseBackupService.ConfigUI.NetFx.exe` are copied and overwritten if they already exist |
| **Windows Service** | If the `DatabaseBackupService` service already exists it is stopped and deleted before the new version is registered. The service is set to **Automatic** start and configured to restart automatically on failure |
| **Desktop shortcut** | A shortcut to `DatabaseBackupService.ConfigUI.NetFx.exe` is created on the **All Users** desktop |

## Prerequisites

| Tool | Where to get it |
|------|-----------------|
| **Inno Setup 6** | <https://jrsoftware.org/isinfo.php> |
| **MSBuild / Visual Studio 2019+** | Already installed with Visual Studio |
| **NuGet** (via MSBuild `/t:Restore`) | Included with Visual Studio – no separate `nuget.exe` needed |

## Building the installer

### Option A – PowerShell script (recommended)

Run the following from the **repository root** in an **elevated** PowerShell window:

```powershell
.\Installer\build-installer.ps1
```

The script will:
1. Restore NuGet packages.
2. Build both .NET Framework projects in **Release** configuration.
3. Compile the Inno Setup script.
4. Write the finished installer to `Installer\Output\DatabaseBackupServiceSetup.exe`.

#### Skip the build step

If you have already built the projects in Visual Studio:

```powershell
.\Installer\build-installer.ps1 -SkipBuild
```

#### Custom Inno Setup path

```powershell
.\Installer\build-installer.ps1 -InnoSetupCompiler "D:\Tools\InnoSetup6\ISCC.exe"
```

### Option B – Inno Setup IDE

1. Build both projects in Visual Studio in **Release | AnyCPU**.
2. Open `Installer\DatabaseBackupService.iss` in the **Inno Setup Compiler** GUI.
3. Press **Compile** (Ctrl+F9).
4. The installer is written to `Installer\Output\DatabaseBackupServiceSetup.exe`.

## Uninstalling

Run **Add or Remove Programs** and remove **"Database Backup Service"**.  
The uninstaller will stop and delete the Windows Service automatically.

## File layout

```
Installer\
  DatabaseBackupService.iss   ← Inno Setup script (this is the "project")
  build-installer.ps1         ← PowerShell build helper
  README.md                   ← This file
  Output\                     ← Created at build time
    DatabaseBackupServiceSetup.exe
```
