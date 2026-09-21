# Dragon Knight Research

Research metadata:
- Tier: T4 feature-gate scaffold
- Domain: assets, game-systems, mod-specific
- Scope: standalone Dragon Knight boss, companion, weapon set, and armor set mod
- Owner: `mods/dragon-knight`
- Status: loader live-verified; DK2 visual-only bundle built and live-load verified; AIR-49 Boss AI package contract fixture-validated and wired default-off in Dragon Knight `0.2.1`; DK4 render-overlay actor diagnostic source built and deployed in Dragon Knight `0.2.3`; owned HOS placement observer source added in Dragon Knight `0.2.4`
- Evidence labels: Documented, local-source-observed
- Game version and branch: unknown for this mod; current repo default is FoA Mono/BepInEx 5 unless a later gate records otherwise
- Last reviewed: 2026-08-02
- Validation state: Level 0 documentation only

## Requested Outcome

Create a completely standalone Dragon Knight mod with:

- a roaming boss;
- a companion;
- weapon sets;
- armor sets.

The user explicitly stated this must be separate from prior Wyrd Hunt/Avalon Awakened integration assumptions. The user then clarified that the proven methods from their companion mod, asset mod, and AI system should be used across this standalone mod, and that AI will need its own AI package.

## Sources Reviewed

- `docs/engineering-process.md:19-40` requires baseline docs, mod research, and target evidence before code; it says to stop when behavior is unclear or no research-backed implementation path is proven.
- `docs/mod-lifecycle.md:3-17` requires research first, including game version, assembly, target class/method candidates, compatibility risks, and decompiled target evidence when game internals are unknown.
- `docs/decision-records.md:5-13` requires decision records for AssetBundle/external file loading, save-affecting behavior, new dependencies, and compatibility tradeoffs.
- `docs/ecosystem/missing-game-aspects-roadmap.md:95-104` marks custom world assets or a prefab framework as missing/blocked.
- `mods/avalon-companions/README.md:7-9` establishes a research-gated companion scaffold and states the current companion lane is pets, not humanoid followers.
- `mods/avalon-companions/README.md:92-110` blocks humanoid NPC spawning/cloning, humanoid combat AI, follow/teleport behavior, dialogue/recruitment/quest state, actor persistence, arbitrary targeting, custom faction aggression, and custom AI in that mod's current scope.
- `mods/avalon-companions/README.md:147-163` records reference patterns for one-session not-saved actors, explicit player commands, Follow/Stay/Defend modes, native defend prompting, lifecycle checks, recover/dismiss behavior, and refusing to touch non-roster pets.
- `mods/avalon-awakened/docs/design.md:5-24` records an external-asset architecture from licensed source library to reviewed register, isolated Unity preparation, script-free visual proof, static dependency gate, Windows AssetBundle, same-version load gate, and later default-off runtime proof.
- `mods/avalon-awakened/docs/design.md:92-103` records Unity `6000.0.64f1`, HDRP `17.0.4`, Mono/BepInEx 5, explicit adapter boundaries, and review/performance requirements for runtime asset work.
- `mods/avalon-awakened/docs/decisions/0004-creature-core-and-native-executor-separation.md:9-27` records the established custom-creature architecture split and states that no universal raw-prefab injector will be created.
- `mods/avalon-awakened/docs/current-creature-injection-process.md:234-245` lists stop conditions for incomplete native actor/template contracts, asset transport being treated as actor proof, unknown licence/ownership, missing gates, and undefined persistence/cleanup/rollback behavior.
- `mods/avalon-awakened/docs/current-creature-injection-process.md:312-323` records current creature-lane non-authorization boundaries, including no unrelated source mutation, no unsupported builds, no population integration, and no public packaging until separately gated.
- `mods/avalon-ai-runtime/docs/architecture.md:1-45` establishes the AI architecture path from third-party AI mods through public contracts, package host, Runtime, Rabbit Blackboard Pro, CrashKonijn GOAP v3, and execution routing.
- `mods/avalon-ai-runtime/docs/architecture.md:110-145` records reference observation, blackboard, computed signal, and goal-arbiter concepts.
- `mods/avalon-ai-runtime/README.md:21-28` documents contracts/runtime/host boundaries and states the Runtime does not own scheduling, targeting, movement, combat, game calls, Blaze execution, Photon authority, or persistence outside the guarded host architecture.
- `mods/wyrd-hunt/docs/research/wyrdness-route-patrol-design-gate-2026-07-12.md:74-90` requires creature-specific decisions for patrol use and keeps bosses, custom creatures, packs, summons, quest actors, and special templates blocked without exact approval.
- Local asset folder observed read-only: `<local-path>`.

