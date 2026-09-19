# 21 — FOV Transition Duration

**Category:** camera / comfort  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example keeps FoA's target FOV values and changes only how long native FOV transitions take.

Target:

~~~text
Awaken.TG.Main.Heroes.VHeroController.SetFoV(HeroFoV.FoVChangeData)
~~~

Config:

~~~text
[Camera]
TransitionDurationMultiplier = 1.35
~~~

Below 1 makes supported transitions faster, 1 is vanilla, and above 1 makes them slower/smoother. Final duration is clamped to 0.01 through 4 seconds.

## Build

~~~powershell
dotnet build .\FovTransitionExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This does not set a custom FOV and does not replace FoA's camera.

The public rewrite is **NOT_RUN**.
