# Build an Owned Native Encounter

Use this guide when a framework or advanced mod needs to spawn temporary native actors while keeping strong ownership over exactly what it created.

This is the beginner-facing version of the proven TGE owned-encounter lifecycle.

Canonical platform contract: [TGE Owned Encounters](../../../platform/components/tainted-grail-extender/encounters.md).  
Working lineage: [TGE Owned Encounter Lifecycle](../../../research/case-studies/frameworks/tge-owned-encounter.md).

## Runnable source

Start from the runnable public example: [TGE owned encounter](../../../examples/hybrid/encounters/tge-owned-encounter/README.md). Build/run it unchanged first, confirm its documented result, then make one change at a time.

## What you will build

```text
choose reviewed allowlisted native template
→ preview/fingerprint request
→ wait for native loading readiness
→ validate placement
→ spawn one temporary actor
→ mark actor not saved
→ return one owned handle
→ observe initialized/live/visual state
→ remove exact owned actor
→ confirm model/view destruction
```

The core rule is simple:

> Never turn "I can spawn an actor" into "I can delete any actor."

## Step 1 — use an allowlisted exact template

Choose a reviewed template identity.

Do not accept:

- fuzzy display-name matches;
- arbitrary caller-provided GUIDs;
- named/story NPCs;
- unknown unique actors.

The first proof used an allowlisted Wyrdspirit.

## Step 2 — preview before execution

Before spawning, produce/validate a preview containing the important request facts:

- template identity;
- requested placement;
- build/session fingerprint;
- relevant constraints.

Execution should only proceed when the preview still describes the intended request.

## Step 3 — wait for native readiness

Do not spawn into an unready world.

Require the relevant native loading/world/player state before requesting actor creation.

If readiness cannot be established, return a blocked result rather than retrying blindly.

## Step 4 — verify placement

Use the reviewed placement rules for:

- player displacement;
- world validity;
- clearance;
- safe distance/range.

Do not use "near the player" as permission to place directly inside the hero, geometry or invalid navigation space.

## Step 5 — spawn through the native lifecycle

Let FoA create the native `Location`/NPC path.

Immediately apply the temporary posture required by this proof:

```text
Location.MarkedNotSaved = true
```

This prevents a disposable proof actor from silently becoming persistent content.

## Step 6 — return an owned handle

The caller should receive an opaque owned handle representing the actor created by this request.

Do not expose a broad "delete native actor by ID" API.

The handle should map only to the exact actor your service owns.

## Step 7 — observe both ends of creation

A successful spawn is stronger than a non-null return value.

Wait for the bounded accepted states:

- initialized Location/NPC;
- living actor;
- active visual.

If final outcome becomes unknown, do not automatically spawn another actor and risk duplicates.

## Step 8 — remove only the owned actor

Cleanup should resolve the owned handle back to the exact owned `Location`.

Then use the native discard lifecycle and wait for:

- owned Location discard;
- model discarded state;
- captured view/visual destruction.

Broad scene scans/deletes are not cleanup.

## Step 9 — support compositions only after one actor works

The accepted proof later expanded a saved **composition definition** into two Wyrdspirits and returned distinct owned handles.

That is the correct generalisation:

```text
composition definition
→ bounded per-entry native spawn
→ distinct owned handle per actor
→ individual lifecycle/cleanup
```

Do not skip the single-actor proof.

## Verification checklist

1. reviewed template identity accepted;
2. unapproved identity rejected;
3. preview/fingerprint matches execution;
4. native loading readiness observed;
5. placement checks pass;
6. actor spawns once;
7. actor is `MarkedNotSaved`;
8. initialized/live/visual state is observed;
9. one owned handle maps to one actor;
10. exact cleanup destroys model and visual;
11. duplicate/unknown-outcome retry does not create extra actors;
12. composition actors get distinct handles.

## Evidence boundary

**Proven:** bounded live spawn/cleanup of one allowlisted Wyrdspirit, followed by a two-Wyrdspirit composition expansion with distinct owned handles.

**Not claimed:** persistent population management, arbitrary templates, story NPC spawning, automatic retry after unknown outcomes or broad actor-deletion authority.
