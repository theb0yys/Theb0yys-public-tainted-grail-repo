# Build a Separate Practice Progression Display

This example watches things the player is already doing and builds a small **mod-owned practice counter**.

It does not replace Tainted Grail's normal XP, talent points, or character stats.

## Build it

~~~powershell
dotnet build .\ProgressionOverlay.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Perform a few actions that normally give proficiency XP.

Watch the overlay and confirm the mod records those activities.

Then spend normal talent/stat points and confirm the mod has not changed the normal Tainted Grail spending rules.

## What to change first

Change how one observed activity contributes to one practice category.

Do not start by adding permanent stat effects.

## How it works

Tainted Grail already reports proficiency activity through its normal XP path.

The example observes that path and copies the useful information into its own small counter:

~~~text
game reports proficiency activity
→ example observes it
→ example updates its own practice totals
→ overlay displays those totals
~~~

The mod's categories are its own categories.

A visible talent-tree label does not automatically mean Tainted Grail has a matching hidden proficiency with the same name.

## Next

[Read the progression overlay guide](../../../../guides/tasks/gameplay/build-a-separate-progression-overlay.md)
