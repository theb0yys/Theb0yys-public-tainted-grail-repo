---
document_type: system
scope: FoA vendor final-price ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
  runtime: VALIDATED_BY_TAINTED_ECONOMY_VENDOR_LANE
last_verified: 2026-09-20
---

# Vendor Pricing Ownership

The narrow final-price owner used by Tainted Economy is:

`TradeUtils.Price(IMerchant seller, IMerchant buyer, Item item, int count = 1)`

The method returns the final vanilla price consumed by later affordability and trade logic.

## Ownership model

```text
native item/shop/player modifiers
→ TradeUtils.Price(...)
→ final integer price
→ vanilla trade checks / TradeUtils.TryTrade
→ native item/wealth transfer
```

This makes the **return value** a useful tuning seam while keeping the transaction itself native.

## Preserve

A bounded price mod should preserve:

- vanilla zero-price/blocked-sale outcomes;
- stolen/fence handling unless explicitly proven otherwise;
- merchant stock ownership;
- merchant wealth ownership;
- item/template state;
- transaction execution.

Changing the final price has **indirect** save consequences only when the player completes a native transaction using that price; it is not itself a direct save-write API.
