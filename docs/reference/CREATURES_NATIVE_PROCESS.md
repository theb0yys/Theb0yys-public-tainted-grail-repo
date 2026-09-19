# Creatures and NPCs: Proven Process, Native Contracts, and Failure Lessons

> **Reference/process page.** This is the public reasoning model derived from the maintainer's controlled creature-integration research. It separates each proof gate so a visual success is never mistaken for a working native actor.

## What this system is

A genuinely new creature is a multi-system integration problem.

The proven research process is intentionally staged:

~~~text
CI1  source intake
→ CI2 native baseline
→ CI3 visual transport
→ CI4A animation mapping
→ CI4 template / actor contract
→ CI5 runtime lifecycle
→ live behavior / death / cleanup proof
~~~

The important lesson is that **each gate proves only its own layer**.

## Who owns it in FoA

A native creature can involve:

- `LocationTemplate`;
- `Location`;
- `NpcTemplate`;
- `NpcElement`;
- fighting style / combat behaviors;
- `ARStateToAnimationMapping`;
- native movement/controller;
- health/damage/death;
- `NpcDummy` / `Corpse`;
- faction/targeting;
- inventory/loot;
- Kandra/Animator/presentation;
- spawn/population owner;
- save/persistence owner.

No single prefab owns all of those responsibilities.

## Important identities, types, and methods

### Native state mapping

A proven hostile-creature mapping used exact `NpcStateType` states including:

- `Idle (1)`
- `Movement (2)`
- `ShortRange (16)`
- `GetHit (32)`
- `Death (44)`

A later defense experiment added exact block states only after a native-compatible defense contract was researched.

### Runtime actor lifecycle

Research uses native actor ownership:

- exact `LocationTemplate` resolution;
- `LocationTemplate.SpawnLocation(...)`;
- `Location`;
- `NpcElement`;
- explicit `MarkedNotSaved` for disposable proof actors;
- native death handoff;
- `Location.Discard()` for cleanup.

### Placement

Controlled proofs add:

- exact distance/anchor rules;
- native position verification;
- ground/controller validation;
- nearby walkable navigation node;
- path connectivity;
- duplicate/session caps.

## Where it exists in the lifecycle

### CI1 — Source intake

Record:

- asset/source package;
- provenance/licence;
- file hashes;
- meshes/skeleton/root;
- animations;
- materials/textures;
- audio;
- blockers.

**Why:** before runtime engineering, know exactly what you own and may redistribute.

**Does not prove:** FoA compatibility.

### CI2 — Native baseline

Choose a behaviorally similar native creature by evidence.

Record:

- template family;
- faction;
- stats;
- fighting style;
- component/attachment order;
- controller/collider expectations;
- death/corpse policy.

**Why:** a native baseline exposes the full contract you need to preserve or intentionally replace.

**What goes wrong:** choosing only by visual similarity can inherit the wrong movement/combat/death assumptions.

### CI3 — Visual transport

Build/load/release only the mod-owned visual through the chosen FoA-compatible asset transport.

Prove:

- catalogue/bundle exists;
- exact visual address resolves;
- dependencies resolve;
- no missing scripts;
- shaders/renderers are valid;
- release works;
- repeat clean load/release cycles;
- record stable fingerprints/hashes.

**Critical boundary:** **CI3 proves a visual can travel. It does not prove a creature exists.**

### CI4A — Animation mapping

Sample/inspect the exact source animations.

Author only the states actually required by the selected native behavior contract.

For the proven hostile slice, the first attack proof started with `ShortRange(16)`, then later mapping gaps forced the addition of Idle/Movement/GetHit/Death.

**Why:** native behavior asks for semantic states, not merely clips.

**What goes wrong:** a creature can attack yet drag during locomotion or disappear incorrectly on death because the mapping is incomplete.

### CI4 — Template and actor contract

Create/validate pack-owned definitions for:

- `NpcTemplate`;
- `LocationTemplate`;
- exact custom GUIDs;
- native-compatible component/attachment ordering;
- fighting style;
- behavior mapping;
- animation coverage;
- scale;
- visual reference;
- controller/collider/grounding contract;
- explicit loot/faction/stat policy.

