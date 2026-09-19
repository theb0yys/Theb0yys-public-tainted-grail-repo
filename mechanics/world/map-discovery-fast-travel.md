<!-- Canonical Wave 5 mechanic page split from docs/reference/MAP_DISCOVERY_FAST_TRAVEL.md. -->
# Map, Discovery, Markers, Fog, and Fast Travel — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/world/map-discovery-fast-travel.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/world/map-discovery-fast-travel.md).
