# Bonfire Services Submenu

This example observes VFireplaceUI.OnInitialize, resolves the native buttonContent and levelUp ButtonConfig, clones the native button visual, and adds a small Services submenu.

The submenu routes directly to the current FireplaceUI owner:

~~~text
Stash   → OpenHeroStorage()
Cooking → CookAction()
Alchemy → AlchemyAction()
Back    → return to native bonfire content
~~~

Build:

~~~powershell
dotnet build .\BonfireServicesSubmenu.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example owns only cloned UI objects. The actual services remain native FireplaceUI actions.

Guide: ../../../../guides/tasks/ui/build-a-native-looking-bonfire-services-menu.md
