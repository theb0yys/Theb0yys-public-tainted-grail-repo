# Proprietary Rendering Systems

FoA does not use one rendering path for every object.

Open the dedicated system page for the owner you are dealing with:

- [Drake / MergedDrake](../systems/drake/README.md) — rigid meshes and ECS/Entities Graphics.
- [Kandra](../systems/kandra/README.md) — skinned/deforming characters and clothes.
- [Medusa](../systems/medusa/README.md) — static long-distance environment geometry.
- [Leshy](../systems/leshy/README.md) — vegetation streaming/rendering/colliders.
- [HLOD](../systems/hlod/README.md) — distant hierarchical proxy content.
- [Shared mipmap streaming](../systems/mipmap-streaming/README.md) — material/texture mip demand shared by several renderers.
- [Critter VAT / ECS](../systems/critter-vat/README.md) — VAT animation combined with Drake visuals.

## Rule

Identify the presentation owner before changing a renderer.

A normal Unity `Renderer` hierarchy may be only the authoring input; the shipping runtime representation can belong to one of these specialised systems.
