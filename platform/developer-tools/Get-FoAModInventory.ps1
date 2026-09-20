[CmdletBinding()]
param(
    [string]$GameRoot,
    [string]$OutputPath,
    [switch]$IncludeBackups,
    [switch]$Quiet,
    [switch]$FailOnConflict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet
$pluginRoot = Join-Path $environment.GameRoot "BepInEx\plugins"

if (-not (Test-Path -LiteralPath $pluginRoot -PathType Container)) {
    throw "BepInEx plug-in directory was not found: $pluginRoot"
}

$dlls = @(
    Get-ChildItem -LiteralPath $pluginRoot -Filter "*.dll" -File -Recurse |
        Where-Object {
            $IncludeBackups -or $_.FullName -notmatch '[\\/]_backup[\\/]'
        }
)

$items = @(
    foreach ($dll in $dlls) {
        $assemblyName = $null
        $assemblyVersion = $null
        $metadataState = "PASSED"

        try {
            $identity = [System.Reflection.AssemblyName]::GetAssemblyName($dll.FullName)
            $assemblyName = $identity.Name
            $assemblyVersion = if ($null -ne $identity.Version) { $identity.Version.ToString() } else { $null }
        }
        catch {
            $metadataState = "FAILED"
        }

        $relativePath = $dll.FullName.Substring($pluginRoot.Length).TrimStart([char]92, [char]47).Replace('\', '/')
        $hash = (Get-FileHash -LiteralPath $dll.FullName -Algorithm SHA256).Hash
        $versionInfo = $dll.VersionInfo

        [pscustomobject]@{
            RelativePath = $relativePath
            FileName = $dll.Name
            AssemblyName = $assemblyName
            AssemblyVersion = $assemblyVersion
            FileVersion = $versionInfo.FileVersion
            ProductVersion = $versionInfo.ProductVersion
            Length = $dll.Length
            SHA256 = $hash
            MetadataState = $metadataState
        }
    }
)

$conflicts = New-Object System.Collections.Generic.List[object]

$assemblyGroups = @(
    $items |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_.AssemblyName) } |
        Group-Object AssemblyName |
        Where-Object { $_.Count -gt 1 }
)

foreach ($group in $assemblyGroups) {
    $hashes = @($group.Group.SHA256 | Sort-Object -Unique)
    $versions = @($group.Group.AssemblyVersion | Where-Object { $_ } | Sort-Object -Unique)

    if ($hashes.Count -eq 1) {
        $kind = "EXACT_DUPLICATE_ASSEMBLY"
    }
    elseif ($versions.Count -gt 1) {
        $kind = "MULTIPLE_ASSEMBLY_VERSIONS"
    }
    else {
        $kind = "DUPLICATE_ASSEMBLY_DIFFERENT_HASH"
    }

    $conflicts.Add([pscustomobject]@{
        Kind = $kind
        Identity = $group.Name
        Paths = @($group.Group.RelativePath)
        Versions = $versions
        Hashes = $hashes
    })
}

$fileGroups = @(
    $items |
        Group-Object FileName |
        Where-Object { $_.Count -gt 1 }
)

foreach ($group in $fileGroups) {
    $hashes = @($group.Group.SHA256 | Sort-Object -Unique)
    if ($hashes.Count -le 1) {
        continue
    }

    $conflicts.Add([pscustomobject]@{
        Kind = "DUPLICATE_FILENAME_DIFFERENT_HASH"
        Identity = $group.Name
        Paths = @($group.Group.RelativePath)
        Versions = @($group.Group.AssemblyVersion | Where-Object { $_ } | Sort-Object -Unique)
        Hashes = $hashes
    })
}

$result = [pscustomobject]@{
    Format = "foa-installed-mod-inventory/1"
    GameRoot = $environment.GameRoot
    Runtime = $environment.Runtime
    Loader = $environment.Loader
    PluginRoot = $pluginRoot
    IncludeBackups = [bool]$IncludeBackups
    ItemCount = $items.Count
    ConflictCount = $conflicts.Count
    Items = $items
    Conflicts = @($conflicts)
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

if (-not $Quiet) {
    $items | Sort-Object AssemblyName, RelativePath | Format-Table AssemblyName, AssemblyVersion, FileVersion, Length, RelativePath -AutoSize | Out-Host
    Write-Host ""

    if ($conflicts.Count -gt 0) {
        Write-Host "Potential duplicate/conflict findings:"
        $conflicts | Format-Table Kind, Identity, Paths -Wrap -AutoSize | Out-Host
    }
    else {
        Write-Host "No duplicate assembly/file-name conflicts detected."
    }

    Write-Host ""
    Write-Host ("Installed mod inventory: {0} DLLs, {1} conflict finding(s)." -f $items.Count, $conflicts.Count)
    Write-Host "This inventory reads assembly/file identity only; it does not extract BepInPlugin GUIDs or prove runtime compatibility."
}

if ($FailOnConflict -and $conflicts.Count -gt 0) {
    throw "Installed mod inventory found $($conflicts.Count) potential duplicate/conflict finding(s)."
}

return $result
