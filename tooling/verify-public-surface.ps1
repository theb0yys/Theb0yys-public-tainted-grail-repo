$ErrorActionPreference = 'Stop'

$forbiddenExtensions = @(
    '.dll', '.exe', '.pdb', '.zip', '.7z', '.rar', '.pak',
    '.assets', '.bundle', '.unity3d', '.resource', '.resources', '.ress',
    '.bank', '.bnk', '.wav', '.mp3', '.ogg', '.flac',
    '.png', '.jpg', '.jpeg', '.dds', '.tga', '.fbx', '.obj', '.blend'
)

$tracked = @(git ls-files)
$failures = New-Object System.Collections.Generic.List[string]

foreach ($path in $tracked) {
    $extension = [System.IO.Path]::GetExtension($path).ToLowerInvariant()
    if ($forbiddenExtensions -contains $extension) {
        $failures.Add("Forbidden public-repo file type: $path")
    }

    if (Test-Path -LiteralPath $path -PathType Leaf) {
        $size = (Get-Item -LiteralPath $path).Length
        if ($size -gt 2MB) {
            $failures.Add("Tracked file exceeds 2 MiB public-repo limit: $path ($size bytes)")
        }
    }
}

$scanFiles = $tracked | Where-Object {
    $_ -ne 'tools/verify-public-surface.ps1' -and
    (Test-Path -LiteralPath $_ -PathType Leaf)
}

$patterns = @(
    @{ Name = 'GitHub token'; Regex = 'gh[pousr]_[A-Za-z0-9_]{20,}' },
    @{ Name = 'GitHub fine-grained token'; Regex = 'github_pat_[A-Za-z0-9_]{20,}' },
    @{ Name = 'Private key'; Regex = 'BEGIN [A-Z ]*PRIVATE KEY' },
    @{ Name = 'User profile path'; Regex = '[A-Za-z]:\\Users\\[^\\\s]+' },
    @{ Name = 'Steam library absolute path'; Regex = '[A-Za-z]:\\[^\r\n]*SteamLibrary\\' }
)

foreach ($path in $scanFiles) {
    try {
        $content = Get-Content -LiteralPath $path -Raw -ErrorAction Stop
    }
    catch {
        continue
    }

    foreach ($pattern in $patterns) {
        if ($content -match $pattern.Regex) {
            $failures.Add("$($pattern.Name) pattern found in: $path")
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host 'Public-surface validation FAILED:' -ForegroundColor Red
    $failures | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    exit 1
}

Write-Host "Public-surface validation PASSED for $($tracked.Count) tracked files."
