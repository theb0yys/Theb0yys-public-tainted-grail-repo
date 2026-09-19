# Content Authoring Journey — Items

This journey teaches the first complete new-content path without making this folder the canonical owner of item-system facts.

## Prerequisites

1. [First content-authoring setup](../../../00-never-made-a-mod-start-here/05_FIRST_CONTENT_AUTHORING.md)
2. [Identity and template concepts](../../../systems/core/templates-and-registries.md)
3. [Items and inventory ownership](../../../systems/items/README.md)

## Journey

### 1. Understand the owners

Read [Items and Inventory](../../../systems/items/README.md).

You should be able to explain the difference between:
- `ItemTemplate`;
- template registration/lookup;
- live `Item`;
- acquisition/distribution owner;
- persistence.

### 2. Follow the bounded custom-item mechanic

Read [Custom Item Integration](../../../mechanics/items/custom-item-integration.md).

Do not generalise the first proven merchant route into every item family or acquisition system.

### 3. Choose one acquisition route

- [Acquisition](../../../mechanics/items/acquisition.md)
- [Distribution](../../../mechanics/items/distribution.md)

Use the route owned by the gameplay system you actually need.

### 4. Keep persistence separate

If the item must survive save/load, read [Saving and Persistence](../../../systems/persistence/README.md) and validate that lifecycle separately.

## Completion outcome

You should be able to trace:

```text
identity
→ template readiness
→ registration
→ provider re-resolution
→ native Item construction
→ one acquisition owner
→ downstream observation
→ separate persistence question
```
