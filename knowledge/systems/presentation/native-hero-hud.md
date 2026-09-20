---
document_type: system
scope: native hero bars / quickslot HUD ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
  runtime: LOAD/PATCH_MARKERS_FOR_REPRESENTATIVE_VERSION
last_verified: 2026-09-20
---

# Native Hero HUD Ownership

`VHeroHUD` owns the combined hero-bar/quickslot visibility path.

## Core path

The private `ShowBars` property participates in `UpdateCanvasGroups()`.

Its normal fallback includes `Hero.WeaponsVisible`, explaining why the hero HUD can hide when weapons are sheathed.

The HUD also owns concrete child bar components and a selected quickslot surface.

## Useful native refresh

When a mod changes only visibility policy, asking the cached `VHeroHUD` to re-run `UpdateCanvasGroups()` is narrower than polling/mutating the hierarchy every frame.

A later project correction used that refresh when menu/dialogue/cursor hide-context state changed, because the vanilla HUD could otherwise remain hidden until another native event re-ran the update path.

## Selected quickslot

`VCSelectedQuickSlot.UpdateIcon()` can refresh the quickslot independently. A mod that hides the quickslot through CanvasGroup state may need to reapply that visibility after native icon refresh.

## Ownership boundary

Prefer CanvasGroup/visibility policy over:

- deactivating native root GameObjects;
- skipping native update methods;
- replacing native stat values;
- creating duplicate health/stamina/mana truth.
