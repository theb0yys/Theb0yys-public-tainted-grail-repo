---
document_type: system
scope: native hero bars / quickslot HUD ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
  runtime: LOAD/PATCH_MARKERS_FOR_REPRESENTATIVE_VERSION
last_verified: 2026-09-20
---

# Native Hero HUD

Use this page when you want to change **Hero health/stamina/mana bars, quickslot visibility, or native HUD show/hide policy**.

The key rule is:

> Prefer changing visibility policy and asking the native HUD to refresh instead of replacing the HUD's gameplay state.

## Main owner

`VHeroHUD` owns the combined Hero bars/quickslot visibility flow.

The private `ShowBars` property participates in `UpdateCanvasGroups()`.

Its normal fallback includes `Hero.WeaponsVisible`, which explains why parts of the HUD can hide when weapons are sheathed.

## Use the native refresh path

If your mod changes only visibility rules, a narrow approach is:

~~~text
change mod-owned visibility condition
→ call cached VHeroHUD.UpdateCanvasGroups()
→ native HUD reapplies its CanvasGroup state
~~~

This is preferable to scanning and mutating the hierarchy every frame.

A project correction used this approach when menu/dialogue/cursor context changed because the vanilla HUD could otherwise remain hidden until another native event refreshed it.

## Quickslot refresh is separate

`VCSelectedQuickSlot.UpdateIcon()` can refresh the selected quickslot independently.

If your mod applies CanvasGroup visibility to the quickslot, native icon refresh may overwrite/reapply parts of its state.

Reapply your visibility policy at the appropriate quickslot refresh point rather than polling the hierarchy.

## What not to replace

For a visibility-only mod, do not:

- deactivate native HUD root GameObjects;
- skip native update methods broadly;
- rewrite health/stamina/mana values;
- create duplicate gameplay stat state.

The HUD should remain a presentation of native Hero state.

## How to verify

Check:

1. normal HUD visible state;
2. weapons sheathed/drawn;
3. pause/menu/dialogue/cursor hide contexts;
4. health/stamina/mana bars;
5. selected quickslot;
6. relevant config changes;
7. native refresh after state changes;
8. no per-frame hierarchy scan;
9. restoration when the mod is disabled.

## Evidence

The ownership/refresh path is decompilation/source-corroborated with representative runtime load/patch markers.
