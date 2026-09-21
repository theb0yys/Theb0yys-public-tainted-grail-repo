[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$FoAGameRoot,

    [string]$ExpectedBuildId = '24270691',
    [string]$ExpectedBranch = 'mono',
    [string]$EvidenceRoot = '',
    [switch]$DeployTaintedWeapons,
    [switch]$LaunchGame,
    [int]$TimeoutSeconds = 600,
    [switch]$KeepTracePlugin
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($env:OS -ne 'Windows_NT') {
    throw 'Tainted Weapons startup tracing is Windows-only.'
}
if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw 'PowerShell 7 or newer is required.'
}

$repoRoot = (& git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw 'Unable to resolve repository root.'
}
$repoRoot = (Resolve-Path -LiteralPath $repoRoot).Path
Set-Location $repoRoot

$sourceCommit = (& git rev-parse HEAD).Trim().ToLowerInvariant()
$workingTreeRows = @(& git status --porcelain)
if ($workingTreeRows.Count -ne 0) {
    throw "Repository must be clean before startup tracing. Status=$($workingTreeRows -join ' | ')"
}
$markerRows = @(& git grep -n -E '^(<<<<<<<|=======|>>>>>>>)' -- 'mods/tainted-weapons/src' 'mods/tainted-weapons/tools' 2>$null)
if ($markerRows.Count -ne 0) {
    throw "Tainted Weapons merge-marker scan failed. Matches=$($markerRows -join ' | ')"
}

$gameRoot = (Resolve-Path -LiteralPath $FoAGameRoot).Path
if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $stamp = [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssZ')
    $EvidenceRoot = Join-Path $repoRoot "documents\runtime-evidence\weapon-framework\mono-$ExpectedBuildId\startup-trace\$stamp"
}
if (Test-Path -LiteralPath $EvidenceRoot) {
    if (@(Get-ChildItem -LiteralPath $EvidenceRoot -Force -ErrorAction SilentlyContinue).Count -ne 0) {
        throw "EvidenceRoot must be absent or empty: $EvidenceRoot"
    }
} else {
    New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
}
$EvidenceRoot = (Resolve-Path -LiteralPath $EvidenceRoot).Path

$failures = [System.Collections.Generic.List[string]]::new()
$warnings = [System.Collections.Generic.List[string]]::new()
function Add-Failure([string]$Message) {
    if (-not [string]::IsNullOrWhiteSpace($Message) -and -not $failures.Contains($Message)) { $failures.Add($Message) }
}
function Add-Warning([string]$Message) {
    if (-not [string]::IsNullOrWhiteSpace($Message) -and -not $warnings.Contains($Message)) { $warnings.Add($Message) }
}
function Get-Sha256([string]$Path) {
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToUpperInvariant()
}
function Write-Json([object]$Value, [string]$Path) {
    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
    $Value | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
}
function Get-GameProcesses([string]$Root) {
    $prefix = [System.IO.Path]::GetFullPath($Root).TrimEnd('\') + '\'
    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($process in Get-Process -ErrorAction SilentlyContinue) {
        try {
            if ([string]::IsNullOrWhiteSpace($process.Path)) { continue }
            $path = [System.IO.Path]::GetFullPath($process.Path)
            if ($path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) { $rows.Add($process) }
        } catch { continue }
    }
    return @($rows)
}
function Resolve-GameExecutable([string]$Root) {
    $preferred = Join-Path $Root 'Fall of Avalon.exe'
    if (Test-Path -LiteralPath $preferred -PathType Leaf) { return $preferred }
    $candidates = @(Get-ChildItem -LiteralPath $Root -File -Filter '*.exe' | Where-Object { $_.Name -match '(?i)tainted|grail|avalon|fall' -and $_.Name -notmatch '(?i)crash|reporter|unins' })
    if ($candidates.Count -ne 1) { throw "Unable to resolve one game executable. Candidates=$($candidates.FullName -join ', ')" }
    return $candidates[0].FullName
}
function Read-LogDelta([string]$Path, [long]$BaselineLength) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return '' }
    $item = Get-Item -LiteralPath $Path
    if ($item.Length -le $BaselineLength) { return '' }
    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
        $null = $stream.Seek($BaselineLength, [System.IO.SeekOrigin]::Begin)
        $reader = [System.IO.StreamReader]::new($stream)
        try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
    } finally { $stream.Dispose() }
}

if (@(Get-GameProcesses $gameRoot).Count -ne 0) {
    throw 'FoA or a process under the game root is already running. Close it before starting the trace.'
}

$reconciliationScript = Join-Path $repoRoot 'mods\tainted-weapons\tools\Invoke-TaintedWeaponsMonoReconciliation.ps1'
$reconciliationRoot = Join-Path $EvidenceRoot 'binary-reconciliation'
$reconciliationArgs = @(
    '-NoProfile', '-File', $reconciliationScript,
    '-FoAGameRoot', $gameRoot,
    '-ExpectedBuildId', $ExpectedBuildId,
    '-ExpectedBranch', $ExpectedBranch,
    '-EvidenceRoot', $reconciliationRoot,
    '-RequireLiveMatch'
)
if ($DeployTaintedWeapons) { $reconciliationArgs += '-Deploy' }
& pwsh @reconciliationArgs
if ($LASTEXITCODE -ne 0) {
    throw "Binary reconciliation failed with exit code $LASTEXITCODE. Startup trace is blocked."
}

