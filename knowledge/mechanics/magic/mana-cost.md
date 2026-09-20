---
document_type: mechanic
scope: player magic mana-cost multipliers
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Magic Mana Cost

Normal magic cost helpers pass through:

`MagicUtils.GetManaCostMultiplier(ICharacter, Item)`

This provides a narrow scalar seam for player magic cost policy.

## Special projectile-regeneration case

`MagicRangedItem` keeps a saved base projectile mana cost and constructs a private runtime `StatCost`.

A compatibility-minded route can refresh the runtime cost from the original saved base rather than rewriting the saved base field.

## Rule

Keep “cost multiplier” separate from:

- damage;
- cooldown;
- cast recovery;
- projectile velocity;
- item-template mutation.

One user-facing “magic cost” concept may span several native owners.
