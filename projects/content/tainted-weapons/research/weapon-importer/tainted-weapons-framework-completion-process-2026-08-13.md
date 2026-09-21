# Tainted Weapons Framework Completion Programme — 2026-08-13

## Control

- Status: current task-specific research-backed completion programme
- Owner: Weapon Importer / Tainted Weapons
- Implementation owner: `mods/tainted-weapons/`
- Parent: `research-backed-engineering-process-2026-08-10.md`
- Baseline: `2e32cddef8c66c511fc58f6b5dae8c56ab6812db`
- Tainted Weapons: `0.3.33`
- Programme state: `RESEARCH_REQUIRED`
- Feature expansion: `PAUSED_FOR_CORRECTIONS`
- Release: `BLOCKED`

## Goal

Finish the rigid-weapon framework through the existing evidence-backed gate system while preserving native item/equip ownership and registered-only Drake presentation. No architectural rewrite is authorised by the coding review.

## Current admission

Current main and source/project versions are pinned and agree at `0.3.33`. Repository conflict-marker search passed. New runtime promotion still requires a fresh clean current-main environment fingerprint because the existing v2 receipt is bound to an older dirty source state.

```text
repository source identity = PASSED
current source version identity = PASSED
clean current-main runtime provenance = REQUIRED
runtime promotion = BLOCKED until fresh admission
```

## Research Units

### RU-TW-CORR-001 — Provider ownership

Current defect: one asset release can collapse a multi-consumer count to zero.

State: `SUPPORTED_DEFECT / READY_FOR_PLANNING`.

Pass condition: two loads, first release leaves one live consumer and blocks bundle release, second release reaches zero exactly once.

### RU-TW-CORR-002 — Lifecycle evidence integrity

Current defect: L8/L11/L12 can accept success-shaped event text instead of independent measurement.

State: `SUPPORTED_DEFECT / READY_FOR_PLANNING`.

Pass condition: measured before/after state plus negative fixtures that deliberately create stale/residual state and fail the gate.

### RU-TW-CORR-003 — Perspective-specific mutation

Current source does not require positive FPP ownership for registered presentation correction. Exact current-build TPP reachability remains a separate evidence question.

State: `RESEARCH_REQUIRED` for native reachability; fail-closed source containment is ready for planning.

Pass condition: TPP/unknown owner never receives FPP-only presentation state.

### RU-TW-CORR-004 — FPP contract lifetime and layer policy

Current defects: cached FPP contract lacks sufficient scene/camera/perspective lifetime identity; first camera-visible layer is not native weapon-layer authority.

State: `RESEARCH_REQUIRED` for native layer authority; invalidation design is ready for planning.

### RU-TW-CORR-005 — Placement maths and bounds proof

Current defects: fallback depth ignores calculated width/height requirements; post-frustum evidence can follow a bounds widening operation.

State: `SUPPORTED_DEFECT / READY_FOR_PLANNING` for maths/proof separation; native conservative-bounds policy remains research-required.

### RU-TW-NATIVE-001 — Drake loading/components contract

Question: are the merged private loading-start/bitmask interventions necessary and what invariants govern them?

State: `RESEARCH_REQUIRED`.

Required evidence: exact current-build loading manager/components manager/bitmask/start/completion/unload contract, then controlled failure/lifecycle evidence.

No further private-state expansion until resolved.

### RU-TW-ID-001 — Identity/compatibility v2

Current issue: definition identity is path-sensitive and omits important package-content/source/environment compatibility inputs.

State: `DESIGN_RESEARCH_REQUIRED` before persistent/public contract changes.

### RU-TW-CLONE-001 — Semantic field profile

Current state: whole-object clone retained; structural/reference verification exists; comprehensive semantic-equivalence proof remains blocked.

Required output: approved field-disposition manifest plus deterministic source-template/source-prefab profiles.

### RU-TW-API-001 — Public API/consumer compatibility

Question: what typed/versioned capability contract replaces reflection-shape fragility while preserving current consumers?

State: `DESIGN_RESEARCH_REQUIRED` before public API promotion.

### RU-TW-PERSIST-001 — Startup/persistence/missing-package

