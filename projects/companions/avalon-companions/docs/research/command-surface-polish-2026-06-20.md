# Research: Command Surface Polish

Date: 2026-06-20
Scope: polish the existing plugin-owned Unity UI companion command surface after the 0.1.29 HUD icon overlay slice.
Question: Can Avalon Companions improve command surface feedback and audit evidence without adding behavior?

## Evidence read

- `mods/avalon-companions/docs/research/post-0.1.29-development-sequence-2026-06-20.md`
- `mods/avalon-companions/docs/research/dialogue-direct-click-gate-2026-06-20.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Findings

- The Unity UI dialogue host already has selected-state colors, direct pointer fallback, and command routing through the approved command delegates.
- The visible command rows can be polished locally by adding non-interactive state accents, clearer disabled text color, and manual hover feedback based on the existing row rectangles.
- Command audit rows already exist in `companion-command-log.csv`; the 0.1.30 slice can strengthen them by adding dialogue-surface rows in the existing `reason` field without changing the CSV schema or command behavior.
- Close behavior is split across command execution, Goodbye, Esc, and debug-panel toggle. A shared close helper can make those routes consistent while preserving command status text.

## Approved route for 0.1.30

- Add visual state accents for hover, selected, danger, and disabled dialogue rows.
- Keep selected command state tied only to existing mode and range values.
- Use muted disabled text and disabled accent color for unavailable companion commands.
- Route Unity button clicks and manual hit-test clicks through one dialogue choice handler.
- Add dialogue audit rows for choice route, command id, label, result, and whether the dialogue closed.
- Route Goodbye, Esc, debug-panel toggle, and post-command close through one close helper.

## Not approved

- New commands or changed command effects.
- Native `VDialogue`, `StoryBookmark`, story graph content, template edits, or save-owned interaction edits.
- Custom AI/pathing, target scans, attack commands, healing, resurrection, persistence, taming, training, loyalty, squads, Core-executed behavior, or actor restoration.

## Validation needed

- Debug build: passed with 0 warnings and 0 errors.
- Release build: passed with 0 warnings and 0 errors.
- Release deploy and live DLL hash check: passed. Built/live file version `0.1.30.0`, SHA-256 `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- In-game smoke remains pending: open `Companion`, verify hover/selected/disabled visuals, click Follow and Stay, click Goodbye, press Esc, confirm `KeypadPeriod` still opens only `Avalon Companions Debug`, and inspect `companion-command-log.csv` for `dialogue-choice` and `dialogue-close` audit rows.
