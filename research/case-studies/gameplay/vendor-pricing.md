# Vendor Price Tuning

The working economy path changes the **final native price result**, not merchant inventory and not the trade transaction itself.

## Working implementation lineage

Tainted Economy validated this lane on a throwaway save for:

- hero buying from a merchant;
- hero selling to a merchant;
- common weapon/armour resale changes;
- disable/reload rollback.

## Native owner

The narrow target is:

~~~text
Awaken.TG.Main.Locations.Shops.TradeUtils.Price(
    IMerchant seller,
    IMerchant buyer,
    Item item,
    int count = 1)
~~~

`TradeUtils.Price` already incorporates the game's price providers, merchant modifiers and stolen/fence handling before returning the amount used by the trade flow.

## Working patch shape

Use a **postfix** and adjust only the returned price.

~~~text
native price providers
→ TradeUtils.Price
→ mod classifies buy/sell + supported item
→ adjust returned price
→ native affordability/transfer/wealth/events continue normally
~~~

That preserves `TradeUtils.TryTrade` as the transaction owner.

## Why this seam is useful

Changing the returned price leaves these systems native:

- seller/buyer identity;
- item ownership;
- stolen/fence rules;
- affordability checks;
- item transfer;
- merchant/hero wealth mutation;
- trade events.

## Safe implementation rules

- default multiplier `1.0`;
- keep direction classification explicit;
- preserve vanilla for unsupported item classes;
- keep zero-price/native-block cases intact;
- clamp arithmetic safely;
- log the original and final value when diagnosing.

## What this pattern does not own

Do not use the price patch to:

- restock merchants;
- add/remove stock;
- rewrite merchant wealth;
- create items;
- bypass stolen-item restrictions.

Those are separate owners.
