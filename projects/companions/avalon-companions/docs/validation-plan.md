# Validation Plan

## Required level

- Level: build for 0.1.42 scaffold, diagnostics, pet companion roster, lifecycle guard, advanced managed-command behavior, close-recall placement hotfix, one native `Companion` prompt opening a plugin-owned vanilla-style Unity UI custom dialogue host, separate IMGUI debug panel, optional Tainted Interface/FoA Mod Manager input scope, optional Tainted Interface shared-icon consumer proof, per-companion icon mapping, optional passive gameplay HUD companion icon overlay and HUD primitive handoff, throttled native ally defend behavior, debug-panel status polish, lifecycle checkpoint diagnostics, managed recovery, metadata/status-only continuity, responsive native refresh scheduling, framework contract extraction, Core handoff design, optional read-only Avalon Core diagnostic bridge, default-off read-only AI boundary diagnostic plus manual debug-panel snapshot aid, AI boundary evidence review and first safe behavior design, default-off read-only AI profile/intent audit classification plus manual debug-panel profile snapshot aid, gated profile-driven native assist with profile-owned native-safe ticks, debug-panel roadmap polish, custom dialogue interface polish, dialogue input correction, dialogue direct-click fallback, command-surface polish, native dialogue supported-surface decision, and lifecycle validation gate route; targeted in-game lifecycle, command, HUD, and diagnostic smoke checks before any release claim beyond alpha.
- Reason: current behavior is plugin load, disabled-by-default diagnostics, disabled-by-default pet/creature candidate dumps, an opt-in pet companion roster, transition/save cleanup diagnostics, Come Close/Heel, follow range profiles, command audit logging, a native custom-dialogue prompt, native defend prompting that stays inside the game's `NpcHeroPetAlly` path, debug-panel-only diagnostics, optional shared icon rendering for the selected roster row with per-companion mapping, manual lifecycle checkpoint diagnostics, managed recovery for dead/invalid/stuck active companions, remembered metadata continuity that still requires explicit `Summon / Swap`, immediate scheduling of existing follow/defend checks after explicit commands, internal framework contract extraction, Core handoff design, read-only Core trust-report logging without behavior change, explicit panel-as-debug labeling/gating, a supported plugin-owned vanilla-style Unity UI command-choice dialogue host with dialogue-specific Rewired/EventSystem input pass-through, passive HUD badge primitive handoff, read-only native AI/movement/target-state evidence logging when explicitly enabled including one manual debug-panel snapshot button for smoke testing, and read-only companion AI profile/intent audit classification when explicitly enabled including one manual debug-panel profile snapshot button for smoke testing. Humanoid companion behavior, true dialogue graphs, runtime-authored native custom dialogue, custom targeting, custom AI, taming, training, loyalty, attack commands, active squads, healing, resurrection, respawn, actor restoration, re-adoption, persistence, and executable Avalon Core runtime integration require separate research.
- Highest completed level: user-reported expanded 0.1.1 roster smoke pass plus partial 0.1.3 live log/CSV validation on 2026-06-15; full transition/save/load validation is still not run. The 2026-06-16 session-continuity/persistence gate is research-only and does not approve actor restoration or persistence.

## 0.1.42 lifecycle validation gate - 2026-06-21

- Scope: define the throwaway-save lifecycle validation route for transition, rest, quit/reload, return, duplicate cleanup, orphan cleanup, and not-saved behavior without changing lifecycle behavior.
- Research gate: `docs/research/lifecycle-validation-gate-2026-06-21.md`.
- Required config: `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableLifecycleSafetyGuard=true`, `Diagnostics.WriteCompanionLifecycleDump=true`, and `Diagnostics.WriteCompanionCommandLog=true`.
- Required smoke route: summon one reviewed managed one-session creature candidate, press `Lifecycle Check`, transition or fast travel, press `Lifecycle Check`, rest, press `Lifecycle Check`, save, quit, reload, return, then inspect `companion-lifecycle.csv` and `companion-command-log.csv`.
- Pass criteria: managed companion rows keep `markedNotSaved=true`; active roster count stays zero or one; duplicate cleanup leaves no more than one active managed roster actor; no unmanaged native `Companion` prompt remains; quit/reload does not auto-restore, auto-respawn, persist, or re-adopt the one-session actor; command rows keep `touchesPersistence=false` and `touchesTargeting=false`.
- Expected exclusions: no lifecycle behavior change, actor persistence, reload restoration, auto-respawn, re-adoption, `MarkedNotSaved` ownership change, saved native command actions, active squads, healing, resurrection, custom AI/pathing, target selection, attack UI, taming, training, loyalty, or Core-executed behavior.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.42.0`.
- Built Release DLL SHA-256: `6EFADED79F911D2B98C8C5163D9F30F93F44C9775F23A6D38EB255D62C3E4096`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.42.0`; live SHA-256 matched `6EFADED79F911D2B98C8C5163D9F30F93F44C9775F23A6D38EB255D62C3E4096`.
- In-game lifecycle smoke: Pending.
- CSV review after smoke: Pending.

## 0.1.41 native dialogue decision gate - 2026-06-21

- Scope: decide whether to prove a safe real `VDialogue` / `StoryBookmark` route now or formally keep the plugin-owned Unity UI as the supported companion dialogue surface.
- Research gate: `docs/research/native-dialogue-decision-gate-2026-06-21.md`.
- Decision record: `docs/decisions/0007-native-dialogue-supported-surface.md`.
- Decision: plugin-owned Unity UI remains the supported companion dialogue surface. The native `Companion` prompt remains a runtime-only `AbstractLocationAction` entry point, not a true native story dialogue.
- Expected exclusions: no fake `StoryBookmark`, runtime story graph, `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, template edit, save-owned interaction list edit, custom AI/pathing, target selection, attack UI, recruitment, quest state, affinity, taming, training, loyalty, persistence, restoration, respawn, or Core-executed behavior.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Failed once when run in parallel with Debug due to a NuGet restore file-exists race, then passed on standalone rerun with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.41.0`.
- Built Release DLL SHA-256: `F6CD7CE29629D2FC16CFFC2F6453D6B9015A581CB5ADAC0C4A7104A0F3CFA5D1`.
- In-game smoke: Not required for this design-only slice because no runtime behavior changes beyond version metadata are intended.

## 0.1.40 profile-driven native assist smoke fix - 2026-06-21

