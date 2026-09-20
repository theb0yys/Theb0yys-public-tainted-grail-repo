---
document_type: system
scope: native crafting/upgrade ingredient ownership across inventory + HeroStorage
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Stash-Aware Crafting and Upgrades

In the inspected FoA paths, normal crafting and gear-upgrade ingredient checks already combine carried inventory with Hero Storage.

## Recipe crafting

`Crafting.FilteredHeroItems` combines:

`HeroItems.Items.Concat(Hero.Storage.Items)`

and builds `SimilarItemsData` from that combined source.

Recipe UI quantities therefore receive the combined carried+stash amount.

## Gear services

`GemsBaseUI<T>` requests Hero Storage, combines hero inventory and storage, checks ingredient possession against that combined data, and consumes through `DropHeroSimilarItems(...)`.

Consumption prefers carried items before stashed items.

## Public modding consequence

If the requested feature is “craft using stash materials”, do not rewrite craftability/consumption until you prove the native path is missing it.

For the reviewed recipe/gear routes, the safe mod was a **display clarity** feature because the game already owned stash-aware checking and spending.
