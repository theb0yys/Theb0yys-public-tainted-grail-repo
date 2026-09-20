---
document_type: mechanic
scope: native clothes/armour equip presentation
runtime: mono
evidence:
  static: DECOMPILED_CONTRACT
  runtime: NOT_PROVEN_FOR_CUSTOM_ARMOUR
  persistence: NOT_PROVEN
last_verified: 2026-09-20
---

# Native Clothes and Kandra Equip Path

FoA clothes/armour presentation is owned by the native clothes lifecycle and Kandra stitching, not by a generic skinned-mesh replacement.

## Static ownership chain

Current decompiled evidence establishes this route:

```text
equipped cloth ARAssetReference
→ BaseClothes.EquipTask / EquipAfterLoaded
→ load cloth GameObject
→ ClothStitcher.Stitch(clothPrefab, KandraRig)
→ enumerate KandraRenderer components
→ KandraRenderer.RedirectToRig(...)
→ native Kandra presentation
```

On unequip, the native route destroys stitched cloth instances and releases the loaded prefab/resource ownership.

## Important detail

The inspected `ClothStitcher` route processes `KandraRenderer` components. A plain `SkinnedMeshRenderer` is not evidence that the asset will participate in FoA's native Kandra clothes path.

## Proof boundary

This is a **static native contract**. It does not prove:

- a custom armour package successfully equips at runtime;
- deformation is visually correct;
- every bone/rig mapping is compatible;
- save restoration works;
- broad cross-build compatibility.

Use [Kandra](../../systems/presentation/kandra/README.md) for the renderer owner.
