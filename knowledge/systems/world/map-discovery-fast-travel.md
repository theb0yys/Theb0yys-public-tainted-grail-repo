# Map Discovery and Fast Travel

Use this page when you want to change **map visibility, discovery, fast-travel permission, marker selection, or teleport behavior**.

Opening the map and teleporting are not the same operation.

## Fast-travel flow

A researched route is:

~~~text
service / fireplace / discovery action
→ map opens
→ MapUI allows fast travel
→ player selects MapMarker
→ MapSceneUI.TryFastTravel(MapMarker)
→ marker/location teleport owner
→ native travel/scene/Hero movement
~~~

Useful methods include:

- `MapUI.AllowFastTravel()`
- `MapUI.FastTravelAllowed`
- `MapSceneUI.TryFastTravel(MapMarker)`
- `LocationDiscovery.OnStart(...)`
- `LocationDiscovery.Teleport`
- `CrossSceneLocationMarker.Teleport`

Some actions that look like "fast travel" only open the map and enable travel; they do not perform the teleport themselves.

## Discovery is another stage

~~~text
LocationDiscovery activates
→ map/discovery state updates
→ marker/fast-travel availability changes
→ later selected teleport route
~~~

Do not patch marker UI text and assume you changed discovery truth.

## Fog is presentation, not travel

Relevant fog surfaces include:

- `MapUI.ToggleFogOfWar`
- `FogOfWar.CreateMaskTexture`

Changing the map mask can change what the player sees without changing:

- destination ownership;
- travel permission;
- scene transition;
- discovery persistence.

## Patch the stage you actually mean

Examples:

- hide/show fog → fog owner;
- allow/disallow travel → permission/validation stage;
- change selection/confirmation → map UI route;
- change destination → marker/location owner;
- change cross-scene transport → Portal/SceneService lifecycle.

## Add guards before teleport begins

If you want to block unsafe travel, intercept before the native teleport/scene transition starts.

Then, when allowed, let the native marker/travel owner continue.

## Marker identity vs destination identity

A visible marker can be only the presentation of a `LocationDiscovery` or other world subject.

Do not use marker text as the sole destination identity.

## Common mistakes

- map opened = teleport occurred;
- marker UI = destination owner;
- fog hook = travel guard;
- cross-scene marker patched without handling scene lifecycle;
- block added after teleport already started.

## How to verify fast-travel changes

Check:

1. what opens the map;
2. current map state;
3. `FastTravelAllowed`;
4. exact marker identity;
5. selection event;
6. validation/confirmation;
7. `TryFastTravel`;
8. destination marker/location;
9. scene/local teleport;
10. Hero arrival;
11. blocked-case feedback;
12. save/discovery state if modified.

For fog changes, verify mask generation, visibility, discovery semantics, map/scene transition, and restoration independently.

## Evidence limits

The map/travel path is strongly mapped for several concrete cases.

Arbitrary new fast-travel markers and persistent discovery records still require their own identity and persistence validation.
