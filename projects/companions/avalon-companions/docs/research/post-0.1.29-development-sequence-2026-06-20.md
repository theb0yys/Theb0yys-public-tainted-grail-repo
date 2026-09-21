# Research: Post-0.1.29 Development Sequence

Date: 2026-06-20
Scope: record the next Avalon Companions development gates after the 0.1.29 HUD icon overlay slice.
Question: What should move next without jumping into unresearched advanced behavior?

## Evidence read

- `mods/avalon-companions/docs/research/panel-debug-native-dialogue-roadmap-2026-06-19.md`
- `mods/avalon-companions/docs/research/dialogue-direct-click-gate-2026-06-20.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/README.md`

## Approved sequence

1. 0.1.30 command-surface polish.
2. Native dialogue decision gate.
3. Lifecycle validation gate.
4. Core integration gate.
5. Custom AI research gate.

## 0.1.30 command-surface polish

0.1.30 may improve the existing plugin-owned Unity UI command surface only:

- Better hover and selected states.
- Clearer disabled-state presentation.
- Consistent close behavior across Goodbye, close, Esc, and command execution.
- Stronger command audit rows that clarify source surface, command, selected companion, result, and whether command execution closed the dialogue.

0.1.30 must not add new companion commands, native story dialogue, custom AI/pathing, target scans, attack commands, taming, training, loyalty, persistence, resurrection, healing, squads, Core-executed behavior, or actor restoration.

## Native dialogue decision gate

After command-surface polish, Avalon Companions must either:

- prove a safe real `VDialogue`/`StoryBookmark` route using valid game data or a proven runtime registration path, or
- formally keep the plugin-owned Unity UI as the supported companion dialogue surface.

No fake story bookmarks, template edits, save-owned interaction list edits, or runtime story graph assumptions are approved by this sequence.

## Lifecycle validation gate

Before persistence or restoration work, run throwaway-save validation for:

- transition,
- rest,
- quit and reload,
- return,
- duplicate cleanup,
- orphan cleanup,
- not-saved behavior.

This gate validates the existing one-session companion lane. It does not approve actor persistence, reload restoration, auto-respawn, saved ownership, or re-adoption.

## Core integration gate

Core integration may begin only after the framework and command surface are stable. The first Core slice must be diagnostic/profile registration behind gates.

The Core gate must not execute companion behavior through Core yet, must not add Core callbacks, and must not move command execution, AI, targeting, persistence, or lifecycle ownership into Core.

## Custom AI research gate

Custom AI remains research-only until native follow/pathing/combat/targeting boundaries are inspected and documented.

Required research targets include:

- native follow/pathing components,
- combat state ownership,
- target selection boundaries,
- hostile filtering,
- interaction with the existing `NpcHeroPetAlly` path,
- failure behavior in towns, stealth, interiors, quests, and transitions.

No custom AI logic, attack commands, target selectors, taming, training, or loyalty systems are approved until this research is complete.
