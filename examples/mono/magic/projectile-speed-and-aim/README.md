# Projectile Speed and Aim

This example patches DamageDealingProjectile.SetBaseDamageParams plus ConfigureShootProjectile.ApplyToProjectile as a fallback.

A ConditionalWeakTable prevents the same projectile instance from being scaled twice.

Aim correction is preserved:

~~~text
final velocity
- PositionOffset.InitialVelocity
= aim component

aim component × multiplier
+ original offset component
= new final velocity
~~~

Build:

~~~powershell
dotnet build .\ProjectileSpeedAndAim.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Edit SpeedMultiplier first. LifetimeMultiplier demonstrates a separate projectile-owned field.

Guide: ../../../../guides/tasks/gameplay/tune-projectile-speed-without-breaking-aim.md
