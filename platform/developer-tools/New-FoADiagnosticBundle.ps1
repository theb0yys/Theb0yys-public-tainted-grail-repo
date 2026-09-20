[CmdletBinding()]
param(
    [string]$GameRoot,
    [string]$Project,
    [string]$OutputDirectory,
    [ValidateRange(50, 20000)]
    [int]$LogTail = 2000,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Protect-Text {
    param(
        [AllowEmptyString()]
        [string]$Text,
        [string]$ResolvedGameRoot
    )

    if ($null -eq $Text) {
        return $null
    }

    $value = $Text
    $replacements = @(
        @($ResolvedGameRoot, "<GAME_ROOT>"),
        @($env:USERPROFILE, "<USERPROFILE>"),
        @($env:HOME, "<HOME>"),
        @($env:LOCALAPPDATA, "<LOCALAPPDATA>"),
        @($env:APPDATA, "<APPDATA>"),
        @($env:TEMP, "<TEMP>"),
        @($env:TMP, "<TEMP>")
    )

    foreach ($pair in $replacements) {
        $from = [string]$pair[0]
        $to = [string]$pair[1]
        if (-not [string]::IsNullOrWhiteSpace($from)) {
            $value = $value.Replace($from, $to)
        }
    }

    $value = [regex]::Replace($value, '[A-Za-z]:\\Users\\[^\\\s]+', '<USERPROFILE>')
    $value = [regex]::Replace($value, 'gh[pousr]_[A-Za-z0-9_]{20,}', '<REDACTED_GITHUB_TOKEN>')
    $value = [regex]::Replace($value, 'github_pat_[A-Za-z0-9_]{20,}', '<REDACTED_GITHUB_TOKEN>')
    $value = [regex]::Replace($value, '(?i)(api[_-]?key|token|secret|password)\s*[:=]\s*[^\s;,]+', '$1=<REDACTED>')

    return $value
}

function Protect-Object {
    param(
        [object]$Value,
        [string]$ResolvedGameRoot
    )

    if ($null -eq $Value) {
        return $null
    }

    if ($Value -is [string]) {
        return Protect-Text -Text $Value -ResolvedGameRoot $ResolvedGameRoot
    }

    if ($Value -is [ValueType]) {
        return $Value
    }

    if ($Value -is [System.Collections.IDictionary]) {
        $copy = [ordered]@{}
        foreach ($key in $Value.Keys) {
            $copy[$key] = Protect-Object -Value $Value[$key] -ResolvedGameRoot $ResolvedGameRoot
        }
        return [pscustomobject]$copy
    }

    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        return @($Value | ForEach-Object { Protect-Object -Value $_ -ResolvedGameRoot $ResolvedGameRoot })
    }

    if ($Value -is [psobject]) {
        $copy = [ordered]@{}
        foreach ($property in $Value.PSObject.Properties) {
            $copy[$property.Name] = Protect-Object -Value $property.Value -ResolvedGameRoot $ResolvedGameRoot
        }
        return [pscustomobject]$copy
    }

    return $Value
}

$environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
$environment = & $environmentScript -GameRoot $GameRoot -Quiet
$resolvedGameRoot = $environment.GameRoot

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $OutputDirectory = Join-Path (Get-Location) ("foa-diagnostics-" + $stamp)
}

if (Test-Path -LiteralPath $OutputDirectory) {
    $existing = @(Get-ChildItem -LiteralPath $OutputDirectory -Force -ErrorAction SilentlyContinue)
    if ($existing.Count -gt 0 -and -not $Force) {
        throw "Diagnostic output directory is not empty: $OutputDirectory"
    }
}
else {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$outputPath = (Resolve-Path -LiteralPath $OutputDirectory).Path

$protectedEnvironment = Protect-Object -Value $environment -ResolvedGameRoot $resolvedGameRoot
$protectedEnvironment | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputPath "environment.json") -Encoding UTF8

$fingerprintScript = Join-Path $PSScriptRoot "Get-FoAFingerprint.ps1"
$fingerprints = & $fingerprintScript -GameRoot $resolvedGameRoot -Quiet
$protectedFingerprints = Protect-Object -Value $fingerprints -ResolvedGameRoot $resolvedGameRoot
ConvertTo-Json -InputObject @($protectedFingerprints) -Depth 8 | Set-Content -LiteralPath (Join-Path $outputPath "fingerprints.json") -Encoding UTF8

