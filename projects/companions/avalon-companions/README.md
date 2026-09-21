# Avalon Companions

Research-gated foundation for a future Tainted Grail: The Fall of Avalon companion or follower mod.

## Status

Version 0.2.12 is a research-gated pet companion scaffold. It loads as a BepInEx v5 Mono plugin, creates conservative config entries, can optionally log a one-shot pet/summon system snapshot, can write GUID-backed pet template and pet/creature shortlist CSVs, contains a pet/animal/creature roster plus one-session candidates that use the game's native hero pet ally and attack-response path where supported, adds a lifecycle guard/dump for transition and save validation, and adds safe managed-command ergonomics through one native `Companion` prompt opening a supported plugin-owned vanilla-style Unity UI dialogue host with optional shared companion portrait/background art, a separate polished IMGUI roster/summon/debug panel with optional per-companion shared Tainted Interface icons, an optional passive gameplay HUD companion icon overlay through Tainted Interface's shared HUD badge primitive when available, optional Tainted Interface/FoA Mod Manager input scope, Come Close/Heel, follow range profiles, throttled native defend prompting, per-companion debug status, lifecycle checkpoint feedback, managed recovery, metadata/status-only continuity, responsive native refresh scheduling, extracted framework contract types, Core handoff design notes, an optional read-only Avalon Core diagnostic bridge, a default-off read-only native AI/movement/target-state diagnostic with a manual debug-panel snapshot button, a 0.1.34 AI boundary evidence review plus first safe behavior design note, a default-off read-only `CompanionAiProfile` / `CompanionAiIntent` audit classifier, a 0.1.36 smoke/evidence review for that audit classifier, a 0.1.37 manual profile-audit smoke aid, a 0.1.38 gated profile-driven native assist bridge, 0.1.39 profile-assist de-duplication, 0.1.40 profile-assist smoke race fix, a 0.1.41 native dialogue decision that keeps true `VDialogue` / `StoryBookmark` integration blocked, a 0.1.42 lifecycle validation gate route, 0.2.x runtime profile and command policy groundwork, 0.2.3 runtime trust/loyalty profile state, 0.2.5 bond-policy command timing modifiers, 0.2.6 summon-panel polish, 0.2.7 dialogue portrait/background polish, 0.2.8 bond UI clarity, 0.2.9 progression persistence gate diagnostics, 0.2.10 profile-only trust/loyalty/bond storage, 0.2.11 read-only taming eligibility gate diagnostics, 0.2.12 read-only live taming encounter evidence diagnostics, debug-panel roadmap notes, and command audit logging.

The current companion lane is pets, not humanoid followers. Humanoid combat companions still need separate actor, faction, quest, and persistence research before implementation.

## Install

Requires the PC Mono branch with BepInEx v5 Mono installed.

Extract the archive so the `plugins` folder merges into `BepInEx/plugins`:

```text
BepInEx/
  plugins/
    AvalonCompanions/
      AvalonCompanions.dll
      README.md
      CHANGELOG.md
```

On first launch, the config is generated at:

```text
BepInEx/config/kane.tgfoa.avalon-companions.cfg
```

## Current scope

