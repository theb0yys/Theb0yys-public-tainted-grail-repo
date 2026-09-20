---
document_type: mechanic
scope: add an existing native item as a generated searchable-container row
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PRIVATE_PROJECT_VALIDATION_PENDING_FULL_MATRIX
last_verified: 2026-09-20
---

# Add an Existing Item to a Search Loot Row

For an **existing loaded ItemTemplate**, a mod can add a normal generated row to an already-owned `SearchAction` container/corpse surface.

## Path

```text
SearchAction search surface
→ resolve existing ItemTemplate
→ per-SearchAction/session roll
→ construct ItemSpawningDataRuntime(template)
→ set quantity
→ add/merge row in _itemsInsideContainer
→ native ContainerUI displays/transfers the row
```

## Persistence consequence

`SearchAction._itemsInsideContainer` is save-backed runtime state.

An unlooted inserted row can therefore persist like other generated search contents.

## Boundary

This is not:

- loot-table editing;
- custom item registration;
- merchant injection;
- quest reward editing;
- broad row scaling/removal.

Use it only when the feature intentionally wants a post-generation existing-item row.
