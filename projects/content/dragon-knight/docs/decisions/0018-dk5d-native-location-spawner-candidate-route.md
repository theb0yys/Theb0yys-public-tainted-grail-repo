# 0018: DK5D Native LocationSpawner Candidate Route

Status: accepted

Date: 2026-08-04

## Context

The user approved a route override: use native `LocationSpawner` candidate placement for the Dragon Knight instead of continuing the authored HOS placement-pack route.

The selected encounter remains Ancient Cromlech in Horns of the South. The boss root remains `-1795.209|80.243|-2915.928`; `AltarInteract` remains reference evidence only and is not the encounter trigger.

The live Cromlech dump `template-diagnostics.20260803-035222.spawner_refs.csv` does not contain a `LocationSpawner` exactly at the boss root. The closest suitable native hostile source row selected for DK5D is:

| Field | Value |
|---|---|
| Native scene | `CampaignMap_HOS_merged` |
| Host path | `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` |
| Host position | `-1860.15|73.81|-2958.34` |
| Source template | `Spec_EnemyMonster_T1_Grindylow` |
| Source template GUID | `fa79aaa0bff59484dab2cf35c5ea805c` |
| Distance to boss root | `77.83m` |
| Evidence | `template-diagnostics.20260803-035222.spawner_refs.csv:line341` |

## Decision

Dragon Knight `0.2.5` may use the native `LocationSpawner` candidate route:

- hook `LocationSpawner.InitFromAttachment`;
- match only the exact source scene, path, position, and source template GUID above;
- resolve and prepare `Spec_DragonKnight_DK4A` through the proven DK4A template GUID;
- replace that one native candidate array with the Dragon Knight template for deterministic boss placement;
- observe `BaseLocationSpawner.OnLocationSpawned`;
- reject no-save actors;
- move the save-owned native Dragon Knight actor to the Cromlech boss root with `Location.MoveAndRotateTo`;
- let native `BaseLocationSpawner` own cooldown, spawned/killed IDs, save, restore, and corpse lifecycle.

Dragon Knight source must still not call `LocationTemplate.SpawnLocation` directly for production placement.

## Boundaries

DK5D authorizes placement only. It does not authorize movement AI, attacks, hitboxes, damage, two-phase combat, companion protection, item registration, armor registration, Rabbit writes, GOAP dispatch, PlayMaker procedure calls, Blaze control, or live AI runtime registration.
