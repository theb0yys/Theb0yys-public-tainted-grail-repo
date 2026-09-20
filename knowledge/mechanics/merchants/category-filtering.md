---
document_type: mechanic
scope: filter normal merchant restock categories
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: NOT_VERIFIED
  compatibility: HIGHER_RISK_PRIVATE_REFLECTION
last_verified: 2026-09-20
---

# Merchant Restock Category Filtering

A merchant implementation can filter repeatable restock output using `ItemTemplate` helpers and attachment checks.

The tricky part is preserving the normal stock owner's compressed state/count behaviour.

One implementation accesses `RestockableStock.Capacity` and private `_compressedItems` storage through reflection.

## Rule

Treat this as **patch-sensitive**.

If the private member shape changes:

- disable filtering;
- do not guess a replacement backing field;
- re-inspect `RestockableStock` ownership and semantics;
- keep ordinary restock behaviour separate from the failed filter layer.

The private owner review had not yet promoted in-game category-filter behaviour.
