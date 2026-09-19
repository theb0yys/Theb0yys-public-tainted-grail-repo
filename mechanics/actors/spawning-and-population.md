<!-- Canonical mechanic. Migrated from docs/reference/SPAWNING_ENCOUNTERS.md. -->
# Spawning, Encounters, and World Placement

> **Reference/process page.** Spawning an actor once is not the same problem as owning persistent world population.

## What this system is

FoA has multiple actor-entry paths with different ownership:

~~~text
controlled one-session Location spawn
native summon/ally spawn
native BaseLocationSpawner population
encounter/group spawn utilities
authored scene placement
persistent population/save-owned spawners
~~~

These must not be treated as interchangeable.

## Who owns it in FoA

Important owners include:

- `LocationTemplate`
- `Location`
- `NpcElement`
- `BaseLocationSpawner`
- native encounter/spawn utilities
- scene/domain ownership
- population/spawner save bookkeeping
- AI/movement/combat owners after actor creation

## Important identities, types, and methods

Researched surfaces include:

- `LocationTemplate.SpawnLocation(...)`
- `BaseLocationSpawner.VerifyPosition(...)`
- `LocationSpawnUtils.SpawnEnemiesAroundHero(...)`
- `Location.MarkedNotSaved`
- `Location.ID`
- `Location.Discard()`
- `NpcMovement.ChangeMainState(...)`
- `NpcAI.EnterCombatWith(...)`
- native spawner candidate/location-template lists
- spawned/killed ID bookkeeping
- cooldown/respawn ownership

## Where it exists in the lifecycle

### Controlled session-only spawn

A safe proof path is:

~~~text
exact LocationTemplate resolves
→ requested position selected
→ placement/clearance validated
→ SpawnLocation(...)
→ immediately MarkedNotSaved = true
→ only then track/read NpcElement/bind behavior
→ validate exact identity/components
→ runtime behavior
→ explicit Location.Discard cleanup
~~~

The ordering of `MarkedNotSaved` matters.

Working-repo incident history showed plugin-spawned NPC state can enter native restore paths when not excluded early enough.

### Native population/spawner path

A native `BaseLocationSpawner` owns far more:

- activation/range;
- candidate selection;
- position verification;
- spawned/killed IDs;
- save/restore;
- cooldown;
- cleanup;
- respawn.

Adding a candidate to a native spawner therefore transfers ownership to a very different persistent lifecycle.

## How we interact with it

### For a proof actor

Keep it session-only and disposable unless persistence is the exact research target.

### For population

Research and preserve the native spawner's ownership instead of simultaneously calling `SpawnLocation`, moving/discarding the actor, and editing its saved population row.

Choose one owner.

### For placement

Do not treat a coordinate or one `VerifyPosition` result as full placement proof.

Validate:

- grounding;
- clearance;
- nav/path reachability;
- nearby geometry;
- scene loaded state;
- actor scale/collider/controller;
- transition/return behavior.

## Why this route

Several creature/world experiments showed that "spawn succeeded" is an extremely weak proof.

An actor can spawn while:

- native initialization later fails;
- root/collider/renderer state is wrong;
- it enters the wrong save lifecycle;
- pathing is impossible;
- cleanup is undefined;
- population duplicates after reload;
- native spawner bookkeeping disagrees with plugin state.

The process separates those concerns.

## What goes wrong

### Spawned Location not marked not-saved immediately

Can leak a controlled proof actor into native restore/save assumptions.

### `VerifyPosition` treated as sufficient

Current research explicitly warns that this helper can catch exceptions and return the unverified input. Independent grounding/clearance/path evidence is still required.

### Direct spawn mixed with native population ownership

Two systems can believe they own lifecycle, cleanup or persistence.

### Encounter success generalized to persistent population

Volatile encounter state and native saved spawner population are different lanes.

### Scene/transient loading state ignored

Active encounters can cross transient scene/loading states; lifecycle needs explicit policy.

### Death observed but actor cleanup not owned

A death hook can resolve encounter state without owning corpse/location teardown.

## How to verify

For a controlled actor:

1. exact template identity;
2. scene/domain readiness;
3. placement;
4. `SpawnLocation`;
5. immediate save-exclusion;
6. complete native initialization;
7. AI/controller/renderer health;
8. behavior;
9. death/corpse where applicable;
10. explicit teardown;
11. no duplicate;
12. leave/return if relevant.

For persistent population add:

- native spawner candidate/owner proof;
- density/count/cooldown;
- save/load;
- killed-ID behavior;
- respawn;
- scene return;
- coexistence with native candidates/mods.

## Current proof boundary

**Strong bounded evidence:** controlled session-only `LocationTemplate.SpawnLocation` routes, immediate `MarkedNotSaved`, and explicit cleanup; multiple creature/encounter implementations use this pattern.

**Separate native ownership evidence:** `BaseLocationSpawner` owns saved population lifecycle.

**Not one generic API:** arbitrary spawn, encounter, companion and persistent-population work require different policies and validation.
