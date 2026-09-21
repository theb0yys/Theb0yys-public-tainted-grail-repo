# Weapon Pipeline Failure Catalogue

## Weapon is in inventory but cannot equip

Check ItemEquipSpec/attachment topology and the custom clone profile before debugging the mesh.

## Equip succeeds but weapon is invisible

Check representation selection, presentation address, Drake resource requests, prototype shape, and mesh/material serving. Do not replace the hand path with a manually parented MeshRenderer.

## Mesh is visible but attacks are wrong

Presentation succeeded; combat did not. Restore the native CharacterWeapon/combat profile and verify sweep/hit geometry and animation events.

## Weapon works in TPP but not FPP

Treat perspective representation as a separate stage. Verify the source archetype's FPP selection and the custom mapping for that consumer.

## Weapon works in world but preview is missing

Inventory/equipment preview is a separate presentation owner. Diagnose that route independently.

## Unequip leaks or stale models remain

Check native detach/discard plus framework/Drake resource release. Repeated equip/unequip is required evidence.

## Works until restart

Verify cold-start registration precedes saved identity resolution and presentation resources are available on load.

## One rigid sword works but another weapon family fails

Do not generalise across archetypes. Build a new source profile for the new family and re-run the pipeline.