- Scope: fix the 0.1.39 smoke race where legacy follow/defend ticks could claim the first native-safe action before the profile loop wrote `ai-profile-native-*` command rows.
- Research gate: `docs/research/profile-driven-native-assist-smoke-fix-2026-06-21.md`.
- 0.1.39 live smoke finding: BepInEx loaded `Avalon Companions 0.1.39` with `EnableProfileDrivenNativeAssist=True`; command-log rows after baseline 315 included repeated `defend-prompt` rows but no `ai-profile-native-defend`; `companion-ai-profile.csv` classified the active `Sharg Candidate` as `NativeCombatObserved` at 2026-06-21 14:30:32 after a regular `defend-prompt` had already set the native defend cooldown at 2026-06-21 14:30:30.
- Behavior: while `Companions.EnableProfileDrivenNativeAssist=true`, the legacy creature follow and defend ticks return early so the profile loop owns the existing catch-up recall and native `NpcHeroPetAlly.EnterCombat()` prompting paths.
- Expected exclusions: no target selector, direct target selection, target override, native target recalculation, direct movement-state override, persistence, progression state, Avalon Core behavior, taming, training, or loyalty.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.40.0`.
- Built Release DLL SHA-256: `112E5883EF968D09EC397FC8895172B0E56DE3ECC7A5BEC2ABF708908CC8A5E6`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.40.0`; live SHA-256 matched `112E5883EF968D09EC397FC8895172B0E56DE3ECC7A5BEC2ABF708908CC8A5E6`.
- BepInEx load validation: Passed. `LogOutput.log` showed `Loading [Avalon Companions 0.1.40]` and `Avalon Companions 0.1.40 loaded` with `EnableProfileDrivenNativeAssist=True; ProfileDrivenNativeAssistSeconds=1`.
- Command-log rerun baseline: row count `375`; rows after this baseline are for the 0.1.40 in-game smoke rerun.
- In-game smoke: Passed. Initial live-attacker rows after baseline `375` included four `ai-profile-native-defend` rows at 2026-06-21 17:02:27 through 17:02:36 with `affectedCount=1`, `touchesTargeting=false`, and `touchesPersistence=false`.
- Dedicated catch-up rerun: Passed. Rows after follow-up baseline `399` included six `ai-profile-native-catch-up` rows at 2026-06-21 17:18:49 through 17:18:54 in Follow/Close mode with `affectedCount=1`, `touchesTargeting=false`, and `touchesPersistence=false`.
- Catch-up profile evidence: Passed. `companion-ai-profile.csv` classified `Sharg Candidate` as `FollowCatchUpCandidate` with `distanceToHero=17.083`, `14.238`, and `14.081` against `catchUpThreshold=14`; all profile mutation/Core flags stayed false.
- Duplicate-row check: Passed. Rows after baseline `399` contained zero `auto-catch-up`, `defend-prompt`, or `auto-defend-prompt` rows and zero unsafe command flags. The same slice included six `ai-profile-native-catch-up` rows and fourteen `ai-profile-native-defend` rows.

## 0.1.39 profile-driven native assist hardening - 2026-06-21

- Scope: de-duplicate the gated profile-driven native assist against the older follow/defend ticks.
- Research gate: `docs/research/profile-driven-native-assist-hardening-2026-06-21.md`.
- Behavior: after `ai-profile-native-catch-up`, defer the regular creature follow tick by the existing follow interval. After `ai-profile-native-defend`, defer the regular creature defend tick by the existing defend interval.
- Expected exclusions: no target selector, direct target selection, target override, native target recalculation, direct movement-state override, persistence, progression state, Avalon Core behavior, taming, training, or loyalty.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.39.0`.
- Built Release DLL SHA-256: `BCA9DBD9D11CEB43CFEC895A95BD4D8AD7F45FE2B7C29A595B92302F35EE0A0C`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.39.0`; live SHA-256 matched `BCA9DBD9D11CEB43CFEC895A95BD4D8AD7F45FE2B7C29A595B92302F35EE0A0C`.
- BepInEx load validation: Pending.
- In-game smoke: Pending. Enable `Companions.EnableProfileDrivenNativeAssist=true`, force a follow catch-up distance breach, then test live hero attackers. Confirm profile action rows do not immediately duplicate with sibling legacy tick rows.

## 0.1.38 profile-driven native assist - 2026-06-21

- Scope: add the first gated `CompanionAiIntent` behavior bridge.
- Research gate: `docs/research/profile-driven-native-assist-2026-06-21.md`.
- Config: `Companions.EnableProfileDrivenNativeAssist=false`; `Companions.ProfileDrivenNativeAssistSeconds=1`.
- Allowed behavior: `FollowCatchUpCandidate` may use existing recall/catch-up placement; `NativeCombatObserved` may use existing `NpcHeroPetAlly.EnterCombat()` only when live hero attackers exist and Defend/native-assist gates allow it.
- Expected command rows: `ai-profile-native-catch-up` and `ai-profile-native-defend`.
- Expected exclusions: no target selector, direct target selection, target override, native target recalculation, direct movement-state override, persistence, progression state, Avalon Core behavior, taming, training, or loyalty.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.38.0`.
- Built Release DLL SHA-256: `607D31232E4E756F383ABA85CEC341D9DC55BFBC6473BA6B15EF31B160AF7992`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.38.0`; live SHA-256 matched `607D31232E4E756F383ABA85CEC341D9DC55BFBC6473BA6B15EF31B160AF7992`.
- BepInEx load validation: Pending.
- In-game smoke: Pending. Enable `Companions.EnableProfileDrivenNativeAssist=true`, force a follow catch-up distance breach, then test live hero attackers. Confirm command rows keep `touchesTargeting=false` and `touchesPersistence=false`.

## 0.1.37 AI profile manual smoke aid - 2026-06-21

- Scope: add a visible debug-panel `AI Profile Check` button for immediate profile-audit smoke rows.
- Research gate: `docs/research/ai-profile-check-smoke-aid-2026-06-21.md`.
- Code behavior: when `Diagnostics.WriteCompanionAiProfileAudit=false`, report the blocked config state and write a blocked `ai-profile-check` command-log row. When enabled, write one immediate `panel-ai-profile-check` snapshot through the existing `companion-ai-profile.csv` writer.
- No-active behavior: with no active managed companion, the enabled snapshot writes one valid `NoActiveCompanion` row.
- Expected exclusions: no command dispatch, native follow/defend refresh queue, movement change, target change, native target recalculation, `TargetOverrideElement.GetTarget`, progression state, persistence, Avalon Core behavior, custom AI execution, taming, training, or loyalty.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.37.0`.
- Built Release DLL SHA-256: `91B0C7E0CBD1462E8B168FFF52379CE2987C30209256309AD5E9384C498DFD16`.
- Live deploy: Not run because `Fall of Avalon` was still running and the live DLL may be locked.
- In-game disabled-config smoke: Pending. Required check: open `Avalon Companions Debug`, click `AI Profile Check` while `Diagnostics.WriteCompanionAiProfileAudit=false`, confirm blocked status, and confirm a blocked `ai-profile-check` row in `companion-command-log.csv`.
- In-game enabled-config smoke: Pending. Required check: set `Diagnostics.WriteCompanionAiProfileAudit=true`, click `AI Profile Check` with no active managed companion and with an active managed companion, and confirm `panel-ai-profile-check` rows in `companion-ai-profile.csv`.
- Coverage-matrix follow-up: use manual rows plus periodic rows to capture `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, interiors, stealth, transition, rest, quit/reload, and return.

## 0.1.36 AI profile audit smoke review - 2026-06-21

- Scope: validate and review the first live `companion-ai-profile.csv` evidence produced by the 0.1.35 profile audit classifier.
- Deployed DLL reviewed: file version `0.1.35.0`, SHA-256 `A67329CC836E85776A95750C2B95C4AE68A506E61714DBD82B2A4461ED176EA0`.
- Live config: `Diagnostics.WriteCompanionAiProfileAudit=true`, `Diagnostics.CompanionAiProfileAuditSeconds=2`, `Diagnostics.WriteCompanionAiBoundaryDiagnostics=true`, and `Diagnostics.CompanionAiBoundaryDiagnosticSeconds=2`.
- BepInEx load validation: Passed. `LogOutput.log` showed `Loading [Avalon Companions 0.1.35]` and `Avalon Companions 0.1.35 loaded` with `WriteCompanionAiProfileAudit=True; CompanionAiProfileAuditSeconds=2`.
- Profile audit CSV: Passed. `companion-ai-profile.csv` was created under the plugin config folder.
- Rows reviewed: 27 rows from `CampaignMap_HOS`, 2026-06-21 02:01:53.091 through 2026-06-21 02:02:49.611.
- Active rows: 14 active `Sharg Candidate` rows.
- Intent coverage: `NoActiveCompanion=13`, `MovementBlocked=5`, `IdleNativePatrol=9`.
- Safety result: `UNSAFE_FLAGS=0`; all reviewed rows kept `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Command audit crosscheck: Passed. Latest relevant smoke rows included `summon/swap`, `recover`, `summon-existing-recall`, `range-close`, `heel`, and one `ai-boundary-check`; relevant rows kept `touchesTargeting=false` and `touchesPersistence=false`.
- Build result: Not required. This slice changed docs/review evidence only and reviewed the already deployed 0.1.35 DLL.
- Missing evidence: `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, interiors, stealth, transition, rest, quit/reload, and return.
- Expected exclusions: no command dispatch from profile audit, movement change, target change, native target recalculation, `TargetOverrideElement.GetTarget`, progression state, persistence, Avalon Core behavior, custom AI execution, taming, training, or loyalty.

