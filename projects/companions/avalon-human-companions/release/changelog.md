# Changelog

## 0.6.2

- Added the stable exact `Location.ID` to the engine-neutral Human observation boundary for the host-owned executor lease.
- Kept the existing exact-actor sequence and dispatch-time revalidation unchanged.
- Kept the native command boundary restricted to Hold, bounded recall, and `NpcHeroPetAlly.EnterCombat()`.
- Release build passed with zero warnings and zero errors. Live deployment and gameplay validation were not performed.

## 0.6.1

- Added one direct engine-neutral Avalon AI Runtime boundary owned by Avalon Human Companions.
- Added exclusive owner acquisition/release, one retained sequence tied to the exact active proof actor, and dispatch-time `Location`/`NpcElement`/managed-marker revalidation.
- Exposed only the reviewed runtime Hold-lock lifecycle, role-aware bounded recall, and native `NpcHeroPetAlly.EnterCombat()` defend handoff.
- Made the standalone automatic brain and legacy automatic assist branch stand down under Runtime ownership while keeping player commands, UI, Dismiss, and the not-saved lifecycle guard active.
- Added the guarded `World.EventSystem` check before `SceneLifetimeEvents.Get` so the boundary cannot recreate the prior startup-loading failure.
- Kept recruitment, capture, existing-NPC conversion, custom targeting/pathfinding, faction/crime/quest/dialogue mutation, persistence, and save data unchanged and blocked.
- Release build passed with zero warnings and zero errors; deployment completed while the game was closed and the live hash matched Release. Live load and behavior validation were not performed.

## 0.6.0

- Added scanner-only recruitment/capture research output: `human-recruitment-capture-research.csv`.
- Classified actor-like scanner rows into friendly recruitment, enemy capture, ambiguous, unknown, or one-session-proof research lanes using template/debug/display/component/faction hint strings only.
- Added blocked dry-run command rows for `recruit-friendly` and `capture-enemy`.
- Kept all recruitment/capture behavior blocked: no prompts, no panel buttons, no live capture/recruitment, no existing-NPC conversion, no faction/hostility/crime/story/dialogue edits, no persistence, and no save data.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.5.3

- Added live debug-panel controls for every `HumanBrainTuning` value: brain tick, Follow leash, Follow grace, combat recall distance, Follow/combat/emergency recall cooldowns, and native defend prompt cooldown.
- Made tuning changes apply immediately by resetting Follow leash grace and queuing a companion-brain refresh after each panel adjustment.
- Added `Save Config` and `Reset Defaults` buttons to the hotkey companion panel.
- Made the panel scrollable and taller so tuning controls stay available without putting debug controls into the NPC dialogue.
- Kept behavior inside the same runtime-only boundary: bounded recall and native `NpcHeroPetAlly.EnterCombat()` only, with no custom pathing, no custom target selection, no recruitment, no persistence, no existing-NPC conversion, and no save data.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.5.2

- Added `HumanBrainTuning` config entries for responsive companion-brain tick rate, Follow leash distance, Follow recall grace, responsive combat recall distance, Follow/combat/emergency recall cooldowns, and native defend prompt cooldown.
- Kept the 0.5.1 defaults so existing behavior remains the baseline until the config is edited.
- Logged the active responsive tuning values on plugin load for quick confirmation in BepInEx logs.
- Kept combat and movement inside the existing runtime-only boundary: bounded recall and native `NpcHeroPetAlly.EnterCombat()` only, with no custom pathing, no custom target selection, no recruitment, no persistence, no existing-NPC conversion, and no save data.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.5.1

- Widened the responsive Follow leash so companions have more room to move naturally before automatic catch-up recall.
- Added a short Follow recall grace window while responsive native assist is enabled, preventing instant teleport as soon as the leash threshold is crossed.
- Widened the responsive combat recall boundary while keeping the same native `NpcHeroPetAlly.EnterCombat()` handoff.
- Reset the Follow leash grace timer on mode changes, successful recalls, and companion-brain resets so stale leash state cannot force the next recall.
- Kept the change inside the existing runtime-only boundary: no custom pathing, no custom target selection, no recruitment, no persistence, no existing-NPC conversion, and no save data.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.5.0

