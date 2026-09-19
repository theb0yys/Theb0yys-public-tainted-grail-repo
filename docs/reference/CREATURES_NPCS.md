# Creatures and NPCs: Proven Injection Process

> **Reference/process page.** The working repository has a locked evidence-backed creature-injection sequence. Each gate proves only its own scope.

## What this system is

A custom creature is not a prefab with AI attached.

The complete problem spans:

~~~text
source assets
→ native baseline
→ visual transport
→ animation mapping
→ NpcTemplate / LocationTemplate contract
→ runtime actor construction
→ AI/combat/death/corpse lifecycle
→ placement/distribution
→ cleanup
→ persistence policy
~~~

The working process intentionally separates those stages.

## Who owns it in FoA

Important native/project ownership surfaces include:

- `NpcTemplate` — NPC gameplay definition;
- `LocationTemplate` — location/actor construction definition;
- `Location` — runtime actor/location owner;
- `NpcElement` — native NPC runtime element;
- native fighting-style/behavior/animation mappings;
- native AI/movement/combat owners;
- `DeathElement`, `NpcDummy`, `Corpse` — death/corpse lifecycle;
- native spawner/population systems — persistent/world distribution;
- Avalon Awakened in the working architecture — custom creature provider/identity/lifecycle owner.

Gameplay consumer mods should consume provider-owned creature identities rather than copying/forking them.

## Important identities, types, and methods

Important custom-creature definitions:

- pack-owned `NpcTemplate` GUID/name;
- pack-owned `LocationTemplate` GUID/name;
- exact native baseline;
- exact visual addresses;
- `ARStateToAnimationMapping`;
- exact attachment/component order;
- resolver/provider route;
- `LocationTemplate.SpawnLocation(...)`;
- `Location.MarkedNotSaved`;
- `Location.Discard()`;
- `NpcDummy` + `Corpse` death handoff.

Common proven animation-state IDs in the bounded creature lane include:

- `Idle (1)`
- `Movement (2)`
- `ShortRange (16)`
- `GetHit (32)`
- `Death (44)`

Additional states require their own evidence.

## Where it exists in the lifecycle

The locked base sequence is:

### CI1 — Source intake

Record:

- source package/path identity;
- SHA-256;
- licence/redistribution status;
- selected prefabs;
- mesh/skinned-mesh facts;
- skeleton/root;
- clips;
- materials/textures;
- audio;
- blockers.

**Proves:** source identity/suitability only.

### CI2 — Native baseline

Choose a native baseline by evidence, not visual resemblance.

Record:

- template family;
- faction;
- stats;
- abstract type;
- fighting style/manoeuvre;
- exact Location attachment order;
- collider/controller expectations;
- death/corpse policy.

**Proves:** which native contract the next proof is trying to preserve.

### CI3 — Visual transport

Build **pack-owned visual roots** through the validated FoA-compatible Addressables/ModService route.

Load, inspect, release, and repeat cleanly.

Record:

- catalogue/bundle;
- hashes;
- explicit roots/addresses;
- dependency closure;
- missing scripts;
- shader/renderer state;
- stable fingerprints.

**Proves visuals only.**

### CI4A — Animation mapping

Map source clips to exact native animation states.

Build/load/validate/release the mapping.

**Does not prove templates, actors, AI, combat or save behavior.**

### CI4 — Template and actor contract

Author pack-owned:

- `NpcTemplate`;
- `LocationTemplate`;
- explicit GUIDs;
- native-compatible attachment order;
- behavior/fighting-style references;
- animation coverage;
- scale policy;
- visual references.

Validate the isolated definitions and load/release behavior.

**Still does not prove runtime actor construction.**

### CI5 — Runtime lifecycle

Only after exact templates exist:

- resolve exact template identities;
- validate the actor before use;
- fail closed on missing components;
- mark proof/session actors not saved immediately;
- preserve native movement/target/attack/damage/death/corpse ownership where proven;
- own cleanup explicitly.

### CI5 Live — controlled behavior proof

On a disposable save/session prove, as applicable:

- visible identity/name;
- placement/clearance;
- native recognition;
- combat entry;
- attacks;
- damage;
- hit reaction;
- lethal transition;
- `Death(44)`;
- native `NpcDummy` + `Corpse` handoff;
- retained body policy;
- cleanup;
- duplicate prevention;
- save-exclusion readback.

