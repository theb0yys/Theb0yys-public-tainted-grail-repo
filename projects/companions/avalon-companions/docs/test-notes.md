# Test Notes

## 0.1.42

- Added `docs/research/lifecycle-validation-gate-2026-06-21.md`.
- 0.1.42 is a lifecycle validation gate route, not a lifecycle behavior change.
- Required in-game smoke: summon one reviewed managed one-session creature candidate on a throwaway save, press `Lifecycle Check`, transition or fast travel, press `Lifecycle Check`, rest, press `Lifecycle Check`, save, quit, reload, return, then inspect `companion-lifecycle.csv` and `companion-command-log.csv`.
- Pass criteria: managed rows keep `markedNotSaved=true`, active roster count stays zero or one, duplicate cleanup leaves at most one active managed actor, no unmanaged native `Companion` prompt remains, no auto-restore/auto-respawn/persistence/re-adoption happens after quit/reload, and command rows keep `touchesPersistence=false` plus `touchesTargeting=false`.
- Does not change lifecycle behavior, actor ownership, `MarkedNotSaved` handling, persistence, reload restoration, auto-respawn, re-adoption, custom AI/pathing, target selection, attack UI, Core execution, taming, training, or loyalty.
- Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.42.0`.
- Built Release DLL SHA-256: `6EFADED79F911D2B98C8C5163D9F30F93F44C9775F23A6D38EB255D62C3E4096`.
- Live deploy passed. Live DLL file version `0.1.42.0`; live SHA-256 matched `6EFADED79F911D2B98C8C5163D9F30F93F44C9775F23A6D38EB255D62C3E4096`.
- In-game lifecycle smoke is pending.
- CSV review after smoke is pending.

## 0.1.41

- Added `docs/research/native-dialogue-decision-gate-2026-06-21.md`.
- Added `docs/decisions/0007-native-dialogue-supported-surface.md`.
- Decision: keep the plugin-owned Unity UI dialogue host as the supported Avalon Companions dialogue surface.
- The runtime native `Companion` prompt remains a not-saved `AbstractLocationAction` entry point into the plugin-owned command UI, not a true native `VDialogue` / `StoryBookmark` route.
- Does not add fake `StoryBookmark` values, runtime story graphs, dialogue attachments, pet-talk attachments, story interaction actions, template edits, save-owned interaction list edits, custom AI/pathing, target selection, attack UI, persistence, Core execution, taming, training, or loyalty.
- Debug build passed with 0 warnings and 0 errors.
- Release build failed once when run in parallel with Debug due to a NuGet restore file-exists race, then passed on standalone rerun with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.41.0`.
- Built Release DLL SHA-256: `F6CD7CE29629D2FC16CFFC2F6453D6B9015A581CB5ADAC0C4A7104A0F3CFA5D1`.
- In-game smoke is not required for this design-only slice because no runtime behavior changes beyond version metadata are intended.

## 0.1.40

- Added `docs/research/profile-driven-native-assist-smoke-fix-2026-06-21.md`.
- 0.1.39 in-game smoke with `Companions.EnableProfileDrivenNativeAssist=true` did not pass: live attacker evidence produced legacy `defend-prompt` rows and `NativeCombatObserved` profile rows, but no `ai-profile-native-defend` row because the older defend tick claimed the native prompt cooldown first.
- While `Companions.EnableProfileDrivenNativeAssist=true`, the older creature follow and defend ticks now stand down so the profile loop owns the already-approved catch-up recall and native ally combat prompt paths.
- Does not add custom targeting, pathing, target overrides, attack buttons, persistence, Core execution, taming, training, or loyalty.
- Debug build, Release build, and live deploy passed.
- Built and live Release DLL file version: `0.1.40.0`.
- Built and live Release DLL SHA-256: `112E5883EF968D09EC397FC8895172B0E56DE3ECC7A5BEC2ABF708908CC8A5E6`.
- BepInEx load validation passed for `Avalon Companions 0.1.40`; the load line included `EnableProfileDrivenNativeAssist=True` and `ProfileDrivenNativeAssistSeconds=1`.
- Command-log rerun baseline is row `375`.
- In-game native-assist smoke passed. Live-attacker testing produced four clean `ai-profile-native-defend` rows and no duplicate legacy defend/catch-up rows after baseline `375`.
- Dedicated catch-up smoke passed after follow-up baseline `399`: six `ai-profile-native-catch-up` rows appeared in Follow/Close mode, `companion-ai-profile.csv` showed `FollowCatchUpCandidate` rows above the `14` catch-up threshold, and duplicate/unsafe checks returned zero.

## 0.1.39

- Added `docs/research/profile-driven-native-assist-hardening-2026-06-21.md`.
- Hardened profile-driven native assist against immediate duplicate follow/defend tick actions.
- After `ai-profile-native-catch-up`, the regular creature follow tick is deferred by the existing follow tick interval.
- After `ai-profile-native-defend`, the regular creature defend tick is deferred by the existing defend tick interval.
- Does not add custom targeting, pathing, target overrides, attack buttons, persistence, Core execution, taming, training, or loyalty.
- Debug build, Release build, and live deploy passed.
- Built and live Release DLL file version: `0.1.39.0`.
- Built and live Release DLL SHA-256: `BCA9DBD9D11CEB43CFEC895A95BD4D8AD7F45FE2B7C29A595B92302F35EE0A0C`.
- BepInEx load validation and in-game native-assist smoke are pending.

## 0.1.38

