# DK5B Authored HOS Placement Route Discovery

Date: 2026-08-03

## Scope

Read-only discovery for the exact Unity authoring route that can place the Dragon Knight `LocationTemplate` into the existing Horns of the South scene `CampaignMap_HOS`.

## Sources reviewed

- User approval in the current task: DK5B is explicitly approved because existing docs blocked the authored pack/live proof, and the exact Unity authoring route is still unproven.
- `mods/dragon-knight/docs/gates/DK5-owned-hos-placement-observer-source-gate-packet.md`: DK5 observer source is implemented, but authored Unity/ModService placement pack build and live observation remain pending.
- `mods/dragon-knight/docs/decisions/0015-owned-hos-placement-route.md`: production placement must be Dragon Knight-owned, target `CampaignMap_HOS`, and avoid DK4 temporary spawn, `BaseLocationSpawner`, vanilla slots, Hollow Druid slots, bear slots, and external runtime placement owners.
- `mods/avalon-core/docs/decisions/0152-custom-scene-placement-overlay-framework.md`: Avalon Core validates authored scene overlay manifests only; it does not build Unity scenes, load ModService assets, execute placement, write saves, or perform cleanup.
- `<local-path>`: Unity project exists and contains `AvalonAirOpenEditor`, `Scenes`, AI proof packages, and vendor assets.

## Read-only search result

Targeted searches under `<local-path>` for C# source and Unity metadata did not prove an existing Dragon Knight placement-pack authoring route.

The search did not find a source file that:

- authors `Spec_DragonKnight_DK4A` into `CampaignMap_HOS`;
- serializes a FoA `LocationTemplate` placement into an authored scene or ModService pack;
- references the Dragon Knight placement ID `dragon-knight.vaelor.cromlech.center`;
- provides a build/deploy command for a save-owned authored placement pack;
- proves live `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` from an authored placement actor.

Observed Unity source was mostly Avalon AI/FoAHost proof code and unrelated vendor code. The only C# `BuildPipeline` matches found in the narrowed source search were unrelated Convai/Unity package code, not FoA scene placement authoring.

## Established facts

- The Unity project path exists.
- The Dragon Knight native template GUIDs already exist:
  - `Spec_DragonKnight_DK4A` / `d7b09116519f7564593be62781bee3db`;
  - `NPCTemplate_DragonKnight_DK4A` / `00f608ee051b57748a6a9ed8dae28678`.
- The accepted encounter scene is `CampaignMap_HOS`.
- The accepted placement is `dragon-knight.vaelor.cromlech.center` at corrected boss root `-1795.209|80.243|-2915.928`.
- The old Cromlech coordinate `-1792.661|79.942|-2912.844` is retained as the altar reference `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1`, not as boss root or trigger.
- Existing overlay manifest validation proves declaration shape only, not Unity authoring, ModService loading, live actor placement, persistence, cleanup, or rollback.

## Missing route evidence

DK5B still needs exact proof of:

- Unity project root to mutate.
- Exact Unity editor source file(s) that will author the placement pack.
- Exact serialized FoA asset type or scene object that represents a placed `LocationTemplate`.
- Exact way `Spec_DragonKnight_DK4A` and `NPCTemplate_DragonKnight_DK4A` are referenced by GUID/address.
- Exact way `CampaignMap_HOS`, altar reference `-1792.661|79.942|-2912.844`, boss root `-1795.209|80.243|-2915.928`, rotation, duplicate key, and activation metadata are stored.
- Exact ModService or scene-pack output folder and catalogue/bundle files.
- Exact build/deploy command and post-build hashes.
- Exact live-load log checks for `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.
- Exact cleanup, rollback, and no-save/save-owned verification plan.

## Boundary

This discovery opens DK5B route-proof source work only. It does not authorize Unity project mutation, authored pack build, FoA deployment, live proof, movement, attacks, phase combat, AI dispatch, items, persistence, cleanup, or rollback.
