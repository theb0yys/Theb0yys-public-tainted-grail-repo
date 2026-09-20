# Add a Services Menu to a Bonfire

This example adds a small **Services** menu to the existing bonfire screen.

The new buttons still call Tainted Grail's own stash, cooking, and alchemy actions.

## Build it

~~~powershell
dotnet build .\BonfireServicesSubmenu.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Open a bonfire.

The example adds a Services entry containing:

~~~text
Stash
Cooking
Alchemy
Back
~~~

Open each service and confirm the normal Tainted Grail screen/action appears.

Close the bonfire and reopen it. The mod should not create duplicate buttons.

## What to change first

Add or remove **one** service row.

For example, start by removing Alchemy, rebuild, and confirm the submenu updates cleanly.

## How it works

The mod waits for the normal bonfire view to initialize.

It copies one of the game's existing button styles so the new rows match the rest of the screen.

When a custom button is clicked, it calls the current FireplaceUI object's normal service method, such as:

~~~text
OpenHeroStorage()
CookAction()
AlchemyAction()
~~~

The mod owns only the extra menu buttons. Tainted Grail still owns the actual stash/crafting services.

## Next

[Read the bonfire submenu guide](../../../../guides/tasks/ui/build-a-native-looking-bonfire-services-menu.md)
