[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FoAGameRoot,

    [string]$ExpectedBuildId = '24270691',
    [string]$ExpectedBranch = 'mono',
    [string]$ExpectedTgMainSha256 = '749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982',
    [string]$ExpectedTgMainMvid = '68528841-991C-481E-BD94-7F1776FC3579',
    [string]$AppManifestPath = '',
    [string]$EvidenceRoot = '',
    [switch]$Deploy,
    [switch]$RequireLiveMatch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($env:OS -ne 'Windows_NT') {
    throw 'Tainted Weapons Mono reconciliation is Windows-only.'
}
if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw 'PowerShell 7 or newer is required.'
}

$repoRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw 'Unable to resolve repository root.'
}
$repoRoot = (Resolve-Path -LiteralPath $repoRoot).Path
Set-Location $repoRoot

$sourceCommit = (& git rev-parse HEAD).Trim().ToLowerInvariant()
if ($sourceCommit -notmatch '^[0-9a-f]{40}$') {
    throw 'Unable to resolve exact source commit.'
}

$workingTreeRows = @(& git status --porcelain)
if ($workingTreeRows.Count -ne 0) {
    throw "Repository must be clean before binary reconciliation. Status=$($workingTreeRows -join ' | ')"
}

$markerRows = @(& git grep -n -E '^(<<<<<<<|=======|>>>>>>>)' -- 'mods/tainted-weapons/src' 2>$null)
if ($markerRows.Count -ne 0) {
    throw "Tainted Weapons source merge-marker scan failed. Matches=$($markerRows -join ' | ')"
}

$gameRoot = (Resolve-Path -LiteralPath $FoAGameRoot).Path
$dataRoot = Join-Path $gameRoot 'Fall of Avalon_Data'
$managedRoot = Join-Path $dataRoot 'Managed'
if (-not (Test-Path -LiteralPath $managedRoot -PathType Container)) {
    throw "Managed directory was not found: $managedRoot"
}

if ([string]::IsNullOrWhiteSpace($AppManifestPath)) {
    $commonRoot = Split-Path -Parent $gameRoot
    $steamAppsRoot = Split-Path -Parent $commonRoot
    $AppManifestPath = Join-Path $steamAppsRoot 'appmanifest_1466060.acf'
}
$AppManifestPath = (Resolve-Path -LiteralPath $AppManifestPath).Path

if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssZ')
    $EvidenceRoot = Join-Path $repoRoot "documents\runtime-evidence\weapon-framework\mono-$ExpectedBuildId\binary-reconciliation\$stamp"
}
if (Test-Path -LiteralPath $EvidenceRoot) {
    if (@(Get-ChildItem -LiteralPath $EvidenceRoot -Force -ErrorAction SilentlyContinue).Count -ne 0) {
        throw "EvidenceRoot must be absent or empty: $EvidenceRoot"
    }
} else {
    New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
}
$EvidenceRoot = (Resolve-Path -LiteralPath $EvidenceRoot).Path

$failures = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()

