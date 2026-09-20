[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [switch]$Quiet,

    [switch]$FailOnError
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-Check {
    param(
        [System.Collections.Generic.List[object]]$Checks,
        [string]$State,
        [string]$Rule,
        [string]$Detail
    )

    $Checks.Add([pscustomobject]@{
        State = $State
        Rule = $Rule
        Detail = $Detail
    })
}

$projectPath = (Resolve-Path -LiteralPath $Project).Path
$projectDir = Split-Path -Parent $projectPath
$projectText = Get-Content -LiteralPath $projectPath -Raw

try {
    [xml]$projectXml = $projectText
}
catch {
    throw "Project file is not valid XML: $projectPath"
}

$checks = New-Object System.Collections.Generic.List[object]

$propertyGroups = @($projectXml.Project.PropertyGroup)
$assemblyName = [string]($propertyGroups | ForEach-Object { $_.AssemblyName } | Where-Object { $_ } | Select-Object -First 1)
$rootNamespace = [string]($propertyGroups | ForEach-Object { $_.RootNamespace } | Where-Object { $_ } | Select-Object -First 1)
$targetFramework = [string]($propertyGroups | ForEach-Object { $_.TargetFramework } | Where-Object { $_ } | Select-Object -First 1)

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    Add-Check $checks "WARNING" "assembly-name" "AssemblyName is not explicit; project filename will be used."
}
else {
    Add-Check $checks "PASSED" "assembly-name" "AssemblyName=$assemblyName"
}

if ([string]::IsNullOrWhiteSpace($rootNamespace)) {
    Add-Check $checks "WARNING" "root-namespace" "RootNamespace is not explicit."
}
else {
    Add-Check $checks "PASSED" "root-namespace" "RootNamespace=$rootNamespace"
}

if ([string]::IsNullOrWhiteSpace($targetFramework)) {
    Add-Check $checks "FAILED" "target-framework" "TargetFramework was not found."
}
else {
    Add-Check $checks "PASSED" "target-framework" "TargetFramework=$targetFramework"
}

$hasBep5Mono = $projectText -match '<Reference Include="BepInEx"'
$hasBep6Mono = $projectText -match 'BepInEx\.Unity\.Mono'
$hasIl2Cpp = $projectText -match 'BepInEx\.Unity\.IL2CPP'
$hasHarmony = $projectText -match '0Harmony|HarmonyLib'

$runtimeMarkers = @($hasBep5Mono, $hasBep6Mono, $hasIl2Cpp) | Where-Object { $_ }
if ($runtimeMarkers.Count -gt 1) {
    Add-Check $checks "FAILED" "runtime-lane" "Project mixes multiple BepInEx runtime host lanes."
}
elseif ($hasIl2Cpp) {
    Add-Check $checks "PASSED" "runtime-lane" "IL2CPP / BepInEx 6"
}
elseif ($hasBep6Mono) {
    Add-Check $checks "PASSED" "runtime-lane" "Mono / BepInEx 6"
}
elseif ($hasBep5Mono) {
    Add-Check $checks "PASSED" "runtime-lane" "Mono / BepInEx 5"
}
else {
    Add-Check $checks "WARNING" "runtime-lane" "No recognized BepInEx runtime host reference was found."
}

$sourceFiles = @(
    Get-ChildItem -LiteralPath $projectDir -Filter "*.cs" -File -Recurse |
        Where-Object {
            $_.FullName -notmatch '[\\/](bin|obj)[\\/]'
        }
)

$sourceText = ($sourceFiles | ForEach-Object {
    Get-Content -LiteralPath $_.FullName -Raw
}) -join [Environment]::NewLine

$usesHarmonyAttributes = $sourceText -match '\[HarmonyPatch|Harmony\('
if ($usesHarmonyAttributes -and -not $hasHarmony) {
    Add-Check $checks "FAILED" "harmony-reference" "Harmony source usage was found but the project does not reference Harmony/0Harmony."
}
elseif ($hasHarmony) {
    Add-Check $checks "PASSED" "harmony-reference" "Harmony reference is present."
}
else {
    Add-Check $checks "NOT_APPLICABLE" "harmony-reference" "No Harmony usage detected."
}

$pluginGuids = New-Object System.Collections.Generic.List[string]
$pluginNames = New-Object System.Collections.Generic.List[string]
$pluginVersions = New-Object System.Collections.Generic.List[string]

foreach ($match in [regex]::Matches($sourceText, 'public\s+const\s+string\s+PluginGuid\s*=\s*"([^"]+)"')) {
    $pluginGuids.Add($match.Groups[1].Value)
}
foreach ($match in [regex]::Matches($sourceText, 'public\s+const\s+string\s+PluginName\s*=\s*"([^"]+)"')) {
    $pluginNames.Add($match.Groups[1].Value)
}
foreach ($match in [regex]::Matches($sourceText, 'public\s+const\s+string\s+PluginVersion\s*=\s*"([^"]+)"')) {
    $pluginVersions.Add($match.Groups[1].Value)
}
foreach ($match in [regex]::Matches($sourceText, '\[BepInPlugin\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"')) {
    $pluginGuids.Add($match.Groups[1].Value)
    $pluginNames.Add($match.Groups[2].Value)
    $pluginVersions.Add($match.Groups[3].Value)
}

