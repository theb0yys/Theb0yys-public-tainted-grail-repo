# HUD Visibility

A buildable example for the native HUD ownership path.

It patches:

- the private VHeroHUD.ShowBars getter to optionally force the native HUD visible;
- VHeroHUD.UpdateCanvasGroups after the game updates its own state;
- VCSelectedQuickSlot.UpdateIcon so selected-quickslot visibility remains consistent.

Per-element visibility is implemented with CanvasGroup on the existing native objects rather than disabling them.

## Build

~~~powershell
dotnet build .\HudVisibility.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Customize the HUD](../../../../guides/tasks/ui/customize-the-hud-with-separate-system-and-visual-proof.md)
