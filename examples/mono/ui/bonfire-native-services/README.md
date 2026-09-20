# Bonfire Native Services and Submenu

This example demonstrates both parts of the bonfire extension pattern:

1. observe the active VFireplaceUI / FireplaceUI owner;
2. clone FoA's existing Level Up ButtonConfig to add a native-looking Services entry;
3. open a small Services submenu made from the same native button template;
4. route each submenu button into the real FireplaceUI service method;
5. destroy only the mod-owned cloned rows when Back is selected or the owner changes.

## Native services used

~~~text
Stash         → FireplaceUI.OpenHeroStorage()
Cooking       → FireplaceUI.CookAction()
Alchemy       → FireplaceUI.AlchemyAction()
Handcrafting  → FireplaceUI.HandcraftingAction()
Back          → destroy the mod-owned submenu rows
~~~

The service transactions themselves remain FoA-owned.

## Build

~~~powershell
dotnet build .\BonfireNativeServices.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## What to change first

Add another submenu row by cloning the existing button template in AddSubmenuButton(...) and point it at another known FireplaceUI service method.

Do not rebuild stash/cooking/alchemy logic inside the UI layer.

Related guides:

- [Add native services to the bonfire menu](../../../../guides/tasks/ui/add-native-services-to-the-bonfire-menu.md)
- [Build a native-looking bonfire services submenu](../../../../guides/tasks/ui/build-a-native-looking-bonfire-services-menu.md)