- Added `HumanBrain.EnableResponsiveNativeAssist`, default true for generated configs.
- Made the companion brain evaluate at a faster bounded cadence while responsive assist is enabled.
- Tightened Follow recovery so the active one-session companion recalls sooner instead of needing to be dragged far behind.
- Added proactive native protection in Follow mode: when the hero already has live attackers, the brain may recall into guard placement and call `NpcHeroPetAlly.EnterCombat()` through the native ally path.
- Kept combat safety unchanged: no custom target scans, no target override, no attack button, no custom pathing, no persistence, and no recruitment.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.4.0

- Polished the runtime-only companion brain for the active one-session human companion.
- Added a smoother automatic follow leash with a small trigger buffer and separate cooldowns for follow, combat, and emergency recalls to reduce repeated catch-up spam.
- Kept explicit `Recall` and `Come Close` on the close placement path while returning clearer command status text.
- Added runtime Hold anchor feedback and cleanup without adding saved hold positions or transition/reload waiting.
- Improved Defend feedback around the native `NpcHeroPetAlly.EnterCombat()` handoff: active attackers report protection, no attackers reports watching for threats.
- Kept the change inside the existing research boundary: no recruitment, persistence, existing-NPC conversion, custom target selection, custom pathing, attack buttons, story dialogue, or save data.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.7

- Improved promoted human companion portrait identity with more specific candidate-family mappings.
- Added distinct embedded portrait selections for outlaw rogue/heavy, highwayman, deranged, archer, spear/halberdier, and armored knight rows.
- Kept the existing Dragon frame/background and passive HUD badge behavior unchanged.
- Kept the change UI-only: no roster approval, recruitment, persistence, existing-NPC conversion, custom target selection, command behavior, or save data changes.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.6

- Added a passive gameplay HUD badge for the active one-session human companion.
- Matched the Avalon Companions overlay route: default top-left placement, bottom-right option, size clamp, hidden while panel/dialogue is open.
- Kept the Dragon background plus inset portrait layering for panel, dialogue, and HUD badge.
- Kept the change UI-only: no recruitment, persistence, existing-NPC conversion, custom target selection, command behavior, or save data changes.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.5

- Added the embedded Tainted Interface companion icon background `Dragon/1 (10).png` as a premium portrait frame.
- Drew the selected summoning-panel portrait as a layered frame plus inset character portrait.
- Drew the active native-dialogue portrait as a Unity UI frame plus inset character portrait.
- Kept the change UI-only: no recruitment, persistence, existing-NPC conversion, custom target selection, command behavior, or save data changes.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.4

- Added embedded Tainted Interface character portraits to the human companion summoning panel.
- Added the active companion portrait to the native `Companion` dialogue bottom band.
- Loaded portrait PNGs through the existing optional Tainted Interface reflection bridge; no icon files are copied into this mod.
- Kept the change UI-only: no recruitment, persistence, existing-NPC conversion, custom target selection, command behavior, or save data changes.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.3

- Promoted the companion selector from the four starter rows to 13 hostile-risk human candidates while keeping the full 42 reviewed rows registered.
- Removed raw template/review/GUID/proof wording from the player-facing dialogue title/status and the hotkey summoning panel.
- Changed the summon panel to product-facing labels and statuses: `Call / Swap`, clean display names, role, active companion, stance, distance, and awareness.
- Removed scanner and lifecycle buttons from the normal summoning panel; those diagnostics remain internal/debug-only code paths.
- Extended role-aware companion-brain placement to the promoted role labels, including the new armored-melee role.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.2

