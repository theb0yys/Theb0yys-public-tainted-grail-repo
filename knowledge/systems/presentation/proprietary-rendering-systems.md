# Rendering Systems

Use this page when a custom mesh, character, prop, vegetation object, or distant scene object **does not behave like an ordinary Unity Renderer**.

FoA uses several specialized rendering systems. The correct integration depends on what kind of object you are working with.

## Which renderer should you investigate?

- [Drake / MergedDrake](drake/README.md) — rigid meshes and ECS / Entities Graphics.
- [Kandra](kandra/README.md) — skinned/deforming characters and clothes.
- [Medusa](medusa/README.md) — static long-distance environment geometry.
- [Leshy](leshy/README.md) — vegetation streaming, rendering, and colliders.
- [HLOD](hlod/README.md) — distant hierarchical proxy content.
- [Shared mipmap streaming](mipmap-streaming/README.md) — texture/material mip demand used by several rendering systems.
- [Critter VAT / ECS](critter-vat/README.md) — VAT animation combined with Drake-style visuals.

## Do not assume the GameObject is the runtime owner

A Unity `Renderer` hierarchy can be only:

- authoring input;
- a temporary load object;
- a front-end representation that later feeds ECS or another runtime renderer.

The shipping/runtime representation may be owned by Drake, Kandra, Medusa, Leshy, HLOD, or another specialized system.

## Before changing rendering

Answer:

1. what kind of object is this?
2. which renderer owns it at runtime?
3. what resource/package data does that renderer expect?
4. who registers it?
5. who releases it?
6. does the object have a separate gameplay owner?
7. what happens on scene unload or equip/unequip?

## Practical rule

Change the renderer through the system that owns the runtime representation.

Do not make a visible GameObject or MeshRenderer the source of truth when the game has already moved ownership into another rendering system.
