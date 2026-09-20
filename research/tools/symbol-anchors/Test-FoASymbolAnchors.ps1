[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Manifest,

    [Parameter(Mandatory = $true)]
    [string]$AssemblyRoot,

    [string]$IlSpyCmd = "ilspycmd",

    [string]$OutputPath,

    [switch]$FailOnMissing
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Normalize-SourceTypeName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $null
    }

    $value = $Name.Trim()
    $value = $value -replace '^global::', ''

    $aliases = @{
        "bool" = "System.Boolean"
        "byte" = "System.Byte"
        "sbyte" = "System.SByte"
        "short" = "System.Int16"
        "ushort" = "System.UInt16"
        "int" = "System.Int32"
        "uint" = "System.UInt32"
        "long" = "System.Int64"
        "ulong" = "System.UInt64"
        "float" = "System.Single"
        "double" = "System.Double"
        "decimal" = "System.Decimal"
        "char" = "System.Char"
        "string" = "System.String"
        "object" = "System.Object"
    }

    if ($aliases.ContainsKey($value)) {
        return $aliases[$value]
    }

    return $value
}

function Get-SimpleTypeName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return $null
    }

    $value = $Name.Replace('+', '.')
    $lastDot = $value.LastIndexOf('.')
    if ($lastDot -ge 0) {
        $value = $value.Substring($lastDot + 1)
    }

    $tick = $value.IndexOf([char]96)
    if ($tick -ge 0) {
        $value = $value.Substring(0, $tick)
    }

    return $value
}

function Invoke-IlSpy {
    param([string[]]$Arguments)

    $captured = @(& $script:IlSpyExecutable @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    $text = ($captured | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine

    return [pscustomobject]@{
        ExitCode = $exitCode
        StdOut = $text
        StdErr = if ($exitCode -eq 0) { "" } else { $text }
    }
}

function Get-TypeInventory {
    param($Assemblies)

    $records = New-Object System.Collections.Generic.List[object]

    foreach ($assembly in $Assemblies) {
        foreach ($entityType in @("c", "i", "s", "d", "e")) {
            $listed = Invoke-IlSpy -Arguments @("-l", $entityType, $assembly.FullName)
            if ($listed.ExitCode -ne 0) {
                continue
            }

            foreach ($line in ($listed.StdOut -split '\r?\n')) {
                $trimmed = $line.Trim()
                if ([string]::IsNullOrWhiteSpace($trimmed)) {
                    continue
                }

                $match = [regex]::Match($trimmed, '^(Class|Interface|Struct|Delegate|Enum)\s+(.+)$')
                if (-not $match.Success) {
                    continue
                }

                $fullName = $match.Groups[2].Value.Trim()
                $records.Add([pscustomobject]@{
                    AssemblyPath = $assembly.FullName
                    AssemblyFile = $assembly.Name
                    Kind = $match.Groups[1].Value
                    FullName = $fullName
                    SimpleName = Get-SimpleTypeName -Name $fullName
                })
            }
        }
    }

    return $records.ToArray()
}

function Resolve-TypeRecord {
    param([string]$Expression, $Inventory)

    $normalized = Normalize-SourceTypeName -Name $Expression
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return [pscustomobject]@{ State = "invalid-type"; Matches = @() }
    }

    $exact = @(
        $Inventory | Where-Object {
            $_.FullName -eq $normalized -or
            $_.FullName.Replace('+', '.') -eq $normalized.Replace('+', '.')
        }
    )

    if ($exact.Count -eq 1) {
        return [pscustomobject]@{ State = "resolved"; Matches = @($exact) }
    }

    if ($exact.Count -gt 1) {
        return [pscustomobject]@{ State = "ambiguous-type"; Matches = @($exact) }
    }

    $simpleName = Get-SimpleTypeName -Name $normalized
    $simple = @($Inventory | Where-Object { $_.SimpleName -eq $simpleName })

    if ($simple.Count -eq 1) {
        return [pscustomobject]@{ State = "resolved"; Matches = @($simple) }
    }

    if ($simple.Count -gt 1) {
        return [pscustomobject]@{ State = "ambiguous-type"; Matches = @($simple) }
    }

    return [pscustomobject]@{ State = "missing-type"; Matches = @() }
}

function Resolve-ParameterName {
    param([string]$Expression, $Inventory)

    $normalized = Normalize-SourceTypeName -Name $Expression
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return $null
    }

    if ($normalized.EndsWith("[]")) {
        $inner = Resolve-ParameterName -Expression $normalized.Substring(0, $normalized.Length - 2) -Inventory $Inventory
        if ($null -eq $inner) {
            return $null
        }
        return $inner + "[]"
    }

    foreach ($prefix in @("ref ", "out ", "in ")) {
        if ($normalized.StartsWith($prefix, [System.StringComparison]::Ordinal)) {
            $inner = Resolve-ParameterName -Expression $normalized.Substring($prefix.Length) -Inventory $Inventory
            if ($null -eq $inner) {
                return $null
            }
            return $inner + "@"
        }
    }

    if ($normalized.StartsWith("System.", [System.StringComparison]::Ordinal)) {
        return $normalized
    }

    $resolved = Resolve-TypeRecord -Expression $normalized -Inventory $Inventory
    if ($resolved.State -eq "resolved") {
        return [string]$resolved.Matches[0].FullName
    }

    return $null
}

