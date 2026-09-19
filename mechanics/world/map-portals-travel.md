<!-- Canonical Wave 5 mechanic page split from docs/reference/MAP_TRAVEL.md. -->
# Map, Portals, Travel, and Scene Transition Ownership — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/world/map-portals-travel.md) first for ownership, identities, lifecycle, and proof scope.

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

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/world/map-portals-travel.md).
