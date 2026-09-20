# Read-only Inventory Projection

A buildable projection/search UI that does not compile against TG.Main types.

At runtime it resolves:

~~~text
TG.Main assembly
→ Hero.Current
→ HeroItems
→ HeroItems.Inventory
→ native Item objects
→ temporary projection rows
~~~

HiddenOnUI items are skipped. Search/filter happens on the projection. The example contains no equip/use/transfer mutation method.

F8 opens/closes the overlay; Escape closes it.

## Build

~~~powershell
dotnet build .\InventoryProjection.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Build a read-only inventory projection](../../../../guides/tasks/ui/build-a-read-only-inventory-projection.md)
