# Systems Cookbook

Systems are the reusable engineering foundations underneath gameplay, graphics, audio and UI mods.

## Proven mechanics index

Start with [PROVEN_MECHANICS.md](PROVEN_MECHANICS.md).

That index records:

~~~text
working mod
→ proven version/scope
→ native mechanism
→ runtime evidence
→ reusable public pattern
→ current cookbook relationship
~~~

A mechanism can be runtime-backed while its clean-room public rewrite remains **NOT_RUN**.

## Harmony and target ownership

Patch the narrowest owner that actually controls the behaviour.

Do not choose a method only because:

- its name sounds relevant;
- a field exists nearby;
- a diagnostic row can see the value;
- another mod patches a related method.

A useful target map should identify the input/producer, native owner, mutation/observation point and downstream lifecycle.

## Lifecycle and attribution

Several cookbook recipes are primarily lifecycle maps:

- [Equipment lifecycle attribution](../recipes/12-equipment-lifecycle-attribution/README.md)
- [Combat action lifecycle attribution](../recipes/13-combat-action-lifecycle-attribution/README.md)
- [Downstream combat outcome attribution](../recipes/14-downstream-combat-outcome-attribution/README.md)

Use lifecycle maps to keep owners separate rather than treating one hook as proof that every downstream system completed.

## Persistence

- [14 Save-slot observer](../14-save-slot-observer-mono/README.md)
- [Safe save-backup architecture](../recipes/07-save-backup/README.md)

Load/save observation, successful save completion, backup creation and restore are different proof levels.

Do not write save-visible state merely because an in-memory route works.

## Dialogue and quests

- [15 Dialogue-choice observer](../15-dialogue-choice-observer-mono/README.md)
- [16 Quest-completion observer](../16-quest-completion-observer-mono/README.md)
- [Dialogue/quest mutation boundary](../recipes/08-dialogue-quest-mutation/README.md)

Observation is not mutation authority.

## Diagnostics

Use diagnostics when the exact runtime owner is unknown.

A good diagnostic:

- reads bounded state;
- has a clear question;
- has a row/session cap;
- does not silently mutate gameplay;
- records enough identity to distinguish similarly named objects;
- has an explicit promotion gate.

Diagnostics are evidence tools, not finished gameplay merely because they execute in game.

## Cross-mod APIs

Prefer a small owned public surface over reflection into another mod's internals.

A safe consumer should:

1. discover the provider;
2. verify the required API/version shape;
3. fail closed when absent or incompatible;
4. call only the documented owner method;
5. keep resource consumption transactional where relevant.

The maintainer corpus has working cross-mod precedent; see [Proven mechanics](PROVEN_MECHANICS.md).

## Asset loading

Keep source assets and proprietary game content out of the public repository.

For external/custom runtime assets, separate:

- source provenance/licence;
- build/cooking process;
- bundle/resource loading;
- runtime object lifecycle;
- shader/material compatibility;
- teardown;
- package validation.

## Evidence and validation

Formal labels: [Testing and Evidence Status](../../../docs/EVIDENCE.md).

The public cookbook uses a strict rule:

> Do not publish a stronger claim than the exact evidence supports.

See [Research](../research/README.md) for candidate hooks that have not reached working-pattern status.