- Added `docs/research/profile-driven-native-assist-2026-06-21.md`.
- Added `Companions.EnableProfileDrivenNativeAssist=false` and `Companions.ProfileDrivenNativeAssistSeconds=1`.
- When enabled, `FollowCatchUpCandidate` can trigger the existing recall/catch-up placement path.
- When enabled, `NativeCombatObserved` can trigger the existing `NpcHeroPetAlly.EnterCombat()` path only when the hero already has live attackers and Defend/native-assist gates allow it.
- Command audit rows are `ai-profile-native-catch-up` and `ai-profile-native-defend`.
- Does not add custom targeting, pathing, target overrides, attack buttons, persistence, Core execution, taming, training, or loyalty.
- Debug build, Release build, and live deploy passed.
- Built and live Release DLL file version: `0.1.38.0`.
- Built and live Release DLL SHA-256: `607D31232E4E756F383ABA85CEC341D9DC55BFBC6473BA6B15EF31B160AF7992`.
- BepInEx load validation and in-game native-assist smoke are pending.

## 0.1.37

- Added `docs/research/ai-profile-check-smoke-aid-2026-06-21.md`.
- Added a debug-panel `AI Profile Check` button for immediate profile-audit smoke snapshots.
- The button is visible in the debug panel even when `Diagnostics.WriteCompanionAiProfileAudit=false`.
- It writes one `panel-ai-profile-check` snapshot to `companion-ai-profile.csv` only when the profile audit config is enabled; otherwise it reports the blocked config state and writes a blocked `ai-profile-check` audit row to `companion-command-log.csv`.
- With no active managed companion, the enabled snapshot writes a valid `NoActiveCompanion` profile row.
- It does not dispatch companion commands, queue native follow/defend refresh, move actors, change targets, persist actors, store progression state, or execute behavior through Avalon Core.
- Debug build and Release build passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.37.0`.
- Built Release DLL SHA-256: `91B0C7E0CBD1462E8B168FFF52379CE2987C30209256309AD5E9384C498DFD16`.
- Live deploy was not run because `Fall of Avalon` was still running and the live DLL may be locked.
- BepInEx load validation and in-game diagnostic smoke are pending.

## 0.1.36

- Added `docs/research/ai-profile-audit-smoke-review-2026-06-21.md`.
- Enabled live profile audit config for the smoke: `Diagnostics.WriteCompanionAiProfileAudit=true`, `Diagnostics.CompanionAiProfileAuditSeconds=2`.
- BepInEx load validation passed for `Avalon Companions 0.1.35`; the load line included `WriteCompanionAiProfileAudit=True` and `CompanionAiProfileAuditSeconds=2`.
- Reviewed live `companion-ai-profile.csv` evidence: 27 rows from `CampaignMap_HOS`, 14 active `Sharg Candidate` rows, 13 `NoActiveCompanion` rows, 5 `MovementBlocked` rows, and 9 `IdleNativePatrol` rows.
- Reviewed safety flags: `UNSAFE_FLAGS=0`; all reviewed rows kept `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Reviewed command-log crosscheck: latest smoke rows included `summon/swap`, `recover`, `summon-existing-recall`, `range-close`, `heel`, and one `ai-boundary-check`; relevant command rows kept `touchesTargeting=false` and `touchesPersistence=false`.
- Did not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.
- Not yet covered: `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, interiors, stealth, transition, rest, quit/reload, or return.
- No code build was required for this evidence-review slice; it reviewed the already deployed 0.1.35 DLL.

## 0.1.35

- Added runtime-only `CompanionAiProfile` and `CompanionAiIntent` framework contracts.
- Added `Diagnostics.WriteCompanionAiProfileAudit=false` and `Diagnostics.CompanionAiProfileAuditSeconds=2`.
- When enabled, the audit appends `companion-ai-profile.csv` rows classifying observed active managed companion state as `IdleNativePatrol`, `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, `MovementBlocked`, `NoTargets`, `EvidenceInsufficient`, or `NoActiveCompanion`.
- Rows explicitly record `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Did not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.
- Debug build, Release build, and Release deploy build passed with 0 warnings and 0 errors.
- Built and live Release DLL file version: `0.1.35.0`.
- Built and live Release DLL SHA-256: `A67329CC836E85776A95750C2B95C4AE68A506E61714DBD82B2A4461ED176EA0`.
- BepInEx load validation and the first in-game profile audit smoke passed during the 0.1.36 evidence-review slice.

## 0.1.34

- Added `docs/research/ai-boundary-evidence-review-2026-06-21.md`.
- Reviewed live `companion-ai-boundary.csv` evidence: 276 rows, 273 periodic rows, 3 manual panel rows, 43 native combat rows, 64 rows with one live hero attacker, and `UNSAFE_FLAGS=0`.
- Reviewed live `companion-command-log.csv` crosscheck: 288 rows, 13 `ai-boundary-check` rows, 36 `summon/swap` rows, and 27 `dismiss` rows at review time.
- Defined only a future non-mutating `CompanionAiProfile` / `CompanionAiIntent` classifier design.
- Did not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.
- Debug build, Release build, and Release deploy build passed with 0 warnings and 0 errors.
- Built and live Release DLL file version: `0.1.34.0`.
- Built and live Release DLL SHA-256: `453D0088285DFE64F6F55AD9EC883FA06E7E31FD74D8057CB708A841C5B7101D`.
- BepInEx load validation for 0.1.34 is pending.

## 0.1.33

- Added a debug-panel `AI Boundary Check` button for immediate diagnostic smoke snapshots.
- The button is visible in the debug panel even when `Diagnostics.WriteCompanionAiBoundaryDiagnostics=false`.
- It writes one `panel-ai-boundary-check` snapshot to `companion-ai-boundary.csv` only when the diagnostic config is enabled; otherwise it reports the blocked config state and writes a blocked `ai-boundary-check` audit row to `companion-command-log.csv`.
- It does not dispatch companion commands, queue native follow/defend refresh, move actors, change targets, persist actors, store progression state, or execute behavior through Avalon Core.
- Debug build and Release build passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.33.0`.
- Built Release DLL SHA-256: `DB64D8DA4B0D7FC4322C883E6BEA61878333ECF95EB72AE1B9E7E1CCC033D347`.
- Live deploy: Passed. Live DLL file version `0.1.33.0`; live SHA-256 matched `DB64D8DA4B0D7FC4322C883E6BEA61878333ECF95EB72AE1B9E7E1CCC033D347`.
- Live deploy, BepInEx load validation, and in-game diagnostic smoke are pending.

