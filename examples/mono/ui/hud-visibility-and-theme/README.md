# HUD Visibility and Theme

This example uses the native VHeroHUD lifecycle.

It patches:
- the private ShowBars getter to optionally keep the native hero HUD visible;
- UpdateCanvasGroups() to apply visual-only CanvasGroup visibility/opacity to health, stamina and mana roots.

Build:

~~~powershell
dotnet build .\HudVisibilityAndTheme.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Original CanvasGroup values are captured before mutation and restored on plug-in unload.

Guide: ../../../../guides/tasks/ui/customize-the-hud-with-separate-system-and-visual-proof.md
