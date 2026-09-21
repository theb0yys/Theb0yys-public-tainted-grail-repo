# Human NPC Companion Proof Step

Date: 2026-06-15

Scope: first local `TG.Main.dll` proof pass for future human NPC companions. This pass proves actor surfaces and blockers. It does not approve recruitment, spawning, cloning, command behavior, dialogue, faction edits, or persistence.

## Tools and target

- Tool: `ilspycmd` 10.1.0.8386.
- Assembly: `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\Fall of Avalon_Data\Managed\TG.Main.dll`.
- Assembly timestamp: 2026-06-13 16:37:25.
- Assembly size: 9,018,880 bytes.

## Decompiled evidence

### Human/NPC actor surface

- `Awaken.TG.Main.Fights.NPCs.NpcElement`
  - Implements `ICharacter`, `IAlive`, `IWithFaction`, `IEquipTarget`, `IItemOwner`, `IWithCrimeNpcValue`, `IWithLookAt`, `ILocationElementWithActor`, and other actor interfaces.
  - Has `TypeForSerialization => 229`.
  - Exposes `Template`, `NpcPresence`, `Controller`, `Movement`, `NpcAI`, `Inventory`, `NpcItems`, `Faction`, `CrimeValue`, `PossibleTargets`, `PossibleAttackers`, `IsUnique`, `IsSummon`, `IsSummonOrAlly`, and `IsHeroSummon`.
  - Stores `SavedInteractionData`, `LastIdlePosition`, `LastOutOfCombatPosition`, dialogue state, visibility state, equipment/clothing state, and cached combat/movement elements.

Decision: `NpcElement` is the core human/creature actor surface, but it is too broad and save-connected to command directly without a classification scanner and safety matrix.

### NPC templates and uniqueness

- `Awaken.TG.Main.Fights.NPCs.NpcAttachment`
  - Owns the `NpcTemplate` reference, actor reference, story-on-death reference, visual prefab data, and abstract `IsUnique`.
  - `SpawnElement()` creates the NPC element from the attachment.
- `Awaken.TG.Main.Fights.NPCs.UniqueNpcAttachment`
  - `IsUnique => true`.
- `Awaken.TG.Main.Fights.NPCs.RepetitiveNpcAttachment`
  - `IsUnique => false`.
  - Has `GetActor()`, `SpawnNpc()`, and `CanSpawnNpc()` paths.

Decision: future candidate selection must reject unique NPCs first and treat repetitive/non-unique attachments as research candidates only, not approved followers.

### NPC presence, stash, and save-sensitive ownership

- `Awaken.TG.Main.Fights.NPCs.Presences.NpcPresence`
  - Uses `NpcRegistry` and `UniqueNpcStash`.
  - Has `TypeForSerialization => 118`.
  - Holds `Template`, `AliveNpc`, manual availability, scene/domain state, `Attach`, `Detach`, `SetManualAvailability`, and `MarkAsDead`.
  - `GetOrCreateNpc()` can unstash a unique NPC or call `Template.SpawnLocation(AbyssPosition, ...)`, then move the location to gameplay.
  - `Refresh()` can teleport NPCs and change NPC presence.

Decision: human NPC companion behavior must not touch unique presence, stash, manual availability, or story presence until a dedicated save/load proof exists.

### Existing summon/ally controller path

- `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`
  - Has `TypeForSerialization => 418`.
  - Follows an `ICharacter Ally`, patrols around the ally, teleports when too far, and targets the ally's `PossibleAttackers`.
  - Uses `OverrideFaction(Ally.GetFactionTemplateForSummon(), FactionOverrideContext.Ally)` and resets that override on discard.
  - Serializes the ally weak reference.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`
  - Extends `NpcAlly`, implements `INpcSummon`, and has `TypeForSerialization => 419`.
  - Marks the parent NPC as `IsHeroSummon`, registers summon events, uses `GameplayUniqueLocation`, reacts to portal, fast travel, and long teleport events, removes search/pickpocket actions, and prevents friendly fire from the hero when configured.
  - Serializes owner item data.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`
  - Extends `NpcHeroSummon`, has `TypeForSerialization => 436`, `DestroyOnRest => false`, and `CharacterLimitedLocationType.None`.
- `Awaken.TG.Main.AI.SummonsAndAllies.NpcAISummon`
  - Implements `INpcSummon`, has `TypeForSerialization => 417`, and serializes an owner weak reference.

Decision: the strongest existing follower-like path is summon/ally ownership, not vanilla human recruitment. A future human pseudo-follower should first prove whether a humanoid summon actor can safely use this path. Do not convert normal NPCs.

### Story-owned NPC control steps

- `Awaken.TG.Main.Stories.Steps.SNpcMove`
  - Moves NPCs selected by `LocationReference`.
  - Calls `api.SetupNpc(...)`, pushes `StoryCommuteToPosition`, and may involve the NPC in the story after movement.
- `Awaken.TG.Main.Stories.Steps.STeleportNpc`
  - Teleports NPCs selected by `LocationReference` through `NpcTeleporter.Teleport(..., TeleportContext.FromStory)`.
- `Awaken.TG.Main.Stories.Steps.SChangeNpcFaction`
  - Deferred story execution that calls `ResetFactionOverride()` or `OverrideFaction(...)`.
- `Awaken.TG.Main.Stories.Steps.SNpcTurnFriendly`
  - Deferred story execution that calls `TurnFriendlyTo(AntagonismLayer.Story, Hero.Current)`.
- `Awaken.TG.Main.Stories.Steps.SNpcTurnHostileBase`
  - Deferred story execution that modifies hostility data, can disable crimes or mark death as non-criminal, calls `TurnHostileTo(AntagonismLayer.Story, target)`, and may call `NpcAI.EnterCombatWith(...)`.

Decision: these prove FoA has native story-controlled NPC movement, teleport, faction, friendliness, and hostility operations. They are not approved mod command APIs because they are story-owned, location-reference driven, and save/quest sensitive.

## Approved next code step

Add a disabled-by-default Human NPC Proof scanner to `Avalon Human Companions`.

The scanner may:

- inspect loaded/live `NpcElement` instances,
- classify template name/GUID when available,
- record `IsUnique`, `IsSummon`, `IsSummonOrAlly`, `IsHeroSummon`, `NpcPresence`, `Faction`, `NpcType`, and attachment class hints,
- record whether summon/ally markers such as `NpcAlly`, `NpcHeroSummon`, `NpcHeroPetAlly`, or `NpcAISummon` are present,
- write plugin-owned CSV evidence under `BepInEx/config/kane.tgfoa.avalon-human-companions`.

The scanner must not:

- spawn, unstash, move, teleport, recruit, convert, command, or dismiss NPCs,
- call story steps,
- call `OverrideFaction`, `ResetFactionOverride`, `TurnFriendlyTo`, `TurnHostileTo`, or `NpcAI.EnterCombatWith`,
- alter presence, unique stash, interaction data, story state, crime state, or save data.

## Behavior still blocked

- Human NPC recruitment.
- Converting vanilla NPCs into followers.
- Human NPC companion dialogue.
- Human NPC companion combat commands.
- Human NPC follow/teleport/catch-up behavior.
- Human NPC persistence or save reconstruction.