function Add-Failure([string]$Message) {
    if (-not [string]::IsNullOrWhiteSpace($Message) -and -not $failures.Contains($Message)) {
        $failures.Add($Message)
    }
}
function Add-Warning([string]$Message) {
    if (-not [string]::IsNullOrWhiteSpace($Message) -and -not $warnings.Contains($Message)) {
        $warnings.Add($Message)
    }
}
function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}
function Write-Json([object]$Value, [string]$Path) {
    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    $Value | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
}
function Get-GameProcesses([string]$Root) {
    $prefix = [System.IO.Path]::GetFullPath($Root).TrimEnd('\') + '\'
    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($process in Get-Process -ErrorAction SilentlyContinue) {
        try {
            if ([string]::IsNullOrWhiteSpace($process.Path)) { continue }
            $path = [System.IO.Path]::GetFullPath($process.Path)
            if ($path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                $rows.Add($process)
            }
        } catch {
            continue
        }
    }
    return @($rows)
}
function Read-VdfValue([string]$Text, [string]$Key) {
    $match = [regex]::Match($Text, '"' + [regex]::Escape($Key) + '"\s+"([^"]*)"', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($match.Success) {
        return $match.Groups[1].Value
    }

    return ''
}

if (-not ('ManagedPeMetadata' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

public sealed class ManagedPeInfo {
    public string AssemblyName { get; set; } = "";
    public string AssemblyVersion { get; set; } = "";
    public string Mvid { get; set; } = "";
    public bool IsManaged { get; set; }
}

public static class ManagedPeMetadata {
    public static ManagedPeInfo Read(string path) {
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var pe = new PEReader(stream, PEStreamOptions.LeaveOpen);
        var result = new ManagedPeInfo();
        if (!pe.HasMetadata) return result;
        var reader = pe.GetMetadataReader();
        var module = reader.GetModuleDefinition();
        result.Mvid = reader.GetGuid(module.Mvid).ToString().ToUpperInvariant();
        result.IsManaged = true;
        if (reader.IsAssembly) {
            var assembly = reader.GetAssemblyDefinition();
            result.AssemblyName = reader.GetString(assembly.Name);
            result.AssemblyVersion = assembly.Version.ToString();
        }
        return result;
    }
}
'@
}

function Get-ManagedFileRecord([string]$Path, [string]$RelativeBase) {
    $item = Get-Item -LiteralPath $Path
    $metadata = $null
    try {
        $metadata = [ManagedPeMetadata]::Read($item.FullName)
    } catch {
        Add-Warning "metadata-read-failed:$($item.Name):$($_.Exception.GetType().Name):$($_.Exception.Message)"
    }
    $relative = [System.IO.Path]::GetRelativePath($RelativeBase, $item.FullName).Replace('\', '/')
    return [ordered]@{
        relative_path = $relative
        bytes = [int64]$item.Length
        sha256 = Get-Sha256 $item.FullName
        last_write_utc = $item.LastWriteTimeUtc.ToString('O')
        managed = $null -ne $metadata -and $metadata.IsManaged
        assembly_name = if ($null -ne $metadata) { $metadata.AssemblyName } else { '' }
        assembly_version = if ($null -ne $metadata) { $metadata.AssemblyVersion } else { '' }
        mvid = if ($null -ne $metadata) { $metadata.Mvid } else { '' }
    }
}

$appManifestText = Get-Content -LiteralPath $AppManifestPath -Raw
$actualBuildId = Read-VdfValue $appManifestText 'buildid'
$targetBuildId = Read-VdfValue $appManifestText 'TargetBuildID'
$actualBranch = Read-VdfValue $appManifestText 'BetaKey'
if ([string]::IsNullOrWhiteSpace($actualBranch)) {
    $actualBranch = Read-VdfValue $appManifestText 'betakey'
}
if ($actualBuildId -ne $ExpectedBuildId) {
        Add-Failure "steam-build-mismatch:expected=${ExpectedBuildId}:actual=${actualBuildId}"
}
if (-not [string]::IsNullOrWhiteSpace($targetBuildId) -and $targetBuildId -ne $ExpectedBuildId) {
        Add-Failure "steam-target-build-mismatch:expected=${ExpectedBuildId}:actual=${targetBuildId}"
}
if ($actualBranch -ne $ExpectedBranch) {
        Add-Failure "steam-branch-mismatch:expected=${ExpectedBranch}:actual=${actualBranch}"
}

$scriptingAssembliesPath = Join-Path $dataRoot 'ScriptingAssemblies.json'
if (-not (Test-Path -LiteralPath $scriptingAssembliesPath -PathType Leaf)) {
    Add-Failure "scripting-assemblies-missing:$scriptingAssembliesPath"
}

$manifestRows = @()
if (Test-Path -LiteralPath $scriptingAssembliesPath -PathType Leaf) {
    $manifestObject = Get-Content -LiteralPath $scriptingAssembliesPath -Raw | ConvertFrom-Json
    $names = if ($null -ne $manifestObject.names) { @($manifestObject.names) }
        elseif ($null -ne $manifestObject.assemblies) { @($manifestObject.assemblies) }
        else { @() }
    $types = if ($null -ne $manifestObject.types) { @($manifestObject.types) } else { @() }
    for ($index = 0; $index -lt $names.Count; $index++) {
        $name = [string]$names[$index]
        $path = Join-Path $managedRoot $name
        $row = [ordered]@{
            ordinal = $index
            assembly_name = $name
            manifest_type = if ($index -lt $types.Count) { $types[$index] } else { $null }
            file_present = Test-Path -LiteralPath $path -PathType Leaf
            bytes = 0
            sha256 = ''
            assembly_version = ''
            mvid = ''
            error = ''
        }
        if ($row.file_present) {
            try {
                $record = Get-ManagedFileRecord $path $managedRoot
                $row.bytes = $record.bytes
                $row.sha256 = $record.sha256
                $row.assembly_version = $record.assembly_version
                $row.mvid = $record.mvid
            } catch {
                $row.error = "$($_.Exception.GetType().Name):$($_.Exception.Message)"
                Add-Failure "manifest-assembly-read-failed:${name}:$($row.error)"
            }
        } else {
            $row.error = 'file-missing'
            Add-Failure "manifest-assembly-missing:$name"
        }
        $manifestRows += [pscustomobject]$row
    }
}
$manifestRows | Export-Csv -LiteralPath (Join-Path $EvidenceRoot 'scripting-assemblies.csv') -NoTypeInformation -Encoding utf8BOM

$listedNames = @($manifestRows | ForEach-Object { $_.assembly_name })
$extraManaged = @(Get-ChildItem -LiteralPath $managedRoot -File -Filter '*.dll' | Where-Object { $listedNames -notcontains $_.Name } | Sort-Object Name)
$extraRows = foreach ($file in $extraManaged) {
    try {
        [pscustomobject](Get-ManagedFileRecord $file.FullName $managedRoot)
    } catch {
        [pscustomobject]@{ relative_path=$file.Name; bytes=$file.Length; sha256=''; last_write_utc=$file.LastWriteTimeUtc.ToString('O'); managed=$false; assembly_name=''; assembly_version=''; mvid=''; error="$($_.Exception.GetType().Name):$($_.Exception.Message)" }
    }
}
$extraRows | Export-Csv -LiteralPath (Join-Path $EvidenceRoot 'extra-managed-assemblies.csv') -NoTypeInformation -Encoding utf8BOM

$coreRelativePaths = @(
    'Fall of Avalon.exe',
    'UnityPlayer.dll',
    'Fall of Avalon_Data/globalgamemanagers',
    'Fall of Avalon_Data/boot.config',
    'Fall of Avalon_Data/ScriptingAssemblies.json',
    'Fall of Avalon_Data/Managed/TG.Main.dll',
    'Fall of Avalon_Data/Managed/Awaken.Babel.dll',
    'Fall of Avalon_Data/Managed/Awaken.Utility.dll',
    'Fall of Avalon_Data/Managed/Awaken.ECS.dll',
    'Fall of Avalon_Data/Managed/Awaken.Kandra.dll',
    'Fall of Avalon_Data/Managed/Unity.Addressables.dll',
    'Fall of Avalon_Data/Managed/Unity.ResourceManager.dll',
    'Fall of Avalon_Data/Managed/Unity.Entities.dll',
    'Fall of Avalon_Data/Managed/Unity.Entities.Graphics.dll',
    'BepInEx/core/BepInEx.dll',
    'BepInEx/core/0Harmony.dll'
)
$coreRows = @()
foreach ($relative in $coreRelativePaths) {
    $path = Join-Path $gameRoot ($relative.Replace('/', '\'))
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Warning "core-artifact-missing:$relative"
        continue
    }
    $coreRows += [pscustomobject](Get-ManagedFileRecord $path $gameRoot)
}
$coreRows | Export-Csv -LiteralPath (Join-Path $EvidenceRoot 'core-artifacts.csv') -NoTypeInformation -Encoding utf8BOM

$tgMainRow = $coreRows | Where-Object { $_.relative_path -eq 'Fall of Avalon_Data/Managed/TG.Main.dll' } | Select-Object -First 1
if ($null -eq $tgMainRow) {
    Add-Failure 'tg-main-not-captured'
} else {
    if ($tgMainRow.sha256 -ne $ExpectedTgMainSha256) {
    Add-Failure "tg-main-sha-mismatch:expected=${ExpectedTgMainSha256}:actual=$($tgMainRow.sha256)"
    }
    if ($tgMainRow.mvid -ne $ExpectedTgMainMvid) {
    Add-Failure "tg-main-mvid-mismatch:expected=${ExpectedTgMainMvid}:actual=$($tgMainRow.mvid)"
    }
}

$sourceFiles = @(& git ls-files -- 'mods/tainted-weapons/src')
$sourceRows = foreach ($path in $sourceFiles) {
    $full = Join-Path $repoRoot $path
    if (-not (Test-Path -LiteralPath $full -PathType Leaf)) { continue }
    [pscustomobject]@{
        path = $path.Replace('\', '/')
        git_blob_sha = (& git hash-object -- $path).Trim().ToLowerInvariant()
        bytes = (Get-Item -LiteralPath $full).Length
        sha256 = Get-Sha256 $full
    }
}
$sourceRows | Export-Csv -LiteralPath (Join-Path $EvidenceRoot 'source-files.csv') -NoTypeInformation -Encoding utf8BOM

$pluginSourcePath = Join-Path $repoRoot 'mods\tainted-weapons\src\Plugin.cs'
$pluginSourceText = Get-Content -LiteralPath $pluginSourcePath -Raw
$pluginVersionMatch = [regex]::Match($pluginSourceText, 'PluginVersion\s*=\s*"([^"]+)"')
$pluginSourceVersion = if ($pluginVersionMatch.Success) { $pluginVersionMatch.Groups[1].Value } else { '' }
if ([string]::IsNullOrWhiteSpace($pluginSourceVersion)) {
    Add-Failure 'plugin-version-not-found'
}

$projectPath = Join-Path $repoRoot 'mods\tainted-weapons\src\TaintedWeapons.csproj'
$buildLog = Join-Path $EvidenceRoot 'build.log'
$buildArguments = @('build', $projectPath, '-c', 'Release', '--nologo', "-p:FoAGameRoot=$gameRoot")
$buildOutput = @(& dotnet @buildArguments 2>&1 | ForEach-Object { $_.ToString() })
$buildExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int]$LASTEXITCODE }
$buildOutput | Set-Content -LiteralPath $buildLog -Encoding utf8NoBOM
if ($buildExitCode -ne 0) {
    Add-Failure "release-build-failed:$buildExitCode"
}

$builtDllPath = Join-Path $repoRoot 'mods\tainted-weapons\src\bin\Release\netstandard2.1\TaintedWeapons.dll'
$builtRecord = $null
if (Test-Path -LiteralPath $builtDllPath -PathType Leaf) {
    $builtRecord = Get-ManagedFileRecord $builtDllPath $repoRoot
} else {
    Add-Failure "built-dll-missing:$builtDllPath"
}

$liveDllPath = Join-Path $gameRoot 'BepInEx\plugins\TaintedWeapons\TaintedWeapons.dll'
$backupPath = ''
if ($Deploy) {
    $running = @(Get-GameProcesses $gameRoot)
    if ($running.Count -ne 0) {
        Add-Failure "game-process-running:$($running.Name -join ',')"
    } elseif ($null -eq $builtRecord) {
        Add-Failure 'deploy-blocked-built-dll-unavailable'
    } else {
        $backupRoot = Join-Path $gameRoot ("BepInEx\_codex_backups\TaintedWeapons\binary-reconciliation-" + [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssZ'))
        New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null
        if (Test-Path -LiteralPath $liveDllPath -PathType Leaf) {
            $backupPath = Join-Path $backupRoot 'TaintedWeapons.dll'
            Copy-Item -LiteralPath $liveDllPath -Destination $backupPath -Force
        }
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $liveDllPath) | Out-Null
        Copy-Item -LiteralPath $builtDllPath -Destination $liveDllPath -Force
    }
}

$liveRecord = $null
if (Test-Path -LiteralPath $liveDllPath -PathType Leaf) {
    $liveRecord = Get-ManagedFileRecord $liveDllPath $gameRoot
} elseif ($RequireLiveMatch -or $Deploy) {
    Add-Failure "live-dll-missing:$liveDllPath"
} else {
    Add-Warning "live-dll-not-found:$liveDllPath"
}

$builtLiveMatch = $false
if ($null -ne $builtRecord -and $null -ne $liveRecord) {
    $builtLiveMatch =
        $builtRecord.sha256 -eq $liveRecord.sha256 -and
        $builtRecord.bytes -eq $liveRecord.bytes -and
        $builtRecord.mvid -eq $liveRecord.mvid -and
        $builtRecord.assembly_version -eq $liveRecord.assembly_version
    if (($RequireLiveMatch -or $Deploy) -and -not $builtLiveMatch) {
        Add-Failure 'built-live-dll-mismatch'
    }
}

$receipt = [ordered]@{
    schema_version = 'tainted-weapons-mono-binary-reconciliation-v1'
    captured_at_utc = [DateTimeOffset]::UtcNow.ToString('O')
    repository = [ordered]@{
        commit = $sourceCommit
        working_tree_clean = $true
        weapon_source_merge_marker_scan = 'pass'
        source_files = @($sourceRows)
    }
    steam = [ordered]@{
        app_id = '1466060'
        app_manifest_path = $AppManifestPath
        app_manifest_sha256 = Get-Sha256 $AppManifestPath
        build_id = $actualBuildId
        target_build_id = $targetBuildId
        branch = $actualBranch
        expected_build_id = $ExpectedBuildId
        expected_branch = $ExpectedBranch
    }
    game_root = $gameRoot
    scripting_assemblies = [ordered]@{
        path = $scriptingAssembliesPath
        sha256 = if (Test-Path -LiteralPath $scriptingAssembliesPath) { Get-Sha256 $scriptingAssembliesPath } else { '' }
        listed_count = $manifestRows.Count
        present_count = @($manifestRows | Where-Object { $_.file_present }).Count
        missing_count = @($manifestRows | Where-Object { -not $_.file_present }).Count
        extra_managed_dll_count = $extraRows.Count
    }
    expected_tg_main = [ordered]@{ sha256=$ExpectedTgMainSha256; mvid=$ExpectedTgMainMvid }
    core_artifacts = @($coreRows)
    plugin = [ordered]@{
        source_version = $pluginSourceVersion
        project_path = [System.IO.Path]::GetRelativePath($repoRoot, $projectPath).Replace('\', '/')
        build_command = 'dotnet ' + ($buildArguments -join ' ')
        build_exit_code = $buildExitCode
        build_log = [System.IO.Path]::GetRelativePath($repoRoot, $buildLog).Replace('\', '/')
        built = $builtRecord
        live_path = $liveDllPath
        live = $liveRecord
        deploy_requested = [bool]$Deploy
        backup_path = $backupPath
        built_live_match = $builtLiveMatch
    }
    failures = @($failures)
    warnings = @($warnings)
    status = if ($failures.Count -ne 0) { 'FAIL' } elseif ($builtLiveMatch) { 'PASS' } else { 'BUILT_ONLY' }
}

$canonical = $receipt | ConvertTo-Json -Depth 100 -Compress
$sha = [System.Security.Cryptography.SHA256]::Create()
try {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($canonical)
    $receipt.receipt_sha256 = ([BitConverter]::ToString($sha.ComputeHash($bytes))).Replace('-', '').ToUpperInvariant()
} finally {
    $sha.Dispose()
}

Write-Json $receipt (Join-Path $EvidenceRoot 'binary-reconciliation.json')
Write-Host "Tainted Weapons binary reconciliation status: $($receipt.status)"
Write-Host "Evidence: $EvidenceRoot"
if ($failures.Count -ne 0) {
    foreach ($failure in $failures) { Write-Error $failure }
    exit 2
}
if (($RequireLiveMatch -or $Deploy) -and -not $builtLiveMatch) {
    exit 3
}
