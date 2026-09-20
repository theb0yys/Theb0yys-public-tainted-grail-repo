# Show More of the Map Without Changing Discovery

This example changes **what the map displays** without telling Tainted Grail that the player has actually explored those places.

That distinction keeps map presentation separate from saved discovery progress.

## Build it

~~~powershell
dotnet build .\MapFogDisplay.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

1. open a save where part of the map is still unexplored;
2. open the Character Sheet map;
3. confirm the fog mask is hidden;
4. confirm map markers can be shown;
5. disable/remove the plug-in and reopen the map.

The normal map presentation should return when the mod is no longer active.

## What to change first

The project has separate settings for:

- hiding the fog mask;
- revealing marker display.

Toggle one at a time so you can see which part each setting controls.

## How it works

The example changes three display-related game calls:

~~~text
MapUI.ToggleFogOfWar
FogOfWar.CreateMaskTexture
FogOfWar.IsPositionRevealed
~~~

It deliberately does **not** change:

~~~text
MapMemory.visitedPixels
~~~

That is the saved discovery information.

So this mod can change what the player sees without rewriting what the game believes has been explored.

## Next

[Read the map-fog display guide](../../../../guides/tasks/ui/change-map-fog-display-only.md)
