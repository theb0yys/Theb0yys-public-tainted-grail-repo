[CmdletBinding()]
param(
    [string]$Root = ".",
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [switch]$FailOnUnresolved
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-RelativePath {
    param([string]$BasePath, [string]$FullPath)

    $base = [System.IO.Path]::GetFullPath($BasePath)
    if (-not $base.EndsWith([System.IO.Path]::DirectorySeparatorChar.ToString())) {
        $base += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = [Uri]$base
    $fileUri = [Uri]([System.IO.Path]::GetFullPath($FullPath))
    return [Uri]::UnescapeDataString($baseUri.MakeRelativeUri($fileUri).ToString()).Replace('\', '/')
}

function Get-RuntimeLane {
    param([string]$RelativePath)

    $value = "/" + $RelativePath.ToLowerInvariant().Replace('\', '/') + "/"
    if ($value.Contains("/il2cpp/")) { return "il2cpp" }
    if ($value.Contains("/mono/")) { return "mono" }
    if ($value.Contains("/hybrid/")) { return "hybrid" }
    return "unspecified"
}

function Get-ProjectFile {
    param([System.IO.FileInfo]$SourceFile)

    $directory = $SourceFile.Directory
    while ($null -ne $directory) {
        $projects = @(Get-ChildItem -LiteralPath $directory.FullName -Filter "*.csproj" -File -ErrorAction SilentlyContinue)
        if ($projects.Count -eq 1) {
            return $projects[0]
        }
        $directory = $directory.Parent
    }

    return $null
}

function Get-ProjectName {
    param([System.IO.FileInfo]$SourceFile)

    $projectFile = Get-ProjectFile -SourceFile $SourceFile
    if ($null -eq $projectFile) {
        return $null
    }

    return $projectFile.Name
}

function Get-Owner {
    param([System.IO.FileInfo]$SourceFile, [hashtable]$Cache)

    $projectFile = Get-ProjectFile -SourceFile $SourceFile
    if ($null -eq $projectFile) {
        return $null
    }

    $project = $projectFile.Name
    if ($Cache.ContainsKey($projectFile.FullName)) {
        return $Cache[$projectFile.FullName]
    }

    $text = (
        Get-ChildItem -LiteralPath $projectFile.Directory.FullName -Filter "*.cs" -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|dist)[\\/]' } |
            ForEach-Object { Get-Content -LiteralPath $_.FullName -Raw }
    ) -join [Environment]::NewLine

    $owner = $null
    $constant = [regex]::Match($text, 'public\s+const\s+string\s+PluginGuid\s*=\s*"([^"]+)"')
    if ($constant.Success) {
        $owner = $constant.Groups[1].Value
    }
    else {
        $attribute = [regex]::Match($text, '\[BepInPlugin\(\s*"([^"]+)"')
        if ($attribute.Success) {
            $owner = $attribute.Groups[1].Value
        }
    }

    $Cache[$projectFile.FullName] = $owner
    return $owner
}

function Get-TypeOfList {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @()
    }

    return @(
        [regex]::Matches($Text, 'typeof\(\s*([^)]+?)\s*\)') |
            ForEach-Object { $_.Groups[1].Value.Trim() }
    )
}

function New-Anchor {
    param(
        [string]$Owner,
        [string]$Project,
        [string]$Runtime,
        [string]$Source,
        [string]$Pattern,
        [string]$TypeExpression,
        [string]$MemberName,
        [string]$MemberKind,
        [string[]]$ParameterTypes,
        [bool]$SignatureExplicit
    )

    return [pscustomobject]@{
        Owner = $Owner
        Project = $Project
        Runtime = $Runtime
        Source = $Source
        Pattern = $Pattern
        TypeExpression = $TypeExpression
        MemberName = $MemberName
        MemberKind = $MemberKind
        ParameterTypeExpressions = @($ParameterTypes)
        SignatureExplicit = $SignatureExplicit
    }
}

$rootPath = (Resolve-Path -LiteralPath $Root).Path
$sourceFiles = @(
    Get-ChildItem -LiteralPath $rootPath -Filter "*.cs" -File -Recurse |
        Where-Object { $_.FullName -notmatch '[\\/](bin|obj|release|dist)[\\/]' }
)

$anchors = New-Object System.Collections.Generic.List[object]
$unresolved = New-Object System.Collections.Generic.List[object]
$ownerCache = @{}

foreach ($sourceFile in $sourceFiles) {
    $text = Get-Content -LiteralPath $sourceFile.FullName -Raw
    if ($text -notmatch 'HarmonyPatch|AccessTools\.(Method|PropertyGetter|PropertySetter)|\.Patch\(') {
        continue
    }

    $relative = Get-RelativePath -BasePath $rootPath -FullPath $sourceFile.FullName
    $runtime = Get-RuntimeLane -RelativePath $relative
    $project = Get-ProjectName -SourceFile $sourceFile
    $owner = Get-Owner -SourceFile $sourceFile -Cache $ownerCache
    $found = 0

    $patterns = @(
        @{
            Name = "HarmonyPatch-type-nameof-signature"
            Regex = '\[HarmonyPatch\(\s*typeof\(([^)]+)\)\s*,\s*nameof\(([^)]+)\)\s*,\s*new\s*\[\]\s*\{([^}]*)\}\s*\)\]'
            Kind = "method"
            Signature = $true
        },
        @{
            Name = "HarmonyPatch-type-string-signature"
            Regex = '\[HarmonyPatch\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"\s*,\s*new\s*\[\]\s*\{([^}]*)\}\s*\)\]'
            Kind = "method"
            Signature = $true
        },
        @{
            Name = "HarmonyPatch-type-nameof"
            Regex = '\[HarmonyPatch\(\s*typeof\(([^)]+)\)\s*,\s*nameof\(([^)]+)\)\s*\)\]'
            Kind = "method"
            Signature = $false
        },
        @{
            Name = "HarmonyPatch-type-string"
            Regex = '\[HarmonyPatch\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"\s*\)\]'
            Kind = "method"
            Signature = $false
        },
        @{
            Name = "HarmonyPatch-string-string"
            Regex = '\[HarmonyPatch\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)\]'
            Kind = "method"
            Signature = $false
        },
        @{
            Name = "AccessTools.Method-type-string-signature"
            Regex = 'AccessTools\.Method\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"\s*,\s*new\s*\[\]\s*\{([^}]*)\}'
            Kind = "method"
            Signature = $true
        },
        @{
            Name = "AccessTools.Method-type-string"
            Regex = 'AccessTools\.Method\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"'
            Kind = "method"
            Signature = $false
        },
        @{
            Name = "AccessTools.PropertyGetter"
            Regex = 'AccessTools\.PropertyGetter\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"'
            Kind = "property-getter"
            Signature = $false
        },
        @{
            Name = "AccessTools.PropertySetter"
            Regex = 'AccessTools\.PropertySetter\(\s*typeof\(([^)]+)\)\s*,\s*"([^"]+)"'
            Kind = "property-setter"
            Signature = $false
        }
    )

    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($text, $pattern.Regex)) {
            $typeExpression = $match.Groups[1].Value.Trim()
            $memberExpression = $match.Groups[2].Value.Trim()
            $memberName = $memberExpression
            if ($pattern.Name -like "*nameof*") {
                if ($memberExpression.Contains(".")) {
                    $memberName = $memberExpression.Substring($memberExpression.LastIndexOf(".") + 1)
                }
            }

            $parameters = @()
            if ($pattern.Signature) {
                $parameters = @(Get-TypeOfList -Text $match.Groups[3].Value)
            }

            $anchors.Add((New-Anchor -Owner $owner -Project $project -Runtime $runtime -Source $relative -Pattern $pattern.Name -TypeExpression $typeExpression -MemberName $memberName -MemberKind $pattern.Kind -ParameterTypes $parameters -SignatureExplicit ([bool]$pattern.Signature)))
            $found++
        }
    }

    if ($found -eq 0) {
        $unresolved.Add([pscustomobject]@{
            Project = $project
            Runtime = $runtime
            Source = $relative
            Reason = "Harmony-like source present but no supported literal target was extracted."
        })
    }
}

