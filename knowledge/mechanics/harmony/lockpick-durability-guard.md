---
document_type: mechanic
scope: conditionally suppress lockpick HP consumption
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PENDING_FOR_CUSTOM_TAINTED_LOCKPICK_ROUTE
last_verified: 2026-09-20
---

# Lockpick Durability Guard

The narrow durability seam is:

`LockpickingInteraction.ConsumePickHP(float)`

A Harmony Prefix can return `false` when a mod-owned predicate is satisfied, skipping only the native HP-consumption call.

## Why this seam is narrow

The researched Tainted Lockpick route deliberately leaves these native:

- whether a lock can be picked;
- key-only lock behaviour;
- success/failure logic;
- crime/ownership checks;
- recipes/vendors/loot.

Only durability pressure is suppressed when the hero owns the reviewed custom lockpick.

## Boundary

The source/build route existed, but the owner documentation still required live validation for custom-template registration, grant, display, lockpicking behaviour and save/load.

Do not generalise a source-inspected Prefix into a cross-version runtime guarantee.