## 0.1.35 AI profile/intent audit classifier - 2026-06-21

- Scope: add runtime-only `CompanionAiProfile` and `CompanionAiIntent` contracts plus a default-off read-only profile audit that classifies active managed companion state without executing behavior.
- Implemented intents: `NoActiveCompanion`, `IdleNativePatrol`, `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, `MovementBlocked`, `NoTargets`, and `EvidenceInsufficient`.
- Config: `Diagnostics.WriteCompanionAiProfileAudit=false` and `Diagnostics.CompanionAiProfileAuditSeconds=2`.
- Output: `companion-ai-profile.csv` under this plugin's BepInEx config folder.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Debug -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.35.0`.
- Built Release DLL SHA-256: `A67329CC836E85776A95750C2B95C4AE68A506E61714DBD82B2A4461ED176EA0`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.35.0`; live SHA-256 matched `A67329CC836E85776A95750C2B95C4AE68A506E61714DBD82B2A4461ED176EA0`.
- BepInEx load validation: Passed during 0.1.36 smoke review.
- In-game profile audit smoke: First pass completed during 0.1.36 smoke review. Required follow-up coverage remains for Stay, follow catch-up, Defend waiting, native combat, interiors, stealth, transition, rest, quit/reload, and return.
- Expected exclusions: no command dispatch, movement change, target change, native target recalculation, `TargetOverrideElement.GetTarget`, progression state, persistence, Avalon Core behavior, custom AI execution, taming, training, or loyalty.

## 0.1.34 AI boundary evidence review and first safe behavior design - 2026-06-21

- Scope: review the first live `companion-ai-boundary.csv` evidence set and define only a future non-mutating `CompanionAiProfile` / `CompanionAiIntent` classifier design.
- Evidence reviewed: 276 AI boundary rows from `CampaignMap_HOS`, with 273 periodic rows, 3 manual panel rows, 43 `npcInCombat=true` rows, 64 rows with one live hero attacker, and `UNSAFE_FLAGS=0`.
- Command audit crosscheck: 288 command rows at review time, including 13 `ai-boundary-check`, 36 `summon/swap`, and 27 `dismiss` rows.
- Design result: a future classifier may observe native state and write audit evidence only. It must not execute behavior, call target recalculation, use `TargetOverrideElement`, override movement, add attack commands, persist actors, store progression state, or execute behavior through Avalon Core.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.34.0`.
- Built Release DLL SHA-256: `453D0088285DFE64F6F55AD9EC883FA06E7E31FD74D8057CB708A841C5B7101D`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.34.0`; live SHA-256 matched `453D0088285DFE64F6F55AD9EC883FA06E7E31FD74D8057CB708A841C5B7101D`.
- BepInEx load validation: Pending.
- In-game classifier smoke: Not applicable; no classifier behavior was implemented in this slice.
- Expected exclusions: no custom AI execution, target override, movement override, target selector, attack command, taming, training, loyalty, persistence, restoration, respawn, active squad, or Core-executed behavior.

## Build validation

- Command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: failed once with missing `Awaken.PackageUtilities.dll` reference, then passed on 2026-06-14 with 0 warnings and 0 errors after adding the local game assembly reference. Re-runs after the GUID-backed template dump, Qrko prototype, pet companion roster, wolf/bear one-session candidate changes, native companion interaction action, animal roster expansion, 0.1.1 offensive/undead roster expansion, 0.1.2 lifecycle guard, and 0.1.3 advanced command behavior passed with 0 warnings and 0 errors. The 2026-06-15 0.1.1 Release deploy build produced a live DLL hash matching the Release output: `A76BC05B776DD767B830F8D0EC600678325863CEAE6C645B91D096E47BE3A8B6`. The 2026-06-15 0.1.2 Release deploy build produced a live DLL hash matching the Release output: `05F17F12D61464839B0342A521796585487643A6949E17B733C220BDD56AC8D6`. The 2026-06-15 0.1.3 Release deploy build produced a live DLL hash matching the Release output: `94AB145208B204BA0F181DF0A6F666A52B2BE90F636B0DAFC48430EF9B009F1A`. The 2026-06-15 0.1.4 Release deploy build produced a live DLL hash matching the Release output: `9542401A4BAD13D6A132435619B65DCC4702A9BDD3089777A7CFF068C905DD06`. The 2026-06-15 0.1.5 Release deploy build produced a live DLL hash matching the Release output: `13D3DED6B89374A17151F0F9DD47D127912506C3CBAE2E0AF49755FA43ABEFE4`. The 2026-06-15 0.1.6 Release deploy build produced a live DLL hash matching the Release output: `516E41A71391159550EDCF402A8DD55398F9DA4FBFD25C83E1B4ED1AE1AB0874`. The 2026-06-15 0.1.7 Release deploy build produced a live DLL hash matching the Release output: `8453D1DA88CC4C173033DC610715144C1F449B8F5766C9846AA150A629471ECB`. The 2026-06-15 0.1.8 Release deploy build produced a live DLL hash matching the Release output: `86A52E26BA1EE94B27902BAA8235AFFC6B978868B12F2E71479173D49476616C`. The 2026-06-15 0.1.9 Release deploy build produced a live DLL hash matching the Release output: `87D09569851700780E9F8E737837DF5469393778AE258B774B3998A7947AF039`. The 2026-06-15 0.1.10 Release deploy build produced a live DLL hash matching the Release output: `5DC8B1F5B5F5A60E1A8831883D31E3E1E4375CE30FECA6062B7AFB0A0B75BB90`. The 2026-06-15 0.1.11 Release deploy build produced a live DLL hash matching the Release output: `C1E5FBE0E4F3442EF57C2EE6071B02067BEA465349359A778C068B0A44F0BC6F`. The 2026-06-15 0.1.12 Release deploy build produced a live DLL hash matching the Release output: `88B560DB5CBF6DC42F0EA380FFA6567A2151D40D321DF42822191A3E5E1FD73F`. The 2026-06-16 0.1.13 Release deploy build produced a live DLL hash matching the Release output: `786830935E0FFD76A286A4345D5803C771A3727D450DAC2417F1EBF0B0717B11`. The 2026-06-16 0.1.14 Debug build passed with 0 warnings and 0 errors. The 2026-06-18 0.1.15 Debug build passed with 0 warnings and 0 errors. The 2026-06-18 0.1.15 Release deploy build passed with 0 warnings and 0 errors; live DLL and Release output both report file version `0.1.15.0` and SHA256 `FECCB2981849A74CF2FC58A83A722C84FAC9A1071F6A96A8A5FC4A9FEBA78AE6`. The 2026-06-18 0.1.16 Debug build passed with 0 warnings and 0 errors. The 2026-06-18 0.1.16 Release deploy build passed with 0 warnings and 0 errors; live DLL and Release output both report file version `0.1.16.0` and SHA256 `81BAF1227C9CE81971EC7382DF00CEFAC3ACEDF5C625C67D3DE1AE4B9A499C5F`. The 2026-06-19 0.1.17 Debug build and local Release build passed with 0 warnings and 0 errors. The first 0.1.18 Debug build failed from an over-escaped string literal in `AvalonCoreDiagnosticBridge`; after correction, the 0.1.18 Debug build, local Release build, and Release deploy build passed with 0 warnings and 0 errors. Live DLL and Release output both report file version `0.1.18.0` and SHA256 `54A72D76226714A400EB054F9336150AA6E7EB9BB650DDEAF9F0907197539AC6`. BepInEx log validation passed on 2026-06-19 with Avalon Companions `0.1.18`, Avalon Core `0.7.7`, and the bridge trust-report line showing `WouldMutateRuntime=False; action=read-only`. The 2026-06-19 0.1.19 Debug build, local Release build, and Release deploy build passed with 0 warnings and 0 errors. Live DLL and Release output both report file version `0.1.19.0` and SHA256 `F8E86BDBE8208731E095A8AB20046555E950CF1D6E5EAD7486C2467F6E10C0D5`. The 2026-06-19 0.1.20 Debug build and Release deploy build passed with 0 warnings and 0 errors. Live DLL and Release output both report file version `0.1.20.0` and SHA256 `A5A2C9CA26BBBEC03F4AE78781FF1110FDD1AE714B2AE32B699123873C3F7701`. The 2026-06-19 0.1.21 local Release build passed with 0 warnings and 0 errors before deploy; live deploy/hash validation is recorded below after deployment.

## 0.1.33 AI boundary manual smoke aid validation - 2026-06-20

- Scope: add a visible debug-panel `AI Boundary Check` button that writes one immediate read-only AI boundary snapshot only when `Diagnostics.WriteCompanionAiBoundaryDiagnostics=true`, and reports the blocked config state when disabled.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.33.0`.
- Built Release DLL SHA-256: `DB64D8DA4B0D7FC4322C883E6BEA61878333ECF95EB72AE1B9E7E1CCC033D347`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.33.0`; live SHA-256 matched `DB64D8DA4B0D7FC4322C883E6BEA61878333ECF95EB72AE1B9E7E1CCC033D347`.
- BepInEx load validation: Pending.
- In-game diagnostic smoke: Pending. Required check: open `Avalon Companions Debug` with the diagnostic config disabled and confirm `AI Boundary Check` is visible and reports the blocked config state. Then enable the diagnostic config, reopen the debug panel, click `AI Boundary Check` with an active managed companion, and confirm one `panel-ai-boundary-check` row appears in `companion-ai-boundary.csv` plus an `ai-boundary-check` command-log row.
- Expected exclusions: no command dispatch, native follow/defend refresh queue, movement change, target change, native target recalculation, `TargetOverrideElement.GetTarget`, progression state, persistence, Avalon Core behavior, custom AI, taming, training, or loyalty.

## 0.1.32 HUD primitive handoff and AI boundary diagnostic validation - 2026-06-20

- Scope: hand the passive companion HUD badge to Tainted Interface's reusable `DrawHudBadge` primitive when available, retain the local texture path as an older-layer fallback, and add a default-off read-only diagnostic that samples active managed companions' native AI, movement, unchecked target relation, possible target/attacker counts, and movement blocker state without changing behavior.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.32.0`.
- Built Release DLL SHA-256: `4C3F766D390835AE7C7AE83F3545B8DBABFBE6291C8EB8D8D523325ED91EA0CD`.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live DLL file version `0.1.32.0`; live SHA-256 matched `4C3F766D390835AE7C7AE83F3545B8DBABFBE6291C8EB8D8D523325ED91EA0CD`.
- BepInEx load validation: Pending.
- In-game HUD smoke: Pending. Required check: summon or swap to an active managed companion, close companion UI, confirm the badge still appears in the configured corner through Tainted Interface 0.1.5, and confirm the badge disappears when no managed companion is active.
- In-game diagnostic smoke: Pending. Required check: enable `Diagnostics.WriteCompanionAiBoundaryDiagnostics=true` on a throwaway save, summon or swap to an active managed companion, confirm `companion-ai-boundary.csv` appends rows, and verify rows keep `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- Expected exclusions: no command dispatch, movement change, target change, native target recalculation, `TargetOverrideElement.GetTarget`, progression state, persistence, Avalon Core behavior, custom AI, taming, training, or loyalty.

## 0.1.31 companion HUD placement hotfix - 2026-06-20

- Scope: raise the passive top-left companion HUD badge so it sits above the Wyrd Scent/status overlay lane shown in user screenshot `Screenshot (3838).png`.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.31.0`.
- Built Release DLL SHA-256: `E47203C8C83A862F0FD01A2DFEB602E34FC4801E4911D71CD39EA03E3298DDB2`.
- First live deploy attempt: failed while Fall of Avalon was running because the live DLL had a user-mapped section open.
- Live deploy: Passed after closing Fall of Avalon. Live DLL file version `0.1.31.0`; live SHA-256 matched `E47203C8C83A862F0FD01A2DFEB602E34FC4801E4911D71CD39EA03E3298DDB2`.
- BepInEx load validation: Pending.
- In-game HUD smoke: Pending. Required check: with `CompanionHudIconCorner=TopLeft`, confirm the badge sits above the Wyrd Scent/status overlay and still disappears when no managed companion is active.
- Expected exclusions: no spawn, summon, recall, dismiss, lifecycle, prompt, dialogue, combat, target, persistence, or Avalon Core behavior changes.

