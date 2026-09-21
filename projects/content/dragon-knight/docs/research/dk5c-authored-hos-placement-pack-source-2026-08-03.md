# DK5C Authored HOS Placement Pack Source

Date: 2026-08-03

## Scope

Source-only implementation proof for the authored Dragon Knight HOS placement pack. This proves exact identifiers, paths, manifest shape, no-save policy, and observer compatibility before Unity mutation or pack build.

## Sources reviewed

- User-required implementation checklist: prove scene target `CampaignMap_HOS`, placement prefab/root, marker ID, multiple-NPC-ready manifest, Dragon Knight template GUIDs, no-save/no-temp rejection, and live observer compatibility.
- `mods/dragon-knight/docs/decisions/0015-owned-hos-placement-route.md`: production placement must be Dragon Knight-owned in `CampaignMap_HOS`, using the accepted template GUIDs and corrected Cromlech boss-root/altar-reference split; DK4 temporary spawn and vanilla slot routes remain rejected.
- `mods/dragon-knight/docs/decisions/0016-dk5b-authored-hos-placement-route-proof.md`: DK5B requires exact source packet proof before Unity mutation, pack build, deployment, or live proof.
- `mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementObserver.cs`: live observer scans `World.All<Location>()`, requires the exact `LocationTemplate`/`NpcTemplate` pair, rejects `MarkedNotSaved` and `IsNotSaved`, and derives actor ID as `foa.location:<Location.ID>`.
- `mods/dragon-knight/placement/dragon-knight-cromlech-overlay.v1.json`: existing Core overlay declaration for Ancient Cromlech uses `CampaignMap_HOS`, `hos.ancient-cromlech.center`, `Spec_DragonKnight_DK4A`, `d7b09116519f7564593be62781bee3db`, and `00f608ee051b57748a6a9ed8dae28678`.
- `mods/avalon-core/docs/decisions/0153-hos-ancient-cromlech-named-location-anchor.md`: Core named-location anchor for Ancient Cromlech is planning and lookup only.
- Read-only scan of `<local-path>`: the project exists with `AvalonAirOpenEditor`, `Scenes`, and vendor assets, but no existing Dragon Knight placement authoring source was proven.
- Live template diagnostic dump `20260803-032730`: `cromlech_target_proof.csv` line 21 proves `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; the same dump records hero position `-1801.847|81.026|-2923.964`, allowing the boss root to be separated `4m` from the altar at `-1795.209|80.243|-2915.928`.

## Established DK5C source route

DK5C source packet:

- Contract source: `mods/dragon-knight/src/Encounter/DragonKnightAuthoredHosPlacementPackContract.cs`.
- Manifest source: `mods/dragon-knight/placement/dragon-knight-dk5c-authored-hos-placement-pack.v1.json`.
- Gate: `mods/dragon-knight/docs/gates/DK5C-authored-hos-placement-pack-source-gate.md`.
- Fixture: `mods/dragon-knight/tests/DragonKnight.DK5CAuthoredPlacementPack.Fixtures`.

Future Unity route named by DK5C, but not executed:

- Unity project root: `<local-path>`.
- Future editor source file: `Assets\Editor\DragonKnight\DK5CAuthoredHosPlacementPackBuilder.cs`.
- Future placement manifest asset: `Assets\DragonKnight\Placement\DK5C\dragon-knight-dk5c-authored-hos-placement-pack.v1.json`.
- Future placement prefab root: `Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab`.
- Future marker component type: `DragonKnight.Editor.DK5CAuthoredPlacementMarker`.
- Future marker ID: `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE`.

## Proven identity

- Scene: `CampaignMap_HOS`.
- Core anchor: `hos.ancient-cromlech.center`.
- Core local ref: `A-HOS-ANCIENT-CROMLECH-17`.
- Placement ID: `dragon-knight.vaelor.cromlech.center`.
- Placement root: `DK5C_DragonKnight_Cromlech_PlacementRoot`.
- LocationTemplate: `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db`.
- NpcTemplate: `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678`.
- Altar reference: `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; `isBossPlacementRoot=false`; `isEncounterTrigger=false`.
- Boss root: `-1795.209|80.243|-2915.928`.
- Boss root derivation: altar reference plus `4m` toward dumped hero position `-1801.847|81.026|-2923.964`.
- Rotation: `0|-140.441|0`.
- Activation trigger: `dragon-knight.encounter.inner-ring-entered`.
- Expected live marker after later authorization: `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.
- Expected actor ID format: `foa.location:<Location.ID>`.

## Boundary

DK5C proves source shape only. It does not mutate Unity, create the prefab root, create the editor builder in the Unity project, build a pack, deploy files, launch FoA, prove live placement, write saves, persist state, run cleanup, roll back, register AI, move actors, attack, phase, or create items.
