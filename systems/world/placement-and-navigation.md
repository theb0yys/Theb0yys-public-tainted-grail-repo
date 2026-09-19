<!-- Canonical Wave 5 native-system page split from docs/reference/WORLD_PLACEMENT_NAVIGATION.md. -->
# World Placement, Navigation, Routes, and Spawn Safety

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/world/placement-and-spawn-safety.md).

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

## Current proof boundary

The handbook can teach the placement/topology validation model and several controlled actor lifecycle paths.

It does not claim a generic spawn-anchor database or universal population injection API. Those require reviewed game-knowledge identities and lane-specific runtime validation.
