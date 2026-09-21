# Dragon Knight Test Notes

## 2026-08-01

- Level 3 loader proof passed.
- Built `mods\dragon-knight\src\DragonKnight.csproj` in Release against `<local-path>`.
- Deployed to `<local-path>`.
- BepInEx log confirmed `Loading [Dragon Knight 0.1.0]`.
- Plugin log confirmed `Dragon Knight 0.1.0 loaded. Enabled=True. Phase=loader-only; visual transport, AI, follower mechanics, and usable items are not active.`
- BepInEx log confirmed `Chainloader startup complete`.
- No visual asset, AI, follower mechanics, usable items, actor spawn, save write, or release package was tested.

## 2026-08-01 DK2 Import Inspection

- Unity `6000.0.64f1` imported `dragonknight_2022333f1.unitypackage`.
- Report: `mods/dragon-knight/docs/generated-dragon-knight-unity-inspection-report.json`.
- Report SHA-256: `588EFF995F5159BC4B9DA1E2DAC7258CC898DC4C2DD1D887374926E3479FCDFB`.
- Imported assets: 102.
- Prefabs: 8.
- Animation clips: `Pose_Check` only.
- Controller states: 0.
- Character-with-weapon prefabs: Fire and Iron verified with zero missing scripts.

## 2026-08-01 DK2 Visual Bundle Build

- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -p:FoAGameRoot='<local-path>` passed for Dragon Knight `0.2.0`.
- Unity `6000.0.64f1` built `dragonknight_visuals` from only the approved Iron/Fire character-with-weapon prefabs.
- Bundle manifest: `mods/dragon-knight/docs/generated-dragon-knight-visual-bundle-manifest.json`.
- Bundle SHA-256: `9C4AE8A3936CCE0BA6FF102CD360AC32D676A27CDC0FD174052D20483274D837`.
- Built DLL SHA-256: `C670F71478C007E945670E225D473220ABF0EE6FC0DA4D68E1E738B2E94D6906`.
- The visual bundle copied to the live Dragon Knight plugin folder.
- The updated DLL copied to the live Dragon Knight plugin folder after Fall of Avalon closed.
- BepInEx log confirmed `Loading [Dragon Knight 0.2.0]`.
- BepInEx log confirmed `Dragon Knight visual bundle loaded: <local-path>`.
- Plugin log confirmed `Phase=DK2 visual-only; actor spawning, AI, attacks, follower mechanics, usable items, saves, and roaming are not active.`
- User reported the in-game visual proof works and the transition looked good. This is recorded as user-observed validation, not automated screenshot or per-hotkey log evidence.

## 2026-08-01 DK3 Source-Only AI Package

- Added `DragonKnight.AI.Package.V2` as a source-only `AvalonAI.Contracts.V2` package assembly.
- The package reserves Dragon Knight boss and companion actor roles.
- The package declares no goals, actions, capabilities, procedure requirements, persistent keys, host mapping, or live runtime registration.
- `dotnet build mods\dragon-knight\ai-package\src\DragonKnight.AI.Package.V2\DragonKnight.AI.Package.V2.csproj -c Release` passed with 0 warnings and 0 errors.
- `dotnet build mods\dragon-knight\ai-package\tests\DragonKnight.AI.Package.V2.Fixtures\DragonKnight.AI.Package.V2.Fixtures.csproj -c Release` passed with 0 warnings and 0 errors.
- `dotnet run --project mods\dragon-knight\ai-package\tests\DragonKnight.AI.Package.V2.Fixtures\DragonKnight.AI.Package.V2.Fixtures.csproj -c Release` passed all 4 fixtures:
  - manifest declares source-only Dragon Knight roles;
  - package exposes no runtime actions before combat evidence;
  - package registers with Runtime V2 offline;
  - package assembly remains Contracts V2 only.

## 2026-08-02 AIR-49 Boss AI Package Contract And Loader Wire

