# Research: Core Handoff Design

Date: 2026-06-19
Scope: define the Avalon Companions 0.1.17 handoff lane to Avalon Core without adding advanced behavior.

## Evidence read

- `docs/engineering-process.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `docs/compatibility-and-versioning.md`
- `docs/decision-records.md`
- `docs/foa-modding-environment.md`
- `docs/research/README.md`
- `Research/Making Tainted Grail The Fall of Avalon Mods-deep-research-report.md`
- `mods/avalon-core/README.md`
- `mods/avalon-core/docs/research.md`
- `mods/avalon-core/docs/design.md`
- `mods/avalon-core/docs/downstream-adapter-integration-guide.md`
- `mods/avalon-core/docs/sample-adapter-descriptor-template.md`
- `mods/avalon-core/docs/decisions/0005-adapter-registration-capability-contracts.md`
- `mods/avalon-core/docs/research/adapter-registration-capability-contracts-2026-06-15.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/docs/research/framework-contract-extraction-2026-06-18.md`
- `mods/avalon-companions/docs/research/responsive-native-behavior-polish-2026-06-18.md`
- `mods/avalon-companions/src/AvalonCompanions.csproj`
- `mods/avalon-companions/src/Plugin.cs`

## Findings

- Documented:
  - Avalon Core's `adapter-registry` is declarative discovery only. It stores adapter and capability descriptors and does not call downstream adapter code.
  - Avalon Core's `companion-registry` stores planned pet, summon, follower, and squad profiles only and never deploys actors.
  - The downstream adapter guide requires a migration decision before an existing mod registers a real adapter descriptor.
  - Downstream runtime execution, mutation, actor spawning, save-visible behavior, and callbacks remain blocked unless a later design and validation gate approves them.
  - Avalon Companions 0.1.16 extracted internal companion vocabulary for a future Core service contract, but explicitly did not add an Avalon Core dependency.
  - The current companion lane remains the native-safe one-session pet/creature path: explicit summon/swap, native ally marker, native hero-attacker defend prompt, lifecycle guard, command audit logging, and plugin-owned metadata only.
- Inferred:
  - The lowest-risk 0.1.17 step is a design handoff, not a runtime integration.
  - Avalon Companions should not hard-depend on Avalon Core or register an adapter descriptor until the dependency/load-order/rollback path is intentionally implemented and validated.
  - The future descriptor should be diagnostic or planning-only first, with `AvalonCapabilityMode.DiagnosticOnly` or `PlanningOnly`, `AvalonSaveImpact.None`, and fail-closed `action=none` safety gates.
- Unknown:
  - Final companion service contract ID and contract version.
  - Whether the first live Core consumer path should read `Plugin.TrustReports`, register an adapter descriptor, register companion profiles, or do both.
  - Whether Avalon Core's current companion profile fields are sufficient for all existing Avalon Companions roster metadata.
  - BepInEx load-order behavior for Avalon Companions when Avalon Core is installed beside it in the user's local setup.

## Decision

Avalon Companions 0.1.17 is a Core handoff design slice only.

Allowed:

1. Record the companion-to-Core handoff boundary.
2. Align version metadata and docs to 0.1.17.
3. Keep the existing 0.1.16 framework contracts as the companion-local vocabulary.
4. Define a future diagnostic/planning-only Core descriptor shape in docs.
5. Preserve the current optional-dependency posture: Avalon Companions still runs without Avalon Core.

Not allowed:

1. Add an Avalon Core project reference.
2. Add `[BepInDependency]` for Avalon Core.
3. Register an Avalon Core adapter descriptor at runtime.
4. Register companion profiles in Avalon Core at runtime.
5. Add custom AI, target selection, attack commands, squads, persistence, reload restoration, respawn, healing, resurrection, humanoid companions, or dialogue/story graph behavior.
6. Move existing runtime command routing, lifecycle control, native ally behavior, or UI behavior into Avalon Core.

## Future handoff shape

The next implementation gate, if selected, should be a no-runtime-behavior integration with an explicit config gate and fail-closed behavior:

- optional or hard Core dependency decision recorded before code changes,
- read-only `Plugin.TrustReports` diagnostic read if Core is present,
- one diagnostic-only or planning-only descriptor registered from plugin startup,
- stable IDs prefixed by `kane.tgfoa.avalon-companions`,
- capability domain selected from Core's existing enum only after source review,
- contract ID such as `kane.tgfoa.avalon-companions.companion-framework`,
- contract version `1.0`,
- safety gate failure mode `action=none`,
- build validation for Avalon Core and Avalon Companions,
- BepInEx log validation proving Core load order and descriptor registration.

## Validation needed

- Debug build for Avalon Companions.
- `git diff --check` scoped to `mods/avalon-companions`.
- No in-game smoke is required for the design-only slice unless behavior code changes beyond version metadata.
