# Recipe — Held Light / Helper Light

**Category:** environment / lighting  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Public recipe:** NOT_RUN

The owner-side Brighter Torches path established the useful architecture but still had unresolved visual validation after its helper-light correction.

## Working shape

1. Find `Hero.Current`.
2. Inspect current main/off-hand item identity.
3. Inspect light components under:
   - main/off-hand transforms;
   - current `CharacterHandBase` weapon views;
   - a narrowly justified fallback transform.
4. For existing lights:
   - record original range/intensity once;
   - apply bounded multipliers;
   - restore original values when the candidate disappears or the mod disables.
5. If the visible torch has no usable light:
   - create one **mod-owned** helper `GameObject`;
   - add a point/spot `Light`;
   - position it relative to the held view;
   - destroy it when unequipped/unloaded.

## Invariants

- no scene-wide light mutation;
- no save writes;
- no shared material edits;
- never lose the original values you need for restore;
- helper light must be off at the vanilla comparison baseline;
- bounded scan interval—do not traverse large hierarchies every frame.

## Evidence warning

The initial real mod loaded successfully, but user comparison showed the first route missed the visible effect. The helper-light correction then built/deployed; its full visual matrix remained pending.

So this is a **path recipe**, not a proven visual preset.
