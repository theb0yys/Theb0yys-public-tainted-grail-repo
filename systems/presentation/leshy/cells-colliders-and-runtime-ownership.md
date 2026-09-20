# Leshy Cells, Colliders, and Runtime Ownership

Use this page when vegetation behaves differently from ordinary Unity renderers or colliders.

Canonical overview: [Leshy](README.md).

## Separate visual residency from physical interaction

Leshy can own streamed vegetation cells while a separate near-player collider path supplies physical interaction.

That means:

```text
visual vegetation resident
≠
ordinary GameObject renderer exists
≠
collider is necessarily present
```

## Cell-based lifetime

The runtime works from scene-specific cell/catalog data rather than one permanently active GameObject per plant.

A mod should therefore avoid:

- caching one vegetation GameObject as the permanent owner;
- assuming a visually absent plant has been destroyed semantically;
- assuming collider lifetime equals visual-cell lifetime.

## Runtime investigation

When tracing a vegetation issue:

1. identify scene/cell;
2. determine whether the cell is resident;
3. identify the Leshy prefab/instance identity;
4. determine whether the issue is rendering, density, culling, or collider provisioning;
5. test entry/exit from the cell and near-player collider range.

## Authoring vs runtime

Vegetation Studio Pro/manual authoring is an input to the Leshy build path.

Runtime mods should target the shipping Leshy owner rather than assuming the authoring object graph survived intact.

See [Scenes Baking](../../world/scenes-baking/README.md).
