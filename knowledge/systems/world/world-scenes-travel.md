# Scenes, Portals, and Travel

Use this page when you are changing **scene transitions, portals, custom interiors, arrival behavior, or scene-lifetime hooks**.

The key rule is:

> A door animation or raw Unity scene load is not the same thing as FoA travel.

## Native travel flow

The researched player path is:

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

Then the loaded scene enters FoA's own scene lifecycle.

## Main owners

- `TravelAction` / `Portal` — travel request and portal handoff;
- `ScenePreloader` / loading UI — transition orchestration;
- `SceneService` — scene discovery, load/unload, operation tracking;
- `MapScene` / `AdditiveScene` — FoA scene lifecycle;
- `SceneConfig` — transition metadata;
- `HeroTeleportMovement` — arrival/portal targeting.

## What a custom scene needs

A native scene route can depend on:

- Unity scene name;
- Addressables address;
- Addressables label `scene`;
- `SceneReference`;
- `SceneConfig`;
- previous-scene/portal tag;
- optional scene-spec identity for authored gameplay objects.

The researched no-hook route requires the Addressables address to match the actual Unity scene name so `SceneService` can complete its same-name operation tracking.

## Native additive scene lifecycle

~~~text
mod catalogue/locator active
→ SceneService discovers scene-labelled resource
→ SceneConfig available
→ native Portal/ScenePreloader transition
→ SceneService.LoadSceneAsync
→ Unity loads scene
→ AdditiveScene.Start
→ SceneService.SceneLoaded
→ scene initialization
→ SceneService.SceneInitialized
→ later readiness events
→ native unload/return
~~~

## Scene identity is not readiness

Useful scene state includes:

~~~text
SceneService.MainSceneRef
SceneService.AdditiveSceneRef
ActiveSceneRef = AdditiveSceneRef when present, otherwise MainSceneRef
~~~

Additional lifecycle signals include:

- `AfterNewDomainSet`
- `EverythingInitialized`
- `AfterSceneFullyInitialized`
- `AfterSceneStoriesExecuted`
- `SafeAfterSceneChanged`

Do not start Hero/Story/scene-owned work merely because `ActiveSceneRef` changed.

Use the readiness point that matches what your feature actually needs.

## Prefer the native travel path

If your feature needs:

- FoA loading screens;
- scene/domain ownership;
- native services;
- arrival targeting;
- correct unload/cleanup;

then use the native Portal/SceneService flow rather than a parallel `SceneManager.LoadScene` path.

## Prove transport before gameplay

For a new custom scene, first prove a minimal scene can:

1. be discovered;
2. load through the native transition;
3. initialize;
4. give control back to the player;
5. unload/return cleanly.

Only then add NPCs, Story, navigation, complex rendering, or custom specs.

## Common mistakes

### Door state treated as travel ownership

A door can open/animate without owning a map change.

### Addressables load treated as complete scene integration

Unity may load the scene while FoA's domain/scene handshake is incomplete.

### Missing/mismatched SceneConfig or scene name/address

The native path can fail even though the bundle itself is valid.

### Coordinates treated as a complete destination

Arrival still depends on scene identity, portal targeting, placement safety, and native movement ownership.

## How to verify custom travel

Check:

1. catalogue resolves the scene;
2. scene-labelled resource is discoverable;
3. `SceneConfig` exists;
4. native transition begins;
5. Addressables loads the exact scene;
6. `SceneLoaded` completes;
7. `SceneInitialized` completes;
8. arrival/player control is valid;
9. native return works;
10. additive scene unloads;
11. Models/listeners/assets are cleaned up.

## Evidence limits

The native travel/`SceneService` path is strongly mapped in the inspected Mono build.

A genuinely new external mod-owned additive scene still needs end-to-end runtime validation before it should be called proven.
