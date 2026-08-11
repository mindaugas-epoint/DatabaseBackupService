# Guide: Publishing .NET Framework Projects as Single EXE Files

This guide explains how to publish `DatabaseBackupService.NetFx` and `DatabaseBackupService.ConfigUI.NetFx` as single executable files.

## Current Status

✅ **DatabaseBackupService.ConfigUI.NetFx** - Already configured with Costura.Fody  
⚠️ **DatabaseBackupService.NetFx** - Needs Costura.Fody configuration

## Option 1: Using Costura.Fody (Recommended)

Costura.Fody embeds all dependencies into the executable at build time.

### Steps for DatabaseBackupService.NetFx

#### 1. Close Visual Studio Solution

The project files need to be edited, which requires closing Visual Studio first.

#### 2. Install NuGet Packages

Add these lines to `DatabaseBackupService.NetFx\packages.config` after BouncyCastle.Cryptography:

```xml
<package id="Costura.Fody" version="6.0.0" targetFramework="net48" developmentDependency="true" />
<package id="Fody" version="6.9.3" targetFramework="net48" developmentDependency="true" />
```

#### 3. Edit Project File

Edit `DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj`:

**A. Add Import at the beginning** (right after the opening `<Project>` tag):

```xml
<Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props" Condition="Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')" />
```

**B. Add Costura Reference** (in the `<ItemGroup>` with other references, after BouncyCastle):

```xml
<Reference Include="Costura, Version=6.0.0.0, Culture=neutral, PublicKeyToken=9919ef960d84173d, processorArchitecture=MSIL">
  <HintPath>..\packages\Costura.Fody.6.0.0\lib\netstandard2.0\Costura.dll</HintPath>
</Reference>
```

**C. Add Error Conditions** (in the `EnsureNuGetPackageBuildImports` target):

```xml
<Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props'))" />
<Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets'))" />
<Error Condition="!Exists('..\packages\Fody.6.9.3\build\Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Fody.6.9.3\build\Fody.targets'))" />
```

**D. Add Import Targets** (at the end, before closing `</Project>` tag):

```xml
<Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets" Condition="Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" />
<Import Project="..\packages\Fody.6.9.3\build\Fody.targets" Condition="Exists('..\packages\Fody.6.9.3\build\Fody.targets')" />
```

#### 4. Restore NuGet Packages

Open PowerShell in the solution directory and run:

```powershell
nuget restore DatabaseBackupService.sln
```

Or if you don't have nuget.exe:

```powershell
msbuild -t:restore DatabaseBackupService.sln
```

#### 5. Reopen Solution and Build

- Reopen the solution in Visual Studio
- Build both projects in **Release** configuration
- The output will be a single EXE file with all dependencies embedded

### Publishing Steps

After configuration, publish using one of these methods:

#### Method 1: Visual Studio (Both Projects)

1. Right-click the project → **Publish**
2. Choose **Folder** as target
3. Select output folder
4. Click **Publish**

The resulting EXE will be in the publish folder with all dependencies embedded.

#### Method 2: MSBuild Command Line

For **DatabaseBackupService.NetFx**:

```powershell
msbuild DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj /p:Configuration=Release /p:Platform=AnyCPU
```

For **DatabaseBackupService.ConfigUI.NetFx**:

```powershell
msbuild DatabaseBackupService.ConfigUI.NetFx\DatabaseBackupService.ConfigUI.NetFx.csproj /p:Configuration=Release /p:Platform=AnyCPU
```

The single EXE files will be in:
- `DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe`
- `DatabaseBackupService.ConfigUI.NetFx\bin\Release\DatabaseBackupService.ConfigUI.NetFx.exe`

## Option 2: Using ILRepack (Alternative)

ILRepack merges multiple assemblies into one, similar to ILMerge.

### Steps

#### 1. Install ILRepack NuGet Package

Add to packages.config:

```xml
<package id="ILRepack" version="2.0.34" targetFramework="net48" />
```

#### 2. Create ILRepack Build Target

Add this to the project file before the closing `</Project>` tag:

```xml
<Target Name="ILRepack" AfterTargets="Build" Condition="'$(Configuration)' == 'Release'">
  <ItemGroup>
    <InputAssemblies Include="$(OutputPath)$(AssemblyName).exe" />
    <InputAssemblies Include="$(OutputPath)*.dll" />
  </ItemGroup>
  <PropertyGroup>
    <ILRepack>..\packages\ILRepack.2.0.34\tools\ILRepack.exe</ILRepack>
    <OutputAssembly>$(OutputPath)$(AssemblyName).Merged.exe</OutputAssembly>
  </PropertyGroup>
  <Exec Command="$(ILRepack) /out:$(OutputAssembly) @(InputAssemblies->'&quot;%(FullPath)&quot;', ' ')" />
</Target>
```

This creates `DatabaseBackupService.NetFx.Merged.exe` with all dependencies.

## Configuration Notes for Single EXE

### Special Cases to Handle

1. **Native DLLs**: Some dependencies like `Microsoft.Data.SqlClient.SNI` include native x86/x64 DLLs that cannot be embedded. Costura will extract them to a temp folder at runtime.

2. **App.config**: Configuration files are NOT embedded. You may need to:
   - Rename `App.config` to `[YourApp].exe.config` manually, or
   - Use embedded resources for configuration

3. **FodyWeavers.xml**: Optionally create `FodyWeavers.xml` in project root for advanced Costura configuration:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Weavers xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="FodyWeavers.xsd">
  <Costura>
    <IncludeDebugSymbols>false</IncludeDebugSymbols>
    <CreateTemporaryAssemblies>false</CreateTemporaryAssemblies>
    <PreloadOrder>
      <!-- List assemblies that should be preloaded -->
    </PreloadOrder>
  </Costura>
</Weavers>
```

## Verification

After building, verify the single EXE:

1. Copy only the `.exe` file to a clean folder (no DLLs except native ones)
2. Run the executable
3. If it runs without "assembly not found" errors, it's working correctly

## Troubleshooting

### "Could not load file or assembly" Error

- Check if a native DLL is missing (copy from bin folder)
- Some assemblies may need to be excluded from embedding

### Build Errors After Adding Costura

- Ensure NuGet packages are restored
- Clean and rebuild the solution
- Check that all Import paths are correct

### Large EXE Size

- Normal - all dependencies are embedded
- Typical size: 20-50 MB depending on dependencies

## Quick Reference: File Locations

After Release build with Costura.Fody:

```
DatabaseBackupService.NetFx\bin\Release\
├── DatabaseBackupService.NetFx.exe          (Single EXE - DEPLOY THIS)
├── DatabaseBackupService.NetFx.exe.config   (Config file - also deploy)
└── x64\SNI.dll                              (Native DLL - may need to deploy)

DatabaseBackupService.ConfigUI.NetFx\bin\Release\
├── DatabaseBackupService.ConfigUI.NetFx.exe         (Single EXE - DEPLOY THIS)
├── DatabaseBackupService.ConfigUI.NetFx.exe.config  (Config file - also deploy)
└── x64\SNI.dll                                      (Native DLL - may need to deploy)
```

## Summary

✅ **ConfigUI project**: Already ready - just build Release configuration  
⚠️ **Service project**: Follow steps 1-5 above to configure Costura.Fody

Both will produce single EXE files that can be deployed without carrying dozens of DLL files.
