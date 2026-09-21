# Dragon Knight

Standalone Tainted Grail: The Fall of Avalon mod lane for the Dragon Knight asset set.

Current status: standalone BepInEx loader built, deployed, and live-load verified through `0.2.2`. DK2 visual-only plugin code builds, the approved Iron/Fire visual AssetBundle has been built, BepInEx live-load verification confirms Dragon Knight `0.2.0` loads the visual bundle, and the user reported the in-game visual proof works with a good transition. The AIR-49 Boss AI package contract now exists as `dragon-knight.boss.ai.v2` / `DragonKnight.AI.Package.V2`; Dragon Knight `0.2.1` wires and validates that package boundary default-off, but does not register live AI. DK4A has a passed isolated native template proof with Dragon Knight `NpcTemplate` GUID `00f608ee051b57748a6a9ed8dae28678` and `LocationTemplate` GUID `d7b09116519f7564593be62781bee3db`. Dragon Knight `0.2.2` and `0.2.3` added a no-save DK4 F7/F6 diagnostic and render overlay proof, but that route is not the production placement answer. Dragon Knight `0.2.4` adds the Dragon Knight-owned HOS placement observer. Dragon Knight `0.2.5` implements the approved DK5D native `LocationSpawner` candidate route: it hooks `LocationSpawner.InitFromAttachment`, replaces the exact Cromlech-area Grindylow native candidate with `Spec_DragonKnight_DK4A`, observes `BaseLocationSpawner.OnLocationSpawned`, rejects no-save actors, then moves the save-owned native actor to the Ancient Cromlech boss root with `Location.MoveAndRotateTo`. It does not call `LocationTemplate.SpawnLocation` directly and does not register live AI. The Unity retarget/render proof now targets the supplied `MCB` custom boss move-set source instead of generic third-party one-handed placeholder clips; Unity `6000.0.64f1` passed the proof with 92 MCB FBX clips, 42 required clips present, 0 failed animations, and phase-2 dual-sword presentation proof using the Fire character-with-weapon prefab plus an offhand Iron sword attached to `hand_l`. Proof bundle SHA-256 is `18A140AAD734EC835A48CFED98F8A5A2C5F2D50D0B9ACAE5FA8E04616E019A22`.

DK5C remains as superseded authored-pack source history. DK5D is the active placement route override. Its native source slot is `CampaignMap_HOS_merged` `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` at `-1860.15|73.81|-2958.34`, source template `Spec_EnemyMonster_T1_Grindylow` / `fa79aaa0bff59484dab2cf35c5ea805c`, evidence `template-diagnostics.20260803-035222.spawner_refs.csv:line341`. The boss root stays `-1795.209|80.243|-2915.928`.

The active placement target still uses `Spec_DragonKnight_DK4A` / `NPCTemplate_DragonKnight_DK4A` at `dragon-knight.vaelor.cromlech.center` / `-1795.209|80.243|-2915.928`, with LocationTemplate GUID `d7b09116519f7564593be62781bee3db` and NpcTemplate GUID `00f608ee051b57748a6a9ed8dae28678`.

## Intended Scope

- One standalone roaming boss lane.
- One standalone companion lane.
- Dragon Knight weapon set lane.
- Dragon Knight armor set lane.

These lanes are intentionally standalone. Existing systems supply proven methods that Dragon Knight should adapt into its own implementation, not runtime behavior to call directly.

## Proven Methods To Adapt

- Companion method: adapt the proven `mods/avalon-companions` lifecycle, command-surface, native assist, not-saved actor, diagnostics, and validation approach inside Dragon Knight.
- Asset import method: adapt the proven `mods/avalon-awakened` external-source inventory, licence gate, isolated Unity authoring, cooked-content transport, hash validation, and missing-content approach inside Dragon Knight for weapons and armor.
- AI method: create a Dragon Knight AI package lane using the proven `mods/avalon-ai-runtime` package/host, blackboard, planning, action-gateway, lease, and fail-closed ownership model.

Dragon Knight must not call the existing companion, asset, or AI mods as production dependencies unless a later Dragon Knight decision explicitly opens that integration. The default path is standalone implementation using the proven methods.

## Current Asset Source

User-selected local source folder:

`<local-path>`

Observed files:

- `dragonknight_2022333f1.unitypackage`
- `dragon_knight_fbx_textures_nakedsingularity`
- `dragon_knight_maya_rig_nakedsingularitystudio`
- `dragon_knight_substance_painter_nakedsingularity`

User-selected local boss move-set source folder:

`<local-path>`

Observed boss move-set files:

- `MCB.zip`
- `MCB` folder with 93 FBX files and 32 Maya source files
- Normal in-place and root-motion boss clips
- Transform in-place and root-motion boss clips
- `SM_Greatsword.fbx`, ignored for this proof because Dragon Knight uses its own one-handed sword prefab

The Unity package metadata includes Dragon Knight character prefabs, weapon prefabs, FBX meshes, textures, shadergraphs, render settings, an overview map, and `Dragon_Knight_Controller.controller`.

## Active Boundary

Allowed now:

