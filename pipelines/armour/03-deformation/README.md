# Armour Stage 3 — Deformation Compatibility

## Objective

Prove that the source geometry can deform acceptably on the intended target rig before treating package/registration work as success.

## Procedure

1. Apply the explicit mapping from Stage 2.
2. Generate the canonical/intermediate skinned representation used by the importer.
3. Exercise representative target poses, including high-stress joints.
4. Inspect:
   - collapsed vertices;
   - inverted/reversed triangles;
   - severe clipping;
   - detached regions;
   - incorrect root/bind orientation;
   - weight discontinuities;
   - unsupported blendshape/skinning assumptions.
5. Record numerical/structural checks where the importer provides them.
6. Preserve before/after evidence for the same geometry.

## Check

Confirm that deformation meets the target acceptance criteria for the intended armour piece and no critical rig/weight defect is hidden by a neutral pose.

## Failure rule

Do not proceed to Kandra runtime registration to “see if it looks better.” Fix the geometry/rig problem in this lane first.

## Before continuing

Correct deformation does not cover Kandra packing, runtime registration, native clothes stitching, equip, or persistence.

## Next

Proceed to [Kandra conversion and package build](../04-kandra-conversion/README.md).
