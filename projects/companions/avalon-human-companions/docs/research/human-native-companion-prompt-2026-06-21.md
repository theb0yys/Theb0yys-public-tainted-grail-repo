# Human Native Companion Prompt Bridge

Date: 2026-06-21

## Scope

Repair the human proof actor's native interaction route by mirroring the working Avalon Companions command-menu pattern: one runtime-only native `Companion` prompt opens the plugin-owned proof command panel for the active one-session proof actor.

This does not add recruitment, persistence, existing-NPC conversion, custom target selection, true Wait, saved hold positions, dialogue, quest state, crime state, or release-ready human companion behavior.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/validation-plan.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `mods/avalon-companions/docs/research/native-quick-commands-2026-06-15.md`
- `mods/avalon-companions/src/Patches/AvalonCompanionCommandAction.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Current problem

Avalon Human Companions 0.1.3 added separate native quick command action elements for the active proof actor, but live testing reported that path did not work. Avalon Companions already hit the same native interaction limitation: multiple quick command actions were not a reliable normal command surface because FoA resolves the first/default available action.

The human proof panel added in 0.1.4 became the validated command route, but it was only hotkey opened.

## Decision

Version 0.1.12 may add one runtime-only native `Companion` action to the active plugin-owned proof actor when both gates are enabled:

- `HumanCommands.EnableNativeCommandActions=true`
- `HumanCommandPanel.EnableProofCommandPanel=true`

Activating the prompt opens the existing proof command panel and writes a lifecycle evidence row. The panel continues to run the already approved proof commands: Spawn Proof, Scan Actors, Lifecycle Check, Follow, Hold, Come Close, Recall, Defend, and Dismiss.

When the proof panel gate is enabled, the plugin removes the older separate quick command action elements from the proof actor so FoA has only one native prompt to resolve.

## Boundary

Allowed:

- Attach one plugin-owned `HumanCompanionCommandAction : AbstractLocationAction` labelled `Companion`.
- Mark the action not saved.
- Make it available only for the active managed proof actor with `NpcHeroPetAlly`.
- Open the existing proof command panel.
- Keep existing command backend and lifecycle guard behavior unchanged.

Not allowed:

- Adding new commands.
- Attaching commands to existing NPCs, unrelated spawned actors, unique/story actors, or wild NPCs.
- Writing to vanilla serialized interaction lists, story graphs, dialogue bookmarks, quest state, crime state, save data, global factions, or actor persistence.
- Claiming human companions are release-ready.

## Validation needed

- Release build.
- Throwaway-save runtime check: spawn the proof actor, interact with it, confirm one native `Companion` prompt appears, confirm it opens the existing proof panel, run Follow or Hold, then Dismiss.
- Confirm older quick command prompts do not appear beside the single `Companion` prompt while the proof panel gate is enabled.
- Confirm `human-proof-lifecycle.csv` includes the `native-companion-prompt` row and still keeps `behaviorApproved=false` and `persistenceApproved=false`.
- Continue the lifecycle/no-persistence gate before adding any more human behavior.