## 0.1.32

- Added a default-off read-only companion AI boundary diagnostic.
- `Diagnostics.WriteCompanionAiBoundaryDiagnostics=false` by default; when enabled, active managed companions append `companion-ai-boundary.csv` rows with native AI, movement, unchecked target relation, possible target/attacker counts, and movement-blocker state.
- Rows explicitly record `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- The diagnostic avoids native target recalculation and `TargetOverrideElement.GetTarget`; it does not dispatch commands, move actors, change targets, persist actors, store progression state, or execute behavior through Avalon Core.
- Debug build and Release build passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.32.0`.
- Built Release DLL SHA-256: `4C3F766D390835AE7C7AE83F3545B8DBABFBE6291C8EB8D8D523325ED91EA0CD`.
- Live deploy, BepInEx load validation, and in-game diagnostic smoke are pending.

## 0.1.31

- Raised the passive top-left companion HUD badge so it sits just above the Wyrd Scent/status overlay lane shown in user screenshot `Screenshot (3838).png`.
- Bottom-right HUD placement and all companion command behavior are unchanged.
- Release build passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.31.0`.
- Built Release DLL SHA-256: `E47203C8C83A862F0FD01A2DFEB602E34FC4801E4911D71CD39EA03E3298DDB2`.
- First live deploy attempt failed while Fall of Avalon was running because the live DLL had a user-mapped section open.
- Live deploy passed after closing Fall of Avalon. Live DLL SHA-256 matched `E47203C8C83A862F0FD01A2DFEB602E34FC4801E4911D71CD39EA03E3298DDB2`.
- BepInEx load validation and in-game HUD smoke are pending: confirm the badge clears the Wyrd Scent/status overlay and still disappears when no managed companion is active.

## 0.1.30

- Polished the existing Unity UI companion command surface with row accents for hover, selected, danger, and disabled states.
- Muted unavailable companion command labels while preserving the existing bottom-text runtime block reason.
- Routed Unity button clicks and manual hit-test clicks through a single dialogue choice handler.
- Added `dialogue-choice` and `dialogue-close` audit rows to `companion-command-log.csv`; the rows record surface, command id, route, result, and close behavior in the existing `reason` field.
- Intended behavior change: command-surface feedback and audit evidence only. Spawn, recall, dismiss, lifecycle, prompt attachment, combat, targeting, persistence, Core behavior, and approved command effects are unchanged.
- Debug build, Release build, and Release deploy build passed with 0 warnings and 0 errors.
- Built and live Avalon Companions DLL file version: `0.1.30.0`.
- Built and live Avalon Companions DLL SHA-256: `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- In-game smoke pending: open `Companion`, verify hover/selected/disabled visuals, click Follow and Stay, click Goodbye, press Esc, confirm `KeypadPeriod` still opens only `Avalon Companions Debug`, and inspect `companion-command-log.csv` for `dialogue-choice` and `dialogue-close` rows.

## 0.1.29

- Added a passive gameplay HUD badge for the active managed companion.
- The badge uses the existing per-companion icon mapping and requests Tainted Interface `hud.companion-icon-background` for the Dragon frame.
- Added `Companions.ShowCompanionHudIcon`, `Companions.CompanionHudIconCorner`, and `Companions.CompanionHudIconSize`.
- The active companion lookup is throttled and texture lookups are cached; the badge hides while the companion debug panel or custom dialogue is visible.
- Intended behavior change: passive HUD overlay only. Spawn, summon, recall, dismiss, lifecycle, native prompt, dialogue, combat, command, target, persistence, and Avalon Core behavior are unchanged.
- Avalon Companions Release build passed with 0 warnings and 0 errors.
- Built current Avalon Companions Release DLL SHA-256: `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- Built current Avalon Companions Release DLL file version: `0.1.30.0`.
- Required Tainted Interface embedded Release build passed with 0 warnings and 0 errors.
- Built Tainted Interface Release DLL SHA-256: `BDCC7657D563FA43EE66E04456FFEF872C590D476E287A3C586202D1D0591525`.
- Built Tainted Interface Release DLL file version: `0.1.4.0`.
- Tainted Interface embedded resource check passed: `DarkFantasy=12`, `Icons=50`, and `TaintedInterface.Assets.Icons.HudCompanionIconBackground.png` is present.
- Live deploy passed. Live Avalon Companions DLL SHA-256 matched `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- Live Tainted Interface DLL SHA-256 matched `BDCC7657D563FA43EE66E04456FFEF872C590D476E287A3C586202D1D0591525`.
- BepInEx load validation passed. `LogOutput.log` showed `Loading [Avalon Companions 0.1.30]` and `Avalon Companions 0.1.30 loaded` with the HUD badge config enabled.
- In-game HUD smoke is pending.

## 0.1.28

