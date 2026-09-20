# Inventory Read-Only Projection

Press F8 to open a small read-only list built from the current HeroItems.Inventory.

The example:

~~~text
Hero.Current
→ HeroItems
→ Inventory
→ native Item rows
→ skip HiddenOnUI templates
→ copy display fields into temporary Row structs
→ search/sort the projection
~~~

It stores no native Item objects as durable truth and exposes no mutation action.

Build:

~~~powershell
dotnet build .\InventoryReadonlyProjection.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This uses a minimal IMGUI host so the projection mechanism is obvious. A production native Character Sheet integration can attach at InventoryUI.AfterViewSpawned and restore at InventoryUI.OnDiscard.

Guide: ../../../../guides/tasks/ui/build-a-read-only-inventory-projection.md
