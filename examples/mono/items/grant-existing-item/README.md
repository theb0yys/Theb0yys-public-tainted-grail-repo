# Grant an Existing Item

Minimal runnable form of the established native grant path:

~~~text
TemplatesProvider.Get<ItemTemplate>(guid)
→ World.Add(new Item(template, quantity))
→ Hero.Current.HeroItems.Add(item)
~~~

The default GUID is the researched existing Bloodstone template. Change it only to another exact existing ItemTemplate identity you have verified.

## Build

~~~powershell
dotnet build .\GrantExistingItem.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Test

Use a disposable save. Press F8.

Expected success log:

~~~text
Grant succeeded: template=...; guid=...; quantity=1.
~~~

The operation fails closed when the hero, template provider or configured template is unavailable.

## Boundary

This grants an existing native item. It does not register new templates or prove custom-item persistence/uninstall behaviour.

Guide: [Grant an existing FoA item](../../../../guides/tasks/items/grant-an-existing-item.md)  
Evidence: [Existing item grants](../../../../research/case-studies/content/item-grants.md)
