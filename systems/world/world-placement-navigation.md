# World Placement, Navigation, Routes, and Spawn Safety

> **Reference page.** Use this when placing actors/objects, choosing spawn anchors, building patrol routes, or reasoning from map coordinates.

## What this system is

A world coordinate answers only:

> where is this point?

It does **not** answer:

- can an actor safely stand there?
- can it navigate from there?
- is it inside the intended scene/region?
- is the route actually connected?
- is a crossing a real navigable intersection?
- is the location quest/story-owned?
- will duplicate actors stack there?
- does the placement survive leave/return or save/load?

The research treats those as separate proofs.

## Who owns it in FoA

Relevant owners can include:

- scene/worldspace identity;
- `MapScene` / scene services;
- native location/spawner systems;
- navigation/pathfinding owners;
- roads/routes/intersections as game-knowledge relationships;
- actor `Location` ownership;
- population/density owners;
- quest/state gates controlling access.

## Important identities, types, and methods

The game-knowledge model distinguishes:

- region;
- worldspace;
- scene;
- area;
- location;
- road;
- road segment;
- intersection;
- route;
- route step;
- game point;
- spawn anchor;
- spawner;
- spawn definition;
- encounter.

These identities should not be collapsed into one coordinate string.

For native actor creation, a researched runtime surface is still `LocationTemplate.SpawnLocation(...)`, but using that call does not prove the chosen placement is safe.

## Where it exists in the lifecycle

A safe placement/spawn investigation is:

~~~text
resolve scene/region
→ identify candidate anchor
→ prove physical clearance
→ prove navigation/traversability
→ prove access/state conditions
→ prove actor/template eligibility
→ create one controlled actor/object
→ observe movement/combat/use
→ leave/return
→ duplicate/density
→ save/load if persistent
→ cleanup/rollback
~~~

A moving route adds:

~~~text
ordered route steps
→ exact road/segment/node relationships
→ entry/exit direction
→ movement owner
→ blocked-route fallback
→ interruption/combat
→ resume/return/despawn
~~~

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

## Current proof boundary

The handbook can teach the placement/topology validation model and several controlled actor lifecycle paths.

It does not claim a generic spawn-anchor database or universal population injection API. Those require reviewed game-knowledge identities and lane-specific runtime validation.
