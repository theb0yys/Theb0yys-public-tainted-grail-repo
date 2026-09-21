# DK5C: Authored HOS Placement Pack Source Gate

Status: source implemented and fixture-gated; Unity mutation, pack build, deployment, and live proof remain blocked

Date: 2026-08-03

## Purpose

Define the source packet for the Dragon Knight-owned authored placement pack that will later place `Spec_DragonKnight_DK4A` into the existing `CampaignMap_HOS` scene at Ancient Cromlech.

## Required Marker

```text
DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=25 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0
```

## Source Packet

- Contract source: `mods/dragon-knight/src/Encounter/DragonKnightAuthoredHosPlacementPackContract.cs`.
- Manifest source: `mods/dragon-knight/placement/dragon-knight-dk5c-authored-hos-placement-pack.v1.json`.
- Existing Core overlay: `mods/dragon-knight/placement/dragon-knight-cromlech-overlay.v1.json`.
- Core named-location anchor: `hos.ancient-cromlech.center` / `A-HOS-ANCIENT-CROMLECH-17`.

## Required Proven Fields

The fixture must prove:

- target scene is exactly `CampaignMap_HOS`;
- Core anchor is exactly `hos.ancient-cromlech.center`;
- placement ID is exactly `dragon-knight.vaelor.cromlech.center`;
- placement prefab root is exactly `Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab`;
- placement root object name is exactly `DK5C_DragonKnight_Cromlech_PlacementRoot`;
- marker ID is exactly `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE`;
- manifest is list-shaped and `supportsMultiplePlacements=true`;
- `LocationTemplate` is exactly `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db`;
- `NpcTemplate` is exactly `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678`;
- altar reference is exactly `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`, with `isBossPlacementRoot=false` and `isEncounterTrigger=false`;
- boss placement root is exactly `-1795.209|80.243|-2915.928`, derived from the altar reference plus `4m` toward dumped hero position `-1801.847|81.026|-2923.964`;
- rotation is exactly `0|-140.441|0`;
- activation trigger remains `dragon-knight.encounter.inner-ring-entered`;
- no-save rejection is explicit: `requiresSaveOwnedLocation=true`, `rejectsMarkedNotSaved=true`, `rejectsIsNotSaved=true`;
- observer compatibility is explicit: expected marker `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`, actor ID format `foa.location:<Location.ID>`, exact template GUIDs, and `3m` placement drift.

## Unity Route Named But Not Executed

The source packet names these future Unity authoring surfaces:

- Unity project root: `<local-path>`.
- Future editor source file: `Assets\Editor\DragonKnight\DK5CAuthoredHosPlacementPackBuilder.cs`.
- Future placement manifest asset: `Assets\DragonKnight\Placement\DK5C\dragon-knight-dk5c-authored-hos-placement-pack.v1.json`.
- Future placement prefab root: `Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab`.
- Future output root: `<local-path>`.
- Future deployment target: `<local-path>`.

These paths are source-contract values only. DK5C does not create, edit, build, copy, or deploy them.

## Stop Conditions

Stop immediately if the source or manifest:

- calls or names `LocationTemplate.SpawnLocation`;
- uses `BaseLocationSpawner`;
- injects or edits vanilla `LocationSpawner` slots;
- uses Hollow Druid, bear, shrine, discovery, altar interactable, prompt visibility, or vanilla actor slots as production placement root, activation trigger, or runtime actor source;
- sets `allowsNativeSpawn`, `allowsVanillaSpawnerSlots`, `allowsRuntimeSceneMutation`, or `allowsSaveWrites` true;
- drops save-owned `Location.ID` proof, host-ownership proof, no-save rejection, or observer compatibility;
- moves actors, attacks, phases, protects companions, creates items, writes saves, registers live AI, writes Rabbit state, dispatches GOAP, calls PlayMaker, controls Blaze, or activates FoAHost.

## Required Later Live Marker

The later live placement proof remains:

```text
DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED
```

It must include owner, encounter, placement, actor role, actor source, actor `Location.ID`, actor ID, scene, `LocationTemplate` GUID, `NpcTemplate` GUID, position, activation trigger, arena radii, `custom-owned=1`, `native-spawn=0`, `vanilla-slot=0`, and `save-owned=1`.
