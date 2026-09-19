<!-- Canonical Wave 5 native-system page split from docs/reference/MAP_DISCOVERY_FAST_TRAVEL.md. -->
# Map, Discovery, Markers, Fog, and Fast Travel

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/world/map-discovery-fast-travel.md).

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

## Current proof boundary

The working research maps several concrete map/travel surfaces and the downstream native scene route.

This is not a blanket proof for arbitrary new fast-travel markers or persistent discovery records; those need their own identity/persistence validation.
