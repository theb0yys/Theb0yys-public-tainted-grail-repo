[CmdletBinding()]
param(
    [string]$GameRoot,
    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FoAGameRoot {
    param([string]$RequestedRoot)

    $candidates = New-Object System.Collections.Generic.List[string]

    if (-not [string]::IsNullOrWhiteSpace($RequestedRoot)) {
        $candidates.Add($RequestedRoot)
    }

    if (-not [string]::IsNullOrWhiteSpace($env:FOA_GAME_ROOT)) {
        $candidates.Add($env:FOA_GAME_ROOT)
    }

    $programFilesX86 = [Environment]::GetFolderPath("ProgramFilesX86")
    if (-not [string]::IsNullOrWhiteSpace($programFilesX86)) {
        $candidates.Add((Join-Path $programFilesX86 "Steam\steamapps\common\Tainted Grail FoA"))
    }

    $programFiles = [Environment]::GetFolderPath("ProgramFiles")
    if (-not [string]::IsNullOrWhiteSpace($programFiles)) {
        $candidates.Add((Join-Path $programFiles "Steam\steamapps\common\Tainted Grail FoA"))
    }

    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate)) {
            continue
        }

        if (Test-Path -LiteralPath $candidate -PathType Container) {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw "Tainted Grail FoA game root was not found. Pass -GameRoot or set FOA_GAME_ROOT."
}

$root = Resolve-FoAGameRoot -RequestedRoot $GameRoot
$dataDir = Join-Path $root "Fall of Avalon_Data"
$managedDir = Join-Path $dataDir "Managed"
$bepInExDir = Join-Path $root "BepInEx"
$coreDir = Join-Path $bepInExDir "core"
$interopDir = Join-Path $bepInExDir "interop"

$monoGameAssembly = Join-Path $managedDir "TG.Main.dll"
$gameAssembly = Join-Path $root "GameAssembly.dll"
$globalMetadata = Join-Path $dataDir "il2cpp_data\Metadata\global-metadata.dat"

$hasMonoGame = Test-Path -LiteralPath $monoGameAssembly -PathType Leaf
$hasIl2CppGame = (Test-Path -LiteralPath $gameAssembly -PathType Leaf) -and
                  (Test-Path -LiteralPath $globalMetadata -PathType Leaf)

if ($hasIl2CppGame) {
    $runtime = "IL2CPP"
}
elseif ($hasMonoGame) {
    $runtime = "Mono"
}
else {
    $runtime = "Unknown"
}

$bep5 = Test-Path -LiteralPath (Join-Path $coreDir "BepInEx.dll") -PathType Leaf
$bep6Core = Test-Path -LiteralPath (Join-Path $coreDir "BepInEx.Core.dll") -PathType Leaf
$bep6Il2Cpp = Test-Path -LiteralPath (Join-Path $coreDir "BepInEx.Unity.IL2CPP.dll") -PathType Leaf
$bep6Mono = Test-Path -LiteralPath (Join-Path $coreDir "BepInEx.Unity.Mono.dll") -PathType Leaf

if ($bep6Il2Cpp) {
    $loader = "BepInEx 6 IL2CPP"
}
elseif ($bep6Mono) {
    $loader = "BepInEx 6 Mono"
}
elseif ($bep5) {
    $loader = "BepInEx 5 Mono"
}
elseif ($bep6Core) {
    $loader = "BepInEx 6 (runtime host not identified)"
}
else {
    $loader = "Not detected"
}

$interopTgMain = Join-Path $interopDir "TG.Main.dll"
$interopReady = Test-Path -LiteralPath $interopTgMain -PathType Leaf

$issues = New-Object System.Collections.Generic.List[string]

if ($runtime -eq "Unknown") {
    $issues.Add("Neither Mono TG.Main.dll nor the IL2CPP GameAssembly/global-metadata marker pair was found.")
}

if (-not (Test-Path -LiteralPath $bepInExDir -PathType Container)) {
    $issues.Add("BepInEx directory was not found.")
}

if ($runtime -eq "Mono" -and -not ($bep5 -or $bep6Mono)) {
    $issues.Add("Mono game markers were found, but a Mono BepInEx host was not detected.")
}

if ($runtime -eq "IL2CPP" -and -not $bep6Il2Cpp) {
    $issues.Add("IL2CPP game markers were found, but BepInEx.Unity.IL2CPP.dll was not detected.")
}

$warnings = New-Object System.Collections.Generic.List[string]

if ($runtime -eq "IL2CPP" -and -not $interopReady) {
    $warnings.Add("Generated IL2CPP interop TG.Main.dll was not found. Loader-only plug-ins can still build; game-referencing IL2CPP projects require generated interop.")
}

$result = [pscustomobject]@{
    GameRoot = $root
    Runtime = $runtime
    Loader = $loader
    BepInExCore = $coreDir
    ManagedDir = $managedDir
    InteropDir = $interopDir
    InteropReady = $interopReady
    BepInEx5Mono = $bep5
    BepInEx6Mono = $bep6Mono
    BepInEx6IL2CPP = $bep6Il2Cpp
    LogPath = Join-Path $bepInExDir "LogOutput.log"
    Ready = ($issues.Count -eq 0)
    Issues = @($issues)
    Warnings = @($warnings)
}

if (-not $Quiet) {
    $result | Format-List | Out-Host
}

return $result
