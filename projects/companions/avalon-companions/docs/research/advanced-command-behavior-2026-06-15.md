# Research: Advanced Command Behavior

Date: 2026-06-15
Scope: add the smallest safe command-behavior improvement after the lifecycle gate, without adding custom target selection or persistence.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/actor-control-proof-design-2026-06-14.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Plugin.cs`

## Decision

Avalon Companions 0.1.3 may add advanced command behavior only where it stays inside existing approved controls:

1. Add a `Heel` command that sets Follow mode, switches to close follow range, and recalls active managed roster companions near the hero.
2. Add a persisted follow range profile with `Close`, `Normal`, and `Far` values.
3. Use the range profile only for Avalon recall placement and one-session candidate catch-up distance.
4. Add `companion-command-log.csv` so summon, swap, recall, heel, range, mode, dismiss, and auto catch-up decisions can be reviewed after a smoke test.

These changes are command ergonomics and diagnostics for managed roster actors only. They do not add a new AI owner, custom pathing, custom combat target selection, manual attack orders, save ownership, or reload restoration.

## Boundary

The following remain blocked:

- manual `Attack` buttons,
- selecting arbitrary targets,
- forcing hostility against nearby actors,
- defend hotkeys outside the existing panel mode,
- custom faction matrices,
- persistent companions or reload respawn,
- active squads,
- humanoid companions,
- wild creature adoption.

`Defend` remains limited to the already approved native path: when a managed one-session candidate has `NpcHeroPetAlly` and `Hero.Current.PossibleAttackers` contains a live attacker, Avalon may prompt `NpcHeroPetAlly.EnterCombat()`. The native game code still chooses the combat target.

## Validation needed

- Debug and Release build validation.
- Confirm `Avalon Companions 0.1.3 loaded` in BepInEx logs.
- Confirm `Heel` sets Close range, Follow mode, and recalls active managed companions.
- Confirm `Close`, `Normal`, and `Far` affect placement/catch-up distance without spawning duplicates.
- Confirm `companion-command-log.csv` writes rows with `touchesTargeting=false` and `touchesPersistence=false` for the new commands.
- Confirm Defend still uses only the native hero-attacker path.
- Confirm no `Attack` or custom target-selection command is exposed.
