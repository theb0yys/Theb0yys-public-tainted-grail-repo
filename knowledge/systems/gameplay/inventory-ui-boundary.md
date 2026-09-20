---
document_type: system
scope: inventory truth vs presentation ownership
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION
last_verified: 2026-09-20
---

# Inventory State vs Custom UI

Use this page when you are building a custom inventory screen and need to decide **what the UI may own and what must stay native**.

FoA already owns the gameplay truth for inventory.

## Keep these native

Do not recreate:

- whether an item exists;
- item quantity;
- equipped state;
- quick-slot state;
- native item classification;
- item movement;
- crime/economy rules;
- persistence.

## Your UI can own

A custom interface can safely own things such as:

- layout/workspaces;
- search;
- filters;
- sorting;
- selection state;
- detail/comparison panels;
- UI-only preferences;
- its own Views/GameObjects/subscriptions.

## Action flow

A good custom interface follows this pattern:

~~~text
read native inventory state
→ present it differently
→ request native action
→ wait for native result
→ refresh from native state
~~~

Do not let the UI become a second inventory database.

See [Native Inventory Lifecycle](native-inventory-lifecycle.md).
