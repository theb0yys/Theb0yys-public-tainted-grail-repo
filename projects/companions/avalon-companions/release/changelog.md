# Changelog

## 0.2.17

- Added the Contracts-only `kane.tgfoa.avalon-companions.ai-advanced-companion` package surface for managed animal companion proposals and wild tame-readiness classification.
- The package can propose catch-up, native defend, hold-anchor, regroup, recover-stuck, retreat/stand-down, tame-ready, tame-unsafe, or no proposal.
- Kept Avalon Companions as the owner of tame, summon, dismiss, persistence, command UI, observation collection, native dispatch, and lifecycle.
- Added no Unity/FoA/native calls, Rabbit/GOAP/Blaze access, PlayMaker access, direct actor ownership, persistence writes, faction changes, target overrides, movement overrides, or command UI ownership to the package.

## 0.2.16

- Added native-defend prompt readiness to the direct Avalon AI Runtime observation boundary.
- Reset the existing per-companion prompt cooldown when no live hero attackers remain, matching the reviewed standalone defend-state behavior.
- Advanced the engine-neutral Avalon Companions package to 0.2.1 so active cooldown produces no proposal instead of an expected dispatch rejection.
- Preserved the existing 2.5-second/bond-adjusted cooldown, native `NpcHeroPetAlly.EnterCombat()` dispatch, command policy, exact-actor lease, targeting ownership, and gameplay scope.

## 0.2.15

- Restored native-defend command capability for only the exact reviewed Wolf and Bear one-session candidates.
- The runtime role policy now matches the existing Wolf/Bear native-ally research without enabling defend for passive deer, pig, cow, or interaction-only animal candidates.

## 0.2.14

- Added the direct, public game-system boundary used only by the single Avalon AI FoA host composition.
- The boundary acquires one exclusive owner, stands down the legacy follow/defend ticks and the profile-assist loop only after acquisition, and releases ownership on host stop.
- Each collected observation creates a one-shot actor lease; catch-up recall and native-defend dispatch revalidate the exact active `Location`, transition readiness, command policy, attackers, and cooldown before using the existing native-safe paths.
- The game-system boundary contains no Avalon AI package/runtime reference, adapter registry, reflection dispatch, targeting override, movement override, actor persistence, or network behavior.

## 0.2.13

- Added a narrow public registration API for optional target-template dialogue commands from companion-adjacent Avalon plugins.
- External commands render inside the existing Unity UI `Companion` dialogue and reuse its input, row styling, direct-click fallback, portrait, and shared icon assets.
- Commands are keyed by owner and command ID, filtered to the active companion template GUID, and can be removed without replacing the dialogue host.
- Avalon Mounts 0.1.7 uses this API through reflection for one wolf-only `Mount / Dismount` row; Companions remains the sole owner of summon, actor lifecycle, interaction, and UI systems.

## 0.2.12

- Added a read-only `Encounter Probe` diagnostic row to the roster/debug panel for nearby reviewed wolf/bear live actors.
- Writes `companion-taming-encounter.csv` rows with reviewed template identity, live component markers, hostility/target evidence, faction/ownership hints, native AI state, movement state, and save flags.
- Writes a `taming-encounter-probe` command-log row with explicit false mutation flags.
- Does not capture actors, adopt wild actors, attach ally markers, change faction/ownership, spawn, dismiss, persist actors, restore actors, move actors, change targets, change command behavior, or execute behavior through Avalon Core.

## 0.2.11

- Added the read-only `Taming Gate` diagnostic row to the roster/debug panel.
- Uses the existing `CompanionTamingProfile` / `CompanionTamingEligibility` classifier to classify reviewed roster entries as tameable candidate, already-companion, unsafe hostile, undead/blocked, passive-only, or evidence insufficient.
- Writes one `taming-eligibility-gate` row to `companion-command-log.csv` with roster counts and explicit false mutation flags.
- Does not capture actors, adopt wild actors, persist actors, restore actors, auto-respawn, take save ownership, change commands, change movement, change targeting, or execute behavior through Avalon Core.

## 0.2.10

