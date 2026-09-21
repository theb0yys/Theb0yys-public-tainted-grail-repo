# Armour Stage 7 — Runtime Visual and Equip Validation

## Objective

Prove the armour behaves correctly as worn clothing, not merely as a registered renderer.

## Validation set

Test and record independently:

- visible geometry under representative animation/motion;
- skin deformation at high-stress joints;
- material/shader correctness;
- body-cover / hidden-body behaviour where applicable;
- culling/LOD behaviour;
- target rig redirection;
- equip;
- unequip;
- re-equip;
- repeated cycles;
- scene/area transition;
- character/perspective situations relevant to the armour;
- cleanup/resource release.

## Procedure

1. Use the exact candidate that passed Stages 1–6.
2. Capture same-mesh comparisons where possible so conversion differences are visible.
3. Do not accept a neutral-pose screenshot as deformation proof.
4. Confirm repeated equip/unequip does not accumulate duplicate stitched renderers or leaked handles.
5. Confirm native owner state and visual state agree.

## Validation gate

Record each runtime dimension separately. A renderer being “visible” is only one row.

## Does not prove

Runtime visual/equip success does not prove save/load, missing-mod behaviour, migration, uninstall, or universal writer semantics.

## Next

Proceed to [persistence and release](../08-persistence-release/README.md).
