# Build a Route Patrol with Native PatrolInteraction

The working route-patrol implementation keeps route authority, actor ownership, and native movement separate.

Working lineage: [Wyrd Route Patrol: Three Owners, One Feature](../../../research/case-studies/encounters/wyrd-route-patrol-ownership.md).

## Runnable source

Start with the [Owned route patrol example](../../../examples/mono/gameplay/owned-route-patrol/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Working owner split

- Wyrd Hunt publishes the read-only Wyrdness/exposure state.
- Avalon Core owns reviewed route identity and ordered route geometry.
- Living Avalon owns spawned patrol actors and cleanup.
- FoA owns navigation, PatrolInteraction movement, detection and combat.

Wyrd Hunt does not spawn or move actors. Living Avalon does not invent route geometry.

## Exact Beach route example

The maintained contract includes:

~~~text
routeRef: ROUTE-HOTS-BEACH-MAIN-SETTLEMENT-PATROL
routeScene: CampaignMap_HOS
spawnScene: CampaignMap_HOS_merged
expected route points: 40
start: anchor:survey:beachsettlement20260710:start
end:   anchor:survey:beachsettlement20260710:end
~~~

A known actor on that route is:

~~~text
Spec_EnemyMonster_T1_Wyrdspirit
843643575fa01ba4292e60afb9291fea
~~~

The same route contract can hold independent one-actor registrations for other reviewed templates.

## Spawn path

For one route slot:

1. require the route scene/authorization to be active;
2. select the exact registered route point;
3. resolve the exact LocationTemplate;
4. call BaseLocationSpawner.VerifyPosition(...) for the candidate placement;
5. call LocationTemplate.SpawnLocation(...);
6. immediately set Location.MarkedNotSaved = true;
7. wait for native Location initialization;
8. require a live NpcElement before binding.

Keep one actor per registration key. Do not respawn the same registration repeatedly during one exposure window.

## Bind to the native patrol service

Living Avalon's RoutePatrolService takes a registration plus:

- owned actor ID;
- live NpcElement;
- ordered waypoints;
- at least two route points.

The service creates the native patrol behavior through:

~~~text
PatrolInteraction
→ PatrolPath
→ PatrolPath.Type.TwoWay
~~~

Do not implement your own transform walker when the native patrol interaction is available.

The registration is also where owner ID, route ID, expected actor count and route metadata stay together.

## Combat remains native

The patrol layer only owns movement policy.

Once the actor encounters the hero, normal FoA NPC detection/combat owns the fight. Do not make the route service become a second combat director.

## Cleanup

When the exposure window closes, the actor becomes invalid, the route unloads, or the owning feature shuts down:

~~~text
owned registration
→ unbind native patrol
→ mark owned Location not saved
→ discard exact owned Location
→ clear registration/session state
~~~

Do not search the world for "similar" actors and delete them.

## Reusable rule

A good route-patrol feature is:

~~~text
read condition
→ resolve reviewed route
→ create one owned actor
→ bind native PatrolInteraction
→ native AI owns combat
→ unbind/discard exact owned actor
~~~
