# Kandra Armour and Clothing Boundary

Use this page when an armour/clothing mod has the correct item identity but the body/garment presentation is wrong.

Canonical overview: [Kandra](README.md).

## Three owners

Keep these distinct:

```text
logical item/equip owner
→ clothing/stitching/visibility owner
→ Kandra renderer/rig owner
```

A successful Kandra mesh registration does not make the item equippable, and a successful equip does not prove the garment's deformation/culling data is correct.

## What Kandra contributes

Kandra owns the deforming render representation:

- rig/bone registration;
- packed skinning data;
- blendshapes;
- triangle/index visibility;
- renderer/material state;
- cleanup/resource lifetime.

## Body culling matters

A garment can depend on triangle-visibility data to hide covered body geometry.

Therefore a custom mesh that visually fits in isolation may still fail in-game through:

- body clipping;
- missing hidden triangles;
- incorrect bind pose/weights;
- wrong rig/bone mapping;
- missing renderer lifecycle integration.

## Verification

For an armour presentation claim, test separately:

1. item/equip identity;
2. garment rig/bone mapping;
3. deformation in representative animations;
4. body triangle culling;
5. first/third-person consumers where relevant;
6. unequip/re-equip;
7. scene/load transition;
8. cleanup/resource release.

See the [native clothes/Kandra mechanic](../../../mechanics/armour/native-clothes-kandra.md) for the bounded public capability.
