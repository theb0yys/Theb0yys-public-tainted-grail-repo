---
document_type: mechanic
scope: observe consumed food/drink item-use after native action execution
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PRIVATE_EVENT_ORDER_PROOF
last_verified: 2026-09-20
---

# Food / Consumable Use Observation

Tainted Survival researched `Item.PerformImmediate(ItemActionType)` as a narrow post-action observation surface for food/consumable use.

The diagnostic route filters to relevant native item classifications and `Eat` / `Use` actions, then records the event **after** native execution.

## Useful pattern

```text
native item action executes
→ confirm action/item class
→ observe resulting player state or project policy
→ add project-owned session effect if explicitly enabled
```

This is safer than replacing the item-use action or rewriting native food templates merely to add a survival overlay.

## Boundary

Observing a consumed item does not prove a generic buff/status contract. If the feature depends on an actual health/status delta, observe that delta explicitly.
