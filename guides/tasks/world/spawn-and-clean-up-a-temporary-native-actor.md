# Spawn and Clean Up a Temporary Native Actor

Use this guide when you need a **session-only native actor** and want to prove cleanup just as carefully as creation.

Working lineage: [Temporary Native Actor: Prove Cleanup, Not Only Spawn](../../../research/case-studies/encounters/temporary-native-actor.md).  
Canonical creature lifecycle: [Creatures and NPCs](../../../knowledge/systems/gameplay/creatures-npcs.md).

## Runnable source

Start from the runnable public example: [Temporary native actor](../../../examples/hybrid/encounters/temporary-native-actor/README.md). Build/run it unchanged first, confirm its documented result, then make one change at a time.

## What you will build

```text
reviewed native LocationTemplate
→ native SpawnLocation
→ immediately MarkedNotSaved
→ wait for initialized NPC
→ wait for living actor + active visual
→ use actor for bounded feature
→ discard exact owned Location
→ confirm model and visual are gone
```

## Step 1 — choose a safe template

Use one reviewed, non-unique template appropriate for disposable runtime spawning.

Avoid:

- named/story NPCs;
- quest-critical actors;
- unique locations;
- templates whose runtime component contract you have not inspected.

## Step 2 — wait for world readiness

Require the world/player/template systems needed for `SpawnLocation`.

Do not convert "service not ready" into repeated spawn attempts.

## Step 3 — spawn through the native owner

Use the native `LocationTemplate.SpawnLocation(...)` route or the reviewed wrapper around it.

The native Location/NPC lifecycle should remain authoritative.

## Step 4 — mark it not saved immediately

For a temporary actor proof:

```csharp
location.MarkedNotSaved = true;
```

Do this immediately after obtaining the owned Location.

Do not wait until shutdown and hope the actor never enters save state.

## Step 5 — prove initialization

Do not declare success at "spawn returned something".

Wait for the exact runtime milestones required by your feature:

- Location initialized;
- valid `NpcElement`;
- actor alive;
- active visual/view.

If one stage fails, report that stage.

## Step 6 — track exact ownership

Store your exact owned Location/actor identity.

You should be able to answer:

> Which actor did this feature create?

without scanning by name and guessing.

## Step 7 — clean up through native discard

When the feature ends:

```text
owned Location
→ Location.Discard()
→ model reaches discarded state
→ captured view/visual is destroyed
```

A hidden GameObject is not cleanup proof.

## Step 8 — handle failure paths

Cleanup should also run when:

- initialization fails after partial creation;
- feature is cancelled;
- scene changes;
- plug-in disables;
- validation rejects the actor after spawn.

Only clean what your feature owns.

## Verification checklist

1. exact reviewed template resolves;
2. one spawn request creates one Location;
3. `MarkedNotSaved` reads back true;
4. valid NPC element appears;
5. actor reaches living state;
6. visual becomes active;
7. feature tracks the exact owned identity;
8. cleanup calls native discard;
9. model reports discarded;
10. captured view/visual is destroyed;
11. no second actor remains after cleanup.

## Evidence boundary

**Proven:** controlled temporary native actor lifecycle from readiness through initialized/live/visual state to exact owned Location/model/view cleanup.

**Not claimed:** persistence, ambient population, arbitrary actor classes or custom creature authoring.
