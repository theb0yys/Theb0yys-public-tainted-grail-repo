[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Baseline,

    [string]$GameRoot,

    [string]$OutputPath,

    [switch]$Quiet,

    [switch]$FailOnChange
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$baselinePath = (Resolve-Path -LiteralPath $Baseline).Path
$baselineJson = Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json

if ($null -eq $baselineJson.Files) {
    throw "Baseline does not contain a Files collection."
}

$currentScript = Join-Path $PSScriptRoot "Get-FoAFingerprint.ps1"
$currentRows = @(& $currentScript -GameRoot $GameRoot -Quiet)

$currentByKey = @{}
foreach ($row in $currentRows) {
    $key = [string]$row.Key
    if ([string]::IsNullOrWhiteSpace($key)) {
        $key = [System.IO.Path]::GetFileName([string]$row.Path)
    }
    $currentByKey[$key] = $row
}

$comparisons = @(
    foreach ($baselineRow in @($baselineJson.Files)) {
        $key = [string]$baselineRow.Key
        if ([string]::IsNullOrWhiteSpace($key)) {
            $key = [System.IO.Path]::GetFileName([string]$baselineRow.Path)
        }

        if (-not $currentByKey.ContainsKey($key)) {
            [pscustomobject]@{
                Key = $key
                State = "MISSING_CURRENT"
                BaselineSHA256 = [string]$baselineRow.SHA256
                CurrentSHA256 = $null
                BaselineLength = $baselineRow.Length
                CurrentLength = $null
            }
            continue
        }

        $current = $currentByKey[$key]

        if (-not [bool]$baselineRow.Exists -and -not [bool]$current.Exists) {
            $state = "UNCHANGED_MISSING"
        }
        elseif (-not [bool]$baselineRow.Exists -and [bool]$current.Exists) {
            $state = "NOW_PRESENT"
        }
        elseif ([bool]$baselineRow.Exists -and -not [bool]$current.Exists) {
            $state = "NOW_MISSING"
        }
        elseif ([string]$baselineRow.SHA256 -eq [string]$current.SHA256) {
            $state = "SAME"
        }
        else {
            $state = "CHANGED"
        }

        [pscustomobject]@{
            Key = $key
            State = $state
            BaselineSHA256 = [string]$baselineRow.SHA256
            CurrentSHA256 = [string]$current.SHA256
            BaselineLength = $baselineRow.Length
            CurrentLength = $current.Length
        }
    }
)

$baselineKeys = @($comparisons.Key)
foreach ($current in $currentRows) {
    $key = [string]$current.Key
    if ([string]::IsNullOrWhiteSpace($key)) {
        $key = [System.IO.Path]::GetFileName([string]$current.Path)
    }

    if ($baselineKeys -contains $key) {
        continue
    }

    $comparisons += [pscustomobject]@{
        Key = $key
        State = "NEW_CURRENT_KEY"
        BaselineSHA256 = $null
        CurrentSHA256 = [string]$current.SHA256
        BaselineLength = $null
        CurrentLength = $current.Length
    }
}

$changed = @(
    $comparisons |
        Where-Object {
            $_.State -notin @("SAME", "UNCHANGED_MISSING")
        }
)

$result = [pscustomobject]@{
    Format = "foa-runtime-fingerprint-comparison/1"
    Baseline = $baselinePath
    BaselineRuntime = [string]$baselineJson.Runtime
    BaselineLoader = [string]$baselineJson.Loader
    ChangeCount = $changed.Count
    RevalidationRequired = ($changed.Count -gt 0)
    Comparisons = $comparisons
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $portableResult = [pscustomobject]@{
        Format = $result.Format
        Baseline = [System.IO.Path]::GetFileName($baselinePath)
        BaselineRuntime = $result.BaselineRuntime
        BaselineLoader = $result.BaselineLoader
        ChangeCount = $result.ChangeCount
        RevalidationRequired = $result.RevalidationRequired
        Comparisons = $result.Comparisons
    }

    $portableResult | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

if (-not $Quiet) {
    $comparisons | Format-Table Key, State, BaselineLength, CurrentLength -AutoSize | Out-Host
    Write-Host ""

    if ($result.RevalidationRequired) {
        Write-Host "Compatibility fingerprint: CHANGED — revalidation required."
        Write-Host "A changed fingerprint does not by itself prove the mod is broken."
    }
    else {
        Write-Host "Compatibility fingerprint: SAME for all baseline keys."
    }
}

if ($FailOnChange -and $result.RevalidationRequired) {
    throw "FoA compatibility fingerprint changed in $($result.ChangeCount) baseline/current item(s). Revalidation is required."
}

return $result
