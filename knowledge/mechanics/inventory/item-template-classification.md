---
document_type: mechanic
scope: ItemTemplate helper-based classification
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  semantics: NEED_SYSTEM_REVIEW_FOR_GENERAL_NATIVE_TRUTH
last_verified: 2026-09-20
---

# ItemTemplate Classification Helpers

Private implementations use native `ItemTemplate` helpers such as:

- `IsConsumable`
- `IsCrafting`
- `IsArrow`
- `IsPlainFood`
- `IsDish`
- `IsFish`
- `IsArmor`
- `IsWeapon`
- `IsShield`
- `IsJewelry`
- `IsEquippable`

and attachment presence such as `LockpickAttachment` for narrow filtering.

## Use correctly

These helpers are useful for implementation filtering. They are not automatically a complete taxonomy of every FoA item category or every version.

When a mechanic depends on category semantics, record the exact helper(s) used rather than translating them into a broader label.
