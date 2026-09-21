[CmdletBinding()]
param(
    [string] $UnityExe = '<local-path>
    [string] $ProjectPath = '<local-path>
    [string] $GemSourceRoot = '<local-path>
    [string] $OutputRoot = '',
    [string] $UnityOutputRoot = '',
    [string] $ManifestPath = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $PSScriptRoot '..\src\bin\Release\netstandard2.1\assets'
}

if ([string]::IsNullOrWhiteSpace($UnityOutputRoot)) {
    $UnityOutputRoot = Join-Path $ProjectPath 'TaintedGemsBuildOutput'
}

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $PSScriptRoot '..\docs\generated-tainted-gems-bundle-manifest.json'
}

$builderSource = Join-Path $PSScriptRoot 'unity\Assets\Editor\TaintedGemsBundleBuilder.cs'

if (-not (Test-Path -LiteralPath $UnityExe)) { throw "Unity not found: $UnityExe" }
if (-not (Test-Path -LiteralPath $ProjectPath)) { throw "Unity project not found: $ProjectPath" }
if (-not (Test-Path -LiteralPath $builderSource)) { throw "Builder source not found: $builderSource" }
if (-not (Test-Path -LiteralPath $GemSourceRoot)) { throw "Gem source root not found: $GemSourceRoot" }

$ProjectPath = (Resolve-Path -LiteralPath $ProjectPath).Path
$tempRoot = [IO.Path]::GetTempPath().TrimEnd('\')
if ($ProjectPath.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing temp Unity project path: $ProjectPath"
}

$UnityOutputRoot = [IO.Path]::GetFullPath($UnityOutputRoot)
if (-not $UnityOutputRoot.StartsWith($ProjectPath, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing Unity output outside the known Unity project: $UnityOutputRoot"
}

$OutputRoot = [IO.Path]::GetFullPath($OutputRoot)
$ManifestPath = [IO.Path]::GetFullPath($ManifestPath)

$assetsEditor = Join-Path $ProjectPath 'Assets\Editor'
$logPath = Join-Path $ProjectPath 'Logs\tainted-gems-bundle-build.log'
$unityManifestPath = Join-Path $ProjectPath 'Logs\tainted-gems-bundle-manifest.json'

New-Item -ItemType Directory -Force -Path $assetsEditor, (Split-Path -Parent $logPath), $UnityOutputRoot, $OutputRoot, (Split-Path -Parent $ManifestPath) | Out-Null
Copy-Item -LiteralPath $builderSource -Destination (Join-Path $assetsEditor 'TaintedGemsBundleBuilder.cs') -Force

$unityArgs = @(
    '-batchmode',
    '-quit',
    '-projectPath', $ProjectPath,
    '-executeMethod', 'TaintedGemsBundleBuilder.Build',
    '-gemSourceRoot', $GemSourceRoot,
    '-bundleOutput', $UnityOutputRoot,
    '-bundleManifest', $unityManifestPath,
    '-logFile', $logPath
)

$processInfo = [Diagnostics.ProcessStartInfo]::new()
$processInfo.FileName = $UnityExe
$processInfo.UseShellExecute = $false
$processInfo.CreateNoWindow = $true
foreach ($arg in $unityArgs) {
    [void] $processInfo.ArgumentList.Add($arg)
}

$process = [Diagnostics.Process]::Start($processInfo)
$process.WaitForExit()
$unityExitCode = $process.ExitCode
if ($unityExitCode -ne 0) {
    if (Test-Path -LiteralPath $logPath) {
        Get-Content -LiteralPath $logPath -Tail 200
    }
    throw "Unity build failed with exit code $unityExitCode. Log: $logPath"
}

$unityBundle = Join-Path $UnityOutputRoot 'tainted_gems.bundle'
$finalBundle = Join-Path $OutputRoot 'tainted_gems.bundle'
Copy-Item -LiteralPath $unityBundle -Destination $finalBundle -Force

$manifest = Get-Content -LiteralPath $unityManifestPath -Raw | ConvertFrom-Json
$finalBundleInfo = Get-Item -LiteralPath $finalBundle
$manifest.bundlePath = $finalBundleInfo.FullName.Replace('\', '/')
$manifest.bundleSha256 = (Get-FileHash -LiteralPath $finalBundle -Algorithm SHA256).Hash
$manifest.bundleBytes = $finalBundleInfo.Length
$manifest | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $ManifestPath -Encoding utf8

Get-Item -LiteralPath $finalBundle, $ManifestPath | Select-Object FullName,Length,LastWriteTime
