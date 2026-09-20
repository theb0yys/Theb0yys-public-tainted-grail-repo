# Merchant Restock

This example patches Shop.OpenShop and calls Restock() only on RestockableStock elements. A per-shop cooldown prevents immediate repeated restocking.

Build:

~~~powershell
dotnet build .\MerchantRestock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

UniqueStock, prices, purchases, wealth and transfers stay native.

Guide: ../../../../guides/tasks/gameplay/tune-restockable-merchant-stock.md
