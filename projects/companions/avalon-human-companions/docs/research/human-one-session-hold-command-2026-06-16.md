# Human One-Session Hold Command

Date: 2026-06-16

## Scope

Research the next bounded command for the active one-session human proof actor after Follow, Come Close, Recall, Defend, and Dismiss.

This does not approve recruitment, persistence, existing NPC conversion, dialogue, quest behavior, inventory, equipment, leveling, release-ready UI, or save-backed roster data.

## Evidence read

- `mods/avalon-human-companions/docs/research.md`
- `mods/avalon-human-companions/docs/design.md`
- `mods/avalon-human-companions/docs/research/human-one-session-command-surface-2026-06-15.md`
- `mods/avalon-human-companions/docs/research/human-proof-command-panel-2026-06-15.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- Local `TG.Main.dll` decompile with `ilspycmd` 10.1.0.8386

## Local TG.Main.dll evidence

Assembly:

```text
A:\SteamLibrary\steamapps\common\Tainted Grail FoA\Fall of Avalon_Data\Managed\TG.Main.dll
```

Relevant decompiled types:

- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
- `Awaken.TG.Main.Fights.NPCs.Providers.NpcCanMoveHandler`
- `Awaken.TG.Main.Fights.NPCs.Providers.ICanMoveProvider`
- `Awaken.TG.Main.Locations.Pets.PetElement`
- `Awaken.TG.Main.Locations.Pets.Variants.PetVariantBase`

Findings:

- `NpcHeroPetAlly` inherits from `NpcHeroSummon` and does not expose a public `SetFollowing`, `Stay`, `Wait`, or `Hold` method.
- `NpcHeroSummon` handles native summon ownership, transition teleport, combat entry, collision behavior, and combat visibility, but does not expose a follow toggle.
- `NpcAlly` owns the native ally follow behavior. It creates a patrol around the ally, updates that patrol toward the ally, and teleports to the ally if too far.
- Creature/pet Stay works through `PetElement.SetFollowing(bool)` and `PetVariantBase.SetFollowing(bool)`. Those APIs are pet-specific and are not present on the human proof actor path.
- `NpcCanMoveHandler` is the native movement gate used by `NpcController`. It accepts `ICanMoveProvider` instances and returns false from `CanMove(...)` if any provider reports `CanMove=false`.
- `ICanMoveProvider` also exposes `CanOverrideDestination` and `ResetMovementSpeed`, which lets a runtime element block movement and destination override without editing templates, stories, or save data.

## Decision

Avalon Human Companions may add a proof-only `Hold` command for the active one-session proof actor.

The implementation must:

- add a plugin-owned runtime-only `ICanMoveProvider` element only to the active managed proof actor,
- return `CanMove=false`, `CanOverrideDestination=false`, and `ResetMovementSpeed=true` while Hold is active,
- mark the actor and the hold element not saved,
- remove the hold element when Follow, Defend, Come Close, or Dismiss runs,
- keep Recall available as a teleport/reposition command without changing the current mode,
- keep lifecycle diagnostics at `behaviorApproved=false` and `persistenceApproved=false`.

This is a runtime movement lock proof, not a native humanoid wait package. It does not prove sandboxed waiting across scene transitions or reloads.

## Not approved

- A command named `Wait`.
- Saving or restoring a held proof actor.
- Holding existing NPCs, unique/story actors, civilians, bosses, quest actors, or unrelated spawned actors.
- Editing vanilla AI packages, story graphs, serialized interaction lists, global factions, crime state, dialogue, quests, or save data.
- Disabling combat targeting or forcing custom targets.
- Claiming release-ready human companion behavior.

## Validation needed

1. Build and deploy 0.1.8.
2. Spawn the one-session proof actor on a throwaway save.
3. Press `Hold`.
4. Confirm the actor stops following/catch-up movement while active.
5. Confirm `Recall` can reposition the actor and Hold remains selected afterward.
6. Confirm `Follow`, `Come Close`, and `Defend` remove the hold block.
7. Confirm `Dismiss` removes the actor and no hold element survives.
8. Confirm `human-proof-lifecycle.csv` rows stay `behaviorApproved=false` and `persistenceApproved=false`.
9. Confirm no existing NPC gains the hold movement block.
