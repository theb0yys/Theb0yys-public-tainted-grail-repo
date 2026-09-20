# Story, Quests, Dialogue, and Choices

> **Runtime-consumer page.** The canonical Story Graph authoring → compilation → `.story` → runtime interpreter architecture lives in [Story Graphs](story-graphs/README.md).

Use this page for **runtime Story ownership, choice observation, quest side effects, and lifecycle boundaries**.

## Runtime owners

Important runtime owners include:

- `Story` — MVC Model coordinating the active Story runtime;
- `StepSequenceRunner` — step progression;
- `StoryStep` / `StoryCondition`;
- `StepResult` — pending/asynchronous completion;
- `Choice` / `VChoice` — choice presentation/selection boundaries;
- quest-specific state owners;
- Story Views/dialogue panels;
- scene/services/actions invoked by concrete steps.

Compilation, binary type IDs, archive storage, and authoring graph structure are owned by [Story Graphs](story-graphs/README.md), not repeated here.

## Runtime progression

A Story resolves an entry/start/bookmark and advances through compiled steps.

A step may complete immediately or return a pending `StepResult`:

```text
Step.Execute
→ StepResult pending
→ external operation completes
→ StepResult.Complete()
→ runner later advances
```

The exact rescheduling behavior is still evidence-bounded; do not build a generic async Story API from that sequence alone.

## Choice observation

Useful bounded observation points include:

- `Story.OfferChoice(ChoiceConfig)` — a choice is offered;
- `VChoice.Select(Choice)` — a concrete option is selected;
- branch/runtime-specific preview or hover surfaces.

These facts are different.

“Choice offered” is not “choice selected”, and either is not automatically a complete dialogue lifecycle event.

For a practical observation procedure, see [Observe a Dialogue Choice](../../how-to/quests-dialogue/observe-a-choice-safely.md).

## Preserve callback ownership

A working IL2CPP correction in the research lineage moved from callback wrapping/component injection to a bounded `VChoice.Select` observation.

The reusable lesson is:

> observe the native transition when possible instead of replacing the Story callback owner.

That reduces lifecycle/freeze risk and keeps Story advancement with the game.

## Quest and durable-state boundary

The active `Story` Model reports `IsNotSaved == true`.

Do not infer that arbitrary runner position or pending wait state is persisted by normal MVC save behavior.

Durable consequences should remain with the native owner of the durable fact—quest state, flags, items, or another separately proven persistence route.

## Scene and cancellation boundary

Story runtime can outlive or reference scene-bound actors/views.

When observing or extending it, test:

- Back/cancel/interrupt;
- dialogue View cleanup;
- scene transition;
- missing/discarded actor targets;
- pending step completion after owner loss;
- duplicate observation after re-entry.

## Runtime compatibility

Mono and IL2CPP may expose different convenient preview/inspection seams.

Do not assume a Mono getter/property hook maps one-to-one to generated IL2CPP representation.

Keep the semantic event you need stable, then resolve a runtime-specific observation adapter.

## What this page does not provide

This is not a custom Story authoring/compiler guide.

Creating new production-compatible Story content still requires the architecture described in [Story Graphs](story-graphs/README.md), including compilation, binary mappings, archive/package integration, localisation, and dependencies.

## Proof boundary

**Strong:** runtime owner model, bounded offered/selected choice observations, non-saved active Story Model fact.

**Partial:** generic async rescheduling/cancellation across all steps, universal quest mutation seams, universal Mono/IL2CPP observation parity.

**Not generally solved:** public custom Story compiler/package pipeline and generic custom Story persistence.