- Establish the mod folder, plugin identity, and release metadata.
- Record the research boundary for future companion behavior.
- Keep humanoid companion behavior disabled.
- Provide disabled-by-default pet/summon diagnostics through `Diagnostics.LogPetSystemDiagnostics`.
- Provide a disabled-by-default GUID-backed template taxonomy dump through `Diagnostics.WritePetTemplateDump`.
- Provide disabled-by-default scene-spawner-backed pet/creature candidate dumps through `Diagnostics.WritePetCreatureShortlistDump`.
- Provide diagnostic-only FOA-Diagnostic Tool crosscheck dumps through `Diagnostics.WritePetCreatureDiagnosticToolCrosscheck`.
- Provide a pet/animal companion roster through `Companions.EnablePetCompanionRoster`.
- Provide the reviewed `ReviewQueueNonUniqueSpawnerBacked` animal, offensive creature, and undead one-session candidates through the same roster. They spawn only by explicit command, must be marked as native hero pet allies, respond through the native ally combat path when the hero has attackers where supported, and are discarded if the ally marker cannot be applied.
- Provide `Companions.EnableLifecycleSafetyGuard` and `Diagnostics.WriteCompanionLifecycleDump` for transition, rest, quit/reload, duplicate, orphan, and not-saved validation.
- Provide `Come Close`/`Heel`, `Companions.FollowRangeProfile`, and `Diagnostics.WriteCompanionCommandLog` for managed-command validation.
- Provide `Debug.EnableDebugPanel` for the temporary IMGUI debug panel while native dialogue is researched.
- Provide `Companions.EnableNativeCommandMenu` for one runtime-only native `Companion` prompt on managed one-session companions.
- Provide `Companions.EnableCustomDialogueInterface` for the plugin-owned custom dialogue command surface opened by the `Companion` prompt.
- Keep `Companions.EnableNativeQuickCommands` as a fallback-only rotating world-prompt mode when `Companions.EnableNativeCommandMenu=false`.
- Provide 0.1.10 native ally behavior coordination: count live hero attackers, log defend armed/clear transitions, throttle repeated native `NpcHeroPetAlly.EnterCombat()` prompts per managed companion, and keep all targeting decisions inside the native ally path.
- Provide 0.1.11 command polish: per-active-companion panel status and clearer Follow/Stay/Defend feedback without adding any new commands.
- Provide 0.1.12 lifecycle validation polish: panel lifecycle summary plus a `Lifecycle Check` diagnostic button that forces the existing lifecycle pass and CSV snapshot.
- Provide 0.1.13 managed recovery: a `Recover` command that removes dead/invalid managed one-session companions and recalls live out-of-range managed companions through the existing verified placement path.
- Provide 0.1.14 metadata/status continuity: remember last selected companion GUID/name, command mode, and follow range in plugin-owned config, show remembered status when no managed actor is active, and keep `Summon / Swap` as the only actor creation path.
- Provide 0.1.15 responsive native polish: explicit companion commands queue the existing follow catch-up and native defend checks for immediate evaluation without clearing defend prompt cooldowns.
- Provide 0.1.16 framework polish: internal companion command/mode/range/recovery and roster-entry contracts are extracted for future Avalon Core routing without changing runtime behavior.
- Provide 0.1.17 Core handoff design: document the companion-to-Core boundary and future diagnostic/planning-only descriptor shape without adding an Avalon Core dependency or runtime registration.
- Provide 0.1.18 Core diagnostic bridge: optionally read Avalon Core host trust reports by reflection and log a read-only summary without adding a dependency, descriptor registration, companion-profile registration, or runtime behavior.
- Provide 0.1.19 debug-panel roadmap polish: label the IMGUI surface and native bridge as debug, keep it behind `Debug.EnableDebugPanel`, and make native custom dialogue the next research gate before custom AI, taming, training, or loyalty systems.
- Provide 0.1.20 custom dialogue interface: the native `Companion` prompt opens a plugin-owned command-choice surface with the approved commands visible at once, while `KeypadPeriod` remains debug-panel-only.
- Provide 0.1.21 shared UI shell integration: when Tainted Interface is installed, the custom dialogue and debug panel use its shared dark-fantasy styles, texture-backed headers/panels/buttons, cursor helper, and input scope; when absent, the existing local IMGUI fallback remains active.
- Provide 0.1.22 dialogue-host correction: the native `Companion` prompt now opens a Unity UI `Canvas` dialogue host instead of the old IMGUI dialogue window; the IMGUI surface remains debug-panel-only.
- Provide 0.1.23 dialogue layout correction: the custom Unity UI host now uses a vanilla-style composition with right-side choices and a bottom dialogue text band instead of a centered panel.
- Provide 0.1.24 shared icon consumer proof: when Tainted Interface is installed, the debug panel can show the selected companion icon from shared IDs such as `companion.wolf`.
- Provide 0.1.25 dialogue input correction: Rewired axis/button reads are allowed while the Unity UI dialogue is visible so EventSystem-driven options can click, while the IMGUI debug panel keeps stricter Rewired neutralization.
- Provide 0.1.27 dialogue direct-click fallback: if FoA's EventSystem still does not deliver `Button.onClick`, the dialogue host manually hit-tests enabled choice rows and dispatches the same approved commands.
- Provide 0.1.28 per-companion icon mapping: existing debug-panel roster families request expanded Tainted Interface icon IDs where available, with `companion.creature` retained only as the unmapped fallback.
- Provide 0.1.29 gameplay HUD icon overlay: active managed companions can show a passive top-left or bottom-right badge using the existing mapped icon and Tainted Interface's Dragon background ID.
- Provide 0.1.30 command-surface polish: dialogue choices have clearer hover, selected, disabled, and danger presentation; Goodbye/Esc/command close routes are consistent; and dialogue audit rows record surface, command id, route, result, and close behavior.
- Provide 0.1.31 HUD placement hotfix: the top-left companion badge is raised above the Wyrd Scent/status overlay lane.
- Provide 0.1.32 shared HUD primitive handoff: the passive companion badge calls Tainted Interface's reusable `DrawHudBadge` API first and keeps the old local texture drawing only as a fallback for older UI Layer builds.
- Provide 0.1.32 companion AI boundary diagnostics: `Diagnostics.WriteCompanionAiBoundaryDiagnostics=false` by default can append `companion-ai-boundary.csv` rows for active managed companions' native AI, movement, current unchecked target relation, target-relation counts, and native movement blockers without dispatching commands or changing AI state.
- Provide 0.1.33 AI boundary smoke aid: the debug panel exposes `AI Boundary Check` as a visible smoke control; it writes one immediate read-only snapshot only when the AI boundary diagnostic config is enabled and otherwise reports the blocked config state.
- Provide 0.1.34 AI boundary evidence review and first safe behavior design: reviewed 276 live AI boundary rows with zero unsafe mutation/Core flags, captured native combat-adjacent evidence through the existing ally path, and defined only a future non-mutating profile/intent classifier design.
- Provide 0.1.35 AI profile/intent audit: `Diagnostics.WriteCompanionAiProfileAudit=false` by default can append `companion-ai-profile.csv` rows that classify active managed companion state as `IdleNativePatrol`, `StayPosition`, `FollowCatchUpCandidate`, `DefendWaitingForThreat`, `NativeCombatObserved`, `MovementBlocked`, `NoTargets`, `EvidenceInsufficient`, or `NoActiveCompanion` without executing behavior.
- Provide 0.1.36 AI profile audit smoke review: validated 0.1.35 BepInEx load with profile audit enabled, reviewed 27 `companion-ai-profile.csv` rows, confirmed active `Sharg Candidate` rows with `MovementBlocked` and `IdleNativePatrol`, and found zero unsafe mutation/Core flags.
- Provide 0.1.37 AI profile smoke aid: the debug panel exposes `AI Profile Check`; it writes one immediate `panel-ai-profile-check` row only when the profile audit config is enabled and otherwise reports the blocked config state.
- Provide 0.1.38 profile-driven native assist: `Companions.EnableProfileDrivenNativeAssist=false` by default can route `FollowCatchUpCandidate` to the existing recall/catch-up path and `NativeCombatObserved` to the existing native ally defend prompt when live hero attackers exist.
- Provide 0.1.39 profile-assist hardening: after profile assist performs catch-up or native defend, the matching legacy follow/defend tick is deferred to avoid immediate duplicate action rows.
- Provide 0.1.40 profile-assist smoke fix: when profile-driven native assist is enabled, the profile loop owns the existing catch-up and native defend paths so legacy ticks cannot claim the first action before profile audit rows are written.
- Provide 0.1.41 native dialogue decision: the plugin-owned Unity UI dialogue host is the supported companion dialogue surface; true native `VDialogue` / `StoryBookmark` integration remains blocked until a safe story content route is proven.
- Provide 0.1.42 lifecycle validation gate: define the throwaway-save route for transition, rest, quit/reload, return, duplicates, orphan cleanup, and not-saved validation without changing lifecycle behavior.
- Provide 0.2.3 runtime trust/loyalty foundation: `Companions.EnableCompanionTrustRuntime=true` tracks in-memory trust and loyalty scores from existing approved companion command/runtime events, shows status in the debug panel and companion dialogue subtitle, and can write `companion-trust` rows to `companion-command-log.csv` through `Diagnostics.WriteCompanionTrustLog=true`. It does not persist, restore, tame, train, target, move, or execute behavior through Avalon Core.
- Provide 0.2.5 bond policy runtime: `Companions.EnableCompanionBondPolicyRuntime=true` lets runtime bond level adjust Avalon-owned catch-up interval, catch-up threshold, native defend prompt cooldown, automatic defend assist availability, and trust gain/penalty scaling. It does not add native stat buffs, target overrides, movement overrides, persistence, taming, training, saved progression, or Core-executed behavior.
- Provide 0.2.6 summon-panel polish: the `KeypadPeriod` panel focuses on roster selection, `Summon / Swap`, active companion safety controls, lifecycle validation, and AI diagnostics; duplicated order controls remain on the supported `Companion` dialogue surface.
- Provide 0.2.7 dialogue portrait/background polish: the supported Unity UI `Companion` dialogue can show the active companion's existing shared icon with the existing shared HUD background frame when Tainted Interface provides those textures. It does not change command behavior.
- Provide 0.2.8 bond UI clarity: the supported dialogue and roster panel show effective bond, trust score, loyalty score, and the current bond-policy effect phrase without changing any command behavior.
- Provide 0.2.9 progression persistence gate diagnostics: the roster panel exposes a read-only `Progression Gate` button that records which trust/loyalty/bond profile fields may later be saved and which actor/save-ownership fields remain blocked.
- Provide 0.2.10 profile-only progression storage: `Companions.EnableCompanionProfileStore=true` saves approved trust, loyalty, last trust event, last mode/range, and effective bond profile fields to `companion-profiles.tsv` keyed by template GUID plus review ID. It does not restore actors, auto-respawn, re-adopt live actors, save locations/health/AI/target state, take save ownership, or change command behavior.
- Provide 0.2.11 taming eligibility gate diagnostics: `CompanionTamingProfile` / `CompanionTamingEligibility` classify reviewed roster entries only, and the roster panel `Taming Gate` writes one read-only `taming-eligibility-gate` command-log row. It does not capture, adopt wild actors, persist actors, restore actors, or change command behavior.
- Provide 0.2.12 live taming encounter evidence diagnostics: the roster panel `Encounter Probe` writes read-only `companion-taming-encounter.csv` rows for nearby reviewed wolf/bear live actors, recording template identity, hostility/target state, faction/ownership hints, AI/movement state, and save flags. It does not capture, adopt, attach ally markers, change faction, spawn, dismiss, persist, move, target, or change command behavior.

