---
document_type: case
scope: Magic Tweaks projectile hook evolution
evidence:
  static: DECOMPILED_AND_SOURCE_CORRECTION
last_verified: 2026-09-20
---

# Projectile Route Coverage and Aim Correction

Magic Tweaks exposed two common reverse-engineering mistakes.

## Mistake 1: one launch wrapper was assumed universal

The first hook covered `ConfigureShootProjectile.ApplyToProjectile` but missed simple/other wrapper routes.

The stronger anchor moved later to `DamageDealingProjectile.SetBaseDamageParams`, with a guarded fallback and per-projectile double-scale protection.

## Mistake 2: total velocity was treated as pure aim velocity

FoA's projectile velocity can include offset-correction data.

Scaling the entire vector broke the hand/fire-point-to-crosshair compensation.

The corrected path scales only the recovered aim component and preserves the offset vector.

## Lesson

A “working method” is not automatically the **complete route**, and a convenient value is not automatically a single semantic quantity.
