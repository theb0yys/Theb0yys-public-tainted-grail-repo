param(
    [Parameter(Mandatory = $true)]
    [string] $DumpPath,

    [string] $OutputCsvPath,

    [string] $OutputMarkdownPath
)

$ErrorActionPreference = 'Stop'

function Test-Truthy([string] $Value) {
    return $Value -match '^(?i:true|1|yes)$'
}

function Test-HumanNameToken([string] $Name) {
    return $Name -match '(?i)(^|_)(NPC|Human|Guard|Peasant|Outlaw|Bandit|Highwayman|Knight|Kamelot|DalRiata|Druid|Wyrdhunter|Warrior|Soldier|Mercenary|Thug|Raider|Pict|Cultist|Priest|Blacksmith|Trader|Vendor|Beggar|Villager|Citizen|Generic)($|_)'
}

function Test-CreatureNameToken([string] $Name) {
    return $Name -match '(?i)(Animal|Monster|Zombie|Skeleton|Pet|Wolf|Bear|Boar|Cow|Deer|Pig|Horse|Rat|Bullrat|CorpseEater|Redcap|Flamegobbler|Grindylow|Wyrdspirit|Sharg|Ogre|Drowner|Floatling|Tadpole)'
}

function Get-RiskFlags($Template, [int] $SpawnerRefCount) {
    $flags = New-Object System.Collections.Generic.List[string]
    $name = [string] $Template.templateName

    if (Test-Truthy $Template.isAbstract) { $flags.Add('abstract') }
    if (Test-Truthy $Template.npcUnique) { $flags.Add('unique') }
    if ($SpawnerRefCount -le 0) { $flags.Add('no-spawner-evidence') }
    if ($name -match '(?i)(Quest|Story|Dialog|Dialogue|Cutscene|Tutorial|Challenge)') { $flags.Add('quest-story-risk') }
    if ($name -match '(?i)(Boss|Champion|Elite|Named)') { $flags.Add('boss-named-risk') }
    if ($name -match '(?i)(Guard|Kamelot|DalRiata)') { $flags.Add('guard-faction-risk') }
    if ($name -match '(?i)(Peasant|Villager|Citizen|Beggar|Trader|Vendor|Blacksmith)') { $flags.Add('civilian-risk') }
    if ($name -match '(?i)(Enemy|Outlaw|Bandit|Highwayman|Raider|Thug|Cultist)') { $flags.Add('hostile-risk') }

    if ($flags.Count -eq 0) {
        return 'none'
    }

    return ($flags | Sort-Object -Unique) -join '|'
}

function Get-ReviewStatus($Template, [int] $SpawnerRefCount) {
    if (Test-Truthy $Template.isAbstract) {
        return 'BlockedAbstract'
    }

    if (Test-Truthy $Template.npcUnique) {
        return 'BlockedUniqueOrStory'
    }

    if ($SpawnerRefCount -le 0) {
        return 'NeedsSpawnerEvidence'
    }

    return 'ReviewQueueNonUniqueSpawnerBacked'
}

function Get-MinNumber($Rows, [string] $PropertyName) {
    $values = @(
        foreach ($row in $Rows) {
            $value = $row.$PropertyName
            $parsed = 0.0
            if ([double]::TryParse([string] $value, [Globalization.NumberStyles]::Float, [Globalization.CultureInfo]::InvariantCulture, [ref] $parsed)) {
                $parsed
            }
        }
    )

    if ($values.Count -eq 0) {
        return ''
    }

    return ($values | Measure-Object -Minimum).Minimum.ToString('0.###', [Globalization.CultureInfo]::InvariantCulture)
}

function First-NonEmpty($Rows, [string] $PropertyName) {
    foreach ($row in $Rows) {
        $value = [string] $row.$PropertyName
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }
    }

    return ''
}

$dump = Resolve-Path -LiteralPath $DumpPath
$templatesPath = Join-Path $dump.Path 'templates.csv'
$spawnerRefsPath = Join-Path $dump.Path 'spawner_refs.csv'
$snapshotPath = Join-Path $dump.Path 'runtime_snapshot.txt'

if (-not (Test-Path -LiteralPath $templatesPath)) {
    throw "Missing templates.csv in $($dump.Path)"
}

if (-not (Test-Path -LiteralPath $spawnerRefsPath)) {
    throw "Missing spawner_refs.csv in $($dump.Path)"
}

if ([string]::IsNullOrWhiteSpace($OutputCsvPath)) {
    $OutputCsvPath = Join-Path 'mods/avalon-human-companions/docs/generated' "human-npc-diagnostic-map-$($dump.ProviderPath | Split-Path -Leaf).csv"
}

if ([string]::IsNullOrWhiteSpace($OutputMarkdownPath)) {
    $OutputMarkdownPath = [IO.Path]::ChangeExtension($OutputCsvPath, '.md')
}

$templates = Import-Csv -LiteralPath $templatesPath
$spawnerRefs = Import-Csv -LiteralPath $spawnerRefsPath
$spawnerByGuid = $spawnerRefs | Where-Object { -not [string]::IsNullOrWhiteSpace($_.templateGuid) } | Group-Object templateGuid -AsHashTable -AsString
$rows = New-Object System.Collections.Generic.List[object]