## Out of scope for 0.2.12

- Humanoid NPC spawning or cloning.
- Humanoid companion combat AI.
- Humanoid follow or teleport behavior.
- Dialogue, recruitment, or quest state.
- Saved actor data or persistent actor records.
- Reload respawn or companion restoration.
- Multi-companion active squads.
- True native game `VDialogue`/story-graph companion UI.
- Manual attack buttons, arbitrary target selection, or custom faction aggression logic.
- Healing commands or health restoration.
- Resurrection or automatic respawn.
- True dialogue/story graph companion menus.
- Runtime-authored native custom dialogue.
- Custom AI logic.
- Capture/taming commands, training, perk trees, inventories, actor-owned progression, or behavior changes from saved affinity.
- Avalon Core runtime dependency, adapter registration, companion-profile registration, registry queries, runtime callbacks, or Core-executed behavior.
- Any companion behavior change beyond the custom dialogue layout, dialogue click/input fixes, UI shell, style source, icon display, input-scope bridge, passive HUD primitive handoff, read-only boundary diagnostics, read-only profile/intent audit evidence, existing gated profile assist, bond-policy timing modifiers, profile-only progression storage, read-only taming eligibility audit, and read-only live taming encounter evidence probe.

## Pet companions

The pet roster uses four validated regular non-abstract `LocationTemplate` GUIDs:

