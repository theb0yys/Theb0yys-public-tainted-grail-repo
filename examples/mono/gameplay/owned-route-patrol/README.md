# Spawn One Enemy That Patrols a Route

This example creates one Wyrdspirit and gives it a short patrol route using Tainted Grail's own patrol system.

It is a good starting point if you want guards, road patrols, wandering enemies, or other actors that move between known points.

## Build it

~~~powershell
dotnet build .\OwnedRoutePatrol.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save and test somewhere with open ground.

## Try it in game

The example uses two keys:

~~~text
F8  create the patrol
F9  remove the patrol created by this mod
~~~

Press F8 once.

The mod creates one Wyrdspirit near the player and gives it a small three-point route.

Press F9 to remove exactly that patrol actor.

## What to change first

Find the three Vector3 route points in Plugin.cs.

Move those points a small amount and rebuild.

That lets you learn the route part without changing the creature, AI, or cleanup code at the same time.

## How it works

The important steps are:

~~~text
choose exact creature template
→ spawn one game-owned Location
→ mark it as session-only
→ attach the game's PatrolInteraction
→ give PatrolInteraction a TwoWay PatrolPath
→ keep the exact Location reference
→ discard that exact Location when finished
~~~

PatrolInteraction is the game's patrol behavior. The example does not move the enemy by changing its Transform every frame.

When the enemy notices the player, its normal Tainted Grail combat AI still handles the fight.

## Next

[Read the route-patrol guide](../../../../guides/tasks/world/build-a-route-patrol-with-split-ownership.md)
