# Armour Stage 2 — Geometry, Skeleton, and Target-Rig Contract

## Objective

Define the source-to-target compatibility problem before generating Kandra data.

## Procedure

1. Record the target body/rig identity and build scope.
2. Capture the target skeleton/bone contract needed by the intended native clothing owner.
3. Compare:
   - source bone set;
   - target bone set;
   - hierarchy relationships;
   - bind poses;
   - vertex weights;
   - scale/orientation;
   - submesh/material layout.
4. Define explicit bone mapping and any allowed remapping rules.
5. Detect unsupported or missing influences before conversion.
6. Record whether topology or weighting must be edited before the deformation gate.

## Output

A source contract, target contract, and deterministic mapping/compatibility report.

## Check

Confirm that the mapping can account for required source influences and target ownership without silently dropping or inventing critical rig data.

## Before continuing

A valid mapping contract does not cover the mesh deforms correctly.

## Next

Proceed to [deformation compatibility](../03-deformation/README.md).