- Read-only asset/source inventory.
- Licence and entitlement evidence capture.
- Mod-local research and decision records.
- Isolated authoring-plan design.
- Dragon Knight-local method planning for companion, asset import, equipment, boss, and AI package lanes.
- Standalone BepInEx loader build and load proof.
- DK2 visual-only AssetBundle build and runtime hotkey proof using only the approved Iron/Fire character-with-weapon prefabs.
- DK3 source-only AI package scaffold using `AvalonAI.Contracts.V2` only, with offline Runtime V2 registration fixtures.
- AIR-49 Dragon Knight Boss AI package contract with fail-closed actor, owner/lease, target, Rabbit, GOAP, capability, phase, and bridge policy checks.
- Dragon Knight loader default-off package-boundary validation for `DragonKnight.AI.Package.V2`; this logs the AIR-49 marker and never registers live AI.
- DK4 documentation-only proof gate for default-off live actor observation, `Location.ID`, ownership, target identity, activation, and cleanup evidence.
- DK4 source-gate packet defining exact diagnostic source files, F7/F6 activation, fixture count, marker text, build/deploy plan, live log checks, cleanup checks, and stop conditions.
- DK4A native actor/template candidate scan, Foredweller serialized probe, proof-profile gate, Foredweller boss profile approval, isolated template proof preparation/source packet, and isolated Unity author/build/finalized load-release proof.
- DK4 default-off diagnostic source for exact target proof, native `SpawnLocation`, live `Location.ID`, diagnostic lease, no-save readback, and cleanup marker.
- DK4 render overlay source that reuses the proven `dragonknight_visuals` Iron character-with-weapon prefab, hides the native bootstrap renderers, disables only overlay colliders, and still leaves movement, attacks, phase combat, AI dispatch, companion protection, items, saves, and roaming inactive.
- Dragon Knight-owned HOS placement observer source for the production lane: it is read-only, targets existing `CampaignMap_HOS` / Ancient Cromlech, supports a multiple-NPC-ready placement manifest, requires the Dragon Knight `NpcTemplate`/`LocationTemplate` GUIDs, requires a live save-owned `Location.ID`, and rejects temporary/no-save placements.
- DK5D native `LocationSpawner` candidate placement source. It uses the approved route override, exact Cromlech-area Grindylow spawner evidence, Harmony hooks, the proven DK4A template GUID, deterministic single-candidate replacement, native `BaseLocationSpawner` save/cooldown/killed-ID ownership, and boss-root relocation via `Location.MoveAndRotateTo`.
- Dragon Knight Ancient Cromlech overlay manifest at `placement/dragon-knight-cromlech-overlay.v1.json`, validated against Avalon Core `avalon.core.custom-scene-placement-overlay.v1` as an authored-scene overlay declaration only.
- DK5B route-proof documentation, read-only Unity-route discovery, and fixtures for the authored HOS placement pack route. This only records the gate; it does not mutate Unity, build a pack, deploy to FoA, launch FoA, or prove live placement.
- DK5C authored HOS placement pack source packet and fixtures. This proves scene target `CampaignMap_HOS`, Core anchor `hos.ancient-cromlech.center`, placement prefab root `DK5C_DragonKnight_Cromlech_PlacementRoot`, marker ID `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE`, a list-shaped multiple-NPC-ready manifest, Dragon Knight template GUIDs, altar reference `-1792.661|79.942|-2912.844`, separate boss root `-1795.209|80.243|-2915.928`, no-save/no-temp rejection, and owned-placement observer compatibility without Unity mutation, pack build, deployment, or live proof.
- Unity-only retarget/render proof builder updates that require the supplied `MCB` boss move-set source and build a proof controller from Dragon Knight custom boss clips, including phase-2 dual-sword presentation proof, without runtime AI, movement, combat, saves, or deployment.

Blocked now:

- Additional `LocationTemplate` or `NpcTemplate` authoring outside the completed DK4A proof.
- Boss roaming, combat, rewards, corpse handling, automatic encounter activation, or native AI dispatch.
- Live proof of DK5D in the installed game until the build/deploy/log check proves `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_PLACED` and `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.
- Treating `placement/dragon-knight-cromlech-overlay.v1.json` as an executor. It is not an executor; it is a superseded source-gated manifest only. DK5D native candidate build/deploy/live proof still has to pass.
- Companion spawning, taming, ally/faction changes, command UI, custom AI, or persistence.
- Weapon or armor item registration, inventory grants, equipment integration, crafting/vendor/loot injection, or save-visible item behavior.
- Dragon Knight AI host binding, live Runtime registration, or runtime action dispatch.
- Dragon Knight AI observation collection, movement, attacks, companion-protect behavior, phase behavior, item behavior, save writes, or roaming.

## Implementation Order

1. Get Dragon Knight into the game as a standalone loader, then a visual-only in-game asset proof.
2. Prove the Dragon Knight native actor/template profile before any `LocationTemplate.SpawnLocation` path.
3. Use the approved DK5D native `LocationSpawner` candidate route for Ancient Cromlech. DK4 remains available only as the older temporary diagnostic and is not the production placement route.
4. Prove the live placed Dragon Knight actor through the owned placement observer: exact scene, template GUIDs, `Location.ID`, actor ID, save-owned state, native spawner ownership, and boss-root relocation.
5. Add bounded goals/actions after actor, target, ownership, capability, and host evidence exists.
6. Add follower mechanics.
7. Add usable weapon and armor items.

## Next Gate

Next validation is the DK5D build/deploy/live gate. It must build and deploy Dragon Knight `0.2.5`, load a save at Ancient Cromlech, prove `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_INSTALLED`, prove `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_PLACED`, prove `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` from a save-owned Dragon Knight `Location.ID`, and keep movement, attacks, phase combat, companion protection, items, armor, live AI dispatch, and roaming blocked until the placed actor is live-proven.
