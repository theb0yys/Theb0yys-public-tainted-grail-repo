# Make Projectiles Faster Without Moving Them Off Aim

This example changes projectile speed while preserving the correction Tainted Grail uses between the firing point and the actual aim direction.

That matters because multiplying the whole velocity vector can make a projectile travel faster **and** drift away from where the player aimed.

## Build it

~~~powershell
dotnet build .\ProjectileSpeedAndAim.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Find SpeedMultiplier in the example and start with a small increase.

Test one known projectile repeatedly at a fixed target.

Confirm:

- the projectile is visibly faster;
- it still travels toward the same point;
- one projectile is not scaled twice.

## What to change first

Change only SpeedMultiplier.

Leave LifetimeMultiplier at its normal value until speed and aim are behaving correctly.

## How it works

The example watches two projectile setup routes:

- DamageDealingProjectile.SetBaseDamageParams;
- ConfigureShootProjectile.ApplyToProjectile as a fallback.

It remembers each projectile it already changed so the same object is not multiplied twice.

For velocity, it separates:

~~~text
the part that aims at the target
+
the small correction from the firing position
~~~

Only the aiming part is scaled. The original correction is then added back.

## Next

[Read the projectile speed and aim guide](../../../../guides/tasks/gameplay/tune-projectile-speed-without-breaking-aim.md)
