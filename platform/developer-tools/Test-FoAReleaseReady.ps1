[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$GameRoot,

    [string]$BaselineFingerprint,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$SkipBuild,

    [switch]$StagePackage,

    [string]$OutputRoot,

    [switch]$Zip,

    [switch]$PersistenceNotApplicable,

    [switch]$Quiet
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-State {
    param(
        [System.Collections.Generic.List[object]]$States,
        [string]$Check,
        [string]$State,
        [string]$Detail
    )

    $States.Add([pscustomobject]@{
        Check = $Check
        State = $State
        Detail = $Detail
    })
}

$states = New-Object System.Collections.Generic.List[object]
$projectPath = (Resolve-Path -LiteralPath $Project).Path
$projectDir = Split-Path -Parent $projectPath

$environment = $null
try {
    $environmentScript = Join-Path $PSScriptRoot "Test-FoAEnvironment.ps1"
    $environment = & $environmentScript -GameRoot $GameRoot -Quiet

    if ($environment.Ready) {
        Add-State $states "Environment" "PASSED" "$($environment.Runtime) / $($environment.Loader)"
    }
    else {
        Add-State $states "Environment" "FAILED" ($environment.Issues -join " | ")
    }
}
catch {
    Add-State $states "Environment" "FAILED" $_.Exception.Message
}

$doctor = $null
try {
    $doctorScript = Join-Path $PSScriptRoot "Test-FoAModProject.ps1"
    $doctor = & $doctorScript -Project $projectPath -GameRoot $(if ($null -ne $environment) { $environment.GameRoot } else { $GameRoot }) -Quiet

    if ($doctor.Ready) {
        $detail = "Project doctor passed"
        if ($doctor.WarningCount -gt 0) {
            $detail += " with $($doctor.WarningCount) warning(s)"
        }
        Add-State $states "ProjectStructure" "PASSED" $detail
    }
    else {
        Add-State $states "ProjectStructure" "FAILED" "$($doctor.FailedCount) blocking project finding(s)"
    }
}
catch {
    Add-State $states "ProjectStructure" "FAILED" $_.Exception.Message
}

if ([string]::IsNullOrWhiteSpace($BaselineFingerprint)) {
    Add-State $states "CompatibilityFingerprint" "NOT_RUN" "No baseline fingerprint was supplied."
}
elseif ($null -eq $environment -or -not $environment.Ready) {
    Add-State $states "CompatibilityFingerprint" "BLOCKED" "Environment validation did not pass."
}
else {
    try {
        $compareScript = Join-Path $PSScriptRoot "Compare-FoAFingerprint.ps1"
        $compat = & $compareScript -Baseline $BaselineFingerprint -GameRoot $environment.GameRoot -Quiet

        if ($compat.RevalidationRequired) {
            Add-State $states "CompatibilityFingerprint" "PARTIAL" "$($compat.ChangeCount) fingerprint change(s); revalidation required, not automatically broken."
        }
        else {
            Add-State $states "CompatibilityFingerprint" "PASSED" "Current runtime/loader fingerprint matches the supplied baseline."
        }
    }
    catch {
        Add-State $states "CompatibilityFingerprint" "FAILED" $_.Exception.Message
    }
}

$buildPassed = $false
if ($SkipBuild) {
    Add-State $states "Build" "NOT_RUN" "Build skipped by request."
}
elseif ($null -eq $environment -or -not $environment.Ready -or $null -eq $doctor -or -not $doctor.Ready) {
    Add-State $states "Build" "BLOCKED" "Environment/project checks did not pass."
}
else {
    try {
        $buildScript = Join-Path $PSScriptRoot "Build-FoAMod.ps1"
        $build = & $buildScript -Project $projectPath -GameRoot $environment.GameRoot -Configuration $Configuration

        if ($build.ArtifactExists) {
            $buildPassed = $true
            Add-State $states "Build" "PASSED" $build.Artifact
        }
        else {
            Add-State $states "Build" "FAILED" "Build returned without the expected artifact."
        }
    }
    catch {
        Add-State $states "Build" "FAILED" $_.Exception.Message
    }
}

try {
    $inventoryScript = Join-Path $PSScriptRoot "Get-FoAModInventory.ps1"
    $inventory = & $inventoryScript -GameRoot $(if ($null -ne $environment) { $environment.GameRoot } else { $GameRoot }) -Quiet

    if ($inventory.ConflictCount -eq 0) {
        Add-State $states "InstalledModConflicts" "PASSED" "$($inventory.ItemCount) installed DLL(s); no duplicate identity/file-name conflicts detected."
    }
    else {
        Add-State $states "InstalledModConflicts" "PARTIAL" "$($inventory.ConflictCount) potential duplicate/conflict finding(s) require review."
    }
}
catch {
    Add-State $states "InstalledModConflicts" "FAILED" $_.Exception.Message
}

try {
    $installCompareScript = Join-Path $PSScriptRoot "Compare-FoAModInstall.ps1"
    $installCompare = & $installCompareScript -Project $projectPath -GameRoot $(if ($null -ne $environment) { $environment.GameRoot } else { $GameRoot }) -Configuration $Configuration -Quiet

    switch ($installCompare.State) {
        "MATCH" {
            Add-State $states "BuiltVsInstalled" "PASSED" "Installed DLL matches the current build artifact."
        }
        "DIFFERENT" {
            Add-State $states "BuiltVsInstalled" "PARTIAL" "Installed DLL differs from the current build artifact."
        }
        "NOT_INSTALLED" {
            Add-State $states "BuiltVsInstalled" "NOT_RUN" "The mod is not currently installed."
        }
        "BUILD_NOT_FOUND" {
            Add-State $states "BuiltVsInstalled" "NOT_RUN" "No build artifact is available for comparison."
        }
        default {
            Add-State $states "BuiltVsInstalled" "PARTIAL" "Comparison state: $($installCompare.State)"
        }
    }
}
catch {
    Add-State $states "BuiltVsInstalled" "FAILED" $_.Exception.Message
}

$package = $null
if (-not $StagePackage) {
    Add-State $states "ReleasePackage" "NOT_RUN" "Package staging was not requested."
}
elseif ($null -eq $doctor -or -not $doctor.Ready) {
    Add-State $states "ReleasePackage" "BLOCKED" "Project doctor did not pass."
}
elseif (-not $SkipBuild -and -not $buildPassed) {
    Add-State $states "ReleasePackage" "BLOCKED" "Build did not pass."
}
else {
    try {
        $releaseScript = Join-Path $PSScriptRoot "New-FoARelease.ps1"
        $releaseArgs = @{
            Project = $projectPath
            GameRoot = $(if ($null -ne $environment) { $environment.GameRoot } else { $GameRoot })
            Configuration = $Configuration
            SkipBuild = $true
        }

        if (-not [string]::IsNullOrWhiteSpace($OutputRoot)) {
            $releaseArgs.OutputRoot = $OutputRoot
        }
        if ($Zip) {
            $releaseArgs.Zip = $true
        }

        $package = & $releaseScript @releaseArgs
        Add-State $states "ReleasePackage" "PASSED" $package.PackageDirectory
    }
    catch {
        Add-State $states "ReleasePackage" "FAILED" $_.Exception.Message
    }
}

Add-State $states "RuntimeLoad" "NOT_RUN" "This script does not launch FoA or prove BepInEx loaded the plug-in."
Add-State $states "FeatureValidation" "NOT_RUN" "Gameplay/visual/audio behaviour requires an explicit runtime validation pass."

if ($PersistenceNotApplicable) {
    Add-State $states "PersistenceValidation" "NOT_APPLICABLE" "Caller explicitly marked persistence as outside this mod's claimed scope."
}
else {
    Add-State $states "PersistenceValidation" "NOT_RUN" "Persistence relevance/validation cannot be inferred safely from project structure."
}

$failedCount = @($states | Where-Object { $_.State -eq "FAILED" }).Count
$blockedCount = @($states | Where-Object { $_.State -eq "BLOCKED" }).Count
$partialCount = @($states | Where-Object { $_.State -eq "PARTIAL" }).Count

$staticReady = ($failedCount -eq 0 -and $blockedCount -eq 0)
$releaseReady = $false

if ($failedCount -gt 0) {
    $overall = "FAILED"
}
elseif ($blockedCount -gt 0) {
    $overall = "BLOCKED"
}
else {
    $overall = "PARTIAL"
}

$result = [pscustomobject]@{
    Format = "foa-release-readiness/1"
    Project = $projectPath
    Overall = $overall
    StaticReadyForRuntimeValidation = $staticReady
    ReleaseReady = $releaseReady
    FailedCount = $failedCount
    BlockedCount = $blockedCount
    PartialCount = $partialCount
    Checks = @($states)
    Package = $package
}

if (-not $Quiet) {
    $states | Format-Table Check, State, Detail -Wrap -AutoSize | Out-Host
    Write-Host ""
    Write-Host ("Overall: {0}" -f $overall)
    Write-Host ("Static ready for runtime validation: {0}" -f $staticReady)
    Write-Host "Release ready: False (runtime and feature validation are not performed by this command)."
}

return $result
