# Change Vendor Buy and Sell Prices

Use this guide when you want to tune merchant prices without replacing stock, affordability, item transfer, stolen-item handling or merchant wealth.

The validated seam is a Harmony **postfix** on:

```text
TradeUtils.Price(IMerchant seller, IMerchant buyer, Item item, int count = 1)
```

The game computes its normal final price first. Your mod then adjusts only the returned result.

Canonical mechanic: [Final Vendor-Price Adjustment](../../../knowledge/mechanics/economy/vendor-price-adjustment.md).  
Working lineage: [Vendor Price Tuning](../../../research/case-studies/gameplay/vendor-pricing.md).

## Runnable source

Start from the minimal public example: [Vendor price postfix example](../../../examples/mono/gameplay/vendor-price-postfix/README.md). Build it unchanged first, confirm the documented log/result, then make one change at a time.

## What you will build

```text
native pricing inputs/providers
→ TradeUtils.Price
→ classify hero buying vs hero selling
→ preserve unsafe/blocked cases
→ apply configured multiplier to __result
→ native TryTrade continues
→ normal affordability/transfer/events remain native
```

## Prerequisites

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. understand a basic Harmony postfix;
3. use a throwaway save for economy testing.

## Step 1 — define the smallest policy

Start with only two settings:

```text
BuyPriceMultiplier  = 1.0
SellPriceMultiplier = 1.0
```

Examples:

- `1.20` buy multiplier → buying costs 20% more;
- `0.80` buy multiplier → buying costs 20% less;
- `1.25` sell multiplier → player receives 25% more when selling.

Do not add category, region, reputation or merchant-specific rules until the base lane works.

## Step 2 — patch the final price result

Use a postfix on `TradeUtils.Price`.

The shape is:

```csharp
// Pseudocode: resolve exact game types/namespaces from your local references.

static void PricePostfix(
    IMerchant seller,
    IMerchant buyer,
    Item item,
    int count,
    ref int __result)
{
    if (__result <= 0)
        return;

    PriceDirection direction = ClassifyDirection(seller, buyer);
    if (direction == PriceDirection.Unknown)
        return;

    float multiplier =
        direction == PriceDirection.HeroBuying
            ? BuyPriceMultiplier
            : SellPriceMultiplier;

    if (!TryScaleSafely(__result, multiplier, out int adjusted))
        return;

    __result = adjusted;
}
```

This is intentionally a result adjustment, not a transaction rewrite.

## Step 3 — classify direction explicitly

Do not assume every `seller/buyer` pair is a hero merchant transaction.

Determine whether:

- the hero is buying;
- the hero is selling;
- the transaction is outside your supported scope.

If you cannot classify it confidently, leave `__result` untouched.

## Step 4 — preserve native blocked and zero-price cases

If the native result is zero or otherwise blocked, keep it that way unless your feature explicitly owns that rule.

Do not turn a native blocked case into a purchasable/sellable transaction by multiplying it.

## Step 5 — scale safely

Protect against:

- invalid/negative multipliers;
- overflow;
- unsupported item classes if you later add category rules;
- exceptions inside your postfix.

On any unexpected condition, return without changing the native price.

## Step 6 — leave the transaction owner alone

Do **not** patch this guide into:

- item transfer;
- merchant stock;
- merchant wealth;
- stolen/fence rules;
- restocking;
- affordability;
- reward generation.

Those are separate systems.

The value of the `TradeUtils.Price` seam is that `TradeUtils.TryTrade` and the rest of the native transaction remain intact.

## Verify in game

Use a disposable save and record baseline prices.

### Buy-side test

1. set both multipliers to `1.0`;
2. record a merchant buy price;
3. set only the buy multiplier;
4. reopen/re-evaluate the trade;
5. confirm the final buy price changes as expected;
6. complete one purchase;
7. confirm item transfer and wealth handling remain normal.

### Sell-side test

1. record a baseline resale price;
2. set only the sell multiplier;
3. confirm the resale price changes;
4. complete one sale;
5. confirm inventory/merchant transaction behaviour remains normal.

### Rollback

Disable the price mutation or restore multipliers to `1.0`, reload as required by your configuration model, and confirm vanilla pricing returns.

The core lane has throwaway-save buy/sell validation and disable/reload rollback evidence.

## Common mistakes

### Patching the whole trade transaction

You only need the final scalar. Replacing `TryTrade` creates unnecessary ownership and compatibility risk.

### Mutating stock to change price

Stock and price are separate owners.

### Treating zero as a normal number

A zero/native-blocked result can carry semantics. Preserve it.

### Applying a multiplier when direction is unknown

Fail closed to vanilla.

### Generalising the core proof

Regional, class-specific and preset pricing refinements have their own evidence. They do not inherit proof automatically from the simple final-price seam.

## Evidence boundary

**Proven:** final buy/sell price adjustment through `TradeUtils.Price`, common resale behaviour on the tested lane, and disable/reload rollback.

**Not claimed:** merchant stock changes, restocking, wealth changes, loot/reward changes, every regional/class-specific pricing policy, or all game versions.

## Next steps

After the base lane is stable, add one refinement at a time:

- item-class multipliers;
- merchant/category policies;
- regional composition.

For each new layer, preserve the same rule: if classification fails, keep the native price.