- Updated `DragonKnight.AI.Package.V2` to package ID `dragon-knight.boss.ai.v2`, assembly `DragonKnight.AI.Package.V2`, and role `dragon-knight.boss`.
- The package declares fail-closed actor, owner/lease, target, Rabbit, GOAP, capability, FoAHost bridge, phase, and combat-gate policy checks.
- `dotnet build mods\dragon-knight\ai-package\src\DragonKnight.AI.Package.V2\DragonKnight.AI.Package.V2.csproj -c Release -m:1` passed with 0 warnings and 0 errors.
- `dotnet run --project mods\dragon-knight\ai-package\tests\DragonKnight.AI.Package.V2.Fixtures\DragonKnight.AI.Package.V2.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_AIR49_AUTHORITY_PACKET_PASS` first and passed all 15 fixtures.
- Wired Dragon Knight `0.2.1` to reference and validate the package boundary at loader startup with `DragonKnightBossAI.Enabled=false` and `DragonKnightBossAI.KillSwitch=true` defaults.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed with 0 warnings and 0 errors.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true` passed with 0 warnings and 0 errors.
- Deployment check confirmed:
  - `DragonKnight.dll` SHA-256 `1A8414BEFE3CF251BC1299DE0DDD7B068B60402B4595845177DE06A288A9D385`;
  - `DragonKnight.AI.Package.V2.dll` SHA-256 `825B29D4CAEEA53E67369378FC8E628FEEA3B158C908FEA66B262C962DDAEFD6`;
  - `AvalonAI.Contracts.V2.dll` SHA-256 `EFA40427D69E753E698DF9C57FCF0EDBA8022578C196E338D810B66A792A2827`.
- BepInEx live log confirmed `Loading [Dragon Knight 0.2.1]`.
- Dragon Knight live log confirmed `DRAGON_KNIGHT_AIR49_AUTHORITY_PACKET_PASS ... goals=8 actions=8 capabilities=8 live-runtime-registration=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0`.
- Dragon Knight live log confirmed `DRAGON_KNIGHT_BOSS_AI_PACKAGE_WIRED package=dragon-knight.boss.ai.v2 ... enabled=False kill-switch=True default-off=1 live-runtime-registration=0`.
- Dragon Knight live log confirmed `Dragon Knight 0.2.1 loaded. Enabled=True. VisualProofEnabled=True. BossAIEnabled=False. BossAIKillSwitch=True.`
- Live Avalon AI Runtime registration, actor observation, target acquisition, movement, attacks, phase combat, companion protection, items, roaming, Rabbit writes, and save writes remain blocked.

## 2026-08-02 DK4 Render Overlay Source And Deployment

- Updated Dragon Knight to `0.2.3`.
- DK4 now requires the proven `dragonknight_visuals` Iron character-with-weapon prefab before F7 can spawn the no-save actor.
- After native actor initialization, DK4 attaches `DragonKnightDk4RenderOverlay_Iron`, disables only the overlay colliders, hides native Unity/Kandra bootstrap renderers, then logs `DRAGON_KNIGHT_DK4_RENDER_OVERLAY_PASS` before `DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_PASS`.
- No movement, attack, damage, death, loot, item, armor, companion, Rabbit, GOAP, PlayMaker, Blaze, live AI registration, save, phase-combat, or roaming behavior was added.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK4Diagnostic.Fixtures\DragonKnight.DK4Diagnostic.Fixtures.csproj -c Release` printed the DK4 marker first and passed all 36 fixtures.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed with 0 warnings and 0 errors.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true` passed and deployed Dragon Knight `0.2.3`.
- Deployed `DragonKnight.dll` SHA-256: `6FF8C2FEC2D8F416A86345680798B001568DE04A3B95A1B977B68F521C8D9F8E`.
- Deployed `DragonKnight.AI.Package.V2.dll` SHA-256 remains `825B29D4CAEEA53E67369378FC8E628FEEA3B158C908FEA66B262C962DDAEFD6`.
- Deployed `AvalonAI.Contracts.V2.dll` SHA-256 remains `EFA40427D69E753E698DF9C57FCF0EDBA8022578C196E338D810B66A792A2827`.
- Deployed `dragonknight_visuals` SHA-256 remains `9C4AE8A3936CCE0BA6FF102CD360AC32D676A27CDC0FD174052D20483274D837`.
- DK4A template catalogue JSON/hash/bundle SHA-256 remain `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`, `E862C7E8A8319E076082CBFC95C61887C7D1C0CFB9BBE2A436F4B789F7477B69`, and `59A9E14CFDBDC9CDA05CE39650F4CD7303242E6C82338F43B467A8B494446278`.
- Current BepInEx log after deployment still showed the already-running FoA process had loaded Dragon Knight `0.2.2`; `0.2.3` live-load and render-overlay F7/F6 pass are pending restart.

## 2026-08-02 Custom Boss Move-Set Proof Source

- User supplied dedicated boss move-set source: `<local-path>`.
- Read-only inventory found 93 `.fbx` files, 32 `.ma` files, `MCB.zip`, and `SM_Greatsword.fbx`.
- Updated `DragonKnightRetargetRenderProofBuilder` to require `-dragonKnightBossMoveSetRoot`, import the `MCB` FBX clips, force Humanoid import, verify Humanoid MCB source avatars, and generate controller states for Iron one-handed, phase transform, Fire one-handed, and selected root-motion proof clips.
- First Unity rerun blocked when the builder attempted direct `CopyFromOther` avatar import from the Dragon Knight avatar onto the MCB files: Unity reported `Copied Avatar Rig Configuration mis-match` and `Transform 'SK_Mannequin' not found in HumanDescription`. The builder was corrected to use Humanoid source-avatar retargeting instead.
- Second Unity rerun passed with marker `DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS`.
- Manifest: `mods/dragon-knight/docs/generated-dragon-knight-retarget-render-proof-manifest.json`.
- Manifest SHA-256 after the MCB-only proof: `0F895BB4F1FC618D4FD3EDCBE2BC4CCC54A47F4DF1AC3EBC6D28BF4758EB56D5`.
- Bundle: `<local-path>`.
- Bundle SHA-256 after the MCB-only proof: `3A4209CC06371399F204491844D5450C2E4E93680C04A4402204E06E638F87B1`.
- Manifest records 92 imported MCB FBX animation assets, 42 required boss animations present, 0 failed animations, 0 Humanoid import failures, and no stale `Kevin Iglesias`, `HumanM@`, `Blink`, or `Blaze AI` animation references.
- Added phase-2 dual-sword presentation proof:
  - mainhand source remains `SK_Dragon_Knight_Fire_Weapon.prefab`;
  - offhand source is `SM_DragonKnight_Sword_Iron.prefab`;
  - offhand object `Phase2_Offhand_IronSword` attaches to `Fire_Phase_2_DualSword/root/pelvis/spine_01/spine_02/spine_03/clavicle_l/upperarm_l/lowerarm_l/hand_l`;
  - offhand render proof has 1 renderer, 1 mesh renderer, and 0 skinned renderers;
  - local transform is position `0|0|0`, rotation `0|0|0|1`, scale `1|1|1`.
- Unity rerun after dual-sword proof passed with marker `DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS`.
- Manifest SHA-256 after dual-sword proof: `AA7DD7B69AD052B7AF67BA24394E53A5606DA27D46BDC5BC3C5C9DF778421AAA`.
- Bundle SHA-256 after dual-sword proof: `18A140AAD734EC835A48CFED98F8A5A2C5F2D50D0B9ACAE5FA8E04616E019A22`.
- `SM_Greatsword.fbx` remains excluded because this boss proof uses the Dragon Knight one-handed sword prefab.
- No runtime actor movement, attack, damage, hitbox, AI, companion-protect, phase-combat, item, save, or roaming behavior was run.

## 2026-08-02 DK4 Live Actor Diagnostic Source And Deployment

- Added Dragon Knight `0.2.2` DK4 diagnostic source:
  - `src/DK4/DragonKnightDk4DiagnosticOptions.cs`;
  - `src/DK4/DragonKnightDk4DiagnosticHost.cs`;
  - `src/DK4/DragonKnightDk4ActorObserver.cs`;
  - `src/DK4/DragonKnightDk4OwnershipLease.cs`;
  - `src/DK4/DragonKnightDk4Marker.cs`.
- Wired `Plugin` to construct, tick, and dispose `DragonKnightDk4DiagnosticHost`.
- Updated `DragonKnight.csproj` to reference FoA native model assemblies and deploy the DK4A template pack to FoA ModService `DragonKnightDK4ATemplate`, including direct mod-root catalogue JSON/hash copies and `StandaloneWindows64` files.
- Added `mods/dragon-knight/tests/DragonKnight.DK4Diagnostic.Fixtures`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK4Diagnostic.Fixtures\DragonKnight.DK4Diagnostic.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_DK4_SOURCE_GATE_BLOCKED` first and passed all 36 fixtures.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed with 0 warnings and 0 errors.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>" -p:DeployOnBuild=true` passed with 0 warnings and 0 errors.
- Deployment check confirmed:
  - `DragonKnight.dll` SHA-256 `EAE481DA595754D868E75138A156537CAD9DFE7988397BF78948A89B81D1BF01`;
  - `DragonKnight.AI.Package.V2.dll` SHA-256 `825B29D4CAEEA53E67369378FC8E628FEEA3B158C908FEA66B262C962DDAEFD6`;
  - `AvalonAI.Contracts.V2.dll` SHA-256 `EFA40427D69E753E698DF9C57FCF0EDBA8022578C196E338D810B66A792A2827`.
