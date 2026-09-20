---
document_type: mechanic
scope: parry window, block impact and guard stamina runtime tuning
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PROJECT_SPECIFIC_PARTIAL
  persistence: NONE
last_verified: 2026-09-20
---

# Parry and Block Tuning

FoA exposes several separate guard-related stats.

## Hero stats

- `HeroStats.ParryWindowBonus`
- `HeroStats.ParryStaminaDamageMultiplier`
- `HeroStats.BlockingStaminaDamageMultiplier`
- `HeroStats.BlockPrepareSpeed`
- `HeroStats.ItemStaminaCostMultiplier`

`BlockParry.AfterEnter(...)` computes parry duration using a base `0.05f` plus the hero's `ParryWindowBonus.ModifiedValue`.

## Item stats

The currently used blocking/parrying item contributes values such as:

- `ParryStaminaCost`;
- `BlockStaminaCostMultiplier`;
- `HoldItemCostPerTick`.

These can be adjusted with non-saved item `StatTweak` elements rather than editing item templates.

## Ownership rule

Use hero stats for hero-wide guard policy and item stats for the blocking item's own cost/handling policy.

Do not rewrite block/parry animation state machines merely to expose a parry-window setting.
