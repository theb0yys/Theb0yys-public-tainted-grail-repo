# Add Extra Hit and Death Effects Without Changing Combat

This example adds small mod-owned particle effects **after** Tainted Grail has already accepted a damage or death event.

It does not change who was hit, how much damage was dealt, whether the target died, or what loot/XP is awarded.

## Build it

~~~powershell
dotnet build .\CombatVfxSidecar.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Fight one normal enemy.

Confirm:

1. a normal hit can create the small extra impact effect;
2. repeated rapid hits are limited by the example's spawn budget;
3. a death creates only one terminal effect for that target;
4. combat behavior is unchanged when the visual effect is disabled.

## What to change first

Change the particle count, size, or lifetime.

Do not change the damage hooks themselves in your first edit.

## How it works

The example listens **after** these game events:

~~~text
HealthElement.OnDamage
HealthElement.OnDeathEvents
~~~

For a hit, it checks that the target is a character and applies a short per-target cooldown before creating a visual.

For a death, it remembers which target already received the death effect so the same death is not decorated repeatedly.

Most importantly, it never patches:

~~~text
Damage.DetermineTargetHit
~~~

That method helps decide gameplay targeting. A visual effect does not need to own that decision.

Every GameObject created by this example has a short lifetime and is also cleaned up when the plug-in unloads.

## Next

[Read the combat VFX guide](../../../../guides/tasks/rendering/build-a-bounded-combat-vfx-sidecar.md)
