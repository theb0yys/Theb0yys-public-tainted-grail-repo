[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$VerificationReport,

    [string]$Owner,

    [string]$Project,

    [ValidateSet("mono", "il2cpp", "hybrid", "unspecified")]
    [string]$Runtime,

    [string]$OutputPath,

    [switch]$AsConfigBlock
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$reportPath = (Resolve-Path -LiteralPath $VerificationReport).Path
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json

if ([string]$report.Format -ne "foa-symbol-anchor-verification/1") {
    throw "Unsupported verification report format: $($report.Format)"
}

$rows = @(
    $report.Results |
        Where-Object {
            -not [string]::IsNullOrWhiteSpace([string]$_.CanonicalTarget) -and
            [string]$_.State -eq "exact-signature-present"
        }
)

if (-not [string]::IsNullOrWhiteSpace($Owner)) {
    $rows = @($rows | Where-Object { [string]$_.Owner -eq $Owner })
}

if (-not [string]::IsNullOrWhiteSpace($Project)) {
    $rows = @($rows | Where-Object { [string]$_.Project -eq $Project })
}

if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
    $rows = @($rows | Where-Object { [string]$_.Runtime -eq $Runtime -or [string]$_.Runtime -eq "hybrid" })
}

$targets = @(
    $rows |
        ForEach-Object { [string]$_.CanonicalTarget } |
        Sort-Object -Unique
)

$pipeValue = $targets -join "|"

if ($AsConfigBlock) {
    $text = @(
        "[Filter]",
        "OwnerId = $Owner",
        "ExpectedTargets = $pipeValue"
    ) -join [Environment]::NewLine
}
else {
    $text = $pipeValue
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    Set-Content -LiteralPath $OutputPath -Value $text -Encoding UTF8
}

if ($AsConfigBlock) {
    Write-Host $text
}
else {
    Write-Host $pipeValue
}

return [pscustomobject]@{
    Format = "foa-harmony-audit-expected-targets/1"
    SourceReport = [System.IO.Path]::GetFileName($reportPath)
    Owner = $Owner
    Project = $Project
    Runtime = $Runtime
    TargetCount = $targets.Count
    Targets = $targets
    ExpectedTargetsValue = $pipeValue
}
