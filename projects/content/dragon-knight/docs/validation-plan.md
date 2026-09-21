# Dragon Knight Validation Plan

## Required Level

- Current level: Level 3 plugin build/load candidate.
- Reason: The first requested runtime step is to get the standalone Dragon Knight loader into BepInEx before visual transport, AI, follower mechanics, or usable items.

## DK0 Source Inventory

Expected checks:

- Source archive hashes captured.
- Unity package pathnames enumerated.
- Candidate prefabs, meshes, textures, materials, animations, and controllers recorded.
- Licence/entitlement evidence recorded or explicitly marked missing.
- Companion, asset-import, and AI proven methods mapped without adding dependencies.
- No source assets copied into repo.
- No Unity import, build, deployment, FoA launch, or save access.

## Runtime Gate Families

Loader:
- `dotnet build` succeeds against the intended FoA Mono/BepInEx v5 install.
- BepInEx log confirms `Dragon Knight 0.1.0` loaded.
- Config is generated under the Dragon Knight plugin GUID.
- Loader log states visual transport, AI, follower mechanics, and usable items are inactive.

Visual:
- Unity `6000.0.64f1` imports the Dragon Knight package.
- Import report records Fire and Iron weapon prefabs with no missing scripts.
- Visual AssetBundle contains only the approved Iron/Fire character-with-weapon prefabs.
- BepInEx loads the bundle from the Dragon Knight plugin folder.
- Visual proof spawns only on explicit hotkey.
- Spawned visual colliders are disabled.
- Visual proof can switch Iron to Fire and discard the active visual.
- No native actor, AI, attacks, follower behavior, items, save writes, or roaming behavior occurs.

Animation/render proof:
- Unity `6000.0.64f1` imports the Dragon Knight package.
- The proof builder is run with `-dragonKnightBossMoveSetRoot` pointing to `<local-path>`.
- The proof builder imports only the `MCB` FBX clips for animation proof and intentionally excludes `SM_Greatsword.fbx`.
- Required Iron/phase-1, phase transform, Fire/phase-2, and root-motion proof clips are present.
- Imported clips are forced to Humanoid import and must expose Humanoid MCB source avatars.
- The generated controller contains explicit Iron one-handed, Fire one-handed, phase transform, and root-motion proof states.
- The proof prefab assigns that controller to the Dragon Knight Iron and Fire humanoid prefabs.
- Phase 2 dual-sword presentation attaches `SM_DragonKnight_Sword_Iron.prefab` to the Fire phase `hand_l` bone and must record a renderable offhand sword.
- The manifest must record `runtimeAiTouched=false`, `gameDeploymentTouched=false`, and `savePathTouched=false`.
- This proof does not validate runtime movement, attacks, hitboxes, damage, AI, companion protection, save behavior, or real two-phase combat.

Boss:
- DK4A native actor/template proof profile before any `LocationTemplate.SpawnLocation` path;
- candidate-scoped serialized native-template probe;
- explicit baseline/profile approval;
- isolated `NpcTemplate` and `LocationTemplate` authoring and load/release proof only after approval;
- DK5D native `LocationSpawner` candidate route before production placement;
- live owned-placement observer proof before AI handoff;
- exact save-exclusion and cleanup validation;
- focused combat, death, corpse, reward/no-reward, and health-bar-name observations.

Companion:
- command UI route;
- ally/faction behavior;
- follow/stay/defend behavior;
- dismissal, death, cleanup, save/load, and missing-content behavior.
- Avalon Companions method comparison without reusing its runtime.

Weapons:
- item/equipment template proof;
- acquisition route;
- equip, stats, icons, localization, save/reload, and missing-content behavior.

Armor:
- wearable slot policy or visual-only policy;
- renderer/Kandra/body compatibility;
- inventory preview, equipment transitions, save/reload, and missing-content behavior.
- Avalon Awakened method comparison without reusing its runtime.

