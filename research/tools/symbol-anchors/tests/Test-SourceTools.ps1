[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$IlSpyCmd
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $PSScriptRoot
$exporter = Join-Path $toolRoot "Export-FoASymbolAnchors.ps1"
$verifier = Join-Path $toolRoot "Test-FoASymbolAnchors.ps1"

foreach ($scriptPath in @($exporter, $verifier)) {
    $text = Get-Content -LiteralPath $scriptPath -Raw
    $null = [scriptblock]::Create($text)
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("foa-symbol-anchor-test-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

try {
    $manifestPath = Join-Path $tempRoot "anchors.json"
    $reportPath = Join-Path $tempRoot "report.json"
    $sourceRoot = Join-Path $PSScriptRoot "SourceFixture"
    $assemblyRoot = Join-Path $PSScriptRoot "FixtureAssembly\bin\Release\net8.0"
    $fixtureAssembly = Join-Path $assemblyRoot "FixtureAssembly.dll"

    Write-Host "Direct ILSpy class listing:"
    & $IlSpyCmd -l c $fixtureAssembly | ForEach-Object { Write-Host ("  " + $_) }

    $manifest = & $exporter -Root $sourceRoot -OutputPath $manifestPath

    if ([string]$manifest.Format -ne "foa-symbol-anchors/1") {
        throw "Unexpected anchor manifest format."
    }

    if ($manifest.UnresolvedCount -ne 0) {
        throw "Synthetic fixture unexpectedly produced unresolved Harmony source."
    }

    $anchors = @($manifest.Anchors)
    if ($anchors.Count -ne 4) {
        throw "Expected 4 synthetic anchors, got $($anchors.Count)."
    }

    $beta = @($anchors | Where-Object { $_.MemberName -eq "Beta" })
    if ($beta.Count -ne 1 -or -not $beta[0].SignatureExplicit) {
        throw "Expected one exact Beta signature anchor."
    }

    if (@($beta[0].ParameterTypeExpressions) -join "," -ne "int,string") {
        throw "Unexpected Beta parameter extraction."
    }

    try {
        $report = & $verifier -Manifest $manifestPath -AssemblyRoot $assemblyRoot -IlSpyCmd $IlSpyCmd -OutputPath $reportPath -FailOnMissing
    }
    catch {
        $_ | Format-List * -Force | Out-Host
        throw
    }

    if ([string]$report.Format -ne "foa-symbol-anchor-verification/1") {
        throw "Unexpected anchor verification format."
    }

    if ($report.MissingOrAmbiguousCount -ne 0) {
        throw "Synthetic anchor verification reported missing or ambiguous targets."
    }

    $exactBeta = @(
        $report.Results |
            Where-Object {
                $_.MemberName -eq "Beta" -and
                $_.State -eq "exact-signature-present"
            }
    )

    if ($exactBeta.Count -ne 1) {
        throw "Expected the synthetic Beta signature to be verified exactly."
    }

    Write-Host "Symbol anchor source-tool test completed successfully."
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}
