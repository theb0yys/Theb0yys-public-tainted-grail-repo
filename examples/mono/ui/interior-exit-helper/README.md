# Show the Way Back to an Interior Entrance

This example remembers where the player entered an interior and displays a simple **EXIT** marker pointing back to that position.

It does not build a dungeon map.

## Build it

~~~powershell
dotnet build .\InteriorExitHelper.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

1. enter a cave, dungeon, crypt, mine, or another interior;
2. wait a moment after the scene loads;
3. walk away from the entrance;
4. look around.

You should see an EXIT marker with the approximate distance back to the remembered entry point.

When you leave the scene, the old position is forgotten automatically.

## What to change first

In Plugin.cs, find the short delay used before the entrance position is captured.

Try changing that delay slightly.

You can also change the marker text or the screen margin without changing any game logic.

## How it works

The mod uses:

- Hero.Current.Coords for the player's world position;
- SceneService to tell whether the current scene is an interior when available;
- Camera.main.WorldToScreenPoint(...) to convert the saved world position into a screen position.

That is enough for this feature. It does not need to read the dungeon mesh, navmesh, quest data, or map-discovery data.

## Next

[Read the interior exit-helper guide](../../../../guides/tasks/world/build-an-interior-exit-helper.md)