$traceProject = Join-Path $repoRoot 'mods\tainted-weapons\tools\TaintedWeapons.StartupTrace\TaintedWeapons.StartupTrace.csproj'
$traceBuildLog = Join-Path $EvidenceRoot 'startup-trace-build.log'
$traceBuildArgs = @('build', $traceProject, '-c', 'Release', '--nologo', "-p:FoAGameRoot=$gameRoot")
$traceBuildOutput = @(& dotnet @traceBuildArgs 2>&1 | ForEach-Object { $_.ToString() })
$traceBuildExitCode = if ($null -eq $LASTEXITCODE) { 0 } else { [int]$LASTEXITCODE }
$traceBuildOutput | Set-Content -LiteralPath $traceBuildLog -Encoding utf8NoBOM
if ($traceBuildExitCode -ne 0) {
    throw "Startup trace plugin build failed. See $traceBuildLog"
}

$builtTraceDll = Join-Path $repoRoot 'mods\tainted-weapons\tools\TaintedWeapons.StartupTrace\bin\Release\netstandard2.1\TaintedWeapons.StartupTrace.dll'
if (-not (Test-Path -LiteralPath $builtTraceDll -PathType Leaf)) {
    throw "Built startup trace DLL was not found: $builtTraceDll"
}

$liveTraceRoot = Join-Path $gameRoot 'BepInEx\plugins\TaintedWeaponsStartupTrace'
$liveTraceDll = Join-Path $liveTraceRoot 'TaintedWeapons.StartupTrace.dll'
$backupTraceDll = ''
$hadExistingTracePlugin = Test-Path -LiteralPath $liveTraceDll -PathType Leaf
if ($hadExistingTracePlugin) {
    $backupRoot = Join-Path $gameRoot ("BepInEx\_codex_backups\TaintedWeaponsStartupTrace\startup-trace-" + [DateTimeOffset]::UtcNow.ToString('yyyyMMddTHHmmssZ'))
    New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null
    $backupTraceDll = Join-Path $backupRoot 'TaintedWeapons.StartupTrace.dll'
    Copy-Item -LiteralPath $liveTraceDll -Destination $backupTraceDll -Force
}
New-Item -ItemType Directory -Force -Path $liveTraceRoot | Out-Null
Copy-Item -LiteralPath $builtTraceDll -Destination $liveTraceDll -Force

$builtTraceSha = Get-Sha256 $builtTraceDll
$liveTraceSha = Get-Sha256 $liveTraceDll
if ($builtTraceSha -ne $liveTraceSha) {
    throw 'Startup trace plugin deployment hash mismatch.'
}

$traceReceiptRoot = Join-Path $gameRoot 'BepInEx\plugins\TaintedWeapons\receipts'
New-Item -ItemType Directory -Force -Path $traceReceiptRoot | Out-Null
$traceStart = [DateTimeOffset]::UtcNow
$existingTraceFiles = @(Get-ChildItem -LiteralPath $traceReceiptRoot -File -Filter 'startup-trace-*.jsonl' -ErrorAction SilentlyContinue | ForEach-Object { $_.FullName })
$logPath = Join-Path $gameRoot 'BepInEx\LogOutput.log'
$logBaselineLength = if (Test-Path -LiteralPath $logPath -PathType Leaf) { (Get-Item -LiteralPath $logPath).Length } else { 0L }

