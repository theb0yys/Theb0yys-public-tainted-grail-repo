# Harmony Runtime Audit

A read-only Harmony patch-table auditor with separate Mono / BepInEx 5 and IL2CPP / BepInEx 6 hosts over one shared audit core.

It complements source-level symbol inventories by answering a different question: which original methods are actually patched in the running AppDomain, and which Harmony owner IDs installed those patches?

## Build

    # Mono / BepInEx 5
    dotnet build HarmonyRuntimeAudit.csproj -c Release -p:FoAGameRoot="<GameRoot>"

    # IL2CPP / BepInEx 6
    dotnet build HarmonyRuntimeAudit.IL2CPP.csproj -c Release -p:FoAGameRoot="<GameRoot>"

Both projects reference BepInEx and Harmony from the user's own local game installation.

## Configuration

The audit is disabled by default.

The Mono host supports `DelaySeconds` before its snapshot. The IL2CPP host takes its snapshot during `BasePlugin.Load()`; its result therefore reflects the patch table visible at that point in BepInEx load order.


    [General]
    Enabled = true
    DelaySeconds = 2

    [Filter]
    OwnerId =
    ExpectedTargets =

OwnerId filters the report to one exact Harmony owner ID. Empty includes all owners.

ExpectedTargets accepts pipe-separated canonical target identities in the same shape printed by the audit:

    Assembly::Namespace.Type.Method(System.Int32,System.String)

The audit compares exact strings and logs each missing expected target.

## Generate expected targets from a verification report

After running the symbol-anchor verifier, derive the exact live-audit target list instead of maintaining it separately:

    .\Get-FoAHarmonyExpectedTargets.ps1 -VerificationReport ".\.local-research\symbol-anchor-report.json" -Owner "your.plugin.guid" -AsConfigBlock

Only anchors that were verified as exact method signatures and have a canonical live identity are included. Name-only or ambiguous targets are omitted rather than guessed.

## Output

For each included patched method, the plug-in logs:

- canonical target identity;
- all Harmony owners;
- prefix owners;
- postfix owners;
- transpiler owners;
- finalizer owners.

The summary reports total patched methods, included methods, and expected target counts.

## Scope

This tool reads Harmony's live patch table through Harmony.GetAllPatchedMethods and Harmony.GetPatchInfo.

It does not invoke game methods, change patch order, install patches, remove patches, write files, or claim that a patched method executed.

Use the runtime tracer when actual invocation observation is required.
