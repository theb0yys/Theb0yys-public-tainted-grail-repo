# Research: Summon Panel Polish

Date: 2026-06-28
Scope: polish the `KeypadPeriod` Avalon Companions panel after the player-facing Unity UI companion dialogue became the supported command surface.
Question: Which controls belong on the summon/debug panel now that companion orders are available through the native `Companion` prompt dialogue?

## Evidence read

- `mods/avalon-companions/docs/research/panel-debug-native-dialogue-roadmap-2026-06-19.md`
- `mods/avalon-companions/docs/research/command-surface-polish-2026-06-20.md`
- `mods/avalon-companions/README.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

The `KeypadPeriod` panel should be a polished roster, summon, lifecycle, and diagnostic surface. The player-facing companion order controls should remain on the `Companion` Unity UI dialogue.

Allowed:

- Rename/reframe the panel header as a companion roster/summon surface while keeping it behind `Debug.EnableDebugPanel`.
- Remove duplicated order controls from the panel: Follow, Hold Position, Defend, Close/Normal/Far range, Come Close, and Recall.
- Keep roster selection, `Summon / Swap`, active safety controls (`Recover Active`, `Dismiss Active`), lifecycle check, and AI diagnostic buttons.
- Show compact selected/active companion status, trust/bond policy, lifecycle, and active companion rows.
- Preserve existing command methods and diagnostics; do not change actor ownership or command behavior.

Not allowed:

- New companion orders.
- New summon targets.
- Native stat buffs.
- Target overrides, movement overrides, custom AI, persistence, taming, training, saved progression, or Core-executed behavior.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- In-game smoke: open `KeypadPeriod`, confirm the panel is focused on roster/summon/diagnostics, confirm redundant order rows are gone, summon/swap still works, `Companion` dialogue still owns orders, and close/Esc restores input.