## 0.1.29 companion HUD icon overlay validation - 2026-06-20

- Scope: draw a passive gameplay HUD badge for the active managed companion using the existing per-companion icon mapping plus Tainted Interface `hud.companion-icon-background`.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`.
- Release build result: Passed with 0 warnings and 0 errors.
- Built current Release DLL file version: `0.1.30.0`.
- Built current Release DLL SHA-256: `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- Required Tainted Interface embedded build: Passed with 0 warnings and 0 errors for file version `0.1.4.0`, SHA-256 `BDCC7657D563FA43EE66E04456FFEF872C590D476E287A3C586202D1D0591525`, with `TaintedInterface.Assets.Icons.HudCompanionIconBackground.png` present.
- Live deploy: Passed. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`; live SHA-256 matched `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- BepInEx load validation: Passed. `LogOutput.log` showed `Loading [Avalon Companions 0.1.30]` and `Avalon Companions 0.1.30 loaded` with `ShowCompanionHudIcon=True`, `CompanionHudIconCorner=TopLeft`, and `CompanionHudIconSize=88`.
- In-game HUD smoke: Pending. Required check: summon or swap to a managed companion, close companion UI, confirm the badge appears in the configured corner with the Dragon background, dismiss the companion, and confirm the badge disappears without command regressions.
- Expected exclusions: no spawn, summon, recall, dismiss, lifecycle, prompt, dialogue, combat, target, persistence, or Avalon Core behavior changes.

## Next development gates after 0.1.42

- Lifecycle validation smoke: run the 0.1.42 throwaway-save route and review `companion-lifecycle.csv` plus `companion-command-log.csv`.
- Core integration gate: after the framework is stable, validate diagnostic/profile registration through Avalon Core behind gates; no Core-executed behavior yet.
- Custom AI research gate: inspect native follow/pathing/combat/targeting boundaries before adding custom AI logic.

## 0.1.30 command-surface polish validation - 2026-06-20

- Scope: improve the existing plugin-owned Unity UI companion command surface without changing command behavior.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`.
- Release build result: Passed with 0 warnings and 0 errors.
- Release deploy command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`.
- Release deploy result: Passed with 0 warnings and 0 errors.
- Built and live DLL file version: `0.1.30.0`.
- Built and live DLL SHA-256: `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- Required in-game smoke: Pending. Open `Companion`, verify hover/selected/disabled visuals, click Follow and Stay, click Goodbye, press Esc, confirm `KeypadPeriod` still opens only `Avalon Companions Debug`, and inspect `companion-command-log.csv` for `dialogue-choice` and `dialogue-close` audit rows.
- Not in scope: native story dialogue, new commands, custom AI/pathing, target selection, persistence, Core-executed behavior, taming, training, or loyalty.

## 0.1.28 per-companion icon mapping validation - 2026-06-20

- Scope: map existing roster families to expanded Tainted Interface companion icon IDs after user screenshot validation showed all non-wolf/non-bear rows using the generic fallback icon.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.28.0`.
- Built Release DLL SHA-256: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- Live deploy: Passed. Live DLL SHA-256 matched Release output: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- BepInEx load validation: Passed on 2026-06-20. `LogOutput.log` showed `Loading [Avalon Companions 0.1.28]` and `Avalon Companions 0.1.28 loaded`.
- In-game icon smoke: Pending. Required check: deploy Tainted Interface 0.1.2 embedded plus Avalon Companions 0.1.28, open `Avalon Companions Debug`, cycle through Qrko, wolf, bear, skeleton, drowner, and one offensive creature row, and confirm distinct icons render without changing command behavior.

