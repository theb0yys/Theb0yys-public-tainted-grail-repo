# Character Damage Observer

This example uses the character-damage lifecycle that working combat/VFX mods use for read-only observation and presentation sidecars.

It does not replace native damage calculation.

## Useful event data

The native damage object exposes the information needed for common sidecars such as:

- target;
- damage dealer;
- amount;
- stamina damage;
- critical hit;
- weak-spot hit;
- blocked/parried result.

## Build

~~~powershell
dotnet build .\CharacterDamageObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Pattern

Observe the completed damage event, classify only the event you care about, then hand it to your own UI/VFX/audio logic.

Keep damage calculation and target resolution native unless your mod explicitly owns those systems.
