<!-- Canonical Wave 5 mechanic page split from docs/reference/WORLD_SCENES_TRAVEL.md. -->
# World, Scenes, Portals, and Travel — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/world/scenes-and-travel.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/world/scenes-and-travel.md).
