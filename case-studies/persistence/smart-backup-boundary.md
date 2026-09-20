---
document_type: case
scope: Smart Save Backups low-interference design
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: LOADER_ONLY_IN_CITED_PLAN
last_verified: 2026-09-20
---

# Smart Backups Without More Save Slots

The backup project explicitly rejected the obvious but riskier design: “add more native save slots.”

Instead it researched native save/provider read surfaces and chose a plugin-owned archive directory.

## Lesson

When the user wants **redundancy**, you may not need to extend the game's persistence model at all.

Copying an existing save through its provider API into a sidecar archive can preserve the native UI/rotation rules and keep restore tooling separate.

The private validation had not yet proven actual backup archive creation, so the public page preserves that gap.
