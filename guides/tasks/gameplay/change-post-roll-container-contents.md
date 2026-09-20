# Change Post-Roll Container Contents

**Evidence status: PARTIAL.** The save-backed ownership boundary is understood; complete runtime/persistence validation is not recorded.

Working lineage: [Container Rules: Post-Roll and Save-Backed](../../../research/case-studies/economy/container-row-boundary.md).

## Key distinction

Changing a generated container row **after roll** is not the same as editing the source loot table.

The relevant runtime list, such as `SearchAction._itemsInsideContainer`, can be save-backed state.

## Bounded operations

Keep these separate:

- quantity scaling;
- rule-based remove/scale;
- corpse-specific filtering.

Do not mix them with:

- loot-table authoring;
- metadata edits;
- item transfer rewrites.

## Process

```text
container/corpse contents generated
→ inspect runtime ItemSpawningDataRuntime rows
→ apply one bounded post-roll rule
→ native ContainerUI/transfer continues
```

## Persistence warning

Because the list can be save-backed, your mutation may become durable.

Validate on a disposable save:

- initial generation;
- rule application;
- save;
- reload;
- reopen;
- no double-application.

## Current proof boundary

Ownership and risk are established. Runtime/save behaviour for each concrete rule must be proven before promotion.
