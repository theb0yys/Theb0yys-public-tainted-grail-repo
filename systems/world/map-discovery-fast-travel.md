# Map, Discovery, Markers, Fog, and Fast Travel

> **Reference page.** Use this when changing map visibility, discovery, fast-travel eligibility, map markers, or teleport behavior.

## What this system is

Opening the map, allowing fast travel, selecting a marker, and actually teleporting are separate stages.

A researched route includes:

~~~text
service / fireplace / discovery action
→ map opens
→ MapUI allows fast travel
→ player selects MapMarker
→ MapSceneUI.TryFastTravel(MapMarker)
→ cross-scene/local marker teleport owner
→ FoA travel/scene/hero movement lifecycle
~~~

Map fog is another presentation/data layer and should not be confused with travel ownership.

## Who owns it in FoA

Important source-located owners include:

- `MapUI`;
- `MapSceneUI`;
- `MapMarker`;
- `LocationDiscovery`;
- `CrossSceneLocationMarker`;
- native portal/travel/SceneService owners downstream;
- `FogOfWar` for one map-fog surface.

## Important identities, types, and methods

Research/source-located surfaces include:

- `MapUI.AllowFastTravel()`;
- `MapUI.FastTravelAllowed`;
- `MapSceneUI.TryFastTravel(MapMarker)`;
- `LocationDiscovery.OnStart(...)`;
- `LocationDiscovery.Teleport`;
- `CrossSceneLocationMarker.Teleport`;
- `MapUI.ToggleFogOfWar`;
- `FogOfWar.CreateMaskTexture`.

Some fireplace/service paths open the map but **do not themselves teleport**.

That distinction is important.

## Where it exists in the lifecycle

### Fast travel

~~~text
fast-travel source/service
→ map open
→ travel permission enabled
→ marker selection
→ TryFastTravel
→ confirmation/validation
→ marker/location teleport
→ cross-scene/native travel
→ hero arrival
~~~

### Discovery

~~~text
LocationDiscovery becomes active/started
→ map/discovery state
→ marker/fast-travel availability
→ eventual teleport owner
~~~

### Fog

Fog/mask creation and map rendering are presentation/discovery surfaces. They do not by themselves own the world travel action.

## How we interact with it

### Patch the stage you actually want to change

Examples:

- show/hide fog → map/fog owner;
- allow/disallow travel → `MapUI.FastTravelAllowed`/validation stage;
- modify selection/confirmation → map UI route;
- alter destination → marker/location owner;
- alter cross-scene load → portal/scene lifecycle.

### Preserve native travel when possible

If a mod wants to add a guard condition (e.g. unsafe area), intercept before the native teleport and then let the game's marker/travel owner execute when allowed.

### Treat marker identity and location identity separately

A visible marker can be presentation for a LocationDiscovery/world subject.

Do not use marker text as the sole destination identity.

## Why this route

Research on world protection/travel found:

- a fireplace FastTravel action opens the map but does not teleport;
- `LocationDiscovery.OnStart` can open the map and enable fast travel;
- the actual selected marker route proceeds through `MapSceneUI.TryFastTravel` and marker/location teleport ownership.

This prevents patching the wrong "fast travel" method merely because it has the right name.

## What goes wrong

### Map open = travel executed

False.

### Marker UI = destination owner

Not necessarily.

### Fog hook used to gate world travel

Presentation and travel ownership differ.

### Cross-scene marker patched without scene lifecycle

The downstream native scene/portal path still has its own identity/config/load requirements.

### Fast-travel guard added after teleport already started

Too late; lifecycle timing matters.

## How to verify

For fast-travel changes:

1. source/service that opens map;
2. map state;
3. `FastTravelAllowed`;
4. exact marker identity;
5. selection event;
6. validation/confirmation;
7. `TryFastTravel`;
8. destination marker/location;
9. scene travel/teleport;
10. hero arrival;
11. blocked-case UI feedback;
12. save/discovery state if modified.

For fog changes, verify mask generation, map visibility, discovery semantics, scene/map change, and restoration independently.

## Current proof boundary

The working research maps several concrete map/travel surfaces and the downstream native scene route.

This is not a blanket proof for arbitrary new fast-travel markers or persistent discovery records; those need their own identity/persistence validation.
