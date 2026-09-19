# Recipe — Buff and Debuff Tuning Evidence Gate

**Category:** statuses / buffs / debuffs  
**Source-path evidence:** SOURCE_CONFIRMED for stat availability; consumer semantics NOT_PROVEN  
**This public recipe:** NOT_RUN

FoA's CharacterStats surface exposes the following public runtime stats in the inspected Mono build:

~~~text
CharacterStats.BuffStrength
CharacterStats.BuffDuration
CharacterStats.DebuffStrength
CharacterStats.DebuffDuration
~~~

That proves the stat surfaces exist. It does **not** yet prove enough about their consumers to publish a mutation example.

## Why there is no tweak code here

The repository already has evidence that non-saved StatTweak elements are a viable mechanism for runtime stat overlays. That mechanism alone does not answer the important status questions:

- which native methods read each of these four stats;
- whether each value belongs to the source character, target character, or both;
- what baseline and direction each value uses;
- whether strength means magnitude, application chance, buildup, tick value, or another operation;
- whether duration is sampled on initial application, renewal, prolong, replacement, stacking, or continuously;
- whether already-active statuses recalculate when the stat changes;
- how positive/negative classification interacts with templates and Status.Type;
- how hero and NPC consumers differ;
- how the stats interact with resistance, buildup, cleansing, stack rules and status-template logic.

Publishing a four-slider mod before those consumer paths are traced would turn a known field name into an unsupported behavior claim.

## Evidence ladder before a public mutation example

1. Trace every relevant read site for BuffStrength, BuffDuration, DebuffStrength and DebuffDuration in the target build.
2. Record the owning character at each read site and prove source-versus-target semantics.
3. Establish the vanilla baseline and multiplication/addition direction for each stat.
4. Map AddStatus outcomes: Add, Upgrade, AddAndProlong, AddAndRenew, Replace and Stack.
5. Test whether existing status instances react to a runtime stat change or only new applications do.
6. Test at least one positive and one negative status on the hero and on an NPC where the native route permits it.
7. Check status removal/expiry, reload and stat-wrapper rebuild behavior.
8. Only after those semantics are proved, author the smallest separate public example using a non-saved runtime mechanism and validate that exact rewrite.

## Boundary

Example 13 already covers status buildup. Examples 28 and 29 cover application events and active-set observation. This recipe intentionally does not reinterpret those paths as buff/debuff strength or duration tuning.

Current mutation status: **NOT_PROVEN**. No speculative tweak example is published.
