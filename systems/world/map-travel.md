# Map, Portals, Travel, and Scene Transition Ownership

> **Reference page.** Map UI, fast travel, portal execution, discovery, and actual scene loading are related but distinct systems.

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

## How we interact with it

### UI-only map changes

A fog-of-war mod can target MapUI/FogOfWar without claiming ownership of travel/discovery persistence.

### Travel observation

Observe the highest relevant semantic entry point and leave native scene transition ownership intact.

### Actor/summon compatibility

If a mod owns session actors, explicitly decide what happens during:

- portal;
- fast travel;
- scene transition;
- long teleport;
- rest.

Do not allow actors to become orphaned accidentally.

### New/custom scenes

Use the native SceneService/Addressables path when trying to participate in FoA scene lifecycle. Do not substitute raw `SceneManager.LoadScene` if native domain/service behavior matters.

## Why this route

Travel research shows that "teleport" is not one operation.

A portal transition also carries native state, scene-loading, destination and actor-lifecycle behavior.

Hooking a lower-level movement/scene call can bypass important ownership.

## What goes wrong

- map button treated as scene owner;
- fog/discovery state conflated with travel permission;
- direct Unity scene load bypasses FoA scene services/domains;
- summons/companions persist through transition without cleanup/reconstruction policy;
- travel hook blocks native Story/system relocations unintentionally;
- coordinates used without scene/worldspace ownership;
- custom scene static/source coherence called runtime-proven before empirical gate.

## How to verify

For travel work, test:

- ordinary portal;
- fast travel;
- long teleport if relevant;
- source/destination scene names/refs;
- hero location/state;
- dependent actors;
- UI/fog/discovery separately;
- transition cancellation/failure;
- save/load/return;
- cleanup;
- native Story/system-triggered relocation remains unaffected unless intentionally changed.

## Current proof boundary

Native portal→ScenePreloader→SceneService ownership is strongly mapped.

Map fog hooks have concrete source/build evidence.

Custom external-scene injection and broad fast-travel mutation require separate runtime validation and are not generic proven processes.
