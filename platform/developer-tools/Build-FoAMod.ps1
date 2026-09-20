[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$NoRestore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = (Resolve-Path -LiteralPath $Project).Path
$projectText = Get-Content -LiteralPath $projectPath -Raw

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet

$expectsIl2Cpp = $projectText -match "BepInEx\.Unity\.IL2CPP"
$expectsBep5Mono = $projectText -match "BepInEx\.dll"
$expectsBep6Mono = $projectText -match "BepInEx\.Unity\.Mono"
$expectsInterop = ($projectText -match "BepInEx\\interop") -or
                  ($projectText -match "Il2Cppmscorlib") -or
                  ($projectText -match "<Reference Include=""TG\.Main""")

if ($expectsIl2Cpp -and $environment.Runtime -ne "IL2CPP") {
    throw "Project expects IL2CPP, but the selected game installation was detected as '$($environment.Runtime)'."
}

if (($expectsBep5Mono -or $expectsBep6Mono) -and $environment.Runtime -ne "Mono") {
    throw "Project expects Mono, but the selected game installation was detected as '$($environment.Runtime)'."
}

if (-not $environment.Ready) {
    throw ("FoA environment is not ready: " + ($environment.Issues -join " | "))
}

if ($expectsBep5Mono -and -not $environment.BepInEx5Mono) {
    throw "Project references BepInEx 5 Mono, but BepInEx.dll was not found in the selected game installation."
}

if ($expectsBep6Mono -and -not $environment.BepInEx6Mono) {
    throw "Project references BepInEx 6 Mono, but BepInEx.Unity.Mono.dll was not found."
}

if ($expectsIl2Cpp -and -not $environment.BepInEx6IL2CPP) {
    throw "Project references BepInEx 6 IL2CPP, but BepInEx.Unity.IL2CPP.dll was not found."
}

if ($expectsIl2Cpp -and $expectsInterop -and -not $environment.InteropReady) {
    throw "Project references generated IL2CPP game interop, but BepInEx\interop\TG.Main.dll was not found. Run the IL2CPP game once through BepInEx to generate interop first."
}

$arguments = @(
    "build",
    $projectPath,
    "-c", $Configuration,
    "-p:GameRoot=$($environment.GameRoot)"
)

if ($NoRestore) {
    $arguments += "--no-restore"
}

Write-Host "dotnet $($arguments -join ' ')"
& dotnet @arguments

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

[xml]$projectXml = Get-Content -LiteralPath $projectPath -Raw
$assemblyName = [string]$projectXml.Project.PropertyGroup.AssemblyName
$targetFramework = [string]$projectXml.Project.PropertyGroup.TargetFramework

if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    $assemblyName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
}

$artifact = Join-Path (Split-Path -Parent $projectPath) "bin\$Configuration\$targetFramework\$assemblyName.dll"

$result = [pscustomobject]@{
    Project = $projectPath
    Runtime = $environment.Runtime
    Configuration = $Configuration
    Artifact = $artifact
    ArtifactExists = (Test-Path -LiteralPath $artifact -PathType Leaf)
}

$result | Format-List | Out-Host
return $result
