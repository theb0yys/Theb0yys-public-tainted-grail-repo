<!-- Canonical Wave 5 mechanic page split from docs/reference/STORY_QUEST_DIALOGUE.md. -->
# Story Graphs, Quests, Dialogue, and Choices — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/story/README.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/story/README.md).