- Fixed the debug panel selected-roster icon resolver so existing companion families request distinct Tainted Interface icon IDs instead of falling back to `companion.creature`.
- Added mappings for Qrko pets, wolf, bear, deer, pig, cow, bullrat, corpse eater, redcap, flamegobbler, grindylow, wyrdspirit, sharg, ogre, zombie, drowner, skeleton, floatling, and tadpole rows.
- Kept `companion.creature` only as the unmapped future fallback.
- Intended behavior change: debug-panel icon selection only. Spawn, recall, dismiss, lifecycle, prompt, dialogue, input, combat, and command behavior are unchanged.
- Release build passed with 0 warnings and 0 errors.
- Built Release DLL SHA-256: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- Built Release DLL file version: `0.1.28.0`.
- Live deploy passed. Live DLL SHA-256 matched Release output: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- BepInEx load validation passed on 2026-06-20: `LogOutput.log` showed `Loading [Avalon Companions 0.1.28]` and `Avalon Companions 0.1.28 loaded`.
- In-game command-click smoke passed by user report and command CSV evidence: `mode-follow` at `2026-06-20 02:45:26.646` and `mode-stay` at `2026-06-20 02:45:36.697` for `Wolf Candidate`.
- The fallback-specific `Avalon Companions dialogue direct choice clicked` marker was not observed, so the click route is not proven to be the manual hit-test fallback; command execution is validated.
- In-game icon smoke, Goodbye, Esc input restoration, and `KeypadPeriod` debug-only behavior remain pending for separate observation.

## 0.1.27

- Added a dialogue-local direct pointer fallback for the vanilla-style Unity UI choices.
- The fallback checks left-click against enabled visible choice row rectangles and dispatches the same command delegates used by `Button.onClick`.
- Intended behavior change: dialogue click dispatch only. The same approved Follow, Stay, Defend, range, Come Close, Recall, Recover, Dismiss, and Goodbye commands are still used.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `9A40595955846CE21E85BDCD08646C724B7920236CCC4BE39E143ADE53A1F3DC`.
- Live and Release DLLs both report file version `0.1.27.0`.
- In-game smoke pending: open `Companion`, click at least Follow or Stay, confirm `Avalon Companions dialogue direct choice clicked` appears in the log, confirm the command runs and the dialogue closes, confirm Goodbye and Esc close, and confirm `KeypadPeriod` still opens only the debug panel.

## 0.1.25

- Fixed the vanilla-style Unity UI dialogue options being visible but not interactable.
- Split the input lock so Rewired axis/button reads are still neutralized for the IMGUI debug panel, but pass through while the Unity UI dialogue is visible for EventSystem button input.
- Kept `PlayerInput.ProcessLateUpdate` blocked while the dialogue is open, so movement/look remain frozen.
- Intended behavior change: dialogue input only. The same approved Follow, Stay, Defend, range, Come Close, Recall, Recover, Dismiss, and Goodbye commands are still used.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `06CBACBF1BDB33B83FA9C9B4AC6DB5CDA78F06E89206CE7AD0EA86409781BDF5`.
- Live and Release DLLs both report file version `0.1.25.0`.
- BepInEx load validation is pending because the game process was already running after deploy; the current session has not loaded 0.1.25 yet.
- In-game smoke pending after restart: open `Companion`, click at least Follow or Stay, confirm the dialogue closes after command execution, confirm Goodbye and Esc close, and confirm `KeypadPeriod` still opens only the debug panel.

## 0.1.24

- Added optional selected-roster icon rendering to the IMGUI debug panel through Tainted Interface's shared icon catalog.
- The bridge is reflection-only and keeps the panel text-only when Tainted Interface is absent, disabled, or missing the requested icon texture.
- Release build passed with 0 warnings and 0 errors.
- Built Release DLL SHA-256: `F876852E5B807B18E453E89E96AC6F4D9CADEF24F61F859CFB5F890FCE2A7FF0`.
- Built Release DLL file version: `0.1.24.0`.
- Live deploy passed on 2026-06-20. Live DLL SHA-256 matched Release output: `F876852E5B807B18E453E89E96AC6F4D9CADEF24F61F859CFB5F890FCE2A7FF0`.
- BepInEx load validation passed on 2026-06-20: `LogOutput.log` showed `Loading [Avalon Companions 0.1.24]` and `Avalon Companions 0.1.24 loaded`.
- Icon render smoke is still pending because the debug panel was not opened during the load check.
- In-game smoke pending: with Tainted Interface installed, open `Avalon Companions Debug`, select Qrko, wolf, bear, and a generic creature row, and confirm icons render from the shared catalog without changing button behavior.

## 0.1.23

- Changed the Unity UI companion dialogue host from a centered panel layout to a vanilla-style composition with right-side choices and a bottom dialogue text band.
- Removed dead centered-panel helper code from `PetCompanionController.DialogueUi.cs` so the `Companion` prompt path no longer constructs the old panel/header/context/footer shape.
- Kept `KeypadPeriod` `Avalon Companions Debug` as the separate IMGUI debug/control panel.
- Intended behavior change: layout only. The same approved Follow, Stay, Defend, range, Come Close, Recall, Recover, Dismiss, and Goodbye commands are still used.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `9BEC336B3CF68271A870D43336E0FE644F49B7E4686702053A9B667113971B35`.
- Live and Release DLLs both report file version `0.1.23.0`.
- BepInEx load validation passed: `LogOutput.log` showed `Loading [Avalon Companions 0.1.23]` and `Avalon Companions 0.1.23 loaded`.
- Native `Companion` prompt layout smoke is pending: open the prompt, confirm the new `layout=vanilla-style` log marker, confirm the centered panel is gone, confirm right-side choices and bottom text render, confirm buttons click, confirm close/Esc restores input, and confirm `KeypadPeriod` only opens debug.

