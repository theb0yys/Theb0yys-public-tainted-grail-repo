# Human One-Session Command Surface

Date: 2026-06-15

## Scope

Add the first command surface for the active one-session human ally proof actor. This is not recruitment, persistence, dialogue, inventory, equipment, leveling, or a full companion panel.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/research/human-one-session-native-ally-proof-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `docs/in-game-ui-quality-standard.md`

## Decision

Avalon Human Companions may add runtime-only native quick command actions to the single managed one-session human ally proof actor after it has the native `NpcHeroPetAlly` marker.

This follows the safer Avalon Companions route:

- attach plugin-owned `AbstractLocationAction` elements only to a tracked one-session actor,
- mark every action and the actor as not saved,
- remove those actions on dismiss or invalid state,
- never mutate templates, story graphs, dialogue bookmarks, vanilla serialized interaction lists, quest state, crime state, or save data.

## Approved commands

- `Follow`: sets proof mode to follow. Runtime-only.
- `Come Close`: sets follow mode and recalls the proof actor near the hero. Runtime-only.
- `Recall`: recalls the proof actor near the hero. Runtime-only.
- `Dismiss`: discards the managed proof actor. Runtime-only.
- `Defend`: sets defend mode and may call `NpcHeroPetAlly.EnterCombat()` only when `Hero.Current.PossibleAttackers` already has a live attacker.

The implementation may also perform a bounded catch-up recall in Follow or Defend mode. This uses `Location.MoveAndRotateTo(...)` on the plugin-owned proof actor only.

## Not approved

- Converting existing NPCs.
- Adding commands to any non-managed actor.
- Custom target selection, arbitrary attack commands, forced hostility, or town/civilian aggression logic.
- True `Stay`, `Wait`, or `Hold Position` behavior until a humanoid ally wait API is researched and validated.
- True dialogue or story choices.
- A custom IMGUI panel or release-ready custom UI.
- Persistent roster records, save reconstruction, transition reconstruction, or quit/relaunch reconstruction.
- Editing vanilla templates, global faction data, crime data, story graphs, or interaction lists.

## Validation needed

- Build and deploy 0.1.3.
- Spawn the one-session ally proof on a throwaway save.
- Confirm native command actions appear only on that proof actor.
- Confirm `Come Close` or `Recall` repositions the proof actor near the hero.
- Confirm `Follow` allows catch-up.
- Confirm `Dismiss` removes the proof actor and the commands disappear.
- Confirm no command actions appear on unrelated NPCs.
- Confirm no `Stay`, `Wait`, or `Hold Position` command is exposed in 0.1.3.
- Confirm no save is continued after this proof.