foreach ($template in $templates) {
    if (-not (Test-Truthy $template.actorLike)) {
        continue
    }

    if ([string]::IsNullOrWhiteSpace($template.npcAttachment) -or $template.npcAttachment -in @('<none>', '<not-applicable>')) {
        continue
    }

    $guid = [string] $template.templateGuid
    $name = [string] $template.templateName
    $spawns = if ($spawnerByGuid.ContainsKey($guid)) { @($spawnerByGuid[$guid]) } else { @() }
    $humanFaction = @($spawns | Where-Object { $_.details -match '(?i)Faction_Humans|Faction_Human' }).Count -gt 0
    $humanName = Test-HumanNameToken $name
    $creatureName = Test-CreatureNameToken $name

    if (-not $humanFaction -and -not $humanName) {
        continue
    }

    if ($creatureName -and -not $humanFaction -and -not $humanName) {
        continue
    }

    $humanEvidence = @()
    if ($humanFaction) { $humanEvidence += 'spawner-faction-human' }
    if ($humanName) { $humanEvidence += 'name-token-human' }
    if ($creatureName) { $humanEvidence += 'creature-name-risk' }

    $spawnerRefCount = $spawns.Count
    $reviewStatus = Get-ReviewStatus $template $spawnerRefCount
    $riskFlags = Get-RiskFlags $template $spawnerRefCount

    $rows.Add([pscustomobject]@{
        templateGuid = $guid
        templateName = $name
        templateType = $template.templateType
        isAbstract = $template.isAbstract
        actorCategory = $template.actorCategory
        npcUnique = $template.npcUnique
        npcAttachment = $template.npcAttachment
        humanEvidence = ($humanEvidence | Sort-Object -Unique) -join '|'
        spawnerRefCount = $spawnerRefCount
        nearestReferenceDistanceToHero = Get-MinNumber $spawns 'referenceDistanceToHero'
        nearestHostPath = First-NonEmpty $spawns 'hostPath'
        scene = First-NonEmpty $spawns 'scene'
        reviewStatus = $reviewStatus
        riskFlags = $riskFlags
        safeSpawnCandidate = 'false'
        rosterApproved = 'false'
        behaviorApproved = 'false'
        persistenceApproved = 'false'
        notes = 'diagnostic review only; not approved for companion roster or behavior'
    })
}

$orderedRows = $rows | Sort-Object -Property `
    @{ Expression = { if ($_.reviewStatus -eq 'ReviewQueueNonUniqueSpawnerBacked') { 0 } elseif ($_.reviewStatus -eq 'NeedsSpawnerEvidence') { 1 } else { 2 } } },
    @{ Expression = 'templateName' }

$outputCsvFull = if ([IO.Path]::IsPathRooted($OutputCsvPath)) { $OutputCsvPath } else { Join-Path (Get-Location) $OutputCsvPath }
$outputMdFull = if ([IO.Path]::IsPathRooted($OutputMarkdownPath)) { $OutputMarkdownPath } else { Join-Path (Get-Location) $OutputMarkdownPath }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputCsvFull) | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputMdFull) | Out-Null

$orderedRows | Export-Csv -LiteralPath $outputCsvFull -NoTypeInformation

$statusCounts = $orderedRows | Group-Object reviewStatus | Sort-Object Name
$riskCounts = $orderedRows |
    ForEach-Object { $_.riskFlags -split '\|' } |
    Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
    Group-Object |
    Sort-Object -Property @{ Expression = 'Count'; Descending = $true }, @{ Expression = 'Name'; Descending = $false }

$snapshotLines = if (Test-Path -LiteralPath $snapshotPath) { Get-Content -LiteralPath $snapshotPath -First 35 } else { @('runtime_snapshot.txt missing') }
$topRows = $orderedRows | Select-Object -First 40

$markdown = New-Object System.Text.StringBuilder
[void] $markdown.AppendLine("# Human NPC Diagnostic Map")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("- Generated: $(Get-Date -Format o)")
[void] $markdown.AppendLine("- Source dump: $($dump.Path)")
[void] $markdown.AppendLine("- Output CSV: $OutputCsvPath")
[void] $markdown.AppendLine("- Safety: diagnostic review only; no roster, behavior, spawn, or persistence approval.")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("## Runtime Context")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine('```text')
foreach ($line in $snapshotLines) {
    [void] $markdown.AppendLine($line)
}
[void] $markdown.AppendLine('```')
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("## Counts")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("- Human diagnostic rows: $($orderedRows.Count)")
foreach ($count in $statusCounts) {
    [void] $markdown.AppendLine("- $($count.Name): $($count.Count)")
}
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("## Risk Flags")
[void] $markdown.AppendLine()
foreach ($count in $riskCounts | Select-Object -First 20) {
    [void] $markdown.AppendLine("- $($count.Name): $($count.Count)")
}
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("## First Review Rows")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("| Status | Template | GUID | Spawner refs | Unique | Risk |")
[void] $markdown.AppendLine("| --- | --- | --- | ---: | --- | --- |")
foreach ($row in $topRows) {
    $templateName = ([string] $row.templateName).Replace('|', '\|')
    $risk = ([string] $row.riskFlags).Replace('|', '<br>')
    [void] $markdown.AppendLine("| $($row.reviewStatus) | $templateName | $($row.templateGuid) | $($row.spawnerRefCount) | $($row.npcUnique) | $risk |")
}
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("## Boundary")
[void] $markdown.AppendLine()
[void] $markdown.AppendLine("Rows in this file are candidates for manual research only. Before any row can become a human companion candidate it still needs non-unique proof, placement/context review, behavior fit review, density/performance review, lifecycle validation, save/load validation, and explicit implementation approval.")

$markdown.ToString() | Set-Content -LiteralPath $outputMdFull -Encoding UTF8

Write-Output "Wrote $($orderedRows.Count) human diagnostic rows."
Write-Output "CSV: $outputCsvFull"
Write-Output "Markdown: $outputMdFull"
