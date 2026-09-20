---
document_type: mechanic
scope: lockpicking sweet-spot tolerance and pick durability pressure
runtime: mono
evidence:
  static: DECOMPILED
  runtime: NOT_RUN_IN_CITED_LOCKPICKING_REFORGED_PLAN
last_verified: 2026-09-20
---

# Lockpicking Tolerance and Pick Damage

Two useful lockpicking seams are:

- `LockAction.Tolerance` — effective lock tolerance;
- `LockpickingInteraction.ConsumePickHP(float)` — pick durability loss.

## Native math

The minigame sweet spot uses the tolerance angle together with hero theft/tolerance modifiers.

Pick HP loss uses native tolerance tool damage, delta time and the hero lockpick-damage multiplier.

## Tuning

A mod can:

- Postfix the effective tolerance result for difficulty policy;
- Prefix `ConsumePickHP` and scale the input duration/damage pressure.

## Boundary

Changing effective `LockAction.Tolerance` may override lock-specific or visual-script difficulty reductions while active.

Do not call the result “native lock difficulty preserved” unless those modifiers remain part of your composition.
