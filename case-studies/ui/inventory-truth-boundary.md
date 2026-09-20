---
document_type: case
scope: Tainted Interface Inventory Suite ownership map
evidence:
  static: DECOMPILATION_AND_SOURCE_REVIEW
  runtime: OUTSTANDING_FOR_INVENTORY_SUITE
last_verified: 2026-09-20
---

# Inventory Suite: Presentation Without a Second Inventory

Installed-build decompilation established `HeroItems`, `Item`, `HeroLoadout`, native actions and Character Sheet lifecycle as authoritative.

The custom UI design therefore owns projection/search/workspaces/selection—not gameplay inventory truth.

A first read-only source slice implemented that boundary and explicitly omitted equip/use/quick-slot/transfer mutation.

## Lesson

The safest way to build a richer interface over a proprietary game is often to keep the proprietary model/action owners intact and treat the custom interface as a projection plus narrow action adapters.

Runtime host/focus/cleanup proof remains separate from static ownership correctness.
