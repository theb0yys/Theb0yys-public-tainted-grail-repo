# World Placement and Navigation

Use this page when you are choosing **where to place an actor/object, where to spawn something, or how to define a patrol/route**.

A coordinate tells you only where a point is.

It does **not** prove the point is safe, reachable, or appropriate.

## A safe placement needs more than XYZ

Check separately:

- scene/region/worldspace;
- physical clearance;
- ground/height;
- navigation/traversability;
- access/state gates;
- nearby quest/story ownership;
- duplicate/density behavior;
- leave/return behavior;
- persistence policy.

## Keep world identities separate

Useful distinctions include:

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

Do not collapse all of these into one coordinate string.

## Spawn anchor vs spawn definition

The **anchor** answers:

> where?

The **spawn definition/owner** answers:

- what actor/object;
- count;
- conditions;
- respawn;
- density;
- lifecycle;
- persistence.

A valid anchor does not prove a valid population system.

## Safe one-actor progression

For a new spawn route, prove in this order:

~~~text
scene/region
→ candidate anchor
→ clearance
→ navigation
→ access/state
→ exact actor/template eligibility
→ one controlled spawn
→ movement/combat/use
→ leave/return
→ duplicates/density
→ save/load if persistent
→ cleanup
~~~

Only after that should you expand to population/density/respawn logic.

## Routes need topology, not just lines

If two roads visually cross, do not call them connected until traversal proves there is an actual junction.

A route should record:

- ordered segments/nodes;
- entry/exit direction;
- access conditions;
- movement owner;
- blocked-route fallback;
- combat/interruption behavior;
- resume/return/despawn behavior.

A polyline on a map is not enough.

## Keep unique/story actors blocked by default

Do not reuse:

- unique actors;
- bosses;
- civilians;
- quest/story characters;
- tutorial actors;
- summon-only actors;

for ambient spawning unless their ownership/eligibility is actually established.

## Common mistakes

### Visual crossing = nav intersection

Can produce impossible route graphs.

### Static path exists = AI can traverse it

Movement/state ownership can still fail.

### Template exists = safe ambient spawn

The template may be story-owned, unique, summon-only, or otherwise context-bound.

### Spawn once = placement safe

The actor may be stuck, off-nav, inside geometry, or duplicate after revisit.

### MarkedNotSaved actor = persistent population

It proves the opposite: deliberate session-only ownership.

### Ignoring density

Individually valid spawns can still create gameplay/performance failures when repeated.

## How to verify a placement

Check:

1. exact scene/region/worldspace;
2. exact anchor identity;
3. position/rotation;
4. collision/clearance;
5. ground/height;
6. navigation/path evidence;
7. access/state conditions;
8. actor/template eligibility;
9. nearby transition/quest hazards;
10. one controlled spawn;
11. movement;
12. combat/interruption if relevant;
13. leave/return;
14. duplicates/density;
15. save/load if persistent;
16. cleanup/rollback;
17. performance/log behavior.

For routes, also prove every ordered segment/junction and a complete traversal.

## Evidence limits

The repository can teach the placement/topology validation method and several controlled actor lifecycles.

It does not provide a universal safe-spawn database or universal population injection API.
