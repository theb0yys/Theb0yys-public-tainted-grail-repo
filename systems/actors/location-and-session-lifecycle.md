<!-- Canonical system page. Migrated from docs/reference/ACTORS_LOCATIONS_SPAWNING.md. -->
# Actors, Locations, Spawning, and Session Ownership

> **Reference page.** Use this before creating, spawning, dismissing, or persisting an NPC/creature/companion.

## What this system is

In FoA, an actor is not simply an instantiated character prefab.

The researched runtime path commonly involves:

~~~text
template identity
→ LocationTemplate
→ Location
→ NpcElement / actor Elements
→ AI/combat/movement/presentation owners
→ death/corpse handoff
→ cleanup
→ persistence policy
~~~

## Who owns it in FoA

Important owners/surfaces include:

- `LocationTemplate` — reusable location/actor-spawn definition;
- `Location` — runtime world/MVC owner;
- `NpcElement` — NPC runtime Element;
- death/corpse Elements such as `DeathElement`, `NpcDummy`, `Corpse`;
- native spawners/population systems for persistent/ambient population;
- project-owned resolver APIs where a mod/framework adds an abstraction layer.

## Important identities, types, and methods

Source-inspected paths include:

- `new TemplateReference(guid).Get<LocationTemplate>()`;
- `LocationTemplate.SpawnLocation(position, rotation)`;
- `Location.MarkedNotSaved = true`;
- `Location.Discard()`;
- `NpcElement.IsAlive`;
- `NpcElement.KeepCorpseAfterDeath`;
- validation of expected `NpcDummy` + `Corpse` handoff.

## Where it exists in the lifecycle

A bounded one-session actor path is:

~~~text
exact template resolves
→ placement chosen
→ SpawnLocation
→ Location/NpcElement initialization
→ validate exact identity/components
→ immediately establish not-saved policy
→ native AI/combat/death owns behavior where proven
→ native death/corpse handoff
→ explicit dismiss/failure cleanup
→ Location.Discard
~~~

This is different from authored ambient population or persistent spawners.

## How we interact with it

### Resolve exact actor/template first

Do not spawn from a display name or "closest looking" template.

### Validate the constructed actor

A successful `SpawnLocation` call is not enough.

Check:

- exact LocationTemplate;
- expected NpcTemplate/attachment;
- expected runtime Elements/components;
- presentation;
- persistence policy;
- required AI/combat/death owner.

### Make session-only intent explicit

For disposable proof actors, set/verify not-saved state immediately.

### Let native owners own behavior

If native movement/combat/death/corpse systems are already part of the accepted actor contract, do not recreate them in parallel.

### Discard through native ownership

Use `Location.Discard()` for owned session actors and clear mod-local state/resources.

## Why this route

The creature research repeatedly separates:

- visual transport;
- template identity;
- actor construction;
- AI/combat/death;
- world population;
- persistence.

A visual prefab can load perfectly while no native actor exists.

A one-session companion can work while still being unsuitable as a persistent population/spawner process.

## What goes wrong

### Prefab = actor assumption

A model/Animator/Kandra object is presentation, not the full native actor lifecycle.

### Spawn call = valid actor assumption

The returned Location can still be the wrong identity or missing required runtime components.

### Session spawn generalized to persistent population

`MarkedNotSaved` is deliberately the opposite of a durable population claim.

### Manual death/cleanup fighting native handoff

Destroying visuals/objects early can bypass native corpse/death ownership.

### Project resolver mistaken for native FoA API

A shared project API can be a useful abstraction without becoming a native game contract.

## How to verify

For a one-session actor proof, validate:

1. exact `LocationTemplate` resolution;
2. deterministic placement;
3. Location created;
4. expected `NpcElement`/identity;
5. not-saved policy;
6. movement/targeting/combat as applicable;
7. damage/hit reaction;
8. native death state;
9. expected corpse/dummy handoff;
10. cleanup/dismiss;
11. duplicate prevention;
12. no unintended persistence.

For persistent population, add separate spawn-owner, density, respawn, leave/return, save/load and orphan/cleanup evidence.

## Current proof boundary

The working corpus contains strong source-inspected one-session Location lifecycle patterns and extensive creature-specific research.

That does **not** mean arbitrary persistent population, ambient spawning, unique actor reuse, or custom creature construction is universally proven.
