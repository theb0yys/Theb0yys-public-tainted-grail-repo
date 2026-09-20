# Character Death Observer

Use this example when you need a read-only signal that a character reached the native death stage. It is intended for diagnostics or presentation side effects, not for replacing death, corpse, loot, or reward ownership.

This example uses the terminal character-death lifecycle used by working death-presentation mods.

## What this event means

The death event is the correct place for terminal character-side presentation such as a bounded death burst, death marker or cleanup trigger.

It is **not** the owner of every downstream system. Corpse creation, loot, rewards and persistence have their own lifecycles.

## Build

~~~powershell
dotnet build .\DeathObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Pattern

1. observe the native terminal death event;
2. identify the character once;
3. run only the mod-owned death sidecar;
4. deduplicate repeated callbacks;
5. leave corpse/loot/reward ownership to their native systems.
