---
document_type: case
scope: Immersive HUD native visibility vs custom visual validation
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: MIXED
last_verified: 2026-09-20
---

# Native HUD Ownership vs Custom Visual Proof

The HUD project contains two very different evidence stories.

## Native visibility

`VHeroHUD.ShowBars` / `UpdateCanvasGroups()` provide a concrete game-owned visibility path. A representative build/load proved the patch route exists.

## Custom replacement visuals

Later custom vitals themes/layouts used generated and embedded resources, screenshot-derived bounds and several geometry corrections.

Many versions had successful build/deploy/hash/resource checks while in-game visual confirmation remained outstanding.

## Lesson

A mod can have **strong system ownership evidence** and **partial visual-design evidence** at the same time. Keep those claims separate.
