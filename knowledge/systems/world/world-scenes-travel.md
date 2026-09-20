# World, Scenes, Portals, and Travel

> **Reference page.** Use this when investigating scene transitions, custom interiors, portals, world placement, or scene-lifetime hooks.

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

## Scene identity is not scene readiness

The scene/domain system exposes several distinct milestones.

Static/source research establishes:

~~~text
SceneService.MainSceneRef
SceneService.AdditiveSceneRef
ActiveSceneRef = AdditiveSceneRef when present, otherwise MainSceneRef
~~~

A domain/scene identity change is earlier than full gameplay readiness. Additional researched lifecycle signals include:

- `AfterNewDomainSet` — domain identity/lifetime changed;
- `EverythingInitialized`;
- `AfterSceneFullyInitialized`;
- `AfterSceneStoriesExecuted`;
- `SafeAfterSceneChanged`.

Do not treat `ActiveSceneRef` changing, or even a new domain being set, as proof that every scene-owned system, Story or Hero-dependent consumer is ready. Select the milestone that matches the state your mod actually needs.

## How we interact with it

### Prefer the native travel path

If you want FoA-managed travel, use the native scene/travel ownership rather than bypassing it with a parallel Unity load path.

### Treat scene metadata as part of the mechanism

A bundle containing a scene is not enough.

The native transition can require:

- scene label;
- exact name/address;
- config entry;
- expected FoA scene root/lifecycle component.

### Isolate transport before gameplay

For custom-scene research, first prove a minimal additive interior before adding:

- NPCs;
- story;
- navigation;
- complex renderer systems;
- custom specs.

## Why this route

Static/source research found several concrete failure boundaries:

- door animation/state is not scene travel ownership;
- name/address mismatch can break `SceneService.SceneLoaded` lookup;
- missing `SceneConfig` can break the transition path;
- raw Unity loading can skip FoA domain/loading orchestration.

## What goes wrong

### Door code treated as travel owner

A door can animate/toggle without owning the map change.

### Addressables load treated as native scene integration

The Unity scene can load while FoA's SceneService/MapScene/AdditiveScene handshake is incomplete.

### Static custom-scene research called runtime proof


### Coordinates treated as placement proof

Valid placement also needs scene/region identity, collision/clearance, navigation/owner, activation and persistence policy.

## How to verify

For a custom scene/travel proof:

1. catalogue resolves the scene;
2. scene-labelled resource is discoverable;
3. `SceneConfig` exists;
4. native transition begins;
5. Addressables loads exact scene;
6. `SceneLoaded` completes;
7. `SceneInitialized` completes;
8. arrival/player control is valid;
9. native return works;
10. additive scene unloads cleanly;
11. Models/listeners/assets are not leaked.

## Current proof boundary

The native travel/SceneService path is strongly mapped in the researched Mono build.

The claim that a genuinely new external mod-owned additive scene works end-to-end remains an explicit runtime-validation gap and should stay labelled that way.