AI:
- Dragon Knight-owned AI package;
- observation schema;
- blackboard/planning/action-gateway boundary;
- host activation and fail-closed release;
- Avalon AI Runtime method comparison without registering to its runtime by default.
- DK3 source-only package fixtures must prove manifest shape, no runtime actions, offline Runtime V2 registration, and contracts-only assembly references.
- AIR-49 package fixtures must prove boss identity, actor ID policy, owner/lease policy, target rejection, session-only Rabbit schema, GOAP goals/actions/costs/preconditions/effects, capability sufficiency, bounded FoAHost procedure requirements, phase gate, combat gate, Runtime V2 offline registration, and contracts-only assembly references.
- Loader package wiring must validate `DragonKnight.AI.Package.V2` default-off and must log blocked if enabled before DK4 live actor/target/host proof.
- DK4 live diagnostic must be default-off, kill-switch guarded, and limited to actor observation, exact `Location.ID`, target `Location.ID`, host ownership, lease release, activation logging, and cleanup proof.
- DK4 must not add movement, attacks, companion-protect behavior, real phase behavior, item behavior, save writes, roaming, or live package behavior.
- DK4A native actor/template proof must follow `mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`; the isolated Unity authoring/build/two-cycle load-release proof passed and generated Dragon Knight template GUIDs.
- DK4 source work must follow `mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-source-gate-packet.md` and remains blocked until DK4A supplies exact Dragon Knight actor/template source and exact eligible target source evidence exists.
- DK5 owned-placement source work must follow `mods/dragon-knight/docs/gates/DK5-owned-hos-placement-observer-source-gate-packet.md`; the observer must stay read-only and must not move actors, attack, phase, protect companions, create items, register live AI, or write saves.
- DK5B authored HOS placement route-proof work must follow `mods/dragon-knight/docs/gates/DK5B-authored-hos-placement-pack-route-proof-source-gate.md`; it may record route-proof docs/fixtures and run read-only Unity-route discovery, but Unity mutation, authored pack build, deployment, live proof, movement, attacks, phase combat, companion protection, items, live AI dispatch, and save writes remain blocked.
- DK5C authored HOS placement pack source work must follow `mods/dragon-knight/docs/gates/DK5C-authored-hos-placement-pack-source-gate.md`; it must prove `CampaignMap_HOS`, prefab root, marker ID, multiple-NPC-ready manifest, Dragon Knight template GUIDs, no-save/no-temp rejection, and live observer compatibility while keeping Unity mutation, authored pack build, deployment, live proof, movement, attacks, phase combat, companion protection, items, live AI dispatch, and save writes blocked.
- DK5D native `LocationSpawner` candidate work must follow `mods/dragon-knight/docs/decisions/0018-dk5d-native-location-spawner-candidate-route.md`; it may hook the exact approved source row and relocate the save-owned spawned Dragon Knight to the boss root, but movement, attacks, phase combat, companion protection, items, live AI dispatch, and non-native save writes remain blocked.

## Current Validation

