---
document_type: mechanic
scope: show carried/stash ingredient split while preserving native craftability
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Stash Ingredient Count Clarity

A UI-only storage/crafting improvement can patch native quantity text while leaving logic untouched.

Useful display targets include:

- `VEditableWorkbenchSlot.OnIngredientQuantityChanged(...)`;
- `VCIngredientUpgradeSlotUI.RefreshRequiredQuantity(...)`.

## Rule

Only show a split such as:

`carried + stash = total / required`

when your independently computed carried/stash split equals the **game-provided combined total**.

Otherwise fall back to the native total.

## Preserve

Do not override:

- craftability;
- ingredient ownership;
- consumption order;
- recipe data;
- upgrade requirements;
- Hero Storage persistence.
