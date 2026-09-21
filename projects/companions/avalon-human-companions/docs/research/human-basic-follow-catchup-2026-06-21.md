# Human Basic Follow Catch-Up - 2026-06-21

## Request

After the native `Companion` dialogue and input path started working in game, the next requested basic functionality was Follow from the human companion dialogue choices.

## Research read

- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-one-session-hold-command-2026-06-16.md`
- `mods/avalon-companions/docs/research/advanced-native-ally-behavior-2026-06-15.md`
- `mods/avalon-companions/docs/research/actor-control-proof-design-2026-06-14.md`
- `mods/avalon-human-companions/src/Plugin.cs`
- Decompiled `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
- Decompiled `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
- Decompiled `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`
- Decompiled `Awaken.TG.Main.AI.Movement.Controllers.NpcController`

## Findings

- The approved human proof lane is still the single plugin-owned, one-session proof actor with the native summon faction and `NpcHeroPetAlly` marker.
- The command-surface research explicitly allows Follow mode and a bounded catch-up recall in Follow or Defend mode using `Location.MoveAndRotateTo(...)` on the plugin-owned proof actor only.
- `NpcHeroPetAlly` inherits `NpcHeroSummon`, which inherits `NpcAlly`. The native ally base owns the real follow/patrol behavior, but the current human proof cannot assume all humanoid enemy templates will move like creature pet templates.
- The current Follow implementation set mode to Follow but made Follow unavailable while already selected and only recalled at the general `HumanCommands.FollowCatchUpDistance` value. In the live dialogue this made Follow look passive after the first selection.
- Custom pathing, custom target selection, persistent follow state, existing-NPC conversion, and true wait/stay behavior remain outside the approved research.

## Decision

Version 0.1.21 keeps the native proof boundary and makes Follow usable as basic proof functionality:

- Follow remains runtime-only and applies only to the managed one-session proof actor.
- Follow can be selected again while already in Follow mode, which re-arms the mode and removes any proof-only Hold movement block.
- Follow uses the already-approved bounded catch-up recall, but clamps the Follow-mode catch-up distance to a closer proof range so the actor stays visibly near the hero.
- Defend keeps using the configurable catch-up distance.
- The implementation does not call custom navigation, does not issue custom destinations, does not add persistent state, and does not touch any non-managed NPC.

## Boundary

This is still not full humanoid companion AI. It does not add recruitment, persistence, existing-NPC conversion, story dialogue, inventory, equipment, leveling, custom pathfinding, custom combat targeting, true Stay/Wait, saved hold positions, save data, or release-ready human companion behavior.
