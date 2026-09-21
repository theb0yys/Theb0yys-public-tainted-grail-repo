# DK5D Native LocationSpawner Candidate Placement

## Inputs read

- User route override: use native `LocationSpawner` candidate placement for Dragon Knight.
- `mods/avalon-awakened/docs/research/ci5g-native-regional-population-correction-2026-07-16.md`: proves the native candidate pattern used by Living Avalon, where `LocationSpawner.InitFromAttachment` can be patched and `BaseLocationSpawner` remains lifecycle/save owner.
- `.work/merlin-workshop-v1.1.0-source/Assets/Code/Main/Locations/Spawners/LocationSpawner.cs`: `InitFromAttachment` copies `spec.LocationsToSpawn` into private `_locationsToSpawn`; `SpawnPrefabInternal` selects from `_locationsToSpawn`, calls `LocationTemplate.SpawnLocation`, and then calls `OnLocationSpawned`.
- `.work/merlin-workshop-v1.1.0-source/Assets/Code/Main/Locations/Spawners/BaseLocationSpawner.cs`: owns spawned location IDs, killed IDs, cooldown, save/restore, and `OnLocationSpawned`.
- `.work/merlin-workshop-v1.1.0-source/Assets/Code/Main/Locations/Location.cs`: `MoveAndRotateTo(Vector3, Quaternion, bool)` updates saved coords/rotation and triggers movement/teleport events.
- `<local-path>`: Cromlech native spawner rows.

## Selected DK5D source row

The exact native source row is:

- scene: `CampaignMap_HOS_merged`;
- host path: `/SpawnerSingle_EnemyMonster_T1_Grindylow_01`;
- host position: `-1860.15|73.81|-2958.34`;
- template: `Spec_EnemyMonster_T1_Grindylow`;
- template GUID: `fa79aaa0bff59484dab2cf35c5ea805c`;
- details: `spawnAmount=1; range=0; cooldown=7200; discardAfterAllKilled=True; snapToGround=True`;
- distance to Dragon Knight boss root: `77.83m`;
- CSV evidence line: `341`.

This is not a placement-root match. DK5D therefore uses the native spawner for creation/save ownership, then relocates the spawned Dragon Knight to the approved boss root.

## Runtime route

Dragon Knight `0.2.5` adds:

- `DragonKnightNativeSpawnerPlacementPatchPlan`;
- `DragonKnightNativeSpawnerPlacementSystem`;
- `DragonKnightNativeSpawnerPlacementContract`;
- a `0Harmony` reference in the Dragon Knight project.

The route:

1. Patches `LocationSpawner.InitFromAttachment`.
2. Matches the exact source row above.
3. Resolves `Spec_DragonKnight_DK4A` by GUID `d7b09116519f7564593be62781bee3db`.
4. Prepares the DK4A template with the proven native bootstrap visual setup.
5. Replaces the exact native candidate array with only `Spec_DragonKnight_DK4A`.
6. Observes `BaseLocationSpawner.OnLocationSpawned`.
7. Rejects any Dragon Knight actor that is `MarkedNotSaved` or `IsNotSaved`.
8. Moves the save-owned Dragon Knight actor to `-1795.209|80.243|-2915.928` using `Location.MoveAndRotateTo`.
9. Leaves native `BaseLocationSpawner` as cooldown, killed-ID, save/restore, and corpse owner.

## Stop conditions

DK5D must fail closed if:

- `0Harmony` is missing;
- the native hook methods cannot be resolved;
- the source scene, path, position, or template GUID changes;
- the DK4A Dragon Knight template cannot be resolved or prepared;
- `_locationsToSpawn` no longer exists as `LocationTemplate[]`;
- the spawned actor is no-save/temporary;
- the owned placement observer cannot later prove `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.

DK5D still does not authorize movement, attacks, phase combat, AI dispatch, companion behavior, items, armor, save writes outside native ownership, cleanup commands, or release packaging.