## 0.1.22

- Replaced the native `Companion` prompt's old IMGUI dialogue window with a Unity UI `Canvas` dialogue host.
- Kept the `KeypadPeriod` `Avalon Companions Debug` panel as the only IMGUI command/control panel.
- Intended behavior change: UI host only. The same approved Follow, Stay, Defend, range, Come Close, Recall, Recover, Dismiss, and close commands are still used.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- First Debug build failed because the project needed a `UnityEngine.UIModule` reference and the new partial file needed the companion framework namespace.
- After correction, Debug build passed with 0 warnings and 0 errors.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `1CDC68A905DB2611A6FE9E6589D6B1CBD6ED69E85096F2AD7D6677DDFF2AE228`.
- Live and Release DLLs both report file version `0.1.22.0`.
- BepInEx load validation passed: `LogOutput.log` showed `Loading [Avalon Companions 0.1.22]` and `Avalon Companions 0.1.22 loaded`.
- Partial in-game debug-panel separation check was observed in the live log tail: `KeypadPeriod` produced `Panel opened` and `Panel closed` output for the debug panel path. A later `Select-String` pass did not return those lines, so this remains an observed live-tail note rather than fully archived log evidence.
- Native `Companion` prompt, Unity UI host marker, dialogue button clicks, close/Esc cursor restoration, and full in-game smoke remain pending.

## 0.1.21

- Added optional Tainted Interface shared styling/input-scope bridge for the custom dialogue interface and `Avalon Companions Debug` panel.
- Preserved the existing local IMGUI styling and FoA Mod Manager scope fallback when Tainted Interface is absent.
- Intended behavior change: UI shell only. Companion commands, native prompt routing, lifecycle behavior, recovery, continuity, and command logging remain unchanged from 0.1.20.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `394346C5832D584EAF563F997E085F7B3DD9EC8BFA6344BF96D61385C4CB10AB`.
- Live and Release DLLs both report file version `0.1.21.0`.
- In-game shared-style/dialogue/debug-panel smoke is pending.

## 0.1.20

- Added the custom dialogue command interface opened by the runtime native `Companion` prompt.
- Added `Companions.EnableCustomDialogueInterface`, default `true`.
- Kept `Debug.EnableDebugPanel` and `KeypadPeriod` as the separate debug-panel path.
- Intended behavior change: command choice is no longer a debug-panel bridge or native quick-command cycle; all approved commands are visible in one custom dialogue surface.
- Did not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `A5A2C9CA26BBBEC03F4AE78781FF1110FDD1AE714B2AE32B699123873C3F7701`.
- Live and Release DLLs both report file version `0.1.20.0`.
- BepInEx load validation and in-game dialogue/input smoke are pending.

## 0.1.19

- Reframed the current IMGUI command panel as the temporary Avalon Companions debug panel.
- Added `Debug.EnableDebugPanel`, default `true`.
- Renamed the native prompt bridge to `Companion Debug`.
- Intended behavior change: no new companion behavior; existing debug-panel command behavior remains available while native dialogue is researched.
- Did not add native dialogue/story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.
- Debug build passed with 0 warnings and 0 errors.
- Local Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `F8E86BDBE8208731E095A8AB20046555E950CF1D6E5EAD7486C2467F6E10C0D5`.
- Live and Release DLLs both report file version `0.1.19.0`.
- BepInEx load validation was not run after deploy; no FoA process was running and `LogOutput.log` had no 0.1.19 load line yet.
- In-game prompt/panel validation pending.

## 0.1.18

- Added optional read-only Avalon Core diagnostic bridge.
- Intended behavior change: none for companion runtime behavior.
- Did not add an Avalon Core dependency, descriptor registration, companion-profile registration, Core registry query, callbacks, or advanced companion behavior.
- First Debug build failed because `AvalonCoreDiagnosticBridge` had an over-escaped string literal in the trust-report log line.
- Corrected the string literal; Debug build passed with 0 warnings and 0 errors.
- Local Release build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `54A72D76226714A400EB054F9336150AA6E7EB9BB650DDEAF9F0907197539AC6`.
- Live and Release DLLs both report file version `0.1.18.0`.
- `git diff --check -- mods/avalon-companions` passed.
- BepInEx log validation passed on 2026-06-19. `LogOutput.log` showed `Loading [Avalon Companions 0.1.18]`, `Loading [Avalon Core 0.7.7]`, and `Avalon Core diagnostic bridge read trust report. CoreVersion=0.7.7; Status=skipped; Reason=scan-disabled; Candidates=0; Valid=0; Blockers=0; Warnings=0; PackRows=0; IssueRows=0; WouldMutateRuntime=False; action=read-only.`

## 0.1.17

- Recorded Core handoff design only.
- Intended behavior change: none.
- Did not add an Avalon Core dependency, descriptor registration, companion-profile registration, callbacks, or advanced companion behavior.
- Debug build passed with 0 warnings and 0 errors.
- Local Release build passed with 0 warnings and 0 errors.
- `git diff --check -- mods/avalon-companions` passed.
- In-game smoke was not run; this slice changes only docs and version metadata.

## 0.1.16

- Started framework polish by extracting internal companion contracts.
- Intended behavior change: none.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `81BAF1227C9CE81971EC7382DF00CEFAC3ACEDF5C625C67D3DE1AE4B9A499C5F`.
- In-game smoke after extraction is pending.

## 0.1.15

