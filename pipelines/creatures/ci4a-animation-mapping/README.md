# Creature CI4A — Animation Mapping

## Objective

Map source animation clips to the exact native animation-state contract required by the selected baseline.

## Common bounded states

The documented creature lane has used states including:

- Idle (1)
- Movement (2)
- ShortRange (16)
- GetHit (32)
- Death (44)

Additional states require their own evidence.

## Procedure

1. Enumerate the native states required by the selected baseline.
2. Map each required state to an authorised source clip.
3. Record loop/root-motion assumptions.
4. Build the mapping asset/contract.
5. Load and validate it in isolation.
6. Release and reload it.
7. Fail the gate if required states are absent or semantically incompatible.

## Check

Confirm that the required baseline states have explicit, tested mappings and the mapping loads/releases cleanly.

## Failure lesson

Combat success does not compensate for missing locomotion, hit, or death states. Missing Movement/GetHit/Death mappings previously produced dragging and broken death behaviour even when attacks worked.

## Before continuing

Template construction, actor lifecycle, AI, combat correctness, corpse handoff, population, or persistence.

## Next

Proceed to [CI4 template contract](../ci4-template-contract/README.md).
