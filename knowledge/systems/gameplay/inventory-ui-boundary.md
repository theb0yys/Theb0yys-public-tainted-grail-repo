---
document_type: system
scope: inventory truth vs presentation ownership
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION
last_verified: 2026-09-20
---

# Inventory Truth vs UI Ownership

FoA's native inventory is already a complete gameplay owner.

A UI framework should not recreate:

- item existence/quantity;
- equipment truth;
- quick-slot truth;
- native classifications;
- item movement;
- crime/economy rules;
- persistence.

## Safe custom UI boundary

A custom interface may own:

- workspaces/layout;
- search/filter/sort over a read-only projection;
- selection and detail panels;
- comparison presentation;
- UI-only preferences;
- its own view objects and subscriptions.

It should delegate gameplay actions back to exact native operations and then refresh from native state.

See [Native Inventory Lifecycle](native-inventory-lifecycle.md).