- DK4A template pack deployment check confirmed:
  - root `catalog_dragon-knight-dk4a-template-v1.json` SHA-256 `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`;
  - root `catalog_dragon-knight-dk4a-template-v1.hash` SHA-256 `E862C7E8A8319E076082CBFC95C61887C7D1C0CFB9BBE2A436F4B789F7477B69`;
  - platform `catalog_dragon-knight-dk4a-template-v1.json` SHA-256 `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`;
  - platform `catalog_dragon-knight-dk4a-template-v1.hash` SHA-256 `E862C7E8A8319E076082CBFC95C61887C7D1C0CFB9BBE2A436F4B789F7477B69`;
  - platform bundle SHA-256 `59A9E14CFDBDC9CDA05CE39650F4CD7303242E6C82338F43B467A8B494446278`.
- Dragon Knight config was armed for the next FoA restart:
  - `DK4Diagnostic.Enabled=true`;
  - `DK4Diagnostic.KillSwitch=false`;
  - `DK4Diagnostic.ActivationHotkey=F7`;
  - `DK4Diagnostic.ReleaseHotkey=F6`;
  - `DK4Diagnostic.MaximumObservationSeconds=10`;
  - `DK4Diagnostic.AllowNativeSpawn=true`;
  - config SHA-256 `9701AE8C8E3C8803FECB519F3339FE27B644035057625B267E4EF1A7EB12CF87`.
