# 02 — Carry Capacity Example

**Category:** player stats / inventory  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This real FoA-target example changes HeroStats.EncumbranceLimit with a non-saved additive StatTweak.

Set TotalCarryCapacity to 0 to use vanilla.

## Build

~~~powershell
dotnet build .\CarryCapacityExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Important design point

Do not edit every item's weight just to change player carrying capacity. This example owns the final carry-limit stat only.

The example discards its old tweak before recalculating it, which matters when config is changed more than once.

## Evidence warning

The underlying carry-weight plug-in has plugin-load/config evidence, but the latest target-capacity behavior still required an in-game character-sheet/encumbrance check in the inspected records.
