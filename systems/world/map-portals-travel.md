<!-- Canonical Wave 5 native-system page split from docs/reference/MAP_TRAVEL.md. -->
# Map, Portals, Travel, and Scene Transition Ownership

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/world/map-portals-travel.md).

## What this system is

A travel action can involve:

~~~text
map/discovery UI
→ fast-travel or portal action
→ native portal state/target
→ ScenePreloader
→ SceneService / Addressables
→ destination scene initialization
→ hero relocation/portal state
→ dependent actor/summon cleanup or reconstruction
~~~

Patching the map screen is not equivalent to owning scene travel.

## Who owns it in FoA

Important researched owners/surfaces include:

- `MapUI`
- `FogOfWar`
- `LocationDiscovery`
- `Portal`
- `Portal.FastTravel`
- `ScenePreloader`
- `SceneService`
- cross-scene markers
- native player/portal state
- actor/summon transition handlers

## Important identities, types, and methods

Known surfaces include:

- `MapUI.ToggleFogOfWar(...)`
- `FogOfWar.CreateMaskTexture(...)`
- `FogOfWar.IsPositionRevealed(...)`
- `Portal.Execute` / `ExecuteInternal`
- `Portal.MapChangeTo(...)`
- `ScenePreloader.ChangeMap(...)`
- `Portal.FastTravel.To(...)`
- `LocationDiscovery.Teleport()`
- `CrossSceneLocationMarker.Teleport()`
- `SceneService.LoadSceneAsync(...)`

## Where it exists in the lifecycle

The researched native portal chain includes:

~~~text
Portal.Execute
→ Portal.ExecuteInternal
→ Portal.MapChangeTo
→ ScenePreloader.ChangeMap
→ SceneService/native scene lifecycle
~~~

`Portal.MapChangeTo` preserves native hero portal state and target-tag handoff rather than bypassing the transition.

Fast travel and other relocation paths can have additional entry points/events.

## Current proof boundary

Native portal→ScenePreloader→SceneService ownership is strongly mapped.

Map fog hooks have concrete source/build evidence.

Custom external-scene injection and broad fast-travel mutation require separate runtime validation and are not generic proven processes.
