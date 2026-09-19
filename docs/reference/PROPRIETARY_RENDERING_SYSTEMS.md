# Questline Rendering and Proprietary Runtime Systems

> **Reference page.** Use this when modding character/equipment presentation, vegetation, static world geometry, renderer-heavy scenes, or anything where ordinary Unity renderer assumptions stop matching the shipping game.

## What this system is

Tainted Grail still uses Unity, but Questline layers several specialised systems on top of conventional Unity authoring.

The strongest supported high-level model is:

~~~text
Unity-friendly authoring
→ Questline conversion/baking
→ specialised persistent data
→ specialised runtime representation
→ specialised culling/draw preparation
→ shared texture/mipmap demand
→ Unity/HDRP/GPU
~~~

Different content classes use different systems.

## Who owns it in FoA

### Drake

General-purpose bridge from conventional Unity renderer concepts into ECS/Entities Graphics while preserving selected GameObject/OOP interaction paths.

A related MergedDrake path flattens many static renderer descriptions into bulk runtime data.

### Kandra

Specialised character/skinned-mesh rendering system.

Used for character/equipment rendering and mesh-to-mesh culling. The exact modern internal draw backend remains less completely recovered than Drake/Leshy/Medusa.

### Leshy

High-volume vegetation system.

Uses spatial/cell streaming and BatchRendererGroup-oriented rendering rather than ordinary per-instance GameObjects.

### Medusa

Specialised static-scene renderer for effectively immutable long-distance/static geometry.

Bakes ordinary Unity `LODGroup` / `MeshRenderer` authoring data into a separate scene dataset and uses scene-lifetime GPU-oriented representation.

### HLOD

Hierarchical LOD system that clusters/groups renderers into simplified distant representations using an ECS-oriented runtime.

### Mipmap streaming

Shared texture-demand layer used across multiple specialised renderer domains, including Drake, Leshy, Medusa, HLOD and Kandra.

Questline controls mip-demand policy while Unity still owns low-level texture/GPU resource handling.

### Scenes Baking

Build/preparation system that can merge/split/flatten scene content.

It is related to the wider bake architecture, but the research does **not** prove it is the master caller/orchestrator for every renderer-specific bake.

## Important identities, types, and methods

Examples of renderer/native concepts surfaced in the research:

- `DrakeMeshRenderer`
- `DrakeLodGroup`
- Drake renderer/resource managers
- `KandraRig`
- `KandraRenderer`
- `LeshyManager`
- Medusa renderer managers/persistence
- HLOD managers/data
- shared mipmap material/provider services
- `BatchRendererGroup`
- Unity Entities Graphics
- Addressables
- scene-related lifetime components

These systems should be treated as owner-specific runtime infrastructure, not as generic interchangeable renderers.

## Where it exists in the lifecycle

A common pattern is:

~~~text
ordinary Unity authoring source
→ editor/build conversion
→ generated/baked specialised data
→ scene or resource loader
→ subsystem manager
→ runtime representation
→ rendering/culling
→ subsystem cleanup / scene cleanup
~~~

Some systems retain GameObjects; others intentionally do not.

Some visual representations may outlive the logical gameplay object temporarily.

## How we interact with it

### Weapons

Do not assume a direct `MeshRenderer` replacement is equivalent to the native equipped-weapon presentation path.

Weapon research increasingly points toward native template/equip ownership plus Drake-compatible prototype/presentation integration.

### Armour

Treat Kandra/native clothes ownership as part of the equip lifecycle.

Loading a skinned mesh is not enough; stitching, rig ownership, renderer state and unequip teardown matter.

### Creatures/NPCs

Separate actor logic from visual ownership.

A valid `Location`/`NpcElement` can still have a broken renderer/rig path, and a valid visual prefab is not a native actor.

### World/static assets

Do not treat Medusa/Leshy/HLOD content as ordinary per-instance GameObject systems when their runtime ownership is data/manager-based.

## Why this route

Many failed or rejected modding approaches come from assuming:

> "If Unity can instantiate or render this object, then it is integrated the same way FoA's native content is."

That is often false.

The game deliberately replaces conventional Unity runtime representation for high-volume or specialised rendering categories.

## What goes wrong

### Direct renderer replacement

May bypass:

- native renderer/resource ownership;
- LOD state;
- ECS entities;
- retained resource keys/refcounts;
- scene lifetime;
- equip ownership;
- culling;
- mip streaming.

### Synthetic ECS creation without full owner graph

Creating the visible entity is not enough if the native system also owns:

- resource registration;
- scene/lifetime components;
- loading state;
- linked-object bookkeeping;
- cleanup components;
- material/mesh refcounts.

### Treating all proprietary archives as the same format

The same `.arch` extension appears in several shipping systems, but the research does not yet prove a single shared archive implementation.

### Assuming Kandra is BRG/Entities Graphics

Kandra is definitely a proprietary character renderer, but its exact draw submission/backend remains unresolved in the promoted evidence.

### Assuming Scenes Baking invokes every renderer baker

Plausible, but not proven.

## How to verify

For renderer/presentation work, prove:

1. native owner/subsystem;
2. source/baked representation;
3. resource/address identity;
4. runtime owner/manager;
5. instance/prototype creation;
6. visibility/culling state;
7. material/mesh resource readiness;
8. scene/equip lifetime;
9. unload/cleanup/refcount behavior;
10. runtime visual result.

Do not stop at "the mesh exists."

## Current proof boundary

High-level system roles for Drake, Leshy and Medusa are strongly supported by first-party/decompiled/shipping evidence.

Kandra and HLOD are real shipping systems with substantial evidence, but parts of their internal writer/runtime architecture remain less completely established.

The public handbook should use these systems to explain ownership and risk, not to claim unsupported custom writer/baker implementations.
