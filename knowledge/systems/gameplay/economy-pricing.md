---
document_type: system
scope: FoA vendor final-price ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_CORROBORATED
  runtime: VALIDATED_BY_TAINTED_ECONOMY_VENDOR_LANE
last_verified: 2026-09-20
---

# Vendor Pricing

Use this page when you want to change **the price the player pays or receives in a native trade** without replacing the trade transaction itself.

The narrow final-price method is:

~~~csharp
TradeUtils.Price(
    IMerchant seller,
    IMerchant buyer,
    Item item,
    int count = 1)
~~~

It returns the final vanilla integer price consumed by later trade/affordability logic.

## Native transaction flow

~~~text
item/shop/player modifiers
→ TradeUtils.Price(...)
→ final integer price
→ vanilla affordability/trade checks
→ TradeUtils.TryTrade
→ native item/wealth transfer
~~~

That makes the return value a useful pricing seam.

## What a price mod should leave native

Unless the feature explicitly owns something else, preserve:

- zero-price/blocked-sale outcomes;
- stolen/fence behavior;
- merchant stock;
- merchant wealth;
- item/template state;
- actual transaction execution.

The pricing mod should change **the price**, then let the native trade system perform the trade.

## Persistence

Changing `TradeUtils.Price` is not itself a save write.

Save-visible consequences happen indirectly if the player completes the native transaction and native inventory/wealth state changes.

## How to verify

Check:

1. exact seller/buyer;
2. exact item/count;
3. vanilla price;
4. modified final price;
5. blocked/zero-price cases preserved;
6. stolen/fence cases behave as intended;
7. affordability uses the modified value;
8. native trade executes once;
9. stock and wealth update normally;
10. unrelated merchants/items remain unchanged.

## Evidence

This final-price seam is decompilation/source-corroborated and has bounded runtime validation in the project vendor lane.