- `Spec_Pet_Qrko`
- `Spec_Pet_Qrko_02`
- `Spec_Pet_Qrko_03`
- `Spec_Pet_Qrko_05`

The same roster also exposes reviewed one-session animal/creature candidates:

- `Spec_AnimalWolf`
- `Spec_AnimalBear`
- `Spec_AnimalBear_Interactions`
- `Spec_AnimalDeer`
- `Spec_AnimalPig`
- `Spec_AnimalCow`
- `Spec_EnemyMonster_T1_CorpseEater`
- `Spec_EnemyMonster_T1_Redcap`
- `Spec_EnemyMonster_T1_Flamegobbler`
- `Spec_EnemyMonster_T1_Grindylow`
- `Spec_EnemyMonster_T1_Wyrdspirit`
- `Spec_EnemyMonster_T2_ShargHoS`
- `Spec_EnemyMonster_T3_Ogre`
- `Spec_EnemyMonster_T4_Bullrat`
- `Spec_EnemyZombie_T1_Classic`
- `Spec_EnemyZombie_T1_ClassicHarder`
- `Spec_EnemyZombie_T1_Drowner`
- `Spec_EnemyZombie_T1_DrownerFullArmor`
- `Spec_EnemyZombie_T1_DrownerHeadArmor`
- `Spec_EnemySkeleton_Melee1H`
- `Spec_EnemySkeleton_Melee2H`
- `Spec_SoS_EnemyMonster_T4_Floatling_WithSpawnAnimation`
- `Spec_SoS_EnemyMonster_T4_TadpoleHatchery`

