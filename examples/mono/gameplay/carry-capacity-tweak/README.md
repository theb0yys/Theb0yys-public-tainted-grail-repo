# Carry Capacity Tweak

This example patches HeroStats.HeroStatsWrapper.Initialize and always targets the current HeroStats.EncumbranceLimit instance.

Before attaching a new non-saved additive StatTweak it discards older tweaks owned by this example.

## Build

~~~powershell
dotnet build .\CarryCapacityTweak.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Change TotalCarryCapacity in the generated BepInEx config and reload/reinitialize the hero stats to observe the current stat instance being updated.

Guide: [Change carry capacity safely](../../../../guides/tasks/gameplay/change-carry-capacity-safely.md)
