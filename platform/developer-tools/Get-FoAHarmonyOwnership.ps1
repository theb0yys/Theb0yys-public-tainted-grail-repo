[CmdletBinding()]
param(
    [string]$Root = ".",
    [string]$OutputPath,
    [switch]$Quiet,
    [switch]$FailOnConflict,
    [switch]$FailOnUnresolved
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-AnchorKey {
    param($Anchor)

    $parameters = @($Anchor.ParameterTypeExpressions)
    $signature = if ([bool]$Anchor.SignatureExplicit) {
        "(" + ($parameters -join ",") + ")"
    }
    else {
        "(*)"
    }

    return (
        [string]$Anchor.Runtime + "|" +
        [string]$Anchor.TypeExpression + "::" +
        [string]$Anchor.MemberName + $signature
    )
}

$rootPath = (Resolve-Path -LiteralPath $Root).Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$exporter = Join-Path $repoRoot "research\tools\symbol-anchors\Export-FoASymbolAnchors.ps1"

if (-not (Test-Path -LiteralPath $exporter -PathType Leaf)) {
    throw "Canonical symbol-anchor extractor not found: $exporter"
}

$tempManifest = Join-Path ([System.IO.Path]::GetTempPath()) ("foa-harmony-ownership-" + [Guid]::NewGuid().ToString("N") + ".json")

try {
    $manifest = & $exporter -Root $rootPath -OutputPath $tempManifest
    $anchors = @($manifest.Anchors)
    $unresolved = @($manifest.Unresolved)

    $annotated = @(
        foreach ($anchor in $anchors) {
            [pscustomobject]@{
                Owner = [string]$anchor.Owner
                Project = [string]$anchor.Project
                Runtime = [string]$anchor.Runtime
                Target = Get-AnchorKey -Anchor $anchor
                TypeExpression = [string]$anchor.TypeExpression
                MemberName = [string]$anchor.MemberName
                MemberKind = [string]$anchor.MemberKind
                SignatureExplicit = [bool]$anchor.SignatureExplicit
                ParameterTypeExpressions = @($anchor.ParameterTypeExpressions)
                Pattern = [string]$anchor.Pattern
                Source = [string]$anchor.Source
            }
        }
    )

    $conflicts = @(
        $annotated |
            Group-Object Target |
            Where-Object {
                @($_.Group.Owner | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique).Count -gt 1
            } |
            ForEach-Object {
                [pscustomobject]@{
                    Target = $_.Name
                    Owners = @($_.Group.Owner | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
                    Projects = @($_.Group.Project | Sort-Object -Unique)
                    Sources = @($_.Group.Source | Sort-Object -Unique)
                }
            }
    )

    $result = [pscustomobject]@{
        Format = "foa-harmony-source-ownership/2"
        Root = $rootPath
        DeclaredTargetCount = $annotated.Count
        ConflictCount = $conflicts.Count
        UnresolvedCount = $unresolved.Count
        Targets = $annotated
        Conflicts = $conflicts
        Unresolved = $unresolved
        LiveAuditTool = "research/tools/harmony-runtime-audit"
        Limitation = "This report describes source-declared targets. Live Harmony ownership is inspected separately in-process."
    }

    if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
        $parent = Split-Path -Parent $OutputPath
        if (-not [string]::IsNullOrWhiteSpace($parent)) {
            New-Item -ItemType Directory -Path $parent -Force | Out-Null
        }

        [pscustomobject]@{
            Format = $result.Format
            Root = "<SOURCE_ROOT>"
            DeclaredTargetCount = $result.DeclaredTargetCount
            ConflictCount = $result.ConflictCount
            UnresolvedCount = $result.UnresolvedCount
            Targets = $result.Targets
            Conflicts = $result.Conflicts
            Unresolved = $result.Unresolved
            LiveAuditTool = $result.LiveAuditTool
            Limitation = $result.Limitation
        } | ConvertTo-Json -Depth 14 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
    }

    if (-not $Quiet) {
        if ($annotated.Count -gt 0) {
            $annotated | Format-Table Owner, Runtime, Target, Project, Source -Wrap -AutoSize | Out-Host
        }
        else {
            Write-Host "No supported literal Harmony targets were extracted."
        }

        Write-Host ""
        Write-Host ("Declared targets: {0}" -f $annotated.Count)
        Write-Host ("Cross-owner overlaps: {0}" -f $conflicts.Count)
        Write-Host ("Unresolved Harmony-like sources: {0}" -f $unresolved.Count)
        Write-Host "Use research/tools/harmony-runtime-audit for live in-process ownership."
    }

    if ($FailOnConflict -and $conflicts.Count -gt 0) {
        throw "Harmony source ownership found $($conflicts.Count) cross-owner target overlap(s)."
    }

    if ($FailOnUnresolved -and $unresolved.Count -gt 0) {
        throw "Harmony source ownership found $($unresolved.Count) unresolved Harmony-like source file(s)."
    }

    return $result
}
finally {
    Remove-Item -LiteralPath $tempManifest -Force -ErrorAction SilentlyContinue
}
