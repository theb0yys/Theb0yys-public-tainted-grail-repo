---
document_type: mechanic
scope: normal merchant stock restock before Shop.OpenShop
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: NOT_VERIFIED_IN_PRIVATE_OWNER_REVIEW
  persistence: NATIVE_STOCK_SIDE_EFFECTS_POSSIBLE
last_verified: 2026-09-20
---

# Merchant Restock on Shop Open

A candidate reusable route is to patch `Shop.OpenShop()` and operate only on normal `RestockableStock` before the native shop UI continues.

## Native context

Research identified:

- `Shop.OpenShop()` as the shop-open flow;
- `Shop.Restock(bool force = false)` as native restock logic;
- `RestockableStock.Restock()` as the repeatable stock owner;
- `UniqueStock` as a separate path for unique/story-locked items.

## Bounded intervention

```text
Shop.OpenShop Prefix
→ inspect configured conditions
→ iterate RestockableStock only
→ invoke normal restock behaviour
→ original Shop.OpenShop continues
```

Do not treat `UniqueStock` like repeatable stock.

## Category filtering risk

One implementation reads `Capacity` and private `_compressedItems` state through reflection/backing fields. That makes category pruning/count restoration patch-sensitive.

## Proof boundary

The private owner review explicitly recorded that in-game restock behaviour and unique-item non-duplication were still **not verified** at that review point.

Publish this as a source-inspected mechanic candidate, not “proven merchant restocking”.
