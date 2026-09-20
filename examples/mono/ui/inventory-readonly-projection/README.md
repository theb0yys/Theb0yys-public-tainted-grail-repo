# Build a Read-Only Custom Inventory View

This example reads the player's real inventory and displays a separate searchable list.

It does not equip, move, delete, or create items.

That makes it a good first step before building a larger inventory UI.

## Build it

~~~powershell
dotnet build .\InventoryReadonlyProjection.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Press F8 to open the example window.

The list is rebuilt from the player's current inventory.

Try:

- picking up an item;
- reopening/refreshing the view;
- searching by name;
- sorting the rows.

Confirm the real inventory remains unchanged.

## What to change first

Change one display field or one sort rule.

For example, add quantity to the row text or change the default sort order.

Do not add item-use/equip buttons until you understand the read-only version.

## How it works

The source reads:

~~~text
Hero.Current
→ HeroItems
→ Inventory
→ Item objects
~~~

It then copies only the information needed for display into small temporary row objects.

The custom list is therefore a **view of the inventory**, not a second inventory.

The example uses a small IMGUI window simply to keep the source easy to read. A larger mod can later attach its presentation to the Character Sheet.

## Next

[Read the inventory projection guide](../../../../guides/tasks/ui/build-a-read-only-inventory-projection.md)
