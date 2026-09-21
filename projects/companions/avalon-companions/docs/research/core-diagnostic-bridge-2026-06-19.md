# Research: Core Diagnostic Bridge

Date: 2026-06-19
Scope: implement Avalon Companions 0.1.18 as an optional read-only Avalon Core diagnostic bridge with no advanced companion behavior.

## Evidence read

- `docs/engineering-process.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `docs/compatibility-and-versioning.md`
- `docs/decision-records.md`
- `docs/foa-modding-environment.md`
- `docs/research/README.md`
- `Research/Making Tainted Grail The Fall of Avalon Mods-deep-research-report.md`
- `mods/avalon-core/docs/downstream-adapter-integration-guide.md`
- `mods/avalon-core/docs/sample-adapter-descriptor-template.md`
- `mods/avalon-core/docs/decisions/0021-read-only-trust-report-host-api.md`
- `mods/avalon-core/docs/decisions/0027-public-mod-facing-read-only-baseline.md`
- `mods/avalon-core/samples/read-only-consumer-template/README.md`
- `mods/avalon-core/samples/read-only-consumer-template/Plugin.cs`
- `mods/avalon-core/src/Plugin.cs`
- `mods/avalon-core/src/HostTrustReports.cs`
- `mods/avalon-companions/docs/research/core-handoff-design-2026-06-19.md`
- `mods/avalon-companions/docs/decisions/0001-core-handoff-design.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/FoAModManagerBridge.cs`
- `mods/avalon-companions/src/Patches/PetSystemDiagnostics.cs`

## Findings

- Documented:
  - Avalon Core exposes `Plugin.TrustReports` as a copied read-only host snapshot.
  - The approved downstream sample reads `Plugin.TrustReports`, validates status/counter shape, rejects `WouldMutateRuntime=true`, and logs `action=read-only`.
  - The normal Core consumer path uses a hard `[BepInDependency]`, but 0.1.17 explicitly kept Avalon Companions Core-optional until the dependency/load-order gate is selected.
  - Avalon Core `HostTrustReportSnapshot` exposes `Status`, `Reason`, `CandidatePackCount`, `ValidPackCount`, `BlockingIssueCount`, `WarningIssueCount`, `PackSummaryLimit`, `Packs`, `Issues`, and `WouldMutateRuntime`.
  - The existing Companion `FoAModManagerBridge` already uses optional reflection to avoid hard runtime dependencies.
- Inferred:
  - 0.1.18 can safely prove the Core diagnostic bridge with optional reflection instead of a hard project reference.
  - The bridge should run once, cache reflection metadata, retry briefly for load-order uncertainty, then stop after success or fail-closed unavailability.
  - This should not register adapters, query adapter registries, register companion profiles, or change companion runtime behavior.

## Decision

Avalon Companions 0.1.18 may add a config-gated optional Core diagnostic bridge:

- Config: `AvalonCore.EnableDiagnosticBridge`, default `true`.
- Runtime surface: one compact read-only log of `AvalonCore.Plugin.TrustReports` when Avalon Core is installed and exposes the expected host API.
- Dependency posture: no project reference and no BepInEx dependency.
- Failure behavior: fail closed with `action=none` if Avalon Core is absent, the API shape is missing, reflection fails, counters are invalid, status is unknown, or `WouldMutateRuntime=true`.
- Runtime cost: capped startup retry only, no permanent polling.

## Boundary

This does not approve:

- Avalon Core adapter descriptor registration,
- Avalon Core companion-profile registration,
- Avalon Core registry queries,
- hard dependency/load-order migration,
- runtime callbacks,
- custom AI or pathfinding,
- target selection or attack commands,
- squads,
- persistence, reload restoration, respawn, resurrection, or healing,
- humanoid companions or dialogue/story graph behavior.

## Validation needed

- Debug build.
- Local Release build.
- `git diff --check -- mods/avalon-companions`.
- In-game/BepInEx log validation before claiming the bridge loads against a live Avalon Core install.
