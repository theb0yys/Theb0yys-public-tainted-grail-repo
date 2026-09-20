---
document_type: mechanic
scope: ordinary native talent-point spend availability
runtime: mono
evidence:
  static: DECOMPILED_AND_SCREEN_CORROBORATED
last_verified: 2026-09-20
---

# Native Campfire Talent Gate

The ordinary character-sheet talent spend path checks native upgrade availability in fireplace context.

This matches the player-facing “add points while sitting by the fire” behaviour documented in the progression research.

## Public consequence

If a mod wants to alter where/when talent points may be spent, that is a **different mechanic** from observing proficiency or applying a non-saved effect.

Do not bypass the native gate merely because a custom progression screen can display the tree.

A custom UI should either preserve the native spend preconditions or explicitly own and validate a replacement rule, including cancel/refund/respec/save behaviour.
