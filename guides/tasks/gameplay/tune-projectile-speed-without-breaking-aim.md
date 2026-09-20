# Tune Projectile Speed Without Breaking Aim

**Evidence status: PARTIAL.** The corrected route/velocity model is established; a complete public runtime route matrix is not recorded.

Working lineage: [Projectile Route Coverage and Aim Correction](../../../research/case-studies/magic/projectile-route-and-aim.md).

## Two important corrections

### One launch wrapper is not universal

The initial hook at `ConfigureShootProjectile.ApplyToProjectile` missed other/simple projectile routes.

The stronger later anchor moved to:

```text
DamageDealingProjectile.SetBaseDamageParams
```

with a guarded fallback and per-projectile double-scale protection.

### Total velocity is not pure aim velocity

FoA projectile velocity can include offset-correction data.

Scaling the whole vector can break hand/fire-point-to-crosshair compensation.

## Safer process

```text
projectile reaches common damage/configuration owner
→ recover aim component + offset correction
→ scale aim component only
→ preserve offset vector
→ prevent duplicate scaling
```

## Verification

For each projectile family you claim:

- hook route fires;
- speed changes;
- crosshair alignment remains correct;
- offset compensation remains intact;
- no projectile is scaled twice;
- alternate/simple wrapper routes are covered or explicitly unsupported.

## Current proof boundary

The corrected later anchor, fallback concept and aim-vs-offset distinction are established. Complete runtime coverage across all projectile routes remains to be validated.
