# Avalon Human Companions

Research-gated scaffold for a future human NPC companion mod for Tainted Grail: The Fall of Avalon.

## Status

Version 0.6.0 is a one-session human companion baseline built from the research/proof lane. It loads as a BepInEx v5 Mono plugin, records the human-NPC companion research boundary in the log, registers the 42 generated `ReviewQueueNonUniqueSpawnerBacked` human proof candidates, promotes 13 hostile-risk candidates into the companion selector, opens native NPC `Companion` interaction through a companion-style vanilla Unity UI host, keeps the hotkey panel as a companion summoning/control panel, removes raw template/review/GUID/proof wording from the player-facing dialogue and summon panel, adds framed embedded Tainted Interface character portraits with more specific candidate-family portrait mapping, adds a passive gameplay HUD companion badge, includes a config-gated runtime-only companion brain with role-aware bounded recall placement, behavior polish, responsive native assist, wider Follow-leash tuning, config-driven responsive tuning, and live debug-panel tuning controls, and adds an evidence-only capture/recruitment classifier to the disabled actor scanner.

The companion brain is enabled by `HumanBrain.EnableCompanionBrain` for newly generated configs. `HumanBrain.EnableRoleAwarePlacement` lets promoted selector roles use different bounded recall offsets around the hero. It only orchestrates the existing bounded recall path and native `NpcHeroPetAlly.EnterCombat()` path for the active one-session proof actor. `HumanBrain.EnableResponsiveNativeAssist` lets the brain evaluate more often, keep Follow responsive without dragging the companion too tightly, use bounded combat recall placement, and proactively call native `NpcHeroPetAlly.EnterCombat()` when the hero already has live attackers. Version 0.5.3 keeps the `HumanBrainTuning` config entries for responsive brain tick, Follow leash, Follow recall grace, combat recall distance, automatic recall cooldowns, and native defend prompt cooldown, and adds live `+`/`-` controls for those values to the hotkey companion panel. The brain does not add recruitment, persistence, existing-NPC conversion, custom target selection, custom pathing, attack buttons, dialogue, quest, crime, faction-table, or save-data behavior.

The proof spawn command requires an explicit `Target.TemplateGuid`, blocks unique NPC templates, marks the spawned location not saved immediately, allows only one proof spawn per session by default, and applies no recruitment, faction, ally, follow, dialogue, story, crime, or persistence behavior.

The actor scanner is disabled by default. When explicitly enabled with a scanner hotkey, it writes plugin-owned CSV evidence under `BepInEx/config/kane.tgfoa.avalon-human-companions` and does not move, command, recruit, capture, convert, faction-edit, interaction-edit, or persist NPCs. Version 0.6.0 adds `human-recruitment-capture-research.csv`, which classifies actor-like rows into friendly-recruitment, enemy-capture, ambiguous, unknown, or one-session-proof research lanes from current string evidence only. Every recruitment/capture row remains blocked with `liveAction=false`, `recruitmentApproved=false`, `captureApproved=false`, and `persistenceApproved=false`.

The one-session native ally proof is disabled by default. It is only for throwaway-save testing with a reviewed non-unique proof target. It mirrors the Avalon Companions creature-candidate path by applying a per-instance hero summon faction override plus `NpcHeroPetAlly` to the spawned proof actor. It does not convert existing NPCs and does not add recruitment, dialogue, story, crime, quest, custom target selection, or persistence.

The native command surface gate is enabled by default for newly generated configs. When `HumanCommands.EnableNativeCommandActions=true` and the proof panel is enabled, one runtime-only native `Companion` prompt is attached only to the active one-session proof actor and opens the existing proof command panel. The older Follow, Hold, Come Close, Recall, Defend, and Dismiss quick actions remain fallback-only when the proof panel is not enabled. Follow can be reselected and uses the approved bounded catch-up recall at a close proof range. Hold is a runtime movement lock for the active spawned proof actor only. True Stay/Wait behavior and saved hold positions remain blocked until a humanoid ally wait API is researched and validated.

The command surface gate is enabled by default for newly generated configs. `HumanCommandPanel.TogglePanelHotkey` defaults to `End` and opens a compact companion control panel for Prev, Call / Swap, Next, Follow, Hold, Defend, Come Close, Recall, and Dismiss. Scanner and lifecycle diagnostics remain internal/debug tools, not normal companion-menu choices. The native NPC `Companion` prompt opens the companion-style dialogue choices instead.

