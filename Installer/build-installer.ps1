#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Builds Release binaries for both .NET Framework projects and then compiles
    the Inno Setup installer.

.DESCRIPTION
    1. Restores NuGet packages for the .NET Framework projects.
    2. Builds DatabaseBackupService.NetFx and DatabaseBackupService.ConfigUI.NetFx
       in Release|AnyCPU configuration.
    3. Compiles the Inno Setup script to produce Output\DatabaseBackupServiceSetup.exe.

.PARAMETER InnoSetupCompiler
    Full path to ISCC.exe.  Defaults to the standard Inno Setup 6 install location.

.PARAMETER SkipBuild
    Skip the MSBuild step and jump straight to the Inno Setup compile step.
    Useful when you have already built the projects manually.

.EXAMPLE
    # Run from the repository root
    .\Installer\build-installer.ps1

.EXAMPLE
    .\Installer\build-installer.ps1 -SkipBuild
#>
param(
    [string]$InnoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot   = Resolve-Path "$PSScriptRoot\.."
$IssScript  = Join-Path $PSScriptRoot "DatabaseBackupService.iss"
$OutputDir  = Join-Path $PSScriptRoot "Output"

# ---------------------------------------------------------------------------
# Locate MSBuild via vswhere (ships with every Visual Studio 2017+ install)
# ---------------------------------------------------------------------------
function Find-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path $vswhere)) {
        $vswhere = Join-Path $env:ProgramFiles "Microsoft Visual Studio\Installer\vswhere.exe"
    }
    if (-not (Test-Path $vswhere)) {
        Write-Error @"
vswhere.exe not found. Make sure Visual Studio 2017 or later is installed.
Expected location: $vswhere
"@
        exit 1
    }

    $installPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -property installationPath
    if (-not $installPath) {
        Write-Error "vswhere could not find a Visual Studio installation with MSBuild."
        exit 1
    }

    # VS 2019+ keeps MSBuild under Current\Bin; fall back to older paths
    $candidates = @(
        "$installPath\MSBuild\Current\Bin\MSBuild.exe",
        "$installPath\MSBuild\15.0\Bin\MSBuild.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    Write-Error "MSBuild.exe not found under: $installPath"
    exit 1
}

$MSBuild = Find-MSBuild
Write-Host "Using MSBuild: $MSBuild" -ForegroundColor DarkGray

# ---------------------------------------------------------------------------
# Step 1 – Build the .NET Framework projects
# ---------------------------------------------------------------------------
if (-not $SkipBuild) {
    Write-Host "`n=== Restoring NuGet packages ===" -ForegroundColor Cyan

    $sln = Get-ChildItem -Path $RepoRoot -Filter "*.sln" | Select-Object -First 1
    if (-not $sln) {
        Write-Error "No .sln file found under $RepoRoot"
        exit 1
    }

    & $MSBuild $sln.FullName /t:Restore /v:minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "NuGet restore failed (exit code $LASTEXITCODE)"
        exit 1
    }

    $projects = @(
        "DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj",
        "DatabaseBackupService.ConfigUI.NetFx\DatabaseBackupService.ConfigUI.NetFx.csproj"
    )

    foreach ($proj in $projects) {
        $fullPath = Join-Path $RepoRoot $proj
        Write-Host "`n=== Building $proj ===" -ForegroundColor Cyan
        & $MSBuild $fullPath /p:Configuration=Release /p:Platform=AnyCPU /v:minimal
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed for $proj (exit code $LASTEXITCODE)"
            exit 1
        }
    }

    # Verify the expected outputs exist
    $expectedFiles = @(
        "DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe",
        "DatabaseBackupService.ConfigUI.NetFx\bin\Release\DatabaseBackupService.ConfigUI.NetFx.exe"
    )
    foreach ($rel in $expectedFiles) {
        $full = Join-Path $RepoRoot $rel
        if (-not (Test-Path $full)) {
            Write-Error "Expected build output not found: $full"
            exit 1
        }
        Write-Host "  Found: $rel" -ForegroundColor Green
    }
}

# ---------------------------------------------------------------------------
# Step 2 – Compile the Inno Setup installer
# ---------------------------------------------------------------------------
Write-Host "`n=== Compiling Inno Setup installer ===" -ForegroundColor Cyan

if (-not (Test-Path $InnoSetupCompiler)) {
    Write-Error @"
Inno Setup Compiler not found at:
  $InnoSetupCompiler

Please install Inno Setup 6 from https://jrsoftware.org/isinfo.php
or pass the correct path via -InnoSetupCompiler parameter.
"@
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

& $InnoSetupCompiler $IssScript
if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed (exit code $LASTEXITCODE)"
    exit 1
}

$installer = Join-Path $OutputDir "DatabaseBackupServiceSetup.exe"
if (Test-Path $installer) {
    Write-Host "`n=== Installer created successfully ===" -ForegroundColor Green
    Write-Host "  $installer" -ForegroundColor Green
} else {
    Write-Warning "ISCC.exe reported success but the output file was not found at: $installer"
}
