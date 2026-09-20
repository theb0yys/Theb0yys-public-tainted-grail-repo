---
document_type: mechanic
scope: open existing FoA services from bonfire context
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: REPRESENTATIVE_SERVICE_FLOW_PROVEN
last_verified: 2026-09-20
---

# Reuse Native Bonfire Services

When the game already owns the service, call the service owner instead of recreating the screen or state transition.

## Pattern

```text
active FireplaceUI / upgraded fireplace context
→ evaluate native/context precondition
→ call native FireplaceUI / GemsUI / CharacterSheet action
→ native modal screen owns its lifecycle
→ on return, restore the bonfire/menu context
```

## Examples

Use the native routes for stash, cooking, alchemy, handcrafting, rest, level up, save, fast travel, recall pet, identify, sharpening/upgrade, armour weight reduction, gem management and transmog.

## Naming matters

“Repair / Gear Care” must not be documented as durability repair when the verified owner is **armour weight reduction**.

“Merchant” should not be documented as a universal spawned merchant when the researched path only selects currently loaded shops.

A useful public mechanic names the exact native capability actually proven.
