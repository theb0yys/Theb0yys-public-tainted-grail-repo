---
document_type: mechanic
scope: open native Hero Storage from an active fireplace/bonfire context
runtime: mono
evidence:
  static: DECOMPILED_AND_MULTI_PROJECT_SOURCE
  runtime: REPRESENTATIVE_SERVICE_LINEAGE
last_verified: 2026-09-20
---

# Open Native Stash from a Campfire

The verified native service route is:

`FireplaceUI.OpenHeroStorage()`

which delegates to the hero's native storage owner.

## Appropriate scope

Call this from an active/visible fireplace or upgraded-bonfire context.

This reuses:

- native HeroStorage;
- native HeroStorageUI;
- native save ownership;
- native transfer logic.

## Do not generalise to “portable stash anywhere”

A direct `Hero.Current.Storage.Open()` call may be reachable, but safe combat/dialogue/loading/input/balance behaviour outside campfire context was not proven by this research.

A custom placeable chest/campfire would be an entirely different world-placement/persistence problem.
