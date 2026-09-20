---
document_type: mechanic
scope: sprint and general negative stamina-use scaling
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  loader: PRIVATE_PLUGIN_LOAD_VALIDATED
  feature_runtime: NOT_COMPLETE_IN_CITED_PLAN
  persistence: NONE
last_verified: 2026-09-20
---

# Stamina Drain Through Native Stat Multipliers

Two different native stats matter:

- `CharacterStats.SprintCostMultiplier` — consumed by hero sprint cost.
- `CharacterStats.StaminaUsageMultiplier` — multiplies broader **negative** stamina changes.

## Bounded route

```text
CharacterStatsWrapper.Initialize
→ add non-saved runtime tweaks
→ sprint path reads SprintCostMultiplier
→ broader negative-stamina path reads StaminaUsageMultiplier
```

This avoids patching every attack/block/dodge/sprint method independently.

## Important scope difference

`StaminaUsageMultiplier` is broader than “combat actions”.

Do not claim one slider controls each action independently until those consumers are mapped and tested.

A sprint-only feature should prefer `SprintCostMultiplier` over globally changing negative stamina usage.

## Save boundary

The tweak is runtime-only; BepInEx config is the only intended persistent mod state.

The cited validation plan had build/load/config evidence but still required action-by-action in-game feature testing.
