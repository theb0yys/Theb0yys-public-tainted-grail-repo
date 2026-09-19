<!-- Canonical Wave 5 native-system page split from docs/reference/STORY_QUEST_DIALOGUE.md. -->
# Story Graphs, Quests, Dialogue, and Choices

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/story/observation-and-intervention.md).

## What this system is

The recovered high-level architecture is:

~~~text
StoryGraph authoring (XNode-based)
→ StoryGraphParser
→ StoryGraphRuntime
→ handwritten .story binary payload
→ Story archive
→ runtime Story : MVC Model
→ StepSequenceRunner
→ StoryStep / StoryCondition
→ StepResult async waits
→ dialogue / choices / quests / scene actions / other effects
~~~

This is a custom Questline system, not simply Unity UI dialogue data.

## Who owns it in FoA

Important owners include:

- `StoryGraph`
- `StoryGraphRuntime`
- runtime `Story` MVC Model
- `StepSequenceRunner`
- `StoryStep`
- `StoryCondition`
- `StepResult`
- `Choice` / `VChoice`
- Story Views such as dialogue/story panels
- quest-specific step/state owners
- Babel/localisation for compiled text
- scene/services/actions invoked by individual concrete steps

## Important identities, types, and methods

Research-backed surfaces include:

- Story graph GUID
- bookmark/chapter name
- `Story.OfferChoice(ChoiceConfig)`
- `VChoice.Select(Choice)`
- `Choice.OnInitialize`
- Mono `Choice.HoverInfos` getter path
- quest completion candidate methods such as qualifying `QuestUtils.Complete` / `SetQuestState`
- `NewGameLoading.OnComplete` as one loading-phase observation
- compiled step/condition byte type IDs
- `LightLocString` for compiled Story text

## Where it exists in the lifecycle

### Runtime Story

The Story Model resolves an entry/start/bookmark, loads compiled graph data, then advances through ordered runtime steps.

A step can return immediately complete or a pending `StepResult`.

For pending work:

~~~text
Step.Execute
→ StepResult pending
→ external operation completes
→ StepResult.Complete()
→ later runner Advance observes completion
~~~

The exact rescheduling mechanism remains incompletely recovered.

### Important save fact

The active `Story` Model reports `IsNotSaved == true`.

Therefore the ordinary MVC save path does **not** persist the active runner position/wait state as a saved Model.

Durable consequences belong in other saved owners: quests/flags/items/etc.

## Current proof boundary

**Strong architecture evidence:** XNode authoring → compiled runtime graph → `.story` binary → MVC Story/runner/steps.

**Useful bounded hooks:** choice offered/selected and several quest/new-game observation candidates.

**Not generally solved:** complete current node-type registry, custom Story packaging/mounting, dialogue/audio lifecycle, async cancellation across every step, custom Story save compatibility, or universal Mono/IL2CPP parity.
