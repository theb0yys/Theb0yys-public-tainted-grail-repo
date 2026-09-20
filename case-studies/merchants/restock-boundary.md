---
document_type: case
scope: Merchant Stock Tweaks restock design
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: NOT_VERIFIED_AT_OWNER_REVIEW
last_verified: 2026-09-20
---

# Merchant Restock Boundary

Merchant Stock Tweaks is useful as an example of a **well-bounded but not yet runtime-promoted** route.

## Chosen boundary

- patch `Shop.OpenShop()` before UI open;
- touch `RestockableStock` only;
- leave `UniqueStock` separate;
- do not hook purchase/removal;
- do not change merchant wealth;
- keep category filtering opt-in.

## Why the status matters

The private self-review recorded no blocking code findings **and** separately recorded that in-game restock behaviour and unique-item non-duplication were still unverified.

Good documentation preserves both facts.
