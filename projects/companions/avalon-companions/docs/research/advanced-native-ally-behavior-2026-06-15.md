# Research: Advanced Native Ally Behavior

Date: 2026-06-15
Scope: improve one-session companion behavior while staying inside the approved native `NpcHeroPetAlly` path.

## Evidence read

- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/advanced-command-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/offensive-creature-roster-expansion-2026-06-15.md`
- `mods/avalon-companions/docs/research/heel-placement-safety-hotfix-2026-06-15.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

Avalon Companions 0.1.10 may add a small advanced behavior layer for managed one-session companions:

1. Count live `Hero.Current.PossibleAttackers` before any defend prompt.
2. Track defend state transitions so the log records when native defend is armed and when it clears.
3. Prompt `NpcHeroPetAlly.EnterCombat()` through the native ally path only when the hero already has live attackers.
4. Apply a per-managed-companion cooldown to repeated automatic defend prompts so Avalon does not call `EnterCombat()` every defend tick.
5. Reset the defend cooldown when the hero no longer has live attackers.
6. Add richer command CSV reasons for native defend prompts and automatic catch-up recalls.

This is behavior coordination around the already approved native ally path. Avalon still does not pick a target, scan nearby actors, force hostility, add an attack command, or persist companions.

## Boundary

This does not approve:

- manual `Attack` buttons,
- arbitrary target selection,
- forcing hostility against nearby actors,
- town or civilian aggression logic,
- custom pathfinding or combat AI,
- healing or health restoration,
- persistence, reload respawn, or companion save records,
- active squads,
- humanoid companions,
- true dialogue/story graph command menus.

## Validation needed

- Debug and Release build validation.
- Live DLL hash/version verification.
- In-game smoke test with a managed offensive or undead candidate:
  - no attackers: Defend mode should wait and not call native combat repeatedly,
  - first live hero attacker: command log should record `defend-armed` and one `defend-prompt` or `auto-defend-prompt`,
  - continued combat: prompts should be throttled by the cooldown,
  - attackers cleared: command log should record `defend-clear`,
  - dismiss during or after combat should remove the managed actor and native command action cleanly.
- Confirm no `Attack` or target-selection UI is exposed.
