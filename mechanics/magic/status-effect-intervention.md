<!-- Canonical Wave 5 mechanic page split from docs/reference/STATUS_EFFECTS.md. -->
# Status Effects, Buildup, Sources, and Safe Observation — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/magic/status-effects.md) first for ownership, identities, lifecycle, and proof scope.

## How we interact with it

### Observe `AddStatus` to learn what actually reaches a character

A postfix observer can inspect:

- target;
- old/new Status;
- result type;
- positive/negative class;
- source character;
- source item.

This is useful before deciding to mutate status behavior.

### Tune buildup at the buildup owner

If the goal is "burn builds 1.5x faster," a scoped `BuildupStatus` input multiplier is closer to the native owner than rewriting every projectile or source that can apply burn.

### Scope by source/target

Broad status hooks are global/high-value.

For player-only effects, verify source and target explicitly.

### Keep status mutation separate from secondary mod state

A mod can react to a negative status by adding its own session-only fatigue, for example, without changing the native disease/curse itself.

That is a different claim and can be safer.

## Why this route

The working research deliberately began with **diagnostic-only** status observation.

That allowed it to establish:

- which statuses are actually added;
- which are positive/negative;
- who sourced them;
- how add/upgrade/stack behavior appears;

before adding any mutation.

Magic research then found buildup has its own native owner, allowing buildup tuning without rewriting every effect source.

## What goes wrong

### AddStatus = buildup

False. Some statuses accumulate before being applied.

### Broad global multiplier without source/target filter

Can affect NPC-vs-NPC, environmental, self, item, spell and scripted status routes.

### Positive/negative name heuristic

Use native status classification where available.

### Status observer called persistence proof

Seeing a Status object added does not prove save/restore/duration across restart.

### Custom spell/status template confused with existing-status tuning

Registering a genuinely new StatusTemplate has separate identity/registration/persistence requirements.

## How to verify

For status observation/tuning:

1. exact target character;
2. exact StatusTemplate;
3. source character/item;
4. direct-add or buildup path;
5. result type;
6. stack/refresh behavior;
7. duration;
8. native gameplay effect;
9. removal/expiry;
10. unrelated status routes unchanged;
11. save/load if persistence is claimed.

For buildup tuning, also verify threshold progression before/after and the final applied status identity.

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/magic/status-effects.md).