- Added responsive native behavior polish inside the existing one-session native ally path.
- Explicit companion commands now queue existing follow catch-up and native defend checks for immediate evaluation on the next update.
- Per-companion defend prompt cooldowns are not cleared by the responsive refresh.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- The live BepInEx plugin DLL was stale at file version `0.1.0.0` before deploy; after deploy, live and Release DLLs both report file version `0.1.15.0`.
- Live DLL hash matched Release output: `FECCB2981849A74CF2FC58A83A722C84FAC9A1071F6A96A8A5FC4A9FEBA78AE6`.
- In-game responsive/native polish smoke is pending.

## 0.1.13

- Added managed recovery inside the existing one-session native ally path.
- Added `Recover` to the panel and fallback native quick-command cycle.
- Recovery runs a forced lifecycle precheck, removes dead/invalid managed actors when the lifecycle guard is enabled, and recalls live managed actors beyond the Far catch-up threshold.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `786830935E0FFD76A286A4345D5803C771A3727D450DAC2417F1EBF0B0717B11`.
- Live and Release DLLs both report file version `0.1.13.0`.
- User-reported in-game recovery smoke completed on 2026-06-16 after the 0.1.13 live DLL deploy.
- CSV/log evidence from that smoke pass has not been inspected in this repo pass.

## 0.1.12

- Added lifecycle validation polish inside the existing panel.
- Panel now shows the latest lifecycle pass summary.
- Added `Lifecycle Check`, which runs the existing lifecycle safety pass with `forceDump=true` and writes a command-log row.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `88B560DB5CBF6DC42F0EA380FFA6567A2151D40D321DF42822191A3E5E1FD73F`.
- Live and Release DLLs both report file version `0.1.12.0`.
- User-reported in-game lifecycle checkpoint smoke completed on 2026-06-15 after the 0.1.12 live DLL deploy.
- CSV/log evidence from that smoke pass has not been inspected in this repo pass.

## 0.1.11

- Added command polish inside the existing panel only.
- Panel status now reports active companion distance, mode state, native ally marker, native prompt state, and not-saved state.
- Follow, Stay, and Defend feedback now describes catch-up, waiting-for-attacker, and native ally behavior more clearly.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `C1E5FBE0E4F3442EF57C2EE6071B02067BEA465349359A778C068B0A44F0BC6F`.
- Live and Release DLLs both report file version `0.1.11.0`.
- In-game retest pending: open the panel with no active companion, then with one managed candidate active, and confirm no Attack button or target selector appears.

## 0.1.10

- Added advanced native ally behavior inside the approved one-session `NpcHeroPetAlly` path.
- Native defend now counts live hero attackers, logs `defend-armed` and `defend-clear`, and throttles repeated automatic `EnterCombat()` prompts per managed companion.
- Automatic catch-up command log rows now include range, distance, threshold, and slot.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `5DC8B1F5B5F5A60E1A8831883D31E3E1E4375CE30FECA6062B7AFB0A0B75BB90`.
- Live and Release DLLs both report file version `0.1.10.0`.
- In-game retest pending: summon an offensive or undead candidate, use Defend mode while attacked, confirm prompts are throttled and no Attack/target selector appears.

## 0.1.9

- Added `Companions.EnableNativeCommandMenu`, default `true`.
- Managed one-session companions now use one runtime-only native `Companion` prompt by default.
- Activating `Companion` opens the Avalon command menu instead of cycling Follow/Stay/Defend/Come Close/Recall/Dismiss as world prompts.
- Rotating quick-command actions remain fallback-only when `Companions.EnableNativeCommandMenu=false`.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `87D09569851700780E9F8E737837DF5469393778AE258B774B3998A7947AF039`.
- Live and Release DLLs both report file version `0.1.9.0`.
- In-game retest pending: interact with a managed companion and confirm one `Companion` prompt opens the command menu.

## 0.1.8

- User smoke test of 0.1.7 showed the native quick-command cursor can stick on the final `Dismiss` prompt and not reset cleanly.
- Reset the native quick-command cursor before Dismiss removes the companion and after successful summon/swap.
- Recorded the next proper-menu direction: one native `Companion` prompt opening a runtime-only command menu instead of cycling world prompts.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `86A52E26BA1EE94B27902BAA8235AFFC6B978868B12F2E71479173D49476616C`.
- Live and Release DLLs both report file version `0.1.8.0`.
- In-game retest pending: dismiss a managed companion, summon/swap again, and confirm the next prompt does not inherit stale `Dismiss`.

## 0.1.7

- User smoke test of 0.1.6 showed the panel no longer opened, but the native prompt only cycled through Follow and Stay because FoA still starts the first/default available quick action.
- Added a native quick-command cursor so only one quick action is available at a time and repeated native interaction can rotate through Follow, Stay, Defend, Come Close, Recall, and Dismiss.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `8453D1DA88CC4C173033DC610715144C1F449B8F5766C9846AA150A629471ECB`.
- Live and Release DLLs both report file version `0.1.7.0`.
- In-game retest pending: repeatedly interact with a managed companion and confirm the prompt advances beyond Follow/Stay.

## 0.1.6

- User smoke test of 0.1.5 showed native interaction still opened the Avalon Companions panel because the older `Companion` panel bridge remained attached beside the quick-command actions.
- Changed `Companions.EnableNativeQuickCommands=true` to remove/disable the panel bridge and expose only the runtime quick-command actions.
- Kept the panel bridge fallback when `Companions.EnableNativeQuickCommands=false`.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `516E41A71391159550EDCF402A8DD55398F9DA4FBFD25C83E1B4ED1AE1AB0874`.
- Live and Release DLLs both report file version `0.1.6.0`.
- In-game retest pending: interact with a managed one-session companion and confirm it no longer opens the panel in quick-command mode.

## 0.1.5