$inventoryScript = Join-Path $PSScriptRoot "Get-FoAModInventory.ps1"
$inventory = & $inventoryScript -GameRoot $resolvedGameRoot -Quiet
$protectedInventory = Protect-Object -Value $inventory -ResolvedGameRoot $resolvedGameRoot
$protectedInventory | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $outputPath "installed-mod-inventory.json") -Encoding UTF8
$plugins = @($inventory.Items)

$projectResult = $null
if (-not [string]::IsNullOrWhiteSpace($Project)) {
    $doctorScript = Join-Path $PSScriptRoot "Test-FoAModProject.ps1"
    $projectResult = & $doctorScript -Project $Project -GameRoot $resolvedGameRoot -Quiet
    $protectedProject = Protect-Object -Value $projectResult -ResolvedGameRoot $resolvedGameRoot
    $protectedProject | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $outputPath "project-doctor.json") -Encoding UTF8
}

$logPath = $environment.LogPath
$logOutputPath = Join-Path $outputPath "relevant-log.txt"

if (Test-Path -LiteralPath $logPath -PathType Leaf) {
    $lines = @(Get-Content -LiteralPath $logPath -Tail $LogTail)
    $protectedLines = @($lines | ForEach-Object { Protect-Text -Text $_ -ResolvedGameRoot $resolvedGameRoot })

    $interesting = @(
        $protectedLines | Where-Object {
            $_ -match '(?i)(error|warning|exception|failed|harmony|chainloader|loading \[|plugin|missing|could not load|typeload|reflection)'
        }
    )

    if ($interesting.Count -eq 0) {
        $interesting = $protectedLines
    }

    $interesting | Set-Content -LiteralPath $logOutputPath -Encoding UTF8
}
else {
    "BepInEx LogOutput.log was not present when this bundle was created." |
        Set-Content -LiteralPath $logOutputPath -Encoding UTF8
}

$summary = New-Object System.Collections.Generic.List[string]
$summary.Add("FoA diagnostic bundle")
$summary.Add("Generated: $([DateTimeOffset]::Now.ToString('o'))")
$summary.Add("Runtime: $($environment.Runtime)")
$summary.Add("Loader: $($environment.Loader)")
$summary.Add("Environment ready: $($environment.Ready)")
$summary.Add("Interop ready: $($environment.InteropReady)")
$summary.Add("Installed DLL count: $($plugins.Count)")
$summary.Add("Installed duplicate/conflict findings: $($inventory.ConflictCount)")

if ($null -ne $projectResult) {
    $summary.Add("Project doctor ready: $($projectResult.Ready)")
    $summary.Add("Project doctor failures: $($projectResult.FailedCount)")
    $summary.Add("Project doctor warnings: $($projectResult.WarningCount)")
}

$summary.Add("")
$summary.Add("This bundle intentionally excludes game assemblies, generated interop assemblies, saves, credentials, and full proprietary data.")
$summary.Add("Paths and common secret/token patterns are redacted on a best-effort basis. Review the files before publishing.")

$summary | Set-Content -LiteralPath (Join-Path $outputPath "diagnostic-report.txt") -Encoding UTF8

$manifestRows = @(
    Get-ChildItem -LiteralPath $outputPath -File |
        ForEach-Object {
            [pscustomobject]@{
                File = $_.Name
                SHA256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
                Length = $_.Length
            }
        }
)

[pscustomobject]@{
    Format = "foa-diagnostic-bundle/1"
    Generated = [DateTimeOffset]::Now.ToString("o")
    Files = $manifestRows
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $outputPath "manifest.json") -Encoding UTF8

$result = [pscustomobject]@{
    OutputDirectory = $outputPath
    Runtime = $environment.Runtime
    Loader = $environment.Loader
    ProjectIncluded = ($null -ne $projectResult)
    Files = @((Get-ChildItem -LiteralPath $outputPath -File).Name)
}

$result | Format-List | Out-Host
Write-Host ""
Write-Host "Review the generated files before attaching them to an issue or sharing them."

return $result
