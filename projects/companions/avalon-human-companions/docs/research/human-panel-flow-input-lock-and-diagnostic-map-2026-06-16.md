# Human Panel Flow, Input Lock, and Diagnostic Map

Date: 2026-06-16

## Scope

Polish the Avalon Human Companions proof panel after the first Hold proof and prepare a diagnostic-only map of human NPC templates for later manual review.

This does not approve recruitment, persistence, existing NPC conversion, save-backed roster data, dialogue, quest state, inventory, equipment, leveling, release-ready UI, or a full human companion roster.

## Evidence read

- `docs/diagnostic-tool-development-policy.md`
- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-hold-command-2026-06-16.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/docs/research/command-polish-status-feedback-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- `mods/foa-mod-manager/src/Patches/GameInputPatch.cs`
- `mods/template-diagnostics/README.md`
- `mods/template-diagnostics/docs/validation-plan.md`

## Decision

Version 0.1.9 may polish the proof panel flow and focus behavior.

Approved panel changes:

- Reorganize the panel into the same practical flow used by Avalon Companions: target/active status, mode and safety status, evidence controls, then commands.
- Add explicit panel status lines for active proof actor distance, not-saved state, native ally marker state, Hold block state, and scanner state.
- Keep `Spawn Proof`, `Scan Actors`, `Follow`, `Hold`, `Come Close`, `Recall`, `Defend`, and `Dismiss` scoped to the active one-session proof actor.
- Add a `Lifecycle Check` button that writes the existing lifecycle evidence for the active proof actor and does not create persistence or recovery behavior.
- Keep the panel disabled by default and controlled by `HumanCommandPanel.EnableProofCommandPanel`.

Approved focus/input changes:

- Add a Harmony input lock based on the proven Avalon Companions and FoA Mod Manager pattern.
- While the human proof panel is open, block FoA `PlayerInput.ProcessLateUpdate`, zero movement/look fields, block Rewired axis/button reads, and block game mouse-position updates.
- Capture and temporarily disable Unity `BaseInputModule` instances while the panel is open, then restore them when the panel closes.
- Continue resetting input axes while the panel is open.

This is an input/focus lock, not a global gameplay pause or time-scale freeze. The implementation must not change `Time.timeScale`.

Approved diagnostic map work:

- Use the dev diagnostic tool lane under `mods/template-diagnostics` and the live dump folder `BepInEx/config/kane.tgfoa.template-diagnostics`.
- Generate repo-local human NPC diagnostic map files under `mods/avalon-human-companions/docs/generated`.
- Use `templates.csv`, `spawner_refs.csv`, `research_term_matches.csv`, and related dev diagnostic dump files as read-only evidence.
- Mark every row as diagnostic review only. `safeSpawnCandidate`, `rosterApproved`, `behaviorApproved`, and `persistenceApproved` must remain false.

## Not approved

- Editing `mods/Tainted-Diagnostic Tool`.
- Adding a saved human roster.
- Converting vanilla or existing NPCs.
- Persisting proof actors or restoring them after reload.
- True Wait/stay across scene transitions or reloads.
- Target selection, attack commands, custom hostility, crime, faction table, dialogue, quest, or story graph edits.
- Calling the panel release-ready without fresh screenshot or video validation.

## Validation needed

1. Build and deploy 0.1.9.
2. Confirm `Avalon Human Companions 0.1.9 loaded` in BepInEx.
3. Open the proof panel in game and confirm mouse/camera/player actions do not pass through while it is open.
4. Confirm `End`, `Esc`, and Close restore cursor/input state.
5. Confirm panel buttons still operate only on the active proof actor.
6. Run or review the dev FOA-Diagnostic Tool dump pipeline and generate human NPC diagnostic map files.
7. Confirm every diagnostic map row remains review-only and does not approve behavior or persistence.
