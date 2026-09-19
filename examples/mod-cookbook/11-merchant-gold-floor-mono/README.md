# 11 — Merchant Gold Floor

**Category:** economy / merchant wealth  
**Source-path evidence:** SOURCE_CONFIRMED  
**This rewritten public example:** NOT_RUN

This example lets vanilla `Shop.Restock(bool)` run first, then tops the merchant's existing wealth up to a configured minimum.

That is a useful pattern when the native operation should remain authoritative and your mod only adds a bounded post-condition.

## Warning

Merchant wealth is save-backed game state. If the game saves after a top-up, the change may persist.

Use a disposable save while learning.

## Build

```powershell
dotnet build .\MerchantGoldFloorExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

The owner-side implementation identifies this exact target, but its inspected validation state did not include compile/load/in-game merchant proof. This recipe therefore stays at **SOURCE_CONFIRMED**.
