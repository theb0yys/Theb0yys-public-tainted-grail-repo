# Economy Runtime Rules

One buildable Mono/BepInEx 5 reference project covering two native runtime seams.

## Merchant restock

Shop.OpenShop is patched with a prefix. The example iterates only RestockableStock elements and calls the native Restock() method before the shop continues normally.

UniqueStock, purchases, prices and merchant wealth are not replaced.

## Post-roll container quantities

SearchAction.OnInitialize is patched with a postfix. The example reads the existing private _itemsInsideContainer collection and scales ItemSpawningDataRuntime.quantity once per SearchAction instance.

This modifies generated runtime rows; it does not rewrite the source loot table.

## Build

~~~powershell
dotnet build .\EconomyRuntimeRules.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Leave both settings at vanilla-preserving defaults first.

Related guides:

- [Tune restockable merchant stock](../../../../guides/tasks/gameplay/tune-restockable-merchant-stock.md)
- [Change post-roll container contents](../../../../guides/tasks/gameplay/change-post-roll-container-contents.md)