- Added native quick-command action elements for managed one-session companions: Follow, Stay, Defend, Come Close, Recall, and Dismiss.
- Added `Companions.EnableNativeQuickCommands`, default `true`.
- Existing native `Companion` action remains the panel bridge.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `13D3DED6B89374A17151F0F9DD47D127912506C3CBAE2E0AF49755FA43ABEFE4`.
- Live and Release DLLs both report file version `0.1.5.0`.
- In-game validation pending: confirm the native prompt/action surface exposes the quick commands on managed companions only, each command writes CSV rows, and dismiss removes all native command actions.

## 0.1.4

- User-reported bug: the close command was perceived as "heal" and could kill companions.
- Applied placement hotfix: visible button text changed to `Come Close`, one-session creature candidates do not use below-normal close placement offsets, and recall targets use native position verification with snap-to-ground enabled.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `9542401A4BAD13D6A132435619B65DCC4702A9BDD3089777A7CFF068C905DD06`.
- Live and Release DLLs both report file version `0.1.4.0`.
- In-game retest pending: summon animal/offensive/undead candidates, press `Come Close`, confirm they are not killed/discarded, and confirm the command log still records `heel` with targeting/persistence flags false.

## 0.1.3

- Added advanced managed-command behavior: Heel, Close/Normal/Far follow range profiles, and `companion-command-log.csv`.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `94AB145208B204BA0F181DF0A6F666A52B2BE90F636B0DAFC48430EF9B009F1A`.
- Live and Release DLLs both report file version `0.1.3.0`.
- Live load validation passed on 2026-06-15: `LogOutput.log` showed `Avalon Companions 0.1.3 loaded` with `ResearchModeOnly=false`, roster enabled, lifecycle dump enabled, command log enabled, and `FollowRangeProfile=1`.
- Live command-log validation partially passed: `companion-command-log.csv` recorded blocked dismiss, select-next, and summon/swap rows. The summon/swap row for `Armored Drowner Candidate` had `touchesTargeting=false` and `touchesPersistence=false`.
- Live lifecycle validation partially passed: `companion-lifecycle.csv` recorded one active tracked `Spec_EnemyZombie_T1_DrownerFullArmor` roster actor with `markedNotSaved=true`, `hasNpcHeroPetAlly=true`, `isTrackedCreature=true`, `hasCommandAction=true`, `duplicatesDiscarded=0`, and `untrackedDiscarded=0`.
- Native interaction bridge validation partially passed: BepInEx log showed `Companion command opened: Armored Drowner Candidate` after the native command action was attached.
- Removed stale zero-byte duplicate DLL at `<local-path>` after BepInEx reported it was skipping older `Avalon Companions 0.1.0`.
- In-game advanced-command validation is still incomplete.
- Required smoke checks still pending: confirm Heel sets Close range plus Follow mode, range buttons change recall/catch-up distance without duplicates, Defend uses only the native hero-attacker path, and no Attack/custom target command is exposed.

## 0.1.2

- Added transition/save lifecycle guard config and `companion-lifecycle.csv` diagnostics.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `05F17F12D61464839B0342A521796585487643A6949E17B733C220BDD56AC8D6`.
- Live and Release DLLs both report file version `0.1.2.0`.
- In-game transition/rest/quit-reload validation is pending.

## 0.1.1

- Creature roster expansion: added all missing rows from FOA-Diagnostic Tool 0.4.3 `creature_templates.csv` with `reviewStatus=ReviewQueueNonUniqueSpawnerBacked`, `npcUnique=false`, `riskFlags=none`, and loaded spawner evidence. Existing wolf, bear, deer, pig, cow, and bullrat rows stayed on the same path.
- Added one-session roster entries for `Spec_AnimalBear_Interactions`, T1 corpse eater/redcap/flamegobbler/grindylow/wyrdspirit, Sharg HoS, ogre, T1 zombie/drowner rows, skeleton 1H/2H, floatling, and tadpole hatchery.
- Debug build passed with 0 warnings and 0 errors.
- Release deploy build passed with 0 warnings and 0 errors.
- Live DLL hash matched Release output: `A76BC05B776DD767B830F8D0EC600678325863CEAE6C645B91D096E47BE3A8B6`.
- Live and Release DLLs both report file version `0.1.1.0`.
- User-reported in-game smoke validation on 2026-06-15: expanded one-session roster actions worked perfectly after the 0.1.1 live DLL deploy. Treat this as functional smoke evidence for summon/actions/dismiss/change behavior, not as transition, rest, quit/reload, persistence, or broad conflict validation.

## 0.1.0