function Test-MemberByType {
    param(
        [string]$AssemblyPath,
        [string]$FullTypeName,
        [string]$MemberName,
        [string]$MemberKind
    )

    $result = Invoke-IlSpy -Arguments @("-t", $FullTypeName, $AssemblyPath)
    if ($result.ExitCode -ne 0) {
        return [pscustomobject]@{
            Found = $false
            ToolError = $true
            Detail = $result.StdErr.Trim()
        }
    }

    $escaped = [regex]::Escape($MemberName)

    if ($MemberKind -eq "property-getter" -or $MemberKind -eq "property-setter") {
        $found = [regex]::IsMatch($result.StdOut, "(?m)\b$escaped\b\s*(\{|=>)")
        return [pscustomobject]@{
            Found = $found
            ToolError = $false
            Detail = if ($found) { "property-present" } else { "property-not-found" }
        }
    }

    $found = [regex]::IsMatch($result.StdOut, "(?m)\b$escaped\b(?:<[^>]+>)?\s*\(")
    return [pscustomobject]@{
        Found = $found
        ToolError = $false
        Detail = if ($found) { "member-name-present" } else { "member-name-not-found" }
    }
}

$manifestPath = (Resolve-Path -LiteralPath $Manifest).Path
$assemblyRootPath = (Resolve-Path -LiteralPath $AssemblyRoot).Path

$command = Get-Command $IlSpyCmd -ErrorAction Stop
$script:IlSpyExecutable = $command.Source

$help = Invoke-IlSpy -Arguments @("--help")
$memberLookupSupported = ($help.StdOut -match '--member') -or ($help.StdErr -match '--member')

$manifestJson = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([string]$manifestJson.Format -ne "foa-symbol-anchors/1") {
    throw "Unsupported manifest format: $($manifestJson.Format)"
}

$assemblies = @(
    Get-ChildItem -LiteralPath $assemblyRootPath -Filter "*.dll" -File -Recurse
)

if ($assemblies.Count -eq 0) {
    throw "No DLLs were found under AssemblyRoot."
}

$inventory = @(Get-TypeInventory -Assemblies $assemblies)
Write-Host ("Types inventoried: {0}" -f $inventory.Count)
if ($inventory.Count -gt 0) {
    $inventory | Select-Object -First 5 | Format-Table AssemblyFile, Kind, FullName, SimpleName -AutoSize | Out-Host
}
$results = New-Object System.Collections.Generic.List[object]

