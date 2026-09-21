# 0017: DK5C Authored HOS Placement Pack Source

Status: accepted source packet; Unity mutation, pack build, deployment, and live proof remain blocked

Date: 2026-08-03

## Context

Decision `0015` selected the production placement route: a Dragon Knight-owned placement pack targeting the existing `CampaignMap_HOS` scene at Ancient Cromlech, rejecting temporary DK4 spawning, vanilla spawner slots, Hollow Druid slots, bear slots, and external runtime owners.

Decision `0016` opened DK5B because the exact authored HOS placement route was unproven. DK5B proved that read-only discovery did not find an existing Dragon Knight placement-pack source in `<local-path>`, and it required a source packet that proves the scene target, placement storage, template refs, output path, no-save/save-owned policy, live observer checks, cleanup, and rollback before Unity mutation.

Avalon Core Decision `0153` adds the Core named-location anchor `hos.ancient-cromlech.center` / `A-HOS-ANCIENT-CROMLECH-17` for Ancient Cromlech in `CampaignMap_HOS` at `-1792.661|79.942|-2912.844`, but it authorizes planning and anchor lookup only. The latest Cromlech dump proves that coordinate is the live `AltarInteract` reference, not the boss placement root.

The user’s current required implementation checklist is: prove scene target `CampaignMap_HOS`, placement prefab/root, marker ID, multiple-NPC-ready manifest, Dragon Knight template GUIDs, no-save/no-temp rejection, and live observer compatibility.

## Decision

Add DK5C as the Dragon Knight-authored HOS placement pack source packet.

DK5C adds:

- `DragonKnightAuthoredHosPlacementPackContract.cs` for compile-checked source constants;
- `dragon-knight-dk5c-authored-hos-placement-pack.v1.json` for a manifest source packet;
- explicit altar-reference data for `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1`;
- a separate boss placement root derived from that altar reference;
- a gate and fixture that prove the source packet remains aligned with DK5 observer requirements and the Core Ancient Cromlech anchor.

DK5C proves:

| Required proof | DK5C value |
|---|---|
| Scene target | `CampaignMap_HOS` |
| Core anchor | `hos.ancient-cromlech.center` / `A-HOS-ANCIENT-CROMLECH-17` |
| Placement ID | `dragon-knight.vaelor.cromlech.center` |
| Placement prefab root | `Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab` |
| Placement root name | `DK5C_DragonKnight_Cromlech_PlacementRoot` |
| Marker ID | `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE` |
| Manifest shape | list-shaped, `supportsMultiplePlacements=true` |
| LocationTemplate | `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db` |
| NpcTemplate | `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678` |
| Altar reference | `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; not boss root; not trigger |
| Boss root | `-1795.209|80.243|-2915.928`, derived from altar reference plus `4m` toward dumped hero position `-1801.847|81.026|-2923.964` |
| Rotation | `0|-140.441|0` |
| No-save/no-temp policy | requires save-owned `Location.ID`, rejects `MarkedNotSaved`, rejects `IsNotSaved` |
| Live observer compatibility | expected marker `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`, actor ID format `foa.location:<Location.ID>`, exact template GUIDs, `3m` drift |

## Boundaries

DK5C does not mutate the Unity project, create the Unity editor builder, create the prefab root, create or edit `CampaignMap_HOS`, build the pack, deploy files, launch FoA, claim live `Location.ID` proof, move actors, attack, phase, protect companions, create items, write saves, register live AI, write Rabbit state, dispatch GOAP, call PlayMaker, control Blaze, activate FoAHost, persist data, clean up, or roll back.

For the Unity-scene boundary in this source packet: DK5C does not create or edit `CampaignMap_HOS`.

The next authorized step after DK5C is a separate Unity mutation gate for creating the named editor builder, placement manifest asset, and prefab root in the Unity project. That later gate must still run build/deploy/live proof and must not claim success until `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` is logged from a save-owned authored actor.

## Protected files

No Tales from the Age of Men, Age of Men, or overhaul files are part of this decision.
