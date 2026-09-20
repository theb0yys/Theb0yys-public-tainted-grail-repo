[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$RestoreLatestBackup,

    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = (Resolve-Path -LiteralPath $Project).Path
$projectDir = Split-Path -Parent $projectPath

[xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
$propertyGroups = @($projectXml.Project.PropertyGroup)
$assemblyName = [string](
    $propertyGroups |
        ForEach-Object { $_.AssemblyName } |
        Where-Object { $_ } |
        Select-Object -First 1
)
$targetFramework = [string](
    $propertyGroups |
        ForEach-Object { $_.TargetFramework } |
        Where-Object { $_ } |
        Select-Object -First 1
)

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
}

if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    throw "TargetFramework was not found in the project."
}

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet

$artifact = Join-Path $projectDir "bin\$Configuration\$targetFramework\$assemblyName.dll"
$destinationDir = Join-Path $environment.GameRoot "BepInEx\plugins\$assemblyName"
$installedDll = Join-Path $destinationDir "$assemblyName.dll"
$backupDir = Join-Path $destinationDir "_backup"

$restorePerformed = $false
$restoredFrom = $null
$preRestoreBackup = $null

if ($RestoreLatestBackup) {
    if (-not (Test-Path -LiteralPath $backupDir -PathType Container)) {
        throw "Backup directory does not exist: $backupDir"
    }

    $escapedAssembly = [regex]::Escape($assemblyName)
    $backups = @(
        Get-ChildItem -LiteralPath $backupDir -Filter "*.dll" -File |
            Where-Object { $_.Name -match ("^" + $escapedAssembly + "\.\d{8}-\d{6}\.dll$") } |
            Sort-Object LastWriteTimeUtc -Descending
    )

    if ($backups.Count -eq 0) {
        throw "No install-created backup matching '$assemblyName.YYYYMMDD-HHmmss.dll' was found."
    }

    New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null

    if (Test-Path -LiteralPath $installedDll -PathType Leaf) {
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $preRestoreBackup = Join-Path $backupDir "$assemblyName.pre-restore.$timestamp.dll"
        Copy-Item -LiteralPath $installedDll -Destination $preRestoreBackup
    }

    $restoredFrom = $backups[0].FullName
    Copy-Item -LiteralPath $restoredFrom -Destination $installedDll -Force

    $sourceHash = (Get-FileHash -LiteralPath $restoredFrom -Algorithm SHA256).Hash
    $restoredHash = (Get-FileHash -LiteralPath $installedDll -Algorithm SHA256).Hash

    if ($sourceHash -ne $restoredHash) {
        throw "Rollback copy hash verification failed."
    }

    $restorePerformed = $true
}

$artifactExists = Test-Path -LiteralPath $artifact -PathType Leaf
$installedExists = Test-Path -LiteralPath $installedDll -PathType Leaf

$artifactHash = if ($artifactExists) { (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash } else { $null }
$installedHash = if ($installedExists) { (Get-FileHash -LiteralPath $installedDll -Algorithm SHA256).Hash } else { $null }

if (-not $artifactExists) {
    $state = "BUILD_NOT_FOUND"
}
elseif (-not $installedExists) {
    $state = "NOT_INSTALLED"
}
elseif ($artifactHash -eq $installedHash) {
    $state = "MATCH"
}
else {
    $state = "DIFFERENT"
}

$backupCount = if (Test-Path -LiteralPath $backupDir -PathType Container) {
    @(
        Get-ChildItem -LiteralPath $backupDir -Filter "*.dll" -File
    ).Count
}
else {
    0
}

$result = [pscustomobject]@{
    Project = $projectPath
    AssemblyName = $assemblyName
    Configuration = $Configuration
    Artifact = $artifact
    ArtifactExists = $artifactExists
    ArtifactSHA256 = $artifactHash
    InstalledDll = $installedDll
    InstalledExists = $installedExists
    InstalledSHA256 = $installedHash
    State = $state
    BackupDirectory = $backupDir
    BackupCount = $backupCount
    RestorePerformed = $restorePerformed
    RestoredFrom = $restoredFrom
    PreRestoreBackup = $preRestoreBackup
}

if (-not $Quiet) {
    $result | Format-List | Out-Host

    if ($state -eq "DIFFERENT") {
        Write-Host "The installed DLL is not the current build artifact."
    }
    elseif ($state -eq "MATCH") {
        Write-Host "The installed DLL matches the current build artifact."
    }
}

return $result
