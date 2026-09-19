# 12 — Restock Merchant Stock on Shop Open

**Category:** economy / merchant inventory  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is the smallest version of the real `Shop.OpenShop` + `RestockableStock.Restock()` path.

It intentionally omits cooldowns, category filtering, private-field reflection, UI metadata and presets.

## Warning

Merchant stock is gameplay/save-backed state. Test on a disposable save.

The example only calls `Restock()` on `RestockableStock` elements. It does not intentionally duplicate unique/static stock.

## Build

```powershell
dotnet build .\MerchantRestockExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

The owner-side path has build/deploy evidence but its in-game restock/unique-stock matrix remained unverified.
