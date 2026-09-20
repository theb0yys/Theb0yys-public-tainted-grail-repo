---
document_type: mechanic
scope: keep native hero HUD visible outside selected hide contexts
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  loader: REPRESENTATIVE_PATCH_LOAD_PROVEN
  feature_runtime: INCOMPLETE_IN_CITED_CURRENT_MATRIX
last_verified: 2026-09-20
---

# Force the Native Hero HUD Visible

A bounded HUD visibility route is a **Postfix** on `VHeroHUD.ShowBars`.

## Policy

```text
native ShowBars computes result
→ if mod enabled and no configured hide context:
     force result true
→ native UpdateCanvasGroups continues
```

Do not skip the original getter/update flow.

## Hide contexts

A custom force-visible policy may intentionally yield to:

- dialogue;
- cutscenes;
- menus;
- cursor-driven modal overlays.

When a hide context closes while weapons remain sheathed, trigger the native `UpdateCanvasGroups()` refresh so the forced visibility is reconsidered immediately.

## Per-element visibility

Individual native health/stamina/mana/quickslot presentation can be adjusted through the existing CanvasGroup layer without deactivating the native roots.

## Evidence boundary

A representative 0.5.0 log proved plugin load and patch installation. Many later custom-HUD/theme combinations had build/deploy evidence but their full in-game visual matrix remained pending.