## How we interact with it

The critical rule is: **do not skip a gate because a later system looks easy to call.**

A visual root cannot authorize actor construction.

A valid `NpcTemplate` cannot authorize world population.

A successfully spawned actor cannot authorize save persistence.

A companion consumer should resolve provider-owned identities instead of inventing a raw fallback template.

## Why this route

Creature work has repeatedly shown that partial success can be dangerously persuasive:

- the mesh loads;
- animations play;
- template serializes;
- actor appears;
- actor moves;

yet combat, death, root bones, colliders, save ownership, cleanup or population can still be wrong.

The gate sequence keeps each success from being used as proof of the next subsystem.

## Failure lessons from controlled creature gates

The creature process exists because partial successes repeatedly exposed missing native contracts.

### Headless graphics was not a valid visual proof

A `-nographics` validation attempt selected an unsupported/null graphics path.

**Lesson:** graphics-dependent Kandra/HDRP validation requires a real graphics-capable environment. A headless asset/load result cannot stand in for rendered acceptance.

### Generic HDRP/Lit was not the native Kandra skinning contract

A controlled actor became visible enough to reveal that ordinary `HDRP/Lit` did not implement the Kandra renderer's required skinning path.

**Lesson:** "mesh is visible" is not native character-renderer compatibility.

The next step was read-only baseline research into the real AnimalBear Kandra/material contract rather than another guessed shader substitution.

### Collider shape did not complete the controller contract

The first actor path reached `NpcController.IsGroundedInternal` with missing controller-grounding data.

Research recovered the exact baseline `CharacterGroundedData` contract and values before rebuilding.

**Lesson:** collider geometry is only one part of movement/controller ownership.

### Combat success did not prove locomotion or death

An early live actor could detect the hero, take damage and use both attacks, yet:

- locomotion dragged;
- death had no accepted fall animation;
- the corpse/body disappeared.

Research found three separate causes:

1. the animation map lacked `Idle(1)`, `Movement(2)`, `GetHit(32)`, and `Death(44)`;
2. the visual retained an inappropriate showcase controller/root-motion setup;
3. cleanup discarded the whole `Location` when the living `NpcElement` disappeared, deleting the native same-Location `NpcDummy` / `Corpse` handoff.

**Lesson:** attack proof is not locomotion/death proof, and cleanup must respect native death ownership.

### A plausible "fists" guard was wrong

A later defense gate matched the exact baseline item GUID and block geometry but rejected the actor because an assumed fists classification was false.

Read-only native research showed the native block owner did not require that classification.

**Lesson:** validate the actual consumer contract. A plausible safety guard can be more wrong than the native owner.

### One-session actor proof is deliberately not population proof

The controlled actor used immediate save exclusion and explicit cleanup.

**Lesson:** a safe disposable `Location` proof should not be promoted into persistent ambient-spawn authority.

## What goes wrong

### Visual success mistaken for actor success

The most common category error.

### Choosing a native baseline by resemblance

A similar-looking creature can have incompatible controller, fighting-style, collider, death or component-order expectations.

### Missing root/bone/renderer contract

Can allow package/registration success while runtime actor initialization fails.

### Raw fallback templates

Rejected because they fork identity and bypass provider ownership.

### Random/world population too early

A controlled one-session actor proof does not establish native spawner ownership, density, respawn, persistence or save lifecycle.

### Save-visible proof actors

Avoid until persistence is deliberately proven. Use `MarkedNotSaved` for controlled session-only proofs.

### Cleanup omitted

Spawn success is incomplete without death/dismiss/failure/scene/shutdown cleanup.

## How to verify

Keep separate receipts for:

- source;
- visual transport;
- animation mapping;
- template contract;
- resolver/provider;
- actor construction;
- combat/death;
- placement/distribution;
- cleanup;
- persistence.

Every receipt should state what it **cannot** be used to claim.

## Current proof boundary

The working repository has strong, repeated evidence for this **process shape**, with several custom creatures progressing through different subsets of CI1–CI5 and live correction gates.

Do not infer that every creature has passed every gate.

The process is reusable; individual creature readiness remains per-creature/per-gate.
