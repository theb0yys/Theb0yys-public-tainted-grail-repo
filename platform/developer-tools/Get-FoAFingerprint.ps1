[CmdletBinding()]
param(
    [string]$GameRoot,
    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet

$paths = New-Object System.Collections.Generic.List[string]

if ($environment.Runtime -eq "Mono") {
    $paths.Add((Join-Path $environment.ManagedDir "TG.Main.dll"))
    $paths.Add((Join-Path $environment.BepInExCore "BepInEx.dll"))
    $paths.Add((Join-Path $environment.BepInExCore "0Harmony.dll"))
}
elseif ($environment.Runtime -eq "IL2CPP") {
    $paths.Add((Join-Path $environment.GameRoot "GameAssembly.dll"))
    $paths.Add((Join-Path $environment.GameRoot "Fall of Avalon_Data\il2cpp_data\Metadata\global-metadata.dat"))
    $paths.Add((Join-Path $environment.BepInExCore "BepInEx.Core.dll"))
    $paths.Add((Join-Path $environment.BepInExCore "BepInEx.Unity.IL2CPP.dll"))
    $paths.Add((Join-Path $environment.BepInExCore "Il2CppInterop.Runtime.dll"))
    $paths.Add((Join-Path $environment.InteropDir "TG.Main.dll"))
}
else {
    throw "Cannot fingerprint an unknown runtime lane."
}

$rows = foreach ($path in $paths) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        [pscustomobject]@{
            Path = $path
            Exists = $false
            Length = $null
            SHA256 = $null
        }
        continue
    }

    $item = Get-Item -LiteralPath $path
    $hash = Get-FileHash -LiteralPath $path -Algorithm SHA256

    [pscustomobject]@{
        Path = $item.FullName
        Exists = $true
        Length = $item.Length
        SHA256 = $hash.Hash
    }
}

if (-not $Quiet) {
    $rows | Format-Table -AutoSize | Out-Host
}

return $rows
