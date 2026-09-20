# Critter VAT / ECS Integration

## What it is

FoA's critter rendering path combines **TAO Vertex Animation Texture (VAT)** with Questline's ECS/Drake runtime.

The VAT package supplies the per-vertex animation encoding and shader/runtime animation state. Questline's proprietary integration connects that data to critter gameplay, ECS movement/control and Drake visual entities.

## High-level flow

~~~text
SkinnedMeshRenderer + animation clips
→ VAT bake
→ position/normal texture array + animation book
→ Drake-compatible visual prefab
→ critter ECS/controller state
→ per-entity VAT animation state
→ shader deformation
→ Drake / Entities Graphics rendering
~~~

## What Questline owns

Questline's integration owns:

- critter spawn/group/controller logic;
- gameplay/audio GameObject proxy;
- movement intent;
- damage/death/loot lifecycle;
- Drake visual-entity lifetime;
- animation state selection and handoff to VAT;
- game-specific culling and visibility relationships.

## Main runtime/authoring types

- `CritterSpawner`
- `Critter`
- Drake visual entity buffers
- TAO `VA_AnimatorParams`
- `VA_CurrentAnimationData`
- transition systems
- VAT shader material properties

## VAT data model

The baker records sampled vertex positions and compressed normals into a texture array. Source vertex identity is retained in mesh UV data so the shader can address the correct texel/frame.

Runtime animation systems advance the clip state and publish material properties used by the VAT shader.

## Why it exists

Small creatures can appear in large numbers. A VAT/ECS path avoids a full skinned-mesh/Animator cost for every visible critter while still allowing a separate gameplay object to own interaction, damage, audio and loot.

## Deeper reference

- [Critter gameplay vs VAT visual entity](gameplay-vs-visual-entity.md)

## Modding relevance

Use this system map when working with critter visuals or animation. Do not assume a normal humanoid/NPC Kandra path applies to VAT critters, and do not treat the VAT visual entity as the authoritative gameplay actor.

## Related systems

- [Drake](../drake/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
