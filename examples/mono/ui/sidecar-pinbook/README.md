# Save Your Own Map Notes

This example lets the player save named world positions without changing Tainted Grail's map data.

Think of it as a small personal pin notebook owned entirely by the mod.

## Build it

~~~powershell
dotnet build .\SidecarPinbook.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

The default controls are:

~~~text
F6  save the current position
F7  show or hide the small pin list
F9  delete the nearest saved pin
~~~

Save two or three positions, move away, then use the HUD list to see their approximate distances.

Restart the game and confirm the pins load again from the mod's own file.

## What to change first

Change the maximum number of saved pins or the keyboard shortcuts.

Those are safer first edits than changing the file format.

## How it works

The current player position comes from:

~~~text
Hero.Current.Coords
~~~

The mod saves its own file under BepInEx\config.

Each row contains:

~~~text
file version
pin name
X position
Y position
Z position
~~~

The file is written through a temporary file first, so a failed write is less likely to damage the previous copy.

This example does not write Tainted Grail's map discovery, fast-travel, or quest-marker data.

## Next

[Read the map pinbook guide](../../../../guides/tasks/ui/build-a-sidecar-map-pinbook.md)
