# Kandra

## What it is

**Kandra** is Questline's proprietary skinned/deforming character-rendering stack.

It replaces a substantial part of Unity's normal `SkinnedMeshRenderer` deformation/rendering path while **retaining Unity Animator, Avatar and Transform skeleton ownership**.

A useful mental model is:

~~~text
Animator / Avatar
→ Transform skeleton
→ Kandra rig and bone preparation
→ packed skinned mesh + weights + blendshapes
→ GPU-oriented deformation buffers
→ Kandra visibility / triangle culling
→ specialised rendering
→ Unity / HDRP
~~~

## What it owns

Kandra owns:

- packed skinned-mesh runtime data;
- rig/bone registration;
- skinning buffers;
- blendshape data and weights;
- renderer slots;
- triangle/index visibility;
- clothing/body triangle culling;
- Kandra material/render integration;
- renderer cleanup and resource lifetime.

It does not replace the Animator or own the logical equipment item.

## Main runtime types

- `KandraRig`
- `KandraRenderer`
- `KandraMesh`
- `KandraRendererManager`
- `RigManager`
- `BonesManager`
- `MeshManager`
- `BlendshapesManager`
- `SkinningManager`

Primary managed assembly: `Awaken.Kandra.dll`.

## Packed deformation model

Kandra mesh data includes separate logical channels for:

- compressed vertex position/normal/tangent data;
- extra UV/tangent information;
- packed bone indices and weights;
- bind poses;
- blendshape data;
- triangle/index data.

The renderer then prepares bone/deformation data for GPU-oriented skinning rather than relying on one Unity `SkinnedMeshRenderer` per character part.

## Clothing and body culling

FoA clothing can hide body triangles underneath garments.

The Kandra toolchain builds garment/body relationships into triangle-visibility information. Runtime rendering can therefore suppress covered body triangles rather than simply relying on alpha materials or deleting body meshes.

This matters for armour mods: a visible skinned mesh is only one piece of the clothing lifecycle.

## Modding relevance

Kandra knowledge is essential for:

- armour and clothing;
- character body parts;
- hair or deforming character attachments;
- custom creature/character visuals that use Kandra;
- equipment presentation involving a deforming mesh.

For armour, keep three owners separate:

~~~text
logical item/equip
→ native clothing/stitching owner
→ Kandra rendering owner
~~~

## Related systems

- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Scenes Baking](../../world/scenes-baking/README.md)
