# Build an Interior Exit Helper

The working Dungeon Exit Helper is intentionally small: remember where the hero entered the current interior and point back to that position.

Working lineage: [Small Exit Helper Instead of a Dungeon Map](../../../research/case-studies/travel/interior-exit-helper.md).

## Runnable source

Start with the [Interior exit helper example](../../../examples/mono/ui/interior-exit-helper/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Scene owner

Use SceneService when available:

- SceneService.ActiveSceneRef.Name
- SceneService.ActiveSceneDisplayName
- SceneService.IsOpenWorld

Fallback to SceneManager.GetActiveScene().name only when SceneService is unavailable.

Build a scene key from the current scene plus whether it is considered an interior. When that key changes:

~~~text
clear old entrance
→ record scene-change time
→ wait briefly for hero placement to settle
→ capture new entrance
~~~

The working implementation uses a default capture delay of 1.25 seconds.

## Capture the entrance

The position source is:

~~~csharp
Hero hero = Hero.Current;
Vector3 entrance = hero.Coords;
~~~

Reject NaN/infinite positions.

Keep the entrance in session memory. You do not need to modify FoA map data or saves.

## Determine whether to show it

The normal interior check is:

~~~text
SceneService available
→ !SceneService.IsOpenWorld
~~~

The working mod also has a scene-name fallback for obvious interior names when SceneService cannot be read.

Optionally hide the marker while:

~~~text
Hero.Current.HeroCombat.IsHeroInFight == true
~~~

## Screen projection

Use Camera.main.WorldToScreenPoint(entrancePosition).

If the point is behind the camera, do not draw it.

Convert Unity screen Y into GUI Y and clamp the result into a safe screen margin so an off-centre entrance still produces an edge marker.

The working implementation also computes:

~~~csharp
float distance = Vector3.Distance(Hero.Current.Coords, entrancePosition);
~~~

That gives a useful "EXIT / 42m" style helper without reconstructing dungeon topology.

## Scene cleanup

On every detected scene change:

- clear _hasEntrance;
- replace the scene key/display name;
- start a new capture delay.

There is no durable map state to migrate or clean up.

## What this feature does not need

Do not reverse-engineer:

- dungeon mesh topology;
- navmesh;
- native map discovery;
- quest state;
- fast-travel data.

For the actual user problem, scene identity + Hero.Current.Coords + Camera projection are enough.
