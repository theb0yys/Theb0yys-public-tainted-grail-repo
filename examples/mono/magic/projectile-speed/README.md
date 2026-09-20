# Magic Projectile Speed

This example teaches the native projectile-tuning path used by a working Magic Tweaks implementation.

It scales player-owned magic projectile speed while leaving unrelated projectiles unchanged.

## What it changes

The patch adjusts the projectile velocity produced by the native magic projectile configuration path.

Use a multiplier of:

- `1.0` for vanilla speed;
- above `1.0` for faster projectiles;
- below `1.0` for slower projectiles.

## Build

~~~powershell
dotnet build .\MagicProjectileSpeed.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Pattern

1. let the game create/configure the projectile;
2. confirm it belongs to the intended magic/player path;
3. scale only the projectile value the mod owns;
4. leave targeting, damage and unrelated projectile systems native.

This is a Mono / BepInEx 5 example.
