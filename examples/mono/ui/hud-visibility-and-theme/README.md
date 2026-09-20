# Change Which HUD Parts Are Visible

This example keeps Tainted Grail's normal HUD objects and changes only whether selected parts are visible and how opaque they are.

It is a safer starting point than rebuilding the entire HUD.

## Build it

~~~powershell
dotnet build .\HudVisibilityAndTheme.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

Load a save and change one setting at a time:

1. force the normal hero bars to remain visible;
2. hide only one bar;
3. change the visible opacity;
4. disable/remove the mod.

When the mod unloads, the original CanvasGroup values should be restored.

## What to change first

Start with one visibility toggle, such as the stamina bar.

Do not move or restyle several HUD elements in the same first test.

## How it works

Tainted Grail's HUD owner is VHeroHUD.

The example watches two important parts of that HUD:

~~~text
ShowBars
UpdateCanvasGroups()
~~~

After the game updates its own HUD, the mod changes only the visual CanvasGroup on known health, stamina, and mana roots.

A CanvasGroup is a Unity component that controls things such as opacity and whether UI receives input.

The real health/stamina/mana values are still owned by the game.

## Next

[Read the HUD customization guide](../../../../guides/tasks/ui/customize-the-hud-with-separate-system-and-visual-proof.md)
