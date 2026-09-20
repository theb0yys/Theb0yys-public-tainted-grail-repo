---
document_type: mechanic
scope: player magic projectile speed, lifetime/range and homing tuning
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC_PARTIAL
  persistence: NONE
last_verified: 2026-09-20
---

# Magic Projectile Tuning

FoA magic projectile creation can use more than one launch wrapper. A robust tuning route therefore needs to anchor after enough projectile identity/owner/damage data exists.

## Owner surfaces

Relevant native types include:

- `DamageDealingProjectile`;
- `MagicProjectile`;
- `HomingProjectile`;
- `ConfigureShootProjectile`;
- `ConfigureShotProjectileSimple`;
- `ConfigureHomingProjectile`.

A stronger main anchor identified by later research is:

`DamageDealingProjectile.SetBaseDamageParams(...)`

with the older shoot-config postfix retained only as a guarded fallback.

## Tuning

- speed — scale recovered aim velocity;
- range — scale projectile `LifeTime`;
- homing — scale reviewed private homing fields;
- player-only default — require hero/player ownership evidence.

Use per-projectile state so multiple launch/configuration paths cannot double-scale one instance.

## Not every spell is a projectile

Hitscan, drain, self/buff and other non-projectile spells have no projectile velocity/lifetime to tune.