- Added profile-only progression storage through `Companions.EnableCompanionProfileStore=true`.
- Saves approved trust, loyalty, last trust event, last known mode/range, and effective bond fields to `companion-profiles.tsv`, keyed by template GUID plus review ID.
- Loads saved profile values into the runtime trust/loyalty profile only.
- Adds `profile-store-save` audit rows with `touchesPersistence=true` while explicitly keeping actor persistence flags false.
- Does not restore actors, auto-respawn, re-adopt live actors, save locations, health, AI state, target state, or change command behavior.

## 0.2.9

- Added a read-only `Progression Gate` diagnostic button to the roster panel.
- The gate records the approved future progression profile fields: companion identity, trust, loyalty, last trust event, last mode/range, and effective bond.
- The gate explicitly blocks actor instance IDs, location IDs, scene, coordinates, health, native action state, target state, saved actor ownership, auto-respawn, reload restoration, and actor re-adoption.
- Does not save trust/loyalty/bond data yet and does not add actor persistence, restoration, taming, training, custom AI, target overrides, movement overrides, or Core-executed behavior.

## 0.2.8

- Clarified bond display in the supported Unity UI `Companion` dialogue and roster panel.
- Dialogue now reports effective bond, trust score, loyalty score, and a short current-effect phrase.
- Roster panel now shows the same summary plus detailed existing policy multipliers for validation.
- Does not add new trust events, commands, native stat buffs, target overrides, movement overrides, persistence, taming, training, saved progression, or Core-executed behavior.

## 0.2.7

- Added a non-interactive companion portrait/background frame to the supported plugin-owned Unity UI `Companion` dialogue.
- Reuses the existing roster icon mapping and `hud.companion-icon-background` shared texture ID through the optional Tainted Interface bridge.
- Keeps dialogue command rows, command IDs, command delegates, policy gates, click fallback, close routes, and audit rows unchanged.
- Does not add new commands, targets, native stat buffs, target overrides, movement overrides, persistence, taming, training, saved progression, or Core-executed behavior.

## 0.2.6

- Polished the `KeypadPeriod` Avalon Companion Roster panel around roster selection, `Summon / Swap`, active companion safety controls, lifecycle validation, and AI diagnostics.
- Removed duplicated order controls from the summon panel: Follow, Hold Position, Defend, range changes, Come Close, and Recall.
- Kept companion orders on the supported plugin-owned `Companion` dialogue surface.
- Does not add new commands, targets, native stat buffs, target overrides, movement overrides, persistence, taming, training, saved progression, or Core-executed behavior.

## 0.2.5

- Coalesced repeated read-only AI profile audit log rows while keeping the diagnostic CSV path unchanged when enabled.
- Made native command-action synchronization idempotent so the active companion loop does not repeatedly remove/re-add already-correct command action elements.
- Added `Companions.EnableCompanionBondPolicyRuntime` so runtime bond level can modify Avalon-owned catch-up interval, catch-up threshold, native defend cooldown, automatic defend assist availability, and trust gain/penalty scaling.
- `Wary` companions respond slower and require explicit Defend for native defend assist; `Trusted` and `Loyal` companions respond faster and use shorter native defend cooldowns.
- Kept the change runtime-only: no persistence, custom AI/pathing, target selection, movement override, taming, training, save data, or Avalon Core execution.

## 0.2.3

- Added runtime-only `CompanionTrustProfile` state to the companion core runtime profile.
- Trust and loyalty scores update from existing approved companion command/runtime events and are visible in the debug panel plus companion dialogue subtitle.
- Added `Companions.EnableCompanionTrustRuntime` and `Diagnostics.WriteCompanionTrustLog`.
- `companion-trust` audit rows are written to `companion-command-log.csv` when command and trust logging are enabled.
- Does not add persistence, reload restoration, actor re-adoption, custom target selection, movement overrides, taming, training, saved loyalty/progression, or Core-executed behavior.

## 0.1.42

- Added the lifecycle validation gate route for throwaway-save transition, rest, quit/reload, return, duplicate cleanup, orphan cleanup, and not-saved evidence.
- Documents the required `Lifecycle Check`, `companion-lifecycle.csv`, and `companion-command-log.csv` smoke route before Core integration, persistence, or custom AI work.
- Does not change lifecycle behavior, actor ownership, `MarkedNotSaved` handling, persistence, reload restoration, auto-respawn, re-adoption, custom AI/pathing, target selection, attack UI, Core execution, taming, training, or loyalty.

