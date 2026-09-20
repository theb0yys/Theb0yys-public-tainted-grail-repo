---
document_type: case
scope: Tainted Economy generated SearchAction row mutation
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
last_verified: 2026-09-20
---

# Container Rules: Post-Roll and Save-Backed

The container work is more consequential than the vendor-price seam because `SearchAction._itemsInsideContainer` is save-backed runtime state.

The project therefore split:

- quantity scaling;
- rule-based remove/scale;
- corpse-specific filtering;

and explicitly blocked loot-table edits, metadata edits and transfer rewrites.

## Lesson

Two mechanics can both “change loot” while having completely different ownership and persistence risk.
