# Armour Stage 1 — Source Intake and Provenance

## Objective

Create a reproducible, redistribution-safe record of the source armour asset before conversion begins.

## Record

- source package/path identity;
- source hash where applicable;
- licence/redistribution status;
- selected mesh objects;
- bind pose / skeleton information;
- bone names and hierarchy;
- skin weights;
- materials/textures;
- blendshapes if present;
- scale/orientation conventions;
- known missing dependencies;
- authoring-tool and export version when relevant.

## Procedure

1. Identify the exact source asset revision.
2. Confirm the author has the right to process and publish the resulting public material.
3. Capture deterministic geometry/skeleton metadata.
4. Separate source evidence from any target-game assumptions.
5. Do not copy proprietary game assets into the public pipeline as a convenience.

## Output

A source-intake record and immutable input set for later comparison.

## Check

Confirm that another author can identify the same authorised source revision and reproduce the recorded geometry/skeleton facts.

## Before continuing

Source suitability does not cover target-rig compatibility, deformation, Kandra conversion, registration, equip, or persistence.

## Next

Proceed to [geometry and target-rig contract](../02-geometry-rig/README.md).
