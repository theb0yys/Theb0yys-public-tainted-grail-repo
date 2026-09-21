# Avalon Human Companions Research

## Evidence read

- `docs/engineering-process.md`
- `docs/code-review-standard.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/actor-control-proof-design-2026-06-14.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Report-deep-research-report.md`
- `Research/foa-universal-actor-companion-framework-2026-06-15.md`
- `Research/foa-actor-interaction-feasibility-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-npc-proof-step-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/safe-proof-spawn-target-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/added-research-review-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-actor-scanner-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-lifecycle-gate-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-hold-command-2026-06-16.md`
- `mods/avalon-human-companions/docs/research/human-panel-flow-input-lock-and-diagnostic-map-2026-06-16.md`
- `mods/avalon-human-companions/docs/research/human-native-companion-prompt-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-companion-style-command-surface-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-proof-candidate-roster-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-proof-roster-panel-scale-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-dialogue-mod-manager-input-scope-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-dialogue-proven-companion-controller-path-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-dialogue-gui-maintenance-path-2026-06-21.md`
- `mods/avalon-human-companions/docs/research/human-recruitment-capture-classifier-2026-07-02.md`

## Research decision

Avalon Human Companions must stay in a separate research/proof lane.

The existing Avalon Companions research approves a narrow pet/creature lane for Qrko plus plugin-owned one-session wolf/bear candidates. That evidence does not approve human NPC companions. The companion feasibility reports classify human followers as a high-risk custom framework path because FoA does not expose a native recruitable human companion system, party UI, follower inventory layer, or proven save-safe human follower persistence path.

## Boundary

This mod may:

- load as a BepInEx plugin,
- bind conservative config entries,
- log the current human companion research gate,
- provide disabled prototype controls that only explain why behavior is blocked,
- run one explicit, config-gated proof spawn for a non-unique `LocationTemplate` GUID, mark it not saved immediately, and apply no companion behavior,
- run one explicit, config-gated one-session ally proof for that same reviewed non-unique target, mark it not saved immediately, and apply only the native summon-faction plus `NpcHeroPetAlly` marker already proven in Avalon Companions creature candidates.

This mod must not:

- recruit, spawn, clone, convert, move, dismiss, or command human NPCs,
- spawn automatically or spawn without an explicit target GUID and proof-spawn gate,
- touch unique, named, boss, story, quest, challenge, tutorial, or scene-critical actors,
- mutate faction, crime, ownership, relationship, hostility, quest, dialogue, story graph, or save state except for the explicitly gated one-session ally proof's per-instance summon-faction override on a plugin-owned spawned proof actor,
- add native dialogue, prompt, or interaction entries to vanilla serialized lists,
- write companion save data,
- claim a working human follower until the exact actor, faction, UI, transition, and save/load path is researched and validated.

## Candidate research areas

- Humanoid summon and pseudo-follower actors.
- Non-unique hostile or civilian templates that can be classified without approving behavior.
- Native `Location`, `NpcElement`, dialogue, story, prompt, and interaction action paths.
- Per-instance faction ownership and target filtering.
- Follow, wait, recall, dismiss, and catch-up mechanisms.
- Save/load and area-transition reconstruction without duplication.

## First proof result

The first local `TG.Main.dll` proof pass is recorded in `docs/research/human-npc-proof-step-2026-06-15.md`.

Findings:

