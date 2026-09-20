# Tune Projectile Speed Without Breaking Aim

Use the common projectile configuration path, preserve FoA's offset-correction vector, and prevent the same projectile instance from being scaled twice.

Working lineage: [Projectile Route Coverage and Aim Correction](../../../research/case-studies/magic/projectile-route-and-aim.md).

## Runnable source

Start from the buildable example: [Projectile speed](../../../examples/mono/magic/projectile-speed/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Main hook

The current implementation uses a postfix on:

~~~text
Awaken.TG.Main.AI.Fights.Projectiles.DamageDealingProjectile.SetBaseDamageParams(...)
~~~

This is a later/common route than `ConfigureShootProjectile.ApplyToProjectile` and catches projectile shapes that do not pass through the original wrapper.

Keep the old `ConfigureShootProjectile.ApplyToProjectile` postfix only as a fallback for routes that configure projectile velocity without reaching `SetBaseDamageParams`.

## Guard against double scaling

Because both hooks may see the same instance, keep per-projectile state.

The working implementation uses a `ConditionalWeakTable` keyed by the projectile instance. Record which scalers have already been applied so speed/lifetime/homing changes run once per projectile.

~~~text
SetBaseDamageParams postfix
      ↘
       per-projectile state → already scaled? → leave unchanged
      ↗
ConfigureShootProjectile fallback
~~~

## Magic-projectile classification

Do not rely only on `projectile is MagicProjectile`.

The implementation accepts magic evidence from several sources, including:

- `MagicProjectile`;
- magic source item;
- magic projectile item;
- `DamageType.MagicalHitSource`;
- supported magic damage subtype.

That catches simple/ballistic magic projectiles that use another concrete projectile subclass.

## Preserve aim correction

FoA projectile velocity may contain two pieces:

~~~text
aim velocity
+ spawn/fire-point offset correction
= final velocity
~~~

Scaling the entire final vector scales the correction too and can move the projectile away from the crosshair path.

The safe shape is:

~~~text
read current velocity
→ recover/retain offset-correction component
→ isolate aim component
→ scale aim component
→ add original correction back
→ write final velocity
~~~

Do not treat every velocity vector as a pure forward-speed vector.

## Other projectile fields

The working patch also reaches projectile-owned values such as:

- `DamageDealingProjectile.<LifeTime>k__BackingField`;
- `Projectile._rb`;
- `HomingProjectile.homingStrength`;
- `HomingProjectile` velocity-limit data.

Keep each change separately guarded in the per-projectile state.

## Diagnostics worth keeping

For route debugging, record a bounded row containing:

- hook route;
- player ownership;
- magic-classification reason;
- source item/projectile identity;
- velocity before/after;
- lifetime before/after;
- homing before/after;
- which scaler actually ran.

That makes missed projectile families obvious without logging every physics tick.