- Live log check found FoA was still running Dragon Knight `0.2.1` from before deployment:
  - `Loading [Dragon Knight 0.2.1]`;
  - no `DRAGON_KNIGHT_DK4_DIAGNOSTIC_CONFIG`;
  - no `DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_PASS`.
- DK4 live actor pass is pending a fresh FoA restart with Dragon Knight `0.2.2`, config arming, F7 at Ancient Cromlech, and F6 cleanup.
- Movement, attacks, phase combat, companion protection, items, roaming, Rabbit writes, GOAP actions, live Avalon AI Runtime registration, and save writes remain blocked.

## 2026-08-01 DK4 Live Actor Observation Gate

- Added a documentation-only DK4 proof gate for default-off Dragon Knight live actor observation and host ownership evidence.
- The gate requires exact Dragon Knight actor source, actor `Location.ID`, target `Location.ID`, ownership lease, FoAHost activation trigger, kill switch, and cleanup proof before source implementation.
- No DK4 source, build, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, save, or roaming behavior was run.

## 2026-08-01 DK4 Source Gate Packet

- Added a blocked DK4 source-gate packet defining exact source files, F7/F6 activation, 36 fixtures, blocked and pass marker text, build/deploy plan, live log checks, cleanup checks, and stop conditions.
- Source implementation remains blocked because Dragon Knight still lacks exact `LocationTemplate`/`NpcTemplate` or existing live `Location` actor source evidence, and lacks exact eligible target source evidence.
- No DK4 source, build, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, save, or roaming behavior was run.

