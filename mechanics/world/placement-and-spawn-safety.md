<!-- Canonical Wave 5 mechanic page split from docs/reference/WORLD_PLACEMENT_NAVIGATION.md. -->
# World Placement, Navigation, Routes, and Spawn Safety — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/world/placement-and-navigation.md) first for ownership, identities, lifecycle, and proof scope.

## How we interact with it

### Record topology, not just positions

If two roads visually cross, do not call it an intersection until a traversable join is established.

### Treat a route as an ordered relationship

A route should identify the ordered segments/nodes and access conditions, not merely a polyline drawn on a map.

### Separate spawn anchor from spawn definition

The anchor answers where.

The spawn definition answers what, ownership, count, respawn, density, conditions and lifecycle.

### Validate a single actor before population logic

For a new spawn path:

1. one exact non-unique actor/template;
2. one safe anchor;
3. one controlled session;
4. native lifecycle;
5. cleanup;
6. only then density/respawn/persistent population.

### Keep unique/story actors denied by default

Unresolved unique, boss, civilian, quest, story, tutorial or summon ownership blocks ambient reuse.

## Why this route

The game-knowledge programme repeatedly records:

> Coordinates alone are not safe placement proof.

It also preserves observed **absence** rather than inventing a route/spawner.

The bounty-hunter example is instructive: templates existed, but repeated diagnostics found no observed vanilla spawner references. The correct result was "spawn route not established," not "pick a coordinate and spawn one."

## What goes wrong

### Visual road crossing = intersection

Can create impossible route graphs.

### Static route geometry = moving actor proof

A path can exist while AI/movement/state transitions fail.

### Template exists = ambient-spawn safe

Templates can be unique, story-owned, summon-only, debug, or otherwise context-bound.

### Spawn succeeds once = placement safe

Actor may be stuck, inside geometry, off navigation, invalid after reload, or stacked after revisit.

### One-session actor = persistent population

A `MarkedNotSaved` proof deliberately avoids the persistent problem.

### Density ignored

Even individually valid spawns can create performance/gameplay failure when duplicated.

## How to verify

For a placement candidate:

1. exact scene/region/worldspace;
2. exact anchor identity;
3. position/rotation;
4. collision/clearance;
5. ground/height correctness;
6. nav/path evidence;
7. access/gate/state conditions;
8. actor/template eligibility;
9. distance from unsafe transitions/quest objects;
10. one controlled spawn;
11. movement from anchor;
12. combat/interruption if applicable;
13. leave/return;
14. duplicate/density;
15. save/load if persistent;
16. cleanup/rollback;
17. performance/log review.

For a route, additionally prove every ordered segment/junction and full traversal.

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/world/placement-and-navigation.md).
