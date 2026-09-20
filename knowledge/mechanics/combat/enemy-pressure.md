---
document_type: mechanic
scope: tune simultaneous enemy attack pressure through Difficulty getters
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC
last_verified: 2026-09-20
---

# Enemy Attack Pressure

A bounded combat-feel route is to Postfix the active `Difficulty` getter results used by `CombatDirector`:

- `MaxEnemiesAttacking`;
- `AttackActionUnBookProlong`.

## Pattern

```text
native difficulty computes result
→ mod adjusts scalar within configured bounds
→ CombatDirector consumes adjusted result
→ native attack booking/AI continues
```

This changes pressure/cadence without replacing combat AI.

## Do not conflate

`MaxEnemiesAttacking` is not “number of enemies aggroed”.

`AttackActionUnBookProlong` is not an animation-speed multiplier.

Both affect coordination state inside the native combat director.