$uniqueGuids = @($pluginGuids | Sort-Object -Unique)
$uniqueNames = @($pluginNames | Sort-Object -Unique)
$uniqueVersions = @($pluginVersions | Sort-Object -Unique)

if ($uniqueGuids.Count -eq 0) {
    Add-Check $checks "WARNING" "plugin-guid" "No literal PluginGuid/BepInPlugin GUID could be extracted."
}
elseif ($uniqueGuids.Count -gt 1) {
    Add-Check $checks "FAILED" "plugin-guid" ("Multiple plugin GUIDs found: " + ($uniqueGuids -join ", "))
}
else {
    $guid = $uniqueGuids[0]
    if ($guid -match 'yourname|community\.taintedgrail\.(template|harmonybasic)|foa-mod-template') {
        Add-Check $checks "FAILED" "plugin-guid" "Template/placeholder plugin GUID is still present: $guid"
    }
    elseif ($guid -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{2,}$') {
        Add-Check $checks "WARNING" "plugin-guid" "Plugin GUID has an unusual format: $guid"
    }
    else {
        Add-Check $checks "PASSED" "plugin-guid" "PluginGuid=$guid"
    }
}

if ($uniqueNames.Count -eq 1) {
    Add-Check $checks "PASSED" "plugin-name" "PluginName=$($uniqueNames[0])"
}
elseif ($uniqueNames.Count -gt 1) {
    Add-Check $checks "WARNING" "plugin-name" ("Multiple plugin names found: " + ($uniqueNames -join ", "))
}
else {
    Add-Check $checks "WARNING" "plugin-name" "No literal plugin name could be extracted."
}

if ($uniqueVersions.Count -eq 1) {
    if ($uniqueVersions[0] -match '^\d+\.\d+\.\d+([-.][0-9A-Za-z.-]+)?$') {
        Add-Check $checks "PASSED" "plugin-version" "PluginVersion=$($uniqueVersions[0])"
    }
    else {
        Add-Check $checks "WARNING" "plugin-version" "Plugin version is not SemVer-like: $($uniqueVersions[0])"
    }
}
elseif ($uniqueVersions.Count -gt 1) {
    Add-Check $checks "WARNING" "plugin-version" ("Multiple plugin versions found: " + ($uniqueVersions -join ", "))
}
else {
    Add-Check $checks "WARNING" "plugin-version" "No literal plugin version could be extracted."
}

$placeholderPatterns = @(
    'TGCommunity\.HarmonyBasic',
    'TGTemplate\.Il2CppHarmonyBasic',
    'TGTemplate\.',
    'FoAModTemplate'
)

$placeholderHits = @()
foreach ($pattern in $placeholderPatterns) {
    if ($projectText -match $pattern -or $sourceText -match $pattern) {
        $placeholderHits += $pattern
    }
}

if ($placeholderHits.Count -gt 0) {
    Add-Check $checks "FAILED" "template-placeholders" ("Starter identifiers remain: " + (($placeholderHits | Sort-Object -Unique) -join ", "))
}
else {
    Add-Check $checks "PASSED" "template-placeholders" "No known starter identity remains."
}

if ($hasIl2Cpp) {
    if ($sourceText -match '\bBaseUnityPlugin\b') {
        Add-Check $checks "FAILED" "host-lifecycle" "IL2CPP project contains BaseUnityPlugin. BepInEx 6 IL2CPP hosts should use BasePlugin."
    }
    elseif ($sourceText -match '\bBasePlugin\b' -and $sourceText -match 'override\s+void\s+Load\s*\(') {
        Add-Check $checks "PASSED" "host-lifecycle" "IL2CPP BasePlugin/Load lifecycle found."
    }
    else {
        Add-Check $checks "WARNING" "host-lifecycle" "Could not confirm IL2CPP BasePlugin/Load lifecycle."
    }
}
elseif ($hasBep5Mono -or $hasBep6Mono) {
    if ($sourceText -match '\bBaseUnityPlugin\b') {
        Add-Check $checks "PASSED" "host-lifecycle" "Mono BaseUnityPlugin lifecycle found."
    }
    else {
        Add-Check $checks "WARNING" "host-lifecycle" "Could not confirm Mono BaseUnityPlugin lifecycle."
    }
}

