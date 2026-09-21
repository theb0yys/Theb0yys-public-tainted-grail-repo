# Avalon Broodmother Companion Test Notes

No Codex-controlled FoA runtime test has been run unless a dated section says otherwise. Live observations below are from user feedback/screenshots and BepInEx log reads.

## 2026-08-14 Spider V2 frame locomotion correction

- User screenshots `Screenshot (5269).png`, `Screenshot (5271).png`, and `Screenshot (5272).png` were written at local `2026-08-14 01:29:36`, `01:29:38`, and `01:29:39` respectively and showed the spawned Spider dragging in reposition instead of continuously locomoting.
- Matching live command CSV rows in `broodmother-command-log.csv` proved the deployed authored-stage build advanced `move.role-position` only in procedure poll chunks: initial rows moved `0.088000007` metres from a `0.0160000008` second delta, while later poll rows jumped `1.89896083` to `2.47827649` metres from `0.345265597` to `0.450595707` second deltas before reporting success.
- Source inspection found `UnitySpawnedSpiderCommandSink.Poll()` was still the movement integrator. `Plugin.Update()` synced bridge state every Unity frame but did not tick active Spider commands, so active move/leap motion could stand still between sparse procedure polls.
- Added frame-level command ticking through `AvalonFoASpiderBlazeCommandBridge`, `AvalonFoASpiderBlazeCommandExecutor`, `AvalonFoASpiderActorController`, and the spawned Unity sink. `Plugin.Update()` now calls the bridge tick after state sync, and `Poll()` now observes active/completed motion instead of being the only motion advance path.
- Motion telemetry now records `motionUpdateSource` and `motionFrameAdvanced` so fresh rows can distinguish Unity-frame advancement from command-poll completion.
- FoAHost Mono V2 fixture execution passed all 242 migration fixtures. The run emitted the existing optional Unity reference warnings and existing `System.Text.Json` conflict warnings, with no fixture failures.
- Broodmother companion release build passed with `0` warnings and `0` errors using explicit FoA, Unity, Rabbit, GOAP, and PlayMaker dependency paths.
- The first closed-game deploy wrapper failed before backup/copy because PowerShell rejected a parent-path validation command. The corrected closed-game deploy passed with timestamped backups under `_codex_backups\20260814-spider-frame-locomotion-014439`.
- Installed DLL hashes match the rebuilt outputs: `AvalonAI.FoAHost.Mono.V2.dll` `DFF5C5E5DD43D4C2B72C04F39D918E639FDA51E24ADCBBF623F4B9A611C8A108` in both live plugin folders, and `AvalonBroodmotherCompanion.dll` `F6107735DC87E0A42C76F7BE88ECA1619211DA6B685B5FE49FE873CF541F212F`.
- A post-deploy process check found no matching FoA process. No fresh FoA launch, live command-log row, or in-game visual pass has been run yet for this frame-locomotion correction.

## 2026-08-12 Spider V2 leap shuffle correction

- Source inspection found that pouncer and ambusher runtime action selection can choose `ActionPouncerLeap` / `ActionAmbusherLeap` whenever the target remains in the leap distance band, while those leap actions reused generic attack effects that did not clear `at_flank` or `at_role_band`.
- Source inspection also found that `leap.animation` played `Spider_jump_v2` as an instant animation stage before `leap.arc`, while `leap.arc` moved the spawned motion root with arc/VFX telemetry without re-binding the jump animation for the moving stage.
- Updated pouncer and ambusher leap effects to clear `at_flank` and `at_role_band` after the leap, forcing the next leap cycle to re-establish flank/role positioning.
- Updated the live Broodmother plugin selector with a same-target post-leap latch so pouncer/ambusher roles disable leap selection until the matching flank action succeeds.
- A first correction mapped the `leap.arc` motion handle to `Spider_jump_v2` and made arc motion start animation internally. That is superseded by the later authored-stage correction below because it mixed the motion handle with animation ownership.
- User live feedback after that deploy reported that the Spider root was jumping but the body was not animated. Source inspection found the `leap.arc` fix still only issued one animation call at motion start and did not prove or re-sample the live animator state during the arc.
- A second temporary correction added a loaded-clip Unity Playables fallback. That is also superseded by the authored-stage correction below because it injected playback outside the declared procedure animation stages.
- Corrected source authoring removes the Playables fallback, keeps `leap.arc` as motion/VFX only, removes `LeapArcMotionHandle` from the animation map, adds explicit `move.flank-animation`, `move.role-animation`, and `move.safe-animation` stages, maps those stages to `Spider_Run_CI4`, and maps safe stop/grounded control to `Spider_Tail_Idle_CI4`.
- Broodmother AI package validation passed for V2 (`BROODMOTHER_AI_PACKAGE_V2_PASS`, 10 fixtures). PlayMaker fixture validation passed all 48 AIR-40 fixtures, including all 15 Broodmother Spider procedure fixture groups. FoAHost V2 fixture execution passed all 242 migration fixtures, with existing optional Unity/System.Text.Json reference warnings only.
- Broodmother companion release build passed with `0` warnings and `0` errors using explicit FoA, Unity, Rabbit, GOAP, and PlayMaker dependency paths. Static scans found no remaining production `AnimationClipPlayable`, `AnimationPlayableOutput`, `FindObjectsOfTypeAll<AnimationClip>`, `clip-playable`, `UnityEngine.Playables`, or `UnityEngine.Animations` path in the corrected Spider host/plugin/package source.
- The pre-deploy process check found `Fall of Avalon.exe` PID `11348` and `UnityCrashHandler64.exe` PID `39884` still running. They were stopped before copying DLLs into the live BepInEx plugin folders.
- Closed-game deployment passed with backups under `_codex_backups\20260812-authored-spider-animation-stages`. Installed DLL hashes match the rebuilt DLL hashes: `AvalonAI.FoAHost.Mono.V2.dll` `A4F1063B9B47C25DB229490AFA5E602CD4C53CD25ACE619461DCB1990692A894` in both live plugin folders, `AvalonBroodmotherCompanion.AI.Package.V2.dll` `E91748462F6F0EBCC966D816A8FD9D40D740449869BC28C83D0DED945F569A03`, and `AvalonBroodmotherCompanion.dll` `A56710D935B20596A88E9AF186DEB4D8F152E11EB1FD630D65013B0CF609ED39`.
- A post-deploy process check found FoA relaunched, the installed DLL hashes still matching the rebuilt hashes above, and `LogOutput.log` containing `Avalon Broodmother Companion 0.1.34 loaded`. The Broodmother command CSV had not advanced past `2026-08-12T18:55:54.0306110+00:00`; no post-deploy Spider command rows or in-game visual pass have been captured yet for this authored-stage correction.

## 2026-08-12 Spider-family visual scale correction

- Live FoA log evidence showed `Spider's Call - Vesper, Pale Ambusher` using `visualScaleMultiplier=3`, with `baseScale=(0.372, 0.372, 0.372)` and `appliedScale=(1.115, 1.115, 1.115)`, matching the user's report that the spawned Spider was too small.
- Updated the three smaller Spider variants to use the same `5.5` visual scale multiplier as the Broodmother variants. The expected Vesper applied scale for the logged base scale is now approximately `(2.046, 2.046, 2.046)`.

## 2026-08-11 Spider V2 instant-stage completion and command diagnostics

- Post-telemetry live command rows proved the current blocking stages were `move.role-position:timed-out` and `bite.animation:timed-out`, with one `bite.telegraph:deadline-expired-before-submit` row and zero `BROODMOTHER_COMPANION_DAMAGE_BRIDGE` / `nativeEntry=HealthElement.TakeDamage` rows.
- Source inspection found the Spider procedure runner submitted an accepted stage and returned `Running` without polling it until a later companion tick. The host controller marks face, animation, damage-window, release, stop-VFX, and interrupt commands as instant once polled, so that runner cadence added an avoidable tick boundary before every instant-stage completion.
- Changed `AvalonPlayMakerSpiderRunner.Tick()` to poll an accepted command immediately and drain succeeded stages within the same runner tick, bounded by the procedure stage count plus one. Non-terminal commands still return `Running`, so movement stages remain observable instead of being forced through.
- Added runner diagnostics for last command stage, action, handle, phase, outcome, reason, poll count, and per-tick completed-stage count.
- Added Broodmother CSV/BepInEx diagnostics for Spider V2 initial and terminal poll rows using those fields, while retaining `finalStage`.
- PlayMaker fixture validation passed: all 15 Broodmother Spider procedure fixture groups passed, and all 48 AIR-40 PlayMaker Runtime V2 binding fixtures passed.
- FoAHost V2 release build passed with `0` warnings and `0` errors using explicit FoA, Unity, Rabbit, GOAP, and PlayMaker dependency paths.
- Broodmother companion release build passed with `0` warnings and `0` errors using the same dependency paths.
- FoAHost V2 fixture execution passed all 237 migration fixtures with the existing optional Unity reference warnings for `Unity.Burst.Unsafe` and `Unity.Collections.LowLevel.ILSupport`.
- Broodmother AI package validation passed for V2 (`BROODMOTHER_AI_PACKAGE_V2_PASS`, 9 fixtures) and V1 (`BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`, 8 fixtures).
- Closed-game deployment passed with timestamped backups under `_codex_backups\20260811-command-completion-diagnostics-closed-deploy`. Installed DLL hashes now match the rebuilt DLL hashes: `AvalonAI.Execution.PlayMaker.dll` `A6256A5110FE76DF51E588B6E075465125F9437865CD0B91DFEA8BCC806B4110`, `AvalonAI.FoAHost.Mono.V2.dll` `88F5EB1EE8EA8A273204E75FD7773C06DB465812D473F9BD1A385AD854D49B56`, and `AvalonBroodmotherCompanion.dll` `F2299CAA10A8F2FB513FBDA55BD76C6CB3E3CE4775DD9CACA33EC6364DE71507`.
- A live post-deploy FoA check found `Fall of Avalon.exe` running from the FoA install and the installed DLL hashes still matching the rebuilt hashes above. The active `LogOutput.log` contained one `Avalon Broodmother Companion 0.1.34 loaded` line.
- The active `LogOutput.log` contained four logged `BROODMOTHER_COMPANION_DAMAGE_BRIDGE` lines, all with `attack=Bite` and `nativeEntry=HealthElement.TakeDamage`; it contained zero `spider-blaze-command-ownership-lost` matches and zero `bite.animation:timed-out` matches.
- Post-fix diagnostic CSV rows from `2026-08-11T03:43:54.6891825Z` through `2026-08-11T03:44:57.8019857Z` contained 44 rows with the new diagnostic fields: 32 Spider V2 starts and 12 terminal polls.
- In those post-fix diagnostic CSV rows, 13 starts were `bite.v1`; nine had `initialStatus=Succeeded`, `initialFinalStage=reservation.release:succeeded`, and `initiallastTickCompletedStages=7`. Four bite starts failed retryable at `bite.damage.commit` with `spider-blaze-command-controller-unsupported`.
- In those post-fix diagnostic CSV rows, 19 starts were `reposition.v1`; all 12 terminal poll rows were `move.role-position:timed-out` with `lastCommandOutcome=Running`, `lastCommandReason=accepted`, and six or seven command polls. There were zero bite terminal timeouts and zero ownership-lost rows in this post-fix diagnostic window.
- A later live row for `avalon.broodmother.spider.procedure.leap.v1` proved `face.launch-vector`, `leap.telegraph`, and `leap.animation` completed in the initial tick (`initiallastTickCompletedStages=3`) before `leap.arc` timed out. The terminal `leap.arc` row had ten command polls and still reported a target distance of `2.883`, proving the remaining block was host motion completion rather than AI action selection or animation-stage rejection.
- Source inspection found the spawned Spider controller advanced motion only by `speed * Time.deltaTime` inside sparse command polls. That under-integrated `move.role-position` and `leap.arc` motion because AI polls are not every Unity frame.
- Changed the Spider controller motion path to pass the runner poll timestamp through the bridge/executor/controller poll interfaces and advance movement by elapsed procedure time since the previous command poll. Running and succeeded motion dispatch reasons now include `motionTarget`, `motionActorBefore`, `motionActorAfter`, `motionRemainingBefore`, `motionRemainingAfter`, `motionStep`, `motionSpeed`, `motionDeltaSeconds`, `motionProgress`, `motionArriveEpsilon`, `motionLeapArc`, `motionArcHeight`, `motionArcOffset`, and `motionVfx`.
- Changed `leap.arc` to require and start the authored leap VFX profile, raise the spawned motion root through an Avalon-owned arc during the leap stage, and stop the leap VFX when the motion command succeeds, cancels, or cleans up. This is not a native FoA jump-state claim; it is the custom motion/animation path required by the Spider commercial-stack packet.
- FoAHost V2 release build passed with `0` warnings and `0` errors. FoAHost V2 fixture execution passed all 240 migration fixtures, with only the existing optional Unity reference warnings for `Unity.Burst.Unsafe` and `Unity.Collections.LowLevel.ILSupport`.
- Broodmother companion release build passed with `0` warnings and `0` errors. Broodmother AI package validation passed for V2 (`BROODMOTHER_AI_PACKAGE_V2_PASS`, 9 fixtures) and V1 (`BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`, 8 fixtures).
- Closed-game deployment passed with timestamped backups under `_codex_backups\20260811-spider-motion-clock-leap-arc-final-deploy`. Installed DLL hashes now match the rebuilt DLL hashes: `AvalonAI.FoAHost.Mono.V2.dll` `24B706FD29561B22A10568EBFD2E8868050C62C1587043CBAF5D307E6E6EE75F`, and `AvalonBroodmotherCompanion.dll` `201BE175F33C08FB41D3F951B79CCEE29490239561C53AA3816CE723642075B8`.
- A post-final-deploy file check found no matching FoA process, the live DLL hashes still matching the final rebuilt hashes above, and no command-log rows or BepInEx log writes after the final deploy time. No post-deploy FoA relaunch or in-game command-log pass has been run yet for this motion-clock/leap-arc correction. The previous intermittent `bite.damage.commit` / `spider-blaze-command-controller-unsupported` rows remain a follow-up only if they persist in fresh post-deploy rows.

