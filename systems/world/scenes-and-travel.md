<!-- Canonical Wave 5 native-system page split from docs/reference/WORLD_SCENES_TRAVEL.md. -->
# World, Scenes, Portals, and Travel

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/world/scene-and-travel-intervention.md).

## What this system is

FoA scene travel is not owned by a door animation and is not equivalent to raw Unity scene loading.

The researched player travel path is:

~~~text
TravelAction.OnStart
→ Portal.Execute
→ Portal.ExecuteInternal
→ Portal.MapChangeTo
→ ScenePreloader.ChangeMap
→ MapChangeLoading.Load
→ SceneService.LoadSceneAsync
→ Addressables.LoadSceneAsync
~~~

The loaded scene then participates in FoA's scene lifecycle.

## Who owns it in FoA

Important owners include:

- `TravelAction` / `Portal` — travel intent and portal handoff;
- `ScenePreloader` / loading UI — transition orchestration;
- `SceneService` — scene discovery/load/unload and operation tracking;
- `MapScene` / `AdditiveScene` — FoA scene lifecycle owner;
- `SceneConfig` — transition metadata;
- `HeroTeleportMovement` — native arrival/portal targeting.

## Important identities, types, and methods

A native scene route can depend on:

- Unity scene name;
- Addressables address;
- Addressables label `scene`;
- `SceneReference`;
- `SceneConfig`;
- previous-scene/portal tag;
- optional scene-spec identity for authored gameplay objects.

The researched no-hook contract for an external scene requires its Addressables address to match the actual Unity scene name so SceneService can complete the same-name operation handshake.

## Where it exists in the lifecycle

A native additive transition shape is:

~~~text
mod catalogue/locator active
→ SceneService discovers scene-labelled locations
→ SceneConfig exists
→ native Portal/ScenePreloader transition
→ SceneService.LoadSceneAsync
→ Unity loads scene
→ AdditiveScene.Start
→ SceneService.SceneLoaded
→ scene initialization
→ SceneService.SceneInitialized
→ loading completes
→ eventual native unload/return
~~~

## Current proof boundary

The native travel/SceneService path is strongly mapped in the researched Mono build.

The claim that a genuinely new external mod-owned additive scene works end-to-end remains an explicit runtime-validation gap and should stay labelled that way.
