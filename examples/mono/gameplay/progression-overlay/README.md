# Separate Progression Overlay

A buildable read-only practice overlay.

It observes the same native proficiency event chain used by Immersive Progression:

~~~text
ProficiencyEventListener.XPGainEvent
→ thread-local source context
→ ProficiencyStats.TryAddXP
→ mod-owned practice counters
→ overlay
~~~

It does not alter vanilla XP, RPG-stat points, talent points, or TalentTreeBase.IsUpgradeAvailable.

## Build

~~~powershell
dotnet build .\ProgressionOverlay.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build a separate progression overlay](../../../../guides/tasks/gameplay/build-a-separate-progression-overlay.md)
