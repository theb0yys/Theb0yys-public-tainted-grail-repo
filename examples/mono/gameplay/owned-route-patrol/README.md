# Owned Route Patrol

This example spawns one exact Wyrdspirit, marks the Location not saved, then binds the actor to FoA's native PatrolInteraction with a TwoWay PatrolPath.

Build:

~~~powershell
dotnet build .\OwnedRoutePatrol.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save and open ground.

Controls:

~~~text
F8  create one owned patrol
F9  discard the exact owned Location and patrol host
~~~

The example uses a short local three-point route so it is immediately runnable. Replace those Vector3 points with reviewed route geometry when integrating a real world route.

Guide: ../../../../guides/tasks/world/build-a-route-patrol-with-split-ownership.md
