---
document_type: mechanic
scope: post-roll crafting-material filtering in NPC corpse loot
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
last_verified: 2026-09-20
---

# Enemy-Corpse Crafting-Material Filtering

NPC corpse loot can use the same generated `SearchAction` row surface as containers while remaining a separate policy lane.

The private route identifies searched locations exposing `NpcElement` and applies a corpse-specific generated-row rule.

## Native basis

`SearchAction.OnNpcDeath` adds corpse loot and related generated rows to `_itemsInsideContainer`.

## Boundary

This is:

- post-roll filtering of existing rows;
- not a loot-table rewrite;
- not a change to NPC template corpse-loot definitions;
- not a merchant/container policy;
- not inventory-transfer mutation.

Keep corpse policy separate from ordinary barrel/sack/chest rules.
