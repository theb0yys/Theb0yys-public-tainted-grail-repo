# Human Role-Aware Brain

Date: 2026-06-28

## Scope

Expand the 0.3.0 companion brain so the four starter human proof candidates no longer share one generic recall placement. This is still runtime-only logic for the active one-session proof actor.

## Evidence read

- `mods/avalon-human-companions/docs/research/human-companion-brain-2026-06-28.md`
- `mods/avalon-human-companions/docs/research/human-proof-candidate-roster-2026-06-21.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/src/Plugin.cs`

## Decision

Version 0.3.1 may add `HumanBrain.EnableRoleAwarePlacement`, default true for newly generated configs. When enabled, the selected starter role changes only the bounded recall offset around the hero:

- One-handed melee stays close behind and right.
- Heavy melee stays close behind and left.
- Spear and shield stays centered close behind.
- Ranged support keeps a wider rear-left spacing.

Explicit `Recall` and `Come Close` keep the close placement so player commands remain predictable. Brain catch-up and combat recall use the selected role profile.

## Boundary

This does not add pathfinding, patrol destinations, target selection, attack commands, AI package edits, recruitment, persistence, save-backed roles, existing-NPC conversion, dialogue, quest, crime, global faction edits, or save data.
