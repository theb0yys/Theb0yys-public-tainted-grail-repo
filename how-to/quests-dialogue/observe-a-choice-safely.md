# Observe a Dialogue Choice Without Taking Story Ownership

Use this when a mod needs to react to a native dialogue choice while leaving FoA's Story system in control.

Canonical architecture: [Story Graphs, Quests, Dialogue, and Choices](../../systems/world/story-quest-dialogue.md).

## Choose the boundary you actually need

Research-backed bounded surfaces include:

- `Story.OfferChoice(...)` — choice offered;
- `VChoice.Select(Choice)` — choice selected;
- branch-specific preview/hover routes.

Offer, preview and selection are different facts. Do not substitute one for another.

## Procedure

1. Identify the exact Story/choice scenario.
2. Choose the narrowest native observation point.
3. Observe the transition; do not replace native callbacks unless the feature truly owns them.
4. Copy only the data required by your feature.
5. Let FoA continue normal Story advancement.
6. Keep your side effect idempotent if the observation can repeat.
7. clean up on dialogue/story/view teardown.

## Avoid callback wrapping by default

The working IL2CPP correction in the research lineage used a bounded `VChoice.Select` observation rather than wrapping/replacing the game's callback ownership.

That preserves native Story execution and reduces lifecycle risk.

## Persistence boundary

The active runtime Story model is not ordinary saved MVC state.

If your reaction must persist, write it through the actual durable owner for that fact—quest/flag/item/native state where appropriate—or through a separately proven mod persistence route.

Do not persist “current dialogue runner position” by assumption.

## Verify

Test:

- exact dialogue/Story identity;
- choice offered;
- chosen option observed once;
- Back/cancel/interrupt;
- subsequent Story advancement;
- dialogue UI cleanup;
- scene transition where relevant;
- save/load only if a durable side effect is claimed;
- Mono and IL2CPP separately.

For custom Story content/compilation, the general public pipeline is not yet solved; do not treat this observation guide as a custom Story authoring route.
