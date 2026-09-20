# Vendor Price Postfix

Minimal final-price adjustment example for TradeUtils.Price.

## Mechanism

~~~text
native providers calculate final price
→ TradeUtils.Price returns vanilla result
→ postfix classifies hero buying vs selling
→ supported direction receives configured multiplier
→ TradeUtils.TryTrade continues natively
~~~

Unknown directions and non-positive native prices remain unchanged.

## Build

~~~powershell
dotnet build .\VendorPricePostfix.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Test

Start at 1.0 / 1.0, record baseline buy/sell prices, then change one multiplier at a time on a disposable save.

The example logs an adjustment only when it actually changes a supported price.

## Boundary

It does not change stock, wealth ownership, item transfer, restocking, stolen-item legality, loot, rewards or recipes.

Guide: [Change vendor buy and sell prices](../../../../guides/tasks/gameplay/change-vendor-prices.md)  
Evidence: [Vendor price tuning](../../../../research/case-studies/gameplay/vendor-pricing.md)
