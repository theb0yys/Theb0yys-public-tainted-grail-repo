[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [string]$OutputRoot,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$Version,

    [string[]]$AdditionalFile = @(),

    [switch]$SkipBuild,

    [switch]$Zip,

    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-PluginVersion {
    param(
        [string]$ProjectDirectory,
        [xml]$ProjectXml,
        [string]$ExplicitVersion
    )

    if (-not [string]::IsNullOrWhiteSpace($ExplicitVersion)) {
        return $ExplicitVersion
    }

    $projectVersion = [string](
        @($ProjectXml.Project.PropertyGroup) |
            ForEach-Object { $_.Version } |
            Where-Object { $_ } |
            Select-Object -First 1
    )

    if (-not [string]::IsNullOrWhiteSpace($projectVersion)) {
        return $projectVersion
    }

    $source = (
        Get-ChildItem -LiteralPath $ProjectDirectory -Filter "*.cs" -File -Recurse |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }
    ) -join [Environment]::NewLine

    $match = [regex]::Match($source, 'public\s+const\s+string\s+PluginVersion\s*=\s*"([^"]+)"')
    if ($match.Success) {
        return $match.Groups[1].Value
    }

    return "unversioned"
}

function Assert-PackageSafety {
    param([string]$PackageRoot)

    $forbiddenNames = @(
        "TG.Main.dll",
        "Assembly-CSharp.dll",
        "GameAssembly.dll",
        "global-metadata.dat",
        "BepInEx.dll",
        "BepInEx.Core.dll",
        "BepInEx.Unity.Common.dll",
        "BepInEx.Unity.Mono.dll",
        "BepInEx.Unity.IL2CPP.dll",
        "0Harmony.dll",
        "Il2CppInterop.Runtime.dll",
        "Il2Cppmscorlib.dll"
    )

    $failures = New-Object System.Collections.Generic.List[string]

    Get-ChildItem -LiteralPath $PackageRoot -File -Recurse | ForEach-Object {
        if ($forbiddenNames -contains $_.Name -or $_.Name -like "UnityEngine*.dll") {
            $failures.Add("Forbidden game/loader/runtime binary: $($_.FullName)")
        }

        if ($_.Extension -in @(".exe", ".pdb", ".7z", ".rar", ".pak", ".assets", ".bundle", ".unity3d")) {
            $failures.Add("Unexpected release file type: $($_.FullName)")
        }

        if ($_.Length -gt 100MB) {
            $failures.Add("Unexpectedly large release file: $($_.FullName) ($($_.Length) bytes)")
        }
    }

    $textFiles = @(
        Get-ChildItem -LiteralPath $PackageRoot -File -Recurse |
            Where-Object { $_.Extension -in @(".md", ".txt", ".json", ".cfg", ".ini", ".xml") }
    )

    foreach ($file in $textFiles) {
        try {
            $text = Get-Content -LiteralPath $file.FullName -Raw
        }
        catch {
            continue
        }

        if ($text -match 'gh[pousr]_[A-Za-z0-9_]{20,}' -or
            $text -match 'github_pat_[A-Za-z0-9_]{20,}' -or
            $text -match 'BEGIN [A-Z ]*PRIVATE KEY' -or
            $text -match '[A-Za-z]:\\Users\\[^\\\s]+') {
            $failures.Add("Possible secret/private path in release text: $($file.FullName)")
        }
    }

    if ($failures.Count -gt 0) {
        $message = "Release safety check failed:" + [Environment]::NewLine + " - " +
            (($failures | Sort-Object -Unique) -join ([Environment]::NewLine + " - "))
        throw $message
    }
}

$projectPath = (Resolve-Path -LiteralPath $Project).Path
$projectDir = Split-Path -Parent $projectPath

$doctorScript = Join-Path $PSScriptRoot "Test-FoAModProject.ps1"
$doctor = & $doctorScript -Project $projectPath -GameRoot $GameRoot -Quiet

if (-not $doctor.Ready) {
    throw "Project doctor failed. Resolve blocking findings before packaging."
}

if (-not $SkipBuild) {
    $buildScript = Join-Path $PSScriptRoot "Build-FoAMod.ps1"
    $build = & $buildScript -Project $projectPath -GameRoot $GameRoot -Configuration $Configuration
    if (-not $build.ArtifactExists) {
        throw "Build did not produce the expected DLL."
    }
}

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

$projectText = Get-Content -LiteralPath $projectPath -Raw
$runtime = if ($projectText -match 'BepInEx\.Unity\.IL2CPP') { "il2cpp" } else { "mono" }
$packageVersion = Get-PluginVersion -ProjectDirectory $projectDir -ProjectXml $projectXml -ExplicitVersion $Version

