# Research: Native Dialogue and Interaction Surface

Date: 2026-06-15
Scope: inspect `TG.Main.dll` for story/dialogue/interaction classes that could support a dialogue-only or native interaction command surface for Avalon Companions.

## Evidence read

- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md`
- `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md`
- `docs/in-game-ui-quality-standard.md`
- `Research/Tainted Grail The Fall of Avalon Companion and Troop System Feasibility Report-deep-research-report.md`
- `Research/foa-actor-interaction-feasibility-2026-06-15.md`
- Local decompilation with `ilspycmd` 10.1.0.8386 against `<local-path>`.

## Decompiled targets

### Native action and interaction surface

- `Awaken.TG.Main.Locations.Location`
  - Implements `IInteractableWithHero`.
  - `AvailableActions(hero)` returns `HeroInteraction.ActionsFromLocation(this)` unless the location is discarded or a hero-involving story is already active.
  - `DefaultAction(hero)` returns the first available action.
  - `Interactable` is backed by `LocationInteractability.interactable`.
- `Awaken.TG.Main.Heroes.Interactions.IInteractableWithHero`
  - Defines `Interactable`, `DisplayName`, `InteractionVSGameObject`, `InteractionPosition`, `AvailableActions`, `DefaultAction`, and `DestroyInteraction`.
- `Awaken.TG.Main.Heroes.Interactions.HeroInteraction`
  - Starts the first available `IHeroAction` from an interactable.
  - `ActionsFromLocation(location)` yields `IHeroActionModel` elements and `ILocationActionProvider` additional actions.
  - Unique NPCs can inherit action elements from their `NpcPresence.ParentModel`.
- `Awaken.TG.Main.Locations.Actions.IHeroAction`
  - Defines the interaction contract: `ActionFrame`, `StartInteraction`, `FinishInteraction`, `EndInteraction`, and `GetAvailability`.
- `Awaken.TG.Main.Locations.Actions.IHeroActionModel`
  - Extends `IHeroAction` and `IModel`; valid while not discarded.
- `Awaken.TG.Main.Locations.Actions.ILocationActionProvider`
  - Can provide additional actions for a `Location`.
- `Awaken.TG.Main.Locations.Actions.AbstractHeroAction<T>`
  - Base implementation for hero actions.
  - Handles item requirements, combat disabling, action blocker checks, start/finish/end flow, and location interaction events.
- `Awaken.TG.Main.Locations.Actions.AbstractLocationAction`
  - Thin `AbstractHeroAction<Location>` base.
- `Awaken.TG.Main.Locations.LocationInteractability`
  - Native values are `Hidden`, `Inactive`, and `Active`.

### Dialogue, story, and talk actions

- `Awaken.TG.Main.Locations.Actions.DialogueAction`
  - Uses a stack of `StoryBookmark` values.
  - Starts `Story.StartStory(StoryConfig.Interactable(interactable, bookmark, typeof(VDialogue)))`.
  - Blocks in combat.
  - For NPCs, disables talk if hostile, not in interact state, dialogue-invisible, angle-gated, or current NPC interaction disallows dialogue.
  - Uses a view focus from attachment, NPC head, or visual fallback.
- `Awaken.TG.Main.Locations.Actions.PetTalkAction`
  - Inherits `DialogueAction`.
  - Adds a `SaveBlocker` availability check.
- `Awaken.TG.Main.Fights.Mounts.MountTalkAction`
  - Inherits `DialogueAction`.
  - Adds mount-specific availability checks.
- `Awaken.TG.Main.Locations.Actions.StoryInteractAction`
  - Uses a `StoryBookmark`.
  - Reads the baked story graph through `StoryGraphRuntime.Get(bookmark.GUID)`.
  - Uses the first start choice text as the action label.
  - Starts `Story.StartStory(StoryConfig.Interactable(interactable, StoryBookmark, null))`.
- `Awaken.TG.Main.Locations.Actions.InteractAction`
  - Generic interaction action with optional label, sound, combat block, and manual-finish flag.
  - Fires native interaction flow but does not provide dialogue choices by itself.

### Action attachments

- `Awaken.TG.Main.Locations.Actions.Attachments.DialogueAttachment`
  - Owns `customDialogueLabel`, `StoryBookmark`, gestures, view focus, and end-story distance.
  - Spawns `DialogueAction`.
- `Awaken.TG.Main.Locations.Actions.Attachments.PetTalkAttachment`
  - Inherits `DialogueAttachment`.
  - Spawns `PetTalkAction`.
- `Awaken.TG.Main.Fights.Mounts.MountTalkAttachment`
  - Inherits `DialogueAttachment`.
  - Spawns `MountTalkAction`.
- `Awaken.TG.Main.Locations.Actions.Attachments.StoryInteractAttachment`
  - Owns `StoryBookmark` and blocked-info behavior.
  - Spawns `StoryInteractAction`.
- `Awaken.TG.Main.Locations.Actions.Attachments.InteractAttachment`
  - Spawns `InteractAction`.

### Story steps around interaction

- `Awaken.TG.Main.Stories.Steps.SPetInteract`
  - Supports `Pet`, `Taunt`, `Stay`, and `Follow`.
  - Applies only to matching locations with `PetVariantBase`.
- `Awaken.TG.Main.Stories.Steps.SPetSetVariant`
  - Starts a pet variant feed/transform sequence through `PetVariantBase`.
- `Awaken.TG.Main.Stories.Conditions.CPetStatus`
  - Checks whether a `PetElement` follows `Hero.Current`.
  - Checks whether a location is a non-base pet variant.
- `Awaken.TG.Main.Stories.Steps.SLocationInteract`
  - Calls `HeroInteraction.StartInteraction(...)` on matching locations.
- `Awaken.TG.Main.Stories.Steps.SLocationStartStory`
  - Starts a story for matching locations with `VDialogue`.
- `Awaken.TG.Main.Stories.Steps.SPerformInteraction`
  - Drives NPC idle interactions by unique interaction ID.
  - This is NPC behavior/idle interaction control, not a player command menu by itself.
- `Awaken.TG.Main.Stories.Steps.SStopInteraction`
  - Drops an NPC back to anchor.
- `Awaken.TG.Main.Stories.Steps.SRemoveAllInteractionOverrides`
  - Removes NPC interaction overrides.
- `Awaken.TG.Main.Stories.Steps.SFakeInteractionPrompt`
  - Adds a temporary story-owned fake interaction UI prompt.
- `Awaken.TG.Main.Stories.Steps.SLocationChangeInteractability`
  - Calls `location.SetInteractability(...)`.
  - Logs an error if used against an NPC location.

### Story and graph primitives

- `Awaken.TG.Main.Stories.StoryBookmark`
  - Stores a `TemplateReference story` and a chapter name.
  - `IsValid` depends on `story.IsSet`.
  - `GUID` comes from the story template reference.
- `Awaken.TG.Main.Stories.Runtime.StoryGraphRuntime`
  - Loads baked story graph data by GUID from the game's story runtime path.
  - `StoryInteractAction` depends on this lookup succeeding.
- `Awaken.TG.Main.Stories.StoryConfig`
  - Provides `Base`, `Location`, and `Interactable` helpers for starting stories with a hero/location context.

### UI and prompt classes

- `Awaken.TG.Main.Heroes.Interactions.HeroInteractionUI`
  - Registers gameplay interact input.
  - Calls `HeroInteraction.StartInteraction(...)` on interact key down.
  - Calls `EndInteraction(...)` on key up when an action is active.
- `Awaken.TG.Main.Heroes.Interactions.VHeroInteractionUI`
  - Displays the interactable name, default action frame, and extra info frames on the always-visible HUD.
- `Awaken.TG.Main.UI.ButtonSystem.Prompt`
  - Lower-level prompt/button model for tap/hold actions.
  - Good for UI panels and game-owned prompt hosts, but not enough by itself to add a world interaction to an NPC.
- `Awaken.TG.Main.UI.Components.ARButton`
  - Lower-level Unity UI button component with hover, selected, pressed, disabled, audio, and mouse/submit handling.
  - Useful for native-looking UI, but not the world interaction system.

### Visual scripting helpers

- `Awaken.TG.VisualScripts.Units.General.StartDialogueWithHero`
  - Starts dialogue only when the target `Location` already has a `DialogueAction`.
  - Uses an optional chapter name override on the existing action bookmark.
- `Awaken.TG.VisualScripts.Units.Locations.LocationActionFinishUnit`
  - Finishes the first `AbstractLocationAction` on a location.
- `Awaken.TG.VisualScripts.Units.Locations.SetLocationInteractabilityUnit`
  - Sets native `LocationInteractability`.
- `Awaken.TG.VisualScripts.Units.Listeners.Events.EvtInteract`
  - Listens to `Location.Events.Interacted`.

### Pet interaction and ally behavior

- `Awaken.TG.Main.Locations.Pets.Variants.PetVariantBase`
  - Exposes `PerformPetting()`, `PerformTaunt()`, `SetFollowing(bool)`, and `StartVariantFeedSequence(...)`.
- `Awaken.TG.Main.Locations.Pets.Variants.NpcAllyPetVariant`
  - On spawn, overrides the NPC faction to `Hero.Current.GetFactionTemplateForSummon()` with summon context and adds `NpcHeroPetAlly`.
  - Pet/feed/transition animations are handled through NPC animator states.
- `Awaken.TG.Main.Animations.FSM.Npc.States.General.NpcPetVariantInteractionState`
  - Native animation state for pet variant interactions and transitions.

## Findings

- Native world interaction is action-element based. A companion `Location` can surface a native interact prompt if it has an available `IHeroActionModel` element or an `ILocationActionProvider` that supplies one.
- The native interact prompt is not a multi-button command menu. `HeroInteraction.StartInteraction` chooses available actions and starts the first one that succeeds. A rich command list needs a story/dialogue choice tree or a separate UI.
- True native dialogue choices require a valid `StoryBookmark` that resolves through `StoryGraphRuntime.Get(bookmark.GUID)`. The current evidence does not prove that a BepInEx plugin can author/register new baked story graph data at runtime.
- `DialogueAction`, `PetTalkAction`, and `MountTalkAction` are good references for how talk actions should be exposed and gated, but they still depend on game-authored story bookmarks.
- `SPetInteract` proves vanilla pet commands exist, but only for actors with `PetVariantBase`. The current one-session creature candidates are plugin-owned ally-marked locations, not proven `PetVariantBase` pets.
- `SPerformInteraction`, `SStopInteraction`, and `SRemoveAllInteractionOverrides` target NPC idle/story interaction state, not companion follow/stay/defend command state.
- `Prompt` and `ARButton` are UI building blocks. They do not make a companion interactable by themselves.
- `SLocationChangeInteractability` should not be used as a shortcut on NPC companions; the native step logs this as an error path.

## Approved next implementation direction

The smallest researched next step is a native world-interaction scaffold for Avalon-owned companions:

1. Add a plugin-owned `IHeroActionModel`/`AbstractLocationAction` element only to managed one-session companion locations.
2. Label it as a talk/command action.
3. On start, either open the existing Avalon Companions command panel in companion context or run a diagnostic command selector.
4. Keep existing panel commands as the actual implementation until native story/dialogue authoring is proven.

This gives the player a native "interact with companion" entry point without pretending that a full `VDialogue` story graph exists.

2026-06-15 implementation update: the actor interaction feasibility brief supports this route specifically as a runtime-only prompt/panel bridge for rostered one-session actors. The implemented scaffold attaches a plugin-owned `AvalonCompanionCommandAction : AbstractLocationAction` only to tracked one-session creature-candidate `Location` instances that already carry the native `NpcHeroPetAlly` marker. The action is `IsNotSaved`, skips the normal location interaction event run, and opens the existing Avalon Companions panel in companion context. It does not mutate `LocationWithNpcInteractionElement`, templates, story bookmarks, story graphs, quest state, faction state, crime state, or vanilla serialized interaction lists.

## Not approved yet

- Replacing the panel with true native dialogue choices.
- Creating fake `StoryBookmark` values without a resolvable story graph.
- Editing game templates or adding runtime `DialogueAttachment`/`PetTalkAttachment` to global templates.
- Using `SPetInteract` on one-session creature candidates unless they are first proven to be safe `PetVariantBase` instances.
- Changing NPC `LocationInteractability` as a command/UI shortcut.
- Adding humanoid companion dialogue or recruitment.

## Validation needed before a native command action ships

- Build after adding the custom action element.
- Spawn each managed one-session creature candidate on a throwaway save and confirm its `Location` has exactly one Avalon-owned command action.
- Confirm the native interact prompt appears only for the active managed companion.
- Confirm interact opens the command surface or logs the intended dry-run command.
- Confirm dismiss removes the action element and the prompt disappears.
- Confirm no prompts appear on wild, quest, unique, or non-managed creatures.
- Confirm normal dialogue with unrelated NPCs is unaffected.