## 0.1.28 carried dialogue direct-click validation - 2026-06-20

- Scope: the current 0.1.28 build carries the 0.1.27 dialogue-local direct pointer fallback while adding only debug-panel icon mapping.
- Release build result: covered by the 0.1.28 per-companion icon mapping validation above; passed with 0 warnings and 0 errors.
- Live deploy hash check: Passed for the current deployed build carrying this fallback. Local and live `AvalonCompanions.dll` both report file version `0.1.28.0` and SHA-256 `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- BepInEx load validation: Passed on 2026-06-20. `LogOutput.log` showed `Loading [Avalon Companions 0.1.28]` and `Avalon Companions 0.1.28 loaded`.
- In-game command-click smoke: Passed by user report and command CSV evidence on 2026-06-20. `companion-command-log.csv` recorded `mode-follow` at `2026-06-20 02:45:26.646` and `mode-stay` at `2026-06-20 02:45:36.697` for `Wolf Candidate` after the 0.1.28 load.
- Direct fallback marker: Not observed. The missing `Avalon Companions dialogue direct choice clicked` marker means the click likely travelled through the normal Unity `Button.onClick` path, or the marker was not flushed; command execution is still validated.
- Remaining in-game UI smoke: Goodbye, Esc input restoration, `KeypadPeriod` debug-only behavior, and debug-panel icon rendering are not separately logged in this pass.

## 0.1.25 dialogue input validation - 2026-06-20

- Scope: fix visible but non-interactable Unity UI dialogue options by allowing Rewired axis/button reads while the dialogue is visible, while keeping the IMGUI debug panel's stricter input lock.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release build result: Passed with 0 warnings and 0 errors.
- Deploy build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deploy build result: Passed with 0 warnings and 0 errors.
- Live deploy hash check: Passed. Local and live `AvalonCompanions.dll` both report file version `0.1.25.0` and SHA-256 `06CBACBF1BDB33B83FA9C9B4AC6DB5CDA78F06E89206CE7AD0EA86409781BDF5`.
- BepInEx load validation: Pending for 0.1.25 because the game process was already running after deploy.
- In-game click smoke: Pending after restart. Required check: open `Companion`, click at least Follow or Stay, confirm command runs and closes the dialogue, confirm Goodbye closes, confirm Esc restores input, and confirm `KeypadPeriod` opens only `Avalon Companions Debug`.

## 0.1.24 shared icon consumer validation - 2026-06-19

- Scope: optional selected-roster icon rendering in the IMGUI debug panel through Tainted Interface's shared icon catalog.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -v:minimal`
- Release build result: Passed with 0 warnings and 0 errors.
- Built Release DLL file version: `0.1.24.0`.
- Built Release DLL SHA-256: `F876852E5B807B18E453E89E96AC6F4D9CADEF24F61F859CFB5F890FCE2A7FF0`.
- Live deploy: Passed on 2026-06-20 after the user requested the live deploy/smoke gate. Copied `AvalonCompanions.dll` to `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll`.
- Live deploy hash check: Passed. Live DLL SHA-256 matched Release output: `F876852E5B807B18E453E89E96AC6F4D9CADEF24F61F859CFB5F890FCE2A7FF0`.
- BepInEx load validation: Passed on 2026-06-20 after launch. `LogOutput.log` showed `Loading [Avalon Companions 0.1.24]` and `Avalon Companions 0.1.24 loaded`.
- In-game icon smoke: Not completed. The log did not show `Avalon Companions Debug` open/close or Tainted Interface icon catalog initialization because the debug panel was not opened during this check. Expected check is `Avalon Companions Debug` showing the selected companion icon when Tainted Interface has the requested shared icon available, while all command buttons continue using the existing approved command paths.

## 0.1.23 vanilla-style dialogue layout validation - 2026-06-19

- Scope: keep the 0.1.22 Unity UI custom dialogue host, but remove the centered panel composition and use right-side choices plus a bottom dialogue text band.
- Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release build result: Passed with 0 warnings and 0 errors.
- Deploy build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deploy build result: Passed with 0 warnings and 0 errors.
- Live deploy hash check: Passed. Local and live `AvalonCompanions.dll` both report file version `0.1.23.0` and SHA-256 `9BEC336B3CF68271A870D43336E0FE644F49B7E4686702053A9B667113971B35`.
- BepInEx load validation: Passed for plugin load. `LogOutput.log` showed `Loading [Avalon Companions 0.1.23]` and `Avalon Companions 0.1.23 loaded`.
- In-game prompt/layout validation pending: open the native `Companion` prompt and confirm the log emits `layout=vanilla-style`, the centered panel is gone, choices appear on the right, the bottom text band appears, buttons click, and close/Esc restores input.

## 0.1.22 Unity UI dialogue host validation - 2026-06-19

- Scope: replace the native `Companion` prompt's old IMGUI dialogue window with a plugin-owned Unity UI `Canvas` dialogue host only.
- First Debug build result: Failed because `Canvas` required an explicit `UnityEngine.UIModule` reference and the new partial file was missing the companion framework namespace.
- Corrected Debug build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Corrected Debug build result: Passed with 0 warnings and 0 errors.
- Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release build result: Passed with 0 warnings and 0 errors.
- Deploy build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deploy build result: Passed with 0 warnings and 0 errors.
- Live deploy hash check: Passed. Local and live `AvalonCompanions.dll` both report file version `0.1.22.0` and SHA-256 `1CDC68A905DB2611A6FE9E6589D6B1CBD6ED69E85096F2AD7D6677DDFF2AE228`.
- BepInEx load validation: Passed. `LogOutput.log` showed `Loading [Avalon Companions 0.1.22]` and `Avalon Companions 0.1.22 loaded`.
- Partial debug-panel separation validation: observed in the live log tail. `KeypadPeriod` produced `Panel opened` and `Panel closed`, but a later `Select-String` pass did not return those lines, so repeatable archived log evidence remains pending.
- In-game validation pending: summon one managed one-session companion, activate `Companion`, confirm the Unity UI dialogue host opens instead of the IMGUI debug panel, click each command once, confirm close/Esc/cursor restoration, and confirm `KeypadPeriod` still opens only `Avalon Companions Debug` visually.

## 0.1.21 shared UI shell validation - 2026-06-19