$artifact = Join-Path $projectDir "bin\$Configuration\$targetFramework\$assemblyName.dll"
if (-not (Test-Path -LiteralPath $artifact -PathType Leaf)) {
    throw "Build artifact was not found: $artifact"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $projectDir "release"
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null
$outputRootPath = (Resolve-Path -LiteralPath $OutputRoot).Path

$packageName = "$assemblyName-$packageVersion-$runtime"
$packageRoot = Join-Path $outputRootPath $packageName

if (Test-Path -LiteralPath $packageRoot) {
    if (-not $Force) {
        throw "Release package directory already exists: $packageRoot. Use -Force to replace that staged package."
    }

    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}

$pluginDir = Join-Path $packageRoot "plugins\$assemblyName"
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null

Copy-Item -LiteralPath $artifact -Destination (Join-Path $pluginDir "$assemblyName.dll")

$documentCandidates = @(
    "README.md",
    "CHANGELOG.md",
    "changelog.md",
    "LICENSE",
    "LICENSE.md"
)

$documentRoots = @(
    $projectDir,
    (Split-Path -Parent $projectDir)
) | Where-Object { $_ } | Select-Object -Unique

foreach ($name in $documentCandidates) {
    foreach ($root in $documentRoots) {
        $source = Join-Path $root $name
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            $destination = Join-Path $pluginDir ([System.IO.Path]::GetFileName($source))
            if (-not (Test-Path -LiteralPath $destination -PathType Leaf)) {
                Copy-Item -LiteralPath $source -Destination $destination
            }
            break
        }
    }
}

foreach ($extra in $AdditionalFile) {
    $resolved = (Resolve-Path -LiteralPath $extra).Path
    Copy-Item -LiteralPath $resolved -Destination (Join-Path $pluginDir ([System.IO.Path]::GetFileName($resolved)))
}

Assert-PackageSafety -PackageRoot $packageRoot

$sourceCommit = $null
try {
    $sourceCommit = (& git -C $projectDir rev-parse HEAD 2>$null | Select-Object -First 1)
}
catch {
    $sourceCommit = $null
}

$payloadRows = @(
    Get-ChildItem -LiteralPath $packageRoot -File -Recurse |
        ForEach-Object {
            $relative = $_.FullName.Substring($packageRoot.Length).TrimStart([char]92, [char]47).Replace('\', '/')
            [pscustomobject]@{
                Path = $relative
                Length = $_.Length
                SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
            }
        }
)

$manifest = [pscustomobject]@{
    Format = "foa-release-package/1"
    Package = $packageName
    AssemblyName = $assemblyName
    Version = $packageVersion
    Runtime = $runtime
    Configuration = $Configuration
    SourceProject = [System.IO.Path]::GetFileName($projectPath)
    SourceCommit = $sourceCommit
    ProjectDoctor = "PASSED"
    RuntimeValidation = "NOT_RUN"
    FeatureValidation = "NOT_RUN"
    PersistenceValidation = "NOT_RUN_UNLESS_SEPARATELY_RECORDED"
    Files = $payloadRows
}

$manifestPath = Join-Path $pluginDir "release-manifest.json"
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $manifestPath -Encoding UTF8

$checksumRows = @(
    Get-ChildItem -LiteralPath $packageRoot -File -Recurse |
        Sort-Object FullName |
        ForEach-Object {
            $relative = $_.FullName.Substring($packageRoot.Length).TrimStart([char]92, [char]47).Replace('\', '/')
            "{0}  {1}" -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash, $relative
        }
)

$checksumPath = Join-Path $packageRoot "SHA256SUMS.txt"
$checksumRows | Set-Content -LiteralPath $checksumPath -Encoding ASCII

Assert-PackageSafety -PackageRoot $packageRoot

$archivePath = $null
$archiveHash = $null

if ($Zip) {
    $archivePath = Join-Path $outputRootPath ($packageName + ".zip")

    if (Test-Path -LiteralPath $archivePath -PathType Leaf) {
        if (-not $Force) {
            throw "Release archive already exists: $archivePath"
        }

        Remove-Item -LiteralPath $archivePath -Force
    }

    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $archivePath -CompressionLevel Optimal
    $archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
}

$result = [pscustomobject]@{
    Package = $packageName
    PackageDirectory = $packageRoot
    Runtime = $runtime
    Version = $packageVersion
    ArtifactSHA256 = (Get-FileHash -LiteralPath $artifact -Algorithm SHA256).Hash
    Archive = $archivePath
    ArchiveSHA256 = $archiveHash
    RuntimeValidation = "NOT_RUN"
    FeatureValidation = "NOT_RUN"
}

$result | Format-List | Out-Host
Write-Host ""
Write-Host "Release package structure and hashes are ready."
Write-Host "Packaging does not prove plug-in load, feature behaviour, save safety, or release readiness."

return $result