## 2026-08-11 Spider V2 timeout final-stage telemetry

- Existing live `broodmother-command-log.csv` rows after `2026-08-11T02:37:00Z` contained three accepted `avalon.broodmother.spider.procedure.bite.v1` starts and six accepted `avalon.broodmother.spider.procedure.reposition.v1` starts. All nine had `initialStatus=Running` and `initialReason=accepted`.
- The same CSV query found zero rows containing `finalStage`, `advanced-spider-ai-v2-poll`, `BROODMOTHER_COMPANION_DAMAGE_BRIDGE`, or `spider-procedure-timed-out`, so the existing live trace does not identify the exact timeout stage.
- Source inspection found `AvalonPlayMakerSpiderRunner` already emitted normalized `final.stage`, but its deadline-expired path did not update that field before returning `spider-procedure-timed-out`.
- Added timeout-stage marking so the runner records the current stage as `<stageId>:timed-out` when the stage is waiting on a command, or `<stageId>:deadline-expired-before-submit` if the procedure deadline expires before that stage is submitted.
- Added Broodmother terminal poll telemetry: terminal Spider V2 polls now write `advanced-spider-ai-v2-poll` command-log rows and include `finalStage=...` in both the command log detail and `BROODMOTHER_COMPANION_ADVANCED_SPIDER_AI_V2_POLL`.
- PlayMaker fixture validation passed: all 14 Broodmother Spider procedure fixture groups passed, and all 48 AIR-40 PlayMaker Runtime V2 binding fixtures passed.
- FoAHost V2 fixture build passed with the existing optional Unity package-cache warnings for `Unity.Burst.Unsafe` and `Unity.Collections.LowLevel.ILSupport`; FoAHost V2 fixture execution passed all 237 migration fixtures.
- Broodmother release build passed with `0` warnings and `0` errors using the FoA/Unity dependency properties.
- Broodmother AI package validation passed for V2 (`BROODMOTHER_AI_PACKAGE_V2_PASS`, 9 fixtures) and V1 (`BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`, 8 fixtures).
- Initial live on-disk deployment was attempted with timestamped backups, but Windows refused overwriting the three loaded DLLs because `Fall of Avalon.exe` had user-mapped sections open. Process termination was not performed because the safety reviewer rejected stopping the running game without explicit current approval due unsaved-progress risk.
- After the user closed FoA, closed-game deployment passed with timestamped backups under `_codex_backups\20260811-telemetry-stage-timeout-closed-deploy`. Installed DLL hashes now match the rebuilt DLL hashes: `AvalonAI.Execution.PlayMaker.dll` `501E1A130E65EF62A267617C2CCE5B9E8B62577B3796E45477C7603012AD4D98`, `AvalonAI.FoAHost.Mono.V2.dll` `927C419FCC940842D019131AA03237A14EA968AA9FDABB13315C06E66CD0E14C`, and `AvalonBroodmotherCompanion.dll` `AFEE2F2DA210C38D41ACF239357F74970F2BF4FE6113F6F89B6578719599DEB9`.
- A post-deploy live log/CSV check at `2026-08-11T03:12:00Z` and later found 15 Spider V2 starts: eight `bite.v1` and seven `reposition.v1`, all `initialStatus=Running` and `initialReason=accepted`. It found 14 terminal `advanced-spider-ai-v2-poll` rows, all `TimedOut` with `reason=spider-procedure-timed-out`: seven `finalStage=move.role-position:timed-out`, six `finalStage=bite.animation:timed-out`, and one `finalStage=bite.telegraph:deadline-expired-before-submit`.
- The same post-deploy window found zero `spider-blaze-command-ownership-lost` starts and zero `BROODMOTHER_COMPANION_DAMAGE_BRIDGE` / `nativeEntry=HealthElement.TakeDamage` matches.
- A later same-turn process check found `Fall of Avalon` running again, with `LogOutput.log` updated and a fresh `Avalon Broodmother Companion 0.1.34 loaded` line. The Broodmother command CSV had not advanced past `2026-08-11T03:13:25.9793821Z`, so no newer Spider V2 command rows were present in that latest launch at the time of the check.

## 2026-08-08 six-variant item-icon cache correction

- End-to-end source review confirmed three Broodmother definitions and three smaller Spider definitions reach the shared registration, no-stack merchant insertion, exact custom-item cast interception, tier-specific public resolver, visual-address selection/readback, one-session spawn/ally/lifecycle path, definition-specific AI role, embedded spider audio, command surface, death handoff, and cleanup routes.
- Source review found `BroodmotherCallItemIconPatch` retained only one global item-icon `Sprite`; refreshing a different variant destroyed the previous sprite even though another merchant/inventory row could still reference it.
- Replaced the single global sprite with definition-keyed sprite, texture, and source caches. All six variant rows can now retain their own matching icon concurrently, and plugin teardown destroys every owned sprite before releasing embedded textures.
- The six icon resources are present under `src/Icons`, decode as `1280x720` JPG files, and remain embedded through the project wildcard resource rule.
- Bumped plugin/project/release version to `0.1.31`.
- Exact source-file preflight passed with route `mod-source`, validation target level `2`, and test lanes `build,dll-freshness`.
- Release build passed with `0` warnings and `0` errors using the configured FoA game references.
- Broodmother companion AI package fixtures passed all 8 checks and emitted `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS` for roles `avalon-broodmother-companion.broodmother|avalon-broodmother-companion.spider`.
- Static six-definition checks passed: six unique custom call GUIDs, three Broodmother definitions, three smaller Spider definitions, and six entries in `SpiderFamilyCompanionDefinitions.All`.
- End-to-end source assertions passed for all-six registration, no-stack merchant insertion and post-count, exact custom-item cast lookup, selected-definition dispatch, tier-specific resolver, visual selection/readback, spawned identity validation, active-definition ownership, and shared embedded audio.
- Presentation checks passed for six exact visual addresses, six exact icon mappings, six embedded JPG icon resources, definition-keyed item `Sprite` retention, and nine embedded spider WAV resources.
- Forbidden-path checks passed: no random/distribution systems, raw LocationTemplate fallback, recipe/loot/reward/container registration, hard Avalon Companions dependency, or hard project/package dependency.
- `git diff --check -- mods/avalon-broodmother-companion` passed before the validation-record update; the final diff check is run after this note is written.
- Built DLL assembly version: `0.1.31.0`.
- Built DLL SHA-256: `95C78FDD4B5F5CBE9878BA75D4C6564DC0255C2CA48E88DF4023362F135426A8`.
- No protected file was changed. Deployment was not run because the six-summon source gate does not authorize external deployment or FoA launch.
- Live FoA merchant rendering, six casts, six visual readbacks, animation/audio behavior, combat, death/corpse, transition, and shutdown cleanup remain unrun and unclaimed for `0.1.31`.

## 2026-08-08 merchant six-item no-stack correction

