# Quick Start: Publishing as Single EXE

## ⚡ Automated Setup (Recommended)

1. **Close Visual Studio**
2. **Run the setup script**:
   ```powershell
   .\Setup-SingleExe.ps1
   ```
3. **Reopen Visual Studio**
4. **Build in Release mode**

Done! Single EXE files will be in `bin\Release\` folders.

---

## 📋 Manual Setup (if automation fails)

### For DatabaseBackupService.NetFx:

1. **Close Visual Studio**

2. **Edit `DatabaseBackupService.NetFx\packages.config`**  
   Add after BouncyCastle line:
   ```xml
   <package id="Costura.Fody" version="6.0.0" targetFramework="net48" developmentDependency="true" />
   <package id="Fody" version="6.9.3" targetFramework="net48" developmentDependency="true" />
   ```

3. **Edit `DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj`**  

   **Add at top** (after `<Project ...>` line):
   ```xml
   <Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props" Condition="Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')" />
   ```

   **Add in references section** (after BouncyCastle):
   ```xml
   <Reference Include="Costura, Version=6.0.0.0, Culture=neutral, PublicKeyToken=9919ef960d84173d, processorArchitecture=MSIL">
     <HintPath>..\packages\Costura.Fody.6.0.0\lib\netstandard2.0\Costura.dll</HintPath>
   </Reference>
   ```

   **Add in `EnsureNuGetPackageBuildImports` target**:
   ```xml
   <Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props'))" />
   <Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets'))" />
   <Error Condition="!Exists('..\packages\Fody.6.9.3\build\Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Fody.6.9.3\build\Fody.targets'))" />
   ```

   **Add before closing `</Project>` tag**:
   ```xml
   <Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets" Condition="Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" />
   <Import Project="..\packages\Fody.6.9.3\build\Fody.targets" Condition="Exists('..\packages\Fody.6.9.3\build\Fody.targets')" />
   ```

4. **Restore packages**:
   ```powershell
   msbuild -t:restore DatabaseBackupService.sln
   ```

5. **Reopen Visual Studio and build**

---

## 🚀 Build Commands

### Visual Studio:
- Select **Release** configuration
- Right-click project → **Build**
- Or: Build → Build Solution (Ctrl+Shift+B)

### Command Line:
```powershell
# Build Service
msbuild DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj /p:Configuration=Release

# Build Config UI
msbuild DatabaseBackupService.ConfigUI.NetFx\DatabaseBackupService.ConfigUI.NetFx.csproj /p:Configuration=Release
```

---

## 📁 Output Files

After building:

```
DatabaseBackupService.NetFx\bin\Release\
└── DatabaseBackupService.NetFx.exe ← Deploy this single file

DatabaseBackupService.ConfigUI.NetFx\bin\Release\
└── DatabaseBackupService.ConfigUI.NetFx.exe ← Deploy this single file
```

**Also deploy:**
- `*.exe.config` - Configuration files
- `x64\SNI.dll` / `x86\SNI.dll` - Native SQL Server dependencies (if needed)

---

## ✅ Verification

Test the single EXE:
```powershell
# Copy to test folder
mkdir C:\Test
copy DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe C:\Test\
cd C:\Test

# Run it
.\DatabaseBackupService.NetFx.exe
```

If it runs without "assembly not found" errors → Success! ✓

---

## 🔧 Current Status

- ✅ **DatabaseBackupService.ConfigUI.NetFx** - Already configured
- ⚠️ **DatabaseBackupService.NetFx** - Needs configuration (run Setup-SingleExe.ps1)

---

## 📚 More Information

See `PUBLISH-SINGLE-EXE-GUIDE.md` for detailed explanations and troubleshooting.
