---
document_type: system
scope: native combat attack-slot pressure
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
last_verified: 2026-09-20
---

# Native Combat Pressure

FoA exposes combat-pressure controls through the active `Difficulty` model and `CombatDirector`.

## Key native values

- `Difficulty.MaxEnemiesAttacking` — concurrent attack pressure budget.
- `Difficulty.AttackActionUnBookProlong` — duration used when an attack action is released/unbooked.

`CombatDirector` consumes these values while coordinating attack-action bookings.

## Behavioural interpretation

Increasing `MaxEnemiesAttacking` can allow more simultaneous attack pressure.

Lowering `AttackActionUnBookProlong` can make attack bookings become available sooner, increasing cadence without directly increasing damage or health.

## Important boundary

These are **combat coordination** values, not AI target selection, damage multipliers or animation rewrites.

A pressure mod can tune these getters while leaving:

- NPC health/damage;
- factions/targets;
- movement;
- weapon templates;
- save state;

native.