$references = @($projectXml.Project.ItemGroup.Reference)
foreach ($reference in $references) {
    if ($null -eq $reference) {
        continue
    }

    $include = [string]$reference.Include
    if ($include -notmatch '^(BepInEx|0Harmony|Harmony|UnityEngine|TG\.|Assembly-CSharp|Awaken\.|Il2Cpp|Il2CppInterop)') {
        continue
    }

    $privateText = [string]$reference.Private
    if ($privateText -eq "true") {
        Add-Check $checks "FAILED" "reference-copy-local" "$include has Private=true; game/loader/runtime references should not be copied into the mod output by default."
    }
    elseif ($privateText -eq "false") {
        Add-Check $checks "PASSED" "reference-copy-local" "$include has Private=false."
    }
    else {
        Add-Check $checks "WARNING" "reference-copy-local" "$include does not explicitly set Private=false."
    }
}

$suspiciousPathPatterns = @(
    '[A-Za-z]:\\Users\\[^\\\r\n]+',
    '[A-Za-z]:\\[^\r\n]*SteamLibrary\\'
)

foreach ($pattern in $suspiciousPathPatterns) {
    if ($projectText -match $pattern -or $sourceText -match $pattern) {
        Add-Check $checks "FAILED" "private-path" "A user-specific/private absolute path pattern was found in project/source text."
        break
    }
}

$forbiddenNames = @(
    "TG.Main.dll",
    "Assembly-CSharp.dll",
    "GameAssembly.dll",
    "global-metadata.dat",
    "BepInEx.dll",
    "BepInEx.Core.dll",
    "BepInEx.Unity.IL2CPP.dll",
    "BepInEx.Unity.Mono.dll",
    "0Harmony.dll",
    "Il2CppInterop.Runtime.dll",
    "Il2Cppmscorlib.dll"
)

$forbiddenFiles = @(
    Get-ChildItem -LiteralPath $projectDir -File -Recurse |
        Where-Object {
            $_.FullName -notmatch '[\\/](bin|obj|release|dist|packages)[\\/]' -and
            $forbiddenNames -contains $_.Name
        }
)

if ($forbiddenFiles.Count -gt 0) {
    Add-Check $checks "FAILED" "bundled-runtime-binary" ("Game/loader/runtime binaries found inside source tree: " + (($forbiddenFiles | ForEach-Object { $_.FullName }) -join "; "))
}
else {
    Add-Check $checks "PASSED" "bundled-runtime-binary" "No known game/loader/runtime binary found in the source tree."
}

if (-not [string]::IsNullOrWhiteSpace($GameRoot)) {
    $environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
    if (Test-Path -LiteralPath $environmentScript -PathType Leaf) {
        try {
            $environment = & $environmentScript -GameRoot $GameRoot -Quiet
            if (-not $environment.Ready) {
                Add-Check $checks "FAILED" "local-environment" ($environment.Issues -join " | ")
            }
            elseif ($hasIl2Cpp -and $environment.Runtime -ne "IL2CPP") {
                Add-Check $checks "FAILED" "local-environment" "Project is IL2CPP but selected installation is $($environment.Runtime)."
            }
            elseif (($hasBep5Mono -or $hasBep6Mono) -and $environment.Runtime -ne "Mono") {
                Add-Check $checks "FAILED" "local-environment" "Project is Mono but selected installation is $($environment.Runtime)."
            }
            else {
                Add-Check $checks "PASSED" "local-environment" "Selected FoA installation matches the project runtime lane."
            }

            if ($hasIl2Cpp -and ($projectText -match 'Il2Cppmscorlib|BepInEx\\interop|<Reference Include="TG\.Main"') -and -not $environment.InteropReady) {
                Add-Check $checks "FAILED" "il2cpp-interop" "Project references generated IL2CPP game interop but the selected installation has no generated interop TG.Main.dll."
            }
        }
        catch {
            Add-Check $checks "FAILED" "local-environment" $_.Exception.Message
        }
    }
}

$failed = @($checks | Where-Object { $_.State -eq "FAILED" })
$warnings = @($checks | Where-Object { $_.State -eq "WARNING" })

$result = [pscustomobject]@{
    Project = $projectPath
    AssemblyName = $assemblyName
    RootNamespace = $rootNamespace
    TargetFramework = $targetFramework
    Runtime = if ($hasIl2Cpp) { "IL2CPP" } elseif ($hasBep5Mono -or $hasBep6Mono) { "Mono" } else { "Unknown" }
    Ready = ($failed.Count -eq 0)
    FailedCount = $failed.Count
    WarningCount = $warnings.Count
    Checks = @($checks)
}

if (-not $Quiet) {
    $checks | Format-Table State, Rule, Detail -AutoSize | Out-Host
    Write-Host ""
    Write-Host ("Project doctor: {0} ({1} failed, {2} warnings)" -f $(if ($result.Ready) { "PASSED" } else { "FAILED" }), $result.FailedCount, $result.WarningCount)
}

if ($FailOnError -and -not $result.Ready) {
    throw "FoA project doctor failed with $($result.FailedCount) blocking finding(s)."
}

return $result
