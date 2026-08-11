# Setup-SingleExe.ps1
# This script configures DatabaseBackupService.NetFx for single EXE publishing using Costura.Fody

Write-Host "=== Configuring DatabaseBackupService.NetFx for Single EXE Publishing ===" -ForegroundColor Cyan
Write-Host ""

$projectDir = "DatabaseBackupService.NetFx"
$packagesConfigPath = "$projectDir\packages.config"
$csprojPath = "$projectDir\DatabaseBackupService.NetFx.csproj"

# Check if Visual Studio solution is open
$vsProcesses = Get-Process | Where-Object { $_.ProcessName -like "*devenv*" }
if ($vsProcesses) {
    Write-Host "WARNING: Visual Studio appears to be running. Please close Visual Studio before running this script." -ForegroundColor Yellow
    Write-Host "Press Ctrl+C to exit, or any other key to continue anyway..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    Write-Host ""
}

# Step 1: Update packages.config
Write-Host "Step 1: Updating packages.config..." -ForegroundColor Green
$packagesContent = Get-Content -Path $packagesConfigPath -Raw

if ($packagesContent -notmatch "Costura\.Fody") {
    Write-Host "  Adding Costura.Fody and Fody packages..." -ForegroundColor Yellow

    $packagesContent = $packagesContent -replace `
        '(<package id="BouncyCastle\.Cryptography"[^>]+/>)', `
        '$1`r`n  <package id="Costura.Fody" version="6.0.0" targetFramework="net48" developmentDependency="true" />`r`n  <package id="Fody" version="6.9.3" targetFramework="net48" developmentDependency="true" />'

    Set-Content -Path $packagesConfigPath -Value $packagesContent -NoNewline
    Write-Host "  ✓ packages.config updated" -ForegroundColor Green
} else {
    Write-Host "  ✓ Costura.Fody already in packages.config" -ForegroundColor Green
}

# Step 2: Update .csproj file
Write-Host ""
Write-Host "Step 2: Updating project file..." -ForegroundColor Green
$csprojContent = Get-Content -Path $csprojPath -Raw

$modified = $false

# Add Costura.Fody.props import at the beginning
if ($csprojContent -notmatch "Costura\.Fody\.props") {
    Write-Host "  Adding Costura.Fody.props import..." -ForegroundColor Yellow
    $csprojContent = $csprojContent -replace `
        '(<Project[^>]+>\r?\n)', `
        '$1  <Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props" Condition="Exists(''..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props'')" />`r`n'
    $modified = $true
    Write-Host "  ✓ Props import added" -ForegroundColor Green
} else {
    Write-Host "  ✓ Costura.Fody.props import already exists" -ForegroundColor Green
}

# Add Costura reference
if ($csprojContent -notmatch 'Reference Include="Costura') {
    Write-Host "  Adding Costura assembly reference..." -ForegroundColor Yellow
    $costuraRef = @'
    <Reference Include="Costura, Version=6.0.0.0, Culture=neutral, PublicKeyToken=9919ef960d84173d, processorArchitecture=MSIL">
      <HintPath>..\packages\Costura.Fody.6.0.0\lib\netstandard2.0\Costura.dll</HintPath>
    </Reference>
'@

    $csprojContent = $csprojContent -replace `
        '(<Reference Include="BouncyCastle\.Cryptography"[^>]+>[^<]+</Reference>)', `
        "`$1`r`n$costuraRef"
    $modified = $true
    Write-Host "  ✓ Costura reference added" -ForegroundColor Green
} else {
    Write-Host "  ✓ Costura reference already exists" -ForegroundColor Green
}

# Add error conditions
if ($csprojContent -notmatch "Costura\.Fody\.6\.0\.0\\build\\Costura\.Fody\.props") {
    Write-Host "  Adding Costura error conditions..." -ForegroundColor Yellow
    $errorConditions = @'
    <Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props'))" />
    <Error Condition="!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets'))" />
    <Error Condition="!Exists('..\packages\Fody.6.9.3\build\Fody.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\Fody.6.9.3\build\Fody.targets'))" />
'@

    $csprojContent = $csprojContent -replace `
        '(</Target>\s*<Import Project="\.\.\\packages\\Serilog)', `
        "$errorConditions`r`n  `$1"
    $modified = $true
    Write-Host "  ✓ Error conditions added" -ForegroundColor Green
} else {
    Write-Host "  ✓ Error conditions already exist" -ForegroundColor Green
}

# Add target imports at the end
if ($csprojContent -notmatch "Costura\.Fody\.6\.0\.0\\build\\Costura\.Fody\.targets") {
    Write-Host "  Adding Costura target imports..." -ForegroundColor Yellow
    $targetImports = @'
  <Import Project="..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets" Condition="Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')" />
  <Import Project="..\packages\Fody.6.9.3\build\Fody.targets" Condition="Exists('..\packages\Fody.6.9.3\build\Fody.targets')" />
'@

    $csprojContent = $csprojContent -replace `
        '(</Project>)', `
        "$targetImports`r`n`$1"
    $modified = $true
    Write-Host "  ✓ Target imports added" -ForegroundColor Green
} else {
    Write-Host "  ✓ Target imports already exist" -ForegroundColor Green
}

if ($modified) {
    Set-Content -Path $csprojPath -Value $csprojContent -NoNewline
    Write-Host "  ✓ Project file updated" -ForegroundColor Green
}

# Step 3: Restore NuGet packages
Write-Host ""
Write-Host "Step 3: Restoring NuGet packages..." -ForegroundColor Green

$msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1

if ($msbuildPath) {
    Write-Host "  Using MSBuild: $msbuildPath" -ForegroundColor Cyan
    & $msbuildPath -t:restore "DatabaseBackupService.sln" -v:minimal

    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ NuGet packages restored successfully" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Package restore had errors (exit code: $LASTEXITCODE)" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠ MSBuild not found. Please restore packages manually." -ForegroundColor Yellow
    Write-Host "    Run: msbuild -t:restore DatabaseBackupService.sln" -ForegroundColor Cyan
}

# Step 4: Summary
Write-Host ""
Write-Host "=== Configuration Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Open the solution in Visual Studio" -ForegroundColor White
Write-Host "  2. Build both projects in Release configuration" -ForegroundColor White
Write-Host "  3. Find single EXE files in:" -ForegroundColor White
Write-Host "     - DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe" -ForegroundColor Cyan
Write-Host "     - DatabaseBackupService.ConfigUI.NetFx\bin\Release\DatabaseBackupService.ConfigUI.NetFx.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "✓ Both projects are now configured for single-EXE publishing!" -ForegroundColor Green
Write-Host ""
