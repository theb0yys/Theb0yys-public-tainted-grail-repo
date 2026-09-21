# Research: Command Polish Status Feedback

Date: 2026-06-15
Scope: improve command feedback for managed companions without adding new behavior paths.

## Evidence read

- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/advanced-command-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/advanced-native-ally-behavior-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Avalon Companions 0.1.11 may add command polish only:

1. Show per-active-companion status in the Avalon command panel.
2. Include display name, distance from the hero, current command mode, native ally marker status, native prompt status, and not-saved state.
3. Make Follow, Stay, and Defend mode feedback describe what the mode is doing.
4. Keep all command buttons unchanged.

This is UI/status feedback over existing managed state. It does not command actors in a new way.

## Boundary

This does not approve:

- Attack buttons,
- target selection,
- target lists,
- custom hostility scans,
- custom combat AI,
- healing,
- persistence,
- squads,
- humanoid companion behavior,
- true dialogue/story graph command menus.

## Validation needed

- Debug and Release build validation.
- Live DLL hash/version verification.
- In-game panel smoke test:
  - with no active companion, status shows no active companion,
  - after summon, status shows one managed companion with distance and not-saved state,
  - Follow/Stay/Defend buttons update mode feedback,
  - no Attack button or target selector appears.