## Standalone Proven-Method Decision

The following existing mods provide proven methods to adapt into Dragon Knight-local implementation:

- `mods/avalon-companions` for companion command, lifecycle, not-saved actor, native assist, and validation methods.
- `mods/avalon-awakened` for asset inventory, licence, isolated authoring, cooked content transport, hash, and missing-content methods.
- `mods/avalon-ai-runtime` for AI package/host boundary, blackboard, planner, action-gateway, lease, ownership, and fail-closed validation methods.

Dragon Knight should implement its own companion, asset, boss, weapon, and armor runtime paths using those proven methods. It must not add compile-time references, runtime dependencies, config coupling, package registration, shared actor ownership, shared asset packs, or shared AI host binding to those existing systems until a Dragon Knight-specific decision explicitly opens that exact integration.

AI is the exception in structure: Dragon Knight needs a Dragon Knight-owned AI package lane, not direct ad hoc AI inside the boss or companion feature code.

## Local Source Observations

Folder contents observed read-only:

- `dragonknight_2022333f1.unitypackage`
- `dragon_knight_fbx_textures_nakedsingularity`
- `dragon_knight_maya_rig_nakedsingularitystudio`
- `dragon_knight_substance_painter_nakedsingularity`

The FBX/textures archive contains:

- `SK_Dragon_knight_UE4.fbx`
- `SK_Dragon_knight_UE4_no_Cape.fbx`
- `SK_Dragon_knight_UE5.fbx`
- `SK_Dragon_knight_UE5_no_Cape.fbx`
- `SK_Dragon_knight_sword.fbx`
- Fire and Iron texture families for body, arm, leg, cloth, and sword materials.

The Unity package pathname metadata includes:

- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SK_DragonKnight_Sword_Fire.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SK_DragonKnight_Sword_Iron.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SM_DragonKnight_Sword_Fire.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SM_DragonKnight_Sword_Iron.prefab`
- `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/ANIMATIONS/CONTROLLER/Dragon_Knight_Controller.controller`
- Dragon Knight material, texture, shadergraph, render-setting, and overview-scene assets.

No licence, EULA, readme, or copyright file was found inside the package or extracted folders. The user supplied Fab library screenshots showing the Dragon Knight listing saved in their library; this is recorded as entitlement evidence for local development.

## Unity Import Verification

Unity `6000.0.64f1` imported `dragonknight_2022333f1.unitypackage` in an isolated temp project and wrote `mods/dragon-knight/docs/generated-dragon-knight-unity-inspection-report.json`.

Verified imported assets:

- 102 assets under `Assets/NAKED_SINGULARITY/DRAGON_KNIGHT`.
- 8 prefabs.
- Fire and Iron character prefabs.
- Fire and Iron character-with-weapon prefabs.
- Fire and Iron skinned sword prefabs.
- Fire and Iron static sword prefabs.
- Character prefabs have `Animator`, `CapsuleCollider`, `Cloth`, `SkinnedMeshRenderer`, and `Transform` components only.
- Character-with-weapon prefabs have 97 transforms, 129-130 components, 14-15 colliders, 16 skinned mesh renderers, one animator, and zero missing scripts.
- The character FBX imports one usable animation clip named `Pose_Check`; `Take 001` is present only as a preview clip.
- `Dragon_Knight_Controller.controller` imports with zero parameters and zero states.

Implication: the base Dragon Knight asset pack supports a visual-only Iron/Fire phase proof now. The base Dragon Knight package alone does not prove custom attack clips, phase state transitions, or combat animation mapping.

## Custom Boss Move-Set Source Observation

The user supplied the dedicated boss move-set source folder:

`<local-path>`

Read-only local observation found:

- `MCB.zip`;
- `MCB` folder;
- 93 `.fbx` files;
- 32 `.ma` files;
- `SM_Greatsword.fbx`.

The relevant Unity retarget candidates are the `MCB` FBX clips:

- `MCB/Normal/In Place` for Iron/phase-1 idle, movement, block, hit, death, and attacks;
- `MCB/Normal/Root_Motion` for root-motion attack proof candidates;
- `MCB/Transform/TIn Place` for phase transform and Fire/phase-2 combat proof candidates;
- `MCB/Transform/TRoot_Motion` for root-motion Fire/phase-2 proof candidates.

`SM_Greatsword.fbx` is intentionally excluded from the Dragon Knight proof. The boss remains one-handed and uses the Dragon Knight sword prefab from the Dragon Knight asset pack. The Maya files remain source evidence only; the Unity proof imports FBX clips.

## Feature Lanes

### Boss Lane

Current status: blocked before runtime.

Needs evidence for:

- exact boss identity, display name, and gameplay role;
- source prefab and visual variant;
- rig and animation contract;
- FoA native actor baseline;
- `NpcTemplate` and `LocationTemplate` authoring strategy;
- combat style, stats, faction, loot/reward, corpse/death behavior, and health-bar name behavior;
- custom boss move-set clip retarget proof against the Dragon Knight humanoid avatar;
- explicit one-handed weapon binding using the Dragon Knight sword prefab;
- roaming route/scene/placement boundary;
- one-session versus persistent behavior;
- save exclusion, cleanup, rollback, and failure behavior.

### Companion Lane

Current status: blocked before runtime.

Needs evidence for:

- whether the companion is the same Dragon Knight archetype or a separate variant;
- ally/faction and hostility policy;
- command surface and UI route;
- follow/stay/defend behavior ownership;
- target selection boundaries;
- actor lifetime, persistence, dismissal, death, and reload behavior;
- whether companion inventory/equipment is visual-only or gameplay-owned.

Proven method to adapt: use `mods/avalon-companions` docs/source to design Dragon Knight's own companion lifecycle, command surface, not-saved actor handling, diagnostics, and native-assist boundaries. This is not approval to depend on or modify Avalon Companions.

### Weapon Set Lane

Current status: blocked before runtime.

Needs evidence for:

- exact weapon meshes, materials, textures, icons, and names;
- FoA item/equipment template target lane;
- inventory grant, crafting, vendor, loot, or boss-drop acquisition;
- save/reload and missing-content behavior;
- combat stats, damage types, rarity, upgrade behavior, and localization.

Proven method to adapt: use `mods/avalon-awakened` asset-source and content-transport gates for Dragon Knight's own weapon import lane. This is not approval to place weapon assets in Avalon Awakened or to register FoA items.

### Armor Set Lane

Current status: blocked before runtime.

Needs evidence for:

- whether armor is a wearable FoA equipment set, a visual-only companion/boss outfit, or both;
- native body/rig compatibility;
- Kandra or renderer transport path;
- armor slots, stats, icons, localization, crafting/vendor/loot/boss-drop acquisition;
- save/reload, missing-content, equipment transition, first-person, and inventory-preview behavior.

Proven method to adapt: use `mods/avalon-awakened` external-asset, Kandra/renderer, isolated-authoring, hash, deployment, and missing-content gates for Dragon Knight's own armor lane. This is not approval to mutate Avalon Awakened or to register FoA equipment.

### AI Lane

Current status: blocked before runtime.

Needs evidence for:

- whether boss AI, companion AI, or both use Dragon Knight-specific AI packages;
- observation schema and actor identity lease;
- blackboard facts, goal policy, action policy, and action-gateway boundary;
- allowed native execution surfaces;
- targeting, movement, attack, defend, retreat, companion-protect, and cleanup rules;
- host activation, stand-down, rollback, and validation behavior.

Proven method to adapt: use `mods/avalon-ai-runtime` architecture and tests to design a Dragon Knight-owned AI package/host boundary. This is not approval to register Dragon Knight with Avalon AI Runtime or to add a runtime dependency before the Dragon Knight AI package gate.

## Implementation Boundary

Allowed now:

- documentation and research-gate setup;
- read-only source inventory;
- hash capture;
- licence/entitlement evidence collection;
- isolated Unity inspection after explicit gate authorization.
- Unity-only retarget/render proof authoring that imports the supplied `MCB` boss move-set FBX clips as Humanoid, verifies each imported MCB avatar is Humanoid, assigns the resulting controller to the Dragon Knight humanoid prefab, and builds a render proof controller/bundle.
- DK2 visual-only AssetBundle build containing only the approved Iron/Fire character-with-weapon prefabs;
- DK2 runtime hotkey proof code that loads the Dragon Knight-owned bundle, spawns a visual-only prefab on explicit request, disables colliders, switches visual phase, and discards the active visual.
- DK2 BepInEx load-log validation after deployment, without save access or gameplay automation.

Not allowed now:

- copying source assets into repo;
- mutating source archives or Unity packages;
- adding compile-time references or runtime dependencies on Avalon Companions, Avalon Awakened, or Avalon AI Runtime;
- putting AI directly into boss/companion feature code instead of a Dragon Knight AI package lane;
- registering templates or items;
- spawning native actors;
- touching saves;
- gameplay automation beyond explicit visual hotkeys;
- claiming in-game visual observation before F8/F10/F9 are tested in a loaded scene;
- claiming runtime attacks, animation events, hitboxes, damage, phase combat, or AI behavior from a Unity-only retarget/render proof;
- release packaging.

## DK4 Live Actor Diagnostic Source

Current status: source implemented, fixture-validated, built, and deployed through Dragon Knight `0.2.3`; live validation is pending FoA restart and F7/F6 at Ancient Cromlech.

The exact DK4 source lane uses the DK4A `LocationTemplate` GUID `d7b09116519f7564593be62781bee3db`, DK4A `NpcTemplate` GUID `00f608ee051b57748a6a9ed8dae28678`, `CampaignMap_HOS`, and live Cromlech target source `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1`.

The diagnostic owns only F7 activation, exact target lookup, exact template resolution, one no-save native `LocationTemplate.SpawnLocation`, actor `Location.ID` observation, `foa.location:<Location.ID>` actor/target IDs, a diagnostic lease, post-initialization Iron render overlay attachment from the proven `dragonknight_visuals` bundle, and F6/plugin-shutdown cleanup. It does not register live AI, write Rabbit state, run GOAP, call PlayMaker, move, attack, phase, protect companions, create items, write saves, or roam.

## Owned HOS Placement Route

Current status: Dragon Knight `0.2.5` source/fixtures implement the user-approved DK5D native `LocationSpawner` candidate route. This is not the DK4 temporary spawn route and does not call `LocationTemplate.SpawnLocation` directly from Dragon Knight plugin code.

The accepted production lane targets the existing Horns of the South scene `CampaignMap_HOS`, with the first placement at Ancient Cromlech:

- placement ID: `dragon-knight.vaelor.cromlech.center`;
- encounter ID: `dragon-knight.vaelor.cromlech`;
- actor role: `dragon-knight.boss`;
- scene: `CampaignMap_HOS` / Horns of the South;
- template pair: `Spec_DragonKnight_DK4A` / `NPCTemplate_DragonKnight_DK4A`;
- template GUIDs: `d7b09116519f7564593be62781bee3db` / `00f608ee051b57748a6a9ed8dae28678`;
- altar reference: `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`, not boss root and not trigger;
- boss root: `-1795.209|80.243|-2915.928`;
- rotation: `0|-140.441|0`;
- edge witness: `-1808.181|82.621|-2931.862`;
- placement tolerance: `3m`;
- outer wake / inner fight / soft leash / hard leash: `25m` / `5m` / `22m` / `25m`.

DK5D uses native source row `CampaignMap_HOS_merged` `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` at `-1860.15|73.81|-2958.34`, source template `Spec_EnemyMonster_T1_Grindylow` / `fa79aaa0bff59484dab2cf35c5ea805c`, evidence `template-diagnostics.20260803-035222.spawner_refs.csv:line341`. It hooks `LocationSpawner.InitFromAttachment`, replaces that exact candidate array with `Spec_DragonKnight_DK4A`, observes `BaseLocationSpawner.OnLocationSpawned`, rejects no-save actors, and moves the save-owned actor to the boss root with `Location.MoveAndRotateTo`.

Dragon Knight still scans live `Location` models in `CampaignMap_HOS`, requires the exact Dragon Knight `LocationTemplate` and `NpcTemplate`, derives `foa.location:<Location.ID>` from the observed actor, rejects `MarkedNotSaved` / `IsNotSaved` placements, and logs `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED` only after the owned placement is live. It does not register AI, attack, phase, protect companions, create items, or write saves outside native spawner ownership.

## Next Required Decision

Next required work is the DK5D build/deploy/live proof for `dragon-knight.vaelor.cromlech.center`. After the owned placement observer logs the live actor `Location.ID`, the next AI gate may define host mapping from the proved actor and target evidence. Native movement, attacks, companion protection, and real phase behavior remain blocked until that later gate supplies exact capabilities and live combat evidence.
