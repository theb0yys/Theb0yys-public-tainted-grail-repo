# 05 — Force Hero HUD Bars Visible

**Category:** UI / HUD  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This rewritten public example:** NOT_RUN

This patches the real VHeroHUD.ShowBars property getter.

The original getter runs first. A postfix changes the returned nullable bool to true when the example is enabled.

## Build

~~~powershell
dotnet build .\ForceHeroHudExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## This is intentionally blunt

A production HUD mod normally needs context rules for dialogue, menus, cutscenes, cursor ownership, disabled HUD roots and other UI states.

This example leaves those policies out so the learner can see the single proven seam.

Use it as a starting point, not as a finished UI design.