- Scope: optional Tainted Interface shared styling/input-scope bridge for the plugin-owned custom dialogue surface and separate debug panel only.
- Local Release build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Local Release build result: Passed with 0 warnings and 0 errors.
- Deploy build command: `dotnet build .\mods\avalon-companions\src\AvalonCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deploy build result: Passed with 0 warnings and 0 errors.
- Live deploy hash check: Passed. Local and live `AvalonCompanions.dll` both report file version `0.1.21.0` and SHA-256 `394346C5832D584EAF563F997E085F7B3DD9EC8BFA6344BF96D61385C4CB10AB`.
- In-game validation pending: launch, confirm `Avalon Companions 0.1.21` load, open native `Companion` custom dialogue once, open `KeypadPeriod` debug panel once, confirm Tainted Interface shared-style log line when the layer is installed, and verify close/cursor restoration.

## Load validation

- Game version:
- Branch:
- BepInEx version:
- Log evidence: `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\LogOutput.log` showed `Avalon Companions 0.1.0 loaded` on 2026-06-14 after deployment.
- Log evidence: `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\LogOutput.log` showed `Avalon Companions 0.1.3 loaded` on 2026-06-15 with roster, lifecycle dump, command log, native defend assist, and follow range profile enabled.
- Duplicate cleanup: BepInEx also reported skipping an older `Avalon Companions 0.1.0`; the stale zero-byte duplicate at `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\plugins\plugins\AvalonCompanions\AvalonCompanions.dll` was removed on 2026-06-15. The live `BepInEx\plugins\AvalonCompanions\AvalonCompanions.dll` remained version `0.1.3.0`.

## Feature validation

- Scenario: launch with the DLL installed and diagnostics disabled.
- Expected: plugin logs `Avalon Companions 0.1.3 loaded` and companion panel/input-lock patches remain inert unless the panel is visible.
- Actual: plugin load confirmed with diagnostics enabled on 2026-06-15. Diagnostics-disabled load remains pending.
- UI screenshot/video evidence, if applicable: not applicable; no UI exists.

## Pet companion validation

- Scenario: set `Research.ResearchModeOnly=false`, set `Companions.EnablePetCompanionRoster=true`, load a throwaway save, and use the roster controls.
- Expected: select among four approved Qrko pet companions plus the reviewed animal, offensive creature, and undead one-session candidates. Qrko entries can summon/swap, recall, dismiss, and use explicit Follow/Stay/Defend panel modes through native pet components where available. Animal/creature candidate entries can spawn only by explicit summon/swap command, are marked not saved, require an `NpcElement`, apply the same native summon-faction plus `NpcHeroPetAlly` path used by `NpcAllyPetVariant`, are tracked only for the current session, and are limited by Avalon controls to recall/catch-up/dismiss plus explicit Follow/Stay/Defend modes. Defend mode and the automatic native defend fallback only prompt `NpcHeroPetAlly.EnterCombat()` when the hero already has live attackers. Passive animals may not have useful combat response. The controller refuses to touch unmanaged pets and does not apply custom targeting, attack commands, persistence, or save/load behavior to candidates.
- Actual: passed for spawn logging on 2026-06-14. BepInEx log reported `Qrko pet prototype spawned Spec_Pet_Qrko[cc30c92e0699e3c41be92d1e99057354]` and `Location marked not saved`.
- Follow-up: roster smoke found that a dismissed/swapped Qrko could remain in world without the managed marker, causing later commands to skip it as unmanaged. Controller now treats any active pet with one of the four validated roster GUIDs as controllable and re-marks it not saved when acting on it.
- Follow-up: latest smoke showed key presses were detected and different GUIDs were selected/spawned in the log, but selection did not visibly swap until summon was pressed again. Next/previous now immediately swap the active pet, and commands show short on-screen status.
- UI follow-up: added a styled command panel on `KeypadPeriod` with selected pet, active pet, summon/swap, next/previous, recall, dismiss, explicit Follow/Stay/Defend mode buttons, current mode text, automatic defend fallback state, and status text. The panel toggle now runs before the gameplay/template readiness gate and reports why commands are unavailable instead of staying hidden. In-game visual validation is pending.
- UI layout follow-up: after screenshot `Screenshot (3570).png`, expected panel behavior is responsive readable sizing on high-resolution displays, screen-clamped placement, draggable header movement, themed focused/pressed controls, and no mouse-event pass-through from clicks inside the panel.
- Input-lock follow-up: while the command panel is open, expected behavior is cursor visible/unlocked during IMGUI draw, Unity input modules disabled, input axes reset, FoA mouse-position updates blocked, player movement/look zeroed, and Rewired axes/buttons returning neutral values. User smoke validation on 2026-06-15 reported this panel/cursor path works. Broader conflict-state checks should still be repeated before a non-alpha release.
- Ally marker follow-up: one-session creature candidate validation should confirm the BepInEx log reports native ally setup applied, the spawned candidate is not hostile to the player, dismiss removes it cleanly, and failed ally setup discards the spawn instead of leaving a hostile creature alive.
- Native defend follow-up: with `Companions.EnableNativeDefendAssist=true`, validation should confirm combat-capable candidates enter combat with a live attacker after the hero is attacked, do not attack civilians without a hero-attacker relation, do not spam logs, and still dismiss cleanly during or after combat. With panel Defend mode selected, validation should confirm the same native entry path is armed even if the automatic fallback is disabled, while still requiring a live hero attacker.
- Native interaction follow-up: after summoning a managed one-session creature candidate, validation should confirm exactly one `Companion` native world prompt/action is attached to the managed actor, no prompt/action appears on wild or quest actors, activating the prompt opens the custom dialogue interface with that actor selected, dismiss removes the action, and the BepInEx log reports only runtime-only action attachment/removal with no save-owned interaction or story graph edits.
- Creature expansion follow-up: after summoning each reviewed one-session candidate, validation should confirm the row spawns only by explicit command, is not immediately hostile, receives or cleanly rejects the native ally marker, can recall and dismiss, has the native `Companion` prompt only while managed, and never writes persistence state.
- Creature expansion actual: user reported on 2026-06-15 that the expanded 0.1.1 roster works perfectly. This clears the immediate smoke gate for summon/actions/dismiss/change behavior, but does not clear transition, rest, quit/reload, persistence, or broad mod-conflict validation.

## Advanced command validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, and `Diagnostics.WriteCompanionCommandLog=true`, then summon one managed roster companion on a throwaway save.
- Expected: the panel shows the current follow range and includes Heel plus Close/Normal/Far buttons.
- Scenario: press `Heel`.
- Expected: the companion mode becomes Follow, follow range becomes Close, active managed companions recall near the hero, and `companion-command-log.csv` records `command=heel` with `touchesTargeting=false` and `touchesPersistence=false`.
- Scenario: press `Come Close` on one-session animal, offensive creature, and undead candidates.
- Expected: the command does not kill or discard the companion; creature candidates keep normal-or-far tuned offsets and use native placement verification before movement.
- Scenario: press Close, Normal, and Far with a companion active.
- Expected: recall placement and one-session catch-up thresholds change according to the selected range without spawning duplicates or touching unmanaged actors. The command log records each range command.
- Scenario: use Defend mode after the hero has a live attacker.
- Expected: Defend still uses only the native `NpcHeroPetAlly.EnterCombat()` path and does not expose an Attack button or arbitrary target selector.
- Actual: partially run on 2026-06-15. `companion-command-log.csv` recorded blocked dismiss, select-next, and summon/swap. The summon/swap row for `Armored Drowner Candidate` had `touchesTargeting=false` and `touchesPersistence=false`. Heel, Close/Normal/Far, and Defend mode command rows were not present yet.

## Native command-menu validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableNativeCommandMenu=true`, and `Companions.EnableCustomDialogueInterface=true`, then summon one managed one-session companion.
- Expected: the managed companion has one runtime-only native `Companion` prompt and no rotating quick-command prompts.
- Scenario: activate the `Companion` prompt.
- Expected: the custom dialogue command interface opens with the selected companion context and exposes Follow, Stay, Defend, Close/Normal/Far range, Come Close, Recall, Recover, Dismiss, and close choices at once.
- Scenario: run each command from the custom dialogue.
- Expected: each command uses the same managed command path as the debug panel/hotkey command, writes a command CSV row, closes the dialogue after the choice, and does not touch story graphs, templates, target selection, healing, persistence, or unmanaged actors.
- Scenario: press `Esc` or the close choice while the custom dialogue is open.
- Expected: the dialogue closes and cursor/world input are restored.
- Scenario: set `Companions.EnableNativeCommandMenu=false` and `Companions.EnableNativeQuickCommands=true`.
- Expected: rotating native quick-command prompts are available as fallback-only behavior.
- Scenario: dismiss the companion.
- Expected: all Avalon native action elements are removed.
- Scenario: summon/swap again after dismiss.
- Expected: the quick-command cursor resets and the next managed companion does not inherit stale `Dismiss`.
- Actual: 0.1.5 user smoke test showed native interaction opened the panel. 0.1.6 user smoke test showed the panel no longer opened, but the prompt only advanced through Follow and Stay. 0.1.7 user smoke test showed the prompt can stick on `Dismiss`. 0.1.9 retest is pending after making the one-prompt command menu the default. 0.1.20 custom dialogue in-game smoke is pending. 0.1.21 shared UI shell smoke showed the surface was still panel-style IMGUI. 0.1.22 Unity UI dialogue-host smoke showed the surface was still centered-panel shaped. 0.1.23 vanilla-style layout smoke is pending.

