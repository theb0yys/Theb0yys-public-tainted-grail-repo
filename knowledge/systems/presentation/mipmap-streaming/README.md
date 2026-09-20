# Shared Mipmap Streaming

## What it is

Questline's shared mipmap system coordinates **texture mip demand** across multiple proprietary renderers.

It does not load textures by itself. Renderer-specific providers publish demand; the shared service reduces that demand to per-texture mip levels; Unity remains responsible for actual texture streaming/residency.

## Core flow

~~~text
renderer state
→ renderer-specific mip factor provider
→ MaterialId demand
→ material-to-texture expansion
→ TextureId demand reduction
→ Texture2D.requestedMipmapLevel
→ Unity texture streaming
~~~

## Main types

- `MipmapsStreamingMasterMaterials`
- `MipmapsStreamingMasterTextures`
- `IMipmapsFactorProvider`
- `MaterialId`
- `TextureId`
- `CameraData`

Shared runtime implementation lives primarily in `Awaken.Utility.dll`, with renderer adapters in their owning systems.

## Systems using it

The architecture connects multiple rendering families, including:

- Drake;
- Kandra;
- Medusa;
- Leshy;
- HLOD;
- other provider-based renderers such as static decals.

Each renderer can compute demand differently while sharing the material/texture registry.

## Why it exists

Without a common service, each renderer would need its own material-to-texture cache and texture-streaming policy.

The shared layer provides one place to coordinate texture demand while allowing very different rendering systems.

## Modding relevance

When changing materials or renderer ownership, remember that texture residency may be driven by this shared demand path rather than the visible renderer alone.

A material being registered is not the same as all texture mips being resident.

## Related systems

- [Drake](../drake/README.md)
- [Kandra](../kandra/README.md)
- [Medusa](../medusa/README.md)
- [Leshy](../leshy/README.md)
- [HLOD](../hlod/README.md)
