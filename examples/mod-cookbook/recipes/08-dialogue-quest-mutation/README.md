# Recipe — Dialogue and Quest Mutation Boundary

**Category:** story systems  
**Observation paths:** LOAD_EVIDENCED  
**Generic mutation API:** NOT_PROVEN

The cookbook provides two read-mostly starting points:

- [15 — Story.OfferChoice observer](../../15-dialogue-choice-observer-mono/README.md)
- [16 — quest completion candidate observer](../../16-quest-completion-observer-mono/README.md)

## Observation is not mutation

Seeing `Story.OfferChoice` tells you a choice set is being offered. It does not automatically prove:

- which choice is ultimately selected;
- cancellation behavior;
- re-entry behavior;
- chapter transition ownership;
- localization/text lifetime;
- save persistence.

Likewise, intercepting `QuestUtils.Complete` or `SetQuestState(...Completed)` does not prove there is one canonical exactly-once quest-completion event.

## Before changing story state

For the exact quest/dialogue target, map:

1. entry point;
2. state owner;
3. normal validation/preconditions;
4. callbacks/events;
5. save-facing state;
6. failure/cancel path;
7. reload behavior.

Then make the smallest change possible.

A story mod that "works" for one click but leaves the native story graph/save state inconsistent is worse than a visible failure.