The proof candidate roster is enabled by default for newly generated configs. `HumanRoster.SelectedCandidateIndex` defaults to the existing reviewed outlaw proof target, and the panel cycles the 13 promoted companion candidates with Prev/Next while the full 42-row reviewed roster stays registered in code. `Call / Swap` still routes through the existing one-session ally proof path and keeps every actor marked not saved. These entries are proof candidates only, not persistent roster approvals.

The lifecycle guard keeps the active proof actor marked not saved, discards it if it loses the expected managed ally marker, removes plugin-owned runtime command and Hold elements, clears tracked state after Dismiss/discard, and writes `human-proof-lifecycle.csv` event rows when enabled. It is evidence and cleanup for the one-session proof actor only; it is not persistence or recruitment.

## Install

Requires the PC Mono branch with BepInEx v5 Mono installed.

Extract the archive so the `plugins` folder merges into `BepInEx/plugins`:

```text
BepInEx/
  plugins/
    AvalonHumanCompanions/
      AvalonHumanCompanions.dll
```

On first launch, the config is generated at:

```text
BepInEx/config/kane.tgfoa.avalon-human-companions.cfg
```

## Current scope

- Establish the human-NPC companion mod folder, plugin identity, and release metadata.
- Keep all human companion behavior blocked by `Research.ResearchModeOnly` unless explicitly running the one-session ally proof on a throwaway save.
- Log the current research gate on load.
- Provide a disabled recruitment-test hotkey that only logs a blocked-behavior warning.
- Provide a default-off safe proof spawn command for one explicit non-unique `LocationTemplate` GUID.
- Provide a default-off actor scanner that writes candidate/component/dry-run command CSV evidence only.
- Provide a default-off one-session native ally proof for one explicit non-unique spawned proof actor.
- Provide a companion-style registered proof-candidate roster for the 42 generated non-unique spawner-backed review rows.
- Provide framed embedded Tainted Interface character portraits for the selected/active one-session companion without copying icon assets into this mod.
- Provide more specific candidate-family portrait mapping for promoted outlaw, highwayman, deranged, archer, spear, heavy, and armored rows.
- Provide a passive gameplay HUD badge for the active one-session companion, hidden while the panel or companion dialogue is open.
- Provide a default-on runtime-only native `Companion` prompt gate for newly generated configs; the prompt still appears only on the active proof actor and opens the proof panel.
- Provide a default-on proof command surface gate for newly generated configs.
- Provide a gated proof-panel scanner button that writes actor CSV evidence only.
- Provide lifecycle diagnostics and guardrails for the active proof actor.
- Provide a proof-only Hold command that applies a plugin-owned runtime movement block only to the active spawned proof actor.
- Provide basic proof Follow by rearming Follow mode and keeping the active proof actor near with bounded catch-up recall.
- Provide 0.4.0 behavior polish for smoother automatic catch-up, runtime Hold anchor feedback, native-safe Defend handoff feedback, and consistent Recall/Come Close command status.
- Provide 0.5.3 responsive native assist with config-driven and debug-panel live-tunable brain tick, Follow leash, Follow recall grace, combat recall distance, recall cooldowns, and proactive native defend when the hero already has live attackers.
- Provide 0.6.0 scanner-only capture/recruitment research lanes and blocked dry-run rows for future friendly recruitment and enemy capture review.
- Use the proven Avalon Companions cursor/scope lifecycle with the current Tainted Interface bridge first and FoA Mod Manager fallback when the interface bridge is absent.
- Provide a dev-diagnostic-tool human NPC map generator and generated review-only map files.

## Out of scope for 0.6.0

- Human NPC recruitment.
- Human NPC capture.
- Persistent human NPC companion spawning or cloning.
- Automatic spawning.
- Unique, named, boss, story, quest, challenge, tutorial, or scene-critical NPC proof spawning.
- Converting vanilla NPCs into followers.
- Story graph dialogue, affinity, romance, or quest state.
- Custom combat AI, custom follow AI, attack commands, equipment, inventory, or leveling.
- Release-ready full human NPC behavior panel.
- True Stay, true Wait, saved hold positions, or transition/reload-safe waiting.
- Save data or persistent companion records.
- Save/quit/reload restoration of proof actors.
- Edits to global faction tables, crime, ownership, unique NPC state, story graphs, or vanilla interaction lists.

## Required next research

Before runtime behavior, document exact local `TG.Main.dll` evidence for:

- scanner CSV output from the current proof target and nearby live actors,
- faction and hostility handling that does not affect guards, civilians, quests, or the player,
- true follow beyond bounded catch-up recall, wait, recall, and dismiss hooks,
- native or plugin-owned interaction UI,
- save/load, area transition, death reload, quit/relaunch, and cleanup behavior.