- Build validation: failed once with missing `Awaken.PackageUtilities.dll` reference, then passed on 2026-06-14 with `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="<local-path>"`. The same build command passed again after adding the GUID-backed template CSV dump, the Qrko pet prototype, and the pet companion roster. The wolf/bear one-session candidate change passed Debug and Release deploy builds on 2026-06-15 with 0 warnings and 0 errors.
- Research validation: local `TG.Main.dll` inspection completed for pet, summon, ally, story-step, template, and common-reference candidates.
- Template dump validation: passed in game on 2026-06-14. Four `Spec_Pet_Qrko*` `LocationTemplate` rows were written to CSV.
- Pet/creature shortlist dump: implemented disabled-by-default CSV writer for scene-spawner-backed pet/creature `LocationTemplate` candidates. Build passed. In-game dump wrote candidates and a shortlist, but `Spec_AnimalWolf` and `Spec_AnimalBear` stayed at `sceneSpawnerRefCount=0`.
- FOA-Diagnostic Tool crosscheck: route-context FOA-Diagnostic Tool dumps such as `20260614-213701` and `20260614-221744` registered `Spec_AnimalWolf` with 26 spawner refs and `Spec_AnimalBear` with 6 spawner refs. Added diagnostic-only comparison output to identify rows seen by FOA-Diagnostic Tool but missed by Avalon's own once-per-session dump. The first in-game crosscheck selected a newer startup dump with only 20 spawner refs, so the selector now skips low-context dumps below `Diagnostics.DiagnosticToolMinimumSpawnerRefs=100`.
- FOA-Diagnostic Tool crosscheck rerun: passed after selector fix. BepInEx log reported `selected dump with 556 spawner refs` from `20260614-221744`; `pet-creature-diagnostic-tool-review.csv` contained 38 review candidates. `Spec_AnimalWolf` had 26 external spawner refs and `Spec_AnimalBear` had 6. Both remained `safeSpawnCandidate=false` and `rosterApproved=false`.
- Game load validation: passed with diagnostics enabled on 2026-06-14.
- Feature validation: Qrko pet prototype spawn path passed in game log on 2026-06-14 after pressing the then-current `F8` shortcut. Default shortcut was later moved to `KeypadPlus` because `F8` conflicts with other local mods. Pet companion roster controls have not yet been smoke-tested in game.
- Roster smoke issue: summon worked, but swap/follow/dismiss/recall could stop working after the active Qrko no longer passed the managed marker check. Fixed by controlling active pets whose template GUID is one of the four validated Qrko roster GUIDs.
- Roster UX issue: latest log showed commands were detected and multiple pet GUIDs were selected/spawned, but next/previous only changed selection and did not visibly swap the active pet. Fixed by making next/previous immediately swap when a roster pet is active and by adding on-screen command status.
- Command panel build: added a styled IMGUI panel on `KeypadPeriod`; build passed, but in-game visual validation is not run yet.
- Command panel screenshot issue: user screenshot `Screenshot (3570).png` showed the fixed-size panel was functionally visible but too small and pinned in the corner on the active high-resolution display. Reworked the panel using the FoA Mod Manager layout pattern: responsive window size, screen clamping, header dragging, custom focused/pressed states, and mouse-event consumption inside the window. Build validation passed; in-game screenshot validation is still required.
- Creature-candidate roster entries: added wolf and bear as selectable one-session candidates, then added deer, pig, cow, and bullrat from the animal roster expansion review. Build validation is required; in-game validation should confirm the panel shows all one-session candidates, summon/swap spawns only the selected candidate, marks it not saved, tracks it for the current session, and limits behavior to recall/catch-up/dismiss plus the native defend entry path.
- Creature placement issue: user screenshots `Screenshot (3565).png` and `Screenshot (3566).png` showed wolf and bear actions working, but the candidates spawned/caught up too close to the player and blocked the camera. The placement logic now keeps Qrko on the small side offset while wolf and bear use farther rear-quarter offsets and a wider catch-up threshold.
- Creature ally marker: local decompilation on 2026-06-15 showed `NpcAllyPetVariant.OnSpawned()` applies the hero summon faction and adds `NpcHeroPetAlly`. Wolf and bear one-session candidates now require `NpcElement`, apply the same native marker path, and are discarded immediately if ally setup fails. Build validation passed; in-game hostility/friendly behavior validation is still required.
- Native defend assist: local decompilation on 2026-06-15 showed `NpcAlly` reads `Ally.PossibleAttackers` and `NpcHeroSummon.EnterCombat()` triggers the native target path. Wolf and bear one-session candidates now check `Hero.Current.PossibleAttackers` on a short tick and call `NpcHeroPetAlly.EnterCombat()` when a live attacker exists. Build validation passed; in-game attack-response validation is still required.
- Companion command modes: added explicit panel Follow, Stay, and Defend mode buttons plus current mode text. Follow and Defend keep pet following/candidate catch-up active; Stay turns following/catch-up off. Recall respects the selected mode. Defend mode uses the same native `NpcHeroPetAlly.EnterCombat()` path and still requires a live hero attacker; `Companions.EnableNativeDefendAssist` remains the automatic fallback outside explicit Defend mode. Build validation passed; in-game command-mode validation is still required.
- Native companion interaction action: added `AvalonCompanionCommandAction` as a runtime-only `AbstractLocationAction` attached only to tracked one-session creature candidates with `NpcHeroPetAlly`. It opens the existing command panel from the native world interaction prompt and does not touch story graphs, story bookmarks, templates, or vanilla serialized interaction lists. Debug build and Release deploy build passed with 0 warnings and 0 errors; live DLL hash matched Release output.
- Animal roster expansion: added deer, pig, cow, and bullrat one-session candidates from the 2026-06-15 live diagnostic CSV evidence. Deer, pig, and cow are passive animal candidates; bullrat is the only newly added offensive creature candidate. Debug and Release deploy builds passed with 0 warnings and 0 errors; live DLL hash matched Release output: `541054D4D911BF46EA4D1121D04F9B2F814E28D092127A2B5D001081EC292CC4`. In-game smoke validation is still required.
- Panel input lock: mirrored FoA Mod Manager cursor/input fixes for the companion panel. Build validation passed after adding cursor capture/restore, Unity input-module disable/restore, axis resets, and FoA/Rewired input-freeze patches. Follow-up cursor report showed the panel could still draw without a usable cursor, so Avalon now re-enforces cursor unlock and input-module blocking from `DrawGui()` immediately before the IMGUI window is drawn, matching the Mod Manager `OnGUI` lifecycle. User smoke validation on 2026-06-15 reported the panel/cursor path works.
- Diagnostics validation: passed in game on 2026-06-14.
- Save/load validation: not run for pet companion behavior.
- Follow/transition validation: not run or not documented yet.