Build/load/release those exact definitions before live spawning.

**Critical boundary:** template load success still does not prove runtime AI/combat/death.

### CI5 — Runtime lifecycle

Only after the exact definitions are accepted:

1. resolve the exact custom `LocationTemplate`;
2. validate identity;
3. spawn through the native Location path;
4. immediately establish disposable not-saved policy for proof actors;
5. validate expected components/owners;
6. let native movement/targeting/combat/damage/death own their accepted responsibilities;
7. observe native corpse/death handoff;
8. clean up through native Location ownership.

### CI5 live proof

A meaningful live gate checks:

- visible correct identity/name;
- valid placement;
- native recognition;
- movement;
- combat entry;
- both/all expected attacks;
- damage;
- hit reaction;
- death state;
- `NpcDummy` / `Corpse` handoff;
- retained body when intended;
- cleanup;
- duplicate prevention;
- not-saved readback.

## Why this route

The creature programme contains several important failures that changed the process.

### Failure — headless graphics proof

A `-nographics` visual attempt selected an unsupported/null graphics path.

**Lesson:** graphics-dependent validation needs a real graphics-capable environment.

### Failure — generic HDRP material on Kandra path

The actor rendered enough to expose that ordinary `HDRP/Lit` did not satisfy the Kandra renderer's required skinning path.

**Lesson:** "the mesh is visible" is not proof of native character-renderer compatibility.

### Failure — missing grounded controller data

Runtime construction reached a controller grounding dereference with missing data.

Research then recovered the exact native grounded-data contract and values from the baseline.

**Lesson:** collider shape is not the whole movement/controller contract.

### Failure — movement dragging / no death animation / corpse disappearing

The first live actor could detect, attack and take damage, but:

- locomotion dragged;
- death animation was missing;
- retained corpse disappeared.

Research found:

- missing Idle/Movement/GetHit/Death state mapping;
- an inappropriate showcase controller/root-motion setup;
- cleanup was discarding the whole `Location` as soon as the living `NpcElement` disappeared, deleting the native corpse transition.

**Lesson:** combat success did not prove locomotion or death lifecycle.

The correction added the missing state contract and changed cleanup to respect the native same-Location `NpcDummy` / `Corpse` handoff.

### Failure — invalid block/fists assumption

A defense gate matched the intended native item GUID/angle but rejected it because an assumed fists classification was false.

Read-only research showed the native block owner did not require that global classification.

**Lesson:** a plausible classification guard can be more wrong than the native owner. Guard on the actual contract.

## What goes wrong

### Visual prefab = creature

False.

### Animation clip = native state

False. State mapping and behavior contract matter.

### One working attack = full combat

Does not prove movement, defense, hit reaction, death or corpse.

### Actor spawn = population system

A controlled `MarkedNotSaved` actor is deliberately not persistent population.

### Cleanup on NpcElement disappearance

Can destroy native death/corpse handoff.

### Native baseline copied wholesale without explicit policy

Stats, faction, loot, uniqueness and story ownership must be reviewed separately.

### Custom AI replacement before native actor contract is understood

Creates overlapping owners for movement/combat/animation/death.

## How to verify

A full creature process should retain evidence at each gate:

### Source
- provenance;
- hashes;
- asset inventory.

### Visual
- exact load;
- renderer/shader;
- no missing dependencies;
- repeated release.

### Animation
- state-to-clip mapping;
- event timing;
- locomotion/root motion;
- hit/death.

### Definition
- custom GUIDs;
- template relationships;
- fighting style;
- controller/grounding;
- collider/hitboxes;
- faction/stats/loot policy.

### Runtime
- native Location/NpcElement;
- placement/nav/path;
- AI/combat;
- damage/hit;
- death/corpse;
- cleanup;
- duplicate/session rules;
- persistence policy.

## Current proof boundary

The maintainer's creature research contains strong staged evidence and user-observed/live results for specific controlled custom creatures.

This page publishes the **general process and failure lessons**, not a claim that every creature profile, rig, AI family, spawn system, loot policy or persistent population route is automatically reusable.

Persistent world population remains a separate gate beyond one-session actor integration.