- Suppressed duplicate automatic catch-up recalls to the same target for the active one-session proof actor.
- Prevented automatic companion-brain recalls from immediately forcing another brain tick, keeping the existing throttled cadence.
- Made native command-action cleanup idempotent so the active proof loop does not repeatedly remove already-absent quick actions.
- Kept the change runtime-only: no recruitment, persistence, custom target selection, custom pathing, existing-NPC conversion, story dialogue, or save data.

## 0.3.1

- Expanded the companion brain with role-aware bounded recall placement for the four starter candidates.
- Added `HumanBrain.EnableRoleAwarePlacement`, default true for newly generated configs.
- Added debug-panel role readout so the active brain role is visible in game.
- Kept explicit `Recall` and `Come Close` on close placement; brain catch-up and combat recall use the selected role profile.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.3.0

- Added the config-gated runtime-only companion brain for the active one-session human companion.
- The brain owns catch-up and Defend-mode native combat prompting when enabled, using only bounded recall and `NpcHeroPetAlly.EnterCombat()`.
- Added brain threat-state logging, native defend cooldown handoff, emergency recall, and debug-panel brain status.
- Kept Hold passive and kept recruitment, persistence, existing-NPC conversion, custom target selection, custom pathing, and save data out of scope.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.2.4

- Cleaned companion command feedback so dialogue and debug-panel commands share the same player-facing status lines.
- Changed the NPC dialogue Dismiss choice to `Part ways.` while keeping the debug-panel Dismiss button label unchanged.
- Kept Spawn, Scan, Lifecycle, and proof diagnostics on the debug panel only.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.2.0

- Productized the native NPC `Companion` dialogue into a companion-facing command menu.
- Removed `Spawn proof`, `Scan actors`, and `Lifecycle check` from the NPC dialogue; those remain available through the debug panel.
- Replaced player-facing proof/status wording in the dialogue with companion-facing text.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed and the live hash matched the release build hash.

## 0.1.21

- Made Follow reselectable for the active one-session proof actor so the command can re-arm Follow mode from the dialogue or proof panel.
- Clamped Follow-mode bounded catch-up recall to a closer proof range while keeping Defend on the configurable catch-up distance.
- Kept the implementation on the approved runtime-only managed proof actor path: no custom pathfinding, custom target selection, recruitment, persistence, existing-NPC conversion, or save-backed state.
- Release build passed with 0 warnings and 0 errors; live DLL deployment completed on 2026-06-24 after the game process was closed.
- In-game Follow validation is still required.

## 0.1.20

- Added the missing Avalon Companions GUI-pass dialogue maintenance path to the human dialogue surface.
- `OnGUI()` now refreshes cursor ownership and services the Unity UI dialogue host while the dialogue is visible, matching `PetCompanionController.DrawGui`.
- Kept the debug panel path, dialogue commands, proof roster, spawn/swap behavior, scanner routing, lifecycle guard, proof gates, recruitment, persistence, existing-NPC conversion, custom targeting, and save behavior unchanged.
- Release build and in-game dialogue input/cursor validation are still required.

## 0.1.19

- Reworked the human dialogue cursor/input controller to follow the proven Avalon Companions path directly.
- Restored shared interface custom UI scope first, with FoA Mod Manager `SetCustomUiScope` fallback, matching the Avalon Companions controller lifecycle.
- Added the shared human `UpdateCursor()` lifecycle used by the companion controller and moved Avalon interactive cursor refresh into `EnsureCursorForPanel()`.
- Restored panel input modules only after neither the human debug panel nor the human dialogue surface remains visible.
- Kept the 0.1.18 dialogue Rewired pass-through because that also matches Avalon Companions.
- Kept proof roster, spawn/swap behavior, commands, scanner routing, proof gates, recruitment, persistence, existing-NPC conversion, custom targeting, and save behavior unchanged.
- Release build passed with 0 warnings and 0 errors. In-game dialogue input/cursor validation is still required.

## 0.1.18