Animal/creature candidates are spawned only by explicit player command, marked not saved, converted through the native summon-faction plus `NpcHeroPetAlly` path used by FoA's `NpcAllyPetVariant`, and tracked only in memory for the current session. The panel has explicit Follow, Stay, and Defend modes. Defend mode checks whether the hero already has live attackers and prompts the managed `NpcHeroPetAlly` to enter native combat; when `Companions.EnableNativeDefendAssist=true`, the same native entry path also runs as the automatic fallback outside Defend mode. Version 0.1.10 counts live hero attackers, logs defend armed/clear transitions, and throttles repeated native defend prompts per managed companion so Avalon does not call `EnterCombat()` every defend tick. Passive animals may not have useful combat response. Offensive and undead candidates still use only the native hero-attacker path. Avalon does not choose targets directly. Custom target selection, attack commands, defend hotkeys, and actor persistence remain out of scope.

0.1.15 queues the existing follow catch-up and native defend checks immediately after explicit companion commands. This makes summon/swap, recall, Come Close, range changes, Follow/Stay/Defend, managed recovery, lifecycle check, and native command entry feel more responsive without changing the underlying native behavior. Per-companion native defend prompt cooldowns are not cleared by this refresh.

0.1.4 exposes the close recall command as `Come Close` to avoid confusion with healing. Internally it still logs as `heel`. Close range does not shrink one-session creature candidate placement below their tuned normal offsets, and recall targets are verified through the game's native placement verifier before moving actors. Range affects Avalon recall placement and one-session candidate catch-up distance only; it does not add custom pathing, healing, or save-owned follower behavior.

Controls:

- `KeypadPeriod`: open or close the debug panel.
- `KeypadPlus`: summon or swap to the selected pet.
- `KeypadMultiply`: select next pet.
- `KeypadDivide`: select previous pet.
- `KeypadEnter`: recall active managed pet companions.
- `KeypadMinus`: dismiss active managed pet companions.
- `Keypad0`: toggle active managed pet companions between follow and stay.

The `KeypadPeriod` panel shows the selected roster entry, active companion, trust/bond policy, lifecycle summary, per-active-companion status, roster navigation, `Summon / Swap`, active safety controls, and diagnostics. `Lifecycle Check` forces the existing lifecycle safety pass with a CSV snapshot so fast travel, area transition, rest, quit, reload, and return can be reviewed. `Taming Gate` writes roster-only eligibility evidence. `Encounter Probe` writes read-only live wolf/bear encounter evidence to `companion-taming-encounter.csv` so template identity, hostility/target state, faction/ownership hints, AI/movement state, and save flags can be reviewed before any tame command. `Recover Active` runs that same lifecycle precheck, removes dead or invalid managed actors when the lifecycle safety guard is enabled, and recalls live managed actors that are beyond the Far catch-up threshold. It does not heal, resurrect, respawn, or persist companion actors. `Dismiss Active` keeps the existing one-session cleanup path. Companion orders such as Follow, Hold Position, Defend, range, Come Close, and Recall are on the supported `Companion` dialogue surface instead of the summon panel. The roster refuses to touch non-roster pets. Managed pet companions are marked not saved until actor save/persistence behavior is intentionally designed.

`Companions.EnableNativeCommandMenu` and `Companions.EnableCustomDialogueInterface` default to `true`. Managed one-session companions receive one runtime-only native action labelled `Companion`, and activating it opens the supported plugin-owned vanilla-style Unity UI dialogue host with right-side Follow, Stay, Defend, Close/Normal/Far range, Come Close, Recall, Recover, Dismiss, and close choices plus a bottom dialogue text band. When Tainted Interface supplies shared textures, the dialogue text band can also show the active companion's roster icon over the shared companion HUD background. `Companions.EnableNativeQuickCommands` remains available only as a fallback when `Companions.EnableNativeCommandMenu=false`. Version 0.1.41 formally keeps this Unity UI host as the supported companion dialogue surface. Directly converting vanilla pet/dialogue menus is not approved because vanilla pet commands depend on `PetVariantBase` and true dialogue choices require valid game-authored or safely registered story graph/bookmark data.

The `KeypadPeriod` debug panel is still available when `Debug.EnableDebugPanel=true`. It is a development and validation surface, not the normal companion command UX.

## Continuity metadata

