# Symbol Anchors

These tools turn source-level Harmony target declarations into a deterministic, public-safe inventory and compare that inventory with locally owned Mono game/reference assemblies after a game update.

They do not copy game binaries or decompiled source into the repository.

## Export anchors from source

    .\Export-FoASymbolAnchors.ps1 -Root "..\..\.." -OutputPath ".\symbol-anchors.json"

The extractor recognizes common literal forms including HarmonyPatch with typeof/nameof or string targets, explicit typeof parameter arrays, and literal AccessTools Method, PropertyGetter and PropertySetter targets.

Dynamic or unsupported target construction remains listed under Unresolved rather than being guessed.

The manifest records source identity, runtime lane inferred from repository path, plug-in owner where it can be found, target type/member, and whether an exact parameter signature was present in source.

## Verify after an update

Install ilspycmd locally and point the verifier at a directory containing assemblies from your own game/reference environment:

    .\Test-FoASymbolAnchors.ps1 -Manifest ".\symbol-anchors.json" -AssemblyRoot "<GameRoot>\Fall of Avalon_Data\Managed" -OutputPath ".\symbol-anchor-report.json"

The verifier uses ILSpy entity listing to resolve types without loading those assemblies into the PowerShell process. When the installed ILSpy version supports the member option, explicit method signatures are checked with XML documentation member IDs; otherwise the verifier falls back to a bounded type decompilation/member-name check.

Result values include exact-signature-present, member-name-present, member-present-signature-unresolved, member-present-signature-not-checked, missing-type, ambiguous-type, missing-member, missing-exact-signature and tool-error.

Use FailOnMissing when missing, ambiguous, or tool-error anchors should stop a local validation command.

## Scope

Anchor verification establishes whether expected type/member identities are present in the inspected local assembly set.

It does not establish method semantics, patch installation, runtime execution, downstream behavior, persistence, or broad compatibility.

Use the runtime tracer and Harmony runtime audit for in-process observations.