- User supplied live merchant screenshot `C:\Users\kane0\Pictures\Screenshots\Screenshot (5101).png` showing only one custom spider-family merchant item after the `0.1.29` six-item/icon source slice.
- Source review found all six spider-family definitions present and the merchant loop iterating `SpiderFamilyCompanionDefinitions.All`, but the stock insertion still called `Stock.AddItem(createdItem, allowStacking: true)` even though cloned custom call templates are marked `canStack=false`.
- Changed the known-merchant stock route to call `Stock.AddItem(createdItem, allowStacking: false)` for these custom call items.
- Added a post-loop merchant stock count log: `BROODMOTHER_CALL_MERCHANT_STOCK complete` when all six expected definitions are found, or `BROODMOTHER_CALL_MERCHANT_STOCK incomplete` with missing variant IDs otherwise.
- Bumped `Avalon Broodmother Companion` to `0.1.30`.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- Broodmother companion AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- `git diff --check -- mods/avalon-broodmother-companion` passed.
- Exact stock source preflight passed with route `mod-source` and validation level target `2`; the broader mod-directory preflight had no catalogue route, so the exact source file route was used.
- Live plugin hard-dependency scan found only the expected soft-reflection API type strings for FoA Mod Manager and Tainted Interface, not hard `ProjectReference`, `PackageReference`, Avalon Companions API, or `using AvalonCompanions` entries.
- Spawn/distribution scan found no `LocationSpawner`, `GroupSpawner`, `AutoGuardSpawningAttachment`, `RegisterRecipe`, `LootTable`, `Reward`, or `Container` routes in live source; the only stock-related import hit was `Awaken.TG.Main.Heroes.Items.LootTables` for runtime stock item data.
- Protected-term scan over `mods/avalon-broodmother-companion` returned only a historical self-report line in this test-notes file; no protected files were touched.
- Release DLL and installed BepInEx DLL assembly version: `0.1.30.0`.
- Release DLL and installed BepInEx DLL SHA-256: `C26F43BFEC7230348C5D49B100A098A8E468B12965D5D9A67426A92B72A7BEFE`.
- Live DLL update passed by copying the rebuilt release DLL to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonBroodmotherCompanion\AvalonBroodmotherCompanion.dll` and reading back the matching version, hash, and size.
- No fresh FoA launch, save access, merchant-opening live check, six-visible-item screenshot, spell-cast live check, or live visual/audio/combat/death validation was run for this correction.

## 2026-08-08 smaller Spider embedded icon resources

- User supplied three smaller Spider JPGs for the three smaller Spider variants and requested them as icons.
- Copied the supplied files into `src/Icons` as `spider-small-skin1-redblack.jpg`, `spider-small-skin2-pale.jpg`, and `spider-small-skin3-gold.jpg`.
- Destination hashes matched the supplied files:
  - `spider-small-skin1-redblack.jpg`: `469F9227E98CDBDF1294039D128839BF694F747F1181BDE06C686CAAE8685073`
  - `spider-small-skin2-pale.jpg`: `6D59580016AEFD5211D5379A2755EB1CC05DE8C3646B46ABE13A1618D3CA3258`
  - `spider-small-skin3-gold.jpg`: `85FA1BD54F59E6ED8389AC9CEFC2E82CF5E0FE20EF48491E7B0CD3DD99C74761`
- Added embedded icon variants for the three smaller Spider call items and wired `Spider's Call - Skin 1`, `Spider's Call - Skin 2`, and `Spider's Call - Skin 3` to those images for item icons, HUD, and command-dialogue portrait.
- Bumped `Avalon Broodmother Companion` to `0.1.29`.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- Broodmother companion AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- Built release DLL assembly version: `0.1.29.0`.
- Built release DLL SHA-256: `8B5305E9505B73B43FD5C58293C0AA7E01EE1E6923A386F9D23CE4A4D70D5E20`.
- Release DLL manifest resources include all six icon resources: three Broodmother JPGs and the three smaller Spider JPGs.
- No deploy-on-build, FoA launch, save access, merchant-opening live check, spell-cast live check, or live icon rendering validation was run for this icon slice.

## 2026-08-08 six spider-family summon items

