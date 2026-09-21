# Research: Metadata Status Continuity

Date: 2026-06-16
Scope: implement the approved 0.1.14 metadata/status continuity slice without actor persistence, reload restoration, respawn, or re-adoption.

## Evidence read

- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/docs/research/session-continuity-evidence-review-2026-06-16.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Avalon Companions 0.1.14 may implement only plugin-owned continuity metadata:

- remember the last selected companion GUID and display name,
- remember the last command mode,
- mirror the current follow range,
- show remembered status when no active managed actor exists,
- tell the player to use the existing explicit `Summon / Swap` command,
- write `continuity-status` command-log rows with `touchesTargeting=false` and `touchesPersistence=false`.

The existing `Summon / Swap` command remains the only actor creation path. Remembered metadata may affect panel/status text and the defaults used by a later explicit player command, but it must not create, restore, respawn, save, or re-adopt actors.

## Boundary

This slice does not approve:

- actor persistence,
- automatic companion restoration after load,
- automatic respawn,
- same-session actor re-adoption after reload,
- changing managed actors or native command actions to saved ownership,
- healing, resurrection, squads, target selection, attack commands, custom AI, or humanoid companions.

## Validation needed

- Build validation for the 0.1.14 code change.
- In-game throwaway-save continuity smoke:
  - select and summon one managed one-session candidate,
  - set Follow, Stay, Defend, and at least one follow range,
  - dismiss or reload so no managed actor is active,
  - confirm the panel reports remembered metadata only,
  - confirm no actor appears until the player explicitly uses `Summon / Swap`,
  - inspect `companion-command-log.csv` for `continuity-status` rows with `touchesPersistence=false` and `touchesTargeting=false`.
