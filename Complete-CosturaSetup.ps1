# Complete-CosturaSetup.ps1
# Run this script to finalize Costura.Fody configuration for single-EXE publishing

Write-Host "=== Completing Costura.Fody Setup ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Close Visual Studio
Write-Host "IMPORTANT: Please close Visual Studio now." -ForegroundColor Yellow
Write-Host "Press any key after closing Visual Studio..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Write-Host ""

$csprojPath = "DatabaseBackupService.NetFx\DatabaseBackupService.NetFx.csproj"
$backupPath = "$csprojPath.backup"

# Backup original
Write-Host "Creating backup of project file..." -ForegroundColor Cyan
Copy-Item $csprojPath $backupPath -Force
Write-Host "  Backup created: $backupPath" -ForegroundColor Green
Write-Host ""

# Read project file
$xml = [xml](Get-Content $csprojPath)
$ns = @{ms = "http://schemas.microsoft.com/developer/msbuild/2003"}

# Check if Costura import already exists
$existingImport = Select-Xml -Xml $xml -XPath "//ms:Import[@Project='..\\packages\\Costura.Fody.6.0.0\\build\\Costura.Fody.props']" -Namespace $ns

if (-not $existingImport) {
    Write-Host "Adding Costura.Fody configuration..." -ForegroundColor Cyan

    # Create import element
    $import = $xml.CreateElement("Import", $xml.Project.NamespaceURI)
    $import.SetAttribute("Project", "..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props")
    $import.SetAttribute("Condition", "Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')")

    # Insert as first child after Project opening tag
    $firstChild = $xml.Project.FirstChild
    $xml.Project.InsertBefore($import, $firstChild) | Out-Null

    Write-Host "  ✓ Added Costura.Fody.props import" -ForegroundColor Green
} else {
    Write-Host "  ✓ Costura.Fody.props import already exists" -ForegroundColor Green
}

# Find the target imports section (before </Project>)
$lastImport = Select-Xml -Xml $xml -XPath "//ms:Import[@Project='..\\packages\\System.ValueTuple.4.6.2\\build\\net471\\System.ValueTuple.targets']" -Namespace $ns

if ($lastImport -and -not (Select-Xml -Xml $xml -XPath "//ms:Import[@Project='..\\packages\\Costura.Fody.6.0.0\\build\\Costura.Fody.targets']" -Namespace $ns)) {
    Write-Host "Adding Costura.Fody target imports..." -ForegroundColor Cyan

    # Create Costura.Fody.targets import
    $costuraTargets = $xml.CreateElement("Import", $xml.Project.NamespaceURI)
    $costuraTargets.SetAttribute("Project", "..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets")
    $costuraTargets.SetAttribute("Condition", "Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')")

    # Create Fody.targets import
    $fodyTargets = $xml.CreateElement("Import", $xml.Project.NamespaceURI)
    $fodyTargets.SetAttribute("Project", "..\packages\Fody.6.9.3\build\Fody.targets")
    $fodyTargets.SetAttribute("Condition", "Exists('..\packages\Fody.6.9.3\build\Fody.targets')")

    # Add after the last import
    $xml.Project.AppendChild($costuraTargets) | Out-Null
    $xml.Project.AppendChild($fodyTargets) | Out-Null

    Write-Host "  ✓ Added Costura.Fody.targets import" -ForegroundColor Green
    Write-Host "  ✓ Added Fody.targets import" -ForegroundColor Green
} else {
    Write-Host "  ✓ Target imports already exist" -ForegroundColor Green
}

# Add error conditions in EnsureNuGetPackageBuildImports target
$target = Select-Xml -Xml $xml -XPath "//ms:Target[@Name='EnsureNuGetPackageBuildImports']" -Namespace $ns

if ($target -and -not (Select-Xml -Xml $xml -XPath "//ms:Error[contains(@Condition, 'Costura.Fody.6.0.0')]" -Namespace $ns)) {
    Write-Host "Adding error conditions..." -ForegroundColor Cyan

    $targetNode = $target.Node

    # Create error elements
    $error1 = $xml.CreateElement("Error", $xml.Project.NamespaceURI)
    $error1.SetAttribute("Condition", "!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props')")
    $error1.SetAttribute("Text", "`$([System.String]::Format('`$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.props'))")

    $error2 = $xml.CreateElement("Error", $xml.Project.NamespaceURI)
    $error2.SetAttribute("Condition", "!Exists('..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets')")
    $error2.SetAttribute("Text", "`$([System.String]::Format('`$(ErrorText)', '..\packages\Costura.Fody.6.0.0\build\Costura.Fody.targets'))")

    $error3 = $xml.CreateElement("Error", $xml.Project.NamespaceURI)
    $error3.SetAttribute("Condition", "!Exists('..\packages\Fody.6.9.3\build\Fody.targets')")
    $error3.SetAttribute("Text", "`$([System.String]::Format('`$(ErrorText)', '..\packages\Fody.6.9.3\build\Fody.targets'))")

    $targetNode.AppendChild($error1) | Out-Null
    $targetNode.AppendChild($error2) | Out-Null
    $targetNode.AppendChild($error3) | Out-Null

    Write-Host "  ✓ Added error conditions" -ForegroundColor Green
} else {
    Write-Host "  ✓ Error conditions already exist" -ForegroundColor Green
}

# Save the file
$xml.Save((Resolve-Path $csprojPath).Path)
Write-Host ""
Write-Host "✓ Project file updated successfully!" -ForegroundColor Green
Write-Host ""

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
$msbuild = & "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\NuGet\msbuild.exe" -t:restore "DatabaseBackupService.sln" 2>&1

if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Packages restored" -ForegroundColor Green
} else {
    Write-Host "  Trying alternative restore method..." -ForegroundColor Yellow
    nuget restore DatabaseBackupService.sln 2>&1
}

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Open Visual Studio" -ForegroundColor White
Write-Host "  2. Build the solution in Release configuration" -ForegroundColor White
Write-Host "  3. Find your single EXE files at:" -ForegroundColor White
Write-Host "     - DatabaseBackupService.NetFx\bin\Release\DatabaseBackupService.NetFx.exe" -ForegroundColor Yellow
Write-Host "     - DatabaseBackupService.ConfigUI.NetFx\bin\Release\DatabaseBackupService.ConfigUI.NetFx.exe" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
