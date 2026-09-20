# Story Graphs, Quests, Dialogue, and Choices

> **Reference page.** Questline Story Graphs are a compiled runtime system with their own binary/runtime lifecycle. A dialogue hook is not the same as owning Story state.

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

## How we interact with it

### Observe choices at exact UI/runtime boundaries

`Story.OfferChoice` can observe an offered choice.

`VChoice.Select(Choice)` can observe actual selection.

Do not assume either one is a complete public event for all dialogue semantics without scenario validation.

### Preserve game callback ownership

The IL2CPP Immersive Backgrounds correction replaced delegate wrapping/component injection with a direct bounded `VChoice.Select` patch and left callback ownership with the game.

That is a useful pattern: observe the native transition rather than replacing its callback system unnecessarily.

### Treat Mono/IL2CPP choice-preview hooks separately

The inspected branches needed different preview integration points.

Do not assume a Mono getter hook maps one-to-one to IL2CPP.

### For custom Story content

A raw XNode graph is not enough.

The production-compatible path would need the Questline parser/writer/binary/archive/localisation/dependency pipeline.

## Why this route

Story research exposed several dangerous shortcuts:

- a graph asset is not the shipping runtime representation;
- compiled node types rely on byte IDs and serializer mappings;
- active Story runner state is not ordinary saved Model state;
- Story can belong to a broader domain than scene-bound actors/views it references;
- async completion/cancellation cleanup matters.

That means "add dialogue by creating a graph" is not a complete process.

## What goes wrong

### Custom binary node IDs guessed

The concrete type-ID registry is incomplete. Collision/incompatible payload schemas can corrupt interpretation.

### Raw visible strings used instead of Babel-compiled identity

Story text is transformed through the localisation/Babel compilation path.

### Story runner state assumed saveable

Active runner state is explicitly not ordinary MVC-saved state.

### Choice hook treated as universal dialogue lifecycle

Offer/selection/hover are individual surfaces; line progression, speaker resolution, audio, interruption and cleanup require their own evidence.

### Scene transition during pending Story step ignored

Story can reference scene-bound actors/views while living in a broader domain.

### Callback wrapping replacing native ownership

Can introduce freeze/lifecycle issues, especially across IL2CPP boundaries.

## How to verify

For a Story/dialogue integration:

- exact graph/story identity;
- exact runtime step/choice target;
- offer/selection scenario;
- cancellation/back/interrupt;
- dialogue View cleanup;
- audio/subtitle behavior if involved;
- quest/state side effect;
- scene transition if involved;
- save during pending state if claimed;
- Mono/IL2CPP separately.

For a custom Story compiler/content path, also verify binary type mapping, archive mounting, localisation, dependencies and version compatibility.

## Current proof boundary

**Strong architecture evidence:** XNode authoring → compiled runtime graph → `.story` binary → MVC Story/runner/steps.

**Useful bounded hooks:** choice offered/selected and several quest/new-game observation candidates.

**Not generally solved:** complete current node-type registry, custom Story packaging/mounting, dialogue/audio lifecycle, async cancellation across every step, custom Story save compatibility, or universal Mono/IL2CPP parity.
