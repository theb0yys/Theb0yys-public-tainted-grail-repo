# Design Notes

## Scope

- Requested behavior: start a twin mod to Avalon Companions for future human NPC companions.
- Current behavior: BepInEx proof scaffold with config, load logging, blocked-behavior warnings, default-off proof spawn, default-off actor scanner evidence output, default-off one-session native ally proof, a registered proof-candidate roster for the 42 generated non-unique spawner-backed review rows, a default-on runtime native `Companion` prompt gate for newly generated configs that opens a companion-style dialogue surface for the active proof actor, fallback runtime native quick commands when the proof command surface is not enabled, a default-on proof debug panel gate for newly generated configs, active-proof lifecycle diagnostics/guardrails, proof-only Hold runtime movement lock, basic Follow via closer bounded catch-up recall, command-surface input/focus lock using the proven Avalon Companions cursor/scope path, dialogue Rewired input pass-through, and a generated dev-diagnostic human NPC map.
- Files expected to change: `mods/avalon-human-companions/**` and `docs/hotkey-registry.md`.
- Risk level: Medium for the one-session ally proof because it applies a per-instance faction override and native ally marker to a spawned proof actor; high for any future human NPC behavior beyond this proof.

## Target

- Assembly: `TG.Main.dll`.
- Class: `AvalonHumanCompanions.Plugin`.
- Method or runtime object: `LocationTemplate.SpawnLocation`, spawned `Location`, spawned `NpcElement`, `NpcElement.OverrideFaction`, `NpcHeroPetAlly`, plugin-owned `AbstractLocationAction` elements, `NpcCanMoveHandler`, plugin-owned `ICanMoveProvider`, and `Location.MoveAndRotateTo`.
- Evidence: current research blocks human NPC companion runtime behavior until actor identity, faction, quest, interaction UI, transition, and save/load behavior are proven. `docs/research/human-npc-proof-step-2026-06-15.md` proves the relevant actor surfaces in local `TG.Main.dll`: `NpcElement`, `NpcAttachment`, `UniqueNpcAttachment`, `RepetitiveNpcAttachment`, `NpcPresence`, `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, `NpcAISummon`, and story-owned NPC movement/faction/hostility steps.

## Approach

- Patch type or plugin behavior: `BaseUnityPlugin` load/config scaffold only; no Harmony patches.
- Config entries:
  - `General.Enabled`, default `true`.
  - `Research.ResearchModeOnly`, default `true`.
  - `Target.TemplateName`, default empty.
  - `Target.TemplateGuid`, default empty.
  - `Diagnostics.LogGateOnLoad`, default `true`.
  - `Prototype.EnableDisabledRecruitmentTest`, default `false`.
  - `Prototype.DisabledRecruitmentTestHotkey`, default `None`.
- `ProofSpawn.EnableSafeSpawnCommand`, default `false`.
- `ProofSpawn.SafeSpawnHotkey`, default `None`.
- `ProofSpawn.SafeSpawnDistance`, default `4`, range `2..12`.
- `ProofSpawn.LimitOneProofSpawnPerSession`, default `true`.
- `ActorScanner.EnableDisabledActorScanner`, default `false`.
- `ActorScanner.ScannerHotkey`, default `None`.
- `ActorScanner.ScanRadius`, default `35`, range `5..120`.
- `ActorScanner.SelectedTarget`, default empty.
- `HumanAllyProof.EnableOneSessionAllyProof`, default `false`.
- `HumanAllyProof.AllyProofHotkey`, default `None`.
- `HumanAllyProof.DismissAllyProofHotkey`, default `None`.
- `HumanAllyProof.AllyProofDistance`, default `5`, range `2..12`.
- `HumanAllyProof.LimitOneAllyProofPerSession`, default `true`.
- `HumanCommands.EnableNativeCommandActions`, default `true`; when the proof panel is enabled, attaches one runtime-only native `Companion` prompt to the active proof actor and opens the existing proof panel.
- `HumanCommands.EnableFollowCatchUp`, default `true`.
- `HumanCommands.FollowCatchUpDistance`, default `18`, range `8..60`; Defend uses the configured value, while Follow clamps to the closer proof-follow range.
- `HumanCommands.EnableNativeDefendAssist`, default `false`.
- `HumanCommandPanel.EnableProofCommandPanel`, default `true`.
- `HumanCommandPanel.TogglePanelHotkey`, default `End`.
- `HumanRoster.EnableProofCandidateRoster`, default `true`.
- `HumanRoster.SelectedCandidateIndex`, default `8`, the existing reviewed outlaw proof target.
- `HumanLifecycle.EnableLifecycleSafetyGuard`, default `true`.
- `HumanLifecycle.WriteLifecycleDiagnostics`, default `true`.
- `HumanLifecycle.LifecycleTickSeconds`, default `5`, range `1..60`.
- Save impact: proof spawns are marked not saved immediately. The actor scanner writes plugin-owned CSV evidence under this plugin's BepInEx config folder. The one-session ally proof applies a per-instance summon-faction override plus `NpcHeroPetAlly` only to the plugin-owned spawned actor and marks it not saved. No companion save data, story state, vanilla template data, global faction data, or config files outside this plugin's BepInEx config are changed.
- Compatibility considerations: no Harmony patches or conversion of existing actors in 0.1.4. The one-session proof, native quick commands, and proof panel touch only a spawned proof actor but still overlap conceptually with summons, factions, combat targeting, Avalon Companions, Avalon Core, population, quests, crime, dialogue, UI, and save/load systems.
- Failure behavior: missing references fail at build through MSBuild validation. Runtime load only logs the gate and returns.
- UI route: styled plugin-owned overlay for proof testing because the 0.1.3 native interaction actions did not work in live testing. The panel is not release-ready until in-game screenshot or video validation is recorded. It must not use default Unity IMGUI chrome, tiny corner debug layout, or unstyled control stacks.

## Safe proof spawn

The first code proof is a disabled-by-default safe spawn command.

It requires:

- `Target.TemplateGuid` set to an explicit reviewed `LocationTemplate` GUID,
- `ProofSpawn.EnableSafeSpawnCommand=true`,
- `ProofSpawn.SafeSpawnHotkey` set to a real key,
- a loaded save with `Hero.Current` available.

It blocks templates without `NpcAttachment`, blocks templates with `NpcAttachment.IsUnique=true`, spawns behind the hero, immediately sets `Location.MarkedNotSaved=true`, verifies a non-unique `NpcElement`, and logs that no companion behavior was applied.

It must not add ally markers, faction overrides, follow commands, dialogue actions, story state, crime state, or persistence.

## One-session native ally proof

The new proof path is disabled by default and separate from the old safe proof spawn. It exists because the old proof correctly proved spawning, but the reviewed target is a hostile outlaw and therefore attacks without an ally marker.

It requires:

- `Research.ResearchModeOnly=false`,
- `Target.TemplateGuid` set to an explicit reviewed `LocationTemplate` GUID,
- `HumanAllyProof.EnableOneSessionAllyProof=true`,
- `HumanAllyProof.AllyProofHotkey` set to a real key,
- a loaded throwaway save with `Hero.Current` available.

It blocks templates without `NpcAttachment`, blocks templates with `NpcAttachment.IsUnique=true`, spawns behind and slightly right of the hero, immediately sets `Location.MarkedNotSaved=true`, verifies a non-unique `NpcElement`, applies the Avalon Companions creature-candidate native ally marker path, and logs the boundary.

The native ally marker path is:

- `NpcElement.OverrideFaction(Hero.Current.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`.
- `NpcElement.AddElement(new NpcHeroPetAlly(Hero.Current))`.

If the spawned actor lacks `NpcElement`, becomes unique, or fails ally setup, it is marked not saved and discarded immediately.

It must not convert existing NPCs, mutate story/dialogue/crime/quest state, add custom target selection, or persist state.

## One-session native command actions

The first command surface was disabled by default and attached runtime-only native quick commands to the active ally proof actor after `NpcHeroPetAlly` was present.

Approved commands:

- `Follow`: allows close bounded catch-up recall and can be reselected to rearm Follow mode.
- `Hold`: applies a plugin-owned runtime movement block to the active proof actor only.
- `Come Close`: sets follow mode and recalls near the hero.
- `Recall`: recalls near the hero.
- `Defend`: arms native defend mode and calls `NpcHeroPetAlly.EnterCombat()` only if the hero has live attackers.
- `Dismiss`: discards the proof actor.

The commands are plugin-owned `AbstractLocationAction` elements and must be removed when the proof actor is dismissed or no longer has the expected ally marker. They must not be attached to existing NPCs, unrelated spawned actors, unique/story actors, or wild NPCs.

True `Stay`, true `Wait`, saved hold positions, and transition/reload-safe waiting remain blocked because no humanoid ally wait API has been researched and validated. Version 0.1.8 only approves a runtime movement lock for the active one-session proof actor.

Version 0.1.12 mirrors the safer Avalon Companions command-menu route for the working proof path. When `HumanCommands.EnableNativeCommandActions=true` and `HumanCommandPanel.EnableProofCommandPanel=true`, the active proof actor receives one runtime-only native `Companion` prompt. Activating that prompt opens the plugin-owned proof command panel, and the older separate quick command action elements are removed so FoA resolves a single native prompt. When the proof panel is not enabled, the older quick-action path remains a fallback only and is still not the preferred command surface.

Version 0.1.13 keeps the 0.1.12 behavior unchanged but makes `HumanCommands.EnableNativeCommandActions` and `HumanCommandPanel.EnableProofCommandPanel` default `true` for newly generated configs, per the explicit gate request. This does not enable proof spawning, research-mode bypass, scanner output, defend assist, persistence, recruitment, or existing-NPC conversion.

Version 0.1.14 incorrectly changed the hotkey proof panel into the companion-style Unity UI host. Version 0.1.15 corrects the split: the native NPC `Companion` prompt opens the transparent full-screen/right-choice/bottom-band dialogue surface, and the hotkey opens a compact Avalon Companions Debug-style IMGUI control panel.

Version 0.1.21 keeps the approved one-session proof lane and makes Follow basic functionality usable: Follow can be selected again while already active, and Follow-mode bounded catch-up recall clamps to a close proof range. It still uses `Location.MoveAndRotateTo(...)` on the managed proof actor only and does not add custom pathfinding or persistent companion state.

## Proof command panel

The proof debug panel gate is enabled by default for newly generated configs and opens with `HumanCommandPanel.TogglePanelHotkey`, default `End`.

Approved panel controls:

- `Prev`: selects the previous registered human proof candidate.
- `Spawn / Swap`: calls the existing one-session ally proof spawn path for the selected proof candidate and discards the active proof actor before swapping.
- `Next`: selects the next registered human proof candidate.
- `Follow`: sets or rearms proof mode to follow and uses close bounded catch-up recall.
- `Hold`: applies a runtime movement lock to the active proof actor.
- `Come Close`: sets follow mode and recalls near the hero.
- `Recall`: recalls near the hero.
- `Defend`: arms native defend mode and calls `NpcHeroPetAlly.EnterCombat()` only if the hero has live attackers.
- `Dismiss`: discards the proof actor.
- `Scan Actors`: calls the existing scanner evidence path only when `ActorScanner.EnableDisabledActorScanner=true`.

The panel must act only on the active plugin-owned proof actor after `NpcHeroPetAlly` is present. It must unlock the cursor while open, close with `End`, `Esc`, or Close, and keep normal play cheap when hidden. It is proof UI, not a release-ready full human companion panel.

Version 0.1.16 adds the missing Avalon Companions-style roster foundation. The panel now shows selected proof candidate count/name, template, GUID, review ID, and risk flags. The roster is still a proof-candidate selector only; it does not make the candidates recruitable, persistent, save-owned, or approved for existing-NPC conversion.

Version 0.1.17 keeps the same roster and command layout but makes panel dimensions responsive on large displays. The panel keeps the fixed minimum, grows to 42% viewport width and 38% viewport height when larger than the minimum, and remains clamped inside screen margins.

Version 0.1.18 corrects the native `Companion` dialogue input/cursor path after live testing showed the dialogue surface could open with frozen input. The human UI scope now tries FoA Mod Manager `SetCustomUiScope` before Tainted Interface fallback, and the Rewired axis/button lock matches Avalon Companions by passing input reads through while the dialogue-style surface is visible. The debug panel keeps the stricter movement/camera/action lock.

Version 0.1.19 supersedes the 0.1.18 scope-order experiment and mirrors the working Avalon Companions controller lifecycle: Tainted Interface custom UI scope first, FoA Mod Manager fallback, one shared `UpdateCursor()` helper for panel/dialogue visibility, interactive cursor refresh inside `EnsureCursorForPanel()`, and input-module restore only after no human UI surface remains.

Version 0.1.5 replaces the 0.1.4 cramped fixed `620x430` vertical stack with a larger responsive modal. The panel is clamped to the current screen, separates actor status from commands, uses larger labels/buttons, and supports dragging from the header. This is still proof UI until a fresh in-game screenshot validates the new layout.

Version 0.1.6 adds a panel scanner trigger because the scanner hotkey did not produce CSV output or a scanner log line during live testing. The button is gated by the existing disabled scanner config and only calls `HumanActorScanner.WriteSnapshot`; it does not approve or perform actor behavior.

Version 0.1.8 adds a proof-only Hold command to the panel and native quick command action set. Hold adds a plugin-owned not-saved `ICanMoveProvider` element to the active proof actor's `NpcElement` and removes it on Follow, Defend, Come Close, or Dismiss. Recall repositions the actor without changing the selected proof mode. This is not a native humanoid wait package and does not approve persistence.

Version 0.1.9 reorganizes the panel into a proof flow: status first, then Prepare, Movement, Combat, and cleanup commands. It also adds a `Lifecycle Check` evidence button that writes the existing lifecycle event row and does not recover, persist, or restore actors.

Version 0.1.9 also adds a Harmony input/focus lock based on the proven Avalon Companions and FoA Mod Manager pattern. While the panel is open, it unlocks the cursor for the panel, disables captured Unity `BaseInputModule` instances, blocks FoA `PlayerInput.ProcessLateUpdate`, zeros movement/look fields, blocks Rewired axes/buttons, continues to reset input axes, and opts into FoA Mod Manager's shared custom UI scope when that manager is installed. That shared scope provides controller cursor support and freezes world time while the proof panel is open. Older managers fall back to controller cursor scope; the local fallback still does not change `Time.timeScale`.

Version 0.1.10 increases the proof panel default size to `1320x860`, increases the readable minimum size, gives the status column more room, slightly increases text/button sizes, clears IMGUI focus after actions, pins explicit button states, and changes selected commands to a calmer teal state. This is UI polish only; it does not add behavior or release-ready human companion functionality.

Version 0.1.11 keeps the proof panel disabled by default and adds optional shared dark-fantasy IMGUI styles, shared custom UI scope, and cursor visibility while the panel is open. Version 0.1.19 keeps that proven Avalon Companions-style scope lifecycle through the current Tainted Interface bridge and uses FoA Mod Manager as fallback when the interface scope is absent. This is UI shell/input integration only and does not change commands, actor gates, scanner output, lifecycle behavior, persistence, or release-ready status.

Version 0.1.12 adds a native entry point for the active proof actor. Version 0.1.15 routes that native `Companion` prompt to a companion-style Unity UI dialogue host and keeps the hotkey panel as debug/control UI. The commands and safety gates remain unchanged, and no native dialogue graph, serialized interaction list, recruitment, persistence, or existing-NPC conversion is added.

## Diagnostic human map

Version 0.1.9 adds `tools/New-HumanNpcDiagnosticMap.ps1`, which reads existing dev FOA-Diagnostic Tool dump files from `mods/template-diagnostics` / `BepInEx/config/kane.tgfoa.template-diagnostics`.

The generated map files live under `docs/generated` and classify human NPC templates for manual review only. Every row must keep `safeSpawnCandidate=false`, `rosterApproved=false`, `behaviorApproved=false`, and `persistenceApproved=false`.

## Scanner proof step

The scanner classifies existing live actor-like `Location` instances and writes plugin-owned CSV evidence under `BepInEx/config/kane.tgfoa.avalon-human-companions`.

The scanner must not spawn, unstash, move, teleport, recruit, convert, command, dismiss, faction-edit, story-edit, crime-edit, interaction-edit, or persist NPCs.

The scanner writes:

- `human-actor-candidates.csv`
- `human-actor-components.csv`
- `human-actor-command-dry-run.csv`
- `human-recruitment-capture-research.csv` in 0.6.0

Every dry-run command row remains blocked with `liveAction=false`. The panel row is evidence for future design only and is not a real in-game panel. A custom human panel remains blocked until native command actions are validated.

Version 0.6.0 adds scanner-only friendly/enemy research lanes for future capture/recruitment work. The new CSV may mark rows as `FriendlyRecruitmentResearch`, `EnemyCaptureResearch`, `DispositionAmbiguousResearch`, `DispositionUnknownResearch`, or `OneSessionProofOnly`, but this is string evidence only. Every row stays blocked with recruitment, capture, persistence, and live action approval false.

## Lifecycle proof step

Version 0.1.7 adds a lifecycle guard and diagnostics for the active one-session proof actor.

The lifecycle guard:

- keeps the tracked proof actor marked not saved,
- discards the actor if `ResearchModeOnly` is re-enabled while it is active,
- discards the actor if it loses the expected `NpcHeroPetAlly` managed proof marker,
- removes plugin-owned quick command actions before Dismiss/discard,
- removes the plugin-owned Hold movement block before Dismiss/discard,
- clears the in-memory tracked actor after Dismiss/discard.

When `HumanLifecycle.WriteLifecycleDiagnostics=true`, the plugin appends `human-proof-lifecycle.csv` under the plugin config folder. Rows are evidence only and explicitly keep `behaviorApproved=false` and `persistenceApproved=false`.

This does not approve persistence, reload restoration, recruitment, existing NPC conversion, or save-backed roster data.

## Review requirements

- Code review required: yes.
- Second review required: yes before any human NPC behavior.
- Reason: human companion behavior can affect actor lifecycle, AI, combat, crime, faction state, quests, dialogue, and persistence.
