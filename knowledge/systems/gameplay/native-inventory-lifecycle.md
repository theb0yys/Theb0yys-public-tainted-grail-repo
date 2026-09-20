---
document_type: system
scope: player inventory / character-sheet native ownership
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION_E5
  runtime: NOT_RUN_BY_THIS_MAP
last_verified: 2026-09-20
source_artifact_sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
---

# Native Inventory Lifecycle

Use this page when you need to understand which native objects own inventory, equipment, quick slots, and Character Sheet actions before building a custom UI or inventory feature.

The inspected FoA build exposes a concrete inventory path. Do not invent generic `InventoryManager` abstractions over it.

## Entry and screen lifecycle

```text
VHeroKeys.Handle
→ CharacterSheetUI.ToggleCharacterSheet(Inventory, ...)
→ CharacterSheetUI
   → OnInitialize
   → OnFullyInitialized
   → CharacterSheetTabs
   → InventoryUI
   → InventorySubTabs
      ├─ BagUI
      ├─ LoadoutsUI
      └─ HeroArmorSetsUI
→ TryDiscard / OnDiscard / OnFullyDiscarded
```

The Character Sheet also owns renderer/HLOD freeze bookkeeping and native close/back lifecycle.

## Authoritative data owners

`HeroItems` is the player inventory/equipment owner.

Important native projections include:

- `Inventory` / contained items;
- `Items` / owned items;
- loadouts;
- equipment-slot state;
- quick-slot state;
- weight;
- add/remove/drop/move operations;
- serialization/deserialization.

`Item` is the concrete item-instance model and owns/derives presentation and action state such as quantity, template, favorite, equipped state, tags, item class, stolen/quest flags, effects, price and action methods.

## Action owners

Native operations already exist for:

- equip/unequip;
- item use;
- quick slots;
- item movement;
- container transfer;
- restrictions/crime;
- persistence.

## Public modding boundary

A custom UI should normally:

```text
read native HeroItems / Item projection
→ present/search/group locally
→ request a native operation through a narrow adapter
→ observe native result/events
→ refresh from native truth
```

Do not maintain a second inventory, equipment or quick-slot truth.
