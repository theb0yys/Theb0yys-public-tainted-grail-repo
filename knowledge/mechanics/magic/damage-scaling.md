---
document_type: mechanic
scope: player magic damage and subtype-weighted scaling
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Magic Damage Scaling

A central scalar seam is the native damage pipeline before final raw calculation.

The researched route filters player-owned magic damage around `HealthElement.OnDamage(Damage)` and applies a multiplier through the damage raw-data modifier.

## Element-aware policy

Native damage data can contain weighted parts such as:

- GenericMagical;
- Fire;
- Cold;
- Poison;
- Electric;
- Wet;
- Wyrdness.

A project can compute a weighted policy multiplier from the subtype percentages.

## Boundary

Do not rewrite the damage owner or skip native `OnDamage`.

Keep subtype policy separate from native subtype identity.

Area-hit radius and visible VFX size are also separate owners: changing overlap radius does not automatically resize the effect art.
