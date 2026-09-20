[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Mono", "IL2CPP")]
    [string]$Runtime,

    [Parameter(Mandatory = $true)]
    [string]$Name,

    [Parameter(Mandatory = $true)]
    [string]$PluginGuid,

    [string]$Destination,
    [string]$Namespace,
    [string]$AssemblyName,
    [string]$Version = "0.1.0",
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Convert-ToIdentifier {
    param([string]$Value)

    $parts = [regex]::Split($Value, "[^A-Za-z0-9]+") | Where-Object { $_.Length -gt 0 }
    if ($parts.Count -eq 0) {
        throw "Could not derive a valid identifier from '$Value'."
    }

    $identifier = ($parts | ForEach-Object {
        if ($_.Length -eq 1) { $_.ToUpperInvariant() }
        else { $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1) }
    }) -join ""

    if ($identifier -match "^[0-9]") {
        $identifier = "_" + $identifier
    }

    return $identifier
}

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..\..")).Path

if ([string]::IsNullOrWhiteSpace($AssemblyName)) {
    $AssemblyName = Convert-ToIdentifier -Value $Name
}

if ([string]::IsNullOrWhiteSpace($Namespace)) {
    $Namespace = $AssemblyName
}

if ([string]::IsNullOrWhiteSpace($Destination)) {
    $Destination = Join-Path (Get-Location) $AssemblyName
}

if ($Runtime -eq "Mono") {
    $source = Join-Path $repoRoot "templates\mono\harmony\basic"
}
else {
    $source = Join-Path $repoRoot "templates\il2cpp\harmony\basic"
}

if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw "Template source was not found: $source"
}

if (Test-Path -LiteralPath $Destination) {
    $existing = @(Get-ChildItem -LiteralPath $Destination -Force -ErrorAction SilentlyContinue)
    if ($existing.Count -gt 0 -and -not $Force) {
        throw "Destination '$Destination' is not empty. Use -Force only if overwriting scaffold files is intentional."
    }
}
else {
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
}

Get-ChildItem -LiteralPath $source -Force | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $Destination -Recurse -Force
}

$project = Get-ChildItem -LiteralPath $Destination -Filter "*.csproj" | Select-Object -First 1
if ($null -eq $project) {
    throw "Copied template did not contain a .csproj file."
}

[xml]$projectXml = Get-Content -LiteralPath $project.FullName -Raw
$oldNamespace = [string]$projectXml.Project.PropertyGroup.RootNamespace

$projectText = Get-Content -LiteralPath $project.FullName -Raw
$projectText = [regex]::Replace($projectText, "<AssemblyName>[^<]+</AssemblyName>", "<AssemblyName>$AssemblyName</AssemblyName>")
$projectText = [regex]::Replace($projectText, "<RootNamespace>[^<]+</RootNamespace>", "<RootNamespace>$Namespace</RootNamespace>")
Set-Content -LiteralPath $project.FullName -Value $projectText -Encoding UTF8

Get-ChildItem -LiteralPath $Destination -Filter "*.cs" -Recurse | ForEach-Object {
    $text = Get-Content -LiteralPath $_.FullName -Raw
    if (-not [string]::IsNullOrWhiteSpace($oldNamespace)) {
        $text = $text.Replace($oldNamespace, $Namespace)
    }

    $text = [regex]::Replace($text, 'public const string PluginGuid = "[^"]+";', ('public const string PluginGuid = "{0}";' -f $PluginGuid))
    $text = [regex]::Replace($text, 'public const string PluginName = "[^"]+";', ('public const string PluginName = "{0}";' -f $Name))
    $text = [regex]::Replace($text, 'public const string PluginVersion = "[^"]+";', ('public const string PluginVersion = "{0}";' -f $Version))
    Set-Content -LiteralPath $_.FullName -Value $text -Encoding UTF8
}

$newProjectPath = Join-Path $Destination ($AssemblyName + ".csproj")
if ($project.FullName -ne $newProjectPath) {
    Move-Item -LiteralPath $project.FullName -Destination $newProjectPath -Force
}

$readmePath = Join-Path $Destination "README.md"
if (Test-Path -LiteralPath $readmePath -PathType Leaf) {
    $readme = Get-Content -LiteralPath $readmePath -Raw
    $readme = $readme.Replace($project.Name, ($AssemblyName + ".csproj"))
    $readme = $readme.Replace("Mono Harmony Basic", $Name)
    $readme = $readme.Replace("IL2CPP Harmony Basic", $Name)
    Set-Content -LiteralPath $readmePath -Value $readme -Encoding UTF8
}

$result = [pscustomobject]@{
    Runtime = $Runtime
    Name = $Name
    PluginGuid = $PluginGuid
    Version = $Version
    Namespace = $Namespace
    AssemblyName = $AssemblyName
    Destination = (Resolve-Path -LiteralPath $Destination).Path
    Project = $newProjectPath
}

$result | Format-List | Out-Host

Write-Host ""
Write-Host "Next:"
Write-Host ('  .\platform\developer-tools\Build-FoAMod.ps1 -Project "{0}" -GameRoot "C:\Path\To\Tainted Grail FoA"' -f $newProjectPath)

return $result