- `dotnet build` passed for `mods\dragon-knight\src\DragonKnight.csproj` against `<local-path>`.
- Deploy build copied `DragonKnight.dll`, `README.md`, `CHANGELOG.md`, and `MANIFEST.md` under `<local-path>`.
- BepInEx live log confirmed `Loading [Dragon Knight 0.1.0]`.
- Dragon Knight live log confirmed `Dragon Knight 0.1.0 loaded. Enabled=True. Phase=loader-only; visual transport, AI, follower mechanics, and usable items are not active.`
- BepInEx live log confirmed `Chainloader startup complete`.
- `dotnet build` passed for Dragon Knight `0.2.0` with DK2 visual-only code.
- Unity `6000.0.64f1` built `dragonknight_visuals` from the imported Dragon Knight package.
- Visual bundle manifest was recorded at `mods/dragon-knight/docs/generated-dragon-knight-visual-bundle-manifest.json`.
- Visual bundle SHA-256 is `9C4AE8A3936CCE0BA6FF102CD360AC32D676A27CDC0FD174052D20483274D837`.
- The visual bundle was copied to `<local-path>`.
- Updated DLL copied to `<local-path>`.
- Deployed DLL SHA-256 is `C670F71478C007E945670E225D473220ABF0EE6FC0DA4D68E1E738B2E94D6906`.
- BepInEx live log confirmed `Loading [Dragon Knight 0.2.0]`.
- Dragon Knight live log confirmed the visual bundle loaded from the Dragon Knight plugin folder.
- Dragon Knight live log confirmed `Phase=DK2 visual-only; actor spawning, AI, attacks, follower mechanics, usable items, saves, and roaming are not active.`
- User-reported in-game visual proof worked and the transition looked good. This was not captured as an automated screenshot or per-hotkey log assertion.
- `dotnet build` passed for `mods\dragon-knight\ai-package\src\DragonKnight.AI.Package.V2\DragonKnight.AI.Package.V2.csproj`.
- `dotnet build` passed for `mods\dragon-knight\ai-package\tests\DragonKnight.AI.Package.V2.Fixtures\DragonKnight.AI.Package.V2.Fixtures.csproj`.
- `dotnet run` passed for `DragonKnight.AI.Package.V2.Fixtures`: manifest, observation-only boundary, Runtime V2 offline registration, and contracts-only assembly checks all passed.
- DK4 documentation-only proof gate recorded at `mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-proof.md`.
- DK4 blocked source-gate packet recorded at `mods/dragon-knight/docs/gates/DK4-live-actor-observation-host-ownership-source-gate-packet.md`.
- DK4A read-only native candidate scan recorded at `mods/dragon-knight/docs/research/dk4a-native-actor-template-candidate-scan-2026-08-01.md`.
- DK4A documentation-only native actor/template proof-profile gate recorded at `mods/dragon-knight/docs/gates/DK4A-native-actor-template-proof-profile.md`.
- DK4A Foredweller serialized native-template probe recorded at `mods/dragon-knight/docs/research/dk4a-foredweller-serialized-native-template-probe-2026-08-01.md`.
- DK4A Foredweller generated report recorded at `mods/dragon-knight/docs/generated/dragon-knight-dk4a-foredweller-template-probe-2026-08-01.json`.
- DK4A Foredweller boss profile approval recorded at `mods/dragon-knight/docs/research/dk4a-foredweller-boss-profile-approval-2026-08-01.md`.
- DK4A Decision 0008 recorded at `mods/dragon-knight/docs/decisions/0008-dk4a-foredweller-boss-profile-approval.md`.
- DK4A isolated template proof preparation recorded at `mods/dragon-knight/docs/research/dk4a-isolated-template-proof-preparation-2026-08-01.md`.
- DK4A isolated template proof source-gate packet recorded at `mods/dragon-knight/docs/gates/DK4A-isolated-template-proof-source-gate-packet.md`.
- DK4A template proof preparation JSON recorded at `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-proof-preparation-2026-08-01.json`.
- DK4A Decision 0009 recorded at `mods/dragon-knight/docs/decisions/0009-dk4a-isolated-template-proof-preparation.md`.
- DK4A authoring source recorded at `mods/dragon-knight/tools/template-proof/DragonKnightDK4ATemplateAuthoring.cs`.
- DK4A template build report recorded at `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-build-2026-08-01.json`.
- DK4A template finalized report recorded at `mods/dragon-knight/docs/generated/dragon-knight-dk4a-template-finalized-2026-08-01.json`.
- DK4A isolated template proof closure recorded at `mods/dragon-knight/docs/research/dk4a-isolated-template-proof-2026-08-01.md`.
- DK4A Decision 0010 recorded at `mods/dragon-knight/docs/decisions/0010-dk4a-isolated-template-proof.md`.
- Generated Dragon Knight `NpcTemplate` GUID: `00f608ee051b57748a6a9ed8dae28678`.
- Generated Dragon Knight `LocationTemplate` GUID: `d7b09116519f7564593be62781bee3db`.
- `dotnet build` passed for AIR-49 `mods\dragon-knight\ai-package\src\DragonKnight.AI.Package.V2\DragonKnight.AI.Package.V2.csproj` with 0 warnings and 0 errors.
- `dotnet run` passed for AIR-49 `DragonKnight.AI.Package.V2.Fixtures`; marker `DRAGON_KNIGHT_AIR49_AUTHORITY_PACKET_PASS` printed first and all 15 fixtures passed.
- `dotnet build` passed for Dragon Knight `0.2.1` with default-off AIR-49 package-boundary validation wiring.
- Deploy build copied Dragon Knight `0.2.1`, `DragonKnight.AI.Package.V2.dll`, and `AvalonAI.Contracts.V2.dll` under `<local-path>`.
- Deployed Dragon Knight `0.2.1` DLL SHA-256 is `1A8414BEFE3CF251BC1299DE0DDD7B068B60402B4595845177DE06A288A9D385`.
- Deployed AIR-49 package DLL SHA-256 is `825B29D4CAEEA53E67369378FC8E628FEEA3B158C908FEA66B262C962DDAEFD6`.
- Deployed Avalon AI Contracts V2 DLL SHA-256 is `EFA40427D69E753E698DF9C57FCF0EDBA8022578C196E338D810B66A792A2827`.
- BepInEx live log confirmed `Loading [Dragon Knight 0.2.1]`.
- Dragon Knight live log confirmed the AIR-49 marker with `goals=8 actions=8 capabilities=8 live-runtime-registration=0`.
- Dragon Knight live log confirmed `DRAGON_KNIGHT_BOSS_AI_PACKAGE_WIRED` with `enabled=False`, `kill-switch=True`, and `default-off=1`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK4Diagnostic.Fixtures\DragonKnight.DK4Diagnostic.Fixtures.csproj -c Release` printed the DK4 marker first and passed all 36 fixtures.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed for Dragon Knight `0.2.2` with 0 warnings and 0 errors.
- Deploy build copied Dragon Knight `0.2.2` and the DK4A template pack to FoA plugin/ModService folders.
- Deployed Dragon Knight `0.2.2` DLL SHA-256 is `EAE481DA595754D868E75138A156537CAD9DFE7988397BF78948A89B81D1BF01`.
- Deployed DK4A root/platform catalogue SHA-256 is `6D91697F8EFF45E0DF8599958519BEF7162E371433D9321723F6C73AA0B25558`.
- Deployed DK4A bundle SHA-256 is `59A9E14CFDBDC9CDA05CE39650F4CD7303242E6C82338F43B467A8B494446278`.
- Dragon Knight config is armed for DK4 on next restart with `DK4Diagnostic.Enabled=true`, `KillSwitch=false`, `ActivationHotkey=F7`, `ReleaseHotkey=F6`, `MaximumObservationSeconds=10`, and `AllowNativeSpawn=true`; config SHA-256 is `9701AE8C8E3C8803FECB519F3339FE27B644035057625B267E4EF1A7EB12CF87`.
- Live log check after deployment still showed the already-running FoA process had loaded Dragon Knight `0.2.1`; `0.2.2` live-load and DK4 F7/F6 pass are pending restart.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK4Diagnostic.Fixtures\DragonKnight.DK4Diagnostic.Fixtures.csproj -c Release` passed all 36 fixtures after adding the DK4 render-overlay assertions.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed for Dragon Knight `0.2.3` with 0 warnings and 0 errors.
- Deploy build copied Dragon Knight `0.2.3` to the FoA plugin folder and recopied the DK4A template pack.
- DK4 `0.2.3` now requires the proven `dragonknight_visuals` Iron render prefab before spawning, attaches `DragonKnightDk4RenderOverlay_Iron` after native initialization and ownership lease, hides native Unity/Kandra bootstrap renderers, and logs `DRAGON_KNIGHT_DK4_RENDER_OVERLAY_PASS` before the DK4 actor-observation pass marker.
- Deployed Dragon Knight `0.2.3` DLL SHA-256 is `6FF8C2FEC2D8F416A86345680798B001568DE04A3B95A1B977B68F521C8D9F8E`.
- Deployed `dragonknight_visuals` SHA-256 remains `9C4AE8A3936CCE0BA6FF102CD360AC32D676A27CDC0FD174052D20483274D837`.
- Dragon Knight config remains armed for DK4; config SHA-256 is now `A28672EF7525C03A93204E5705C29FA13E02D4A7DD85E6CAD98D82068A7AFEA7`.
- Live log check after `0.2.3` deployment still showed the already-running FoA process had loaded Dragon Knight `0.2.2`; `0.2.3` live-load and DK4 render-overlay F7/F6 pass are pending restart.
- Unity `6000.0.64f1` reran `DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS` after switching to the supplied `MCB` boss move-set source.
- Retarget/render proof manifest SHA-256 is `AA7DD7B69AD052B7AF67BA24394E53A5606DA27D46BDC5BC3C5C9DF778421AAA`.
- Proof bundle SHA-256 is `18A140AAD734EC835A48CFED98F8A5A2C5F2D50D0B9ACAE5FA8E04616E019A22`.
- Manifest records 92 imported MCB FBX animation assets, 42 required boss animations present, 0 failed animations, 0 Humanoid import failures, no generic third-party clip references, and phase-2 dual-sword proof passed with the offhand Iron sword attached to `hand_l`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS` first and passed all 24 fixtures for the read-only owned HOS placement observer source gate.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed for Dragon Knight `0.2.4` with 0 warnings and 0 errors.
- DK5B route-proof gate added as a repo-local source gate. Read-only discovery found the Unity project path but did not prove an existing Dragon Knight placement-pack source.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_DK5B_AUTHORED_HOS_PLACEMENT_ROUTE_PROOF_SOURCE_GATE_PASS` first and passed all 18 fixtures for the DK5B route-proof source gate.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release`; the DK5 observer marker printed first and all 24 fixtures passed.
- Re-ran `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"`; build passed with 0 warnings and 0 errors.
- DK5C authored HOS placement pack source packet added as a repo-local source gate. It records scene `CampaignMap_HOS`, Core anchor `hos.ancient-cromlech.center`, prefab root `DK5C_DragonKnight_Cromlech_PlacementRoot`, marker ID `DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE`, multiple-NPC-ready manifest shape, Dragon Knight template GUIDs, no-save/no-temp rejection, and live observer compatibility.
- First DK5C fixture run failed one boundary-token assertion because the decision/gate did not contain the exact phrase does not create or edit `CampaignMap_HOS`. Decision `0017` was updated with that explicit boundary.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5CAuthoredPlacementPack.Fixtures\DragonKnight.DK5CAuthoredPlacementPack.Fixtures.csproj -c Release` printed `DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=23 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0` first and passed all 23 fixtures after the boundary update.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release`; all 24 DK5 owned-placement observer fixtures passed.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release`; all 18 DK5B authored placement-route fixtures passed.
- Re-ran `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"`; build passed with 0 warnings and 0 errors.
- DK5C altar/root split correction added after dump `20260803-032730`: altar reference is `AltarInteract` / `CM_Stonehenge_0_2258891627046429048_1` at `-1792.661|79.942|-2912.844`, while boss root is `-1795.209|80.243|-2915.928` with rotation `0|-140.441|0`.
- Expected corrected DK5C fixture marker: `DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=25 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0`.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5CAuthoredPlacementPack.Fixtures\DragonKnight.DK5CAuthoredPlacementPack.Fixtures.csproj -c Release`; all 25 corrected DK5C authored placement-pack fixtures passed.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release`; all 26 DK5 owned-placement observer fixtures passed.
- Re-ran `dotnet run --project mods\dragon-knight\tests\DragonKnight.DK5BPlacementRoute.Fixtures\DragonKnight.DK5BPlacementRoute.Fixtures.csproj -c Release`; all 18 DK5B authored placement-route fixtures passed.
- Re-ran Avalon Core anchor and overlay fixtures; all 17 HoS named-location fixtures and all 20 custom scene-placement overlay fixtures passed.
- Re-ran `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"`; build passed with 0 warnings and 0 errors.
- DK5D native `LocationSpawner` candidate route override added for Dragon Knight `0.2.5`.
- DK5D exact native source anchor: `CampaignMap_HOS_merged` `/SpawnerSingle_EnemyMonster_T1_Grindylow_01` at `-1860.15|73.81|-2958.34`, source template `Spec_EnemyMonster_T1_Grindylow` / `fa79aaa0bff59484dab2cf35c5ea805c`, evidence `template-diagnostics.20260803-035222.spawner_refs.csv:line341`.
- Expected DK5D owned-placement marker: `DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS fixtures=32 owner=dragon-knight.boss.host actor-source=native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot scene=CampaignMap_HOS placements=1 multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0`.
- `dotnet run --project mods\dragon-knight\tests\DragonKnight.OwnedPlacement.Fixtures\DragonKnight.OwnedPlacement.Fixtures.csproj -c Release` printed the DK5D marker first and passed all 32 fixtures.
- `dotnet build mods\dragon-knight\src\DragonKnight.csproj -c Release -m:1 -p:FoAGameRoot="<local-path>"` passed for Dragon Knight `0.2.5` with 0 warnings and 0 errors.
- Deploy build copied Dragon Knight `0.2.5` to `<local-path>`.
- Deployed Dragon Knight `0.2.5` DLL SHA-256 is `D8666413C4B320C1E6FBCCA70E5D2E587435003D4703FCFA7C02DB9972E5177F`.

## Not Run

- Automated or screenshot-backed F8/F10/F9 visual proof.
- DK4 `0.2.3` live-load after restart.
- DK4 `0.2.3` render-overlay live pass after F7 at Ancient Cromlech.
- DK4 `0.2.3` live actor observation, ownership lease, target identity, activation, or cleanup validation.
- DK4 F6 release pass after `0.2.3` live actor insertion.
- DK5D live proof markers: `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_INSTALLED`, `DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_PLACED`, and `DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED`.
- Dragon Knight AI host mapping or live Runtime registration.
- Dragon Knight AI observation, live planning, runtime action, movement, attack, defend, companion-protect, or phase behavior.
- Runtime actor/item/equipment validation.
- Release package validation.
