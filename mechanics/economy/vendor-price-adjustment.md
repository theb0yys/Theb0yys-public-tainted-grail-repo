---
document_type: mechanic
scope: final vendor buy/sell price adjustment
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: LEVEL_3_THROWAWAY_SAVE_VALIDATED_FOR_CORE_LANE
  rollback: VALIDATED_FOR_CORE_LANE
last_verified: 2026-09-20
---

# Final Vendor-Price Adjustment

The validated Tainted Economy core lane uses a Harmony **Postfix** on `TradeUtils.Price`.

## Path

```text
native pricing computes result
→ classify direction: hero buying or hero selling
→ preserve blocked/zero/unsafe cases
→ compose configured multiplier
→ adjust __result only
→ vanilla TryTrade continues
```

## Why this seam

It preserves:

- native pricing inputs;
- native stolen/fence decision;
- affordability checks;
- stock;
- merchant wealth;
- transaction execution.

## Fail closed

Leave vanilla price unchanged if:

- the original result is non-positive;
- buy/sell direction cannot be classified;
- item classification is unknown where a class-specific rule is required;
- multiplication would overflow;
- the lane throws.

## Validation boundary

The private validation plan records throwaway-save buy-side and sell-side validation, common weapon/armour resale evidence, and disable/reload rollback for the core price lane.

Later presets/class-specific/regional refinements have their own narrower evidence state and should not inherit the core lane's proof automatically.
