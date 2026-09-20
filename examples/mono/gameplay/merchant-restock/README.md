# Restock a Merchant When the Shop Opens

This example asks Tainted Grail's existing merchant-stock objects to restock before the shop screen opens.

It does not create a second shop system.

## Build it

~~~powershell
dotnet build .\MerchantRestock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Use a merchant with normal restockable stock.

1. note the current stock;
2. open the shop;
3. close and reopen it;
4. watch the BepInEx log;
5. confirm the configured cooldown prevents immediate repeated restocks.

Use a disposable test save while experimenting with merchant inventory.

## What to change first

Change the per-merchant cooldown.

Then try changing the number of restock passes from 1 to 2.

Avoid category filtering until the basic restock behavior is clear.

## How it works

Tainted Grail shops contain different kinds of stock.

This example touches only:

~~~text
RestockableStock
~~~

When Shop.OpenShop is about to run, the mod calls the stock object's own:

~~~text
Restock()
~~~

Unique items, prices, purchases, merchant wealth, and item transfers are left to the normal game code.

## Next

[Read the merchant restock guide](../../../../guides/tasks/gameplay/tune-restockable-merchant-stock.md)
