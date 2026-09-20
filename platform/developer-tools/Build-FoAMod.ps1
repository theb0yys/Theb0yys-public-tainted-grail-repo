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

if ($expectsIl2Cpp -and $environment.Runtime -ne "IL2CPP") {
    throw "Project expects IL2CPP, but the selected game installation was detected as '$($environment.Runtime)'."
}

if (-not $expectsIl2Cpp -and $environment.Runtime -ne "Mono") {
    throw "Project expects the Mono template lane, but the selected game installation was detected as '$($environment.Runtime)'."
}

if (-not $environment.Ready) {
    throw ("FoA environment is not ready: " + ($environment.Issues -join " | "))
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
