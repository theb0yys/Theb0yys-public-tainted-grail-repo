# Tune Combat Pressure Without Raw Damage Multipliers

**Evidence status: PARTIAL.** The owner model and intervention direction are established, but this case study does not record a complete end-to-end runtime validation matrix.

Working lineage: [Combat Pressure Without Raw Damage Multipliers](../../../research/case-studies/combat/pressure-not-damage.md).

## Goal

Make combat feel harder by changing **coordination and resource pressure**, not by multiplying every enemy's damage or health.

The working design focused on:

- concurrent attack slots;
- attack-booking turnover;
- runtime stamina pressure.

## Process

1. Identify the native owner for the coordination/resource value you want to change.
2. Change one pressure lever at a time.
3. Leave damage/health at vanilla during the first proof.
4. Observe whether native AI, movement and attack execution remain intact.
5. Add a second lever only after the first can be isolated.

A useful sequence is:

```text
baseline combat
→ change concurrent attack pressure
→ validate
→ reset
→ change stamina pressure
→ validate
→ combine only after both are understood
```

## What not to do

Do not treat "harder combat" as permission to:

- multiply all incoming damage;
- rewrite AI controllers;
- change every enemy family at once;
- combine timing, stamina, damage and health changes in the first test.

## Verification

Record:

- exact native owner/value changed;
- baseline encounter;
- changed encounter;
- whether attack coordination changed;
- whether stamina pressure changed;
- whether native abilities/movement remained functional;
- whether the mod returns cleanly to baseline.

## Current proof boundary

The case establishes the owner-first design direction and chosen pressure levers. A release-ready guide still needs explicit runtime validation for the exact values/targets used by the implementation.