try {
    if ($LaunchGame) {
        $gameExecutable = Resolve-GameExecutable $gameRoot
        $process = Start-Process -FilePath $gameExecutable -WorkingDirectory $gameRoot -PassThru
        Write-Host "FoA started with PID $($process.Id). Reach the title screen and close the game normally. The harness will not terminate it."
        try {
            Wait-Process -Id $process.Id -Timeout $TimeoutSeconds -ErrorAction Stop
        } catch {
            Add-Failure "game-did-not-exit-before-timeout:$TimeoutSeconds"
            Write-Warning 'FoA was not terminated by the harness. Close it normally and rerun the trace.'
        }
    } else {
        Write-Host 'Launch FoA manually, wait for the title screen, then close it normally.'
        $null = Read-Host 'Press Enter only after FoA has closed'
    }

    if (@(Get-GameProcesses $gameRoot).Count -ne 0) {
        Add-Failure 'game-process-still-running-after-capture'
    }

    $newTraceFiles = @(Get-ChildItem -LiteralPath $traceReceiptRoot -File -Filter 'startup-trace-*.jsonl' -ErrorAction SilentlyContinue |
        Where-Object { $existingTraceFiles -notcontains $_.FullName -and $_.LastWriteTimeUtc -ge $traceStart.UtcDateTime.AddSeconds(-2) } |
        Sort-Object LastWriteTimeUtc -Descending)
    if ($newTraceFiles.Count -eq 0) {
        Add-Failure 'startup-trace-file-not-created'
    }

    $traceFile = if ($newTraceFiles.Count -gt 0) { $newTraceFiles[0].FullName } else { '' }
    $traceRows = @()
    if (-not [string]::IsNullOrWhiteSpace($traceFile)) {
        Copy-Item -LiteralPath $traceFile -Destination (Join-Path $EvidenceRoot 'startup-trace.jsonl') -Force
        foreach ($line in Get-Content -LiteralPath $traceFile) {
            if ([string]::IsNullOrWhiteSpace($line)) { continue }
            try { $traceRows += $line | ConvertFrom-Json } catch { Add-Failure "invalid-trace-json:$($_.Exception.Message)" }
        }
    }

    $requiredMethodFragments = @(
        'ApplicationScene.InitAll',
        'ApplicationScene.InitLocalization',
        'BabelManager.Initialize',
        'TemplatesLoader.CreateAndLoad',
        'TemplatesLoader.set_FinishedLoading'
    )
    foreach ($fragment in $requiredMethodFragments) {
        if (-not ($traceRows | Where-Object { $_.event -eq 'method-exit' -and [string]$_.method -like "*$fragment*" })) {
            Add-Failure "required-startup-event-missing:$fragment"
        }
    }

    if (-not ($traceRows | Where-Object { $_.event -eq 'trace-plugin-awake' })) {
        Add-Failure 'trace-plugin-awake-missing'
    }
    if ($traceRows | Where-Object { $_.mutationViolation -eq $true }) {
        Add-Failure 'custom-template-registration-observed-during-non-mutating-trace'
    }
    $maxRegistered = @($traceRows | Where-Object { $null -ne $_.customRegisteredCount } | ForEach-Object { [int]$_.customRegisteredCount } | Measure-Object -Maximum).Maximum
    if ($null -ne $maxRegistered -and $maxRegistered -gt 0) {
        Add-Failure "custom-registered-count-nonzero:$maxRegistered"
    }

    $logDelta = Read-LogDelta $logPath $logBaselineLength
    $logDeltaPath = Join-Path $EvidenceRoot 'bepinex-log-delta.log'
    $logDelta | Set-Content -LiteralPath $logDeltaPath -Encoding utf8NoBOM
    if ($logDelta -notmatch 'Tainted Weapons\s+0\.3\.20\s+loaded') {
        Add-Failure 'tainted-weapons-loaded-line-missing'
    }
    if ($logDelta -match '(?i)template_registered|itemGrant|item grant|saveWrites=true|inventoryWrites=true') {
        Add-Failure 'mutation-indicator-found-in-log-delta'
    }

    $receipt = [ordered]@{
        schema_version = 'tainted-weapons-non-mutating-startup-trace-v1'
        captured_at_utc = [DateTimeOffset]::UtcNow.ToString('O')
        source_commit = $sourceCommit
        game_root = $gameRoot
        expected_build_id = $ExpectedBuildId
        expected_branch = $ExpectedBranch
        reconciliation_receipt = [System.IO.Path]::GetRelativePath($repoRoot, (Join-Path $reconciliationRoot 'binary-reconciliation.json')).Replace('\', '/')
        trace_plugin = [ordered]@{
            built_sha256 = $builtTraceSha
            live_sha256 = $liveTraceSha
            deployed_path = $liveTraceDll
            previous_backup = $backupTraceDll
        }
        launch_mode = if ($LaunchGame) { 'harness-launched' } else { 'manual' }
        trace_file = $traceFile
        trace_file_sha256 = if (-not [string]::IsNullOrWhiteSpace($traceFile)) { Get-Sha256 $traceFile } else { '' }
        trace_row_count = $traceRows.Count
        max_custom_registered_count = if ($null -eq $maxRegistered) { $null } else { $maxRegistered }
        log_delta_sha256 = Get-Sha256 $logDeltaPath
        failures = @($failures)
        warnings = @($warnings)
        status = if ($failures.Count -eq 0) { 'PASS' } else { 'FAIL' }
    }
    Write-Json $receipt (Join-Path $EvidenceRoot 'startup-trace-receipt.json')
    Write-Host "Tainted Weapons startup trace status: $($receipt.status)"
    Write-Host "Evidence: $EvidenceRoot"
} finally {
    if (-not $KeepTracePlugin) {
        if (Test-Path -LiteralPath $liveTraceDll -PathType Leaf) {
            Remove-Item -LiteralPath $liveTraceDll -Force
        }
        if ($hadExistingTracePlugin -and -not [string]::IsNullOrWhiteSpace($backupTraceDll)) {
            New-Item -ItemType Directory -Force -Path $liveTraceRoot | Out-Null
            Copy-Item -LiteralPath $backupTraceDll -Destination $liveTraceDll -Force
        }
    }
}

if ($failures.Count -ne 0) {
    foreach ($failure in $failures) { Write-Error $failure }
    exit 2
}
