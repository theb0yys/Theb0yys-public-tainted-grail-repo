# Research: Managed Companion Recovery

Date: 2026-06-16
Scope: add a conservative recovery command for already-managed one-session companions without adding persistence, respawn, healing, custom AI, or target selection.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/docs/research/advanced-command-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/heel-placement-safety-hotfix-2026-06-15.md`
- `mods/avalon-companions/docs/research/advanced-native-ally-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/lifecycle-validation-checkpoint-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`

## Current approved behavior

- Managed roster actors are one-session only.
- Creature candidates must already be in Avalon's runtime tracking list and use the native summon-faction plus `NpcHeroPetAlly` path.
- Recall and catch-up already use Avalon's existing placement path and native `BaseLocationSpawner.VerifyPosition(...)`.
- Lifecycle safety already marks managed actors not saved, discards untracked one-session roster allies, discards duplicate active roster actors, and writes CSV evidence.
- Command logging already records managed command decisions and keeps targeting/persistence flags false.

## Decision

Avalon Companions 0.1.13 may add managed companion recovery inside the existing one-session native ally path only:

1. Add a `Recover` command to the Avalon panel and fallback quick-command cycle.
2. Run the existing lifecycle safety pass first, with a forced CSV snapshot.
3. Inspect only active managed roster actors returned by the existing roster/native-ally collection path.
4. Treat discarded locations as invalid and remove them from runtime tracking.
5. Treat managed actors without required live components as invalid and discard them if the lifecycle safety guard is enabled.
6. Treat a managed `NpcElement` whose `ICharacter` view reports dead as dead, remove native command actions, mark it not saved, and discard it if the lifecycle safety guard is enabled.
7. Treat live managed actors beyond the current Far catch-up threshold as stuck/out-of-range and recall them through the existing verified placement path.
8. Write `companion-command-log.csv` rows for recovery actions and keep `touchesTargeting=false` and `touchesPersistence=false`.
9. Keep the normal lifecycle snapshot available through `companion-lifecycle.csv`.

## Boundary

This does not approve:

- healing,
- resurrection,
- respawn,
- reload restoration,
- companion persistence,
- active squads,
- custom pathing or AI,
- target selection,
- attack commands,
- humanoid companions,
- wild actor conversion,
- save-owned native interaction edits,
- story or dialogue graph changes.

If a managed one-session actor is dead, the 0.1.13 recovery path removes it instead of healing or reviving it. The player can explicitly summon a new one-session companion afterward.

## Validation needed

- Debug and Release build validation.
- Live DLL hash/version verification.
- In-game recovery smoke test on a throwaway save:
  - summon one managed one-session candidate,
  - move it far away or force a stuck/out-of-range state,
  - press `Recover`,
  - confirm it recalls through existing placement without duplicate spawn,
  - kill or invalidate a managed candidate,
  - press `Recover`,
  - confirm it is removed without healing, resurrection, persistence, or target selection,
  - inspect `companion-command-log.csv` for `recover` rows and `companion-lifecycle.csv` for forced lifecycle rows.