0.1.14 stores only plugin-owned metadata under the BepInEx config file:

- `Continuity.RememberedCompanionGuid`
- `Continuity.RememberedCompanionName`
- `Continuity.RememberedCommandMode`
- `Continuity.RememberedFollowRange`

When no managed companion actor is active, the panel can report the remembered companion and tells the player to use `Summon / Swap`. That command still uses the existing explicit one-session summon path. The continuity metadata does not restore, respawn, persist, save, heal, resurrect, or re-adopt actors.

## Lifecycle gate

`Companions.EnableLifecycleSafetyGuard` defaults to `true`. When the roster is enabled and the runtime gate is open, the guard marks managed roster actors not saved, watches scene changes and long hero moves, resolves live `NpcHeroPetAlly` parents, discards untracked one-session roster allies, and discards excess active roster actors. The panel `Lifecycle Check` button runs this same guard manually with a forced CSV snapshot. This prevents transition/reload leftovers from becoming persistent companions. It does not restore companions after reload and does not add save ownership.

`Diagnostics.WriteCompanionLifecycleDump` defaults to `true`. It appends `companion-lifecycle.csv` under this plugin's BepInEx config folder. Use that file for fast travel, area transition, rest, quit/reload, duplicate, and orphan checks.

`Diagnostics.WriteCompanionCommandLog` defaults to `true`. It appends `companion-command-log.csv` under this plugin's BepInEx config folder. Use that file to confirm which command ran, why it was blocked, why an automatic catch-up recall happened, when native defend armed/cleared, what the managed recovery command inspected, recalled, removed, or blocked, or when metadata/status continuity reported a remembered companion. The command log is audit output only; it does not approve custom targeting, persistence, active squads, healing, resurrection, respawn, or attack commands.

## AI boundary diagnostics

`Diagnostics.WriteCompanionAiBoundaryDiagnostics` defaults to `false`. When enabled, Avalon Companions samples active managed companions at `Diagnostics.CompanionAiBoundaryDiagnosticSeconds` intervals and appends `companion-ai-boundary.csv` under this plugin's BepInEx config folder.