Question: is custom template registration available before saved template lookup, and what non-destructive behaviour applies when content is absent or incompatible?

State: `BLOCKED / RESEARCH_AND_RUNTIME_REQUIRED`.

## Completion waves

### C0 — Admission and freeze

Freeze current `0.3.33`; pause feature expansion; capture fresh clean current-main runtime provenance before new runtime promotion; bind later build/deploy/runtime evidence to exact corrected source and DLL.

### C1 — Deterministic corrections

After a bounded implementation plan:

- correct provider reference counting;
- replace L8/L11/L12 self-certification with measured state and negative fixtures;
- correct fallback placement maths with deterministic tests;
- remove stale hard-coded validation-version assumptions and distinguish unknown from zero.

Proof: V0/V1 plus relevant V2 source checks.

No new presentation features, private Drake expansion, identity migration or persistence work in C1.

### C2 — Perspective containment

- positive FPP ownership required for FPP-only correction;
- TPP/unknown fail closed;
- FPP contract carries scene/camera/perspective generation and invalidates on owner change/dispose;
- remove arbitrary first-visible-layer fallback;
- restrict layer changes to proven presentation ownership;
- keep raw geometry proof independent from any culling policy.

Proof: V0/V2, then V6/V7 differential runtime.

### C3 — Drake native-contract decision

Resolve RU-TW-NATIVE-001, then decide whether merged private loading/bitmask work is retained, reduced or removed. Record counter/handle/bitmask ownership, partial load states, double-start protection, completion/unload order and reset policy.

### C4 — Durable contracts

Complete identity/compatibility v2, semantic field/source profiles, typed/versioned API and current-consumer migration plan.

### C5 — Build/runtime negative controls

Validate custom FPP/TPP, both perspective transitions, scene/camera changes, weapon swaps, duplicate instances, two different weapons, vanilla controls, previews, missing bundle fallback, one-of-two consumer release, partial Drake failures and raw bounds proof.

Proof: V3/V4/V6/V7/V8. One visible FPP weapon is insufficient.

### C6 — Persistence safety

Using disposable copied saves only, prove restart, copied-save, framework/package absence, restoration, incompatible definition/source profile, presentation failure with valid item identity and multi-pack restore.

Proof: V9 plus startup/static evidence.

### C7 — Multi-pack, compatibility and release

Require at least two independent consumers/packs, collision/isolation matrix, exact supported Mono tuple, package/release provenance, independent review and human promotion. IL2CPP remains unsupported unless separately proven.

Proof: V10/V11.

## Gate rule

Every correction follows:

```text
claim/evidence
 -> bounded planning
 -> APPROVED_FOR_IMPLEMENTATION for exact scope
 -> dedicated correction branch
 -> smallest implementation
 -> self-review
 -> independent review
 -> claim-fit validation
 -> owner gate
 -> promotion only for passed scope
```

If planning returns `RESEARCH_REQUIRED`, source mutation for that unresolved claim does not proceed.

## First execution unit — TW-FINISH-C1A

Purpose: remove deterministic correctness/evidence defects before further presentation work.

Planning targets:

- `TaintedWeaponProviderOwnerLedger.cs`
- `TaintedWeaponProviderLifecycleHarness.cs`
- only fixture-producing code required for measured evidence
- only pure placement-math policy required for deterministic correction/tests
- Tainted Weapons validation tooling where version/unknown-state assumptions are stale

Non-goals: no new presentation features, no private Drake expansion, no registrar change, no identity/API migration, no persistence work, no release claim.

Entry state: `READY_FOR_PLANNING` after programme review.

## Parallel next research lane

`RU-TW-NATIVE-001`: establish the exact current-build Drake loading, completion, bitmask and unload contracts needed to decide whether the merged `0.3.33` private intervention should be retained, reduced or removed.

## Definition of finished

```text
C0 PASSED
C1 PASSED
C2 PASSED
C3 PASSED
C4 PASSED
C5 PASSED
C6 PASSED
C7 PASSED
independent review PASSED
human promotion complete
```

Until then, architecture is retained, work is bounded by the current wave, and release remains blocked.