## Debug panel validation

- Scenario: set `Debug.EnableDebugPanel=true`, `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, and `Companions.EnableNativeCommandMenu=true`, then summon one managed one-session companion.
- Expected: the world prompt label is `Companion` and opens the custom dialogue interface. The debug panel does not open from the native prompt.
- Scenario: press `KeypadPeriod`.
- Expected: the `Avalon Companions Debug` panel opens or closes, uses the shared companion UI input lock, and reports runtime block reasons when commands are unavailable.
- Scenario: set `Debug.EnableDebugPanel=false`.
- Expected: the debug panel hotkey closes or ignores the panel, but the native `Companion` custom dialogue prompt still works when `Companions.EnableCustomDialogueInterface=true`; fallback quick commands appear only if `Companions.EnableNativeCommandMenu=false` and `Companions.EnableNativeQuickCommands=true`.
- Expected exclusions: no native dialogue graph, fake `StoryBookmark`, custom AI, taming, training, loyalty, persistence, restoration, respawn, healing, squads, attack button, or target selector.
- Actual: 0.1.19 Debug, Release, and Release deploy builds passed. 0.1.20 Debug and Release deploy builds passed. 0.1.21 local Release build passed before deploy. 0.1.23 Debug, Release, and Release deploy builds passed, and BepInEx load validation passed. In-game prompt/dialogue/debug-panel smoke is pending.

## Advanced native ally behavior validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableNativeDefendAssist=true`, and `Diagnostics.WriteCompanionCommandLog=true`, then summon a managed offensive or undead one-session candidate on a throwaway save.
- Expected with no hero attackers: Defend mode waits for an attacker and does not repeatedly prompt native combat.
- Expected when the hero has a live attacker: `companion-command-log.csv` records `defend-armed` plus one `defend-prompt` or `auto-defend-prompt`, and the companion uses only native `NpcHeroPetAlly.EnterCombat()`.
- Expected during continued combat: repeated automatic native defend prompts are throttled per managed companion.
- Expected after attackers clear: `companion-command-log.csv` records `defend-clear`, and the next attacker wave can prompt immediately again.
- Expected catch-up rows: `auto-catch-up` rows include range, distance, threshold, and slot.
- Expected exclusions: no Attack button, target selector, custom hostility scan, healing command, persistence, or squad behavior is exposed.
- Actual: 0.1.10 in-game retest pending.

## Command polish validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, and open the Avalon command panel before summoning a companion.
- Expected: panel shows no active companion and exposes no Attack button or target selector.
- Scenario: summon one managed one-session candidate and open the panel.
- Expected: panel shows that candidate's distance from the hero, mode state, native ally marker state, native prompt state, and not-saved state.
- Scenario: press Follow, Stay, and Defend.
- Expected: mode feedback describes catch-up on, catch-up off, or waiting/armed native defend behavior.
- Actual: 0.1.11 in-game retest pending.

## Managed recovery validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableLifecycleSafetyGuard=true`, and `Diagnostics.WriteCompanionCommandLog=true`, then summon one managed one-session candidate on a throwaway save.
- Expected: the panel exposes `Recover`, and the fallback native quick-command cycle can also expose `Recover` when `Companions.EnableNativeCommandMenu=false`.
- Scenario: move or strand the managed companion beyond the Far catch-up threshold, then press `Recover`.
- Expected: the companion is recalled through existing verified placement, no duplicate actor spawns, `companion-command-log.csv` records `command=recover`, and `touchesTargeting=false`, `touchesPersistence=false`.
- Scenario: kill or structurally invalidate a managed one-session candidate, then press `Recover`.
- Expected: the actor is removed if the lifecycle safety guard is enabled. It is not healed, resurrected, respawned, persisted, retargeted, or converted into a squad member.
- Actual: user-reported 0.1.13 in-game recovery smoke completed on 2026-06-16 after the live DLL deploy. CSV/log evidence from that smoke pass has not been inspected in this repo pass.

## Session continuity / persistence gate validation

- Scenario: inspect latest 0.1.12 and 0.1.13 `companion-lifecycle.csv` and `companion-command-log.csv` evidence before writing continuity code.
- Expected: rows show managed one-session actors remain `markedNotSaved=true`, command rows keep `touchesPersistence=false`, lifecycle rows do not imply saved command actions, auto-respawn, actor restoration, or persistent ownership.
- Scenario: summon one managed one-session candidate, fast travel or area-transition, rest, save, quit, reload, and return on a throwaway save.
- Expected: no duplicate active companion, no orphan native `Companion` prompt, no unmanaged leftover ally, and no automatic companion respawn after reload. If the companion is gone after reload because it was not saved, that is still correct.
- Scenario: after reload, use only explicit player command to summon/swap the previously selected roster entry.
- Expected: any later continuity implementation uses existing explicit one-session summon/swap behavior and may only remember metadata/status until the player acts.
- Actual: CSV/log inspection completed on 2026-06-16. `companion-lifecycle.csv` had 1504 rows and `companion-command-log.csv` had 215 rows. No managed lifecycle row with a `templateGuid` had `markedNotSaved=false`; no command row had `touchesPersistence=true`; no command row had `touchesTargeting=true`. The latest 0.1.13 recovery/lifecycle rows for `Corpse Eater Candidate` were clean, and the latest shutdown row reported no active roster actors. Older duplicate-cleanup rows showed the lifecycle guard discarding an excess active roster actor. This evidence clears only metadata/status continuity for a future slice. Full rest, quit, reload, return, no-orphan-prompt, and no-auto-respawn validation remains not run.

## Metadata/status continuity validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, and `Diagnostics.WriteCompanionCommandLog=true`, then select a reviewed companion, set mode/range, and leave no managed actor active.
- Expected: `Continuity.RememberedCompanionGuid`, `Continuity.RememberedCompanionName`, `Continuity.RememberedCommandMode`, and `Continuity.RememberedFollowRange` store plugin-owned metadata only.
- Expected: the panel reports remembered companion metadata when no managed actor is active and tells the player to use `Summon / Swap`.
- Expected: no actor restores, respawns, saves, or re-adopts until the player explicitly uses the existing `Summon / Swap` command.
- Expected: `companion-command-log.csv` can record `continuity-status` rows with `touchesTargeting=false` and `touchesPersistence=false`.
- Actual: 0.1.14 Debug build passed on 2026-06-16 with 0 warnings and 0 errors. In-game continuity smoke pending.