- Fixed the native `Companion` dialogue input/cursor freeze by matching Avalon Companions' dialogue input path.
- Human dialogue UI now lets Rewired axis/button reads pass while the dialogue-style surface is visible, while keeping the debug panel input lock strict.
- Human UI cursor/input scope tried FoA Mod Manager `SetCustomUiScope` first and fell back to the shared interface bridge only if the Mod Manager API was unavailable. Version 0.1.19 supersedes this scope order to match Avalon Companions.
- Kept proof roster, spawn/swap behavior, commands, scanner routing, proof gates, recruitment, persistence, existing-NPC conversion, custom targeting, and save behavior unchanged.
- Release build passed with 0 warnings and 0 errors. In-game dialogue input/cursor validation is still required.

## 0.1.17

- Adjusted the debug panel sizing after screenshot evidence showed the registered proof roster panel was too cramped on a 3840-wide viewport.
- Kept the compact Avalon Companions-style layout but changed dimensions to grow on large displays: 42% viewport width and 38% viewport height, clamped to screen margins.
- Kept proof roster, command behavior, scanner routing, proof gates, recruitment, persistence, existing-NPC conversion, custom targeting, and save behavior unchanged.
- Screenshot also showed `Gate: research mode on`, so disabled live proof buttons were expected with the current config.

## 0.1.16

- Registered the 42 generated `ReviewQueueNonUniqueSpawnerBacked` human proof candidates in a companion-style runtime roster.
- Added `HumanRoster.EnableProofCandidateRoster` and `HumanRoster.SelectedCandidateIndex`.
- Updated the debug panel to show selected candidate count/name, template, GUID, review ID, and risk flags.
- Added Prev, Spawn / Swap, and Next controls that mirror the Avalon Companions selected-index flow.
- Routed selected candidates through the existing one-session ally proof backend, including research gate, proof gate, non-unique checks, not-saved marking, and native summon-faction plus `NpcHeroPetAlly`.
- Scanner evidence now defaults to the selected proof candidate GUID when `ActorScanner.SelectedTarget` is empty.
- Kept all human safety boundaries unchanged: no recruitment, persistence, existing-NPC conversion, story graph dialogue, custom targeting, true Wait, saved hold positions, or release-ready human companion behavior.
- Release build passed with 0 warnings and 0 errors. In-game roster/panel validation is still required.

## 0.1.15

- Corrected the command-surface split to match Avalon Companions.
- Native NPC `Companion` interaction now opens the companion-style right-side dialogue choices and bottom dialogue text band.
- `HumanCommandPanel.TogglePanelHotkey` now opens a compact Avalon Companions Debug-style IMGUI control panel instead of the dialogue surface.
- Kept all human proof command backends and safety boundaries unchanged.
- Release build passed with 0 warnings and 0 errors. In-game validation is still required.

## 0.1.14

- Replaced the visible centered IMGUI proof command panel with a companion-style vanilla Unity UI command surface.
- The surface now mirrors Avalon Companions' layout: transparent full-screen blocker, right-side command choices, and a bottom dialogue text band.
- Routed Follow, Hold, Defend, Come Close, Recall, Spawn Proof, Scan Actors, Lifecycle Check, Dismiss, and Goodbye through the existing proof backend.
- Kept all human safety boundaries unchanged: no recruitment, persistence, existing-NPC conversion, story graph dialogue, custom targeting, true Wait, or saved hold positions.
- Release build passed with 0 warnings and 0 errors. In-game visual/native-prompt validation is still required.

## 0.1.13

- Made `HumanCommands.EnableNativeCommandActions` default to `true` for newly generated configs.
- Made `HumanCommandPanel.EnableProofCommandPanel` default to `true` for newly generated configs.
- Kept proof spawning, scanner output, research-mode override, defend assist, recruitment, persistence, existing-NPC conversion, true Wait, and saved hold positions separately gated or blocked.
- Existing BepInEx configs are not overwritten by BepInEx default changes; set the two keys manually in an existing config if they are already present as `false`.
- Release build passed with 0 warnings and 0 errors. Clean config generation and in-game native prompt validation are still required.

