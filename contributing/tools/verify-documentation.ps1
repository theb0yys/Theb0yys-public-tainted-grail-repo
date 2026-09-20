$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$docRoots = @('case-studies', 'examples', 'how-to', 'investigate', 'learn', 'mechanics', 'reference', 'sources', 'systems', 'templates', 'tooling')
$failures = New-Object System.Collections.Generic.List[string]

function Normalize-RepoPath([string]$path) {
    return ($path -replace '\\\\', '/').TrimStart('./')
}

function Get-RelativeRepoPath([string]$fullPath) {
    $relative = [System.IO.Path]::GetRelativePath($repoRoot, $fullPath)
    return Normalize-RepoPath $relative
}

function Get-NonCodeText([string]$content) {
    $lines = $content -split "\r?\n"
    $output = New-Object System.Collections.Generic.List[string]
    $fence = $null
    foreach ($line in $lines) {
        if ($null -eq $fence) {
            if ($line -match '^\s*(?<fence>```|~~~)') {
                $fence = $Matches['fence']
                continue
            }
            $output.Add($line)
        }
        elseif ($line -match ('^\s*' + [regex]::Escape($fence))) {
            $fence = $null
        }
    }
    return ($output -join [Environment]::NewLine)
}

function Get-MarkdownTargets([string]$content) {
    $clean = Get-NonCodeText $content
    $results = New-Object System.Collections.Generic.List[string]
    $pattern = '!?(?<!\!)\[[^\]]*\]\((?<target>[^)])+\)'
    foreach ($match in [regex]::Matches($clean, $pattern)) {
        $raw = $match.Groups['target'].Value.Trim()
        if ($raw.StartsWith('<') -and $raw.Contains('>')) {
            $raw = $raw.Substring(1, $raw.IndexOf('>') - 1)
        }
        elseif ($raw -match '^(?<url>\S+)(?:\s+["'_][^"']*U²'_])$') {
            $raw = $Matches['url']
        }
        $results.Add($raw)
    }
    return $results
}