## 2026-08-01 DK4A Native Actor/Template Candidate Scan

- Added a read-only native actor/template candidate scan for the Dragon Knight blocker.
- The latest local template diagnostic dump shows Foredweller T6 Knight, Highwayman 1H, Drowned Knight, Skeleton Archmage, Green Knight, and Summon Knight candidate rows.
- No Dragon Knight native baseline was selected.
- Added a documentation-only DK4A native actor/template proof-profile gate.
- Preferred next evidence path is a serialized native-template probe for Foredweller T6 Knight, or explicit approval to use the already serialized Highwayman 1H values as a disposable Dragon Knight profile.
- No template authoring, runtime source, build, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, armor, save, roaming, or `LocationTemplate.SpawnLocation` behavior was run.

## 2026-08-01 DK4A Foredweller Serialized Native-Template Probe

- Generated `mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json` from read-only Unity YAML/meta parsing of the isolated native project.
- Generated report SHA-256: `FAB32552B56DE4BFC5FC8B367A89FD2EDCC01672AB913785CC9A6579FFDC417F`.
- Added `mods/dragon-knight/docs/research/dk4a-foredweller-serialized-native-template-probe-2026-08-01.md`.
- The probe records Foredweller T6 Knight NPC GUID `439a2dc3cf64e4e4aa792be5b4034cf1`, location GUID `24ee850d0ffdbf64ea2b07b6a718d63b`, native visual address `b4d3a4e9a58fb1c4ea4c239f267faa56`, simplified dead-body address `cee084d53322ec04ca39752e9754314a`, and location component order ending in `CustomCombatAttachment`, `AliveAudioAttachment`, and `MarkerAttachment`.
- No Dragon Knight native baseline was selected, and no template authoring, runtime source, build, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, armor, save, roaming, or `LocationTemplate.SpawnLocation` behavior was run.

## 2026-08-01 DK4A Foredweller Boss Profile Approval

- Added `mods/dragon-knight/docs/research/dk4a-foredweller-boss-profile-approval-2026-08-01.md`.
- Added `mods/dragon-knight/docs/decisions/0008-dk4a-foredweller-boss-profile-approval.md`.
- The Foredweller T6 Knight pair is now selected as the disposable Dragon Knight proof profile:
  - NPC template GUID `439a2dc3cf64e4e4aa792be5b4034cf1`;
  - location template GUID `24ee850d0ffdbf64ea2b07b6a718d63b`.
- Boss-level stats mean the exact serialized Foredweller T6 Knight stat values from the probe: level `40`, health `10000`, stamina `250`, stamina regen `5`, melee/ranged/magic damage `113`/`50`/`60`, weight `300`, poise `1000`, and XP level/tier/reward `8`/`0`/`640`.
- No higher numeric stat overrides, custom loot, custom corpse policy, native Dragon Knight visual root, template authoring, runtime source, build, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, armor, save, roaming, or `LocationTemplate.SpawnLocation` behavior was run.

