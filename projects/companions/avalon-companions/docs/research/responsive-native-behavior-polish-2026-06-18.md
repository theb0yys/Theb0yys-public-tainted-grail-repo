# Research: Responsive Native Behavior Polish

Date: 2026-06-18
Scope: make managed companion commands feel more responsive while staying inside the approved one-session native ally behavior path.

## Evidence read

- `docs/engineering-process.md`
- `docs/foa-modding-environment.md`
- `docs/research/README.md`
- `docs/mod-lifecycle.md`
- `docs/code-review-standard.md`
- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/advanced-native-ally-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/session-continuity-persistence-gate-2026-06-16.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Current evidence

- Follow catch-up for one-session creature candidates already runs through Avalon's existing managed recall path on a throttled tick.
- Defend response already stays inside the native `NpcHeroPetAlly.EnterCombat()` path and only acts when `Hero.Current.PossibleAttackers` contains live attackers.
- Per-companion defend prompt cooldowns already prevent repeated `EnterCombat()` calls during sustained combat.
- Existing command paths already cover summon/swap, recall, Come Close, follow range changes, Follow/Stay/Defend mode changes, managed recovery, panel commands, hotkeys, and native quick commands.

## Decision

Avalon Companions 0.1.15 may add a responsive native refresh that resets only the controller's next follow/defend tick timestamps after explicit player or native command actions. This lets the next update evaluate the existing catch-up and native defend paths immediately instead of waiting for their normal 0.75 second follow tick or 0.35 second defend tick.

Approved command triggers:

1. Summon/swap or recalling an already active selected companion.
2. Recall and Come Close.
3. Follow range changes.
4. Follow, Stay, or Defend mode changes.
5. Managed recovery.
6. Opening the native `Companion` command menu or running a fallback native quick command.
7. Manual lifecycle check after it runs the existing lifecycle guard.

## Boundary

This polish does not add:

- custom AI or pathfinding,
- arbitrary target scans,
- attack buttons,
- forced hostility,
- target selection,
- defend hotkeys,
- healing, resurrection, or respawn,
- actor persistence,
- reload restoration,
- same-session re-adoption after reload,
- active squads,
- humanoid companions.

The responsive refresh must not clear per-companion defend prompt cooldowns. If a native defend prompt just ran, the next immediate tick still observes the existing cooldown and must not call `EnterCombat()` again until the cooldown expires or attackers clear through the existing path.

## Validation needed

- Debug build.
- `git diff --check`.
- Throwaway-save smoke when available:
  - summon a managed one-session candidate and confirm command actions still work,
  - change Follow/Stay/Defend modes and confirm no Attack button or target selector appears,
  - let a live attacker engage the hero and confirm native defend remains cooldown-throttled,
  - run Recall, Come Close, range changes, Recover, and Lifecycle Check and confirm command rows keep `touchesTargeting=false` and `touchesPersistence=false`,
  - confirm no companion auto-restores after reload.