## Responsive native polish validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableNativeDefendAssist=true`, and `Diagnostics.WriteCompanionCommandLog=true`, then summon one managed one-session candidate on a throwaway save.
- Expected: summon/swap, recall, Come Close, range changes, Follow/Stay/Defend mode changes, managed recovery, lifecycle check, and native command entry queue the existing follow catch-up and native defend checks for immediate evaluation on the next update.
- Expected: per-companion native defend prompt cooldowns are not cleared by the refresh, so repeated `NpcHeroPetAlly.EnterCombat()` prompts remain throttled.
- Expected: command rows continue to record `touchesTargeting=false` and `touchesPersistence=false`.
- Expected exclusions: no Attack button, target selector, custom pathing, custom AI, healing, respawn, persistence, reload restoration, re-adoption, or squad behavior is exposed.
- Actual: 0.1.15 Debug build passed on 2026-06-18 with 0 warnings and 0 errors. 0.1.15 Release deploy build passed on 2026-06-18 with live DLL hash matching Release output: `FECCB2981849A74CF2FC58A83A722C84FAC9A1071F6A96A8A5FC4A9FEBA78AE6`. In-game responsive/native polish smoke pending.

## Lifecycle validation

- Scenario: set `Research.ResearchModeOnly=false`, `Companions.EnablePetCompanionRoster=true`, `Companions.EnableLifecycleSafetyGuard=true`, and `Diagnostics.WriteCompanionLifecycleDump=true`, then summon one reviewed one-session candidate on a throwaway save.
- Expected: summon writes `companion-lifecycle.csv` rows with `markedNotSaved=true`, `isTrackedCreature=true` for creature candidates, and one active roster actor.
- Scenario: fast travel or trigger a long-position transition with the companion active.
- Expected: no duplicate active companion appears; the CSV records a `scene-change`, `long-hero-move`, or periodic row with one active roster actor or a clean no-active row if the native path removed it.
- Scenario: area/scene transition with the companion active.
- Expected: no unmanaged `Companion` prompt is left behind; untracked one-session roster allies are discarded by the lifecycle guard and recorded in CSV notes.
- Scenario: rest, then inspect the world and CSV.
- Expected: no duplicate, no saved extra copy, and no unmanaged native companion action.
- Scenario: save, quit, reload, and return to the save.
- Expected: one-session companions do not restore as persistent actors. If any untracked one-session ally is present, lifecycle guard discards it and records `untracked one-session roster ally`.
- Scenario: press `Lifecycle Check` before and after fast travel, area transition, rest, and reload checks.
- Expected: `companion-lifecycle.csv` records `panel-lifecycle-check` rows, and `companion-command-log.csv` records `lifecycle-check` rows without adding persistence, restoration, or squads.
- Actual: partially run on 2026-06-15. `companion-lifecycle.csv` recorded one active tracked `Spec_EnemyZombie_T1_DrownerFullArmor` roster actor with `markedNotSaved=true`, `hasNpcHeroPetAlly=true`, `isTrackedCreature=true`, `hasCommandAction=true`, `duplicatesDiscarded=0`, and `untrackedDiscarded=0`. The 0.1.12 lifecycle checkpoint smoke was user-reported complete on 2026-06-15 after the live DLL deploy. CSV/log evidence from that smoke pass has not been inspected in this repo pass, and full transition/rest/quit-reload evidence remains incomplete.

## Diagnostic validation

- Scenario: set `Diagnostics.LogPetSystemDiagnostics=true` on a throwaway save.
- Expected: one BepInEx log line reports current pet/summon counts, pet base variant availability, Qrko mount template count, and loaded `Spec_Pet_*` template names with GUIDs.
- Scenario: set `Diagnostics.WritePetTemplateDump=true` on a throwaway save.
- Expected: `pet-template-candidates.csv` is written under this plugin's BepInEx config folder with GUID-backed `Spec_Pet_*` `LocationTemplate` rows marked `safeSpawnCandidate=false`.
- Actual: passed on 2026-06-14. Runtime log reported `PetElements=0`, `PetVariants=0`, `HeroSummons=0`, `HeroPetAllies=0`, `CommonReferences.PetBaseVariant.IsSet=True`, `QrkoMountTemplates=4`, and four `Spec_Pet_Qrko*` templates.
- Scenario: set `Diagnostics.WritePetCreatureShortlistDump=true` on a throwaway save.
- Expected: `pet-creature-location-candidates.csv` and `pet-creature-location-shortlist.csv` are written under this plugin's BepInEx config folder. Rows stay diagnostics-only with `safeSpawnCandidate=false` and `rosterApproved=false`.
- Expected with `Diagnostics.WritePetCreatureDiagnosticToolCrosscheck=true`: if a latest FOA-Diagnostic Tool dump exists, `pet-creature-diagnostic-tool-crosscheck.csv` and `pet-creature-diagnostic-tool-review.csv` are also written under this plugin's BepInEx config folder. These files compare Avalon live-scan evidence with FOA-Diagnostic Tool `spawner_refs.csv` evidence and still keep `safeSpawnCandidate=false` and `rosterApproved=false`.
- Expected selector behavior: crosscheck source selection skips FOA-Diagnostic Tool startup dumps below `Diagnostics.DiagnosticToolMinimumSpawnerRefs=100` and prefers the newest route-context dump.
- Actual: build passed after implementation. Earlier in-game dump validation produced CSVs, but `Spec_AnimalWolf` and `Spec_AnimalBear` were missing from Avalon's own shortlist despite FOA-Diagnostic Tool evidence. First crosscheck output was written, but selected a low-context 20-row startup dump.
- Follow-up actual: thresholded selector rerun passed on 2026-06-14. BepInEx log reported `selected dump with 556 spawner refs` from FOA-Diagnostic Tool dump `20260614-221744`; crosscheck output wrote 275 rows and 38 review candidates. `Spec_AnimalWolf` and `Spec_AnimalBear` appeared as FOA-Diagnostic Tool review candidates only, with `safeSpawnCandidate=false` and `rosterApproved=false`.

## Release validation

- Package checked:
- README/changelog/manifest aligned: yes for scaffold docs.
- Known limitations documented: yes, research-gated companion behavior is documented.

## Not run

- Diagnostics-disabled load validation.
- Visual follow behavior confirmation.
- Command panel screenshot/visual validation.
- Native companion interaction prompt in-game validation.
- Broader companion panel cursor/input-lock conflict-state validation beyond the 2026-06-15 user smoke check.
- Transition, rest, quit/reload, and persistence validation for the expanded animal, offensive creature, and undead one-session candidates.
- Full in-game lifecycle guard CSV validation for transition/rest/quit-reload behavior.
- Full in-game Heel/range/Defend command validation for 0.1.3.
- In-game native command-menu validation for 0.1.9.
- In-game advanced native ally behavior validation for 0.1.10.
- In-game command polish panel validation for 0.1.11.
- CSV/log inspection for the user-reported 0.1.13 managed recovery smoke pass.
- CSV/log inspection for the user-reported 0.1.12 lifecycle checkpoint smoke pass.
- Pet/creature shortlist in-game dump validation.
- In-game transition, rest, quit, reload, return, and persistence tests.
- Session-continuity/persistence gate throwaway-save rest, quit, reload, return, no-orphan-prompt, and no-auto-respawn validation.
- 0.1.14 in-game metadata/status continuity smoke and CSV inspection.
- 0.1.15 in-game responsive/native polish smoke and CSV inspection.
- 0.1.17 in-game smoke; not required for the design-only Core handoff slice because no runtime behavior changed beyond version metadata.
- 0.1.23 native `Companion` prompt layout smoke: `layout=vanilla-style` marker, right-side choices, bottom text band, click handling, close/Esc input restoration, and `KeypadPeriod` debug-only behavior.
- 0.1.28 per-companion icon smoke with Tainted Interface 0.1.2.
- 0.1.28 full UI smoke for Goodbye, Esc input restoration, `KeypadPeriod` debug-only behavior, and debug-panel icon rendering.
- 0.1.25 in-game dialogue click smoke after EventSystem/Rewired pass-through.
- 0.1.24 shared-icon in-game smoke.
