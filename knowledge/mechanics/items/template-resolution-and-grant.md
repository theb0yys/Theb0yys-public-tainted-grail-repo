---
document_type: mechanic
scope: resolve a loaded ItemTemplate and grant an Item to Hero.Current
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PARTIAL_BY_CONSUMER
  persistence: NOT_GENERALIZED
last_verified: 2026-09-20
---

# Item Template Resolution and Hero Grant

The reusable item path is:

```text
World.Services ready
→ TemplatesProvider.AllLoaded
→ resolve exact ItemTemplate identity
→ construct Item from that template
→ add Item to World
→ add Item to Hero.Current.HeroItems
```

This path is represented in multiple private implementations and is suitable as a **mechanic**, not as proof that every granted item is persistence-safe.

## Preconditions

- `World.Services` exists.
- `TemplatesProvider` is available and `AllLoaded` is true.
- The template identity is exact and reviewed.
- `Hero.Current` exists and is not discarded.
- `HeroItems` is available.

## Boundary

Granting an item proves inventory acquisition behaviour only for the tested route. It does not automatically prove:

- custom-template registration;
- equipment presentation;
- merchant/loot distribution;
- save restoration;
- missing-mod behaviour.

For custom definitions, see [Weapons](../weapons/README.md) or the existing [custom item guide](../../../guides/tasks/items/custom-items.md).