## 0.1.12

- Added one runtime-only native `Companion` prompt for the active one-session human proof actor.
- When the proof panel is enabled, the prompt opens the existing plugin-owned proof command panel and removes the older separate quick command action elements from the native prompt surface.
- Kept all existing proof command gates unchanged: no new commands, recruitment, persistence, existing NPC conversion, dialogue, custom target selection, true Wait, or saved hold positions.
- Release build passed with 0 warnings and 0 errors. In-game native prompt validation is still required.

## 0.1.11

- Added optional Tainted Interface integration for shared dark-fantasy proof-panel styling and shared custom UI scope.
- Kept the existing local IMGUI panel skin and direct FoA Mod Manager scope fallback when Tainted Interface is absent.
- Kept the proof panel disabled by default and did not change proof commands, actor gates, scanner output, lifecycle behavior, persistence, target selection, or hotkeys.
- Build/deploy passed and local/live DLL hashes match.

## 0.1.10

- Increased the proof panel default size from `980x620` to `1320x860`.
- Increased the readable minimum panel size and left status-column width.
- Slightly increased title, label, muted text, close button, and command button sizing.
- Cleared IMGUI hot/keyboard focus after panel actions so buttons do not stay in a strange clicked/focused color state.
- Pinned explicit button `normal`, `hover`, `active`, `focused`, and `on*` states so Unity's default button skin does not leak unexpected colors.
- Changed selected command buttons from the harsh brown state to a calmer teal selected state with gold text.
- Build/deploy passed and local/live DLL hashes match.
- No human NPC roster, recruitment, existing NPC conversion, persistence, true Wait, custom targeting, dialogue, quest, scanner behavior, lifecycle behavior, or command behavior was added.

## 0.1.9

- Reorganized the proof panel into status, prepare/evidence, movement, combat, cleanup, and status-footer flow.
- Added panel input/focus lock based on the proven Avalon Companions and FoA Mod Manager Harmony/Rewired pattern.
- Added `Lifecycle Check` as an evidence-only panel button.
- Added `tools/New-HumanNpcDiagnosticMap.ps1` and generated review-only human NPC map files from dev FOA-Diagnostic Tool dump `20260616-090615`.
- Build/deploy passed. User-reported in-game 0.1.9 panel focus/input validation passed, and `LogOutput.log` confirmed 0.1.9 load plus input-lock patch application.
- No human NPC roster, recruitment, existing NPC conversion, persistence, true Wait, custom targeting, dialogue, quest, or release-ready UI behavior was added.

## 0.1.8

- Added a proof-only Hold command for the active one-session human proof actor.
- Hold applies a plugin-owned runtime-only movement block through `ICanMoveProvider` on the active proof actor's `NpcElement`.
- Added Hold to the proof command panel and native quick command action set.
- Hold cleanup runs when Follow, Come Close, Defend, or Dismiss is selected.
- Recall remains a reposition command and does not change the selected proof mode.
- No true Stay/Wait package, saved hold position, transition/reload-safe waiting, recruitment, persistence, existing NPC conversion, custom targeting, or release-ready UI behavior was added.

## 0.1.7

- Added lifecycle diagnostics/guardrails for the active one-session proof actor.
- Added `HumanLifecycle.EnableLifecycleSafetyGuard`, `HumanLifecycle.WriteLifecycleDiagnostics`, and `HumanLifecycle.LifecycleTickSeconds`.
- The guard keeps the active proof actor marked not saved, discards it if it loses the expected managed proof marker, and clears tracked state after Dismiss/discard.
- Added `human-proof-lifecycle.csv` event rows for lifecycle evidence. Rows are plugin-owned evidence only and keep behavior and persistence approval false.
- No recruitment, persistence, existing NPC conversion, save-backed roster, dialogue, quest, inventory, equipment, leveling, or release-ready UI behavior was added.

