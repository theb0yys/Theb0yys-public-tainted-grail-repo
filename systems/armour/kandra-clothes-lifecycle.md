# Native Armour / Kandra Clothes Lifecycle

Document type: **native system**.

This page explains the native clothing presentation lifecycle. It does not claim that a generic custom-armour importer is fully proven.

## Ownership model

```text
armour ItemTemplate / equip definition
→ BaseClothes
→ clothing asset load
→ target KandraRig
→ ClothStitcher
→ KandraRenderer redirected to target rig
→ active clothing presentation

unequip
→ native clothes cleanup
→ stitched clothing destroyed/released
→ asset reference released
```

## Native owners

- `ItemTemplate` — logical armour/equipment definition.
- `BaseClothes` — native clothing equip/presentation owner.
- `KandraRig` — target character rig.
- `ClothStitcher` — stitching/redirect path.
- `KandraRenderer` — clothing render representation.
- `KandraRendererManager` — renderer registration lifecycle.

## Key contract

A normal skinned Unity mesh is not automatically native clothing.

The native route expects Kandra-aware renderer/rig state that can be stitched or redirected into the target character rig.

## Lifecycle boundaries

### Source/deformation comes before equip

Geometry may be valid as source data while still being incompatible with the target deformation/rig contract.

### Renderer registration is not equip

A structurally valid or registered Kandra renderer is not yet an equipped armour item.

### Equip is not persistence

A visually correct current-session result still requires separate scene/reload/save/missing-mod proof where claimed.

### Unequip cleanup is part of correctness

Do not stop validation when the item first appears.

## Canonical mechanic

- [Custom armour integration](../../mechanics/armour/custom-armour-integration.md)

## Related systems

- [Items and Inventory](../items/README.md)
- [Native object ownership](../core/native-object-ownership.md)
- [Saving and persistence](../persistence/README.md)
- [Proprietary rendering](../../docs/reference/PROPRIETARY_RENDERING_SYSTEMS.md)

## Current proof boundary

The native `BaseClothes → ClothStitcher → KandraRig/KandraRenderer` ownership shape is strongly supported. General production custom-armour conversion, equip/deformation acceptance and persistence remain bounded by the mechanic page's stated evidence.