$filteredAnchors = @(
    foreach ($anchor in $anchors) {
        if ($anchor.SignatureExplicit) {
            $anchor
            continue
        }

        $hasExactSibling = @(
            $anchors | Where-Object {
                $_.SignatureExplicit -and
                $_.Source -eq $anchor.Source -and
                $_.TypeExpression -eq $anchor.TypeExpression -and
                $_.MemberName -eq $anchor.MemberName -and
                $_.MemberKind -eq $anchor.MemberKind
            }
        ).Count -gt 0

        if (-not $hasExactSibling) {
            $anchor
        }
    }
)

$deduped = @(
    $filteredAnchors |
        Sort-Object Runtime, Project, Owner, TypeExpression, MemberName, MemberKind, SignatureExplicit, Source, Pattern -Unique
)

$result = [pscustomobject]@{
    Format = "foa-symbol-anchors/1"
    SourceRoot = "<SOURCE_ROOT>"
    AnchorCount = $deduped.Count
    UnresolvedCount = $unresolved.Count
    Anchors = $deduped
    Unresolved = @($unresolved | Sort-Object Runtime, Project, Source)
    Limitation = "Only supported literal Harmony and AccessTools source shapes are extracted. Dynamic target construction remains explicit under Unresolved."
}

$parent = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Host ("Symbol anchors: {0}" -f $deduped.Count)
Write-Host ("Unresolved Harmony-like source files: {0}" -f $unresolved.Count)
Write-Host ("Manifest: {0}" -f $OutputPath)

if ($FailOnUnresolved -and $unresolved.Count -gt 0) {
    throw "Symbol anchor extraction left $($unresolved.Count) Harmony-like source file(s) unresolved."
}

return $result
