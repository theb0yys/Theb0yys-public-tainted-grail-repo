# Tune Restockable Merchant Stock

**Evidence status: PARTIAL.** The code boundary/self-review is strong, but in-game restock behaviour and unique-item non-duplication were explicitly unverified in the cited case.

Working lineage: [Merchant Restock Boundary](../../../research/case-studies/merchants/restock-boundary.md).

## Safe boundary

The proposed route is:

- patch `Shop.OpenShop()` before UI open;
- touch `RestockableStock` only;
- leave `UniqueStock` separate;
- do not hook purchase/removal;
- do not change merchant wealth;
- keep category filtering opt-in.

## Process

```text
shop opening
→ identify supported merchant
→ inspect RestockableStock
→ apply bounded restock/category rule
→ native shop UI/transaction continue
```

## Important distinction

Restockable stock and unique stock are different ownership lanes.

Do not duplicate unique items because a generic stock loop happened to find them.

## Required runtime proof

Before promotion, validate:

- expected stock change;
- close/reopen;
- actual restock cycle;
- no unique-item duplication;
- purchase/removal remains native;
- save/load behaviour where relevant.

## Current proof boundary

This is an implementation guide for the reviewed boundary, not a claim that merchant restocking is fully runtime-proven across shops.
