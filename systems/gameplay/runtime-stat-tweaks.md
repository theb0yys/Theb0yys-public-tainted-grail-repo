---
document_type: system
scope: non-saved runtime hero/character stat modification pattern
runtime: mono
evidence:
  static: MULTI_PROJECT_SOURCE_AND_DECOMPILE_CORROBORATION
last_verified: 2026-09-20
---

# Runtime Stat Tweaks

Several FoA systems consume mutable `Stat` / `LimitedStat` values owned by hero/character stat containers.

A recurring modding pattern is:

```text
native stat wrapper initializes/rebuilds stat object
→ mod postfix runs
→ attach mod-owned StatTweak element
→ mark tweak not saved
→ native gameplay reads modified stat
→ recreate/discard tweak when stat object is rebuilt or feature disables
```

## Why initialize-time ownership matters

Some stat wrappers can rebuild the underlying stat object.

A mod that keeps a reference to an old tweak/stat can appear to “apply” while the UI/gameplay reads a newer native stat instance.

The Carry Weight Tweaks 0.3.2 correction explicitly discards/recreates its tweak against the current `EncumbranceLimit` after `HeroStatsWrapper.Initialize`.

## Persistence boundary

A tweak marked non-saved changes runtime `ModifiedValue`; it is not the same as:

- changing `BaseValue`;
- writing save data;
- permanently spending progression resources;
- changing difficulty configuration.

## Compatibility

Multiple tweaks can compose through the native tweak system, but mods targeting the same stat still need compatibility testing for order/meaning.
