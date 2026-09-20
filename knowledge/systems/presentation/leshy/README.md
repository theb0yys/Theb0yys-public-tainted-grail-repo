# Leshy

Use this page when you are changing **vegetation that is streamed/cell-based and does not behave like a collection of ordinary always-active GameObjects**.

## What Leshy does

Leshy is Questline's baked and streamed vegetation runtime.

Vegetation Studio Pro can be used for authoring/procedural placement, then eligible vegetation is converted into Leshy's own cells and runtime data.

## What Leshy owns

Leshy owns:

- vegetation cells;
- scene-specific streamed vegetation data;
- cell residency/loading;
- density filtering;
- compact instance data;
- GPU upload/expansion;
- `BatchRendererGroup` rendering;
- camera/light culling;
- nearby collider provisioning;
- mipmap-demand participation.

## Main runtime types

Types under `Awaken.TG.LeshyRenderer` include:

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

Current Windows content uses scene-specific files such as:

~~~text
StreamingAssets/Leshy/<scene>/CellsCatalog.leshy
StreamingAssets/Leshy/<scene>/Matrices.bin
~~~

The runtime turns those files into resident cell/instance state.

## Runtime flow

~~~text
vegetation authoring
→ Leshy build conversion
→ CellsCatalog.leshy + Matrices.bin
→ LeshyManager
→ visible/resident cells
→ compact instance data
→ GPU expansion
→ BatchRendererGroup
→ camera/light rendering
~~~

A separate near-player collider path provides physical interaction where required.

## Why Leshy exists

Open-world vegetation can involve extremely large instance counts.

Leshy avoids keeping every plant/tree as a permanently active ordinary GameObject/Renderer.

## When this page is useful

Use Leshy knowledge when:

- ordinary renderer changes do not affect vegetation;
- vegetation appears/disappears with cell residency/distance;
- visual and collider behavior differ;
- you are investigating Leshy data or scene conversion.

## Common mistakes

- treating authored vegetation objects as the permanent runtime representation;
- assuming visible vegetation and nearby colliders have the same lifetime;
- changing one cell/file without understanding scene-specific catalog relationships;
- treating Leshy as the owner of unrelated gameplay/world state.

## What to verify

Check:

1. scene/Leshy data identity;
2. cell catalogue/matrix data;
3. cell load/residency;
4. instance expansion;
5. camera/light culling;
6. collider provisioning near the player;
7. material/mipmap behavior;
8. scene unload/cleanup.

## Related pages

- [Shared mipmap streaming](../mipmap-streaming/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