## 2026-08-01 DK4A Isolated Template Proof Preparation

- Added `mods/dragon-knight/docs/research/dk4a-isolated-template-proof-preparation-2026-08-01.md`.
- Added `mods/dragon-knight/docs/gates/DK4A-isolated-template-proof-source-gate-packet.md`.
- Added `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-proof-preparation-2026-08-01.json`.
- Added `mods/dragon-knight/docs/decisions/0009-dk4a-isolated-template-proof-preparation.md`.
- The preparation locks source Foredweller hash guards, target Dragon Knight template asset names, address patterns ending in generated GUIDs, output root `<local-path>`, required markers, and stop conditions.
- Read-only YAML checks confirmed the Foredweller source NPC and location templates expose `metadata.notes`, allowing the later authoring script to use a Dragon Knight proof marker.
- No Unity project mutation, template authoring, final Dragon Knight template GUID generation, Addressables build, load/release proof, runtime source, deploy, game launch, actor observation, host mapping, movement, attack, phase combat, companion-protect, item, armor, save, roaming, or `LocationTemplate.SpawnLocation` behavior was run.

## 2026-08-01 DK4A Isolated Template Proof

- Added `mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`.
- Copied the authoring source to the isolated Unity project as `Assets/Editor/DragonKnightDK4ATemplateAuthoring.cs`; copied source SHA-256 matched repo source `C9EBB090B1B16F7CCBFBB59D256B660BEB8E4A04CFBAE6EFC1485E83C295BF88`.
- Unity `6000.0.64f1` author/build stage passed with marker `DRAGON_KNIGHT_DK4A_TEMPLATE_BUILD_PASS explicitAssets=2; npcTemplateCreated=true; locationTemplateCreated=true; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false`.
- Unity `6000.0.64f1` finalized two-cycle load/release stage passed with marker `DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false`.
- Added `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-build-2026-08-01.json`, SHA-256 `A2BC89842C7CE55DE8039C3479E255D1D0ED47F6D2BFE6FCE82E8324F75FF285`.
- Added `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-finalized-2026-08-01.json`, SHA-256 `9096E105E398B6F2B907AE13C5B8C8E8334DD8327CE186759D19043139EC34E4`.
- Generated `NPCTemplate_DragonKnight_DK4A` GUID `00f608ee051b57748a6a9ed8dae28678` with address `dragon-knight/boss/dk4a/npc-template--00f608ee051b57748a6a9ed8dae28678`.
- Generated `Spec_DragonKnight_DK4A` GUID `d7b09116519f7564593be62781bee3db` with address `dragon-knight/boss/dk4a/location-template--d7b09116519f7564593be62781bee3db`.
- Finalized report validates exact approved Foredweller boss stats, location component order, `template` labels, absent `templateSO`, two load cycles, released handles, and unchanged guarded Foredweller source hashes.
- No runtime source, deployment, FoA launch, save access, actor construction, `LocationTemplate.SpawnLocation`, movement, attack, phase combat, companion-protect, item, armor, roaming, or AI behavior was run.

## 2026-08-03 DK5B Authored HOS Placement Route Proof

- Added DK5B route-proof research, decision, and source gate for the authored Dragon Knight-owned HOS placement pack path.
- Read-only discovery confirmed the Unity project root exists at `<local-path>`, but did not prove an existing Dragon Knight placement-pack authoring source under `Assets`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release` printed the DK5B marker first and passed all 18 fixtures.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release`; all 24 DK5 owned-placement observer fixtures passed.
- Re-ran `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"`; build passed with 0 warnings and 0 errors.
- DK5B keeps Unity project mutation, authored pack build, FoA deployment, live observation, movement, attacks, phase combat, companion protection, items, live AI dispatch, save writes, persistence, cleanup, and rollback blocked until exact route proof is supplied and separately authorized.

