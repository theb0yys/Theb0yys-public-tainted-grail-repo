[CmdletBinding()]
param(
    [string]$Root = ".",
    [string]$OutputPath,
    [switch]$Quiet,
    [switch]$FailOnConflict
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$rootPath = (Resolve-Path -LiteralPath $Root).Path
$projects = @(
    Get-ChildItem -LiteralPath $rootPath -Filter "*.csproj" -File -Recurse |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|dist)[\\/]' }
)

$records = New-Object System.Collections.Generic.List[object]

foreach ($project in $projects) {
    $projectDir = $project.Directory.FullName
    $sourceFiles = @(
        Get-ChildItem -LiteralPath $projectDir -Filter "*.cs" -File -Recurse |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|dist)[\\/]' }
    )

    if ($sourceFiles.Count -eq 0) {
        continue
    }

    $sourceText = ($sourceFiles | ForEach-Object {
        Get-Content -LiteralPath $_.FullName -Raw
    }) -join [Environment]::NewLine

    if ($sourceText -notmatch 'HarmonyPatch|AccessTools\.(Method|PropertyGetter|PropertySetter)|\.Patch\(') {
        continue
    }

    $ownerGuid = $null
    $guidMatch = [regex]::Match($sourceText, 'public\s+const\s+string\s+PluginGuid\s*=\s*"([^"]+)"')
    if ($guidMatch.Success) {
        $ownerGuid = $guidMatch.Groups[1].Value
    }
    else {
        $attributeMatch = [regex]::Match($sourceText, '\[BepInPlugin\(\s*"([^"]+)"')
        if ($attributeMatch.Success) {
            $ownerGuid = $attributeMatch.Groups[1].Value
        }
    }

    if ([string]::IsNullOrWhiteSpace($ownerGuid)) {
        $ownerGuid = [System.IO.Path]::GetFileNameWithoutExtension($project.Name)
    }

    foreach ($sourceFile in $sourceFiles) {
        $text = Get-Content -LiteralPath $sourceFile.FullName -Raw
        $relativeSource = $sourceFile.FullName.Substring($rootPath.Length).TrimStart([char]92, [char]47).Replace('\', '/')

        $patterns = @(
            @{
                Name = "HarmonyPatch-type-nameof"
                Regex = '\[HarmonyPatch\(\s*typeof\(([^\)]+)\)\s*,\s*nameof\(([^\)]+)\)\s*\)\]'
                Target = { param($m)
                    $typeName = $m.Groups[1].Value.Trim()
                    $methodExpr = $m.Groups[2].Value.Trim()
                    $methodName = if ($methodExpr.Contains(".")) { $methodExpr.Substring($methodExpr.LastIndexOf(".") + 1) } else { $methodExpr }
                    return "$typeName::$methodName"
                }
            },
            @{
                Name = "HarmonyPatch-type-string"
                Regex = '\[HarmonyPatch\(\s*typeof\(([^\)]+)\)\s*,\s*"([^"]+)"\s*\)\]'
                Target = { param($m) return "$($m.Groups[1].Value.Trim())::$($m.Groups[2].Value)" }
            },
            @{
                Name = "HarmonyPatch-string-string"
                Regex = '\[HarmonyPatch\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)\]'
                Target = { param($m) return "$($m.Groups[1].Value)::$($m.Groups[2].Value)" }
            },
            @{
                Name = "AccessTools.Method"
                Regex = 'AccessTools\.Method\(\s*typeof\(([^\)]+)\)\s*,\s*"([^"]+)"'
                Target = { param($m) return "$($m.Groups[1].Value.Trim())::$($m.Groups[2].Value)" }
            },
            @{
                Name = "AccessTools.PropertyGetter"
                Regex = 'AccessTools\.PropertyGetter\(\s*typeof\(([^\)]+)\)\s*,\s*"([^"]+)"'
                Target = { param($m) return "$($m.Groups[1].Value.Trim())::get_$($m.Groups[2].Value)" }
            },
            @{
                Name = "AccessTools.PropertySetter"
                Regex = 'AccessTools\.PropertySetter\(\s*typeof\(([^\)]+)\)\s*,\s*"([^"]+)"'
                Target = { param($m) return "$($m.Groups[1].Value.Trim())::set_$($m.Groups[2].Value)" }
            }
        )

        foreach ($pattern in $patterns) {
            foreach ($match in [regex]::Matches($text, $pattern.Regex)) {
                $target = & $pattern.Target $match
                $records.Add([pscustomobject]@{
                    Owner = $ownerGuid
                    Project = $project.Name
                    Target = $target
                    Pattern = $pattern.Name
                    Source = $relativeSource
                })
            }
        }
    }
}

