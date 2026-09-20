# Carry Capacity Tweak

Patch HeroStats.HeroStatsWrapper.Initialize and attach one non-saved additive StatTweak to the current EncumbranceLimit.

~~~powershell
dotnet build .\CarryCapacityTweak.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Discard old owned tweaks before rebinding to the current stat instance.

Guide: ../../../../guides/tasks/gameplay/change-carry-capacity-safely.md
