---
document_type: mechanic
scope: quantity scaling of generated SearchAction container rows
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  persistence: SAVE_BACKED_RUNTIME_ROWS
last_verified: 2026-09-20
---

# Generated Container-Row Quantity Scaling

`SearchAction.OnInitialize` resolves container loot into saved runtime rows:

`List<ItemSpawningDataRuntime> _itemsInsideContainer`

A bounded lane can scale the quantity of **already generated rows** and leave transfer/inventory logic native.

## Important persistence boundary

These runtime rows are serialized container state.

Changing them is not equivalent to changing a transient UI number.

## Preserve

Do not use this lane to edit:

- loot tables;
- item templates/elements;
- theft/crime metadata;
- `MoveItem` / `RemoveItem` / transfer logic;
- merchant stock/wealth.

Use a per-instance guard so the same `SearchAction` is not repeatedly multiplied in one session.
