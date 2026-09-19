# 04 — Magic Projectile Speed Example

**Category:** magic / projectiles  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example scales the velocity of player-owned magic projectiles.

The owner-side path has actual user-reported projectile-speed behavior plus plugin-load/patch evidence.

## Why there are two hooks

FoA has more than one projectile setup route. The example observes:

- DamageDealingProjectile.SetBaseDamageParams
- ConfigureShootProjectile.ApplyToProjectile

A ConditionalWeakTable marker prevents the same projectile from being scaled twice.

## Filter

The example changes only projectiles that:

- are owned by Hero.Current; and
- are MagicProjectile or use a magic source item.

This avoids teaching a global "multiply every projectile in the game" patch.

## Build

~~~powershell
dotnet build .\MagicProjectileSpeedExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Try SpeedMultiplier=1.5 first, not an extreme value.

## Important detail

The code separates the projectile's existing position-offset velocity from its main launch velocity before scaling, then adds the offset back. That preserves the native offset/aim correction better than blindly multiplying the whole vector.