## 0.1.41

- Closed the native dialogue decision gate.
- Formally keeps the plugin-owned Unity UI dialogue host as the supported Avalon Companions dialogue surface.
- Keeps the native `Companion` prompt as a runtime-only `AbstractLocationAction` entry point, not a true native `VDialogue` / `StoryBookmark` route.
- Real native story dialogue remains blocked until a later research gate proves safe game-authored companion story content, supported runtime story graph registration, or a native choice UI API that does not require story graph content.
- Does not add fake bookmarks, story graphs, dialogue attachments, template edits, save-owned interaction edits, custom AI/pathing, target selection, attack UI, persistence, progression, taming, training, loyalty, or Core-executed behavior.

## 0.1.40

- Fixed the profile-driven native assist smoke race where legacy follow/defend ticks could claim the first native-safe action before profile audit rows were written.
- While `Companions.EnableProfileDrivenNativeAssist=true`, legacy follow/defend ticks stand down and the profile loop owns the existing catch-up recall and native hero-pet ally defend prompt paths.
- Does not add custom target selection, movement override, target override elements, attack UI, persistence, progression, taming, training, loyalty, or Core-executed behavior.

## 0.1.39

- Hardened gated profile-driven native assist against immediate duplicate action rows.
- After `ai-profile-native-catch-up`, the regular creature follow tick is deferred by the existing follow tick interval.
- After `ai-profile-native-defend`, the regular creature defend tick is deferred by the existing defend tick interval.
- Does not add custom target selection, movement override, target override elements, attack UI, persistence, progression, taming, training, loyalty, or Core-executed behavior.

## 0.1.38

- Added gated profile-driven native assist through `Companions.EnableProfileDrivenNativeAssist`.
- `FollowCatchUpCandidate` can now route to the existing recall/catch-up placement path when the gate is enabled.
- `NativeCombatObserved` can now route to the existing native `NpcHeroPetAlly.EnterCombat()` path when the hero has live attackers and Defend/native-assist gates allow it.
- Adds command audit rows for `ai-profile-native-catch-up` and `ai-profile-native-defend`.
- Does not add custom target selection, movement override, target override elements, attack UI, persistence, progression, taming, training, loyalty, or Core-executed behavior.

## 0.1.37

- Added a debug-panel `AI Profile Check` button for the profile-audit coverage gate.
- When `Diagnostics.WriteCompanionAiProfileAudit=true`, the button writes one immediate `panel-ai-profile-check` snapshot to `companion-ai-profile.csv`.
- When the audit config is disabled, the button reports the blocked config state and records a blocked `ai-profile-check` command-log row.
- Does not dispatch commands, queue native refresh, change movement, change targets, persist actors, store progression state, execute behavior through Core, or add custom AI/progression systems.

## 0.1.36

- Reviewed the first live `companion-ai-profile.csv` smoke evidence set.
- Validated BepInEx load for deployed 0.1.35 with `WriteCompanionAiProfileAudit=True`.
- Reviewed 27 profile rows from `CampaignMap_HOS`, including 14 active `Sharg Candidate` rows.
- Confirmed `MovementBlocked`, `IdleNativePatrol`, and `NoActiveCompanion` intent output with `UNSAFE_FLAGS=0`.
- Does not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.

## 0.1.35

- Added runtime-only `CompanionAiProfile` and `CompanionAiIntent` framework contracts.
- Added a default-off read-only profile audit through `Diagnostics.WriteCompanionAiProfileAudit`.
- When enabled, the audit appends `companion-ai-profile.csv` rows for active managed companions and classifies observed native state as idle patrol, stay position, follow catch-up candidate, defend waiting, native combat observed, movement blocked, no targets, no active companion, or evidence insufficient.
- Profile audit rows explicitly keep command, movement, targeting, persistence, and Core execution flags false.
- Does not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.

## 0.1.34