$deduped = @(
    $records |
        Sort-Object Owner, Project, Target, Source, Pattern -Unique
)

$conflicts = @(
    $deduped |
        Group-Object Target |
        Where-Object {
            @($_.Group.Owner | Sort-Object -Unique).Count -gt 1
        } |
        ForEach-Object {
            [pscustomobject]@{
                Target = $_.Name
                Owners = @($_.Group.Owner | Sort-Object -Unique)
                Projects = @($_.Group.Project | Sort-Object -Unique)
                Sources = @($_.Group.Source | Sort-Object -Unique)
            }
        }
)

$unparsedHints = @(
    foreach ($project in $projects) {
        $projectDir = $project.Directory.FullName
        $hits = @(
            Get-ChildItem -LiteralPath $projectDir -Filter "*.cs" -File -Recurse |
                Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|dist)[\\/]' } |
                ForEach-Object {
                    $text = Get-Content -LiteralPath $_.FullName -Raw
                    if ($text -match 'HarmonyPatch|AccessTools\.|\.Patch\(') {
                        $_.FullName.Substring($rootPath.Length).TrimStart([char]92, [char]47).Replace('\', '/')
                    }
                }
        )

        if ($hits.Count -gt 0 -and @($deduped | Where-Object { $_.Project -eq $project.Name }).Count -eq 0) {
            [pscustomobject]@{
                Project = $project.Name
                Sources = $hits
                State = "UNPARSED_DYNAMIC_OR_UNSUPPORTED_PATTERN"
            }
        }
    }
)

$result = [pscustomobject]@{
    Format = "foa-harmony-source-ownership/1"
    Root = $rootPath
    ProjectCount = $projects.Count
    DeclaredTargetCount = $deduped.Count
    ConflictCount = $conflicts.Count
    UnparsedProjectCount = $unparsedHints.Count
    Targets = $deduped
    Conflicts = $conflicts
    Unparsed = $unparsedHints
    RuntimeOwnership = "NOT_RUN"
    Limitation = "Static source parsing does not enumerate Harmony's live in-process patch table. Dynamic targets and unsupported source shapes may be absent."
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

if (-not $Quiet) {
    if ($deduped.Count -gt 0) {
        $deduped | Format-Table Owner, Target, Project, Source -AutoSize | Out-Host
    }
    else {
        Write-Host "No supported literal Harmony target declarations were found."
    }

    Write-Host ""

    if ($conflicts.Count -gt 0) {
        Write-Host "Declared source-level target overlaps:"
        $conflicts | Format-Table Target, Owners, Projects -Wrap -AutoSize | Out-Host
    }
    else {
        Write-Host "No cross-owner source-level target overlaps detected."
    }

    if ($unparsedHints.Count -gt 0) {
        Write-Host ""
        Write-Host "Projects containing Harmony-like code that this static parser could not resolve:"
        $unparsedHints | Format-Table Project, State, Sources -Wrap -AutoSize | Out-Host
    }

    Write-Host ""
    Write-Host "Runtime Harmony ownership: NOT_RUN (requires in-process runtime inspection)."
}

if ($FailOnConflict -and $conflicts.Count -gt 0) {
    throw "Harmony source ownership scan found $($conflicts.Count) cross-owner target overlap(s)."
}

return $result
