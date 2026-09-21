# Decision 0012: DK4 Live Actor Diagnostic Source

Date: 2026-08-02

## Status

Accepted for source implementation and fixture validation. Live actor pass remains pending until FoA loads the deployed Dragon Knight `0.2.2` build and the DK4 diagnostic is armed in config.

## Decision

Implement the DK4 live actor observation diagnostic in Dragon Knight source as a default-off, F7/F6, no-save proof path.

The diagnostic may create one Dragon Knight actor from the proved DK4A `LocationTemplate` and observe its live `Location.ID`, derive `ActorId=foa.location:<Location.ID>`, acquire one Dragon Knight-owned diagnostic lease, prove the exact Cromlech target `Location.ID`, and release the actor through `Location.Discard()`.

## Evidence

- DK4 source packet authorizes the exact source files, config keys, F7/F6 activation, actor/target contracts, 36 fixtures, marker lines, build/deploy plan, and stop conditions.
- DK4A template proof supplies `NPCTemplate_DragonKnight_DK4A` GUID `00f608ee051b57748a6a9ed8dae28678` and `Spec_DragonKnight_DK4A` GUID `d7b09116519f7564593be62781bee3db`.
- Cromlech geometry proof supplies `AltarInteract` Location.ID `CM_Stonehenge_0_2258891627046429048_1` as measurement/scene proof only, with explicit rejection as encounter trigger.
- The Dragon Knight design locks Ancient Cromlech / Stonehenge, outer wake `25m`, inner fight ring `5m`, soft leash `22m`, hard leash `25m`, and forbids shrine/prompt/discovery activation as the trigger.

## Boundaries

- `DK4Diagnostic.Enabled=false`, `DK4Diagnostic.KillSwitch=true`, and `DK4Diagnostic.AllowNativeSpawn=false` remain source defaults.
- F7 is the only activation route; F6 and plugin shutdown release the active actor.
- The diagnostic must not register Dragon Knight with live Avalon AI Runtime.
- The diagnostic must not write Rabbit state, run GOAP goals/actions, call PlayMaker, move the actor, attack, start phase combat, protect companions, create items, write saves, or add roaming/population.
- `AltarInteract` is a target proof source only; it is not an encounter trigger and is never interacted with.
