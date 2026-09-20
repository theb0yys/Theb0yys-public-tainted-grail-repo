# Change Carry Capacity

This example changes how much the player can carry without changing item weights.

## Build it

~~~powershell
dotnet build .\CarryCapacity.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Start with a clear inventory weight.

Change the target carry capacity in the config, then load or reload the player state so the hero stats are initialized again.

Check that the carry limit changes while the individual item weights stay the same.

## What to change first

Change only the configured total carry capacity.

Do not edit the stat-hook code until you understand why it runs when hero stats are rebuilt.

## How it works

The player carry limit lives in:

~~~text
HeroStats.EncumbranceLimit
~~~

Tainted Grail may recreate that stat object during the player's lifetime.

For that reason, the example patches:

~~~text
HeroStatsWrapper.Initialize
~~~

Every time the game creates the current hero stats, the mod:

1. removes its older carry-capacity tweak;
2. reads the new current EncumbranceLimit;
3. adds one temporary amount needed to reach the configured total.

The tweak is marked as not saved.

## Next

[Read the carry-capacity guide](../../../../guides/tasks/gameplay/change-carry-capacity-safely.md)