- Implemented the `docs/spider-family-six-summons-gate-2026-08-08.md` source slice: three Broodmother call items and three smaller Spider call items, all cloned from native `Wolf's Call`, all routed through the existing guarded one-session companion lifecycle.
- Added exact per-item definitions for `Broodmother's Call`, `Broodmother's Call - Skin 2`, `Broodmother's Call - Skin 3`, `Spider's Call - Skin 1`, `Spider's Call - Skin 2`, and `Spider's Call - Skin 3`.
- Each spell item now selects the matching Avalon Awakened public resolver, current CI4 LocationTemplate/NpcTemplate GUID pair, visual address, visual scale multiplier, display name, and source-only AI actor role before the same summon/swap path accepts the actor.
- The existing configured known-merchant route now attempts to stock all six call items in `Shop_Vendor_Tier1` with the same duplicate guards and quantity setting.
- Broodmother variant item/HUD/dialogue icons use the embedded red/black, pale, and gold Broodmother images. Smaller Spider variants use the existing Tainted Interface/procedural fallback route because no smaller-spider icon resources are documented in this plugin.
- Bumped `Avalon Broodmother Companion` to `0.1.28`.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- Broodmother companion AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS` with roles `avalon-broodmother-companion.broodmother|avalon-broodmother-companion.spider`.
- Built release DLL assembly version: `0.1.28.0`.
- Built release DLL SHA-256: `51BDC312C8B28F1EA159DA4A07B479F7229C2A8C8C3E9D86823F7EDEC5D11C0F`.
- No deploy-on-build, FoA launch, save access, merchant-opening live check, spell-cast live check, or live visual/audio/combat/death validation was run for this source slice.

## 2026-08-08 custom creature and summonable companion process lock

- Locked the accepted Broodmother command-dialogue process into `mods/avalon-awakened/docs/canonical-custom-creature-importer-process.md` as the reusable process for future custom creature imports and one-session summonable custom creature companions.
- The canonical guide now includes a proven-process lock, complete-path requirements, the expanded summonable companion addendum, blocked bypasses, required per-creature records, hard stops, and validation-marker requirements.
- Added the required pre-implementation checklist for future custom creature or summonable companion slices. Every applicable item must be checked by cited evidence before code, build, deployment, FoA launch, save access, or external state mutation.
- The lock cites the repo full-proven-path rules, in-game UI quality standard, Broodmother design and validation plan, and the 2026-08-08 end-to-end source/deployed-binary assurance record.
- No runtime source, build metadata, deployment state, game files, saves, protected files, assets, or external state were changed in this documentation slice.

## 2026-08-08 end-to-end researched command-dialogue process assurance

- Rechecked the Broodmother companion command-dialogue implementation against the repository complete-proven-path requirement, the Broodmother design and validation docs, and the Avalon Companions input research notes.
- Source mapping found every researched segment present in Broodmother-owned code: spell route intercept, active runtime Broodmother ownership, native runtime-only `Companion` action, exact active-actor availability gate, plugin-owned Unity UI open lifecycle, Tainted Interface-first cursor scope, FoA Mod Manager fallback, native `ForceCursorVisibility`, GUI-pass maintenance, game/player input lock, Rewired/EventSystem pass-through while the Unity dialogue is visible, Unity `Button.onClick` dispatch, real-pointer manual hit-test dispatch, command handler execution, dialogue close, cursor release, prompt cleanup, and shutdown dismissal.
- Source mapping found command execution still routes through the documented Broodmother commands: Follow, Hold Position, Defend, Keep Close, Keep Pace, Keep Distance, Come Close, Recall, Recover, Dismiss, and Leave.
- Negative scans found no Broodmother `src` `ProjectReference`, `PackageReference`, Avalon Companions interop, blocked story/dialogue API, old submit-release/debounce gate, pointer-release fallback, mouse-up fallback, or virtual-cursor fallback.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- Broodmother companion AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- `git diff --check -- mods/avalon-broodmother-companion` passed.
- The installed BepInEx DLL reports assembly version `0.1.27.0`.
- The just-built release DLL and installed BepInEx DLL hashes matched: `42CCACB41DD2CE20B0643C75C6653E5133794C5757DE8C41AE45D7E89C4A9052`.
- This verifies the researched process is implemented end-to-end in source and that the installed DLL is the same binary as the current release build. It does not claim a fresh FoA launch, save access, live menu click, live controller cursor, or live close/restore runtime observation for this pass.

## 2026-08-08 native command-dialogue input-lock parity correction

- User required the Broodmother companion to use the correct known companion command-dialogue process instead of another live proof chase.
- Source comparison found Broodmother already owned the native `Companion` action, Unity UI dialogue host, real-pointer manual hit-test, Tainted Interface-first cursor scope, FoA Mod Manager fallback, native `ForceCursorVisibility`, and GUI-pass maintenance, but did not have the Broodmother-owned equivalent of Avalon Companions `PanelInputLockPatch`.
- Added `BroodmotherCompanionInputLockPatch`, matching the proven input segment: block `GameUI.UpdateMousePosition` and `PlayerInput.ProcessLateUpdate` while Broodmother companion UI is visible, zero player movement/look fields, keep Rewired axis/button neutralization for the IMGUI panel, and allow Rewired reads to pass while the Unity command-dialogue menu is visible or FoA Mod Manager controller-cursor input reads are active.
- Added Broodmother-local FoA Mod Manager reflection for `IsControllerCursorInputReadActive`.
- Bumped `Avalon Broodmother Companion` to `0.1.27`.
- No Avalon Companions bridge/API dependency, hard FoA Mod Manager reference, hard Tainted Interface reference, story/native-dialogue API, command body, spawn route, save route, AI target logic, or asset path was changed.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- Broodmother companion AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- `git diff --check -- mods/avalon-broodmother-companion` passed.
- Source scan found no Broodmother source `ProjectReference`, `PackageReference`, Avalon Companions interop, blocked story/dialogue API, or hard dependency reference entries. The only FoA Mod Manager and Tainted Interface matches are the expected soft-reflection API type strings.
- Deploy-on-build passed and installed `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonBroodmotherCompanion\AvalonBroodmotherCompanion.dll` with assembly version `0.1.27.0`.
- No FoA launch, save access, live menu click/choice test, live controller cursor test, or live close/restore test was run after this build.

## 2026-08-08 native command-dialogue GUI-pass maintenance correction

- User required the custom Broodmother command dialogue checked against the working companion command-dialogue process and fixed.
- Research comparison found Broodmother already had the native runtime-only `Companion` action, Tainted Interface first / FoA Mod Manager fallback cursor scope order, native `ForceCursorVisibility`, Unity `Button.onClick`, and manual `Input.GetMouseButtonDown(0)` real-pointer row hit-test dispatcher. The missing proven-path segment was the dialogue GUI-pass maintenance used by the working companion correction path while the menu remains visible.
- `BroodmotherCompanionDialogueHost.MaintainFromGuiPass()` now reasserts cursor ownership, host existence, UI sync, and manual hit-test selection from `Plugin.OnGUI()` while the Broodmother dialogue is visible.
- Command bodies, AI movement/combat policy, spawn ownership, save ownership, story/native-dialogue APIs, Avalon Companions dependency boundaries, and non-terminal menu-open behavior were not changed.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Removed-gate source scan found no old submit-debounce or pointer-release gate terms in the Broodmother live source.
- Live plugin source scan found no `ProjectReference`, `PackageReference`, Avalon Companions bridge/API reference, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Repository protected-name scan over the selected Broodmother source, design, research, validation, release, and AI-package paths returned no matches; no protected files were touched.
- Release DLL hash: `B33597767BE9840820CDCE0DEEC060FB9A783CF28203FAA23AEC835BA2588A84`.
- Release assembly version: `0.1.26.0`.
- No live BepInEx deployment, FoA launch, save access, or live menu click retest was run by Codex for this correction because the live-validation gate requires explicit current-task authorization before live deployment and game/save access.

## 2026-08-08 native command-dialogue Unity button route correction

- User required the actual selected command row to use the proven dispatch path too, with no remaining Broodmother-only gate in front of `Button.onClick`.
- Runtime log review after `0.1.24` showed the game loaded `Avalon Broodmother Companion 0.1.24` and opened the companion-style Unity UI Broodmother dialogue host, but the latest log tail contained no following `dialogue direct choice clicked`, `manual-hit-test`, or `dialogue-choice` row.
- Source comparison found Avalon Companions wires `button.onClick` directly to `RunDialogueUiChoice(..., route: "unity-button")`, while Broodmother still routed Unity `Button.onClick` through `RunChoice(...)` with a Broodmother-specific submit-release/debounce gate.
- Broodmother `0.1.25` removes `ChoiceInputDebounceSeconds`, `_openedFrame`, `_choiceInputBlockedUntil`, `_choiceSubmitReleasedAfterOpen`, `CanAcceptChoiceInput`, `IsPastOpeningDebounce`, `UpdateChoiceInputReleaseGate`, and `IsChoiceSubmitHeld` from the command-dialogue host.
- Unity `Button.onClick` and manual hit-test selection now both call the same `RunChoice(binding, route)` command dispatcher. `_lastChoiceFrame` remains only as same-frame duplicate suppression so one click cannot dispatch twice if Unity and manual paths both report it.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Live plugin source scan found no `ProjectReference`, `PackageReference`, Avalon Companions bridge/API reference, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Removed-gate source scan found no `ChoiceInputDebounceSeconds`, `_choiceInputBlockedUntil`, `_choiceSubmitReleasedAfterOpen`, `_openedFrame`, `CanAcceptChoiceInput`, `IsPastOpeningDebounce`, `UpdateChoiceInputReleaseGate`, or `IsChoiceSubmitHeld` entries in the command-dialogue host.
- Protected-term scan over the touched Broodmother source, design, research, validation, changelog, and manifest paths returned no matches; no protected files were touched.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `28BF047465F356BC661065A8470A5FDCA22DF721CCEA9DC694BB8BA5B5B75285`.
- Deployed assembly version read back as `0.1.25.0`.
- Post-deploy process check found no running `Fall of Avalon` process. The current BepInEx log still shows the earlier `Avalon Broodmother Companion 0.1.24` load marker, so a fresh FoA launch is required to load `0.1.25`.
- No FoA launch, save access, or live menu click retest was run by Codex after this build.

## 2026-08-08 native command-dialogue proven path correction

- User rejected the Broodmother-specific pointer-release attempt and required the command menu to copy the proven working companion path exactly.
- Source review of Avalon Companions found the proven direct command-menu fallback: `Input.GetMouseButtonDown(0)`, one `_lastDialoguePointerFrame` frame guard, real `Input.mousePosition`, visible enabled row hit-test, and dispatch through `RunDialogueUiChoice(..., route: "manual-hit-test")`.
- Broodmother `0.1.23` still had Broodmother-only pointer-release state and a mouse-up fallback. Broodmother `0.1.24` removes `_pointerSubmitReleasedAfterOpen`, `_pointerChoiceDispatchedForCurrentPress`, `CanAcceptPointerChoiceInput`, `Input.GetMouseButtonUp(0)`, and `manual-hit-test-release`.
- The Broodmother manual fallback now mirrors the proven path: `Input.GetMouseButtonDown(0)`, `_lastPointerFrame`, real `Input.mousePosition`, visible enabled row hit-test, and direct dispatch through the same `RunChoice` command handler. Normal Unity `Button.onClick` choices keep the opening submit-release safety gate separately.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Live plugin source scan found no `ProjectReference`, `PackageReference`, Avalon Companions bridge/API reference, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Rejected pointer-release source scan found no `_pointerSubmitReleasedAfterOpen`, `_pointerChoiceDispatchedForCurrentPress`, `CanAcceptPointerChoiceInput`, `Input.GetMouseButtonUp`, `manual-hit-test-release`, `mouse-up`, `pointer-release`, or `mouse-release` entries in the Broodmother live source.
- Protected-term scan over the touched Broodmother source, design, research, validation, changelog, and manifest paths returned no matches; a broader scan that included historical test notes returned one old self-report line only. No protected files were touched.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `3D829A5FC90D9FD35F44A8F1A3BB4E3351BF2EE9EDB5AB5AF189526455412056`.
- Deployed assembly version read back as `0.1.24.0`.
- Post-deploy process check found no running `Fall of Avalon` process. The current BepInEx log still shows the earlier `Avalon Broodmother Companion 0.1.23` load marker, so a fresh FoA launch is required to load `0.1.24`.
- No FoA launch, save access, or live menu click retest was run by Codex after this build.

## 2026-08-08 native command-dialogue pointer dispatch gate correction

- User-reported `Screenshot (5089).png` showed the native `Companion` command-dialogue menu open with row hover registering, but the hovered option was not being chosen.
- Current BepInEx log read showed the live game session was still `Avalon Broodmother Companion 0.1.21`; the Broodmother command log showed a later `native-companion-prompt` open row at `2026-08-08T09:58:26.1902207+00:00` with no following `dialogue-choice` row in the latest tail.
- Source review found the Unity `Button.onClick` route and manual pointer hit-test both went through the same global submit-release gate. That kept the anti-auto-select debounce, but could also block real mouse row selection when another submit source stayed held or did not release cleanly.
- The dialogue host now splits direct pointer acceptance from normal submit acceptance: normal Unity button submit still requires the full submit-release gate, while the real-pointer manual fallback requires the opening debounce plus a mouse-specific release gate.
- The real-pointer manual fallback now handles both mouse-down and mouse-release row hit-tests against visible enabled command choices, and suppresses duplicate dispatch for the same mouse press.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Live plugin source scan found no `ProjectReference`, `PackageReference`, Avalon Companions bridge/API reference, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Protected-term scan over touched Broodmother paths returned only the existing historical test-note self-report line about protected-term scanning; no protected files were touched.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `0A7E981FEFCBDAFDB18C290B76D0CC7AB9282109D3F5D3A33303BA960D8D1AC0`.
- Deployed assembly version read back as `0.1.23.0`.
- Post-deploy process check found no running `Fall of Avalon` process. The current BepInEx log still shows the earlier `Avalon Broodmother Companion 0.1.21` load marker, so a fresh FoA launch is required to load `0.1.23`.
- No FoA launch, save access, or live menu click retest was run by Codex after this build.

## 2026-08-08 embedded Broodmother icon resources

- User supplied three Broodmother JPGs and requested them embedded for icons, with only the current variation used now and the other two kept for later additional summons once this variation is proven.
- Copied the three supplied JPGs into `src/Icons` as `broodmother-current-redblack.jpg`, `broodmother-reserved-pale.jpg`, and `broodmother-reserved-gold.jpg`; SHA-256 checks matched the user-supplied source files byte-for-byte.
- Added embedded resource compilation for `Icons/*.jpg`; release DLL manifest resources include `AvalonBroodmotherCompanion.Icons.broodmother-current-redblack.jpg`, `AvalonBroodmotherCompanion.Icons.broodmother-reserved-pale.jpg`, and `AvalonBroodmotherCompanion.Icons.broodmother-reserved-gold.jpg`.
- Added a Broodmother-owned embedded icon runtime that decodes the active red/black JPG through game-shipped `UnityEngine.ImageConversionModule`, center-crops it to a square icon texture, caches it, and releases it on plugin teardown.
- `Broodmother's Call`, the companion HUD, and the native command-dialogue portrait now use the embedded current red/black Broodmother icon before any Tainted Interface fallback.
- Pale and gold images are embedded as reserved icon resources only; no pale/gold summons or variant selection logic were added in this slice.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA -p:DeployOnBuild=true`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- Release and deployed DLL hashes matched: `A46762F8E5D0FB6B09D75B2DCAE237EA55E453554348D520DE81DD9F739C58FA`.
- Deployed assembly version read back as `0.1.22.0`.
- FoA was running during this build/deploy, and the current BepInEx log still shows `Avalon Broodmother Companion 0.1.21 loaded`; restart FoA before live retesting this `0.1.22` DLL.
- No save access or live icon-rendering check was run by Codex after this build.

## 2026-08-08 native command-dialogue real-pointer cursor correction

- User-reported `Screenshot (5081).png` showed the native `Companion` command-dialogue menu open with a small Broodmother-owned yellow virtual cursor artifact and options not usable.
- Avalon Companions dialogue research and source show the proven route is a Unity UI dialogue host with normal `Button.onClick` handlers plus a direct click fallback using `Input.mousePosition` against visible choice rectangles.
- Removed the Broodmother-owned visible virtual cursor from the dialogue host, removed the menu `OnGUI` cursor drawing/event path, and changed hover/manual hit testing back to Unity's real pointer position.
- Kept FoA Mod Manager `SetCustomUiScope`, native `ForceCursorVisibility`, opening submit-release debounce, and persistent non-terminal command switching.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA -p:DeployOnBuild=true`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- Release and deployed DLL hashes matched: `9B14CE4680AD4CF10CB75B3B24EC031DD22AD0C5A4A452D9C4230F0C6385788D`.
- Deployed assembly version read back as `0.1.21.0`.
- FoA was running during this build/deploy, and the current BepInEx log still shows `Avalon Broodmother Companion 0.1.20 loaded`; restart FoA before live retesting this `0.1.21` DLL.
- No save access, live menu click retest, or active-session command-log proof was run by Codex after this build.

## 2026-08-08 native command-dialogue persistent switching and creature importer process

- User removed loot from scope and requested only dialogue command switching plus a full proven custom creature importer process document.
- Source review found `RunChoice(...)` dispatched every non-leave command with `closesDialogue=true` and closed the host immediately after `RunDialogueCommand(...)`, while the plugin command executor already mutates follow/hold/defend and range state.
- The dialogue host now keeps non-terminal commands open after dispatch, resets the submit/debounce gate, and calls `Sync()` so selected mode/range highlights and bottom status text update immediately. `Leave` and `Dismiss` remain the only closing choices.
- Added `mods/avalon-awakened/docs/canonical-custom-creature-importer-process.md` as an evidence-backed guide for future custom creatures; it summarizes the proven CI1-CI5/live process and companion/presentation addenda without replacing `current-creature-injection-process.md`.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA -p:DeployOnBuild=true`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion` and the new custom creature process doc.
- Source scans found no live plugin `ProjectReference` or `PackageReference` entries and no blocked true native dialogue/story graph or Avalon Companions bridge/API references.
- Protected-term scan passed for selected touched source/docs/release/AI/process paths; the only broader hit remains a historical test-note self-report line.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `B55FF28EDFF0E6FB3714A58D4E055216B024B9FD1945617FAA7AF915DAF251AA`.
- Deployed assembly version read back as `0.1.20.0`.
- Post-deploy process checks found no running FoA process before or after build/deploy, so no live session/save was touched.
- No FoA launch, save access, live native menu switching check, or live creature-importer gate was run by Codex after this build.

## 2026-08-08 native animated-death visibility correction

- User-reported `Screenshot (5070).png` showed the Broodmother body rendered magenta at apparent death and no complete death/lifecycle result.
- Latest available BepInEx log review still showed the loaded Broodmother session as `0.1.16`, and the latest lifecycle row reported `animatedDeath=0` while logging `BROODMOTHER_COMPANION_LIFECYCLE_READY`.
- Research-backed correction follows the CI5E/Dragon visible Death(44) pattern: require initialized `PlayAnimationDeathBehaviour` linked from `DeathElement`, require `KeepBody=true`, validate the private `PostponedRagdollBehaviourBase._alivePrefab` field as `GameObject`, clear only that null-safe suppression target, and keep native `DeathElement`, `NpcDummy`, `Corpse`, and same-session material-retention ownership unchanged.
- Lifecycle ready is no longer logged unless animated-death visibility is ready; waiting logs now include death-behaviour counts, initialized count, DeathElement linkage, visibility readiness, and suppression-cleared readback.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Source scans found no live plugin `ProjectReference` or `PackageReference` entries and no blocked true native dialogue/story graph or Avalon Companions bridge/API references.
- Protected-term scan passed for Broodmother source, README, selected docs, release metadata, and AI-package paths.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `36BB445FEDE9FB4FE17EE0CA0131F458DFC85EBBD4336A76E4CDFCEC573A55CE`.
- Deployed assembly version read back as `0.1.19.0`.
- Compiled release and deployed DLL Unicode probes found `BROODMOTHER_COMPANION_ANIMATED_DEATH_VISIBILITY_READY`, `_alivePrefab`, and `0.1.19`.
- Post-deploy process check found no running `Fall of Avalon` process. The current BepInEx log still only contains the earlier `0.1.16` load marker, so a fresh FoA launch is required to load `0.1.19`.

## 2026-08-08 native command-dialogue submit-release correction

- User reported the native `Companion` command-dialogue menu now opens and closes immediately.
- Live BepInEx and Broodmother command logs showed the loaded game session was still `Avalon Broodmother Companion 0.1.16`, and each menu open was followed by `dialogue-choice leave` on the same frame through `manual-hit-test`.
- The command-dialogue host now requires the debounce window to elapse and requires mouse/Enter/Space/controller submit to be released once after opening before any choice can dispatch.
- The last live lifecycle log did contain `BROODMOTHER_COMPANION_LIFECYCLE_READY`, but it also read `animatedDeath=0`; death animation remains a live lifecycle validation item, not a verified fix.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Source scans found no live plugin `ProjectReference` or `PackageReference` entries and no blocked true native dialogue/story graph or Avalon Companions bridge/API references.
- Protected-term scan passed for Broodmother source, README, docs, release metadata, and AI-package paths when excluding historical test-note lines that themselves record protected-term scan wording.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `2170E6918C5C577C974906C54DD9636E10FDAECA511290F3571D06F7058738AE`.
- Deployed assembly version read back as `0.1.18.0`.
- Post-deploy process check found no running `Fall of Avalon` process. The current BepInEx log still only contains the earlier `0.1.16` load marker, so a fresh FoA launch is required to load `0.1.18`.

## 2026-08-08 native command-dialogue debounce and death lifecycle correction

- User clarified the required command surface is the native `Companion` command-dialogue menu like Avalon Companions, not an attack button, target selector, or separate quick-command stack.
- Avalon Companions research keeps the supported route as a runtime-only native `Companion` prompt opening a plugin-owned command-dialogue UI and keeps true `VDialogue`, story graphs, and attack command UI blocked.
- Live 0.1.16 command log showed `native-companion-prompt opened` followed one or two frames later by `dialogue-choice rangefar`, proving the menu accepted opening/stray input as an immediate command.
- The Broodmother command-dialogue host now blocks command-choice dispatch for a short debounce window after opening.
- `Screenshot (5062).png` showed the Broodmother health bar empty with magenta body presentation and no full death presentation.
- BepInEx log inspection found no `BROODMOTHER_COMPANION_NATIVE_DEATH_ACCEPTED` row for that live death report.
- Source review found dead/discarded live NPC paths cleared the active companion and released Broodmother runtime material clones before FoA's native dummy/corpse death handoff could be observed.
- Dead/discarded live NPC state now logs `BROODMOTHER_COMPANION_NATIVE_DEATH_PENDING`, closes command/audio surfaces, keeps the same-session not-saved location tracked, waits for exact native `NpcDummy` plus `Corpse`, and clears command state only after accepted handoff.
- Accepted native death now retains Broodmother death visual material references until plugin teardown.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Source scans found no live plugin `ProjectReference` or `PackageReference` entries and no blocked true native dialogue/story graph or Avalon Companions bridge/API references.
- Quick-command attachment scan found only the unused `EnsureNativeQuickCommandActions(...)` definition; the active synchronization path attaches the single `BroodmotherCompanionCommandAction` and removes stale quick-command actions.
- Protected-term scan passed for Broodmother source, README, docs, release metadata, and AI-package paths when excluding the historical test-note line that itself records protected-term scan wording.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `8CC5A72E62FEC871F4057E2A23F06E9748411685BCC66A5DEF2C911C43926F49`.
- Deployed assembly version read back as `0.1.17.0`.
- FoA was running during the deploy check, and the active BepInEx log still showed `Avalon Broodmother Companion 0.1.16 loaded`; restart FoA before live retesting this 0.1.17 DLL.

## 2026-08-08 Broodmother LeapJump VFX and menu reset correction

- Live 0.1.15 feedback reported that the combat logic was working, but the ranged attack had no visual effect and using the companion dialogue/menu reset the Broodmother.
- Live BepInEx evidence showed `BROODMOTHER_COMPANION_DAMAGE_BRIDGE attack=LeapJump` rows for successful native damage commits, plus repeated `recalled Broodmother during follow catch-up` rows around menu/context activity.
- Source review found `Update()` still ran the full `TickCompanion()` while the companion menu was visible, allowing follow catch-up/combat AI ticks during UI ownership.
- Source review found native dialogue Keep Close / Keep Pace / Keep Distance choices passed `recallActive=true` into `SetFollowRange(...)`, making spacing choices perform hidden recalls.
- The plugin now skips the full companion tick while the companion menu is visible, while still ticking the dialogue host, embedded audio runtime, and custom spell feature.
- Native dialogue range choices now update spacing only; `Come Close` and `Recall` remain the explicit Broodmother relocation commands.
- Successful LeapJump bridge damage now spawns a transient plugin-owned green/gold Unity arc VFX between the Broodmother and target. It does not mutate native VFX templates, shared registries, or asset bundles.
- The damage bridge now caches target name and coordinates before `HealthElement.TakeDamage(...)`, reducing post-damage target-discard readback risk.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Live plugin source scan found no `ProjectReference` or `PackageReference` entries under `mods/avalon-broodmother-companion/src`.
- Source-only dependency review found only the expected soft-reflection FoA Mod Manager and Tainted Interface strings; no hard project/package reference was added.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `91EDAEA49FF92CCE61C2B2F3E7332BBB6440BC982DB5DDAD9DD328CFB63E9255`.
- Deployed assembly version read back as `0.1.16.0`.
- Post-deploy process check found no running FoA process, and the current BepInEx log still reads `Avalon Broodmother Companion 0.1.15 loaded`, so `0.1.16` is deployed for the next launch but not yet loaded in a live session.
- Live FoA validation is still required to prove visible LeapJump VFX and menu choices no longer recall/reset the Broodmother unexpectedly.

## 2026-08-07 Broodmother native hitbox and damage bridge correction

- Live screenshots `Screenshot (5054).png` and `Screenshot (5055).png` showed native target logic and HUD state were present while the Broodmother still did not take or deal damage.
- BepInEx and `broodmother-command-log.csv` evidence showed `nativeTargeted=1`, `petPrompted=1`, `currentTargetMatched=1`, and `NpcAI.InCombat=1`, so the remaining failure was not target recognition or menu attachment.
- Local `TG.Main.dll` decompilation confirmed native damageability is owned by `HealthElement` hitboxes, not merely by enabled Unity colliders, and confirmed `HealthElement.TakeDamage(Damage)` as the native damage entry point.
- Avalon Awakened generated CI4 animation mapping records the ShortRange attack clips as event-free, so accepted native combat handoff can still lack outgoing attack-release damage.
- The lifecycle surface now registers the active Broodmother's enabled actor colliders with `HealthElement.AddHitbox(...)`, moves those runtime hitbox objects to FoA's `Hitboxes` layer, logs `healthHitboxes`, `healthHitboxAdds`, and `hitboxLayerChanges`, and restores the runtime hitbox/layer changes on cleanup.
- ShortBite and LeapJump commits now use a Broodmother-only damage bridge after native target handoff succeeds. The bridge skips non-damage handoffs and hero targets, requires documented range, creates a normal physical melee `Damage` from Broodmother NPC melee damage, calls the target's `HealthElement.TakeDamage(damage)`, and logs `BROODMOTHER_COMPANION_DAMAGE_BRIDGE`.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for Broodmother source, AI source, release metadata, and README paths.
- Source-only dependency scan found no live plugin `ProjectReference`, `PackageReference`, Avalon Companions bridge/API reference, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `0DBAEFDF771BC763ADDCCA53F72966FFEF2B772753B3AFBE5581698C205F21F4`.
- Release and deployed assembly versions both read back as `0.1.15.0`.
- Post-deploy process check found no running FoA process, and the last BepInEx load marker still reads `Avalon Broodmother Companion 0.1.14 loaded`, so `0.1.15` is deployed for the next launch but not yet loaded in a live session.
- Live FoA hit/damage validation is still required after deployment.

## 2026-08-06 static validation

- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Source structural checks found Broodmother LocationTemplate GUID `efa4bbbdea2fb744fa293152772cd544`, Broodmother NpcTemplate GUID `9a1585dc7e420de4da65a839d062a647`, custom spell GUID `b7d0d4e6c0de4bb0a000000000000001`, Wolf's Call source GUID `3bd577472a0191c44bf298a82553cf3b`, and Magic Summon Ally graph GUID `1ed1718ce1b64c747a329eb4269f529b`.
- Source structural checks found `TryResolveWorldBroodmotherTemplate`, `AllowRuntimeSummon`, `Hero.Current`, `SpawnLocation`, immediate `MarkedNotSaved`, spawned-location validation, native ally marker setup, recall, dismiss, and shutdown cleanup markers.
- Prohibited source-term search found no `LocationSpawner`, `GroupSpawner`, `AutoGuardSpawningAttachment`, recipe registration, custom summon template, Spider constants, population rows, or route rows.
- Custom spell source review found native `Wolf's Call` clone registration, custom-template-only skill-reference rewrite, and default-disabled spell grant.
- Hotkey source review found all configurable hotkeys default to `KeyCode.None`; the only other key is `KeyCode.Escape` for closing the optional panel.

## 2026-08-06 merchant stock update

- Release build passed with 0 warnings and 0 errors after adding the known merchant stock route.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `3339ADE16A83A1D6126CDAA73BE1295C9B172FFA89FA318AD529716D3D0079E4`.
- Source structural checks found the known Tier 1 vendor GUID `75a071140bc819d4ab6e9e37abfdfa59`, `ShopUI.OnFullyInitialized`, `RestockableStock`, `Stock.AddItem`, `MerchantStock`, and `BROODMOTHER_CALL_MERCHANT_STOCK` log markers.
- Prohibited source-term search found no `LocationSpawner`, `GroupSpawner`, `AutoGuardSpawningAttachment`, recipe registration, custom summon template, Spider constants, population rows, or route rows.
- Live BepInEx config was explicitly armed with `MerchantStock.Enabled=true`, `MerchantStock.TargetShopGuid=75a071140bc819d4ab6e9e37abfdfa59`, and `MerchantStock.Quantity=1`.

Pending live validation:
- Open `Shop_Vendor_Tier1` and confirm a `BROODMOTHER_CALL_MERCHANT_STOCK added` log row.
- Throwaway-save summon path after explicit authorization.
- Throwaway-save Broodmother's Call registration, grant, cast, and cleanup after explicit authorization.
- Disabled-default no-spawn behavior.
- Recall, dismiss, duplicate prevention, save-exclusion readback, scene transition, shutdown cleanup, and command log review.

## 2026-08-06 Broodmother AI package source fixture

- `dotnet run --project mods/avalon-broodmother-companion/ai-package/tests/AvalonBroodmotherCompanion.AI.Package.V1.Fixtures/AvalonBroodmotherCompanion.AI.Package.V1.Fixtures.csproj -c Release` passed 8/8 fixtures.
- Passed checks covered package manifest, native/save ownership safety constants, exact live hero attacker goal policy, role and positioning rules, attack selection, telegraph/release/ShortRange 16 commit policy, Runtime V2 registration, and Contracts-only assembly references.
- Marker emitted: `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS package=kane.tgfoa.avalon-broodmother-companion.ai-spider-companion.v1 owner=kane.tgfoa.avalon-broodmother-companion role=avalon-broodmother-companion.broodmother target-source=hero.possible-attackers.live-only template=Spec_Broodmother_CI4 short-range=16 max-attack-slots=1 direct-native=0 native-jump=0 rabbit-bypass=0 goap-bypass=0 blaze-bypass=0 playmaker-native=0 save=0 random-spawn=0 persistence=0`.
- This was source/package validation only. No live host binding, deployment, FoA launch, save access, runtime package execution, or public packaging was run.

## 2026-08-06 Broodmother's Call cast hotfix

- Support bundle `20260806-135149Z-4e92624b` reported `NullReferenceException` during `Broodmother's Call` on `Magic_Summon_Ally` / `SkillSpawnLocation`.
- Release build passed with 0 warnings and 0 errors after adding the exact custom spell `Skill.Perform()` intercept.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `78C86F68F051D4032EF28786FCE8A8F5204C00B0B809DA68C218FAAB707E8A30`.
- Live BepInEx config was set to `Spell.EnableBroodmotherCallCompanionRoute=true` and `Spell.EnableBroodmotherCallSummonTarget=false`.
- The custom spell route now skips native `SkillSpawnLocation` for `Broodmother's Call` and calls the existing guarded one-session Broodmother companion summon path.
- The legacy native `SummonPrefab` rewrite setting now defaults to `false`.
- Live recast check after `0.1.2` load reached the custom spell intercept twice and skipped native `SkillSpawnLocation`, but both casts blocked before spawning because Avalon Awakened's Broodmother resolver returned `the exact Broodmother template did not resolve from the native template map`.
- Root cause for the no-spawn result: the approved `AvalonAwakenedSpiderBroodmotherCI4Template` ModService pack was missing from `C:\Users\kane0\AppData\LocalLow\Questline\Fall of Avalon\Mods`.
- Installed the approved pack from `D:\AA_S4T\AvalonAwakenedSpiderBroodmotherCI4Template` to the live FoA user `Mods` folder. Installed hashes matched the approved proof: JSON `31ADDF182B163EF01052246A00CF2640A291621E052E0045D5C03832C2D37C87`, hash file `CECA205C67DCB93495203B389A0139A2F265C97DC51762C49115A809A5BDD72F`, main bundle `EA7A621572F38E95EDA4A60AEC7FA2AC277A5A03040D4CE14535D0464E960263`, builtin shader bundle `AB208124DB3D02CA16A102DE140DF9DD361E1155C880ADD73012902D10EEAFE1`.
- No newer Avalon Exceptions incident existed after `20260806-135149Z-4e92624b` at the time of this check.
- Fresh FoA restart and one recast remain required because native ModService template discovery happens at startup.

## 2026-08-06 source-only AI wiring and direct native prompt

- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- The live plugin now compiles the primitive Broodmother AI contract/types as linked source and calls `BroodmotherCompanionAiV1Contract.Evaluate(...)` for live hero attacker decisions.
- Added a direct runtime-only native `Companion` prompt for the active verified Broodmother. At that point it opened the plugin panel and did not use Avalon Companions interop or add a mod dependency.
- No FoA launch, deployment, save access, live prompt interaction, live dialogue validation, live recast, or live leap/jump validation was run in this slice.

## 2026-08-06 spell description and Broodmother scale correction

- Updated `Broodmother's Call` display description to describe the actual allied one-session companion behavior, one-active limit, and recast replacement.
- Increased the Broodmother-only native visual scale multiplier from `5.0` to `5.5`, making this Broodmother version 10% larger.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- No FoA launch, deployment, save access, live spell UI check, or live scale visual validation was run in this slice.

## 2026-08-06 companion dialogue, native health-bar name, and status health bar

- Spawn now passes the display-name override `Broodmother` to `LocationTemplate.SpawnLocation(...)` and validates `Location.DisplayName` before accepting the companion, matching the native health-bar naming route recorded in the elephant correction research.
- The runtime-only native `Companion` prompt now opens a Broodmother-owned Unity UI dialogue command host instead of the small command panel.
- The dialogue host exposes Follow, Defend, Recall, Dismiss, and Leave choices, closes after each choice, and routes choices through the existing Broodmother command methods.
- The dialogue host shows the current mode/status, active display name, live threat count in Defend mode, and a health-percentage bar from `NpcElement.AliveStats.Health.Percentage`.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Source scan found no `StoryBookmark`, `StoryGraph`, `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, `VDialogue`, `PetTalkAction`, or `DialogueAction` references in Broodmother companion source.
- Source scan found no `ProjectReference`, `AvalonCompanionsInterop`, or `RegisterDialogueCommand` usage in Broodmother companion source/project files.
- No FoA launch, deployment, save access, live native prompt click, live dialogue rendering, native enemy health-bar display, or live combat validation was run in this slice.

## 2026-08-06 live responsiveness and companion HUD correction

- Added a plugin-owned active companion HUD that shows Broodmother display name, health percentage, follow/defend mode, live hero attacker count, and native prompt status independent of native hostile health-bar display.
- Later native-menu correction removed the companion-dialogue fallback hotkey route; the active Broodmother menu now opens through the native `Companion` prompt.
- Kept summon, recall, defend, dismiss, and panel hotkeys default-disabled.
- Reduced `Runtime.TickSeconds` and `CompanionAI.CombatPromptSeconds` defaults to `0.35` seconds and migrated previous default values `0.75` / `1.25` to the new defaults.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Source scan found no `StoryBookmark`, `StoryGraph`, `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, `VDialogue`, `PetTalkAction`, or `DialogueAction` references in Broodmother companion source.
- Source scan found no `ProjectReference`, `AvalonCompanionsInterop`, or `RegisterDialogueCommand` usage in Broodmother companion source/project files.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `132B474EF6830251EC633F7D6482569D822CE7E9DEC8624EE800CF0A6CDD4FA9`.
- No FoA launch, save access, live HUD rendering check, live hotkey interaction check, or live combat validation was run by Codex after this build.

## 2026-08-06 companion HUD legibility pass

- Live screenshot `Screenshot (5033).png` showed the plugin-owned Broodmother HUD was present in the live game, but the default IMGUI text and compact layout were too small and cramped for combat readability.
- Enlarged the Broodmother HUD plate, title text, health bar, mode/threat line, dialogue prompt, and status line without changing the companion logic, native prompt attachment, dialogue command host, AI package, summon route, or dependencies.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `6653D443A08BB8D647D12D57F1039BE4330B6661E34B5C7C267227C6EC1CF6E5`.
- No FoA launch, save access, or live second-screenshot HUD check was run by Codex after this build.

## 2026-08-06 companion command and responsiveness correction

- Live feedback after the HUD build reported no reliable dialogue options, recognition, logic, or responsiveness.
- Live logs showed the custom spell route spawned the Broodmother, attached the native `Companion` prompt, and loaded the live DLL, while command logs showed the advanced spider AI repeatedly returned `Hold` at mid-range before the target entered the attack commit envelope.
- This correction is superseded by the following native-menu correction; the active command surface is no longer this temporary direct command window route.
- Added immediate target pressure for live `Hero.PossibleAttackers`: while a live attacker exists, the host pushes `NpcAI.EnterCombatWith(target, forceChange: true)` plus `NpcHeroPetAlly.EnterCombat()` every `0.2` seconds before the package attack commit window opens.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Source scan found no `StoryBookmark`, `StoryGraph`, `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, `VDialogue`, `PetTalkAction`, `DialogueAction`, `ProjectReference`, `AvalonCompanionsInterop`, or `RegisterDialogueCommand` usage in Broodmother companion source/project files.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `AE6FE9ED16FFCABA1D18A9189F261DFFF9A699FE21DDD239B7D40137C3A2C94B`.
- No FoA restart, save access, live command-menu visual check, or live responsiveness retest was run by Codex after this build.

## 2026-08-06 native Companion menu correction

- User clarified the Broodmother must use the same native `Companion` prompt/menu style as the other companions.
- Restored a Broodmother-owned Unity UI dialogue host opened from the runtime-only native `Companion` `AbstractLocationAction`.
- The dialogue host shows right-side choices and a bottom dialogue text band with Follow, Defend, Recall, Dismiss, Leave, display name, mode/status, live threat count, and health percentage.
- Removed the companion-dialogue fallback hotkey route and the temporary direct command window route.
- Re-added the game-shipped `UnityEngine.UI` and `UnityEngine.UIModule` references needed for the Unity UI host; no Avalon Companions project reference or runtime dependency was added.
- Kept immediate target pressure for live `Hero.PossibleAttackers`; removed the unvalidated host-side engage-reposition teleport settings/path.
- Broodmother companion Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Source scan found no blocked story/dialogue or Avalon Companions interop references in the live Broodmother companion source/project files.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `1C8BB5B16DFFD9C0F8BA5FC7CAEC628921EFC72CFFD2A963B41ED8094A0AA498`.
- No FoA restart, save access, live native prompt visual check, live menu interaction check, or live combat retest was run by Codex after this build.

## 2026-08-07 Broodmother's Call tooltip and embedded item icon correction

- Rewrote the cloned `Broodmother's Call` light/heavy cast tooltip rows through `ItemTemplate.lightCastInfo` and `ItemTemplate.heavyCastInfo`, setting both rows to `Summon` and Broodmother summon/replace descriptions instead of inherited Wolf's Call text.
- Added a Broodmother-owned `ItemIconComponent.Refresh` prefix for the exact custom spell GUID `b7d0d4e6c0de4bb0a000000000000001`, using an embedded/procedural spider icon with no Tainted Interface dependency and no external icon file.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `404AABEF5AAEA6385951B0898F4B2AA83EFD2CF389D643955E24B30226787E58`.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- No FoA restart, save access, live inventory tooltip visual check, or live item icon rendering check was run by Codex after this build.

## 2026-08-07 native Companion prompt self-owned synchronization correction

- User rejected any bridge/dependency route and required the proven native companion process to be in the Broodmother mod itself.
- No Avalon Companions bridge or interop path was added. The Broodmother mod now self-owns the direct runtime-only native `Companion` action surface.
- The active Broodmother prompt is now synchronized through the companion lifecycle instead of only attached once: action and actor are marked not saved, stale prompt actions are removed on disabled/invalid/dismiss/shutdown paths, and the HUD reports `Companion ready` only when the actual native action is attached.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `C085A4C5E8F70C6EA7896190BD034ABA0D7936EAFDB5400110FB41D8145B8859`.
- `git diff --check` passed for the touched Broodmother companion source files before documentation updates.
- Protected-term scan passed for the touched Broodmother companion source files.
- Source scan found no Avalon Companions bridge/API references in the Broodmother source.
- No FoA restart, save access, live native prompt visual check, live menu interaction check, or live combat retest was run by Codex after this build.

## 2026-08-07 Tainted Interface companion UI and icon reflection

- Added a Broodmother-local Tainted Interface reflection bridge for `BeginCustomUiScope`, `EndCustomUiScope`, `EnsureInteractiveCursor`, `GetIcon`, and `GetItemIcon`; no project reference, package reference, or hard runtime dependency was added.
- Changed the Broodmother native `Companion` dialogue host to mirror the Avalon Companions menu process: Follow, Hold Position, Defend, Keep Close, Keep Pace, Keep Distance, Come Close, Recall, Recover, Dismiss, Leave, selected mode/range highlights, bottom listening/status text, and a Tainted Interface `companion.creature` portrait with `hud.companion-icon-background` when available.
- Added Broodmother command state for Hold Position and close/normal/far follow range. Follow catch-up now respects Hold Position and uses the companion range constants Close `14`, Far `34`, and existing Broodmother config for Normal.
- Updated Come Close to set close/follow and recall the Broodmother, Recover to recall only when out of range, and the command log to include `holdPositionMode` and `followRange`.
- Changed the `Broodmother's Call` item icon patch to try Tainted Interface first with `GetItemIcon(custom spell GUID)` and then `GetIcon(companion.creature)`, falling back to the embedded/procedural spider texture if Tainted Interface is unavailable or has no mapping.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for the touched Broodmother companion source and docs files.
- Protected-term scan passed for the touched Broodmother companion source and docs files.
- Source scans found no blocked story/dialogue references, no Avalon Companions bridge/API references, and no hard Tainted Interface project/package/reference entries. Only the expected Broodmother-local reflection strings were present.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `E86EBBEBE6EB8C48C46250897CE14B252185369E06FB12FC1D87E2B0DD22F190`.
- No FoA restart, save access, live native prompt visual check, live item icon rendering check, or live combat retest was run by Codex after this build.

## 2026-08-07 native interaction and Broodmother-side combat recognition correction

- Live feedback after the Tainted Interface reflection build reported no native interaction UI, no dialogue/options, and enemies walking past the Broodmother.
- Added Broodmother-owned native quick-command fallback actions alongside the `Companion` menu action: Follow, Hold Position, Defend, Come Close, Recall, Recover, and Dismiss. These actions route through the same Broodmother command executor as the dialogue menu, are marked runtime-only/not saved, and are removed with the active actor.
- Broadened immediate target pressure and advanced spider AI target selection from only `Hero.PossibleAttackers` to prioritized native candidates: `Hero.PossibleAttackers`, Broodmother `NpcElement.PossibleAttackers`, then Broodmother `NpcElement.PossibleTargets`.
- Command logs now include `targetSource` and candidate count for combat handoffs, and the HUD/dialogue threat count uses the same native combat candidate source as the combat selector.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures. Marker emitted `target-source=native-combat-candidates.prioritized-live-only`.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for the Broodmother companion source/docs/README/AI package paths.
- Source scans found no blocked story/dialogue references, no Avalon Companions bridge/API references, and no hard Tainted Interface project/package/reference entries. Only the expected Broodmother-local reflection strings were present.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `6C497316515CA68C5D4E6847786BE1399D0B1548AAD79851EF4C751590B28DF5`.
- Live FoA validation is still required to prove native prompt rendering, quick-command rendering, dialogue menu opening, and enemy engagement from Broodmother-side native targets.

## 2026-08-07 native menu action-stack correction

- User reported the Broodmother still was not connected to the game through the native interaction path.
- Repo evidence showed the working Avalon Companions process keeps one runtime-only native `Companion` `AbstractLocationAction` as the supported native menu entry point, while quick commands are an alternate fallback mode rather than a simultaneous action stack.
- Broodmother prompt synchronization now attaches the single `Companion` menu action and removes stale quick-command action elements from earlier builds on every synchronization pass.
- The `Companion` action availability check now matches Avalon Companions by using the action's owning `ParentModel` location.
- The HUD now reports `Native menu: Companion attached` only when the `Companion` action itself is attached.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures. Marker emitted `target-source=native-combat-candidates.prioritized-live-only`.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for `mods/avalon-broodmother-companion`.
- Source scans found no active native quick-command action attachment path, no blocked story/dialogue references, no Avalon Companions bridge/API references, and no hard Tainted Interface project/package/reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `1CB130075BAE4A7958779979DA6E03D56710D1B69F1E0069A629E52E8CF63FD7`.
- FoA was not running during the deploy check, so the next launch should load this deployed DLL. No save access, live native prompt visual check, live menu interaction check, or live combat retest was run by Codex after this build.

## 2026-08-07 embedded spider audio integration

- User supplied spider creature audio from `D:\downlods\Development folder\development assets\asset storage\c&c\AUDIO\creature\spider`.
- Converted the nine MP3 source files to durable Broodmother plugin PCM WAV resources under `src/Audio/spider` using FFmpeg `8.1.1`.
- Embedded one walking cue, one long call cue, and seven attack cues into `AvalonBroodmotherCompanion.dll` through `EmbeddedResource`.
- Added a Broodmother-local embedded audio runtime that validates RIFF/WAVE PCM resources, creates in-memory FMOD Core 3D one-shot sounds, updates/stops active voices, and releases owned sounds on teardown.
- Wired call audio to successful summon and recall, attack audio to successful native combat handoff paths, and walking audio to follow-distance ticks when the Broodmother is active and not already handling combat.
- Added audio config defaults: enabled `true`, volume `0.72`, min/max distance `1.0`/`28.0`, call cooldown `60.0`, attack cooldown `1.2`, walk cooldown `4.0`.
- Source MP3 hashes were pinned before conversion. Embedded WAV hashes: walk `CE1A0B265475BE76C3E4086551941B9AA401963D566D93CAB0C6A5746FD82A59`, call `1193D8B75A4C141EB354D17FEEC26190FED7DF5D2B11D145203A482112869D62`, attack cues `3F664CCE86A963231AC410CA0384DA66EAE438E4B6634E52548CE75C3D7B9356`, `DB1121EB661E741B1149E8086B01EC676AF6FFD478DFDCB005444FF51CCD7B68`, `D241D2CCC33B40FEDBEA02AE178B1E4CD4408D8571D4472E014354B6B3CF4AD2`, `98AA51C0F9E153B40C9836A840BF20C06D05B932E8D68F769C653F46AD0656C1`, `583EEBD2D04E0628559A415FB3234C100489F3ABEDE8210D88CE18F4CD47EEEC`, `A7A934A37E555D96B6074809C93C1EA0A7C9564AD1194E3653D72582372D6A4F`, and `FEF2E3C6A5CAD1F753DC747DFC1A0E740EE10ABEAA2A79C1F4DBE66673EDD070`.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- Static assembly resource inspection found all nine `AvalonBroodmotherCompanion.Audio.Spider.*` resources in the built DLL.
- FFprobe inspection confirmed every embedded WAV is `pcm_s16le`; the walking cue is 48 kHz stereo `4.320000` seconds, the call cue is 24 kHz stereo `58.440000` seconds, and each attack cue is 48 kHz stereo `3.024000` seconds.
- Source scans found no native audio attachment mutation, no FMOD Studio bank/event replacement, and no shared audio registry edit in Broodmother source.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `9478642CABA7463E14FD70C44905AE34E959DBCA15047EF32588DF100E4F497E`.
- FoA was not running during the deploy check, so the next launch should load this deployed DLL. No save access, live embedded audio startup check, or audible cue test was run by Codex after this build.

## 2026-08-07 native interactability and mid-range combat handoff correction

- Live screenshot `Screenshot (5049).png` showed the Broodmother HUD/icon/name/health were present, but user feedback reported no native dialogue/options and no responsive combat logic.
- Live command log inspection showed no `native-companion-prompt opened` row after the latest `synchronized` row, so the HUD's previous `Companion attached` text was only proving an action element existed, not that the native interaction scanner could start it.
- Local `TG.Main.dll` decompilation confirmed `Location.Interactable` is backed by `LocationInteractability`, and `Location.SetInteractability(LocationInteractability.Active)` is the native setter used by the game's visual scripting unit.
- Broodmother prompt synchronization now stores the active actor's original `LocationInteractability`, sets only the active verified not-saved Broodmother instance to `Active` while the native `Companion` prompt is attached, restores the original value during prompt cleanup, and reports `Native menu: Companion active` only when `Location.Interactable` is true.
- Live command log inspection showed repeated `advanced-spider-ai | Hold | broodmother.attack-not-committable` at about `6` to `8` meters while immediate target pressure only reported method-call success. The source-only AI package now makes mid-range `Reposition` drive native combat handoff instead of `Hold`; ShortRange 16 still requires the proven bite/leap release window.
- Broodmother combat handoff now mirrors native `NpcAlly.FindTarget()` more closely by registering the selected target with `NpcElement.TryAddPossibleCombatTarget(target)` before `NpcAI.EnterCombatWith(target, forceChange: true)`, and logs `targetRegistered`, `targetRegisteredChanged`, `npcInCombat`, `currentTarget`, and `currentTargetMatched` readback fields.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for `mods/avalon-broodmother-companion`.
- Source scans found no blocked story/dialogue references, no Avalon Companions bridge/API references, and no hard Tainted Interface project/package/reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `AB1B7C6C065FD6359E2D67DD2CBD8A6048CB3FBFC0F6ECAE7400B77CC605A55D`.
- Live FoA validation is still required to prove `Native menu: Companion active`, native menu opening, and visible Broodmother engagement in combat.

## 2026-08-07 Broodmother lifecycle and damageability correction

- Live feedback reported the Broodmother had no lifecycle and could not be hit.
- Local `TG.Main.dll` decompilation confirmed native `NpcHeroSummon` adds `HideHealthBar`, can add `HeroSummonInvisibility`, disables AlivePrefab colliders while the ally is not in combat, and prevents current-hero damage when the hero setting blocks damage to summons.
- Broodmother lifecycle maintenance now sets/readbacks `KeepCorpseAfterDeath=true`, requires `DeathElement`, removes `HideHealthBar`, removes `HeroSummonInvisibility`, re-enables AlivePrefab colliders every tick, requires enabled layer-24 non-trigger `BoxCollider` hitboxes after controller readiness, and logs `BROODMOTHER_COMPANION_LIFECYCLE_READY`.
- Added a Broodmother-scoped friendly-fire patch that bypasses `NpcHeroSummon.TryPreventFriendlyFire` only when the summon instance is the active Broodmother and the damage dealer is `Hero.Current`.
- Active companion cleanup now accepts native death only through the exact `NpcDummy` plus `Corpse` handoff and logs `BROODMOTHER_COMPANION_NATIVE_DEATH_ACCEPTED`, instead of treating a discarded/dead `NpcElement` as just a broken active actor.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `4D85C3E3D1E874D2502FD4BBBC8EC3D0EBA68A9A68F6ECB6902ECD145ACB6134`.
- No FoA restart, save access, live hit test, live native healthbar check, enemy-damage test, or death/corpse test was run by Codex after this build.

## 2026-08-07 Broodmother native dialogue recovery and collider correction

- Live screenshot `Screenshot (5050).png` showed the Broodmother HUD/name/icon/health present with `Native menu: Companion active`, but also showed `Broodmother lifecycle warning: Broodmother native actor has no layer-24 non-trigger BoxCollider hitboxes`; live BepInEx/command logs showed prompt synchronization but no native prompt-open entry.
- Removed the false Broodmother lifecycle failure on missing layer-24 `BoxCollider` hitboxes. The lifecycle surface now re-enables native AlivePrefab colliders and fails only when the native actor has no colliders or all colliders remain disabled after reassertion.
- Added a scoped native-interact recovery path for dialogue: pressing `E` opens the same Broodmother-owned companion menu only when the runtime native `Companion` action is attached, the active Broodmother is interactable, the hero is within the configured distance, and the camera is focused on that actor.
- Command logs now distinguish `native-companion-prompt,fallback-opened`, `native-companion-prompt,fallback-blocked`, and the existing `native-companion-prompt,opened` result so the next live check can identify whether the scanner, focus gate, or menu host is failing.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for `mods/avalon-broodmother-companion`.
- Source-only dependency scan found no Broodmother source `ProjectReference`, `PackageReference`, Avalon Companions interop, blocked story/dialogue API, or hard Tainted Interface reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `D69D942E395D666AA671AEA958B66530BE77D72B9DF8DA82E9A89572A9B28A5E`.
- Post-deploy log/process check found no running FoA process and the last BepInEx-loaded Broodmother version remained `0.1.10`, so `0.1.11` is deployed for the next launch but not yet loaded in a live session.
- No FoA restart, save access, live `E` interaction test, live dialogue rendering check, live hit test, enemy-damage test, or death/corpse test was run by Codex after this build.

## 2026-08-07 Broodmother companion menu cursor ownership correction

- Live screenshot `Screenshot (5052).png` showed the Broodmother companion menu now opens, but user feedback reported the cursor flashes and recenters every second.
- Source inspection found `BroodmotherCompanionDialogueHost.Tick()` called `EnsureCursor()` every frame, and `EnsureCursor()` called `TaintedInterfaceReflection.EnsureInteractiveCursor()` every frame while the Tainted scope was active.
- Repo UI research and examples establish `FoAModManagerApi.SetCustomUiScope(ownerId, active, freezeWorld)` plus `SetControllerCursorScope(ownerId, active)` as the shared cursor/input ownership route for modal custom UI.
- Added a Broodmother-local `FoAModManagerBridge` soft reflection bridge. The dialogue host now claims FoA Mod Manager custom UI scope once while opening the menu, releases it once on close/destroy, uses Tainted Interface custom scope only if FoA Mod Manager is absent, and only falls back to direct Unity cursor capture if neither shared scope is available.
- The dialogue close command log now records both `modManagerScope` and `taintedInterfaceScope`.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for `mods/avalon-broodmother-companion`.
- Source-only dependency scan found no Broodmother source `ProjectReference`, `PackageReference`, Avalon Companions interop, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `E3F1B421A2FEBFB276C64AF52C50140B4982C779CB8938DCF2C5E58C56169293`.
- Post-deploy log/process check found no running FoA process and the last BepInEx-loaded Broodmother version remained `0.1.11`, so `0.1.12` is deployed for the next launch but not yet loaded in a live session.
- No FoA restart, save access, live cursor-flash retest, live controller cursor test, or live close/restore test was run by Codex after this build.

## 2026-08-07 Broodmother companion menu virtual cursor correction

- Live screenshot `Screenshot (5053).png` showed the companion menu open, but user feedback reported only a small center dot that would not move away from the center for more than about a second.
- FoA Mod Manager research showed `SetCustomUiScope` owns input/cursor state and blocks game input, but the manager's controller cursor is internal and normal Unity `Input.mousePosition` can still reflect FoA's centered reticle.
- The Broodmother dialogue host now draws its own virtual cursor inside the Unity UI overlay, moves it from mouse delta plus keyboard/controller axes, uses it for button hover/hit-tests instead of `Input.mousePosition`, and accepts submit from left click, Enter, Space, or controller south button.
- Release build passed with 0 warnings and 0 errors using `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`.
- Broodmother companion AI package fixtures passed: all 8 `AvalonBroodmotherCompanion.AI.Package.V1` fixtures.
- `git diff --check` passed for `mods/avalon-broodmother-companion`.
- Protected-term scan passed for `mods/avalon-broodmother-companion`.
- Source-only dependency scan found no Broodmother source `ProjectReference`, `PackageReference`, Avalon Companions interop, blocked story/dialogue API, hard FoA Mod Manager reference, or hard Tainted Interface reference entries.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `A6001D0AA6973E0EFB017FDC5A4BD0D455EBE0A5E7C8F9A56F9E2E48B02742F0`.
- Post-deploy log/process check found no running FoA process and the last BepInEx-loaded Broodmother version remained `0.1.12`, so `0.1.13` is deployed for the next launch but not yet loaded in a live session.
- No FoA restart, save access, live virtual cursor retest, live click/submit test, or live close/restore test was run by Codex after this build.

## 2026-08-07 Broodmother companion menu native cursor event correction

- Live feedback after `0.1.13` reported the same center-dot cursor issue, and BepInEx logs confirmed `Avalon Broodmother Companion 0.1.13 loaded`.
- Working examples showed two missing pieces: `mods/crime-and-consequences/src/Plugin.cs` attaches `Awaken.TG.Main.UI.Cursors.ForceCursorVisibility` to the current hero while its popup is open, and working Mod Manager/companion panels process mouse events through `OnGUI`.
- The Broodmother dialogue host now attaches native `ForceCursorVisibility` while the menu is open, removes it on close/destroy, feeds the virtual cursor from `OnGUI` mouse move/drag events, draws the virtual cursor through `OnGUI` over the Unity UI, and handles GUI mouse-down hit-tests against the virtual cursor.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- `git diff --check -- mods/avalon-broodmother-companion` passed.
- Protected-term scan found no `Tales from the Age of Men`, `Tales From The Age Of Men`, `Age of Men`, or `overhaul` matches in the Broodmother source/docs/release/AI-package paths.
- Source-only dependency scan found no hard `FoAModManager`, hard `TaintedInterface`, Avalon Companions interop, native dialogue/story graph, or package/project reference matches in the Broodmother source project paths.
- Release and deployed DLL hashes matched: `D4FE1BC733C403153C429268A9BC81E8E7BFD7D01D35877DF06CAE55208DAB81`.
- Post-deploy log/process check found no running FoA process and the last BepInEx-loaded Broodmother version remained `0.1.13`, so `0.1.14` is deployed for the next launch but not yet loaded in a live session.
- No FoA restart, save access, live virtual cursor retest, live click/submit test, or live close/restore test was run by Codex after this build.

## 2026-08-08 Broodmother command-dialogue proven input path correction

- Live feedback after `0.1.25` reported the native `Companion` command menu still highlighted/registered rows but would not choose an option.
- Source review found the working Avalon Companions path opens companion dialogue through Tainted Interface `BeginCustomUiScope` first, falls back to FoA Mod Manager `SetCustomUiScope`, captures/restores the real cursor state, and keeps cursor visibility/unlock reasserted while visible.
- Source review found Broodmother still used the failed Mod-Manager-first scope order and returned early on active scope, so it did not run the same visible-menu cursor loop as the proven companion implementation.
- Broodmother `0.1.26` now uses the proven scope order and cursor loop while keeping the existing Unity `Button.onClick` plus manual real-pointer hit-test dispatch unchanged.
- Release build passed with `0` warnings and `0` errors for `mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj`.
- AI package fixtures passed all 8 checks and reported `BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS`.
- `git diff --check -- mods/avalon-broodmother-companion` passed.
- Source-only dependency scan found no hard `FoAModManager`, hard `TaintedInterface`, Avalon Companions interop, native dialogue/story graph, or package/project reference matches in the Broodmother source project paths.
- Deploy-on-build passed and updated the live BepInEx plugin folder.
- Release and deployed DLL hashes matched: `8C8B1C3BA6D581E9F9099DE1D68C7A21A012F29A73096F69562C6BCC3CD501F5`.
- Deployed assembly version read back as `0.1.26.0`.
- Protected-term scan returned one existing documentation validation-note line that literally names the protected terms; no protected files were touched.
- No FoA restart, save access, live menu click/choice test, live controller cursor test, or live close/restore test was run by Codex after this build.
