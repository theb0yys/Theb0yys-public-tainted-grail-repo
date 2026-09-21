# Human Companion Brain

Date: 2026-06-28

## Scope

Add a runtime-only companion brain for the active one-session human proof actor. This is logic over existing approved primitives, not persistence, recruitment, custom pathfinding, custom target selection, or existing-NPC conversion.

## Evidence read

- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-hold-command-2026-06-16.md`
- `mods/avalon-human-companions/docs/research/human-basic-follow-catchup-2026-06-21.md`
- `mods/avalon-companions/docs/research/profile-driven-native-assist-2026-06-21.md`
- `mods/avalon-companions/docs/research/profile-driven-native-assist-smoke-fix-2026-06-21.md`

## Decision

Version 0.3.0 may add `HumanBrain.EnableCompanionBrain`, default true for newly generated configs. When enabled, the brain owns the active proof actor's periodic command loop and reuses only:

- the existing bounded recall path through `Location.MoveAndRotateTo(...)`,
- the existing native ally defend path through `NpcHeroPetAlly.EnterCombat()`,
- the existing runtime Hold movement block by respecting Hold and not moving or defending while Hold is selected.

The brain may:

- recall in Follow or Defend when the actor is too far,
- recall closer before Defend prompts while the hero has live attackers,
- prompt native defend in Defend mode with a cooldown,
- write lifecycle evidence rows for threat detection, threat clear, and brain defend prompts,
- expose current brain state in the debug panel.

## Boundary

The brain must not choose targets, scan nearby enemies for attack targets, override combat targets, add attack buttons, add custom movement destinations beyond bounded recall placement, save companion state, restore after reload, recruit existing NPCs, convert existing NPCs, or touch dialogue, quest, crime, faction tables, or save data.
