# Human Panel Scanner Trigger

Date: 2026-06-15

## Scope

Add one proof-panel button that triggers the existing disabled actor scanner because the configured scanner hotkey did not produce CSV output or a BepInEx scanner log line during the latest live check.

This does not approve recruitment, conversion, persistence, custom combat, dialogue, faction edits, targeting edits, or behavior for existing NPCs.

## Evidence read

- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/review-notes.md`
- `mods/avalon-human-companions/docs/research/human-actor-scanner-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `docs/in-game-ui-quality-standard.md`

## Decision

Avalon Human Companions may add a `Scan Actors` button to the proof panel only if it calls the existing `HumanActorScanner.WriteSnapshot` path.

The button must:

- require the existing `ActorScanner.EnableDisabledActorScanner` config gate,
- use the existing configured scan radius and selected target,
- write only the existing plugin-owned CSV files under `BepInEx/config/kane.tgfoa.avalon-human-companions`,
- show only a status message after the scan is requested,
- avoid spawning, moving, commanding, dismissing, recruiting, converting, faction-editing, target-editing, story-editing, crime-editing, interaction-editing, or persisting actors.

The button must not:

- bypass the scanner config gate,
- run every frame,
- create a new hotkey,
- add any behavior command,
- approve existing live actors for companion behavior.

## Validation needed

- Build and deploy.
- Confirm the panel opens after restart/apply.
- Confirm `Scan Actors` is enabled only when `ActorScanner.EnableDisabledActorScanner=true`.
- Press `Scan Actors` in game and confirm these files are written:
  - `human-actor-candidates.csv`
  - `human-actor-components.csv`
  - `human-actor-command-dry-run.csv`
- Review the CSVs before implementing any new human companion behavior.

## First validation result

- `LogOutput.log` confirmed 0.1.6 loaded and the proof command panel opened.
- `Scan Actors` wrote all three expected CSV files under the plugin config folder.
- The scanner logged `9` candidate rows, `28` component rows, and `6` dry-run command rows.
- CSV review confirmed all dry-run command rows remained `blocked=true` and `liveAction=false`.
- The reviewed target GUID `2bd34a05d1e1fb94f9770b9ee7f23be2` was not present because the scan ran before the one-session proof actor was created.
- Required follow-up: spawn the proof actor first, then run `Scan Actors` again.
