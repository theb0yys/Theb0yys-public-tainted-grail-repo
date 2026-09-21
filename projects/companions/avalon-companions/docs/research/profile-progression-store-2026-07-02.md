# Research: Companion Profile Progression Store

Date: 2026-07-02
Scope: implement the 0.2.9 progression persistence gate as profile-only storage.
Question: Can Avalon save trust, loyalty, and effective bond profile data without restoring actors or taking save ownership?

## Evidence read

- `mods/avalon-companions/docs/research/progression-persistence-gate-2026-07-01.md`
- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/docs/research/runtime-trust-loyalty-2026-06-28.md`
- `mods/avalon-companions/docs/research/runtime-bond-policy-2026-06-28.md`
- `mods/avalon-companions/src/Framework/CompanionCoreRuntimeProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionTrustProfile.cs`
- `mods/avalon-companions/src/Framework/CompanionBondPolicy.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

0.2.10 may persist approved companion progression profile fields only.

Storage key:

- template GUID,
- review ID.

Storage path:

- `BepInEx/config/kane.tgfoa.avalon-companions/companion-profiles.tsv`

Approved stored columns:

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

## Implementation boundary

The store is loaded into `CompanionCoreRuntimeProfile.TrustProfile` only. It does not:

- restore actors,
- auto-respawn actors,
- re-adopt live actors,
- save or load actor instance IDs,
- save or load location IDs, scene, coordinates, health, movement state, combat state, native action state, or target state,
- set game save ownership,
- change command behavior,
- execute behavior through Avalon Core.

The existing command mode/follow range runtime path remains authoritative for command behavior. Stored mode/range are audit/profile fields only.

## Audit

When a saved profile changes, 0.2.10 writes a `profile-store-save` row to `companion-command-log.csv` with:

- `touchesPersistence=true`,
- `profileOnlyPersistence=true`,
- `noActorRestore=true`,
- `noSaveOwnership=true`,
- `noAutoRespawn=true`,
- `noActorReAdoption=true`,
- `storesLocation=false`,
- `storesHealth=false`,
- `storesAiState=false`,
- `storesTargetState=false`,
- `touchesCommands=false`,
- `touchesMovement=false`,
- `touchesTargeting=false`,
- `touchesActorPersistence=false`,
- `coreExecuted=false`.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- Live DLL deploy and hash check.
- In-game smoke: trigger trust/loyalty changes, confirm `companion-profiles.tsv` contains only approved columns, quit/reload, verify bond values load into the profile, and verify no actor is restored, respawned, re-adopted, or save-owned.
