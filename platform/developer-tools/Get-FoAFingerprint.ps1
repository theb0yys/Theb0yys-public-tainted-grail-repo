[CmdletBinding()]
param(
    [string]$GameRoot,
    [string]$OutputPath,
    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet

$targets = New-Object System.Collections.Generic.List[object]

if ($environment.Runtime -eq "Mono") {
    $targets.Add([pscustomobject]@{ Key = "game/TG.Main.dll"; Path = (Join-Path $environment.ManagedDir "TG.Main.dll") })
    $targets.Add([pscustomobject]@{ Key = "loader/BepInEx.dll"; Path = (Join-Path $environment.BepInExCore "BepInEx.dll") })
    $targets.Add([pscustomobject]@{ Key = "loader/0Harmony.dll"; Path = (Join-Path $environment.BepInExCore "0Harmony.dll") })
}
elseif ($environment.Runtime -eq "IL2CPP") {
    $targets.Add([pscustomobject]@{ Key = "game/GameAssembly.dll"; Path = (Join-Path $environment.GameRoot "GameAssembly.dll") })
    $targets.Add([pscustomobject]@{ Key = "game/global-metadata.dat"; Path = (Join-Path $environment.GameRoot "Fall of Avalon_Data\il2cpp_data\Metadata\global-metadata.dat") })
    $targets.Add([pscustomobject]@{ Key = "loader/BepInEx.Core.dll"; Path = (Join-Path $environment.BepInExCore "BepInEx.Core.dll") })
    $targets.Add([pscustomobject]@{ Key = "loader/BepInEx.Unity.IL2CPP.dll"; Path = (Join-Path $environment.BepInExCore "BepInEx.Unity.IL2CPP.dll") })
    $targets.Add([pscustomobject]@{ Key = "loader/Il2CppInterop.Runtime.dll"; Path = (Join-Path $environment.BepInExCore "Il2CppInterop.Runtime.dll") })
    $targets.Add([pscustomobject]@{ Key = "interop/TG.Main.dll"; Path = (Join-Path $environment.InteropDir "TG.Main.dll") })
}
else {
    throw "Cannot fingerprint an unknown runtime lane."
}

$rows = @(
    foreach ($target in $targets) {
        $path = [string]$target.Path

        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            [pscustomobject]@{
                Key = [string]$target.Key
                Path = $path
                Exists = $false
                Length = $null
                SHA256 = $null
                FileVersion = $null
            }
            continue
        }

        $item = Get-Item -LiteralPath $path
        $hash = Get-FileHash -LiteralPath $path -Algorithm SHA256

        [pscustomobject]@{
            Key = [string]$target.Key
            Path = $item.FullName
            Exists = $true
            Length = $item.Length
            SHA256 = $hash.Hash
            FileVersion = $item.VersionInfo.FileVersion
        }
    }
)

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    [pscustomobject]@{
        Format = "foa-runtime-fingerprint/1"
        Generated = [DateTimeOffset]::Now.ToString("o")
        Runtime = $environment.Runtime
        Loader = $environment.Loader
        Files = $rows
    } | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

if (-not $Quiet) {
    $rows | Format-Table Key, Exists, Length, SHA256 -AutoSize | Out-Host

    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
        Write-Host ""
        Write-Host "Fingerprint baseline written to: $OutputPath"
    }
}

return $rows