## 0.1.6

- Added a gated `Scan Actors` button to the proof command panel.
- The button uses the existing disabled actor scanner path and is enabled only when `ActorScanner.EnableDisabledActorScanner=true`.
- The button writes the existing CSV evidence files only; it does not move, command, recruit, convert, faction-edit, target-edit, interaction-edit, persist, or otherwise approve actors.
- Added research notes for the panel scanner trigger because the scanner hotkey did not produce CSV output or a scanner log line during the latest live check.

## 0.1.5

- Polished the proof command panel layout after in-game testing showed the 0.1.4 panel was too small and crowded.
- Increased the panel from a cramped fixed debug-sized window to a larger responsive modal with minimum/maximum screen clamps.
- Split actor status and command controls into separate sections, enlarged labels/buttons, added clearer proof-only status text, and allowed dragging from the header area.
- Kept behavior unchanged: commands still only target the active plugin-owned one-session proof actor; no Stay/Wait/Hold Position, existing NPC conversion, save data, custom targeting, or persistence was added.
- Build and live DLL hash validation passed; in-game 0.1.5 screenshot validation is still required before calling the panel release-ready.

## 0.1.4

- Added a disabled-by-default proof command panel for the active one-session human ally proof actor.
- Added `HumanCommandPanel.EnableProofCommandPanel` and `HumanCommandPanel.TogglePanelHotkey`; default panel hotkey is `End`.
- Panel commands use the existing proof backend for Spawn Proof, Follow, Come Close, Recall, Defend, and Dismiss.
- Disabled native quick command actions in the live test config because the native interaction path did not work in game.
- Kept the panel proof-only: no Stay/Wait/Hold Position, no existing NPC conversion, no save data, no vanilla interaction-list edits, and no release-ready UI claim before screenshot validation.

## 0.1.3

- Added disabled-by-default native quick command actions for the active one-session human ally proof actor.
- Added Follow, Come Close, Recall, Defend, and Dismiss commands as runtime-only `AbstractLocationAction` elements.
- Kept commands restricted to the plugin-owned proof actor with `NpcHeroPetAlly`; no existing NPC conversion, save data, dialogue, story, quest, crime, global faction, or custom target selection is added.
- Kept true Stay/Wait/Hold Position blocked until a humanoid ally wait API is researched and validated.

## 0.1.2

- Added a disabled-by-default `HumanAllyProof` config section for one throwaway-save native ally proof.
- Mirrored the Avalon Companions creature-candidate ownership path on a plugin-owned spawned human proof actor: hero summon faction override plus `NpcHeroPetAlly`.
- Added guards for research mode, reviewed target GUID, non-unique attachment, non-unique spawned `NpcElement`, one proof per session, not-saved marking, setup failure discard, and explicit dismiss.
- Documented that this is not recruitment, persistence, dialogue, command UI, custom targeting, or conversion of existing NPCs.

## 0.1.1

- Added a default-off human actor scanner that writes plugin-owned CSV evidence only.
- Added dry-run command classification rows for follow, stay, recall, dismiss, defend, and panel. All rows remain blocked with `liveAction=false`.
- Documented that human behavior and a human command panel remain blocked until scanner CSV review and separate ownership, faction/targeting, interaction, transition, and save/load proofs pass.
- Updated proof notes to record the user-confirmed safe proof spawn smoke test.

## 0.1.0

- Added the initial Avalon Human Companions diagnostic scaffold.
- Added conservative config gates for future human NPC companion research.
- Documented that human NPC recruitment, spawning, command behavior, dialogue, faction changes, and persistence are blocked until separate research and validation pass.
- Added a default-off safe proof spawn command for one explicit non-unique `LocationTemplate` GUID. The proof spawn is marked not saved immediately and applies no companion behavior.