foreach ($anchor in @($manifestJson.Anchors)) {
    $typeResolution = Resolve-TypeRecord -Expression ([string]$anchor.TypeExpression) -Inventory $inventory

    if ($typeResolution.State -ne "resolved") {
        $results.Add([pscustomobject]@{
            Source = [string]$anchor.Source
            Runtime = [string]$anchor.Runtime
            TypeExpression = [string]$anchor.TypeExpression
            MemberName = [string]$anchor.MemberName
            SignatureExplicit = [bool]$anchor.SignatureExplicit
            State = $typeResolution.State
            AssemblyFile = $null
            ResolvedType = $null
            Detail = if ($typeResolution.Matches.Count -gt 0) { (@($typeResolution.Matches.FullName) -join " | ") } else { $null }
        })
        continue
    }

    $typeRecord = $typeResolution.Matches[0]
    $resolvedType = [string]$typeRecord.FullName
    $assemblyPath = [string]$typeRecord.AssemblyPath
    $state = $null
    $detail = $null

    if ([bool]$anchor.SignatureExplicit -and [string]$anchor.MemberKind -eq "method" -and $memberLookupSupported) {
        $parameterNames = New-Object System.Collections.Generic.List[string]
        $signatureResolvable = $true

        foreach ($parameterExpression in @($anchor.ParameterTypeExpressions)) {
            $resolvedParameter = Resolve-ParameterName -Expression ([string]$parameterExpression) -Inventory $inventory
            if ([string]::IsNullOrWhiteSpace($resolvedParameter)) {
                $signatureResolvable = $false
                break
            }
            $parameterNames.Add($resolvedParameter)
        }

        if ($signatureResolvable) {
            $docId = "M:$resolvedType.$([string]$anchor.MemberName)"
            if ($parameterNames.Count -gt 0) {
                $docId += "(" + ($parameterNames -join ",") + ")"
            }

            $member = Invoke-IlSpy -Arguments @("-m", $docId, $assemblyPath)
            if ($member.ExitCode -eq 0 -and -not [string]::IsNullOrWhiteSpace($member.StdOut)) {
                $state = "exact-signature-present"
                $detail = $docId
            }
            else {
                $state = "missing-exact-signature"
                $detail = $docId
            }
        }
        else {
            $fallback = Test-MemberByType -AssemblyPath $assemblyPath -FullTypeName $resolvedType -MemberName ([string]$anchor.MemberName) -MemberKind ([string]$anchor.MemberKind)
            if ($fallback.ToolError) {
                $state = "tool-error"
                $detail = $fallback.Detail
            }
            elseif ($fallback.Found) {
                $state = "member-present-signature-unresolved"
                $detail = "Parameter source expressions could not all be resolved to metadata names."
            }
            else {
                $state = "missing-member"
                $detail = $fallback.Detail
            }
        }
    }
    else {
        $fallback = Test-MemberByType -AssemblyPath $assemblyPath -FullTypeName $resolvedType -MemberName ([string]$anchor.MemberName) -MemberKind ([string]$anchor.MemberKind)

        if ($fallback.ToolError) {
            $state = "tool-error"
            $detail = $fallback.Detail
        }
        elseif ($fallback.Found) {
            if ([bool]$anchor.SignatureExplicit) {
                $state = "member-present-signature-not-checked"
            }
            else {
                $state = "member-name-present"
            }
            $detail = $fallback.Detail
        }
        else {
            $state = "missing-member"
            $detail = $fallback.Detail
        }
    }

    $results.Add([pscustomobject]@{
        Source = [string]$anchor.Source
        Runtime = [string]$anchor.Runtime
        TypeExpression = [string]$anchor.TypeExpression
        MemberName = [string]$anchor.MemberName
        SignatureExplicit = [bool]$anchor.SignatureExplicit
        State = $state
        AssemblyFile = [string]$typeRecord.AssemblyFile
        ResolvedType = $resolvedType
        Detail = $detail
    })
}

$missingStates = @(
    "missing-type",
    "ambiguous-type",
    "missing-member",
    "missing-exact-signature",
    "invalid-type",
    "tool-error"
)

$missing = @($results | Where-Object { $_.State -in $missingStates })
$uncertain = @(
    $results | Where-Object {
        $_.State -in @(
            "member-present-signature-unresolved",
            "member-present-signature-not-checked"
        )
    }
)

$report = [pscustomobject]@{
    Format = "foa-symbol-anchor-verification/1"
    Manifest = [System.IO.Path]::GetFileName($manifestPath)
    AssemblyRoot = "<ASSEMBLY_ROOT>"
    IlSpyMemberLookup = $memberLookupSupported
    AssemblyCount = $assemblies.Count
    TypeCount = $inventory.Count
    AnchorCount = $results.Count
    MissingOrAmbiguousCount = $missing.Count
    SignatureUncertainCount = $uncertain.Count
    Results = $results.ToArray()
    Limitation = "Identity presence does not establish runtime execution, semantic equivalence, compatibility, persistence, or behavior."
}

if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }

    $report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding UTF8
}

$results | Format-Table State, AssemblyFile, ResolvedType, MemberName, Source -AutoSize | Out-Host
Write-Host ""
Write-Host ("Anchors checked: {0}" -f $results.Count)
Write-Host ("Missing/ambiguous/tool-error: {0}" -f $missing.Count)
Write-Host ("Signature uncertain: {0}" -f $uncertain.Count)

if ($FailOnMissing -and $missing.Count -gt 0) {
    throw "Symbol anchor verification found $($missing.Count) missing, ambiguous, or tool-error anchor(s)."
}

return $report
