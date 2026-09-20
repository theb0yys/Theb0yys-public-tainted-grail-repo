---
document_type: mechanic
scope: regional multiplier layer on TradeUtils.Price
runtime: mono
evidence:
  static: SOURCE_AND_METADATA_INSPECTED
  runtime: PARTIAL_BY_RULE_SET
last_verified: 2026-09-20
---

# Regional Vendor Pricing

Regional pricing composes an additional multiplier on the already-bounded `TradeUtils.Price` return-value lane.

## Identity input

The non-hero merchant context can be built from:

- shop runtime type;
- shop template;
- parent `Location`;
- location template/spec;
- location/spec tags.

Rules can then match that **read-only identity context** and apply hero-buy/hero-sell multipliers.

## Boundary

This route changes only the price result.

It does not mutate:

- location data;
- merchant wealth;
- merchant stock;
- item templates;
- loot rows;
- reward state.

Treat rule matching as configuration policy on a proven price seam, not as proof of canonical “regions” unless the location identity itself has been promoted.
