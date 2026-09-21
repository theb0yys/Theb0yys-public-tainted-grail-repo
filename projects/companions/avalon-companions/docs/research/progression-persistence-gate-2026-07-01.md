# Research: Companion Progression Persistence Gate

Date: 2026-07-01
Scope: decide which trust, loyalty, and bond data may safely survive reloads without restoring actors or taking save ownership.
Question: Can Avalon persist progression profile data later while keeping managed companion actors one-session only?

## Evidence read

- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/docs/research/runtime-trust-loyalty-2026-06-28.md`
- `mods/avalon-companions/docs/research/runtime-bond-policy-2026-06-28.md`
- `mods/avalon-companions/docs/research/bond-ui-clarity-2026-07-01.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/src/Framework/CompanionCoreRuntimeProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionTrustProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionBondPolicy.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Progression persistence may later store only companion profile data keyed by reviewed roster identity. It must not store or restore game actor ownership.

Allowed future persisted fields:

- profile schema version,
- template GUID,
- template name,
- display name,
- review ID,
- `requiresPetComponent`,
- `runtimeApproved`,
- trust score,
- loyalty score,
- trust level,
- loyalty level,
- last trust event,
- last known command mode,
- last known follow range,
- effective bond level.

Blocked fields:

- location ID,
- actor instance ID,
- scene,
- coordinates,
- health,
- NPC working/combat/movement state,
- native action component state,
- target state,
- saved actor ownership,
- auto-respawn request,
- reload restoration request,
- actor re-adoption marker.

## 0.2.9 implementation

Add a read-only `Progression Gate` diagnostic button to the `KeypadPeriod` roster panel. The button writes one `progression-persistence-gate` command-log row that records the approved and blocked storage shape, current selected companion sample values, and the safety flags:

- `noActorRestore=true`,
- `noSaveOwnership=true`,
- `noAutoRespawn=true`,
- `touchesCommands=false`,
- `touchesMovement=false`,
- `touchesTargeting=false`,
- `touchesPersistence=false`,
- `coreExecuted=false`.

This is a gate report only. It does not save trust, loyalty, or bond data yet.

## Not approved

- Saved actors.
- Companion actor restoration after reload.
- Auto-respawn.
- Live actor re-adoption after reload.
- Saving native action state.
- Saving location, scene, coordinates, health, movement, combat, or target state.
- Taming, training, feeding, perk trees, unlocks, inventory, custom AI, target overrides, movement overrides, or Avalon Core-executed behavior.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- Live DLL deploy and hash check.
- In-game smoke: open `KeypadPeriod`, click `Progression Gate`, confirm a `progression-persistence-gate` row appears in `companion-command-log.csv`, and verify the row keeps `touchesPersistence=false`, `noActorRestore=true`, and `noSaveOwnership=true`.