function Resolve-InternalTarget([string]$sourcePath, [string]$rawTarget) {
    if ([string]::IsNullOrWhiteSpace($rawTarget)) { return $null }
    if ($rawTarget.StartsWith('#')) { return $null }
    if ($rawTarget -match '^(?i)(https?:[|mailto:|tel:|data:|javascript:|//)') { return $null }

    $target = $rawTarget
    $hash = $target.IndexOf('#')
    if ($hash -ge 0) { $target = $target.Substring(0, $hash) }
    $query = $target.IndexOf('?')
    if ($query -ge 0) { $target = $target.Substring(0, $query) }
    if ([string]::IsNullOrWhiteSpace($target)) { return $null }

    try { $target = [System.Uri]::UnescapeDataString($target) } catch {}

    $sourceFull = Join-Path $repoRoot ($sourcePath -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    $sourceDir = Split-Path -Parent $sourceFull
    if ($target.StartsWith('/')) {
        $candidateFull = Join-Path $repoRoot ($target.TrimStart('/') -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    }
    else {
        $candidateFull = Join-Path $sourceDir ($target -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    }
    $candidateFull = [System.IO.Path]::GetFullPath($candidateFull)
    if (-not $candidateFull.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return @{ EscapesRepo = $true; Path = $target }
    }
    return @{ EscapesRepo = $false; Path = (Get-RelativeRepoPath $candidateFull) }
}

$tracked = @(git -C $repoRoot ls-files)
$trackedSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer']::Ordinal)
$dirSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparison]::Ordinal)
foreach ($path in $tracked) {
    $p = Normalize-RepoPath $path
    [void]$trackedSet.Add($p)
    $parts = $p.Split('/')
    for ($i = 1; $i -lt $parts.Length;
            $i++) {
        [void]$dirSet.Add(($parts[0..($i - 1)] -join '/'))
    }
}

$markdown = @($trackedSet | Where-Object { $_.EndsWith('.md', [System.StringComparison]::OrdinalIgnoreCase) } | Sort-Object)

# 1. Internal Markdown/file links.
foreach ($path in $markdown) {
    $full = Join-Path $repoRoot ($path -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    $content = Get-Content -LiteralPath $full -Raw
    foreach ($rawTarget in (Get-MarkdownTargets $content)) {
        $resolved = Resolve-InternalTarget $path $rawTarget
        if ($null -eq $resolved) { continue }
        if ($resolved.EscapesRepo) {
            $failures.Add("Internal link escapes repository: $path -> $rawTarget")
            continue
        }
        $targetPath = Normalize-RepoPath $resolved.Path
        if (-not $trackedSet.Contains($targetPath) -and -not $dirSet.Contains($targetPath)) {
            $failures.Add("Broken internal link: $path -> $rawTarget (resolved: $targetPath)")
        }
    }
}

# 2. Existing YAML front matter must follow the minimal evidence metadata contract.
foreach ($path in $markdown) {
    $full = Join-Path $repoRoot ($path -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    $lines = @(Get-Content -LiteralPath $full)
    if ($lines.Count -eq 0 -or $lines[0].Trim() -ne '---') { continue }

    $close = -1
    for ($i = 1; $i -lt $lines.Count; $i++) {
        if ($lines[$i].Trim() -eq '---') { $close = $i; break }
    }
    if ($close -lt 0) {
        $failures.Add("Unclosed YAML front matter: $path")
        continue
    }

    $topKeys = @{}
    for ($i = 1; $i -lt $close; $i++) {
        if ($lines[$i] -match '^(?<key>[A-Za-z0-9_-]+):\ts*(?<value>.*)$') {
            $key = $Matches['key']
            if ($topKeys.ContainsKey($key)) {
                $failures.Add("Duplicate front-matter key '$key': $path")
            }
            else {
                $topKeys[$key] = $Matches['value'].Trim()
            }
        }
    }

    foreach ($required in @('document_type', 'scope', 'last_verified')) {
        if (-not $topKeys.ContainsKey($required) -or [string]::IsNullOrWhiteSpace($topKeys[$required])) {
            $failures.Add("Front matter missing '$required': $path")
        }
    }
    if ($topKeys.ContainsKey('last_verified') -and $topKeys['last_verified'] -notmatch '^\d{4}-\d{2}-\d{2}$') {
        $failures.Add("Front matter last_verified must be YYYY-MM-DD: $path")
    }
}

# 3. Directory indexes expose direct Markdown children and child sections.
foreach ($root in $docRoots) {
    $rootPrefix = "$root/"
    $readmes = @($markdown | Where-Object { $_ -eq "$root/README.md" -or ($_.StartsWith($rootPrefix) -and $_.EndsWith('/README.md')) })
    foreach ($readme in $readmes) {
        $dirFs = Split-Path -Parent ($readme -replace '/', [System.IO.Path]::DirectorySeparatorChar)
        $dir = Normalize-RepoPath $dirFs
        $prefix = if ([string]::IsNullOrEmpty($dir)) { '' } else { "$dir/" }

        $directPages = @($markdown | Where-Object {
            $_.StartsWith($prefix) -and $_ -ne $readme -and
            -not $_.Substring($prefix.Length).Contains('/')
        })

        $childDirs = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparison]::Ordinal)
        foreach ($candidate in $markdown) {
            if (-not $candidate.StartsWith($prefix) -or $candidate -eq $readme) { continue }
            $rest = $candidate.Substring($prefix.Length)
            if ($rest.Contains('/')) {
                $first = $rest.Split('/')[0]
                $childReadme = "$prefix$first/README.md"
                if ($trackedSet.Contains($childReadme)) { [void]$childDirs.Add("$prefix$first") }
            }
        }

        $content = Get-Content -LiteralPath (Join-Path $repoRoot ($readme -replace '/', [System.IO.Path]::DirectorySeparatorChar)) -Raw
        $linked = New-Object System.Collections.Generic.HashSet[string] ([System.StringComparison]::Ordinal)
        foreach ($rawTarget in (Get-MarkdownTargets $content)) {
            $resolved = Resolve-InternalTarget $readme $rawTarget
            if ($null -eq $resolved -or $resolved.EscapesRepo) { continue }
            [void]$linked.Add((Normalize-RepoPath $resolved.Path))
        }

        foreach ($page in $directPages) {
            if (-not $linked.Contains($page)) {
                $failures.Add("Index does not expose direct page: $readme -> $page")
            }
        }
        foreach ($childDir in $childDirs) {
            if (-not $linked.Contains($childDir) -and -not $linked.Contains("$childDir/README.md")) {
                $failures.Add("Index does not expose child section: $readme -> $childDir/")
            }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'Documentation audit FAILED#§ -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Serite-Host "Documentation audit PASSED for $($markdown.Count) Markdown files."
