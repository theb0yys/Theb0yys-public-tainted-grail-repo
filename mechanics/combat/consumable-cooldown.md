---
document_type: mechanic
scope: project-owned in-memory cooldown around player consumable Item.Use
runtime: mono
evidence:
  static: SOURCE_AND_NATIVE_TARGET_INSPECTED
  runtime: PROJECT_SPECIFIC
  persistence: NONE
last_verified: 2026-09-20
---

# Consumable Use Cooldown

A combat-pressure mod can add a **mod-owned in-memory cooldown** without editing consumable templates.

## Route

```text
Item.Use Prefix
→ classify hero-owned consumable
→ if relevant project cooldown active:
     return false / block use
→ otherwise let native Item.Use run
→ Postfix compares quantity/health/mana/stamina/status deltas
→ record last successful use timestamp
```

## Why postfix confirmation matters

Do not start a cooldown merely because `Item.Use` was called.

Record it only after evidence suggests the native action actually consumed/applied the item.

## Boundary

This is mod policy, not a native cooldown system.

Keep timestamps session-local unless persistence is separately designed.
