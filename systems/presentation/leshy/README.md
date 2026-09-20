# Leshy

## What it is

**Leshy** is Questline's baked and streamed vegetation runtime.

Vegetation Studio Pro is used for authoring/procedural placement and initial vegetation setup. Questline then converts eligible vegetation into Leshy's own cells and runtime representation.

## What it owns

Leshy owns:

- vegetation cells;
- scene-specific streamed vegetation data;
- cell loading/residency;
- density filtering;
- compact instance representation;
- GPU upload/expansion;
- `BatchRendererGroup` vegetation rendering;
- camera/light culling;
- nearby collider provisioning;
- mipmap-demand participation.

## Main runtime types

Current runtime types live under `Awaken.TG.LeshyRenderer`, including:

- `LeshyManager`
- `LeshyCells`
- `LeshyLoadingManager`
- `LeshyRendering`
- `LeshyPersistence`
- `LeshyPrefabs`
- `LeshyCollidersManager`
- camera/light culling components

The main game host is `TG.Main.dll`.

## Data layout

Current Windows content uses scene-specific loose files:

~~~text
StreamingAssets/Leshy/<scene>/CellsCatalog.leshy
StreamingAssets/Leshy/<scene>/Matrices.bin
~~~

The runtime reads those files into cell/instance state.

## Runtime shape

~~~text
VSP/manual vegetation authoring
→ Leshy build conversion
→ CellsCatalog.leshy + Matrices.bin
→ LeshyManager
→ visible/resident cells
→ compact instance data
→ GPU expansion
→ BatchRendererGroup
→ camera/light rendering
~~~

A separate near-player collider path provides physical vegetation interaction where required.

## Why it exists

Open-world vegetation can involve enormous instance counts. Leshy avoids treating every plant/tree as a permanently active ordinary Unity renderer/GameObject.

## Deeper reference

- [Leshy cells, colliders, and runtime ownership](cells-colliders-and-runtime-ownership.md)

## Modding relevance

Use Leshy knowledge when a vegetation change appears to ignore ordinary Unity renderer manipulation or when the visual/collider representation changes with distance and cell residency.

Vegetation art/placement and Leshy runtime ownership are different stages.

## Related systems

- [Scenes Baking](../../world/scenes-baking/README.md)
- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