- Reviewed the first live `companion-ai-boundary.csv` evidence set for the custom AI boundary gate.
- Recorded 276 reviewed rows, including 43 native combat rows and 64 rows with a live hero attacker, with `UNSAFE_FLAGS=0`.
- Added the first safe behavior design boundary: a future runtime-only `CompanionAiProfile` / `CompanionAiIntent` classifier may observe and audit native state, but must not execute behavior.
- Does not add custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, or Core-executed behavior.

## 0.1.33

- Added a debug-panel `AI Boundary Check` button for the AI boundary diagnostic smoke gate.
- The button is visible in the debug panel even when the diagnostic config is disabled, so the smoke gate has an obvious UI control.
- It writes one immediate `panel-ai-boundary-check` snapshot to `companion-ai-boundary.csv` only when `Diagnostics.WriteCompanionAiBoundaryDiagnostics=true`; otherwise it reports the blocked config state and records a blocked `ai-boundary-check` audit row in `companion-command-log.csv`.
- It does not dispatch companion commands, queue native follow/defend refresh, change movement, change targets, call native target recalculation, call `TargetOverrideElement.GetTarget`, persist actors, store progression state, or execute behavior through Avalon Core.

## 0.1.32

- Added a shared HUD primitive handoff for the passive companion badge when Tainted Interface exposes `DrawHudBadge`; the existing local draw path remains the fallback.
- Added `Diagnostics.WriteCompanionAiBoundaryDiagnostics`, default `false`.
- When enabled, the AI boundary diagnostic appends `companion-ai-boundary.csv` rows for active managed companions with native `NpcAI`, movement, unchecked target relation, relation counts, and `NpcCanMoveHandler` state.
- Diagnostic rows explicitly record `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.
- The diagnostic does not dispatch commands, change movement, change targets, call native target recalculation, call `TargetOverrideElement.GetTarget`, store progression state, persist actors, or execute behavior through Avalon Core.
- Custom AI, attack commands, target selection, taming, training, loyalty, persistence, and Core-executed behavior remain blocked until runtime diagnostic evidence is reviewed.

## 0.1.31

- Raised the passive top-left companion HUD badge so it sits just above the Wyrd Scent/status overlay lane instead of overlapping it.
- Keeps bottom-right placement and companion behavior unchanged.

## 0.1.30

- Polished the plugin-owned Unity UI companion command surface.
- Added explicit hover, selected, danger, and disabled row accents plus clearer disabled text coloring.
- Routed Unity button clicks and manual hit-test clicks through the same dialogue choice handler.
- Made Goodbye, Esc, debug-panel toggle, and post-command close use one close helper.
- Added dialogue audit rows that record command id, label, route, result, and close behavior in `companion-command-log.csv`.
- Does not add commands, change command behavior, add native story dialogue, custom AI/pathing, target selection, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.29

- Added a passive gameplay HUD badge for the active managed companion.
- The badge uses the existing per-companion icon mapping and the shared Tainted Interface `hud.companion-icon-background` Dragon asset.
- Added config for `Companions.ShowCompanionHudIcon`, `Companions.CompanionHudIconCorner`, and `Companions.CompanionHudIconSize`.
- The HUD lookup is throttled and cached; the overlay hides while the debug panel or custom dialogue is visible.
- Does not change spawn, command routing, native prompt behavior, dialogue layout/input, custom AI/pathing, target selection, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.28

- Mapped current companion roster families to expanded Tainted Interface icon IDs so the debug panel no longer uses one generic icon for most entries.
- Added UI-only mappings for Qrko pets, wolf, bear, deer, pig, cow, bullrat, corpse eater, redcap, flamegobbler, grindylow, wyrdspirit, sharg, ogre, zombie, drowner, skeleton, floatling, and tadpole selections.
- Kept `companion.creature` as the fallback for future unmapped entries.
- Requires Tainted Interface 0.1.2 for the expanded embedded catalog; the panel still falls back cleanly if the layer or icon texture is absent.
- Does not change spawn, command routing, native prompt behavior, dialogue layout/input, custom AI/pathing, target selection, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.27

- Added a dialogue-local direct pointer fallback for the vanilla-style Unity UI choices.
- Enabled visible choice rows are manually hit-tested on left-click and dispatched through the same existing command delegates used by `Button.onClick`.
- Added a compact log marker when the direct-click route dispatches: `Avalon Companions dialogue direct choice clicked`.
- Kept movement/look blocking, the IMGUI debug-panel input lock, and all companion command behavior unchanged.
- Does not add custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.25

- Fixed the vanilla-style Unity UI companion dialogue options being visible but not interactable.
- Rewired axis/button reads now pass through while the Unity UI dialogue is visible so FoA's EventSystem input path can drive button hover/click/submit.
- The IMGUI debug panel keeps the stricter input lock: Unity input modules disabled, input axes reset, and Rewired button/axis reads neutralized while the panel is open.
- Kept `PlayerInput.ProcessLateUpdate` blocked while the dialogue is open so movement/look remain frozen.
- Kept all dialogue choices routed through the existing approved command methods; no command behavior changed.
- Does not add custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.24

- Added an optional Tainted Interface icon bridge by reflection.
- The IMGUI debug panel now renders the selected roster entry through the shared icon catalog when Tainted Interface has the requested texture available.
- Started the shared icon consumer proof with companion catalog IDs such as `companion.pet`, `companion.wolf`, `companion.bear`, and `companion.creature`.
- Kept the existing fallback path when Tainted Interface or an icon texture is absent.
- Does not change companion command routing, native prompt behavior, dialogue layout, custom AI/pathing, target selection, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.23

- Changed the Unity UI companion dialogue host from a centered custom panel layout to a vanilla-style composition with right-side choices and a bottom dialogue text band.
- Removed leftover centered-panel helper code from the companion dialogue host path.
- Kept `Avalon Companions Debug` as the IMGUI debug/control panel on `KeypadPeriod`.
- Kept all dialogue choices routed through the existing approved command methods; no command behavior changed.
- Kept true vanilla `VDialogue`/StoryBookmark dialogue blocked until story graph registration is proven.
- Does not add custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.22

- Replaced the native `Companion` prompt's old IMGUI dialogue window with a plugin-owned Unity UI `Canvas` dialogue host.
- Kept `Avalon Companions Debug` as the IMGUI debug/control panel on `KeypadPeriod`.
- Kept all dialogue choices routed through the existing approved command methods; no command behavior changed.
- Kept true vanilla `VDialogue`/StoryBookmark dialogue blocked until story graph registration is proven.
- Does not add custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.21

- Added an optional Tainted Interface bridge by reflection.
- The custom dialogue interface and `Avalon Companions Debug` panel now prefer Tainted Interface shared dark-fantasy styles, texture-backed headers/panels/buttons, cursor helper, and custom UI input scope when that runtime is installed.
- Kept the existing local IMGUI style and FoA Mod Manager scope fallback when Tainted Interface is absent.
- Does not add commands, change command routing, alter companion ownership, add target selection, custom AI/pathing, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.20

- Added `Companions.EnableCustomDialogueInterface`, default `true`.
- Changed the runtime native prompt from `Companion Debug` back to `Companion`.
- Activating `Companion` now opens a plugin-owned custom dialogue command interface instead of the debug panel.
- The dialogue surface exposes Follow, Stay, Defend, Close/Normal/Far range, Come Close, Recall, Recover, Dismiss, and close choices without cycling native quick commands.
- Kept `KeypadPeriod` and `Debug.EnableDebugPanel` as the separate debug-panel path.
- Shared cursor/input locking across the custom dialogue and debug panel.
- Does not add true native story dialogue, StoryBookmarks, story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.19

- Reframed the IMGUI companion panel as a temporary debug panel.
- Added `Debug.EnableDebugPanel`, default `true`, to gate the debug panel and native debug-prompt bridge while native dialogue is researched.
- Renamed the runtime native prompt bridge from `Companion` to `Companion Debug`.
- Recorded the roadmap order: debug panel, native custom dialogue proof, custom AI logic, then taming/training/loyalty systems.
- Does not add native dialogue/story graphs, custom AI/pathing, target scans, attack commands, taming, training, loyalty, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or Avalon Core runtime behavior.

## 0.1.18

- Added an optional Avalon Core diagnostic bridge.
- The bridge reads `AvalonCore.Plugin.TrustReports` by reflection when Core is installed and logs one compact read-only summary.
- The bridge fails closed with `action=none` when Core is absent, the API shape is missing, counters are invalid, or `WouldMutateRuntime=true`.
- Does not add an Avalon Core dependency, adapter registration, companion-profile registration, Core registry query, callbacks, custom AI/pathing, target scans, attack commands, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or dialogue graphs.

## 0.1.17

- Recorded the Avalon Core handoff design boundary for the companion framework.
- Added a companion-local decision record for the future diagnostic/planning-only Core lane.
- Kept Avalon Core optional and did not add runtime descriptor registration, companion-profile registration, callbacks, or behavior changes.
- Does not add custom AI/pathing, target scans, attack commands, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or dialogue graphs.

## 0.1.16

- Started companion framework polish.
- Moved internal command, mode, follow-range, recovery-state, and roster-entry contract types into `AvalonCompanions.Framework`.
- Runtime behavior is intended to be unchanged from 0.1.15.
- Does not add Avalon Core integration yet, custom AI/pathing, target scans, attack commands, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or dialogue graphs.

## 0.1.15

- Added responsive native behavior polish inside the existing one-session companion path.
- Explicit summon/swap, recall, Come Close, range, mode, recovery, lifecycle-check, and native command entry paths now queue the existing follow catch-up and native defend ticks for immediate evaluation.
- The refresh does not clear per-companion defend prompt cooldowns, so repeated `NpcHeroPetAlly.EnterCombat()` prompts remain throttled.
- Does not add custom AI/pathing, target scans, attack commands, defend hotkeys, healing, resurrection, respawn, persistence, reload restoration, re-adoption, active squads, humanoid companions, or dialogue graphs.

## 0.1.14

- Added metadata/status-only continuity for the approved session-continuity slice.
- Added plugin-owned `Continuity.RememberedCompanionGuid`, `Continuity.RememberedCompanionName`, `Continuity.RememberedCommandMode`, and `Continuity.RememberedFollowRange` config entries.
- The command panel now reports remembered companion metadata when no managed actor is active and tells the player to use the existing `Summon / Swap` path.
- Command CSV rows can record `continuity-status` decisions with `touchesTargeting=false` and `touchesPersistence=false`.
- Does not add actor persistence, reload restoration, auto-respawn, same-session actor re-adoption after reload, healing, resurrection, active squads, custom AI/pathing, attack commands, target selection, humanoid companions, or dialogue graphs.

## 0.1.13

- Added managed companion recovery inside the existing one-session native ally path.
- Added a `Recover` panel command and fallback native quick-command action.
- Recovery runs a lifecycle precheck, removes dead or invalid managed actors when the lifecycle guard is enabled, and recalls live managed actors beyond the Far catch-up threshold through the existing verified placement path.
- Command CSV rows now record recovery counts for inspected, recalled, removed, invalid, dead, and guard-blocked companions.
- Does not add healing, resurrection, respawn, persistence, active squads, custom AI/pathing, attack commands, target selection, humanoid companions, or dialogue graphs.

## 0.1.12

- Added lifecycle validation polish to the Avalon command panel.
- The panel now shows the latest lifecycle pass summary.
- Added a panel-only `Lifecycle Check` diagnostic button that runs the existing lifecycle safety pass with a forced CSV snapshot.
- Does not add persistence, reload restoration, active squads, travel hooks, attack commands, target selection, healing, humanoid companions, or dialogue graphs.

## 0.1.11

- Added command polish to the Avalon command panel.
- The panel now shows per-active-companion status: distance, mode state, native ally marker, native prompt state, and not-saved state.
- Follow, Stay, and Defend mode feedback now explains catch-up, waiting-for-attacker, and native ally behavior more clearly.
- Does not add attack commands, target selection, custom AI/pathing, healing, persistence, humanoid companions, dialogue graphs, or squads.

## 0.1.10

- Added advanced native ally behavior while staying inside the approved `NpcHeroPetAlly` path.
- Native defend now counts live hero attackers, logs defend armed/clear transitions, and throttles repeated automatic `EnterCombat()` prompts per managed companion.
- Automatic catch-up command log rows now include distance, threshold, range, and slot details.
- Does not add attack commands, target selection, custom AI/pathing, healing, persistence, humanoid companions, dialogue graphs, or squads.

## 0.1.9

- Added `Companions.EnableNativeCommandMenu`, default `true`.
- Managed one-session companions now receive one runtime-only native `Companion` prompt by default.
- Activating `Companion` opens the Avalon command menu with Follow, Stay, Defend, Come Close, Recall, and Dismiss.
- Rotating native quick-command actions remain fallback-only when `Companions.EnableNativeCommandMenu=false`.
- Does not add dialogue/story graphs, template edits, save-owned interaction lists, attack commands, target selection, healing, persistence, or squads.

## 0.1.8

- Reset the native quick-command cursor before `Dismiss` removes a companion and after successful summon/swap.
- Documented the next command-surface direction: one native `Companion` prompt opening a proper runtime-only command menu.
- Kept direct vanilla pet/dialogue menu conversion blocked until `PetVariantBase`, `PetTalkAction`, `DialogueAction`, story graph/bookmark, and save-owned interaction safety are separately proven.
- Does not add dialogue/story graphs, template edits, save-owned interaction lists, attack commands, target selection, healing, persistence, or squads.

## 0.1.7

- Changed native quick-command availability so only one quick action is available at a time.
- Added a native quick-command cursor that rotates through `Follow`, `Stay`, `Defend`, `Come Close`, `Recall`, and `Dismiss`, skipping a mode command that is already active.
- Reason: user smoke testing showed FoA still starts the first/default available quick action, so separate available quick actions only surfaced Follow/Stay.
- Does not add dialogue/story graphs, template edits, save-owned interaction lists, attack commands, target selection, healing, persistence, or squads.

## 0.1.6

- Changed native quick-command mode so managed one-session companions receive quick-command actions instead of the older `Companion` panel bridge.
- Kept the `Companion` panel bridge as the fallback when `Companions.EnableNativeQuickCommands=false`.
- Split panel-bridge and quick-command availability checks so disabling the panel action does not disable the quick commands.
- Reason: user smoke testing showed FoA starts the first/default action, so keeping the panel bridge alongside quick commands opened the panel instead of running native commands.
- Does not add dialogue/story graphs, template edits, save-owned interaction lists, attack commands, target selection, healing, persistence, or squads.

## 0.1.5

- Added `Companions.EnableNativeQuickCommands`, default `true`.
- Managed one-session companions now receive runtime-only native quick-command action elements for `Follow`, `Stay`, `Defend`, `Come Close`, `Recall`, and `Dismiss`.
- Kept the existing `Companion` native action as the panel bridge.
- Native quick commands reuse the same panel command paths and command CSV logging.
- Does not add dialogue/story graphs, template edits, save-owned interaction lists, attack commands, target selection, healing, persistence, or squads.

## 0.1.4

- Renamed the visible `Heel` button to `Come Close` to avoid confusion with a healing command. The command still logs as `heel` for CSV continuity.
- Fixed close-recall safety for one-session creature candidates by keeping their Close placement at the previously tuned Normal offset instead of shrinking it toward the hero.
- Runs recall target positions through `BaseLocationSpawner.VerifyPosition(..., allowSnapToGround: true)` before moving managed companions.
- Does not add healing, custom pathing, target selection, attack commands, persistence, or squads.

## 0.1.3

- Added `Heel`, which switches managed companions to Follow mode, selects Close follow range, and recalls active managed roster companions near the hero.
- Added `Companions.FollowRangeProfile` with Close, Normal, and Far placement/catch-up profiles.
- Added range buttons to the companion panel and status text showing the current follow range.
- Added `Diagnostics.WriteCompanionCommandLog`, writing `companion-command-log.csv` rows for summon, swap, recall, heel, range, mode, dismiss, blocked, and auto catch-up decisions.
- Kept advanced commands inside the existing managed roster path: no manual attack button, custom target selection, persistence, active squads, or humanoid companion behavior.

## 0.1.2

- Added `Companions.EnableLifecycleSafetyGuard` for transition/save safety checks.
- Added `Diagnostics.WriteCompanionLifecycleDump`, writing `companion-lifecycle.csv` for summon, recall, dismiss, scene-change, long hero move, shutdown, duplicate, orphan, and not-saved validation.
- Re-marks managed roster actors not saved during lifecycle passes.
- Detects live roster `NpcHeroPetAlly` parents even when they are not in Avalon's in-memory tracker.
- Discards untracked one-session roster allies and excess active roster actors when the lifecycle guard is enabled.
- Does not add persistence, reload respawn, companion restoration, custom travel behavior, or active squads.

## 0.1.1

- Added the full clean FOA-Diagnostic Tool 0.4.3 `ReviewQueueNonUniqueSpawnerBacked` animal/offensive/undead candidate set to the roster.
- Added `Spec_AnimalBear_Interactions`, T1 corpse eater/redcap/flamegobbler/grindylow/wyrdspirit, Sharg HoS, ogre, zombie/drowner/skeleton rows, floatling, and tadpole hatchery as explicit one-session candidates.
- Kept all new rows on the existing native summon-faction plus `NpcHeroPetAlly` path: explicit summon only, marked not saved, runtime tracked only, recall/dismiss/catch-up, and native defend entry only when the hero has live attackers.
- Expanded default pet/creature diagnostic terms for the same creature families.
- Did not add persistence, custom target selection, attack commands, defend hotkeys, wild conversion, humanoid companions, or save/load support.

## 0.1.0

- Added the initial Avalon Companions research-gated scaffold.
- Added plugin identity, config entries, reference validation, and inert patch registry.
- Documented that companion runtime behavior is blocked until follower, actor lifecycle, and save/load evidence exists.
- Added local pet/summon target research identifying diagnostics as the next safe code step.
- Added disabled-by-default pet/summon diagnostics for current pet/summon counts, pet base reference availability, Qrko mount template count, and loaded `Spec_Pet_*` candidates.
- Added disabled-by-default GUID-backed `Spec_Pet_*` template CSV output for taxonomy review; rows are explicitly marked as not approved spawn candidates.
- Added disabled-by-default throwaway-save Qrko pet prototype behind `ResearchModeOnly`, prototype enable, and hotkey gates.
- Expanded the prototype into a Qrko pet companion roster with four validated pet templates, summon/swap, next/previous selection, recall, dismiss, follow/stay controls, and explicit panel command modes.
- Added a styled in-game pet command panel opened with `KeypadPeriod`.
- Added disabled-by-default pet/creature `LocationTemplate` candidate and shortlist CSV dumps using loaded scene spawner evidence. These rows are diagnostics-only and keep `safeSpawnCandidate=false` and `rosterApproved=false`.
- Added diagnostic-only FOA-Diagnostic Tool crosscheck CSVs so Avalon can report when the external `spawner_refs.csv` sees pet/creature evidence that Avalon's once-per-session scan missed.
- Added `Diagnostics.DiagnosticToolMinimumSpawnerRefs` so the crosscheck skips low-context FOA-Diagnostic Tool startup dumps by default.
- Added wolf and bear one-session creature candidates with the native hero pet ally marker.
- Added native wolf/bear defend assist that prompts managed `NpcHeroPetAlly` candidates to enter combat when the hero has live attackers.
- Reworked the companion command panel with the FoA Mod Manager responsive/clamped IMGUI window pattern so it is readable on high-resolution displays and draggable by its header.
- Added explicit panel Follow, Stay, and Defend mode buttons. Defend mode uses the same native hero-attacker path as the automatic defend fallback and does not add custom target selection.
- Matched the FoA Mod Manager cursor lifecycle by re-enforcing cursor unlock and input-module blocking immediately before drawing the companion IMGUI panel.
- Added deer, pig, cow, and bullrat one-session candidates from reviewed live diagnostic evidence. Passive animals use the existing native ally path for follow/recall/dismiss; bullrat is the new offensive creature candidate. None of these add persistence, custom targeting, or save/load behavior.
