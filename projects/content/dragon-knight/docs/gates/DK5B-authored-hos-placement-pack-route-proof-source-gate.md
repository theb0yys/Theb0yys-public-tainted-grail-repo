# DK5B: Authored HOS Placement Pack Route-Proof Source Gate

Status: approved source gate; route evidence required before Unity mutation, pack build, deployment, or live proof

Date: 2026-08-03

## Purpose

Define the exact proof gate for discovering and authorizing the Unity authoring route that can place the Dragon Knight `LocationTemplate` into the existing `CampaignMap_HOS` scene as a Dragon Knight-owned authored placement pack.

## Required Marker

```text
DRAGON_KNIGHT_DK5B_AUTHORED_HOS_PLACEMENT_ROUTE_PROOF_SOURCE_GATE_PASS fixtures=18 overlay=dragon-knight.cromlech.overlay.v1 scene=CampaignMap_HOS placement=dragon-knight.vaelor.cromlech.center route-proof-gate=1 route-proven=0 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 runtime-ai=0 movement=0 attacks=0 phase-combat=0 save-write=0
```

## Allowed Now

- Repo-local DK5B decision, research, gate, README, validation, and fixture updates.
- Read-only discovery against `<local-path>`.
- Read-only checks of existing FoA ModService output folders and Dragon Knight generated template reports.
- A later DK5B source packet that names the exact Unity editor authoring files, serialized placement asset type, output path, build command, deployment path, hashes, live observer marker, and cleanup/rollback proof.

## Not Allowed Now

- Mutating the Unity project.
- Creating or editing Unity scenes, prefabs, Addressables groups, catalogues, bundles, ModService outputs, FoA install files, or saves.
- Launching FoA for live DK5B proof.
- Claiming `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` from an authored placement actor.
- Movement, attacks, damage, hitboxes, real phase combat, companion protection, follower mechanics, weapon items, armor items, rewards, corpse handling, roaming, Rabbit writes, GOAP dispatch, PlayMaker calls, Blaze control, FoAHost activation, save writes, persistence, cleanup, or rollback.

## Required Route Evidence

The next source packet must prove all of the following before any Unity project mutation:

- exact Unity project root;
- exact Unity editor source file(s) to add or edit;
- exact serialized FoA asset, component, scene object, or ModService record that represents a placed `LocationTemplate`;
- exact reference mechanism for `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db`;
- exact reference mechanism for `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678`;
- exact `CampaignMap_HOS` scene reference and how the placement is stored in that scene or pack;
- exact altar reference `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`, marked as neither boss root nor trigger;
- exact boss placement transform `-1795.209|80.243|-2915.928` with rotation `0|-140.441|0`;
- exact duplicate key and Dragon Knight ownership metadata;
- exact save-owned behavior and how no-save/temporary placement is avoided;
- exact ModService or scene-pack output folder, catalogue files, bundle files, and hashes;
- exact build command and deployment command;
- exact live log checks for `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`;
- exact cleanup, rollback, and re-run safety checks.

## Stop Conditions

Stop immediately if the route or source:

- calls or names `LocationTemplate.SpawnLocation`;
- uses `BaseLocationSpawner`;
- injects or edits vanilla `LocationSpawner` slots;
- uses Hollow Druid, bear, shrine, discovery, altar interactable, prompt visibility, or vanilla actor slots as production placement;
- moves actors, attacks, phases, protects companions, creates items, writes saves, registers live AI, or activates FoAHost;
- treats `placement/dragon-knight-cromlech-overlay.v1.json` as an executor;
- claims route proof from the external Unity project without naming exact source files and serialized output.

## Required Live Marker After Later Authorization

The later live placement proof is still:

```text
DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED
```

It must include owner, encounter, placement, actor role, actor source, actor `Location.ID`, actor ID, scene, `LocationTemplate` GUID, `NpcTemplate` GUID, position, activation trigger, arena radii, `custom-owned=1`, `native-spawn=0`, `vanilla-slot=0`, and `save-owned=1`.
