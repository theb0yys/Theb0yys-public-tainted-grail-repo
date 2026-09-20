[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = (Resolve-Path -LiteralPath $Project).Path

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet

if (-not $environment.Ready) {
    throw ("FoA environment is not ready: " + ($environment.Issues -join " | "))
}

if (-not $SkipBuild) {
    $buildScript = Join-Path $PSScriptRoot "Build-FoAMod.ps1"
    $buildResult = & $buildScript -Project $projectPath -GameRoot $environment.GameRoot -Configuration $Configuration
    if (-not $buildResult.ArtifactExists) {
        throw "Build completed without the expected DLL artifact."
    }
}

[xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
$assemblyName = [string]$projectXml.Project.PropertyGroup.AssemblyName
$targetFramework = [string]$projectXml.Project.PropertyGroup.TargetFramework

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
}

$artifact = Join-Path (Split-Path -Parent $projectPath) "bin\$Configuration\$targetFramework\$assemblyName.dll"

if (-not (Test-Path -LiteralPath $artifact -PathType Leaf)) {
    throw "Build artifact was not found: $artifact"
}

$pluginRoot = Join-Path $environment.GameRoot "BepInEx\plugins"
$destinationDir = Join-Path $pluginRoot $assemblyName
$destinationDll = Join-Path $destinationDir ($assemblyName + ".dll")

New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null

$backupPath = $null
if (Test-Path -LiteralPath $destinationDll -PathType Leaf) {
    $backupDir = Join-Path $destinationDir "_backup"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = Join-Path $backupDir ($assemblyName + "." + $timestamp + ".dll")
    Copy-Item -LiteralPath $destinationDll -Destination $backupPath
}

Copy-Item -LiteralPath $artifact -Destination $destinationDll -Force

$sourceHash = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash
$installedHash = (Get-FileHash -LiteralPath $destinationDll -Algorithm SHA256).Hash

if ($sourceHash -ne $installedHash) {
    throw "Installed DLL hash does not match the build artifact."
}

$result = [pscustomobject]@{
    Project = $projectPath
    Runtime = $environment.Runtime
    Artifact = $artifact
    InstalledDll = $destinationDll
    Backup = $backupPath
    SHA256 = $installedHash
    CopyVerified = $true
}

$result | Format-List | Out-Host
Write-Host ""
Write-Host "Install copy verified. This does not prove the plug-in loads."
Write-Host "Use Watch-FoALog.ps1 while launching the game to verify loader output."

return $result
