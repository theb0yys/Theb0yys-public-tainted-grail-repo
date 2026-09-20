# Carry Capacity

This example follows the current HeroStats.EncumbranceLimit instance instead of holding a stale Stat reference.

The postfix runs whenever HeroStatsWrapper.Initialize rebuilds hero stats, removes older mod-owned carry tweaks, and attaches one non-saved additive StatTweak to the current EncumbranceLimit.

## Build

~~~powershell
dotnet build .\CarryCapacity.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Change carry capacity safely](../../../../guides/tasks/gameplay/change-carry-capacity-safely.md)
