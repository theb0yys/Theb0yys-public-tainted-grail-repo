[CmdletBinding()]
param(
    [string]$GameRoot,
    [string]$Pattern,

    [ValidateRange(1, 10000)]
    [int]$Tail = 80
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet
$logPath = $environment.LogPath

if (-not (Test-Path -LiteralPath $logPath -PathType Leaf)) {
    throw "BepInEx log was not found: $logPath"
}

Write-Host "Watching $logPath"

if ([string]::IsNullOrWhiteSpace($Pattern)) {
    Get-Content -LiteralPath $logPath -Tail $Tail -Wait
}
else {
    Write-Host "Filter: $Pattern"
    Get-Content -LiteralPath $logPath -Tail $Tail -Wait |
        Where-Object { $_ -match $Pattern }
}
