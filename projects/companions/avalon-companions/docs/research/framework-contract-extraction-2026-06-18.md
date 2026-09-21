# Research: Framework Contract Extraction

Date: 2026-06-18
Scope: begin companion framework polish by extracting internal contract types from the working controller without changing runtime behavior.

## Evidence read

- `docs/engineering-process.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/AvalonCompanions.csproj`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/NativeCompanionCommand.cs`

## Decision

Avalon Companions 0.1.16 may extract internal companion framework contracts while preserving the 0.1.15 native-safe behavior:

1. Move companion command/mode/range/recovery enum contracts into `AvalonCompanions.Framework`.
2. Move the reviewed roster entry data record into `AvalonCompanions.Framework`.
3. Keep native action classes, controller methods, command routing, lifecycle logic, and UI behavior unchanged.
4. Do not add an Avalon Core dependency yet. This slice prepares stable internal seams for a later Core service contract.

## Boundary

This does not approve:

- custom AI or pathfinding,
- new command behavior,
- attack buttons,
- arbitrary target selection,
- persistence,
- reload restoration,
- respawn,
- active squads,
- humanoid companions,
- Avalon Core runtime integration.

## Validation needed

- Debug build.
- Release build/deploy only if a live test is needed.
- `git diff --check`.
- In-game smoke may reuse the existing 0.1.15 command checklist because behavior is intended to be unchanged.