## 2026-08-03 DK5C Authored HOS Placement Pack Source

- Added DK5C authored HOS placement pack source contract, manifest, gate, decision, research note, and fixture project.
- The packet proves scene target `CampaignMap_HOS`, Core anchor `hos.ancient-cromlech.center`, prefab root `DK5C_DragonKnight_Cromlech_PlacementRoot`, marker ID `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE`, multiple-NPC-ready manifest shape, Dragon Knight template GUIDs, no-save/no-temp rejection, and live observer compatibility.
- First DK5C fixture run failed one boundary-token assertion because the decision/gate did not contain the exact phrase does not create or edit `CampaignMap_HOS`. Decision `0017` was updated with that explicit boundary.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5CAuthoredPlacementPack.Fixtures\DragonKnight.DK5CAuthoredPlacementPack.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=23 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0` first and passed all 23 fixtures after the boundary update.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release`; all 24 DK5 owned-placement observer fixtures passed.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release`; all 18 DK5B authored placement-route fixtures passed.
- Re-ran `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"`; build passed with 0 warnings and 0 errors.
- Unity mutation, pack build, deployment, live proof, movement, attacks, phase combat, companion protection, items, live AI dispatch, save writes, persistence, cleanup, and rollback remain blocked.

## 2026-08-03 DK5C Altar Reference / Boss Root Split

- Corrected the DK5C packet after dump `20260803-032730` proved the old coordinate is live `AltarInteract`, not the boss root.
- Altar reference: `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`; not boss root; not trigger.
- Boss root: `-1795.209|80.243|-2915.928`, rotation `0|-140.441|0`, derived by offsetting `4m` from the altar reference toward dumped hero position `-1801.847|81.026|-2923.964`.
- Expected corrected marker: `DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=25 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5CAuthoredPlacementPack.Fixtures\DragonKnight.DK5CAuthoredPlacementPack.Fixtures.csproj -c Release` printed the corrected marker first and passed all 25 fixtures.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release` passed all 26 fixtures.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release` passed all 18 fixtures.
- Avalon Core `HosNamedLocation` and `CustomScenePlacementOverlay` fixtures passed all 17 and 20 fixtures respectively.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed with 0 warnings and 0 errors.

## 2026-08-04 DK5D Native LocationSpawner Candidate Route

- Added DK5D native `LocationSpawner` candidate placement source after the user approved the route override.
- Selected exact native source anchor: `CampaignMap_HOS_merged` `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` at `-1860.15|73.81|-2958.34`, source template `Spec_EnemyMonster_T1_Grindylow` / `fa79aaa0bff59484dab2cf35c5ea805c`, evidence `template-diagnostics.20260803-035222.spawner_refs.csv:line341`.
- Dragon Knight `0.2.5` now patches `LocationSpawner.InitFromAttachment`, replaces that exact candidate array with `Spec_DragonKnight_DK4A`, observes `BaseLocationSpawner.OnLocationSpawned`, rejects no-save actors, and moves the save-owned actor to `-1795.209|80.243|-2915.928` with `Location.MoveAndRotateTo`.
- Expected owned-placement fixture marker: `DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS fixtures=32 owner=dragon-knight.boss.host actor-source=native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot scene=CampaignMap_HOS placements=1 multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release` printed the DK5D source gate marker first and passed all 32 fixtures.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed with 0 warnings and 0 errors.
- Deploy build copied Dragon Knight `0.2.5` to `<local-path>`; local and deployed DLL SHA-256 both read `D8666413C4B320C1E6FBCCA70E5D2E587435003D4703FCFA7C02DB9972E5177F`.
- Live game log check did not yet show DK5D markers; only older Dragon Knight visual/AIR-49 markers were present. Live Cromlech proof still requires loading the installed build in game and observing `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_INSTALLED`, `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_PLACED`, and `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.
