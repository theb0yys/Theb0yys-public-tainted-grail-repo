# Kandra

Use this page when you are working with **skinned/deforming character meshes, armour/clothing, hair, body parts, or other visuals that follow a character rig**.

## What Kandra does

Kandra is Questline's specialized skinned/deformation renderer.

It replaces a substantial part of Unity's normal `SkinnedMeshRenderer` runtime path while still relying on Unity's Animator, Avatar, and Transform skeleton.

A useful model is:

~~~text
Animator / Avatar
→ Transform skeleton
→ Kandra rig/bone preparation
→ packed mesh / weights / blendshapes
→ GPU deformation buffers
→ triangle visibility / culling
→ Kandra rendering
→ Unity / HDRP
~~~

## What Kandra owns

Kandra owns:

- packed skinned-mesh data;
- rig/bone registration;
- skinning buffers;
- blendshape data;
- renderer slots;
- triangle/index visibility;
- clothing/body triangle culling;
- Kandra material/render integration;
- renderer/resource cleanup.

It does **not** replace the Animator and it does not own the logical equipment item.

For armour/clothing, keep these layers separate:

~~~text
logical Item/equip
→ native clothing/stitching
→ Kandra rendering
~~~

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

## Packed mesh/deformation data

Kandra's mesh data includes separate channels for:

- compressed position/normal/tangent data;
- extra UV/tangent data;
- bone indices and weights;
- bind poses;
- blendshapes;
- triangle/index data.

The runtime prepares this data for GPU-oriented skinning rather than relying on one normal Unity `SkinnedMeshRenderer` per character part.

## Clothing and body culling

FoA clothing can hide body triangles underneath garments.

The Kandra pipeline builds garment/body visibility relationships into triangle-culling information.

That means armour integration is more than "attach a skinned mesh":

- the rig must match;
- the renderer must register;
- deformation must work;
- covered body triangles may need culling;
- cleanup must release the Kandra resources.

## When this page is useful

Use Kandra knowledge for:

- armour and clothing;
- Hero/NPC body parts;
- hair/deforming attachments;
- Kandra-backed creature visuals;
- equipment presentation using deforming meshes.

## Common mistakes

- treating a Unity `SkinnedMeshRenderer` as the final native representation;
- assuming successful Kandra registration proves correct deformation;
- forgetting that the logical item/equip owner is separate;
- ignoring clothing/body triangle culling;
- destroying a visible GameObject without releasing Kandra-owned resources.

## What to verify

Check:

1. target rig identity;
2. bone/bind-pose compatibility;
3. packed mesh data;
4. renderer registration;
5. visual deformation;
6. blendshapes if used;
7. material/triangle visibility;
8. clothing/body culling;
9. native equip/stitch lifecycle if wearable;
10. renderer/resource cleanup.

## Related pages

- [Armour and native clothing](../../gameplay/armour.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
