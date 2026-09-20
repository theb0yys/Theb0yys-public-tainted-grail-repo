---
document_type: mechanic
scope: scale player-dealt NPC poise damage without changing normal damage
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC
last_verified: 2026-09-20
---

# Player Poise-Damage Scaling

A narrow route operates around `NpcGeneralFSM.OnDamageTaken(DamageOutcome)`.

## Pattern

Prefix:

1. require player-dealt damage;
2. ignore zero poise / excluded DOT as configured;
3. save original `Damage.Parameters.PoiseDamage`;
4. write a copied `DamageParameters` struct with scaled poise only.

Postfix:

5. restore the original poise value.

## Why restore

The same damage object may be observed by later listeners.

Temporary mutation keeps the chosen poise-processing owner seeing the adjusted scalar while reducing leakage into unrelated downstream observers.

## Preserve

Do not change:

- normal damage;
- NPC poise-meter upper limit;
- stagger state;
- NPC template;
- animation state;
- save data.