- `NpcElement` is the core NPC actor surface and is tied to faction, crime, inventory/equipment, movement, AI, dialogue, possible targets/attackers, presence, interaction data, and serialization.
- `NpcAttachment`, `UniqueNpcAttachment`, and `RepetitiveNpcAttachment` give the first uniqueness classification surface.
- `NpcPresence` uses `NpcRegistry`, `UniqueNpcStash`, template spawning, attach/detach, manual availability, teleport, death marking, and serialization, so unique/story presence remains blocked.
- `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, and `NpcAISummon` prove a follower-like summon/ally lane exists.
- Story steps such as `SNpcMove`, `STeleportNpc`, `SChangeNpcFaction`, `SNpcTurnFriendly`, and `SNpcTurnHostileBase` prove native NPC control exists, but those APIs are story-owned and not approved as mod command APIs.

Implemented first code step: add a default-off safe proof spawn command. Approved next code step after a throwaway-save proof spawn is a disabled scanner that classifies existing live NPCs and writes plugin-owned CSV evidence only.

## First proof target

The first safe proof spawn target is recorded in `docs/research/safe-proof-spawn-target-2026-06-15.md`.

- Template: `Spec_Enemy_Generic_Tier1_Outlaw_1H`
- GUID: `2bd34a05d1e1fb94f9770b9ee7f23be2`
- Evidence: `Regular`, `IsAbstract=false`, `actor-npc-repetitive`, `NpcUnique=false`, `RepetitiveNpcAttachment`, and vanilla spawner usage in the template diagnostics dump.
- Status: approved only for one throwaway-save safe spawn command test. It is not approved for companion behavior, route population, save persistence, or faction/ally changes.

## Added research review

The added research review is recorded in `docs/research/added-research-review-2026-06-15.md`.

Decision: the added research does not approve recruitable human companions. It reinforces the current sequence: finish the throwaway-save safe proof spawn, then implement a disabled actor scanner that writes plugin-owned evidence only. Later interaction UI should be plugin-owned and runtime-only, not written into vanilla serialized interaction lists.

## Scanner proof implementation

The scanner proof is recorded in `docs/research/human-actor-scanner-2026-06-15.md`.

Version 0.1.1 implements the approved disabled actor scanner. It is default-off, hotkey-gated, and writes these plugin-owned files under `BepInEx/config/kane.tgfoa.avalon-human-companions`:

- `human-actor-candidates.csv`
- `human-actor-components.csv`
- `human-actor-command-dry-run.csv`

The scanner classifies nearby live actor-like `Location` rows and known pet/summon/ally markers. Its dry-run command rows keep `liveAction=false` and `blocked=true` for follow, stay, recall, dismiss, defend, and panel. Human behavior and a human command panel remain blocked until the scanner CSVs are reviewed and separate actor ownership, faction/targeting, interaction UI, transition, and save/load proofs pass.

## Recruitment/capture classifier

The recruitment/capture classifier is recorded in `docs/research/human-recruitment-capture-classifier-2026-07-02.md`.

Version 0.6.0 extends the existing disabled actor scanner with `human-recruitment-capture-research.csv` and blocked dry-run rows for `recruit-friendly` and `capture-enemy`. The classifier sorts actor-like rows into friendly recruitment, enemy capture, ambiguous, unknown, or one-session-proof research lanes using template/debug/display/component/faction hint strings only.

This is evidence only. It does not add capture, recruitment, prompts, panel buttons, existing-NPC conversion, faction edits, hostility edits, crime edits, transition handling, persistence, or save data. Every row stays `blocked=true`, `liveAction=false`, `recruitmentApproved=false`, `captureApproved=false`, and `persistenceApproved=false`.

## One-session native ally proof

The one-session ally proof is recorded in `docs/research/human-one-session-native-ally-proof-2026-06-15.md`.

Version 0.1.2 adds a disabled-by-default `HumanAllyProof` config section. It mirrors the Avalon Companions creature-candidate native ally path for one plugin-owned, reviewed, non-unique human proof spawn:

- `OverrideFaction(Hero.Current.GetFactionTemplateForSummon(), FactionOverrideContext.Summon)`.
- `new NpcHeroPetAlly(Hero.Current)`.
- `Location.MarkedNotSaved=true`.
- discard on failed native ally setup.
- no conversion of existing NPCs.
- no persistence, dialogue, panel, story, crime, quest, custom target selection, or save data.

This proof exists because the safe proof-spawn target is a hostile outlaw and is expected to attack when spawned without an ally marker. If the one-session ally proof still attacks after the marker is applied, the next step is to inspect humanoid-specific hostility/combat state with scanner CSVs and local `TG.Main.dll`, not to add custom target forcing.

## One-session command surface

The first command surface is recorded in `docs/research/human-one-session-command-surface-2026-06-15.md`.

Version 0.1.3 adds disabled-by-default native quick command actions for the single active human ally proof actor only:

- `Follow`
- `Hold` in 0.1.8, as a proof-only runtime movement lock
- `Come Close`
- `Recall`
- `Defend`
- `Dismiss`

These are runtime-only `AbstractLocationAction` elements attached only after the proof actor has `NpcHeroPetAlly`. They are removed on dismiss or invalid state, marked not saved, and do not edit templates, story graphs, dialogue, crime, quest state, global factions, or save data. Defend uses the same native lane as Avalon Companions: `NpcHeroPetAlly.EnterCombat()` only when the hero already has live attackers.

True `Stay`, true `Wait`, saved hold positions, and transition/reload-safe waiting remain blocked until a humanoid ally wait API is researched and validated.

## Proof command panel

The proof command panel is recorded in `docs/research/human-proof-command-panel-2026-06-15.md`.

Version 0.1.4 adds a disabled-by-default, styled, plugin-owned overlay because the 0.1.3 native interaction actions did not work in live testing. The panel uses `HumanCommandPanel.TogglePanelHotkey`, default `End`, which is registered in `docs/hotkey-registry.md`.

The panel is proof UI only. It may spawn the existing one-session ally proof through the guarded proof path and run Follow, Hold, Come Close, Recall, Defend, and Dismiss through the existing command backend for the active proof actor only. It does not add true Stay/Wait behavior, saved hold positions, convert existing NPCs, add commands to unrelated actors, edit vanilla interaction lists, or write save data.

## One-session Hold command

The one-session Hold command is recorded in `docs/research/human-one-session-hold-command-2026-06-16.md`.

Version 0.1.8 adds a proof-only Hold command for the active one-session proof actor. Local `TG.Main.dll` research did not find a native humanoid `SetFollowing`, `Stay`, or `Wait` API on `NpcHeroPetAlly`, `NpcHeroSummon`, or `NpcAlly`. The approved proof path is a plugin-owned runtime-only `ICanMoveProvider` element attached only to the active proof actor's `NpcElement` while Hold is selected.

Hold is removed when Follow, Come Close, Defend, or Dismiss runs. Recall remains a reposition command and does not change the selected mode. Lifecycle diagnostics must continue to keep `behaviorApproved=false` and `persistenceApproved=false`.

## Panel flow, input lock, and diagnostic map

The panel flow/input lock/diagnostic map step is recorded in `docs/research/human-panel-flow-input-lock-and-diagnostic-map-2026-06-16.md`.

Version 0.1.9 reorganizes the proof panel to match the practical Avalon Companions flow: proof status, runtime state, preparation/evidence controls, movement commands, combat/cleanup commands, and current status. It adds `Lifecycle Check` as an evidence-only button.

Version 0.1.9 also adds the proven Harmony input lock used by Avalon Companions and FoA Mod Manager. The lock blocks player movement/camera/action input and captured Unity input modules while the panel is open. This is not a global time-scale pause.

## Human proof candidate roster

The foundational roster step is recorded in `docs/research/human-proof-candidate-roster-2026-06-21.md`.

Version 0.1.16 registers the 42 `ReviewQueueNonUniqueSpawnerBacked` rows from the generated diagnostic map as proof candidates, mirroring Avalon Companions' selected-index roster pattern. It adds `HumanRoster.EnableProofCandidateRoster`, `HumanRoster.SelectedCandidateIndex`, selected candidate labels, Prev, Spawn / Swap, and Next controls to the debug panel.

The roster feeds the existing one-session ally proof target selection only. It does not approve persistent human companions, recruitment, conversion of existing NPCs, dialogue, quest state, custom targeting, save data, or release-ready behavior. The spawn backend still requires the existing proof gates and still performs non-unique NPC checks before marking the spawned actor not saved and applying the native summon-faction plus `NpcHeroPetAlly` proof marker.

## Human proof roster panel scale

The screenshot-driven panel scale step is recorded in `docs/research/human-proof-roster-panel-scale-2026-06-21.md`.

Version 0.1.17 keeps the 0.1.16 registered proof roster unchanged but changes debug panel sizing from a fixed `900x620` to a responsive minimum-plus-viewport calculation. This makes the panel grow on 3840-wide displays while preserving screen-margin clamping. The screenshot also showed `Gate: research mode on`, so `Spawn / Swap` was correctly disabled by config; this scale fix does not bypass proof gates.

## Human dialogue Mod Manager input scope

The dialogue input/cursor fix is recorded in `docs/research/human-dialogue-mod-manager-input-scope-2026-06-21.md`.

Version 0.1.18 keeps the debug panel input lock strict but matches Avalon Companions for the native `Companion` dialogue surface: Rewired axis/button reads pass while the dialogue-style surface is visible, and FoA Mod Manager controller cursor reads pass through. The human UI scope now tries FoA Mod Manager `SetCustomUiScope` first and falls back to Tainted Interface only when the manager API is unavailable.

This is a cursor/input ownership correction only. It does not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, save data, or release-ready human companion behavior.

## Human dialogue proven companion controller path

The proven controller-path correction is recorded in `docs/research/human-dialogue-proven-companion-controller-path-2026-06-21.md`.

Version 0.1.19 supersedes the 0.1.18 Mod Manager-first scope experiment and mirrors the working Avalon Companions controller lifecycle instead: Tainted Interface custom scope first, FoA Mod Manager fallback, shared `UpdateCursor()`, interactive cursor refresh inside `EnsureCursorForPanel()`, and input-module restore only after no human UI surface remains. The 0.1.18 Rewired dialogue pass-through remains because it is part of the proven companion input-lock path.

This is still a cursor/input controller correction only. It does not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, save data, or release-ready human companion behavior.

## Panel size and button state polish

The 0.1.10 panel size/state polish step is recorded in `docs/research/human-panel-size-state-polish-2026-06-16.md`.

Version 0.1.10 increases the proof panel size after screenshot evidence showed the 0.1.9 panel was still too small and vertically clipped. It also pins IMGUI button states and clears control focus after clicks so action buttons do not remain in unexpected colors.

The dev diagnostic map generator `tools/New-HumanNpcDiagnosticMap.ps1` was run against dev FOA-Diagnostic Tool dump `20260616-090615`. It generated `docs/generated/human-npc-diagnostic-map-20260616-090615.csv` and `.md` with 544 review-only rows: 42 `ReviewQueueNonUniqueSpawnerBacked`, 102 `NeedsSpawnerEvidence`, and 400 `BlockedUniqueOrStory`. All rows kept spawn, roster, behavior, and persistence approval false.

## Shared UI layer proof-panel styling

Version 0.1.11 may migrate only the existing proof command panel shell to the shared Tainted Interface style/scope path.

Approved:

- Prefer Tainted Interface shared dark-fantasy IMGUI styles when the optional runtime is installed.
- Prefer Tainted Interface custom UI scope for the proof panel when available, preserving the existing FoA Mod Manager scope fallback.
- Keep the local proof-panel style path as the no-layer fallback.
- Keep the panel disabled by default and scoped to `HumanCommandPanel.EnableProofCommandPanel`.

Not approved:

- New proof commands.
- Changes to spawn, scanner, ally marker, Hold, Dismiss, lifecycle, faction, save/load, persistence, or target-selection behavior.
- Claiming release-ready human companion UI without fresh in-game validation.

## Native Companion prompt bridge

The native Companion prompt bridge is recorded in `docs/research/human-native-companion-prompt-2026-06-21.md`.

Version 0.1.12 mirrors the working Avalon Companions command-menu pattern for the active one-session proof actor: when `HumanCommands.EnableNativeCommandActions=true` and `HumanCommandPanel.EnableProofCommandPanel=true`, the plugin attaches one runtime-only native `Companion` prompt to the active managed proof actor. Activating it opens the existing proof command panel.

While the proof panel gate is enabled, the older separate native quick command actions are removed so FoA has one prompt to resolve. This is an interaction bridge only. It does not add commands, recruitment, persistence, dialogue, save-backed state, existing-NPC conversion, custom target selection, or release-ready human companion behavior.

Version 0.1.13 makes those two command-surface gates default `true` for newly generated configs at the user's explicit request. The implementation boundary is unchanged: proof spawning, research-mode override, scanner output, defend assist, recruitment, persistence, existing-NPC conversion, true Wait, saved hold positions, dialogue, quest state, crime state, and release-ready companion behavior remain blocked or separately gated.

## Companion-style command surface

The companion-style command surface is recorded in `docs/research/human-companion-style-command-surface-2026-06-21.md`.

Version 0.1.14 put the vanilla-style Unity UI composition on the wrong surface. Version 0.1.15 corrects the split to match Avalon Companions: the native NPC `Companion` prompt opens the right-side dialogue choices and bottom dialogue text band, while `HumanCommandPanel.TogglePanelHotkey` opens a compact Avalon Companions Debug-style IMGUI control panel.

This is a surface/layout change only. The choices route to the existing proof backend and do not add recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, custom targeting, true Wait, saved hold positions, or release-ready human companion behavior.

## Human basic follow catch-up

The 0.1.21 Follow catch-up adjustment is recorded in `docs/research/human-basic-follow-catchup-2026-06-21.md`.

Version 0.1.21 keeps the approved one-session native ally proof lane but makes Follow usable as basic proof functionality: Follow can be reselected while already active, and Follow-mode bounded catch-up recall is clamped to a closer proof range so the managed actor stays visibly near the hero. Defend keeps the configurable catch-up distance.

This is still bounded recall on the plugin-owned proof actor only. It does not add custom pathfinding, custom destinations, custom combat targeting, recruitment, persistence, existing-NPC conversion, story graph dialogue, quest state, crime state, true Wait, saved hold positions, save data, or release-ready human companion behavior.

## Next validation

- Build 0.1.21, then validate Follow on a throwaway save: spawn/swap a proof actor, select Follow from the native `Companion` dialogue, walk beyond the close proof range, and confirm bounded catch-up keeps the actor near without affecting unrelated NPCs.
- Validate both surfaces on a throwaway save: native `Companion` prompt opens dialogue choices, and `End` opens the debug/control panel.
- Validate panel input lock on a throwaway save: open the panel, confirm camera/movement/action input does not pass through, then close with `End`, `Esc`, and Close and confirm input restores.
- Validate Hold on a throwaway save: spawn the proof actor, press Hold, confirm movement stops, confirm Recall repositions without changing Hold mode, and confirm Follow/Come Close/Defend/Dismiss remove the hold block.
- Continue the proof lifecycle/no-persistence gate recorded in `docs/research/human-proof-lifecycle-gate-2026-06-15.md`.
- Manually review the generated human NPC diagnostic map before any candidate is added to an implementation roster.
- Keep human companion behavior blocked until lifecycle cleanup, Hold cleanup, input lock, and candidate map review are validated.
