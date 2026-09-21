# Research: Lifecycle Validation Checkpoint

Date: 2026-06-15
Scope: make transition/save validation easier without adding persistence or new companion ownership.

## Evidence read

- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Avalon Companions 0.1.12 may add lifecycle validation polish only:

1. Show a lifecycle status line in the existing Avalon command panel.
2. Add a panel-only `Lifecycle Check` diagnostic button.
3. Reuse the existing lifecycle safety pass with `forceDump=true`.
4. Write a command-log row for the manual checkpoint.
5. Keep all checkpoint output under the existing `companion-lifecycle.csv` and `companion-command-log.csv` diagnostic paths.

This is a validation helper for fast travel, area transition, rest, quit, reload, and return tests. It does not create or restore companions.

## Boundary

This does not approve:

- persistent companion saves,
- reload respawn,
- companion restoration,
- active squads,
- new travel hooks,
- custom save ownership,
- target selection,
- attack commands,
- healing,
- humanoid companions.

## Validation needed

- Debug and Release build validation.
- Live DLL hash/version verification.
- In-game lifecycle smoke test:
  - summon one managed one-session candidate,
  - press `Lifecycle Check`,
  - confirm `companion-lifecycle.csv` gets a `panel-lifecycle-check` row,
  - fast travel or transition,
  - press `Lifecycle Check` again,
  - confirm no duplicate, orphan prompt, or persistent companion claim appears.