Rows record scene/hero context, active roster count, companion template/name/position, not-saved and native ally markers, `NpcAI` working/combat/alert/visibility state, unchecked current target relation, possible target/attacker counts, current `NpcMovement` state, `NpcCanMoveHandler` blocker state, and whether native target override elements are present. The diagnostic explicitly records `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.

The debug panel also exposes `AI Boundary Check`, `AI Profile Check`, `Progression Gate`, and `Taming Gate` beside `Lifecycle Check`. These buttons stay visible for smoke validation. When the boundary diagnostic config is enabled, `AI Boundary Check` writes one immediate `panel-ai-boundary-check` snapshot for active managed companions and adds an `ai-boundary-check` audit row to `companion-command-log.csv`. If the config is disabled, the click reports the blocked config state and writes only a blocked audit row, so the probe remains default-off. `Progression Gate` writes a read-only `progression-persistence-gate` command-log row that records the approved progression profile fields and blocked actor/save ownership fields. `Taming Gate` writes a read-only `taming-eligibility-gate` command-log row that classifies reviewed roster entries as tameable candidate, already-companion, unsafe hostile, undead/blocked, passive-only, or evidence insufficient.

The probe is evidence-only. It does not call native target recalculation, does not call `TargetOverrideElement.GetTarget`, does not dispatch commands, does not move actors, does not change targets, does not write progression state, and does not execute behavior through Avalon Core. Custom AI, attack commands, target selection, taming, training, actor persistence, and behavior-changing progression remain blocked until this CSV evidence is reviewed across peaceful, combat, stealth, interior, transition, rest, quit/reload, and return cases.

0.1.34 reviewed the first live AI boundary evidence set: 276 rows, 3 manual `panel-ai-boundary-check` rows, 273 periodic rows, 43 `npcInCombat=true` rows, 64 rows with one live hero attacker, and zero unsafe mutation/Core flags. The reviewed rows were all from `CampaignMap_HOS`, so this is enough to design a non-mutating `CompanionAiProfile` / `CompanionAiIntent` layer, but not enough to approve target overrides, movement overrides, actor persistence, taming, training, behavior-changing progression beyond the profile-only store, or Core-executed behavior.

`Diagnostics.WriteCompanionAiProfileAudit` defaults to `false`. When enabled, 0.1.35 samples active managed companions at `Diagnostics.CompanionAiProfileAuditSeconds` intervals and appends `companion-ai-profile.csv` under this plugin's BepInEx config folder. Rows record the runtime-only `CompanionAiProfile`, the classified `CompanionAiIntent`, mode/range, distance and catch-up threshold, hero attacker count, native possible target/attacker counts, movement state, native movement blocker state, and target override element presence. Rows explicitly record `touchesCommands=false`, `touchesMovement=false`, `touchesTargeting=false`, `touchesPersistence=false`, and `coreExecuted=false`.

When the profile audit config is enabled, `AI Profile Check` writes one immediate `panel-ai-profile-check` row to `companion-ai-profile.csv`. With no active managed companion, it writes a valid `NoActiveCompanion` row. With active managed companions, it writes one row per active managed companion. If the config is disabled, the click reports the blocked config state and writes only a blocked `ai-profile-check` command-log row.

The profile audit is evidence-only. It does not dispatch commands, does not call native target recalculation, does not call `TargetOverrideElement.GetTarget`, does not change movement, does not change targets, does not store progression state, and does not execute behavior through Avalon Core.

`Companions.EnableProfileDrivenNativeAssist` defaults to `false`. When enabled, 0.1.40 evaluates `CompanionAiIntent` classifications at `Companions.ProfileDrivenNativeAssistSeconds` intervals and may perform only two existing native-safe actions: recall/catch-up for `FollowCatchUpCandidate`, and `NpcHeroPetAlly.EnterCombat()` for `NativeCombatObserved` when the hero already has live attackers and Defend/native-assist gates allow it. It logs `ai-profile-native-catch-up` and `ai-profile-native-defend` command rows. While enabled, the older follow/defend ticks stand down so they cannot claim the first catch-up or defend action before the profile loop logs it. It does not add direct target selection, movement-state override, target override, attack UI, actor persistence, taming, training, behavior-changing progression, or Core execution.

`Companions.EnableCompanionTrustRuntime` defaults to `true`. When enabled, 0.2.3 attaches a `CompanionTrustProfile` to each `CompanionCoreRuntimeProfile`. Trust and loyalty start neutral, change only from existing approved command/runtime events, and are shown in the debug panel plus the plugin-owned dialogue subtitle. When `Diagnostics.WriteCompanionTrustLog=true` and command logging is enabled, trust changes append `companion-trust` rows to `companion-command-log.csv`. This profile does not change targeting, movement, AI, taming, training, actor persistence, or Core behavior.

`Companions.EnableCompanionBondPolicyRuntime` defaults to `true`. When enabled, 0.2.5 evaluates the lower of trust and loyalty level as the effective bond. `Wary` companions respond slower, keep a wider catch-up threshold, have longer native defend cooldowns, and cannot use automatic native defend assist unless the player explicitly orders Defend. `Trusted` and `Loyal` companions respond faster, catch up sooner, and use shorter native defend cooldowns. These modifiers affect only Avalon-owned command timing and audit reasons. Version 0.2.8 makes that state readable in the `Companion` dialogue subtitle and roster panel.

`Companions.EnableCompanionProfileStore` defaults to `true`. When enabled, 0.2.10 saves approved profile-only progression fields to `companion-profiles.tsv` under this plugin's BepInEx config folder. The key is template GUID plus review ID. Saved columns are profile schema version, template GUID/name, display name, review ID, pet/runtime approval flags, trust score, loyalty score, trust level, loyalty level, last trust event, last known mode/range, and effective bond level. Loading this file hydrates trust/loyalty profile values only. It does not restore actors, auto-respawn, re-adopt live actors, save location/health/AI/target state, take save ownership, or change command behavior. Store writes add `profile-store-save` rows to `companion-command-log.csv` with `touchesPersistence=true` and actor-persistence flags explicitly false.

0.2.11 adds a read-only taming eligibility audit over the reviewed roster. The `Taming Gate` button builds `CompanionTamingProfile` rows from existing roster identity and runtime role/temperament classification only, then writes one `taming-eligibility-gate` row to `companion-command-log.csv`. The row explicitly keeps capture, wild actor adoption, actor persistence, actor restore, auto-respawn, command behavior, save ownership, location/health/AI/target state, movement, targeting, persistence, and Core execution disabled.

0.1.36 reviewed the first live profile audit smoke: 27 rows from `CampaignMap_HOS`, 14 active `Sharg Candidate` rows, 13 `NoActiveCompanion` rows, 5 `MovementBlocked` rows, 9 `IdleNativePatrol` rows, and zero unsafe mutation/Core flags. This validates load/config/file writing and two active-state classifications. It does not validate Stay, follow catch-up, Defend waiting after movement unblocks, native combat observed, interiors, stealth, transition, rest, quit/reload, return, or any behavior execution.

## Avalon Core diagnostic bridge

`AvalonCore.EnableDiagnosticBridge` defaults to `true`. When Avalon Core is installed, Avalon Companions tries to read `AvalonCore.Plugin.TrustReports` once by optional reflection and logs a compact read-only trust-report summary. If Core is absent, the API shape is missing, the counters are invalid, or the report would mutate runtime behavior, the bridge fails closed with `action=none`. This does not add an Avalon Core dependency, register adapter descriptors, register companion profiles, query Core registries, or change companion behavior.

## Diagnostics

`Diagnostics.LogPetSystemDiagnostics` defaults to `false`. When enabled, the plugin waits for FoA common references, the hero, and loaded templates to become available, then logs one snapshot containing:

- current `PetElement`, `PetVariantBase`, `NpcHeroSummon`, and `NpcHeroPetAlly` counts,
- whether `CommonReferences.PetBaseVariant` is set,
- Qrko mount template count,
- loaded `Spec_Pet_*` template names with their canonical template GUIDs.

`Diagnostics.WritePetTemplateDump` defaults to `false`. When enabled, the same one-shot diagnostic writes `pet-template-candidates.csv` under this plugin's BepInEx config folder. The CSV records `templateGuid`, Unity object name, CLR type, template type, abstract status, and a `safeSpawnCandidate=false` marker for every loaded `Spec_Pet_*` `LocationTemplate`.

`Diagnostics.WritePetCreatureShortlistDump` defaults to `false`. When enabled, the same one-shot diagnostic writes:

- `pet-creature-location-candidates.csv`
- `pet-creature-location-shortlist.csv`
- `pet-creature-diagnostic-tool-crosscheck.csv`, when `Diagnostics.WritePetCreatureDiagnosticToolCrosscheck=true` and a FOA-Diagnostic Tool dump exists
- `pet-creature-diagnostic-tool-review.csv`, when `Diagnostics.WritePetCreatureDiagnosticToolCrosscheck=true` and a FOA-Diagnostic Tool dump exists

The dump joins loaded `LocationTemplate` rows to loaded scene `LocationSpawnerAttachment`, `GroupSpawnerAttachment`, and `AutoGuardSpawningAttachment` references. Rows include template GUID/name/type, scene spawner evidence, matched pet/creature terms, `NpcAttachment.IsUnique` evidence when available, name-risk blocking, `shortlistCandidate`, `safeSpawnCandidate=false`, and `rosterApproved=false`.

The FOA-Diagnostic Tool crosscheck reads the latest `kane.tgfoa.template-diagnostics/<timestamp>/spawner_refs.csv` and compares that evidence against Avalon's own one-shot scan. Rows seen by FOA-Diagnostic Tool but not by Avalon are marked as registration/timing mismatches for manual review, not as approved companions.

`Diagnostics.DiagnosticToolMinimumSpawnerRefs` defaults to `100`, so the crosscheck skips low-context startup dumps with only a few spawner rows and prefers the newest route-context dump. Set it to `0` only when intentionally checking the newest dump regardless of size.

This does not spawn, transform, persist, teleport, or command actors.

## Build

Example local build:

```powershell
dotnet build .\src\AvalonCompanions.csproj -p:FoAGameRoot="C:\Program Files (x86)\Steam\steamapps\common\Tainted Grail FoA"
```

Optional deploy after build:

```powershell
dotnet build .\src\AvalonCompanions.csproj -p:DeployOnBuild=true
```

## Required next research

After the 0.1.42 lifecycle validation gate is documented, follow the remaining gate order:

- Lifecycle validation smoke: run the 0.1.42 throwaway-save route and review `companion-lifecycle.csv` plus `companion-command-log.csv` before any persistence, Core, or custom AI work.
- Core integration gate: after the framework is stable, add diagnostic/profile registration through Avalon Core behind gates, with no Core-executed behavior yet.
- Custom AI behavior gate: custom AI execution, target overrides, movement overrides, attack commands, taming, training, loyalty, persistence, and Core-executed behavior remain blocked until `companion-ai-boundary.csv` and `companion-ai-profile.csv` evidence covers Stay, follow catch-up, Defend waiting, native combat, stealth, interiors, transition, rest, quit/reload, and return.
