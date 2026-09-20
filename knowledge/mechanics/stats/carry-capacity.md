---
document_type: mechanic
scope: final hero carry-capacity runtime override
runtime: mono
evidence:
  static: SOURCE_AND_TARGET_RESEARCH
  loader: PRIVATE_PLUGIN_LOAD_VALIDATED
  feature_runtime: OUTSTANDING_FOR_LATEST_CORRECTION
  persistence: NONE
last_verified: 2026-09-20
---

# Carry Capacity Through EncumbranceLimit

FoA uses:

- `HeroItems.CurrentWeight` for current carried weight;
- `HeroStats.EncumbranceLimit` for maximum capacity;
- `HeroTweaks.RefreshEncumbrance()` to compare the two.

## Route

```text
HeroStatsWrapper.Initialize
→ current EncumbranceLimit created
→ discard stale mod carry tweak
→ if configured capacity > 0:
     add non-saved additive tweak =
       desired final capacity - native base capacity
→ native encumbrance/UI reads modified limit
```

## Why recreate the tweak

A community report showed a very large configured capacity still presenting a vanilla-like limit.

The project correction recognized that `HeroStatsWrapper.Initialize` can rebuild the stat object. The mod now recreates its tweak against the **current** `EncumbranceLimit` rather than updating a stale reference.

## Boundary

Do not:

- change item weights;
- call permanent stat setters;
- write save data;
- keep a helper tweak active when configured final capacity is zero.

The latest correction had build evidence; its final 90,000/vanilla in-game character-sheet check remained outstanding in the cited plan.
