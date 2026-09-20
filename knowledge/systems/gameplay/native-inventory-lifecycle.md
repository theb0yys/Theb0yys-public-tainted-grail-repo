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

Use this page when your mod needs to **read or change the player's inventory, equipment, loadouts, quick slots, or Character Sheet UI**.

The key rule is:

> Keep native `HeroItems` / `Item` state as the source of truth. A custom UI should project that state, not replace it.

## Opening the native inventory

The inspected Mono path is:

~~~text
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
~~~

The Character Sheet also participates in renderer/HLOD freeze handling and the native back/close lifecycle.

## What owns inventory state?

`HeroItems` is the player inventory/equipment owner.

It exposes or owns native state for:

- contained/owned items;
- loadouts;
- equipment slots;
- quick slots;
- weight;
- add/remove/drop/move operations;
- serialization/deserialization.

`Item` is the runtime item instance and carries/derives state such as:

- quantity;
- template;
- favorite/equipped state;
- tags/classification;
- stolen/quest flags;
- effects;
- price;
- item actions.

## Use native actions for gameplay changes

FoA already has native operations for:

- equip/unequip;
- item use;
- quick-slot handling;
- item movement;
- container transfer;
- crime/restriction checks;
- persistence.

Prefer calling the native operation that owns the action instead of maintaining a parallel inventory model.

## Safe custom-UI pattern

A custom inventory UI should normally do this:

~~~text
read HeroItems / Item state
→ build search/filter/sort locally
→ user chooses an action
→ call the narrow native operation
→ observe result/events
→ refresh from HeroItems / Item state
~~~

That allows the custom UI to own presentation while native systems continue to own gameplay state.

## What not to duplicate

Do not create a second source of truth for:

- item quantity;
- equipped slots;
- loadouts;
- quick slots;
- crime/stolen rules;
- item persistence.

If your UI disagrees with `HeroItems`, the UI should refresh rather than forcing the game to match its private copy.

## Evidence limits

This page is based on current Mono static/decompilation evidence for the inspected build.

The page maps native ownership and lifecycle. It does not by itself prove every custom UI action path at runtime.
