param(
    [switch]$NoFailOnIndexGaps
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $repoRoot

$markdownFiles = @(git ls-files '*.md')
$failures = New-Object System.Collections.Generic.List[string]
$warnings = New-Object System.Collections.Generic.List[string]

function Resolve-LocalMarkdownTarget {
    param([string]$SourcePath, [string]$RawTarget)

    $target = $RawTarget.Trim()
    if ($target.StartsWith('<') -and $target.EndsWith('>')) {
        $target = $target.Substring(1, $target.Length - 2)
    }

    if ($target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { return $null }
    if ($target.StartsWith('#')) { return $null }

    $target = $target -replace '#.*$', ''
    $target = $target -replace '\?.*$', ''
    if ([string]::IsNullOrWhiteSpace($target)) { return $null }

    try { $target = [System.Uri]::UnescapeDataString($target) } catch {}

    if ($target.StartsWith('/')) {
        $candidate = Join-Path $repoRoot $target.TrimStart('/')
    } else {
        $sourceDir = Split-Path -Parent (Join-Path $repoRoot $SourcePath)
        $candidate = Join-Path $sourceDir $target
    }

    try { return [System.IO.Path]::GetFullPath($candidate) }
    catch { return $candidate }
}

foreach ($path in $markdownFiles) {
    $fullPath = Join-Path $repoRoot $path
    $content = Get-Content -LiteralPath $fullPath -Raw
    $matches = [regex]::Matches($content, '!?\[[^\]]*\]\((?<target>[^)]+)\)')

    foreach ($match in $matches) {
        $rawTarget = $match.Groups['target'].Value
        if ($rawTarget -match '\s+"[^"]*"$') {
            $rawTarget = ($rawTarget -split '\s+"', 2)[0]
        }

        $resolved = Resolve-LocalMarkdownTarget -SourcePath $path -RawTarget $rawTarget
        if ($null -eq $resolved) { continue }

        if (-not (Test-Path -LiteralPath $resolved)) {
            $relative = $resolved
            try { $relative = [System.IO.Path]::GetRelativePath($repoRoot, $resolved) } catch {}
            $failures.Add("Broken local Markdown link: $path -> $rawTarget (resolved: $relative)")
        }
    }
}

$indexRoots = @(
    'learn','how-to','reference','systems','mechanics','investigate',
    'diagnose','case-studies','examples','templates','tooling','sources'
)

$indexExcludedPrefixes = @(
    'templates/mods/',
    'templates/mono/',
    'templates/il2cpp/',
    'templates/hybrid/mono-merlin/',
    'templates/merlin/basic/Assets/'
)

foreach ($root in $indexRoots) {
    if (-not (Test-Path -LiteralPath $root -PathType Container)) { continue }

    $readmes = @(Get-ChildItem -LiteralPath $root -Filter README.md -File -Recurse)
    foreach ($readme in $readmes) {
        $readmeRel = [System.IO.Path]::GetRelativePath($repoRoot, $readme.FullName).Replace('\','/')
        $dirRel = (Split-Path -Parent $readmeRel).Replace('\','/')

        $excluded = $false
        foreach ($prefix in $indexExcludedPrefixes) {
            if (($dirRel + '/').StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
                $excluded = $true
                break
            }
        }
        if ($excluded) { continue }

        $siblings = @(
            Get-ChildItem -LiteralPath $readme.Directory.FullName -File -Filter '*.md' |
                Where-Object { $_.Name -ne 'README.md' }
        )
        if ($siblings.Count -eq 0) { continue }

        $readmeText = Get-Content -LiteralPath $readme.FullName -Raw
        foreach ($sibling in $siblings) {
            $escapedName = [regex]::Escape($sibling.Name)
            $linked = $readmeText -match "\]\([^)]*$escapedName(?:#[^)]*)?\)"
            if (-not $linked) {
                $message = "Unindexed sibling Markdown page: $dirRel/$($sibling.Name) is not linked from $readmeRel"
                if ($NoFailOnIndexGaps) { $warnings.Add($message) }
                else { $failures.Add($message) }
            }
        }
    }
}

$frontMatterFiles = New-Object System.Collections.Generic.List[string]
foreach ($path in $markdownFiles) {
    $firstLine = Get-Content -LiteralPath (Join-Path $repoRoot $path) -TotalCount 1
    if ($firstLine -eq '---') { $frontMatterFiles.Add($path) }
}

foreach ($root in @('mechanics/','investigate/')) {
    $rootFiles = @($markdownFiles | Where-Object { $_.StartsWith($root) })
    foreach ($metadataPath in $rootFiles) {
        if (-not $frontMatterFiles.Contains($metadataPath)) {
            $failures.Add("Missing front matter: $metadataPath")
            continue
        }

        $metadataContent = Get-Content -LiteralPath (Join-Path $repoRoot $metadataPath) -Raw
        $metadataMatch = [regex]::Match($metadataContent, '(?s)^---\s*\r?\n(?<yaml>.*?)\r?\n---')
        if (-not $metadataMatch.Success) {
            $failures.Add("Malformed front matter: $metadataPath")
            continue
        }

        $yaml = $metadataMatch.Groups['yaml'].Value
        foreach ($requiredKey in @('document_type','scope','last_verified')) {
            if ($yaml -notmatch "(?m)^$requiredKey\s*:") {
                $failures.Add("Front matter missing required key '$requiredKey': $metadataPath")
            }
        }

        $dateMatch = [regex]::Match($yaml, '(?m)^last_verified\s*:\s*(?<date>[^\r\n]+)')
        if ($dateMatch.Success -and $dateMatch.Groups['date'].Value.Trim() -notmatch '^\d{4}-\d{2}-\d{2}

$paragraphOwners = @{}
foreach ($path in $markdownFiles) {
    if ($path.StartsWith('templates/')) { continue }
    if ($path.StartsWith('contributing/authoring/reference-audit/packets/')) { continue }

    $content = Get-Content -LiteralPath (Join-Path $repoRoot $path) -Raw
    foreach ($paragraph in [regex]::Split($content, '(?:\r?\n){2,}')) {
        $normalized = (($paragraph -replace '\s+', ' ').Trim())
        if ($normalized.Length -lt 160) { continue }
        if ($normalized.StartsWith('#') -or $normalized.StartsWith([string][char]96) -or $normalized.StartsWith('~~~')) { continue }

        if (-not $paragraphOwners.ContainsKey($normalized)) {
            $paragraphOwners[$normalized] = New-Object System.Collections.Generic.List[string]
        }
        $paragraphOwners[$normalized].Add($path)
    }
}

foreach ($entry in $paragraphOwners.GetEnumerator()) {
    $owners = @($entry.Value | Sort-Object -Unique)
    if ($owners.Count -gt 1) {
        $failures.Add("Duplicate long paragraph in: $($owners -join ', ')")
    }
}

Write-Host "Documentation audit: $($markdownFiles.Count) Markdown files."
Write-Host "Front matter present on $($frontMatterFiles.Count) Markdown files."

if ($warnings.Count -gt 0) {
    Write-Host 'Documentation audit warnings:' -ForegroundColor Yellow
    $warnings | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Yellow }
}

if ($failures.Count -gt 0) {
    Write-Host 'Documentation audit FAILED:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host 'Documentation audit PASSED.' -ForegroundColor Green
) {
            $failures.Add("Front matter last_verified is not YYYY-MM-DD: $metadataPath")
        }
    }
}

$paragraphOwners = @{}
foreach ($path in $markdownFiles) {
    if ($path.StartsWith('templates/')) { continue }

    $content = Get-Content -LiteralPath (Join-Path $repoRoot $path) -Raw
    foreach ($paragraph in [regex]::Split($content, '(?:\r?\n){2,}')) {
        $normalized = (($paragraph -replace '\s+', ' ').Trim())
        if ($normalized.Length -lt 160) { continue }
        if ($normalized.StartsWith('#') -or $normalized.StartsWith([string][char]96) -or $normalized.StartsWith('~~~')) { continue }

        if (-not $paragraphOwners.ContainsKey($normalized)) {
            $paragraphOwners[$normalized] = New-Object System.Collections.Generic.List[string]
        }
        $paragraphOwners[$normalized].Add($path)
    }
}

foreach ($entry in $paragraphOwners.GetEnumerator()) {
    $owners = @($entry.Value | Sort-Object -Unique)
    if ($owners.Count -gt 1) {
        $warnings.Add("Duplicate long paragraph in: $($owners -join ', ')")
    }
}

Write-Host "Documentation audit: $($markdownFiles.Count) Markdown files."
Write-Host "Front matter present on $($frontMatterFiles.Count) Markdown files."

if ($warnings.Count -gt 0) {
    Write-Host 'Documentation audit warnings:' -ForegroundColor Yellow
    $warnings | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Yellow }
}

if ($failures.Count -gt 0) {
    Write-Host 'Documentation audit FAILED:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host 'Documentation audit PASSED.' -ForegroundColor Green
