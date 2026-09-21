# Creature CI2 — Native Baseline

## Objective

Choose the FoA actor family whose native contract the imported creature will preserve or deliberately adapt.

## Rule

Choose by evidence, not visual resemblance.

## Record

- exact native NpcTemplate/LocationTemplate family;
- faction/classification;
- stats/abstract type;
- fighting style/manoeuvre;
- movement/controller expectations;
- collider and CharacterGroundedData expectations;
- exact Location attachment/component order;
- animation-state expectations;
- death/corpse policy;
- presentation/renderer family;
- persistence/save assumptions.

## Procedure

1. Inspect candidate native baselines.
2. Reject candidates that only look similar but have incompatible controller, fighting-style, death, component-order, or renderer contracts.
3. Select one baseline and record the exact identities/build scope.
4. State which native systems will remain owners during the first live proof.

## Check

Confirm that the baseline contract is explicit enough to drive visual, animation, template, controller, combat, and death validation.

## Before continuing

The imported creature can yet use that baseline.

## Next

Proceed to [CI3 visual transport](../ci3-visual-transport/README.md).
