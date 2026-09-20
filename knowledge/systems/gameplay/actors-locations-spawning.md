# Actors, NPCs, and Runtime Spawning

Use this page when you want to **spawn, control, dismiss, or persist an NPC, creature, or companion**.

The key rule is:

> A spawned GameObject is not automatically a FoA actor.

A native actor normally has a chain like:

~~~text
LocationTemplate
→ Location
→ NpcElement
→ AI/combat/movement/presentation
→ death/corpse lifecycle
→ cleanup
→ persistence policy
~~~

## Important native owners

- `LocationTemplate` — reusable actor/location-spawn definition.
- `Location` — runtime world/MVC owner.
- `NpcElement` — NPC runtime Element.
- `DeathElement`, `NpcDummy`, `Corpse` — death/corpse-related state.
- native spawner/population systems — durable or ambient population ownership.

Project/framework resolver APIs can wrap these systems, but they are not native FoA contracts by themselves.

## Basic one-session spawn path

Source-inspected paths include:

- `new TemplateReference(guid).Get<LocationTemplate>()`
- `LocationTemplate.SpawnLocation(position, rotation)`
- `Location.MarkedNotSaved = true`
- `Location.Discard()`
- `NpcElement.IsAlive`
- `NpcElement.KeepCorpseAfterDeath`

A bounded temporary-actor flow is:

~~~text
resolve exact LocationTemplate
→ choose placement
→ SpawnLocation
→ wait for Location/NpcElement readiness
→ verify expected identity/components
→ mark not saved
→ let native AI/combat/death run
→ observe corpse/death handoff
→ explicit dismiss/failure cleanup
→ Location.Discard
~~~

## Resolve exact identity first

Do not spawn from display names or "closest-looking" templates.

Know the exact `LocationTemplate` and, where relevant, `NpcTemplate`/attachments you expect.

## Spawn returning is not readiness

After `SpawnLocation`, check:

- Location is not discarded;
- expected template identity;
- expected `NpcElement`;
- actor is initialized/alive where relevant;
- visual exists when the feature needs it;
- expected AI/combat/death components are present.

## Make temporary ownership explicit

For disposable proof actors, set/verify `MarkedNotSaved = true` as early as practical.

That distinguishes a temporary actor from persistent world population.

It does not prove every save-neutrality edge case, so still validate the behavior you claim.

## Let native systems own behavior

If the accepted actor contract already includes native movement, combat, death, and corpse handling, keep those native systems in charge.

Do not recreate them in parallel just because the mod created the actor.

## Clean up by the actor you own

Use the exact `Location` reference/handle your mod created.

Do not search globally for a matching native ID and delete the "closest" or first match.

`Location.Discard()` is the recurring cleanup operation, but validation should still observe downstream Model/View cleanup.

## Temporary actor vs persistent population

These are different features.

A one-session companion proof does not establish:

- ambient spawn ownership;
- respawn;
- density management;
- leave/return behavior;
- persistent world state;
- save/load restoration.

Treat those as separate gates.

## Common mistakes

- prefab/model = full actor;
- spawn call returned = actor is fully ready;
- temporary `MarkedNotSaved` proof = persistent population;
- destroying visuals manually before native death/corpse handling finishes;
- treating a project resolver API as native game behavior;
- reusing unique/story/boss actors without ownership proof.

## How to verify a temporary actor

Check:

1. exact `LocationTemplate`;
2. deterministic safe placement;
3. `Location` created;
4. expected `NpcElement`/identity;
5. not-saved policy;
6. movement/targeting/combat if required;
7. damage/hit reaction;
8. native death state;
9. corpse/dummy handoff;
10. dismiss/cleanup;
11. duplicate prevention;
12. no unintended persistence.

Persistent population needs additional evidence for spawn ownership, density, respawn, leave/return, save/load, and orphan cleanup.

## Evidence limits

There is strong source-inspected evidence for temporary native `Location` actor lifecycles and substantial creature-specific research.

That does not make arbitrary persistent population, unique-actor reuse, or generic custom creature construction universally proven.
